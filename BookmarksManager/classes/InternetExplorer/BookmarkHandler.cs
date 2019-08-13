using System;
using System.IO;
using BookmarksManager.BookmarkBase;

namespace BookmarksManager.IE
{
    internal class BookmarkHandler : BookmarkHandlerBase
    {
        private static readonly string BookmarkPath = Environment.GetFolderPath(Environment.SpecialFolder.Favorites);

        internal override void AddBookmark(Bookmark bookmark)
        {
            using (StreamWriter writer = new StreamWriter(BookmarkPath + "\\" + bookmark.Name + ".url"))
            {
                writer.WriteLine("[InternetShortcut]");
                writer.WriteLine("URL=" + bookmark.Url);
                writer.Flush();
            }
        }

        internal override void DeleteBookmark(Bookmark bookmark) =>
            File.Delete(BookmarkHandler.FullLoc(bookmark.Name));

        internal override bool BookmarkExist(string bookmarkName) =>
            File.Exists(FullLoc(bookmarkName));

        internal static string FullLoc(string bookmarkName) =>
            Path.Combine(BookmarkPath, bookmarkName + ".url");
    }
}
