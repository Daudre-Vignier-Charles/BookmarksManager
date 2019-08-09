using System;
using System.IO;
using BookmarksHandler.BookmarkBase;

namespace BookmarksHandler.IE
{
    internal static class BookmarkHandler
    {
        private static string BookmarkPath = Environment.GetFolderPath(Environment.SpecialFolder.Favorites);

        internal static void AddBookmark(Bookmark bookmark)
        {
            using (StreamWriter writer = new StreamWriter(BookmarkPath + "\\" + bookmark.Name + ".url"))
            {
                writer.WriteLine("[InternetShortcut]");
                writer.WriteLine("URL=" + bookmark.Url);
                writer.Flush();
            }
        }

        internal static void RemoveBookmark(string bookmarkName) =>
            File.Delete(BookmarkHandler.FullLoc(bookmarkName));

        internal static bool BookmarkExist(string bookmarkName) =>
            File.Exists(FullLoc(bookmarkName));

        internal static string FullLoc(string bookmarkName) =>
            Path.Combine(BookmarkPath, bookmarkName + ".url");
    }
}
