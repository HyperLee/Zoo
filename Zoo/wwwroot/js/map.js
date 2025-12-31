/**
 * 動物園互動地圖模組
 * 提供地圖縮放、拖曳、區域點擊等互動功能
 */
const ZooMap = (function () {
    'use strict';

    // 設定常數
    const CONFIG = {
        minScale: 0.5,
        maxScale: 3,
        scaleStep: 0.2,
        animationDuration: 300
    };

    // 狀態變數
    let state = {
        scale: 1,
        translateX: 0,
        translateY: 0,
        isDragging: false,
        startX: 0,
        startY: 0,
        zonesData: [],
        selectedZoneId: null,
        svgDocument: null
    };

    // DOM 元素參考
    let elements = {
        mapContainer: null,
        mapWrapper: null,
        zooMap: null,
        mapLoading: null,
        zoneList: null,
        zoneAnimalsCard: null,
        zoneAnimalsList: null,
        zoneAnimalsLoading: null,
        zoneAnimalsEmpty: null,
        selectedZoneName: null,
        viewAllZoneAnimals: null,
        zoomIn: null,
        zoomOut: null,
        resetZoom: null,
        helpCard: null
    };

    /**
     * 初始化地圖模組
     * @param {Array} zonesData - 區域資料陣列
     */
    function init(zonesData) {
        state.zonesData = zonesData || [];
        
        // 取得 DOM 元素參考
        elements.mapContainer = document.getElementById('mapContainer');
        elements.mapWrapper = document.getElementById('mapWrapper');
        elements.zooMap = document.getElementById('zooMap');
        elements.mapLoading = document.getElementById('mapLoading');
        elements.zoneList = document.getElementById('zoneList');
        elements.zoneAnimalsCard = document.getElementById('zoneAnimalsCard');
        elements.zoneAnimalsList = document.getElementById('zoneAnimalsList');
        elements.zoneAnimalsLoading = document.getElementById('zoneAnimalsLoading');
        elements.zoneAnimalsEmpty = document.getElementById('zoneAnimalsEmpty');
        elements.selectedZoneName = document.getElementById('selectedZoneName');
        elements.viewAllZoneAnimals = document.getElementById('viewAllZoneAnimals');
        elements.zoomIn = document.getElementById('zoomIn');
        elements.zoomOut = document.getElementById('zoomOut');
        elements.resetZoom = document.getElementById('resetZoom');
        elements.helpCard = document.getElementById('helpCard');

        // 綁定事件
        bindControlEvents();
        bindMapEvents();
        bindZoneListEvents();

        // 載入 SVG 完成後初始化
        if (elements.zooMap) {
            elements.zooMap.addEventListener('load', onSvgLoad);
            
            // 檢查 SVG 是否已載入
            if (elements.zooMap.contentDocument) {
                onSvgLoad();
            }
        }

        console.log('ZooMap 初始化完成');
    }

    /**
     * SVG 載入完成處理
     */
    function onSvgLoad() {
        try {
            state.svgDocument = elements.zooMap.contentDocument;
            
            if (state.svgDocument) {
                // 隱藏載入指示器
                if (elements.mapLoading) {
                    elements.mapLoading.style.display = 'none';
                }

                // 綁定 SVG 內部區域的點擊事件
                bindSvgZoneEvents();
                
                console.log('SVG 地圖載入完成');
            }
        } catch (error) {
            console.error('SVG 載入錯誤:', error);
        }
    }

    /**
     * 綁定 SVG 區域事件
     */
    function bindSvgZoneEvents() {
        if (!state.svgDocument) return;

        const zonePaths = state.svgDocument.querySelectorAll('.zone-path');
        
        zonePaths.forEach(path => {
            // 點擊事件
            path.addEventListener('click', function (e) {
                e.preventDefault();
                const zoneId = this.getAttribute('data-zone-id');
                selectZone(zoneId);
            });

            // Hover 效果
            path.addEventListener('mouseenter', function () {
                this.style.cursor = 'pointer';
                this.style.filter = 'brightness(1.1)';
            });

            path.addEventListener('mouseleave', function () {
                this.style.filter = '';
            });
        });
    }

    /**
     * 綁定控制按鈕事件
     */
    function bindControlEvents() {
        if (elements.zoomIn) {
            elements.zoomIn.addEventListener('click', () => zoom(CONFIG.scaleStep));
        }
        
        if (elements.zoomOut) {
            elements.zoomOut.addEventListener('click', () => zoom(-CONFIG.scaleStep));
        }
        
        if (elements.resetZoom) {
            elements.resetZoom.addEventListener('click', resetView);
        }
    }

    /**
     * 綁定地圖事件（滾輪、拖曳）
     */
    function bindMapEvents() {
        if (!elements.mapContainer) return;

        // 滾輪縮放
        elements.mapContainer.addEventListener('wheel', function (e) {
            e.preventDefault();
            const delta = e.deltaY > 0 ? -CONFIG.scaleStep : CONFIG.scaleStep;
            zoom(delta);
        }, { passive: false });

        // 拖曳功能
        elements.mapContainer.addEventListener('mousedown', startDrag);
        document.addEventListener('mousemove', drag);
        document.addEventListener('mouseup', endDrag);

        // 觸控支援
        elements.mapContainer.addEventListener('touchstart', handleTouchStart, { passive: false });
        elements.mapContainer.addEventListener('touchmove', handleTouchMove, { passive: false });
        elements.mapContainer.addEventListener('touchend', handleTouchEnd);
    }

    /**
     * 綁定區域清單事件
     */
    function bindZoneListEvents() {
        if (!elements.zoneList) return;

        const zoneItems = elements.zoneList.querySelectorAll('.zone-item');
        
        zoneItems.forEach(item => {
            item.addEventListener('click', function (e) {
                e.preventDefault();
                const zoneId = this.getAttribute('data-zone-id');
                selectZone(zoneId);
                
                // 高亮並定位到地圖上的區域
                highlightZoneOnMap(zoneId);
            });
        });
    }

    /**
     * 縮放地圖
     * @param {number} delta - 縮放增量
     */
    function zoom(delta) {
        const newScale = Math.max(CONFIG.minScale, Math.min(CONFIG.maxScale, state.scale + delta));
        
        if (newScale !== state.scale) {
            state.scale = newScale;
            updateTransform();
        }
    }

    /**
     * 重置視圖
     */
    function resetView() {
        state.scale = 1;
        state.translateX = 0;
        state.translateY = 0;
        updateTransform(true);
    }

    /**
     * 更新地圖變換
     * @param {boolean} animate - 是否動畫
     */
    function updateTransform(animate = false) {
        if (!elements.mapWrapper) return;

        if (animate) {
            elements.mapWrapper.style.transition = `transform ${CONFIG.animationDuration}ms ease`;
        } else {
            elements.mapWrapper.style.transition = '';
        }

        elements.mapWrapper.style.transform = 
            `translate(${state.translateX}px, ${state.translateY}px) scale(${state.scale})`;
    }

    /**
     * 開始拖曳
     */
    function startDrag(e) {
        if (e.button !== 0) return; // 只處理左鍵
        
        state.isDragging = true;
        state.startX = e.clientX - state.translateX;
        state.startY = e.clientY - state.translateY;
        
        if (elements.mapContainer) {
            elements.mapContainer.style.cursor = 'grabbing';
        }
    }

    /**
     * 拖曳中
     */
    function drag(e) {
        if (!state.isDragging) return;
        
        e.preventDefault();
        state.translateX = e.clientX - state.startX;
        state.translateY = e.clientY - state.startY;
        updateTransform();
    }

    /**
     * 結束拖曳
     */
    function endDrag() {
        state.isDragging = false;
        
        if (elements.mapContainer) {
            elements.mapContainer.style.cursor = 'grab';
        }
    }

    /**
     * 觸控開始
     */
    function handleTouchStart(e) {
        if (e.touches.length === 1) {
            const touch = e.touches[0];
            state.isDragging = true;
            state.startX = touch.clientX - state.translateX;
            state.startY = touch.clientY - state.translateY;
        }
    }

    /**
     * 觸控移動
     */
    function handleTouchMove(e) {
        if (!state.isDragging || e.touches.length !== 1) return;
        
        e.preventDefault();
        const touch = e.touches[0];
        state.translateX = touch.clientX - state.startX;
        state.translateY = touch.clientY - state.startY;
        updateTransform();
    }

    /**
     * 觸控結束
     */
    function handleTouchEnd() {
        state.isDragging = false;
    }

    /**
     * 選擇區域
     * @param {string} zoneId - 區域 ID
     */
    function selectZone(zoneId) {
        if (!zoneId) return;

        state.selectedZoneId = zoneId;
        
        // 更新區域清單的選中狀態
        updateZoneListSelection(zoneId);
        
        // 載入並顯示區域動物
        loadZoneAnimals(zoneId);
        
        // 在 SVG 上高亮區域
        highlightZoneOnMap(zoneId);

        console.log('選擇區域:', zoneId);
    }

    /**
     * 更新區域清單選中狀態
     * @param {string} zoneId - 區域 ID
     */
    function updateZoneListSelection(zoneId) {
        if (!elements.zoneList) return;

        const zoneItems = elements.zoneList.querySelectorAll('.zone-item');
        
        zoneItems.forEach(item => {
            if (item.getAttribute('data-zone-id') === zoneId) {
                item.classList.add('active');
            } else {
                item.classList.remove('active');
            }
        });
    }

    /**
     * 在地圖上高亮區域
     * @param {string} zoneId - 區域 ID
     */
    function highlightZoneOnMap(zoneId) {
        if (!state.svgDocument) return;

        // 重置所有區域樣式
        const allPaths = state.svgDocument.querySelectorAll('.zone-path');
        allPaths.forEach(path => {
            path.style.strokeWidth = '3';
            path.style.filter = '';
        });

        // 高亮選中區域
        const selectedPath = state.svgDocument.querySelector(`[data-zone-id="${zoneId}"]`);
        if (selectedPath) {
            selectedPath.style.strokeWidth = '5';
            selectedPath.style.filter = 'drop-shadow(0 0 8px rgba(0,0,0,0.5))';
        }
    }

    /**
     * 載入區域動物
     * @param {string} zoneId - 區域 ID
     */
    async function loadZoneAnimals(zoneId) {
        if (!elements.zoneAnimalsCard) return;

        // 顯示動物卡片，隱藏說明卡片
        elements.zoneAnimalsCard.style.display = 'block';
        if (elements.helpCard) {
            elements.helpCard.style.display = 'none';
        }

        // 顯示載入中
        showAnimalsLoading(true);

        // 找出區域名稱
        const zone = state.zonesData.find(z => z.id === zoneId);
        if (zone && elements.selectedZoneName) {
            elements.selectedZoneName.textContent = zone.nameZh;
        }

        // 更新「查看更多」連結
        if (elements.viewAllZoneAnimals) {
            elements.viewAllZoneAnimals.href = `/Animals?zone=${zoneId}`;
        }

        try {
            const response = await fetch(`/api/zones/${zoneId}/animals`);
            
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }

            const data = await response.json();
            renderAnimals(data.animals);
            
        } catch (error) {
            console.error('載入區域動物失敗:', error);
            renderAnimals([]);
        }
    }

    /**
     * 顯示/隱藏載入中狀態
     * @param {boolean} show - 是否顯示
     */
    function showAnimalsLoading(show) {
        if (elements.zoneAnimalsLoading) {
            elements.zoneAnimalsLoading.style.display = show ? 'block' : 'none';
        }
        if (elements.zoneAnimalsList) {
            elements.zoneAnimalsList.style.display = show ? 'none' : 'flex';
        }
        if (elements.zoneAnimalsEmpty) {
            elements.zoneAnimalsEmpty.style.display = 'none';
        }
    }

    /**
     * 渲染動物清單
     * @param {Array} animals - 動物資料陣列
     */
    function renderAnimals(animals) {
        showAnimalsLoading(false);

        if (!elements.zoneAnimalsList) return;

        if (!animals || animals.length === 0) {
            elements.zoneAnimalsList.innerHTML = '';
            if (elements.zoneAnimalsEmpty) {
                elements.zoneAnimalsEmpty.style.display = 'block';
            }
            return;
        }

        // 只顯示前 4 隻動物
        const displayAnimals = animals.slice(0, 4);

        elements.zoneAnimalsList.innerHTML = displayAnimals.map(animal => `
            <div class="col">
                <a href="/Animals/${animal.id}" class="text-decoration-none">
                    <div class="card h-100 animal-mini-card">
                        <img src="${animal.thumbnailUrl || '/images/animals/placeholder.webp'}" 
                             class="card-img-top" 
                             alt="${animal.chineseName}"
                             style="height: 80px; object-fit: cover;"
                             onerror="this.src='/images/animals/placeholder.webp'">
                        <div class="card-body p-2 text-center">
                            <h6 class="card-title mb-0 small">${animal.chineseName}</h6>
                            <small class="text-muted">${animal.englishName || ''}</small>
                        </div>
                    </div>
                </a>
            </div>
        `).join('');
    }

    /**
     * 定位到指定區域
     * @param {string} zoneId - 區域 ID
     */
    function panToZone(zoneId) {
        const zone = state.zonesData.find(z => z.id === zoneId);
        if (!zone || !elements.mapContainer) return;

        // 計算需要的位移以將區域置中
        const containerRect = elements.mapContainer.getBoundingClientRect();
        const centerX = containerRect.width / 2;
        const centerY = containerRect.height / 2;

        // 根據區域在 SVG 中的位置計算位移（假設 SVG 為 800x600）
        const svgWidth = 800;
        const svgHeight = 600;
        
        // 這裡使用 zones.json 中的 position 資料
        // 由於 SVG viewBox 是 800x600，我們需要縮放
        state.translateX = centerX - (zone.position?.x || svgWidth / 2) * state.scale;
        state.translateY = centerY - (zone.position?.y || svgHeight / 2) * state.scale;

        updateTransform(true);
    }

    // 公開 API
    return {
        init: init,
        zoom: zoom,
        resetView: resetView,
        selectZone: selectZone,
        panToZone: panToZone
    };
})();

// 確保全域可用
window.ZooMap = ZooMap;
