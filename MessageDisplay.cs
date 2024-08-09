namespace Discord.cs
{
	internal class MessageDisplay(TableLayoutPanel panel, MainScreen parent)
	{
		public DiscordMessage[] messages = [];
		public DiscordChannel? channel;

		private bool locked = false;

		// Run on render thread!
		public async Task RenderMessages()
		{
			if (locked) return;
			locked = true;
			// reset panel state
			panel.Controls.Clear();
			panel.RowCount = 0;
			panel.RowStyles.Clear();

			if (channel == null) return;
			if (channel.Type == ChannelType.Voice)
			{
				OnlyText("This is a voice channel...");
				// TODO
				return;
			}
			await Task.Run(FullyRefreshMessages);
			if (messages.Length == 0)
			{
				OnlyText("This channel is empty...");
				return;
			}
			MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", $"Rendering messages for {channel.Name}"));
			messages = SortMessages();


			Button loadMore = new()
			{
				Margin = new Padding(3),
				Text = "Load more",
				AutoSize = true,
				Location = new Point(0, 0),
				Size = new Size(75, 23),
				TabIndex = 3
			};

			loadMore.Click += async (sender, e) =>
			{
				await LoadMore();
			};

			panel.RowCount++;
			panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 27));
			panel.Controls.Add(loadMore, 0, 0);
			List<Panel> results = new();

			int i = 0, nexty = loadMore.Height;
			foreach (var message in messages)
			{
				(bool success, Stream stream) = await Task.Run(async () => await Utils.DownloadCDNImageAsync(message.Author.User.Avatar));
				Image image = success ? Image.FromStream(stream) : Image.FromFile("Resources/blank_user_icon.jpg");

				image = Utils.ResizeImage(image, 50, 50);

				Panel containerPanel = new()
				{
					Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
					AutoSize = true,
					Location = new Point(0, nexty),
					Size = new Size(585, 56),
					TabIndex = 0,
					Tag = message
				};

				PictureBox pfp = new()
				{
					Location = new Point(3, 3),
					Size = new Size(50, 50),
					SizeMode = PictureBoxSizeMode.CenterImage,
					TabIndex = 0,
					TabStop = false,
					Image = image
				};

				Label username = new()
				{
					AutoSize = true,
					Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point, 0),
					Location = new Point(60, 7),
					Size = new Size(47, 19),
					TabIndex = 1,
					Text = string.Format("{0} - {1}",
						message.Author.Member?.Nickname ?? message.Author.User.Username,
						message.SentAt.ToString("dd/MM/yyyy HH:mm:ss"))
							+ (message.Author.User.Type == DiscordUserType.Bot ? " (BOT)" : "")
							+ (message.EditedAt != null ? string.Format(" (edited {0})", ((DateTime)message.EditedAt).ToString("dd/MM/yyyy HH:mm:ss")) : "")
				};

				Label content = new()
				{
					AutoSize = true,
					Location = new Point(60, 29),
					Size = new Size(38, 15),
					TabIndex = 2,
					Text = message.Content
				};

				containerPanel.Controls.Add(pfp);
				containerPanel.Controls.Add(username);
				containerPanel.Controls.Add(content);
				results.Add(containerPanel);

				nexty += containerPanel.Height;

				i++;
			}

			foreach (var containerPanel in results)
			{
				if (containerPanel.Tag is DiscordMessage message)
				{
					MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", $"Rendering message {message.Id} with timestamp {message.SentAt} and content {message.Content}"));
				}
			}

			// results.Sort(new PanelComparer());

			i = 1; // load more button is already there
			foreach (var containerPanel in results)
			{
				panel.RowCount++;
				panel.RowStyles.Add(new RowStyle(SizeType.Absolute, containerPanel.Height));
				panel.Controls.Add(containerPanel, 0, i);
				i++;
			}

			Panel last = results.Last();
			panel.AutoScroll = true;
			panel.AutoScrollPosition = last.Location;
			MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", $"Messages rendered for {channel.Name}"));
			panel.ScrollControlIntoView(last);
			locked = false;
		}

		public DiscordMessage[] SortMessages()
		{
			// i hate this
			DedupMessages();
			IComparer<DiscordMessage> comparer = new MessageComparer();
			return messages.OrderBy(x => x, comparer).ToArray();
		}

		public void DedupMessages()
		{
			messages = messages.GroupBy(x => x.Id).Select(x => x.First()).ToArray();

			// filter out blank non-bot messages
			messages = messages.Where(x => !string.IsNullOrWhiteSpace(x.Content) && x.Author.User.Type != DiscordUserType.Bot).ToArray();
		}

		public async Task ManuallyAddMessage(DiscordMessage message)
		{
			messages = messages.Append(message).ToArray();

			messages = SortMessages();
			await parent.Invoke(RenderMessages);
		}

		public async Task LoadMore(uint count = 20)
		{
			await LoadMessages(count);
			messages = SortMessages();
			await parent.Invoke(RenderMessages);
		}

		public async Task SetChannel(DiscordChannel channel)
		{
			this.channel = channel;
			parent.label1.Text = "#" + channel.Name;
			messages = [];
			messages = await Task.Run(async () => await LoadMessages());
			await parent.Invoke(RenderMessages);
		}

		public async Task<DiscordMessage[]> LoadMessages(uint count = 20)
		{
			if (channel == null) return [];
			if (channel.Type == ChannelType.Voice)
			{
				// TODO
				return [];
			}
			MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", $"Loading messages for {channel.Name}"));
			try
			{
				var msgs = await parent.client.GetChannelMessagesAsync(channel.Id, new MessageFilters()
				{
					Limit = count,
					BeforeId = messages.Length > 0 ? messages[^1].Id : null

				});
				if (msgs.Count > 0)
				{
					messages = msgs.Concat(messages).ToArray();
				}
				SortMessages();
			}
			catch (Exception e)
			{
				MainScreen.Log(new LogMessage(LogSeverity.Error, "Discord.cs", $"Failed to load messages: {e.Message}"));

			}
			return messages;
		}

		public async Task FullyRefreshMessages()
		{
			uint length = (uint)messages.Length;
			if (channel == null) return;
			MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", $"Fully refreshing messages for {channel.Name}"));
			messages = [];
			messages = await LoadMessages(length);
			MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", $"Fully refreshed messages for {channel.Name}"));
		}

		public void OnlyText(string text)
		{
			ClearMessages();
			Label label = new()
			{
				Text = text,
				AutoSize = true
			};
			panel.Controls.Add(label);
			label.Location = new Point((panel.Width - label.Width) / 2, (panel.Height - label.Height) / 2);

		}

		public void ClearMessages()
		{
			messages = [];
			panel.Controls.Clear();
		}
	}

	public class MessageComparer : IComparer<DiscordMessage>
	{
		public int Compare(DiscordMessage? x, DiscordMessage? y)
		{
			if (x == null || y == null) return 0;
			return x.SentAt.CompareTo(y.SentAt);
		}
	}

	public class PanelComparer : IComparer<Panel>
	{
		public int Compare(Panel? x, Panel? y)
		{
			if (x == null || y == null) return 0;
			return x.Tag is DiscordMessage xmsg && y.Tag is DiscordMessage ymsg ? new MessageComparer().Compare(xmsg, ymsg) : 0;
		}
	}
}