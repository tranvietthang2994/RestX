namespace RestX.WebApp.Services.Interfaces
{
    public interface IAiService
    {
        Task<string> GetDishSuggestionsAsync(string userMessage, Guid ownerId);
        Task<string> ChatWithAiAsync(string userMessage, string conversationHistory);
    }
} 