using Simon.Library;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

#if WINDOWS_PHONE
using CloudEmoticon.Resources;
using Microsoft.Phone.Controls;
using Simon.Library.Controls;
#endif

namespace CloudEmoticon
{
    public class EmoticonItem
    {
        public string Text { get; private set; }
        public string Note { get; private set; }

        public EmoticonItem(string text, string note)
        {
            Text = text;
            Note = note;
        }

        public override string ToString()
        {
            return string.Format("Emoticon: \"{0}\" \n Note: \"{1}\"", Text.Trim(), Note);
        }
    }

    public class EmoticonCategory : AppCollection<EmoticonItem>
    {
        public string Name { get; protected set; }

        public EmoticonCategory(string name)
        {
            Name = name;
        }
    }

    public class RecentList : EmoticonCategory
    {
        AppCollection<string> recent;
        Dictionary<int, string> noteMap;

        public RecentList(AppCollection<string> items, Dictionary<int, string> notes)
            : base("Recent")
        {
            recent = items;
            noteMap = notes;
            if (recent.Count > 0)
                Rebuild();
            recent.CollectionChanged += recent_CollectionChanged;
        }

        void recent_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Rebuild();
        }

        public void Rebuild()
        {
            Clear();
            string note = null;
            foreach (string item in App.ViewModel.Recent.Reverse())
            {
                noteMap.TryGetValue(item.GetHashCode(), out note);
                Add(new EmoticonItem(item, note));
            }
        }
    }

    public class FavoriteList : EmoticonCategory
    {
        AppCollection<string> favorite;
        Dictionary<int, string> noteMap;

        public FavoriteList(AppCollection<string> items, Dictionary<int, string> notes)
            : base("Favorite")
        {
            favorite = items;
            noteMap = notes;
            if (favorite.Count > 0)
                Rebuild();
            favorite.CollectionChanged += favorite_CollectionChanged;
        }

        void favorite_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Rebuild();
        }

        public void Rebuild()
        {
            Clear();
            string note = null;
            foreach (string item in favorite)
            {
                noteMap.TryGetValue(item.GetHashCode(), out note);
                Add(new EmoticonItem(item, note));
            }
        }
    }

    public class EmoticonRepository : AppCollection<EmoticonCategory>, INotifyPropertyChanged
    {
        private string info = "";
        public string Info
        {
            get { return info; }
            private set
            {
                if (info != value)
                {
                    info = value;
                    OnPropertyChanged();
                }
            }
        }

        private string url = "";
        public int HashCode { get; private set; }
        public string URL
        {
            get { return url; }
            private set
            {
                url = value;
                HashCode = url.GetHashCode();
            }
        }

        public DateTime LastUpdate { get; private set; }
        public bool LastUpdateSuccess { get; private set; }

        public EmoticonRepository(string url)
        {
            URL = url.Trim();
            if (!App.ViewModel.InfoMap.ContainsKey(HashCode))
            {
                App.ViewModel.InfoMap.Add(HashCode, Info);
                App.Settings.Save();

                // Info = AppResources.Unsynced;
                info = "Unsynced";
            }
            else
                TryLoadCache();
        }

        public void TryLoadCache()
        {
            Dictionary<int, string> cacheMap = App.ViewModel.CacheMap;
            if (cacheMap.ContainsKey(HashCode))
                parseFile(cacheMap[HashCode]);
        }

        private void parseFile(string file)
        {
            LastUpdateSuccess = false;

            XDocument xdoc = XDocument.Parse(file);
            if (xdoc.Root.Name.LocalName == "emoji" &&
                xdoc.Root.Element("infoos") != null &&
                xdoc.Root.Element("infoos").Element("info") != null)
            {
                Clear();

                Info = xdoc.Root.Element("infoos").Element("info").Value.Trim();

                App.ViewModel.InfoMap[HashCode] = Info;
                App.Settings.Save();

                EmoticonCategory category;
                string note;
                foreach (XElement item in xdoc.Root.Elements("category"))
                {
                    category = new EmoticonCategory(item.Attribute("name").Value);
                    foreach (XElement entry in item.Elements("entry"))
                    {
                        note = entry.Element("note") != null ? entry.Element("note").Value : "";
                        category.Add(new EmoticonItem(entry.Element("string").Value, note));
                    }
                    Add(category);
                }

                LastUpdateSuccess = true;
            }
            else
            {
                // Info = AppResources.WrongFormat;
                Info = "Wrong XML Format.";
                TryLoadCache();
            }
        }

        public async Task<bool> Update()
        {
            LastUpdate = DateTime.UtcNow;
            try
            {
                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(20);
                string response = await client.GetStringAsync(URL);

                Dictionary<int, string> cacheMap = App.ViewModel.CacheMap;
                if (cacheMap.ContainsKey(HashCode))
                    cacheMap[HashCode] = response;
                else
                    cacheMap.Add(HashCode, response);
                App.Settings.Save();

                parseFile(response);
            }
            catch (HttpRequestException)
            {
                LastUpdateSuccess = false;
            }
            catch (TaskCanceledException)
            {
                LastUpdateSuccess = false;
            }
            if (LastUpdateSuccess)
            {
                App.Settings["lastUpdate"] = LastUpdate;
                App.Settings.Save();
            }
            return LastUpdateSuccess;
        }

        public new event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                UIDispatcher.BeginInvoke(PropertyChanged, this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class EmoticonList : AppCollection<EmoticonCategory>
    {
        public bool IsUpdating { get; private set; }
        public AppCollection<EmoticonRepository> Repositories { get; private set; }
        private AppCollection<string> urls;

        public EmoticonList(AppCollection<string> repositories)
        {
            IsUpdating = false;
            Repositories = new AppCollection<EmoticonRepository>();
            urls = repositories;
            urls.CollectionChanged += URLs_CollectionChanged;
            foreach (string url in urls)
                addRepository(url);
            Rebuild();
        }

        private void URLs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (EmoticonRepository repository in Repositories)
                repository.CollectionChanged -= repository_CollectionChanged;
            Repositories.Clear();
            Clear();
            foreach (string repository in urls)
                addRepository(repository);
            Rebuild();
        }

        private async void addRepository(string url)
        {
            EmoticonRepository repository = new EmoticonRepository(url);
            await Repositories.Add(repository, true);
            repository.CollectionChanged += repository_CollectionChanged;
        }

        private bool removeRepository(string url)
        {
            int index = urls.IndexOf(url);
            if (index < 0)
                return false;
            removeRepository(Repositories[index]);
            return true;
        }

        private async void removeRepository(EmoticonRepository repository)
        {
            repository.CollectionChanged -= repository_CollectionChanged;
            await Repositories.Remove(repository, true);

            await App.ViewModel.Repositories.Remove(repository.URL, true);
            App.ViewModel.InfoMap.Remove(repository.HashCode);
            App.Settings.Save();

            Rebuild();
        }

        public async Task<bool> UpdateRepositories()
        {
            if (IsUpdating || Repositories.Count == 0)
                return false;
            IsUpdating = true;

#if WINDOWS_PHONE
            AppPage.ProgressIndicator.IsIndeterminate = true;
            AppPage.ProgressIndicator.Text = AppResources.Processing;
#endif

            List<Task<bool>> tasks = new List<Task<bool>>();
            foreach (EmoticonRepository repository in Repositories)
#if WINDOWS_PHONE_71
                tasks.Add(TaskEx.Run((Func<Task<bool>>)repository.Update));
            await TaskEx.WhenAll(tasks);
#else
                tasks.Add(Task.Run((Func<Task<bool>>)repository.Update));
            await Task.WhenAll(tasks);
#endif
            IsUpdating = false;

            bool allFailed = true;
            bool allSucceed = true;
            foreach (var task in tasks)
            {
                if (task.Result)
                    allFailed = false;
                else
                    allSucceed = false;
            }

            if (allFailed)
            {
#if WINDOWS_PHONE
                AppPage.ProgressIndicator.IsIndeterminate = false;
                AppPage.ProgressIndicator.Value = 0;
                AppPage.ProgressIndicator.Text = AppResources.UpdateFailed;
                AppPage.ProgressIndicator.Hide(2000);
#endif

                return false;
            }
            else if (allSucceed)
            {
#if WINDOWS_PHONE
                AppPage.ProgressIndicator.IsIndeterminate = false;
                AppPage.ProgressIndicator.Value = 1;
                AppPage.ProgressIndicator.Text = AppResources.Updated;
                AppPage.ProgressIndicator.Hide(2000);
#endif

                return true;
            }
            else
            {
#if WINDOWS_PHONE
                AppPage.ProgressIndicator.IsIndeterminate = false;
                AppPage.ProgressIndicator.Value = 1;
                AppPage.ProgressIndicator.Text = AppResources.Updated;
                AppPage.ProgressIndicator.Hide(2000);
#endif

                return true;
            }

        }

        void repository_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Rebuild();
        }

        public void Rebuild()
        {
            this.Clear();
            foreach (EmoticonRepository repository in Repositories)
                foreach (EmoticonCategory category in repository)
                    this.Add(category);
//#if WINDOWS_PHONE
//            if (Count > 0 && ((PhoneApplicationFrame)App.RootFrame).Content is MainPage)
//                UIDispatcher.BeginInvoke(() =>
//                    ((MainPage)((PhoneApplicationFrame)App.RootFrame).Content).EmoticonSelector.ScrollTo(this[0]));
//#endif
        }
    }
}