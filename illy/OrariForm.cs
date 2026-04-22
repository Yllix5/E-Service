using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.IO;

namespace illy
{
    public partial class OrariForm : Form
    {
        private int userId;
        private string connectionString =
        "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";


        public OrariForm(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            SetupGridView();
            LoadOrari();
        }

        private void SetupGridView()
        {
            OrariGridView.AutoGenerateColumns = true;
            OrariGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            OrariGridView.MultiSelect = false;
            OrariGridView.AllowUserToAddRows = false;
            OrariGridView.ReadOnly = true;

            OrariGridView.RowTemplate.Height = 25;
        }

        private void LoadOrari()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // 1. Merr të dhënat e studentit (DrejtimID, GrupID, Viti, Semestri)
                    string userQuery = @"
                SELECT 
                    u.DrejtimID, 
                    u.GrupID, 
                    u.Viti, 
                    u.Semestri, 
                    d.EmriDrejtimit, 
                    g.EmriGrupit
                FROM Userat u
                LEFT JOIN Drejtimet d ON u.DrejtimID = d.DrejtimID
                LEFT JOIN Grupet g ON u.GrupID = g.GrupID
                WHERE u.UserID = @UserID";

                    int drejtimId = 0, grupId = 0, viti = 0, semestri = 0;
                    string emriDrejtimit = "Nuk është përcaktuar", emriGrupit = "Nuk është përcaktuar";

