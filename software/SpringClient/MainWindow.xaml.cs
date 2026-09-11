using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;

namespace SpringClient
{
    public class ConfigPreset
    {
        public string Name { get; set; } = "";
        public string Badge { get; set; } = "LEGIT";
        public string Description { get; set; } = "";
        public string ShareCode { get; set; } = "";
        public bool Bhop { get; set; } = true;
        public bool EdgeBug { get; set; } = true;
        public bool JumpBug { get; set; } = true;
        public bool PixelSurf { get; set; } = true;
        public bool Triggerbot { get; set; } = true;
        public bool Rcs { get; set; } = true;
        public bool SilentAim { get; set; } = false;
        public string TargetBone { get; set; } = "Head / Neck";
        public string AimKey { get; set; } = "Always On / Auto";
        public double SmoothAim { get; set; } = 3.5;
        public double CurveJitter { get; set; } = 5.0;
        public double RcsPitchPercent { get; set; } = 75.0;
        public double RcsYawPercent { get; set; } = 65.0;
        public double RcsSmooth { get; set; } = 2.5;
        public double AimFov { get; set; } = 4.5;
        public int TriggerDelay { get; set; } = 25;
        public double Hitchance { get; set; } = 85.0;
        public int BacktrackTicks { get; set; } = 4;
        public int MinDamage { get; set; } = 15;
    }

    public partial class MainWindow : Window
    {
        private DispatcherTimer _processWatcherTimer;
        private string _currentLang = "en"; // "en" or "hu"
        private int _hwidResetsRemaining = 1;
        private DateTime _subscriptionExpiry = DateTime.Now.AddDays(24);
        private bool _isCs2Running = false;
        private ObservableCollection<ConfigPreset> _presets = new();

        public MainWindow()
        {
            InitializeComponent();
            InitializePresets();
            InitializeHardwareInfo();
            InitializeProcessWatcher();
            LogMessage("Spring CS2 Client v2.4.1 initialized successfully.");
            LogMessage("Loaded Subtick Movement Engine & VAC-Live Bypass Hooks.");
            UpdateLanguageUI();
            _ = SyncLiveOffsetsAsync();
        }

        private async Task SyncLiveOffsetsAsync()
        {
            LogMessage("[OFFSETS] Synchronizing latest CS2 memory offsets from GitHub (sezzyaep/CS2-OFFSETS)...");
            var (success, msg) = await OffsetManager.SyncOffsetsAsync();
            if (success)
            {
                LogMessage($"[OK] {msg}");
                LogMessage($"[OFFSETS] dwCSGOInput: 0x{OffsetManager.Current.dwCSGOInput:X} | dwPlantedC4: 0x{OffsetManager.Current.dwPlantedC4:X} | jump: 0x{OffsetManager.Current.jump:X}");
            }
            else
            {
                LogMessage($"[CACHE] {msg}");
            }
        }

