# Data Model: 動物園導覽系統

**Feature Branch**: `001-zoo-guide-system`  
**Created**: 2025-12-31  
**Status**: Complete

## 實體關係圖 (概念)

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   Animal    │────▶│    Zone     │◀────│   Route     │
│  (動物)     │ N:1 │   (區域)    │ N:M │   (路線)    │
└─────────────┘     └─────────────┘     └─────────────┘
      │                    │
      │ 1:N                │ 1:N
      ▼                    ▼
┌─────────────┐     ┌─────────────┐
│    Quiz     │     │  Facility   │
│  (測驗)     │     │   (設施)    │
└─────────────┘     └─────────────┘
```

---

## 實體定義

### 1. Animal (動物)

系統核心實體，儲存動物的完整資訊。

```csharp
/// <summary>
/// 動物實體，包含動物的基本資訊、分類、多媒體資源等
/// </summary>
public class Animal
{
    /// <summary>唯一識別碼</summary>
    public required string Id { get; init; }
    
    /// <summary>中文名稱</summary>
    public required string ChineseName { get; init; }
    
    /// <summary>英文名稱</summary>
    public required string EnglishName { get; init; }
    
    /// <summary>學名 (拉丁文)</summary>
    public required string ScientificName { get; init; }
    
    /// <summary>所屬區域 ID</summary>
    public required string ZoneId { get; init; }
    
    /// <summary>分類資訊</summary>
    public required Classification Classification { get; init; }
    
    /// <summary>特徵描述</summary>
    public required Description Description { get; init; }
    
    /// <summary>趣味事實 (3-5 項)</summary>
    public required IReadOnlyList<string> FunFacts { get; init; }
    
    /// <summary>IUCN 保育等級</summary>
    public required ConservationStatus ConservationStatus { get; init; }
    
    /// <summary>多媒體資源</summary>
    public required MediaResources Media { get; init; }
    
    /// <summary>相關動物 ID 清單</summary>
    public IReadOnlyList<string> RelatedAnimalIds { get; init; } = [];
}
```

#### Classification (分類資訊)

```csharp
/// <summary>動物分類資訊</summary>
public class Classification
{
    /// <summary>生物分類 (哺乳類、鳥類、爬蟲類、兩棲類、魚類、無脊椎動物)</summary>
    public required BiologicalClass BiologicalClass { get; init; }
    
    /// <summary>棲息地 (熱帶雨林、沙漠、草原、極地、海洋、淡水、山區)</summary>
    public required Habitat Habitat { get; init; }
    
    /// <summary>飲食習性 (肉食性、草食性、雜食性)</summary>
    public required Diet Diet { get; init; }
    
    /// <summary>活動時間 (日行性、夜行性、晨昏性)</summary>
    public required ActivityPattern ActivityPattern { get; init; }
}
```

#### Description (特徵描述)

```csharp
/// <summary>動物特徵描述</summary>
public class Description
{
    /// <summary>體型描述 (如: 體長 2-3 公尺)</summary>
    public required string Size { get; init; }
    
    /// <summary>外觀特色</summary>
    public required string Appearance { get; init; }
    
    /// <summary>生活習性</summary>
    public required string Behavior { get; init; }
    
    /// <summary>完整介紹 (中文)</summary>
    public required string FullDescriptionZh { get; init; }
    
    /// <summary>完整介紹 (英文)</summary>
    public required string FullDescriptionEn { get; init; }
}
```

#### MediaResources (多媒體資源)

```csharp
/// <summary>動物多媒體資源</summary>
public class MediaResources
{
    /// <summary>圖片路徑清單 (至少 3 張動物森友會風格插畫)</summary>
    public required IReadOnlyList<string> Images { get; init; }
    
    /// <summary>叫聲音效路徑 (可選)</summary>
    public string? SoundPath { get; init; }
    
    /// <summary>縮圖路徑</summary>
    public required string ThumbnailPath { get; init; }
}
```

---

### 2. Zone (區域)

動物園的地理分區。

```csharp
/// <summary>園區區域</summary>
public class Zone
{
    /// <summary>唯一識別碼</summary>
    public required string Id { get; init; }
    
