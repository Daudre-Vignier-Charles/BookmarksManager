using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using BookmarksManager.BookmarkBase;

namespace BookmarksManager
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region DECLARE
        bool applyOnExit = true;
        bool bookmarkAdded = false;

        Bookmarks bookmarks;
        Dictionary<Browser, bool> initializationSuccessful = new Dictionary<Browser, bool>()
        {
            { Browser.Chrome, true },
            { Browser.Firefox, true },
            { Browser.IE, true }
        };

        Firefox.BookmarkHandler firefoxBookmarkHandler;
        Chrome.BookmarkHandler chromeBookmarkHandler;
        IE.BookmarkHandler ieBookmarkHandler;
        #endregion DECLARE

        public MainWindow()
        {
            InitializeComponent();

            #region CHECK
            // Check running browsers
            // IE does not need any check, bookmarks can be removed and added without closing IE
            if (CheckRun(Browser.Chrome))
                initializationSuccessful[Browser.Chrome] = false;
            if (CheckRun(Browser.Firefox))
                initializationSuccessful[Browser.Firefox] = false;

            // Check BookmarkManager settings
            CheckSettings();
            #endregion CHECK

            #region INIT
            // Init Handlers
            try
            {
                firefoxBookmarkHandler = initializationSuccessful[Browser.Firefox] ? new Firefox.BookmarkHandler() : null;
            }
            catch (BookmarkHandlerInitializationException e)
            {
                MessageBox.Show(e.Message, BookmarkHandlerInitializationException.title, MessageBoxButton.OK, MessageBoxImage.Warning);
                initializationSuccessful[Browser.Firefox] = false;
            }
            try
            {
                chromeBookmarkHandler = initializationSuccessful[Browser.Chrome] ? new Chrome.BookmarkHandler() : null;
            }
            catch (BookmarkHandlerInitializationException e)
            {
                MessageBox.Show(e.Message, BookmarkHandlerInitializationException.title, MessageBoxButton.OK, MessageBoxImage.Warning);
                initializationSuccessful[Browser.Chrome] = false;
            }
            // IE initialization cannot fail
            ieBookmarkHandler = new IE.BookmarkHandler();
            #endregion INIT

            // init bookmarks
            bookmarks = new Bookmarks(chromeBookmarkHandler, firefoxBookmarkHandler, ieBookmarkHandler);
            bookmarks.Deserialize();

            // UI
            UIBuilder();
        }

        private void UIBuilder()
        {
            foreach (Bookmark bookmark in bookmarks.allBookmarks.Values)
            {
                UIInsertBookmark(bookmark);
            }
        }

        private void UIInsertBookmark(Bookmark bookmark)
        {
            NStackPanelBuilder(bookmark.Name);
            BStackPanelBuilder(IEStackPanel, Browser.IE, bookmark);
            BStackPanelBuilder(FirefoxStackPanel, Browser.Firefox, bookmark);
            BStackPanelBuilder(ChromeStackPanel, Browser.Chrome, bookmark);
        }

        private void BStackPanelBuilder(StackPanel stackPanel, Browser browser, Bookmark bookmark)
        {
            stackPanel.Children.Add(new Separator());
            stackPanel.Children.Add(CheckBoxBuilder(browser, bookmark));
        }

        private void NStackPanelBuilder(string name)
        {
            NameStackPanel.Children.Add(new Separator());
            NameStackPanel.Children.Add(new Label() { Content = name, Height = 26 });
        }

        private CheckBox CheckBoxBuilder(Browser browser, Bookmark bookmark)
        {
            CheckBox checkBox = new CheckBox()
            {
                IsThreeState = false,
                Tag = bookmark.Name,
                Height = 20,
                Margin = new Thickness(0, 6, 0, 0),
                Opacity = initializationSuccessful[browser] ? 100 : 0
            };
            switch (browser)
            {
                case Browser.Chrome:
                    checkBox.IsEnabled = bookmark.Chrome;
                    checkBox.IsChecked = bookmark.ChromeExist;
                    checkBox.Checked += ChromeCheckBox_Checked;
                    checkBox.Unchecked += ChromeCheckBox_Unchecked;
                    break;
                case Browser.IE:
                    checkBox.IsEnabled = bookmark.IE;
                    checkBox.IsChecked = bookmark.IEExist;
                    checkBox.Checked += IECheckBox_Checked;
                    checkBox.Unchecked += IECheckBox_Unchecked;
                    break;
                case Browser.Firefox:
                    checkBox.IsEnabled = bookmark.Firefox;
                    checkBox.IsChecked = bookmark.FirefoxExist;
                    checkBox.Checked += FirefoxCheckBox_Checked;
                    checkBox.Unchecked += FirefoxCheckBox_Unchecked;
                    break;
            }
            return checkBox;
        }

        private void IECheckBox_Checked(object sender, RoutedEventArgs e) =>
            ieBookmarkHandler.AddBookmark(bookmarks.allBookmarks[((CheckBox)sender).Tag.ToString()]);

        private void IECheckBox_Unchecked(object sender, RoutedEventArgs e) =>
            ieBookmarkHandler.DeleteBookmark(bookmarks.allBookmarks[((CheckBox)sender).Tag.ToString()]);

        private void ChromeCheckBox_Checked(object sender, RoutedEventArgs e) =>
            chromeBookmarkHandler.AddBookmark(bookmarks.allBookmarks[((CheckBox)sender).Tag.ToString()]);

        private void ChromeCheckBox_Unchecked(object sender, RoutedEventArgs e) =>
            chromeBookmarkHandler.DeleteBookmark(bookmarks.allBookmarks[((CheckBox)sender).Tag.ToString()]);

        private void FirefoxCheckBox_Checked(object sender, RoutedEventArgs e) =>
            firefoxBookmarkHandler.AddBookmark(bookmarks.allBookmarks[((CheckBox)sender).Tag.ToString()]);

        private void FirefoxCheckBox_Unchecked(object sender, RoutedEventArgs e) =>
            firefoxBookmarkHandler.DeleteBookmark(bookmarks.allBookmarks[((CheckBox)sender).Tag.ToString()]);

        private void Copyright_Click(object sender, RoutedEventArgs e) =>
            MessageBox.Show(Properties.Resources.Copyright, "Copyright", MessageBoxButton.OK, MessageBoxImage.Information);

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (applyOnExit)
                chromeBookmarkHandler.Apply();
            if (bookmarkAdded)
                if (MessageBox.Show("Bookmark was added.\nWould you want save ?", "Save modifications",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    bookmarks.Serialize();
        }
        private bool CheckRun(Browser browser)
        {
            while (true)
            {
                if (Browsers.IsRunning(browser))
                {
                    MessageBoxResult result = MessageBox.Show(
                        String.Format("{0} browser is running, please close it and press OK.\nRe-open it after exiting BookmarksManager", browser.ToString()),
                        String.Format("Please close {0} browser", browser.ToString()), MessageBoxButton.OKCancel, MessageBoxImage.Warning);
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

        private void AddBookmarkButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(AddBookmarkName.Text))
            {
                MessageBox.Show("Name cannot be empty.", "Error while adding bookmark",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }
            foreach (Bookmark bookmark in bookmarks.allBookmarks.Values)
            {
                if (bookmark.Name == AddBookmarkName.Text)
                {
                    MessageBox.Show("Bookmark already exist",
                        "Error while adding bookmark",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }
            }
            if (!CheckUrl(AddBookmarkUrl.Text))
            {
                MessageBox.Show("Url cannot be empty and must begin with \"http://\" or \"https://\".", "Error while adding bookmark",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }
            bookmarks.allBookmarks[AddBookmarkName.Text] = bookmarks.BuildBoomark(
                AddBookmarkName.Text,
                AddBookmarkUrl.Text,
                (bool)AddBookmarkIe.IsChecked,
                (bool)AddBookmarkChrome.IsChecked,
                (bool)AddBookmarkFirefox.IsChecked);
            UIInsertBookmark(bookmarks.allBookmarks[AddBookmarkName.Text]);
            bookmarkAdded = true;
        }

        private bool CheckUrl(string url) =>
            (url.ToLower().StartsWith("http://") && url.Length > 8) || (url.ToLower().StartsWith("https://") && url.Length > 9);
    }
}
