using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace illy
{
    public partial class ProvimetProfessor : Form
    {
        private int userId;

        private string connectionString =
        "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";

        public ProvimetProfessor(int userId)
        {
            InitializeComponent();
            this.userId = userId;

            lendaComboBox.DropDownHeight = 200;
            zgjidhStudentinComboBox.DropDownHeight = 200;
            afatiComboBox.DropDownHeight = 200;
            lendaComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            zgjidhStudentinComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            lendaComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            

            dataProvimitPicker.Value = DateTime.Today;
            dataProvimitPicker.MaxDate = DateTime.Today;

            SetupGridView();
            LoadLendet();
            LoadProvimet();
            LoadAfatet();
        }

        private void SetupGridView()
        {
            provimetDataGridView.AutoGenerateColumns = true;
            provimetDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            provimetDataGridView.MultiSelect = false;
            provimetDataGridView.AllowUserToAddRows = false;
            provimetDataGridView.ReadOnly = true;
            provimetDataGridView.RowTemplate.Height = 25;
        }

        // ===============================
        // Load afatet
        // ===============================
        private void LoadAfatet()
        {
            afatiComboBox.Items.Clear();
            afatiComboBox.Items.Add("Afati i janarit");
            afatiComboBox.Items.Add("Afati i prillit");
            afatiComboBox.Items.Add("Afati i qershorit");
            afatiComboBox.Items.Add("Afati i shtatorit");

            // opsionale – zgjidh një default
            if (afatiComboBox.Items.Count > 0)
            {
                afatiComboBox.SelectedIndex = 0;   // Janari shfaqet ne fillim.
            }
        }

        // ===============================
        // LOAD LËNDËT
        // ===============================
        private void LoadLendet()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string query = @"SELECT LendeID, EmriLendes 
                                     FROM Lendet 
                                     WHERE ProfesoriID = @ProfesoriID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@ProfesoriID", userId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            lendaComboBox.Items.Clear();

                            while (reader.Read())
                            {
                                lendaComboBox.Items.Add(new ComboBoxItem
                                {
                                    Text = reader["EmriLendes"].ToString(),
                                    Value = Convert.ToInt32(reader["LendeID"])
                                });
                            }
                        }
                    }
                }

                if (lendaComboBox.Items.Count > 0)
                {
                    lendaComboBox.SelectedIndex = 0;
                    LoadStudentet();
                }
                else
                {
                    MessageBox.Show("Nuk u gjetën lëndë për këtë profesor.",
                        "Informacion",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                lendaComboBox.SelectedIndexChanged += lendaComboBox_SelectedIndexChanged;
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Gabim në databazë gjatë ngarkimit të lëndëve:\n" + ex.Message,
                    "Gabim SQL",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gabim i papritur:\n" + ex.Message,
                    "Gabim",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ===============================
        // LOAD STUDENTËT (PA ATA ME NOTË 10)
        // ===============================
        private void LoadStudentet()
        {
            try
            {
                if (lendaComboBox.SelectedItem == null) return;

                ComboBoxItem selectedLenda = (ComboBoxItem)lendaComboBox.SelectedItem;

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string query = @"
                SELECT DISTINCT 
                    u.UserID, 
                    u.Username
                FROM Userat u
                INNER JOIN Lendet l ON 1=1   -- lidhje fiktive për të marrë atributet e lëndës
                WHERE u.RoleID = 1
                  AND u.Viti = l.Viti
                  AND (u.Semestri = l.Semestri OR l.Semestri IS NULL OR u.Semestri IS NULL)
                  AND u.DrejtimID = l.DrejtimID
                  AND u.NendrejtimID = l.NendrejtimID
                  AND l.LendeID = @LendeID
                  AND NOT EXISTS (
                      SELECT 1 
                      FROM Provimet p 
                      WHERE p.StudentiID = u.UserID 
                        AND p.LendeID = @LendeID 
                        AND p.Nota = 10
                  )
                ORDER BY u.Username";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@LendeID", selectedLenda.Value);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            zgjidhStudentinComboBox.Items.Clear();
                            while (reader.Read())
                            {
                                zgjidhStudentinComboBox.Items.Add(new ComboBoxItem
                                {
                                    Text = reader["Username"].ToString(),
                                    Value = Convert.ToInt32(reader["UserID"])
                                });
                            }
                        }
                    }
                }

                if (zgjidhStudentinComboBox.Items.Count == 0)
                {
                    MessageBox.Show("Nuk u gjetën studentë që përputhen me vitin, semestrin, drejtimin dhe nëndrejtimin e kësaj lënde.",
                        "Nuk ka studentë të përshtatshëm",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else if (zgjidhStudentinComboBox.Items.Count == 1)
                {
                    zgjidhStudentinComboBox.SelectedIndex = 0;
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show("Gabim në databazë gjatë ngarkimit të studentëve:\n" + sqlEx.Message +
                                "\n\nNumri i gabimit: " + sqlEx.Number,
                    "Gabim SQL",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (InvalidCastException castEx)
            {
                MessageBox.Show("Gabim gjatë konvertimit të të dhënave:\n" + castEx.Message,
                    "Gabim konvertimi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gabim i papritur gjatë ngarkimit të studentëve:\n" + ex.Message +
                                "\n\n" + ex.GetType().Name,
                    "Gabim i përgjithshëm",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void lendaComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadStudentet();
        }

        // ===============================
        // LOAD PROVIMET
        // ===============================
        private void LoadProvimet()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string query = @"
                        SELECT p.ProvimID,
                               u.Username AS Studenti,
                               l.EmriLendes AS Lenda,
                               p.DataProvimit,
                               p.Piket,
                               p.Nota,
                               p.Afati
                        FROM Provimet p
                        INNER JOIN Userat u ON p.StudentiID = u.UserID
                        INNER JOIN Lendet l ON p.LendeID = l.LendeID
                        WHERE p.ProfesoriID = @ProfesoriID
                        ORDER BY p.DataProvimit DESC";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@ProfesoriID", userId);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            provimetDataGridView.DataSource = dt;

                            if (provimetDataGridView.Columns.Contains("ProvimID"))
                                provimetDataGridView.Columns["ProvimID"].Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gabim gjatë ngarkimit të provimeve:\n" + ex.Message,
                    "Gabim",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ===============================
        // SHTO PROVIM (ME BLOKIM NOTA 10)
        // ===============================
        private void btnShtoProvimin_Click(object sender, EventArgs e)
        {
            if (lendaComboBox.SelectedItem == null ||
                zgjidhStudentinComboBox.SelectedItem == null)
            {
                MessageBox.Show("Zgjidh lëndën dhe studentin!",
                    "Gabim",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(piketTextBox.Text, out int piket) ||
                !int.TryParse(notaTextBox.Text, out int nota))
            {
                MessageBox.Show("Pikët dhe nota duhet të jenë numra!",
                    "Gabim",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (nota < 5 || nota > 10)
            {
                MessageBox.Show("Nota duhet të jetë nga 5 deri 10.",
                    "Gabim",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                ComboBoxItem selectedLenda = (ComboBoxItem)lendaComboBox.SelectedItem;
                ComboBoxItem selectedStudent = (ComboBoxItem)zgjidhStudentinComboBox.SelectedItem;

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Kontroll nëse ekziston nota 10
                    string checkQuery = @"SELECT COUNT(*) FROM Provimet
                                          WHERE StudentiID=@StudentID
                                          AND LendeID=@LendeID
                                          AND Nota=10";

                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                    {
                        checkCmd.Parameters.AddWithValue("@StudentID", selectedStudent.Value);
                        checkCmd.Parameters.AddWithValue("@LendeID", selectedLenda.Value);

                        int exists = (int)checkCmd.ExecuteScalar();

                        if (exists > 0)
                        {
                            MessageBox.Show("Ky student e ka tashmë notën 10 në këtë lëndë!",
                                "Ndalohet",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Stop);
                            return;
                        }
                    }

                    string insertQuery = @"INSERT INTO Provimet
                        (LendeID, StudentiID, ProfesoriID, DataProvimit, Piket, Afati, Nota)
                        VALUES
                        (@LendeID, @StudentID, @ProfesorID, @DataProvimit, @Piket, @Afati, @Nota)";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@LendeID", selectedLenda.Value);
                        cmd.Parameters.AddWithValue("@StudentID", selectedStudent.Value);
                        cmd.Parameters.AddWithValue("@ProfesorID", userId);
                        cmd.Parameters.AddWithValue("@DataProvimit", dataProvimitPicker.Value);
                        cmd.Parameters.AddWithValue("@Piket", piket);
                        cmd.Parameters.AddWithValue("@Afati", afatiComboBox.Text);
                        cmd.Parameters.AddWithValue("@Nota", nota);

                        cmd.ExecuteNonQuery();
                    }
                }

                LoadProvimet();
                LoadStudentet();

                MessageBox.Show("Provimi u shtua me sukses!",
                    "Sukses",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Gabim SQL gjatë shtimit:\n" + ex.Message,
                    "Gabim SQL",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gabim gjatë shtimit:\n" + ex.Message,
                    "Gabim",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ===============================
        // FSHIJ PROVIM
        // ===============================
        private void btnFshijProvimin_Click(object sender, EventArgs e)
        {
            if (provimetDataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Zgjidh një provim për fshirje!",
                    "Gabim",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Jeni të sigurt?",
                "Konfirmim",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                int provimId = Convert.ToInt32(
                    provimetDataGridView.SelectedRows[0].Cells["ProvimID"].Value);

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    using (SqlCommand cmd =
                        new SqlCommand("DELETE FROM Provimet WHERE ProvimID=@ID", con))
                    {
                        cmd.Parameters.AddWithValue("@ID", provimId);
                        cmd.ExecuteNonQuery();
                    }
                }

                LoadProvimet();
                LoadStudentet();

                MessageBox.Show("Provimi u fshi me sukses!",
                    "Sukses",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gabim gjatë fshirjes:\n" + ex.Message,
                    "Gabim",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        public class ComboBoxItem
        {
            public string Text { get; set; }
            public int Value { get; set; }
            public override string ToString() => Text;
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            PerkrahjaTeknike PTK = new PerkrahjaTeknike();
            PTK.ShowDialog();
        }
    }
}