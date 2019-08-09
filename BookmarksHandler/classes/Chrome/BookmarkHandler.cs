﻿using System;
using System.IO;
using System.Windows;
using System.Configuration;
using System.Text;
using Newtonsoft.Json;
using BookmarksHandler.BookmarkBase;

namespace BookmarksHandler.Chrome
{
    internal class BookmarkHandler
    {

        private readonly string BookmarkFolderName = Settings.BookmarkFolderName;

        private static string BookmarkFilePath =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
            @"\Google\Chrome\User Data\Default\Bookmarks";

        private Newtonsoft.Json.Linq.JObject ChromeBookmarks;

        private Newtonsoft.Json.Linq.JArray Childrens;

        public BookmarkHandler()
        {
            if (!File.Exists(BookmarkFilePath))
            {
                MessageBox.Show("Chrome bookmark file not found, create new file.",
                    "Info",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                File.WriteAllText(BookmarkFilePath, Encoding.Default.GetString(Properties.Resources.ChromeBookmarkOrigin));
            }

            bool exist = false;
            ChromeBookmarks = Deserialize();
            foreach (Newtonsoft.Json.Linq.JObject jsonObject in ChromeBookmarks["roots"]["bookmark_bar"]["children"])
            {
                if (jsonObject["name"].ToString() == BookmarkFolderName)
                {
                    exist = true;
                    Childrens = (Newtonsoft.Json.Linq.JArray)jsonObject["children"];
                }
            }
            if (!exist)
            {
                Newtonsoft.Json.Linq.JArray jsonArray =
                    (Newtonsoft.Json.Linq.JArray)ChromeBookmarks["roots"]["bookmark_bar"]["children"]; ;
                Newtonsoft.Json.Linq.JObject jsonObject =
                    Newtonsoft.Json.Linq.JObject.Parse(
                        String.Format("{{\"children\": [  ], \"name\": \"{0}\", \"type\": \"folder\" }}", BookmarkFolderName));
                jsonArray.Add(jsonObject);
                Childrens = (Newtonsoft.Json.Linq.JArray)jsonObject["children"];
            }
        }

        private static Newtonsoft.Json.Linq.JObject Deserialize() =>
            (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(System.IO.File.ReadAllText(BookmarkFilePath));

        public void Apply()
        {
            File.WriteAllText(BookmarkFilePath, JsonConvert.SerializeObject(ChromeBookmarks, Formatting.Indented));
        }

        public void AddBookmark(Bookmark bookmark)
        {
            Childrens.Add(Newtonsoft.Json.Linq.JObject.Parse(
                String.Format("{{\"name\": \"{0}\", \"type\": \"url\", \"url\": \"{1}\"}}", bookmark.Name, bookmark.Url)));
        }

        public void RemoveBookmark(Bookmark bookmark)
        {
            foreach (Newtonsoft.Json.Linq.JObject Child in Childrens)
            {
                if (Child["name"].ToString() == bookmark.Name)
                {
                    Childrens.Remove(Child);
                    return;
                }
            }
        }

        public bool BookmarkExist(string bookmarkName)
        {
            foreach (Newtonsoft.Json.Linq.JObject jObject in Childrens)
            {
                if (jObject["name"].ToString() == bookmarkName)
                    return true;
            }
            return false;
        }
    }
}
