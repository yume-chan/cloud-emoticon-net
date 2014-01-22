using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Windows;

namespace Simon.Library.Controls
{
    /// <summary>
    /// The class for the AppPage control for Windows Phone projects.
    /// </summary>
    public class AppPage : PhoneApplicationPage
    {
        private PageOrientation lastOrientation;

        /// <summary>
        /// Gets a value that indicate the global <code>ProgressIndicator</code>
        /// </summary>
        public static readonly AppProgressIndicator ProgressIndicator = new AppProgressIndicator();

        /// <summary>
        /// Initializes a new instance of the GooglePlus.Library.Controls.AppPage class.
        /// </summary>
        public AppPage()
            : base()
        {
            this.SetValue(TiltEffect.IsTiltEnabledProperty, true);

            NavigationInTransition navigateInTransition = new NavigationInTransition();
            navigateInTransition.Backward = new TurnstileTransition { Mode = TurnstileTransitionMode.BackwardIn };
            navigateInTransition.Forward = new TurnstileTransition { Mode = TurnstileTransitionMode.ForwardIn };
            TransitionService.SetNavigationInTransition(this, navigateInTransition);

            NavigationOutTransition navigateOutTransition = new NavigationOutTransition();
            navigateOutTransition.Backward = new TurnstileTransition { Mode = TurnstileTransitionMode.BackwardOut };
            navigateOutTransition.Forward = new TurnstileTransition { Mode = TurnstileTransitionMode.ForwardOut };
            TransitionService.SetNavigationOutTransition(this, navigateOutTransition);

            lastOrientation = this.Orientation;
            OrientationChanged += new EventHandler<OrientationChangedEventArgs>(Page_OrientationChanged);

            Loaded += new RoutedEventHandler(Page_Loaded);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SystemTray.ProgressIndicator = ProgressIndicator;
        }

        private void Page_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            PageOrientation newOrientation = e.Orientation;

            // Orientations are (clockwise) 'PortraitUp', 'LandscapeRight', 'LandscapeLeft'

            RotateTransition transitionElement = new RotateTransition();

            switch (newOrientation)
            {
                case PageOrientation.Landscape:
                case PageOrientation.LandscapeRight:
                    // Come here from PortraitUp (i.e. clockwise) or LandscapeLeft?
                    if (lastOrientation == PageOrientation.PortraitUp)
                        transitionElement.Mode = RotateTransitionMode.In90Counterclockwise;
                    else
                        transitionElement.Mode = RotateTransitionMode.In180Clockwise;
                    break;
                case PageOrientation.LandscapeLeft:
                    // Come here from LandscapeRight or PortraitUp?
                    if (lastOrientation == PageOrientation.LandscapeRight)
                        transitionElement.Mode = RotateTransitionMode.In180Counterclockwise;
                    else
                        transitionElement.Mode = RotateTransitionMode.In90Clockwise;
                    break;
                case PageOrientation.Portrait:
                case PageOrientation.PortraitUp:
                    // Come here from LandscapeLeft or LandscapeRight?
                    if (lastOrientation == PageOrientation.LandscapeLeft)
                        transitionElement.Mode = RotateTransitionMode.In90Counterclockwise;
                    else
                        transitionElement.Mode = RotateTransitionMode.In90Clockwise;
                    break;
                default:
                    break;
            }

            // Execute the transition
            PhoneApplicationPage phoneApplicationPage = (PhoneApplicationPage)(((PhoneApplicationFrame)Application.Current.RootVisual)).Content;
            ITransition transition = transitionElement.GetTransition(phoneApplicationPage);
            transition.Completed += delegate
            {
                transition.Stop();
            };
            transition.Begin();

            lastOrientation = newOrientation;
        }

        public static readonly DependencyProperty AppBarProperty =
            DependencyProperty.Register("AppBar", typeof(AppBar), typeof(AppPage), new PropertyMetadata(null, onAppBarChanged));
        /// <summary>
        /// Gets or sets the AppBar of AppPage.
        /// </summary>
        public AppBar AppBar
        {
            get { return (AppBar)GetValue(AppBarProperty); }
            set { SetValue(AppBarProperty, value); }
        }
        private static void onAppBarChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            AppPage page = (AppPage)sender;
            AppBar appBar = (AppBar)e.NewValue;
            if (appBar != null)
                page.ApplicationBar = appBar.WrappedObject;
            else
                page.ApplicationBar = null;
        }
    }
}
