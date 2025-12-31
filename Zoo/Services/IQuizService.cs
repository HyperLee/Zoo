namespace Zoo.Services;

/// <summary>
/// 測驗服務介面，提供測驗題目的存取與答案驗證功能
/// </summary>
public interface IQuizService
{
    /// <summary>
    /// 根據動物 ID 取得測驗題目
    /// </summary>
    /// <param name="animalId">動物的唯一識別碼</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>該動物的測驗題目集合</returns>
    /// <example>
    /// <code>
    /// var quizzes = await quizService.GetByAnimalIdAsync("lion-001");
    /// foreach (var quiz in quizzes)
    /// {
    ///     Console.WriteLine(quiz.QuestionZh);
    /// }
    /// </code>
    /// </example>
    Task<IReadOnlyList<Models.Quiz>> GetByAnimalIdAsync(string animalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 取得所有測驗題目
    /// </summary>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>所有測驗題目的集合</returns>
    /// <example>
    /// <code>
    /// var allQuizzes = await quizService.GetAllAsync();
    /// Console.WriteLine($"總共有 {allQuizzes.Count} 道題目");
    /// </code>
    /// </example>
    Task<IReadOnlyList<Models.Quiz>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 根據 ID 取得單一測驗題目
    /// </summary>
    /// <param name="quizId">測驗題目的唯一識別碼</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>找到的測驗題目，若不存在則回傳 null</returns>
    /// <example>
    /// <code>
    /// var quiz = await quizService.GetByIdAsync("quiz-lion-001");
    /// if (quiz is not null)
    /// {
    ///     Console.WriteLine($"題目: {quiz.QuestionZh}");
    /// }
    /// </code>
    /// </example>
    Task<Models.Quiz?> GetByIdAsync(string quizId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 驗證測驗答案
    /// </summary>
    /// <param name="quizId">測驗題目的唯一識別碼</param>
    /// <param name="answer">使用者提交的答案（選擇題為選項索引，是非題為布林值）</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>答案驗證結果，若題目不存在則回傳 null</returns>
    /// <example>
    /// <code>
    /// var result = await quizService.ValidateAnswerAsync("quiz-lion-001", 2);
    /// if (result is not null)
    /// {
    ///     Console.WriteLine(result.IsCorrect ? "答對了！" : "答錯了！");
    /// }
    /// </code>
    /// </example>
    Task<QuizAnswerResult?> ValidateAnswerAsync(string quizId, object answer, CancellationToken cancellationToken = default);

    /// <summary>
    /// 取得隨機測驗題目
    /// </summary>
    /// <param name="count">要取得的題目數量</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>隨機選取的測驗題目集合</returns>
    /// <example>
    /// <code>
    /// var randomQuizzes = await quizService.GetRandomAsync(5);
    /// Console.WriteLine($"隨機取得 {randomQuizzes.Count} 道題目");
    /// </code>
    /// </example>
    Task<IReadOnlyList<Models.Quiz>> GetRandomAsync(int count, CancellationToken cancellationToken = default);
}

/// <summary>
/// 測驗答案驗證結果
/// </summary>
public class QuizAnswerResult
{
    /// <summary>
    /// 是否回答正確
    /// </summary>
    public required bool IsCorrect { get; init; }

    /// <summary>
    /// 正確答案（選擇題為索引，是非題為布林值）
    /// </summary>
    public required object CorrectAnswer { get; init; }

    /// <summary>
    /// 答題回饋 (中文)
    /// </summary>
    public required string FeedbackZh { get; init; }

    /// <summary>
    /// 答題回饋 (英文)
    /// </summary>
    public required string FeedbackEn { get; init; }
}
