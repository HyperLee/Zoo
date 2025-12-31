# Tasks: 動物園導覽系統

**Input**: Design documents from `/specs/001-zoo-guide-system/`  
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅

**Tests**: 遵循憲章 Principle II「測試優先開發」原則，每個 Phase 包含對應的單元測試與整合測試任務。

**Organization**: 任務依 User Story 分組，支援獨立實作與測試。

## Format: `[ID] [P?] [Story] Description`

- **[P]**: 可平行執行（不同檔案、無相依性）
- **[Story]**: 所屬 User Story（如 US1、US2、US3）
- 所有任務包含確切檔案路徑

## Path Conventions

- **主專案**: `Zoo/` (ASP.NET Core Razor Pages)
- **測試專案**: `Zoo.Tests/`
- **規格文件**: `specs/001-zoo-guide-system/`

---

## Phase 1: Setup (專案初始化)

**Purpose**: 專案基礎設定與結構建立

- [X] T001 建立專案資料夾結構，包含 `Zoo/Models/`、`Zoo/Services/`、`Zoo/Data/` 目錄
- [X] T002 [P] 安裝 Serilog 相關套件至 `Zoo/Zoo.csproj`（Serilog.AspNetCore、Serilog.Sinks.Console、Serilog.Sinks.File）
- [X] T003 [P] 設定 Serilog 於 `Zoo/Program.cs` 中進行結構化日誌記錄
- [X] T004 [P] 更新 `Zoo/appsettings.json` 加入 DataPaths 設定區塊
- [X] T005 [P] 建立共用 Layout 樣式與導航列於 `Zoo/Pages/Shared/_Layout.cshtml`
- [X] T005a [P] 建立測試專案 `Zoo.Tests/Zoo.Tests.csproj`，安裝 xUnit、Moq、Microsoft.AspNetCore.Mvc.Testing 套件
- [X] T005b [P] 建立測試專案資料夾結構 `Zoo.Tests/Unit/`、`Zoo.Tests/Integration/`

---

## Phase 2: Foundational (基礎架構)

**Purpose**: 所有 User Story 必須依賴的核心基礎設施

**⚠️ CRITICAL**: 此階段完成前，任何 User Story 都無法開始

### 資料模型 (Models)

- [X] T006 [P] 建立列舉定義於 `Zoo/Models/Enums.cs`（BiologicalClass、Habitat、Diet、ActivityPattern、ConservationStatus、RouteType、FacilityType、QuizType）
- [X] T007 [P] 建立 Coordinate 記錄類型於 `Zoo/Models/Coordinate.cs`
- [X] T008 [P] 建立 Classification 類別於 `Zoo/Models/Classification.cs`
- [X] T009 [P] 建立 Description 類別於 `Zoo/Models/Description.cs`
- [X] T010 [P] 建立 MediaResources 類別於 `Zoo/Models/MediaResources.cs`
- [X] T011 建立 Animal 類別於 `Zoo/Models/Animal.cs`（依賴 T006-T010）
- [X] T012 [P] 建立 Zone 類別於 `Zoo/Models/Zone.cs`
- [X] T013 [P] 建立 Route 類別於 `Zoo/Models/Route.cs`
- [X] T014 [P] 建立 Facility 類別於 `Zoo/Models/Facility.cs`
- [X] T015 [P] 建立 Quiz 及 QuizOption 類別於 `Zoo/Models/Quiz.cs`

### JSON 資料服務

- [X] T016 建立泛型 JSON 資料服務介面 `Zoo/Services/IJsonDataService.cs`
- [X] T017 實作 JsonDataService 於 `Zoo/Services/JsonDataService.cs`（含 IMemoryCache 緩存）
- [X] T018 於 `Zoo/Program.cs` 註冊 JsonDataService 與 IMemoryCache 服務

### 初始 JSON 資料檔案

- [X] T019 [P] 建立範例動物資料於 `Zoo/Data/animals.json`（至少 3 隻動物）
- [X] T020 [P] 建立園區區域資料於 `Zoo/Data/zones.json`
- [X] T021 [P] 建立設施資料於 `Zoo/Data/facilities.json`
- [X] T022 [P] 建立導覽路線資料於 `Zoo/Data/routes.json`
- [X] T023 [P] 建立測驗題目資料於 `Zoo/Data/quizzes.json`

