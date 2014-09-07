using CloudEmoticon.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CloudEmoticon
{
    public class CopyCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            if (parameter == null)
                return false;
            return true;
        }

        public event EventHandler CanExecuteChanged = delegate { };

        public void Execute(object parameter)
        {
            string text = (string)parameter;
            Clipboard.SetText(text);

            MainPage.ProgressIndicator.Text = AppResources.Copied;
            MainPage.ProgressIndicator.IsIndeterminate = false;
            MainPage.ProgressIndicator.Value = 0;
            MainPage.ProgressIndicator.Hide(2000);

            if (App.ViewModel.Recent.Contains(text))
                App.ViewModel.Recent.Remove(text);
            if (App.ViewModel.Recent.Count == 50)
                App.ViewModel.Recent.Remove(App.ViewModel.Recent.ElementAt(49));
            App.ViewModel.Recent.Add(text);
            App.Settings.Save();
            App.ViewModel.RecentList.Rebuild();
        }
    }

    public class EditCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            if (parameter == null)
                return false;
            return App.ViewModel.Favorite.Contains(((EmoticonItem)parameter).Text);
        }

        public event EventHandler CanExecuteChanged = delegate { };

        public void Execute(object parameter)
        {
            MainPage.EditPrompt((EmoticonItem)parameter);
        }
    }


    public class AddFavoriteCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            if (parameter == null)
                return false;
            return !App.ViewModel.Favorite.Contains(((EmoticonItem)parameter).Text);
        }

        public event EventHandler CanExecuteChanged = delegate { };

        public async void Execute(object parameter)
        {
            EmoticonItem item = (EmoticonItem)parameter;

            await App.ViewModel.Favorite.Add(item.Text, true);
            App.ViewModel.NoteMap.Add(item.Text.GetHashCode(), item.Note);
            App.Settings.Save();

            App.ViewModel.FavoriteList.Rebuild();

            MainPage page = ((MainPage)((PhoneApplicationFrame)App.RootFrame).Content);
            page.pivot.SelectedIndex = 1;
        }
    }

    public class RemoveFavoriteCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            if (parameter == null)
                return false;
            return App.ViewModel.Favorite.Contains((string)parameter);
        }

        public event EventHandler CanExecuteChanged = delegate { };

        public async void Execute(object parameter)
        {
            string item = (string)parameter;

            await App.ViewModel.Favorite.Remove(item, true);
            App.ViewModel.NoteMap.Remove(item.GetHashCode());
            App.Settings.Save();

            App.ViewModel.FavoriteList.Rebuild();
        }
    }

    public class RemoveRepositoryCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            if (parameter == null)
                return false;
            return true;
        }

        public event EventHandler CanExecuteChanged = delegate { };

        public void Execute(object parameter)
        {
            CustomMessageBox messageBox = new CustomMessageBox()
            {
                Title = AppResources.RemoveComfirmTitle,
                Message = AppResources.RemoveComfirmText,
                LeftButtonContent = AppResources.Yes,
                IsLeftButtonEnabled = true,
                RightButtonContent = AppResources.No,
                IsRightButtonEnabled = true
            };
            messageBox.Dismissed += (s, e) =>
            {
                if (e.Result == CustomMessageBoxResult.LeftButton)
                    App.ViewModel.Repositories.Remove((string)parameter);
            };
            messageBox.Show();
        }
    }

    public class PinCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            if (parameter == null)
                return false;
            return !ShellTile.ActiveTiles.Any(tile =>
                tile.NavigationUri.ToString().EndsWith("?copy=" + ((EmoticonItem)parameter).Text));
        }

        public event EventHandler CanExecuteChanged;
        public void Refresh()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, new EventArgs());
        }

        public void Execute(object parameter)
        {
            EmoticonItem item = (EmoticonItem)parameter;
            string hash = item.GetHashCode().ToString();

            Grid grid = new Grid()
            {
                Width = 159,
                Height = 159,
                Background = (SolidColorBrush)App.Current.Resources["PhoneAccentBrush"]
            };
            TextBlock textBlock = new TextBlock()
            {
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                TextWrapping = TextWrapping.NoWrap,
                Text = item.Text,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 50
            };
            grid.Children.Add(textBlock);

            grid.Arrange(new Rect(0, 0, grid.Width, grid.Height));
            grid.Measure(new Size(grid.Width, grid.Height));

            WriteableBitmap writeableBitmap = new WriteableBitmap(grid, null);
            writeableBitmap.Invalidate();
            string smallImage = string.Format("/Shared/ShellContent/small.{0}.jpg", hash);
            using (IsolatedStorageFileStream file =
                IsolatedStorageFile.GetUserStoreForApplication().CreateFile(smallImage))
                writeableBitmap.SaveJpeg(file, (int)grid.Width, (int)grid.Height, 0, 95);

            grid.Width = 336;
            grid.Height = 336;
            textBlock.FontSize = 60;
            grid.Arrange(new Rect(0, 0, grid.Width, grid.Height));
            grid.Measure(new Size(grid.Width, grid.Height));
            writeableBitmap = new WriteableBitmap(grid, null);
            writeableBitmap.Invalidate();
            string middleImage = string.Format("/Shared/ShellContent/middle.{0}.jpg", hash);
            using (IsolatedStorageFileStream file =
                IsolatedStorageFile.GetUserStoreForApplication().CreateFile(middleImage))
                writeableBitmap.SaveJpeg(file, (int)grid.Width, (int)grid.Height, 0, 95);

            grid.Width = 691;
            textBlock.FontSize = 70;
            grid.Arrange(new Rect(0, 0, 691, 336));
            grid.Measure(new Size(691, 336));
            writeableBitmap = new WriteableBitmap(grid, null);
            writeableBitmap.Invalidate();
            string wideImage = string.Format("/Shared/ShellContent/wide.{0}.jpg", hash);
            using (IsolatedStorageFileStream file =
                IsolatedStorageFile.GetUserStoreForApplication().CreateFile(wideImage))
                writeableBitmap.SaveJpeg(file, (int)grid.Width, (int)grid.Height, 0, 95);

#if WINDOWS_PHONE_71
            ShellTile.Create(new Uri(string.Format("/MainPage.xaml?copy={0}", item.Text), UriKind.Relative), new StandardTileData()
            {
                Title = item.Note != "" ? item.Note : item.Text,
                BackgroundImage = new Uri("isostore:" + smallImage, UriKind.Absolute)
            });
#else
            ShellTile.Create(new Uri(string.Format("/MainPage.xaml?copy={0}", item.Text), UriKind.Relative), new FlipTileData()
            {
                Title = item.Note != "" ? item.Note : item.Text,
                SmallBackgroundImage = new Uri("isostore:" + smallImage, UriKind.Absolute),
                BackgroundImage = new Uri("isostore:" + middleImage, UriKind.Absolute),
                WideBackgroundImage = new Uri("isostore:" + wideImage, UriKind.Absolute)
            }, true);
#endif
        }
    }

    public class RefreshDataCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return parameter is ContextMenu;
        }

        public event EventHandler CanExecuteChanged = delegate { };

        public void Execute(object parameter)
        {
            ContextMenu menu = parameter as ContextMenu;
            menu.SetBinding(FrameworkElement.DataContextProperty,
                new Binding("DataContext") { Source = menu.Owner ?? App.RootFrame });

            ((PinCommand)App.Current.Resources["PinCommand"]).Refresh();
        }
    }

    public class RateCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged = delegate { };

        public void Execute(object parameter)
        {
            new MarketplaceReviewTask().Show();
        }
    }

    public class FeedbackCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged = delegate { };

        public void Execute(object parameter)
        {
            new EmailComposeTask()
            {
                To = "cnsimonchan@gmail.com",
                Subject = "Feedback from Cloud Emoticon",
                Body = AppResources.FeedbackHelp
            }.Show();
        }
    }

    public class EnableToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((bool)value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
