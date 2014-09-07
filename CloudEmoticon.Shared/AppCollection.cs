using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Simon.Library
{
#if WINDOWS_PHONE
    /// <summary>
    /// Fake Serializable attribute for Windows Phone
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class Serializable : Attribute { }

    /// <summary>
    /// Fake NonSerialized attribute for Windows Phone
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class NonSerialized : Attribute { }
#endif

    /// <summary>
    /// Represents a dynamic data collection that provides notifications when items
    ///    get added, removed, or when the entire list is refreshed.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    [Serializable]
    public class AppCollection<T> : ObservableCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
#if WINDOWS_PHONE
        public static Dispatcher UIDispatcher = Deployment.Current.Dispatcher;
#else
        private static Dispatcher dispathcer;
        public static Dispatcher UIDispatcher
        {
            get
            {
                if (dispathcer == null)
                {
                    if (Application.Current != null)
                        dispathcer = Application.Current.Dispatcher;
                    else
                        dispathcer = Dispatcher.CurrentDispatcher;
                }
                return dispathcer;
            }
        }
#endif

        public AppCollection() : base() { }

        public new async void Add(T item)
        {
            await Add(item, false);
        }
        public async Task Add(T item, bool wait)
        {
            if (Items.IsReadOnly)
                throw new NotSupportedException("Readonly Collection");
            if (wait)
            {
                AutoResetEvent @event = new AutoResetEvent(false);
                if (UIDispatcher.CheckAccess())
                {
                    InsertItem(Items.Count, item);
                    @event.Set();
                }
                else
                {
                    UIDispatcher.BeginInvoke(() =>
                    {
                        InsertItem(Items.Count, item);
                        @event.Set();
                    });
                }
#if WINDOWS_PHONE_71
                await TaskEx.Run(() => @event.WaitOne());
#else
                await Task.Run(() => @event.WaitOne());
#endif
            }
            else
                if (UIDispatcher.CheckAccess())
                    InsertItem(Items.Count, item);
                else
                    UIDispatcher.BeginInvoke(() => InsertItem(Items.Count, item));
        }

        public new async void Clear()
        {
            await Clear(false);
        }
        public async Task Clear(bool wait)
        {
            if (Items.IsReadOnly)
                throw new NotSupportedException("Readonly Collection");
            if (wait)
            {
                AutoResetEvent @event = new AutoResetEvent(false);
                if (UIDispatcher.CheckAccess())
                {
                    ClearItems();
                    @event.Set();
                }
                else
                {
                    UIDispatcher.BeginInvoke(() =>
                    {
                        ClearItems();
                        @event.Set();
                    });
                }
#if WINDOWS_PHONE_71
                await TaskEx.Run(() => @event.WaitOne());
#else
                await Task.Run(() => @event.WaitOne());
#endif
            }
            else
                if (UIDispatcher.CheckAccess())
                    ClearItems();
                else
                    UIDispatcher.BeginInvoke(() => ClearItems());
        }

        public new async void Remove(T item)
        {
            await Remove(item, false);
        }
        public async Task<bool> Remove(T item, bool wait)
        {
            if (Items.IsReadOnly)
                throw new NotSupportedException("Readonly Collection");
            int index = Items.IndexOf(item);
            if (index < 0)
                return false;
            if (wait)
            {
                AutoResetEvent @event = new AutoResetEvent(false);
                if (UIDispatcher.CheckAccess())
                {
                    RemoveItem(index);
                    @event.Set();
                }
                else
                {
                    UIDispatcher.BeginInvoke(() =>
                    {
                        RemoveItem(index);
                        @event.Set();
                    });
                }
#if WINDOWS_PHONE_71
                await TaskEx.Run(() => @event.WaitOne());
#else
                await Task.Run(() => @event.WaitOne());
#endif
            }
            else
                if (UIDispatcher.CheckAccess())
                    RemoveItem(index);
                else
                    UIDispatcher.BeginInvoke(() => RemoveItem(index));
            return true;
        }
    }
}
