using System.Windows.Forms;

namespace MacroMechanicsHub.Services
{
    public class NotificacionService
    {
        private readonly NotifyIcon _notifyIcon;

        public NotificacionService(NotifyIcon notifyIcon)
        {
            _notifyIcon = notifyIcon;
        }

        public void ShowNotification(string title, string message)
        {
            _notifyIcon.BalloonTipTitle = title;
            _notifyIcon.BalloonTipText = message;
            _notifyIcon.ShowBalloonTip(3000);
        }
    }
}
