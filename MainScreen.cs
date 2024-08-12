using Discord.Gateway;

namespace Discord.cs
{
	public partial class MainScreen : Form
	{
		public DiscordSocketClient? client;
		private ServerList? serverList;
		public static readonly DiscordNetLog log = new();
		public readonly ChannelManager channelManager;
		private readonly MessageDisplay messageDisplay;
		private readonly string tokenPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".token");

		public MainScreen()
		{
			InitializeComponent();
			log.Show();
			log.SendToBack();
			BringToFront();


			try
			{
				InitializeDiscordNet();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}

			messageDisplay = new MessageDisplay(panel1, this);
			channelManager = new ChannelManager(messageDisplay, treeView1, this);
			serverList = new ServerList(client!, this);
		}

		private void InitializeDiscordNet()
		{
			client = new DiscordSocketClient();

			try
			{
				string token = "";
				if (!File.Exists(tokenPath))
				{
					CodeForm.ShowPopup(this, TokenPopupCallback);
				}
				else
				{
					token = File.ReadAllText(tokenPath);
					if (token.Trim().Length == 0 || token == null) throw new Exception("Failed to read token from token file");
					Task.Run(() => LoginToDiscordNet(token.Trim()));
				}
			}
			catch (Exception ex)
			{
				Log(new LogMessage(LogSeverity.Error, "Discord.cs", ex.Message));
			}
		}

		public void TokenPopupCallback(string token)
		{
			try
			{

				Task.Run(() => LoginToDiscordNet(token));
				File.WriteAllText(tokenPath, token);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		public static void InvalidateToken()
		{
			File.Delete(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".token"));
		}

		public static ContextMenuStrip CreateContextMenu(ToolStripItemConfig[] items)
		{
			var menu = new ContextMenuStrip();
			foreach (var item in items)
			{
				menu.Items.Add(item.ToMenuItem());
			}
			return menu;
		}

		private async Task LoginToDiscordNet(string token)
		{
			try
			{
				client = new DiscordSocketClient();

				client.OnLoggedIn += async (_, _) =>
				{
					Invoke(() => Text = $"Discord.cs [Connected as {client.User.Username}]");
					if (serverList == null) serverList = new ServerList(client, this);
					await serverList.RefreshServerList();
				};

				client.OnLoggedOut += async (client, eventargs) =>
				{
					Invoke(() => Text = "Discord.cs [Disconnected]");
					Log(new LogMessage(LogSeverity.Info, "Discord.cs", "Logged out, reason: " + eventargs.Reason));
					await client.LoginAsync(token);
				};

				client.OnMessageReceived += onMessage;

				Resize += async (sender, e) =>
				{
					await Invoke(messageDisplay.RenderMessages);
					tableLayoutPanel1.Height = splitContainer1.Height;
				};

				await client.LoginAsync(token);
			}
			catch (Exception ex)
			{
				try
				{
					Log(ex.Message);
				}
				catch
				{
					MessageBox.Show(ex.Message);
				}
			}
		}

		public static void Log(LogMessage msg)
		{
			log.Invoke(new Action(() => LogInternal(msg.ToString())));
		}

		public static void Log(string msg)
		{
			log.Invoke(new Action(() => LogInternal(msg)));
		}

		private static void LogInternal(string msg)
		{
			log.richTextBox1.AppendText(msg + "\n");
			log.richTextBox1.ScrollToCaret();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			try
			{
				InitializeDiscordNet();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private async Task<DiscordMessage?> sendMessage(string text)
		{
			if (client == null || messageDisplay.channel == null)
			{
				Log(new LogMessage(LogSeverity.Error, "Discord.cs", "Client or channel is null"));
				return null;
			}
			DiscordMessage message = await client.SendMessageAsync(messageDisplay.channel.Id, new MessageProperties
			{
				Content = text
			});
			await messageDisplay.ManuallyAddMessage(message);
			return message;
		}

		private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Enter:
					e.Handled = true;
					e.SuppressKeyPress = true;
					if (e.Shift)
					{
						richTextBox1.AppendText("\n");
					}
					else
					{
						string text = richTextBox1.Text.Trim();
						if (text.Length > 0)
						{
							Log(new LogMessage(LogSeverity.Info, "Discord.cs", "Sending message: " + text));
							Task.Run(async () => await sendMessage(text));
							richTextBox1.Clear();
						}
					}
					break;
			}
		}

		private void onMessage(DiscordSocketClient sender, MessageEventArgs e)
		{
			if (e.Message.Channel.Id == messageDisplay.channel?.Id)
			{
				Invoke(new Action(async () => await messageDisplay.ManuallyAddMessage(e.Message)));
			}
		}
	}

	public record LogMessage(LogSeverity Severity, string Source, string Message)
	{
		public override string ToString() => $"[{Severity}] {Source}: {Message}";
	}

	public enum LogSeverity
	{
		Error,
		Warning,
		Info,
		Debug
	}

	public record ToolStripItemConfig(string Text,  EventHandler Click, Image? Image = null)
	{
		public ToolStripMenuItem ToMenuItem() => new(Text, Image, Click);

		public ToolStripItemConfig(string Text, Action Click, Image? Image = null) : this(Text, new EventHandler((_, _) => Click()), Image) { }
	}
}