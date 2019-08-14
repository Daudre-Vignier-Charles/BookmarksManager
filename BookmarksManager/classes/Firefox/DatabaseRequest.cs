using System;
using System.Xml.Linq;
using System.Data.SQLite;
using System.Collections.Generic;
using BookmarksManager.Firefox.Base;

namespace BookmarksManager.Firefox
{
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

        private bool BExist(string name, string parentId, string type)
        {
            command.CommandText = String.Format(commandRepository["bookmarkExist"], name, type, parentId);
            SQLiteDataReader reader = command.ExecuteReader();
            reader.Read();
            bool exist = reader.HasRows;
            reader.Close();
            return exist;
        }

        internal bool BookmarkExist(string name) =>
            BExist(name, folderId, "1");

        private bool BookmarkFolderExist() =>
            BExist(folderName, "3", "2");

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
    }
}
