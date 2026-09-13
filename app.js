/* ==========================================================================
   spring — Application Logic (Landing & Panel)
   Languages: English (EN) & Hungarian (HU)
   ========================================================================== */

const HTML_KEYS = new Set([
  'hero_title', 'spotlight_heading', 'island_desc', 'p_hwid_explanation'
]);

document.addEventListener('DOMContentLoaded', () => {

  // ============================================================
  // SHARED UTILITIES
  // ============================================================

  function initLucideIcons() {
    if (window.lucide) window.lucide.createIcons();
  }

  // --- Light / Dark Theme Switcher ---
  const themeBtn = document.getElementById('theme-toggle-btn');
  const themeText = document.getElementById('theme-text');
  let currentTheme = localStorage.getItem('spring_theme') || 'dark';

  function applyTheme(theme) {
    currentTheme = theme;
    localStorage.setItem('spring_theme', theme);
    document.documentElement.classList.toggle('light', theme === 'light');
    document.documentElement.classList.toggle('dark', theme === 'dark');

    if (themeText) themeText.textContent = theme === 'light' ? 'Light' : 'Dark';
    const darkIcon = document.querySelector('.theme-icon-dark');
    const lightIcon = document.querySelector('.theme-icon-light');
    if (darkIcon && lightIcon) {
      darkIcon.classList.toggle('hidden', theme === 'light');
      lightIcon.classList.toggle('hidden', theme === 'dark');
    }
  }

  if (themeBtn) {
    themeBtn.addEventListener('click', () => {
      applyTheme(currentTheme === 'dark' ? 'light' : 'dark');
    });
  }
  applyTheme(currentTheme);

  // --- Scroll Reveal Entrance Animations ---
  const observerOptions = { threshold: 0.15, rootMargin: '0px 0px -50px 0px' };
  const scrollObserver = new IntersectionObserver((entries, observer) => {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        entry.target.classList.add('reveal-active');
        observer.unobserve(entry.target);
      }
    });
  }, observerOptions);

  document.querySelectorAll('.section-container, .feature-card, .panel-card, .spotlight-grid').forEach(el => {
    el.classList.add('reveal-on-scroll');
    scrollObserver.observe(el);
  });

  // --- Draggable Website HUD Interface Engine ---
  function makeElementDraggable(elmnt, container) {
    if (!elmnt || !container) return;
    let pos1 = 0, pos2 = 0, pos3 = 0, pos4 = 0;

    elmnt.onmousedown = dragMouseDown;
    elmnt.ontouchstart = dragTouchStart;

    function dragMouseDown(e) {
      e.preventDefault();
      pos3 = e.clientX;
      pos4 = e.clientY;
      elmnt.classList.add('is-dragging');
      document.onmouseup = closeDragElement;
      document.onmousemove = elementDrag;
    }

    function elementDrag(e) {
      e.preventDefault();
      pos1 = pos3 - e.clientX;
      pos2 = pos4 - e.clientY;
      pos3 = e.clientX;
      pos4 = e.clientY;

      let newTop = elmnt.offsetTop - pos2;
      let newLeft = elmnt.offsetLeft - pos1;

      // Container bounds check
      const maxLeft = container.clientWidth - elmnt.offsetWidth;
      const maxTop = container.clientHeight - elmnt.offsetHeight;
      if (newLeft < 0) newLeft = 0;
      if (newTop < 0) newTop = 0;
      if (newLeft > maxLeft) newLeft = maxLeft;
      if (newTop > maxTop) newTop = maxTop;

      elmnt.style.top = newTop + "px";
      elmnt.style.left = newLeft + "px";
      elmnt.style.transform = "none";
    }

    function closeDragElement() {
      elmnt.classList.remove('is-dragging');
      document.onmouseup = null;
      document.onmousemove = null;
    }

    function dragTouchStart(e) {
      if (e.touches.length !== 1) return;
      const touch = e.touches[0];
      pos3 = touch.clientX;
      pos4 = touch.clientY;
      elmnt.classList.add('is-dragging');
      document.ontouchend = closeTouchDrag;
      document.ontouchmove = touchDrag;
    }

    function touchDrag(e) {
      if (e.touches.length !== 1) return;
      const touch = e.touches[0];
      pos1 = pos3 - touch.clientX;
      pos2 = pos4 - touch.clientY;
      pos3 = touch.clientX;
      pos4 = touch.clientY;

      let newTop = elmnt.offsetTop - pos2;
      let newLeft = elmnt.offsetLeft - pos1;
      const maxLeft = container.clientWidth - elmnt.offsetWidth;
      const maxTop = container.clientHeight - elmnt.offsetHeight;
      if (newLeft < 0) newLeft = 0;
      if (newTop < 0) newTop = 0;
      if (newLeft > maxLeft) newLeft = maxLeft;
      if (newTop > maxTop) newTop = maxTop;

      elmnt.style.top = newTop + "px";
      elmnt.style.left = newLeft + "px";
      elmnt.style.transform = "none";
    }

    function closeTouchDrag() {
      elmnt.classList.remove('is-dragging');
      document.ontouchend = null;
      document.ontouchmove = null;
    }
  }

  // Initialize Draggable Elements on HUD Screen Simulator
  const hudCanvas = document.getElementById('hud-canvas');
  if (hudCanvas) {
    makeElementDraggable(document.getElementById('island-pill'), hudCanvas);
    makeElementDraggable(document.getElementById('draggable-keybinds'), hudCanvas);
    makeElementDraggable(document.getElementById('draggable-speedo'), hudCanvas);
  }

  function showToast(msg, type = 'success') {
    const tc = document.getElementById('toast-container');
    if (!tc) return;
    const colors = {
      success: 'rgba(132, 204, 22, 0.9)',
      error: 'rgba(251, 113, 133, 0.9)',
      info: 'rgba(6, 182, 212, 0.9)',
    };
    const toast = document.createElement('div');
    toast.className = 'toast';
    toast.style.borderColor = colors[type] || colors.success;
    toast.innerHTML = `<span>${msg}</span>`;
    tc.appendChild(toast);
    setTimeout(() => {
      toast.style.opacity = '0';
      toast.style.transform = 'translateY(10px)';
      toast.style.transition = 'all 0.3s ease';
      setTimeout(() => toast.remove(), 320);
    }, 3200);
  }

  // ============================================================
  // TRANSLATION DICTIONARY: ENGLISH (EN) & HUNGARIAN (HU)
  // ============================================================
  const TRANSLATIONS = {
    en: {
      // Landing Navbar
      nav_home: "Home",
      nav_pricing: "Pricing",
      nav_panel: "Panel",
      nav_support: "Support",
      btn_login_panel: "Open Panel",

      // Landing Hero
      hero_title: '<span class="gradient-text">spring</span> — the cleanest <br>CS2 product on the market.',
      hero_subtitle: "flawless subtick movement, aesthetic visual modules, and vac-live undetected status. what else could you ask for?",
      btn_panel_cta: "User Dashboard",
      btn_buy_now: "Get It Now",
      btn_discord: "Join Discord",

      // Why Spring
      why_tag: "Why spring?",
      why_title: "nothing superfluous.",
      why_desc: "our product focuses on delivering a smooth, enjoyable gameplay experience, achieved by polishing each core feature to absolute perfection.",
      spotlight_heading: 'the smoothest bunnyhop you have ever seen.<br><span class="text-muted">just like every movement feature ^^</span>',
      testimonial_quote: '"insanely clean software, was hitting edgebugs seamlessly on premier while listening to tracks on dynamic island. movement feels like butter. easily 10/10 recommended."',
      author_role: "CS2 & OSU Creator · 100k followers",
      btn_buy_product: "Get Product",

      // Stats
      stat1_sub: "and every single one tuned to perfection!",
      stat2_sub: "zero frame drops — your game performance stays untouched",
      stat3_sub: "active 24/7 web ticket support with fast response times x_o",

      // Combat & Visual Engine Showcase
      combat_desc: 'subtick precision aimbot, independent vertical/horizontal RCS, 3D skeleton ESP, hit-rate filters, and advanced trajectory visualizers. <span class="text-faint">zero lag, maximum clarity! ^_^</span>',
      combat_hint: "Interactive Combat & Visual Engine — click any module on the right",
      tab_aimbot_title: "Subtick Aimbot & FOV",
      tab_aimbot_desc: "Bone priority selection (Head, Chest, Nearest) with customizable FOV overlay ring and smooth humanized curve tracking.",
      tab_rcs_title: "Independent Pitch & Yaw RCS",
      tab_rcs_desc: "Standalone Recoil Control System with separate vertical (pitch) and horizontal (yaw) compensation sliders.",
      tab_skeleton_title: "3D Skeleton & Health ESP",
      tab_skeleton_desc: "Full bone hierarchy skeleton rendering, color-coded health bars, weapon icons, and enemy snaplines.",
      tab_bomb_title: "C4 Bomb & Defuse HUD",
      tab_bomb_desc: "Real-time C4 plant countdown, site A/B indicator, and millisecond defuse timer HUD bar.",
      tab_grenade_title: "Grenade Trajectory Helper",
      tab_grenade_desc: "Parabolic physics arcs, grenade warning rings, and jump-throw lineup markers.",

      // Inside Features
      inside_title: "what's inside spring",
      inside_desc: "hover over any icon — every single module is alive and handcrafted.",
      f1_title: "subtick movement",
      f1_desc: "bhop, edge-bug, jump-bug & pixel-surf working seamlessly on every valve server!",
      f2_title: "subtle aimbot",
      f2_desc: "humanized smoothing, seeded triggerbot, standalone RCS recoil control. (◕‿◕)",
      f3_title: "hud customization",
      f3_desc: "multiple aesthetic themes, full RGB picker, and clean glassmorphism UI.",
      f4_title: "cloud configs",
      f4_desc: "configurations sync instantly across your devices via shareable codes.",
      f5_title: "native performance",
      f5_desc: "compiled native code with under 2% CPU and GPU overhead!",

      // Pricing
      pricing_tag: "Pricing",
      pricing_title: "straightforward, affordable plans.",
      pricing_desc: "one all-inclusive software, four durations. no hidden upsells, locked features or recurring auto-bills.",
      included_title: "WHAT'S INCLUDED",
      inc_1: "full features unlocked",
      inc_2: "all updates and game patches included",
      inc_3: "priority 24/7 web ticket support",
      inc_4: "1 subscription freeze allowance",
      inc_5: "1 automated HWID reset",
      hwid_note: "additional HWID resets available through panel",

      // Panel Translations
      p_nav_home: "Back to Home",
      p_tab_overview: "Overview",
      p_tab_keys: "Activate Key",
      p_tab_configs: "Cloud Configs",
      p_tab_hwid: "HWID Reset",
      p_tab_support: "Support & Tickets",
      p_welcome_title: "Welcome back, LO!",
      p_welcome_desc: "Manage your active license, cloud configurations, and client modules.",
      p_btn_download: "Download Spring Loader (.exe)",
      p_stat_sub_title: "Subscription Status",
      p_stat_hwid_title: "HWID Lock",
      p_stat_cfg_title: "Cloud Configs",
      p_stat_sync_on: "Auto-sync enabled",
      p_stat_ver_title: "Product Build",
      p_stat_up_to_date: "Up to date & secure",
      p_news_title: "Latest Update: Subtick Movement v2.4",
      p_news_date: "Yesterday, 18:40",
      p_news_body: "We updated the bunnyhop and edgebug engine for the latest Counter-Strike 2 patches! Full Dynamic Island synchronization and instant cloud configs hot-reload are now active.",
      p_keys_heading: "Activate License Key",
      p_keys_sub: "Enter your 16-character license or promo token received upon purchase.",
      p_key_label: "License Key",
      p_key_hint: "Example: <code>SPRING-30DAY-2026-X89F</code>",
      p_btn_activate: "Activate License",
      p_cfg_heading: "Cloud Configuration Manager",
      p_cfg_sub: "Create, share, and hotload presets directly into your game without restarting.",
      p_btn_create_cfg: "Create New Config",
      p_hwid_heading: "HWID Hardware Management",
      p_hwid_sub: "PC locking is required to secure your active license and account.",
      p_hwid_current_title: "Current Registered HWID:",
      p_hwid_explanation: "You are granted <strong>1 free automated HWID reset</strong> when changing PC components or upgrading your system.",
      p_btn_reset_hwid: "Reset HWID Lock",
      p_support_heading: "Customer Support",
      p_support_sub: "Need assistance with injection, offsets, or configuration? We're available 24/7.",
      p_sup_ticket_val: "Live Online Support",
      p_sup_ticket_sub: "Automatic offset resolution & ticketing",
      p_sup_faq_sub: "Installation, VAC bypass guides & presets"
    },

    hu: {
      // Landing Navbar
      nav_home: "Főoldal",
      nav_pricing: "Árazás",
      nav_panel: "Vezérlőpult",
      nav_support: "Támogatás",
      btn_login_panel: "Panel Megnyitása",

      // Landing Hero
      hero_title: '<span class="gradient-text">spring</span> — a legtisztább <br>CS2 szoftver a piacon.',
      hero_subtitle: "hibátlan subtick mozgás, letisztult vizuális modulok és vac-live undetected státusz. mi kellhet még?",
      btn_panel_cta: "Felhasználói Panel",
      btn_buy_now: "Vásárlás Most",
      btn_support: "Ügyfélszolgálat & Útmutatók",

      // Why Spring
      why_tag: "Miért a spring?",
      why_title: "semmi felesleges.",
      why_desc: "termékünk az élvezetes és sima játékélményre összpontosít, amit minden funkció tökélyre csiszolásával értünk el.",
      spotlight_heading: 'a legsimaabb bunnyhop, amit valaha láttál.<br><span class="text-muted">ahogy az összes mozgásfunkció ^^</span>',
      testimonial_quote: '"hihetetlenül sima szoftver, folyamatosan adta az edgebugokat premier meccsen miközben zenét hallgattam a dynamic islanden. olyan sima mint a vaj. simán 10/10, ajánlom."',
      author_role: "CS2 & OSU tartalomgyártó · 100k követő",
      btn_buy_product: "Termék Megvétele",

      // Stats
      stat1_sub: "és minden egyes funkció tökéletesen finomhangolva!",
      stat2_sub: "nulla fps esés — a játékélményed makulátlan marad",
      stat3_sub: "aktív 24/7 webes segítségnyújtás gyors válaszidővel x_o",

      // Dynamic Island
      island_desc: 'zenelejátszás, értesítések, c4 bomba időzítő, beérkező gránátok, ellenfél felszerelés — minden egy elegáns lebegő kapszulában a képernyő tetején. <span class="text-faint">pont mint a telefonodon! ^_^</span>',
      island_hint: "Interaktív Dynamic Island szimulátor — válassz kategóriát a jobb oldalon",
      tab_media_title: "média",
      tab_media_desc: "kompatibilis: Spotify, Apple Music, YouTube, SoundCloud — szám adatai a kapszulában! ♪(´▽｀)♪",
      tab_notif_title: "értesítések",
      tab_notif_desc: "rendszer riasztások: hangerő, fényerő, ping és szerverkapcsolat. (｡◕‿◕｡)",
      tab_bomb_title: "bomba időzítő",
      tab_bomb_desc: "lerakott C4 visszaszámlálás és ezredmásodperc pontosságú defuse jelző. (；´o`)",
      tab_grenade_title: "gránát riasztás",
      tab_grenade_desc: "azonnali figyelmeztetés, ha vaku, füst vagy molotov repül feléd! Σ(°ロ°)",
      tab_loadout_title: "felszerelés esp",
      tab_loadout_desc: "ellenfelek főfegyverei, hatástalanító készlet és C4 hordozó egy pillantásra. (｀・ω・´) gyors áttekintés.",

      // Inside Features
      inside_title: "mi van a spring-ben",
      inside_desc: "húzd az egeret bármelyik ikon fölé — minden egyes modul animált és egyedi.",
      f1_title: "subtick mozgás",
      f1_desc: "bhop, edge-bug, jump-bug és pixel-surf akadásmentesen működik az összes szerveren!",
      f2_title: "finomhangolt aimbot",
      f2_desc: "emberi simítás, magozott triggerbot, önálló RCS visszarúgás vezérlés. (◕‿◕)",
      f3_title: "hud testreszabás",
      f3_desc: "több stílusos téma, teljes RGB választó és letisztult glassmorphism felület.",
      f4_title: "felhős beállítások",
      f4_desc: "a konfigurációid azonnal szinkronizálódnak az eszközeid között megosztható kódokkal.",
      f5_title: "natív teljesítmény",
      f5_desc: "optimalizált natív kód 2% alatti CPU és GPU terheléssel!",

      // Pricing
      pricing_tag: "Árazás",
      pricing_title: "átlátható, elérhető csomagok.",
      pricing_desc: "egy teljes körű termék, négy időtartam. nincsenek rejtett költségek, zárolt funkciók vagy automatikus levonások.",
      included_title: "MIT TARTALMAZ",
      inc_1: "minden funkció feloldva",
      inc_2: "összes játékfrissítés automatikusan mellékelve",
      inc_3: "kiemelt 24/7 webes támogatás",
      inc_4: "1 licenc fagyasztási lehetőség",
      inc_5: "1 automatikus HWID visszaállítás",
      hwid_note: "további HWID visszaállítások a supporton keresztül érhetők el",

      // Panel Translations
      p_nav_home: "Vissza a Főoldalra",
      p_tab_overview: "Áttekintés",
      p_tab_keys: "Kulcs Aktiválás",
      p_tab_configs: "Felhős Konfigurációk",
      p_tab_hwid: "HWID Kezelés",
      p_tab_support: "Támogatás & Jegyek",
      p_welcome_title: "Üdv újra, LO!",
      p_welcome_desc: "Kezeld aktív előfizetésedet, felhős konfigurációidat és kliens beállításaidat.",
      p_btn_download: "Spring Loader Letöltése (.exe)",
      p_stat_sub_title: "Előfizetés Állapota",
      p_stat_hwid_title: "HWID Zárolás",
      p_stat_cfg_title: "Felhős Konfigurációk",
      p_stat_sync_on: "Automatikus szinkronizáció aktív",
      p_stat_ver_title: "Szoftver Verzió",
      p_stat_up_to_date: "Naprakész és biztonságos",
      p_news_title: "Legfrissebb Update: Subtick Movement v2.4",
      p_news_date: "Tegnap, 18:40",
      p_news_body: "Frissítettük a bunnyhop és edgebug motort a legújabb CS2 javításokhoz! A Dynamic Island szinkronizáció és azonnali konfig újratöltés már éles.",
      p_keys_heading: "Licenckulcs Aktiválása",
      p_keys_sub: "Add meg a vásárláskor kapott 16 jegyű kódodat vagy előfizetési kulcsodat.",
      p_key_label: "Licenckulcs",
      p_key_hint: "Példa: <code>SPRING-30DAY-2026-X89F</code>",
      p_btn_activate: "Kulcs Aktiválása",
      p_cfg_heading: "Felhő Konfiguráció Menedzser",
      p_cfg_sub: "Hozz létre, ossz meg és tölts be beállításokat közvetlenül a játékba újraindítás nélkül.",
      p_btn_create_cfg: "Új Konfiguráció Létrehozása",
      p_hwid_heading: "Hardverazonosító (HWID) Kezelés",
      p_hwid_sub: "A géphez kötés az előfizetésed és fiókod védelmére szolgál.",
      p_hwid_current_title: "Jelenlegi regisztrált HWID:",
      p_hwid_explanation: "Rendelkezésedre áll <strong>1 ingyenes automata HWID visszaállítás</strong> gépváltás vagy alkatrészcsere esetén.",
      p_btn_reset_hwid: "HWID Zárolás Törlése",
      p_support_heading: "Ügyfélszolgálat",
      p_support_sub: "Segítségre van szükséged az injektálással vagy a beállításokkal? 24/7 elérhetőek vagyunk.",
      p_sup_ticket_val: "Élő Online Ügyfélszolgálat",
      p_sup_ticket_sub: "Automatikus hibaelhárítás és hibajegyek",
      p_sup_faq_sub: "Telepítési és VAC megkerülési útmutatók"
    }
  };

  // Get persisted or default language
  let currentLang = localStorage.getItem('spring_lang') || 'en';
  if (currentLang !== 'en' && currentLang !== 'hu') currentLang = 'en';

  // Common language apply function
  function applyLanguage(lang) {
    currentLang = lang;
    localStorage.setItem('spring_lang', lang);

    const currentLangEl = document.getElementById('current-lang-text');
    if (currentLangEl) currentLangEl.textContent = lang.toUpperCase();

    document.querySelectorAll('.lang-option').forEach(o => {
      o.classList.toggle('active', o.dataset.lang === lang);
    });

    const dict = TRANSLATIONS[lang];
    if (!dict) return;

    document.querySelectorAll('[data-i18n]').forEach(el => {
      const key = el.dataset.i18n;
      if (!dict[key]) return;
      if (HTML_KEYS.has(key)) {
        el.innerHTML = dict[key];
      } else {
        el.textContent = dict[key];
      }
    });

    // Notify respective page logic
    if (window.onLangChanged) window.onLangChanged(lang);
  }

  // Setup Language Dropdown in header
  function setupLangDropdown() {
    const langBtn = document.getElementById('lang-btn');
    const langMenu = document.getElementById('lang-menu');
    if (langBtn) {
      langBtn.addEventListener('click', e => {
        e.stopPropagation();
        if (langMenu) langMenu.classList.toggle('hidden');
      });
    }
    document.addEventListener('click', () => {
      if (langMenu) langMenu.classList.add('hidden');
    });
    document.querySelectorAll('.lang-option').forEach(opt => {
      opt.addEventListener('click', e => {
        e.stopPropagation();
        applyLanguage(opt.dataset.lang);
        if (langMenu) langMenu.classList.add('hidden');
      });
    });
  }

  setupLangDropdown();

  // ============================================================
  // PAGE ROUTING & INITIALIZATION
  // ============================================================
  const isPanel = document.body.classList.contains('panel-body');
  if (!isPanel) {
    initLandingPage();
  } else {
    initPanelPage();
  }

  // Apply current language at startup
  applyLanguage(currentLang);
  initLucideIcons();

  // ============================================================
  // LANDING PAGE LOGIC
  // ============================================================
  function initLandingPage() {
    let selectedTier = '30';

    const COMBAT_PREVIEWS = {
      aimbot: {
        title: '🎯 Subtick Aimbot & FOV Engine',
        body: `
          <div class="preview-metric"><span class="metric-label">Targeting Bone:</span><span class="metric-val text-cyan">Head / Neck (Bone #6)</span></div>
          <div class="preview-metric"><span class="metric-label">FOV Cone:</span><span class="metric-val text-lime">4.5° (Visible Overlay Ring)</span></div>
          <div class="preview-metric"><span class="metric-label">Curve Smoothness:</span><span class="metric-val text-amber">3.5x Humanized</span></div>
          <div class="preview-metric"><span class="metric-label">Hit-Check:</span><span class="metric-val text-lime">Raycast Visible Only</span></div>
        `
      },
      rcs: {
        title: '⚡ Independent Pitch & Yaw RCS',
        body: `
          <div class="preview-metric"><span class="metric-label">Vertical Pitch RCS:</span><span class="metric-val text-rose">75% Compensation</span></div>
          <div class="preview-metric"><span class="metric-label">Horizontal Yaw RCS:</span><span class="metric-val text-cyan">65% Compensation</span></div>
          <div class="preview-metric"><span class="metric-label">Recoil Smooth:</span><span class="metric-val text-lime">2.5x Decay Curve</span></div>
        `
      },
      skeleton: {
        title: '☠️ 3D Skeleton & Box ESP',
        body: `
          <div class="preview-metric"><span class="metric-label">Skeleton Hierarchy:</span><span class="metric-val text-lime">19 Bone Connectors</span></div>
          <div class="preview-metric"><span class="metric-label">Health Bar:</span><span class="metric-val text-lime">Dynamic Color-Coded</span></div>
          <div class="preview-metric"><span class="metric-label">Weapon Icons:</span><span class="metric-val text-cyan">AK-47 / AWP / C4</span></div>
          <div class="preview-metric"><span class="metric-label">Target Snaplines:</span><span class="metric-val text-amber">Bottom Screen Anchor</span></div>
        `
      },
      bomb: {
        title: '💣 C4 Bomb & Defuse HUD',
        body: `
          <div class="preview-metric"><span class="metric-label">Planted Site:</span><span class="metric-val text-rose">B Site (Active Countdown)</span></div>
          <div class="preview-metric"><span class="metric-label">Bomb Timer:</span><span class="metric-val text-amber">34.2s Remaining</span></div>
          <div class="preview-metric"><span class="metric-label">Defuse Check:</span><span class="metric-val text-lime">5.0s Defuse Kit OK</span></div>
        `
      },
      grenades: {
        title: '🎯 Grenade Lineup & Trajectory',
        body: `
          <div class="preview-metric"><span class="metric-label">Physics Model:</span><span class="metric-val text-cyan">CS2 Parabolic Arc</span></div>
          <div class="preview-metric"><span class="metric-label">Jump-Throw Indicator:</span><span class="metric-val text-lime">Subtick Auto-Release</span></div>
          <div class="preview-metric"><span class="metric-label">Proximity Alert:</span><span class="metric-val text-rose">Molotov / Flashbang Active</span></div>
        `
      }
    };

    }

    // Dynamic island tab clicks
    document.querySelectorAll('.island-tab-item').forEach(item => {
      item.addEventListener('click', () => {
        document.querySelectorAll('.island-tab-item').forEach(i => i.classList.remove('active'));
        item.classList.add('active');
        updateDynamicIsland(item.dataset.mode);
      });
    });

    // Tier pricing selector
    const tierRows = document.querySelectorAll('.tier-row');
    const selLabel = document.getElementById('selected-tier-label');
    const selPrice = document.getElementById('selected-tier-price');
    const selRate  = document.getElementById('selected-tier-rate');
    const buyBtnText = document.getElementById('buy-btn-text');
    const buyActionBtn = document.getElementById('buy-action-btn');

    function updateTierDisplay() {
      const row = document.querySelector(`.tier-row[data-tier="${selectedTier}"]`);
      if (!row) return;
      const price = row.dataset.price;
      const perday = row.dataset.perday;

      tierRows.forEach(r => {
        const bar = r.querySelector('.active-indicator-bar');
        if (r.dataset.tier === selectedTier) {
          r.classList.add('active');
          if (!bar) r.insertAdjacentHTML('afterbegin', '<span class="active-indicator-bar"></span>');
        } else {
          r.classList.remove('active');
          if (bar) bar.remove();
        }
      });

      const dayWord = currentLang === 'hu' ? 'nap' : 'days';
      const perLabel = currentLang === 'hu' ? 'nap, egyszeri díj' : 'day, one-time';
      const buyWord = currentLang === 'hu' ? 'vásárlás:' : 'purchase';

      if (selLabel) selLabel.textContent = `${selectedTier} ${dayWord}`;
      if (selPrice) selPrice.textContent = `$${price}`;
      if (selRate)  selRate.textContent  = `~$${perday} / ${perLabel}`;
      if (buyBtnText) buyBtnText.textContent = `${buyWord} ${selectedTier} ${dayWord}`;
    }

    tierRows.forEach(row => {
      row.addEventListener('click', () => {
        selectedTier = row.dataset.tier;
        updateTierDisplay();
      });
    });

    // --- Interactive Checkout Modal Logic ---
    const checkoutModal = document.getElementById('checkout-modal');
    const closeModalBtn = document.getElementById('close-modal-btn');
    const modalTierTitle = document.getElementById('modal-tier-title');
    const modalTierDetails = document.getElementById('modal-tier-details');
    const modalGenKey = document.getElementById('modal-generated-key');
    const copyModalKeyBtn = document.getElementById('copy-modal-key-btn');
    const activateNowBtn = document.getElementById('activate-now-btn');
    const paypalContainer = document.getElementById('paypal-button-container');
    const licenseResultBox = document.getElementById('license-result-box');

    function generateKey(tier) {
      const chars = '0123456789ABCDEF';
      const randSeg = Array.from({ length: 4 }, () => chars[Math.floor(Math.random() * chars.length)]).join('');
      return `SPRING-${tier}DAY-2026-${randSeg}`;
    }

    function renderPayPalButtons(tierDays, priceAmount) {
      if (!window.paypal || !paypalContainer) return;
      paypalContainer.innerHTML = ''; // Clear previous button instance

      window.paypal.Buttons({
        style: {
          layout: 'vertical',
          color: 'gold',
          shape: 'rect',
          label: 'pay'
        },
        createOrder: (data, actions) => {
          return actions.order.create({
            purchase_units: [{
              description: `Spring CS2 Client - ${tierDays} Days Access`,
              amount: {
                currency_code: 'USD',
                value: priceAmount
              }
            }]
          });
        },
        onApprove: (data, actions) => {
          return actions.order.capture().then((details) => {
            const isHu = currentLang === 'hu';
            const newKey = generateKey(tierDays);

            // Grant active subscription
            const addedDays = parseInt(tierDays) || 30;
            const curExp = localStorage.getItem('spring_expiry');
            const baseDate = (curExp && new Date(curExp) > new Date()) ? new Date(curExp) : new Date();
            const newExp = new Date(baseDate.getTime() + addedDays * 24 * 60 * 60 * 1000).toISOString();
            localStorage.setItem('spring_expiry', newExp);
            localStorage.setItem('spring_sub_active', 'true');

            // Show License key display
            if (modalGenKey) modalGenKey.textContent = newKey;
            if (licenseResultBox) licenseResultBox.classList.remove('hidden');
            if (activateNowBtn) activateNowBtn.classList.remove('hidden');
            if (paypalContainer) paypalContainer.classList.add('hidden');

            showToast(isHu ? '🎉 Fizetés sikeres! Licenc aktiválva és letöltés megkezdődött.' : '🎉 Payment Successful! License unlocked & download started.', 'success');

            // Trigger instant download of SpringLoader.exe
            setTimeout(() => {
              const link = document.createElement('a');
              link.href = 'SpringLoader.exe';
              link.download = 'SpringLoader.exe';
              document.body.appendChild(link);
              link.click();
              document.body.removeChild(link);
            }, 1000);
          });
        },
        onError: (err) => {
          showToast(currentLang === 'hu' ? '❌ PayPal fizetési hiba történt.' : '❌ PayPal transaction failed. Please try again.', 'error');
        }
      }).render('#paypal-button-container');
    }

    if (buyActionBtn && checkoutModal) {
      buyActionBtn.addEventListener('click', () => {
        const row = document.querySelector(`.tier-row[data-tier="${selectedTier}"]`);
        const price = row ? row.dataset.price : '7.99';
        const isHu = currentLang === 'hu';

        if (modalTierTitle) {
          modalTierTitle.textContent = isHu 
            ? `Előfizetés vásárlása: ${selectedTier} Nap` 
            : `PayPal Subscription: ${selectedTier} Days`;
        }
        if (modalTierDetails) {
          modalTierDetails.textContent = isHu
            ? `${selectedTier} Nap Korlátlan Hozzáférés • $${price}`
            : `${selectedTier} Days Unlimited Access • $${price}`;
        }

        if (licenseResultBox) licenseResultBox.classList.add('hidden');
        if (activateNowBtn) activateNowBtn.classList.add('hidden');
        if (paypalContainer) paypalContainer.classList.remove('hidden');

        renderPayPalButtons(selectedTier, price);
        checkoutModal.classList.remove('hidden');
        initLucideIcons();
      });
    }

    if (closeModalBtn && checkoutModal) {
      closeModalBtn.addEventListener('click', () => {
        checkoutModal.classList.add('hidden');
      });
      checkoutModal.addEventListener('click', e => {
        if (e.target === checkoutModal) checkoutModal.classList.add('hidden');
      });
    }

    if (copyModalKeyBtn && modalGenKey) {
      copyModalKeyBtn.addEventListener('click', () => {
        const key = modalGenKey.textContent.trim();
        navigator.clipboard.writeText(key).then(() => {
          showToast(currentLang === 'hu' ? `📋 Kulcs vágólapra másolva: ${key}` : `📋 Key copied: ${key}`);
          copyModalKeyBtn.innerHTML = '<i data-lucide="check" class="text-lime"></i>';
          initLucideIcons();
          setTimeout(() => {
            copyModalKeyBtn.innerHTML = '<i data-lucide="copy"></i>';
            initLucideIcons();
          }, 2000);
        }).catch(() => {});
      });
    }

    if (activateNowBtn) {
      activateNowBtn.addEventListener('click', () => {
        const link = document.createElement('a');
        link.href = 'SpringLoader.exe';
        link.download = 'SpringLoader.exe';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
      });
    }

    }

    window.onLangChanged = () => {
      updateDynamicIsland();
      updateTierDisplay();
    };

    updateDynamicIsland('media');
    updateTierDisplay();
  }

  // ============================================================
  // PANEL PAGE LOGIC (English & Hungarian)
  // ============================================================
  function initPanelPage() {
    let hwidResets = parseInt(localStorage.getItem('spring_hwid_resets') || '1');
    let configs = JSON.parse(localStorage.getItem('spring_configs') || 'null') || [
      {
        id: 1,
        name: 'Legit Movement & Bhop v2.1',
        icon: 'file-code',
        color: 'text-lime',
        descEn: 'Optimal settings for silent bunnyhop and buttery smooth aim-assist smoothing.',
        descHu: 'Optimális beállítások a csendes bunnyhophoz és a finom célzásrásegítéshez.',
        downloads: 1420,
        badge: 'DEFAULT',
        code: 'CFG-SPRING-LEGIT-881',
        timeEn: '2 days ago',
        timeHu: '2 napja',
        active: true
      },
      {
        id: 2,
        name: 'EdgeBug Pro Streamer Pack',
        icon: 'zap',
        color: 'text-cyan',
        descEn: 'Config with automatic sound alerts on Dynamic Island and maximum edge-bug rate.',
        descHu: 'Konfig automatikus Dynamic Island hangértesítéssel és maximális edge-bug rátával.',
        downloads: 890,
        badge: 'PRO',
        code: 'CFG-SPRING-EDGE-404',
        timeEn: '5 days ago',
        timeHu: '5 napja',
        active: false
      },
      {
        id: 3,
        name: 'Rage HVH Semi-Rage Setup',
        icon: 'shield-alert',
        color: 'text-rose',
        descEn: 'Aggressive triggerbot hitboxes and standalone pitch-yaw RCS for HVH servers.',
        descHu: 'Agresszív triggerbot és önálló RCS visszarúgás szabályozás zárt szerverekre.',
        downloads: 2100,
        badge: 'HVH',
        code: 'CFG-SPRING-RAGE-999',
        timeEn: '1 week ago',
        timeHu: '1 hete',
        active: false
      }
    ];

    function saveConfigs() {
      localStorage.setItem('spring_configs', JSON.stringify(configs));
      const countEl = document.getElementById('configs-count-stat');
      if (countEl) countEl.textContent = `${configs.length} / 10 Slots`;
    }

    // --- Tab routing ---
    const sidebarItems = document.querySelectorAll('.sidebar-item');
    const tabPages = document.querySelectorAll('.panel-tab-page');

    function navigateTo(tabId) {
      if (tabId === 'download') tabId = 'overview';
      const validTabs = ['overview', 'keys', 'configs', 'hwid', 'support'];
      if (!validTabs.includes(tabId)) tabId = 'overview';

      sidebarItems.forEach(i => i.classList.toggle('active', i.dataset.tab === tabId));
      tabPages.forEach(p => {
        const show = p.id === `tab-${tabId}`;
        p.classList.toggle('active', show);
        p.classList.toggle('hidden', !show);
      });
      if (tabId === 'configs') renderConfigs();
      initLucideIcons();
    }

    // Web Audio Synthesizer Audition Helper
    window.playAudioPreview = function(type) {
      try {
        const AudioCtx = window.AudioContext || window.webkitAudioContext;
        if (!AudioCtx) return;
        const ctx = new AudioCtx();
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();

        osc.connect(gain);
        gain.connect(ctx.destination);

        if (type === 'valve_bell') {
          osc.type = 'sine';
          osc.frequency.setValueAtTime(1400, ctx.currentTime);
          osc.frequency.exponentialRampToValueAtTime(300, ctx.currentTime + 0.15);
          gain.gain.setValueAtTime(0.3, ctx.currentTime);
          gain.gain.exponentialRampToValueAtTime(0.01, ctx.currentTime + 0.15);
          osc.start(ctx.currentTime);
          osc.stop(ctx.currentTime + 0.15);
          showToast('♪ Playing Valve Metallic Bell Soundpack Preview', 'info');
        } else if (type === 'rust_headshot') {
          osc.type = 'triangle';
          osc.frequency.setValueAtTime(800, ctx.currentTime);
          osc.frequency.exponentialRampToValueAtTime(120, ctx.currentTime + 0.12);
          gain.gain.setValueAtTime(0.5, ctx.currentTime);
          gain.gain.exponentialRampToValueAtTime(0.01, ctx.currentTime + 0.12);
          osc.start(ctx.currentTime);
          osc.stop(ctx.currentTime + 0.12);
          showToast('♪ Playing Rust Crunch Headshot Soundpack Preview', 'info');
        } else if (type === 'bubble_pop') {
          osc.type = 'sine';
          osc.frequency.setValueAtTime(400, ctx.currentTime);
          osc.frequency.exponentialRampToValueAtTime(900, ctx.currentTime + 0.08);
          gain.gain.setValueAtTime(0.2, ctx.currentTime);
          gain.gain.exponentialRampToValueAtTime(0.01, ctx.currentTime + 0.08);
          osc.start(ctx.currentTime);
          osc.stop(ctx.currentTime + 0.08);
          showToast('♪ Playing Soft Bubble Pop Soundpack Preview', 'info');
        }
      } catch (err) {}
    };

    sidebarItems.forEach(item => {
      item.addEventListener('click', () => {
        const tab = item.dataset.tab;
        window.location.hash = tab;
        navigateTo(tab);
        if (window.innerWidth < 850) {
          document.querySelector('.panel-main-content')?.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
      });
    });

    // Check hash on load
    const initialHash = window.location.hash.replace('#', '') || 'overview';
    navigateTo(initialHash);

    // Auto-fill pending key if directed from checkout
    const pendingKey = sessionStorage.getItem('pending_spring_key');
    if (pendingKey) {
      navigateTo('keys');
      const keyInp = document.getElementById('license-key-input');
      if (keyInp) {
        keyInp.value = pendingKey;
        sessionStorage.removeItem('pending_spring_key');
        showToast(currentLang === 'hu' ? '✨ Licenckulcs automatikusan beillesztve! Kattints az Aktiválásra.' : '✨ License token pre-filled! Click Activate to continue.');
      }
    }

    // --- Download button ---
    const dlBtn = document.getElementById('download-client-btn');
    if (dlBtn) {
      dlBtn.addEventListener('click', () => {
        const isHu = currentLang === 'hu';
        // Software locked until full launch phase is completed
        showToast(isHu ? '🔒 A szoftver letöltése zárva van a végső kiadás befejezéséig! Kérjük várj a hivatalos indulásra.' : '🔒 Software downloads are locked until the full release launch is complete! Stay tuned.', 'info');
        return;

        const orig = dlBtn.innerHTML;
        dlBtn.disabled = true;

        const steps = isHu ? [
          [0,    '<i data-lucide="loader-2"></i> <span>Letöltés előkészítése...</span>'],
          [800,  '<i data-lucide="shield-check"></i> <span>Licenc ellenőrzése...</span>'],
          [1600, '<i data-lucide="server"></i> <span>Kapcsolódás a szerverhez...</span>'],
          [2400, '<i data-lucide="download"></i> <span>SpringLoader.exe letöltése...</span>'],
        ] : [
          [0,    '<i data-lucide="loader-2"></i> <span>Preparing download package...</span>'],
          [800,  '<i data-lucide="shield-check"></i> <span>Validating active license...</span>'],
          [1600, '<i data-lucide="server"></i> <span>Establishing encrypted tunnel...</span>'],
          [2400, '<i data-lucide="download"></i> <span>Downloading SpringLoader.exe...</span>'],
        ];

        steps.forEach(([delay, html]) => {
          setTimeout(() => { dlBtn.innerHTML = html; initLucideIcons(); }, delay);
        });

        setTimeout(() => {
          dlBtn.innerHTML = orig;
          dlBtn.disabled = false;
          initLucideIcons();

          // Direct download of the standalone native loader
          const link = document.createElement('a');
          link.href = 'SpringLoader.exe';
          link.download = 'SpringLoader.exe';
          document.body.appendChild(link);
          link.click();
          document.body.removeChild(link);

          const successMsg = isHu
            ? '✅ SpringLoader.exe sikeresen letöltve! Indítsd közvetlenül a játék előtt.'
            : '✅ SpringLoader.exe downloaded! Run directly before starting CS2.';
          showToast(successMsg, 'success');
        }, 3200);
      });
    }

    // --- Key activation form ---
    const keyForm = document.getElementById('key-form');
    if (keyForm) {
      keyForm.addEventListener('submit', e => {
        e.preventDefault();
        const inp = document.getElementById('license-key-input');
        const val = inp.value.trim().toUpperCase();
        const isHu = currentLang === 'hu';

        if (!val || val.length < 10) {
          showToast(isHu ? '❌ Érvénytelen kulcs formátum.' : '❌ Please enter a valid license token.', 'error');
          return;
        }

        const btn = keyForm.querySelector('button[type="submit"]');
        btn.disabled = true;
        btn.innerHTML = `<i data-lucide="loader-2"></i> <span>${isHu ? 'Kulcs hitelesítése...' : 'Verifying license token...'}</span>`;
        initLucideIcons();

        setTimeout(() => {
          if (val.startsWith('SPRING-') || val.length >= 16) {
            // Determine days from key
            let addedDays = 30;
            if (val.includes('14DAY')) addedDays = 14;
            else if (val.includes('90DAY')) addedDays = 90;
            else if (val.includes('180DAY')) addedDays = 180;

            // Extend subscription
            const curExp = localStorage.getItem('spring_expiry');
            const baseDate = curExp ? new Date(curExp) : new Date();
            const newExp = new Date(baseDate.getTime() + addedDays * 24 * 60 * 60 * 1000).toISOString();
            localStorage.setItem('spring_expiry', newExp);

            updateSubCountdown();
            inp.value = '';
            showToast(isHu ? `🎉 Sikeres aktiválás! +${addedDays} nap hozzáadva.` : `🎉 Key activated! +${addedDays} days added to your account.`);
            navigateTo('overview');
          } else {
            showToast(isHu ? '❌ A megadott kulcs nem található vagy lejárt.' : '❌ Invalid or expired license token.', 'error');
          }
          btn.disabled = false;
          btn.innerHTML = `<i data-lucide="check-circle-2"></i> <span data-i18n="p_btn_activate">${isHu ? 'Kulcs Aktiválása' : 'Activate License'}</span>`;
          initLucideIcons();
        }, 1500);
      });
    }

    // --- Cloud configs renderer ---
    function renderConfigs() {
      const grid = document.getElementById('configs-list-grid');
      if (!grid) return;
      const isHu = currentLang === 'hu';

      grid.innerHTML = configs.map(cfg => {
        const desc = isHu ? (cfg.descHu || cfg.descEn || cfg.desc) : (cfg.descEn || cfg.descHu || cfg.desc);
        const time = isHu ? (cfg.timeHu || '2 napja') : (cfg.timeEn || '2 days ago');
        const dls = isHu ? 'Letöltés' : 'Downloads';
        const loadedText = isHu ? 'Betöltve' : 'Loaded';
        const loadText = isHu ? 'Betöltés' : 'Load';

        return `
          <div class="config-card glass-panel" data-config-id="${cfg.id}">
            <div class="config-card-header">
              <div class="config-title-group">
                <i data-lucide="${cfg.icon}" class="${cfg.color}"></i>
                <h4 class="config-name">${cfg.name}</h4>
              </div>
              <span class="badge-tag">${cfg.badge}</span>
            </div>
            <p class="config-desc">${desc}</p>
            <div class="config-meta">
              <span><i data-lucide="download" class="icon-xs"></i> ${cfg.downloads.toLocaleString()} ${dls}</span>
              <span><i data-lucide="clock" class="icon-xs"></i> ${time}</span>
            </div>
            <div class="config-actions">
              <button class="btn-sm primary copy-config-btn" data-code="${cfg.code}">
                <i data-lucide="share-2"></i> ${cfg.code.split('-').pop()}
              </button>
              <button class="btn-sm secondary load-config-btn" data-id="${cfg.id}">
                ${cfg.active ? `<i data-lucide="check"></i> ${loadedText}` : `<i data-lucide="play"></i> ${loadText}`}
              </button>
              <button class="btn-sm secondary delete-config-btn" data-id="${cfg.id}" style="margin-left:auto;color:var(--accent-rose)" title="Delete">
                <i data-lucide="trash-2"></i>
              </button>
            </div>
          </div>`;
      }).join('');

      initLucideIcons();
    }

    // --- Create config modal logic ---
    const createCfgBtn = document.getElementById('create-config-btn');
    const createCfgModal = document.getElementById('create-config-modal');
    const closeCfgModalBtn = document.getElementById('close-cfg-modal-btn');
    const newCfgForm = document.getElementById('new-config-form');

    if (createCfgBtn && createCfgModal) {
      createCfgBtn.addEventListener('click', () => {
        createCfgModal.classList.remove('hidden');
        initLucideIcons();
      });
    }

    if (closeCfgModalBtn && createCfgModal) {
      closeCfgModalBtn.addEventListener('click', () => {
        createCfgModal.classList.add('hidden');
      });
      createCfgModal.addEventListener('click', e => {
        if (e.target === createCfgModal) createCfgModal.classList.add('hidden');
      });
    }

    if (newCfgForm && createCfgModal) {
      newCfgForm.addEventListener('submit', e => {
        e.preventDefault();
        const nameInput = document.getElementById('new-cfg-name-input');
        const styleSelect = document.getElementById('new-cfg-style-select');
        const descInput = document.getElementById('new-cfg-desc-input');

        const name = nameInput.value.trim();
        if (!name) return;

        const badge = styleSelect ? styleSelect.value : 'CUSTOM';
        const desc = descInput ? descInput.value.trim() : '';

        const iconMap = {
          LEGIT: { icon: 'file-code', color: 'text-lime' },
          PRO: { icon: 'zap', color: 'text-cyan' },
          HVH: { icon: 'shield-alert', color: 'text-rose' },
          CUSTOM: { icon: 'sliders-horizontal', color: 'text-amber' }
        };
        const meta = iconMap[badge] || iconMap.CUSTOM;

        const newId = Date.now();
        configs.unshift({
          id: newId,
          name: name,
          icon: meta.icon,
          color: meta.color,
          descEn: desc || 'Custom user configuration. Created from web panel.',
          descHu: desc || 'Egyéni felhasználói konfig. Létrehozva a webpanelből.',
          downloads: 0,
          badge: badge,
          code: `CFG-SPRING-${String(newId).slice(-4)}`,
          timeEn: 'Just now',
          timeHu: 'Most',
          active: false
        });

        saveConfigs();
        renderConfigs();

        nameInput.value = '';
        if (descInput) descInput.value = '';
        createCfgModal.classList.add('hidden');

        const isHu = currentLang === 'hu';
        showToast(isHu ? `✨ Új konfiguráció mentve: "${name}"` : `✨ Preset created & synced: "${name}"`);
      });
    }

    // Config actions event delegation
    const configsGrid = document.getElementById('configs-list-grid');
    if (configsGrid) {
      configsGrid.addEventListener('click', e => {
        const copyBtn = e.target.closest('.copy-config-btn');
        const loadBtn = e.target.closest('.load-config-btn');
        const delBtn  = e.target.closest('.delete-config-btn');
        const isHu = currentLang === 'hu';

        if (copyBtn) {
          navigator.clipboard.writeText(copyBtn.dataset.code).catch(() => {});
          showToast(isHu ? `📋 Kód kimásolva: ${copyBtn.dataset.code}` : `📋 Code copied: ${copyBtn.dataset.code}`);
        }

        if (loadBtn) {
          const id = parseInt(loadBtn.dataset.id, 10);
          configs.forEach(c => c.active = (c.id === id));
          const target = configs.find(c => c.id === id);
          if (target) target.downloads++;
          saveConfigs();
          renderConfigs();
          showToast(isHu ? '⚡ Konfiguráció betöltve és szinkronizálva!' : '⚡ Config loaded and synchronized with game!');
        }

        if (delBtn) {
          const id = parseInt(delBtn.dataset.id, 10);
          const confirmMsg = isHu ? 'Biztosan törlöd ezt a konfigurációt?' : 'Delete this configuration preset?';
          if (confirm(confirmMsg)) {
            configs = configs.filter(c => c.id !== id);
            saveConfigs();
            renderConfigs();
            showToast(isHu ? '🗑️ Konfiguráció törölve.' : '🗑️ Config removed.', 'info');
          }
        }
      });
    }

    // --- Interactive Support Ticket Form ---
    const ticketForm = document.getElementById('support-ticket-form');
    if (ticketForm) {
      ticketForm.addEventListener('submit', e => {
        e.preventDefault();
        const msgInp = document.getElementById('ticket-message-input');
        const catSelect = document.getElementById('ticket-subject-select');
        const btn = document.getElementById('submit-ticket-btn');
        const isHu = currentLang === 'hu';

        if (!msgInp || !msgInp.value.trim()) return;

        btn.disabled = true;
        btn.innerHTML = `<i data-lucide="loader-2"></i> <span>${isHu ? 'Hibajegy küldése...' : 'Submitting ticket...'}</span>`;
        initLucideIcons();

        setTimeout(() => {
          const ticketId = 'TICKET-' + Math.floor(1000 + Math.random() * 9000);
          btn.disabled = false;
          btn.innerHTML = `<i data-lucide="send"></i> <span>${isHu ? 'Hibajegy Elküldése' : 'Send Ticket to Helpdesk'}</span>`;
          msgInp.value = '';
          initLucideIcons();

          const toastMsg = isHu
            ? `📨 Sikeres beküldés (#${ticketId})! Automatikus válasz érkezett az ügyfélszolgálattól.`
            : `📨 Ticket created (#${ticketId})! Automated offset resolution dispatched.`;
          showToast(toastMsg, 'success');
        }, 1400);
      });
    }

    // --- HWID Reset ---
    const hwidBtn = document.getElementById('reset-hwid-btn');
    const hwidBtnText = document.getElementById('reset-hwid-btn-text');

    function updateHwidUI() {
      if (!hwidBtn) return;
      const isHu = currentLang === 'hu';

      if (hwidResets <= 0) {
        if (hwidBtnText) hwidBtnText.textContent = isHu ? 'Nincs több visszaállítás (0)' : 'No Resets Remaining (0)';
        hwidBtn.style.opacity = '0.5';
        hwidBtn.style.cursor = 'not-allowed';
      } else {
        if (hwidBtnText) hwidBtnText.textContent = isHu ? `HWID Zárolás Törlése (${hwidResets} elérhető)` : `Reset HWID Lock (${hwidResets} available)`;
        hwidBtn.style.opacity = '1';
        hwidBtn.style.cursor = 'pointer';
      }
      initLucideIcons();
    }

    function genHwid() {
      const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';
      const seg = n => Array.from({ length: n }, () => chars[Math.floor(Math.random() * chars.length)]).join('');
      return `DESKTOP-${seg(5)}-${seg(5)}-SPRING`;
    }

    if (hwidBtn) {
      updateHwidUI();
      hwidBtn.addEventListener('click', () => {
        const isHu = currentLang === 'hu';
        if (hwidResets <= 0) {
          showToast(isHu ? '❌ Nincs több ingyenes HWID törlésed. Nyiss jegyet az Ügyfélszolgálat fülön.' : '❌ No resets left. Open a ticket in the Support tab.', 'error');
          return;
        }

        const confirmMsg = isHu ? 'Biztosan alaphelyzetbe állítod a hardverazonosítót?' : 'Reset your hardware identification lock?';
        if (!confirm(confirmMsg)) return;

        hwidBtn.disabled = true;
        const prevText = hwidBtnText.textContent;
        hwidBtnText.textContent = isHu ? 'Alaphelyzetbe állítás...' : 'Resetting HWID...';

        setTimeout(() => {
          hwidResets = Math.max(0, hwidResets - 1);
          localStorage.setItem('spring_hwid_resets', hwidResets);

          const hwidStr = document.getElementById('current-hwid-str');
          if (hwidStr) hwidStr.textContent = genHwid();

          hwidBtn.disabled = false;
          hwidBtnText.textContent = prevText;
          updateHwidUI();

          showToast(isHu ? '✅ HWID sikeresen törölve! Új hardver csatolható.' : '✅ HWID successfully unbound! Ready for new device.');
        }, 1600);
      });
    }

    // --- Subscription countdown ---
    function updateSubCountdown() {
      const subDaysEl = document.getElementById('sub-days-left');
      const subValEl = document.getElementById('sub-status-val');
      const badgeSub = document.getElementById('badge-sub-status');
      const isHu = currentLang === 'hu';

      let exp = localStorage.getItem('spring_expiry');
      if (!exp) {
        exp = new Date(Date.now() + 24 * 24 * 60 * 60 * 1000).toISOString();
        localStorage.setItem('spring_expiry', exp);
      }

      const expDate = new Date(exp);
      const diffDays = Math.max(0, Math.ceil((expDate - Date.now()) / (1000 * 60 * 60 * 24)));
      const dateStr = expDate.toLocaleDateString(isHu ? 'hu-HU' : 'en-US', { day: '2-digit', month: '2-digit', year: 'numeric' });

      if (subDaysEl) {
        subDaysEl.textContent = isHu
          ? `Hátralévő idő: ${diffDays} nap (${dateStr}-ig)`
          : `Remaining: ${diffDays} days (until ${dateStr})`;
      }
      if (subValEl) {
        subValEl.textContent = isHu ? `${diffDays} Nap Aktív` : `${diffDays} Days Active`;
      }
      if (badgeSub) {
        badgeSub.innerHTML = `<span class="pulse-dot"></span> ${diffDays} ${isHu ? 'NAP AKTÍV' : 'DAYS ACTIVE'}`;
      }
    }

    updateSubCountdown();

    window.onLangChanged = () => {
      renderConfigs();
      updateHwidUI();
      updateSubCountdown();
    };

    renderConfigs();
    saveConfigs();
  }

});
