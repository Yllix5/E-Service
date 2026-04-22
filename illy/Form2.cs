using System;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace illy
{
    public partial class Form2 : Form
    {
        private string connectionString =
            "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";
        private static readonly HttpClient httpClient = new HttpClient();
        private bool isDragging = false;
        private Point dragStartPoint;

        public Form2()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
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

        private async void resetoFjalekalimin_Click(object sender, EventArgs e)
        {
            // Merr IP PUBLIKE (jo private)
            string ipAddress = await GetPublicIPAddress() ?? GetIPAddress();
            string phoneNumber = nrTelefonit.Text.Trim();
            string deviceId = GetDeviceId();
            bool isWhitelisted = IsWhitelistedIP(ipAddress);

            // Kontrollo fushat
            if (string.IsNullOrWhiteSpace(phoneNumber) ||
                string.IsNullOrWhiteSpace(nrKontrates.Text) ||
                string.IsNullOrWhiteSpace(nrPersonal.Text) ||
                string.IsNullOrWhiteSpace(backupCode.Text))
            {
                MessageBox.Show("Ju lutem plotësoni të gjitha fushat!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogFailedVerification(phoneNumber, ipAddress);
                CheckAndBlockAfterFailure(phoneNumber, ipAddress, deviceId, isWhitelisted);
                return;
            }

            // Kontrollo nëse IP-ja PUBLIKE është e bllokuar (vetëm për IP jo të bardhë)
            if (!isWhitelisted && IsIPBlocked(ipAddress))
            {
                MessageBox.Show("IP-ja juaj është bllokuar për shkak të përpjekjeve të shumta të dështuara!", "IP Bllokuar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogFailedVerification(phoneNumber, ipAddress);
                return;
            }

            // Kontrollo nëse pajisja është e bllokuar (për IP të bardhë)
            if (isWhitelisted && IsDeviceBlocked(deviceId, phoneNumber))
            {
                MessageBox.Show("Ky pajisje është e bllokuar për 24 orë!", "Bllokuar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    // Merr Salt_PersonalNumber
                    string saltQuery = @"
                        SELECT Salt_PersonalNumber
                        FROM Userat
                        WHERE PhoneNumber = @phone
                        AND ContractNumber = @contract";
                    byte[] saltPersonalNumber;
                    using (SqlCommand saltCmd = new SqlCommand(saltQuery, con))
                    {
                        saltCmd.Parameters.AddWithValue("@phone", phoneNumber);
                        saltCmd.Parameters.AddWithValue("@contract", nrKontrates.Text);
                        var result = saltCmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value)
                        {
                            LogFailedVerification(phoneNumber, ipAddress);
                            CheckAndBlockAfterFailure(phoneNumber, ipAddress, deviceId, isWhitelisted);
                            MessageBox.Show("Të dhënat tuaja nuk u gjetën. Kontrolloni sërish.", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        saltPersonalNumber = (byte[])result;
                    }

                    // Verifiko të dhënat e plota
                    string query = @"
                        SELECT COUNT(1)
                        FROM Userat
                        WHERE PhoneNumber = @phone
                        AND ContractNumber = @contract
                        AND PersonalNumber = HASHBYTES('SHA2_256', @personal + CAST(@saltPN AS NVARCHAR(32)))
                        AND BackupCode = @backup";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@phone", phoneNumber);
                        cmd.Parameters.AddWithValue("@contract", nrKontrates.Text);
                        cmd.Parameters.AddWithValue("@personal", nrPersonal.Text);
                        cmd.Parameters.AddWithValue("@saltPN", saltPersonalNumber);
                        cmd.Parameters.AddWithValue("@backup", backupCode.Text);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());

                        if (count == 1)
                        {
                            ResetFailedAttempts(phoneNumber);
                            MessageBox.Show("Të dhënat janë të sakta! Vazhdo me ndryshimin e fjalëkalimit.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            vendosFjalekalimTeRi setPasswordForm = new vendosFjalekalimTeRi(phoneNumber, nrKontrates.Text);
                            setPasswordForm.ShowDialog();
                            this.Hide();
                        }
                        else
                        {
                            LogFailedVerification(phoneNumber, ipAddress);
                            CheckAndBlockAfterFailure(phoneNumber, ipAddress, deviceId, isWhitelisted);
                            MessageBox.Show("Një ose më shumë të dhëna janë të pasakta!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Gabim gjatë verifikimit: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LogFailedVerification(phoneNumber, ipAddress);
                    CheckAndBlockAfterFailure(phoneNumber, ipAddress, deviceId, isWhitelisted);
                }
            }
        }

        // Metodë për të kontrolluar dhe bllokuar pas dështimit
        private void CheckAndBlockAfterFailure(string phoneNumber, string ipAddress, string deviceId, bool isWhitelisted)
        {
            int attempts = GetFailedAttempts(phoneNumber);
            if (attempts >= 3)
            {
                if (isWhitelisted)
                {
                    BlockDevice(deviceId, phoneNumber, ipAddress);
                    MessageBox.Show("Ky pajisje është bllokuar për 24 orë për shkak të përpjekjeve të shumta të dështuara!", "Bllokuar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    BlockIP(ipAddress);
                    MessageBox.Show("IP-ja juaj është bllokuar për 24 orë për shkak të përpjekjeve të shumta të dështuara!", "IP Bllokuar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async Task<string> GetPublicIPAddress()
        {
            try
            {
                var response = await httpClient.GetStringAsync("http://api.ipify.org");
                return response.Trim();
            }
            catch
            {
                return null;
            }
        }

        private string GetIPAddress()
        {
            try
            {
                return System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList
                    .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.ToString() ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private string GetDeviceId()
        {
            string machineName = Environment.MachineName;
            string deviceIdFile = Path.Combine(Application.StartupPath, "device_id.txt");
            if (File.Exists(deviceIdFile))
            {
                return File.ReadAllText(deviceIdFile);
            }
            string deviceId = $"{machineName}-{Guid.NewGuid().ToString()}";
            File.WriteAllText(deviceIdFile, deviceId);
            return deviceId;
        }

        private bool IsWhitelistedIP(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress) || ipAddress == "Unknown")
                return false;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT COUNT(*) FROM WhitelistedIPs WHERE IPAddress = @ip";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@ip", ipAddress);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        private void LogFailedVerification(string phoneNumber, string ipAddress)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    string query = "INSERT INTO FailedLogins (Username, AttemptTime, IPAddress) VALUES (@username, @attemptTime, @ipAddress)";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@username", phoneNumber?.Trim() ?? "Unknown");
                        cmd.Parameters.AddWithValue("@attemptTime", DateTime.Now);
                        cmd.Parameters.AddWithValue("@ipAddress", ipAddress);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Gabim gjatë regjistrimit të përpjekjes: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private int GetFailedAttempts(string phoneNumber)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT COUNT(*) FROM FailedLogins WHERE Username = @username";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@username", phoneNumber?.Trim() ?? "Unknown");
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        private void ResetFailedAttempts(string phoneNumber)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "DELETE FROM FailedLogins WHERE Username = @username";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@username", phoneNumber?.Trim() ?? "Unknown");
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private bool IsIPBlocked(string ipAddress)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT COUNT(*) FROM BlockedIPs WHERE IPAddress = @ipAddress AND ExpirationTime > GETDATE()";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@ipAddress", ipAddress);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        private bool IsDeviceBlocked(string deviceId, string username)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = @"
                    SELECT COUNT(*)
                    FROM BlockedDevices
                    WHERE DeviceID = @deviceId
                      AND Username = @username
                      AND ExpirationTime > GETDATE()";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@deviceId", deviceId);
                    cmd.Parameters.AddWithValue("@username", username);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        private void BlockIP(string ipAddress)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "INSERT INTO BlockedIPs (IPAddress, BlockTime, ExpirationTime) VALUES (@ipAddress, @blockTime, @expirationTime)";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@ipAddress", ipAddress);
                    cmd.Parameters.AddWithValue("@blockTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("@expirationTime", DateTime.Now.AddHours(24));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void BlockDevice(string deviceId, string username, string ipAddress)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = @"
                    INSERT INTO BlockedDevices (DeviceID, Username, IPAddress, BlockTime, ExpirationTime)
                    VALUES (@deviceId, @username, @ipAddress, @blockTime, @expirationTime)";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@deviceId", deviceId);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@ipAddress", ipAddress ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@blockTime", DateTime.Now);
                    cmd.Parameters.AddWithValue("@expirationTime", DateTime.Now.AddHours(24));
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}