using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace illy
{
    public partial class PerkrahjaTeknike: Form
    {
        private bool isDragging = false;
        private Point dragStartPoint;
        public PerkrahjaTeknike()
        {
            InitializeComponent();

            this.MouseDown += Form2_MouseDown;
            this.MouseMove += Form2_MouseMove;
            this.MouseUp += Form2_MouseUp;
        }

        private void Form2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStartPoint = new Point(e.X, e.Y);
            }
        }

        private void Form2_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point p = PointToScreen(new Point(e.X, e.Y));
                this.Location = new Point(p.X - dragStartPoint.X, p.Y - dragStartPoint.Y);
            }
        }

        private void Form2_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Adresa e email-it të kingut
                string emailAddress = "yqafleshi123@gmail.com";

                // Subjekti i email-it
                string subject = "Përshëndetje";

                // Përmbajtja (body) e email-it
                string body = "Përshëndetje o ma i rani,\n\nQysh e bone qit projekt be????\ni qart je fort<3!";

                // Kodoj subjektin dhe përmbajtjen për t'i bërë URL-friendly
                string encodedSubject = Uri.EscapeDataString(subject);
                string encodedBody = Uri.EscapeDataString(body);

                // Krijo URL-në e Gmail me parametrat to, su, dhe body
                string gmailUrl = $"https://mail.google.com/mail/u/1/?fs=1&tf=cm&source=mailto&to={emailAddress}&su={encodedSubject}&body={encodedBody}";

                // Hap Gmail në shfletues me email-in e ri e merr nga sistemi operativ./1
                System.Diagnostics.Process.Start(gmailUrl);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë hapjes së email-it: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
