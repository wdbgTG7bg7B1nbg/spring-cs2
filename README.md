# 🌸 SPRING — CS2 Subtick Product, Web Panel & Desktop Loader Software

A complete, premium CS2 subtick ecosystem with an **actual compiled native desktop GUI client** (`SpringLoader.exe`), transparent frosted-glass GUI, live injection console logs, Steam auto-launch capability, in-game Dynamic Island HUD overlay, Cloudflare domain configuration for **`rykga.com`**, and clean bilingual support (**English 🇬🇧 & Magyar 🇭🇺**).

---

## ⚡ What's Included

### 1. 🖥️ Compiled Native Windows Desktop GUI Loader (`SpringLoader.exe`)
A custom translucent frosted glass desktop application built with C# & WPF:
- **See-Through Acrylic/Frosted Glass UI**: `#D00c0c12` translucent backdrop with blurred lime & cyan ambient canvas orbs and glowing neon borders.
- **Steam CS2 Launcher**: `🚀 Launch CS2 (Steam)` button launches Counter-Strike 2 directly via `steam://rungameid/730`.
- **Live Injection & Telemetry Console**: Transparent terminal window with real-time logs, timestamping, process detection, and a `Clear Logs` button.
- **Latency & Tick Benchmark**: `⏱ Test Latency` simulates 10,000 subtick movement frames and benchmarks EdgeBug accuracy.
- **In-Game Overlay Launcher**: `🎮 Open In-Game Menu` or `⚡ Inject into CS2` directly activates the floating transparent overlay and Dynamic Island HUD.
- **Cloud Configurations**: Loaded from `spring_client_config.json`, including Default Legit, EdgeBug Pro, and Rage HVH.
- **HWID Protection**: Binds to Windows `MachineGuid` with automated unbind capability.
- **Bilingual Switch**: Toggle between **English** and **Magyar** instantly in the titlebar.

**Executable Locations:**
- `SpringLoader.exe` (Root folder — ready to run!)
- `SpringLoader.bat` (Root batch launcher)
- `software/SpringClient.exe`
- `software/publish/SpringClient.exe`

---

### 2. 🎮 In-Game Transparent Overlay & Dynamic Island HUD (`InGameOverlayWindow`)
- **Top Floating Pill**: Draggable Dynamic Island with 4 switchable modes:
  1. 🎵 Spotify / Media Playback
  2. 💣 C4 Bomb Timer with site location & defuse kit countdown
  3. ⚠️ Flashbang & Molotov vector warnings
  4. 📦 Enemy Loadout inspector
- **In-Game Menu**: Toggled on/off via `[INSERT]` or `[ESC]`.
- **Live Sliders**: EdgeBug tolerance (0.5 – 4.0 ms), aim smoothing (1.0x – 10.0x), and recoil compensation (20% – 100%).

---

### 3. 🌐 Web Application & Cloudflare Setup (`rykga.com`)
The web application is ready for deployment onto your domain **`rykga.com`** using Cloudflare Pages:
- **`CNAME`**: Configured with `rykga.com`.
- **`_headers`**: Configured with strict security headers, CORS, and automatic attachment headers for `SpringLoader.exe`.
- **`wrangler.toml`**: Configured for Cloudflare Pages deployment targeting `rykga.com`.
- **`index.html`**: Interactive Dynamic Island simulator, pricing calculator, responsive navbar, and language selector.
- **`panel.html`**: Cloud user dashboard with download link to `SpringLoader.exe`, subscription manager, and config creator.

#### ☁️ How to Deploy to `rykga.com` with Cloudflare:
1. **Via Cloudflare Pages Dashboard (Git or Direct Upload)**:
   - Log into [Cloudflare Dashboard](https://dash.cloudflare.com/).
   - Navigate to **Workers & Pages** → **Create application** → **Pages**.
   - Either connect your GitHub repository or choose **Direct Upload** and upload this folder (`lclipped`).
   - In your project settings, go to **Custom Domains** → click **Set up a custom domain** → enter `rykga.com`.
   - Cloudflare will automatically configure DNS records and provision an SSL certificate.
2. **Via Cloudflare CLI (Wrangler)**:
   ```powershell
   npx wrangler pages deploy . --project-name spring-cs2
   ```

---

## 🚀 How to Run Locally

### 1. Launch the Desktop Software
- **Native GUI Executable**: Double-click `SpringLoader.exe` in the root folder.
- **Batch Launcher**: Double-click `SpringLoader.bat`.
- **CLI Terminal Loader**:
  ```powershell
  node software/spring-loader.js
  ```

### 2. Run the Local Web Server
```powershell
npx serve .
```
- Landing Page: [http://localhost:3000](http://localhost:3000)
- User Panel: [http://localhost:3000/panel.html](http://localhost:3000/panel.html)

---

## 🇭🇺 Magyar Használati Útmutató

1. **A Loader Indítása**:
   Kattints duplán a `SpringLoader.exe` fájlra a gyökérmappában. Megnyílik az átlátszó, üveg hatású (frosted glass) betöltő.
   - A `🚀 Launch CS2 (Steam)` gomb automatikusan elindítja a játékot a Steamen keresztül.
   - Az `⚡ Inject into CS2` vagy a `🎮 Open In-Game Menu` gomb azonnal megjeleníti a játékbeli Dynamic Islandot és menüt.
   - A játékban nyomd meg az `[INSERT]` billentyűt a menü elrejtéséhez vagy megjelenítéséhez!
2. **Weboldal Feltöltése a `rykga.com` Domainre**:
   A mappa tartalmazza a `CNAME`, `_headers` és `wrangler.toml` konfigurációs fájlokat. A Cloudflare Pages felületén közvetlenül feltöltheted ezt a könyvtárat, vagy csatlakoztathatod a GitHub tárolóhoz, majd hozzáadhatod a `rykga.com` egyedi domaint.

---

&copy; 2026 spring software &bull; rykga.com. All rights reserved.
