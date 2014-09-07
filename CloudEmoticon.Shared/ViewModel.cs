using Simon.Library;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CloudEmoticon
{
    public class ViewModel
    {
        private AppCollection<string> backing_Favorite;
        public AppCollection<string> Favorite
        {
            get
            {
                if (backing_Favorite == null)
                    backing_Favorite = (AppCollection<string>)App.Settings["favorite"];
                return backing_Favorite;
            }
        }

        private Dictionary<int, string> backing_NoteMap;
        public Dictionary<int, string> NoteMap
        {
            get
            {
                if (backing_NoteMap == null)
                    backing_NoteMap = (Dictionary<int, string>)App.Settings["noteMap"];
                return backing_NoteMap;
            }
        }

        private AppCollection<string> backing_Repositories;
        public AppCollection<string> Repositories
        {
            get
            {
                if (backing_Repositories == null)
                    backing_Repositories = (AppCollection<string>)App.Settings["repositories"];
                return backing_Repositories;
            }
        }

        private Dictionary<int, string> backing_InfoMap;
        public Dictionary<int, string> InfoMap
        {
            get
            {
                if (backing_InfoMap == null)
                    backing_InfoMap = (Dictionary<int, string>)App.Settings["infoMap"];
                return backing_InfoMap;
            }
        }

        private Dictionary<int, string> backing_CacheMap;
        public Dictionary<int, string> CacheMap
        {
            get
            {
                if (backing_CacheMap == null)
                    backing_CacheMap = (Dictionary<int, string>)App.Settings["cacheMap"];
                return backing_CacheMap;
            }
        }

        private AppCollection<string> backing_Recent;
        public AppCollection<string> Recent
        {
            get
            {
                if (backing_Recent == null)
                    backing_Recent = (AppCollection<string>)App.Settings["recent"];
                return backing_Recent;
            }
        }

        private EmoticonList backing_EmoticonList;
        public EmoticonList EmoticonList
        {
            get
            {
                if (backing_EmoticonList == null)
                    backing_EmoticonList = new EmoticonList(Repositories);
                return backing_EmoticonList;
            }
        }

        private FavoriteList backing_FavoriteList;
        public FavoriteList FavoriteList
        {
            get
            {
                if (backing_FavoriteList == null)
                    backing_FavoriteList = new FavoriteList(Favorite, NoteMap);
                return backing_FavoriteList;
            }
        }

        private RecentList backing_RecentList;
        public RecentList RecentList
        {
            get
            {
                if (backing_RecentList == null)
                    backing_RecentList = new RecentList(Recent, NoteMap);
                return backing_RecentList;
            }
        }

        private ViewModel()
        {
            if (App.Settings["firstStart"] == null)
            {
                App.Settings["favorite"] = new AppCollection<string>();
                App.Settings["noteMap"] = new Dictionary<int, string>();
                App.Settings["repositories"] = new AppCollection<string>();
                App.Settings["infoMap"] = new Dictionary<int, string>();
                App.Settings["cacheMap"] = new Dictionary<int, string>();
                App.Settings["firstStart"] = false;
#if WINDOWS_PHONE
            }
            // Version 1.0.1
            if (App.Settings["recent"] == null)
            {
#endif
                App.Settings["recent"] = new AppCollection<string>();
                App.Settings["updateWhen"] = 1;
                App.Settings["updateWiFi"] = false;
                App.Settings["lastUpdate"] = DateTime.MinValue;
            }
        }

        public static ViewModel Instance { get; private set; }
        static ViewModel()
        {
            Instance = new ViewModel();
        }
    }
}
