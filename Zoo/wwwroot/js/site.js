// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// ===== 無障礙功能模組 =====

/**
 * 無障礙設定管理器
 * 管理高對比模式、字體大小調整等無障礙功能
 */
const AccessibilityManager = {
    storageKey: 'zoo-accessibility-settings',

    /**
     * 初始化無障礙功能
     */
    init: function() {
        this.loadSettings();
        this.createAccessibilityToolbar();
        this.initKeyboardNavigation();
    },

    /**
     * 載入已儲存的設定
     */
    loadSettings: function() {
        const settings = this.getSettings();
        
        if (settings.highContrast) {
            document.body.classList.add('high-contrast');
        }
        
        if (settings.fontSize) {
            document.body.classList.add(`font-size-${settings.fontSize}`);
        }
    },

    /**
     * 取得設定
     * @returns {Object} 無障礙設定物件
     */
    getSettings: function() {
        try {
            const stored = localStorage.getItem(this.storageKey);
            return stored ? JSON.parse(stored) : {};
        } catch (e) {
            console.warn('無法讀取無障礙設定:', e);
            return {};
        }
    },

    /**
     * 儲存設定
     * @param {Object} settings - 無障礙設定物件
     */
    saveSettings: function(settings) {
        try {
            localStorage.setItem(this.storageKey, JSON.stringify(settings));
        } catch (e) {
            console.warn('無法儲存無障礙設定:', e);
        }
    },

    /**
     * 切換高對比模式
     */
    toggleHighContrast: function() {
        const settings = this.getSettings();
        settings.highContrast = !settings.highContrast;
        
        if (settings.highContrast) {
            document.body.classList.add('high-contrast');
        } else {
            document.body.classList.remove('high-contrast');
        }
        
        this.saveSettings(settings);
        this.updateToolbarState();
    },

    /**
     * 設定字體大小
     * @param {string} size - 字體大小 (small, normal, large, xlarge)
     */
    setFontSize: function(size) {
        const settings = this.getSettings();
        const validSizes = ['small', 'normal', 'large', 'xlarge'];
        
        // 移除所有字體大小類別
        validSizes.forEach(s => {
            document.body.classList.remove(`font-size-${s}`);
        });
        
        if (validSizes.includes(size) && size !== 'normal') {
            document.body.classList.add(`font-size-${size}`);
            settings.fontSize = size;
        } else {
            settings.fontSize = null;
        }
        
        this.saveSettings(settings);
        this.updateToolbarState();
    },

    /**
     * 建立無障礙工具列
     */
    createAccessibilityToolbar: function() {
        const toolbar = document.createElement('div');
        toolbar.className = 'accessibility-toolbar';
        toolbar.setAttribute('role', 'toolbar');
        toolbar.setAttribute('aria-label', '無障礙工具');
        
        toolbar.innerHTML = `
            <button type="button" class="btn btn-sm btn-outline-secondary" 
                    id="toggleHighContrastBtn" 
                    onclick="AccessibilityManager.toggleHighContrast()"
                    aria-pressed="false"
                    title="切換高對比模式">
                <i class="bi bi-circle-half"></i>
                <span class="btn-text">高對比</span>
            </button>
            <div class="btn-group btn-group-sm" role="group" aria-label="字體大小">
                <button type="button" class="btn btn-outline-secondary" 
                        onclick="AccessibilityManager.setFontSize('small')"
                        title="小字體">
                    <i class="bi bi-type"></i><small>A</small>
                </button>
                <button type="button" class="btn btn-outline-secondary" 
                        onclick="AccessibilityManager.setFontSize('normal')"
                        title="標準字體">
                    <i class="bi bi-type"></i>A
                </button>
                <button type="button" class="btn btn-outline-secondary" 
                        onclick="AccessibilityManager.setFontSize('large')"
                        title="大字體">
                    <i class="bi bi-type"></i><strong>A</strong>
                </button>
            </div>
        `;
        
        document.body.appendChild(toolbar);
        this.updateToolbarState();
    },

    /**
     * 更新工具列狀態
     */
    updateToolbarState: function() {
        const settings = this.getSettings();
        const contrastBtn = document.getElementById('toggleHighContrastBtn');
        
        if (contrastBtn) {
            contrastBtn.setAttribute('aria-pressed', settings.highContrast ? 'true' : 'false');
            if (settings.highContrast) {
                contrastBtn.classList.add('active');
            } else {
                contrastBtn.classList.remove('active');
            }
        }
    },

    /**
     * 初始化鍵盤導航
     */
    initKeyboardNavigation: function() {
        // 為可聚焦元素添加鍵盤支援
        document.addEventListener('keydown', function(e) {
            // Skip link 功能 (Alt + 1)
            if (e.altKey && e.key === '1') {
                e.preventDefault();
                const mainContent = document.querySelector('main') || document.querySelector('[role="main"]');
                if (mainContent) {
                    mainContent.setAttribute('tabindex', '-1');
                    mainContent.focus();
                }
            }
            
            // 返回頂部 (Alt + Home)
            if (e.altKey && e.key === 'Home') {
                e.preventDefault();
                ScrollManager.scrollToTop();
            }
        });

        // 為動物卡片添加鍵盤支援
        document.querySelectorAll('.animal-card').forEach(card => {
            card.setAttribute('tabindex', '0');
            card.addEventListener('keydown', function(e) {
                if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    const link = this.querySelector('a');
                    if (link) {
                        link.click();
                    }
                }
            });
        });
    }
};

