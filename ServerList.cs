using Discord.Gateway;

namespace Discord.cs
{
    internal class ServerList(DiscordSocketClient client)
    {
        private readonly HttpClient httpclient = new()
        {
            BaseAddress = new Uri("https://cdn.discordapp.com"),
            Timeout = TimeSpan.FromSeconds(5)
        };

        public ServerItem[] RefreshServerList()
        {
            var servers = client.GetGuilds().OrderBy(g => g.Name);
            var images = new ServerItem[servers.Count()];
            int i = 0;
            foreach (var server in servers)
            {
                if (server == null) continue;

                (bool success, Stream stream) = GetIconForServer(server);

                Image? image = null;
                if (success)
                {
                    image = Image.FromStream(stream);
                }
                else
                {
                    image = Image.FromFile("Resources/blank_server_icon.png");
                }

                MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", $"Rendering server icon for {server.Name}"));
                images[i] = new ServerItem(server, ResizeImage(image, 50, 50));

                MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", $"Rendered server {server.Name}"));

                i++;
            }

            return images;
        }

        private (bool, Stream) GetIconForServer(PartialGuild server)
        {
            if (server.Icon == null) return (false, Stream.Null);
            MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", $"Rendering server {server.Name}"));
            //MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", $"URL: https://cdn.discordapp.com/icons/{server.Id}/{server.
            try
            {
                HttpRequestMessage request = new()
                {
                    RequestUri = new Uri(server.Icon.Url + ".png"),
                    Method = HttpMethod.Get,
                    Headers =
                    {
                        { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:127.0) Gecko/20100101 Firefox/127.0 Discord.cs/0.1" }
                    }
                };
                HttpResponseMessage response = httpclient.Send(request);
                Stream stream = response.EnsureSuccessStatusCode().Content.ReadAsStream();

                return (true, stream);
            }
            catch (Exception ex)
            {
                MainScreen.Log(new LogMessage(LogSeverity.Error, "Discord.cs", $"Failed to download icon for {server.Name}: {ex.Message}"));
                return (false, Stream.Null);
            }
        }

        private static Bitmap ResizeImage(Image input, int width, int height)
        {
            Bitmap output = new(width, height);
            using (Graphics g = Graphics.FromImage(output))
            {
                g.DrawImage(input, 0, 0, width, height);
            }
            return output;
        }
    }
}
