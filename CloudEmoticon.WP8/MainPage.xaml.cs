using CloudEmoticon.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Shell;
using Simon.Library.Controls;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Windows.Networking.Connectivity;

namespace CloudEmoticon
{
    public partial class MainPage : AppPage
    {
        public static EmoticonList EmoticonList { get; private set; }
        public static FavoriteList FavoriteList { get; private set; }
        public static RecentList RecentList { get; private set; }

        public MainPage()
            : base()
        {
            InitializeComponent();

            if (App.Settings["firstStart"] == null)
            {
                App.Settings["favorite"] = new HashSet<string>();
                App.Settings["noteMap"] = new Dictionary<string, string>();
                App.Settings["repositories"] = new HashSet<string>();
                App.Settings["infoMap"] = new Dictionary<string, string>();
                App.Settings["cacheMap"] = new Dictionary<string, string>();
                App.Settings["firstStart"] = false;
                App.Settings.Save();
            }
            // Version 1.0.1
            if (App.Settings["recent"] == null)
            {
                App.Settings["recent"] = new HashSet<string>();
                App.Settings["updateWhen"] = 1;
                App.Settings["updateWiFi"] = false;
                App.Settings["lastUpdate"] = DateTime.MinValue;
                App.Settings.Save();
            }

            EmoticonList = new EmoticonList();
            EmoticonSelector.ItemsSource = EmoticonList;

            FavoriteList = new FavoriteList();
            FavoriteSelector.ItemsSource = FavoriteList;
            FavoriteList.CollectionChanged += FavoriteList_CollectionChanged;
            toggleEmptyLabel();

            RecentList = new RecentList();
            RecentSelector.ItemsSource = RecentList;
            if (RecentList.Count == 0)
                pivot.SelectedIndex = 1;
        }

        void toggleEmptyLabel()
        {
            if (FavoriteList.Count == 0)
            {
                ListEmptyLabel.Visibility = Visibility.Visible;
                FavoriteSelector.Visibility = Visibility.Collapsed;
            }
            else
            {
                ListEmptyLabel.Visibility = Visibility.Collapsed;
                FavoriteSelector.Visibility = Visibility.Visible;
            }
        }

        void FavoriteList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            toggleEmptyLabel();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (NavigationContext.QueryString.ContainsKey("copy"))
            {
                Clipboard.SetText(NavigationContext.QueryString["copy"]);
                throw new Exception("App exit");
            }
        }

        public static void EditPrompt(EmoticonItem item = null)
        {
            item = item ?? new EmoticonItem(null, null);

            StackPanel panel = new StackPanel();
            PhoneTextBox TextBox = new PhoneTextBox();
            PhoneTextBox NoteBox = new PhoneTextBox();
            if (item.Text != null)
            {
                TextBox.Text = item.Text;
                NoteBox.Text = item.Note;
            }
            TextBox.Hint = AppResources.Emoticon;
            NoteBox.Hint = AppResources.Note;
            panel.Children.Add(TextBox);
            panel.Children.Add(NoteBox);

            CustomMessageBox messageBox = new CustomMessageBox()
            {
                Message = AppResources.AddEmoticon,
                Content = panel,
                LeftButtonContent = AppResources.OK,
                IsLeftButtonEnabled = true,
                RightButtonContent = AppResources.Cancel,
                IsRightButtonEnabled = true
            };

            TextBox.TextChanged += (s, ev) =>
            {
                messageBox.IsLeftButtonEnabled = !string.IsNullOrWhiteSpace(TextBox.Text);
            };
            messageBox.Dismissed += (s, ev) =>
            {
                if (ev.Result == CustomMessageBoxResult.LeftButton)
                {
                    HashSet<string> favorite = SettingPage.Favorite;
                    Dictionary<string, string> noteMap = SettingPage.NoteMap;

                    if (favorite.Contains(item.Text))
                    {
                        favorite.Remove(item.Text);
                        noteMap.Remove(item.Text);
                    }
                    if (favorite.Contains(TextBox.Text))
                        favorite.Remove(TextBox.Text);
                    favorite.Add(TextBox.Text);

                    if (noteMap.ContainsKey(TextBox.Text))
                        noteMap[TextBox.Text] = NoteBox.Text;
                    else
                        noteMap.Add(TextBox.Text, NoteBox.Text);

                    App.Settings.Save();
                    MainPage.FavoriteList.Rebuild();
                }
            };
            messageBox.Show();
            TextBox.Focus();
        }

