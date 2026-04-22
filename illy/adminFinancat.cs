using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace illy
{
    public partial class adminFinancat : Form
    {
        private string connectionString =
        "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";

        private int selectedFinancID = -1; // Për të ruajtur FinancID e rreshtit të zgjedhur për përditësim/fshirje

        public adminFinancat(int userId)
        {
            InitializeComponent();
            SetupGridView();
            LoadFinancat();
        }

        private void SetupGridView()
        {
            ShfaqFincancatGridView.AutoGenerateColumns = true;
            ShfaqFincancatGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            ShfaqFincancatGridView.MultiSelect = false;
            ShfaqFincancatGridView.AllowUserToAddRows = false;
            ShfaqFincancatGridView.ReadOnly = true;

            // Aktivizo word wrapping për pershkrimin
            ShfaqFincancatGridView.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            ShfaqFincancatGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        }

        private void LoadFinancat()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                        SELECT 
                            f.FinancaID, -- Shto FinancaID në query
                            u.Username AS Emri,
                            f.Shuma,
                            f.Pershkrimi,
                            f.DataPageses
                        FROM Financat f
                        JOIN Userat u ON f.StudentiID = u.UserID
                        ORDER BY f.DataPageses DESC";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable financatTable = new DataTable();
                            adapter.Fill(financatTable);

                            if (financatTable.Rows.Count == 0)
                            {
                                MessageBox.Show("Nuk u gjetën pagesa.", "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }

                            ShfaqFincancatGridView.DataSource = financatTable;

                            // Fshih kolonën FinancaID nga pamja
                            ShfaqFincancatGridView.Columns["FinancaID"].Visible = false;

                            // Përshtat kolonat
                            ShfaqFincancatGridView.Columns["Emri"].HeaderText = "Emri";
                            ShfaqFincancatGridView.Columns["Shuma"].HeaderText = "Shuma";
                            ShfaqFincancatGridView.Columns["Pershkrimi"].HeaderText = "Përshkrimi";
                            ShfaqFincancatGridView.Columns["DataPageses"].HeaderText = "Data e Pagesës";

                            // Përshtat gjerësinë e kolonave
                            ShfaqFincancatGridView.Columns["Emri"].Width = 150;
                            ShfaqFincancatGridView.Columns["Shuma"].Width = 150;
                            ShfaqFincancatGridView.Columns["Pershkrimi"].Width = 150;
                            ShfaqFincancatGridView.Columns["DataPageses"].Width = 150;

                            // Formato datën
                            if (ShfaqFincancatGridView.Columns.Contains("DataPageses"))
                            {
                                ShfaqFincancatGridView.Columns["DataPageses"].DefaultCellStyle.Format = "yyyy-MM-dd";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ngarkimit të financave: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void shtoButton_Click(object sender, EventArgs e)
        {
            string nrKontrates = nrKontratesTextBox.Text.Trim();
            string shumaStr = shumaTextBox.Text.Trim();
            string perseritShumenStr = perseritShumenTextBox.Text.Trim();
            string pershkrimi = "Pagesë për studentin."; // Mund të shtosh një TextBox për përshkrimin nëse dëshiron

            // Valido fushat
            if (string.IsNullOrWhiteSpace(nrKontrates) || string.IsNullOrWhiteSpace(shumaStr) || string.IsNullOrWhiteSpace(perseritShumenStr))
            {
                MessageBox.Show("Ju lutem plotësoni të gjitha fushat!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Valido shumën
            if (!decimal.TryParse(shumaStr, out decimal shuma) || shuma <= 0)
            {
                MessageBox.Show("Shuma e futur nuk është e vlefshme!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (shumaStr != perseritShumenStr)
            {
                MessageBox.Show("Shumat nuk përputhen! Ju lutem sigurohuni që shumat të jenë të njëjta.", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Kontrollo nëse numri i kontratës ekziston
            int studentId = -1;
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT UserID FROM Userat WHERE ContractNumber = @ContractNumber AND RoleID = 1";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@ContractNumber", nrKontrates);
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            studentId = (int)result;
                        }
                        else
                        {
                            MessageBox.Show("Numri i kontratës nuk u gjet ose nuk i përket një studenti!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë verifikimit të numrit të kontratës: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Shto pagesën në databazë
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "INSERT INTO Financat (StudentiID, Shuma, Pershkrimi, DataPageses) " +
                                  "VALUES (@StudentiID, @Shuma, @Pershkrimi, @DataPageses)";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@StudentiID", studentId);
                        cmd.Parameters.AddWithValue("@Shuma", shuma);
                        cmd.Parameters.AddWithValue("@Pershkrimi", pershkrimi);
                        cmd.Parameters.AddWithValue("@DataPageses", DateTime.Now);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Pagesa u shtua me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadFinancat(); // Rifresko grid-in
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë shtimit të pagesës: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void perditsoButton_Click(object sender, EventArgs e)
        {
            if (selectedFinancID == -1)
            {
                MessageBox.Show("Ju lutem zgjidhni një pagesë për të përditësuar!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string shumaStr = shumaTextBox.Text.Trim();
            string perseritShumenStr = perseritShumenTextBox.Text.Trim();

            // Valido fushat
            if (string.IsNullOrWhiteSpace(shumaStr) || string.IsNullOrWhiteSpace(perseritShumenStr))
            {
                MessageBox.Show("Ju lutem plotësoni shumën dhe përsëritjen e shumës!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Valido shumën
            if (!decimal.TryParse(shumaStr, out decimal shuma) || shuma <= 0)
            {
                MessageBox.Show("Shuma e futur nuk është e vlefshme!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (shumaStr != perseritShumenStr)
            {
                MessageBox.Show("Shumat nuk përputhen! Ju lutem sigurohuni që shumat të jenë të njëjta.", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Përditëso pagesën në databazë
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "UPDATE Financat SET Shuma = @Shuma, DataPageses = @DataPageses WHERE FinancaID = @FinancaID"; // Korrigjo FinancID në FinancaID
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Shuma", shuma);
                        cmd.Parameters.AddWithValue("@DataPageses", DateTime.Now);
                        cmd.Parameters.AddWithValue("@FinancaID", selectedFinancID);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Pagesa u përditësua me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadFinancat(); // Rifresko grid-in
                ClearFields();
                selectedFinancID = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë përditësimit të pagesës: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void fshiButton_Click(object sender, EventArgs e)
        {
            if (selectedFinancID == -1)
            {
                MessageBox.Show("Ju lutem zgjidhni një pagesë për të fshirë!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult result = MessageBox.Show("Jeni të sigurt që dëshironi të fshini këtë pagesë?", "Konfirmim", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes)
                return;

            // Fshi pagesën nga databaza
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "DELETE FROM Financat WHERE FinancaID = @FinancaID"; // Korrigjo FinancID në FinancaID
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@FinancaID", selectedFinancID);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Pagesa u fshi me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadFinancat(); // Rifresko grid-in
                ClearFields();
                selectedFinancID = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë fshirjes së pagesës: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShfaqFincancatGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                try
                {
                    // Merr FinancaID direkt nga rreshti i zgjedhur
                    selectedFinancID = Convert.ToInt32(ShfaqFincancatGridView.Rows[e.RowIndex].Cells["FinancaID"].Value);

                    // Plotëso fushat për përditësim
                    shumaTextBox.Text = ShfaqFincancatGridView.Rows[e.RowIndex].Cells["Shuma"].Value.ToString();
                    perseritShumenTextBox.Text = ShfaqFincancatGridView.Rows[e.RowIndex].Cells["Shuma"].Value.ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Gabim gjatë zgjedhjes së pagesës: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearFields()
        {
            nrKontratesTextBox.Text = "";
            shumaTextBox.Text = "";
            perseritShumenTextBox.Text = "";
        }
    }
}