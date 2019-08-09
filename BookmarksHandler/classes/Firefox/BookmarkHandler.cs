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

        internal void AddBookmark(Bookmark bookmark) =>
            request.AddBookmark(bookmark.Url, bookmark.Name);

        internal void DeleteBookmark(Bookmark bookmark) =>
            request.DeleteBookmark(bookmark.Name);

        internal bool BookmarkExist(string name) =>
            request.BookmarkExist(name);
    }
}
