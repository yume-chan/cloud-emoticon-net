using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Simon.Library.Controls
{
    /// <summary>
    /// An item that can be added to the menu of an Simon.Library.Controls.AppBar.
    /// </summary>
    public class AppBarMenuItem : FrameworkElement, IApplicationBarMenuItem
    {
        /// <summary>
        /// Gets the wrapped Microsoft.Phone.Shell.ApplicationBarMenuItem object.
        /// </summary>
        public ApplicationBarMenuItem WrappedObject { get; private set; }

        /// <summary>
        /// Creates a new instance of the Simon.Library.Controls.AppBarMenuItem
        ///    class.
        /// </summary>
        public AppBarMenuItem()
        {
            WrappedObject = new ApplicationBarMenuItem();
        }

        /// <summary>
        /// Creates a new instance of the Simon.Library.Controls.AppBarMenuItem
        ///    class.
        /// </summary>
        /// <param name="text">The string to display as the menu item.</param>
        public AppBarMenuItem(string text)
            : this()
        {
            Text = text;
        }

        #region IApplicationBarMenuItem
        /// <summary>
        /// Occurs when the user taps the menu item.
        /// </summary>
        public event EventHandler Click
        {
            add { WrappedObject.Click += value; }
            remove { WrappedObject.Click -= value; }
        }

        /// <summary>
        /// Gets or sets the enabled status of the menu item.
        /// </summary>
        public bool IsEnabled
        {
            get { return WrappedObject.IsEnabled; }
            set { WrappedObject.IsEnabled = value; }
        }

        /// <summary>
        /// Identifies the Simon.Library.Controls.AppBarMenuItem.TextProperty dependency property.
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(AppBarMenuItem), new PropertyMetadata("", onTextChanged));
        /// <summary>
        /// Gets or sets the string that appears as the menu item.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        private static void onTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((AppBarMenuItem)sender).WrappedObject.Text = (string)e.NewValue;
        }
        #endregion
    }
}
