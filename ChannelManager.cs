﻿namespace Discord.cs
{
    public class ChannelManager(MessageDisplay messageDisplay, TreeView channelTree, MainScreen parent)
    {
        private DiscordGuild? server;
        private IReadOnlyList<GuildChannel>? channels;
        private bool locked = false;

        public void SetServer(DiscordGuild? server)
        {
            this.server = server;

			if (server == null) {
				// Clear the channel tree and clear the messages
				// e.g. no longer a member of the server

				channelTree.Nodes.Clear();
				messageDisplay.ClearMessages();
				return;
			}

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

			async void clicked(object? sender, TreeNodeMouseClickEventArgs e)
			{

				if (locked)
				{
					MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", "Channel selection locked"));
					return;
				}
				locked = true;
				MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", "Channel selection unlocked, locking..."));

				if (e.Node != null && e.Node.Tag is GuildChannel channel && Utils.IsMessageChannel(channel))
				{
					MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", $"Selected channel {channel.Name}"));
					await messageDisplay.SetChannel(channel);
				}
				locked = false;
			}


			foreach (var channel in channels)
            {
                // organise into categories
                bool isCategory = channel.Type == ChannelType.Category;
				var menu = MainScreen.CreateContextMenu([
					new("Open", async (s, e) => await SetChannel(channel)),
					new("Copy ID", (s, e) => Clipboard.SetText(channel.Id.ToString())),
				]);
                TreeNode node = new()
                {
                    Text = (!isCategory ? "#" : "") + channel.Name,
                    ForeColor = channel.Type == ChannelType.Voice ? Color.Blue : Color.Black,
                    Name = channel.Id.ToString(),
                    Tag = channel,
					ContextMenuStrip = menu
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
			// for some reason this fires twice?? even with one click
            channelTree.NodeMouseClick += clicked;
            MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", "Channels rendered"));
        }


        public async Task SetChannel(GuildChannel channel)
        {
            if (Utils.IsMessageChannel(channel))
            {
                await messageDisplay.SetChannel(channel);
            }

            parent.label1.Text = channel.Name;

            await parent.Invoke(async () => await RenderChannels());
        }
    }
}
