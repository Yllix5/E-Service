using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace illy
{
    public partial class adminEProfesori : Form
    {
        private string connectionString =
            "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";
        private int userId;

        public adminEProfesori(int userId)
        {
            InitializeComponent();
            this.userId = userId;

            LoadLendet();
            LoadProfesoret();

            selektoComboBox.DropDownHeight = 200;
            selektoComboBox.DropDownStyle = ComboBoxStyle.DropDownList;

            shfaqProfesoretGridView.CellClick += ShfaqProfesoretGridView_CellClick;
            kerkoTextBox.TextChanged += KerkoTextBox_TextChanged;
            selektoComboBox.SelectedIndexChanged += selektoComboBox_SelectedIndexChanged;
        }

        private void LoadLendet()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT LendeID, EmriLendes FROM Lendet ORDER BY EmriLendes";
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, con))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        DataRow allRow = dt.NewRow();
                        allRow["LendeID"] = 0;
                        allRow["EmriLendes"] = "Të gjithë profesorët";
                        dt.Rows.InsertAt(allRow, 0);

                        selektoComboBox.DataSource = dt;
                        selektoComboBox.DisplayMember = "EmriLendes";
                        selektoComboBox.ValueMember = "LendeID";
                    }
                }
                selektoComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ngarkimit të lëndëve: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void selektoComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (selektoComboBox.SelectedValue != null && int.TryParse(selektoComboBox.SelectedValue.ToString(), out int lendeId))
            {
                if (lendeId == 0)
                {
                    LoadProfesoret();
                }
                else
                {
                    try
                    {
                        using (SqlConnection con = new SqlConnection(connectionString))
                        {
                            con.Open();
                            string query = @"
                                SELECT u.Username AS Emri, u.PhoneNumber AS 'Numri Telefonit', u.Email
                                FROM Userat u
                                JOIN Lendet l ON u.UserID = l.ProfesoriID
                                WHERE l.LendeID = @LendeID AND u.RoleID = 2";
                            using (SqlDataAdapter adapter = new SqlDataAdapter(query, con))
                            {
                                adapter.SelectCommand.Parameters.AddWithValue("@LendeID", lendeId);
                                DataTable dt = new DataTable();
                                adapter.Fill(dt);
                                shfaqProfesoretGridView.DataSource = dt;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Gabim gjatë filtrimit: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void LoadProfesoret()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                        SELECT u.Username AS Emri, u.PhoneNumber AS 'Numri Telefonit', u.Email
                        FROM Userat u
                        WHERE u.RoleID = 2";
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, con))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        shfaqProfesoretGridView.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ngarkimit të profesorëve: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Ky është vendi ku e rregullojmë crash-in
        private void ShfaqProfesoretGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Kontroll sigurie: mos crash-o nëse indeksi është invalid ose grid-i është bosh
            if (e.RowIndex < 0 || e.RowIndex >= shfaqProfesoretGridView.Rows.Count)
            {
                return; // Asgjë nuk bëhet nëse rreshti nuk ekziston
            }

            DataGridViewRow row = shfaqProfesoretGridView.Rows[e.RowIndex];

            // Merr emrin në mënyrë të sigurt (nëse qelia është null, mos crash-o)
            string emriProfesorit = row.Cells["Emri"]?.Value?.ToString()?.Trim() ?? "";

            // Shfaq emrin në kerkoTextBox
            kerkoTextBox.Text = emriProfesorit;

            // Vendos kursorin në fund dhe fokuso
            kerkoTextBox.SelectionStart = kerkoTextBox.Text.Length;
            kerkoTextBox.SelectionLength = 0;
            kerkoTextBox.Focus();
        }

        private void KerkoTextBox_TextChanged(object sender, EventArgs e)
        {
            string kerko = kerkoTextBox.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(kerko))
            {
                LoadProfesoret();
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                        SELECT u.Username AS Emri, u.PhoneNumber AS 'Numri Telefonit', u.Email
                        FROM Userat u
                        WHERE u.RoleID = 2 AND LOWER(u.Username) LIKE @kerko";
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, con))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@kerko", "%" + kerko + "%");
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        shfaqProfesoretGridView.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë kërkimit: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShtoButton_Click(object sender, EventArgs e)
        {
            ShtoProfessor shtoForm = new ShtoProfessor();
            shtoForm.ShowDialog();
            LoadProfesoret();
        }

        private void PërditsoButton_Click(object sender, EventArgs e)
        {
            if (shfaqProfesoretGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Zgjidhni një profesor nga tabela për ta përditësuar!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string emri = shfaqProfesoretGridView.SelectedRows[0].Cells["Emri"].Value?.ToString()?.Trim();

            if (string.IsNullOrEmpty(emri))
            {
                MessageBox.Show("Emri i profesorit nuk u gjet!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ShtoProfessor editForm = new ShtoProfessor(emri);
            editForm.ShowDialog();
            LoadProfesoret();
        }

        private void FshijeButton_Click(object sender, EventArgs e)
        {
            if (shfaqProfesoretGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Zgjidhni një profesor nga tabela për ta fshirë!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string emri = shfaqProfesoretGridView.SelectedRows[0].Cells["Emri"].Value?.ToString()?.Trim();

            DialogResult result = MessageBox.Show(
                $"Jeni të sigurt që doni të fshini profesorin '{emri}'?\n\nKjo do të heqë lidhjet e tij me lëndët!",
                "Konfirmim Fshirje",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.No) return;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    string clearLendetQuery = @"
                        UPDATE Lendet 
                        SET ProfesoriID = NULL 
                        WHERE ProfesoriID = (SELECT UserID FROM Userat WHERE Username = @Emri AND RoleID = 2)";
                    using (SqlCommand clearCmd = new SqlCommand(clearLendetQuery, con))
                    {
                        clearCmd.Parameters.AddWithValue("@Emri", emri);
                        clearCmd.ExecuteNonQuery();
                    }

                    string deleteQuery = "DELETE FROM Userat WHERE Username = @Emri AND RoleID = 2";
                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, con))
                    {
                        deleteCmd.Parameters.AddWithValue("@Emri", emri);
                        int rows = deleteCmd.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            MessageBox.Show("Profesor u fshi me sukses! (Lidhjet me lëndët u hoqën)", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadProfesoret();
                            kerkoTextBox.Text = ""; // Pastro kërkimin
                        }
                        else
                        {
                            MessageBox.Show("Profesor nuk u gjet për fshirje!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Gabim gjatë fshirjes: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}