**Checkpoint**: 基礎設施完成 - User Story 實作可以開始 ✅

---

## Phase 3: User Story 1 - 瀏覽動物資訊 (Priority: P1) 🎯 MVP

**Goal**: 使用者可瀏覽動物清單與詳細資訊頁面，包含名稱、棲息地、飲食習慣、趣味事實及保育等級

**Independent Test**: 訪問 `/Animals` 顯示動物卡片清單；訪問 `/Animals/{id}` 顯示完整動物資訊

### 服務層

- [X] T024 建立 IAnimalService 介面於 `Zoo/Services/IAnimalService.cs`
- [X] T025 實作 AnimalService 於 `Zoo/Services/AnimalService.cs`（GetAllAsync、GetByIdAsync、GetRelatedAsync）
- [X] T026 於 `Zoo/Program.cs` 註冊 AnimalService 服務

### 頁面實作

- [X] T027 [P] [US1] 建立動物卡片部分檢視於 `Zoo/Pages/Shared/_AnimalCard.cshtml`
- [X] T028 [US1] 建立動物清單頁面 `Zoo/Pages/Animals/Index.cshtml` 與 `Zoo/Pages/Animals/Index.cshtml.cs`
- [X] T029 [US1] 建立動物詳情頁面 `Zoo/Pages/Animals/Details.cshtml` 與 `Zoo/Pages/Animals/Details.cshtml.cs`
- [X] T030 [P] [US1] 更新首頁 `Zoo/Pages/Index.cshtml` 顯示精選動物卡片
- [X] T031 [US1] 實作動物詳情頁底部的「相關動物推薦」區塊於 `Zoo/Pages/Animals/Details.cshtml`

### 靜態資源

- [X] T032 [P] [US1] 建立動物圖片目錄結構 `Zoo/wwwroot/images/animals/`
- [X] T033 [P] [US1] 建立音效目錄結構 `Zoo/wwwroot/audio/`
- [X] T034 [P] [US1] 更新網站樣式 `Zoo/wwwroot/css/site.css` 加入動物卡片樣式

### 測試 (US1)

- [X] T034a [US1] 建立 AnimalService 單元測試於 `Zoo.Tests/Unit/Services/AnimalServiceTests.cs`
- [X] T034b [US1] 建立動物清單頁面整合測試於 `Zoo.Tests/Integration/Pages/AnimalsIndexTests.cs`
- [X] T034c [US1] 建立動物詳情頁面整合測試於 `Zoo.Tests/Integration/Pages/AnimalsDetailsTests.cs`

**Checkpoint**: User Story 1 完成 - 可獨立瀏覽動物資訊（含測試通過） ✅

---

## Phase 4: User Story 2 - 搜尋與篩選動物 (Priority: P1) 🎯 MVP

**Goal**: 使用者可透過關鍵字搜尋或條件篩選找到特定動物

**Independent Test**: 在搜尋框輸入「獅」顯示相關結果；使用篩選條件縮小結果範圍

### 服務層

- [X] T035 建立 ISearchService 介面於 `Zoo/Services/ISearchService.cs`
- [X] T036 實作 SearchService 於 `Zoo/Services/SearchService.cs`（SearchAsync、SuggestAsync、篩選邏輯）
- [X] T037 於 `Zoo/Program.cs` 註冊 SearchService 服務

### API 端點

- [X] T038 [US2] 建立搜尋建議 API 端點 `Zoo/Pages/Api/Search/Suggest.cshtml.cs`（GET /api/search/suggest）
- [X] T039 [P] [US2] 建立動物清單 API 端點 `Zoo/Pages/Api/Animals/Index.cshtml.cs`（GET /api/animals）
- [X] T040 [P] [US2] 建立單一動物 API 端點 `Zoo/Pages/Api/Animals/Details.cshtml.cs`（GET /api/animals/{id}）

### 頁面實作

- [X] T041 [US2] 建立搜尋結果頁面 `Zoo/Pages/Search/Index.cshtml` 與 `Zoo/Pages/Search/Index.cshtml.cs`
- [X] T042 [US2] 更新動物清單頁面 `Zoo/Pages/Animals/Index.cshtml` 加入篩選功能（生物分類、棲息地、飲食習性、活動時間）
- [X] T043 [P] [US2] 建立共用搜尋框部分檢視於 `Zoo/Pages/Shared/_SearchBox.cshtml`

