using System;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Phone.Shell;

namespace Simon.Library.Controls
{
    /// <summary>
    /// Provides methods and properties for interacting with the progress indicator on the system tray on an application page.
    /// </summary>
    public class AppProgressIndicator : ProgressIndicator
    {
        private DispatcherTimer timer = new DispatcherTimer();
        private Dispatcher dispatcher = Deployment.Current.Dispatcher;

        /// <summary>
        /// Creates a new instance of the <code>AppProgressIndicator</code> class.
        /// </summary>
        public AppProgressIndicator()
            : base()
        {
            base.IsVisible = true;
            timer.Tick += timer_Tick;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (AppTitle != null)
            {
                Text = AppTitle;
                IsIndeterminate = false;
                Value = 0;
            }
            timer.Stop();

            if (Hided != null)
                if (dispatcher.CheckAccess())
                    Hided(this, new EventArgs());
                else
                    dispatcher.BeginInvoke(Hided, this, new EventArgs());
        }

        private void hide(int timeout)
        {
            if (timeout == 0)
                timer.Stop();
            else
            {
                if (timer.IsEnabled)
                    timer.Stop();

                timer.Interval = TimeSpan.FromMilliseconds(timeout);
                timer.Start();
            }
        }

        public event EventHandler Hided;

        private string backing_AppTitle;

        public string AppTitle
        {
            get { return backing_AppTitle; }
            set
            {
                if (backing_AppTitle != value)
                {
                    backing_AppTitle = value;
                    if (!timer.IsEnabled)
                        Text = value;
                }
            }
        }

        /// <summary>
        /// Hide the ProgressIndicator after the defined timeout.
        /// </summary>
        /// <param name="timeout">Time before the ProgressIndicator hide.</param>
        public void Hide(int timeout)
        {
            if (dispatcher.CheckAccess())
                hide(timeout);
            else
                dispatcher.BeginInvoke(() => { hide(timeout); });
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the progress indicator on the
        /// system tray on the current application page is determinate or indeterminate.
        /// </summary>
        /// <value>
        /// true if the progress indicator is indeterminate; false if the progress bar
        /// is determinate.
        /// </value>
        public new bool IsIndeterminate
        {
            get { return base.IsIndeterminate; }
            set
            {
                bool newValue = value;

                if (dispatcher.CheckAccess())
                    base.IsIndeterminate = newValue;
                else
                    dispatcher.BeginInvoke(() => { base.IsIndeterminate = newValue; });
            }
        }

        /// <summary>
        /// Gets the visibility of the progress indicator on the system tray
        /// on the current application page.
        /// </summary>
        /// <value>
        /// true if the progress indicator is visible; otherwise, false.
        /// </value>
        public new bool IsVisible
        {
            get { return true; }
        }

        /// <summary>
        /// Gets or sets the text of the progress indicator on the system tray on the
        /// current application page.
        /// </summary>
        /// <value>
        /// The text of the progress indicator on the system tray.
        /// </value>
        public new string Text
        {
            get { return base.Text; }
            set
            {
                string newValue;
                if (value != null && !string.IsNullOrWhiteSpace(value))
                    newValue = value;
                else
                    newValue = AppTitle;

                if (dispatcher.CheckAccess())
                    base.Text = newValue;
                else
                    dispatcher.BeginInvoke(() => { base.Text = newValue; });
            }
        }

        /// <summary>
        /// Gets or sets the value of the progress indicator on the system tray on the
        /// current application page.
        /// </summary>
        /// <value>
        /// The value of the progress indicator on the system tray.
        /// </value>
        public new double Value
        {
            get { return base.Value; }
            set
            {
                double newValue = value;

                if (dispatcher.CheckAccess())
                    base.Value = newValue;
                else
                    dispatcher.BeginInvoke(() => { base.Value = newValue; });
            }
        }
    }
}
