using CloudEmoticon.Resources;
using Microsoft.Phone.Controls;
using Simon.Library;
using Simon.Library.Controls;
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
            return string.Format("Emoticon: \"{0}\": {1}", Text, Note);
        }
    }

    public class EmoticonCategory : AppCollection<EmoticonItem>
    {
        public string Name { get; protected set; }

        public EmoticonCategory(string name)
        {
            Name = name;
        }

        public EmoticonCategory(string name, List<EmoticonItem> list)
            : base(list)
        {
            Name = name;
        }
    }

    public class RecentList : EmoticonCategory
    {
        public RecentList()
            : base("Recent")
        {
            if (SettingPage.Recent.Count > 0)
                Rebuild();
        }

        public void Rebuild()
        {
            this.Clear();
            Dictionary<string, string> noteMap = SettingPage.NoteMap;
            string note;
            foreach (string item in SettingPage.Recent.Reverse())
            {
                note = noteMap.ContainsKey(item) ? noteMap[item] : "";
                this.Add(new EmoticonItem(item, note));
            }
        }
    }

    public class FavoriteList : EmoticonCategory
    {
        public FavoriteList()
            : base("Favorite")
        {
            if (SettingPage.Favorite.Count > 0)
                Rebuild();
        }

        public void Rebuild()
        {
            this.Clear();
            Dictionary<string, string> noteMap = SettingPage.NoteMap;
            foreach (string item in SettingPage.Favorite)
                this.Add(new EmoticonItem(item, noteMap[item]));
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
        public string HashString { get; private set; }
        public string URL
        {
            get { return url; }
            private set
            {
                url = value;
                HashString = url.GetHashCode().ToString();
            }
        }

        public DateTime LastUpdate { get; private set; }
        public bool LastUpdateSuccess { get; private set; }
        public bool Initialized = false;

        public EmoticonRepository(string url)
        {
            url = url.Trim();
            URL = url;
            HashSet<string> repositories = SettingPage.Repositories;
            if (!repositories.Contains(url))
            {
                repositories.Add(url);

                SettingPage.InfoMap.Add(HashString, Info);
                App.Settings.Save();

                Info = AppResources.Unsynced;
            }
            else
                Info = SettingPage.InfoMap[HashString];

            TryLoadCache();
        }

        public void TryLoadCache()
        {
            Dictionary<string, string> cacheMap = SettingPage.CacheMap;
            if (cacheMap.ContainsKey(HashString))
                parseFile(cacheMap[HashString]);
        }

        private void parseFile(string file)
        {
            LastUpdateSuccess = false;

            XDocument xdoc = XDocument.Parse(file);
            if (xdoc.Root.Name.LocalName == "emoji" &&
                xdoc.Root.Element("infoos") != null &&
                xdoc.Root.Element("infoos").Element("info") != null)
            {
                this.ClearItems();

                Info = xdoc.Root.Element("infoos").Element("info").Value.Trim();

                SettingPage.InfoMap[URL] = Info;
                App.Settings.Save();

                List<EmoticonItem> list = new List<EmoticonItem>();
                foreach (XElement item in xdoc.Root.Elements("category"))
                {
                    string note;
                    list.Clear();
                    foreach (XElement entry in item.Elements("entry"))
                    {
                        note = entry.Element("note") != null ? entry.Element("note").Value : "";
                        list.Add(new EmoticonItem(entry.Element("string").Value, note));
                    }
                    this.Add(new EmoticonCategory(item.Attribute("name").Value, list));
                }

                LastUpdateSuccess = true;
            }
            else
                Info = AppResources.WrongFormat;

            if (!Initialized)
                Initialized = true;
        }

        public async Task<bool> Update()
        {
            LastUpdate = DateTime.UtcNow;
            try
            {
                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(20);
                string response = await client.GetStringAsync(URL);

                Dictionary<string, string> cacheMap = SettingPage.CacheMap;
                if (cacheMap.ContainsKey(HashString))
                    cacheMap[HashString] = response;
                else
                    cacheMap.Add(HashString, response);
                App.Settings.Save();

                parseFile(response);
            }
            catch (HttpRequestException ex)
            {
                LastUpdateSuccess = false;
            }
            catch (TaskCanceledException ex)
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
                Deployment.Current.Dispatcher.BeginInvoke(PropertyChanged, this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class EmoticonList : AppCollection<EmoticonCategory>
    {
        public bool IsUpdating { get; private set; }
        public AppCollection<EmoticonRepository> Repositories { get; private set; }

        public EmoticonList()
        {
            IsUpdating = false;
            Repositories = new AppCollection<EmoticonRepository>();
        }

        public void AddRepository(EmoticonRepository repository)
        {
            Repositories.Add(repository);
            repository.CollectionChanged += repository_CollectionChanged;
            Rebuild();
        }

        public void RemoveRepository(string url)
        {
            EmoticonRepository itemToDelete =
                Repositories.First(repo => { return repo.URL == url; });
            if (itemToDelete != null)
                RemoveRepository(itemToDelete);
        }

        public void RemoveRepository(EmoticonRepository repository)
        {
            repository.CollectionChanged -= repository_CollectionChanged;
            Repositories.Remove(repository);

            SettingPage.Repositories.Remove(repository.URL);
            SettingPage.InfoMap.Remove(repository.HashString);
            App.Settings.Save();

            Rebuild();
        }

        public async Task<bool> UpdateRepositories()
        {
            if (IsUpdating || Repositories.Count == 0)
                return false;
            IsUpdating = true;

            AppPage.ProgressIndicator.IsIndeterminate = true;
            AppPage.ProgressIndicator.Text = AppResources.Processing;
            AppPage.ProgressIndicator.IsVisible = true;

            List<Task<bool>> tasks = new List<Task<bool>>();
            foreach (EmoticonRepository repository in Repositories)
                tasks.Add(Task.Run((Func<Task<bool>>)repository.Update));
            await Task.Run(() => Task.WaitAll(tasks.ToArray()));

            IsUpdating = false;

            if (!tasks.Any(task => { return task.Result == true; }))
            {
                AppPage.ProgressIndicator.IsIndeterminate = false;
                AppPage.ProgressIndicator.Value = 0;
                AppPage.ProgressIndicator.Text = AppResources.UpdateFailed;
                AppPage.ProgressIndicator.Hide(2000);

                return false;
            }
            else if (tasks.Any(task => { return task.Result == false; }))
            {
                AppPage.ProgressIndicator.IsIndeterminate = false;
                AppPage.ProgressIndicator.Value = 1;
                AppPage.ProgressIndicator.Text = AppResources.Updated;
                AppPage.ProgressIndicator.Hide(2000);

                return true;
            }
            else
            {
                AppPage.ProgressIndicator.IsIndeterminate = false;
                AppPage.ProgressIndicator.Value = 1;
                AppPage.ProgressIndicator.Text = AppResources.Updated;
                AppPage.ProgressIndicator.Hide(2000);

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
            if (Count > 0 && ((PhoneApplicationFrame)App.RootFrame).Content is MainPage)
                UIDispatcher.BeginInvoke(() =>
                    ((MainPage)((PhoneApplicationFrame)App.RootFrame).Content).EmoticonSelector.ScrollTo(this[0]));
        }
    }
}
