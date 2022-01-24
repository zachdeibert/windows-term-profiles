using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mindmagma.Curses;

namespace Com.GitHub.ZachDeibert.WindowsTermProfiles.UI {
    class Window : IDisposable {
        private bool Disposed;
        protected readonly InputHandler Input;
        public IntPtr Id;
        protected readonly int Width;
        protected readonly int Height;
        protected int X { get; private set; }
        protected int Y { get; private set; }

        public virtual void RefreshContents() {
        }

        public void Refresh() {
            NCurses.Box(Id, '\0', '\0');
            RefreshContents();
            NCurses.WindowRefresh(Id);
        }

        protected virtual void Create() {
            X = (Input.Width - Width) / 2;
            Y = (Input.Height - Height) / 2;
            Id = NCurses.NewWindow(Height, Width, Y, X);
        }

        private void Destroy() {
            NCurses.WindowBackground(Id, 0);
            NCurses.ClearWindow(Id);
            NCurses.WindowRefresh(Id);
            NCurses.DeleteWindow(Id);
        }

        private void HandleResize() {
            Destroy();
            Create();
            NCurses.WindowRefresh(Id);
            Refresh();
        }

        protected void PostConstruct() {
            Create();
            NCurses.WindowRefresh(Id);
            Refresh();
        }

        public Window(InputHandler input, int width = 50, int height = 5) {
            Input = input;
            Width = width;
            Height = height;
            input.WindowResize += HandleResize;
        }

        protected virtual void Dispose(bool disposing) {
            if (!Disposed) {
                if (disposing) {
                    Input.WindowResize -= HandleResize;
                }
                Destroy();
                Disposed = true;
            }
        }

        ~Window() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
