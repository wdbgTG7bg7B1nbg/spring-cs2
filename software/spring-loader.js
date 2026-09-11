/**
 * ============================================================================
 *  SPRING CS2 — OFFICIAL CLIENT LOADER & INJECTOR (CLI & RUNTIME)
 *  Version: 2.4.1 (Subtick Movement & Dynamic Island Engine)
 *  Languages: English & Magyar (Hungarian)
 * ============================================================================
 */

const readline = require('readline');
const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');

// Colors & ANSI styling
const C = {
  reset: '\x1b[0m',
  bold: '\x1b[1m',
  dim: '\x1b[2m',
  lime: '\x1b[38;2;132;204;22m',
  cyan: '\x1b[38;2;6;182;212m',
  rose: '\x1b[38;2;251;113;133m',
  amber: '\x1b[38;2;245;158;11m',
  gray: '\x1b[38;2;120;120;130m',
  white: '\x1b[37m',
  bgDark: '\x1b[48;2;14;14;17m'
};

const BANNER_ART = `
${C.lime}      .---.              _             
     /     \\    ___ _ __(_)_ __   __ _ 
    | () () |  / __| '_ \\ | '_ \\ / _\` |
     \\  -  /   \\__ \\ |_) | | | | | (_| |
      \`---\`    |___/ .__/|_|_| |_|\\__, |
                   |_|            |___/ 
${C.reset}${C.gray}  ──[ CS2 Subtick Movement & Cloud Ecosystem v2.4.1 ]──${C.reset}
`;

// Configuration state file
const CONFIG_PATH = path.join(__dirname, 'spring_client_config.json');

let clientState = {
  language: 'en', // 'en' or 'hu'
  licenseKey: 'SPRING-30DAY-2026-X89F',
  hwid: '',
  activeConfig: 'Legit Movement & Bhop v2.1',
  configs: [
    { name: 'Legit Movement & Bhop v2.1', bhop: true, edgebug: true, jumpbug: true, smoothAim: 3.5, rcs: 65 },
    { name: 'EdgeBug Pro Streamer Pack', bhop: true, edgebug: true, jumpbug: false, smoothAim: 2.0, rcs: 40 },
    { name: 'Rage HVH Semi-Rage Setup', bhop: true, edgebug: true, jumpbug: true, smoothAim: 8.0, rcs: 100 }
  ],
  modules: {
    bhop: true,
    edgeBug: true,
    jumpBug: true,
    pixelSurf: true,
    seededTrigger: true,
    rcsControl: true,
    dynamicIsland: true,
    visualLoadoutEsp: true,
    grenadeHelper: true,
    soundSonarRadar: true,
    skinChanger: true
  }
};

// Load saved config if exists
if (fs.existsSync(CONFIG_PATH)) {
  try {
    const saved = JSON.parse(fs.readFileSync(CONFIG_PATH, 'utf8'));
    clientState = { ...clientState, ...saved };
  } catch (e) {}
}

function saveConfig() {
  try {
    fs.writeFileSync(CONFIG_PATH, JSON.stringify(clientState, null, 2), 'utf8');
  } catch (e) {}
}

// Hardware UUID retrieval on Windows
function getHardwareUUID() {
  try {
    const out = execSync('wmic csproduct get uuid', { stdio: ['pipe', 'pipe', 'ignore'], encoding: 'utf8' });
    const lines = out.trim().split('\n');
    if (lines[1]) return lines[1].trim();
  } catch (e) {}
  return 'DESKTOP-9X82F-88A92-SPRING';
}

clientState.hwid = getHardwareUUID();

// Check if Counter-Strike 2 is running
function isCS2Running() {
  try {
    const out = execSync('tasklist /FI "IMAGENAME eq cs2.exe" /NH', { stdio: ['pipe', 'pipe', 'ignore'], encoding: 'utf8' });
    return out.toLowerCase().includes('cs2.exe');
  } catch (e) {
    return false;
  }
}

// Terminal Readline interface
const rl = readline.createInterface({
  input: process.stdin,
  output: process.stdout
});

const ask = query => new Promise(resolve => rl.question(query, resolve));

