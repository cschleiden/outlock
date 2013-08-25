namespace WinRTOutlookLockscreenApp
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.Messaging;

    using Shared;

    using Windows.ApplicationModel.Background;
    using Windows.Networking.PushNotifications;
    using Windows.Storage;
    using Windows.UI.Notifications;

    public static class Logic
    {
        public static SettingsModel Settings { get; set; }

        public static Task LoadSettings()
        {
            return Task.Run(
                async () =>
                    {
                        try
                        {
                            var localFolder = ApplicationData.Current.LocalFolder;
                            var settingsFile = await localFolder.GetFileAsync(GlobalConstants.SettingsFileName);

                            var serializer = new DataContractSerializer(typeof(SettingsModel));

                            using (var stream = await settingsFile.OpenReadAsync())
                            {
                                Settings = (SettingsModel)serializer.ReadObject(stream.AsStreamForRead());
                            }
                        }
                        catch (Exception)
                        {
                            // Revert to default config
                            Settings = new SettingsModel() { Tag = Guid.NewGuid().ToString() };
                        }
                    });
        }

        public static Task SaveSettings()
        {
            return Task.Run(async () =>
                    {
                        // Save to file
                        var localFolder = ApplicationData.Current.LocalFolder;
                        var settingsFile =
                            await localFolder.CreateFileAsync(GlobalConstants.SettingsFileName, CreationCollisionOption.ReplaceExisting);

                        var serializer = new DataContractSerializer(typeof(SettingsModel));
                        using (var stream = await settingsFile.OpenAsync(FileAccessMode.ReadWrite))
                        {
                            serializer.WriteObject(stream.AsStreamForWrite(), Settings);
                        }
                    });
        }

        public static void InitBackgroundTask()
        {
            const string TaskName = "UpdateMailCountBackgroundTask";

            var taskRegistered = BackgroundTaskRegistration.AllTasks.Any(task => task.Value.Name == TaskName);
            if (!taskRegistered && Settings.UseFile)
            {
                // Register task
                var builder = new BackgroundTaskBuilder
                {
                    Name = TaskName,
                    TaskEntryPoint = "WinRTOutlookLockscreenApp.BackgroundTasks.BackgroundTask"
                };


                builder.SetTrigger(new TimeTrigger(15, false));

                var task = builder.Register();
            }
            else if (taskRegistered && !Settings.UseFile)
            {
                // Unregister
                var existingTask = BackgroundTaskRegistration.AllTasks.Single(task => task.Value.Name == TaskName);
                existingTask.Value.Unregister(true);
            }
        }

        public static async Task InitNotificationsAsync()
        {
            if (Settings.UsePush)
            {
                await SetupChannel();
            }
            else
            {
                await RemoveChannel();
            }

            const string TaskName = "UpdateMailCountBackgroundTaskPush";

            var taskRegistered = BackgroundTaskRegistration.AllTasks.Any(task => task.Value.Name == TaskName);

            if (!taskRegistered && Settings.UsePush)
            {
                // Register task
                var builder = new BackgroundTaskBuilder
                                  {
                                      Name = TaskName,
                                      TaskEntryPoint =
                                          "WinRTOutlookLockscreenApp.BackgroundTasks.PushBackgroundTask"
                                  };

                // Refresh every 20 days
                builder.SetTrigger(new TimeTrigger((uint)TimeSpan.FromDays(20).TotalMinutes, false));

                var task = builder.Register();
            }
            else if (taskRegistered && !Settings.UsePush)
            {
                BackgroundTaskRegistration.AllTasks.Single(task => task.Value.Name == TaskName).Value.Unregister(true);
            }
        }

        public static async Task RemoveChannel()
        {
            var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();

            using (var hub = new NotificationHub(GlobalConstants.NotificationHubName, GlobalConstants.NotificationHubListeningSecret))
            {
                await hub.UnregisterAllAsync(channel.Uri);
            }

            channel.Close();
        }

        public static async Task SetupChannel()
        {
            var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();

            using (var hub = new NotificationHub(GlobalConstants.NotificationHubName, GlobalConstants.NotificationHubListeningSecret))
            {
                await hub.RegisterNativeAsync(channel.Uri, new[] { Logic.Settings.Tag });
            }
        }

        public static async Task UpdateCountFromFile()
        {
            // Read file
            var localFolder = ApplicationData.Current.LocalFolder;

            try
            {
                var outlookFile = await localFolder.GetFileAsync("outlook.txt");

                string countString = await FileIO.ReadTextAsync(outlookFile);

                uint count = uint.Parse(countString);

                // Set Count
                var badgeContent = new BadgeNumericNotificationContent(count);
                BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(badgeContent.CreateNotification());
            }
            catch (Exception)
            {
                // Please move on, there is nothing to see here
            }
        }

        public static async Task Update()
        {
            InitBackgroundTask();
            await InitNotificationsAsync();

            await UpdateCountFromFile();
        }
    }
}