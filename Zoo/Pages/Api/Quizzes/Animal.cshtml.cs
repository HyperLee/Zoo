using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Zoo.Services;

namespace Zoo.Pages.Api.Quizzes;

/// <summary>
/// 取得指定動物的測驗題目 API 端點
/// GET /api/quizzes/animal/{animalId}
/// </summary>
public class AnimalModel : PageModel
{
    private readonly IQuizService _quizService;
    private readonly ILogger<AnimalModel> _logger;

    /// <summary>
    /// 初始化動物測驗 API
    /// </summary>
    /// <param name="quizService">測驗服務</param>
    /// <param name="logger">日誌記錄器</param>
    public AnimalModel(IQuizService quizService, ILogger<AnimalModel> logger)
    {
        _quizService = quizService;
        _logger = logger;
    }

    /// <summary>
    /// 處理取得動物測驗題目的請求
    /// </summary>
    /// <param name="animalId">動物 ID</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>測驗題目清單的 JSON 回應</returns>
    /// <response code="200">成功取得測驗題目</response>
    /// <response code="400">動物 ID 無效</response>
    public async Task<IActionResult> OnGetAsync(string? animalId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(animalId))
        {
            _logger.LogWarning("收到無效的動物 ID 請求");

            return new JsonResult(new
            {
                type = "https://tools.ietf.org/html/rfc7807",
                title = "Bad Request",
                status = 400,
                detail = "動物 ID 不可為空",
                instance = HttpContext.Request.Path.ToString()
            })
            {
                StatusCode = 400
            };
        }

        _logger.LogDebug("收到取得動物 {AnimalId} 測驗題目的請求", animalId);

        var quizzes = await _quizService.GetByAnimalIdAsync(animalId, cancellationToken);

        _logger.LogInformation("取得動物 {AnimalId} 的 {Count} 道測驗題目", animalId, quizzes.Count);

        return new JsonResult(new
        {
            quizzes = quizzes.Select(q => new
            {
                id = q.Id,
                type = q.Type.ToString(),
                questionZh = q.QuestionZh,
                questionEn = q.QuestionEn,
                options = q.Options?.Select(o => new
                {
                    textZh = o.TextZh,
                    textEn = o.TextEn
                })
            })
        });
    }
}
