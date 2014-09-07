using Simon.Library;
using System.Windows;

namespace CloudEmoticon
{
    /// <summary>
    /// Interaction logic for xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// An instance of <code>AppSettings</code>
        /// </summary>
        public static AppSettings Settings = new AppSettings();

        /// <summary>
        /// An instance of <code>ViewModel</code>
        /// </summary>
        public static ViewModel ViewModel = ViewModel.Instance;
    }
}
