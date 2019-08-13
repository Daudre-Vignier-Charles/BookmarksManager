using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace BookmarksManager.BookmarkBase
{
    internal class Bookmarks
    {
        private static string xmlFile = "bookmarkList.xml";

        public Dictionary<string, Bookmark> Deserialize(Chrome.BookmarkHandler chromeBookmarkHandler, Firefox.BookmarkHandler firefoxBookmarkHandler, IE.BookmarkHandler ieBookmarkHandler)
        {
            bookmarks bookmarks = null;
            bookmarksBookmark[] bookmarkList;
            Dictionary<string, Bookmark> ret = new Dictionary<string, Bookmark>();
            XmlSerializer serializer = new XmlSerializer(typeof(bookmarks));
            using (FileStream stream = new FileStream(xmlFile, FileMode.Open))
            {
                bookmarks = (bookmarks)serializer.Deserialize(stream);
            }
            bookmarkList = bookmarks.bookmark;
            foreach (bookmarksBookmark bookmark in bookmarkList)
            {
                ret[bookmark.name] = new Bookmark()
                {
                    Name = bookmark.name,
                    Url = bookmark.url,
                    ChromeExist = chromeBookmarkHandler?.BookmarkExist(bookmark.name) ?? false,
                    FirefoxExist = firefoxBookmarkHandler?.BookmarkExist(bookmark.name) ?? false,
                    IEExist = ieBookmarkHandler?.BookmarkExist(bookmark.name) ?? false

                };
                ret[bookmark.name].Chrome = (bookmark.chrome || !bookmark.chromeSpecified) && chromeBookmarkHandler != null ? true : false;
                ret[bookmark.name].Firefox = (bookmark.firefox || !bookmark.firefoxSpecified)  && firefoxBookmarkHandler != null ? true : false;
                ret[bookmark.name].IE = (bookmark.ie || !bookmark.ieSpecified) && ieBookmarkHandler != null ? true : false;
            }
            return ret;
        }
    }
}
