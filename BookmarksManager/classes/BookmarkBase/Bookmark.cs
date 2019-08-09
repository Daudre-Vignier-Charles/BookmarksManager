using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace BookmarksManager.BookmarkBase
{
    public class Bookmark
    {
        [XmlElement("name")]
        public string Name;

        [XmlElement("url")]
        public string Url;

        [XmlElement("ie")]
        public bool IE;

        [XmlElement("chrome")]
        public bool Chrome;

        [XmlElement("firefox")]
        public bool Firefox;

        public bool IEExist;

        public bool ChromeExist;

        public bool FirefoxExist;
    }
}