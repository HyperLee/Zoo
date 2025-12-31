namespace Zoo.Services;

/// <summary>
/// 泛型 JSON 資料服務介面，提供從 JSON 檔案載入資料的功能
/// </summary>
public interface IJsonDataService
{
    /// <summary>
    /// 非同步載入指定類型的資料集合
    /// </summary>
    /// <typeparam name="T">資料類型</typeparam>
    /// <param name="fileName">JSON 檔案名稱（不含路徑）</param>
    /// <param name="rootPropertyName">JSON 根屬性名稱（用於從根物件提取陣列）</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>資料集合</returns>
    /// <example>
    /// <code>
    /// // 載入動物資料
    /// var animals = await jsonDataService.LoadAsync&lt;Animal&gt;("animals.json", "animals");
    /// 
    /// // 載入區域資料
    /// var zones = await jsonDataService.LoadAsync&lt;Zone&gt;("zones.json", "zones");
    /// </code>
    /// </example>
    Task<IReadOnlyList<T>> LoadAsync<T>(
        string fileName,
        string rootPropertyName,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// 清除指定檔案的快取
    /// </summary>
    /// <param name="fileName">JSON 檔案名稱</param>
    void ClearCache(string fileName);

    /// <summary>
    /// 清除所有快取
    /// </summary>
    void ClearAllCache();
}
