using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorHelper;
using StringTokenFormatter;

namespace Com.GitHub.ZachDeibert.WindowsTermProfiles.Model {
    class Command {
        public CommandType Type {
            get {
                if (NewTab == true) {
                    return CommandType.NewTab;
                } else if (MoveDirection != null) {
                    return CommandType.MoveFocus;
                } else {
                    return CommandType.SplitPane;
                }
            }
        }

        public bool? NewTab { get; set; }

        public string Profile { get; set; }

        public string CD { get; set; }

        public string[] CommandLine { get; set; }

        public string Title { get; set; }

        public bool? ColorFromTitle { get; set; }

        private string _Color;
        public string Color {
            get {
                if (ColorFromTitle == true && Title != null) {
                    Random rand = new(Title.Aggregate(7, (a, b) => 31 * a + b));
                    return $"#{ColorConverter.HsvToHex(new HSV(rand.Next(360), (byte) (80 + rand.Next(20)), (byte) (80 + rand.Next(20)))).Value}";
                } else {
                    return _Color;
                }
            }
            set => _Color = value;
        }

        public SplitDirection? SplitDirection { get; set; }

        public double? Size { get; set; }

        public bool? Duplicate { get; set; }

        public MoveDirection? MoveDirection { get; set; }

        public bool? SuppressApplicationTitle { get; set; }

        public Command ApplyDefaults(Command defaults, ReplacementVariables repl) {
            if (NewTab == null) {
                NewTab = defaults.NewTab;
            }
            if (Profile == null) {
                Profile = defaults.Profile;
            } else {
                Profile = Profile.FormatToken(repl.Replace);
            }
            if (CD == null) {
                CD = defaults.CD;
            } else {
                CD = CD.FormatToken(repl.Replace);
            }
            if (CommandLine == null) {
                CommandLine = defaults.CommandLine;
            } else {
                CommandLine = CommandLine.Select(s => s.FormatToken(repl.Replace)).ToArray();
            }
            if (Title == null) {
                Title = defaults.Title;
            } else {
                Title = Title.FormatToken(repl.Replace);
            }
            if (ColorFromTitle == null) {
                ColorFromTitle = defaults.ColorFromTitle;
            }
            if (_Color == null) {
                _Color = defaults._Color;
            } else {
                _Color = _Color.FormatToken(repl.Replace);
            }
            if (SplitDirection == null) {
                SplitDirection = defaults.SplitDirection;
            }
            if (Size == null) {
                Size = defaults.Size;
            }
            if (Duplicate == null) {
                Duplicate = defaults.Duplicate;
            }
            if (MoveDirection == null) {
                MoveDirection = defaults.MoveDirection;
            }
            if (SuppressApplicationTitle == null) {
                SuppressApplicationTitle = defaults.SuppressApplicationTitle;
            }
            return this;
        }
    }
}
