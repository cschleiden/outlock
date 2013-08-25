// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace WinRTOutlookLockscreenApp
{
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Restore settings
            this.ToggleFile.IsOn = Logic.Settings.UseFile;
            this.TogglePush.IsOn = Logic.Settings.UsePush;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.UpdateSettings(this.ToggleFile.IsOn, this.TogglePush.IsOn);
        }

        private async void UpdateSettings(bool useFile, bool usePush)
        {
            // Write updated settings to local storage
            Logic.Settings.UseFile = useFile;
            Logic.Settings.UsePush = usePush;            

            // Update tasks
            Logic.Update();

            // Persist
            Logic.SaveSettings();
        }
    }
}
