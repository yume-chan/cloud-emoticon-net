using System;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

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
            SupportedOrientations = SupportedPageOrientation.Portrait;
            Orientation = PageOrientation.Portrait;

            SetValue(TiltEffect.IsTiltEnabledProperty, true);

            SetValue(SystemTray.ProgressIndicatorProperty, ProgressIndicator);
            SetValue(SystemTray.IsVisibleProperty, true);

            NavigationInTransition navigateInTransition = new NavigationInTransition();
            navigateInTransition.Backward = new TurnstileFeatherTransition { Mode = TurnstileFeatherTransitionMode.BackwardIn };
            navigateInTransition.Forward = new TurnstileFeatherTransition { Mode = TurnstileFeatherTransitionMode.ForwardIn };
            TransitionService.SetNavigationInTransition(this, navigateInTransition);

            NavigationOutTransition navigateOutTransition = new NavigationOutTransition();
            navigateOutTransition.Backward = new TurnstileFeatherTransition { Mode = TurnstileFeatherTransitionMode.BackwardOut };
            navigateOutTransition.Forward = new TurnstileFeatherTransition { Mode = TurnstileFeatherTransitionMode.ForwardOut };
            TransitionService.SetNavigationOutTransition(this, navigateOutTransition);

            lastOrientation = Orientation;
            OrientationChanged += new EventHandler<OrientationChangedEventArgs>(Page_OrientationChanged);
        }

        public static readonly DependencyProperty AppTitleProperty = DependencyProperty.Register("AppTitle",
            typeof(string), typeof(AppPage), new PropertyMetadata("", titleChangedCallback));
        public string AppTitle
        {
            get { return (string)GetValue(AppTitleProperty); }
            set { SetValue(AppTitleProperty, value); }
        }
        private static void titleChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PhoneApplicationPage)d).Title = (string)e.NewValue;
            ProgressIndicator.AppTitle = (string)e.NewValue;
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
