using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using BookmarksManager.BookmarkBase;
using ieBookmarkHandler = BookmarksManager.IE.BookmarkHandler;


namespace BookmarksManager
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

            // UI
            UIBuilder();
        }

        private enum Browser
        {
            IE,
            Chrome,
            Firefox
        }

        private void UIBuilder()
        {
            foreach (KeyValuePair<string, Bookmark> bookmark in allBookmarks)
            {
                NameStackPanel.Children.Add(new Separator());
                NameStackPanel.Children.Add(new Label() { Content = bookmark.Value.Name, Height = 26 });
                CheckBox IECheckBox = CheckBoxBuilder(Browser.IE, bookmark);
                IEStackPanel.Children.Add(new Separator());
                IEStackPanel.Children.Add(IECheckBox);
                CheckBox ChromeCheckBox = CheckBoxBuilder(Browser.Chrome, bookmark);
                ChromeStackPanel.Children.Add(new Separator());
                ChromeStackPanel.Children.Add(ChromeCheckBox);
                CheckBox FirefoxCheckBox = CheckBoxBuilder(Browser.Firefox, bookmark);
                FirefoxStackPanel.Children.Add(new Separator());
                FirefoxStackPanel.Children.Add(FirefoxCheckBox);
            }
        }

        private CheckBox CheckBoxBuilder(Browser browser, KeyValuePair<string, Bookmark> bookmark)
        {
            CheckBox checkBox = new CheckBox()
            {
                IsThreeState = false,
                Tag = bookmark.Value.Name,
                Height = 20,
                Margin = new Thickness(0, 6, 0, 0)
            };
            switch (browser)
            {
                case Browser.Chrome:
                    checkBox.IsEnabled = bookmark.Value.Chrome;
                    checkBox.IsChecked = bookmark.Value.ChromeExist;
                    checkBox.Checked += ChromeCheckBox_Checked;
                    checkBox.Unchecked += ChromeCheckBox_Unchecked;
                    break;
                case Browser.IE:
                    checkBox.IsEnabled = bookmark.Value.IE;
                    checkBox.IsChecked = bookmark.Value.IEExist;
                    checkBox.Checked += IECheckBox_Checked;
                    checkBox.Unchecked += IECheckBox_Unchecked;
                    break;
                case Browser.Firefox:
                    checkBox.IsEnabled = bookmark.Value.Firefox;
                    checkBox.IsChecked = bookmark.Value.FirefoxExist;
                    checkBox.Checked += FirefoxCheckBox_Checked;
                    checkBox.Unchecked += FirefoxCheckBox_Unchecked;
                    break;
            }
            return checkBox;
        }

        private void IECheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            ieBookmarkHandler.AddBookmark(allBookmarks[cb.Tag.ToString()]);
        }

        private void IECheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            ieBookmarkHandler.RemoveBookmark(cb.Tag.ToString());
        }

        private void ChromeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            chromeBookmarkHandler.AddBookmark(allBookmarks[cb.Tag.ToString()]);
        }

        private void ChromeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            chromeBookmarkHandler.RemoveBookmark(allBookmarks[cb.Tag.ToString()]);
        }

        private void FirefoxCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            firefoxBookmarkHandler.AddBookmark(allBookmarks[cb.Tag.ToString()]);
        }

        private void FirefoxCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            firefoxBookmarkHandler.DeleteBookmark(allBookmarks[cb.Tag.ToString()]);
        }

        private void Copyright_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(Properties.Resources.Copyright, "Copyright", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            chromeBookmarkHandler.Apply();
        }
    }
}
