namespace Discord.cs
{
	internal class ServerList(DiscordClient client, MainScreen parent)
	{
		public async Task RefreshServerList()
		{
			try
			{
				if (client == null) return;
				MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", "Refreshing server list"));
				ServerItem[] images = await RefetchServerList();
				MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", "Rendering server list"));
				parent.Invoke(new Action(() => RenderServerList(images)));
				MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", "Server list refreshed"));
			}
			catch (Exception ex)
			{
				MainScreen.Log(new LogMessage(LogSeverity.Error, "Discord.cs", ex.Message));
			}
		}

		// Must be called from UI thread!!!
		private void RenderServerList(ServerItem[] images)
		{
			int i = 0;
			parent.tableLayoutPanel1.Controls.Clear();
			foreach (var server in images)
			{
				void click(object? sender, EventArgs e)
				{
					MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", $"Setting active server to {server.Guild.Name}"));
					parent.label2.Text = server.Guild.Name;
					parent.channelManager.SetServer(server.Guild);
				}

				var menu = MainScreen.CreateContextMenu([
					new("Open", click),
					new("Leave", async (s, e) =>
					{
						await server.Guild.LeaveAsync();
						await RefreshServerList();
						parent.channelManager.SetServer(null);
					}),
					new("Copy ID", (s, e) => Clipboard.SetText(server.Guild.Id.ToString())),
				]);
				var box = new PictureBox()
				{
					Image = server.Image,
					Name = $"pictureBox-{i}",
					Size = new Size(50, 50),
					Location = new Point(2, 2),
					Cursor = Cursors.Hand
				};


				// add box as a new row to tablePanel
				parent.tableLayoutPanel1.RowCount++;
				parent.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
				parent.tableLayoutPanel1.Controls.Add(box, 0, i);
				i++;
			}
		}

		private async Task<ServerItem[]> RefetchServerList()
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
			ServerItem item = new(server, Utils.ResizeImage(image, 50, 50));
			return item;
		}
	}

	public record ServerItem(DiscordGuild Guild, Image Image);
}
