using Discord.WebSocket;

namespace Discord.cs
{
    internal class ServerList(DiscordSocketClient _client, MainScreen mainScreen)
    {

        private async Task RerenderServerList(IReadOnlyCollection<SocketGuild> servers)
        {
            PictureBox[] pictureBoxes = new PictureBox[servers.Count];
            int i = 0;
            foreach (var server in servers)
            {

                HttpClient client = new();
                HttpResponseMessage response = await client.GetAsync(server.IconUrl);
                Stream stream = await response.Content.ReadAsStreamAsync();
                var icon = System.Drawing.Image.FromStream(stream);
                pictureBoxes[i] = new PictureBox
                {
                    Location = new Point(0, i * 50),
                    Width = 50,
                    Height = 50,
                    Image = icon,
                    SizeMode = PictureBoxSizeMode.StretchImage
                };

                pictureBoxes[i].Click += (sender, e) =>
                {
                    // TODO: Implement server selection
                };

                i++;
            }

            mainScreen.listView1.Items.Clear();
            mainScreen.listView1.Controls.AddRange(pictureBoxes);
        }

        public void RefreshServerList()
        {
            var guilds = _client.Guilds;
            RerenderServerList(guilds);
        }
    }
}
