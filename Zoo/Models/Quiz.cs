using System.Text.Json.Serialization;

namespace Zoo.Models;

/// <summary>
/// 測驗題目
/// </summary>
public class Quiz
{
    /// <summary>
    /// 唯一識別碼
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// 關聯動物 ID
    /// </summary>
    public required string AnimalId { get; init; }

    /// <summary>
    /// 題目類型
    /// </summary>
    public required QuizType Type { get; init; }

    /// <summary>
    /// 題目內容 (中文)
    /// </summary>
    public required string QuestionZh { get; init; }

    /// <summary>
    /// 題目內容 (英文)
    /// </summary>
    public required string QuestionEn { get; init; }

    /// <summary>
    /// 選項 (選擇題用)
    /// </summary>
    public IReadOnlyList<QuizOption>? Options { get; init; }

    /// <summary>
    /// 正確答案索引 (選擇題) 或 true/false (是非題)
    /// </summary>
    /// <remarks>
    /// 選擇題: 整數索引 (0-based)
    /// 是非題: 布林值
    /// </remarks>
    [JsonPropertyName("answer")]
    public required object Answer { get; init; }

    /// <summary>
    /// 答對回饋 (中文)
    /// </summary>
    public required string CorrectFeedbackZh { get; init; }

    /// <summary>
    /// 答對回饋 (英文)
    /// </summary>
    public required string CorrectFeedbackEn { get; init; }
}

/// <summary>
/// 測驗選項
/// </summary>
public class QuizOption
{
    /// <summary>
    /// 選項文字 (中文)
    /// </summary>
    public required string TextZh { get; init; }

    /// <summary>
    /// 選項文字 (英文)
    /// </summary>
    public required string TextEn { get; init; }
}