// ===== 返回頂部功能模組 =====

/**
 * 捲動管理器
 * 管理返回頂部按鈕和平滑捲動功能
 */
const ScrollManager = {
    scrollButton: null,
    showThreshold: 300,

    /**
     * 初始化捲動管理器
     */
    init: function() {
        this.createScrollButton();
        this.bindScrollEvents();
    },

    /**
     * 建立返回頂部按鈕
     */
    createScrollButton: function() {
        const button = document.createElement('button');
        button.id = 'scrollToTopBtn';
        button.className = 'btn btn-success rounded-circle shadow';
        button.setAttribute('type', 'button');
        button.setAttribute('aria-label', '返回頂部');
        button.setAttribute('title', '返回頂部');
        button.innerHTML = '<i class="bi bi-arrow-up"></i>';
        
        // 樣式設定
        button.style.cssText = `
            position: fixed;
            bottom: 20px;
            right: 20px;
            width: 50px;
            height: 50px;
            display: none;
            z-index: 1050;
            transition: opacity 0.3s, transform 0.3s;
            opacity: 0;
        `;
        
        button.addEventListener('click', this.scrollToTop);
        document.body.appendChild(button);
        this.scrollButton = button;
    },

    /**
     * 綁定捲動事件
     */
    bindScrollEvents: function() {
        const self = this;
        let ticking = false;
        
        window.addEventListener('scroll', function() {
            if (!ticking) {
                window.requestAnimationFrame(function() {
                    self.handleScroll();
                    ticking = false;
                });
                ticking = true;
            }
        });
    },

    /**
     * 處理捲動事件
     */
    handleScroll: function() {
        if (!this.scrollButton) return;
        
        const scrollY = window.scrollY || window.pageYOffset;
        
        if (scrollY > this.showThreshold) {
            this.scrollButton.style.display = 'flex';
            this.scrollButton.style.alignItems = 'center';
            this.scrollButton.style.justifyContent = 'center';
            setTimeout(() => {
                this.scrollButton.style.opacity = '1';
                this.scrollButton.style.transform = 'translateY(0)';
            }, 10);
        } else {
            this.scrollButton.style.opacity = '0';
            this.scrollButton.style.transform = 'translateY(20px)';
            setTimeout(() => {
                if (this.scrollButton.style.opacity === '0') {
                    this.scrollButton.style.display = 'none';
                }
            }, 300);
        }
    },

    /**
     * 平滑捲動到頂部
     */
    scrollToTop: function() {
        window.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    }
};

// ===== 裝置偵測模組 =====

/**
 * 裝置偵測器
 * 偵測行動裝置和低解析度螢幕並顯示提示
 */
