# Research: 動物園導覽系統

**Feature Branch**: `001-zoo-guide-system`  
**Created**: 2025-12-31  
**Status**: Complete

## 研究摘要

本文件記錄動物園導覽系統技術選型與最佳實踐的研究結果。

---

## 1. JSON 檔案資料儲存策略

### Decision: 使用 System.Text.Json 配合檔案緩存

**Rationale**: 
- System.Text.Json 是 .NET 內建的高效能 JSON 序列化函式庫
- 相比 Newtonsoft.Json，效能更佳且無需額外相依套件
- 支援 Source Generator 進一步提升效能
- 透過 IMemoryCache 緩存讀取的 JSON 資料，避免重複 I/O 操作

**Alternatives Considered**:

| 選項 | 優點 | 缺點 | 結論 |
| --- | --- | --- | --- |
| Newtonsoft.Json | 功能豐富、社群熟悉 | 額外相依、效能較差 | 不採用 |
| LiteDB | 嵌入式 NoSQL、支援 LINQ | 增加複雜度、需學習成本 | 不採用 |
| SQLite + EF Core | 關聯式查詢、EF Core 支援 | 需資料庫設定、違反簡單原則 | 不採用 |
| **System.Text.Json** | 內建、高效能、免相依 | 功能較 Newtonsoft 少 | **採用** |

**Implementation Pattern**:

```csharp
public class JsonDataService<T> where T : class
{
    private readonly IMemoryCache _cache;
    private readonly string _filePath;
    
    public async Task<IReadOnlyList<T>> GetAllAsync()
    {
        return await _cache.GetOrCreateAsync($"json_{typeof(T).Name}", async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(30);
            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<T>>(json) ?? [];
        }) ?? [];
    }
}
```

---

## 2. Serilog 整合最佳實踐

### Decision: 使用 Serilog 搭配 Console 與 File Sink

**Rationale**: 
- Serilog 是 .NET 生態系中最受歡迎的結構化日誌函式庫
- 支援多種 Sink（輸出目標）與豐富的格式化選項
- 與 ASP.NET Core 完美整合，支援 Request Logging Middleware
- 符合憲章 V. 可觀察性與監控原則

**Required Packages**:
- `Serilog.AspNetCore` (主要套件，整合 ASP.NET Core)
- `Serilog.Sinks.Console` (控制台輸出)
- `Serilog.Sinks.File` (檔案輸出)

**Configuration Approach**:

```csharp
// Program.cs
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/zoo-.log", 
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

builder.Host.UseSerilog();
```

---

## 3. ASP.NET Core 10.0 Razor Pages 架構

### Decision: 採用 Razor Pages 而非 MVC 或 Minimal APIs

**Rationale**: 
- Razor Pages 適合頁面導向的應用程式（如本專案的導覽網站）
- 每個頁面有獨立的 PageModel，邏輯清晰易維護
- 相比 MVC，減少 Controller 的間接層
- 相比 Minimal APIs，更適合傳統網頁應用程式

**Alternatives Considered**:

| 選項 | 適用場景 | 本專案評估 |
| --- | --- | --- |
| MVC | 複雜的 Web 應用程式 | 過度設計 |
| Minimal APIs | API 優先、微服務 | 不適合網頁導覽 |
| **Razor Pages** | 頁面導向、CRUD | **最適合** |
| Blazor Server | SPA 互動體驗 | 增加複雜度 |

---

## 4. 搜尋功能實作策略

### Decision: 使用 LINQ 記憶體搜尋

**Rationale**: 
- 資料量小（50 種動物），記憶體搜尋效能足夠
- 無需引入全文搜尋引擎（如 Lucene.NET、Elasticsearch）
- 支援模糊搜尋使用 `Contains()` 即可滿足需求
- 可透過 IMemoryCache 緩存搜尋結果

**Implementation Pattern**:

```csharp
public class SearchService : ISearchService
{
    public async Task<IEnumerable<Animal>> SearchAsync(string keyword)
    {
        var animals = await _animalService.GetAllAsync();
        
        if (string.IsNullOrWhiteSpace(keyword))
            return animals;
            
        var lowerKeyword = keyword.ToLowerInvariant();
        
        return animals.Where(a => 
            a.ChineseName.Contains(keyword) ||
            a.EnglishName.Contains(lowerKeyword, StringComparison.OrdinalIgnoreCase) ||
            a.ScientificName.Contains(lowerKeyword, StringComparison.OrdinalIgnoreCase));
    }
}
```

---

## 5. 靜態地圖實作

### Decision: 使用 SVG 圖片配合 JavaScript 互動