### 前端互動

- [X] T044 [US2] 實作即時搜尋建議功能於 `Zoo/wwwroot/js/search.js`（自動完成、搜尋歷史）
- [X] T045 [P] [US2] 更新 Layout 加入全站搜尋框於 `Zoo/Pages/Shared/_Layout.cshtml`

### 測試 (US2)

- [X] T045a [US2] 建立 SearchService 單元測試於 `Zoo.Tests/Unit/Services/SearchServiceTests.cs`
- [X] T045b [US2] 建立搜尋 API 整合測試於 `Zoo.Tests/Integration/Api/SearchApiTests.cs`

**Checkpoint**: User Story 2 完成 - 可搜尋與篩選動物（含測試通過） ✅

---

## Phase 5: User Story 3 - 瀏覽互動地圖 (Priority: P2)

**Goal**: 使用者可透過互動地圖了解動物園整體配置

**Independent Test**: 開啟 `/Map` 頁面，可縮放、拖曳地圖，點擊區域顯示該區動物清單

### 服務層

- [X] T046 建立 IZoneService 介面於 `Zoo/Services/IZoneService.cs`
- [X] T047 實作 ZoneService 於 `Zoo/Services/ZoneService.cs`（GetAllAsync、GetByIdAsync、GetAnimalsByZoneAsync）
- [X] T048 於 `Zoo/Program.cs` 註冊 ZoneService 服務

### API 端點

- [X] T049 [P] [US3] 建立區域清單 API 端點 `Zoo/Pages/Api/Zones/Index.cshtml.cs`（GET /api/zones）
- [X] T050 [P] [US3] 建立區域動物 API 端點 `Zoo/Pages/Api/Zones/Animals.cshtml.cs`（GET /api/zones/{id}/animals）

### 頁面實作

- [X] T051 [US3] 建立互動地圖頁面 `Zoo/Pages/Map/Index.cshtml` 與 `Zoo/Pages/Map/Index.cshtml.cs`
- [X] T052 [P] [US3] 建立園區 SVG 地圖於 `Zoo/wwwroot/images/map/zoo-map.svg`

### 前端互動

- [X] T053 [US3] 實作地圖互動功能於 `Zoo/wwwroot/js/map.js`（縮放、拖曳、區域點擊）
- [X] T054 [P] [US3] 更新地圖樣式於 `Zoo/wwwroot/css/map.css`（hover 效果、區域標示）

**Checkpoint**: User Story 3 完成 - 可瀏覽互動地圖 ✅

---

## Phase 6: User Story 4 - 導覽路線規劃 (Priority: P2)

**Goal**: 使用者可選擇預設路線或自訂參觀路線

**Independent Test**: 選擇「精華導覽路線」顯示路線軌跡；勾選動物後系統規劃最佳路徑

### 服務層

- [X] T055 建立 IRouteService 介面於 `Zoo/Services/IRouteService.cs`
- [X] T056 實作 RouteService 於 `Zoo/Services/RouteService.cs`（GetAllAsync、GetByIdAsync、PlanCustomRouteAsync）
- [X] T057 於 `Zoo/Program.cs` 註冊 RouteService 服務

### API 端點

- [X] T058 [P] [US4] 建立路線清單 API 端點 `Zoo/Pages/Api/Routes/Index.cshtml.cs`（GET /api/routes）
- [X] T059 [P] [US4] 建立路線詳情 API 端點 `Zoo/Pages/Api/Routes/Details.cshtml.cs`（GET /api/routes/{id}）
- [X] T060 [US4] 建立自訂路線規劃 API 端點 `Zoo/Pages/Api/Routes/Plan.cshtml.cs`（POST /api/routes/plan）

### 頁面實作

- [X] T061 [US4] 建立路線規劃頁面 `Zoo/Pages/Routes/Index.cshtml` 與 `Zoo/Pages/Routes/Index.cshtml.cs`
- [X] T062 [US4] 整合路線顯示於地圖頁面 `Zoo/Pages/Map/Index.cshtml`

### 前端互動

- [X] T063 [US4] 實作路線規劃功能於 `Zoo/wwwroot/js/route-planner.js`（選擇動物、規劃路線、分享連結）
- [X] T064 [US4] 實作路線分享功能（URL 編碼路線資訊）

