using CloudEmoticon.Resources;
using Microsoft.Phone.Controls;
using Simon.Library;
using Simon.Library.Controls;
using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace CloudEmoticon
{
    public partial class SettingPage : AppPage
    {
        private AppCollection<EmoticonRepository> repositories;

        private AppBar RepositoriesAppBar;

        public SettingPage()
            : base()
        {
            InitializeComponent();

            repositories = App.ViewModel.EmoticonList.Repositories;
            ResponsitoriesSelector.ItemsSource = repositories;
            repositories.CollectionChanged += Repositories_CollectionChanged;

            if (repositories.Count == 0)
            {
                ListEmptyLabel.Visibility = Visibility.Visible;
                ResponsitoriesSelector.Visibility = Visibility.Collapsed;
            }
            else
            {
                ListEmptyLabel.Visibility = Visibility.Collapsed;
                ResponsitoriesSelector.Visibility = Visibility.Visible;
            }

            updateWhenPicker.SelectedIndex = (int)App.Settings["updateWhen"];
            updateWiFiSwitch.IsChecked = (bool)App.Settings["updateWiFi"];

            RepositoriesAppBar = AppBar;
        }

        void Repositories_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (repositories.Count == 0)
            {
                ListEmptyLabel.Visibility = Visibility.Visible;
                ResponsitoriesSelector.Visibility = Visibility.Collapsed;
            }
            else
            {
                ListEmptyLabel.Visibility = Visibility.Collapsed;
                ResponsitoriesSelector.Visibility = Visibility.Visible;
            }
        }

        public static void AddRepositoryPropmt()
        {
            PhoneTextBox textbox1 = new PhoneTextBox();
            textbox1.Hint = AppResources.Url;

            CustomMessageBox messageBox = new CustomMessageBox()
            {
                Message = AppResources.AddRepository,
                Content = textbox1,
                LeftButtonContent = AppResources.OK,
                IsLeftButtonEnabled = false,
                RightButtonContent = AppResources.Cancel,
                IsRightButtonEnabled = true
            };
            textbox1.TextChanged += (s, ev) =>
            {
                messageBox.IsLeftButtonEnabled = !string.IsNullOrWhiteSpace(textbox1.Text);
            };
            messageBox.Dismissed += async (s, ev) =>
            {
                if (ev.Result == CustomMessageBoxResult.LeftButton)
                {
                    try
                    {
                        Uri uri = new Uri(textbox1.Text, UriKind.Absolute);
                        App.ViewModel.Repositories.Add(textbox1.Text);
                        await App.ViewModel.EmoticonList.UpdateRepositories();
                    }
                    catch (UriFormatException)
                    {
                        MessageBox.Show(AppResources.UrlError);
                    }
                }
            };
            messageBox.Show();
            textbox1.Focus();
        }

        private void AppBarAddButton_Click(object sender, EventArgs e)
        {
            AddRepositoryPropmt();
        }

        private async void LoadButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            App.ViewModel.Repositories.Add(AppResources.DefaultList);
            await App.ViewModel.EmoticonList.UpdateRepositories();
        }

        private void clearRecentButton_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.Recent.Clear();
            App.Settings.Save();
            App.ViewModel.RecentList.Rebuild();
        }

        private void updateWhenPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (updateWhenPicker == null)
                return;
            App.Settings["updateWhen"] = updateWhenPicker.SelectedIndex;
            App.Settings.Save();
        }

        private void updateWiFiSwitch_Checked(object sender, RoutedEventArgs e)
        {
            App.Settings["updateWiFi"] = true;
            App.Settings.Save();
        }

        private void updateWiFiSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            App.Settings["updateWiFi"] = false;
            App.Settings.Save();
        }

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot.SelectedIndex == 1)
                AppBar = RepositoriesAppBar;
            else
                AppBar = null;
        }

#if !WINDOWS_PHONE_71
        private void AppBarStoreButton_Click(object sender, EventArgs e)
        {
            new WebBrowserTask() { Uri = new Uri(AppResources.CloudEmoticonStore) }.Show();
        }
#endif
    }
}