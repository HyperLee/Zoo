/**
 * 瀏覽記錄模組
 * 使用 localStorage 追蹤使用者已瀏覽的動物
 * @module history
 */

const BrowsingHistory = (function () {
    'use strict';

    const STORAGE_KEY = 'zoo_browsing_history';
    const HISTORY_CHANGED_EVENT = 'browsingHistoryChanged';
    const MAX_HISTORY_ITEMS = 100;

    /**
     * 取得所有瀏覽記錄
     * @returns {object[]} 瀏覽記錄陣列
     */
    function getAll() {
        try {
            const data = localStorage.getItem(STORAGE_KEY);
            return data ? JSON.parse(data) : [];
        } catch (error) {
            console.error('讀取瀏覽記錄時發生錯誤:', error);
            return [];
        }
    }

    /**
     * 取得所有已瀏覽的動物 ID
     * @returns {string[]} 動物 ID 陣列
     */
    function getViewedIds() {
        const history = getAll();
        return [...new Set(history.map(item => item.animalId))];
    }

    /**
     * 檢查動物是否已瀏覽過
     * @param {string} animalId - 動物 ID
     * @returns {boolean} 是否已瀏覽
     */
    function hasViewed(animalId) {
        if (!animalId) {
            return false;
        }
        const viewedIds = getViewedIds();
        return viewedIds.includes(animalId);
    }

    /**
     * 記錄動物瀏覽
     * @param {string} animalId - 動物 ID
     * @param {object} [animalInfo] - 動物基本資訊
     */
    function recordView(animalId, animalInfo = null) {
        if (!animalId) {
            console.warn('動物 ID 不能為空');
            return;
        }

        try {
            const history = getAll();
            
            // 建立瀏覽記錄項目
            const historyItem = {
                animalId: animalId,
                viewedAt: new Date().toISOString(),
                chineseName: animalInfo?.chineseName || '',
                englishName: animalInfo?.englishName || '',
                thumbnailPath: animalInfo?.thumbnailPath || ''
            };

            // 加入到記錄開頭
            history.unshift(historyItem);

            // 限制記錄數量
            const trimmedHistory = history.slice(0, MAX_HISTORY_ITEMS);

            localStorage.setItem(STORAGE_KEY, JSON.stringify(trimmedHistory));

            // 觸發變更事件
            dispatchChangeEvent('add', animalId);
        } catch (error) {
            console.error('記錄瀏覽時發生錯誤:', error);
        }
    }

    /**
     * 取得最近瀏覽的動物（去重）
     * @param {number} [limit=10] - 最大數量
     * @returns {object[]} 最近瀏覽的動物清單
     */
    function getRecent(limit = 10) {
        const history = getAll();
        const seen = new Set();
        const recent = [];

        for (const item of history) {
            if (!seen.has(item.animalId)) {
                seen.add(item.animalId);
                recent.push(item);
                if (recent.length >= limit) {
                    break;
                }
            }
        }

        return recent;
    }

    /**
     * 取得瀏覽統計
     * @returns {object} 瀏覽統計資訊
     */
    function getStats() {
        const history = getAll();
        const viewedIds = getViewedIds();
        
        // 計算每個動物的瀏覽次數
        const viewCounts = {};
        history.forEach(item => {
            viewCounts[item.animalId] = (viewCounts[item.animalId] || 0) + 1;
        });

        // 找出最常瀏覽的動物
        let mostViewed = null;
        let maxCount = 0;
        for (const [id, count] of Object.entries(viewCounts)) {
            if (count > maxCount) {
                maxCount = count;
                mostViewed = id;
            }
        }

        return {
            totalViews: history.length,
            uniqueAnimals: viewedIds.length,
            mostViewedId: mostViewed,
            mostViewedCount: maxCount,
            viewCounts: viewCounts
        };
    }

    /**
     * 清除所有瀏覽記錄
     */
    function clear() {
        try {
            localStorage.removeItem(STORAGE_KEY);
            dispatchChangeEvent('clear', null);
        } catch (error) {
            console.error('清除瀏覽記錄時發生錯誤:', error);
        }
    }

    /**
     * 取得已瀏覽動物數量
     * @returns {number} 已瀏覽的不同動物數量
     */
    function count() {
        return getViewedIds().length;
    }

    /**
     * 觸發瀏覽記錄變更事件
     * @param {string} action - 動作類型
     * @param {string|null} animalId - 相關的動物 ID
     */
    function dispatchChangeEvent(action, animalId) {
        const event = new CustomEvent(HISTORY_CHANGED_EVENT, {
            detail: {
                action: action,
                animalId: animalId,
                count: count(),
                stats: getStats()
            }
        });
        window.dispatchEvent(event);
    }

    /**
     * 監聽瀏覽記錄變更事件
     * @param {function} callback - 回呼函式
     */
    function onChange(callback) {
        if (typeof callback === 'function') {
            window.addEventListener(HISTORY_CHANGED_EVENT, (event) => {
                callback(event.detail);
            });
        }
    }

    /**
     * 更新頁面上的瀏覽標示
     */
    function updateViewedIndicators() {
        const viewedIds = getViewedIds();
        const cards = document.querySelectorAll('[data-animal-card]');
        
        cards.forEach((card) => {
            const animalId = card.getAttribute('data-animal-id');
            if (animalId && viewedIds.includes(animalId)) {
                card.classList.add('animal-viewed');
                
                // 新增已瀏覽標示（如果不存在）
                if (!card.querySelector('.viewed-badge')) {
                    const badge = document.createElement('span');
                    badge.className = 'viewed-badge badge bg-secondary position-absolute';
                    badge.style.cssText = 'top: 10px; left: 10px; font-size: 0.7rem;';
                    badge.innerHTML = '<i class="bi bi-eye-fill me-1"></i>已瀏覽';
                    
                    const imgContainer = card.querySelector('.position-relative');
                    if (imgContainer) {
                        imgContainer.appendChild(badge);
                    }
                }
            }
        });
    }

    /**
     * 匯出瀏覽記錄為文字格式
     * @returns {string} 瀏覽記錄文字
     */
    function exportAsText() {
        const recent = getRecent(50);
        const stats = getStats();
        
        if (recent.length === 0) {
            return '您尚未瀏覽任何動物。';
        }

        const lines = [
            '=== 我的動物瀏覽記錄 ===',
            `匯出日期：${new Date().toLocaleDateString('zh-TW')}`,
            `共瀏覽 ${stats.uniqueAnimals} 種不同動物`,
            `總瀏覽次數：${stats.totalViews} 次`,
            '',
            '--- 最近瀏覽 ---'
        ];

        recent.forEach((item, index) => {
            const viewedDate = new Date(item.viewedAt).toLocaleDateString('zh-TW');
            if (item.chineseName) {
                lines.push(`${index + 1}. ${item.chineseName} (${item.englishName}) - ${viewedDate}`);
            } else {
                lines.push(`${index + 1}. 動物 ID: ${item.animalId} - ${viewedDate}`);
            }
        });

        lines.push('---');
        lines.push('來自：動物園導覽系統');

        return lines.join('\n');
    }

    /**
     * 自動記錄當前頁面的動物瀏覽
     * 需要頁面包含 data-animal-page 和相關資料屬性
     */
    function autoRecordCurrentPage() {
        const pageElement = document.querySelector('[data-animal-page]');
        if (pageElement) {
            const animalId = pageElement.getAttribute('data-animal-id');
            const animalName = pageElement.getAttribute('data-animal-name');
            const animalNameEn = pageElement.getAttribute('data-animal-name-en');
            const thumbnail = pageElement.getAttribute('data-animal-thumbnail');

            if (animalId) {
                recordView(animalId, {
                    chineseName: animalName,
                    englishName: animalNameEn,
                    thumbnailPath: thumbnail
                });
            }
        }
    }

    /**
     * 初始化瀏覽記錄模組
     */
    function init() {
        // 自動記錄當前頁面
        autoRecordCurrentPage();
        
        // 更新瀏覽標示
        updateViewedIndicators();
    }

    // 公開介面
    return {
        getAll,
        getViewedIds,
        hasViewed,
        recordView,
        getRecent,
        getStats,
        clear,
        count,
        onChange,
        updateViewedIndicators,
        exportAsText,
        autoRecordCurrentPage,
        init
    };
})();

// 頁面載入完成後初始化
document.addEventListener('DOMContentLoaded', function () {
    BrowsingHistory.init();
});
