using System;

namespace BookmarksManager.BookmarkBase
{

    /// <summary>
    /// Browser's bookmarks are managed with class built from BookmarkHandlerBase.
    /// Each BookmarkHandler must can check if a bookmark exist and delete or add a bookmark.
    /// </summary>
    /// <exception cref="BookmarkHandlerInitializationException">
    /// Any fatal error into a BookmarkHandler while initializing class throw BookmarkHandlerInitializationException.
    /// </exception>
    internal abstract class BookmarkHandlerBase
    {
        internal abstract void AddBookmark(Bookmark bookmark);

        internal abstract void DeleteBookmark(Bookmark bookmark);

        internal abstract bool BookmarkExist(string bookmarkName);
    }


    /// <summary>
    /// Any fatal error into a BookmarkHandler while initializing class must throw BookmarkHandlerInitializationException>
    /// Previous exception message is passed as argument
    /// </summary>
    [Serializable]
    internal class BookmarkHandlerInitializationException : Exception
    {
        internal static string title = "Initialization Exception";
        internal BookmarkHandlerInitializationException(Browser browser, string exception)
            : base(String.Format("{0}BookmarkHandler Initialization Exception : {1}", browser.ToString(), exception)) { }
    }
}
