using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace illy
{
    public partial class DetyratList : Form
    {
        private int lendeId;
        private int userId;
        private string connectionString =
            "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";

        public DetyratList(int lendeId, int userId)
        {
            InitializeComponent();
            this.lendeId = lendeId;
            this.userId = userId;
            SetupGridView();
            LoadDetyrat();
        }

        private void SetupGridView()
        {
            detyratGridView.Columns.Clear();
            detyratGridView.Columns.Add("Titulli", "Emri i resursit");
            detyratGridView.Columns.Add("Profesori", "Profesori");
            detyratGridView.Columns.Add("Tipi", "Tipi");

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
            detyratGridView.Columns.Add(shihButtonColumn);

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
            detyratGridView.Columns.Add(shkarkoButtonColumn);

            detyratGridView.AutoGenerateColumns = false;
            detyratGridView.RowTemplate.Height = 40;
            detyratGridView.BorderStyle = BorderStyle.None;
            detyratGridView.CellBorderStyle = DataGridViewCellBorderStyle.None;
            detyratGridView.ColumnHeadersHeight = 40;
            detyratGridView.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(147, 112, 219);
            detyratGridView.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            detyratGridView.EnableHeadersVisualStyles = false;
            detyratGridView.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.LightGray;
            detyratGridView.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;

            detyratGridView.CellFormatting += DetyratGridView_CellFormatting;
            detyratGridView.CellMouseEnter += DetyratGridView_CellMouseEnter;
            detyratGridView.CellMouseLeave += DetyratGridView_CellMouseLeave;
        }

        private void DetyratGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (detyratGridView.Columns[e.ColumnIndex].Name == "ShihButton" ||
                detyratGridView.Columns[e.ColumnIndex].Name == "ShkarkoButton")
            {
                if (detyratGridView.Rows[e.RowIndex].Cells[e.ColumnIndex] is DataGridViewButtonCell)
                {
                    e.CellStyle.BackColor = System.Drawing.Color.Red;
                    e.CellStyle.ForeColor = System.Drawing.Color.White;
                    e.CellStyle.Padding = new Padding(0);
                    e.CellStyle.SelectionBackColor = System.Drawing.Color.Red;
                    e.CellStyle.SelectionForeColor = System.Drawing.Color.White;
                }
            }
        }

        private void DetyratGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && (detyratGridView.Columns[e.ColumnIndex].Name == "ShihButton" ||
                                    detyratGridView.Columns[e.ColumnIndex].Name == "ShkarkoButton"))
            {
                detyratGridView.Cursor = Cursors.Hand;
            }
        }

        private void DetyratGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && (detyratGridView.Columns[e.ColumnIndex].Name == "ShihButton" ||
                                    detyratGridView.Columns[e.ColumnIndex].Name == "ShkarkoButton"))
            {
                detyratGridView.Cursor = Cursors.Default;
            }
        }

        private void LoadDetyrat()
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
                            m.Pershkrimi,
                            m.FileName,
                            u.Username AS Profesori,
                            m.Tipi
                        FROM Materialet m
                        INNER JOIN Userat u ON m.ProfesoriID = u.UserID
                        WHERE m.LendeID = @LendeID
                          AND TRIM(UPPER(m.Tipi)) = 'DETYRË'
                        ORDER BY m.DataPostimit DESC";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@LendeID", lendeId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            detyratGridView.Rows.Clear();
                            while (reader.Read())
                            {
                                string titulli = reader["Titulli"].ToString();
                                string profesori = reader["Profesori"].ToString();
                                string tipi = reader["Tipi"].ToString();
                                detyratGridView.Rows.Add(titulli, profesori, tipi);
                            }
                        }
                    }
                }

                if (detyratGridView.Rows.Count == 0)
                {
                    MessageBox.Show("Nuk ka detyra të disponueshme për këtë lëndë.",
                                    "Informacion",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ngarkimit të detyrave: {ex.Message}",
                                "Gabim",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        private void detyratGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
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
                        cmd.Parameters.AddWithValue("@Titulli", detyratGridView.Rows[e.RowIndex].Cells["Titulli"].Value.ToString());
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
                                MessageBox.Show("Detyra nuk u gjet!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë gjetjes së detyrës: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (detyratGridView.Columns[e.ColumnIndex].Name == "ShihButton")
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
                    MessageBox.Show($"Gabim gjatë shikimit të detyrës: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (detyratGridView.Columns[e.ColumnIndex].Name == "ShkarkoButton")
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
                                MessageBox.Show("Detyra u shkarkua me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    MessageBox.Show($"Gabim gjatë shkarkimit të detyrës: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}