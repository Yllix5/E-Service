using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace illy
{
    public partial class VleresoStudentin : Form
    {
        private int userId; // ProfesoriID 
        private string connectionString =
        "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";

        private int drejtimId; // DrejtimID i profesorit
        private int nendrejtimId; // NendrejtimID i profesorit

        public VleresoStudentin(int userId)
        {
            InitializeComponent();
            this.userId = userId;

            zgjidhStudentinComboBox.DropDownHeight = 150;
            zgjidhStudentinComboBox.DropDownStyle = ComboBoxStyle.DropDownList;

            SetupGridView();
            if (!LoadDrejtimId()) // Kontrollo DrejtimID dhe NendrejtimID të profesorit
            {
                this.Close(); // Mbyll formën nëse drejtimi mungon
                return;
            }
            LoadLendet();
            LoadVleresimet();
        }

        private void SetupGridView()
        {
            VleresoGridView.AutoGenerateColumns = true;
            VleresoGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            VleresoGridView.MultiSelect = false;
            VleresoGridView.AllowUserToAddRows = false;
            VleresoGridView.ReadOnly = true;
            VleresoGridView.RowTemplate.Height = 25;

            // Shto ngjarjen për përzgjedhjen e rreshtit
            VleresoGridView.SelectionChanged += VleresoGridView_SelectionChanged;
        }

        private bool LoadDrejtimId()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT DrejtimID, NendrejtimID FROM Userat WHERE UserID = @UserID";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                if (reader["DrejtimID"] != DBNull.Value)
                                {
                                    drejtimId = Convert.ToInt32(reader["DrejtimID"]);
                                }
                                else
                                {
                                    MessageBox.Show("Drejtimi i profesorit nuk është përcaktuar në databazë! Ju lutem kontaktoni administratorin.", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return false;
                                }

                                if (reader["NendrejtimID"] != DBNull.Value)
                                {
                                    nendrejtimId = Convert.ToInt32(reader["NendrejtimID"]);
                                }
                                else
                                {
                                    MessageBox.Show("Nëndrejtimi i profesorit nuk është përcaktuar në databazë! Ju lutem kontaktoni administratorin.", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return false;
                                }

                                return true;
                            }
                            else
                            {
                                MessageBox.Show("Profesori nuk u gjet në databazë!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ngarkimit të drejtimit dhe nëndrejtimit: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void LoadLendet()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                        SELECT LendeID, EmriLendes, Viti, Semestri
                        FROM Lendet
                        WHERE ProfesoriID = @ProfesoriID";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@ProfesoriID", userId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            zgjidhComboBox.Items.Clear();
                            while (reader.Read())
                            {
                                zgjidhComboBox.Items.Add(new ComboBoxItem
                                {
                                    Text = reader["EmriLendes"].ToString(),
                                    Value = reader.GetInt32(0), // LendeID
                                    Viti = reader.GetInt32(2), // Viti
                                    Semestri = reader.GetInt32(3) // Semestri
                                });
                            }
                        }
                    }
                }

                if (zgjidhComboBox.Items.Count > 0)
                {
                    zgjidhComboBox.SelectedIndex = 0; // Zgjidh lëndën e parë si parazgjedhje
                    LoadStudentet(); // Ngarko studentët për lëndën e zgjedhur
                }
                else
                {
                    MessageBox.Show("Nuk u gjetën lëndë për këtë profesor!", "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                // Shto event handler për ndryshimin e përzgjedhjes në ComboBox
                zgjidhComboBox.SelectedIndexChanged += zgjidhComboBox_SelectedIndexChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ngarkimit të lëndëve: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadStudentet()
        {
            try
            {
                if (zgjidhComboBox.SelectedItem == null) return;

                ComboBoxItem selectedLenda = (ComboBoxItem)zgjidhComboBox.SelectedItem;
                int vitiLendes = selectedLenda.Viti;
                int semestriLendes = selectedLenda.Semestri;

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                        SELECT UserID, Username AS EmriStudentit
                        FROM Userat
                        WHERE RoleID = 1
                        AND DrejtimID = @DrejtimID
                        AND NendrejtimID = @NendrejtimID
                        AND Viti = @Viti
                        AND Semestri = @Semestri";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@DrejtimID", drejtimId);
                        cmd.Parameters.AddWithValue("@NendrejtimID", nendrejtimId);
                        cmd.Parameters.AddWithValue("@Viti", vitiLendes);
                        cmd.Parameters.AddWithValue("@Semestri", semestriLendes);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            zgjidhStudentinComboBox.Items.Clear();
                            while (reader.Read())
                            {
                                zgjidhStudentinComboBox.Items.Add(new ComboBoxItem
                                {
                                    Text = reader["EmriStudentit"].ToString(),
                                    Value = reader.GetInt32(0) // UserID
                                });
                            }
                        }
                    }
                }

                if (zgjidhStudentinComboBox.Items.Count > 0)
                {
                    zgjidhStudentinComboBox.SelectedIndex = 0; // Zgjidh studentin e parë si parazgjedhje
                }
                else
                {
                    MessageBox.Show("Nuk u gjetën studentë për këtë lëndë, drejtim, vit dhe semestër!", "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ngarkimit të studentëve: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadVleresimet()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                SELECT 
                    r.RezultatID,
                    u.Username AS EmriStudentit,
                    l.EmriLendes AS Lenda,
                    r.Piket,
                    r.Pershkrimi,
                    r.DataRegjistrimit
                FROM Rezultatet r
                INNER JOIN Userat u ON r.StudentiID = u.UserID
                INNER JOIN Lendet l ON r.LendeID = l.LendeID
                WHERE r.ProfesoriID = @ProfesoriID
                ORDER BY r.DataRegjistrimit DESC";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@ProfesoriID", userId);
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            VleresoGridView.DataSource = dt;

                            // Fshih kolonën RezultatID
                            if (VleresoGridView.Columns.Contains("RezultatID"))
                            {
                                VleresoGridView.Columns["RezultatID"].Visible = false; // Fshih kolonën nga pamja
                            }

                            // Përshtat kolonat me header-a të shkurtër
                            VleresoGridView.Columns["EmriStudentit"].HeaderText = "Student";
                            VleresoGridView.Columns["Lenda"].HeaderText = "Lënda";
                            VleresoGridView.Columns["Piket"].HeaderText = "Pikët";
                            VleresoGridView.Columns["Pershkrimi"].HeaderText = "Përshkrim";
                            VleresoGridView.Columns["DataRegjistrimit"].HeaderText = "Data";

                            // Përshtat gjerësinë e kolonave
                            VleresoGridView.Columns["EmriStudentit"].Width = 120;
                            VleresoGridView.Columns["Lenda"].Width = 100;
                            VleresoGridView.Columns["Piket"].Width = 60;
                            VleresoGridView.Columns["Pershkrimi"].Width = 120;
                            VleresoGridView.Columns["DataRegjistrimit"].Width = 140;

                            // Aktivizo text wrapping për kolonën "Përshkrim"
                            VleresoGridView.Columns["Pershkrimi"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                            VleresoGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

                            // Formato datën
                            if (VleresoGridView.Columns.Contains("DataRegjistrimit"))
                            {
                                VleresoGridView.Columns["DataRegjistrimit"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";
                            }

                            // Shfaq mesazh nëse nuk ka vlerësime
                            if (VleresoGridView.Rows.Count == 0)
                            {
                                MessageBox.Show("Nuk ka vlerësime të regjistruara ende.", "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ngarkimit të vlerësimeve: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void zgjidhComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadStudentet(); // Rifresko studentët kur ndryshon lënda
        }

        private void VleresoGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (VleresoGridView.SelectedRows.Count > 0)
            {
                DataGridViewRow row = VleresoGridView.SelectedRows[0];
                piketTextBox.Text = row.Cells["Piket"].Value?.ToString();
                pershkrimTextBox.Text = row.Cells["Pershkrimi"].Value?.ToString();

                // Zgjidh lëndën në zgjidhComboBox
                string selectedLenda = row.Cells["Lenda"].Value?.ToString();
                foreach (ComboBoxItem item in zgjidhComboBox.Items)
                {
                    if (item.Text == selectedLenda)
                    {
                        zgjidhComboBox.SelectedItem = item;
                        break;
                    }
                }

                // Zgjidh studentin në zgjidhStudentinComboBox
                string selectedStudent = row.Cells["EmriStudentit"].Value?.ToString();
                foreach (ComboBoxItem item in zgjidhStudentinComboBox.Items)
                {
                    if (item.Text == selectedStudent)
                    {
                        zgjidhStudentinComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void ShtoButton_Click(object sender, EventArgs e)
        {
            if (zgjidhComboBox.SelectedItem == null || zgjidhStudentinComboBox.SelectedItem == null)
            {
                MessageBox.Show("Zgjidh lëndën dhe studentin!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(piketTextBox.Text, out int piket) || piket < 0 || piket > 100)
            {
                MessageBox.Show("Pikët duhet të jenë një numër mes 0 dhe 100!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(pershkrimTextBox.Text) || pershkrimTextBox.Text == "Përshkruaj ketu.")
            {
                MessageBox.Show("Shkruaj një përshkrim!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                        INSERT INTO Rezultatet (LendeID, StudentiID, ProfesoriID, Piket, Pershkrimi, DataRegjistrimit)
                        VALUES (@LendeID, @StudentiID, @ProfesoriID, @Piket, @Pershkrimi, @DataRegjistrimit)";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        ComboBoxItem selectedLenda = (ComboBoxItem)zgjidhComboBox.SelectedItem;
                        ComboBoxItem selectedStudent = (ComboBoxItem)zgjidhStudentinComboBox.SelectedItem;
                        cmd.Parameters.AddWithValue("@LendeID", selectedLenda.Value);
                        cmd.Parameters.AddWithValue("@StudentiID", selectedStudent.Value);
                        cmd.Parameters.AddWithValue("@ProfesoriID", userId);
                        cmd.Parameters.AddWithValue("@Piket", piket);
                        cmd.Parameters.AddWithValue("@Pershkrimi", pershkrimTextBox.Text);
                        cmd.Parameters.AddWithValue("@DataRegjistrimit", DateTime.Now);

                        cmd.ExecuteNonQuery();
                    }
                }

                ClearFields();
                LoadVleresimet(); // Rifresko grid-in
                MessageBox.Show("Vlerësimi u shtua me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë shtimit të vlerësimit: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PerditsoButton_Click(object sender, EventArgs e)
        {
            if (VleresoGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Zgjidh një vlerësim për të përditësuar!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (zgjidhComboBox.SelectedItem == null || zgjidhStudentinComboBox.SelectedItem == null)
            {
                MessageBox.Show("Zgjidh lëndën dhe studentin!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(piketTextBox.Text, out int piket) || piket < 0 || piket > 100)
            {
                MessageBox.Show("Pikët duhet të jenë një numër mes 0 dhe 100!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(pershkrimTextBox.Text) || pershkrimTextBox.Text == "Përshkruaj ketu.")
            {
                MessageBox.Show("Shkruaj një përshkrim!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                int rezultatId = Convert.ToInt32(VleresoGridView.SelectedRows[0].Cells["RezultatID"].Value);
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                        UPDATE Rezultatet
                        SET LendeID = @LendeID, StudentiID = @StudentiID, Piket = @Piket, Pershkrimi = @Pershkrimi
                        WHERE RezultatID = @RezultatID";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        ComboBoxItem selectedLenda = (ComboBoxItem)zgjidhComboBox.SelectedItem;
                        ComboBoxItem selectedStudent = (ComboBoxItem)zgjidhStudentinComboBox.SelectedItem;
                        cmd.Parameters.AddWithValue("@LendeID", selectedLenda.Value);
                        cmd.Parameters.AddWithValue("@StudentiID", selectedStudent.Value);
                        cmd.Parameters.AddWithValue("@Piket", piket);
                        cmd.Parameters.AddWithValue("@Pershkrimi", pershkrimTextBox.Text);
                        cmd.Parameters.AddWithValue("@RezultatID", rezultatId);

                        cmd.ExecuteNonQuery();
                    }
                }

                ClearFields();
                LoadVleresimet(); // Rifresko grid-in
                MessageBox.Show("Vlerësimi u përditësua me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë përditësimit të vlerësimit: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FshijeButton_Click(object sender, EventArgs e)
        {
            if (VleresoGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Zgjidh një vlerësim për të fshirë!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult result = MessageBox.Show("Jeni të sigurt që doni të fshini këtë vlerësim?", "Konfirmim", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
                return;

            try
            {
                int rezultatId = Convert.ToInt32(VleresoGridView.SelectedRows[0].Cells["RezultatID"].Value);
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "DELETE FROM Rezultatet WHERE RezultatID = @RezultatID";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@RezultatID", rezultatId);
                        cmd.ExecuteNonQuery();
                    }
                }

                ClearFields();
                LoadVleresimet(); // Rifresko grid-in
                MessageBox.Show("Vlerësimi u fshi me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë fshirjes së vlerësimit: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearFields()
        {
            piketTextBox.Clear();
            pershkrimTextBox.Clear();
            if (zgjidhComboBox.Items.Count > 0) zgjidhComboBox.SelectedIndex = 0;
            if (zgjidhStudentinComboBox.Items.Count > 0) zgjidhStudentinComboBox.SelectedIndex = 0;
            VleresoGridView.ClearSelection();
        }

        public class ComboBoxItem
        {
            public string Text { get; set; }
            public int Value { get; set; }
            public int Viti { get; set; }
            public int Semestri { get; set; }

            public override string ToString()
            {
                return Text; // Kjo siguron që ComboBox të shfaqë vlerën e Text
            }
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            PerkrahjaTeknike PTK = new PerkrahjaTeknike();
            PTK.ShowDialog();
        }
    }
}