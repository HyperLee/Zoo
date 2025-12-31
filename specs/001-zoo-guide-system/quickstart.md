# Quick Start: 動物園導覽系統

**Feature Branch**: `001-zoo-guide-system`  
**Created**: 2025-12-31

## 環境需求

- **.NET SDK**: 10.0 或更高版本
- **IDE**: Visual Studio 2022 17.12+ / VS Code + C# DevKit
- **瀏覽器**: Chrome、Firefox、Edge 或 Safari 最新版本
- **解析度**: 最低 1366x768

---

## 快速開始

### 1. 複製專案

```powershell
git clone https://github.com/HyperLee/Zoo.git
cd Zoo
git checkout 001-zoo-guide-system
```

### 2. 還原相依套件

```powershell
dotnet restore
```

### 3. 建構專案

```powershell
dotnet build
```

### 4. 執行應用程式

```powershell
dotnet run --project Zoo
```

應用程式將於 `https://localhost:5001` 或 `http://localhost:5000` 啟動。

---

## 專案結構總覽

```
Zoo/                           # 主專案
├── Program.cs                 # 應用程式進入點
├── appsettings.json           # 設定檔
├── Data/                      # JSON 資料檔案
│   ├── animals.json
│   ├── zones.json
│   └── ...
├── Models/                    # 資料模型
├── Services/                  # 業務邏輯
├── Pages/                     # Razor Pages
└── wwwroot/                   # 靜態資源

Zoo.Tests/                     # 測試專案
├── Unit/
└── Integration/
```

---

## 設定說明

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/zoo-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      }
    ]
  },
  "DataPaths": {
    "Animals": "Data/animals.json",
    "Zones": "Data/zones.json",
    "Routes": "Data/routes.json",
    "Facilities": "Data/facilities.json",
    "Quizzes": "Data/quizzes.json"
  }
}
```

---

## 開發指令

### 建構

```powershell
dotnet build
```

### 執行 (開發模式)

```powershell
dotnet watch --project Zoo
```

### 執行測試

```powershell
dotnet test
```

### 執行特定測試

```powershell
dotnet test --filter "FullyQualifiedName~AnimalServiceTests"
```

---

## 新增動物資料

1. 編輯 `Zoo/Data/animals.json`
2. 新增動物物件 (參考 [data-model.md](data-model.md))
3. 將動物圖片放置於 `Zoo/wwwroot/images/animals/`
4. 重新啟動應用程式

### 動物資料範例

```json
{
  "id": "penguin-001",
  "chineseName": "皇帝企鵝",
  "englishName": "Emperor Penguin",
  "scientificName": "Aptenodytes forsteri",
  "zoneId": "polar-zone",
  "classification": {
    "biologicalClass": "Bird",
    "habitat": "Polar",
    "diet": "Carnivore",
    "activityPattern": "Diurnal"
  },
  "description": {
    "size": "身高 100-130 公分，體重 22-45 公斤",
    "appearance": "黑白相間的羽毛，頸部有橙黃色斑紋",
    "behavior": "群居動物，由雄鳥孵蛋",
    "fullDescriptionZh": "皇帝企鵝是現存體型最大的企鵝...",
    "fullDescriptionEn": "The emperor penguin is the largest penguin species..."
  },
  "funFacts": [
    "皇帝企鵝可潛水至 500 公尺深",
    "孵蛋期間雄鳥不進食長達 2 個月",
    "企鵝的黑白色是一種偽裝"
  ],
  "conservationStatus": "NT",
  "media": {
    "images": [
      "/images/animals/penguin-001-1.webp",
      "/images/animals/penguin-001-2.webp",
      "/images/animals/penguin-001-3.webp"
    ],
    "soundPath": "/audio/penguin-call.mp3",
    "thumbnailPath": "/images/animals/penguin-001-thumb.webp"
  },
  "relatedAnimalIds": ["seal-001", "polar-bear-001"]
}
```

---

## 常見問題

### Q: 頁面載入緩慢？

檢查 `logs/` 目錄中的日誌檔案，確認是否有錯誤。確保 JSON 資料檔案格式正確。

### Q: 圖片無法顯示？

確認圖片已放置於 `wwwroot/images/` 目錄，且路徑與 JSON 中的設定一致。

### Q: 如何新增新區域？

編輯 `Zoo/Data/zones.json`，新增區域物件，並更新地圖 SVG 檔案。

---

## 相關文件

- [功能規格](spec.md)
- [實作計畫](plan.md)
- [資料模型](data-model.md)
- [研究文件](research.md)
- [API 合約](contracts/api-routes.md)
