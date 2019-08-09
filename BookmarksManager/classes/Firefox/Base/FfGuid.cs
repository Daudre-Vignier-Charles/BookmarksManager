using System;

namespace BookmarksManager.Firefox.Base
{
    internal static class FfGuid
    {
        private static readonly Random rng = new Random();
        private const string str = "abcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ-";

        /// <summary>
        /// Return random GUID
        /// </summary>
        static internal string Get()
        {
            string randomStr = "";
            for (int i = 1; i <= 12; i++)
                randomStr += str[rng.Next(str.Length)];
            return randomStr;
        }
    }
}
