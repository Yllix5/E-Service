using System;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Drawing;

namespace illy
{
    public partial class vendosFjalekalimTeRi : Form
    {
        private string connectionString =
        "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";

        private string phoneNumber;
        private string contractNumber;

        private bool isDragging = false;
        private Point dragStartPoint;
        public vendosFjalekalimTeRi(string phoneNumber, string contractNumber)
        {
            InitializeComponent();
            textPassword.UseSystemPasswordChar = true;
            konfirmojePassword.UseSystemPasswordChar = true;
            this.phoneNumber = phoneNumber;
            this.contractNumber = contractNumber;
           
            this.FormBorderStyle = FormBorderStyle.None; // Forma pa kornizë
            this.StartPosition = FormStartPosition.CenterScreen;
            // Shto event handlers për zhvendosjen
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

        private void textPassword_TextChanged(object sender, EventArgs e)
        {
            // Mund të shtosh logjikë për të kontrolluar fjalëkalimin në kohë reale (opsionale)
        }

        private void konfirmojePassword_TextChanged(object sender, EventArgs e)
        {
            // Mund të shtosh logjikë për të kontrolluar konfirmimin në kohë reale (opsionale)
        }

        private void shfaqPasswordin_CheckedChanged(object sender, EventArgs e)
        {
            if (shfaqPasswordin.Checked)
            {
                textPassword.UseSystemPasswordChar = false;
                konfirmojePassword.UseSystemPasswordChar = false;
            }
            else
            {
                textPassword.UseSystemPasswordChar = true;
                konfirmojePassword.UseSystemPasswordChar = true;
            }
        }

        private void ndryshoButton_Click(object sender, EventArgs e)
        {
            // Kontrollo nëse fushat janë të zbrazëta
            if (string.IsNullOrWhiteSpace(textPassword.Text) || string.IsNullOrWhiteSpace(konfirmojePassword.Text))
            {
                MessageBox.Show("Ju lutem plotësoni të gjitha fushat!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Kontrollo nëse fjalëkalimet përputhen
            if (textPassword.Text != konfirmojePassword.Text)
            {
                MessageBox.Show("Fjalëkalimet nuk përputhen!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Kontrollo kriteret e fjalëkalimit (opsionale)
            if (textPassword.Text.Length < 8)
            {
                MessageBox.Show("Fjalëkalimi duhet të jetë të paktën 8 karaktere i gjatë!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    // Merr UserID bazuar në PhoneNumber dhe ContractNumber
                    int userId;
                    string getUserIdQuery = "SELECT UserID FROM Userat WHERE PhoneNumber = @phone AND ContractNumber = @contract";
                    using (SqlCommand getUserIdCmd = new SqlCommand(getUserIdQuery, con))
                    {
                        getUserIdCmd.Parameters.AddWithValue("@phone", phoneNumber);
                        getUserIdCmd.Parameters.AddWithValue("@contract", contractNumber);
                        var userIdResult = getUserIdCmd.ExecuteScalar();
                        if (userIdResult == null)
                        {
                            MessageBox.Show("Përdoruesi nuk u gjet! Ju lutem kontaktoni administratorin.", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        userId = Convert.ToInt32(userIdResult);
                    }

                    // Gjenero Salt të ri
                    byte[] newSalt = Guid.NewGuid().ToByteArray().Take(16).ToArray();
                    string newPassword = textPassword.Text;

                    // Hash fjalëkalimin e ri me Salt të ri
                    string updateQuery = "UPDATE Userat " +
                                        "SET Password = HASHBYTES('SHA2_256', @newPassword + CAST(@newSalt AS NVARCHAR(32))), " +
                                        "Salt = @newSalt, " +
                                        "UpdatedAT = GETDATE() " +
                                        "WHERE PhoneNumber = @phone AND ContractNumber = @contract";

                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, con))
                    {
                        updateCmd.Parameters.AddWithValue("@newPassword", newPassword);
                        updateCmd.Parameters.AddWithValue("@newSalt", newSalt);
                        updateCmd.Parameters.AddWithValue("@phone", phoneNumber);
                        updateCmd.Parameters.AddWithValue("@contract", contractNumber);

                        int rowsAffected = updateCmd.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            MessageBox.Show("Përdoruesi nuk u gjet! Ju lutem kontaktoni administratorin.", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    // Regjistro ndryshimin e fjalëkalimit në PasswordChangeLogs
                    LogPasswordChange(userId, GetIPAddress());

                    MessageBox.Show("Fjalëkalimi u vendos me sukses! Ju do të ktheheni në formën e kyçjes për të përdorur fjalëkalimin e ri.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Mbyll formën aktuale dhe trego sukses
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Gabim gjatë vendosjes së fjalëkalimit: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string GetIPAddress()
        {
            return Dns.GetHostEntry(Dns.GetHostName()).AddressList
                .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.ToString() ?? "Unknown";
        }

        private void LogPasswordChange(int userId, string ipAddress)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    string query = "INSERT INTO PasswordChangeLogs (UserID, ChangeTime, IPAddress) VALUES (@userId, @changeTime, @ipAddress)";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@changeTime", DateTime.Now);
                        cmd.Parameters.AddWithValue("@ipAddress", ipAddress);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Gabim gjatë regjistrimit të ndryshimit të fjalëkalimit: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}