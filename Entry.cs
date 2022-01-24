using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Com.GitHub.ZachDeibert.WindowsTermProfiles.Model;
using Com.GitHub.ZachDeibert.WindowsTermProfiles.UI;

namespace Com.GitHub.ZachDeibert.WindowsTermProfiles {
    public static class Entry {
        private static IEnumerable<string> GenerateCommand(Command c, int i) {
            if (i > 0) {
                yield return ";";
            }
            switch (c.Type) {
                case CommandType.NewTab:
                case CommandType.SplitPane:
                    switch (c.Type) {
                        case CommandType.NewTab:
                            yield return "new-tab";
                            break;
                        case CommandType.SplitPane:
                            yield return "split-pane";
                            break;
                    }
                    if (!string.IsNullOrWhiteSpace(c.Profile)) {
                        yield return "--profile";
                        yield return c.Profile;
                    }
                    if (!string.IsNullOrWhiteSpace(c.CD)) {
                        yield return "--startingDirectory";
                        yield return c.CD;
                    }
                    if (!string.IsNullOrWhiteSpace(c.Title)) {
                        yield return "--title";
                        yield return c.Title;
                    }
                    if (!string.IsNullOrWhiteSpace(c.Color)) {
                        yield return "--tabColor";
                        yield return c.Color;
                    }
                    if (c.SuppressApplicationTitle == true) {
                        yield return "--suppressApplicationTitle";
                    }
                    if (c.Type == CommandType.SplitPane) {
                        switch (c.SplitDirection) {
                            case SplitDirection.Horizontal:
                                yield return "--horizontal";
                                break;
                            case SplitDirection.Vertical:
                                yield return "--vertical";
                                break;
                        }
                        if (c.Size != null) {
                            yield return "--size";
                            yield return $"{c.Size}";
                        }
                        if (c.Duplicate == true) {
                            yield return "--duplicate";
                        }
                    }
                    if (c.CommandLine != null) {
                        foreach (string cmd in c.CommandLine) {
                            yield return cmd;
                        }
                    }
                    break;
                case CommandType.MoveFocus:
                    yield return "move-focus";
                    switch (c.MoveDirection) {
                        case MoveDirection.Up:
                            yield return "up";
                            break;
                        case MoveDirection.Down:
                            yield return "down";
                            break;
                        case MoveDirection.Left:
                            yield return "left";
                            break;
                        case MoveDirection.Right:
                            yield return "right";
                            break;
                    }
                    break;
                default:
                    throw new Exception("Invalid enum value");
            }
        }

        public static void Main() {
            Profile root = Profile.Load();
            using ProfileSelector ui = new();
            Profile prof = ui.DisplayUI(root);
            Process.Start("wt.exe", prof.Commands.SelectMany(GenerateCommand));
        }
    }
}
