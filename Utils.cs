namespace Discord.cs
{
    internal class Utils
    {
        public static List<ImageCache> imageCache = new();

        public static (bool, Stream) DownloadCDNImage(DiscordCDNImage input, HttpClient? httpClient = null)
        {
            httpClient ??= new()
            {
                BaseAddress = new Uri("https://cdn.discordapp.com"),
                Timeout = TimeSpan.FromSeconds(5)
            };
            if (input == null) return (false, Stream.Null);
            try
            {
                if (imageCache.Any(x => x.Url == input.Url))
                {
                    return (true, imageCache.First(x => x.Url == input.Url).Image);
                }

                HttpRequestMessage request = new()
                {
                    RequestUri = new Uri(input.Url + ".png"),
                    Method = HttpMethod.Get,
                    Headers =
                    {
                        { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:127.0) Gecko/20100101 Firefox/127.0 Discord.cs/0.1" }
                    }
                };
                HttpResponseMessage response = httpClient.Send(request);
                Stream stream = response.EnsureSuccessStatusCode().Content.ReadAsStream();

                imageCache.Add(new ImageCache { Url = input.Url, Image = stream });

                return (true, stream);
            }
            catch (Exception ex)
            {
                MainScreen.Log(new LogMessage(LogSeverity.Error, "Discord.cs", $"Failed to download icon for {input.Url}: {ex.Message}"));
                return (false, Stream.Null);
            }
        }

        public static async Task<(bool, Stream)> DownloadCDNImageAsync(DiscordCDNImage input, HttpClient? httpClient = null)
        {
            httpClient ??= new()
            {
                BaseAddress = new Uri("https://cdn.discordapp.com"),
                Timeout = TimeSpan.FromSeconds(5)
            };
            if (input == null) return (false, Stream.Null);
            try
            {
                if (imageCache.Any(x => x.Url == input.Url))
                {
                    return (true, imageCache.First(x => x.Url == input.Url).Image);
                }

                HttpRequestMessage request = new()
                {
                    RequestUri = new Uri(input.Url + ".png"),
                    Method = HttpMethod.Get,
                    Headers =
                    {
                        { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:127.0) Gecko/20100101 Firefox/127.0 Discord.cs/0.1" }
                    }
                };
                HttpResponseMessage response = await httpClient.SendAsync(request);
                Stream stream = await response.EnsureSuccessStatusCode().Content.ReadAsStreamAsync();

                imageCache.Add(new ImageCache { Url = input.Url, Image = stream });

                return (true, stream);
            }
            catch (Exception ex)
            {
                MainScreen.Log(new LogMessage(LogSeverity.Error, "Discord.cs", $"Failed to download icon for {input.Url}: {ex.Message}"));
                return (false, Stream.Null);
            }
        }

        public static Bitmap ResizeImage(Image input, int width, int height)
        {
            Bitmap output = new(width, height);
            using (Graphics g = Graphics.FromImage(output))
            {
                g.DrawImage(input, 0, 0, width, height);
            }
            return output;
        }

        public static TextChannel? IsTextChannel(GuildChannel? channel)
        {
            if (channel == null) return null;
            return channel.Type == ChannelType.Text ? (TextChannel)channel : null;
        }

        public static PrivateChannel? IsPrivateChannel(DiscordChannel? channel)
        {
            if (channel == null) return null;
            return channel.Type == ChannelType.DM ? (PrivateChannel)channel : null;
        }

        public static DiscordGroup? IsGroupChannel(DiscordChannel? channel)
        {
            if (channel == null) return null;
            return channel.Type == ChannelType.Group ? (DiscordGroup)channel : null;
        }

        public static VoiceChannel? IsVoiceChannel(GuildChannel? channel)
        {
            if (channel == null) return null;
            return channel.Type == ChannelType.Voice ? (VoiceChannel)channel : null;
        }

        public static GuildChannel? IsCategoryChannel(GuildChannel? channel)
        {
            if (channel == null) return null;
            return channel.Type == ChannelType.Category ? channel : null;
        }

        public static bool IsMessageChannel(DiscordChannel? channel)
        {
            return channel != null && (channel.IsText);
        }
    }

    public class ImageCache
    {
        public required string Url { get; set; }
        public required Stream Image { get; set; }
    }
}
