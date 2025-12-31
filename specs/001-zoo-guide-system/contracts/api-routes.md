# API Contracts: 動物園導覽系統

**Feature Branch**: `001-zoo-guide-system`  
**Created**: 2025-12-31  
**Status**: Complete

## 概述

本專案採用 ASP.NET Core Razor Pages 架構，主要透過頁面路由提供內容。以下定義系統的 URL 路由與必要的 AJAX API 端點。

---

## 頁面路由 (Razor Pages)

### 首頁

| 路由 | 頁面 | 說明 |
| --- | --- | --- |
| `/` | `Pages/Index.cshtml` | 網站首頁，展示精選動物與快速導航 |
| `/About` | `Pages/About/Index.cshtml` | 關於我們 |
| `/Privacy` | `Pages/Privacy.cshtml` | 隱私權政策 |

### 動物瀏覽

| 路由 | 頁面 | 說明 |
| --- | --- | --- |
| `/Animals` | `Pages/Animals/Index.cshtml` | 動物清單頁面，支援篩選 |
| `/Animals/{id}` | `Pages/Animals/Details.cshtml` | 動物詳情頁面 |

### 地圖導覽

| 路由 | 頁面 | 說明 |
| --- | --- | --- |
| `/Map` | `Pages/Map/Index.cshtml` | 互動地圖頁面 |

### 搜尋

| 路由 | 頁面 | 說明 |
| --- | --- | --- |
| `/Search` | `Pages/Search/Index.cshtml` | 搜尋結果頁面 |
| `/Search?q={keyword}` | - | 帶關鍵字的搜尋 |

---

## API 端點 (AJAX 用)

為支援即時搜尋建議與地圖互動，提供以下 JSON API 端點。

### 搜尋 API

#### GET `/api/search/suggest`

即時搜尋建議（自動完成）。

**Query Parameters**:

| 參數 | 類型 | 必填 | 說明 |
| --- | --- | --- | --- |
| `q` | string | 是 | 搜尋關鍵字 |
| `limit` | int | 否 | 回傳筆數上限 (預設: 5) |

**Response** (200 OK):

```json
{
  "suggestions": [
    {
      "id": "lion-001",
      "name": "非洲獅",
      "englishName": "African Lion",
      "thumbnailUrl": "/images/animals/lion-001-thumb.webp"
    }
  ]
}
```

---

### 動物 API

#### GET `/api/animals`

取得動物清單（支援篩選）。

**Query Parameters**:

| 參數 | 類型 | 必填 | 說明 |
| --- | --- | --- | --- |
| `class` | string | 否 | 生物分類篩選 |
| `habitat` | string | 否 | 棲息地篩選 |
| `diet` | string | 否 | 飲食習性篩選 |
| `activity` | string | 否 | 活動時間篩選 |
| `zone` | string | 否 | 區域 ID 篩選 |

**Response** (200 OK):

```json
{
  "animals": [
    {
      "id": "lion-001",
      "chineseName": "非洲獅",
      "englishName": "African Lion",
      "thumbnailUrl": "/images/animals/lion-001-thumb.webp",
      "conservationStatus": "VU",
      "classification": {
        "biologicalClass": "Mammal",
        "habitat": "Grassland"
      }
    }
  ],
  "total": 50
}
```

#### GET `/api/animals/{id}`

取得單一動物詳情。

**Response** (200 OK):

```json
{
  "id": "lion-001",
  "chineseName": "非洲獅",
  "englishName": "African Lion",
  "scientificName": "Panthera leo",
  "description": { ... },
  "funFacts": [ ... ],
  "conservationStatus": "VU",
  "media": { ... },
  "relatedAnimals": [ ... ]
}
```

**Response** (404 Not Found):

```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Not Found",
  "status": 404,
  "detail": "找不到 ID 為 'invalid-id' 的動物"
}
```

---

### 區域 API

#### GET `/api/zones`

取得所有區域清單。

**Response** (200 OK):

```json
{
  "zones": [
    {
      "id": "africa-zone",
      "nameZh": "非洲區",
      "nameEn": "Africa Zone",
      "animalCount": 12
    }
  ]
}
```

#### GET `/api/zones/{id}/animals`

取得指定區域的動物清單。

**Response** (200 OK):

```json
{
  "zone": {
    "id": "africa-zone",
    "nameZh": "非洲區"
  },
  "animals": [
    {
      "id": "lion-001",
      "chineseName": "非洲獅",
      "thumbnailUrl": "/images/animals/lion-001-thumb.webp"
    }
  ]
}
```

---

### 測驗 API

#### GET `/api/quizzes/animal/{animalId}`

取得指定動物的測驗題目。

**Response** (200 OK):

```json
{
  "quizzes": [
    {
      "id": "quiz-lion-001",
      "type": "MultipleChoice",
      "questionZh": "非洲獅每天大約睡多少小時？",
      "questionEn": "How many hours does an African lion sleep per day?",
      "options": [
        { "textZh": "8 小時", "textEn": "8 hours" },
        { "textZh": "12 小時", "textEn": "12 hours" },
        { "textZh": "20 小時", "textEn": "20 hours" },
        { "textZh": "4 小時", "textEn": "4 hours" }
      ]
    }
  ]
}
```

#### POST `/api/quizzes/{quizId}/answer`

提交測驗答案。

**Request Body**:

```json
{
  "answer": 2
}
```

**Response** (200 OK):

```json
{
  "correct": true,
  "correctAnswer": 2,
  "feedbackZh": "答對了！獅子是貓科動物中最懶的，每天可睡眠長達 20 小時。",
  "feedbackEn": "Correct! Lions are among the laziest cats, sleeping up to 20 hours a day."
}
```

---

## 錯誤回應格式

所有 API 錯誤遵循 RFC 7807 Problem Details 格式：

```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Bad Request",
  "status": 400,
  "detail": "搜尋關鍵字不得為空",
  "instance": "/api/search/suggest?q="
}
```

---

## HTTP 狀態碼

| 狀態碼 | 說明 |
| --- | --- |
| 200 | 成功 |
| 400 | 請求參數錯誤 |
| 404 | 資源不存在 |
| 500 | 伺服器內部錯誤 |
