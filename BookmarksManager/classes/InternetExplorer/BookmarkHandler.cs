using System;
using System.IO;
using BookmarksManager.BookmarkBase;

namespace BookmarksManager.IE
{
    /// <summary>
    /// IE's bookmarks are managed with lnk file into the favorites special folder.
    /// </summary>
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

        /// <summary>
        /// Get file path for bookmark passed as argument.
        /// </summary>
        /// <param name="bookmarkName"></param>
        /// <returns>bookmark file path</returns>
        internal static string FullLoc(string bookmarkName) =>
            Path.Combine(BookmarkPath, bookmarkName + ".url");
    }
}
