namespace WinRTLockscreen
{
    using System;

    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Windows.Forms;

    using Microsoft.ServiceBus.Notifications;

    using Shared;

    using Outlook = Microsoft.Office.Interop.Outlook;

    public partial class ThisAddIn
    {
        private int unreadMail;

        /// <summary>
        /// Tag for sending
        /// </summary>
        private string HubTag;

        private Outlook.MAPIFolder inbox;

        private Outlook.Items items;

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            this.inbox =
                this.Application.ActiveExplorer().Session.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderInbox);

            this.items = this.inbox.Items;
            
            this.items.ItemAdd += this.MailCountChanged;                       
            this.items.ItemRemove += this.MailCountChanged;           
            this.items.ItemChange += this.MailCountChanged;
   
            // Read settings
            this.ReadSettings();

            // Trigger initial write
            this.MailCountChanged();
        }

        private void MailCountChanged()
        {
            this.MailCountChanged(null);
        }

        private void ReadSettings()
        {
            try
            {
                using (
                    var file =
                        File.Open(
                            Path.Combine(
                                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "packages", GlobalConstants.packageName, "LocalState", GlobalConstants.SettingsFileName),
                            FileMode.Open,
                            FileAccess.Read))
                {
                    var serializer = new DataContractSerializer(typeof(SettingsModel));

                    this.Settings = (SettingsModel)serializer.ReadObject(file);
                }
            }
            catch (Exception)
            {
                // Fail silently here..
                this.Settings = new SettingsModel();
            }
        }

        private SettingsModel Settings { get; set; }

        private async void MailCountChanged(object item)
        {
            // Update unread count            
            int unreadItems = this.items.Restrict("[Unread]=true").Count;

            if (unreadItems == this.unreadMail)
            {
                // Nothing changed, do not update
                return;
            }

            this.unreadMail = unreadItems;

            if (this.Settings.UseFile)
            {
                try
                {
                    using (
                        var file =
                            File.Open(
                                Path.Combine(
                                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                    "packages",
                                    GlobalConstants.packageName,
                                    "LocalState", "outlook.txt"),
                                FileMode.Create,
                                FileAccess.Write,
                                FileShare.ReadWrite))
                    {
                        using (var writer = new StreamWriter(file))
                        {
                            writer.WriteLine(this.unreadMail);
                        }
                    }
                }
                catch (Exception)
                {
                    // Fail silently here.. 
                    MessageBox.Show(
                        "Cannot write to shared file in order to communicate with application. Is the application installed?",
                        "WinRTOutlookLockscreen");
                }
            }

            if (this.Settings.UsePush)
            {

                var hub =
                    NotificationHubClient.CreateClientFromConnectionString(
                        GlobalConstants.NotificationHubSendingSecret,
                        GlobalConstants.NotificationHubName);

                var toast = string.Format("<badge value=\"{0}\" />", this.unreadMail);

                await hub.SendWindowsNativeNotificationAsync(toast, this.HubTag);
            }
        }        

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            this.items.ItemAdd -= this.MailCountChanged;
            this.items.ItemRemove -= this.MailCountChanged;
            this.items.ItemChange -= this.MailCountChanged;

            Marshal.ReleaseComObject(this.items);
            Marshal.ReleaseComObject(this.inbox);
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
