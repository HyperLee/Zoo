/**
 * 音效播放模組
 * 提供動物叫聲播放功能，包含播放、暫停、音量控制
 * @module AudioPlayer
 */
(function () {
    'use strict';

    /**
     * 音效播放器類別
     */
    class AudioPlayer {
        /**
         * 建立音效播放器實例
         * @param {HTMLAudioElement} audioElement - 音訊元素
         * @param {Object} options - 設定選項
         */
        constructor(audioElement, options = {}) {
            this.audio = audioElement;
            this.isPlaying = false;
            this.options = {
                defaultVolume: 0.7,
                fadeInDuration: 200,
                fadeOutDuration: 200,
                onPlay: null,
                onPause: null,
                onEnded: null,
                onError: null,
                ...options
            };

            this._init();
        }

        /**
         * 初始化播放器
         * @private
         */
        _init() {
            // 設定預設音量
            this.audio.volume = this.options.defaultVolume;

            // 綁定事件處理器
            this.audio.addEventListener('play', () => this._handlePlay());
            this.audio.addEventListener('pause', () => this._handlePause());
            this.audio.addEventListener('ended', () => this._handleEnded());
            this.audio.addEventListener('error', (e) => this._handleError(e));
            this.audio.addEventListener('timeupdate', () => this._handleTimeUpdate());
            this.audio.addEventListener('loadedmetadata', () => this._handleLoadedMetadata());
        }

        /**
         * 播放音效
         * @returns {Promise<void>}
         */
        async play() {
            try {
                await this.audio.play();
                this.isPlaying = true;
            } catch (error) {
                console.error('音效播放失敗:', error);
                this._handleError(error);
            }
        }

        /**
         * 暫停音效
         */
        pause() {
            this.audio.pause();
            this.isPlaying = false;
        }

        /**
         * 切換播放/暫停
         * @returns {Promise<void>}
         */
        async toggle() {
            if (this.isPlaying) {
                this.pause();
            } else {
                await this.play();
            }
        }

        /**
         * 停止播放並重置
         */
        stop() {
            this.pause();
            this.audio.currentTime = 0;
        }

        /**
         * 設定音量
         * @param {number} volume - 音量值 (0-1)
         */
        setVolume(volume) {
            const clampedVolume = Math.max(0, Math.min(1, volume));
            this.audio.volume = clampedVolume;
        }

        /**
         * 取得當前音量
         * @returns {number} 音量值 (0-1)
         */
        getVolume() {
            return this.audio.volume;
        }

        /**
         * 靜音切換
         * @returns {boolean} 是否靜音
         */
        toggleMute() {
            this.audio.muted = !this.audio.muted;
            return this.audio.muted;
        }

        /**
         * 取得播放進度百分比
         * @returns {number} 進度百分比 (0-100)
         */
        getProgress() {
            if (this.audio.duration === 0 || isNaN(this.audio.duration)) {
                return 0;
            }
            return (this.audio.currentTime / this.audio.duration) * 100;
        }

        /**
         * 設定播放位置
         * @param {number} percent - 進度百分比 (0-100)
         */
        setProgress(percent) {
            if (!isNaN(this.audio.duration)) {
                this.audio.currentTime = (percent / 100) * this.audio.duration;
            }
        }

        /**
         * 取得格式化的當前時間
         * @returns {string} 格式化時間字串 (mm:ss)
         */
        getCurrentTimeFormatted() {
            return this._formatTime(this.audio.currentTime);
        }

        /**
         * 取得格式化的總時長
         * @returns {string} 格式化時間字串 (mm:ss)
         */
        getDurationFormatted() {
            return this._formatTime(this.audio.duration);
        }

        /**
         * 格式化時間
         * @param {number} seconds - 秒數
         * @returns {string} 格式化時間字串
         * @private
         */
        _formatTime(seconds) {
            if (isNaN(seconds)) return '0:00';
            const mins = Math.floor(seconds / 60);
            const secs = Math.floor(seconds % 60);
            return `${mins}:${secs.toString().padStart(2, '0')}`;
        }

        /**
         * 處理播放事件
         * @private
         */
        _handlePlay() {
            this.isPlaying = true;
            if (typeof this.options.onPlay === 'function') {
                this.options.onPlay(this);
            }
        }

        /**
         * 處理暫停事件
         * @private
         */
        _handlePause() {
            this.isPlaying = false;
            if (typeof this.options.onPause === 'function') {
                this.options.onPause(this);
            }
        }

        /**
         * 處理播放結束事件
         * @private
         */
        _handleEnded() {
            this.isPlaying = false;
            this.audio.currentTime = 0;
            if (typeof this.options.onEnded === 'function') {
                this.options.onEnded(this);
            }
        }

        /**
         * 處理錯誤事件
         * @param {Error} error - 錯誤物件
         * @private
         */
        _handleError(error) {
            this.isPlaying = false;
            if (typeof this.options.onError === 'function') {
                this.options.onError(error, this);
            }
        }

        /**
         * 處理時間更新事件
         * @private
         */
        _handleTimeUpdate() {
            if (typeof this.options.onTimeUpdate === 'function') {
                this.options.onTimeUpdate(this);
            }
        }

        /**
         * 處理 metadata 載入完成事件
         * @private
         */
        _handleLoadedMetadata() {
            if (typeof this.options.onLoadedMetadata === 'function') {
                this.options.onLoadedMetadata(this);
            }
        }

        /**
         * 銷毀播放器
         */
        destroy() {
            this.stop();
            this.audio.removeEventListener('play', this._handlePlay);
            this.audio.removeEventListener('pause', this._handlePause);
            this.audio.removeEventListener('ended', this._handleEnded);
            this.audio.removeEventListener('error', this._handleError);
            this.audio.removeEventListener('timeupdate', this._handleTimeUpdate);
            this.audio.removeEventListener('loadedmetadata', this._handleLoadedMetadata);
        }
    }

    /**
     * 音效播放器管理器
     * 管理頁面上所有的音效播放器實例
     */
    class AudioPlayerManager {
        constructor() {
            this.players = new Map();
        }

        /**
         * 建立並註冊播放器
         * @param {string} id - 播放器 ID
         * @param {HTMLAudioElement} audioElement - 音訊元素
         * @param {Object} options - 設定選項
         * @returns {AudioPlayer} 播放器實例
         */
        create(id, audioElement, options = {}) {
            // 如果已存在，先銷毀
            if (this.players.has(id)) {
                this.destroy(id);
            }

            const player = new AudioPlayer(audioElement, options);
            this.players.set(id, player);
            return player;
        }

        /**
         * 取得播放器實例
         * @param {string} id - 播放器 ID
         * @returns {AudioPlayer|undefined} 播放器實例
         */
        get(id) {
            return this.players.get(id);
        }

        /**
         * 暫停所有播放器
         */
        pauseAll() {
            this.players.forEach(player => player.pause());
        }

        /**
         * 銷毀指定播放器
         * @param {string} id - 播放器 ID
         */
        destroy(id) {
            const player = this.players.get(id);
            if (player) {
                player.destroy();
                this.players.delete(id);
            }
        }

        /**
         * 銷毀所有播放器
         */
        destroyAll() {
            this.players.forEach(player => player.destroy());
            this.players.clear();
        }
    }

    // 建立全域管理器實例
    window.audioPlayerManager = new AudioPlayerManager();

    /**
     * 初始化動物叫聲播放器 UI
     * @param {string} containerId - 容器元素 ID
     * @param {string} audioSrc - 音訊檔案路徑
     * @param {string} animalName - 動物名稱
     */
    window.initAnimalSoundPlayer = function (containerId, audioSrc, animalName) {
        const container = document.getElementById(containerId);
        if (!container || !audioSrc) return;

        // 建立播放器 HTML
        container.innerHTML = `
            <div class="audio-player" role="region" aria-label="${animalName} 叫聲播放器">
                <audio id="${containerId}-audio" src="${audioSrc}" preload="metadata"></audio>
                
                <div class="audio-player-controls">
                    <button type="button" 
                            class="btn btn-success audio-play-btn" 
                            id="${containerId}-play-btn"
                            aria-label="播放 ${animalName} 的叫聲"
                            title="播放/暫停">
                        <i class="bi bi-play-fill"></i>
                    </button>
                    
                    <div class="audio-progress-container">
                        <input type="range" 
                               class="audio-progress" 
                               id="${containerId}-progress"
                               min="0" 
                               max="100" 
                               value="0"
                               aria-label="播放進度"
                               title="播放進度">
                        <div class="audio-time">
                            <span id="${containerId}-current">0:00</span>
                            <span>/</span>
                            <span id="${containerId}-duration">0:00</span>
                        </div>
                    </div>
                    
                    <div class="audio-volume-container">
                        <button type="button" 
                                class="btn btn-link audio-volume-btn" 
                                id="${containerId}-volume-btn"
                                aria-label="靜音切換"
                                title="靜音">
                            <i class="bi bi-volume-up"></i>
                        </button>
                        <input type="range" 
                               class="audio-volume" 
                               id="${containerId}-volume"
                               min="0" 
                               max="100" 
                               value="70"
                               aria-label="音量控制"
                               title="音量">
                    </div>
                </div>
            </div>
        `;

        const audioElement = document.getElementById(`${containerId}-audio`);
        const playBtn = document.getElementById(`${containerId}-play-btn`);
        const progressSlider = document.getElementById(`${containerId}-progress`);
        const currentTimeEl = document.getElementById(`${containerId}-current`);
        const durationEl = document.getElementById(`${containerId}-duration`);
        const volumeBtn = document.getElementById(`${containerId}-volume-btn`);
        const volumeSlider = document.getElementById(`${containerId}-volume`);

        // 建立播放器實例
        const player = window.audioPlayerManager.create(containerId, audioElement, {
            defaultVolume: 0.7,
            onPlay: () => {
                playBtn.innerHTML = '<i class="bi bi-pause-fill"></i>';
                playBtn.setAttribute('aria-label', `暫停 ${animalName} 的叫聲`);
            },
            onPause: () => {
                playBtn.innerHTML = '<i class="bi bi-play-fill"></i>';
                playBtn.setAttribute('aria-label', `播放 ${animalName} 的叫聲`);
            },
            onEnded: () => {
                playBtn.innerHTML = '<i class="bi bi-play-fill"></i>';
                playBtn.setAttribute('aria-label', `播放 ${animalName} 的叫聲`);
                progressSlider.value = 0;
            },
            onTimeUpdate: (p) => {
                progressSlider.value = p.getProgress();
                currentTimeEl.textContent = p.getCurrentTimeFormatted();
            },
            onLoadedMetadata: (p) => {
                durationEl.textContent = p.getDurationFormatted();
            },
            onError: () => {
                playBtn.disabled = true;
                playBtn.innerHTML = '<i class="bi bi-exclamation-triangle"></i>';
                playBtn.setAttribute('title', '音訊載入失敗');
            }
        });

        // 播放/暫停按鈕
        playBtn.addEventListener('click', () => {
            // 暫停其他播放中的音訊
            window.audioPlayerManager.pauseAll();
            player.toggle();
        });

        // 進度條控制
        progressSlider.addEventListener('input', () => {
            player.setProgress(parseFloat(progressSlider.value));
        });

        // 音量按鈕（靜音切換）
        volumeBtn.addEventListener('click', () => {
            const isMuted = player.toggleMute();
            const icon = volumeBtn.querySelector('i');
            if (isMuted) {
                icon.className = 'bi bi-volume-mute';
                volumeSlider.value = 0;
            } else {
                updateVolumeIcon(player.getVolume() * 100);
                volumeSlider.value = player.getVolume() * 100;
            }
        });

        // 音量滑桿
        volumeSlider.addEventListener('input', () => {
            const volume = parseFloat(volumeSlider.value) / 100;
            player.setVolume(volume);
            player.audio.muted = false;
            updateVolumeIcon(volumeSlider.value);
        });

        /**
         * 更新音量圖示
         * @param {number} volume - 音量值 (0-100)
         */
        function updateVolumeIcon(volume) {
            const icon = volumeBtn.querySelector('i');
            if (volume === 0) {
                icon.className = 'bi bi-volume-mute';
            } else if (volume < 50) {
                icon.className = 'bi bi-volume-down';
            } else {
                icon.className = 'bi bi-volume-up';
            }
        }

        return player;
    };

    /**
     * 簡易播放動物叫聲（用於快速播放按鈕）
     * @param {string} soundPath - 音訊檔案路徑
     * @param {HTMLButtonElement} button - 觸發按鈕元素
     */
    window.playAnimalSoundSimple = function (soundPath, button) {
        // 暫停所有其他播放器
        window.audioPlayerManager.pauseAll();

        // 取得或建立音訊元素
        let audio = document.getElementById('simple-animal-sound');
        if (!audio) {
            audio = document.createElement('audio');
            audio.id = 'simple-animal-sound';
            audio.preload = 'none';
            document.body.appendChild(audio);
        }

        const icon = button ? button.querySelector('i') : null;
        const originalIconClass = icon ? icon.className : '';

        // 如果正在播放同一音訊，則暫停
        if (!audio.paused && audio.src.endsWith(soundPath)) {
            audio.pause();
            audio.currentTime = 0;
            if (icon) icon.className = 'bi bi-volume-up';
            return;
        }

        // 設定新音訊來源
        audio.src = soundPath;

        // 更新按鈕狀態
        if (icon) icon.className = 'bi bi-pause-fill';

        // 播放結束時重置按鈕
        audio.onended = () => {
            if (icon) icon.className = originalIconClass || 'bi bi-volume-up';
        };

        audio.onerror = () => {
            if (icon) icon.className = 'bi bi-exclamation-triangle';
            console.error('音訊播放失敗:', soundPath);
        };

        // 播放音訊
        audio.play().catch(error => {
            console.error('音訊播放失敗:', error);
            if (icon) icon.className = originalIconClass || 'bi bi-volume-up';
        });
    };

})();
