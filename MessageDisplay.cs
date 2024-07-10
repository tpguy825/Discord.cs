namespace Discord.cs
{
    internal class MessageDisplay
    {
        private readonly List<DiscordMessage> messages = [];
        public DiscordChannel? channel;
        public readonly MessagesPaginator paginator;
        private readonly ListView listView;
        private readonly MainScreen parent;

        public MessageDisplay(ListView listView, MainScreen parent)
        {
            paginator = new MessagesPaginator();
            this.listView = listView;
            this.parent = parent;
        }

        public void SetChannel(TextChannel? channel)
        {
            if (channel == null)
            {
                MainScreen.Log(new LogMessage(LogSeverity.Error, "Discord.cs", "Channel is null"));
                return;
            }
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

        public async Task RenderMessages(bool loadMore = false)
        {
            if (channel == null || paginator == null) return;
            listView.SmallImageList ??= new ImageList();
            listView.SmallImageList.Images.Clear();
            if (channel.Type == ChannelType.Voice)
            {
                // TODO
                listView.Items.Add(new ListViewItem("Voice channels are not supported at the moment"));
                return;
            }
            MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", $"Rendering messages for {channel.Name}"));
            List<ListViewItem> results = new();
            if (loadMore)
                await paginator.LoadMoreMessages();
            foreach (var message in await paginator.RefreshMessages())
            {
                (bool success, Stream stream) = Utils.DownloadCDNImage(message.Author.User.Avatar);
                Image image = success ? Image.FromStream(stream) : Image.FromFile("Resources/blank_user_icon.jpg");

                listView.SmallImageList.Images.Add($"message-{message.Id}", image);
                ListViewItem item = new()
                {
                    Name = $"message-{message.Id}",
                    Text = message.Content,
                    Tag = message,
                    ImageKey = $"message-{message.Id}"
                };
                results.Add(item);
            }
            listView.Items.Clear();
            listView.Items.AddRange(results.ToArray());
        }
    }
}
