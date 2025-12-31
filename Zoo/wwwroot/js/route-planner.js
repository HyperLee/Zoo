/**
 * 路線規劃模組
 * 提供自訂路線選擇、規劃、分享等功能
 */
const RoutePlanner = (function () {
    'use strict';

    // 狀態變數
    let state = {
        animalsData: [],
        zonesData: [],
        selectedAnimalIds: new Set(),
        plannedRoute: null
    };

    // DOM 元素參考
    let elements = {
        animalSearch: null,
        zoneFilter: null,
        animalCheckboxes: null,
        selectedAnimalCount: null,
        selectedZoneCount: null,
        estimatedTime: null,
        selectedAnimalsList: null,
        emptyMessage: null,
        planRouteBtn: null,
        viewOnMapBtn: null,
        shareRouteBtn: null,
        clearSelectionBtn: null,
        shareModal: null,
        shareUrl: null,
        copyShareUrl: null,
        copySuccess: null
    };

    /**
     * 初始化路線規劃模組
     * @param {Array} animalsData - 動物資料陣列
     * @param {Array} zonesData - 區域資料陣列
     */
    function init(animalsData, zonesData) {
        state.animalsData = animalsData || [];
        state.zonesData = zonesData || [];

        // 取得 DOM 元素參考
        elements.animalSearch = document.getElementById('animalSearch');
        elements.zoneFilter = document.getElementById('zoneFilter');
        elements.animalCheckboxes = document.querySelectorAll('.animal-checkbox');
        elements.selectedAnimalCount = document.getElementById('selectedAnimalCount');
        elements.selectedZoneCount = document.getElementById('selectedZoneCount');
        elements.estimatedTime = document.getElementById('estimatedTime');
        elements.selectedAnimalsList = document.getElementById('selectedAnimalsList');
        elements.emptyMessage = document.getElementById('emptyMessage');
        elements.planRouteBtn = document.getElementById('planRouteBtn');
        elements.viewOnMapBtn = document.getElementById('viewOnMapBtn');
        elements.shareRouteBtn = document.getElementById('shareRouteBtn');
        elements.clearSelectionBtn = document.getElementById('clearSelectionBtn');
        elements.shareModal = document.getElementById('shareModal');
        elements.shareUrl = document.getElementById('shareUrl');
        elements.copyShareUrl = document.getElementById('copyShareUrl');
        elements.copySuccess = document.getElementById('copySuccess');

        // 綁定事件
        bindEvents();

        // 綁定預設路線分享按鈕
        bindPresetRouteShareButtons();

        console.log('RoutePlanner 初始化完成');
    }

    /**
     * 綁定事件處理
     */
    function bindEvents() {
        // 動物搜尋
        if (elements.animalSearch) {
            elements.animalSearch.addEventListener('input', debounce(filterAnimals, 300));
        }

        // 區域篩選
        if (elements.zoneFilter) {
            elements.zoneFilter.addEventListener('change', filterAnimals);
        }

        // 動物選擇
        elements.animalCheckboxes.forEach(checkbox => {
            checkbox.addEventListener('change', onAnimalSelectionChange);
        });

        // 規劃路線按鈕
        if (elements.planRouteBtn) {
            elements.planRouteBtn.addEventListener('click', planRoute);
        }

        // 在地圖上查看按鈕
        if (elements.viewOnMapBtn) {
            elements.viewOnMapBtn.addEventListener('click', viewOnMap);
        }

        // 分享路線按鈕
        if (elements.shareRouteBtn) {
            elements.shareRouteBtn.addEventListener('click', shareRoute);
        }

        // 清除選擇按鈕
        if (elements.clearSelectionBtn) {
            elements.clearSelectionBtn.addEventListener('click', clearSelection);
        }

        // 複製分享連結按鈕
        if (elements.copyShareUrl) {
            elements.copyShareUrl.addEventListener('click', copyShareUrl);
        }
    }

    /**
     * 綁定預設路線分享按鈕
     */
    function bindPresetRouteShareButtons() {
        const shareButtons = document.querySelectorAll('.btn-share-route');
        shareButtons.forEach(btn => {
            btn.addEventListener('click', function () {
                const routeId = this.getAttribute('data-route-id');
                const routeName = this.getAttribute('data-route-name');
                sharePresetRoute(routeId, routeName);
            });
        });
    }

    /**
     * 防抖函式
     */
    function debounce(func, wait) {
        let timeout;
        return function (...args) {
            clearTimeout(timeout);
            timeout = setTimeout(() => func.apply(this, args), wait);
        };
    }

    /**
     * 篩選動物清單
     */
    function filterAnimals() {
        const searchTerm = elements.animalSearch?.value.toLowerCase() || '';
        const zoneId = elements.zoneFilter?.value || '';

        const animalItems = document.querySelectorAll('.animal-item');
        const zoneGroups = document.querySelectorAll('.zone-group');

        animalItems.forEach(item => {
            const animalName = item.getAttribute('data-animal-name')?.toLowerCase() || '';
            const checkbox = item.querySelector('.animal-checkbox');
            const animalZoneId = checkbox?.getAttribute('data-zone-id') || '';

            const matchesSearch = searchTerm === '' || animalName.includes(searchTerm);
            const matchesZone = zoneId === '' || animalZoneId === zoneId;

            item.style.display = matchesSearch && matchesZone ? '' : 'none';
        });

        // 隱藏空的區域群組
        zoneGroups.forEach(group => {
            const groupZoneId = group.getAttribute('data-zone-id');
            const visibleItems = group.querySelectorAll('.animal-item:not([style*="display: none"])');
            
            if (zoneId && zoneId !== groupZoneId) {
                group.style.display = 'none';
            } else {
                group.style.display = visibleItems.length > 0 ? '' : 'none';
            }
        });
    }

    /**
     * 處理動物選擇變更
     */
    function onAnimalSelectionChange(e) {
        const animalId = e.target.value;
        
        if (e.target.checked) {
            state.selectedAnimalIds.add(animalId);
        } else {
            state.selectedAnimalIds.delete(animalId);
        }

        updateSelectionUI();
    }

    /**
     * 更新選擇的 UI 顯示
     */
    function updateSelectionUI() {
        const count = state.selectedAnimalIds.size;

        // 更新計數
        if (elements.selectedAnimalCount) {
            elements.selectedAnimalCount.textContent = count;
        }

        // 計算區域數量
        const selectedZones = new Set();
        state.selectedAnimalIds.forEach(id => {
            const animal = state.animalsData.find(a => a.id === id);
            if (animal) {
                selectedZones.add(animal.zoneId);
            }
        });

        if (elements.selectedZoneCount) {
            elements.selectedZoneCount.textContent = selectedZones.size;
        }

        // 估算時間
        const estimatedMinutes = calculateEstimatedTime(count, selectedZones.size);
        if (elements.estimatedTime) {
            elements.estimatedTime.textContent = estimatedMinutes;
        }

        // 更新選擇的動物清單
        updateSelectedAnimalsList();

        // 更新按鈕狀態
        const hasSelection = count > 0;
        if (elements.planRouteBtn) {
            elements.planRouteBtn.disabled = !hasSelection;
        }
        if (elements.viewOnMapBtn) {
            elements.viewOnMapBtn.disabled = !state.plannedRoute;
        }
        if (elements.shareRouteBtn) {
            elements.shareRouteBtn.disabled = !state.plannedRoute;
        }
    }

    /**
     * 計算預估時間
     */
    function calculateEstimatedTime(animalCount, zoneCount) {
        const minutesPerZone = 15;
        const minutesPerAnimal = 5;
        const zoneTime = Math.max(zoneCount - 1, 0) * minutesPerZone;
        const animalTime = animalCount * minutesPerAnimal;
        return zoneTime + animalTime;
    }

    /**
     * 更新選擇的動物清單
     */
    function updateSelectedAnimalsList() {
        if (!elements.selectedAnimalsList) return;

        if (state.selectedAnimalIds.size === 0) {
            if (elements.emptyMessage) {
                elements.emptyMessage.style.display = '';
            }
            elements.selectedAnimalsList.innerHTML = elements.emptyMessage?.outerHTML || '';
            return;
        }

        const selectedAnimals = Array.from(state.selectedAnimalIds)
            .map(id => state.animalsData.find(a => a.id === id))
            .filter(a => a);

        elements.selectedAnimalsList.innerHTML = selectedAnimals.map(animal => `
            <div class="d-flex align-items-center mb-2 p-2 border rounded selected-animal-item">
                <img src="${animal.thumbnailUrl || '/images/animals/placeholder.webp'}" 
                     alt="${animal.chineseName}" 
                     class="rounded me-2"
                     style="width: 40px; height: 40px; object-fit: cover;"
                     onerror="this.src='/images/animals/placeholder.webp'">
                <div class="flex-grow-1">
                    <div class="fw-semibold small">${animal.chineseName}</div>
                    <small class="text-muted">${animal.englishName}</small>
                </div>
                <button type="button" class="btn btn-sm btn-outline-danger" 
                        onclick="RoutePlanner.removeAnimal('${animal.id}')">
                    <i class="bi bi-x"></i>
                </button>
            </div>
        `).join('');
    }

    /**
     * 移除動物
     */
    function removeAnimal(animalId) {
        state.selectedAnimalIds.delete(animalId);
        
        // 更新 checkbox 狀態
        const checkbox = document.querySelector(`.animal-checkbox[value="${animalId}"]`);
        if (checkbox) {
            checkbox.checked = false;
        }

        state.plannedRoute = null;
        updateSelectionUI();
    }

    /**
     * 規劃路線
     */
    async function planRoute() {
        if (state.selectedAnimalIds.size === 0) {
            alert('請至少選擇一隻動物');
            return;
        }

        const animalIds = Array.from(state.selectedAnimalIds);

        try {
            const response = await fetch('/api/routes/plan', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    animalIds: animalIds,
                    includeAnimals: true
                })
            });

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.detail || '規劃路線失敗');
            }

            const result = await response.json();
            state.plannedRoute = result;

            // 更新按鈕狀態
            if (elements.viewOnMapBtn) {
                elements.viewOnMapBtn.disabled = false;
            }
            if (elements.shareRouteBtn) {
                elements.shareRouteBtn.disabled = false;
            }

            // 顯示成功訊息
            alert(`路線規劃完成！\n途經 ${result.zoneIds.length} 個區域\n預估時間約 ${result.estimatedMinutes} 分鐘`);

            console.log('路線規劃結果:', result);

        } catch (error) {
            console.error('規劃路線失敗:', error);
            alert('規劃路線失敗：' + error.message);
        }
    }

    /**
     * 在地圖上查看
     */
    function viewOnMap() {
        if (!state.plannedRoute || !state.plannedRoute.shareCode) {
            alert('請先規劃路線');
            return;
        }

        // 導航到地圖頁面，帶上分享碼
        window.location.href = `/Map?custom=${state.plannedRoute.shareCode}`;
    }

    /**
     * 分享路線
     */
    function shareRoute() {
        if (!state.plannedRoute || !state.plannedRoute.shareCode) {
            alert('請先規劃路線');
            return;
        }

        const shareUrl = `${window.location.origin}/Routes?share=${state.plannedRoute.shareCode}`;
        
        if (elements.shareUrl) {
            elements.shareUrl.value = shareUrl;
        }
        if (elements.copySuccess) {
            elements.copySuccess.style.display = 'none';
        }

        // 顯示模態視窗
        if (elements.shareModal) {
            const modal = new bootstrap.Modal(elements.shareModal);
            modal.show();
        }
    }

    /**
     * 分享預設路線
     */
    function sharePresetRoute(routeId, routeName) {
        const shareUrl = `${window.location.origin}/Map?route=${routeId}`;

        if (elements.shareUrl) {
            elements.shareUrl.value = shareUrl;
        }
        if (elements.copySuccess) {
            elements.copySuccess.style.display = 'none';
        }

        // 更新模態視窗標題
        const modalLabel = document.getElementById('shareModalLabel');
        if (modalLabel) {
            modalLabel.innerHTML = `<i class="bi bi-share me-2"></i>分享 ${routeName}`;
        }

        // 顯示模態視窗
        if (elements.shareModal) {
            const modal = new bootstrap.Modal(elements.shareModal);
            modal.show();
        }
    }

    /**
     * 複製分享連結
     */
    async function copyShareUrl() {
        const url = elements.shareUrl?.value;
        if (!url) return;

        try {
            await navigator.clipboard.writeText(url);
            if (elements.copySuccess) {
                elements.copySuccess.style.display = 'block';
                setTimeout(() => {
                    elements.copySuccess.style.display = 'none';
                }, 3000);
            }
        } catch (error) {
            console.error('複製失敗:', error);
            // 備用方案：選取文字
            elements.shareUrl.select();
            document.execCommand('copy');
        }
    }

    /**
     * 清除選擇
     */
    function clearSelection() {
        state.selectedAnimalIds.clear();
        state.plannedRoute = null;

        // 清除所有 checkbox
        elements.animalCheckboxes.forEach(checkbox => {
            checkbox.checked = false;
        });

        // 重置篩選
        if (elements.animalSearch) {
            elements.animalSearch.value = '';
        }
        if (elements.zoneFilter) {
            elements.zoneFilter.value = '';
        }
        filterAnimals();

        updateSelectionUI();
    }

    /**
     * 從分享碼載入路線
     */
    async function loadFromShareCode(shareCode) {
        if (!shareCode) return;

        try {
            const response = await fetch(`/api/routes/plan?shareCode=${encodeURIComponent(shareCode)}`);
            
            if (!response.ok) {
                throw new Error('無效的分享碼');
            }

            const result = await response.json();
            
            // 設定選擇的動物
            state.selectedAnimalIds = new Set(result.animalIds);
            state.plannedRoute = result;

            // 更新 checkbox 狀態
            elements.animalCheckboxes.forEach(checkbox => {
                checkbox.checked = state.selectedAnimalIds.has(checkbox.value);
            });

            updateSelectionUI();

            console.log('從分享碼載入路線:', result);

        } catch (error) {
            console.error('載入分享碼失敗:', error);
            alert('無法載入分享的路線：' + error.message);
        }
    }

    // 公開 API
    return {
        init: init,
        removeAnimal: removeAnimal,
        loadFromShareCode: loadFromShareCode,
        planRoute: planRoute,
        clearSelection: clearSelection
    };
})();

// 確保全域可用
window.RoutePlanner = RoutePlanner;