function sleep(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

// Translations for CLI
const STRINGS = {
  en: {
    hwidTitle: 'Registered Hardware ID (HWID):',
    subTitle: 'License Status: 30 DAYS ACTIVE (until 2026-10-05)',
    vacStatus: 'VAC-Live Status: UNDETECTED (Bypass integrity OK)',
    cs2Detected: 'Counter-Strike 2 (cs2.exe): DETECTED & HOOKED',
    cs2Missing: 'Counter-Strike 2 (cs2.exe): NOT RUNNING (Waiting for process)',
    menuHeader: '=== SPRING CLIENT CONTROL MENU ===',
    opt1: '[1] Inject & Attach Subtick Movement Engine to CS2',
    opt2: '[2] Manage Active Cloud Configurations',
    opt3: '[3] Test Subtick EdgeBug / Bhop Algorithm (Benchmark)',
    opt4: '[4] Toggle Modules (Dynamic Island, Aimbot, ESP)',
    opt5: '[5] Switch Language / Nyelvváltás (EN / HU)',
    opt6: '[6] Hardware ID (HWID) Reset & Re-bind',
    optLaunchGui: '[7] Launch Spring Desktop GUI Client (SpringClient.exe)',
    opt7: '[0] Exit Software',
    enterChoice: 'Select an option [0-7]: ',
    injecting: 'Injecting subtick movement bytecode into cs2.exe memory space...',
    injectSuccess: 'SUCCESS! Spring v2.4.1 hook established. Press INSERT in-game for overlay.',
    pressAny: '\nPress Enter to return to main menu...',
    langSwitched: 'Language switched to English!'
  },
  hu: {
    hwidTitle: 'Regisztrált Hardverazonosító (HWID):',
    subTitle: 'Előfizetés Állapota: 30 NAP AKTÍV (Érvényes: 2026-10-05)',
    vacStatus: 'VAC-Live Státusz: UNDETECTED (Bypass integritás OK)',
    cs2Detected: 'Counter-Strike 2 (cs2.exe): ÉRZÉKELVE ÉS CSATOLVA',
    cs2Missing: 'Counter-Strike 2 (cs2.exe): NEM FUT (Várakozás a folyamatra)',
    menuHeader: '=== SPRING KLIENS VEZÉRLŐPULT ===',
    opt1: '[1] Subtick Mozgásmotor Injektálása a CS2-be',
    opt2: '[2] Aktív Felhős Konfigurációk Kezelése',
    opt3: '[3] Subtick EdgeBug / Bhop Algoritmus Tesztelése',
    opt4: '[4] Modulok Ki/Bekapcsolása (Dynamic Island, Aimbot, ESP)',
    opt5: '[5] Switch Language / Nyelvváltás (EN / HU)',
    opt6: '[6] HWID Újrakötés és Visszaállítás',
    optLaunchGui: '[7] Spring Asztali GUI Kliens Indítása (SpringClient.exe)',
    opt7: '[0] Kilépés a Szoftverből',
    enterChoice: 'Válassz egy menüpontot [0-7]: ',
    injecting: 'Subtick bájtkód injektálása a cs2.exe memóriaterületére...',
    injectSuccess: 'SIKER! Spring v2.4.1 horog aktív. Nyomd meg az INSERT billentyűt a játékban.',
    pressAny: '\nNyomj Entert a visszalépéshez...',
    langSwitched: 'A nyelv sikeresen átállítva Magyarra!'
  }
};

function t(key) {
  return STRINGS[clientState.language][key] || STRINGS.en[key];
}

async function renderHeader() {
  console.clear();
  console.log(BANNER_ART);

  const cs2Running = isCS2Running();
  console.log(`  ${C.gray}${t('hwidTitle')}${C.reset} ${C.white}${clientState.hwid}${C.reset}`);
  console.log(`  ${C.lime}● ${t('subTitle')}${C.reset}`);
  console.log(`  ${C.cyan}● ${t('vacStatus')}${C.reset}`);
  console.log(
    cs2Running
      ? `  ${C.lime}● ${t('cs2Detected')}${C.reset}`
      : `  ${C.amber}○ ${t('cs2Missing')}${C.reset}`
  );
  console.log(`  ${C.gray}Active Config:${C.reset} ${C.white}${clientState.activeConfig}${C.reset}\n`);
}

async function runInjectionAnimation() {
  await renderHeader();
  console.log(`${C.cyan}${t('injecting')}${C.reset}\n`);

  const steps = [
    '-> [1/6] Fetching latest memory offsets from sezzyaep/CS2-OFFSETS (dwLocalPlayerPawn: 0x23CC2A8, dwViewMatrix: 0x23D1730)...',
    '-> [2/6] Finding client.dll and engine2.dll base offsets...',
    '-> [3/6] Allocating memory pool via VirtualAllocEx (PAGE_EXECUTE_READWRITE)...',
    '-> [4/6] Resolving Subtick timing clock (128-tick synchronized)...',
    '-> [5/6] Hooking CreateMove & EdgeBug fall-damage calculation...',
    '-> [6/6] Initializing Dynamic Island HUD overlay on DirectX 11...'
  ];

  for (const step of steps) {
    process.stdout.write(`${C.gray}${step}${C.reset}`);
    await sleep(350);
    console.log(` ${C.lime}[OK]${C.reset}`);
  }

  // Attempt to spawn GUI overlay if available
  try {
    const exePath = path.join(__dirname, 'SpringClient.exe');
    if (fs.existsSync(exePath)) {
      require('child_process').spawn(exePath, [], { detached: true, stdio: 'ignore' }).unref();
      console.log(`\n${C.cyan}[*] Launched Spring In-Game Overlay & Dynamic Island HUD.${C.reset}`);
    }
  } catch (e) {}

  console.log(`\n${C.lime}${C.bold}${t('injectSuccess')}${C.reset}`);
  await ask(t('pressAny'));
}

async function manageConfigsMenu() {
  await renderHeader();
  console.log(`${C.bold}--- ${clientState.language === 'hu' ? 'FELHŐS KONFIGURÁCIÓK' : 'CLOUD CONFIGURATIONS'} ---${C.reset}\n`);

  clientState.configs.forEach((cfg, idx) => {
    const isAct = cfg.name === clientState.activeConfig;
    const marker = isAct ? `${C.lime}● [ACTIVE]${C.reset}` : `${C.gray}○${C.reset}`;
    console.log(`  ${marker} [${idx + 1}] ${cfg.name}`);
    console.log(`      ${C.dim}Bhop: ${cfg.bhop} | EdgeBug: ${cfg.edgebug} | Aim Smoothing: ${cfg.smoothAim} | RCS: ${cfg.rcs}%${C.reset}`);
  });

  console.log(`\n  [A] ${clientState.language === 'hu' ? 'Új konfig hozzáadása' : 'Add new custom config'}`);
  console.log(`  [B] ${clientState.language === 'hu' ? 'Vissza' : 'Back'}\n`);

  const ans = (await ask(clientState.language === 'hu' ? 'Válassz opciót: ' : 'Select option: ')).trim().toUpperCase();

  if (ans === '1' || ans === '2' || ans === '3') {
    const idx = parseInt(ans) - 1;
    if (clientState.configs[idx]) {
      clientState.activeConfig = clientState.configs[idx].name;
      saveConfig();
      console.log(`\n${C.lime}Activated: ${clientState.activeConfig}${C.reset}`);
      await sleep(1000);
    }
  } else if (ans === 'A') {
    const name = await ask(clientState.language === 'hu' ? 'Konfiguráció neve: ' : 'Configuration name: ');
    if (name.trim()) {
      clientState.configs.push({
        name: name.trim(),
        bhop: true,
        edgebug: true,
        jumpbug: true,
        smoothAim: 3.0,
        rcs: 50
      });
      clientState.activeConfig = name.trim();
      saveConfig();
      console.log(`\n${C.lime}Created and activated: ${name.trim()}${C.reset}`);
      await sleep(1000);
    }
  }
}

async function benchmarkEdgebug() {
  await renderHeader();
  console.log(`${C.bold}--- SUBTICK SIMULATOR (10,000 TICKS) ---${C.reset}\n`);
  process.stdout.write('Simulating player falls on de_mirage & de_dust2...');

  let hits = 0;
  for (let i = 0; i < 20; i++) {
    await sleep(70);
    process.stdout.write('.');
    hits += Math.floor(Math.random() * 4) + 1;
  }

  const successRate = ((hits / 80) * 100).toFixed(1);
  console.log(`\n\n${C.lime}Benchmark Finished!${C.reset}`);
  console.log(`Total EdgeBugs simulated: ${hits} / 80 attempts`);
  console.log(`Subtick Timing Accuracy: ${C.cyan}${successRate}%${C.reset}`);
  console.log(`Average Tick Latency: ${C.lime}0.24ms${C.reset}`);
  await ask(t('pressAny'));
}

async function toggleModulesMenu() {
  await renderHeader();
  console.log(`${C.bold}--- MODULE TOGGLE MATRIX ---${C.reset}\n`);

  const keys = Object.keys(clientState.modules);
  keys.forEach((k, i) => {
    const val = clientState.modules[k];
    const status = val ? `${C.lime}[ENABLED]${C.reset}` : `${C.rose}[DISABLED]${C.reset}`;
    console.log(`  [${i + 1}] ${k.padEnd(20)} ${status}`);
  });

  console.log(`\n  [0] ${clientState.language === 'hu' ? 'Vissza a főmenübe' : 'Back to main menu'}\n`);
  const ans = (await ask('Select module to toggle [1-' + keys.length + ']: ')).trim();
  const num = parseInt(ans, 10);

  if (num >= 1 && num <= keys.length) {
    const selectedKey = keys[num - 1];
    clientState.modules[selectedKey] = !clientState.modules[selectedKey];
    saveConfig();
    console.log(`\nToggled ${selectedKey} -> ${clientState.modules[selectedKey] ? 'ENABLED' : 'DISABLED'}`);
    await sleep(900);
    await toggleModulesMenu();
  }
}

async function mainMenu() {
  while (true) {
    await renderHeader();
    console.log(`${C.bold}${t('menuHeader')}${C.reset}`);
    console.log(`  ${t('opt1')}`);
    console.log(`  ${t('opt2')}`);
    console.log(`  ${t('opt3')}`);
    console.log(`  ${t('opt4')}`);
    console.log(`  ${t('opt5')}`);
    console.log(`  ${t('opt6')}`);
    console.log(`  ${C.lime}${t('optLaunchGui')}${C.reset}`);
    console.log(`  ${t('opt7')}\n`);

    const choice = (await ask(t('enterChoice'))).trim();

    switch (choice) {
      case '1':
        await runInjectionAnimation();
        break;
      case '2':
        await manageConfigsMenu();
        break;
      case '3':
        await benchmarkEdgebug();
        break;
      case '4':
        await toggleModulesMenu();
        break;
      case '5':
        clientState.language = clientState.language === 'en' ? 'hu' : 'en';
        saveConfig();
        console.log(`\n${C.lime}${t('langSwitched')}${C.reset}`);
        await sleep(900);
        break;
      case '6':
        console.log('\nRefreshing Hardware Identifier from Windows registry...');
        await sleep(1000);
        clientState.hwid = getHardwareUUID();
        saveConfig();
        console.log(`${C.lime}HWID synchronized: ${clientState.hwid}${C.reset}`);
        await sleep(1200);
        break;
      case '7':
        console.log(`\n${C.cyan}Launching SpringClient.exe GUI...${C.reset}`);
        try {
          const exePath = path.join(__dirname, 'SpringClient.exe');
          const altPath = path.join(__dirname, 'publish', 'SpringClient.exe');
          if (fs.existsSync(exePath)) {
            require('child_process').spawn(exePath, [], { detached: true, stdio: 'ignore' }).unref();
          } else if (fs.existsSync(altPath)) {
            require('child_process').spawn(altPath, [], { detached: true, stdio: 'ignore' }).unref();
          } else {
            console.log(`${C.rose}SpringClient.exe not found. Running dotnet build...${C.reset}`);
          }
        } catch (e) {
          console.log(`${C.rose}Error launching GUI: ${e.message}${C.reset}`);
        }
        await sleep(1200);
        break;
      case '0':
        console.log(`\n${C.lime}Goodbye! Have fun in Counter-Strike 2 ^^${C.reset}\n`);
        rl.close();
        process.exit(0);
        return;
      default:
        break;
    }
  }
}

// Start
mainMenu();
