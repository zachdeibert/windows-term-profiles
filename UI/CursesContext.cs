using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mindmagma.Curses;

namespace Com.GitHub.ZachDeibert.WindowsTermProfiles.UI {
    class CursesContext : IDisposable {
        private bool Disposed;
        public readonly IntPtr Screen;
        public readonly List<ColorPair> ColorPairs = new();

        public CursesContext() {
            Screen = NCurses.InitScreen();
            NCurses.StartColor();
        }

        protected virtual void Dispose(bool disposing) {
            if (!Disposed) {
                NCurses.EndWin();
                Disposed = true;
            }
        }

        ~CursesContext() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
