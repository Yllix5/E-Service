using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace illy
{
    public partial class Estudenti : Form
    {
        private int userId; // ProfesoriID (p.sh., Ilir Keka me UserID = 2042)
        private string connectionString =
        "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";


        public Estudenti(int userId)
        {
            InitializeComponent();
            this.userId = userId;

            SetupGridView();
            LoadLendet();
            LoadShkarkimet();
        }

        private void SetupGridView()
        {
            eStudentGridView.AutoGenerateColumns = true;
            eStudentGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            eStudentGridView.MultiSelect = false;
            eStudentGridView.AllowUserToAddRows = false;
            eStudentGridView.ReadOnly = true;
            eStudentGridView.RowTemplate.Height = 25;
        }

        private void LoadLendet()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                        SELECT LendeID, EmriLendes
                        FROM Lendet
                        WHERE ProfesoriID = @ProfesoriID";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@ProfesoriID", userId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            renditComboBox.Items.Clear();
                            // Shto opsionin për të gjitha lëndët
                            renditComboBox.Items.Add(new ComboBoxItem
                            {
                                Text = "Të gjitha lëndët",
                                Value = 0
                            });

                            while (reader.Read())
                            {
                                renditComboBox.Items.Add(new ComboBoxItem
                                {
                                    Text = reader["EmriLendes"].ToString(),
                                    Value = reader.GetInt32(0) // LendeID
                                });
                            }
                        }
                    }
                }

                if (renditComboBox.Items.Count > 0)
                {
                    renditComboBox.SelectedIndex = 0; // Zgjidh "Të gjitha lëndët" si parazgjedhje
                }
                else
                {
                    MessageBox.Show("Nuk u gjetën lëndë për këtë profesor!", "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                // Shto event handler për ndryshimin e përzgjedhjes në ComboBox
                renditComboBox.SelectedIndexChanged += renditComboBox_SelectedIndexChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ngarkimit të lëndëve: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadShkarkimet(int? lendeId = null)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                        SELECT 
                            s.ShkarkimID,
                            u.Username AS EmriStudentit,
                            l.EmriLendes AS Lenda,
                            m.Titulli AS TitulliMaterialit,
                            s.DataShkarkimit
                        FROM ShkarkimetMaterialeve s
                        INNER JOIN Materialet m ON s.MaterialID = m.MaterialID
                        INNER JOIN Userat u ON s.StudentiID = u.UserID
                        INNER JOIN Lendet l ON m.LendeID = l.LendeID
                        WHERE m.ProfesoriID = @ProfesoriID
                        AND (@LendeID IS NULL OR m.LendeID = @LendeID)
                        ORDER BY s.DataShkarkimit DESC";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@ProfesoriID", userId);
                        cmd.Parameters.AddWithValue("@LendeID", (object)lendeId ?? DBNull.Value);
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            eStudentGridView.DataSource = dt;

                            // Përshtat kolonat me header-a më të shkurtër
                            eStudentGridView.Columns["ShkarkimID"].HeaderText = "ID";
                            eStudentGridView.Columns["EmriStudentit"].HeaderText = "Student";
                            eStudentGridView.Columns["Lenda"].HeaderText = "Lënda";
                            eStudentGridView.Columns["TitulliMaterialit"].HeaderText = "Material";
                            eStudentGridView.Columns["DataShkarkimit"].HeaderText = "Data";

                            // Përshtat gjerësinë e kolonave
                            eStudentGridView.Columns["ShkarkimID"].Width = 50;
                            eStudentGridView.Columns["EmriStudentit"].Width = 120;
                            eStudentGridView.Columns["Lenda"].Width = 100;
                            eStudentGridView.Columns["TitulliMaterialit"].Width = 100; // Zvogëlo gjerësinë
                            eStudentGridView.Columns["DataShkarkimit"].Width = 140; // Rrit gjerësinë për të shfaqur datën plotësisht

                            // Aktivizo text wrapping për kolonën "Material"
                            eStudentGridView.Columns["TitulliMaterialit"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                            eStudentGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells; // Rregullo lartësinë e rreshtave automatikisht

                            // Formato datën
                            if (eStudentGridView.Columns.Contains("DataShkarkimit"))
                            {
                                eStudentGridView.Columns["DataShkarkimit"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";
                            }

                            // Shfaq mesazh nëse nuk ka shkarkime
                            if (eStudentGridView.Rows.Count == 0)
                            {
                                MessageBox.Show("Asnjë student nuk i ka shkarkuar materialet tuaja ende.", "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ngarkimit të shkarkimeve: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void renditComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxItem selectedLenda = (ComboBoxItem)renditComboBox.SelectedItem;
            if (selectedLenda.Value == 0) // "Të gjitha lëndët"
            {
                LoadShkarkimet(); // Shfaq të gjitha shkarkimet
            }
            else
            {
                LoadShkarkimet(selectedLenda.Value); // Shfaq shkarkimet për lëndën e zgjedhur
            }
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            PerkrahjaTeknike PTK = new PerkrahjaTeknike();
            PTK.ShowDialog();
        }
    }
}