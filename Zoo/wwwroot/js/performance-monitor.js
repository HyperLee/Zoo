/**
 * 前端效能監測模組
 * 用於記錄和分析頁面載入效能
 */
const PerformanceMonitor = {
    metrics: {},
    threshold: 3000, // 3 秒的效能目標

    /**
     * 初始化效能監測
     */
    init: function() {
        if (!window.performance || !window.performance.timing) {
            console.warn('此瀏覽器不支援 Performance API');
            return;
        }

        // 頁面完全載入後記錄指標
        if (document.readyState === 'complete') {
            this.recordMetrics();
        } else {
            window.addEventListener('load', () => {
                // 延遲執行以確保所有資源都已載入
                setTimeout(() => this.recordMetrics(), 0);
            });
        }

        // 監測長時間任務
        this.observeLongTasks();

        // 監測最大內容繪製 (LCP)
        this.observeLCP();

        // 監測首次輸入延遲 (FID)
        this.observeFID();

        // 監測累計版面配置位移 (CLS)
        this.observeCLS();
    },

    /**
     * 記錄效能指標
     */
    recordMetrics: function() {
        const timing = window.performance.timing;
        const navigationStart = timing.navigationStart;

        this.metrics = {
            // 頁面載入時間
            pageLoadTime: timing.loadEventEnd - navigationStart,
            
            // DOM 內容載入時間
            domContentLoaded: timing.domContentLoadedEventEnd - navigationStart,
            
            // DOM 互動時間
            domInteractive: timing.domInteractive - navigationStart,
            
            // 首次繪製時間 (使用 Performance Entry API)
            firstPaint: this.getFirstPaint(),
            
            // 首次內容繪製時間
            firstContentfulPaint: this.getFirstContentfulPaint(),
            
            // DNS 查詢時間
            dnsLookup: timing.domainLookupEnd - timing.domainLookupStart,
            
            // TCP 連接時間
            tcpConnect: timing.connectEnd - timing.connectStart,
            
            // 伺服器回應時間
            serverResponse: timing.responseEnd - timing.requestStart,
            
            // DOM 解析時間
            domParsing: timing.domComplete - timing.domLoading,
            
            // 資源載入時間
            resourceLoading: timing.loadEventEnd - timing.domContentLoadedEventEnd,
            
            // 時間戳記
            timestamp: new Date().toISOString(),
            
            // 頁面 URL
            url: window.location.pathname
        };

        this.logMetrics();
        this.checkPerformanceThreshold();
    },

    /**
     * 取得首次繪製時間
     * @returns {number} 首次繪製時間（毫秒）
     */
    getFirstPaint: function() {
        if (window.performance && window.performance.getEntriesByType) {
            const paintEntries = window.performance.getEntriesByType('paint');
            const firstPaint = paintEntries.find(entry => entry.name === 'first-paint');
            return firstPaint ? Math.round(firstPaint.startTime) : 0;
        }
        return 0;
    },

    /**
     * 取得首次內容繪製時間
     * @returns {number} 首次內容繪製時間（毫秒）
     */
    getFirstContentfulPaint: function() {
        if (window.performance && window.performance.getEntriesByType) {
            const paintEntries = window.performance.getEntriesByType('paint');
            const fcp = paintEntries.find(entry => entry.name === 'first-contentful-paint');
            return fcp ? Math.round(fcp.startTime) : 0;
        }
        return 0;
    },

    /**
     * 監測長時間任務
     */
    observeLongTasks: function() {
        if ('PerformanceObserver' in window) {
            try {
                const observer = new PerformanceObserver((list) => {
                    for (const entry of list.getEntries()) {
                        console.warn('偵測到長時間任務:', {
                            duration: Math.round(entry.duration) + 'ms',
                            startTime: Math.round(entry.startTime) + 'ms'
                        });
                    }
                });
                observer.observe({ entryTypes: ['longtask'] });
            } catch (e) {
                // 某些瀏覽器可能不支援 longtask
            }
        }
    },

    /**
     * 監測最大內容繪製 (LCP)
     */
    observeLCP: function() {
        if ('PerformanceObserver' in window) {
            try {
                const observer = new PerformanceObserver((list) => {
                    const entries = list.getEntries();
                    const lastEntry = entries[entries.length - 1];
                    this.metrics.largestContentfulPaint = Math.round(lastEntry.startTime);
                    
                    if (this.metrics.largestContentfulPaint > 2500) {
                        console.warn('LCP 超過建議值 (2.5 秒):', this.metrics.largestContentfulPaint + 'ms');
                    }
                });
                observer.observe({ entryTypes: ['largest-contentful-paint'] });
            } catch (e) {
                // 某些瀏覽器可能不支援
            }
        }
    },

    /**
     * 監測首次輸入延遲 (FID)
     */
    observeFID: function() {
        if ('PerformanceObserver' in window) {
            try {
                const observer = new PerformanceObserver((list) => {
                    for (const entry of list.getEntries()) {
                        this.metrics.firstInputDelay = Math.round(entry.processingStart - entry.startTime);
                        
                        if (this.metrics.firstInputDelay > 100) {
                            console.warn('FID 超過建議值 (100ms):', this.metrics.firstInputDelay + 'ms');
                        }
                    }
                });
                observer.observe({ entryTypes: ['first-input'] });
            } catch (e) {
                // 某些瀏覽器可能不支援
            }
        }
    },

    /**
     * 監測累計版面配置位移 (CLS)
     */
    observeCLS: function() {
        if ('PerformanceObserver' in window) {
            try {
                let clsValue = 0;
                const observer = new PerformanceObserver((list) => {
                    for (const entry of list.getEntries()) {
                        if (!entry.hadRecentInput) {
                            clsValue += entry.value;
                        }
                    }
                    this.metrics.cumulativeLayoutShift = clsValue.toFixed(4);
                    
                    if (clsValue > 0.1) {
                        console.warn('CLS 超過建議值 (0.1):', clsValue.toFixed(4));
                    }
                });
                observer.observe({ entryTypes: ['layout-shift'] });
            } catch (e) {
                // 某些瀏覽器可能不支援
            }
        }
    },

    /**
     * 記錄效能指標到控制台
     */
    logMetrics: function() {
        console.group('📊 頁面效能指標');
        console.log('頁面載入時間:', this.metrics.pageLoadTime + 'ms');
        console.log('DOM 內容載入:', this.metrics.domContentLoaded + 'ms');
        console.log('首次繪製:', this.metrics.firstPaint + 'ms');
        console.log('首次內容繪製:', this.metrics.firstContentfulPaint + 'ms');
        console.log('DNS 查詢:', this.metrics.dnsLookup + 'ms');
        console.log('伺服器回應:', this.metrics.serverResponse + 'ms');
        console.groupEnd();
    },

    /**
     * 檢查效能是否達標
     */
    checkPerformanceThreshold: function() {
        const loadTime = this.metrics.pageLoadTime;
        
        if (loadTime > this.threshold) {
            console.warn(`⚠️ 頁面載入時間 (${loadTime}ms) 超過目標值 (${this.threshold}ms)`);
            
            // 可以在這裡發送效能警告到後端
            this.reportSlowPage();
        } else {
            console.log(`✅ 頁面載入時間 (${loadTime}ms) 符合效能目標`);
        }
    },

    /**
     * 回報慢速頁面
     */
    reportSlowPage: function() {
        // 可以實作發送到後端的邏輯
        // 這裡僅記錄到本地儲存以供分析
        try {
            const slowPages = JSON.parse(localStorage.getItem('zoo-slow-pages') || '[]');
            slowPages.push({
                url: this.metrics.url,
                loadTime: this.metrics.pageLoadTime,
                timestamp: this.metrics.timestamp
            });
            
            // 只保留最近 50 筆記錄
            if (slowPages.length > 50) {
                slowPages.shift();
            }
            
            localStorage.setItem('zoo-slow-pages', JSON.stringify(slowPages));
        } catch (e) {
            console.warn('無法儲存效能記錄:', e);
        }
    },

    /**
     * 取得效能報告
     * @returns {Object} 效能指標物件
     */
    getReport: function() {
        return { ...this.metrics };
    },

    /**
     * 取得資源載入效能
     * @returns {Array} 資源效能陣列
     */
    getResourcePerformance: function() {
        if (!window.performance || !window.performance.getEntriesByType) {
            return [];
        }

        const resources = window.performance.getEntriesByType('resource');
        return resources
            .map(resource => ({
                name: resource.name,
                type: resource.initiatorType,
                duration: Math.round(resource.duration),
                size: resource.transferSize || 0
            }))
            .sort((a, b) => b.duration - a.duration)
            .slice(0, 10); // 只返回最慢的 10 個資源
    }
};

// 自動初始化
document.addEventListener('DOMContentLoaded', function() {
    PerformanceMonitor.init();
});

// 匯出供其他模組使用
window.PerformanceMonitor = PerformanceMonitor;