    /// <summary>區域名稱 (中文)</summary>
    public required string NameZh { get; init; }
    
    /// <summary>區域名稱 (英文)</summary>
    public required string NameEn { get; init; }
    
    /// <summary>區域描述</summary>
    public required string Description { get; init; }
    
    /// <summary>地圖上的座標 (SVG 座標系統)</summary>
    public required Coordinate Position { get; init; }
    
    /// <summary>SVG 路徑 ID (用於地圖點擊偵測)</summary>
    public required string SvgPathId { get; init; }
    
    /// <summary>區域顏色 (用於地圖顯示)</summary>
    public required string Color { get; init; }
}
```

---

### 3. Route (導覽路線)

預設導覽路線定義。

```csharp
/// <summary>導覽路線</summary>
public class Route
{
    /// <summary>唯一識別碼</summary>
    public required string Id { get; init; }
    
    /// <summary>路線名稱 (中文)</summary>
    public required string NameZh { get; init; }
    
    /// <summary>路線名稱 (英文)</summary>
    public required string NameEn { get; init; }
    
    /// <summary>路線類型</summary>
    public required RouteType Type { get; init; }
    
    /// <summary>路線描述</summary>
    public required string Description { get; init; }
    
    /// <summary>預估步行時間 (分鐘)</summary>
    public required int EstimatedMinutes { get; init; }
    
    /// <summary>途經區域 ID 清單 (依序)</summary>
    public required IReadOnlyList<string> ZoneIds { get; init; }
    
    /// <summary>途經動物 ID 清單 (依序)</summary>
    public required IReadOnlyList<string> AnimalIds { get; init; }
}
```

---

### 4. Facility (設施)

園區設施資訊。

```csharp
/// <summary>園區設施</summary>
public class Facility
{
    /// <summary>唯一識別碼</summary>
    public required string Id { get; init; }
    
    /// <summary>設施名稱 (中文)</summary>
    public required string NameZh { get; init; }
    
    /// <summary>設施名稱 (英文)</summary>
    public required string NameEn { get; init; }
    
    /// <summary>設施類型</summary>
    public required FacilityType Type { get; init; }
    
    /// <summary>所屬區域 ID</summary>
    public required string ZoneId { get; init; }
    
    /// <summary>地圖上的座標</summary>
    public required Coordinate Position { get; init; }
    
    /// <summary>圖示名稱</summary>
    public required string IconName { get; init; }
}
```

---

### 5. Quiz (測驗)

知識測驗題目。

```csharp
/// <summary>測驗題目</summary>
public class Quiz
{
    /// <summary>唯一識別碼</summary>
    public required string Id { get; init; }
    
    /// <summary>關聯動物 ID</summary>
    public required string AnimalId { get; init; }
    
    /// <summary>題目類型</summary>
    public required QuizType Type { get; init; }
    
    /// <summary>題目內容 (中文)</summary>
    public required string QuestionZh { get; init; }
    
    /// <summary>題目內容 (英文)</summary>
    public required string QuestionEn { get; init; }
    
    /// <summary>選項 (選擇題用)</summary>
    public IReadOnlyList<QuizOption>? Options { get; init; }
    
    /// <summary>正確答案索引 (選擇題) 或 true/false (是非題)</summary>
    public required object Answer { get; init; }
    
    /// <summary>答對回饋 (中文)</summary>
    public required string CorrectFeedbackZh { get; init; }
    
    /// <summary>答對回饋 (英文)</summary>
    public required string CorrectFeedbackEn { get; init; }
}

/// <summary>測驗選項</summary>
public class QuizOption
{
    /// <summary>選項文字 (中文)</summary>
    public required string TextZh { get; init; }
    
    /// <summary>選項文字 (英文)</summary>
    public required string TextEn { get; init; }
}
```

---

## 列舉定義

```csharp
/// <summary>生物分類</summary>
public enum BiologicalClass
{
    Mammal,         // 哺乳類
    Bird,           // 鳥類
    Reptile,        // 爬蟲類
    Amphibian,      // 兩棲類
    Fish,           // 魚類
    Invertebrate    // 無脊椎動物
}

