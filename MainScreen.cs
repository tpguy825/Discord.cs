using Discord.WebSocket;
using System.Reactive.Threading.Tasks;

namespace Discord.cs
{
    public partial class MainScreen : Form
    {
        public DiscordSocketClient? client;
        private ServerList? serverList;
        private DiscordNetLog? log;

        public MainScreen()
        {
            InitializeComponent();
            Task.Run(InitializeDiscordNet).Start();
            log = new DiscordNetLog();
            log.Show(this);
        }

        private async Task InitializeDiscordNet()
        {
            client = new DiscordSocketClient();
            client.Log += Log;

            client.Ready += RefreshServerList;

            //  You can assign your bot token to a string, and pass that in to connect.
            //  This is, however, insecure, particularly if you plan to have your code hosted in a public repository.
            //var token = "token";

            // Some alternative options would be to keep your token in an Environment Variable or a standalone file.
            // var token = Environment.GetEnvironmentVariable("NameOfYourEnvironmentVariable");
            var token = File.ReadAllText(".token");
            // var token = JsonConvert.DeserializeObject<AConfigurationClass>(File.ReadAllText("config.json")).Token;

            await client.LoginAsync(TokenType.Bearer, token);
            await client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private async Task RefreshServerList()
        {
            if (client == null) return;
            listView1.Items.Clear();
            serverList = new ServerList(client, this);
            serverList.RefreshServerList();
        }

        private Task Log(LogMessage msg)
        {
            log.richTextBox1.AppendText(msg.ToString() + "\n");
            return Task.CompletedTask;
        }


        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
