namespace Discord.cs
{
    internal class ChannelManager(MessageDisplay messageDisplay, TreeView channelTree, MainScreen parent)
    {
        private DiscordGuild? server;
        private IReadOnlyList<GuildChannel>? channels;

        public void SetServer(DiscordGuild server)
        {
            this.server = server;
            parent.Invoke(new Action(RenderChannels));
        }

        public void RenderChannels()
        {
            if (server == null) return;
            messageDisplay.ClearMessages();
            channels = server.GetChannels();

            channelTree.Nodes.Clear();
            foreach (var channel in channels)
            {
                TreeNode node = new()
                {
                    Name = $"channel-{channel.Id}",
                    Text = "#" + channel.Name,
                    Tag = channel,
                };
                channelTree.Nodes.Add(node);
            }

            channelTree.ExpandAll();
            channelTree.AfterSelect += (sender, e) =>
            {
                if (e.Node != null && e.Node.Tag is GuildChannel channel)
                {
                    messageDisplay.SetChannel(channel);
                }
            };
        }


        public void SetChannel(GuildChannel channel)
        {
            messageDisplay.ClearMessages();
            messageDisplay.SetChannel(channel);

            parent.Invoke(new Action(RenderChannels));
        }
    }
}
