const StreamDeck = {
    state: {
        isConnected: false,
        isStreaming: false,
        isRecording: false,
        currentScene: null,
        scenes: [],
        audioSources: [],
        sceneItems: [],
        stats: null,
        replayBuffer: null,
        customConfig: null,
        currentPageId: 'default',
        editMode: false,
        expandedSections: {
            controls: true,
            scenes: true,
            sources: false,
            audio: false,
            custom: true
        }
    },

    pollInterval: null,
    POLL_RATE: 1500,

    icons: {
        link: '<svg viewBox="0 0 24 24"><path d="M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.71"/><path d="M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.71-1.71"/></svg>',
        linkOff: '<svg viewBox="0 0 24 24"><path d="M9 17H7A5 5 0 0 1 7 7h2"/><path d="M15 7h2a5 5 0 0 1 0 10h-2"/><line x1="8" y1="12" x2="16" y2="12"/></svg>',
        broadcast: '<svg viewBox="0 0 24 24"><circle cx="12" cy="12" r="2"/><path d="M16.24 7.76a6 6 0 0 1 0 8.49m-8.48-.01a6 6 0 0 1 0-8.49m11.31-2.82a10 10 0 0 1 0 14.14m-14.14 0a10 10 0 0 1 0-14.14"/></svg>',
        record: '<svg viewBox="0 0 24 24"><circle cx="12" cy="12" r="10" fill="currentColor"/></svg>',
        tv: '<svg viewBox="0 0 24 24"><rect x="2" y="7" width="20" height="15" rx="2" ry="2"/><polyline points="17 2 12 7 7 2"/></svg>',
        mic: '<svg viewBox="0 0 24 24"><path d="M12 1a3 3 0 0 0-3 3v8a3 3 0 0 0 6 0V4a3 3 0 0 0-3-3z"/><path d="M19 10v2a7 7 0 0 1-14 0v-2"/><line x1="12" y1="19" x2="12" y2="23"/><line x1="8" y1="23" x2="16" y2="23"/></svg>',
        micOff: '<svg viewBox="0 0 24 24"><line x1="1" y1="1" x2="23" y2="23"/><path d="M9 9v3a3 3 0 0 0 5.12 2.12M15 9.34V4a3 3 0 0 0-5.94-.6"/><path d="M17 16.95A7 7 0 0 1 5 12v-2m14 0v2a7 7 0 0 1-.11 1.23"/><line x1="12" y1="19" x2="12" y2="23"/><line x1="8" y1="23" x2="16" y2="23"/></svg>',
        volume: '<svg viewBox="0 0 24 24"><polygon points="11 5 6 9 2 9 2 15 6 15 11 19 11 5"/><path d="M19.07 4.93a10 10 0 0 1 0 14.14M15.54 8.46a5 5 0 0 1 0 7.07"/></svg>',
        volumeOff: '<svg viewBox="0 0 24 24"><polygon points="11 5 6 9 2 9 2 15 6 15 11 19 11 5"/><line x1="23" y1="9" x2="17" y2="15"/><line x1="17" y1="9" x2="23" y2="15"/></svg>',
        eye: '<svg viewBox="0 0 24 24"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>',
        eyeOff: '<svg viewBox="0 0 24 24"><path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"/><line x1="1" y1="1" x2="23" y2="23"/></svg>',
        save: '<svg viewBox="0 0 24 24"><path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z"/><polyline points="17 21 17 13 7 13 7 21"/><polyline points="7 3 7 8 15 8"/></svg>',
        replay: '<svg viewBox="0 0 24 24"><polygon points="5 3 19 12 5 21 5 3"/><line x1="19" y1="3" x2="19" y2="21"/></svg>',
        zap: '<svg viewBox="0 0 24 24"><polygon points="13 2 3 14 12 14 11 22 21 10 12 10 13 2"/></svg>',
        settings: '<svg viewBox="0 0 24 24"><circle cx="12" cy="12" r="3"/><path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1 0 2.83 2 2 0 0 1-2.83 0l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-2 2 2 2 0 0 1-2-2v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83 0 2 2 0 0 1 0-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1-2-2 2 2 0 0 1 2-2h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 0-2.83 2 2 0 0 1 2.83 0l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 2-2 2 2 0 0 1 2 2v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 0 2 2 0 0 1 0 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 2 2 2 2 0 0 1-2 2h-.09a1.65 1.65 0 0 0-1.51 1z"/></svg>',
        plus: '<svg viewBox="0 0 24 24"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>',
        edit: '<svg viewBox="0 0 24 24"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>',
        trash: '<svg viewBox="0 0 24 24"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/></svg>',
        chevronDown: '<svg viewBox="0 0 24 24"><polyline points="6 9 12 15 18 9"/></svg>',
        chevronRight: '<svg viewBox="0 0 24 24"><polyline points="9 18 15 12 9 6"/></svg>',
        activity: '<svg viewBox="0 0 24 24"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>',
        cpu: '<svg viewBox="0 0 24 24"><rect x="4" y="4" width="16" height="16" rx="2" ry="2"/><rect x="9" y="9" width="6" height="6"/><line x1="9" y1="1" x2="9" y2="4"/><line x1="15" y1="1" x2="15" y2="4"/><line x1="9" y1="20" x2="9" y2="23"/><line x1="15" y1="20" x2="15" y2="23"/><line x1="20" y1="9" x2="23" y2="9"/><line x1="20" y1="14" x2="23" y2="14"/><line x1="1" y1="9" x2="4" y2="9"/><line x1="1" y1="14" x2="4" y2="14"/></svg>',
        loader: '<svg viewBox="0 0 24 24"><circle cx="12" cy="12" r="10" stroke-width="2" stroke-dasharray="32" stroke-linecap="round"/></svg>'
    },

    async init() {
        await this.loadCustomConfig();
        this.renderControlButtons();
        this.renderPageTabs();
        await this.fetchStatus();
        this.startPolling();
        this.initKeyboardShortcuts();
    },

    async loadCustomConfig() {
        try {
            const response = await fetch('/api/buttons');
            this.state.customConfig = await response.json();
            if (this.state.customConfig.pages.length > 0) {
                this.state.currentPageId = this.state.customConfig.pages[0].id;
            }
        } catch (error) {
            console.error('Failed to load custom config:', error);
            this.state.customConfig = { pages: [{ id: 'default', name: 'Main', buttons: [] }] };
        }
    },

    renderPageTabs() {
        const container = document.getElementById('pageTabs');
        if (!container || !this.state.customConfig) return;

        const pages = this.state.customConfig.pages;
        container.innerHTML = `
            <div class="page-tabs-inner">
                ${pages.map(page => `
                    <button class="page-tab ${page.id === this.state.currentPageId ? 'active' : ''}"
                            data-page-id="${page.id}">
                        ${this.escapeHtml(page.name)}
                    </button>
                `).join('')}
                <button class="page-tab add-page-btn" title="Add Page">
                    ${this.icons.plus}
                </button>
            </div>
            <button class="edit-mode-btn ${this.state.editMode ? 'active' : ''}" title="Edit Mode">
                ${this.icons.edit}
            </button>
        `;

        container.querySelectorAll('.page-tab[data-page-id]').forEach(tab => {
            tab.addEventListener('click', () => {
                this.state.currentPageId = tab.dataset.pageId;
                this.renderPageTabs();
                this.renderCustomButtons();
                this.hapticFeedback();
            });
        });

        container.querySelector('.add-page-btn')?.addEventListener('click', () => this.addPage());
        container.querySelector('.edit-mode-btn')?.addEventListener('click', () => this.toggleEditMode());
    },

    renderControlButtons() {
        const container = document.getElementById('controlButtons');
        if (!container) return;

        container.innerHTML = `
            <button class="deck-button connect-btn" data-action="connect">
                ${this.icons.link}
                <span class="deck-button-label">Connect</span>
            </button>
            <button class="deck-button" data-action="stream" disabled>
                ${this.icons.broadcast}
                <span class="deck-button-label">Stream</span>
            </button>
            <button class="deck-button" data-action="record" disabled>
                ${this.icons.record}
                <span class="deck-button-label">Record</span>
            </button>
            <button class="deck-button replay-btn" data-action="replay-save" disabled title="Save Replay">
                ${this.icons.save}
                <span class="deck-button-label">Clip</span>
            </button>
            <button class="deck-button" data-action="replay-toggle" disabled title="Toggle Replay Buffer">
                ${this.icons.replay}
                <span class="deck-button-label">Buffer</span>
            </button>
        `;

        container.querySelectorAll('.deck-button').forEach(btn => {
            btn.addEventListener('click', () => this.handleButtonClick(btn));
        });
    },

    renderSceneButtons() {
        const container = document.getElementById('sceneButtons');
        if (!container) return;

        if (this.state.scenes.length === 0) {
            container.innerHTML = '<p class="empty-message">Connect to OBS to see scenes</p>';
            return;
        }

        container.innerHTML = this.state.scenes.map(scene => `
            <button class="deck-button ${scene.isActive ? 'active-scene' : ''}"
                    data-action="scene"
                    data-scene="${this.escapeHtml(scene.name)}">
                ${this.icons.tv}
                <span class="deck-button-label">${this.escapeHtml(scene.name)}</span>
            </button>
        `).join('');

        container.querySelectorAll('.deck-button').forEach(btn => {
            btn.addEventListener('click', () => this.handleButtonClick(btn));
        });
    },

    renderSourceButtons() {
        const container = document.getElementById('sourceButtons');
        const section = document.getElementById('sourceSection');
        if (!container || !section) return;

        if (this.state.sceneItems.length === 0) {
            section.style.display = 'none';
            return;
        }

        section.style.display = 'block';
        container.innerHTML = this.state.sceneItems.map(item => `
            <button class="deck-button ${!item.isVisible ? 'source-hidden' : ''}"
                    data-action="source"
                    data-scene="${this.escapeHtml(item.sceneName)}"
                    data-item-id="${item.id}">
                ${item.isVisible ? this.icons.eye : this.icons.eyeOff}
                <span class="deck-button-label">${this.escapeHtml(item.name)}</span>
            </button>
        `).join('');

        container.querySelectorAll('.deck-button').forEach(btn => {
            btn.addEventListener('click', () => this.handleButtonClick(btn));
        });
    },

    renderAudioButtons() {
        const container = document.getElementById('audioButtons');
        const section = document.getElementById('audioSection');
        if (!container || !section) return;

        if (this.state.audioSources.length === 0) {
            section.style.display = 'none';
            return;
        }

        section.style.display = 'block';
        container.innerHTML = this.state.audioSources.map(source => `
            <button class="deck-button ${source.isMuted ? 'muted' : ''}"
                    data-action="audio"
                    data-source="${this.escapeHtml(source.name)}">
                ${source.isMuted ? this.icons.volumeOff : this.icons.volume}
                <span class="deck-button-label">${this.escapeHtml(source.name)}</span>
            </button>
        `).join('');

        container.querySelectorAll('.deck-button').forEach(btn => {
            btn.addEventListener('click', () => this.handleButtonClick(btn));
        });
    },

    renderCustomButtons() {
        const container = document.getElementById('customButtons');
        if (!container || !this.state.customConfig) return;

        const page = this.state.customConfig.pages.find(p => p.id === this.state.currentPageId);
        if (!page || page.buttons.length === 0) {
            if (this.state.editMode) {
                container.innerHTML = `
                    <button class="deck-button add-button-btn" data-action="add-button">
                        ${this.icons.plus}
                        <span class="deck-button-label">Add Button</span>
                    </button>
                `;
            } else {
                container.innerHTML = '<p class="empty-message">No custom buttons. Enable edit mode to add some.</p>';
            }
            this.bindCustomButtonEvents(container);
            return;
        }

        container.innerHTML = page.buttons.map(btn => `
            <button class="deck-button custom-button"
                    data-action="custom"
                    data-button-id="${btn.id}"
                    data-page-id="${page.id}"
                    style="background: ${btn.color}; color: ${btn.textColor}">
                ${this.getIcon(btn.icon)}
                <span class="deck-button-label">${this.escapeHtml(btn.label)}</span>
                ${this.state.editMode ? `<span class="edit-overlay">${this.icons.edit}</span>` : ''}
            </button>
        `).join('') + (this.state.editMode ? `
            <button class="deck-button add-button-btn" data-action="add-button">
                ${this.icons.plus}
                <span class="deck-button-label">Add Button</span>
            </button>
        ` : '');

        this.bindCustomButtonEvents(container);
    },

    bindCustomButtonEvents(container) {
        container.querySelectorAll('.deck-button').forEach(btn => {
            btn.addEventListener('click', () => this.handleButtonClick(btn));
        });
    },

    renderStatsPanel() {
        const container = document.getElementById('statsPanel');
        if (!container) return;

        if (!this.state.isConnected || !this.state.stats) {
            container.style.display = 'none';
            return;
        }

        container.style.display = 'block';
        const stats = this.state.stats;
        const droppedFramePercent = stats.outputTotalFrames > 0
            ? ((stats.outputSkippedFrames / stats.outputTotalFrames) * 100).toFixed(1)
            : 0;

        container.innerHTML = `
            <div class="stats-grid">
                <div class="stat-item">
                    <span class="stat-label">FPS</span>
                    <span class="stat-value">${stats.activeFps.toFixed(1)}</span>
                </div>
                <div class="stat-item">
                    <span class="stat-label">CPU</span>
                    <span class="stat-value ${stats.cpuUsage > 80 ? 'stat-warning' : ''}">${stats.cpuUsage.toFixed(1)}%</span>
                </div>
                <div class="stat-item">
                    <span class="stat-label">Dropped</span>
                    <span class="stat-value ${droppedFramePercent > 1 ? 'stat-warning' : ''}">${droppedFramePercent}%</span>
                </div>
                ${stats.streamKbitsPerSec > 0 ? `
                    <div class="stat-item">
                        <span class="stat-label">Bitrate</span>
                        <span class="stat-value">${(stats.streamKbitsPerSec / 1000).toFixed(1)} Mbps</span>
                    </div>
                ` : ''}
                ${stats.streamTimecode ? `
                    <div class="stat-item">
                        <span class="stat-label">Stream</span>
                        <span class="stat-value">${stats.streamTimecode.split('.')[0]}</span>
                    </div>
                ` : ''}
                ${stats.recordTimecode ? `
                    <div class="stat-item">
                        <span class="stat-label">Record</span>
                        <span class="stat-value">${stats.recordTimecode.split('.')[0]}</span>
                    </div>
                ` : ''}
            </div>
        `;
    },

    updateUI() {
        const statusDot = document.getElementById('statusDot');
        const statusText = document.getElementById('statusText');
        const connectBtn = document.querySelector('[data-action="connect"]');
        const streamBtn = document.querySelector('[data-action="stream"]');
        const recordBtn = document.querySelector('[data-action="record"]');
        const replaySaveBtn = document.querySelector('[data-action="replay-save"]');
        const replayToggleBtn = document.querySelector('[data-action="replay-toggle"]');

        if (this.state.isConnected) {
            statusDot?.classList.add('connected');
            if (statusText) statusText.textContent = 'Connected';
            connectBtn?.classList.add('connected');
            if (connectBtn) connectBtn.innerHTML = `${this.icons.linkOff}<span class="deck-button-label">Disconnect</span>`;
            if (streamBtn) streamBtn.disabled = false;
            if (recordBtn) recordBtn.disabled = false;
            if (replaySaveBtn) replaySaveBtn.disabled = false;
            if (replayToggleBtn) replayToggleBtn.disabled = false;
        } else {
            statusDot?.classList.remove('connected');
            if (statusText) statusText.textContent = 'Disconnected';
            connectBtn?.classList.remove('connected');
            if (connectBtn) connectBtn.innerHTML = `${this.icons.link}<span class="deck-button-label">Connect</span>`;
            if (streamBtn) streamBtn.disabled = true;
            if (recordBtn) recordBtn.disabled = true;
            if (replaySaveBtn) replaySaveBtn.disabled = true;
            if (replayToggleBtn) replayToggleBtn.disabled = true;
        }

        if (this.state.isStreaming) {
            streamBtn?.classList.add('streaming');
            if (streamBtn) streamBtn.querySelector('.deck-button-label').textContent = 'Live';
        } else {
            streamBtn?.classList.remove('streaming');
            if (streamBtn) streamBtn.querySelector('.deck-button-label').textContent = 'Stream';
        }

        if (this.state.isRecording) {
            recordBtn?.classList.add('recording');
            if (recordBtn) recordBtn.querySelector('.deck-button-label').textContent = 'Recording';
        } else {
            recordBtn?.classList.remove('recording');
            if (recordBtn) recordBtn.querySelector('.deck-button-label').textContent = 'Record';
        }

        if (this.state.replayBuffer?.isActive) {
            replayToggleBtn?.classList.add('replay-active');
            if (replayToggleBtn) replayToggleBtn.querySelector('.deck-button-label').textContent = 'Buffer On';
        } else {
            replayToggleBtn?.classList.remove('replay-active');
            if (replayToggleBtn) replayToggleBtn.querySelector('.deck-button-label').textContent = 'Buffer';
        }

        this.renderSceneButtons();
        this.renderSourceButtons();
        this.renderAudioButtons();
        this.renderCustomButtons();
        this.renderStatsPanel();
    },

    async fetchStatus() {
        try {
            const response = await fetch('/api/obs/status');
            const data = await response.json();

            this.state.isConnected = data.isConnected;
            this.state.currentScene = data.currentScene;
            this.state.isStreaming = data.isStreaming;
            this.state.isRecording = data.isRecording;
            this.state.scenes = data.scenes || [];
            this.state.audioSources = data.audioSources || [];
            this.state.sceneItems = data.sceneItems || [];
            this.state.stats = data.stats;
            this.state.replayBuffer = data.replayBuffer;

            this.updateUI();
        } catch (error) {
            console.error('Failed to fetch status:', error);
        }
    },

    async handleButtonClick(button) {
        const action = button.dataset.action;

        if (this.state.editMode && action === 'custom') {
            this.openButtonEditor(button.dataset.pageId, button.dataset.buttonId);
            return;
        }

        this.hapticFeedback();
        button.classList.add('loading');

        try {
            switch (action) {
                case 'connect':
                    if (this.state.isConnected) {
                        await this.apiPost('/api/obs/disconnect');
                    } else {
                        const result = await this.apiPost('/api/obs/connect');
                        if (!result.success) {
                            this.showToast('Failed to connect to OBS', 'error');
                        }
                    }
                    break;

                case 'stream':
                    await this.apiPost('/api/obs/stream/toggle');
                    break;

                case 'record':
                    await this.apiPost('/api/obs/record/toggle');
                    break;

                case 'replay-save':
                    await this.apiPost('/api/obs/replay/save');
                    this.showToast('Replay saved!', 'success');
                    break;

                case 'replay-toggle':
                    await this.apiPost('/api/obs/replay/toggle');
                    break;

                case 'scene':
                    const sceneName = button.dataset.scene;
                    await this.apiPost(`/api/obs/scene/${encodeURIComponent(sceneName)}`);
                    break;

                case 'source':
                    const sourceScene = button.dataset.scene;
                    const itemId = button.dataset.itemId;
                    await this.apiPost(`/api/obs/scene/${encodeURIComponent(sourceScene)}/item/${itemId}/toggle`);
                    break;

                case 'audio':
                    const sourceName = button.dataset.source;
                    await this.apiPost(`/api/obs/audio/${encodeURIComponent(sourceName)}/toggle`);
                    break;

                case 'custom':
                    const pageId = button.dataset.pageId;
                    const buttonId = button.dataset.buttonId;
                    await this.apiPost(`/api/buttons/pages/${pageId}/buttons/${buttonId}/execute`);
                    break;

                case 'add-button':
                    this.openButtonEditor(this.state.currentPageId, null);
                    break;
            }

            await this.fetchStatus();
        } catch (error) {
            console.error('Action failed:', error);
            this.showToast('Action failed', 'error');
        } finally {
            button.classList.remove('loading');
        }
    },

    async apiPost(url, data = null) {
        const options = { method: 'POST' };
        if (data) {
            options.headers = { 'Content-Type': 'application/json' };
            options.body = JSON.stringify(data);
        }
        const response = await fetch(url, options);
        return await response.json();
    },

    startPolling() {
        this.pollInterval = setInterval(() => this.fetchStatus(), this.POLL_RATE);
    },

    stopPolling() {
        if (this.pollInterval) {
            clearInterval(this.pollInterval);
            this.pollInterval = null;
        }
    },

    hapticFeedback() {
        if ('vibrate' in navigator) {
            navigator.vibrate(10);
        }
    },

    showToast(message, type = 'info') {
        const container = document.getElementById('toastContainer');
        if (!container) return;

        const toast = document.createElement('div');
        toast.className = `toast ${type}`;
        toast.textContent = message;
        container.appendChild(toast);

        setTimeout(() => toast.remove(), 3000);
    },

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    },

    getIcon(iconName) {
        return this.icons[iconName] || this.icons.zap;
    },

    toggleEditMode() {
        this.state.editMode = !this.state.editMode;
        this.renderPageTabs();
        this.renderCustomButtons();
        this.hapticFeedback();
    },

    async addPage() {
        const name = prompt('Enter page name:');
        if (!name) return;

        try {
            const response = await fetch('/api/buttons/pages', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ name })
            });
            const page = await response.json();
            await this.loadCustomConfig();
            this.state.currentPageId = page.id;
            this.renderPageTabs();
            this.renderCustomButtons();
        } catch (error) {
            this.showToast('Failed to create page', 'error');
        }
    },

    openButtonEditor(pageId, buttonId) {
        const modal = document.getElementById('buttonEditorModal');
        if (!modal) return;

        let button = null;
        if (buttonId) {
            const page = this.state.customConfig.pages.find(p => p.id === pageId);
            button = page?.buttons.find(b => b.id === buttonId);
        }

        document.getElementById('editorPageId').value = pageId;
        document.getElementById('editorButtonId').value = buttonId || '';
        document.getElementById('buttonLabel').value = button?.label || '';
        document.getElementById('buttonIcon').value = button?.icon || 'zap';
        document.getElementById('buttonColor').value = button?.color || '#21262d';
        document.getElementById('buttonTextColor').value = button?.textColor || '#f0f6fc';

        this.currentEditingActions = button?.actions ? [...button.actions] : [];
        this.renderActionsList();

        modal.style.display = 'flex';
    },

    closeButtonEditor() {
        const modal = document.getElementById('buttonEditorModal');
        if (modal) modal.style.display = 'none';
    },

    renderActionsList() {
        const container = document.getElementById('actionsList');
        if (!container) return;

        if (!this.currentEditingActions || this.currentEditingActions.length === 0) {
            container.innerHTML = '<p class="empty-message">No actions. Add actions below.</p>';
            return;
        }

        const actionTypes = {
            0: 'Switch Scene', 1: 'Toggle Source', 2: 'Toggle Mute', 3: 'Set Volume',
            4: 'Start Stream', 5: 'Stop Stream', 6: 'Toggle Stream',
            7: 'Start Record', 8: 'Stop Record', 9: 'Toggle Record',
            10: 'Save Replay', 11: 'Toggle Filter', 12: 'Trigger Hotkey', 13: 'Delay'
        };

        container.innerHTML = this.currentEditingActions.map((action, index) => `
            <div class="action-item">
                <span>${actionTypes[action.type] || 'Unknown'}</span>
                ${action.target ? `<span class="action-target">${this.escapeHtml(action.target)}</span>` : ''}
                ${action.delayMs ? `<span class="action-delay">${action.delayMs}ms</span>` : ''}
                <button type="button" class="remove-action-btn" data-index="${index}">${this.icons.trash}</button>
            </div>
        `).join('');

        container.querySelectorAll('.remove-action-btn').forEach(btn => {
            btn.addEventListener('click', () => {
                this.currentEditingActions.splice(parseInt(btn.dataset.index), 1);
                this.renderActionsList();
            });
        });
    },

    addAction() {
        const typeSelect = document.getElementById('newActionType');
        const targetInput = document.getElementById('newActionTarget');
        const delayInput = document.getElementById('newActionDelay');

        const action = {
            type: parseInt(typeSelect.value),
            target: targetInput.value || null,
            value: null,
            delayMs: parseInt(delayInput.value) || 0
        };

        if (!this.currentEditingActions) this.currentEditingActions = [];
        this.currentEditingActions.push(action);
        this.renderActionsList();

        targetInput.value = '';
        delayInput.value = '0';
    },

    async saveButton() {
        const pageId = document.getElementById('editorPageId').value;
        const buttonId = document.getElementById('editorButtonId').value;
        const label = document.getElementById('buttonLabel').value;
        const icon = document.getElementById('buttonIcon').value;
        const color = document.getElementById('buttonColor').value;
        const textColor = document.getElementById('buttonTextColor').value;

        const button = {
            label,
            icon,
            color,
            textColor,
            actions: this.currentEditingActions || []
        };

        try {
            if (buttonId) {
                await fetch(`/api/buttons/pages/${pageId}/buttons/${buttonId}`, {
                    method: 'PUT',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(button)
                });
            } else {
                await fetch(`/api/buttons/pages/${pageId}/buttons`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(button)
                });
            }

            await this.loadCustomConfig();
            this.renderCustomButtons();
            this.closeButtonEditor();
            this.showToast('Button saved!', 'success');
        } catch (error) {
            this.showToast('Failed to save button', 'error');
        }
    },

    async deleteButton() {
        const pageId = document.getElementById('editorPageId').value;
        const buttonId = document.getElementById('editorButtonId').value;

        if (!buttonId) return;
        if (!confirm('Delete this button?')) return;

        try {
            await fetch(`/api/buttons/pages/${pageId}/buttons/${buttonId}`, {
                method: 'DELETE'
            });

            await this.loadCustomConfig();
            this.renderCustomButtons();
            this.closeButtonEditor();
            this.showToast('Button deleted', 'success');
        } catch (error) {
            this.showToast('Failed to delete button', 'error');
        }
    },

    initKeyboardShortcuts() {
        document.addEventListener('keydown', (e) => {
            if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA') return;

            switch (e.key) {
                case '1': case '2': case '3': case '4': case '5':
                case '6': case '7': case '8': case '9':
                    const sceneIndex = parseInt(e.key) - 1;
                    if (this.state.scenes[sceneIndex]) {
                        this.apiPost(`/api/obs/scene/${encodeURIComponent(this.state.scenes[sceneIndex].name)}`);
                        this.fetchStatus();
                    }
                    break;
                case 's':
                    if (e.ctrlKey || e.metaKey) {
                        e.preventDefault();
                        this.apiPost('/api/obs/stream/toggle');
                        this.fetchStatus();
                    }
                    break;
                case 'r':
                    if (e.ctrlKey || e.metaKey) {
                        e.preventDefault();
                        this.apiPost('/api/obs/record/toggle');
                        this.fetchStatus();
                    }
                    break;
                case 'Escape':
                    this.closeButtonEditor();
                    break;
            }
        });
    },

    toggleSection(sectionId) {
        this.state.expandedSections[sectionId] = !this.state.expandedSections[sectionId];
        const section = document.getElementById(`${sectionId}Section`);
        const header = section?.querySelector('.section-header');
        const content = section?.querySelector('.button-grid');

        if (header && content) {
            header.classList.toggle('collapsed', !this.state.expandedSections[sectionId]);
            content.style.display = this.state.expandedSections[sectionId] ? 'grid' : 'none';
        }
    }
};

document.addEventListener('DOMContentLoaded', () => StreamDeck.init());
