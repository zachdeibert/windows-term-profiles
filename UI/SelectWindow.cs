using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mindmagma.Curses;

namespace Com.GitHub.ZachDeibert.WindowsTermProfiles.UI {
    class SelectWindow : Window {
        private const int VK_ENTER = 0x0D;
        private const int VK_UP = 0x26;
        private const int VK_DOWN = 0x28;
        private readonly ColorPair Background;
        private readonly ColorPair Select;
        private readonly string Title;
        private readonly string[] Options;
        private int Selected;

        public event Action<int> SelectedOption;

        public override void RefreshContents() {
            NCurses.MoveWindowAddString(Id, 0, 1, Title);
            int i = 0;
            foreach (string option in
                Options.Zip(
                    Enumerable.Range(0, 10)
                        .Select(n => n.ToString())
                        .Concat(
                            Enumerable.Range(0, 26)
                                .Select(n => $"{'A' + n}")
                    ), (a, b) => $" {b}.  {a}".PadRight(Width - 2))) {
                if (i == Selected) {
                    NCursesExt.WindowAttributeOn(Id, Select.Id);
                }
                NCurses.MoveWindowAddString(Id, i + 1, 1, option);
                if (i == Selected) {
                    NCursesExt.WindowAttributeOff(Id, Select.Id);
                }
                ++i;
            }
            NCurses.MoveWindowAddString(Id, Selected + 1, 2, "");
        }

        private void HandleKey(int keycode, uint modifiers) {
            switch (keycode) {
                case VK_ENTER:
                    SelectedOption?.Invoke(Selected);
                    return;
                case VK_UP:
                    if (Selected > 0) {
                        --Selected;
                    }
                    break;
                case VK_DOWN:
                    if (Selected < Options.Length - 1) {
                        ++Selected;
                    }
                    break;
                default:
                    int i = int.MaxValue;
                    if (keycode is >= '0' and <= '9') {
                        i = keycode - '0';
                    } else if (keycode is >= 'A' and <= 'Z') {
                        i = 10 + keycode - 'A';
                    } else if (keycode is >= 'a' and <= 'z') {
                        i = 10 + keycode - 'a';
                    }
                    if (i < Options.Length) {
                        SelectedOption?.Invoke(i);
                    }
                    return;
            }
            Refresh();
        }

        private void HandleMouse(int x, int y) {
            if (x >= X + 1 && x < X + Width - 1 && y >= Y + 1 && y < Y + Height - 1) {
                SelectedOption?.Invoke(y - (Y + 1));
            }
        }

        protected override void Create() {
            base.Create();
            NCurses.WindowBackground(Id, Background.Id);
        }

        public SelectWindow(InputHandler input, ColorPair background, ColorPair select, string title, string[] options)
            : base(
                  input,
                  Math.Max(2 + title.Length, 8 + options.Max(s => s.Length)),
                  2 + options.Length) {
            Background = background;
            Select = select;
            Title = title;
            Options = options;
            Selected = 0;
            input.KeyDown += HandleKey;
            input.MouseDown += HandleMouse;
            PostConstruct();
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                Input.KeyDown -= HandleKey;
                Input.MouseDown -= HandleMouse;
            }
            base.Dispose(disposing);
        }
    }
}
