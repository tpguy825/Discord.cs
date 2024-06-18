using Discord.Rest;

namespace Discord.cs
{
    internal class ServerList(DiscordRestClient client, MainScreen mainScreen)
    {

        private async Task RerenderServerList(IEnumerable<RestUserGuild> servers)
        {
            PictureBox[] pictureBoxes = new PictureBox[servers.Count()];
            int i = 0;
            foreach (var server in servers)
            {

                HttpClient client = new();
                System.Drawing.Image icon;
                if (server.IconUrl == null)
                {
                    var image = (System.Drawing.Image?)mainScreen.resources.GetObject("blank_server_icon");
                    if (image != null)
                    {
                        icon = image;
                    }
                    else
                    {
                        icon = new Bitmap(50, 50);
                    }
                }
                else
                {
                    HttpResponseMessage response = await client.GetAsync(server.IconUrl);
                    Stream stream = await response.Content.ReadAsStreamAsync();
                    icon = System.Drawing.Image.FromStream(stream);
                }
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

        public async Task RefreshServerList()
        {
            var guilds = await client.GetGuildSummariesAsync().FlattenAsync();
            await RerenderServerList(guilds);
        }
    }
}
