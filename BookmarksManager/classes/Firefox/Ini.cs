using System;
using System.IO;
using System.Configuration;
using MadMilkman.Ini;

namespace BookmarksManager.Firefox
{
    enum FirefoxVersion
    {
        release,
        esr
    }

    internal class Ini
    {
        private string firefoxIniFile;
        public readonly string bookmarkDatabasePath;

        public Ini()
        {
            firefoxIniFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Mozilla\Firefox\profiles.ini");
            if (!File.Exists(firefoxIniFile))
                throw new FileNotFoundException("File " + firefoxIniFile + " not found");

            string profileName = GetProfileName(BookmarksManager.Settings.firefoxVersion);
            if (profileName == null)
                throw new Exception("Error while parsing ini file " + firefoxIniFile);
            bookmarkDatabasePath = GetBookmarkFilePath(profileName);
            if (!File.Exists(bookmarkDatabasePath))
                throw new FileNotFoundException("File " + bookmarkDatabasePath + " not found");

        }

        private string GetBookmarkFilePath(string profileName) =>
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Mozilla\Firefox\Profiles\" + profileName + @"\places.sqlite";

        private string GetProfileName(FirefoxVersion version)
        {
            IniFile ini = new IniFile();
            ini.Load(firefoxIniFile);
            foreach (IniSection section in ini.Sections)
            {
                switch (version)
                {
                    case FirefoxVersion.esr:
                        if (section.Name.StartsWith("Profile") && section.Keys["Default"] != null && section.Keys["Default"].Value == "1")
                            return section.Keys["Path"].Value.Substring(9);
                        else
                            continue;
                    case FirefoxVersion.release:
                        if (section.Name.StartsWith("Install") && section.Keys["Default"] != null)
                            return section.Keys["Default"].Value.Substring(9);
                        else
                            continue;
                }
            }
            return null;
        }
    }
}
