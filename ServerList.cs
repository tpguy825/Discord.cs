namespace Discord.cs
{
    internal class ServerList(DiscordClient client)
    {
        public async Task<ServerItem[]> RefreshServerList()
        {
            var servers = (await client.GetGuildsAsync()).OrderBy(g => g.Name);
            var images = new Task<ServerItem>[servers.Count()];
            int i = 0;
            foreach (var server in servers)
            {
                if (server == null) continue;

                images[i] = Task.Run(() => GetServerItem(server.GetGuild()));

                i++;
            }

            return await Task.WhenAll(images);
        }

        private static async Task<ServerItem> GetServerItem(DiscordGuild server)
        {

            (bool success, Stream stream) = await Utils.DownloadCDNImageAsync(server.Icon);

            Image image = success ? Image.FromStream(stream) : Image.FromFile("Resources/blank_server_icon.png");

            MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", $"Rendering server icon for {server.Name}"));
            ServerItem item = new(server, Utils.ResizeImage(image, 50, 50));

            MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", $"Rendered server {server.Name}"));
            return item;
        }
    }
}
