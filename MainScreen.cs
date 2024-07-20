using Discord.Gateway;

namespace Discord.cs
{
    public partial class MainScreen : Form
    {
        public DiscordSocketClient? client;
        private ServerList? serverList;
        public static readonly DiscordNetLog log = new();
        private readonly ChannelManager channelManager;
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
                    Task.Run(() => LoginToDiscordNet(token));
                }
            }
            catch (Exception ex)
            {
                Log(new LogMessage(LogSeverity.Error, "Discord.cs", ex.Message));
            }
        }

        private void TokenPopupCallback(string token)
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

        private async Task LoginToDiscordNet(string token)
        {
            try
            {
                client = new DiscordSocketClient();

                client.OnLoggedIn += async (_, _) =>
                {
                    Invoke(() => Text = $"Discord.cs [Connected as {client.User.Username}]");
                    await RefreshServerList();
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

        private async Task RefreshServerList()
        {
            try
            {
                if (client == null) return;
                Log(new LogMessage(LogSeverity.Info, "Discord.cs", "Refreshing server list"));
                serverList = new ServerList(client);
                ServerItem[] images = await serverList.RefreshServerList();
                Log(new LogMessage(LogSeverity.Info, "Discord.cs", "Rendering server list"));
                Invoke(new Action(() => RenderServerList(images)));
                Log(new LogMessage(LogSeverity.Info, "Discord.cs", "Server list refreshed"));
            }
            catch (Exception ex)
            {
                Log(new LogMessage(LogSeverity.Error, "Discord.cs", ex.Message));
            }
        }

        // Must be called from UI thread!!!
        private void RenderServerList(ServerItem[] images)
        {
            int i = 0;
            tableLayoutPanel1.Controls.Clear();
            foreach (var server in images)
            {
                var box = new PictureBox()
                {
                    Image = server.Image,
                    Name = $"pictureBox-{i}",
                    Size = new Size(50, 50),
                    Location = new Point(2, 2),
                    Cursor = Cursors.Hand
                };
                box.Click += (sender, e) =>
                {
                    SetActiveServer(server.Guild);
                };


                // add box as a new row to tablePanel
                tableLayoutPanel1.RowCount++;
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
                tableLayoutPanel1.Controls.Add(box, 0, i);
                i++;
            }
        }

        private void SetActiveServer(DiscordGuild guild)
        {
            Log(new LogMessage(LogSeverity.Info, "Discord.cs", $"Setting active server to {guild.Name}"));
            label2.Text = guild.Name;
            channelManager.SetServer(guild);
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

    public record ServerItem(DiscordGuild Guild, Image Image);
}