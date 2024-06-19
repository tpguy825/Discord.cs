using Discord.Gateway;
using Microsoft.Maui.Graphics;

namespace Discord.cs
{
    internal class ServerList(DiscordSocketClient client)
    {
        public async Task<PictureBox[]> RefreshServerList()
        {
            var servers = await client.GetGuildsAsync();
            PictureBox[] pictureBoxes = new PictureBox[servers.Count];
            int i = 0;
            foreach (var server in servers)
            {
                MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", $"Rendering server {server.Name}"));
                HttpClient client = new();
                Stream stream = server.Icon.Download(DiscordCDNImageFormat.PNG).PlatformImage.AsStream();
                MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", $"Stream is {stream.Length} bytes long"));
                Image icon = Image.FromStream(stream);
                MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", $"Rendering server icon for {server.Name}"));
                pictureBoxes[i] = new PictureBox
                {
                    Location = new System.Drawing.Point(0, i * 50),
                    Size = new System.Drawing.Size(50, 50),
                    Image = ResizeImage(icon, 50, 50),
                    Name = "servericon" + i,
                    SizeMode = PictureBoxSizeMode.StretchImage
                };

                pictureBoxes[i].Click += (sender, e) =>
                {
                    // TODO: Implement server selection
                };

                MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", $"Rendered server {server.Name}"));

                i++;
            }

            return pictureBoxes;
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