**Checkpoint**: User Story 4 完成 - 可規劃導覽路線 ✅

---

## Phase 7: User Story 5 - 收藏與瀏覽進度追蹤 (Priority: P2)

**Goal**: 使用者可收藏動物並追蹤已瀏覽的動物

**Independent Test**: 點擊收藏按鈕加入收藏清單；已瀏覽動物顯示不同視覺標示

### 前端實作

- [X] T065 [P] [US5] 建立收藏管理模組於 `Zoo/wwwroot/js/favorites.js`（localStorage 操作）
- [X] T066 [P] [US5] 建立瀏覽記錄模組於 `Zoo/wwwroot/js/history.js`（localStorage 操作）
- [X] T067 [US5] 更新動物卡片加入收藏按鈕於 `Zoo/Pages/Shared/_AnimalCard.cshtml`
- [X] T068 [US5] 更新動物詳情頁面加入收藏按鈕於 `Zoo/Pages/Animals/Details.cshtml`
- [X] T069 [US5] 實作收藏清單頁面 `Zoo/Pages/Favorites/Index.cshtml` 與 `Zoo/Pages/Favorites/Index.cshtml.cs`
- [X] T070 [US5] 實作收藏清單匯出功能（下載文字檔案）

**Checkpoint**: User Story 5 完成 - 可收藏動物與追蹤進度 ✅

---

## Phase 8: User Story 6 - 多媒體互動體驗 (Priority: P3)

**Goal**: 使用者可聆聽動物叫聲並觀看動畫展示

**Independent Test**: 點擊播放按鈕聆聯動物叫聲；切換不同姿態圖片檢視

### 前端實作

- [X] T071 [P] [US6] 建立音效播放模組於 `Zoo/wwwroot/js/audio-player.js`（播放、暫停、音量控制）
- [X] T072 [P] [US6] 建立圖片輪播模組於 `Zoo/wwwroot/js/image-carousel.js`（輪播、姿態切換）
- [X] T073 [US6] 更新動物詳情頁面加入音效播放器於 `Zoo/Pages/Animals/Details.cshtml`
- [X] T074 [US6] 更新動物詳情頁面加入圖片輪播於 `Zoo/Pages/Animals/Details.cshtml`
- [X] T075 [P] [US6] 更新樣式加入媒體播放器樣式於 `Zoo/wwwroot/css/site.css`

**Checkpoint**: User Story 6 完成 - 可體驗多媒體內容 ✅

---

## Phase 9: User Story 7 - 知識測驗互動 (Priority: P3)

**Goal**: 使用者可透過測驗檢驗對動物的了解

**Independent Test**: 點擊「小測驗」顯示題目；答對顯示鼓勵訊息與冷知識

### 服務層

- [ ] T076 建立 IQuizService 介面於 `Zoo/Services/IQuizService.cs`
- [ ] T077 實作 QuizService 於 `Zoo/Services/QuizService.cs`（GetByAnimalIdAsync、ValidateAnswerAsync）
- [ ] T078 於 `Zoo/Program.cs` 註冊 QuizService 服務

### API 端點

- [ ] T079 [P] [US7] 建立測驗題目 API 端點 `Zoo/Pages/Api/Quizzes/Animal.cshtml.cs`（GET /api/quizzes/animal/{animalId}）
- [ ] T080 [P] [US7] 建立答案驗證 API 端點 `Zoo/Pages/Api/Quizzes/Answer.cshtml.cs`（POST /api/quizzes/{quizId}/answer）

### 頁面實作

- [ ] T081 [P] [US7] 建立測驗彈窗部分檢視於 `Zoo/Pages/Shared/_QuizModal.cshtml`
- [ ] T082 [US7] 更新動物詳情頁面加入測驗按鈕於 `Zoo/Pages/Animals/Details.cshtml`
- [ ] T083 [US7] 建立綜合測驗頁面 `Zoo/Pages/Quiz/Index.cshtml` 與 `Zoo/Pages/Quiz/Index.cshtml.cs`

### 前端互動

- [ ] T084 [US7] 實作測驗互動功能於 `Zoo/wwwroot/js/quiz.js`（顯示題目、提交答案、顯示結果）

**Checkpoint**: User Story 7 完成 - 可進行知識測驗

---

