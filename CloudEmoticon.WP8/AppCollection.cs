using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Threading;

namespace Simon.Library
{
    /// <summary>
    /// Represents a dynamic data collection that provides notifications when items
    ///    get added, removed, or when the entire list is refreshed.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    public class AppCollection<T> : ObservableCollection<T>
    {
        public static Dispatcher UIDispatcher = Deployment.Current.Dispatcher;

        /// <summary>
        /// Initializes a new, empty instance of the Simon.Library.AppCollection<T> class.
        /// </summary>
        public AppCollection() : base() { }

        /// <summary>
        /// Initializes a new instance of the Simon.Library.AppCollection<T>
        ///    class and populates it with items copied from the specified collection.
        /// </summary>
        /// <param name="collection">The collection from which the items are copied.</param>
        public AppCollection(IEnumerable<T> collection)
            : base()
        {
            foreach (T item in collection)
                Add(item);
        }

        /// <summary>
        /// Initializes a new instance of the Simon.Library.AppCollection<T>
        ///    class and populates it with items copied from the specified list.
        /// </summary>
        /// <param name="list">The list from which the items are copied.</param>
        public AppCollection(List<T> list)
            : base()
        {
            int startIndex = Items.Count;
            foreach (T item in list)
                Items.Add(item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new ReadOnlyCollection<T>(list), startIndex));
        }

        /// <summary>
        /// Adds an object to the end of the Simon.Library.AppCollection<T>.
        /// </summary>
        /// <param name="item">
        /// The object to be added to the end of the Simon.Library.AppCollection<T>.
        ///     The value can be null for reference types.
        /// </param>
        public new void Add(T item)
        {
            if (UIDispatcher.CheckAccess())
                base.Add(item);
            else
                UIDispatcher.BeginInvoke(() => base.Add(item));
        }

        /// <summary>
        /// Removes all elements from the Simon.Library.AppCollection<T>.
        /// </summary>
        public new void ClearItems()
        {
            if (UIDispatcher.CheckAccess())
                base.ClearItems();
            else
                UIDispatcher.BeginInvoke(() => base.ClearItems());
        }

        /// <summary>
        /// Removes all items from the Simon.Library.AppCollection<T>.
        /// </summary>
        public new void Clear()
        {
            this.ClearItems();
        }
    }
}
