using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace illy
{
    public partial class MaterialetListForm : Form
    {
        private int lendeId;
        private int userId;
        private bool isDetyra;
        private string connectionString =
        "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";


        public MaterialetListForm(int lendeId, int userId, bool isDetyra)
        {
            InitializeComponent();
            this.lendeId = lendeId;
            this.userId = userId;
            this.isDetyra = isDetyra;
            SetupGridView();
            LoadMaterialet();
        }

        private void SetupGridView()
        {
            // Pastro kolonat ekzistuese
            materialetGridView.Columns.Clear();

            // Shto kolonat e personalizuara
            materialetGridView.Columns.Add("Titulli", "Emri i resursit");
            materialetGridView.Columns.Add("Profesori", "Profesori");
            materialetGridView.Columns.Add("Tipi", "Tipi");

            // Shto kolonën me buton për "Shih"
            DataGridViewButtonColumn shihButtonColumn = new DataGridViewButtonColumn
            {
                Name = "ShihButton",
                HeaderText = "Shih",
                Text = "Shih",
                UseColumnTextForButtonValue = true,
                FlatStyle = FlatStyle.Flat
            };
            shihButtonColumn.DefaultCellStyle.BackColor = System.Drawing.Color.Red;
            shihButtonColumn.DefaultCellStyle.ForeColor = System.Drawing.Color.White;
            shihButtonColumn.DefaultCellStyle.Padding = new Padding(0);
            materialetGridView.Columns.Add(shihButtonColumn);

            // Shto kolonën me buton për "Shkarko"
            DataGridViewButtonColumn shkarkoButtonColumn = new DataGridViewButtonColumn
            {
                Name = "ShkarkoButton",
                HeaderText = "Shkarko",
                Text = "Shkarko",
                UseColumnTextForButtonValue = true,
                FlatStyle = FlatStyle.Flat
            };
            shkarkoButtonColumn.DefaultCellStyle.BackColor = System.Drawing.Color.Red;
            shkarkoButtonColumn.DefaultCellStyle.ForeColor = System.Drawing.Color.White;
            shkarkoButtonColumn.DefaultCellStyle.Padding = new Padding(0);
            materialetGridView.Columns.Add(shkarkoButtonColumn);

            // Konfiguro stilin e DataGridView
            materialetGridView.AutoGenerateColumns = false;
            materialetGridView.RowTemplate.Height = 40;
            materialetGridView.BorderStyle = BorderStyle.None;
            materialetGridView.CellBorderStyle = DataGridViewCellBorderStyle.None;
            materialetGridView.ColumnHeadersHeight = 40;
            materialetGridView.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(147, 112, 219);
            materialetGridView.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            materialetGridView.EnableHeadersVisualStyles = false;
            materialetGridView.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.LightGray;
            materialetGridView.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;

            // Shto event-in CellFormatting për të siguruar stilin e butonave
            materialetGridView.CellFormatting += MaterialetGridView_CellFormatting;

            // Shto event-in CellMouseEnter për të ndryshuar kursorin në Hand
            materialetGridView.CellMouseEnter += MaterialetGridView_CellMouseEnter;
            // Shto event-in CellMouseLeave për të kthyer kursorin në Default
            materialetGridView.CellMouseLeave += MaterialetGridView_CellMouseLeave;
        }

        private void MaterialetGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (materialetGridView.Columns[e.ColumnIndex].Name == "ShihButton" ||
                materialetGridView.Columns[e.ColumnIndex].Name == "ShkarkoButton")
            {
                if (materialetGridView.Rows[e.RowIndex].Cells[e.ColumnIndex] is DataGridViewButtonCell)
                {
                    e.CellStyle.BackColor = System.Drawing.Color.Red;
                    e.CellStyle.ForeColor = System.Drawing.Color.White;
                    e.CellStyle.Padding = new Padding(0);
                    e.CellStyle.SelectionBackColor = System.Drawing.Color.Red;
                    e.CellStyle.SelectionForeColor = System.Drawing.Color.White;
                }
            }
        }

        private void MaterialetGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            // Ndrysho kursorin në Hand kur kalon mbi butonat "Shih" ose "Shkarko"
            if (e.RowIndex >= 0 && (materialetGridView.Columns[e.ColumnIndex].Name == "ShihButton" ||
                                    materialetGridView.Columns[e.ColumnIndex].Name == "ShkarkoButton"))
            {
                materialetGridView.Cursor = Cursors.Hand;
            }
        }

        private void MaterialetGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            // Kthe kursorin në Default kur largohesh nga qeliza
            if (e.RowIndex >= 0 && (materialetGridView.Columns[e.ColumnIndex].Name == "ShihButton" ||
                                    materialetGridView.Columns[e.ColumnIndex].Name == "ShkarkoButton"))
            {
                materialetGridView.Cursor = Cursors.Default;
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
                     m.Titulli,
                        m.Tipi,
                        m.FileName,
                        u.Username AS Profesori
            FROM Materialet m
            JOIN Userat u ON m.ProfesoriID = u.UserID
            WHERE m.LendeID = @LendeID
            AND m.Tipi IN ('Ligjerata', 'Ushtrime') 
                ORDER BY m.DataPostimit DESC";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@LendeID", lendeId);
                        cmd.Parameters.AddWithValue("@IsDetyra", isDetyra ? 1 : 0);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            materialetGridView.Rows.Clear();

                            while (reader.Read())
                            {
                                string titulli = reader["Titulli"].ToString();
                                string profesori = reader["Profesori"].ToString();
                                string tipi = reader["Tipi"].ToString();

                                materialetGridView.Rows.Add(titulli, profesori, tipi);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ngarkimit të materialeve: {ex.Message}",
                                "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void materialetGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            int materialId;
            string fileName;
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT MaterialID, FileName FROM Materialet WHERE Titulli = @Titulli AND LendeID = @LendeID";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Titulli", materialetGridView.Rows[e.RowIndex].Cells["Titulli"].Value.ToString());
                        cmd.Parameters.AddWithValue("@LendeID", lendeId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                materialId = Convert.ToInt32(reader["MaterialID"]);
                                fileName = reader["FileName"].ToString();
                            }
                            else
                            {
                                MessageBox.Show("Materiali nuk u gjet!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë gjetjes së materialit: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (materialetGridView.Columns[e.ColumnIndex].Name == "ShihButton")
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();
                        string query = "SELECT FileData FROM Materialet WHERE MaterialID = @MaterialID";
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@MaterialID", materialId);
                            byte[] fileData = (byte[])cmd.ExecuteScalar();

                            if (fileData == null || fileData.Length == 0)
                            {
                                MessageBox.Show("Skedari është bosh ose nuk ekziston!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            string tempFilePath = Path.Combine(Path.GetTempPath(), fileName);
                            File.WriteAllBytes(tempFilePath, fileData);
                            System.Diagnostics.Process.Start(tempFilePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Gabim gjatë shikimit të materialit: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (materialetGridView.Columns[e.ColumnIndex].Name == "ShkarkoButton")
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();
                        string query = "SELECT FileData FROM Materialet WHERE MaterialID = @MaterialID";
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@MaterialID", materialId);
                            byte[] fileData = (byte[])cmd.ExecuteScalar();

                            if (fileData == null || fileData.Length == 0)
                            {
                                MessageBox.Show("Skedari është bosh ose nuk ekziston!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            SaveFileDialog saveFileDialog = new SaveFileDialog();
                            saveFileDialog.FileName = fileName;

                            string extension = Path.GetExtension(fileName).ToLower();
                            switch (extension)
                            {
                                case ".pdf":
                                    saveFileDialog.Filter = "PDF Files|*.pdf";
                                    break;
                                case ".doc":
                                case ".docx":
                                    saveFileDialog.Filter = "Word Files|*.doc;*.docx";
                                    break;
                                default:
                                    saveFileDialog.Filter = "All Files|*.*";
                                    break;
                            }

                            if (saveFileDialog.ShowDialog() == DialogResult.OK)
                            {
                                File.WriteAllBytes(saveFileDialog.FileName, fileData);
                                MessageBox.Show("Materiali u shkarkua me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                string insertQuery = "INSERT INTO ShkarkimetMaterialeve (MaterialID, StudentiID, DataShkarkimit) VALUES (@MaterialID, @StudentiID, @DataShkarkimit)";
                                using (SqlCommand insertCmd = new SqlCommand(insertQuery, con))
                                {
                                    insertCmd.Parameters.AddWithValue("@MaterialID", materialId);
                                    insertCmd.Parameters.AddWithValue("@StudentiID", userId);
                                    insertCmd.Parameters.AddWithValue("@DataShkarkimit", DateTime.Now);
                                    insertCmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Gabim gjatë shkarkimit të materialit: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}