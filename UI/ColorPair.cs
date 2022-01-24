using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mindmagma.Curses;

namespace Com.GitHub.ZachDeibert.WindowsTermProfiles.UI {
    class ColorPair : IDisposable {
        private bool Disposed;
        private readonly CursesContext Ctx;
        private readonly short Index;
        public readonly uint Id;

        public ColorPair(CursesContext ctx, Color fg, Color bg) {
            Ctx = ctx;
            int idx = ctx.ColorPairs.IndexOf(null);
            if (idx < 0) {
                idx = ctx.ColorPairs.Count;
                ctx.ColorPairs.Add(null);
            }
            Index = (short) idx;
            ctx.ColorPairs[idx] = this;
            Id = NCurses.ColorPair(Index + 1);
            NCurses.InitPair((short) (Index + 1), (short) fg, (short) bg);
        }

        protected virtual void Dispose(bool disposing) {
            if (!Disposed) {
                if (disposing) {
                    Ctx.ColorPairs[Index] = null;
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
