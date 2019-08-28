using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using BookmarksManager.BookmarkBase;
using System.Windows.Input;

namespace BookmarksManager
{
    public partial class MainWindow : Window
    {
        bool mutexInitialOwnership;
        static System.Threading.Mutex mutex;
        bool mutexOwner;

        bool fatalError = false;

        static readonly string[] protos = new string[] { "http", "https", "ftp" };
        bool bookmarksModified = false;

        Bookmarks bookmarks;

        internal class BookmarkHandlerInfos
        {
            internal BookmarkHandlerBase handler;
            internal Type handlerType = null;
            internal StackPanel stackPanel;
            internal bool initializationSuccessful = true;
        }

        internal class UIInfo
        {
            internal string name;
            internal Browser browser;
        }

        Dictionary<Browser, BookmarkHandlerInfos> infos = new Dictionary<Browser, BookmarkHandlerInfos>();

        public MainWindow()
        {
            InitializeComponent();

            // Check singleton violation
            mutex = new System.Threading.Mutex(true, "{00358e6-2dcd-49e2-8874-f6fde19994cd}", out mutexInitialOwnership);
            if (!mutexInitialOwnership)
            {
                MessageBox.Show("only one instance at a time");
                fatalError = true;
                this.Close();
                return;
            }

            // Check BookmarkManager settings
            CheckSettings();

            // Populate bookmark handlers and browsers infos
            infos[Browser.IE] = new BookmarkHandlerInfos() { stackPanel = IEStackPanel, handlerType=typeof(IE.BookmarkHandler)};
            infos[Browser.Chrome] = new BookmarkHandlerInfos() { stackPanel = ChromeStackPanel, handlerType = typeof(Chrome.BookmarkHandler)};
            infos[Browser.Firefox] = new BookmarkHandlerInfos() { stackPanel = FirefoxStackPanel, handlerType = typeof(Firefox.BookmarkHandler)};

            // Check running browser, fail if user click cancel or exit
            if (CheckRun(Browser.Chrome))
                infos[Browser.Chrome].initializationSuccessful = false;
            if (CheckRun(Browser.Firefox))
                infos[Browser.Firefox].initializationSuccessful = false;

            // Initialize bookmark handlers
            foreach (KeyValuePair<Browser, BookmarkHandlerInfos> info in infos)
            {
                try
                {
                    info.Value.handler = info.Value.initializationSuccessful ? (BookmarkHandlerBase)Activator.CreateInstance(info.Value.handlerType) : null;
                }
                catch (BookmarkHandlerInitializationException e)
                {
                    MessageBox.Show(e.Message, BookmarkHandlerInitializationException.title, MessageBoxButton.OK, MessageBoxImage.Warning);
                    info.Value.initializationSuccessful = false;
                }
            }

            // init bookmarks
            bookmarks = new Bookmarks(infos[Browser.IE].handler, infos[Browser.Chrome].handler, infos[Browser.Firefox].handler);

            try
            {
                bookmarks.Deserialize();
            }
            catch (System.IO.IOException)
            {
                MessageBox.Show(
                    "Fatal error while getting bookmark list from bookmarkList.xml, file does not exist or is not accessible.",
                    "Fatal error - file IO",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                fatalError = true;
                this.Close();
                return;
            }

            // UI
            UIBuilder();
        }

        /// <summary>
        /// Chrome and Firefox needs to be closed while using BookmarksManager
        /// </summary>
        /// <param name="browser"></param>
        /// <returns></returns>
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
                err += " Setting default BookmarkFolderName=\"Corporate bookmarks\"";
                MessageBox.Show(err, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                Settings.BookmarkFolderName = "Corporate bookmarks";
            }
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
            NameStackPanelBuilder(bookmark.Name);
            foreach (KeyValuePair<Browser, BookmarkHandlerInfos> info in infos)
            {
                CheckBoxStackPanelBuilder(info.Value.stackPanel, info.Key, bookmark);
            }
            DeleteStackPanelBuilder(bookmark.Name);
        }

        private void NameStackPanelBuilder(string name)
        {
            NameStackPanel.Children.Add(new Separator());
            NameStackPanel.Children.Add(new Label() { Content = name, Height = 26 });
        }

        private void CheckBoxStackPanelBuilder(StackPanel stackPanel, Browser browser, Bookmark bookmark)
        {
            stackPanel.Children.Add(new Separator());
            stackPanel.Children.Add(CheckBoxBuilder(browser, bookmark));
        }

