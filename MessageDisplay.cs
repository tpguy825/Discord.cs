using Discord.Gateway;

namespace Discord.cs
{
    internal class MessageDisplay(ListView listView, DiscordSocketClient client)
    {
        private readonly List<DiscordMessage> messages = [];

        public void AddMessage(DiscordMessage message)
        {
            messages.Add(message);
            (bool success, Stream stream) = Utils.DownloadCDNImage(message.Author.User.Avatar);
            Image image = success ? Image.FromStream(stream) : Image.FromFile("Resources/blank_user_icon.png");

            listView.SmallImageList ??= new ImageList();
            listView.SmallImageList.Images.Add($"message-{message.Id}", image);
            ListViewItem item = new()
            {
                Name = $"message-{message.Id}",
                Text = message.ToString(),
                Tag = message,
                ImageKey = $"message-{message.Id}"
            };
            listView.Items.Add(item);
        }

        public void RemoveMessage(DiscordMessage message)
        {
            messages.Remove(message);
            listView.Items.Remove(new ListViewItem(message.ToString()));
        }

        public void SetChannel(GuildChannel channel)
        {
            ClearMessages();
            foreach (var message in client.GetChannelMessages(channel.Id))
            {
                AddMessage(message);
            }
            MainScreen.RunOnUIThread(new Action(RenderMessages));
        }

        public void ClearMessages()
        {
            messages.Clear();
            listView.Items.Clear();
        }

        public void RenderMessages()
        {
            listView.SmallImageList ??= new ImageList();
            foreach (var message in messages)
            {
                (bool success, Stream stream) = Utils.DownloadCDNImage(message.Author.User.Avatar);
                Image image = success ? Image.FromStream(stream) : Image.FromFile("Resources/blank_user_icon.png");

                listView.SmallImageList.Images.Add($"message-{message.Id}", image);
                ListViewItem item = new()
                {
                    Name = $"message-{message.Id}",
                    Text = message.ToString(),
                    Tag = message,
                    ImageKey = $"message-{message.Id}"
                };
                listView.Items.Add(item);
            }
        }
    }
}
