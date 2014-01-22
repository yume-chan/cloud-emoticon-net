﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Simon.Library.Controls;
using Simon.Library;
using CloudEmoticon.Resources;

namespace CloudEmoticon
{
    public partial class SettingPage : AppPage
    {
        public static HashSet<string> Favorite = (HashSet<string>)App.Settings["favorite"];
        public static Dictionary<string, string> NoteMap = (Dictionary<string, string>)App.Settings["noteMap"];
        public static HashSet<string> Repositories = (HashSet<string>)App.Settings["repositories"];
        public static Dictionary<string, string> InfoMap = (Dictionary<string, string>)App.Settings["infoMap"];
        public static Dictionary<string, string> CacheMap = (Dictionary<string, string>)App.Settings["cacheMap"];

        AppCollection<EmoticonRepository> repositories;

        public SettingPage()
            : base()
        {
            InitializeComponent();

            repositories = MainPage.EmoticonList.Repositories;
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
        }

        void Repositories_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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
            messageBox.Dismissed += (s, ev) =>
            {
                if (ev.Result == CustomMessageBoxResult.LeftButton)
                {
                    try
                    {
                        Uri uri = new Uri(textbox1.Text, UriKind.Absolute);
                        MainPage.EmoticonList.AddRepository(new EmoticonRepository(textbox1.Text));
                        MainPage.EmoticonList.UpdateRepositories();
                    }
                    catch (UriFormatException ex)
                    {
                        MessageBox.Show(AppResources.UrlError);
                    }
                }
            };
            messageBox.Show();
        }

        private void AppBarAddButton_Click(object sender, EventArgs e)
        {
            AddRepositoryPropmt();
        }

        private void LoadButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MainPage.EmoticonList.AddRepository(new EmoticonRepository(AppResources.DefaultList));
            MainPage.EmoticonList.UpdateRepositories();
        }
    }
}