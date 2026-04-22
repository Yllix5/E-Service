using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace illy
{
    public partial class ProvimetForm : Form
    {
        private int userId;
        private string connectionString =
        "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";

        public ProvimetForm(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            SetupGridView();
            SetupFilterComboBox();
            LoadProvimet();
        }

        private void SetupGridView()
        {
            ProvimetGridView.AutoGenerateColumns = true;
            ProvimetGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            ProvimetGridView.MultiSelect = false;
            ProvimetGridView.AllowUserToAddRows = false;
            ProvimetGridView.ReadOnly = true;
            ProvimetGridView.RowTemplate.Height = 25;
        }

        // ==============================
        // FILTER COMBOBOX
        // ==============================
        private void SetupFilterComboBox()
        {
            provimetComboBox.Items.Clear();
            provimetComboBox.Items.Add("Shfaq të gjitha provimet");
            provimetComboBox.Items.Add("Shfaq provimet e refuzuara");
            provimetComboBox.Items.Add("Shfaq provimet e kaluara");
            provimetComboBox.Items.Add("Shfaq provimet e deshtuara");
            provimetComboBox.SelectedIndex = 0;

            provimetComboBox.SelectedIndexChanged += (s, e) => LoadProvimet();
        }

        // ==============================
        // LOAD PROVIMET
        // ==============================
        private void LoadProvimet()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string filter = "";

                    if (provimetComboBox.SelectedIndex == 1)
                        filter = " AND p.Statusi = 'Refuzuar'";
                    else if (provimetComboBox.SelectedIndex == 2)
                        filter = " AND p.Statusi = 'Kaluar'";
                    else if (provimetComboBox.SelectedIndex == 3)
                        filter = "AND p.Statusi = 'Deshtuar'";

                        string provimetQuery = $@"
                        SELECT 
                            p.ProvimID,
                            l.EmriLendes AS Lenda,
                            u.Username AS Profesori,
                            p.DataProvimit,
                            p.Piket,
                            p.Nota,
                            p.Afati,
                            p.Statusi
                        FROM Provimet p
                        INNER JOIN Lendet l ON p.LendeID = l.LendeID
                        LEFT JOIN Userat u ON p.ProfesoriID = u.UserID
                        WHERE p.StudentiID = @StudentiID
                        {filter}
                        ORDER BY p.DataProvimit DESC";

                    using (SqlCommand cmd = new SqlCommand(provimetQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@StudentiID", userId);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            ProvimetGridView.DataSource = dt;

                            if (ProvimetGridView.Columns.Contains("ProvimID"))
                                ProvimetGridView.Columns["ProvimID"].Visible = false;

                            ProvimetGridView.Columns["Lenda"].HeaderText = "Lënda";
                            ProvimetGridView.Columns["Profesori"].HeaderText = "Profesori";
                            ProvimetGridView.Columns["DataProvimit"].HeaderText = "Data";
                            ProvimetGridView.Columns["Piket"].HeaderText = "Pikët";
                            ProvimetGridView.Columns["Nota"].HeaderText = "Nota";
                            ProvimetGridView.Columns["Afati"].HeaderText = "Afati";
                            ProvimetGridView.Columns["Statusi"].HeaderText = "Statusi";

                            ProvimetGridView.Columns["DataProvimit"].DefaultCellStyle.Format = "yyyy-MM-dd";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gabim gjatë ngarkimit:\n" + ex.Message,
                    "Gabim",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ==============================
        // REFUZO PROVIM
        // ==============================
        private void refuzoButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (ProvimetGridView.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Zgjidh një provim për ta refuzuar!",
                        "Gabim",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                var selectedRow = ProvimetGridView.SelectedRows[0];
                if (selectedRow.Cells["ProvimID"].Value == null ||
                    selectedRow.Cells["Nota"].Value == null)
                {
                    MessageBox.Show("Të dhënat e provimit janë të pavlefshme!",
                        "Gabim",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                int provimId = Convert.ToInt32(selectedRow.Cells["ProvimID"].Value);
                int nota = Convert.ToInt32(selectedRow.Cells["Nota"].Value);

                // Vetëm nota 10 nuk mund të refuzohet (kushti yt i vjetër)
                if (nota == 10)
                {
                    MessageBox.Show("Nota 10 nuk mund të refuzohet!",
                        "Ndalohet",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Stop);
                    return;
                }

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Kontrollo nëse provimi është transferuar në NotatPerfundimtare
                    string checkFinaleQuery = @"
                SELECT COUNT(*) 
                FROM NotatPerfundimtare np
                INNER JOIN Provimet p ON np.LendeID = p.LendeID 
                                      AND np.StudentiID = p.StudentiID
                WHERE p.ProvimID = @ProvimID";

                    using (SqlCommand checkCmd = new SqlCommand(checkFinaleQuery, con))
                    {
                        checkCmd.Parameters.AddWithValue("@ProvimID", provimId);
                        int count = (int)checkCmd.ExecuteScalar();

                        if (count > 0)
                        {
                            MessageBox.Show("Ky provim është regjistruar si notë përfundimtare dhe refuzimi nuk lejohet më sipas rregullores së fakultetit.\n\nKontaktoni referentët për më shumë informacion.",
                                "Refuzimi i pamundur",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                            return;
                        }
                    }

                    // Nëse nuk është në finale → vazhdo me konfirmim dhe refuzim
                    if (MessageBox.Show(
                        "A jeni i sigurt që dëshironi ta refuzoni këtë provim?",
                        "Konfirmim",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) != DialogResult.Yes)
                        return;

                    string updateQuery = "UPDATE Provimet SET Statusi='Refuzuar' WHERE ProvimID=@ID";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@ID", provimId);
                        cmd.ExecuteNonQuery();
                    }
                }

                LoadProvimet();
                MessageBox.Show("Provimi u refuzua me sukses!",
                    "Sukses",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (FormatException)
            {
                MessageBox.Show("Gabim në konvertimin e të dhënave!",
                    "Gabim",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Gabim në databazë:\n" + ex.Message,
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

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            ReferentForm RF = new ReferentForm();
            RF.ShowDialog();
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            PerkrahjaTeknike PTK = new PerkrahjaTeknike();
            PTK.ShowDialog();
        }

    }
}