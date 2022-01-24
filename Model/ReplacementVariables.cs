using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.GitHub.ZachDeibert.WindowsTermProfiles.Model {
    class ReplacementVariables {
        private readonly Dictionary<string, string> Replacements = new();

        public string Replace(string val) {
            if (Replacements.TryGetValue(val, out string replacement)) {
                return replacement;
            } else {
                return $"{'{'}{val}{'}'}";
            }
        }

        public ReplacementVariables() {
            string cwd = Environment.CurrentDirectory;
            Replacements["WorkingDirectory"] = cwd;
            Replacements["WorkingDirectory:WSL"] = $"/mnt/{char.ToLower(cwd[0])}/{cwd[3..].Replace('\\', '/')}";
            for (IDictionaryEnumerator it = Environment.GetEnvironmentVariables().GetEnumerator(); it.MoveNext(); ) {
                Replacements[$"Environment:{it.Key}"] = it.Value.ToString();
            }
        }
    }
}
