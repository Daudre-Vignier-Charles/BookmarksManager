using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using BookmarksManager.BookmarkBase;
using System.Drawing;

namespace BookmarksManager
{

    internal enum Browser
    {
        IE,
        Chrome,
        Firefox
    }
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool applyOnExit = true;

        Bookmarks bookmarks = new Bookmarks();
        Dictionary<string, Bookmark> allBookmarks;
        Dictionary<Browser, bool> initializationSuccessful = new Dictionary<Browser, bool>()
        {
            { Browser.Chrome, true },
            { Browser.Firefox, true },
            { Browser.IE, true }
        };

        Firefox.BookmarkHandler firefoxBookmarkHandler;
        Chrome.BookmarkHandler chromeBookmarkHandler;
        IE.BookmarkHandler ieBookmarkHandler;

        public MainWindow()
        {
            InitializeComponent();

            if (!CheckRun()) //Warning debug
            {
                this.Close();
                return;
            }

            CheckSettings();

            // Init Handlers
            try
            {
                firefoxBookmarkHandler = new Firefox.BookmarkHandler();
            }
            catch (BookmarkHandlerInitializationException e)
            {
                MessageBox.Show(e.Message, BookmarkHandlerInitializationException.title, MessageBoxButton.OK, MessageBoxImage.Warning);
                initializationSuccessful[Browser.Firefox] = false;
            }
            try
            {
                chromeBookmarkHandler = new Chrome.BookmarkHandler();
            }
            catch (BookmarkHandlerInitializationException e)
            {
                MessageBox.Show(e.Message, BookmarkHandlerInitializationException.title, MessageBoxButton.OK, MessageBoxImage.Warning);
                initializationSuccessful[Browser.Chrome] = false;
            }
            try
            {
                ieBookmarkHandler = new IE.BookmarkHandler();
            }
            catch (BookmarkHandlerInitializationException e)
            {
                MessageBox.Show(e.Message, BookmarkHandlerInitializationException.title, MessageBoxButton.OK, MessageBoxImage.Warning);
                initializationSuccessful[Browser.IE] = false;
            }

            // get all bookmarks
            allBookmarks = bookmarks.Deserialize(chromeBookmarkHandler, firefoxBookmarkHandler, ieBookmarkHandler);

            // UI
            UIBuilder();
        }

        private void UIBuilder()
        {
            foreach (KeyValuePair<string, Bookmark> bookmark in allBookmarks)
            {
                NStackPanelBuilder(bookmark.Key);
                // TODO : Link x.BookmarkHandler fatal error, does not build if error:x.BookmarkHandler
                BStackPanelBuilder(IEStackPanel, Browser.IE, bookmark);
                BStackPanelBuilder(FirefoxStackPanel, Browser.Firefox, bookmark);
                BStackPanelBuilder(ChromeStackPanel, Browser.Chrome, bookmark);
            }
        }

        private void BStackPanelBuilder(StackPanel stackPanel, Browser browser, KeyValuePair<string, Bookmark> bookmark)
        {
            stackPanel.Children.Add(new Separator());
            stackPanel.Children.Add(CheckBoxBuilder(browser, bookmark));
        }

        private void NStackPanelBuilder(string name)
        {
            NameStackPanel.Children.Add(new Separator());
            NameStackPanel.Children.Add(new Label() { Content = name, Height = 26 });
        }

        private CheckBox CheckBoxBuilder(Browser browser, KeyValuePair<string, Bookmark> bookmark)
        {
            CheckBox checkBox = new CheckBox()
            {
                IsThreeState = false,
                Tag = bookmark.Value.Name,
                Height = 20,
                Margin = new Thickness(0, 6, 0, 0),
            };
            if (!initializationSuccessful[browser])
            {
                checkBox.Opacity = 0;
            }
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
            ieBookmarkHandler.DeleteBookmark(allBookmarks[cb.Tag.ToString()]);
        }

        private void ChromeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            chromeBookmarkHandler.AddBookmark(allBookmarks[cb.Tag.ToString()]);
        }

        private void ChromeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            chromeBookmarkHandler.DeleteBookmark(allBookmarks[cb.Tag.ToString()]);
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
            if (applyOnExit)
            {
                chromeBookmarkHandler.Apply();
            }
        }

        private bool CheckRun()
        {
            while (true)
            {
                if (Browsers.IsRunning(Browser.Chrome) || Browsers.IsRunning(Browser.Firefox))
                {
                    MessageBoxResult result = MessageBox.Show(
                        "Chrome or Firefox browser is running, please close it and press OK.\nRe-open it after exiting BookmarksManager",
                        "Please close Firefox and Chrome browser", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Cancel)
                    {
                        applyOnExit = false;
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
       }

        private void CheckSettings()
        {
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
        }
    }
}