/// <summary>棲息地</summary>
public enum Habitat
{
    TropicalRainforest,  // 熱帶雨林
    Desert,              // 沙漠
    Grassland,           // 草原
    Polar,               // 極地
    Ocean,               // 海洋
    Freshwater,          // 淡水
    Mountain             // 山區
}

/// <summary>飲食習性</summary>
public enum Diet
{
    Carnivore,   // 肉食性
    Herbivore,   // 草食性
    Omnivore     // 雜食性
}

/// <summary>活動時間</summary>
public enum ActivityPattern
{
    Diurnal,     // 日行性
    Nocturnal,   // 夜行性
    Crepuscular  // 晨昏性
}

/// <summary>IUCN 保育等級</summary>
public enum ConservationStatus
{
    LC,  // 無危 (Least Concern)
    NT,  // 近危 (Near Threatened)
    VU,  // 易危 (Vulnerable)
    EN,  // 瀕危 (Endangered)
    CR,  // 極危 (Critically Endangered)
    EW,  // 野外滅絕 (Extinct in the Wild)
    EX   // 滅絕 (Extinct)
}

/// <summary>路線類型</summary>
public enum RouteType
{
    Complete,       // 完整路線
    Highlights,     // 精華路線
    FamilyFriendly, // 親子路線
    Photography,    // 攝影路線
    Themed          // 主題路線
}

/// <summary>設施類型</summary>
public enum FacilityType
{
    RestArea,       // 休息區
    Restroom,       // 洗手間
    ServiceCenter,  // 服務中心
    FirstAid,       // 急救站
    FoodCourt,      // 餐飲區
    GiftShop        // 紀念品店
}

/// <summary>測驗題目類型</summary>
public enum QuizType
{
    MultipleChoice,  // 選擇題
    TrueFalse        // 是非題
}
```

---

## 共用類型

```csharp
/// <summary>座標</summary>
public record Coordinate(double X, double Y);
```

---

## JSON 資料結構範例

### animals.json

```json
{
  "animals": [
    {
      "id": "lion-001",
      "chineseName": "非洲獅",
      "englishName": "African Lion",
      "scientificName": "Panthera leo",
      "zoneId": "africa-zone",
      "classification": {
        "biologicalClass": "Mammal",
        "habitat": "Grassland",
        "diet": "Carnivore",
        "activityPattern": "Crepuscular"
      },
      "description": {
        "size": "體長 1.4-2.5 公尺，體重 120-250 公斤",
        "appearance": "雄獅擁有標誌性的鬃毛，毛色從金黃到棕色",
        "behavior": "群居動物，由雌獅負責狩獵",
        "fullDescriptionZh": "非洲獅是現存最大的貓科動物之一...",
        "fullDescriptionEn": "The African lion is one of the largest cats..."
      },
      "funFacts": [
        "獅子每天可睡眠長達 20 小時",
        "獅吼可傳達 8 公里遠",
        "雌獅是主要的狩獵者"
      ],
      "conservationStatus": "VU",
      "media": {
        "images": [
          "/images/animals/lion-001-1.webp",
          "/images/animals/lion-001-2.webp",
          "/images/animals/lion-001-3.webp"
        ],
        "soundPath": "/audio/lion-roar.mp3",
        "thumbnailPath": "/images/animals/lion-001-thumb.webp"
      },
      "relatedAnimalIds": ["leopard-001", "cheetah-001"]
    }
  ]
}
```

---

## 驗證規則

| 實體 | 欄位 | 規則 |
| --- | --- | --- |
| Animal | Id | 必填，唯一，格式: `{species}-{number}` |
| Animal | ChineseName | 必填，1-50 字元 |
| Animal | EnglishName | 必填，1-100 字元 |
| Animal | FunFacts | 必填，至少 3 項，最多 5 項 |
| Animal | Media.Images | 必填，至少 3 張圖片 |
| Zone | Id | 必填，唯一 |
| Route | EstimatedMinutes | 必填，正整數 |
| Quiz | Options | 選擇題時必填，至少 2 選項 |

---

## 狀態轉換

本系統為純展示性質，實體無狀態轉換需求。使用者的收藏與瀏覽狀態儲存於瀏覽器 localStorage，不影響後端資料。
