using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Zoo.Services;

namespace Zoo.Pages.Api.Quizzes;

/// <summary>
/// 測驗答案驗證 API 端點
/// POST /api/quizzes/{quizId}/answer
/// </summary>
public class AnswerModel : PageModel
{
    private readonly IQuizService _quizService;
    private readonly ILogger<AnswerModel> _logger;

    /// <summary>
    /// 初始化答案驗證 API
    /// </summary>
    /// <param name="quizService">測驗服務</param>
    /// <param name="logger">日誌記錄器</param>
    public AnswerModel(IQuizService quizService, ILogger<AnswerModel> logger)
    {
        _quizService = quizService;
        _logger = logger;
    }

    /// <summary>
    /// 處理測驗答案提交
    /// </summary>
    /// <param name="quizId">測驗題目 ID</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>答案驗證結果的 JSON 回應</returns>
    /// <response code="200">答案驗證成功</response>
    /// <response code="400">請求參數無效</response>
    /// <response code="404">找不到測驗題目</response>
    public async Task<IActionResult> OnPostAsync(string? quizId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(quizId))
        {
            _logger.LogWarning("收到無效的測驗 ID 請求");

            return new JsonResult(new
            {
                type = "https://tools.ietf.org/html/rfc7807",
                title = "Bad Request",
                status = 400,
                detail = "測驗 ID 不可為空",
                instance = HttpContext.Request.Path.ToString()
            })
            {
                StatusCode = 400
            };
        }

        // 讀取請求主體
        AnswerRequest? answerRequest;
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(body))
            {
                _logger.LogWarning("請求主體為空");

                return new JsonResult(new
                {
                    type = "https://tools.ietf.org/html/rfc7807",
                    title = "Bad Request",
                    status = 400,
                    detail = "請求主體不可為空",
                    instance = HttpContext.Request.Path.ToString()
                })
                {
                    StatusCode = 400
                };
            }

            answerRequest = JsonSerializer.Deserialize<AnswerRequest>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "解析請求主體失敗");

            return new JsonResult(new
            {
                type = "https://tools.ietf.org/html/rfc7807",
                title = "Bad Request",
                status = 400,
                detail = "無效的 JSON 格式",
                instance = HttpContext.Request.Path.ToString()
            })
            {
                StatusCode = 400
            };
        }

        if (answerRequest?.Answer is null)
        {
            _logger.LogWarning("答案欄位為空");

            return new JsonResult(new
            {
                type = "https://tools.ietf.org/html/rfc7807",
                title = "Bad Request",
                status = 400,
                detail = "答案欄位不可為空",
                instance = HttpContext.Request.Path.ToString()
            })
            {
                StatusCode = 400
            };
        }

        _logger.LogDebug("收到測驗 {QuizId} 的答案驗證請求，答案: {Answer}", quizId, answerRequest.Answer);

        var result = await _quizService.ValidateAnswerAsync(quizId, answerRequest.Answer, cancellationToken);

        if (result is null)
        {
            _logger.LogWarning("找不到測驗 {QuizId}", quizId);

            return new JsonResult(new
            {
                type = "https://tools.ietf.org/html/rfc7807",
                title = "Not Found",
                status = 404,
                detail = $"找不到 ID 為 '{quizId}' 的測驗題目",
                instance = HttpContext.Request.Path.ToString()
            })
            {
                StatusCode = 404
            };
        }

        _logger.LogInformation("測驗 {QuizId} 答案驗證完成，結果: {IsCorrect}", quizId, result.IsCorrect);

        return new JsonResult(new
        {
            correct = result.IsCorrect,
            correctAnswer = result.CorrectAnswer,
            feedbackZh = result.FeedbackZh,
            feedbackEn = result.FeedbackEn
        });
    }

    /// <summary>
    /// 答案請求模型
    /// </summary>
    private sealed class AnswerRequest
    {
        /// <summary>
        /// 使用者提交的答案
        /// </summary>
        public object? Answer { get; set; }
    }
}
