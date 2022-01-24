using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Mindmagma.Curses;

namespace Com.GitHub.ZachDeibert.WindowsTermProfiles.UI {
    static class NCursesExt {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int dt_wattron(IntPtr win, uint attributes);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int dt_wattroff(IntPtr win, uint attributes);

        private static readonly NativeLibraryLoader.NativeLibrary lib
            = (NativeLibraryLoader.NativeLibrary) typeof(NCurses)
                .Assembly
                .GetType("Mindmagma.Curses.Interop.NCursesLibraryHandle")
                .GetField("lib", BindingFlags.NonPublic | BindingFlags.Static)
                .GetValue(null);

        private static readonly dt_wattron wattron = NativeToDelegate<dt_wattron>("wattron");

        private static readonly dt_wattroff wattroff = NativeToDelegate<dt_wattroff>("wattroff");

        private static D NativeToDelegate<D>(string exportedFunctionName) => lib.LoadFunction<D>(exportedFunctionName);

        private static void ThrowOnFailure(int result, string method) {
            if (result == -1) {
                throw new DotnetCursesException(method + "() returned ERR");
            }
        }

        public static void WindowAttributeOn(IntPtr win, uint attributes) {
            ThrowOnFailure(wattron(win, attributes), "AttributeOn");
        }

        public static void WindowAttributeOff(IntPtr win, uint attributes) {
            ThrowOnFailure(wattroff(win, attributes), "AttributeOff");
        }
    }
}
