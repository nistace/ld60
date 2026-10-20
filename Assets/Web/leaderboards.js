const search = document.querySelector('#names');
const table = document.querySelector('#leaderboard');
const players = new Map();
let levels = [];
let sortLevel;

function formatTime(time) {
    const seconds = Math.floor(time / 100);
    const milliseconds = (time % 100);
    return `${String(seconds).padStart(2, '0')}.${String(milliseconds).padStart(2, '0')}`;
}

function render() {
    const names = search.value.split(',').map(name => name.trim().toLowerCase()).filter(Boolean);
    const matches = [...players.entries()]
        .filter(([name]) => names.length === 0 || names.some(term => name.toLowerCase().includes(term)))
        .sort(([nameA, timesA], [nameB, timesB]) => {
            const timeA = timesA.get(sortLevel) ?? Infinity;
            const timeB = timesB.get(sortLevel) ?? Infinity;
            return (timeA === timeB ? 0 : timeA - timeB) || nameA.localeCompare(nameB);
        });

    const header = document.createElement('tr');
    const playerHeading = document.createElement('th');
    playerHeading.scope = 'col';
    playerHeading.textContent = 'Player';
    header.append(playerHeading);
    for (const [index, level] of levels.entries()) {
        const heading = document.createElement('th');
        heading.scope = 'col';
        heading.setAttribute('aria-sort', level === sortLevel ? 'ascending' : 'none');
        const button = document.createElement('button');
        button.type = 'button';
        button.textContent = `M${String(index + 1).padStart(2, '0')}`;
        button.addEventListener('click', () => {
            sortLevel = level;
            render();
        });
        heading.append(button);
        header.append(heading);
    }
    table.tHead.replaceChildren(header);

    const rows = matches.map(([name, times]) => {
        const row = document.createElement('tr');
        const player = document.createElement('th');
        player.scope = 'row';
        player.textContent = name;
        row.append(player);
        for (const level of levels) {
            const cell = document.createElement('td');
            cell.textContent = times.has(level) ? formatTime(times.get(level)) : '-';
            row.append(cell);
        }
        return row;
    });
    table.tBodies[0].replaceChildren(...rows);
    table.hidden = players.size === 0;
}

async function loadScores() {
    try {
        const response = await fetch('https://nathanistace.be/gamejams/ld60/leaderboards.php');
        if (!response.ok) throw new Error('Request failed');
        const data = await response.json();
        if (!Array.isArray(data.scores)) throw new Error('Invalid scores');
        const levelNames = new Set();
        for (const score of data.scores) {
            const level = String(score.level);
            const name = String(score.user_name);
            const time = Number(score.time);
            if (!Number.isSafeInteger(time) || time < 0) continue;
            levelNames.add(level);
            if (!players.has(name)) players.set(name, new Map());
            const times = players.get(name);
            times.set(level, Math.min(times.get(level) ?? Infinity, time));
        }
        levels = [...levelNames].sort();
        sortLevel = levels[0];
        render();
    } catch (error) {
    }
}

search.addEventListener('input', render);
loadScores();
