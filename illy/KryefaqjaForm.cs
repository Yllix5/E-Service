using System;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace illy
{
    public partial class KryefaqjaForm : Form
    {
        private int userId;
        private string connectionString =
        "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";


        public KryefaqjaForm(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            LoadUserData(); // Ngarko të dhënat e përdoruesit kur forma hapet
        }

        public void LoadUserData() // Ndryshuar nga private në public
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Merr të dhënat e përdoruesit dhe drejtimit
                    string userQuery = @"
                        SELECT Username, DataLindjes, Email, PhoneNumber, EmriDrejtimit, Photo
                        FROM Userat AS u
                        JOIN Drejtimet as d
                        ON u.DrejtimID = d.DrejtimID
                        WHERE UserID = @UserID";
                    using (SqlCommand userCmd = new SqlCommand(userQuery, con))
                    {
                        userCmd.Parameters.AddWithValue("@UserID", userId);
                        using (SqlDataReader reader = userCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Përshëndetje (label3)
                                label3.Text = $"Përshëndetje {reader["Username"].ToString()}";

                                // Emri (label16)
                                label16.Text = reader["Username"].ToString();

                                // Data e Lindjes (label8)
                                if (reader["DataLindjes"] != DBNull.Value)
                                {
                                    DateTime dataLindjes = Convert.ToDateTime(reader["DataLindjes"]);
                                    label8.Text = dataLindjes.ToString("dd-MM-yyyy");
                                }
                                else
                                {
                                    label8.Text = "Nuk është përcaktuar";
                                }

                                // Email (label15)
                                label15.Text = reader["Email"].ToString() ?? "Nuk është përcaktuar";

                                // Profesor në (label12) - Supozojmë se është Drejtimi
                                label12.Text = reader["EmriDrejtimit"].ToString() ?? "Nuk është përcaktuar";

                                // Numri i Telefonit (label10)
                                label10.Text = reader["PhoneNumber"].ToString() ?? "Nuk është përcaktuar";

                                // Fotoja (profilePictureBox)
                                if (reader["Photo"] != DBNull.Value)
                                {
                                    byte[] imageData = (byte[])reader["Photo"];
                                    profilePictureBox.Image = ByteArrayToImage(imageData);
                                }
                                else
                                {
                                    profilePictureBox.Image = null;
                                }
                            }
                            else
                            {
                                MessageBox.Show($"Përdoruesi me ID {userId} nuk u gjet në databazë!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }
                    }

                    // Merr të dhënat e logimeve nga tabela LoginLogs
                    string loginQuery = @"
                        SELECT TOP 2 LoginTime
                        FROM LoginLogs
                        WHERE UserID = @UserID
                        ORDER BY LoginTime DESC";
                    using (SqlCommand loginCmd = new SqlCommand(loginQuery, con))
                    {
                        loginCmd.Parameters.AddWithValue("@UserID", userId);
                        using (SqlDataReader loginReader = loginCmd.ExecuteReader())
                        {
                            // Kontrollo nëse ka të paktën një logim
                            if (loginReader.Read())
                            {
                                // Login time (logimi aktual, i pari në renditje)
                                DateTime loginTime = Convert.ToDateTime(loginReader["LoginTime"]);
                                label20.Text = loginTime.ToString("dd-MM-yyyy HH:mm:ss");

                                // Last login (logimi i fundit para këtij, nëse ekziston)
                                if (loginReader.Read())
                                {
                                    DateTime lastLoginTime = Convert.ToDateTime(loginReader["LoginTime"]);
                                    label19.Text = lastLoginTime.ToString("dd-MM-yyyy HH:mm:ss");
                                }
                                else
                                {
                                    label19.Text = "Ky është logimi juaj i parë";
                                }
                            }
                            else
                            {
                                label19.Text = "Ky është logimi juaj i parë";
                                label20.Text = "Nuk ka të dhëna";
                            }
                        }
                    }

                    // Merr emrin e makinës aktuale
                    label21.Text = Environment.MachineName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ngarkimit të të dhënave: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Image ByteArrayToImage(byte[] byteArrayIn)
        {
            using (MemoryStream ms = new MemoryStream(byteArrayIn))
            {
                return Image.FromStream(ms);
            }
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            NdrroTelefoninProf Nderro = new NdrroTelefoninProf(userId, this); // Kalojmë this (KryefaqjaForm)
            Nderro.ShowDialog(); // Hap formën si dialog modal
            // Heqim LoadUserData() këtu sepse do të thirret nga NdrroTelefoninProf
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            PerkrahjaTeknike PTK = new PerkrahjaTeknike();
            PTK.ShowDialog();
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            PublikoNjoftime publiko = new PublikoNjoftime(userId);
            publiko.ShowDialog();
        }
    }
}