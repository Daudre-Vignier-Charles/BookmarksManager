using System;
using System.Configuration;
using BookmarksManager.Firefox;

namespace BookmarksManager
{
    internal static class Settings
    {
        internal static string BookmarkFolderName = ConfigurationManager.AppSettings["BookmarkFolderName"];
        internal static readonly FirefoxVersion firefoxVersion = (FirefoxVersion)Enum.Parse(typeof(FirefoxVersion), ConfigurationManager.AppSettings["FirefoxVersion"]);
    }
}
