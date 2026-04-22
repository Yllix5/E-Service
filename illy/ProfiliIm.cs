using System;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace illy
{
    public partial class ProfiliIm : Form
    {
        private int userId;
        private string connectionString =
        "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";


        public ProfiliIm(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            LoadUserProfile();
        }

        private void LoadUserProfile()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                            SELECT 
                                u.Username,
                                u.Email, 
                                u.ContractNumber, 
                                u.Viti, 
                                u.DataLindjes,  -- Shto fushën DataLindjes
                                u.Photo,
                                d.EmriDrejtimit, 
                                n.EmriNendrejtimit, 
                                u.PhoneNumber,
                                g.EmriGrupit
                            FROM Userat u
                            LEFT JOIN Drejtimet d ON u.DrejtimID = d.DrejtimID
                            LEFT JOIN Nendrejtimet n ON u.NendrejtimID = n.NendrejtimID
                            LEFT JOIN Grupet g ON u.GrupID = g.GrupID
                            WHERE u.UserID = @UserID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Shfaq emrin e studentit (label3)
                                label3.Text = reader["Username"].ToString();
                                label10.Text = reader["Username"].ToString();

                                // Shfaq drejtimin (label4)
                                label4.Text = reader["EmriDrejtimit"].ToString();

                                // Shfaq nëndrejtimin (label5)
                                label5.Text = reader["EmriNendrejtimit"].ToString();

                                // Shfaq numrin e kontratës (label6)
                                label6.Text = reader["ContractNumber"].ToString();

                                // Shfaq grupin (label8)
                                label8.Text = reader["EmriGrupit"].ToString();

                                // Shfaq të dhënat e tjera
                                // Shfaq Datën e Lindjes në label12 (formato si string)
                                if (reader["DataLindjes"] != DBNull.Value)
                                {
                                    DateTime dataLindjes = Convert.ToDateTime(reader["DataLindjes"]);
                                    label12.Text = dataLindjes.ToString("dd/MM/yyyy"); // Formato datën sipas dëshirës
                                }
                                else
                                {
                                    label12.Text = "Nuk është caktuar"; 
                                }

                                label14.Text = reader["ContractNumber"].ToString(); // Numri i Kontratës (i njëjtë me label6)
                                label16.Text = reader["Email"].ToString(); // Email
                                label18.Text = reader["PhoneNumber"].ToString();

                                // Shfaq foton e profilit në profilePicture
                                if (reader["Photo"] != DBNull.Value)
                                {
                                    byte[] imageData = (byte[])reader["Photo"];
                                    profilePicture.Image = ByteArrayToImage(imageData);
                                }
                                else
                                {
                                    
                                }
                            }
                            else
                            {
                                MessageBox.Show("Përdoruesi nuk u gjet!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ngarkimit të profilit: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            ReferentForm FormaRef = new ReferentForm();
            FormaRef.ShowDialog();
        }

        private void profilePicture_Click(object sender, EventArgs e)
        {
            // Mund të shtosh logjikë për klikimin e profilePicture (p.sh., për të ndryshuar foton)
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            PerkrahjaTeknike PTK = new PerkrahjaTeknike();
            PTK.ShowDialog(); // Posta dhe telekomunikacioni i kosoves.
        }
    }
}