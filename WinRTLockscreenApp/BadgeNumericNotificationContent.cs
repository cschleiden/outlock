namespace WinRTOutlookLockscreenApp
{
    using System;

    using Windows.Data.Xml.Dom;
    using Windows.UI.Notifications;

    public sealed class BadgeNumericNotificationContent
    {
        /// <summary> 
        /// Default constructor to create a numeric badge content object. 
        /// </summary> 
        public BadgeNumericNotificationContent()
        {
        }

        /// <summary> 
        /// Constructor to create a numeric badge content object with a number. 
        /// </summary> 
        /// <param name="number"> 
        /// The number that will appear on the badge.  If the number is 0, the badge 
        /// will be removed.  The largest value that will appear on the badge is 99. 
        /// Numbers greater than 99 are allowed, but will be displayed as "99+". 
        /// </param> 
        public BadgeNumericNotificationContent(uint number)
        {
            this.m_Number = number;
        }

        /// <summary> 
        /// The number that will appear on the badge.  If the number is 0, the badge 
        /// will be removed.  The largest value that will appear on the badge is 99. 
        /// Numbers greater than 99 are allowed, but will be displayed as "99+". 
        /// </summary> 
        public uint Number
        {
            get { return this.m_Number; }
            set { this.m_Number = value; }
        }

        /// <summary> 
        /// Retrieves the notification Xml content as a string. 
        /// </summary> 
        /// <returns>The notification Xml content as a string.</returns> 
        public string GetContent()
        {
            return String.Format("<badge version='{0}' value='{1}'/>", 1, this.m_Number);
        }

        /// <summary> 
        /// Retrieves the notification Xml content as a string. 
        /// </summary> 
        /// <returns>The notification Xml content as a string.</returns> 
        public override string ToString()
        {
            return this.GetContent();
        }

#if !WINRT_NOT_PRESENT
        /// <summary> 
        /// Retrieves the notification Xml content as a WinRT Xml document. 
        /// </summary> 
        /// <returns>The notification Xml content as a WinRT Xml document.</returns> 
        public XmlDocument GetXml()
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(this.GetContent());
            return xml;
        }

        /// <summary> 
        /// Creates a WinRT BadgeNotification object based on the content. 
        /// </summary> 
        /// <returns>A WinRT BadgeNotification object based on the content.</returns> 
        public BadgeNotification CreateNotification()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(this.GetContent());
            return new BadgeNotification(xmlDoc);
        }
#endif

        private uint m_Number = 0;
    }
}