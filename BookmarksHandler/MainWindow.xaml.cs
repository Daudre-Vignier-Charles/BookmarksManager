using BookmarksHandler.BookmarkBase;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using ieBookmarkHandler = BookmarksHandler.IE.BookmarkHandler;

namespace BookmarksHandler
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Bookmarks bookmarks = new Bookmarks();
        Dictionary<string, Bookmark> allBookmarks;
        Firefox.BookmarkHandler firefoxBookmarkHandler;
        Chrome.BookmarkHandler chromeBookmarkHandler;

        public MainWindow()
        {
            InitializeComponent();

            // Check settings
            if (String.IsNullOrEmpty(Settings.BookmarkFolderName))
            {
                string err;
                if (!File.Exists("BookmarksHandler.exe.config"))
                    err = "BookmarksHandler.exe.config not found.";
                else
                    err = "BookmarkFolderName property is not set or BookmarksHandler.exe.config is empty.";
                err += " Setting default ChromeBookmarkFolderName=\"Corporate bookmarks\"";
                MessageBox.Show(err, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                Settings.BookmarkFolderName = "Corporate bookmarks";
            }

            // Init Handlers
            firefoxBookmarkHandler = new Firefox.BookmarkHandler();
            chromeBookmarkHandler = new Chrome.BookmarkHandler();

            // get all bookmarks
            allBookmarks = bookmarks.Deserialize(chromeBookmarkHandler, firefoxBookmarkHandler);
        }
    }
}
