using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StringTokenFormatter;

namespace Com.GitHub.ZachDeibert.WindowsTermProfiles.Model {
    class Profile {
        public string Name { get; set; }
        public Command Defaults { get; set; }
        public List<Command> Commands { get; set; }
        public List<Profile> Profiles { get; set; }

        public Profile ApplyDefaults(Command defaults, ReplacementVariables repl) {
            Name = Name.FormatToken(repl.Replace);
            if (Defaults != null) {
                Defaults.ApplyDefaults(defaults, repl);
            } else {
                Defaults = defaults;
            }
            if (Commands != null) {
                foreach (Command cmd in Commands) {
                    cmd.ApplyDefaults(Defaults, repl);
                }
                Command first = Commands.FirstOrDefault(c => c.Type != CommandType.MoveFocus);
                if (first != null) {
                    first.NewTab = true;
                }
            }
            if (Profiles != null) {
                foreach (Profile prof in Profiles) {
                    prof.ApplyDefaults(Defaults, repl);
                }
            }
            return this;
        }

        private static string GetFilePath([CallerFilePath] string filePath = "") => filePath;

        public static Profile Load() =>
            JsonConvert.DeserializeObject<Profile>(
                File.ReadAllText(
                    Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(GetFilePath())), "profiles.json")))
            .ApplyDefaults(new(), new());
    }
}
