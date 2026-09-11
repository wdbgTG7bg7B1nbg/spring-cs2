using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace SpringInternal
{
    public static class InternalEntry
    {
        // CS2 In-Process Hooks and State
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

        private const int GWLP_WNDPROC = -4;
        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_KEYUP = 0x0101;
        private const int VK_INSERT = 0x2D;
        private const int VK_F11 = 0x7A;

        // In-Process Direct Offsets (Zero RPM/WPM overhead)
        public static IntPtr ClientDllBase = IntPtr.Zero;
        public static IntPtr EngineDllBase = IntPtr.Zero;

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

                // Start In-Process Subtick Movement Loop
                StartSubtickMovementThread();
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
                    // Trigger in-game renderer toggle
                    return IntPtr.Zero;
                }
            }

            // Pass message to CS2 original message pump
            return CallWindowProc(_originalWndProc, hWnd, msg, wParam, lParam);
        }

        private static void StartSubtickMovementThread()
        {
            var thread = new Thread(() =>
            {
                while (_isInjected)
                {
                    try
                    {
                        // Direct pointer read for local player flags & velocity (0 RPM overhead)
                        if (ClientDllBase != IntPtr.Zero)
                        {
                            // In-process subtick timing calculation
                        }
                    }
                    catch {}
                    Thread.Sleep(1);
                }
            })
            {
                IsBackground = true,
                Priority = ThreadPriority.Highest
            };
            thread.Start();
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
