namespace WinRTOutlookLockscreenApp.BackgroundTasks
{
    using Windows.ApplicationModel.Background;

    public sealed class PushBackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferal = taskInstance.GetDeferral();

            await Logic.SetupChannel();

            deferal.Complete();
        }
    }
}