**Rationale**: 
- SVG 支援無損縮放，適合地圖展示
- 可為每個區域定義 `<path>` 或 `<polygon>` 元素
- 透過 JavaScript 監聽點擊事件，顯示區域資訊
- 無需引入複雜的地圖函式庫（如 Leaflet、OpenLayers）

**Implementation Approach**:
1. 設計師建立園區 SVG 地圖
2. 每個區域設定 `data-zone-id` 屬性
3. JavaScript 處理點擊事件，載入區域動物清單
4. 使用 CSS 處理 hover 效果

---

## 6. 多語系支援策略

### Decision: 使用 ASP.NET Core 內建本地化功能

**Rationale**: 
- ASP.NET Core 提供完整的本地化支援
- 使用 `.resx` 資源檔管理翻譯文字
- 支援 URL 路由、Cookie、Accept-Language Header 切換語系
- 動物資料的多語系內容直接儲存於 JSON 中

**Required Setup**:
- `IStringLocalizer<T>` 用於 View 文字本地化
- 資源檔：`Resources/Pages/Index.zh-TW.resx`, `Resources/Pages/Index.en.resx`
- 動物 JSON 包含 `chineseName`, `englishName` 欄位

---

## 7. 瀏覽器本地儲存

### Decision: 使用 localStorage 儲存收藏與瀏覽記錄

**Rationale**: 
- 符合規格要求：使用者資料僅儲存於本地
- 無需後端資料庫支援
- API 簡單，跨頁面持久化
- 容量足夠（約 5MB）

**Implementation Pattern**:

```javascript
// 收藏管理
const favorites = {
    add: (animalId) => {
        const list = JSON.parse(localStorage.getItem('favorites') || '[]');
        if (!list.includes(animalId)) {
            list.push(animalId);
            localStorage.setItem('favorites', JSON.stringify(list));
        }
    },
    remove: (animalId) => {
        let list = JSON.parse(localStorage.getItem('favorites') || '[]');
        list = list.filter(id => id !== animalId);
        localStorage.setItem('favorites', JSON.stringify(list));
    },
    getAll: () => JSON.parse(localStorage.getItem('favorites') || '[]'),
    has: (animalId) => favorites.getAll().includes(animalId)
};
```

---

## 8. 無障礙設計 (WCAG 2.1 Level AA)

### Decision: 採用語意化 HTML + ARIA 屬性

**Key Implementation Points**:

| 要求 | 實作方式 |
| --- | --- |
| 鍵盤導航 | 所有互動元素使用 `tabindex`、`<button>`、`<a>` |
| 圖片替代文字 | 所有 `<img>` 提供 `alt` 屬性 |
| 對比度 | 文字與背景對比度 ≥ 4.5:1 |
| 表單標籤 | 所有輸入欄位使用 `<label>` 關聯 |
| Skip Link | 提供「跳至主要內容」連結 |
| 動態內容 | 使用 `aria-live` 通知螢幕閱讀器 |

---

## 9. 測試策略

### Decision: xUnit + Moq + WebApplicationFactory

**Test Categories**:

| 類型 | 目標 | 工具 |
| --- | --- | --- |
| 單元測試 | Services、Models | xUnit + Moq |
| 整合測試 | Razor Pages | WebApplicationFactory |
| 效能測試 | 頁面載入時間 | 瀏覽器 DevTools |

**Coverage Goals**:
- 關鍵業務邏輯（Services）: > 80%
- Razor Pages: 主要路徑覆蓋
- 邊界情況: 搜尋空字串、無結果、無效 ID

---

## 10. 效能優化策略

### Decision: 靜態資源緩存 + JSON 記憶體緩存

**Implementation**:
1. 靜態資源（CSS、JS、圖片）設定長期 Cache-Control
2. JSON 資料使用 `IMemoryCache` 緩存 30 分鐘
3. Response Compression 壓縮 HTML/CSS/JS
4. 圖片使用 WebP 格式（如瀏覽器支援）

```csharp
// Program.cs
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

app.UseResponseCompression();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=604800");
    }
});
```

---

## 研究結論

所有技術選型已確認，無 NEEDS CLARIFICATION 項目。專案採用簡單且成熟的技術堆疊：

- **後端**: ASP.NET Core 10.0 Razor Pages + System.Text.Json
- **日誌**: Serilog (Console + File)
- **前端**: Bootstrap 5 + jQuery + 原生 JavaScript
- **儲存**: JSON 檔案 + IMemoryCache
- **測試**: xUnit + Moq + WebApplicationFactory

此架構滿足小型教育網站的需求，同時保持簡單易維護的特性。
