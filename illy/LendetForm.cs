using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace illy
{
    public partial class LendetForm : Form
    {
        private int userId;
        private string connectionString =
        "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";


        public LendetForm(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            SetupGridView();
            LoadLendet();
        }

        private void SetupGridView()
        {
            LëndëtGridView.AutoGenerateColumns = true;
            LëndëtGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            LëndëtGridView.MultiSelect = false;
            LëndëtGridView.AllowUserToAddRows = false;
            LëndëtGridView.ReadOnly = true;
        }

        private void LoadLendet()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Merr DrejtimID, NendrejtimID, Viti dhe Semestri nga tabela Userat
                    string userQuery = "SELECT DrejtimID, NendrejtimID, Viti, Semestri FROM Userat WHERE UserID = @UserID";
                    int drejtimId = 0, nendrejtimId = 0, viti = 1, semestri = 0;

                    using (SqlCommand userCmd = new SqlCommand(userQuery, con))
                    {
                        userCmd.Parameters.AddWithValue("@UserID", userId);
                        using (SqlDataReader reader = userCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                drejtimId = reader["DrejtimID"] != DBNull.Value ? reader.GetInt32(0) : 0;
                                nendrejtimId = reader["NendrejtimID"] != DBNull.Value ? reader.GetInt32(1) : 0;
                                viti = reader["Viti"] != DBNull.Value ? reader.GetInt32(2) : 1;
                                semestri = reader["Semestri"] != DBNull.Value ? reader.GetInt32(3) : 0;
                            }
                            else
                            {
                                MessageBox.Show("Përdoruesi nuk u gjet në databazë!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }
                    }

                    if (drejtimId == 0 || nendrejtimId == 0 || viti == 0 || semestri == 0)
                    {
                        MessageBox.Show("Të dhënat e përdoruesit (Drejtim, Nendrejtim, Viti, Semestri) nuk janë përcaktuar!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Llogarit semestrin maksimal (viti dhe semestri aktual)
                    int semestriMaksimal = (viti - 1) * 2 + semestri; // P.sh., Viti 2, Semestri 3 = (2-1)*2 + 3 = 5

                    // Merr lëndët nga semestri 1 deri në semestrin aktual, bashkë me emailin e profesorit dhe ECTS
                    string lendetQuery = @"
                        SELECT l.EmriLendes, l.Viti, u.Email AS EmailProfesorit, l.Semestri, l.ECTS
                        FROM Lendet l
                        LEFT JOIN Userat u ON l.ProfesoriID = u.UserID
                        WHERE l.DrejtimID = @DrejtimID 
                        AND l.NendrejtimID = @NendrejtimID 
                        AND ((l.Viti - 1) * 2 + l.Semestri) <= @SemestriMaksimal
                        ORDER BY l.Viti, l.Semestri, l.EmriLendes";

                    using (SqlCommand lendetCmd = new SqlCommand(lendetQuery, con))
                    {
                        lendetCmd.Parameters.AddWithValue("@DrejtimID", drejtimId);
                        lendetCmd.Parameters.AddWithValue("@NendrejtimID", nendrejtimId);
                        lendetCmd.Parameters.AddWithValue("@SemestriMaksimal", semestriMaksimal);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(lendetCmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            if (dt.Rows.Count == 0)
                            {
                                MessageBox.Show("Nuk u gjetën lëndë për semestrat e zgjedhur.", "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }

                            LëndëtGridView.DataSource = dt;

                            // Përshtat kolonat
                            LëndëtGridView.Columns["EmriLendes"].HeaderText = "Emri i Lëndës";
                            LëndëtGridView.Columns["Viti"].HeaderText = "Viti";
                            LëndëtGridView.Columns["EmailProfesorit"].HeaderText = "Email i Profesorit";
                            LëndëtGridView.Columns["Semestri"].HeaderText = "Semestri";
                            LëndëtGridView.Columns["ECTS"].HeaderText = "ECTS";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ngarkimit të lëndëve: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            ReferentForm RF = new ReferentForm();
            RF.ShowDialog();
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            PerkrahjaTeknike PTK = new PerkrahjaTeknike();
            PTK.ShowDialog();
        }
    }
}