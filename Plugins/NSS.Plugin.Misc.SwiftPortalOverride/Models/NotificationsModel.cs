using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Models
{
    public class NotificationsModel
    {
        public NotificationsModel()
        {
            NotificationContext = new NotificationModel();
        }
        public bool HasError
        { 
            get => !string.IsNullOrEmpty(Error); 
            set { HasError = value; } 
        }
        public string Error { get; set; }
        public NotificationModel NotificationContext { get; set; }


        #region Nested Classes

        public class NotificationModel
        {
            public NotificationModel()
            {
                Notifications = new List<NotificationItemModel>();
            }
            public IList<NotificationItemModel> Notifications { get; set; }
        }

        public class NotificationItemModel
        {
            public NotificationItemModel()
            {
                Preferences = new List<PreferenceModel>();
            }
            public string Title { get; set; }
            public IList<PreferenceModel> Preferences { get; set; }
        }

        public class PreferenceModel
        {
            public string Key { get; set; }
            public bool Value { get; set; }
        }

        // for controller update req
        public class NotificationUpdateModel
        {
            public IList<PreferenceModel> Preferences { get; set; }
        }

        #endregion

    }
}
