using System.IO.IsolatedStorage;

namespace Simon.Library
{
    /// <summary>
    /// Provides methods for interacting with the ApplicationSettings.
    /// </summary>
    public class AppSettings
    {
        private static IsolatedStorageSettings _nativeObject;
        /// <summary>
        /// Gets the <code>IsolatedStorageSettings.ApplicationSettings</code> object.
        /// </summary>
        public static IsolatedStorageSettings NativeObject
        {
            get
            {
                if (_nativeObject == null)
                    _nativeObject = IsolatedStorageSettings.ApplicationSettings;
                return _nativeObject;
            }
        }

        /// <summary>
        /// Gets a value for the specified key.
        /// </summary>
        /// <typeparam name="T">The <code>System.Type</code> of the value to get.</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The value will be returned if the specified key is not found.</param>
        /// <returns>The value associated with the specified key if the key is found; otherwise, <paramref name="defaultValue"/></returns>
        public T GetValue<T>(string key, T defaultValue)
        {
            T result;
            if (!NativeObject.TryGetValue<T>(key, out result))
                result = defaultValue;
            return result;
        }

        /// <summary>
        /// Determines if the application settings dictionary contains the specified key.
        /// </summary>
        /// <param name="key">The key for the entry to be located.</param>
        /// <returns>true if the dictionary contains the specified key; otherwise, false.</returns>
        public bool Contains(string key)
        {
            return NativeObject.Contains(key);
        }

        /// <summary>
        /// Removes the entry with the specified key.
        /// </summary>
        /// <param name="key">The key for the entry to be deleted.</param>
        /// <returns>true if the specified key was removed; otherwise, false.</returns>
        public bool Remove(string key)
        {
            return NativeObject.Remove(key);
        }

        /// <summary>
        /// Saves data written to the current System.IO.IsolatedStorage.IsolatedStorageSettings object.
        /// </summary>
        public void Save()
        {
            NativeObject.Save();
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the item to get or set.</param>
        /// <returns>The value associated with the specified key, or null if the key is not found.</returns>
        public object this[string key]
        {
            get
            {
                return GetValue<object>(key, null);
            }
            set
            {
                NativeObject[key] = value;
            }
        }
    }
}
