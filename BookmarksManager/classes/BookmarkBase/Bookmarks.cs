using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Windows;

namespace BookmarksManager.BookmarkBase
{
    /// <summary>
    /// Build Bookmark objects from user defined bookmarkList.xml
    /// </summary>
    internal class Bookmarks
    {
        private static string xmlFile = "bookmarkList.xml";
        private BookmarkHandlerBase firefoxBookmarkHandler;
        private BookmarkHandlerBase chromeBookmarkHandler;
        private BookmarkHandlerBase ieBookmarkHandler;
        internal Dictionary<string, Bookmark> allBookmarks = new Dictionary<string, Bookmark>();
        XmlSerializer serializer = new XmlSerializer(typeof(bookmarks));

        public Bookmarks(BookmarkHandlerBase ieBookmarkHandler, BookmarkHandlerBase chromeBookmarkHandler, BookmarkHandlerBase firefoxBookmarkHandler)
        {
            this.ieBookmarkHandler = ieBookmarkHandler;
            this.firefoxBookmarkHandler = firefoxBookmarkHandler;
            this.chromeBookmarkHandler = chromeBookmarkHandler;
        }

        internal void Deserialize()
        {
            bookmarks bookmarks = null;
            bookmarksBookmark[] bookmarkList;
            using (FileStream stream = new FileStream(xmlFile, FileMode.Open))
            {
                bookmarks = (bookmarks)serializer.Deserialize(stream);
            }
            bookmarkList = bookmarks.bookmark;
            foreach (bookmarksBookmark bookmark in bookmarkList)
            {
                allBookmarks[bookmark.name] = BuildBoomark(
                    bookmark.name,
                    bookmark.url,
                    (bookmark.ie || !bookmark.ieSpecified) && ieBookmarkHandler != null ? true : false,
                    (bookmark.chrome || !bookmark.chromeSpecified) && chromeBookmarkHandler != null ? true : false,
                    (bookmark.firefox || !bookmark.firefoxSpecified) && firefoxBookmarkHandler != null ? true : false);
            }
        }

        public void Serialize()
        {
            bookmarks bm = new bookmarks();
            List<bookmarksBookmark> abm = new List<bookmarksBookmark>();
            foreach (Bookmark bookmark in allBookmarks.Values)
            {
                abm.Add(
                    new bookmarksBookmark()
                    {
                        name = bookmark.Name,
                        url = bookmark.Url,
                        ie = bookmark.IE,
                        ieSpecified = true,
                        chrome = bookmark.Chrome,
                        chromeSpecified = true,
                        firefox = bookmark.Firefox,
                        firefoxSpecified = true
                    });
            }
            bm.bookmark = abm.ToArray();
            try
            {
                using (FileStream stream = new FileStream(xmlFile, FileMode.Create))
            {
                serializer.Serialize(stream, bm);
            }
            }
            catch(System.UnauthorizedAccessException)
            {
                MessageBox.Show("bookmarkList.xml cannot be edited. Change will not be saved.");
            }
        }

        internal Bookmark BuildBoomark(string name, string url, bool ie, bool chrome, bool firefox) =>
            new Bookmark()
            {
                Name = name,
                Url = url,
                ChromeExist = chromeBookmarkHandler?.BookmarkExist(name) ?? false,
                FirefoxExist = firefoxBookmarkHandler?.BookmarkExist(name) ?? false,
                IEExist = ieBookmarkHandler?.BookmarkExist(name) ?? false,
                IE = ie,
                Chrome = chrome,
                Firefox = firefox
            };
    }
}
