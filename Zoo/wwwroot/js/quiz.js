/**
 * 動物園知識測驗互動模組
 * 提供測驗題目顯示、答案提交與結果展示功能
 */
(function () {
    'use strict';

    /**
     * 測驗狀態管理
     */
    const state = {
        quizzes: [],           // 當前測驗題目列表
        currentIndex: 0,       // 當前題目索引
        correctCount: 0,       // 答對題數
        isAnswered: false,     // 當前題目是否已作答
        animalId: null,        // 關聯的動物 ID
        animalName: null,      // 關聯的動物名稱
        quizTitle: '知識測驗'  // 測驗標題
    };

    /**
     * DOM 元素快取
     */
    let elements = null;

    /**
     * 初始化 DOM 元素參照
     */
    function initElements() {
        elements = {
            modal: document.getElementById('quizModal'),
            modalTitle: document.getElementById('quizModalTitle'),
            loading: document.getElementById('quizLoading'),
            empty: document.getElementById('quizEmpty'),
            content: document.getElementById('quizContent'),
            result: document.getElementById('quizResult'),
            progress: document.getElementById('quizProgress'),
            score: document.getElementById('quizScore'),
            question: document.getElementById('quizQuestion'),
            questionEn: document.getElementById('quizQuestionEn'),
            options: document.getElementById('quizOptions'),
            trueFalse: document.getElementById('quizTrueFalse'),
            feedback: document.getElementById('quizFeedback'),
            feedbackIcon: document.getElementById('feedbackIcon'),
            feedbackTitle: document.getElementById('feedbackTitle'),
            feedbackText: document.getElementById('feedbackText'),
            feedbackTextEn: document.getElementById('feedbackTextEn'),
            nextBtn: document.getElementById('quizNextBtn'),
            retryBtn: document.getElementById('quizRetryBtn'),
            resultIcon: document.getElementById('resultIcon'),
            resultTitle: document.getElementById('resultTitle'),
            resultScore: document.getElementById('resultScore'),
            resultProgress: document.getElementById('resultProgress'),
            resultMessage: document.getElementById('resultMessage')
        };
    }

    /**
     * 重置測驗狀態
     */
    function resetState() {
        state.quizzes = [];
        state.currentIndex = 0;
        state.correctCount = 0;
        state.isAnswered = false;
        state.animalId = null;
        state.animalName = null;
        state.quizTitle = '知識測驗';
    }

    /**
     * 顯示載入狀態
     */
    function showLoading() {
        if (elements.loading) elements.loading.style.display = 'block';
        if (elements.empty) elements.empty.style.display = 'none';
        if (elements.content) elements.content.style.display = 'none';
        if (elements.result) elements.result.style.display = 'none';
        if (elements.nextBtn) elements.nextBtn.style.display = 'none';
        if (elements.retryBtn) elements.retryBtn.style.display = 'none';
    }

    /**
     * 顯示無題目狀態
     */
    function showEmpty() {
        if (elements.loading) elements.loading.style.display = 'none';
        if (elements.empty) elements.empty.style.display = 'block';
        if (elements.content) elements.content.style.display = 'none';
        if (elements.result) elements.result.style.display = 'none';
    }

    /**
     * 顯示題目內容
     */
    function showContent() {
        if (elements.loading) elements.loading.style.display = 'none';
        if (elements.empty) elements.empty.style.display = 'none';
        if (elements.content) elements.content.style.display = 'block';
        if (elements.result) elements.result.style.display = 'none';
    }

    /**
     * 顯示測驗結果
     */
    function showResult() {
        if (elements.loading) elements.loading.style.display = 'none';
        if (elements.empty) elements.empty.style.display = 'none';
        if (elements.content) elements.content.style.display = 'none';
        if (elements.result) elements.result.style.display = 'block';
        if (elements.nextBtn) elements.nextBtn.style.display = 'none';
        if (elements.retryBtn) elements.retryBtn.style.display = 'inline-block';
    }

    /**
     * 從 API 載入動物測驗題目
     * @param {string} animalId - 動物 ID
     * @returns {Promise<Array>} 測驗題目列表
     */
    async function fetchAnimalQuizzes(animalId) {
        const response = await fetch(`/api/quizzes/animal/${encodeURIComponent(animalId)}`);
        if (!response.ok) {
            throw new Error(`API 錯誤: ${response.status}`);
        }
        const data = await response.json();
        return data.quizzes || [];
    }

    /**
     * 從 API 載入隨機測驗題目
     * @param {number} count - 題目數量
     * @returns {Promise<Array>} 測驗題目列表
     */
    async function fetchRandomQuizzes(count) {
        const response = await fetch(`/api/quizzes/random?count=${count}`);
        if (!response.ok) {
            // 如果沒有隨機 API，回退到取得所有題目再隨機選取
            const allResponse = await fetch('/api/quizzes');
            if (!allResponse.ok) {
                throw new Error(`API 錯誤: ${allResponse.status}`);
            }
            const data = await allResponse.json();
            const all = data.quizzes || [];
            return shuffleArray(all).slice(0, count);
        }
        const data = await response.json();
        return data.quizzes || [];
    }

    /**
     * 提交測驗答案
     * @param {string} quizId - 測驗 ID
     * @param {*} answer - 使用者答案
     * @returns {Promise<Object>} 驗證結果
     */
    async function submitAnswer(quizId, answer) {
        const response = await fetch(`/api/quizzes/${encodeURIComponent(quizId)}/answer`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ answer })
        });

        if (!response.ok) {
            throw new Error(`API 錯誤: ${response.status}`);
        }

        return await response.json();
    }

    /**
     * 陣列隨機排序
     * @param {Array} array - 要排序的陣列
     * @returns {Array} 排序後的新陣列
     */
    function shuffleArray(array) {
        const shuffled = [...array];
        for (let i = shuffled.length - 1; i > 0; i--) {
            const j = Math.floor(Math.random() * (i + 1));
            [shuffled[i], shuffled[j]] = [shuffled[j], shuffled[i]];
        }
        return shuffled;
    }

    /**
     * 渲染當前題目
     */
    function renderQuestion() {
        const quiz = state.quizzes[state.currentIndex];
        if (!quiz) return;

        // 更新進度與分數
        elements.progress.textContent = `第 ${state.currentIndex + 1} 題 / 共 ${state.quizzes.length} 題`;
        elements.score.textContent = `答對: ${state.correctCount} 題`;

        // 更新題目
        elements.question.textContent = quiz.questionZh;
        elements.questionEn.textContent = quiz.questionEn;

        // 隱藏回饋區
        elements.feedback.style.display = 'none';
        elements.nextBtn.style.display = 'none';

        // 根據題目類型渲染選項
        if (quiz.type === 'MultipleChoice') {
            renderMultipleChoice(quiz);
        } else if (quiz.type === 'TrueFalse') {
            renderTrueFalse();
        }

        state.isAnswered = false;
    }

    /**
     * 渲染選擇題選項
     * @param {Object} quiz - 測驗題目
     */
    function renderMultipleChoice(quiz) {
        elements.options.style.display = 'block';
        elements.trueFalse.style.display = 'none';

        elements.options.innerHTML = quiz.options.map((option, index) => `
            <button type="button" 
                    class="list-group-item list-group-item-action d-flex align-items-center"
                    data-answer="${index}">
                <span class="badge bg-secondary me-3">${String.fromCharCode(65 + index)}</span>
                <div>
                    <div>${option.textZh}</div>
                    <small class="text-muted">${option.textEn}</small>
                </div>
            </button>
        `).join('');

        // 綁定選項點擊事件
        elements.options.querySelectorAll('.list-group-item').forEach(btn => {
            btn.addEventListener('click', handleOptionClick);
        });
    }

    /**
     * 渲染是非題選項
     */
    function renderTrueFalse() {
        elements.options.style.display = 'none';
        elements.trueFalse.style.display = 'flex';

        // 重置按鈕狀態
        elements.trueFalse.querySelectorAll('.btn').forEach(btn => {
            btn.classList.remove('selected', 'correct', 'incorrect', 'disabled');
            btn.disabled = false;
        });

        // 綁定是非題點擊事件
        elements.trueFalse.querySelectorAll('.btn').forEach(btn => {
            btn.removeEventListener('click', handleTrueFalseClick);
            btn.addEventListener('click', handleTrueFalseClick);
        });
    }

    /**
     * 處理選擇題選項點擊
     * @param {Event} event - 點擊事件
     */
    async function handleOptionClick(event) {
        if (state.isAnswered) return;

        const btn = event.currentTarget;
        const answer = parseInt(btn.dataset.answer, 10);

        await handleAnswer(answer, btn, 'option');
    }

    /**
     * 處理是非題點擊
     * @param {Event} event - 點擊事件
     */
    async function handleTrueFalseClick(event) {
        if (state.isAnswered) return;

        const btn = event.currentTarget;
        const answer = btn.dataset.answer === 'true';

        await handleAnswer(answer, btn, 'trueFalse');
    }

    /**
     * 處理答案提交
     * @param {*} answer - 使用者答案
     * @param {HTMLElement} selectedBtn - 選中的按鈕
     * @param {string} type - 題目類型 ('option' 或 'trueFalse')
     */
    async function handleAnswer(answer, selectedBtn, type) {
        state.isAnswered = true;

        const quiz = state.quizzes[state.currentIndex];

        // 禁用所有選項
        if (type === 'option') {
            elements.options.querySelectorAll('.list-group-item').forEach(btn => {
                btn.classList.add('disabled');
            });
        } else {
            elements.trueFalse.querySelectorAll('.btn').forEach(btn => {
                btn.disabled = true;
            });
        }

        // 標記選中
        selectedBtn.classList.add('selected');

        try {
            const result = await submitAnswer(quiz.id, answer);

            // 更新答對數
            if (result.correct) {
                state.correctCount++;
                elements.score.textContent = `答對: ${state.correctCount} 題`;
            }

            // 顯示正確/錯誤狀態
            if (type === 'option') {
                showOptionFeedback(result, answer);
            } else {
                showTrueFalseFeedback(result, answer);
            }

            // 顯示回饋文字
            showFeedback(result);

            // 顯示下一題按鈕或結果
            if (state.currentIndex < state.quizzes.length - 1) {
                elements.nextBtn.style.display = 'inline-block';
            } else {
                // 最後一題，延遲顯示結果
                setTimeout(showFinalResult, 1500);
            }
        } catch (error) {
            console.error('提交答案失敗:', error);
            alert('提交答案時發生錯誤，請稍後再試');
            state.isAnswered = false;
        }
    }

    /**
     * 顯示選擇題回饋
     * @param {Object} result - 驗證結果
     * @param {number} userAnswer - 使用者答案
     */
    function showOptionFeedback(result, userAnswer) {
        const correctAnswer = result.correctAnswer;

        elements.options.querySelectorAll('.list-group-item').forEach((btn, index) => {
            if (index === correctAnswer) {
                btn.classList.add('correct');
            }
            if (index === userAnswer && !result.correct) {
                btn.classList.add('incorrect');
            }
        });
    }

    /**
     * 顯示是非題回饋
     * @param {Object} result - 驗證結果
     * @param {boolean} userAnswer - 使用者答案
     */
    function showTrueFalseFeedback(result, userAnswer) {
        const correctAnswer = result.correctAnswer;

        elements.trueFalse.querySelectorAll('.btn').forEach(btn => {
            const btnAnswer = btn.dataset.answer === 'true';
            if (btnAnswer === correctAnswer) {
                btn.classList.add('correct');
            }
            if (btnAnswer === userAnswer && !result.correct) {
                btn.classList.add('incorrect');
            }
        });
    }

    /**
     * 顯示答題回饋
     * @param {Object} result - 驗證結果
     */
    function showFeedback(result) {
        elements.feedback.style.display = 'block';
        elements.feedback.className = 'alert ' + (result.correct ? 'alert-success' : 'alert-danger');
        elements.feedbackIcon.className = 'bi fs-3 me-3 ' + (result.correct ? 'bi-check-circle-fill' : 'bi-x-circle-fill');
        elements.feedbackTitle.textContent = result.correct ? '答對了！🎉' : '答錯了 😅';
        elements.feedbackText.textContent = result.feedbackZh;
        elements.feedbackTextEn.textContent = result.feedbackEn;
    }

    /**
     * 顯示最終結果
     */
    function showFinalResult() {
        showResult();

        const total = state.quizzes.length;
        const correct = state.correctCount;
        const percentage = Math.round((correct / total) * 100);

        // 更新結果內容
        elements.resultScore.textContent = `你答對了 ${correct} / ${total} 題`;
        elements.resultProgress.style.width = `${percentage}%`;
        elements.resultProgress.textContent = `${percentage}%`;

        // 根據分數顯示不同的圖示和訊息
        if (percentage === 100) {
            elements.resultIcon.innerHTML = '<i class="bi bi-trophy-fill fs-1 text-warning"></i>';
            elements.resultTitle.textContent = '太厲害了！🏆';
            elements.resultMessage.textContent = '你是動物知識王！所有題目都答對了！';
            elements.resultProgress.className = 'progress-bar bg-warning';
        } else if (percentage >= 80) {
            elements.resultIcon.innerHTML = '<i class="bi bi-star-fill fs-1 text-success"></i>';
            elements.resultTitle.textContent = '表現優異！⭐';
            elements.resultMessage.textContent = '你對動物有很深的了解，繼續保持！';
            elements.resultProgress.className = 'progress-bar bg-success';
        } else if (percentage >= 60) {
            elements.resultIcon.innerHTML = '<i class="bi bi-hand-thumbs-up-fill fs-1 text-info"></i>';
            elements.resultTitle.textContent = '做得不錯！👍';
            elements.resultMessage.textContent = '再多了解一些動物知識會更棒！';
            elements.resultProgress.className = 'progress-bar bg-info';
        } else if (percentage >= 40) {
            elements.resultIcon.innerHTML = '<i class="bi bi-emoji-smile fs-1 text-primary"></i>';
            elements.resultTitle.textContent = '繼續加油！😊';
            elements.resultMessage.textContent = '多觀察動物，你會學到更多有趣的知識！';
            elements.resultProgress.className = 'progress-bar bg-primary';
        } else {
            elements.resultIcon.innerHTML = '<i class="bi bi-book fs-1 text-secondary"></i>';
            elements.resultTitle.textContent = '來學習吧！📚';
            elements.resultMessage.textContent = '瀏覽動物詳情頁面，學習更多有趣的動物知識！';
            elements.resultProgress.className = 'progress-bar bg-secondary';
        }
    }

    /**
     * 前往下一題
     */
    function nextQuestion() {
        if (state.currentIndex < state.quizzes.length - 1) {
            state.currentIndex++;
            renderQuestion();
        }
    }

    /**
     * 重新開始測驗
     */
    function retryQuiz() {
        state.currentIndex = 0;
        state.correctCount = 0;
        state.isAnswered = false;

        // 重新排序題目
        state.quizzes = shuffleArray(state.quizzes);

        showContent();
        renderQuestion();
    }

    /**
     * 開始動物測驗
     * @param {string} animalId - 動物 ID
     * @param {string} animalName - 動物名稱
     */
    async function startAnimalQuiz(animalId, animalName) {
        if (!elements) {
            initElements();
        }

        resetState();
        state.animalId = animalId;
        state.animalName = animalName;
        state.quizTitle = `${animalName} 小測驗`;

        // 更新彈窗標題
        elements.modalTitle.textContent = state.quizTitle;

        // 顯示彈窗
        const modal = new bootstrap.Modal(elements.modal);
        modal.show();

        showLoading();

        try {
            const quizzes = await fetchAnimalQuizzes(animalId);

            if (quizzes.length === 0) {
                showEmpty();
                return;
            }

            state.quizzes = shuffleArray(quizzes);
            showContent();
            renderQuestion();
        } catch (error) {
            console.error('載入測驗題目失敗:', error);
            showEmpty();
        }
    }

    /**
     * 開始隨機測驗
     * @param {number} count - 題目數量
     * @param {string} title - 測驗標題
     */
    async function startRandomQuiz(count, title = '知識測驗') {
        if (!elements) {
            initElements();
        }

        resetState();
        state.quizTitle = title;

        // 更新彈窗標題
        elements.modalTitle.textContent = state.quizTitle;

        // 顯示彈窗
        const modal = new bootstrap.Modal(elements.modal);
        modal.show();

        showLoading();

        try {
            const quizzes = await fetchRandomQuizzes(count);

            if (quizzes.length === 0) {
                showEmpty();
                return;
            }

            state.quizzes = shuffleArray(quizzes);
            showContent();
            renderQuestion();
        } catch (error) {
            console.error('載入測驗題目失敗:', error);
            showEmpty();
        }
    }

    /**
     * 初始化模組
     */
    function init() {
        initElements();

        // 綁定下一題按鈕
        if (elements.nextBtn) {
            elements.nextBtn.addEventListener('click', nextQuestion);
        }

        // 綁定重試按鈕
        if (elements.retryBtn) {
            elements.retryBtn.addEventListener('click', retryQuiz);
        }

        // 綁定動物詳情頁的測驗按鈕
        const startQuizBtn = document.getElementById('startQuizBtn');
        if (startQuizBtn) {
            startQuizBtn.addEventListener('click', function () {
                const animalId = this.dataset.animalId;
                const animalName = this.dataset.animalName;
                startAnimalQuiz(animalId, animalName);
            });
        }
    }

    // DOM 載入完成後初始化
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    // 匯出公開 API
    window.ZooQuiz = {
        startAnimalQuiz,
        startRandomQuiz
    };
})();
