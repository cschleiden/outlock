namespace WinRTOutlookLockscreenApp.BackgroundTasks
{
    using Windows.ApplicationModel.Background;

    public sealed class BackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferal = taskInstance.GetDeferral();

            await Logic.UpdateCountFromFile();

            deferal.Complete();
        }
    }
}
