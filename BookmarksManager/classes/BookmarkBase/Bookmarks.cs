using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace BookmarksManager.BookmarkBase
{
    /// <summary>
    /// Build Bookmark objects from user defined bookmarkList.xml
    /// </summary>
    internal class Bookmarks
    {
        private static string xmlFile = "bookmarkList.xml";
        private Firefox.BookmarkHandler firefoxBookmarkHandler;
        private Chrome.BookmarkHandler chromeBookmarkHandler;
        private IE.BookmarkHandler ieBookmarkHandler;
        internal Dictionary<string, Bookmark> allBookmarks = new Dictionary<string, Bookmark>();
        XmlSerializer serializer = new XmlSerializer(typeof(bookmarks));

        public Bookmarks(Chrome.BookmarkHandler chromeBookmarkHandler, Firefox.BookmarkHandler firefoxBookmarkHandler, IE.BookmarkHandler ieBookmarkHandler)
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
            using (FileStream stream = new FileStream(xmlFile, FileMode.Create))
            {
                serializer.Serialize(stream, bm);
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