        private void AppBarAddButton_Click(object sender, EventArgs e)
        {
            if (pivot.SelectedIndex == 2)
                SettingPage.AddRepositoryPropmt();
            else if (pivot.SelectedIndex == 1)
                EditPrompt();
        }

        private async void AppBarRefreshButton_Click(object sender, EventArgs e)
        {
            await EmoticonList.UpdateRepositories();
        }

        private void AppBarSettingButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingPage.xaml", UriKind.Relative));
        }

        bool firstStart = true;
        private async void AppPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (firstStart)
            {
                if (SettingPage.Repositories.Count != 0)
                {
                    foreach (string url in SettingPage.Repositories)
                        EmoticonList.AddRepository(new EmoticonRepository(url));

                    TimeSpan timeout = new TimeSpan();
                    switch ((int)App.Settings["updateWhen"])
                    {
                        case 0:
                            timeout = TimeSpan.MaxValue;
                            break;
                        case 1:
                            timeout = new TimeSpan(0);
                            break;
                        case 2:
                            timeout = new TimeSpan(1, 0, 0, 0);
                            break;
                        case 3:
                            timeout = new TimeSpan(3, 0, 0, 0);
                            break;
                        case 4:
                            timeout = new TimeSpan(7, 0, 0, 0);
                            break;
                    }
                    if (DateTime.UtcNow - (DateTime)App.Settings["lastUpdate"] > timeout &&
                        (!(bool)App.Settings["updateWiFi"] ||
                        ((bool)App.Settings["updateWiFi"] && DeviceNetworkInformation.IsWiFiEnabled)))
                        await EmoticonList.UpdateRepositories();
                }
                else
                {
                    if ((bool?)App.Settings["noprompt"] != true)
                    {
                        CheckBox noPrompt = new CheckBox() { Content = AppResources.NoPrompt, IsChecked = false };
                        CustomMessageBox messageBox = new CustomMessageBox()
                        {
                            Message = AppResources.NoRepositories,
                            Content = noPrompt,
                            LeftButtonContent = AppResources.Yes,
                            IsLeftButtonEnabled = true,
                            RightButtonContent = AppResources.No,
                            IsRightButtonEnabled = true
                        };
                        noPrompt.Checked += (s, ev) =>
                        {
                            messageBox.IsLeftButtonEnabled = false;
                            App.Settings["noprompt"] = true;
                            App.Settings.Save();
                        };
                        noPrompt.Unchecked += (s, ev) =>
                        {
                            messageBox.IsLeftButtonEnabled = true;
                            App.Settings["noprompt"] = false;
                            App.Settings.Save();
                        };
                        messageBox.Dismissed += async (s, ev) =>
                        {
                            if (ev.Result == CustomMessageBoxResult.LeftButton)
                            {
                                EmoticonList.AddRepository(new EmoticonRepository(AppResources.DefaultList));
                                await EmoticonList.UpdateRepositories();
                            }
                        };
                        messageBox.Show();
                    }
                }

                firstStart = false;
            }
            else
            {
                if (EmoticonList.Count > 0)
                    EmoticonSelector.ScrollTo(EmoticonList[0]);
            }
        }

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (pivot.SelectedIndex)
            {
                case 0:
                    AppBar.ButtonList[0].Visiable = false;
                    AppBar.ButtonList[1].Visiable = false;
                    AppBar.Mode = ApplicationBarMode.Minimized;
                    break;
                case 1:
                    AppBar.ButtonList[0].Visiable = true;
                    AppBar.ButtonList[1].Visiable = false;
                    AppBar.Mode = ApplicationBarMode.Default;
                    break;
                case 2:
                    AppBar.ButtonList[0].Visiable = true;
                    AppBar.ButtonList[1].Visiable = true;
                    AppBar.Mode = ApplicationBarMode.Default;
                    break;
            }
        }
    }
}