namespace Discord.cs
{
    internal class MessageDisplay(ListView listView, DiscordClient client, MainScreen parent)
    {
        private readonly List<DiscordMessage> messages = [];
        private DiscordChannel? channel;
        private MessagesPaginator paginator = new(client);

        public void SetChannel(TextChannel? channel)
        {
            if (channel == null) return;
            this.channel = channel;
            ClearMessages();
            parent.label1.Text = "#" + channel.Name;
            paginator.SetChannel(channel);
            parent.Invoke(new Action(async () => await RenderMessages()));
        }

        public void ClearMessages()
        {
            messages.Clear();
            listView.Items.Clear();
            paginator.ClearMessages();
            listView.SmallImageList?.Images.Clear();
        }

        public async Task RenderMessages()
        {
            if (channel == null || paginator == null) return;
            listView.SmallImageList ??= new ImageList();
            if (channel.Type == ChannelType.Voice)
            {
                listView.Items.Add(new ListViewItem("Voice channels are not supported at the moment"));
                return;
            }
            MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", $"Rendering messages for {channel.Name}"));
            foreach (var message in await paginator.LoadMoreMessages())
            {
                (bool success, Stream stream) = Utils.DownloadCDNImage(message.Author.User.Avatar);
                Image image = success ? Image.FromStream(stream) : Image.FromFile("Resources/blank_user_icon.png");

                listView.SmallImageList.Images.Add($"message-{message.Id}", image);
                ListViewItem item = new()
                {
                    Name = $"message-{message.Id}",
                    Text = message.Content,
                    Tag = message,
                    ImageKey = $"message-{message.Id}"
                };
                listView.Items.Add(item);
            }
        }
    }
}
