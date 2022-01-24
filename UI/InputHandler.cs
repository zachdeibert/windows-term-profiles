using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Mindmagma.Curses;

namespace Com.GitHub.ZachDeibert.WindowsTermProfiles.UI {
    class InputHandler : IDisposable {
        [StructLayout(LayoutKind.Explicit)]
        struct KEY_EVENT_RECORD_uChar {
            [FieldOffset(0)]
            public char UnicodeChar;
            [FieldOffset(0)]
            public byte AsciiChar;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct KEY_EVENT_RECORD {
            [FieldOffset(0)]
            public byte bKeyDown;
            [FieldOffset(4)]
            public ushort wRepeatCount;
            [FieldOffset(6)]
            public ushort wVirtualKeyCode;
            [FieldOffset(8)]
            public ushort wVirtualScanCode;
            [FieldOffset(10)]
            public KEY_EVENT_RECORD_uChar uChar;
            [FieldOffset(12)]
            public uint dwControlKeyState;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct COORD {
            public short x;
            public short y;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MOUSE_EVENT_RECORD {
            public COORD dwMousePosition;
            public uint dwButtonState;
            public uint dwControlKeyState;
            public uint dwEventFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct WINDOW_BUFFER_SIZE_RECORD {
            public COORD dwSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MENU_EVENT_RECORD {
            public uint dwCommandId;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct FOCUS_EVENT_RECORD {
            public byte bSetFocus;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct INPUT_RECORD_Event {
            [FieldOffset(0)]
            public KEY_EVENT_RECORD KeyEvent;
            [FieldOffset(0)]
            public MOUSE_EVENT_RECORD MouseEvent;
            [FieldOffset(0)]
            public WINDOW_BUFFER_SIZE_RECORD WindowBufferSizeEvent;
            [FieldOffset(0)]
            public MENU_EVENT_RECORD MenuEvent;
            [FieldOffset(0)]
            public FOCUS_EVENT_RECORD FocusEvent;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct INPUT_RECORD {
            public ushort EventType;
            public INPUT_RECORD_Event Event;
        }

        private const uint STD_INPUT_HANDLE = 0xFFFFFFF6;
        private const uint ENABLE_WINDOW_INPUT = 0x0008;
        private const uint ENABLE_MOUSE_INPUT = 0x0010;
        private const uint ENABLE_EXTENDED_FLAGS = 0x0080;
        private const ushort KEY_EVENT = 0x0001;
        private const ushort MOUSE_EVENT = 0x0002;
        private const ushort WINDOW_BUFFER_SIZE_EVENT = 0x0004;
        private const uint FROM_LEFT_1ST_BUTTON_PRESSED = 0x0001;

        [DllImport("Kernel32.dll")]
        private static extern IntPtr GetStdHandle(uint nStdHandle);

        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern bool ReadConsoleInput(IntPtr hConsoleInput, [Out] INPUT_RECORD[] lpBuffer, uint nLength, out uint lpNumberOfEventsRead);

        private bool Disposed;
        private readonly IntPtr STDIN;
        private readonly uint OldMode;
        private bool Run;

        private readonly object SizeLock = new();

        private int _Width = 0;
        public int Width {
            get {
                lock (SizeLock) {
                    if (_Width == 0) {
                        _Width = NCurses.Columns;
                    }
                }
                return _Width;
            }
        }

        private int _Height = 0;
        public int Height {
            get {
                lock (SizeLock) {
                    if (_Height == 0) {
                        _Height = NCurses.Lines;
                    }
                }
                return _Height;
            }
        }

        public event Action<int, uint> KeyDown;
        public event Action<int, int> MouseDown;
        public event Action WindowResize;

        public void Process() {
            INPUT_RECORD[] buffer = new INPUT_RECORD[16];
            Run = true;
            while (Run) {
                if (!ReadConsoleInput(STDIN, buffer, (uint) buffer.Length, out uint count)) {
                    throw new Exception($"ReadConsoleInput: {Marshal.GetLastWin32Error()}");
                }
                for (uint i = 0; i < count; ++i) {
                    switch (buffer[i].EventType) {
                        case KEY_EVENT:
                            if (buffer[i].Event.KeyEvent.bKeyDown != 0) {
                                KeyDown?.Invoke(buffer[i].Event.KeyEvent.wVirtualKeyCode,
                                                buffer[i].Event.KeyEvent.dwControlKeyState);
                            }
                            break;
                        case MOUSE_EVENT:
                            if (buffer[i].Event.MouseEvent.dwButtonState == FROM_LEFT_1ST_BUTTON_PRESSED) {
                                MouseDown?.Invoke(buffer[i].Event.MouseEvent.dwMousePosition.x,
                                                  buffer[i].Event.MouseEvent.dwMousePosition.y);
                            }
                            break;
                        case WINDOW_BUFFER_SIZE_EVENT:
                            lock (SizeLock) {
                                _Width = buffer[i].Event.WindowBufferSizeEvent.dwSize.x;
                                _Height = buffer[i].Event.WindowBufferSizeEvent.dwSize.y;
                            }
                            WindowResize?.Invoke();
                            break;
                    }
                }
            }
        }

        public void Stop() => Run = false;

        public InputHandler() {
            STDIN = GetStdHandle(STD_INPUT_HANDLE);
            if (!GetConsoleMode(STDIN, out OldMode)) {
                throw new Exception($"GetConsoleMode: {Marshal.GetLastWin32Error()}");
            }
            if (!SetConsoleMode(STDIN, ENABLE_WINDOW_INPUT | ENABLE_MOUSE_INPUT | ENABLE_EXTENDED_FLAGS)) {
                throw new Exception($"SetConsoleMode: {Marshal.GetLastWin32Error()}");
            }
        }

        protected virtual void Dispose(bool disposing) {
            if (!Disposed) {
                if (disposing) {
                    Stop();
                }
                if (!SetConsoleMode(STDIN, OldMode)) {
                    throw new Exception($"SetConsoleMode: {Marshal.GetLastWin32Error()}");
                }
                Disposed = true;
            }
        }

        ~InputHandler() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
