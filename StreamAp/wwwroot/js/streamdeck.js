const StreamDeck = {
    state: {
        isConnected: false,
        isStreaming: false,
        isRecording: false,
        currentScene: null,
        scenes: [],
        audioSources: []
    },

    pollInterval: null,
    POLL_RATE: 1500,

    // SVG Icons
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
        loader: '<svg viewBox="0 0 24 24"><circle cx="12" cy="12" r="10" stroke-width="2" stroke-dasharray="32" stroke-linecap="round"/></svg>'
    },

    async init() {
        this.renderControlButtons();
        await this.fetchStatus();
        this.startPolling();
    },

    renderControlButtons() {
        const container = document.getElementById('controlButtons');
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
        `;

        container.querySelectorAll('.deck-button').forEach(btn => {
            btn.addEventListener('click', () => this.handleButtonClick(btn));
        });
    },

    renderSceneButtons() {
        const container = document.getElementById('sceneButtons');

        if (this.state.scenes.length === 0) {
            container.innerHTML = '<p style="color: var(--sd-text-secondary); grid-column: 1/-1;">Connect to OBS to see scenes</p>';
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

    renderAudioButtons() {
        const container = document.getElementById('audioButtons');
        const section = document.getElementById('audioSection');

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

    updateUI() {
        // Update connection status
        const statusDot = document.getElementById('statusDot');
        const statusText = document.getElementById('statusText');
        const connectBtn = document.querySelector('[data-action="connect"]');
        const streamBtn = document.querySelector('[data-action="stream"]');
        const recordBtn = document.querySelector('[data-action="record"]');

        if (this.state.isConnected) {
            statusDot.classList.add('connected');
            statusText.textContent = 'Connected';
            connectBtn.classList.add('connected');
            connectBtn.innerHTML = `${this.icons.linkOff}<span class="deck-button-label">Disconnect</span>`;
            streamBtn.disabled = false;
            recordBtn.disabled = false;
        } else {
            statusDot.classList.remove('connected');
            statusText.textContent = 'Disconnected';
            connectBtn.classList.remove('connected');
            connectBtn.innerHTML = `${this.icons.link}<span class="deck-button-label">Connect</span>`;
            streamBtn.disabled = true;
            recordBtn.disabled = true;
        }

        // Update stream button
        if (this.state.isStreaming) {
            streamBtn.classList.add('streaming');
            streamBtn.querySelector('.deck-button-label').textContent = 'Live';
        } else {
            streamBtn.classList.remove('streaming');
            streamBtn.querySelector('.deck-button-label').textContent = 'Stream';
        }

        // Update record button
        if (this.state.isRecording) {
            recordBtn.classList.add('recording');
            recordBtn.querySelector('.deck-button-label').textContent = 'Recording';
        } else {
            recordBtn.classList.remove('recording');
            recordBtn.querySelector('.deck-button-label').textContent = 'Record';
        }

        // Update scene and audio buttons
        this.renderSceneButtons();
        this.renderAudioButtons();
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

            this.updateUI();
        } catch (error) {
            console.error('Failed to fetch status:', error);
        }
    },

    async handleButtonClick(button) {
        const action = button.dataset.action;
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

                case 'scene':
                    const sceneName = button.dataset.scene;
                    await this.apiPost(`/api/obs/scene/${encodeURIComponent(sceneName)}`);
                    break;

                case 'audio':
                    const sourceName = button.dataset.source;
                    await this.apiPost(`/api/obs/audio/${encodeURIComponent(sourceName)}/toggle`);
                    break;
            }

            // Refresh status after action
            await this.fetchStatus();
        } catch (error) {
            console.error('Action failed:', error);
            this.showToast('Action failed', 'error');
        } finally {
            button.classList.remove('loading');
        }
    },

    async apiPost(url) {
        const response = await fetch(url, { method: 'POST' });
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

    showToast(message, type = 'info') {
        const container = document.getElementById('toastContainer');
        const toast = document.createElement('div');
        toast.className = `toast ${type}`;
        toast.textContent = message;
        container.appendChild(toast);

        setTimeout(() => {
            toast.remove();
        }, 3000);
    },

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
};

document.addEventListener('DOMContentLoaded', () => StreamDeck.init());