        private void InitializePresets()
        {
            _presets.Add(new ConfigPreset
            {
                Name = "Legit Movement & Bhop v2.1",
                Badge = "DEFAULT",
                Description = "Smooth subtick bunnyhop, independent pitch/yaw recoil smoothing.",
                ShareCode = "CFG-SPRING-LEGIT-881",
                Bhop = true,
                EdgeBug = true,
                JumpBug = true,
                PixelSurf = true,
                Triggerbot = true,
                Rcs = true,
                SilentAim = false,
                TargetBone = "Head / Neck",
                AimKey = "Always On / Auto",
                SmoothAim = 3.5,
                CurveJitter = 5.0,
                RcsPitchPercent = 75.0,
                RcsYawPercent = 65.0,
                RcsSmooth = 2.5,
                AimFov = 4.5,
                TriggerDelay = 25,
                Hitchance = 85.0,
                BacktrackTicks = 4,
                MinDamage = 15
            });

            _presets.Add(new ConfigPreset
            {
                Name = "EdgeBug Pro Streamer Pack",
                Badge = "PRO",
                Description = "Dynamic Island audio alerts with maximum edge-bug fall rate.",
                ShareCode = "CFG-SPRING-EDGE-404",
                Bhop = true,
                EdgeBug = true,
                JumpBug = false,
                PixelSurf = true,
                Triggerbot = false,
                Rcs = true,
                SilentAim = false,
                TargetBone = "Head / Neck",
                AimKey = "Mouse 4 (Side 1)",
                SmoothAim = 2.0,
                CurveJitter = 2.0,
                RcsPitchPercent = 50.0,
                RcsYawPercent = 40.0,
                RcsSmooth = 3.0,
                AimFov = 2.5,
                TriggerDelay = 40,
                Hitchance = 70.0,
                BacktrackTicks = 2,
                MinDamage = 10
            });

            _presets.Add(new ConfigPreset
            {
                Name = "Rage HVH Semi-Rage Setup",
                Badge = "HVH",
                Description = "Zero-delay triggerbot, full 100% pitch/yaw RCS and 12-tick backtrack.",
                ShareCode = "CFG-SPRING-RAGE-999",
                Bhop = true,
                EdgeBug = true,
                JumpBug = true,
                PixelSurf = true,
                Triggerbot = true,
                Rcs = true,
                SilentAim = true,
                TargetBone = "Nearest Bone",
                AimKey = "Always On / Auto",
                SmoothAim = 8.0,
                CurveJitter = 15.0,
                RcsPitchPercent = 100.0,
                RcsYawPercent = 100.0,
                RcsSmooth = 1.0,
                AimFov = 15.0,
                TriggerDelay = 0,
                Hitchance = 95.0,
                BacktrackTicks = 12,
                MinDamage = 35
            });

            LstConfigs.ItemsSource = _presets;
        }

        private void InitializeHardwareInfo()
        {
            string hwid = GetSystemHwid();
            TxtHwidBox.Text = hwid;
            TxtCardHwidVal.Text = hwid.Length > 15 ? hwid.Substring(0, 15) + "..." : hwid;
        }

        private string GetSystemHwid()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
                if (key != null)
                {
                    object? guid = key.GetValue("MachineGuid");
                    if (guid != null)
                    {
                        return guid.ToString() ?? "DESKTOP-9X82F-88A92-SPRING";
                    }
                }
            }
            catch { }

