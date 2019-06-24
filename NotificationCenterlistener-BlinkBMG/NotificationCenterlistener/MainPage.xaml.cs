using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace NotificationCenterlistener
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        bool isAccessGranted = false;
        public MainPage()
        {
            this.InitializeComponent();
            NotificationListView = new ListView();
            CheckNotificationAccess();
            CheckNotifications();

        }

        public async void CheckNotifications()
        {
        }

        public async void CheckNotificationAccess()
        {

            // Get the listener
            UserNotificationListener listener = UserNotificationListener.Current;

            // And request access to the user's notifications (must be called from UI thread)
            UserNotificationListenerAccessStatus accessStatus = await listener.RequestAccessAsync();

            switch (accessStatus)
            {
                // This means the user has granted access.
                case UserNotificationListenerAccessStatus.Allowed:
                    // Yay! Proceed as normal
                    isAccessGranted = true;
                    // Subscribe to foreground event
                    listener.NotificationChanged += Listener_NotificationChanged;
                    // Get the toast notifications
                    //IReadOnlyList<UserNotification> notifs = await listener.GetNotificationsAsync(NotificationKinds.Toast);
                    break;

                // This means the user has denied access.
                // Any further calls to RequestAccessAsync will instantly
                // return Denied. The user must go to the Windows settings
                // and manually allow access.
                case UserNotificationListenerAccessStatus.Denied:

                    // Show UI explaining that listener features will not
                    // work until user allows access.
                    break;

                // This means the user closed the prompt without
                // selecting either allow or deny. Further calls to
                // RequestAccessAsync will show the dialog again.
                case UserNotificationListenerAccessStatus.Unspecified:

                    // Show UI that allows the user to bring up the prompt again
                    break;
            }
            
        }

        public class NotificationCenterData
        {
            public string AppName;
            public string Title;
            public string Body;
        }

        private async void Listener_NotificationChanged(UserNotificationListener sender, UserNotificationChangedEventArgs args)
        {
            IReadOnlyList<UserNotification> notifs = await sender.GetNotificationsAsync(NotificationKinds.Toast);
            ObservableCollection<NotificationCenterData> listItems = new ObservableCollection<NotificationCenterData>();

            Debug.WriteLine("--------------NEW Event Received----------------");
            foreach(var notif in notifs)
            {
                NotificationBinding toastBinding = notif.Notification.Visual.GetBinding(KnownNotificationBindings.ToastGeneric);
                if (toastBinding != null)
                {
                    // And then get the text elements from the toast binding
                    IReadOnlyList<AdaptiveNotificationText> textElements = toastBinding.GetTextElements();

                    // Treat the first text element as the title text
                    string titleText = textElements.FirstOrDefault()?.Text;

                    // We'll treat all subsequent text elements as body text,
                    // joining them together via newlines.
                    string bodyText = string.Join("\n", textElements.Skip(1).Select(t => t.Text));

                    string payload = "App: " + notif.AppInfo.DisplayInfo.DisplayName + " => Title: " + titleText + ", Body: " + bodyText;
                    Debug.WriteLine(payload);
                    listItems.Add(new NotificationCenterData() {
                        AppName = notif.AppInfo.DisplayInfo.DisplayName,
                        Title = titleText,
                        Body = bodyText
                    });
                }
            }
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                NotificationListView.ItemsSource = listItems;
            });
        }

    }
}
