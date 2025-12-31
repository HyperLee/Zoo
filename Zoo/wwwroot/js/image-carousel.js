/**
 * 圖片輪播模組
 * 提供動物圖片輪播與姿態切換功能
 * @module ImageCarousel
 */
(function () {
    'use strict';

    /**
     * 圖片輪播類別
     */
    class ImageCarousel {
        /**
         * 建立圖片輪播實例
         * @param {HTMLElement} container - 容器元素
         * @param {Object} options - 設定選項
         */
        constructor(container, options = {}) {
            this.container = container;
            this.options = {
                autoPlay: false,
                autoPlayInterval: 4000,
                showIndicators: true,
                showControls: true,
                showThumbnails: true,
                transitionDuration: 300,
                placeholderImage: '/images/placeholder-animal.webp',
                onSlideChange: null,
                ...options
            };

            this.images = [];
            this.currentIndex = 0;
            this.isPlaying = false;
            this.autoPlayTimer = null;

            this._init();
        }

        /**
         * 初始化輪播器
         * @private
         */
        _init() {
            // 從 data 屬性讀取圖片清單
            const imagesData = this.container.dataset.images;
            if (imagesData) {
                try {
                    this.images = JSON.parse(imagesData);
                } catch (e) {
                    console.error('無法解析圖片資料:', e);
                    this.images = [];
                }
            }

            if (this.images.length === 0) {
                console.warn('圖片輪播器沒有圖片資料');
                return;
            }

            this._render();
            this._bindEvents();

            if (this.options.autoPlay && this.images.length > 1) {
                this.play();
            }
        }

        /**
         * 渲染輪播器 HTML
         * @private
         */
        _render() {
            const animalName = this.container.dataset.animalName || '動物';

            this.container.innerHTML = `
                <div class="image-carousel" role="region" aria-label="${animalName} 圖片輪播">
                    <div class="carousel-main">
                        <img src="${this.images[0]}" 
                             class="carousel-image" 
                             id="${this.container.id}-main-image"
                             alt="${animalName} 的照片 1"
                             onerror="this.src='${this.options.placeholderImage}'">
                        
                        ${this.images.length > 1 && this.options.showControls ? `
                            <button type="button" 
                                    class="carousel-control carousel-control-prev"
                                    aria-label="上一張圖片"
                                    title="上一張">
                                <i class="bi bi-chevron-left"></i>
                            </button>
                            <button type="button" 
                                    class="carousel-control carousel-control-next"
                                    aria-label="下一張圖片"
                                    title="下一張">
                                <i class="bi bi-chevron-right"></i>
                            </button>
                        ` : ''}
                        
                        ${this.images.length > 1 && this.options.showIndicators ? `
                            <div class="carousel-indicators">
                                ${this.images.map((_, i) => `
                                    <button type="button" 
                                            class="carousel-indicator ${i === 0 ? 'active' : ''}"
                                            data-index="${i}"
                                            aria-label="切換至第 ${i + 1} 張圖片"
                                            aria-current="${i === 0 ? 'true' : 'false'}">
                                    </button>
                                `).join('')}
                            </div>
                        ` : ''}
                        
                        ${this.images.length > 1 ? `
                            <div class="carousel-counter">
                                <span id="${this.container.id}-counter">1</span> / ${this.images.length}
                            </div>
                        ` : ''}
                    </div>
                    
                    ${this.images.length > 1 && this.options.showThumbnails ? `
                        <div class="carousel-thumbnails" role="tablist" aria-label="圖片縮圖">
                            ${this.images.map((img, i) => `
                                <button type="button" 
                                        class="carousel-thumbnail ${i === 0 ? 'active' : ''}"
                                        data-index="${i}"
                                        role="tab"
                                        aria-selected="${i === 0 ? 'true' : 'false'}"
                                        aria-label="檢視第 ${i + 1} 張圖片"
                                        title="圖片 ${i + 1}">
                                    <img src="${img}" 
                                         alt="${animalName} 縮圖 ${i + 1}"
                                         onerror="this.src='${this.options.placeholderImage}'">
                                </button>
                            `).join('')}
                        </div>
                    ` : ''}
                    
                    ${this.images.length > 1 && this.options.autoPlay ? `
                        <div class="carousel-autoplay">
                            <button type="button" 
                                    class="btn btn-sm btn-outline-secondary carousel-autoplay-btn"
                                    aria-label="自動播放控制"
                                    title="暫停自動播放">
                                <i class="bi bi-pause-fill"></i>
                            </button>
                        </div>
                    ` : ''}
                </div>
            `;

            // 儲存 DOM 參考
            this.mainImage = document.getElementById(`${this.container.id}-main-image`);
            this.counter = document.getElementById(`${this.container.id}-counter`);
            this.indicators = this.container.querySelectorAll('.carousel-indicator');
            this.thumbnails = this.container.querySelectorAll('.carousel-thumbnail');
            this.prevBtn = this.container.querySelector('.carousel-control-prev');
            this.nextBtn = this.container.querySelector('.carousel-control-next');
            this.autoPlayBtn = this.container.querySelector('.carousel-autoplay-btn');
        }

        /**
         * 綁定事件
         * @private
         */
        _bindEvents() {
            // 上一張/下一張按鈕
            if (this.prevBtn) {
                this.prevBtn.addEventListener('click', () => this.prev());
            }
            if (this.nextBtn) {
                this.nextBtn.addEventListener('click', () => this.next());
            }

            // 指示器點擊
            this.indicators.forEach(indicator => {
                indicator.addEventListener('click', () => {
                    const index = parseInt(indicator.dataset.index, 10);
                    this.goTo(index);
                });
            });

            // 縮圖點擊
            this.thumbnails.forEach(thumbnail => {
                thumbnail.addEventListener('click', () => {
                    const index = parseInt(thumbnail.dataset.index, 10);
                    this.goTo(index);
                });
            });

            // 自動播放按鈕
            if (this.autoPlayBtn) {
                this.autoPlayBtn.addEventListener('click', () => {
                    if (this.isPlaying) {
                        this.pause();
                    } else {
                        this.play();
                    }
                });
            }

            // 鍵盤導航
            this.container.addEventListener('keydown', (e) => {
                switch (e.key) {
                    case 'ArrowLeft':
                        e.preventDefault();
                        this.prev();
                        break;
                    case 'ArrowRight':
                        e.preventDefault();
                        this.next();
                        break;
                    case ' ':
                        e.preventDefault();
                        if (this.options.autoPlay) {
                            this.isPlaying ? this.pause() : this.play();
                        }
                        break;
                }
            });

            // 滑鼠懸停時暫停自動播放
            this.container.addEventListener('mouseenter', () => {
                if (this.isPlaying) {
                    this._stopAutoPlayTimer();
                }
            });

            this.container.addEventListener('mouseleave', () => {
                if (this.isPlaying) {
                    this._startAutoPlayTimer();
                }
            });

            // 觸控滑動支援
            this._initTouchEvents();
        }

        /**
         * 初始化觸控事件
         * @private
         */
        _initTouchEvents() {
            let touchStartX = 0;
            let touchEndX = 0;

            this.container.addEventListener('touchstart', (e) => {
                touchStartX = e.changedTouches[0].screenX;
            }, { passive: true });

            this.container.addEventListener('touchend', (e) => {
                touchEndX = e.changedTouches[0].screenX;
                const diff = touchStartX - touchEndX;

                // 滑動距離需超過 50px
                if (Math.abs(diff) > 50) {
                    if (diff > 0) {
                        this.next();
                    } else {
                        this.prev();
                    }
                }
            }, { passive: true });
        }

        /**
         * 切換到指定圖片
         * @param {number} index - 圖片索引
         */
        goTo(index) {
            if (index < 0 || index >= this.images.length) return;
            if (index === this.currentIndex) return;

            const previousIndex = this.currentIndex;
            this.currentIndex = index;

            // 更新主圖片
            this.mainImage.style.opacity = '0';
            setTimeout(() => {
                this.mainImage.src = this.images[index];
                this.mainImage.alt = `${this.container.dataset.animalName || '動物'} 的照片 ${index + 1}`;
                this.mainImage.style.opacity = '1';
            }, this.options.transitionDuration / 2);

            // 更新計數器
            if (this.counter) {
                this.counter.textContent = index + 1;
            }

            // 更新指示器
            this.indicators.forEach((indicator, i) => {
                indicator.classList.toggle('active', i === index);
                indicator.setAttribute('aria-current', i === index ? 'true' : 'false');
            });

            // 更新縮圖
            this.thumbnails.forEach((thumbnail, i) => {
                thumbnail.classList.toggle('active', i === index);
                thumbnail.setAttribute('aria-selected', i === index ? 'true' : 'false');
            });

            // 觸發回呼
            if (typeof this.options.onSlideChange === 'function') {
                this.options.onSlideChange(index, previousIndex, this);
            }
        }

        /**
         * 下一張圖片
         */
        next() {
            const nextIndex = (this.currentIndex + 1) % this.images.length;
            this.goTo(nextIndex);
        }

        /**
         * 上一張圖片
         */
        prev() {
            const prevIndex = (this.currentIndex - 1 + this.images.length) % this.images.length;
            this.goTo(prevIndex);
        }

        /**
         * 開始自動播放
         */
        play() {
            if (this.images.length <= 1) return;
            
            this.isPlaying = true;
            this._startAutoPlayTimer();

            if (this.autoPlayBtn) {
                this.autoPlayBtn.innerHTML = '<i class="bi bi-pause-fill"></i>';
                this.autoPlayBtn.setAttribute('title', '暫停自動播放');
            }
        }

        /**
         * 暫停自動播放
         */
        pause() {
            this.isPlaying = false;
            this._stopAutoPlayTimer();

            if (this.autoPlayBtn) {
                this.autoPlayBtn.innerHTML = '<i class="bi bi-play-fill"></i>';
                this.autoPlayBtn.setAttribute('title', '開始自動播放');
            }
        }

        /**
         * 啟動自動播放計時器
         * @private
         */
        _startAutoPlayTimer() {
            this._stopAutoPlayTimer();
            this.autoPlayTimer = setInterval(() => {
                this.next();
            }, this.options.autoPlayInterval);
        }

        /**
         * 停止自動播放計時器
         * @private
         */
        _stopAutoPlayTimer() {
            if (this.autoPlayTimer) {
                clearInterval(this.autoPlayTimer);
                this.autoPlayTimer = null;
            }
        }

        /**
         * 取得當前圖片索引
         * @returns {number} 當前索引
         */
        getCurrentIndex() {
            return this.currentIndex;
        }

        /**
         * 取得圖片總數
         * @returns {number} 圖片數量
         */
        getImageCount() {
            return this.images.length;
        }

        /**
         * 銷毀輪播器
         */
        destroy() {
            this.pause();
            this.container.innerHTML = '';
        }
    }

    /**
     * 圖片輪播管理器
     * 管理頁面上所有的輪播器實例
     */
    class ImageCarouselManager {
        constructor() {
            this.carousels = new Map();
        }

        /**
         * 建立並註冊輪播器
         * @param {string} id - 輪播器 ID
         * @param {HTMLElement} container - 容器元素
         * @param {Object} options - 設定選項
         * @returns {ImageCarousel} 輪播器實例
         */
        create(id, container, options = {}) {
            // 如果已存在，先銷毀
            if (this.carousels.has(id)) {
                this.destroy(id);
            }

            const carousel = new ImageCarousel(container, options);
            this.carousels.set(id, carousel);
            return carousel;
        }

        /**
         * 取得輪播器實例
         * @param {string} id - 輪播器 ID
         * @returns {ImageCarousel|undefined} 輪播器實例
         */
        get(id) {
            return this.carousels.get(id);
        }

        /**
         * 暫停所有輪播器
         */
        pauseAll() {
            this.carousels.forEach(carousel => carousel.pause());
        }

        /**
         * 銷毀指定輪播器
         * @param {string} id - 輪播器 ID
         */
        destroy(id) {
            const carousel = this.carousels.get(id);
            if (carousel) {
                carousel.destroy();
                this.carousels.delete(id);
            }
        }

        /**
         * 銷毀所有輪播器
         */
        destroyAll() {
            this.carousels.forEach(carousel => carousel.destroy());
            this.carousels.clear();
        }
    }

    // 建立全域管理器實例
    window.imageCarouselManager = new ImageCarouselManager();

    /**
     * 初始化動物圖片輪播器
     * @param {string} containerId - 容器元素 ID
     * @param {string[]} images - 圖片路徑陣列
     * @param {string} animalName - 動物名稱
     * @param {Object} options - 其他選項
     * @returns {ImageCarousel} 輪播器實例
     */
    window.initAnimalImageCarousel = function (containerId, images, animalName, options = {}) {
        const container = document.getElementById(containerId);
        if (!container) {
            console.error('找不到輪播器容器:', containerId);
            return null;
        }

        // 設定 data 屬性
        container.dataset.images = JSON.stringify(images);
        container.dataset.animalName = animalName;

        // 建立輪播器
        return window.imageCarouselManager.create(containerId, container, {
            autoPlay: false,
            showThumbnails: true,
            showControls: true,
            showIndicators: true,
            ...options
        });
    };

    /**
     * 簡易圖片切換（用於縮圖點擊）
     * @param {string} imageUrl - 圖片 URL
     * @param {HTMLElement} thumbnail - 縮圖元素
     * @param {string} mainImageId - 主圖片元素 ID
     */
    window.changeMainImageSimple = function (imageUrl, thumbnail, mainImageId = 'mainImage') {
        const mainImage = document.getElementById(mainImageId);
        if (!mainImage) return;

        // 淡出效果
        mainImage.style.transition = 'opacity 0.2s ease-in-out';
        mainImage.style.opacity = '0';

        setTimeout(() => {
            mainImage.src = imageUrl;
            mainImage.style.opacity = '1';
        }, 200);

        // 更新縮圖活動狀態
        const thumbnailContainer = thumbnail.closest('.carousel-thumbnails, .card-body');
        if (thumbnailContainer) {
            thumbnailContainer.querySelectorAll('.thumbnail-selector, .carousel-thumbnail').forEach(t => {
                t.classList.remove('active');
                t.setAttribute('aria-selected', 'false');
            });
        }
        thumbnail.classList.add('active');
        thumbnail.setAttribute('aria-selected', 'true');
    };

})();
