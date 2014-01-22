using CloudEmoticon.Resources;
using Simon.Library;
using Simon.Library.Controls;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
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
        public string Name { get; private set; }

        public EmoticonCategory(string name)
        {
            Name = name;
        }

        public EmoticonCategory()
        {
            Name = AppResources.Favorite;
            if (SettingPage.Favorite.Count > 0)
                Rebuild();
        }

        public void Rebuild()
        {
            if (Name != AppResources.Favorite)
                throw new NotSupportedException();
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
            get
            {
                return info;
            }
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
        public string URL
        {
            get
            {
                return url;
            }
            private set
            {
                if (url != value)
                {
                    url = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime LastUpdate { get; private set; }
        public bool LastUpdateSuccess { get; private set; }
        public bool Initialized = false;

        public EmoticonRepository(string url)
        {
            URL = url;
            HashSet<string> repositories = SettingPage.Repositories;
            if (!repositories.Contains(url))
            {
                repositories.Add(url);

                SettingPage.InfoMap.Add(URL.GetHashCode().ToString(), Info);
                App.Settings.Save();

                Info = AppResources.Unsynced;
            }
            else
                Info = SettingPage.InfoMap[URL.GetHashCode().ToString()];
        }

        public void TryLoadCache()
        {
            Dictionary<string, string> cacheMap = SettingPage.CacheMap;
            if (cacheMap.ContainsKey(URL.GetHashCode().ToString()))
                parseFile(cacheMap[URL.GetHashCode().ToString()]);
        }

        private void parseFile(string file)
        {
            LastUpdate = DateTime.UtcNow;

            XDocument xdoc = XDocument.Parse(file);
            if (xdoc.Root.Name.LocalName == "emoji")
            {
                lock (EmoticonList.ListLock)
                {
                    this.ClearItems();
                    Info = xdoc.Root.Element("infoos").Element("info").Value;

                    SettingPage.InfoMap[URL] = Info;
                    App.Settings.Save();

                    EmoticonCategory category;
                    foreach (XElement item in xdoc.Root.Elements("category"))
                    {
                        category = new EmoticonCategory(item.Attribute("name").Value);
                        string note;
                        foreach (XElement entry in item.Elements("entry"))
                        {
                            note = entry.Element("note") != null ? entry.Element("note").Value : "";
                            category.Add(new EmoticonItem(entry.Element("string").Value, note));
                        }
                        this.Add(category);
                    }
                }
            }

            LastUpdateSuccess = true;
            if (!Initialized)
                Initialized = true;
        }

        public async Task<bool> Update()
        {
            try
            {
                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(20);
                string response = await client.GetStringAsync(URL);

                Dictionary<string, string> cacheMap = SettingPage.CacheMap;
                if (cacheMap.ContainsKey(URL))
                    cacheMap[URL] = response;
                else
                    cacheMap.Add(URL, response);
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
            return LastUpdateSuccess;
        }

        public new event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName)));
        }
    }

    public class EmoticonList : AppCollection<EmoticonCategory>
    {
        public static object ListLock = new object();
        public AppCollection<EmoticonRepository> Repositories { get; private set; }

        public bool IsUpdating { get; private set; }

        public EmoticonList()
        {
            IsUpdating = false;
            Repositories = new AppCollection<EmoticonRepository>();
        }

        public void AddRepository(EmoticonRepository repository)
        {
            Repositories.Add(repository);
            repository.CollectionChanged += repository_CollectionChanged;
        }

        public void RemoveRepository(string url)
        {
            EmoticonRepository itemToDelete = null;
            foreach (EmoticonRepository repository in Repositories)
                if (repository.URL == url)
                    itemToDelete = repository;
            if (itemToDelete != null)
                RemoveRepository(itemToDelete);
        }

        public void RemoveRepository(EmoticonRepository repository)
        {
            repository.CollectionChanged -= repository_CollectionChanged;
            Repositories.Remove(repository);

            SettingPage.Repositories.Remove(repository.URL);
            SettingPage.InfoMap.Remove(repository.URL.GetHashCode().ToString());
            App.Settings.Save();

            Rebuild();
        }

        public void UpdateRepositories()
        {
            Deployment.Current.Dispatcher.BeginInvoke(doUpdate);
        }

        public async void doUpdate()
        {
            if (IsUpdating || Repositories.Count == 0)
                return;
            IsUpdating = true;

            AppPage.ProgressIndicator.IsIndeterminate = true;
            AppPage.ProgressIndicator.Text = AppResources.Processing;
            AppPage.ProgressIndicator.IsVisible = true;

            List<Task<bool>> tasks = new List<Task<bool>>();
            foreach (EmoticonRepository repository in Repositories)
            {
                if (!repository.Initialized)
                    repository.TryLoadCache();
                tasks.Add(Task.Run((Func<Task<bool>>)repository.Update));
            }
            await Task.Run(() => Task.WaitAll(tasks.ToArray()));

            if (!tasks.Any(task => { return task.Result == true; }))
            {
                AppPage.ProgressIndicator.IsIndeterminate = false;
                AppPage.ProgressIndicator.Value = 0;
                AppPage.ProgressIndicator.Text = AppResources.UpdateFailed;
                AppPage.ProgressIndicator.Hide(2000);
            }
            else if (tasks.Any(task => { return task.Result == false; }))
            {
                AppPage.ProgressIndicator.IsIndeterminate = false;
                AppPage.ProgressIndicator.Value = 1;
                AppPage.ProgressIndicator.Text = AppResources.Updated;
                AppPage.ProgressIndicator.Hide(2000);
            }
            else
            {
                AppPage.ProgressIndicator.IsIndeterminate = false;
                AppPage.ProgressIndicator.Value = 1;
                AppPage.ProgressIndicator.Text = AppResources.Updated;
                AppPage.ProgressIndicator.Hide(2000);
            }

            IsUpdating = false;
        }

        void repository_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Rebuild();
        }

        public void Rebuild()
        {
            lock (ListLock)
            {
                this.Clear();
                foreach (EmoticonRepository repository in Repositories)
                    foreach (EmoticonCategory category in repository)
                        this.Add(category);
            }
        }
    }
}
