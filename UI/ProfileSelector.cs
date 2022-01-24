using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mindmagma.Curses;
using Com.GitHub.ZachDeibert.WindowsTermProfiles.Model;

namespace Com.GitHub.ZachDeibert.WindowsTermProfiles.UI {
    class ProfileSelector : IDisposable {
        private bool Disposed;
        private readonly InputHandler Input;
        private readonly CursesContext Ctx;
        private readonly ColorPair Background;
        private readonly ColorPair Select;

        private int SelectFrom(string title, IEnumerable<string> options) {
            using SelectWindow win = new(Input, Background, Select, title, options.ToArray());
            int ret = -1;
            win.SelectedOption += x => {
                ret = x;
                Input.Stop();
            };
            Input.Process();
            return ret;
        }

        public Profile DisplayUI(Profile root) {
            Stack<Profile> menu = new();
            menu.Push(root);
            while ((menu.First().Profiles?.Count ?? 0) > 0) {
                string title = "Select Profile";
                if ((menu.First().Name?.Length ?? 0) > 0) {
                    title = menu.First().Name;
                }
                IEnumerable<string> options = menu.First().Profiles.Select(p => p.Name);
                if (menu.Count > 1) {
                    options = options.Prepend("Back");
                }
                int idx = SelectFrom(title, options);
                if (menu.Count > 1) {
                    if (idx == 0) {
                        menu.Pop();
                        continue;
                    }
                    --idx;
                }
                menu.Push(menu.First().Profiles[idx]);
            }
            return menu.First();
        }

        private void HandleResize() {
            NCurses.Clear();
            NCurses.Refresh();
        }

        public ProfileSelector() {
            Input = new();
            Ctx = new();
            Background = new(Ctx, Color.White, Color.Blue);
            Select = new(Ctx, Color.White, Color.Cyan);
            Input.WindowResize += HandleResize;
        }

        protected virtual void Dispose(bool disposing) {
            if (!Disposed) {
                if (disposing) {
                    Input.WindowResize -= HandleResize;
                    Select.Dispose();
                    Background.Dispose();
                    Ctx.Dispose();
                    Input.Dispose();
                }
                Disposed = true;
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
