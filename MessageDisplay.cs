namespace Discord.cs
{
    internal class MessageDisplay(Panel panel, MainScreen parent)
    {
        public DiscordMessage[] messages = [];
        public DiscordChannel? channel;

        // Run on render thread!
        public async Task RenderMessages()
        {
            SortMessages();
            if (channel == null) return;
            if (channel.Type == ChannelType.Voice)
            {
                // TODO
                return;
            }
            if (messages.Length == 0)
            {
                OnlyText("This channel is empty...");
                return;
            }
            panel.Controls.Clear();
            MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", $"Rendering messages for {channel.Name}"));

            Button loadMore = new()
            {
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

            panel.Controls.Add(loadMore);
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
                    Text = string.Format("{0} - {1}", (message.Author.Member?.Nickname ?? message.Author.User.Username), message.SentAt.ToString("HH:mm")) + (message.Author.User.Type == DiscordUserType.Bot || message.Author.User.Type == DiscordUserType.Webhook ? " (BOT)" : "") + (message.EditedAt != null ? string.Format(" (edited {0})", ((DateTime)message.EditedAt).ToString("HH:mm")) : "")
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
                panel.Controls.Add(containerPanel);
                results.Add(containerPanel);

                nexty += containerPanel.Height;

                i++;
            }

            panel.AutoScroll = true;
            panel.AutoScrollPosition = results.Last().Location;
            MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", $"Messages rendered for {channel.Name}"));
            panel.ScrollControlIntoView(results.Last());
        }

        public void SortMessages()
        {
            IComparer<DiscordMessage> comparer = new MessageComparer();
            Array.Sort(messages, comparer);
        }

        public async Task ManuallyAddMessage(DiscordMessage message)
        {
            messages = messages.Append(message).ToArray();

            SortMessages();
            await parent.Invoke(RenderMessages);
        }

        public async Task LoadMore(uint count = 50)
        {
            await LoadMessages(count);
            await parent.Invoke(RenderMessages);
        }

        public async Task SetChannel(DiscordChannel channel)
        {
            this.channel = channel;
            messages = [];
            messages = await Task.Run(async () => await LoadMessages(50));
            await parent.Invoke(RenderMessages);
        }

        public async Task<DiscordMessage[]> LoadMessages(uint count = 50)
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
            }
            catch (Exception e)
            {
                MainScreen.Log(new LogMessage(LogSeverity.Error, "Discord.cs", $"Failed to load messages: {e.Message}"));

            }
            return messages;
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
}