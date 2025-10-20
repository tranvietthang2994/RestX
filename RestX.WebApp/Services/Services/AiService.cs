using System.Text;
using System.Text.Json;
using RestX.WebApp.Models;
using RestX.WebApp.Services.Interfaces;

namespace RestX.WebApp.Services.Services
{
    public class AiService : BaseService, IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IDishService _dishService;
        private readonly ICategoryService _categoryService;
        private readonly IOwnerService _ownerService;
        private readonly string _apiKey;
        private readonly string _apiUrl;

        public AiService(
            IRepository repo, 
            IHttpContextAccessor httpContextAccessor,
            HttpClient httpClient,
            IConfiguration configuration,
            IDishService dishService,
            ICategoryService categoryService,
            IOwnerService ownerService) 
            : base(repo, httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _dishService = dishService;
            _categoryService = categoryService;
            _ownerService = ownerService;
            _apiKey = _configuration["GeminiApi:ApiKey"];
            _apiUrl = _configuration["GeminiApi:Url"];
        }

        public async Task<string> GetDishSuggestionsAsync(string userMessage, Guid ownerId)
        {
            try
            {
                // Get restaurant's menu data
                var dishes = await _dishService.GetDishesByOwnerIdAsync();
                var categories = await _categoryService.GetCategoriesAsync();
                
                // Get restaurant information
                var owner = await _ownerService.GetOwnerByIdAsync(ownerId);
                var restaurantName = owner?.Name ?? "Nhà hàng";

                // Build context about the restaurant's menu
                var menuContext = BuildMenuContext(dishes, categories);

                // Create AI prompt for dish suggestions
                var prompt = $@"Bạn là một AI assistant thông minh cho {restaurantName}. Dựa vào thực đơn của nhà hàng và yêu cầu của khách hàng, hãy gợi ý những món ăn phù hợp.

THỰC ĐƠN NHÀ HÀNG:
{menuContext}

YÊU CẦU KHÁCH HÀNG: {userMessage}

Hãy trả lời bằng tiếng Việt và:
1. Gợi ý 2-3 món ăn phù hợp nhất từ thực đơn
2. Giải thích tại sao gợi ý những món này
3. Đưa ra thông tin về giá cả
4. Có thể gợi ý combo hoặc thức uống kèm theo
5. Sử dụng emoji để làm câu trả lời sinh động hơn

Trả lời một cách thân thiện và chuyên nghiệp như một nhân viên tư vấn giỏi.";

                return await CallGeminiApiAsync(prompt);
            }
            catch (Exception ex)
            {
                return $"Xin lỗi, hiện tại hệ thống AI đang gặp sự cố. Vui lòng thử lại sau. ❌\n\nLỗi: {ex.Message}";
            }
        }

        public async Task<string> ChatWithAiAsync(string userMessage, string conversationHistory)
        {
            try
            {
                var prompt = $@"Bạn là một AI assistant thân thiện cho nhà hàng. Hãy trả lời câu hỏi của khách hàng một cách nhiệt tình và hữu ích.

LỊCH SỬ HỘI THOẠI:
{conversationHistory}

KHÁCH HÀNG: {userMessage}

Hãy trả lời bằng tiếng Việt, thân thiện và sử dụng emoji phù hợp.";

                return await CallGeminiApiAsync(prompt);
            }
            catch (Exception ex)
            {
                return $"Xin lỗi, tôi đang gặp sự cố kỹ thuật. Vui lòng thử lại sau. ❌\n\nLỗi: {ex.Message}";
            }
        }

        private string BuildMenuContext(List<Dish> dishes, List<Category> categories)
        {
            var menuBuilder = new StringBuilder();
            
            foreach (var category in categories)
            {
                var categoryDishes = dishes.Where(d => d.CategoryId == category.Id).ToList();
                if (categoryDishes.Any())
                {
                    menuBuilder.AppendLine($"\n📂 {category.Name.ToUpper()}:");
                    foreach (var dish in categoryDishes)
                    {
                        menuBuilder.AppendLine($"   • {dish.Name} - {dish.Price:N0}đ");
                        if (!string.IsNullOrEmpty(dish.Description))
                        {
                            menuBuilder.AppendLine($"     Mô tả: {dish.Description}");
                        }
                    }
                }
            }

            return menuBuilder.ToString();
        }

        private async Task<string> CallGeminiApiAsync(string prompt)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return "⚠️ Chưa cấu hình API key cho Gemini AI. Vui lòng liên hệ quản trị viên.";
            }

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    topP = 0.8,
                    topK = 40,
                    maxOutputTokens = 1024
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_apiUrl}?key={_apiKey}", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent);
                
                return geminiResponse?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text 
                       ?? "Xin lỗi, tôi không thể tạo phản hồi lúc này. 😔";
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return $"⚠️ Lỗi API: {response.StatusCode}\n{errorContent}";
            }
        }
    }

    // Gemini API Response Models
    public class GeminiResponse
    {
        public List<GeminiCandidate> candidates { get; set; }
    }

    public class GeminiCandidate
    {
        public GeminiContent content { get; set; }
    }

    public class GeminiContent
    {
        public List<GeminiPart> parts { get; set; }
    }

    public class GeminiPart
    {
        public string text { get; set; }
    }
} 