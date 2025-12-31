/**
 * 搜尋功能模組
 * 提供即時搜尋建議、自動完成和搜尋歷史功能
 */

(function () {
    'use strict';

    // 搜尋歷史儲存鍵
    const SEARCH_HISTORY_KEY = 'zoo_search_history';
    // 搜尋歷史最大筆數
    const MAX_HISTORY_ITEMS = 10;
    // 防抖延遲時間 (毫秒)
    const DEBOUNCE_DELAY = 300;
    // 最小搜尋字元數
    const MIN_SEARCH_LENGTH = 1;

    /**
     * 初始化搜尋功能
     * @param {string} containerId - 搜尋框容器的 ID
     */
    function initializeSearch(containerId) {
        const container = document.getElementById(containerId);
        if (!container) {
            return;
        }

        const input = container.querySelector('.search-input');
        const suggestionsContainer = container.querySelector('.search-suggestions');
        const historyContainer = container.querySelector('.search-history');
        const clearHistoryBtn = container.querySelector('.clear-history-btn');

        if (!input) {
            return;
        }

        let debounceTimer = null;
        let currentFocus = -1;

        // 輸入事件處理
        input.addEventListener('input', function () {
            const value = this.value.trim();

            // 清除先前的計時器
            clearTimeout(debounceTimer);

            // 隱藏搜尋歷史
            if (historyContainer) {
                historyContainer.style.display = 'none';
            }

            // 檢查最小字元數
            if (value.length < MIN_SEARCH_LENGTH) {
                if (suggestionsContainer) {
                    suggestionsContainer.style.display = 'none';
                }
                return;
            }

            // 設定防抖計時器
            debounceTimer = setTimeout(function () {
                fetchSuggestions(value, suggestionsContainer, input);
            }, DEBOUNCE_DELAY);
        });

        // 焦點事件處理
        input.addEventListener('focus', function () {
            const value = this.value.trim();

            // 如果輸入框為空，顯示搜尋歷史
            if (value.length === 0 && historyContainer) {
                showSearchHistory(historyContainer);
            }
        });

        // 鍵盤導航
        input.addEventListener('keydown', function (e) {
            const activeContainer = suggestionsContainer?.style.display !== 'none'
                ? suggestionsContainer
                : historyContainer?.style.display !== 'none'
                    ? historyContainer
                    : null;

            if (!activeContainer) {
                return;
            }

            const items = activeContainer.querySelectorAll('.suggestion-item, .history-item');

            if (e.key === 'ArrowDown') {
                e.preventDefault();
                currentFocus++;
                if (currentFocus >= items.length) {
                    currentFocus = 0;
                }
                setActiveItem(items, currentFocus);
            } else if (e.key === 'ArrowUp') {
                e.preventDefault();
                currentFocus--;
                if (currentFocus < 0) {
                    currentFocus = items.length - 1;
                }
                setActiveItem(items, currentFocus);
            } else if (e.key === 'Enter' && currentFocus > -1) {
                e.preventDefault();
                if (items[currentFocus]) {
                    items[currentFocus].click();
                }
            } else if (e.key === 'Escape') {
                if (suggestionsContainer) {
                    suggestionsContainer.style.display = 'none';
                }
                if (historyContainer) {
                    historyContainer.style.display = 'none';
                }
                currentFocus = -1;
            }
        });

        // 點擊外部關閉下拉選單
        document.addEventListener('click', function (e) {
            if (!container.contains(e.target)) {
                if (suggestionsContainer) {
                    suggestionsContainer.style.display = 'none';
                }
                if (historyContainer) {
                    historyContainer.style.display = 'none';
                }
                currentFocus = -1;
            }
        });

        // 清除搜尋歷史
        if (clearHistoryBtn) {
            clearHistoryBtn.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                clearSearchHistory();
                if (historyContainer) {
                    historyContainer.style.display = 'none';
                }
            });
        }

        // 表單提交時儲存搜尋歷史
        const form = container.querySelector('form');
        if (form) {
            form.addEventListener('submit', function () {
                const value = input.value.trim();
                if (value.length > 0) {
                    saveSearchHistory(value);
                }
            });
        }
    }

    /**
     * 取得搜尋建議
     * @param {string} keyword - 搜尋關鍵字
     * @param {HTMLElement} container - 建議容器
     * @param {HTMLInputElement} input - 輸入框
     */
    async function fetchSuggestions(keyword, container, input) {
        if (!container) {
            return;
        }

        try {
            const response = await fetch(`/api/search/suggest?q=${encodeURIComponent(keyword)}&limit=5`);

            if (!response.ok) {
                throw new Error('搜尋建議請求失敗');
            }

            const data = await response.json();
            displaySuggestions(data.suggestions, container, input);
        } catch (error) {
            console.error('取得搜尋建議時發生錯誤:', error);
            container.style.display = 'none';
        }
    }

    /**
     * 顯示搜尋建議
     * @param {Array} suggestions - 搜尋建議陣列
     * @param {HTMLElement} container - 建議容器
     * @param {HTMLInputElement} input - 輸入框
     */
    function displaySuggestions(suggestions, container, input) {
        container.innerHTML = '';

        if (!suggestions || suggestions.length === 0) {
            container.innerHTML = '<div class="dropdown-item text-muted">找不到相關結果</div>';
            container.style.display = 'block';
            return;
        }

        suggestions.forEach(function (suggestion) {
            const item = document.createElement('a');
            item.href = `/Animals/${suggestion.id}`;
            item.className = 'dropdown-item suggestion-item d-flex align-items-center';

            // 縮圖
            const thumbnail = document.createElement('img');
            thumbnail.src = suggestion.thumbnailUrl || '/images/animals/default-thumb.webp';
            thumbnail.alt = suggestion.name;
            thumbnail.className = 'rounded me-2';
            thumbnail.style.width = '40px';
            thumbnail.style.height = '40px';
            thumbnail.style.objectFit = 'cover';

            // 名稱容器
            const nameContainer = document.createElement('div');
            nameContainer.className = 'd-flex flex-column';

            // 中文名稱
            const chineseName = document.createElement('span');
            chineseName.className = 'fw-bold';
            chineseName.textContent = suggestion.name;

            // 英文名稱
            const englishName = document.createElement('small');
            englishName.className = 'text-muted';
            englishName.textContent = suggestion.englishName;

            nameContainer.appendChild(chineseName);
            nameContainer.appendChild(englishName);

            item.appendChild(thumbnail);
            item.appendChild(nameContainer);

            // 點擊時儲存搜尋歷史
            item.addEventListener('click', function () {
                saveSearchHistory(input.value.trim());
            });

            container.appendChild(item);
        });

        // 加入「查看所有結果」連結
        const viewAllItem = document.createElement('a');
        viewAllItem.href = `/Search?q=${encodeURIComponent(input.value.trim())}`;
        viewAllItem.className = 'dropdown-item text-center text-success border-top mt-2 pt-2';
        viewAllItem.innerHTML = '<i class="bi bi-arrow-right-circle me-1"></i>查看所有結果';
        viewAllItem.addEventListener('click', function () {
            saveSearchHistory(input.value.trim());
        });
        container.appendChild(viewAllItem);

        container.style.display = 'block';
    }

    /**
     * 顯示搜尋歷史
     * @param {HTMLElement} container - 歷史容器
     */
    function showSearchHistory(container) {
        const history = getSearchHistory();
        const itemsContainer = container.querySelector('.search-history-items');

        if (!itemsContainer) {
            return;
        }

        itemsContainer.innerHTML = '';

        if (history.length === 0) {
            container.style.display = 'none';
            return;
        }

        history.forEach(function (keyword) {
            const item = document.createElement('a');
            item.href = `/Search?q=${encodeURIComponent(keyword)}`;
            item.className = 'dropdown-item history-item d-flex align-items-center';

            const icon = document.createElement('i');
            icon.className = 'bi bi-clock me-2 text-muted';

            const text = document.createElement('span');
            text.textContent = keyword;

            item.appendChild(icon);
            item.appendChild(text);

            itemsContainer.appendChild(item);
        });

        container.style.display = 'block';
    }

    /**
     * 設定作用中的項目
     * @param {NodeList} items - 項目列表
     * @param {number} index - 作用中的索引
     */
    function setActiveItem(items, index) {
        items.forEach(function (item, i) {
            if (i === index) {
                item.classList.add('active');
                item.setAttribute('aria-selected', 'true');
            } else {
                item.classList.remove('active');
                item.setAttribute('aria-selected', 'false');
            }
        });
    }

    /**
     * 取得搜尋歷史
     * @returns {Array} 搜尋歷史陣列
     */
    function getSearchHistory() {
        try {
            const history = localStorage.getItem(SEARCH_HISTORY_KEY);
            return history ? JSON.parse(history) : [];
        } catch (error) {
            console.error('讀取搜尋歷史時發生錯誤:', error);
            return [];
        }
    }

    /**
     * 儲存搜尋歷史
     * @param {string} keyword - 搜尋關鍵字
     */
    function saveSearchHistory(keyword) {
        if (!keyword || keyword.trim().length === 0) {
            return;
        }

        try {
            let history = getSearchHistory();

            // 移除重複項目
            history = history.filter(function (item) {
                return item.toLowerCase() !== keyword.toLowerCase();
            });

            // 加入到最前面
            history.unshift(keyword);

            // 限制數量
            if (history.length > MAX_HISTORY_ITEMS) {
                history = history.slice(0, MAX_HISTORY_ITEMS);
            }

            localStorage.setItem(SEARCH_HISTORY_KEY, JSON.stringify(history));
        } catch (error) {
            console.error('儲存搜尋歷史時發生錯誤:', error);
        }
    }

    /**
     * 清除搜尋歷史
     */
    function clearSearchHistory() {
        try {
            localStorage.removeItem(SEARCH_HISTORY_KEY);
        } catch (error) {
            console.error('清除搜尋歷史時發生錯誤:', error);
        }
    }

    // DOM 載入完成後初始化
    document.addEventListener('DOMContentLoaded', function () {
        // 初始化所有搜尋框
        const searchContainers = document.querySelectorAll('.search-box-container');
        searchContainers.forEach(function (container) {
            initializeSearch(container.id);
        });
    });

    // 公開 API
    window.ZooSearch = {
        initialize: initializeSearch,
        getHistory: getSearchHistory,
        saveHistory: saveSearchHistory,
        clearHistory: clearSearchHistory
    };
})();
