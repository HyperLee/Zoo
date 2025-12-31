# Implementation Plan: 動物園導覽系統

**Branch**: `001-zoo-guide-system` | **Date**: 2025-12-31 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-zoo-guide-system/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

建立一個以教育為導向的互動式數位動物園導覽平台，使用 ASP.NET Core 10.0 Razor Pages 架構，讓遊客透過電腦瀏覽器深入了解動物的生態、習性及保育知識。系統採用 JSON 檔案儲存動物資料，不使用資料庫軟體，以簡化架構並專注於學習練習目的。

## Technical Context

**Language/Version**: C# 14 (.NET 10.0)  
**Framework**: ASP.NET Core 10.0 Razor Pages  
**Primary Dependencies**: Serilog (日誌記錄)、Bootstrap 5 (UI 框架)、jQuery (前端互動)  
**Storage**: JSON 檔案 (動物資料、區域資料、路線資料等)  
**Testing**: xUnit + Moq (單元測試)、WebApplicationFactory (整合測試)  
**Target Platform**: Windows/Linux Web Server，僅支援電腦版瀏覽器  
**Project Type**: Web (ASP.NET Core Razor Pages)  
**Performance Goals**: 頁面載入 < 3 秒、支援 1000 同時使用者  
**Constraints**: 最低螢幕解析度 1366x768、符合 WCAG 2.1 Level AA  
**Scale/Scope**: 50 種動物、約 10 個頁面

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 原則 | 狀態 | 說明 |
| --- | --- | --- |
| I. 程式碼品質至上 | ✅ 通過 | 遵循 C# 14 最佳實踐、PascalCase 命名、XML 檔案註解 |
| II. 測試優先開發 | ✅ 通過 | 使用 xUnit + Moq 進行單元測試，WebApplicationFactory 進行整合測試 |
| III. 使用者體驗一致性 | ✅ 通過 | 使用 Bootstrap 5 統一設計語言，符合 WCAG 2.1 Level AA |
| IV. 效能與延展性 | ✅ 通過 | 頁面載入 < 3 秒、async/await 模式、JSON 檔案靜態緩存 |
| V. 可觀察性與監控 | ✅ 通過 | 使用 Serilog 結構化日誌 |
| VI. 安全優先 | ✅ 通過 | 系統無需認證（純展示），輸入驗證採用 Data Annotations |

## Project Structure

### Documentation (this feature)

```text
specs/001-zoo-guide-system/
├── plan.md              # 本檔案 (/speckit.plan 指令輸出)
├── research.md          # Phase 0 輸出 (/speckit.plan 指令)
├── data-model.md        # Phase 1 輸出 (/speckit.plan 指令)
├── quickstart.md        # Phase 1 輸出 (/speckit.plan 指令)
├── contracts/           # Phase 1 輸出 (/speckit.plan 指令)
└── tasks.md             # Phase 2 輸出 (/speckit.tasks 指令)
```

### Source Code (repository root)

```text
Zoo/                           # ASP.NET Core Razor Pages 專案
├── Program.cs                 # 應用程式進入點與服務設定
├── appsettings.json           # 應用程式設定
├── appsettings.Development.json
├── Data/                      # JSON 資料檔案
│   ├── animals.json           # 動物資料
│   ├── zones.json             # 園區區域資料
│   ├── routes.json            # 導覽路線資料
│   ├── facilities.json        # 設施資料
│   └── quizzes.json           # 測驗題目資料
├── Models/                    # 資料模型
│   ├── Animal.cs
│   ├── Zone.cs
│   ├── Route.cs
│   ├── Facility.cs
│   └── Quiz.cs
├── Services/                  # 業務邏輯服務
│   ├── IAnimalService.cs
│   ├── AnimalService.cs
│   ├── IZoneService.cs
│   ├── ZoneService.cs
│   ├── ISearchService.cs
│   ├── SearchService.cs
│   └── JsonDataService.cs     # JSON 資料存取服務
├── Pages/                     # Razor Pages
│   ├── Index.cshtml           # 首頁
│   ├── Animals/               # 動物相關頁面
│   │   ├── Index.cshtml       # 動物清單
│   │   └── Details.cshtml     # 動物詳情
│   ├── Map/                   # 地圖相關頁面
│   │   └── Index.cshtml       # 互動地圖
│   ├── Search/                # 搜尋頁面
│   │   └── Index.cshtml       # 搜尋結果
│   ├── About/                 # 關於我們
│   │   └── Index.cshtml
│   └── Shared/                # 共用元件
│       ├── _Layout.cshtml
│       └── _AnimalCard.cshtml # 動物卡片部分檢視
└── wwwroot/                   # 靜態資源
    ├── css/
    ├── js/
    ├── images/
    │   ├── animals/           # 動物插畫
    │   └── map/               # 地圖圖片
    └── audio/                 # 動物叫聲音效

Zoo.Tests/                     # 測試專案
├── Unit/                      # 單元測試
│   ├── Services/
│   └── Models/
└── Integration/               # 整合測試
    └── Pages/
```

**Structure Decision**: 採用 ASP.NET Core Razor Pages 單一專案結構。此架構簡單且適合小型教育網站，所有資料儲存於 JSON 檔案中，透過 Services 層封裝資料存取邏輯。

## Complexity Tracking

> 本專案架構簡單，無憲章違規情況

本專案刻意保持簡單架構：
- 使用 JSON 檔案取代資料庫，降低部署複雜度
- 不使用前端框架（Vue/React），僅使用原生 JavaScript + jQuery
- 單一專案結構，無微服務或多專案架構