const DeviceDetector = {
    minWidth: 1366,
    minHeight: 768,
    hasShownMobileWarning: false,
    hasShownResolutionWarning: false,

    /**
     * 初始化裝置偵測
     */
    init: function() {
        this.checkDevice();
        this.checkResolution();
        
        // 監聽視窗大小變化
        window.addEventListener('resize', () => {
            this.checkResolution();
        });
    },

    /**
     * 偵測行動裝置
     */
    checkDevice: function() {
        const isMobile = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
        const isTouchDevice = 'ontouchstart' in window || navigator.maxTouchPoints > 0;
        
        if ((isMobile || isTouchDevice) && !this.hasShownMobileWarning) {
            this.showMobileWarning();
            this.hasShownMobileWarning = true;
        }
    },

    /**
     * 偵測螢幕解析度
     */
    checkResolution: function() {
        const width = window.innerWidth;
        const height = window.innerHeight;
        
        if ((width < this.minWidth || height < this.minHeight) && !this.hasShownResolutionWarning) {
            this.showResolutionWarning(width, height);
            this.hasShownResolutionWarning = true;
        }
    },

    /**
     * 顯示行動裝置警告
     */
    showMobileWarning: function() {
        this.showWarningToast(
            '行動裝置提示',
            '本網站針對電腦瀏覽器最佳化。在行動裝置上，部分功能可能無法正常運作。建議使用電腦瀏覽以獲得最佳體驗。',
            'warning'
        );
    },

    /**
     * 顯示解析度警告
     * @param {number} width - 目前寬度
     * @param {number} height - 目前高度
     */
    showResolutionWarning: function(width, height) {
        this.showWarningToast(
            '螢幕解析度提示',
            `目前解析度 ${width}x${height} 較低。建議使用 ${this.minWidth}x${this.minHeight} 或更高解析度以獲得最佳瀏覽體驗。`,
            'info'
        );
    },

    /**
     * 顯示警告 Toast
     * @param {string} title - 標題
     * @param {string} message - 訊息內容
     * @param {string} type - 類型 (warning, info, danger)
     */
    showWarningToast: function(title, message, type = 'warning') {
        // 檢查是否已存在 toast 容器
        let toastContainer = document.getElementById('toastContainer');
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.id = 'toastContainer';
            toastContainer.className = 'toast-container position-fixed top-0 end-0 p-3';
            toastContainer.style.zIndex = '1080';
            document.body.appendChild(toastContainer);
        }

        const toastId = 'toast-' + Date.now();
        const iconClass = type === 'warning' ? 'bi-exclamation-triangle-fill' : 'bi-info-circle-fill';
        const bgClass = type === 'warning' ? 'text-warning' : 'text-info';
        
        const toastHtml = `
            <div id="${toastId}" class="toast" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="toast-header">
                    <i class="bi ${iconClass} ${bgClass} me-2"></i>
                    <strong class="me-auto">${title}</strong>
                    <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="關閉"></button>
                </div>
                <div class="toast-body">
                    ${message}
                </div>
            </div>
        `;
        
        toastContainer.insertAdjacentHTML('beforeend', toastHtml);
        
        const toastElement = document.getElementById(toastId);
        const toast = new bootstrap.Toast(toastElement, { autohide: true, delay: 8000 });
        toast.show();
        
        // 自動移除 toast 元素
        toastElement.addEventListener('hidden.bs.toast', function() {
            this.remove();
        });
    }
};

// ===== 頁面初始化 =====

document.addEventListener('DOMContentLoaded', function() {
    // 初始化無障礙功能
    AccessibilityManager.init();
    
    // 初始化返回頂部按鈕
    ScrollManager.init();
    
    // 初始化裝置偵測
    DeviceDetector.init();
    
    // 初始化 Bootstrap tooltips
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    tooltipTriggerList.forEach(function(tooltipTriggerEl) {
        new bootstrap.Tooltip(tooltipTriggerEl);
    });
});

// 匯出模組供其他腳本使用
window.AccessibilityManager = AccessibilityManager;
window.ScrollManager = ScrollManager;
window.DeviceDetector = DeviceDetector;
