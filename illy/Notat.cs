using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.IO;

namespace illy
{
    public partial class NotatForm : Form
    {
        private int userId;

        private string connectionString =
        "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";

        public NotatForm(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            SetupGridView();
            LoadNotat();
        }

        private void SetupGridView()
        {
            notatGridView.AutoGenerateColumns = true;
            notatGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            notatGridView.MultiSelect = false;
            notatGridView.AllowUserToAddRows = false;
            notatGridView.ReadOnly = true;
            notatGridView.RowTemplate.Height = 25;
        }

        private void LoadNotat()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Kontrollo nëse studenti ekziston
                    string checkUserQuery = "SELECT Username FROM Userat WHERE UserID = @UserID";

                    using (SqlCommand checkCmd = new SqlCommand(checkUserQuery, con))
                    {
                        checkCmd.Parameters.AddWithValue("@UserID", userId);

                        object result = checkCmd.ExecuteScalar();

                        if (result == null)
                        {
                            MessageBox.Show("Studenti nuk u gjet në databazë!",
                                            "Gabim",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Error);
                            return;
                        }
    
                    }

                    // Merr notat e studentit
                    string notaQuery = @"
                        SELECT 
                            l.EmriLendes AS Lenda,
                            ISNULL(u.Username, 'Nuk është caktuar') AS Profesori,
                            n.NotaPerfundimtare AS Nota,
                            CONVERT(VARCHAR(10), n.DataLlogaritjes, 104) AS Data
                        FROM NotatPerfundimtare n
                        INNER JOIN Lendet l ON n.LendeID = l.LendeID
                        LEFT JOIN Userat u ON l.ProfesoriID = u.UserID
                        WHERE n.StudentiID = @StudentiID
                        ORDER BY n.DataLlogaritjes DESC";

                    using (SqlCommand notaCmd = new SqlCommand(notaQuery, con))
                    {
                        notaCmd.Parameters.AddWithValue("@StudentiID", userId);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(notaCmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            if (dt.Rows.Count == 0)
                            {
                                MessageBox.Show("Nuk ka nota të regjistruara për këtë student.",
                                                "Informacion",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Information);
                            }

                            notatGridView.DataSource = dt;

                            // Përshtat kolonat
                            if (notatGridView.Columns.Contains("Lenda"))
                                notatGridView.Columns["Lenda"].HeaderText = "Lënda";

                            if (notatGridView.Columns.Contains("Profesori"))
                                notatGridView.Columns["Profesori"].HeaderText = "Profesori";

                            if (notatGridView.Columns.Contains("Nota"))
                                notatGridView.Columns["Nota"].HeaderText = "Nota Përfundimtare";

                            if (notatGridView.Columns.Contains("Data"))
                                notatGridView.Columns["Data"].HeaderText = "Data e Regjistrimit";

                            // Gjerësia e kolonave
                            notatGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Gabim në databazë: {ex.Message}",
                                "Gabim SQL",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ngarkimit të notave: {ex.Message}",
                                "Gabim",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        // Export Notat si CSV
        private void EksportoNotat_Click(object sender, EventArgs e)
        {
            try
            {
                if (notatGridView.Rows.Count == 0)
                {
                    MessageBox.Show("Nuk ka të dhëna për të eksportuar!",
                                    "Informacion",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    Title = "Ruaj Notat si CSV",
                    FileName = "Notat.csv"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName))
                    {
                        // Header
                        sw.WriteLine("Lënda,Profesori,Nota,Data");

                        foreach (DataGridViewRow row in notatGridView.Rows)
                        {
                            string[] rowData = new string[]
                            {
                                row.Cells["Lenda"].Value?.ToString(),
                                row.Cells["Profesori"].Value?.ToString(),
                                row.Cells["Nota"].Value?.ToString(),
                                row.Cells["Data"].Value?.ToString()
                            };

                            sw.WriteLine(string.Join(",", rowData));
                        }
                    }

                    MessageBox.Show("Notat u eksportuan me sukses!",
                                    "Sukses",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë eksportimit: {ex.Message}",
                                "Gabim",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            ReferentForm rf = new ReferentForm();
            rf.ShowDialog();
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            PerkrahjaTeknike ptk = new PerkrahjaTeknike();
            ptk.ShowDialog();
        }
    }
}