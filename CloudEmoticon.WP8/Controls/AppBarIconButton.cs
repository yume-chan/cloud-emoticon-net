using Microsoft.Phone.Shell;
using System;
using System.Windows;

namespace Simon.Library.Controls
{
    /// <summary>
    /// An Application Bar button with an icon.
    /// </summary>
    public class AppBarIconButton : FrameworkElement, IApplicationBarIconButton
    {
        /// <summary>
        /// Gets the wrapped Microsoft.Phone.Shell.ApplicationBarIconButton object.
        /// </summary>
        public ApplicationBarIconButton WrappedObject { get; private set; }

        /// <summary>
        /// Creates a new instance of the Simon.Library.Controls.AppBarIconButton
        ///    class.
        /// </summary>
        public AppBarIconButton()
        {
            WrappedObject = new ApplicationBarIconButton();
        }

        /// <summary>
        /// Creates a new instance of the Simon.Library.Controls.AppBarIconButton
        ///    class with the specified icon.
        /// </summary>
        /// <param name="iconUri">The URI of the icon to use for the button.</param>
        public AppBarIconButton(Uri iconUri)
            : this()
        {
            WrappedObject.IconUri = iconUri;
        }

        #region IApplicationBarIconButton
        /// <summary>
        /// Occurs when the user taps a button in the Application Bar.
        /// </summary>
        public event EventHandler Click
        {
            add { WrappedObject.Click += value; }
            remove { WrappedObject.Click -= value; }
        }

        /// <summary>
        /// Gets or sets the URI of the icon to use for the button.
        /// </summary>
        public Uri IconUri
        {
            get { return WrappedObject.IconUri; }
            set { WrappedObject.IconUri = value; }
        }

        /// <summary>
        /// Gets or sets the enabled status of the button.
        /// </summary>
        public bool IsEnabled
        {
            get { return WrappedObject.IsEnabled; }
            set { WrappedObject.IsEnabled = value; }
        }

        /// <summary>
        /// Identifies the Simon.Library.Controls.AppBarButton.TextProperty dependency property.
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(AppBarIconButton), new PropertyMetadata("", onTextChanged));
        /// <summary>
        /// Gets or sets the label for the icon button.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        private static void onTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((AppBarIconButton)sender).WrappedObject.Text = (string)e.NewValue;
        }
        #endregion
    }
}
