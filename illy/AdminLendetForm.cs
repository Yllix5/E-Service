using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace illy
{
    public partial class AdminLendetForm : Form
    {
        private string connectionString =
            "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";

        public AdminLendetForm(int userId)
        {
            InitializeComponent();
            NgarkoLendet(); // Load all initially

            // Event for CellClick
            shfaqLendetGridView.CellClick += shfaqLendetGridView_CellClick;
            kerkoTextBox.TextChanged += kerkoTextBox_TextChanged;
        }

        private void NgarkoLendet(string kerkim = "")
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string query = @"
                        SELECT
                            l.LendeID,
                            l.EmriLendes AS Lënda,
                            ISNULL(u.Username, 'Pa profesor') AS Profesori,
                            l.Viti,
                            l.Semestri,
                            l.ECTS
                        FROM Lendet l
                        LEFT JOIN Userat u ON l.ProfesoriID = u.UserID
                        WHERE l.EmriLendes LIKE @kerkim OR ISNULL(u.Username, '') LIKE @kerkim
                        ORDER BY l.EmriLendes";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@kerkim", "%" + kerkim + "%");

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    shfaqLendetGridView.DataSource = dt;

                    if (shfaqLendetGridView.Columns["LendeID"] != null)
                        shfaqLendetGridView.Columns["LendeID"].Visible = false;

                    shfaqLendetGridView.AutoResizeColumns();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gabim gjatë ngarkimit të lëndëve:\n" + ex.Message, "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void kerkoTextBox_TextChanged(object sender, EventArgs e)
        {
            string teksti = kerkoTextBox.Text.Trim();
            NgarkoLendet(teksti);
        }

        private void shfaqLendetGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= shfaqLendetGridView.Rows.Count)
                return;

            DataGridViewRow row = shfaqLendetGridView.Rows[e.RowIndex];

            // Get the emriLendes safely
            string emriLendes = row.Cells["Lënda"].Value?.ToString()?.Trim() ?? "";

            kerkoTextBox.TextChanged -= kerkoTextBox_TextChanged;
            kerkoTextBox.Text = emriLendes;
            kerkoTextBox.SelectionStart = emriLendes.Length;
            kerkoTextBox.SelectionLength = 0;
            kerkoTextBox.TextChanged += kerkoTextBox_TextChanged;

            // We do NOT reload DataGridView here
        }

        private void ShtoButton_Click(object sender, EventArgs e)
        {
            ShtoPerditsoLende forma = new ShtoPerditsoLende(null);
            if (forma.ShowDialog() == DialogResult.OK)
                NgarkoLendet();
        }

        private void PërditsoButton_Click(object sender, EventArgs e)
        {
            if (shfaqLendetGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Zgjidh një lëndë për ta përditësuar!", "Kujdes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int lendeID = Convert.ToInt32(shfaqLendetGridView.SelectedRows[0].Cells["LendeID"].Value);
            ShtoPerditsoLende forma = new ShtoPerditsoLende(lendeID);
            if (forma.ShowDialog() == DialogResult.OK)
                NgarkoLendet();
        }

        private void FshijeButton_Click(object sender, EventArgs e)
        {
            if (shfaqLendetGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Zgjidh një lëndë për ta fshirë!", "Kujdes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int lendeID = Convert.ToInt32(shfaqLendetGridView.SelectedRows[0].Cells["LendeID"].Value);
            string emri = shfaqLendetGridView.SelectedRows[0].Cells["Lënda"].Value.ToString();

            if (MessageBox.Show($"Dëshiron të fshish lëndën '{emri}'?\nKjo veprim nuk mund të zhbëhet!", "Konfirmim", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                return;

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "DELETE FROM Lendet WHERE LendeID = @id";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", lendeID);
                        cmd.ExecuteNonQuery();
                    }
                }
                NgarkoLendet();
                MessageBox.Show("Lënda u fshi me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gabim gjatë fshirjes (mund të ketë varësi në tabela të tjera):\n" + ex.Message, "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void renditButton1_Click(object sender, EventArgs e)
        {
            shfaqLendetGridView.Sort(shfaqLendetGridView.Columns["Lënda"], System.ComponentModel.ListSortDirection.Ascending);
        }

        private void renditButton2_Click(object sender, EventArgs e)
        {
            shfaqLendetGridView.Sort(shfaqLendetGridView.Columns["Lënda"], System.ComponentModel.ListSortDirection.Descending);
        }

        private void caktoProfessorButton_Click(object sender, EventArgs e)
        {
            caktoProfessor cakto = new caktoProfessor(null);
            cakto.ShowDialog();
            NgarkoLendet();
        }

        private void perditsoProfessorButton_Click(object sender, EventArgs e)
        {
            if (shfaqLendetGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Zgjidh një lëndë për të ndryshuar profesorin!", "Kujdes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int lendeID = Convert.ToInt32(shfaqLendetGridView.SelectedRows[0].Cells["LendeID"].Value);
            caktoProfessor perditso = new caktoProfessor(lendeID);
            perditso.ShowDialog();
            NgarkoLendet();
        }
    }
}