using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace illy
{
    public partial class ShtoNoten : Form
    {
        private int userId; // ProfesoriID 
        private string connectionString =
        "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";

        private int drejtimId; // DrejtimID i profesorit
        private int nendrejtimId; // NendrejtimID i profesorit

        public ShtoNoten(int userId)
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
            LoadNotat();
        }

        private void SetupGridView()
        {
            NotaGridView.AutoGenerateColumns = true;
            NotaGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            NotaGridView.MultiSelect = false;
            NotaGridView.AllowUserToAddRows = false;
            NotaGridView.ReadOnly = true;
            NotaGridView.RowTemplate.Height = 25;

            // Shto ngjarjen për përzgjedhjen e rreshtit
            NotaGridView.SelectionChanged += NotaGridView_SelectionChanged;
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
                                MessageBox.Show($"Profesori me ID {userId} nuk u gjet në databazë!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    MessageBox.Show("Nuk u gjetën lëndë për profesorin!", "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    LoadPiket(); // Ngarko pikët për studentin e zgjedhur
                }
                else
                {
                    MessageBox.Show("Nuk u gjetën studentë për këtë lëndë, drejtim, vit dhe semestër!", "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    piketStudentit.Text = "0"; // Vendos 0 nëse nuk ka studentë
                }

                // Shto event handler për ndryshimin e studentit
                zgjidhStudentinComboBox.SelectedIndexChanged += zgjidhStudentinComboBox_SelectedIndexChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ngarkimit të studentëve: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void zgjidhComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadStudentet(); // Rifresko studentët kur ndryshon lënda
        }

        private void zgjidhStudentinComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadPiket(); // Rifresko pikët kur ndryshon studenti
        }

        private void LoadPiket()
        {
            try
            {
                if (zgjidhComboBox.SelectedItem == null || zgjidhStudentinComboBox.SelectedItem == null)
                {
                    piketStudentit.Text = "0";
                    return;
                }

                ComboBoxItem selectedLenda = (ComboBoxItem)zgjidhComboBox.SelectedItem;
                ComboBoxItem selectedStudent = (ComboBoxItem)zgjidhStudentinComboBox.SelectedItem;

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                        SELECT SUM(Piket) AS TotalPiket
                        FROM Rezultatet
                        WHERE LendeID = @LendeID AND StudentiID = @StudentiID";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@LendeID", selectedLenda.Value);
                        cmd.Parameters.AddWithValue("@StudentiID", selectedStudent.Value);
                        object result = cmd.ExecuteScalar();
                        if (result != DBNull.Value && result != null)
                        {
                            piketStudentit.Text = result.ToString();
                        }
                        else
                        {
                            piketStudentit.Text = "0"; // Vendos 0 nëse nuk ka pikë
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ngarkimit të pikëve: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                piketStudentit.Text = "0";
            }
        }

        private void LoadNotat()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                        SELECT 
                            np.NotaID,
                            u.Username AS EmriStudentit,
                            l.EmriLendes AS Lenda,
                            (SELECT SUM(Piket) FROM Rezultatet r WHERE r.LendeID = np.LendeID AND r.StudentiID = np.StudentiID) AS Piket,
                            np.NotaPerfundimtare AS Nota,
                            np.DataLlogaritjes AS Data
                        FROM NotatPerfundimtare np
                        INNER JOIN Userat u ON np.StudentiID = u.UserID
                        INNER JOIN Lendet l ON np.LendeID = l.LendeID
                        WHERE l.ProfesoriID = @ProfesoriID
                        ORDER BY np.DataLlogaritjes DESC";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@ProfesoriID", userId);
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            NotaGridView.DataSource = dt;

                            // Fshih kolonën NotaID
                            if (NotaGridView.Columns.Contains("NotaID"))
                            {
                                NotaGridView.Columns["NotaID"].Visible = false;
                            }

                            // Përshtat kolonat me header-a të shkurtër
                            NotaGridView.Columns["EmriStudentit"].HeaderText = "Student";
                            NotaGridView.Columns["Lenda"].HeaderText = "Lënda";
                            NotaGridView.Columns["Piket"].HeaderText = "Pikët";
                            NotaGridView.Columns["Nota"].HeaderText = "Nota";
                            NotaGridView.Columns["Data"].HeaderText = "Data";

                            // Përshtat gjerësinë e kolonave
                            NotaGridView.Columns["EmriStudentit"].Width = 120;
                            NotaGridView.Columns["Lenda"].Width = 100;
                            NotaGridView.Columns["Piket"].Width = 60;
                            NotaGridView.Columns["Nota"].Width = 60;
                            NotaGridView.Columns["Data"].Width = 140;

                            // Formato datën
                            if (NotaGridView.Columns.Contains("Data"))
                            {
                                NotaGridView.Columns["Data"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";
                            }

                            // Shfaq mesazh nëse nuk ka nota
                            if (NotaGridView.Rows.Count == 0)
                            {
                                MessageBox.Show("Nuk ka nota përfundimtare të regjistruara ende.", "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ngarkimit të notave: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void NotaGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (NotaGridView.SelectedRows.Count > 0)
            {
                DataGridViewRow row = NotaGridView.SelectedRows[0];
                notaTextBox.Text = row.Cells["Nota"].Value?.ToString();
                piketStudentit.Text = row.Cells["Piket"].Value?.ToString() ?? "0"; // Shfaq pikët në label

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

            if (!int.TryParse(notaTextBox.Text, out int nota) || nota < 5 || nota > 10)
            {
                MessageBox.Show("Nota duhet të jetë një numër mes 5 dhe 10!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                        INSERT INTO NotatPerfundimtare (LendeID, StudentiID, NotaPerfundimtare, DataLlogaritjes)
                        VALUES (@LendeID, @StudentiID, @Nota, @DataLlogaritjes)";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        ComboBoxItem selectedLenda = (ComboBoxItem)zgjidhComboBox.SelectedItem;
                        ComboBoxItem selectedStudent = (ComboBoxItem)zgjidhStudentinComboBox.SelectedItem;
                        cmd.Parameters.AddWithValue("@LendeID", selectedLenda.Value);
                        cmd.Parameters.AddWithValue("@StudentiID", selectedStudent.Value);
                        cmd.Parameters.AddWithValue("@Nota", nota);
                        cmd.Parameters.AddWithValue("@DataLlogaritjes", DateTime.Now);

                        cmd.ExecuteNonQuery();
                    }
                }

                ClearFields();
                LoadNotat(); // Rifresko grid-in
                MessageBox.Show("Nota u shtua me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë shtimit të notës: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PerditsoButton_Click(object sender, EventArgs e)
        {
            if (NotaGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Zgjidh një notë për të përditësuar!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (zgjidhComboBox.SelectedItem == null || zgjidhStudentinComboBox.SelectedItem == null)
            {
                MessageBox.Show("Zgjidh lëndën dhe studentin!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(notaTextBox.Text, out int nota) || nota < 5 || nota > 10)
            {
                MessageBox.Show("Nota duhet të jetë një numër mes 5 dhe 10!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                int notaId = Convert.ToInt32(NotaGridView.SelectedRows[0].Cells["NotaID"].Value);
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                        UPDATE NotatPerfundimtare
                        SET LendeID = @LendeID, StudentiID = @StudentiID, NotaPerfundimtare = @Nota
                        WHERE NotaID = @NotaID";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        ComboBoxItem selectedLenda = (ComboBoxItem)zgjidhComboBox.SelectedItem;
                        ComboBoxItem selectedStudent = (ComboBoxItem)zgjidhStudentinComboBox.SelectedItem;
                        cmd.Parameters.AddWithValue("@LendeID", selectedLenda.Value);
                        cmd.Parameters.AddWithValue("@StudentiID", selectedStudent.Value);
                        cmd.Parameters.AddWithValue("@Nota", nota);
                        cmd.Parameters.AddWithValue("@NotaID", notaId);

                        cmd.ExecuteNonQuery();
                    }
                }

                ClearFields();
                LoadNotat(); // Rifresko grid-in
                MessageBox.Show("Nota u përditësua me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë përditësimit të notës: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FshijeButton_Click(object sender, EventArgs e)
        {
            if (NotaGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Zgjidh një notë për të fshirë!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult result = MessageBox.Show("Jeni të sigurt që doni të fshini këtë notë?", "Konfirmim", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
                return;

            try
            {
                int notaId = Convert.ToInt32(NotaGridView.SelectedRows[0].Cells["NotaID"].Value);
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "DELETE FROM NotatPerfundimtare WHERE NotaID = @NotaID";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@NotaID", notaId);
                        cmd.ExecuteNonQuery();
                    }
                }

                ClearFields();
                LoadNotat(); // Rifresko grid-in
                MessageBox.Show("Nota u fshi me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë fshirjes së notës: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearFields()
        {
            notaTextBox.Clear();
            if (zgjidhComboBox.Items.Count > 0) zgjidhComboBox.SelectedIndex = 0;
            if (zgjidhStudentinComboBox.Items.Count > 0) zgjidhStudentinComboBox.SelectedIndex = 0;
            piketStudentit.Text = "0";
            NotaGridView.ClearSelection();
        }

        public class ComboBoxItem
        {
            public string Text { get; set; }
            public int Value { get; set; }
            public int Viti { get; set; }
            public int Semestri { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            PerkrahjaTeknike PTK = new PerkrahjaTeknike();
            PTK.ShowDialog();
        }
    }
}