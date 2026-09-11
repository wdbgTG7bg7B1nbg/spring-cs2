using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace SpringClient
{
    public class MemoryEngine
    {
        private static MemoryEngine? _instance;
        public static MemoryEngine Instance => _instance ??= new MemoryEngine();

        // Process Handles
        private IntPtr _hProcess = IntPtr.Zero;
        private int _processId = 0;
        private IntPtr _clientDllBase = IntPtr.Zero;
        private IntPtr _engineDllBase = IntPtr.Zero;
        private bool _isRunning = false;
        private Thread? _workerThread;

        // Configuration toggles and sliders
        public bool BhopEnabled { get; set; } = true;
        public bool EdgeBugEnabled { get; set; } = true;
        public bool JumpBugEnabled { get; set; } = true;
        public bool PixelSurfEnabled { get; set; } = true;
        public bool RcsEnabled { get; set; } = true;
        public bool TriggerbotEnabled { get; set; } = true;
        public bool ThirdpersonEnabled { get; set; } = true;

        public double RcsPitchPercent { get; set; } = 75.0; // 0 - 100%
        public double RcsYawPercent { get; set; } = 65.0;   // 0 - 100%
        public double RcsSmooth { get; set; } = 2.5;
        public int TriggerDelay { get; set; } = 25; // ms
        public double SmoothAim { get; set; } = 3.5;
        public double ThirdpersonDistance { get; set; } = 120.0;
        public int ThirdpersonKey { get; set; } = 0x56; // V key

        // Skin Changer & Cosmetic Overrides
        public bool SkinChangerEnabled { get; set; } = true;
        public int SelectedKnifeDefinition { get; set; } = 507; // Karambit (507)
        public int KnifePaintKit { get; set; } = 415; // Emerald (415)
        public int Ak47PaintKit { get; set; } = 724; // Wild Lotus (724)
        public int AwpPaintKit { get; set; } = 756; // Gungnir (756)
        public int M4A1PaintKit { get; set; } = 984; // Printstream (984)
        public float SkinWear { get; set; } = 0.001f; // Factory New
        public int SkinSeed { get; set; } = 661; // Blue Gem Tier 1
        public int StatTrakKills { get; set; } = 1337;

        private bool _thirdpersonActive = false;
        private bool _thirdpersonKeyWasDown = false;

        // Recoil state tracking
        private Vector2 _oldAimPunch = Vector2.Zero;
        private int _oldShotsFired = 0;

        // Win32 API Imports
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, int th32ProcessID);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool Module32First(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool Module32Next(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private const uint PROCESS_ALL_ACCESS = 0x1F0FFF;
        private const uint PROCESS_VM_READ = 0x0010;
        private const uint PROCESS_VM_WRITE = 0x0020;
        private const uint PROCESS_VM_OPERATION = 0x0008;

        private const uint TH32CS_SNAPMODULE = 0x00000008;
        private const uint TH32CS_SNAPMODULE32 = 0x00000010;

        private const int VK_SPACE = 0x20;
        private const int VK_LBUTTON = 0x01;
        private const int VK_LCONTROL = 0xA2;
        private const int VK_SHIFT = 0x10;

        private const uint MOUSEEVENTF_MOVE = 0x0001;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private struct MODULEENTRY32
        {
            public uint dwSize;
            public uint th32ModuleID;
            public uint th32ProcessID;
            public uint GlblcntUsage;
            public uint ProccntUsage;
            public IntPtr modBaseAddr;
            public uint modBaseSize;
            public IntPtr hModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szExePath;
        }

        public bool IsAttached => _hProcess != IntPtr.Zero && _clientDllBase != IntPtr.Zero;
        public int AttachedPid => _processId;

        public bool AttachToCs2()
        {
            try
            {
                var procs = Process.GetProcessesByName("cs2");
                if (procs.Length == 0) return false;

                _processId = procs[0].Id;
                _hProcess = OpenProcess(PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION, false, _processId);
                if (_hProcess == IntPtr.Zero)
                {
                    _hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, _processId);
                }

                if (_hProcess == IntPtr.Zero) return false;

                _clientDllBase = GetModuleBaseAddress(_processId, "client.dll");
                _engineDllBase = GetModuleBaseAddress(_processId, "engine2.dll");

                if (_clientDllBase == IntPtr.Zero)
                {
                    // Fallback module address resolution
                    foreach (ProcessModule m in procs[0].Modules)
                    {
                        if (m.ModuleName.Equals("client.dll", StringComparison.OrdinalIgnoreCase))
                            _clientDllBase = m.BaseAddress;
                        else if (m.ModuleName.Equals("engine2.dll", StringComparison.OrdinalIgnoreCase))
                            _engineDllBase = m.BaseAddress;
                    }
                }

                if (_clientDllBase != IntPtr.Zero)
                {
                    StartWorker();
                    return true;
                }
            }
            catch {}
            return false;
        }

        private IntPtr GetModuleBaseAddress(int pid, string moduleName)
        {
            IntPtr snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE | TH32CS_SNAPMODULE32, pid);
            if (snapshot == IntPtr.Zero || snapshot == new IntPtr(-1)) return IntPtr.Zero;

            var entry = new MODULEENTRY32 { dwSize = (uint)Marshal.SizeOf<MODULEENTRY32>() };
            IntPtr baseAddr = IntPtr.Zero;

            if (Module32First(snapshot, ref entry))
            {
                do
                {
                    if (entry.szModule.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                    {
                        baseAddr = entry.modBaseAddr;
                        break;
                    }
                } while (Module32Next(snapshot, ref entry));
            }

            CloseHandle(snapshot);
            return baseAddr;
        }

        public void StartWorker()
        {
            if (_isRunning) return;
            _isRunning = true;

            _workerThread = new Thread(WorkerLoop)
            {
                IsBackground = true,
                Priority = ThreadPriority.Highest,
                Name = "SpringSubtickMemoryWorker"
            };
            _workerThread.Start();
        }

        public void StopWorker()
        {
            _isRunning = false;
            if (_hProcess != IntPtr.Zero)
            {
                CloseHandle(_hProcess);
                _hProcess = IntPtr.Zero;
            }
        }

        private void WorkerLoop()
        {
            while (_isRunning)
            {
                try
                {
                    if (!IsAttached || !IsCs2Foreground())
                    {
                        Thread.Sleep(50);
                        continue;
                    }

                    var offsets = OffsetManager.Current;

                    // 1. Read LocalPlayer Pawn
                    IntPtr localPlayerPawn = Read<IntPtr>(_clientDllBase + (int)offsets.dwLocalPlayerPawn);
                    if (localPlayerPawn == IntPtr.Zero)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    int health = Read<int>(localPlayerPawn + (int)offsets.m_iHealth);
                    if (health <= 0)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    int flags = Read<int>(localPlayerPawn + (int)offsets.m_fFlags);
                    int shotsFired = Read<int>(localPlayerPawn + (int)offsets.m_iShotsFired);
                    Vector2 aimPunch = Read<Vector2>(localPlayerPawn + (int)offsets.m_aimPunchAngle);

                    // ==========================================
                    // 2. AUTO BUNNYHOP & SUBTICK ALIGNMENT
                    // ==========================================
                    if (BhopEnabled && (GetAsyncKeyState(VK_SPACE) & 0x8000) != 0)
                    {
                        // Flag 1 = FL_ONGROUND (0x1)
                        bool onGround = (flags & 1) != 0;
                        if (onGround)
                        {
                            // Trigger Subtick Jump
                            if (offsets.jump != 0)
                            {
                                Write<int>(_clientDllBase + (int)offsets.jump, 65537); // +jump
                                Thread.Sleep(2);
                                Write<int>(_clientDllBase + (int)offsets.jump, 256);   // -jump
                            }
                        }
                    }

                    // ==========================================
                    // 3. STANDALONE RECOIL CONTROL (PITCH / YAW)
                    // ==========================================
                    if (RcsEnabled)
                    {
                        if (shotsFired > 1)
                        {
                            Vector2 punchDelta = (aimPunch - _oldAimPunch) * 2.0f;

                            // Scale by user's pitch & yaw sliders
                            float pitchFactor = (float)(RcsPitchPercent / 100.0);
                            float yawFactor = (float)(RcsYawPercent / 100.0);

                            int moveX = (int)(-punchDelta.Y * yawFactor * 10.0f);
                            int moveY = (int)(punchDelta.X * pitchFactor * 10.0f);

                            if (Math.Abs(moveX) > 0 || Math.Abs(moveY) > 0)
                            {
                                mouse_event(MOUSEEVENTF_MOVE, moveX, moveY, 0, UIntPtr.Zero);
                            }

                            _oldAimPunch = aimPunch;
                        }
                        else
                        {
                            _oldAimPunch = Vector2.Zero;
                        }
                    }

                    // ==========================================
                    // 4. TRIGGERBOT (CROSSHAIR TARGET DETECTION)
                    // ==========================================
                    if (TriggerbotEnabled)
                    {
                        // Crosshair entity index in CS2 (dwCSGOInput or m_iIDEntIndex)
                        int crosshairEntId = Read<int>(localPlayerPawn + 0x1458);
                        if (crosshairEntId > 0)
                        {
                            IntPtr entList = Read<IntPtr>(_clientDllBase + (int)offsets.dwEntityList);
                            if (entList != IntPtr.Zero)
                            {
                                IntPtr listEntry = Read<IntPtr>(entList + 0x8 * ((crosshairEntId & 0x7FFF) >> 9) + 16);
                                if (listEntry != IntPtr.Zero)
                                {
                                    IntPtr targetPawn = Read<IntPtr>(listEntry + 120 * (crosshairEntId & 0x1FF));
                                    if (targetPawn != IntPtr.Zero)
                                    {
                                        int targetHealth = Read<int>(targetPawn + (int)offsets.m_iHealth);
                                        int targetTeam = Read<int>(targetPawn + (int)offsets.m_iTeamNum);
                                        int localTeam = Read<int>(localPlayerPawn + (int)offsets.m_iTeamNum);

                                        if (targetHealth > 0 && targetTeam != localTeam)
                                        {
                                            if (TriggerDelay > 0) Thread.Sleep(TriggerDelay);
                                            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
                                            Thread.Sleep(15);
                                            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // ==========================================
                    // 5. 3RD PERSON PERSPECTIVE (TOGGLE KEY & CAMERA OVERRIDE)
                    // ==========================================
                    if (ThirdpersonEnabled)
                    {
                        short keyState = GetAsyncKeyState(ThirdpersonKey);
                        bool isKeyDown = (keyState & 0x8000) != 0;
                        if (isKeyDown && !_thirdpersonKeyWasDown)
                        {
                            _thirdpersonActive = !_thirdpersonActive;
                        }
                        _thirdpersonKeyWasDown = isKeyDown;

                        if (_thirdpersonActive)
                        {
                            // In-process / RPM camera override
                            Write<bool>(localPlayerPawn + 0x1634, true);
                        }
                    }

                    // ==========================================
                    // 6. EXTERNAL SKIN CHANGER & VIEWMODEL OVERRIDE ENGINE
                    // ==========================================
                    if (SkinChangerEnabled)
                    {
                        ProcessSkinChangerOverrides(localPlayerPawn, offsets);
                    }

                    Thread.Sleep(1); // 1ms High-tick subtick cycle
                }
                catch {}
            }
        }

        private void ProcessSkinChangerOverrides(IntPtr localPlayerPawn, OffsetData offsets)
        {
            try
            {
                // Read weapon inventory entity handle array (m_pWeaponServices -> m_hMyWeapons)
                IntPtr weaponServices = Read<IntPtr>(localPlayerPawn + 0x1100);
                if (weaponServices == IntPtr.Zero) return;

                int weaponCount = Read<int>(weaponServices + 0x58); // m_hMyWeapons size
                if (weaponCount <= 0 || weaponCount > 64) return;

                IntPtr weaponArrayPtr = Read<IntPtr>(weaponServices + 0x48); // m_hMyWeapons buffer
                if (weaponArrayPtr == IntPtr.Zero) return;

                IntPtr entList = Read<IntPtr>(_clientDllBase + (IntPtr)offsets.dwEntityList);
                if (entList == IntPtr.Zero) return;

                for (int i = 0; i < weaponCount; i++)
                {
                    uint handle = Read<uint>(weaponArrayPtr + i * 0x4);
                    if (handle == 0 || handle == 0xFFFFFFFF) continue;

                    // Resolve Entity Pawn Address from Handle
                    IntPtr listEntry = Read<IntPtr>(entList + (int)(0x8 * ((handle & 0x7FFF) >> 9) + 16));
                    if (listEntry == IntPtr.Zero) continue;

                    IntPtr weaponEntity = Read<IntPtr>(listEntry + (int)(120 * (handle & 0x1FF)));
                    if (weaponEntity == IntPtr.Zero) continue;

                    short itemDefIndex = Read<short>(weaponEntity + 0x1BA); // m_iItemDefinitionIndex

                    int targetPaintKit = 0;
                    if (itemDefIndex == 7) targetPaintKit = Ak47PaintKit; // AK-47
                    else if (itemDefIndex == 9) targetPaintKit = AwpPaintKit; // AWP
                    else if (itemDefIndex == 60) targetPaintKit = M4A1PaintKit; // M4A1-S
                    else if (itemDefIndex >= 500 || itemDefIndex == 42) // Knives / Default Knife
                    {
                        targetPaintKit = KnifePaintKit;
                        if (SelectedKnifeDefinition > 0 && SelectedKnifeDefinition != itemDefIndex)
                        {
                            Write<short>(weaponEntity + 0x1BA, (short)SelectedKnifeDefinition); // Override Knife Model Definition
                        }
                    }

                    if (targetPaintKit > 0)
                    {
                        Write<int>(weaponEntity + 0x1520, targetPaintKit); // m_nFallbackPaintKit
                        Write<float>(weaponEntity + 0x1524, SkinWear);     // m_flFallbackWear
                        Write<int>(weaponEntity + 0x1528, SkinSeed);       // m_nFallbackSeed
                        Write<int>(weaponEntity + 0x152C, StatTrakKills);  // m_nFallbackStatTrak
                        Write<int>(weaponEntity + 0x1518, -1);             // m_iItemIDHigh = -1 (forces fallback skin override)
                    }
                }
            }
            catch {}
        }

        private bool IsCs2Foreground()
        {
            IntPtr fgHwnd = GetForegroundWindow();
            if (fgHwnd == IntPtr.Zero) return false;
            GetWindowThreadProcessId(fgHwnd, out uint fgPid);
            return fgPid == _processId;
        }

        // Memory Read / Write Helper methods
        public T Read<T>(IntPtr address) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            byte[] buffer = new byte[size];
            if (ReadProcessMemory(_hProcess, address, buffer, size, out _))
            {
                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                try
                {
                    return Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
                }
                finally
                {
                    handle.Free();
                }
            }
            return default;
        }

        public bool Write<T>(IntPtr address, T value) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            byte[] buffer = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(value, ptr, true);
                Marshal.Copy(ptr, buffer, 0, size);
                return WriteProcessMemory(_hProcess, address, buffer, size, out _);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
    }
}
