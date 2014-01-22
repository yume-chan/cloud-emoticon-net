using CloudEmoticon.Resources;
using Microsoft.Phone.Controls;
using Simon.Library.Controls;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace CloudEmoticon
{
    public partial class MainPage : AppPage
    {
        public static EmoticonList EmoticonList { get; private set; }
        public static EmoticonCategory FavoriteList { get; private set; }

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

            EmoticonList = new EmoticonList();
            EmoticonSelector.ItemsSource = EmoticonList;
            EmoticonList.CollectionChanged += EmoticonList_CollectionChanged;

            FavoriteList = new EmoticonCategory();
            FavoriteSelector.ItemsSource = FavoriteList;
            FavoriteList.CollectionChanged += FavoriteList_CollectionChanged;
            toggleEmptyLabel();
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

        void EmoticonList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (EmoticonList.Count > 0)
                EmoticonSelector.ScrollTo(EmoticonList[0]);
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
            StackPanel panel = new StackPanel();
            PhoneTextBox TextBox = new PhoneTextBox();
            PhoneTextBox NoteBox = new PhoneTextBox();
            if (item != null)
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
                    if (item != null)
                        favorite.Remove(item.Text);
                    favorite.Add(TextBox.Text);

                    Dictionary<string, string> noteMap = SettingPage.NoteMap;
                    if (item != null && item.Text == TextBox.Text)
                        noteMap[item.Text] = NoteBox.Text;
                    else
                        noteMap.Add(TextBox.Text, NoteBox.Text);

                    App.Settings.Save();
                    MainPage.FavoriteList.Rebuild();
                }
            };
            messageBox.Show();
        }

        private void AppBarAddButton_Click(object sender, EventArgs e)
        {
            if (pivot.SelectedIndex == 1)
                SettingPage.AddRepositoryPropmt();
            else
                EditPrompt();
        }

        private void AppBarRefreshButton_Click(object sender, EventArgs e)
        {
            EmoticonList.UpdateRepositories();
        }

        private void AppBarSettingButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingPage.xaml", UriKind.Relative));
        }

        bool firstStart = true;
        private void AppPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (firstStart)
            {
                if (SettingPage.Repositories.Count != 0)
                {
                    foreach (string url in SettingPage.Repositories)
                        EmoticonList.AddRepository(new EmoticonRepository(url));
                    EmoticonList.UpdateRepositories();
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
                        messageBox.Dismissed += (s, ev) =>
                        {
                            if (ev.Result == CustomMessageBoxResult.LeftButton)
                            {
                                EmoticonList.AddRepository(new EmoticonRepository(AppResources.DefaultList));
                                EmoticonList.UpdateRepositories();
                            }
                        };
                        messageBox.Show();
                    }
                }
                firstStart = false;
            }
        }
    }
}