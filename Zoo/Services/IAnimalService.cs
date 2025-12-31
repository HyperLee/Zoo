namespace Zoo.Services;

/// <summary>
/// 動物服務介面，提供動物資料的存取功能
/// </summary>
public interface IAnimalService
{
    /// <summary>
    /// 取得所有動物
    /// </summary>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>所有動物的集合</returns>
    /// <example>
    /// <code>
    /// var animals = await animalService.GetAllAsync();
    /// foreach (var animal in animals)
    /// {
    ///     Console.WriteLine(animal.ChineseName);
    /// }
    /// </code>
    /// </example>
    Task<IReadOnlyList<Models.Animal>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 根據 ID 取得單一動物
    /// </summary>
    /// <param name="id">動物的唯一識別碼</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>找到的動物，若不存在則回傳 null</returns>
    /// <example>
    /// <code>
    /// var animal = await animalService.GetByIdAsync("lion-001");
    /// if (animal is not null)
    /// {
    ///     Console.WriteLine($"找到動物: {animal.ChineseName}");
    /// }
    /// </code>
    /// </example>
    Task<Models.Animal?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 取得指定動物的相關動物
    /// </summary>
    /// <param name="animalId">動物的唯一識別碼</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>相關動物的集合</returns>
    /// <example>
    /// <code>
    /// var relatedAnimals = await animalService.GetRelatedAsync("lion-001");
    /// Console.WriteLine($"相關動物數量: {relatedAnimals.Count}");
    /// </code>
    /// </example>
    Task<IReadOnlyList<Models.Animal>> GetRelatedAsync(string animalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 取得精選動物（用於首頁展示）
    /// </summary>
    /// <param name="count">要取得的動物數量</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>精選動物的集合</returns>
    /// <example>
    /// <code>
    /// var featured = await animalService.GetFeaturedAsync(3);
    /// // 回傳 3 隻精選動物
    /// </code>
    /// </example>
    Task<IReadOnlyList<Models.Animal>> GetFeaturedAsync(int count = 3, CancellationToken cancellationToken = default);
}
