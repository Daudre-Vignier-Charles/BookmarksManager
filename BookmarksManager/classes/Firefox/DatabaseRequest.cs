using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Xml.Linq;

namespace BookmarksManager.Firefox
{
    /// <summary>
    /// Firefox store bookmarks using a SqLite database.
    /// Classe is used to query database.
    /// </summary>
    internal class DatabaseRequest
    {
        SQLiteConnection databaseConnection;
        SQLiteCommand command;
        Dictionary<string, string> commandRepository = new Dictionary<string, string>();

        private string folderName;
        private string folderId;

        public DatabaseRequest(string databasePath, string bookmarkFolderName)
        {
            this.folderName = bookmarkFolderName;

            // Open database
            databaseConnection = new SQLiteConnection(String.Format("Data Source={0};Version=3;", databasePath));
            databaseConnection.Open();
            command = new SQLiteCommand() { Connection = databaseConnection };

            // Setup commandRepository
            foreach (XElement element in XElement.Parse(BookmarksManager.Properties.Resources.FirefoxSqliteCommands).Elements())
                commandRepository.Add(element.Name.LocalName, element.Value);

            // Check if bookmark folder is ok and get Id
            if (!BookmarkFolderExist())
            {
                AddBookMarkFolder();
            }
            folderId = GetBookmarkFolderId();
        }

        /// <summary>
        /// Return fk's id.
        /// Fk is the bookmark url stored into moz_places table.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private string GetFkId(string url, string name)
        {
            string id;
            command.CommandText = String.Format(commandRepository["getFkId"], name, url);
            SQLiteDataReader reader = command.ExecuteReader();
            reader.Read();
            if (reader.HasRows)
                id = reader["id"].ToString();
            else
                id = null;
            reader.Close();
            return id;
        }

        /// <summary>
        /// Add a new fk and return his id.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private string AddFk(string url, string name)
        {
            command.CommandText = String.Format(commandRepository["addFk"], url, name, FfGuid.Get(), FfHash.Get(url));
            command.ExecuteNonQuery();
            return GetFkId(url, name);
        }

        internal void AddBookmark(string url, string name)
        {
            string fkId = GetFkId(url, name);
            if (String.IsNullOrEmpty(fkId))
                fkId = AddFk(url, name);
            command.CommandText = String.Format(commandRepository["addBookmark"], folderId, name, FfGuid.Get(), FfTimestamp.Get(), FfTimestamp.Get(), fkId);
            command.ExecuteNonQuery();
        }

        internal void DeleteBookmark(string name)
        {
            command.CommandText = String.Format(commandRepository["deleteBookmark"], name, folderId);
            command.ExecuteNonQuery();
        }

        private bool BExist(string name, string parentId, bookmarkType type)
        {
            command.CommandText = String.Format(commandRepository["bookmarkExist"], name, (int)type, parentId);
            SQLiteDataReader reader = command.ExecuteReader();
            reader.Read();
            bool exist = reader.HasRows;
            reader.Close();
            return exist;
        }

        internal bool BookmarkExist(string name) =>
            BExist(name, folderId, bookmarkType.Bookmark);

        private bool BookmarkFolderExist() =>
            BExist(folderName, "3", bookmarkType.Folder);

        private void AddBookMarkFolder()
        {
            command.CommandText = String.Format(commandRepository["addBookmarkFolder"], folderName, FfGuid.Get(), FfTimestamp.Get(), FfTimestamp.Get());
            command.ExecuteNonQuery();
        }

        private string GetBookmarkFolderId()
        {
            command.CommandText = String.Format(commandRepository["getBookmarkFolderId"], folderName);
            SQLiteDataReader reader = command.ExecuteReader();
            reader.Read();
            string id = reader["id"].ToString();
            reader.Close();
            return id;
        }

        private enum bookmarkType
        {
            Bookmark = 1,
            Folder = 2,
            Separator = 3
        }
    }

    /// <summary>
    /// Return a random 12 bytes GUID.
    /// </summary>
    internal static class FfGuid
    {
        private static readonly Random rng = new Random();
        private const string str = "abcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ-";

        /// <summary>
        /// Return a random 12 bytes GUID
        /// </summary>
        static internal string Get()
        {
            string randomStr = "";
            for (int i = 1; i <= 12; i++)
                randomStr += str[rng.Next(str.Length)];
            return randomStr;
        }
    }

    /// <summary>
    /// Firefox 50+ database url hash.
    /// This code is a C# port of :
    /// C version https://github.com/bencaradocdavies/sqlite-mozilla-url-hash and
    /// Python version https://gist.github.com/boppreh/a9737acb2abf015e6e828277b40efe71 .
    /// Thanks to bencaradocdavies and boppreh.
    /// </summary>
    internal static class FfHash
    {
        private const uint magicConst = 2654435769;

        private static uint RotateLeft5(uint value) =>
            (value << 5) | (value >> 27) & uint.MaxValue;

        private static uint AddToHash(uint hash, uint value) =>
            (magicConst * (RotateLeft5(hash) ^ value)) & uint.MaxValue;

        private static uint Hash(string url)
        {
            uint hash = 0;
            foreach (char c in url)
                hash = AddToHash(hash, c);
            return hash;
        }

        /// <summary>
        /// Return url's hash.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        internal static long Get(string url) =>
            ((long)(Hash(url.Substring(0, url.IndexOf(':'))) & 65535) << 32) + Hash(url);
    }


    /// <summary>
    /// Firefox use Unix Epoch format (microsecond).
    /// </summary>
    internal static class FfTimestamp
    {
        /// <summary>
        /// Return timestamp in Unix Epoch format (microsecond).
        /// </summary>
        static internal long Get() =>
            (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds * 1000;
    }
}
