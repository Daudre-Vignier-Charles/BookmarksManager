using System;
using System.IO;
using System.Configuration;
using MadMilkman.Ini;

namespace BookmarksManager.Firefox
{

    /// <summary>
    /// There are two versions (update channels) of Firefox : ESR (long term support) and Release (standard).
    /// BookmarksManager use the user defined version into config file.
    /// </summary>
    enum FirefoxVersion
    {
        release,
        esr
    }


    /// <summary>
    /// Ini is used to get the Firefox database path.
    /// Once class is initiated, database path is stored into bookmarkDatabasePath property.
    /// </summary>
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
            bookmarkDatabasePath = GetDatabasePath(profileName);
            if (!File.Exists(bookmarkDatabasePath))
                throw new FileNotFoundException("File " + bookmarkDatabasePath + " not found");

        }

        /// <summary>
        /// Get database path from profile
        /// </summary>
        /// <param name="profileName"></param>
        /// <returns></returns>
        private string GetDatabasePath(string profileName) =>
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Mozilla\Firefox\Profiles\" + profileName + @"\places.sqlite";

        /// <summary>
        /// Get profile name from ini file.
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
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
