using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace illy
{
    public partial class ShtoMaterialForm : Form
    {
        private int userId;
        private string connectionString =
        "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";

        private byte[] selectedFileData; // Për të ruajtur skedarin e zgjedhur
        private string selectedFileName; // Për të ruajtur emrin e skedarit
        private int drejtimId; // Për të ruajtur DrejtimID e profesorit
        private int nendrejtimId; // Për të ruajtur NendrejtimID e profesorit

        public ShtoMaterialForm(int userId)
        {
            InitializeComponent();
            this.userId = userId;

            // Konfiguro ComboBox për tipin
            tipiComboBox.Items.AddRange(new string[] { "Ligjerata", "Ushtrime", "Detyrë" });
            tipiComboBox.SelectedIndex = 0; // Zgjidh "Ligjerata" si parazgjedhje

            SetupGridView();
            if (!LoadDrejtimId()) // Kontrollo nëse DrejtimID dhe NendrejtimID u ngarkuan me sukses
            {
                this.DialogResult = DialogResult.Cancel;
                return;
            }
            LoadLendet();
            LoadMaterialet();
        }

        private void SetupGridView()
        {
            ShtoMaterialGridView.AutoGenerateColumns = true;
            ShtoMaterialGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            ShtoMaterialGridView.MultiSelect = false;
            ShtoMaterialGridView.AllowUserToAddRows = false;
            ShtoMaterialGridView.ReadOnly = true;
            ShtoMaterialGridView.RowTemplate.Height = 25;

            // Shto ngjarjen për përzgjedhjen e rreshtit
            ShtoMaterialGridView.SelectionChanged += ShtoMaterialGridView_SelectionChanged;
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
                                // Kontrollo DrejtimID
                                if (reader["DrejtimID"] != DBNull.Value)
                                {
                                    drejtimId = Convert.ToInt32(reader["DrejtimID"]);
                                }
                                else
                                {
                                    MessageBox.Show("Drejtimi i profesorit nuk është përcaktuar në databazë! Ju lutem kontaktoni administratorin.", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return false;
                                }

                                // Kontrollo NendrejtimID
                                if (reader["NendrejtimID"] != DBNull.Value)
                                {
                                    nendrejtimId = Convert.ToInt32(reader["NendrejtimID"]);
                                }
                                else
                                {
                                    MessageBox.Show("Nëndrejtimi i profesorit nuk është përcaktuar në databazë! Ju lutem kontaktoni administratorin.", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return false;
                                }

                                return true; // DrejtimID dhe NendrejtimID u ngarkuan me sukses
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
                MessageBox.Show($"Gabim gjatë ngarkimit të drejtimit dhe nëndrejtimit të profesorit: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        SELECT LendeID, EmriLendes, Viti
                        FROM Lendet
                        WHERE ProfesoriID = @ProfesoriID";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@ProfesoriID", userId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            merrLëndët.Items.Clear(); // Pastro ComboBox-in para se të ngarkosh
                            while (reader.Read())
                            {
                                // Shto lëndët në ComboBox bashkë me Viti
                                merrLëndët.Items.Add(new ComboBoxItem
                                {
                                    Text = reader["EmriLendes"].ToString(),
                                    Value = reader.GetInt32(0), // LendeID
                                    Viti = reader.GetInt32(2) // Viti
                                });
                            }
                        }
                    }
                }

                if (merrLëndët.Items.Count > 0)
                {
                    merrLëndët.SelectedIndex = 0; // Zgjidh lëndën e parë si parazgjedhje
                }
                else
                {
                    MessageBox.Show("Nuk u gjetën lëndë për këtë profesor!", "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ngarkimit të lëndëve: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadMaterialet()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                        SELECT 
                            m.MaterialID,
                            l.EmriLendes AS Lenda,
                            nd.EmriNendrejtimit AS Nendrejtimi,
                            m.Titulli,
                            m.Pershkrimi,
                            m.FileName,
                            m.DataPostimit,
                            m.Tipi
                        FROM Materialet m
                        INNER JOIN Lendet l ON m.LendeID = l.LendeID
                        INNER JOIN Nendrejtimet nd ON m.NendrejtimID = nd.NendrejtimID
                        WHERE m.ProfesoriID = @ProfesoriID
                        ORDER BY m.DataPostimit DESC";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@ProfesoriID", userId);
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            ShtoMaterialGridView.DataSource = dt;

                            // Përshtat kolonat
                            ShtoMaterialGridView.Columns["MaterialID"].HeaderText = "ID";
                            ShtoMaterialGridView.Columns["Lenda"].HeaderText = "Lënda";
                            ShtoMaterialGridView.Columns["Nendrejtimi"].HeaderText = "Nëndrejtimi";
                            ShtoMaterialGridView.Columns["Titulli"].HeaderText = "Titulli";
                            ShtoMaterialGridView.Columns["Pershkrimi"].HeaderText = "Përshkrimi";
                            ShtoMaterialGridView.Columns["FileName"].HeaderText = "Emri i Skedarit";
                            ShtoMaterialGridView.Columns["DataPostimit"].HeaderText = "Data e Postimit";
                            ShtoMaterialGridView.Columns["Tipi"].HeaderText = "Tipi";

                            // Përshtat gjerësinë e kolonave
                            ShtoMaterialGridView.Columns["MaterialID"].Width = 80;
                            ShtoMaterialGridView.Columns["Lenda"].Width = 150;
                            ShtoMaterialGridView.Columns["Nendrejtimi"].Width = 150;
                            ShtoMaterialGridView.Columns["Titulli"].Width = 150;
                            ShtoMaterialGridView.Columns["Pershkrimi"].Width = 200;
                            ShtoMaterialGridView.Columns["FileName"].Width = 150;
                            ShtoMaterialGridView.Columns["DataPostimit"].Width = 120;
                            ShtoMaterialGridView.Columns["Tipi"].Width = 100;

                            // Formato datën
                            if (ShtoMaterialGridView.Columns.Contains("DataPostimit"))
                            {
                                ShtoMaterialGridView.Columns["DataPostimit"].DefaultCellStyle.Format = "yyyy-MM-dd";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ngarkimit të materialeve: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShtoButton_Click(object sender, EventArgs e)
        {
            if (merrLëndët.SelectedItem == null)
            {
                MessageBox.Show("Zgjidh një lëndë!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(titullTextBox.Text))
            {
                MessageBox.Show("Shkruaj një titull!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(shtoPermbajtje.Text))
            {
                MessageBox.Show("Shkruaj një përshkrim!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Hap dialogun për zgjedhjen e skedarit
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "PDF files (*.pdf)|*.pdf|Word files (*.doc;*.docx)|*.doc;*.docx|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedFileData = File.ReadAllBytes(openFileDialog.FileName);
                    selectedFileName = Path.GetFileName(openFileDialog.FileName);
                }
                else
                {
                    return; // Anulo nëse nuk zgjidhet skedar
                }
            }

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                        INSERT INTO Materialet (LendeID, ProfesoriID, DrejtimID, NendrejtimID, Viti, Titulli, Pershkrimi, FileData, FileName, DataPostimit, Tipi)
                        VALUES (@LendeID, @ProfesoriID, @DrejtimID, @NendrejtimID, @Viti, @Titulli, @Pershkrimi, @FileData, @FileName, @DataPostimit, @Tipi)";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        ComboBoxItem selectedLenda = (ComboBoxItem)merrLëndët.SelectedItem;
                        cmd.Parameters.AddWithValue("@LendeID", selectedLenda.Value);
                        cmd.Parameters.AddWithValue("@ProfesoriID", userId);
                        cmd.Parameters.AddWithValue("@DrejtimID", drejtimId); // Drejtimi i profesorit
                        cmd.Parameters.AddWithValue("@NendrejtimID", nendrejtimId); // NendrejtimID i profesorit
                        cmd.Parameters.AddWithValue("@Viti", selectedLenda.Viti); // Viti nga lënda
                        cmd.Parameters.AddWithValue("@Titulli", titullTextBox.Text);
                        cmd.Parameters.AddWithValue("@Pershkrimi", shtoPermbajtje.Text);
                        cmd.Parameters.AddWithValue("@FileData", selectedFileData);
                        cmd.Parameters.AddWithValue("@FileName", selectedFileName);
                        cmd.Parameters.AddWithValue("@DataPostimit", DateTime.Now);
                        cmd.Parameters.AddWithValue("@Tipi", tipiComboBox.SelectedItem.ToString()); // Shto Tipi

                        cmd.ExecuteNonQuery();
                    }
                }

                // Pastro fushat dhe rifresko gridin
                ClearFields();
                LoadMaterialet();
                MessageBox.Show("Materiali u shtua me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë shtimit të materialit: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PërditsoButton_Click(object sender, EventArgs e)
        {
            if (ShtoMaterialGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Zgjidh një material për të përditësuar!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (merrLëndët.SelectedItem == null)
            {
                MessageBox.Show("Zgjidh një lëndë!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(titullTextBox.Text))
            {
                MessageBox.Show("Shkruaj një titull!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(shtoPermbajtje.Text))
            {
                MessageBox.Show("Shkruaj një përshkrim!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                int materialId = Convert.ToInt32(ShtoMaterialGridView.SelectedRows[0].Cells["MaterialID"].Value);
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                        UPDATE Materialet
                        SET LendeID = @LendeID, DrejtimID = @DrejtimID, NendrejtimID = @NendrejtimID, Viti = @Viti, Titulli = @Titulli, Pershkrimi = @Pershkrimi, Tipi = @Tipi
                        WHERE MaterialID = @MaterialID";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        ComboBoxItem selectedLenda = (ComboBoxItem)merrLëndët.SelectedItem;
                        cmd.Parameters.AddWithValue("@LendeID", selectedLenda.Value);
                        cmd.Parameters.AddWithValue("@DrejtimID", drejtimId);
                        cmd.Parameters.AddWithValue("@NendrejtimID", nendrejtimId);
                        cmd.Parameters.AddWithValue("@Viti", selectedLenda.Viti);
                        cmd.Parameters.AddWithValue("@Titulli", titullTextBox.Text);
                        cmd.Parameters.AddWithValue("@Pershkrimi", shtoPermbajtje.Text);
                        cmd.Parameters.AddWithValue("@Tipi", tipiComboBox.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@MaterialID", materialId);

                        cmd.ExecuteNonQuery();
                    }

                    // Përditëso skedarin nëse është zgjedhur një i ri
                    if (selectedFileData != null && !string.IsNullOrEmpty(selectedFileName))
                    {
                        string updateFileQuery = @"
                            UPDATE Materialet
                            SET FileData = @FileData, FileName = @FileName
                            WHERE MaterialID = @MaterialID";
                        using (SqlCommand cmd = new SqlCommand(updateFileQuery, con))
                        {
                            cmd.Parameters.AddWithValue("@FileData", selectedFileData);
                            cmd.Parameters.AddWithValue("@FileName", selectedFileName);
                            cmd.Parameters.AddWithValue("@MaterialID", materialId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                // Pastro fushat dhe rifresko gridin
                ClearFields();
                LoadMaterialet();
                MessageBox.Show("Materiali u përditësua me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë përditësimit të materialit: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FshijeButton_Click(object sender, EventArgs e)
        {
            if (ShtoMaterialGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Zgjidh një material për të fshirë!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult result = MessageBox.Show("Jeni të sigurt që doni të fshini këtë material?", "Konfirmim", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
                return;

            try
            {
                int materialId = Convert.ToInt32(ShtoMaterialGridView.SelectedRows[0].Cells["MaterialID"].Value);
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "DELETE FROM Materialet WHERE MaterialID = @MaterialID";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@MaterialID", materialId);
                        cmd.ExecuteNonQuery();
                    }
                }

                // Pastro fushat dhe rifresko gridin
                ClearFields();
                LoadMaterialet();
                MessageBox.Show("Materiali u fshi me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë fshirjes së materialit: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShtoMaterialGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (ShtoMaterialGridView.SelectedRows.Count > 0)
            {
                DataGridViewRow row = ShtoMaterialGridView.SelectedRows[0];
                titullTextBox.Text = row.Cells["Titulli"].Value?.ToString();
                shtoPermbajtje.Text = row.Cells["Pershkrimi"].Value?.ToString();

                // Zgjidh lëndën në ComboBox
                string selectedLenda = row.Cells["Lenda"].Value?.ToString();
                foreach (ComboBoxItem item in merrLëndët.Items)
                {
                    if (item.Text == selectedLenda)
                    {
                        merrLëndët.SelectedItem = item;
                        break;
                    }
                }

                // Zgjidh tipin në ComboBox
                string selectedTipi = row.Cells["Tipi"].Value?.ToString();
                tipiComboBox.SelectedItem = selectedTipi;
            }
        }

        private void ClearFields()
        {
            titullTextBox.Clear();
            shtoPermbajtje.Clear();
            selectedFileData = null;
            selectedFileName = null;
            tipiComboBox.SelectedIndex = 0;
            if (merrLëndët.Items.Count > 0) merrLëndët.SelectedIndex = 0;
            ShtoMaterialGridView.ClearSelection();
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            PerkrahjaTeknike PTK = new PerkrahjaTeknike();
            PTK.ShowDialog();
        }
    }

    // Klasë ndihmëse për ComboBox me Viti
    public class ComboBoxItem
    {
        public string Text { get; set; }
        public int Value { get; set; }
        public int Viti { get; set; } // Shto Viti

        public override string ToString()
        {
            return Text;
        }
    }
}