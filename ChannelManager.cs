namespace Discord.cs
{
    internal class ChannelManager(MessageDisplay messageDisplay, TreeView channelTree, MainScreen parent)
    {
        private DiscordGuild? server;
        private IReadOnlyList<GuildChannel>? channels;

        public void SetServer(DiscordGuild server)
        {
            this.server = server;
            parent.Invoke(new Action(async () => await RenderChannels()));
        }

        public async Task RenderChannels()
        {
            if (server == null) return;
            MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", "Rendering channels"));
            messageDisplay.ClearMessages();
            MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", "Cleared messages"));
            channels = await server.GetChannelsAsync();
            MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", "Retreived channels"));
            channelTree.Nodes.Clear();
            channelTree.ShowLines = true;
            channelTree.ShowPlusMinus = true;
            foreach (var channel in channels)
            {
                // organise into categories
                bool isCategory = channel.Type == ChannelType.Category;
                TreeNode node = new()
                {
                    Text = (!isCategory ? "#" : "") + channel.Name,
                    ForeColor = channel.Type == ChannelType.Voice ? Color.Blue : Color.Black,
                    Name = channel.Id.ToString(),
                    Tag = channel
                };
                if (isCategory)
                {
                    channelTree.Nodes.Add(node);
                }
                else
                {
                    if (channel.ParentId != null)
                    {
                        ulong parentId = (ulong)channel.ParentId;
                        TreeNode? parent = channelTree.Nodes.Find(parentId.ToString(), true).FirstOrDefault();
                        parent?.Nodes.Add(node);
                    }
                    else
                    {
                        channelTree.Nodes.Add(node);
                    }
                }
            }

            channelTree.ExpandAll();
            channelTree.NodeMouseClick += (sender, e) =>
            {
                if (e.Node.Tag is GuildChannel channel && Utils.IsMessageChannel(channel))
                {
                    MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", $"Selected channel {channel.Name}"));
                    messageDisplay.SetChannel((TextChannel)channel);
                }
            };
            MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", "Channels rendered"));
        }


        public void SetChannel(GuildChannel channel)
        {
            if (Utils.IsMessageChannel(channel))
            {
                // It's a message channel, ignore types
                // c# doesn't support union types :(
                messageDisplay.SetChannel((TextChannel)channel);
            }

            parent.Invoke(new Action(async () => await RenderChannels()));
        }
    }
}