                    using (SqlCommand userCmd = new SqlCommand(userQuery, con))
                    {
                        userCmd.Parameters.AddWithValue("@UserID", userId);
                        using (SqlDataReader reader = userCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                drejtimId = reader["DrejtimID"] != DBNull.Value ? reader.GetInt32(0) : 0;
                                grupId = reader["GrupID"] != DBNull.Value ? reader.GetInt32(1) : 0;
                                viti = reader["Viti"] != DBNull.Value ? reader.GetInt32(2) : 0;
                                semestri = reader["Semestri"] != DBNull.Value ? reader.GetInt32(3) : 0;
                                emriDrejtimit = reader["EmriDrejtimit"] != DBNull.Value ? reader.GetString(4) : emriDrejtimit;
                                emriGrupit = reader["EmriGrupit"] != DBNull.Value ? reader.GetString(5) : emriGrupit;
                            }
                            else
                            {
                                MessageBox.Show("Përdoruesi nuk u gjet në databazë!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }
                    }

                    // Shfaq në label
                    label4.Text = emriDrejtimit;
                    label5.Text = emriGrupit;

                    // Kontrollo nëse ka të dhëna të nevojshme
                    if (drejtimId == 0 || grupId == 0 || viti == 0 || semestri == 0)
                    {
                        MessageBox.Show("Të dhënat e studentit (Drejtim, Grup, Viti, Semestri) nuk janë të plota!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // 2. Query-ja e korrigjuar: filtro sipas Viti dhe Semestri të studentit
                    string orariQuery = @"
                SELECT
                    o.Dita,
                    ISNULL(o.Salla, 'Nuk është caktuar') AS Salla,
                    CONVERT(VARCHAR(5), o.KohaFillimit, 108) + ' - ' + CONVERT(VARCHAR(5), o.KohaMbarimit, 108) AS Koha,
                    ISNULL(l.EmriLendes, 'Lëndë e paidentifikuar') AS Lenda,
                    ISNULL(u.Username, 'Profesori i paidentifikuar') AS Profesori,
                    o.Lloji AS Tipi
                FROM Oraret o
                LEFT JOIN Lendet l ON o.LendeID = l.LendeID
                LEFT JOIN Userat u ON l.ProfesoriID = u.UserID
                WHERE o.GrupID = @GrupID
                  AND o.Viti = @Viti              -- FILTRIMI I RI
                  AND o.Semestri = @Semestri      -- FILTRIMI I RI
                ORDER BY
                    CASE o.Dita
                        WHEN 'E Hënë' THEN 1
                        WHEN 'E Martë' THEN 2
                        WHEN 'E Mërkurë' THEN 3
                        WHEN 'E Enjte' THEN 4
                        WHEN 'E Premte' THEN 5
                        ELSE 6
                    END,
                    o.KohaFillimit";

                    using (SqlCommand orariCmd = new SqlCommand(orariQuery, con))
                    {
                        orariCmd.Parameters.AddWithValue("@GrupID", grupId);
                        orariCmd.Parameters.AddWithValue("@Viti", viti);       // <-- ky është kyç
                        orariCmd.Parameters.AddWithValue("@Semestri", semestri); // <-- ky është kyç

                        using (SqlDataAdapter adapter = new SqlDataAdapter(orariCmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            if (dt.Rows.Count == 0)
                            {
                                MessageBox.Show($"Nuk u gjet orar për Viti {viti}, Semestri {semestri}, Grupi {emriGrupit}.",
                                                "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }

                            OrariGridView.DataSource = dt;

                            // Përshtat kolonat (siç ke pasur)
                            if (OrariGridView.Columns.Contains("Dita")) OrariGridView.Columns["Dita"].HeaderText = "Dita";
                            if (OrariGridView.Columns.Contains("Salla")) OrariGridView.Columns["Salla"].HeaderText = "Salla";
                            if (OrariGridView.Columns.Contains("Koha")) OrariGridView.Columns["Koha"].HeaderText = "Koha";
                            if (OrariGridView.Columns.Contains("Lenda")) OrariGridView.Columns["Lenda"].HeaderText = "Lënda";
                            if (OrariGridView.Columns.Contains("Profesori")) OrariGridView.Columns["Profesori"].HeaderText = "Profesori";
                            if (OrariGridView.Columns.Contains("Tipi")) OrariGridView.Columns["Tipi"].HeaderText = "Tipi";

                            // Gjerësi kolonash
                            if (OrariGridView.Columns.Contains("Dita")) OrariGridView.Columns["Dita"].Width = 100;
                            if (OrariGridView.Columns.Contains("Salla")) OrariGridView.Columns["Salla"].Width = 150;
                            if (OrariGridView.Columns.Contains("Koha")) OrariGridView.Columns["Koha"].Width = 120;
                            if (OrariGridView.Columns.Contains("Lenda")) OrariGridView.Columns["Lenda"].Width = 220;
                            if (OrariGridView.Columns.Contains("Profesori")) OrariGridView.Columns["Profesori"].Width = 140;
                            if (OrariGridView.Columns.Contains("Tipi")) OrariGridView.Columns["Tipi"].Width = 100;

                            // Formatimi i kohës
                            if (OrariGridView.Columns.Contains("Koha"))
                            {
                                OrariGridView.Columns["Koha"].DefaultCellStyle.Format = "HH:mm";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ngarkimit të orarit:\n{ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Shto metodën për eksportimin e orarit si CSV
        private void EksportoOrarin_Click(object sender, EventArgs e)
        {
            try
            {
                if (OrariGridView.Rows.Count == 0)
                {
                    MessageBox.Show("Nuk ka të dhëna për të eksportuar!", "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    Title = "Ruaj Orarin si CSV",
                    FileName = "Orari.csv"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName))
                    {
                        // Shkruaj header-in
                        sw.WriteLine("Dita,Salla,Koha,Lënda,Profesori,Tipi");

                        // Shkruaj rreshtat
                        foreach (DataGridViewRow row in OrariGridView.Rows)
                        {
                            string[] rowData = new string[]
                            {
                                row.Cells["Dita"].Value?.ToString(),
                                row.Cells["Salla"].Value?.ToString(),
                                row.Cells["Koha"].Value?.ToString(),
                                row.Cells["Lenda"].Value?.ToString(),
                                row.Cells["Profesori"].Value?.ToString(),
                                row.Cells["Tipi"].Value?.ToString()
                            };
                            sw.WriteLine(string.Join(",", rowData));
                        }
                    }

                    MessageBox.Show("Orari u eksportua me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë eksportimit të orarit: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
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