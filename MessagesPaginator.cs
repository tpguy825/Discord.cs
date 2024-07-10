namespace Discord.cs
{
    internal class MessagesPaginator(uint pageSize = 20)
    {
        private List<DiscordMessage> messages = [];
        private IMessageChannel? channel;

        public void SetChannel(IMessageChannel channel)
        {
            this.channel = channel;
            messages.Clear();
        }

        public void ClearMessages()
        {
            messages.Clear();
        }

        public List<DiscordMessage> Dump()
        {
            return messages;
        }

        public void ManuallyAddMessage(DiscordMessage message)
        {
            messages.Add(message);
            messages = messages.OrderBy(msg => msg.SentAt).ToList();
        }

        private void DeduplicateMessages()
        {
            messages = messages.GroupBy(msg => msg.Id).Select(group => group.First()).ToList();
        }

        public async Task<DiscordMessage[]> RefreshMessages()
        {

            if (channel == null) return [];
            MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", "Refreshing messages"));
            try
            {
                MessageFilters filter = new()
                {
                    Limit = (uint)messages.Count > pageSize ? (uint)messages.Count : pageSize

                };
                IReadOnlyList<DiscordMessage> newMessages = await channel.GetMessagesAsync(filter);
                messages = newMessages.OrderBy(msg => msg.SentAt).ToList();
                DeduplicateMessages();
                return messages.ToArray();
            }
            catch (Exception ex)
            {
                MainScreen.Log(new LogMessage(LogSeverity.Error, "Discord.cs", $"Failed to refresh messages: {ex.Message}"));
                return [];
            }
        }

        public async Task<DiscordMessage[]> LoadMoreMessages()
        {
            if (channel == null) return [];
            MainScreen.Log(new LogMessage(LogSeverity.Info, "Discord.cs", $"Loading {pageSize} more messages"));
            try
            {
                MessageFilters filter = new()
                {
                    Limit = pageSize,
                    BeforeId = messages.Count > 0 ? messages[^1].Id : null
                };
                IReadOnlyList<DiscordMessage> newMessages = await channel.GetMessagesAsync(filter);
                messages = messages.Concat(newMessages).OrderBy(msg => msg.SentAt).ToList();
                DeduplicateMessages();
                return messages.ToArray();
            }
            catch (Exception ex)
            {
                MainScreen.Log(new LogMessage(LogSeverity.Error, "Discord.cs", $"Failed to load messages: {ex.Message}"));
                return [];
            }
        }
    }
}
