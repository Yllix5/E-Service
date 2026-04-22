using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace illy
{
    public partial class adminBallinaForm : Form
    {
        private string connectionString =
            "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";
        private int userId;

        public adminBallinaForm(int userId)
        {
            InitializeComponent();
            this.userId = userId;

            logsComboBox.Items.AddRange(new string[]
            {
                "Failed Logins",
                "Successful Logins",
                "Blocked Devices",
                "Blocked IPs",
                "Whitelisted IPs"
            });
            logsComboBox.SelectedIndex = 0;
            logsComboBox.SelectedIndexChanged += LogsComboBox_SelectedIndexChanged;

            this.Shown += async (s, e) => await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            await Task.Run(() =>
            {
                string username = GetUsername();
                string machineName = Environment.MachineName;
                (string currentLogin, string previousLogin) = GetLastLogins();

                this.Invoke((MethodInvoker)(() =>
                {
                    label1.Text = $"Përshëndetje {username}!";
                    label5.Text = machineName;
                    label10.Text = previousLogin;
                    label12.Text = currentLogin;
                }));
            });

            LoadLogsData(); // Ngarko fillimisht log-et
        }

        private string GetUsername()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT Username FROM Userat WHERE UserID = @UserID";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        return cmd.ExecuteScalar()?.ToString() ?? "Administrator";
                    }
                }
            }
            catch
            {
                return "Administrator";
            }
        }

        private (string current, string previous) GetLastLogins()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                        SELECT TOP 2 LoginTime
                        FROM LoginLogs
                        WHERE UserID = @UserID
                        ORDER BY LoginTime DESC";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            string current = "Nuk ka të dhëna";
                            string previous = "Asnjë login paraprak";

                            if (reader.Read())
                            {
                                current = reader.GetDateTime(0).ToString("dd/MM/yyyy HH:mm:ss");
                                if (reader.Read())
                                {
                                    previous = reader.GetDateTime(0).ToString("dd/MM/yyyy HH:mm:ss");
                                }
                            }

                            return (current, previous);
                        }
                    }
                }
            }
            catch
            {
                return ("Gabim", "Gabim");
            }
        }

        private void LoadLogsData()
        {
            string selectedLog = logsComboBox.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedLog)) return;

            string query = "";

            switch (selectedLog)
            {
                case "Failed Logins":
                    query = "SELECT AttemptTime, Username, IPAddress FROM FailedLogins ORDER BY AttemptTime DESC";
                    break;
                case "Successful Logins":
                    query = @"
                        SELECT ll.LoginTime, u.Username, ll.IPAddress
                        FROM LoginLogs ll
                        JOIN Userat u ON ll.UserID = u.UserID
                        ORDER BY ll.LoginTime DESC";
                    break;
                case "Blocked Devices":
                    query = "SELECT DeviceID, Username, IPAddress, BlockTime, ExpirationTime FROM BlockedDevices ORDER BY BlockTime DESC";
                    break;
                case "Blocked IPs":
                    query = "SELECT IPAddress, BlockTime, ExpirationTime FROM BlockedIPs ORDER BY BlockTime DESC";
                    break;
                case "Whitelisted IPs":
                    query = "SELECT IPAddress FROM WhitelistedIPs";
                    break;
                default:
                    return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, con))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        logsDataGridView.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ngarkimit të {selectedLog}:\n{ex.Message}",
                                "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LogsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadLogsData();
        }

        // Butoni Shto IP të bardhë
        private void ShtoButton_Click(object sender, EventArgs e)
        {
            using (var shto = new ShtoWhiteListed())
            {
                DialogResult result = shto.ShowDialog();

                if (result == DialogResult.OK)
                {
                    // Rifresko grid-in vetëm nëse aktualisht është zgjedhur "Whitelisted IPs"
                    if (logsComboBox.SelectedItem?.ToString() == "Whitelisted IPs")
                    {
                        LoadLogsData();
                    }

                    MessageBox.Show("IP u shtua me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        // Butoni Fshije IP të bardhë (vetëm kur është zgjedhur "Whitelisted IPs")
        private void FshijeButton_Click(object sender, EventArgs e)
        {
            // Kontrollo nëse aktualisht është në "Whitelisted IPs"
            if (logsComboBox.SelectedItem?.ToString() != "Whitelisted IPs")
            {
                MessageBox.Show("Ju lutem zgjidhni opsionin 'Whitelisted IPs' për të fshirë një IP.",
                                "Kujdes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Kontrollo nëse është zgjedhur një rresht në grid
            if (logsDataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Zgjidhni një IP nga lista për ta fshirë!",
                                "Kujdes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string ipToDelete = logsDataGridView.SelectedRows[0].Cells["IPAddress"].Value?.ToString();

            if (string.IsNullOrEmpty(ipToDelete))
            {
                MessageBox.Show("Nuk mund të lexohet IP-ja e zgjedhur!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult confirm = MessageBox.Show($"Jeni të sigurt që doni të fshini IP-në '{ipToDelete}' nga lista e bardhë?",
                                                   "Konfirmim Fshirje", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string deleteQuery = "DELETE FROM WhitelistedIPs WHERE IPAddress = @IP";
                    using (SqlCommand cmd = new SqlCommand(deleteQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@IP", ipToDelete);
                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            MessageBox.Show($"IP-ja '{ipToDelete}' u fshi me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadLogsData(); // Rifresko grid-in menjëherë
                        }
                        else
                        {
                            MessageBox.Show("IP-ja nuk u gjet për fshirje!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë fshirjes:\n{ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}