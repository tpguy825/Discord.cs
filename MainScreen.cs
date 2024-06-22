using Discord.Gateway;

namespace Discord.cs
{
    public partial class MainScreen : Form
    {
        public DiscordSocketClient? client;
        private ServerList? serverList;
        public static readonly DiscordNetLog log = new();
        private MessageDisplay messageDisplay;
        private ChannelManager channelManager;

        public MainScreen()
        {
            InitializeComponent();
            log.Show(this);
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

            messageDisplay = new MessageDisplay(listView1, client!);
            channelManager = new ChannelManager(messageDisplay, treeView1);

        }

        private void InitializeDiscordNet()
        {
            client = new DiscordSocketClient();

            try
            {
                string token = "";
                if (!File.Exists(".token"))
                {
                    CodeForm.ShowPopup(this, TokenPopupCallback);
                }
                else
                {
                    token = File.ReadAllText(".token");
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
                File.WriteAllText(".token", token);
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

                client.OnLoggedIn += (_, _) =>
                {
                    Text = $"Discord.cs [Connected as {client.User.Username}]";
                    RefreshServerList();
                };

                client.OnLoggedOut += (_, _) =>
                {
                    Text = "Discord.cs [Disconnected]";
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

        // Try to avoid if possible, can't guarantee it works
        public static void RunOnUIThread(Action action)
        {
            ((MainScreen)log.ParentForm!).Invoke(action);
        }

        // Must be called from UI thread!!!
        private void RenderServerList(ServerItem[] images)
        {
            int i = 0;
            splitContainer1.Panel1.Controls.Clear();
            foreach (var server in images)
            {
                var box = new PictureBox()
                {
                    Image = server.Image,
                    Name = $"pictureBox-{i}",
                    Size = new Size(50, 50),
                    Location = new Point(0, i * 50),
                    Cursor = Cursors.Hand
                };
                box.Click += (sender, e) =>
                {
                    SetActiveServer(server.Guild);
                };
                splitContainer1.Panel1.Controls.Add(box);
                i++;
            }
        }

        private void SetActiveServer(DiscordGuild guild)
        {
            Log(new LogMessage(LogSeverity.Info, "Discord.cs", $"Setting active server to {guild.Name}"));
            channelManager.SetServer(guild);
        }

        public static void Log(LogMessage msg)
        {
            log.richTextBox1.AppendText(msg.ToString() + "\n");
            log.richTextBox1.ScrollToCaret();
        }

        public static void Log(string msg)
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