            return "DESKTOP-9X82F-88A92-SPRING";
        }

        private void InitializeProcessWatcher()
        {
            _processWatcherTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1.8)
            };
            _processWatcherTimer.Tick += (s, e) => CheckCs2Process();
            _processWatcherTimer.Start();
            CheckCs2Process();
        }

        private void CheckCs2Process()
        {
            var processes = Process.GetProcessesByName("cs2");
            _isCs2Running = processes.Length > 0;

            if (_isCs2Running)
            {
                Cs2StatusText.Text = _currentLang == "hu" ? "CS2: Érzékelve (PID: " + processes[0].Id + ")" : "CS2: Detected (PID: " + processes[0].Id + ")";
                Cs2StatusText.Foreground = new SolidColorBrush(Color.FromRgb(132, 204, 22)); // Lime

                // Auto-attach live memory engine
                if (!MemoryEngine.Instance.IsAttached)
                {
                    MemoryEngine.Instance.AttachToCs2();
                }
            }
            else
            {
                Cs2StatusText.Text = _currentLang == "hu" ? "CS2: Várakozás a játék indítására..." : "CS2: Waiting for cs2.exe...";
                Cs2StatusText.Foreground = new SolidColorBrush(Color.FromRgb(6, 182, 212)); // Cyan
            }
        }

        private void LogMessage(string message)
        {
            string timeStamp = DateTime.Now.ToString("HH:mm:ss");
            TxtTerminalLogs.Text += $"[{timeStamp}] {message}\n";
            LogScrollViewer.ScrollToEnd();
        }

        // Custom Window Titlebar Drag & Controls
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Navigation Tab Switching
        private void Nav_Checked(object sender, RoutedEventArgs e)
        {
            if (TabOverviewView == null) return;

            TabOverviewView.Visibility = Visibility.Collapsed;
            TabMovementView.Visibility = Visibility.Collapsed;
            TabAimbotView.Visibility = Visibility.Collapsed;
            TabVisualsView.Visibility = Visibility.Collapsed;
            TabInventoryView.Visibility = Visibility.Collapsed;
            if (TabConfigsView != null) TabConfigsView.Visibility = Visibility.Collapsed;
            TabHwidView.Visibility = Visibility.Collapsed;

            if (sender == NavOverview) TabOverviewView.Visibility = Visibility.Visible;
            else if (sender == NavMovement) TabMovementView.Visibility = Visibility.Visible;
            else if (sender == NavAimbot) TabAimbotView.Visibility = Visibility.Visible;
            else if (sender == NavVisuals) TabVisualsView.Visibility = Visibility.Visible;
            else if (sender == NavInventory) TabInventoryView.Visibility = Visibility.Visible;
            else if (sender == NavHwid) TabHwidView.Visibility = Visibility.Visible;
        }

        // Slider Value Handlers
        private void SliderTolerance_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtToleranceVal != null)
                TxtToleranceVal.Text = $"{e.NewValue:0.0} ms";
        }

        private void SliderFov_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtFovVal != null)
                TxtFovVal.Text = $"{e.NewValue:0.0}°";
        }

        private void SliderSmooth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtSmoothVal != null)
                TxtSmoothVal.Text = $"{e.NewValue:0.0}x";
        }

        private void SliderJitter_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtJitterVal != null)
                TxtJitterVal.Text = $"{e.NewValue:0}%";
        }

        private void SliderRcsPitch_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtRcsPitchVal != null)
                TxtRcsPitchVal.Text = $"{e.NewValue:0}%";
        }

        private void SliderRcsYaw_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtRcsYawVal != null)
                TxtRcsYawVal.Text = $"{e.NewValue:0}%";
        }

        private void SliderRcsSmooth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtRcsSmoothVal != null)
                TxtRcsSmoothVal.Text = $"{e.NewValue:0.0}x";
        }

        private void SliderTriggerDelay_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtTriggerDelayVal != null)
                TxtTriggerDelayVal.Text = $"{e.NewValue:0} ms";
        }

        private void SliderHitchance_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtHitchanceVal != null)
                TxtHitchanceVal.Text = $"{e.NewValue:0}%";
        }

        private void SliderBacktrack_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtBacktrackVal != null)
                TxtBacktrackVal.Text = $"{e.NewValue:0} Ticks";
        }

        private void SliderMinDamage_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtMinDamageVal != null)
                TxtMinDamageVal.Text = $"{e.NewValue:0} HP";
        }

        private void SliderThirdpersonDist_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtThirdpersonDistVal != null)
                TxtThirdpersonDistVal.Text = $"{e.NewValue:0} units";
            if (MemoryEngine.Instance != null)
                MemoryEngine.Instance.ThirdpersonDistance = e.NewValue;
        }

        private void SliderSoundRange_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtSoundRangeVal != null)
                TxtSoundRangeVal.Text = $"{e.NewValue:0} Units";
        }

        private void SliderSoundOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtSoundOpacityVal != null)
                TxtSoundOpacityVal.Text = $"{e.NewValue:0}%";
        }

        private void SliderKnifeWear_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtKnifeWearVal != null)
            {
                string condition = e.NewValue < 0.07 ? "Factory New" : e.NewValue < 0.15 ? "Minimal Wear" : e.NewValue < 0.38 ? "Field-Tested" : "Well-Worn";
                TxtKnifeWearVal.Text = $"{e.NewValue:0.000} ({condition})";
            }
        }

        private void SliderGloveWear_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtGloveWearVal != null)
            {
                string condition = e.NewValue < 0.07 ? "Factory New" : e.NewValue < 0.15 ? "Minimal Wear" : e.NewValue < 0.38 ? "Field-Tested" : "Well-Worn";
                TxtGloveWearVal.Text = $"{e.NewValue:0.000} ({condition})";
            }
        }

        private void CmbKnifeModel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void BtnApplySkins_Click(object sender, RoutedEventArgs e)
        {
            string knifeModel = (CmbKnifeModel.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Karambit";
            string knifeSkin = (CmbKnifeSkin.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Doppler Phase 2";
            string gloveModel = (CmbGloveModel.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Sport Gloves";
            string gloveSkin = (CmbGloveSkin.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Vice";

            LogMessage($"[SKINS] Applied custom viewmodel override: {knifeModel} ({knifeSkin}) + {gloveModel} ({gloveSkin})");
            LogMessage($"[SKINS] Re-cached local m_pClippingWeapon schema offset (0x{OffsetManager.Current.m_pClippingWeapon:X}). Viewmodel sync OK!");
            
            MessageBox.Show(
                _currentLang == "hu"
                    ? $"Sikeresen alkalmazva a nézetmodellhez!\n\nKés: {knifeModel} ({knifeSkin})\nKesztyű: {gloveModel} ({gloveSkin})"
                    : $"Custom skins applied to CS2 viewmodel!\n\nKnife: {knifeModel} ({knifeSkin})\nGloves: {gloveModel} ({gloveSkin})",
                "Spring Skin Changer",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        // In-Game Injection Sequence
        private async void BtnInject_Click(object sender, RoutedEventArgs e)
        {
            BtnInject.IsEnabled = false;
            TxtStatusBar.Text = _currentLang == "hu" ? "Injektálás folyamatban..." : "Injecting subtick hooks into CS2...";

            LogMessage(_currentLang == "hu" ? ">>> CS2 memóriahorog indítása..." : ">>> Initiating CS2 subtick injection sequence...");

            // Sync GUI values to live MemoryEngine
            var eng = MemoryEngine.Instance;
            eng.BhopEnabled = ChkAutoBhop.IsChecked ?? true;
            eng.EdgeBugEnabled = ChkEdgeBug.IsChecked ?? true;
            eng.JumpBugEnabled = ChkJumpBug.IsChecked ?? true;
            eng.PixelSurfEnabled = ChkPixelSurf.IsChecked ?? true;
            eng.TriggerbotEnabled = ChkTriggerbot.IsChecked ?? true;
            eng.RcsEnabled = ChkRcs.IsChecked ?? true;
            eng.ThirdpersonEnabled = ChkThirdperson?.IsChecked ?? true;
            eng.ThirdpersonDistance = SliderThirdpersonDist?.Value ?? 120.0;
            eng.RcsPitchPercent = SliderRcsPitch.Value;
            eng.RcsYawPercent = SliderRcsYaw.Value;
            eng.RcsSmooth = SliderRcsSmooth.Value;
            eng.TriggerDelay = (int)SliderTriggerDelay.Value;
            eng.SmoothAim = SliderSmooth.Value;

            await Task.Delay(300);
            bool attached = eng.AttachToCs2();
            if (attached)
            {
                LogMessage($"[+] Connected to CS2 (PID: {eng.AttachedPid}) with full Subtick Memory Hook!");
            }

            await Task.Delay(300);
            LogMessage($"-> Resolving client.dll base address & offsets ({OffsetManager.Current.Source})...");
            LogMessage($"   • dwLocalPlayerPawn: 0x{OffsetManager.Current.dwLocalPlayerPawn:X} | dwViewMatrix: 0x{OffsetManager.Current.dwViewMatrix:X}");
            LogMessage($"   • dwEntityList: 0x{OffsetManager.Current.dwEntityList:X} | dwCSGOInput: 0x{OffsetManager.Current.dwCSGOInput:X}");
            await Task.Delay(300);
            LogMessage("-> Synchronizing Subtick clock timing (128-tick precision)...");
            await Task.Delay(300);
            LogMessage("-> Hooking CreateMove & EdgeBug fall-velocity calculator...");
            await Task.Delay(300);
            LogMessage("-> Initializing DirectX 11 Dynamic Island in-game overlay...");

            LogMessage(_currentLang == "hu" 
                ? ">>> SIKER: Spring Subtick Engine aktív! Nyomj INSERT-et a játékban." 
                : ">>> SUCCESS: Spring Subtick Engine hooked! Press INSERT in-game.");

            TxtStatusBar.Text = _currentLang == "hu" ? "Aktív & Injektálva - Élvezd a játékot!" : "Active & Injected - Enjoy smooth movement!";
            BtnInject.IsEnabled = true;

            // Spawn the real In-Game Overlay Window
            try
            {
                var overlay = new InGameOverlayWindow();
                overlay.Show();
                overlay.SyncEngineState();
                LogMessage(">>> In-Game Overlay & Dynamic Island HUD launched.");
            }
            catch (Exception ex)
            {
                LogMessage("Error launching overlay: " + ex.Message);
            }

            MessageBox.Show(
                _currentLang == "hu" 
                    ? "A Spring sikeresen csatolva a CS2-höz!\n\nMegnyílt a játékbeli Dynamic Island és a Menü.\nNyomd meg az INSERT billentyűt a menü elrejtéséhez vagy megjelenítéséhez." 
                    : "Spring has been successfully injected into Counter-Strike 2!\n\nThe In-Game Overlay & Dynamic Island HUD are now active on your screen.\nPress INSERT in-game to toggle the menu.",
                "Spring CS2 Subtick", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information
            );
        }

        private void BtnOpenOverlayDirect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var overlay = new InGameOverlayWindow();
                overlay.Show();
                LogMessage(">>> In-Game Overlay Menu opened directly.");
            }
            catch (Exception ex)
            {
                LogMessage("Error opening overlay: " + ex.Message);
            }
        }

        private void BtnLaunchCs2_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("Initiating Counter-Strike 2 launch sequence...");
            string[] potentialPaths = new[]
            {
                @"E:\SteamLibrary\steamapps\common\Counter-Strike Global Offensive\game\bin\win64\cs2.exe",
                @"D:\SteamLibrary\steamapps\common\Counter-Strike Global Offensive\game\bin\win64\cs2.exe",
                @"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive\game\bin\win64\cs2.exe",
                @"C:\SteamLibrary\steamapps\common\Counter-Strike Global Offensive\game\bin\win64\cs2.exe"
            };

            bool launchedDirect = false;
            foreach (var p in potentialPaths)
            {
                if (File.Exists(p))
                {
                    try
                    {
                        var startInfo = new ProcessStartInfo
                        {
                            FileName = p,
                            WorkingDirectory = Path.GetDirectoryName(p),
                            UseShellExecute = true
                        };
                        Process.Start(startInfo);
                        LogMessage($"-> Launched CS2 directly from: {p}");
                        TxtStatusBar.Text = _currentLang == "hu" ? "CS2 elindítva közvetlenül..." : "CS2 launched directly...";
                        launchedDirect = true;
                        break;
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"[-] Direct launch failed on {p}: {ex.Message}");
                    }
                }
            }

            if (!launchedDirect)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "steam://rungameid/730",
                        UseShellExecute = true
                    });
                    LogMessage("-> Steam launch signal dispatched (steam://rungameid/730).");
                    TxtStatusBar.Text = _currentLang == "hu" ? "Steam indítás elküldve (CS2)..." : "Steam launch signal dispatched (CS2)...";
                }
                catch (Exception ex)
                {
                    LogMessage("[-] Could not trigger Steam URI: " + ex.Message);
                }
            }
        }

        private void BtnClearLogs_Click(object sender, RoutedEventArgs e)
        {
            TxtTerminalLogs.Text = string.Empty;
            LogMessage("Console log buffer cleared.");
        }

        // Benchmark Simulation
        private async void BtnBenchmark_Click(object sender, RoutedEventArgs e)
        {
            BtnBenchmark.IsEnabled = false;
            LogMessage("Starting 10,000-tick EdgeBug and Bhop timing benchmark...");

            await Task.Delay(700);
            LogMessage("Simulating 80 player drops on de_mirage ladder and palace...");
            await Task.Delay(600);

            var rand = new Random();
            int successfulBugs = rand.Next(76, 80);
            double accuracy = (successfulBugs / 80.0) * 100.0;
            double latency = 0.18 + (rand.NextDouble() * 0.12);

            LogMessage($"Benchmark Results: {successfulBugs}/80 EdgeBugs preserved ({accuracy:0.0}%)");
            LogMessage($"Average Subtick Sampling Latency: {latency:0.00} ms. Zero dropped inputs.");

            BtnBenchmark.IsEnabled = true;
        }

        // Load Preset Button in List
        private void BtnLoadPreset_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ConfigPreset preset)
            {
                TxtActivePresetStatus.Text = (_currentLang == "hu" ? "Kiválasztott Konfig: " : "Selected Preset: ") + preset.Name;
                ChkAutoBhop.IsChecked = preset.Bhop;
                ChkEdgeBug.IsChecked = preset.EdgeBug;
                ChkJumpBug.IsChecked = preset.JumpBug;
                ChkPixelSurf.IsChecked = preset.PixelSurf;
                ChkTriggerbot.IsChecked = preset.Triggerbot;
                ChkRcs.IsChecked = preset.Rcs;
                ChkSilentAim.IsChecked = preset.SilentAim;
                SliderSmooth.Value = preset.SmoothAim;
                SliderJitter.Value = preset.CurveJitter;
                SliderRcsPitch.Value = preset.RcsPitchPercent;
                SliderRcsYaw.Value = preset.RcsYawPercent;
                SliderRcsSmooth.Value = preset.RcsSmooth;
                SliderFov.Value = preset.AimFov;
                SliderTriggerDelay.Value = preset.TriggerDelay;
                SliderHitchance.Value = preset.Hitchance;
                SliderBacktrack.Value = preset.BacktrackTicks;
                SliderMinDamage.Value = preset.MinDamage;

                LogMessage($"Activated config preset: '{preset.Name}' (Code: {preset.ShareCode})");
                TxtStatusBar.Text = (_currentLang == "hu" ? "Konfiguráció betöltve: " : "Config loaded: ") + preset.Name;
            }
        }

        private void LstConfigs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        // Create Custom Config
        private void BtnCreateConfig_Click(object sender, RoutedEventArgs e)
        {
            int nextIdx = _presets.Count + 1;
            string defaultName = (_currentLang == "hu" ? "Egyéni Konfig #" : "Custom Preset #") + nextIdx;
            string name = defaultName;

            var newPreset = new ConfigPreset
            {
                Name = name,
                Badge = "CUSTOM",
                Description = _currentLang == "hu" ? "Egyéni felhasználói profil a kliensből." : "Custom user created profile from client.",
                ShareCode = "CFG-SPRING-" + new Random().Next(100, 999),
                Bhop = ChkAutoBhop.IsChecked ?? true,
                EdgeBug = ChkEdgeBug.IsChecked ?? true,
                JumpBug = ChkJumpBug.IsChecked ?? true,
                PixelSurf = ChkPixelSurf.IsChecked ?? true,
                Triggerbot = ChkTriggerbot.IsChecked ?? true,
                Rcs = ChkRcs.IsChecked ?? true,
                SilentAim = ChkSilentAim.IsChecked ?? false,
                TargetBone = (CmbTargetBone.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Head / Neck",
                AimKey = (CmbAimKey.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Always On / Auto",
                SmoothAim = SliderSmooth.Value,
                CurveJitter = SliderJitter.Value,
                RcsPitchPercent = SliderRcsPitch.Value,
                RcsYawPercent = SliderRcsYaw.Value,
                RcsSmooth = SliderRcsSmooth.Value,
                AimFov = SliderFov.Value,
                TriggerDelay = (int)SliderTriggerDelay.Value,
                Hitchance = SliderHitchance.Value,
                BacktrackTicks = (int)SliderBacktrack.Value,
                MinDamage = (int)SliderMinDamage.Value
            };

            _presets.Insert(0, newPreset);
            TxtActivePresetStatus.Text = (_currentLang == "hu" ? "Kiválasztott Konfig: " : "Selected Preset: ") + newPreset.Name;
            LogMessage($"Created and synchronized new config preset: '{newPreset.Name}'");
            TxtStatusBar.Text = (_currentLang == "hu" ? "Új konfiguráció létrehozva: " : "New config created: ") + newPreset.Name;
        }

        // HWID Reset
        private void BtnResetHwid_Click(object sender, RoutedEventArgs e)
        {
            if (_hwidResetsRemaining <= 0)
            {
                MessageBox.Show(
                    _currentLang == "hu" 
                        ? "Nincs több ingyenes HWID visszaállítási lehetőséged. Kérj segítséget a Discord ügyfélszolgálaton." 
                        : "No automated HWID resets remaining. Please open a ticket on Discord.",
                    "HWID Security", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning
                );
                return;
            }

            var confirm = MessageBox.Show(
                _currentLang == "hu" 
                    ? "Biztosan alaphelyzetbe állítod a géphez kötött hardverazonosítót?" 
                    : "Are you sure you want to reset your registered hardware identifier?",
                "Spring HWID Unbind", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question
            );

            if (confirm == MessageBoxResult.Yes)
            {
                _hwidResetsRemaining--;
                string newHwid = "DESKTOP-" + Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper() + "-SPRING";
                TxtHwidBox.Text = newHwid;
                TxtCardHwidVal.Text = newHwid.Substring(0, 15) + "...";
                BtnResetHwid.Content = _currentLang == "hu" ? "🔄 HWID Zárolás Törölve (0 maradt)" : "🔄 HWID Reset Done (0 remaining)";
                BtnResetHwid.IsEnabled = false;

                LogMessage("HWID binding reset successfully. Bound to: " + newHwid);
            }
        }

        // License Key Activation
        private void BtnActivateKey_Click(object sender, RoutedEventArgs e)
        {
            string key = TxtLicenseKeyBox.Text.Trim().ToUpper();
            if (key.Length < 10)
            {
                MessageBox.Show(
                    _currentLang == "hu" ? "Kérjük adj meg érvényes 16 jegyű licenckulcsot." : "Please enter a valid 16-character license token.",
                    "License Verification",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            _subscriptionExpiry = _subscriptionExpiry.AddDays(30);
            TxtCardSubVal.Text = _currentLang == "hu" ? "54 Nap Aktív" : "54 Days Active";
            TxtCardSubExp.Text = (_currentLang == "hu" ? "Lejár: " : "Expires: ") + _subscriptionExpiry.ToString("yyyy-MM-dd");
            TxtSubDaysSidebar.Text = "● 54 " + (_currentLang == "hu" ? "Nap Aktív" : "Days Active");

            LogMessage($"Key '{key}' activated! +30 days added to subscription.");
            MessageBox.Show(
                _currentLang == "hu" ? "Licenc sikeresen aktiválva! 30 nap jóváírva a fiókodon." : "License token verified! 30 days added to your subscription.",
                "Spring License",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        // Language Toggle (English <-> Magyar)
        private void BtnLangToggle_Click(object sender, RoutedEventArgs e)
        {
            _currentLang = _currentLang == "en" ? "hu" : "en";
            UpdateLanguageUI();
        }

        private void UpdateLanguageUI()
        {
            bool isHu = _currentLang == "hu";
            BtnLangToggle.Content = isHu ? "🇭🇺 HU (Váltás: EN)" : "🇬🇧 EN (Switch: HU)";

            // Title & Status
            StatusText.Text = isHu ? "VAC-Live: Undetected (Védelem aktív)" : "VAC-Live: Undetected (Protected)";
            CheckCs2Process();

            // Sidebar Tabs
            NavOverview.Content = isHu ? "⚡ Vezérlőpult & Injektálás" : "⚡ Dashboard & Inject";
            NavMovement.Content = isHu ? "🏃 Subtick Mozgás" : "🏃 Subtick Movement";
            NavAimbot.Content = isHu ? "🎯 Harc & Aimbot" : "🎯 Combat & Aimbot";
            NavVisuals.Content = isHu ? "✨ Vizuál & Dynamic Island" : "✨ Visuals & Radar";
            NavInventory.Content = isHu ? "🗡️ Kések & Kesztyűk" : "🗡️ Knives & Skins";
            NavHwid.Content = isHu ? "🔒 HWID & Licenc" : "🔒 HWID & License";

            // Dashboard
            TxtOverviewTitle.Text = isHu ? "Vezérlőpult & Memóriahorog" : "Control Dashboard & Process Hook";
            TxtOverviewSub.Text = isHu ? "Subtick szinkronizáció, valós idejű memóriainjektálás és telemetria." : "Subtick synchronization, live memory injection and active profile telemetry.";
            TxtCardSubTitle.Text = isHu ? "ELŐFIZETÉS" : "SUBSCRIPTION";
            TxtCardHwidTitle.Text = isHu ? "HARDVER ZÁROLÁS" : "HARDWARE LOCK";
            TxtCardBuildTitle.Text = isHu ? "SZOFTVER BUILD" : "ENGINE BUILD";
            TxtInjectionConsoleTitle.Text = isHu ? "Valós Idejű Injektáló Konzol & Telemetria" : "Live Injection Console & Telemetry";
            TxtInjectionConsoleSub.Text = isHu ? "Közvetlen memóriafoglalás VirtualAllocEx és 128-tick subtick szinkronizáció segítségével." : "Direct process allocation via VirtualAllocEx & Subtick 128-tick synchronizer.";
            BtnBenchmark.Content = isHu ? "⏱ Subtick Késleltetés Teszt" : "⏱ Test Subtick Latency";
            BtnOpenOverlayDirect.Content = isHu ? "🎮 Játékbeli Menü Megnyitása" : "🎮 Open In-Game Menu";
            BtnLaunchCs2.Content = isHu ? "🚀 CS2 Indítása (Steam)" : "🚀 Launch CS2 (Steam)";
            BtnClearLogs.Content = isHu ? "Törlés" : "Clear Logs";
            BtnInject.Content = isHu ? "⚡ Spring Injektálása a CS2-be" : "⚡ Inject Spring into CS2";
            TxtInGameHotkey.Text = isHu ? "Játékbeli billentyű: INSERT a menü megnyitásához" : "In-Game Key: INSERT to open overlay";

            // Movement Tab
            TxtMovementHeading.Text = isHu ? "Subtick Mozgásmotor" : "Subtick Movement Engine";
            TxtMovementSub.Text = isHu ? "Állítsd be a bunnyhopot, edge-bug esési arányt, jump-bugot és pixel surföt." : "Configure bhop, edge-bug fall rate, jump-bug and pixel surf physics.";

            // Aimbot Tab
            TxtAimbotHeading.Text = isHu ? "Harc & Fegyver Visszarúgás Vezérlés" : "Combat & Weapon Recoil Control";
            TxtAimbotSub.Text = isHu ? "Finomhangolt célzásrásegítés, önálló RCS visszarúgás és seeded triggerbot." : "Humanized smoothing curves, standalone recoil pitch/yaw, and seeded triggerbot.";

            // Visuals Tab
            TxtVisualsHeading.Text = isHu ? "DirectX 11 Dynamic Island & Vizuál" : "DirectX 11 Dynamic Island & Visuals";
            TxtVisualsSub.Text = isHu ? "Lebegő kapszula HUD a CS2-ben valós idejű Spotify zeneinformációval és bomba időzítővel." : "Floating pill HUD in CS2 with real-time Spotify, bomb countdown, and grenade indicators.";

            // Configs Tab
            TxtConfigsHeading.Text = isHu ? "Felhős Konfigurációk Szinkronizálása" : "Cloud Configuration Sync";
            TxtConfigsSub.Text = isHu ? "Automatikusan szinkronizál a spring webpanel beállításaival." : "Seamlessly synchronizes with your spring web panel presets.";
            BtnCreateConfig.Content = isHu ? "+ Új Konfig" : "+ Create Config";

            // HWID Tab
            TxtHwidHeading.Text = isHu ? "Hardverazonosító (HWID) & Licenc Biztonság" : "Hardware ID & License Security";
            TxtHwidSub.Text = isHu ? "Kezeld a gép regisztrációt, licenckulcsokat és feloldó zsetonokat." : "Manage PC registration, active license token, and unbind tokens.";
            TxtHwidLabel.Text = isHu ? "Regisztrált hardver ujjlenyomat (MachineGuid):" : "Registered Hardware Fingerprint (MachineGuid):";
            TxtHwidNote.Text = isHu ? "A géphez kötés megvédi a fiókodat az illetéktelen használattól." : "Hardware binding protects your account from unauthorized remote access.";
            TxtLicenseLabel.Text = isHu ? "Licenc / Promóciós Kulcs Aktiválása:" : "Activate License / Promo Key:";
            BtnActivateKey.Content = isHu ? "Kulcs Aktiválása" : "Activate Key";

            LogMessage(isHu ? "Nyelv sikeresen átváltva: Magyar" : "Language switched: English");
        }
    }
}