        private CheckBox CheckBoxBuilder(Browser browser, Bookmark bookmark)
        {
            CheckBox checkBox = new CheckBox()
            {
                IsThreeState = false,
                Tag = new UIInfo() { name=bookmark.Name, browser=browser },
                Height = 20,
                Margin = new Thickness(0, 6, 0, 0),
                Opacity = infos[browser].initializationSuccessful ? 100 : 0,
            };
            switch (browser)
            {
                case Browser.Chrome:
                    checkBox.IsEnabled = bookmark.Chrome;
                    checkBox.IsChecked = bookmark.ChromeExist;
                    break;
                case Browser.IE:
                    checkBox.IsEnabled = bookmark.IE;
                    checkBox.IsChecked = bookmark.IEExist;
                    break;
                case Browser.Firefox:
                    checkBox.IsEnabled = bookmark.Firefox;
                    checkBox.IsChecked = bookmark.FirefoxExist;
                    break;
            }
            checkBox.Checked += CheckBox_Switch;
            checkBox.Unchecked += CheckBox_Switch;
            return checkBox;
        }

        private void DeleteStackPanelBuilder(string name)
        {
            DeleteStackPanel.Children.Add(new Separator());
            Button button = new Button()
            {
                Tag = name,
                Height = 15,
                Width = 15,
                Margin = new Thickness(0, 6, 0, 5),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Background = System.Windows.Media.Brushes.IndianRed
            };
            button.Click += DeleteBookmarkButton_Click;
            DeleteStackPanel.Children.Add(button);
        }

        void CheckBox_Switch(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb.Tag is null)
                return;
            UIInfo tag = cb.Tag as UIInfo;
            if (tag is null)
                return;
            if ((bool)cb.IsChecked)
                infos[tag.browser].handler.AddBookmark(bookmarks.allBookmarks[tag.name]);
            else
                infos[tag.browser].handler.DeleteBookmark(bookmarks.allBookmarks[tag.name]);
        }

        private void Copyright_Click(object sender, RoutedEventArgs e) =>
            MessageBox.Show(Properties.Resources.Copyright, "Copyright", MessageBoxButton.OK, MessageBoxImage.Information);

        private void AddBookmarkTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                AddBookmarkButton_Click(this, new RoutedEventArgs());
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
                if (bookmark.Url == AddBookmarkUrl.Text)
                {
                    MessageBox.Show("Bookmark with this url already exist : " + bookmark.Name,
                        "Error while adding bookmark",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }
            }
            if (!IsValidUrl(AddBookmarkUrl.Text))
            {
                MessageBox.Show("Url cannot be empty and must begin with \"http://\", \"https://\" or \"ftp://\".", "Error while adding bookmark",
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
            bookmarksModified = true;
        }

        private bool IsValidUrl(string url)
        {
            if (String.IsNullOrWhiteSpace(url))
                return false;
            string[] urlpart = url.Split(new char[] { ':' }, 2);
            foreach (string proto in protos)
                if (urlpart[0] == proto)
                    if (urlpart[1].StartsWith("//") && urlpart[1].Length > 2)
                        return true;
            return false;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!(mutex is null) && mutexInitialOwnership)
                mutex.ReleaseMutex();
            if (!fatalError)
            {
                if (infos[Browser.Chrome].initializationSuccessful)
                    ((Chrome.BookmarkHandler)infos[Browser.Chrome].handler).Apply();
                if (bookmarksModified)
                    if (MessageBox.Show("Bookmark was modified.\nWould you want to save changes ?", "Save bookmarks changes",
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        bookmarks.Serialize();
            }
        }

        private void DeleteBookmarkButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            foreach (StackPanel stackPanel in new StackPanel[]{ IEStackPanel, ChromeStackPanel, FirefoxStackPanel })
            {
                foreach (object cb in stackPanel.Children)
                {
                    CheckBox checkBox = cb as CheckBox;
                    if (checkBox is null)
                        continue;
                    UIInfo uiInfo = checkBox.Tag as UIInfo;
                    if (uiInfo is null)
                        continue;
                    if (uiInfo.name == (string)button.Tag)
                    {
                        stackPanel.Children.RemoveAt(stackPanel.Children.IndexOf(checkBox) - 1 );
                        stackPanel.Children.Remove(checkBox);
                        break;
                    }
                }
            }
            foreach (object l in NameStackPanel.Children)
            {
                Label label = l as Label;
                if (label is null)
                    continue;
                if (label.Content == button.Tag)
                {
                    NameStackPanel.Children.RemoveAt(NameStackPanel.Children.IndexOf(label) - 1);
                    NameStackPanel.Children.Remove(label);
                    break;
                }
            }
            bookmarks.allBookmarks.Remove((string)button.Tag);
            DeleteStackPanel.Children.RemoveAt(DeleteStackPanel.Children.IndexOf(button) - 1);
            DeleteStackPanel.Children.Remove(button);
            bookmarksModified = true;
        }
    }
}
