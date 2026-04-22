using System.Windows.Forms;

namespace illy
{
    public static class NotifyHelper
    {
        public static NotifyIcon NotifyIcon { get; private set; }
        private static Form currentDashboard = null;

        public static void Initialize(NotifyIcon notifyIcon)
        {
            if (notifyIcon == null) return;

            NotifyIcon = notifyIcon;
            NotifyIcon.Visible = true;
            NotifyIcon.Text = "E-Service is running";

            // Klikimi ose double click toggle show/hide
            NotifyIcon.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ToggleCurrentForm();
                }
            };

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Show Dashboard", null, (s, e) => ShowCurrentForm());
            menu.Items.Add("-");
            menu.Items.Add("Exit Application", null, (s, e) => Application.Exit());
            NotifyIcon.ContextMenuStrip = menu;
        }

        public static void SetCurrentForm(Form form, string appName, string username)
        {
            currentDashboard = form;

            string tooltip = $"{appName} - {username}";
            if (NotifyIcon != null)
            {
                NotifyIcon.Text = tooltip;
                NotifyIcon.ShowBalloonTip(4000, appName, $"Logged in as: {username}", ToolTipIcon.Info);
            }
        }

        public static void ClearCurrentForm()
        {
            currentDashboard = null;

            if (NotifyIcon != null)
            {
                NotifyIcon.Visible = false;
                NotifyIcon.Dispose();
                NotifyIcon = null;
            }
        }

        private static void ToggleCurrentForm()
        {
            if (currentDashboard == null) return;

            if (currentDashboard.Visible)
            {
                currentDashboard.Hide();
            }
            else
            {
                ShowCurrentForm();
            }
        }

        private static void ShowCurrentForm()
        {
            if (currentDashboard == null) return;

            currentDashboard.Show();
            currentDashboard.WindowState = FormWindowState.Normal;
            currentDashboard.BringToFront();
            currentDashboard.Activate();
        }
    }
}