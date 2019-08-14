using System.Collections.Generic;
using System.Diagnostics;

namespace BookmarksManager
{
    /// <summary>
    /// Each of the managed browsers
    /// </summary>
    internal enum Browser
    {
        IE,
        Chrome,
        Firefox
    }

    /// <summary>
    /// Used to check if browser is running.
    /// </summary>
    internal class Browsers
    {
        internal static readonly Dictionary<Browser, string> browsersExe = new Dictionary<Browser, string>()
        {
            { Browser.IE, "iexplore" },
            { Browser.Chrome, "chrome" },
            { Browser.Firefox, "firefox" }
        };

        internal static bool IsRunning (Browser browser)
        {
            Process[] processes = Process.GetProcessesByName(browsersExe[browser]);
            if (processes.Length == 0)
                return false;
            else
                return true;
        }
    }
}
