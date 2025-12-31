using System.Text.Json;
using Zoo.Models;

namespace Zoo.Services;

/// <summary>
/// 測驗服務實作，提供測驗題目的存取與答案驗證功能
/// </summary>
public class QuizService : IQuizService
{
    private readonly IJsonDataService _jsonDataService;
    private readonly ILogger<QuizService> _logger;
    private static readonly Random _random = new();

    /// <summary>
    /// 初始化測驗服務
    /// </summary>
    /// <param name="jsonDataService">JSON 資料服務</param>
    /// <param name="logger">日誌記錄器</param>
    public QuizService(IJsonDataService jsonDataService, ILogger<QuizService> logger)
    {
        _jsonDataService = jsonDataService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Quiz>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("取得所有測驗題目");

        var quizzes = await _jsonDataService.LoadAsync<Quiz>(
            "quizzes.json",
            "quizzes",
            cancellationToken);

        _logger.LogInformation("成功取得 {Count} 道測驗題目", quizzes.Count);

        return quizzes;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Quiz>> GetByAnimalIdAsync(string animalId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(animalId))
        {
            _logger.LogWarning("動物 ID 不可為空");
            return [];
        }

        _logger.LogDebug("根據動物 ID 取得測驗題目: {AnimalId}", animalId);

        var allQuizzes = await GetAllAsync(cancellationToken);
        var quizzes = allQuizzes
            .Where(q => q.AnimalId.Equals(animalId, StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();

        if (quizzes.Count == 0)
        {
            _logger.LogWarning("找不到動物 {AnimalId} 的測驗題目", animalId);
        }
        else
        {
            _logger.LogDebug("成功取得動物 {AnimalId} 的 {Count} 道測驗題目", animalId, quizzes.Count);
        }

        return quizzes;
    }

    /// <inheritdoc />
    public async Task<Quiz?> GetByIdAsync(string quizId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(quizId))
        {
            _logger.LogWarning("測驗 ID 不可為空");
            return null;
        }

        _logger.LogDebug("根據 ID 取得測驗題目: {QuizId}", quizId);

        var allQuizzes = await GetAllAsync(cancellationToken);
        var quiz = allQuizzes.FirstOrDefault(q => q.Id.Equals(quizId, StringComparison.OrdinalIgnoreCase));

        if (quiz is null)
        {
            _logger.LogWarning("找不到 ID 為 {QuizId} 的測驗題目", quizId);
        }
        else
        {
            _logger.LogDebug("成功取得測驗題目: {QuizId}", quiz.Id);
        }

        return quiz;
    }

    /// <inheritdoc />
    public async Task<QuizAnswerResult?> ValidateAnswerAsync(string quizId, object answer, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(quizId))
        {
            _logger.LogWarning("測驗 ID 不可為空");
            return null;
        }

        _logger.LogDebug("驗證測驗答案: {QuizId}, 答案: {Answer}", quizId, answer);

        var quiz = await GetByIdAsync(quizId, cancellationToken);

        if (quiz is null)
        {
            return null;
        }

        var isCorrect = IsAnswerCorrect(quiz, answer);

        _logger.LogInformation(
            "測驗 {QuizId} 答案驗證結果: {IsCorrect}，使用者答案: {UserAnswer}，正確答案: {CorrectAnswer}",
            quizId, isCorrect, answer, quiz.Answer);

        return new QuizAnswerResult
        {
            IsCorrect = isCorrect,
            CorrectAnswer = quiz.Answer,
            FeedbackZh = quiz.CorrectFeedbackZh,
            FeedbackEn = quiz.CorrectFeedbackEn
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Quiz>> GetRandomAsync(int count, CancellationToken cancellationToken = default)
    {
        if (count <= 0)
        {
            _logger.LogWarning("題目數量必須大於 0");
            return [];
        }

        _logger.LogDebug("取得 {Count} 道隨機測驗題目", count);

        var allQuizzes = await GetAllAsync(cancellationToken);

        if (allQuizzes.Count == 0)
        {
            _logger.LogWarning("沒有可用的測驗題目");
            return [];
        }

        var shuffled = allQuizzes
            .OrderBy(_ => _random.Next())
            .Take(count)
            .ToList()
            .AsReadOnly();

        _logger.LogInformation("成功取得 {Count} 道隨機測驗題目", shuffled.Count);

        return shuffled;
    }

    /// <summary>
    /// 判斷使用者答案是否正確
    /// </summary>
    /// <param name="quiz">測驗題目</param>
    /// <param name="userAnswer">使用者答案</param>
    /// <returns>是否正確</returns>
    private static bool IsAnswerCorrect(Quiz quiz, object userAnswer)
    {
        // 處理 JsonElement 類型（從 API 請求反序列化時）
        if (userAnswer is JsonElement jsonElement)
        {
            userAnswer = jsonElement.ValueKind switch
            {
                JsonValueKind.Number => jsonElement.GetInt32(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String => jsonElement.GetString() ?? string.Empty,
                _ => userAnswer
            };
        }

        // 正確答案也可能是 JsonElement
        var correctAnswer = quiz.Answer;
        if (correctAnswer is JsonElement correctJsonElement)
        {
            correctAnswer = correctJsonElement.ValueKind switch
            {
                JsonValueKind.Number => correctJsonElement.GetInt32(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String => correctJsonElement.GetString() ?? string.Empty,
                _ => correctAnswer
            };
        }

        return quiz.Type switch
        {
            QuizType.MultipleChoice => CompareMultipleChoiceAnswer(correctAnswer, userAnswer),
            QuizType.TrueFalse => CompareTrueFalseAnswer(correctAnswer, userAnswer),
            _ => false
        };
    }

    /// <summary>
    /// 比較選擇題答案
    /// </summary>
    private static bool CompareMultipleChoiceAnswer(object correctAnswer, object userAnswer)
    {
        // 嘗試將兩個值轉換為整數進行比較
        if (TryConvertToInt(correctAnswer, out var correctIndex) && 
            TryConvertToInt(userAnswer, out var userIndex))
        {
            return correctIndex == userIndex;
        }

        return false;
    }

    /// <summary>
    /// 比較是非題答案
    /// </summary>
    private static bool CompareTrueFalseAnswer(object correctAnswer, object userAnswer)
    {
        // 嘗試將兩個值轉換為布林值進行比較
        if (TryConvertToBool(correctAnswer, out var correctBool) && 
            TryConvertToBool(userAnswer, out var userBool))
        {
            return correctBool == userBool;
        }

        return false;
    }

    /// <summary>
    /// 嘗試將值轉換為整數
    /// </summary>
    private static bool TryConvertToInt(object value, out int result)
    {
        result = 0;

        return value switch
        {
            int i => (result = i) == i,
            long l => (result = (int)l) == l,
            double d => (result = (int)d) == d,
            string s => int.TryParse(s, out result),
            _ => false
        };
    }

    /// <summary>
    /// 嘗試將值轉換為布林值
    /// </summary>
    private static bool TryConvertToBool(object value, out bool result)
    {
        result = false;

        switch (value)
        {
            case bool b:
                result = b;
                return true;
            case string s:
                return bool.TryParse(s, out result);
            case int i:
                result = i != 0;
                return true;
            default:
                return false;
        }
    }
}
