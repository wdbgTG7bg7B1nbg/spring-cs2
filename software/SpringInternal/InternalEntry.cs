using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace SpringInternal
{
    public static class InternalEntry
    {
        // CS2 In-Process Hooks and Internal State
        private static bool _isInjected = false;
        private static bool _menuOpen = false;
        private static IntPtr _cs2Hwnd = IntPtr.Zero;
        private static IntPtr _originalWndProc = IntPtr.Zero;
        private static WndProcDelegate? _wndProcDelegate;

        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowA(string? lpClassName, string? lpWindowName);

        [DllImport("user32.dll")]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrA")]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandleA(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, UIntPtr dwExtraInfo);

        private const int GWLP_WNDPROC = -4;
        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_KEYUP = 0x0101;
        private const int VK_SPACE = 0x20;
        private const int VK_INSERT = 0x2D;
        private const int VK_F11 = 0x7A;

        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;

        // In-Process Direct Base Pointers (Zero RPM/WPM Syscall Overhead)
        public static IntPtr ClientDllBase = IntPtr.Zero;
        public static IntPtr EngineDllBase = IntPtr.Zero;

        // Dynamic Embedded Offsets
        public static long dwLocalPlayerPawn = 37538824; // 0x23CC2A8
        public static long dwEntityList = 39287776;      // 0x2576BE0
        public static long jump = 34316304;              // 0x20B8210
        public static long m_iHealth = 0x344;
        public static long m_iTeamNum = 0x3E3;
        public static long m_fFlags = 0x3EC;
        public static long m_iShotsFired = 0x23E4;
        public static long m_aimPunchAngle = 0x187C;

        // Internal Config Toggles
        public static bool BhopEnabled { get; set; } = true;
        public static bool EdgeBugEnabled { get; set; } = true;
        public static bool JumpBugEnabled { get; set; } = true;
        public static bool RcsEnabled { get; set; } = true;
        public static bool TriggerbotEnabled { get; set; } = true;
        public static bool ThirdpersonEnabled { get; set; } = true;
        public static bool SkinChangerEnabled { get; set; } = true;

        public static int SelectedKnifeDef { get; set; } = 507; // Karambit
        public static int KnifePaintKit { get; set; } = 415;     // Emerald

        [UnmanagedCallersOnly(EntryPoint = "DllMain")]
        public static bool DllMain(IntPtr hinstDLL, uint fdwReason, IntPtr lpvReserved)
        {
            switch (fdwReason)
            {
                case 1: // DLL_PROCESS_ATTACH
                    Task.Run(() => InitializeInternalHooks());
                    break;
                case 0: // DLL_PROCESS_DETACH
                    DetachHooks();
                    break;
            }
            return true;
        }

        public static void InitializeInternalHooks()
        {
            if (_isInjected) return;
            _isInjected = true;

            try
            {
                // Resolve in-process module base pointers
                ClientDllBase = GetModuleHandleA("client.dll");
                EngineDllBase = GetModuleHandleA("engine2.dll");

                // Locate CS2 Window
                _cs2Hwnd = FindWindowA("SDL_app", "Counter-Strike 2");
                if (_cs2Hwnd == IntPtr.Zero)
                {
                    _cs2Hwnd = Process.GetCurrentProcess().MainWindowHandle;
                }

                if (_cs2Hwnd != IntPtr.Zero)
                {
                    // Hook CS2 native WndProc for direct in-process input capture
                    _wndProcDelegate = new WndProcDelegate(HookedWndProc);
                    IntPtr newWndProcPtr = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate);
                    _originalWndProc = SetWindowLongPtr(_cs2Hwnd, GWLP_WNDPROC, newWndProcPtr);
                }

                // Start In-Process Direct Pointer Subtick Loop
                StartInternalEngineThread();
            }
            catch {}
        }

        private static IntPtr HookedWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == WM_KEYDOWN)
            {
                int key = wParam.ToInt32();
                if (key == VK_INSERT || key == VK_F11)
                {
                    _menuOpen = !_menuOpen;
                    return IntPtr.Zero;
                }
            }

            return CallWindowProc(_originalWndProc, hWnd, msg, wParam, lParam);
        }

        private static void StartInternalEngineThread()
        {
            var thread = new Thread(() =>
            {
                Vector2 oldAimPunch = Vector2.Zero;

                while (_isInjected)
                {
                    try
                    {
                        if (ClientDllBase == IntPtr.Zero)
                        {
                            Thread.Sleep(50);
                            continue;
                        }

                        // Direct Unsafe Memory Pointer Read (0 Syscall Overhead)
                        IntPtr localPlayerPawn = ReadDirect<IntPtr>(ClientDllBase + (int)dwLocalPlayerPawn);
                        if (localPlayerPawn == IntPtr.Zero)
                        {
                            Thread.Sleep(10);
                            continue;
                        }

                        int health = ReadDirect<int>(localPlayerPawn + (int)m_iHealth);
                        if (health <= 0)
                        {
                            Thread.Sleep(10);
                            continue;
                        }

                        int flags = ReadDirect<int>(localPlayerPawn + (int)m_fFlags);

                        // 1. IN-PROCESS AUTO BHOP & SUBTICK ALIGNMENT
                        if (BhopEnabled && (GetAsyncKeyState(VK_SPACE) & 0x8000) != 0)
                        {
                            bool onGround = (flags & 1) != 0;
                            if (onGround)
                            {
                                WriteDirect<int>(ClientDllBase + (int)jump, 65537); // +jump
                            }
                            else
                            {
                                WriteDirect<int>(ClientDllBase + (int)jump, 256);   // -jump
                            }
                        }

                        // 2. IN-PROCESS 3RD PERSON PERSPECTIVE
                        if (ThirdpersonEnabled)
                        {
                            WriteDirect<bool>(localPlayerPawn + 0x1634, true);
                        }

                        // 3. IN-PROCESS SKIN CHANGER OVERRIDES
                        if (SkinChangerEnabled)
                        {
                            ProcessInternalSkinChanger(localPlayerPawn);
                        }

                        Thread.Sleep(1);
                    }
                    catch {}
                }
            })
            {
                IsBackground = true,
                Priority = ThreadPriority.Highest,
                Name = "SpringInternalEngineThread"
            };
            thread.Start();
        }

        private static unsafe void ProcessInternalSkinChanger(IntPtr localPlayerPawn)
        {
            try
            {
                IntPtr weaponServices = ReadDirect<IntPtr>(localPlayerPawn + 0x1100);
                if (weaponServices == IntPtr.Zero) return;

                int weaponCount = ReadDirect<int>(weaponServices + 0x58);
                if (weaponCount <= 0 || weaponCount > 64) return;

                IntPtr weaponArrayPtr = ReadDirect<IntPtr>(weaponServices + 0x48);
                IntPtr entList = ReadDirect<IntPtr>(ClientDllBase + (int)dwEntityList);
                if (weaponArrayPtr == IntPtr.Zero || entList == IntPtr.Zero) return;

                for (int i = 0; i < weaponCount; i++)
                {
                    uint handle = ReadDirect<uint>(weaponArrayPtr + i * 0x4);
                    if (handle == 0 || handle == 0xFFFFFFFF) continue;

                    IntPtr listEntry = ReadDirect<IntPtr>(entList + (int)(0x8 * ((handle & 0x7FFF) >> 9) + 16));
                    if (listEntry == IntPtr.Zero) continue;

                    IntPtr weaponEntity = ReadDirect<IntPtr>(listEntry + (int)(120 * (handle & 0x1FF)));
                    if (weaponEntity == IntPtr.Zero) continue;

                    short itemDefIndex = ReadDirect<short>(weaponEntity + 0x1BA);

                    if (itemDefIndex >= 500 || itemDefIndex == 42)
                    {
                        if (SelectedKnifeDef > 0 && SelectedKnifeDef != itemDefIndex)
                        {
                            WriteDirect<short>(weaponEntity + 0x1BA, (short)SelectedKnifeDef);
                        }
                        WriteDirect<int>(weaponEntity + 0x1520, KnifePaintKit); // Emerald (415)
                        WriteDirect<float>(weaponEntity + 0x1524, 0.001f);      // Factory New
                        WriteDirect<int>(weaponEntity + 0x1518, -1);            // Force refresh
                    }
                }
            }
            catch {}
        }

        // Direct Unsafe Memory Reader (Zero Win32 System Call Overhead)
        public static unsafe T ReadDirect<T>(IntPtr address) where T : unmanaged
        {
            if (address == IntPtr.Zero) return default;
            byte* ptr = (byte*)address.ToPointer();
            return *(T*)ptr;
        }

        public static unsafe void WriteDirect<T>(IntPtr address, T value) where T : unmanaged
        {
            if (address == IntPtr.Zero) return;
            byte* ptr = (byte*)address.ToPointer();
            *(T*)ptr = value;
        }

        public static void DetachHooks()
        {
            _isInjected = false;
            if (_cs2Hwnd != IntPtr.Zero && _originalWndProc != IntPtr.Zero)
            {
                try
                {
                    SetWindowLongPtr(_cs2Hwnd, GWLP_WNDPROC, _originalWndProc);
                }
                catch {}
            }
        }
    }
}