## Phase 10: User Story 8 - 多語言切換 (Priority: P3)

**Goal**: 使用者可將介面切換為英文

**Independent Test**: 點擊語言切換按鈕，整個網站內容切換為英文

### 本地化設定

- [ ] T085 [P] [US8] 安裝本地化套件並設定於 `Zoo/Program.cs`
- [ ] T086 [P] [US8] 建立資源檔案目錄 `Zoo/Resources/`
- [ ] T087 [P] [US8] 建立繁體中文資源檔 `Zoo/Resources/Pages/Index.zh-TW.resx`
- [ ] T088 [P] [US8] 建立英文資源檔 `Zoo/Resources/Pages/Index.en.resx`
- [ ] T089 [US8] 更新所有頁面使用 IStringLocalizer 進行文字本地化
- [ ] T090 [P] [US8] 建立語言切換部分檢視於 `Zoo/Pages/Shared/_LanguageSwitcher.cshtml`
- [ ] T091 [US8] 更新 Layout 加入語言切換按鈕於 `Zoo/Pages/Shared/_Layout.cshtml`

**Checkpoint**: User Story 8 完成 - 支援多語言切換

---

## Phase 11: Polish & Cross-Cutting Concerns

**Purpose**: 跨 User Story 的改進與優化

### 無障礙設計

- [ ] T092 [P] 為所有圖片加入 Alt Text（WCAG 2.1 Level AA）
- [ ] T093 [P] 實作完整鍵盤導航支援
- [ ] T094 [P] 建立高對比模式選項於 `Zoo/wwwroot/css/high-contrast.css`
- [ ] T095 [P] 實作字體大小調整功能

### 導航與 UX

- [ ] T096 [P] 實作麵包屑導航於 `Zoo/Pages/Shared/_Breadcrumb.cshtml`
- [ ] T097 [P] 實作返回頂部按鈕於 `Zoo/wwwroot/js/site.js`
- [ ] T098 [P] 實作動物頁面上一個/下一個切換按鈕

### 錯誤處理

- [ ] T099 [P] 實作行動裝置偵測與提示訊息
- [ ] T100 [P] 實作低解析度偵測與提示訊息
- [ ] T101 [P] 更新錯誤頁面 `Zoo/Pages/Error.cshtml` 顯示友善錯誤訊息

### 關於頁面

- [ ] T102 建立關於我們頁面 `Zoo/Pages/About/Index.cshtml` 與 `Zoo/Pages/About/Index.cshtml.cs`

### 驗證與清理

- [ ] T103 執行 quickstart.md 驗證流程
- [ ] T104 程式碼清理與重構

### 效能驗證 (SC-003, SC-004)

- [ ] T105 建立前端效能監測腳本於 `Zoo/wwwroot/js/performance-monitor.js`（記錄頁面載入時間）
- [ ] T105a 建立效能測試案例於 `Zoo.Tests/Integration/PerformanceTests.cs`（驗證頁面載入 < 3 秒）
- [ ] T105b 執行效能基準測試，確保 95% 頁面載入時間 < 3 秒 (SC-003, SC-004)

### 無障礙驗證 (SC-005)

- [ ] T106 執行 WCAG 2.1 Level AA 自動化檢測（使用 axe-core 或 Pa11y）
- [ ] T106a 建立無障礙測試案例於 `Zoo.Tests/Integration/AccessibilityTests.cs`
- [ ] T106b 驗證所有頁面無障礙測試通過率 100% (SC-005)

### 跨瀏覽器相容性測試 (SC-008)

- [ ] T107 驗證 Chrome 最新版本及前兩版功能相容性
- [ ] T107a 驗證 Firefox 最新版本及前兩版功能相容性
- [ ] T107b 驗證 Edge 最新版本及前兩版功能相容性
- [ ] T107c 驗證 Safari 最新版本及前兩版功能相容性

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: 無相依性 - 可立即開始
- **Foundational (Phase 2)**: 依賴 Setup 完成 - **阻擋所有 User Stories**
- **User Stories (Phase 3-10)**: 全部依賴 Foundational 完成
  - US1 (P1) 與 US2 (P1) 為 MVP 核心
  - US3-US5 (P2) 可在 US1/US2 後平行進行
  - US6-US8 (P3) 為進階功能
- **Polish (Phase 11)**: 依賴所有所需 User Stories 完成

