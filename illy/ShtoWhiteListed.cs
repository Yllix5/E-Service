using System;
using System.Data.SqlClient;
using System.Net;
using System.Windows.Forms;
using System.Drawing;

namespace illy
{
    public partial class ShtoWhiteListed : Form
    {
        private string connectionString =
            "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";

        private bool isDragging = false;
        private Point dragStartPoint;
        public ShtoWhiteListed()
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
        private void shtoIPbutton_Click(object sender, EventArgs e)
        {
            string ip = IPTextBox.Text.Trim();
            string ipConfirm = perseritTextBox.Text.Trim();

            // 1. Kontrollo nëse fushat janë bosh
            if (string.IsNullOrWhiteSpace(ip) || string.IsNullOrWhiteSpace(ipConfirm))
            {
                MessageBox.Show("Ju lutem plotësoni të dy fushat për IP-në!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Kontrollo nëse IP-të përputhen
            if (ip != ipConfirm)
            {
                MessageBox.Show("IP-të nuk përputhen! Ju lutem kontrolloni dhe provoni përsëri.", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                perseritTextBox.Focus();
                perseritTextBox.SelectAll();
                return;
            }

            // 3. Kontrollo nëse IP-ja është valide (IPv4 ose IPv6)
            if (!IsValidIP(ip))
            {
                MessageBox.Show("IP-ja nuk është valide! Ju lutem futni një adresë IP korrekte (IPv4 ose IPv6).", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                IPTextBox.Focus();
                IPTextBox.SelectAll();
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // 4. Kontrollo nëse IP-ja ekziston tashmë
                    string checkQuery = "SELECT COUNT(*) FROM WhitelistedIPs WHERE IPAddress = @IP";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                    {
                        checkCmd.Parameters.AddWithValue("@IP", ip);
                        int count = (int)checkCmd.ExecuteScalar();

                        if (count > 0)
                        {
                            MessageBox.Show("Kjo IP tashmë është e shtuar në listën e bardhë!", "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }

                    // 5. Shto IP-në e re
                    string insertQuery = "INSERT INTO WhitelistedIPs (IPAddress) VALUES (@IP)";
                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, con))
                    {
                        insertCmd.Parameters.AddWithValue("@IP", ip);
                        insertCmd.ExecuteNonQuery();
                    }

                    MessageBox.Show($"IP-ja '{ip}' u shtua me sukses në listën e bardhë!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Pastro fushat pas shtimit
                    IPTextBox.Clear();
                    perseritTextBox.Clear();
                    IPTextBox.Focus();


                    
                    
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë shtimit të IP-së:\n{ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Funksion për të kontrolluar nëse IP-ja është valide (IPv4 ose IPv6)
        private bool IsValidIP(string ip)
        {
            return IPAddress.TryParse(ip, out _);
        }

        // Opsionale: buton Anulo / Mbyll
        private void btnAnulo_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}