namespace Zoo.Services;

/// <summary>
/// 區域服務介面，提供園區區域資料的存取功能
/// </summary>
public interface IZoneService
{
    /// <summary>
    /// 取得所有區域
    /// </summary>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>所有區域的集合</returns>
    /// <example>
    /// <code>
    /// var zones = await zoneService.GetAllAsync();
    /// foreach (var zone in zones)
    /// {
    ///     Console.WriteLine(zone.NameZh);
    /// }
    /// </code>
    /// </example>
    Task<IReadOnlyList<Models.Zone>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 根據 ID 取得單一區域
    /// </summary>
    /// <param name="id">區域的唯一識別碼</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>找到的區域，若不存在則回傳 null</returns>
    /// <example>
    /// <code>
    /// var zone = await zoneService.GetByIdAsync("africa-zone");
    /// if (zone is not null)
    /// {
    ///     Console.WriteLine($"找到區域: {zone.NameZh}");
    /// }
    /// </code>
    /// </example>
    Task<Models.Zone?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 取得指定區域內的所有動物
    /// </summary>
    /// <param name="zoneId">區域的唯一識別碼</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>該區域內的動物集合</returns>
    /// <example>
    /// <code>
    /// var animals = await zoneService.GetAnimalsByZoneAsync("africa-zone");
    /// Console.WriteLine($"區域內動物數量: {animals.Count}");
    /// </code>
    /// </example>
    Task<IReadOnlyList<Models.Animal>> GetAnimalsByZoneAsync(string zoneId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 取得所有區域及其動物數量
    /// </summary>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>區域與動物數量的字典</returns>
    /// <example>
    /// <code>
    /// var zoneStats = await zoneService.GetZoneAnimalCountsAsync();
    /// foreach (var (zone, count) in zoneStats)
    /// {
    ///     Console.WriteLine($"{zone.NameZh}: {count} 隻動物");
    /// }
    /// </code>
    /// </example>
    Task<IReadOnlyDictionary<Models.Zone, int>> GetZoneAnimalCountsAsync(CancellationToken cancellationToken = default);
}
