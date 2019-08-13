using System;
using BookmarksManager.BookmarkBase;

namespace BookmarksManager.Firefox
{
    internal class BookmarkHandler : BookmarkHandlerBase
    {
        DatabaseRequest request;
        Ini ini = new Ini();

        public BookmarkHandler()
        {
            try
            {
                request = new DatabaseRequest(ini.bookmarkDatabasePath, Settings.BookmarkFolderName);
            }
            catch (Exception e)
            {
                throw new BookmarkHandlerInitializationException(Browser.Firefox, e.Message);
            }
        }

        internal override void AddBookmark(Bookmark bookmark) =>
            request.AddBookmark(bookmark.Url, bookmark.Name);

        internal override void DeleteBookmark(Bookmark bookmark) =>
            request.DeleteBookmark(bookmark.Name);

        internal override bool BookmarkExist(string bookmarkName) =>
            request.BookmarkExist(bookmarkName);
    }
}
