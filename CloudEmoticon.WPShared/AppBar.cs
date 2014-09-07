using Microsoft.Phone.Shell;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Microsoft.Phone.Controls;

namespace Simon.Library.Controls
{
    /// <summary>
    /// Defines events and properties for the Application Bar in Windows Phone applications.
    /// </summary>
    [ContentProperty("Items")]
    public class AppBar : ItemsControl, IApplicationBar
    {
        /// <summary>
        /// Gets the wrapped <code>ApplicationBar</code> object.
        /// </summary>
        public ApplicationBar WrappedObject { get; private set; }

        public List<AppBarIconButton> ButtonList { get; private set; }
        public List<AppBarMenuItem> MenuItemList { get; private set; }

        /// <summary>
        /// Initializes a new instance of the Simon.Library.Controls.AppBar class.
        /// </summary>
        public AppBar()
        {
            WrappedObject = new ApplicationBar();
            WrappedObject.MatchOverriddenTheme();

            ButtonList = new List<AppBarIconButton>();
            MenuItemList = new List<AppBarMenuItem>();
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            if (DesignerProperties.IsInDesignTool)
                return;

            ButtonList.Clear();
            WrappedObject.Buttons.Clear();
            MenuItemList.Clear();
            WrappedObject.MenuItems.Clear();
            foreach (object item in Items)
            {
                if (item is AppBarIconButton)
                {
                    AppBarIconButton button = (AppBarIconButton)item;
                    button.Owner = this;
                    if (button.Visiable)
                        WrappedObject.Buttons.Add(button.WrappedObject);
                    ButtonList.Add(button);
                }
                else if (item is AppBarMenuItem)
                {
                    WrappedObject.MenuItems.Add(((AppBarMenuItem)item).WrappedObject);
                    MenuItemList.Add((AppBarMenuItem)item);
                }
                else
                    throw new InvalidCastException();
            }
        }

        public void RebuildButtons()
        {
            WrappedObject.Buttons.Clear();
            foreach (AppBarIconButton button in ButtonList)
                if (button.Visiable)
                    WrappedObject.Buttons.Add(button.WrappedObject);
        }

        #region IApplicationBar
        /// <summary>
        /// Occurs when the user opens or closes the Application Bar by clicking the
        /// ellipsis.
        /// </summary>
        public event EventHandler<ApplicationBarStateChangedEventArgs> StateChanged
        {
            add { WrappedObject.StateChanged += value; }
            remove { WrappedObject.StateChanged -= value; }
        }

        /// <summary>
        /// Gets or sets the background color of the Application Bar.
        /// </summary>
        public Color BackgroundColor
        {
            get { return WrappedObject.BackgroundColor; }
            set { WrappedObject.BackgroundColor = value; }
        }

        /// <summary>
        /// Gets the list of icon buttons that appear on the Application Bar.
        /// </summary>
        public IList Buttons
        {
            get { return WrappedObject.Buttons; }
        }

        /// <summary>
        /// Gets the distance that the Application Bar extends into a page when the
        /// <code>Microsoft.Phone.Shell.IApplicationBar.Mode</code> property is set
        /// to <code>Microsoft.Phone.Shell.ApplicationBarMode.Default</code>.
        /// </summary>
        public double DefaultSize
        {
            get { return WrappedObject.DefaultSize; }
        }

        /// <summary>
        /// Gets or sets the foreground color of the Application Bar.
        /// </summary>
        public Color ForegroundColor
        {
            get { return WrappedObject.ForegroundColor; }
            set { WrappedObject.ForegroundColor = value; }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the user sees the (optional)
        /// menu items when they click the ellipsis to expand the Application Bar.
        /// </summary>
        public bool IsMenuEnabled
        {
            get { return WrappedObject.IsMenuEnabled; }
            set { WrappedObject.IsMenuEnabled = value; }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the Application Bar is visible.
        /// </summary>
        public bool IsVisible
        {
            get { return WrappedObject.IsVisible; }
            set { WrappedObject.IsVisible = value; }
        }

        /// <summary>
        /// Gets the list of the menu items that appear on the Application Bar.
        /// </summary>
        public IList MenuItems
        {
            get { return WrappedObject.MenuItems; }
        }

        /// <summary>
        /// Gets the distance that the Application Bar extends into a page when the 
        /// <code>Microsoft.Phone.Shell.IApplicationBar.Mode</code> property is set 
        /// to <code>Microsoft.Phone.Shell.ApplicationBarMode.Minimized</code>.
        /// </summary>
        public double MiniSize
        {
            get { return WrappedObject.MiniSize; }
        }

        /// <summary>
        /// Gets or sets the size of the Application Bar.
        /// </summary>
        public ApplicationBarMode Mode
        {
            get { return WrappedObject.Mode; }
            set { WrappedObject.Mode = value; }
        }
        #endregion
    }
}
