using Discord.Rest;

namespace Discord.cs
{
    public partial class MainScreen : Form
    {
        public DiscordRestClient? client;
        private ServerList? serverList;
        private readonly DiscordNetLog log = new();

        public MainScreen()
        {
            InitializeComponent();
            Task.Run(InitializeDiscordNet);
            log.Show(this);
        }

        private async Task InitializeDiscordNet()
        {
            client = new DiscordRestClient();
            client.Log += Log;

            try
            {
                var token = File.ReadAllText(".token").Trim();

                client.LoggedIn += async () =>
                {
                    Text = $"Discord.cs [Connected as {client.CurrentUser.Username}]";
                    await RefreshServerList();
                };

                client.LoggedOut += () =>
                {
                    Text = "Discord.cs [Disconnected]";
                    return Task.CompletedTask;
                };

                await client.LoginAsync(TokenType.Bearer, token);
            }
            catch (Exception ex)
            {
                await Log(new LogMessage(LogSeverity.Error, "Discord.cs", ex.Message));
            }
        }

        private async Task 

        private async Task RefreshServerList()
        {
            if (client == null) return;
            listView1.Items.Clear();
            serverList = new ServerList(client, this);
            await serverList.RefreshServerList();
        }

        private Task Log(LogMessage msg)
        {
            log.richTextBox1.AppendText(msg.ToString() + "\n");
            return Task.CompletedTask;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (client == null) return;
            var token = File.ReadAllText(".token").Trim();
            Task.Run(async () =>
            {
                try
                {
                    await client.LogoutAsync();
                    await client.LoginAsync(TokenType.Bearer, token);
                    await RefreshServerList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }
    }
}
