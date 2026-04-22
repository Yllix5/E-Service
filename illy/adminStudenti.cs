using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace illy
{
    public partial class adminStudenti : Form
    {
        private string connectionString =
        "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";
        private int userId;

        public adminStudenti(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            LoadStudents();
            shfaqStudentGridView.CellClick += ShfaqStudentGridView_CellClick;
        }

        private void LoadStudents(string filter = "", string orderBy = "Username ASC")
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT u.Username AS 'Emri dhe Mbiemri', u.PhoneNumber AS 'Numri Telefonit', u.Email, u.ContractNumber AS Kontrata, " +
                                   "d.EmriDrejtimit AS Drejtimi, n.EmriNendrejtimit AS Nendrejtimi, g.EmriGrupit AS Grupi, u.DataLindjes AS 'Data e Lindjes' " +
                                   "FROM Userat u " +
                                   "JOIN Drejtimet d ON u.DrejtimID = d.DrejtimID " +
                                   "JOIN Nendrejtimet n ON u.NendrejtimID = n.NendrejtimID " +
                                   "JOIN Grupet g ON u.GrupID = g.GrupID " +
                                   "WHERE u.RoleID = 1"; // RoleID 1 for students
                    if (!string.IsNullOrWhiteSpace(filter))
                    {
                        query += " AND (u.Username LIKE @Filter OR u.Email LIKE @Filter OR u.PhoneNumber LIKE @Filter)";
                    }
                    query += $" ORDER BY {orderBy}";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        if (!string.IsNullOrWhiteSpace(filter))
                        {
                            cmd.Parameters.AddWithValue("@Filter", "%" + filter + "%");
                        }
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            shfaqStudentGridView.DataSource = dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ngarkimit të studentëve: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShfaqStudentGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = shfaqStudentGridView.Rows[e.RowIndex];
                string username = row.Cells["Emri dhe Mbiemri"].Value?.ToString();
                kerkoTextBox.Text = username; // Populate search box with selected student's name
            }
        }

        private void ShtoButton_Click(object sender, EventArgs e)
        {
            shtoStudent shtoForm = new shtoStudent();
            shtoForm.ShowDialog();
            LoadStudents(); // Refresh the grid after adding a student
        }

        private void perditsoButton_Click(object sender, EventArgs e)
        {
            if (shfaqStudentGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Ju lutem zgjidhni një student për ta përditësuar!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                DataGridViewRow selectedRow = shfaqStudentGridView.SelectedRows[0];
                string username = selectedRow.Cells["Emri dhe Mbiemri"].Value?.ToString();
                string phoneNumber = selectedRow.Cells["Numri Telefonit"].Value?.ToString();
                string email = selectedRow.Cells["Email"].Value?.ToString();
                string contract = selectedRow.Cells["Kontrata"].Value?.ToString();
                string drejtimi = selectedRow.Cells["Drejtimi"].Value?.ToString();
                string nendrejtimi = selectedRow.Cells["Nendrejtimi"].Value?.ToString();
                string grupi = selectedRow.Cells["Grupi"].Value?.ToString();

                // Handle DataLindjes with a null check
                DateTime dateOfBirth;
                if (selectedRow.Cells["Data e Lindjes"].Value == null || selectedRow.Cells["Data e Lindjes"].Value == DBNull.Value)
                {
                    dateOfBirth = DateTime.Today.AddYears(-18);
                }
                else
                {
                    dateOfBirth = Convert.ToDateTime(selectedRow.Cells["Data e Lindjes"].Value);
                }

                // Fetch the student's photo and backupCode from the database
                byte[] photo = null;
                string backupCode = "";
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT Photo, BackupCode FROM Userat WHERE Username = @Username AND RoleID = 1";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                if (!reader.IsDBNull(0))
                                {
                                    photo = (byte[])reader["Photo"];
                                }
                                backupCode = reader["BackupCode"].ToString();
                            }
                        }
                    }
                }

                // Open shtoStudent form with the selected student's data
                shtoStudent shtoForm = new shtoStudent(
                    username, phoneNumber, email, contract, drejtimi, nendrejtimi, grupi, dateOfBirth, photo, backupCode);
                shtoForm.ShowDialog();
                LoadStudents(); // Refresh the grid after updating
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë hapjes së formës për përditësim: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void kerkoTextBox_TextChanged(object sender, EventArgs e)
        {
            string filter = kerkoTextBox.Text.Trim();
            LoadStudents(filter);
        }

        private void renditButton1_Click(object sender, EventArgs e)
        {
            LoadStudents(kerkoTextBox.Text.Trim(), "Username ASC"); // A-Z
        }

        private void renditButton2_Click(object sender, EventArgs e)
        {
            LoadStudents(kerkoTextBox.Text.Trim(), "Username DESC"); // Z-A
        }

        private void fshijeButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(kerkoTextBox.Text))
            {
                MessageBox.Show("Ju lutem zgjidhni ose shkruani emrin e studentit për ta fshirë!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult result = MessageBox.Show("A je i sigurt që dëshiron ta fshish?", "Konfirmim Fshirje", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    string query = "DELETE FROM Userat WHERE Username = @Username AND RoleID = 1";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Username", kerkoTextBox.Text.Trim());
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Studenti u fshi me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadStudents();
                            kerkoTextBox.Clear();
                        }
                        else
                        {
                            MessageBox.Show("Nuk u gjet asnjë student për fshirje!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
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