### User Story Dependencies

| User Story | Priority | 依賴 | 說明 |
| --- | --- | --- | --- |
| US1 瀏覽動物資訊 | P1 | Foundational | 核心功能，可獨立測試 |
| US2 搜尋與篩選 | P1 | Foundational + US1 | 依賴動物資料服務 |
| US3 互動地圖 | P2 | Foundational | 可獨立測試 |
| US4 路線規劃 | P2 | US3 | 依賴地圖功能 |
| US5 收藏追蹤 | P2 | US1 | 依賴動物頁面 |
| US6 多媒體體驗 | P3 | US1 | 依賴動物詳情頁 |
| US7 知識測驗 | P3 | US1 | 依賴動物詳情頁 |
| US8 多語言 | P3 | 所有頁面完成 | 最後實作 |

### Within Each User Story

- Models → Services → API 端點 → 頁面實作 → 前端互動
- 核心實作完成後才能進行整合
- Story 完成後再進行下一個

### Parallel Opportunities

**Phase 1 (Setup)**:
- T002、T003、T004、T005 可平行執行

**Phase 2 (Foundational)**:
- T006-T010、T012-T015 可平行執行
- T019-T023 可平行執行

**User Stories**:
- US1 與 US3 可平行進行（不同功能模組）
- US5、US6、US7 可在 US1 完成後平行進行

---

## Parallel Example: Phase 2 Models

```bash
# 同時建立所有列舉和基礎類型:
Task T006: "建立列舉定義於 Zoo/Models/Enums.cs"
Task T007: "建立 Coordinate 記錄類型於 Zoo/Models/Coordinate.cs"
Task T008: "建立 Classification 類別於 Zoo/Models/Classification.cs"
Task T009: "建立 Description 類別於 Zoo/Models/Description.cs"
Task T010: "建立 MediaResources 類別於 Zoo/Models/MediaResources.cs"

# 同時建立實體模型 (依賴 T006-T010 完成):
Task T012: "建立 Zone 類別於 Zoo/Models/Zone.cs"
Task T013: "建立 Route 類別於 Zoo/Models/Route.cs"
Task T014: "建立 Facility 類別於 Zoo/Models/Facility.cs"
Task T015: "建立 Quiz 類別於 Zoo/Models/Quiz.cs"
```

---

## Implementation Strategy

### MVP First (P1 功能)

1. 完成 Phase 1: Setup
2. 完成 Phase 2: Foundational (**CRITICAL - 阻擋所有 Stories**)
3. 完成 Phase 3: User Story 1 (瀏覽動物資訊)
4. **STOP and VALIDATE**: 獨立測試 US1
5. 完成 Phase 4: User Story 2 (搜尋與篩選)
6. **MVP 完成**: 可部署展示

### Incremental Delivery

1. Setup + Foundational → 基礎完成
2. 加入 US1 → 測試 → 部署 (基礎 MVP)
3. 加入 US2 → 測試 → 部署 (完整 MVP)
4. 加入 US3 + US5 → 測試 → 部署 (地圖 + 收藏)
5. 加入 US4 → 測試 → 部署 (路線規劃)
6. 加入 US6 + US7 → 測試 → 部署 (多媒體 + 測驗)
7. 加入 US8 → 測試 → 部署 (多語言)
8. Polish → 最終發布

### Suggested MVP Scope

根據 spec.md 的 Assumptions 區塊：
> **MVP 範圍**：P1 功能（動物資訊瀏覽、搜尋篩選）+ 簡化版 P2（靜態地圖瀏覽），不含路線規劃、收藏功能

因此建議 MVP 包含：
- Phase 1: Setup
- Phase 2: Foundational
- Phase 3: User Story 1 (瀏覽動物資訊)
- Phase 4: User Story 2 (搜尋與篩選)
- Phase 5: User Story 3 (僅靜態地圖瀏覽，不含點擊互動)

---

## Notes

- `[P]` 任務 = 不同檔案、無相依性，可平行執行
- `[Story]` 標籤對應特定 User Story 以便追蹤
- 每個 User Story 應可獨立完成與測試
- 每個任務或邏輯群組完成後進行 commit
- 在任何 checkpoint 可停下來獨立驗證 story
- 避免：模糊任務、同檔案衝突、破壞獨立性的跨 story 相依
