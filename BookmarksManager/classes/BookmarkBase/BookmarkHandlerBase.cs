using System;

namespace BookmarksManager.BookmarkBase
{
    internal abstract class BookmarkHandlerBase
    {
        internal abstract void AddBookmark(Bookmark bookmark);

        internal abstract void DeleteBookmark(Bookmark bookmark);

        internal abstract bool BookmarkExist(string bookmarkName);
    }

    [Serializable]
    internal class BookmarkHandlerInitializationException : Exception
    {
        internal static string title = "Initialization Exception";
        internal BookmarkHandlerInitializationException(Browser browser, string exception)
            : base(String.Format("{0}BookmarkHandler Initialization Exception : {1}", browser.ToString(), exception)) { }
    }
}
