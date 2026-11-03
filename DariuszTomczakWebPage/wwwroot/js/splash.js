(function () {
    const root = document.getElementById('splash');
    if (!root) return;

    const LINE1 = 'DARIUSZ';
    const LINE2 = 'TOMCZAK';
    const DURATION_S = 3;
    const HOLD_MS = 2000; // how long the finished board stays visible
    const MODE = 'Assembler'; // 'Assembler' | 'Hex + Assembler' | 'Hex' | 'Litery'
    const SECTION_MARKS = [40, 75, 100];

    const app = document.getElementById('app');
    const errorUi = document.getElementById('blazor-error-ui');
    const board = root.querySelector('.splash-board');
    const progressBar = root.querySelector('.splash-progress div');
    const sectionEls = Array.from(root.querySelectorAll('.splash-sections li'));

    if (app) app.inert = true;

    const rows = [LINE1, LINE2].map((r) => r.toUpperCase().slice(0, 14)).filter((r) => r.length > 0);
    const cols = Math.max(1, ...rows.map((r) => r.length));
    const base = MODE === 'Hex' || MODE === 'Litery' ? 12 : 20;
    const cells = [];
    let total = 0;
    rows.forEach((row, r) => {
        for (let i = 0; i < cols; i++) {
            const start = i + r * 2;
            const settle = base + i * 3 + r * 5 + ((i * 7 + r * 3) % 4);
            total = Math.max(total, settle + 2);
            cells.push({ i, r, start, settle, target: row[i] || ' ' });
        }
    });

    board.style.setProperty('--splash-cols', cols);
    board.style.setProperty('--splash-font', ((100 - (cols - 1) * 1.4) / cols * 0.82).toFixed(2) + 'cqi');

    const spans = cells.map(() => {
        const tile = document.createElement('div');
        tile.className = 'splash-tile';
        const span = document.createElement('span');
        span.textContent = '\u00A0';
        tile.appendChild(span);
        board.appendChild(tile);
        return span;
    });

    const LETTERS = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789&#+';
    const HEX = '0123456789ABCDEF';
    const ASM = [
        ['MOV EAX,1', 'XOR ECX,ECX', 'CMP EAX,ECX', 'JNZ 4F2Ah', 'CALL 1C40h', 'SHL EAX,2'],
        ['LEA ESI,[EBP-8]', 'INC EDI', 'TEST AL,AL', 'POP EBX', 'ADD ESP,8', 'RET']
    ];
    const hexEnd = MODE === 'Hex + Assembler' ? 8 : 0;
    const asmStep = 3;

    let tick = 0;
    let doneAt = 0;
    let failed = false;
    let appReady = false;
    let lastProgress = -1;
    let closed = false;

    // Blazor reports real download progress through this CSS custom property.
    function loadPercent() {
        if (appReady) return 100;
        const raw = getComputedStyle(document.documentElement).getPropertyValue('--blazor-load-percentage');
        const value = parseFloat(raw);
        return isNaN(value) ? 0 : Math.min(99, value);
    }

    function renderBoard() {
        const done = tick >= total;
        cells.forEach((c, idx) => {
            let ch = ' ';
            let color = '#9CA3AF';
            if (done || tick >= c.settle) {
                ch = c.target;
                color = '#1E3A5F';
            } else if (tick >= c.start) {
                const noise = tick * 5 + c.i * 11 + c.r * 17;
                if (MODE === 'Litery') {
                    ch = LETTERS[noise % LETTERS.length];
                } else if (MODE === 'Hex' || tick < hexEnd) {
                    ch = HEX[noise % HEX.length];
                } else {
                    const lines = ASM[c.r % ASM.length];
                    const line = lines[Math.floor((tick - hexEnd) / asmStep) % lines.length];
                    ch = line[c.i] || ' ';
                    color = '#B45309';
                }
            }
            spans[idx].textContent = ch === ' ' ? '\u00A0' : ch;
            spans[idx].style.color = color;
        });
        root.classList.toggle('splash-done', done);
    }

    function renderProgress() {
        const pct = loadPercent();
        if (pct === lastProgress) return;
        lastProgress = pct;
        progressBar.style.width = pct + '%';
        sectionEls.forEach((li, k) => {
            const ready = pct >= SECTION_MARKS[k];
            li.classList.toggle('ready', ready);
            const sr = li.querySelector('.splash-sr');
            if (sr) sr.textContent = ready ? 'loaded' : 'loading';
        });
    }

    function tryClose() {
        if (closed || !appReady) return;
        if (!failed && (!doneAt || Date.now() - doneAt < HOLD_MS)) return;
        closed = true;
        clearInterval(timer);

        if (app) app.inert = false;
        root.setAttribute('aria-busy', 'false');
        root.classList.add('splash-hidden');

        const heading = app && app.querySelector('h1');
        if (heading) {
            if (!heading.hasAttribute('tabindex')) heading.setAttribute('tabindex', '-1');
            heading.focus({ preventScroll: true });
        }

        setTimeout(() => root.remove(), 450);
    }

    renderBoard();
    renderProgress();

    const ms = (DURATION_S * 1000) / total;
    const t0 = Date.now();
    const timer = setInterval(() => {
        if (tick < total) {
            const next = Math.min(total, Math.floor((Date.now() - t0) / ms));
            if (next !== tick) {
                tick = next;
                renderBoard();
                if (tick >= total) doneAt = Date.now();
            }
        }
        // If startup fails, get out of the way so the error UI is visible.
        if (errorUi && getComputedStyle(errorUi).display !== 'none') {
            failed = true;
            appReady = true;
        }
        renderProgress();
        tryClose();
    }, Math.max(16, Math.floor(ms / 2)));

    window.splash = {
        appReady() {
            appReady = true;
            renderProgress();
            tryClose();
        }
    };
})();
