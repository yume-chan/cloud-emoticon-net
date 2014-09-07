using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Simon.Library
{
#if !WINDOWS_PHONE
    /// <summary>
    /// This class is implemented to store user settings in an Isolated storage file.
    /// </summary>
    [Serializable]
    public class IsolatedStorageSettings : Dictionary<string, object>
    {
        const string filename = "Settings.bin";
        static string fullpath = AppDomain.CurrentDomain.BaseDirectory + filename;

        /// <summary>
        /// Its a private constructor.
        /// </summary>
        private IsolatedStorageSettings() : base() { }

        /// <summary>
        /// Its a static singleton instance.
        /// </summary>
        public static IsolatedStorageSettings ApplicationSettings { get; private set; }

        protected IsolatedStorageSettings(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        /// <summary>
        /// Its static constructor.
        /// </summary>
        static IsolatedStorageSettings()
        {
            if (!File.Exists(fullpath))
                ApplicationSettings = new IsolatedStorageSettings();
            else
                try
                {
                    using (FileStream stream = new FileStream(fullpath, FileMode.Open))
                        ApplicationSettings = (IsolatedStorageSettings)new BinaryFormatter().Deserialize(stream);
                }
                catch (Exception)
                {
                    ApplicationSettings = new IsolatedStorageSettings();
                }
        }

        /// <summary>
        /// It serializes dictionary in binary format and stores it in a binary file.
        /// </summary>
        public void Save()
        {
            try
            {
                using (FileStream stream = new FileStream(fullpath, FileMode.Create))
                    new BinaryFormatter().Serialize(stream, (Dictionary<string, object>)ApplicationSettings);
            }
            catch (Exception)
            {

            }
        }
    }
#endif

    /// <summary>
    /// Provides methods for interacting with the ApplicationSettings.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Gets the <code>IsolatedStorageSettings.ApplicationSettings</code> object.
        /// </summary>
        public static IsolatedStorageSettings NativeObject { get; private set; }

        static AppSettings()
        {
            NativeObject = IsolatedStorageSettings.ApplicationSettings;
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
            if (Contains(key))
                result = (T)NativeObject[key];
            else
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
            return NativeObject.ContainsKey(key);
        }

        /// <summary>
        /// Removes the entry with the specified key.
        /// </summary>
        /// <param name="key">The key for the entry to be deleted.</param>
        /// <returns>true if the specified key was removed; otherwise, false.</returns>
        public bool Remove(string key)
        {
            bool result = NativeObject.Remove(key);
            Save();
            return result;
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
                Save();
            }
        }

        /// <summary>
        /// Gets a value for the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The value will be returned if the specified key is not found.</param>
        /// <returns>The value associated with the specified key if the key is found; otherwise, <paramref name="defaultValue"/></returns>
        public object this[string key, object defaultValue]
        {
            get
            {
                return GetValue(key, defaultValue);
            }
            set
            {
                NativeObject[key] = value;
                Save();
            }
        }
    }
}
