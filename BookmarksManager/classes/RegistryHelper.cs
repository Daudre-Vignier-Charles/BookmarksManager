using Microsoft.Win32;
using System;

namespace BookmarksManager.classes
{
    internal class RegistryHelper
    {
        private const string path = @"SOFTWARE\BookmarksManager";

        public RegistryHelper()
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(path);

            if (registryKey is null)
            {
                registryKey = Registry.CurrentUser.CreateSubKey(path);
                registryKey.SetValue("guid", Guid.NewGuid().ToString());
            }
            else
            {
                try
                {
                    registryKey.GetValue("guid").ToString();
                }
                catch (NullReferenceException)
                {
                    registryKey = Registry.CurrentUser.CreateSubKey(path);
                    registryKey.SetValue("guid", Guid.NewGuid().ToString());
                }
            }
        }

        public string GetGuid()
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(path);
            return registryKey.GetValue("guid").ToString();
        }
    }
}
