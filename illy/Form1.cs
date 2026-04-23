using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Oauth2.v2;
using Google.Apis.Util.Store;
using System.Drawing;
using System.Threading;
using Newtonsoft.Json;
using System.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace illy
{
    public partial class Form1 : Form
    {
        private string connectionString =
            "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";
        private const int MaxLoginAttempts = 3;
        private const string VPN_API_KEY = ""; // Shto VPN API KEY KETU!!!
        private static readonly HttpClient client = new HttpClient();
        private bool isDragging = false;
        private Point dragStartPoint;

        public Form1(bool showPasswordChangedMessage = false)
        {
            InitializeComponent();

            NotifyHelper.Initialize(notifyIcon1);
            Perdoruesi.Text = string.Empty;
            Fjalekalimi.Text = string.Empty;
            shfaqPasswordin.Checked = false;
            Fjalekalimi.UseSystemPasswordChar = true;

            this.MouseDown += Form2_MouseDown;
            this.MouseMove += Form2_MouseMove;
            this.MouseUp += Form2_MouseUp;

            if (showPasswordChangedMessage)
            {
                MessageBox.Show("Fjalëkalimi juaj u ndryshua me sukses! Ju lutem kyçuni me fjalëkalimin e ri.", "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

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

        private string GetUsernameFromUserId(int userId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT Username FROM Userat WHERE UserID = @id";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@id", userId);
                    return cmd.ExecuteScalar()?.ToString() ?? "Unknown";
                }
            }
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

        private async void kycuButton_Click(object sender, EventArgs e)
        {
            // Ndal klikimet e shumëfishta: Disable butonin menjëherë
            kycuButton.Enabled = false;

            try
            {
                string ipAddress = await GetPublicIPAddress();
                if (string.IsNullOrEmpty(ipAddress) || ipAddress == "Unknown")
                {
                    MessageBox.Show("Nuk mund të merret IP-ja publike. Provoni më vonë ose kontrolloni lidhjen.", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string username = Perdoruesi.Text.Trim();
                string deviceId = GetDeviceId();
                bool isWhitelisted = IsWhitelistedIP(ipAddress);
                bool isSuspiciousIP = await CheckIPForVPNProxyTor(ipAddress);

                if (isSuspiciousIP)
                {
                    MessageBox.Show("Qasja u refuzua! Ju jeni duke përdorur një VPN, Proxy, ose Tor.", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (isWhitelisted && IsDeviceBlocked(deviceId, username))
                {
                    MessageBox.Show("Ky pajisje është e bllokuar për 24 orë!", "Bllokuar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!isWhitelisted && IsIPBlocked(ipAddress))
                {
                    MessageBox.Show("Ky IP është i bllokuar për shkak të përpjekjeve të dështuara!", "Bllokuar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(Fjalekalimi.Text))
                {
                    MessageBox.Show("Ju lutem plotësoni të gjitha fushat!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LogFailedLogin(username, ipAddress, false, deviceId);
                    int deviceAttempts = GetFailedAttemptsByDevice(deviceId, username, ipAddress, isWhitelisted);
                    if (deviceAttempts >= MaxLoginAttempts)
                    {
                        if (isWhitelisted)
                            BlockDevice(deviceId, username, ipAddress);
                        else
                            BlockIP(ipAddress);
                        MessageBox.Show("Ky pajisje / IP është bllokuar për 24 orë për shkak të përpjekjeve të dështuara!", "Bllokuar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    return;
                }

                int attempts = GetFailedAttempts(username);
                if (attempts >= MaxLoginAttempts)
                {
                    LogFailedLogin(username, ipAddress, false, deviceId);
                    int deviceAttempts = GetFailedAttemptsByDevice(deviceId, username, ipAddress, isWhitelisted);
                    if (deviceAttempts >= MaxLoginAttempts)
                    {
                        if (isWhitelisted)
                            BlockDevice(deviceId, username, ipAddress);
                        else
                            BlockIP(ipAddress);
                        MessageBox.Show("Ky pajisje / IP është bllokuar për 24 orë për shkak të përpjekjeve të dështuara!", "Bllokuar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    MessageBox.Show("Shumë përpjekje të dështuara! Provo përsëri pas disa minutash ose kontakto administratorin.", "Bllokuar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    try
                    {
                        con.Open();

                        string saltQuery = "SELECT Salt, RoleID FROM Userat WHERE Username = @username";
                        byte[] salt = null;
                        int roleId = -1;

                        using (SqlCommand saltCmd = new SqlCommand(saltQuery, con))
                        {
                            saltCmd.Parameters.AddWithValue("@username", username);
                            using (SqlDataReader reader = saltCmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    salt = (byte[])reader["Salt"];
                                    roleId = (int)reader["RoleID"];
                                }
                                else
                                {
                                    LogFailedLogin(username, ipAddress, false, deviceId);
                                    int deviceAttempts = GetFailedAttemptsByDevice(deviceId, username, ipAddress, isWhitelisted);
                                    if (deviceAttempts >= MaxLoginAttempts)
                                    {
                                        if (isWhitelisted)
                                            BlockDevice(deviceId, username, ipAddress);
                                        else
                                            BlockIP(ipAddress);
                                        MessageBox.Show("Ky pajisje / IP është bllokuar për 24 orë për shkak të përpjekjeve të dështuara!", "Bllokuar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                    MessageBox.Show("Përdoruesi nuk ekziston!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }
                            }
                        }

                        string query = "SELECT UserID FROM Userat WHERE Username = @username AND Password = HASHBYTES('SHA2_256', @password + CAST(@salt AS NVARCHAR(32)))";

                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@username", username);
                            cmd.Parameters.AddWithValue("@password", Fjalekalimi.Text);
                            cmd.Parameters.AddWithValue("@salt", salt);

                            var userIdResult = cmd.ExecuteScalar();

                            if (userIdResult != null)
                            {
                                int userId = Convert.ToInt32(userIdResult);
                                LogSuccessfulLogin(userId, ipAddress, false, deviceId);
                                ResetFailedAttempts(username);
                                MessageBox.Show("Kyçja u bë me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                this.Hide();
                                OpenDashboardBasedOnRole(userId, roleId);
                            }
                            else
                            {
                                LogFailedLogin(username, ipAddress, false, deviceId);
                                int deviceAttempts = GetFailedAttemptsByDevice(deviceId, username, ipAddress, isWhitelisted);
                                if (deviceAttempts >= MaxLoginAttempts)
                                {
                                    if (isWhitelisted)
                                        BlockDevice(deviceId, username, ipAddress);
                                    else
                                        BlockIP(ipAddress);
                                    MessageBox.Show("Ky pajisje / IP është bllokuar për 24 orë për shkak të përpjekjeve të dështuara!", "Bllokuar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                                MessageBox.Show("Përdoruesi ose fjalëkalimi i gabuar!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Gabim gjatë kyçjes: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        LogFailedLogin(username, ipAddress, false, deviceId);
                        int deviceAttempts = GetFailedAttemptsByDevice(deviceId, username, ipAddress, isWhitelisted);
                        if (deviceAttempts >= MaxLoginAttempts)
                        {
                            if (isWhitelisted)
                                BlockDevice(deviceId, username, ipAddress);
                            else
                                BlockIP(ipAddress);
                            MessageBox.Show("Ky pajisje / IP është bllokuar për 24 orë për shkak të përpjekjeve të dështuara!", "Bllokuar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            finally
            {
                // Ri-aktivizo butonin pas operacionit (edhe nëse ka error ose sukses)
                kycuButton.Enabled = true;
            }
        }

        private async void googlePicture_Click(object sender, EventArgs e)
        {
            
            googlePicture.Enabled = false;

            try
            {
                string ipAddress = await GetPublicIPAddress();
                if (string.IsNullOrEmpty(ipAddress) || ipAddress == "Unknown")
                {
                    MessageBox.Show("Nuk mund të merret IP-ja publike. Provoni më vonë.", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string deviceId = GetDeviceId();
                bool isWhitelisted = IsWhitelistedIP(ipAddress);
                bool isSuspiciousIP = await CheckIPForVPNProxyTor(ipAddress);

                if (isSuspiciousIP)
                {
                    MessageBox.Show("Qasja u refuzua! Ju jeni duke përdorur një VPN, Proxy, ose Tor.", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (isWhitelisted && IsDeviceBlocked(deviceId, "GoogleUser"))
                {
                    MessageBox.Show("Ky pajisje është e bllokuar për 24 orë!", "Bllokuar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!isWhitelisted && IsIPBlocked(ipAddress))
                {
                    MessageBox.Show("Ky IP është i bllokuar për shkak të përpjekjeve të dështuara!", "Bllokuar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int attempts = GetFailedAttempts("GoogleUser");
                if (attempts >= MaxLoginAttempts)
                {
                    LogFailedLogin("GoogleUser", ipAddress, false, deviceId);
                    int deviceAttempts = GetFailedAttemptsByDevice(deviceId, "GoogleUser", ipAddress, isWhitelisted);
                    if (deviceAttempts >= MaxLoginAttempts)
                    {
                        if (isWhitelisted)
                            BlockDevice(deviceId, "GoogleUser", ipAddress);
                        else
                            BlockIP(ipAddress);
                        MessageBox.Show("Ky pajisje / IP është bllokuar për 24 orë për shkak të përpjekjeve të dështuara!", "Bllokuar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    MessageBox.Show("Shumë përpjekje të dështuara me Google!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // ====================== GOOGLE LOGIN - KODI I PLOTË ======================
                try
                {
                    string[] scopes = { "https://www.googleapis.com/auth/userinfo.email", "https://www.googleapis.com/auth/userinfo.profile" };
                    string clientId = ""; // Gjenero google clientID...
                    string clientSecret = ""; // Gjenero clientSecret ...
                    var fileDataStore = new FileDataStore(Guid.NewGuid().ToString(), true);

                    var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        new ClientSecrets
                        {
                            ClientId = clientId,
                            ClientSecret = clientSecret
                        },
                        scopes,
                        "user",
                        CancellationToken.None,
                        fileDataStore);

                    if (credential != null)
                    {
                        var service = new Oauth2Service(new Google.Apis.Services.BaseClientService.Initializer
                        {
                            HttpClientInitializer = credential
                        });

                        var userInfo = await service.Userinfo.Get().ExecuteAsync();
                        string googleId = userInfo.Id;
                        string email = userInfo.Email;
                        string username = email.Split('@')[0];

                        if (!email.EndsWith("@universitetiaab.com", StringComparison.OrdinalIgnoreCase))
                        {
                            MessageBox.Show("Kyçja me Google lejohet vetëm për email-et e universitetit AAB (@universitetiaab.com)!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            LogFailedLogin("GoogleUser", ipAddress, false, deviceId);
                            return;
                        }

                        using (SqlConnection con = new SqlConnection(connectionString))
                        {
                            con.Open();

                            string query = "SELECT UserID, RoleID FROM Userat WHERE GoogleID = @googleId";

                            using (SqlCommand cmd = new SqlCommand(query, con))
                            {
                                cmd.Parameters.AddWithValue("@googleId", googleId);

                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        int userId = (int)reader["UserID"];
                                        int roleId = (int)reader["RoleID"];
                                        LogSuccessfulLogin(userId, ipAddress, false, deviceId);
                                        ResetFailedAttempts("GoogleUser");
                                        ResetFailedAttempts(username);
                                        MessageBox.Show($"Kyçja me Google u bë me sukses! Mirë se erdhe, {email}", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        this.Hide();
                                        OpenDashboardBasedOnRole(userId, roleId);
                                    }
                                    else
                                    {
                                        string insertQuery = "INSERT INTO Userat (Username, Password, Salt, PhoneNumber, ContractNumber, Email, GoogleID, RoleID) " +
                                                             "OUTPUT INSERTED.UserID " +
                                                             "VALUES (@username, @password, @salt, @phone, @contract, @email, @googleId, @roleId)";

                                        using (SqlCommand insertCmd = new SqlCommand(insertQuery, con))
                                        {
                                            insertCmd.Parameters.AddWithValue("@username", username);
                                            insertCmd.Parameters.AddWithValue("@password", new byte[] { });
                                            insertCmd.Parameters.AddWithValue("@salt", new byte[] { });
                                            insertCmd.Parameters.AddWithValue("@phone", "N/A");
                                            insertCmd.Parameters.AddWithValue("@contract", "N/A");
                                            insertCmd.Parameters.AddWithValue("@email", email);
                                            insertCmd.Parameters.AddWithValue("@googleId", googleId);
                                            insertCmd.Parameters.AddWithValue("@roleId", 1);

                                            int newUserId = (int)insertCmd.ExecuteScalar();

                                            LogSuccessfulLogin(newUserId, ipAddress, false, deviceId);
                                            ResetFailedAttempts("GoogleUser");
                                            ResetFailedAttempts(username);
                                            MessageBox.Show($"Regjistrimi dhe kyçja me Google u bë me sukses! Mirë se erdhe, {email}", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                            this.Hide();
                                            OpenDashboardBasedOnRole(newUserId, 1);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Gabim gjatë kyçjes me Google: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LogFailedLogin("GoogleUser", ipAddress, false, deviceId);
                }
            }
            finally
            {
                // Ri-aktivizo butonin pas operacionit
                googlePicture.Enabled = true;
            }
        }

        private async Task<bool> CheckIPForVPNProxyTor(string ipAddress)
        {
            try
            {
                if (string.IsNullOrEmpty(ipAddress) || ipAddress == "Unknown")
                    return true;

                string apiUrl = $"https://vpnapi.io/api/{ipAddress}?key={VPN_API_KEY}";
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                    return true;

                string jsonResponse = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(jsonResponse);

                bool isVPN = result.security.vpn;
                bool isProxy = result.security.proxy;
                bool isTor = result.security.tor;

                return isVPN || isProxy || isTor;
            }
            catch
            {
                return true;
            }
        }

        private async Task<string> GetPublicIPAddress()
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync("http://api.ipify.org");
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStringAsync();
                return null;
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
                var ipAddress = Dns.GetHostEntry(Dns.GetHostName())
                    .AddressList
                    .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                return ipAddress?.ToString() ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private void OpenDashboardBasedOnRole(int userId, int roleId)
        {
            string username = GetUsernameFromUserId(userId);

            switch (roleId)
            {
                case 1: // Student
                    Form3 studentDashboard = new Form3(userId);
                    studentDashboard.Show();
                    NotifyHelper.SetCurrentForm(studentDashboard, "E-Studenti", username);
                    break;

                case 2: // Professor
                    ProfessorDashboard professorDashboard = new ProfessorDashboard(userId);
                    professorDashboard.Show();
                    NotifyHelper.SetCurrentForm(professorDashboard, "E-Profesori", username);
                    break;

                case 3: // Admin
                    AdminDashboard adminDashboard = new AdminDashboard(userId);
                    adminDashboard.Show();
                    NotifyHelper.SetCurrentForm(adminDashboard, "E-Administratori", username);
                    break;

                default:
                    MessageBox.Show("Roli i përdoruesit nuk është i njohur!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Show();
                    return;
            }

            this.Hide();
        }

        private void LogFailedLogin(string username, string ipAddress, bool isSuspiciousIP, string deviceId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    string query = "INSERT INTO FailedLogins (Username, AttemptTime, IPAddress, IsSuspiciousIP, DeviceID) VALUES (@username, @attemptTime, @ipAddress, @isSuspiciousIP, @deviceId)";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@attemptTime", DateTime.Now);
                        cmd.Parameters.AddWithValue("@ipAddress", ipAddress);
                        cmd.Parameters.AddWithValue("@isSuspiciousIP", isSuspiciousIP ? 1 : 0);
                        cmd.Parameters.AddWithValue("@deviceId", deviceId);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Gabim gjatë regjistrimit të përpjekjes së dështuar: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LogSuccessfulLogin(int userId, string ipAddress, bool isSuspiciousIP, string deviceId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    string query = "INSERT INTO LoginLogs (UserID, LoginTime, IPAddress, IsSuspiciousIP, DeviceID) VALUES (@userId, @loginTime, @ipAddress, @isSuspiciousIP, @deviceId)";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@loginTime", DateTime.Now);
                        cmd.Parameters.AddWithValue("@ipAddress", ipAddress);
                        cmd.Parameters.AddWithValue("@isSuspiciousIP", isSuspiciousIP ? 1 : 0);
                        cmd.Parameters.AddWithValue("@deviceId", deviceId);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Gabim gjatë regjistrimit të kyçjes: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private int GetFailedAttempts(string username)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT COUNT(*) FROM FailedLogins WHERE Username = @username";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@username", username?.Trim() ?? "Unknown");
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        private void ResetFailedAttempts(string username)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "DELETE FROM FailedLogins WHERE Username = @username";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@username", username?.Trim() ?? "Unknown");
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private int GetFailedAttemptsByDevice(string deviceId, string username, string ipAddress, bool isWhitelisted)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = isWhitelisted
                    ? @"SELECT COUNT(*) FROM FailedLogins WHERE DeviceID = @deviceId AND Username = @username"
                    : @"SELECT COUNT(*) FROM FailedLogins WHERE IPAddress = @ipAddress AND Username = @username";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@deviceId", deviceId);
                    cmd.Parameters.AddWithValue("@username", username);
                    if (!isWhitelisted)
                        cmd.Parameters.AddWithValue("@ipAddress", ipAddress);

                    return Convert.ToInt32(cmd.ExecuteScalar());
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

        private void shfaqPasswordin_CheckedChanged(object sender, EventArgs e)
        {
            Fjalekalimi.UseSystemPasswordChar = !shfaqPasswordin.Checked;
        }

        private void resetFjalekalimi_Click(object sender, EventArgs e)
        {
            Form2 resetForm = new Form2();
            resetForm.ShowDialog();
        }

        private void guna2PictureBox3_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.facebook.com/AAB.edu/?locale=sq_AL");
        }

        private void guna2PictureBox4_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.instagram.com/kolegji_aab/?hl=en");
        }

        private void guna2PictureBox5_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://x.com/KolegjiAAB?ref_src=twsrc%5Egoogle%7Ctwcamp%5Eserp%7Ctwgr%5Eauthor");
        }

        private void guna2ControlBox1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // When the login form is closing (logout or exit), clean everything
            NotifyHelper.ClearCurrentForm();
            base.OnFormClosing(e);
        }
    }
}
