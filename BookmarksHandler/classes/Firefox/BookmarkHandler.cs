using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookmarksHandler.BookmarkBase;
using BookmarksHandler.Firefox.Base;

namespace BookmarksHandler.Firefox
{
    internal class BookmarkHandler
    {
        DatabaseRequest request;
        Ini ini = new Ini();

        public BookmarkHandler()
        {
            request = new DatabaseRequest(ini.bookmarkDatabasePath, Settings.BookmarkFolderName);
        }

        internal void AddBookmark(string name, string url) =>
            request.AddBookmark(url, name);

        internal void DeleteBookmark(string name) =>
            request.DeleteBookmark(name);

        internal bool BookmarkExist(string name) =>
            request.BookmarkExist(name);
    }
}
