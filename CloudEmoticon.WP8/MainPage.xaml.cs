using CloudEmoticon.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Shell;
using Simon.Library;
using Simon.Library.Controls;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace CloudEmoticon
{
    public partial class MainPage : AppPage
    {
        public MainPage()
            : base()
        {
            InitializeComponent();

            DataContext = App.ViewModel;

            App.ViewModel.FavoriteList.CollectionChanged += FavoriteList_CollectionChanged;
            toggleEmptyLabel();

            if (App.ViewModel.RecentList.Count == 0)
                if (App.ViewModel.FavoriteList.Count == 0)
                    pivot.SelectedIndex = 2;
                else
                    pivot.SelectedIndex = 1;
        }

        void toggleEmptyLabel()
        {
            if (App.ViewModel.FavoriteList.Count == 0)
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
                    AppCollection<string> favorite = App.ViewModel.Favorite;
                    Dictionary<int, string> noteMap = App.ViewModel.NoteMap;

                    if (favorite.Contains(item.Text))
                    {
                        favorite.Remove(item.Text);
                        noteMap.Remove(item.Text.GetHashCode());
                    }
                    if (favorite.Contains(TextBox.Text))
                        favorite.Remove(TextBox.Text);
                    favorite.Add(TextBox.Text);

                    if (noteMap.ContainsKey(TextBox.Text.GetHashCode()))
                        noteMap[TextBox.Text.GetHashCode()] = NoteBox.Text;
                    else
                        noteMap.Add(TextBox.Text.GetHashCode(), NoteBox.Text);

                    App.Settings.Save();
                    App.ViewModel.FavoriteList.Rebuild();
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
            await App.ViewModel.EmoticonList.UpdateRepositories();
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
                AutoResetEvent @event = new AutoResetEvent(false);

                var queryStrings = NavigationContext.QueryString;
                if (queryStrings.ContainsKey("addResposity"))
                {
                    CustomMessageBox messageBox = new CustomMessageBox()
                    {
                        Message = string.Format(AppResources.AddResposityComfirm, queryStrings["addResposity"]),
                        LeftButtonContent = AppResources.Yes,
                        IsLeftButtonEnabled = true,
                        RightButtonContent = AppResources.No,
                        IsRightButtonEnabled = true,
                    };
                    messageBox.Dismissed += async (s, ev) =>
                    {
                        @event.Set();
                        if (ev.Result == CustomMessageBoxResult.LeftButton)
                        {
                            App.ViewModel.Repositories.Add(queryStrings["addResposity"]);
                            await App.ViewModel.EmoticonList.UpdateRepositories();
                        }
                    };
                    messageBox.Show();
                }
                else
                    @event.Set();

                await Task.Run(() => { @event.WaitOne(); });

                if (App.ViewModel.Repositories.Count != 0)
                {
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
                        await App.ViewModel.EmoticonList.UpdateRepositories();
                }
                else
                {
                    if ((bool?)App.Settings["noprompt"] != true)
                    {
                        CheckBox noPrompt = new CheckBox() { Content = AppResources.NoPrompt, IsChecked = false };
                        TextBlock noPromptHint = new TextBlock() { Text = AppResources.NoPromptHint, FontSize = (double)App.Current.Resources["PhoneFontSizeSmall"], Visibility = Visibility.Collapsed };
                        StackPanel container = new StackPanel();
                        container.Children.Add(noPrompt);
                        container.Children.Add(noPromptHint);
                        CustomMessageBox messageBox = new CustomMessageBox()
                        {
                            Message = AppResources.NoRepositories,
                            Content = container,
                            LeftButtonContent = AppResources.Yes,
                            IsLeftButtonEnabled = true,
                            RightButtonContent = AppResources.No,
                            IsRightButtonEnabled = true
                        };
                        noPrompt.Checked += (s, ev) =>
                        {
                            messageBox.IsLeftButtonEnabled = false;
                            noPromptHint.Visibility = Visibility.Visible;
                            App.Settings["noprompt"] = true;
                            App.Settings.Save();
                        };
                        noPrompt.Unchecked += (s, ev) =>
                        {
                            messageBox.IsLeftButtonEnabled = true;
                            noPromptHint.Visibility = Visibility.Collapsed;
                            App.Settings["noprompt"] = false;
                            App.Settings.Save();
                        };
                        messageBox.Dismissed += async (s, ev) =>
                        {
                            if (ev.Result == CustomMessageBoxResult.LeftButton)
                            {
                                App.ViewModel.Repositories.Add(AppResources.DefaultList);
                                await App.ViewModel.EmoticonList.UpdateRepositories();
                            }
                        };
                        messageBox.Show();
                    }
                }

                firstStart = false;
            }
            else
            {
                if (App.ViewModel.EmoticonList.Count > 0)
                    EmoticonSelector.ScrollTo(App.ViewModel.EmoticonList[0]);
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