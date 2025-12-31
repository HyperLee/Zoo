/**
 * 收藏管理模組
 * 使用 localStorage 儲存使用者收藏的動物清單
 * @module favorites
 */

const FavoritesManager = (function () {
    'use strict';

    const STORAGE_KEY = 'zoo_favorites';
    const FAVORITES_CHANGED_EVENT = 'favoritesChanged';

    /**
     * 取得所有收藏的動物 ID
     * @returns {string[]} 收藏的動物 ID 陣列
     */
    function getAll() {
        try {
            const data = localStorage.getItem(STORAGE_KEY);
            return data ? JSON.parse(data) : [];
        } catch (error) {
            console.error('讀取收藏資料時發生錯誤:', error);
            return [];
        }
    }

    /**
     * 檢查動物是否已收藏
     * @param {string} animalId - 動物 ID
     * @returns {boolean} 是否已收藏
     */
    function isFavorite(animalId) {
        if (!animalId) {
            return false;
        }
        const favorites = getAll();
        return favorites.includes(animalId);
    }

    /**
     * 新增動物至收藏
     * @param {string} animalId - 動物 ID
     * @param {object} [animalInfo] - 動物基本資訊（可選，用於快速顯示）
     * @returns {boolean} 是否成功新增
     */
    function add(animalId, animalInfo = null) {
        if (!animalId) {
            console.warn('動物 ID 不能為空');
            return false;
        }

        try {
            const favorites = getAll();
            if (!favorites.includes(animalId)) {
                favorites.push(animalId);
                localStorage.setItem(STORAGE_KEY, JSON.stringify(favorites));

                // 儲存動物基本資訊以便快速顯示
                if (animalInfo) {
                    saveAnimalInfo(animalId, animalInfo);
                }

                // 觸發自訂事件
                dispatchChangeEvent('add', animalId);
                return true;
            }
            return false;
        } catch (error) {
            console.error('新增收藏時發生錯誤:', error);
            return false;
        }
    }

    /**
     * 從收藏中移除動物
     * @param {string} animalId - 動物 ID
     * @returns {boolean} 是否成功移除
     */
    function remove(animalId) {
        if (!animalId) {
            return false;
        }

        try {
            const favorites = getAll();
            const index = favorites.indexOf(animalId);
            if (index > -1) {
                favorites.splice(index, 1);
                localStorage.setItem(STORAGE_KEY, JSON.stringify(favorites));

                // 移除動物資訊
                removeAnimalInfo(animalId);

                // 觸發自訂事件
                dispatchChangeEvent('remove', animalId);
                return true;
            }
            return false;
        } catch (error) {
            console.error('移除收藏時發生錯誤:', error);
            return false;
        }
    }

    /**
     * 切換收藏狀態
     * @param {string} animalId - 動物 ID
     * @param {object} [animalInfo] - 動物基本資訊
     * @returns {boolean} 切換後的收藏狀態（true = 已收藏）
     */
    function toggle(animalId, animalInfo = null) {
        if (isFavorite(animalId)) {
            remove(animalId);
            return false;
        } else {
            add(animalId, animalInfo);
            return true;
        }
    }

    /**
     * 清除所有收藏
     */
    function clear() {
        try {
            localStorage.removeItem(STORAGE_KEY);
            localStorage.removeItem(STORAGE_KEY + '_info');
            dispatchChangeEvent('clear', null);
        } catch (error) {
            console.error('清除收藏時發生錯誤:', error);
        }
    }

    /**
     * 取得收藏數量
     * @returns {number} 收藏數量
     */
    function count() {
        return getAll().length;
    }

    /**
     * 儲存動物基本資訊
     * @param {string} animalId - 動物 ID
     * @param {object} info - 動物資訊
     */
    function saveAnimalInfo(animalId, info) {
        try {
            const allInfo = getAnimalInfoMap();
            allInfo[animalId] = {
                id: animalId,
                chineseName: info.chineseName || '',
                englishName: info.englishName || '',
                thumbnailPath: info.thumbnailPath || '',
                addedAt: new Date().toISOString()
            };
            localStorage.setItem(STORAGE_KEY + '_info', JSON.stringify(allInfo));
        } catch (error) {
            console.error('儲存動物資訊時發生錯誤:', error);
        }
    }

    /**
     * 移除動物資訊
     * @param {string} animalId - 動物 ID
     */
    function removeAnimalInfo(animalId) {
        try {
            const allInfo = getAnimalInfoMap();
            delete allInfo[animalId];
            localStorage.setItem(STORAGE_KEY + '_info', JSON.stringify(allInfo));
        } catch (error) {
            console.error('移除動物資訊時發生錯誤:', error);
        }
    }

    /**
     * 取得所有動物資訊對應表
     * @returns {object} 動物 ID 到動物資訊的對應表
     */
    function getAnimalInfoMap() {
        try {
            const data = localStorage.getItem(STORAGE_KEY + '_info');
            return data ? JSON.parse(data) : {};
        } catch (error) {
            console.error('讀取動物資訊時發生錯誤:', error);
            return {};
        }
    }

    /**
     * 取得動物資訊
     * @param {string} animalId - 動物 ID
     * @returns {object|null} 動物資訊或 null
     */
    function getAnimalInfo(animalId) {
        const allInfo = getAnimalInfoMap();
        return allInfo[animalId] || null;
    }

    /**
     * 匯出收藏清單為文字格式
     * @returns {string} 收藏清單文字
     */
    function exportAsText() {
        const favorites = getAll();
        const infoMap = getAnimalInfoMap();
        
        if (favorites.length === 0) {
            return '您的收藏清單是空的。';
        }

        const lines = [
            '=== 我的動物收藏清單 ===',
            `匯出日期：${new Date().toLocaleDateString('zh-TW')}`,
            `共 ${favorites.length} 個收藏`,
            '',
            '---'
        ];

        favorites.forEach((id, index) => {
            const info = infoMap[id];
            if (info) {
                lines.push(`${index + 1}. ${info.chineseName} (${info.englishName})`);
            } else {
                lines.push(`${index + 1}. 動物 ID: ${id}`);
            }
        });

        lines.push('---');
        lines.push('來自：動物園導覽系統');

        return lines.join('\n');
    }

    /**
     * 下載收藏清單為文字檔
     */
    function downloadAsTxt() {
        const content = exportAsText();
        const blob = new Blob([content], { type: 'text/plain;charset=utf-8' });
        const url = URL.createObjectURL(blob);
        
        const link = document.createElement('a');
        link.href = url;
        link.download = `我的動物收藏_${new Date().toISOString().slice(0, 10)}.txt`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
    }

    /**
     * 觸發收藏變更事件
     * @param {string} action - 動作類型（add, remove, clear）
     * @param {string|null} animalId - 相關的動物 ID
     */
    function dispatchChangeEvent(action, animalId) {
        const event = new CustomEvent(FAVORITES_CHANGED_EVENT, {
            detail: {
                action: action,
                animalId: animalId,
                count: count(),
                favorites: getAll()
            }
        });
        window.dispatchEvent(event);
    }

    /**
     * 監聽收藏變更事件
     * @param {function} callback - 回呼函式
     */
    function onChange(callback) {
        if (typeof callback === 'function') {
            window.addEventListener(FAVORITES_CHANGED_EVENT, (event) => {
                callback(event.detail);
            });
        }
    }

    /**
     * 更新頁面上所有收藏按鈕的狀態
     */
    function updateAllButtons() {
        const buttons = document.querySelectorAll('[data-favorite-btn]');
        buttons.forEach((btn) => {
            const animalId = btn.getAttribute('data-animal-id');
            if (animalId) {
                updateButtonState(btn, isFavorite(animalId));
            }
        });
    }

    /**
     * 更新單一收藏按鈕的狀態
     * @param {HTMLElement} button - 按鈕元素
     * @param {boolean} isFav - 是否已收藏
     */
    function updateButtonState(button, isFav) {
        const icon = button.querySelector('i');
        const text = button.querySelector('.favorite-text');

        if (isFav) {
            button.classList.remove('btn-outline-danger');
            button.classList.add('btn-danger');
            if (icon) {
                icon.classList.remove('bi-heart');
                icon.classList.add('bi-heart-fill');
            }
            if (text) {
                text.textContent = '已收藏';
            }
            button.setAttribute('aria-pressed', 'true');
            button.setAttribute('title', '從收藏移除');
        } else {
            button.classList.remove('btn-danger');
            button.classList.add('btn-outline-danger');
            if (icon) {
                icon.classList.remove('bi-heart-fill');
                icon.classList.add('bi-heart');
            }
            if (text) {
                text.textContent = '收藏';
            }
            button.setAttribute('aria-pressed', 'false');
            button.setAttribute('title', '加入收藏');
        }
    }

    /**
     * 初始化收藏按鈕事件
     */
    function initButtons() {
        // 使用事件委派處理收藏按鈕點擊
        document.addEventListener('click', function (event) {
            const button = event.target.closest('[data-favorite-btn]');
            if (!button) {
                return;
            }

            event.preventDefault();
            event.stopPropagation();

            const animalId = button.getAttribute('data-animal-id');
            const animalName = button.getAttribute('data-animal-name') || '';
            const animalNameEn = button.getAttribute('data-animal-name-en') || '';
            const thumbnail = button.getAttribute('data-animal-thumbnail') || '';

            if (!animalId) {
                return;
            }

            const nowFavorite = toggle(animalId, {
                chineseName: animalName,
                englishName: animalNameEn,
                thumbnailPath: thumbnail
            });

            updateButtonState(button, nowFavorite);

            // 顯示提示訊息
            showToast(nowFavorite 
                ? `已將「${animalName}」加入收藏` 
                : `已將「${animalName}」從收藏移除`);
        });

        // 頁面載入時更新所有按鈕狀態
        updateAllButtons();
    }

    /**
     * 顯示提示訊息
     * @param {string} message - 訊息內容
     */
    function showToast(message) {
        // 檢查是否已有 toast 容器
        let toastContainer = document.getElementById('favoriteToastContainer');
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.id = 'favoriteToastContainer';
            toastContainer.className = 'toast-container position-fixed bottom-0 end-0 p-3';
            toastContainer.style.zIndex = '1100';
            document.body.appendChild(toastContainer);
        }

        // 建立 toast 元素
        const toastId = 'toast-' + Date.now();
        const toastHtml = `
            <div id="${toastId}" class="toast align-items-center text-bg-success border-0" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="d-flex">
                    <div class="toast-body">
                        <i class="bi bi-heart-fill me-2"></i>${message}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="關閉"></button>
                </div>
            </div>
        `;
        toastContainer.insertAdjacentHTML('beforeend', toastHtml);

        // 顯示 toast
        const toastElement = document.getElementById(toastId);
        const toast = new bootstrap.Toast(toastElement, { delay: 2000 });
        toast.show();

        // 移除已隱藏的 toast
        toastElement.addEventListener('hidden.bs.toast', function () {
            toastElement.remove();
        });
    }

    // 公開介面
    return {
        getAll,
        isFavorite,
        add,
        remove,
        toggle,
        clear,
        count,
        getAnimalInfo,
        getAnimalInfoMap,
        exportAsText,
        downloadAsTxt,
        onChange,
        updateAllButtons,
        updateButtonState,
        initButtons,
        showToast
    };
})();

// 頁面載入完成後初始化
document.addEventListener('DOMContentLoaded', function () {
    FavoritesManager.initButtons();
});
