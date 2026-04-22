using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace illy
{
    public partial class FinancatForm : Form
    {
        private int userId;
        private string connectionString =
        "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";

        public FinancatForm(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            SetupGridView();
            LoadFinancat();
        }

        private void SetupGridView()
        {
            financatGridView.AutoGenerateColumns = true;
            financatGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            financatGridView.MultiSelect = false;
            financatGridView.AllowUserToAddRows = false;
            financatGridView.ReadOnly = true;
        }

        private void LoadFinancat()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string query = @"
                        SELECT Shuma, Pershkrimi, DataPageses
                        FROM Financat
                        WHERE StudentiID = @StudentiID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@StudentiID", userId);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            if (dt.Rows.Count == 0)
                            {
                                MessageBox.Show("Nuk u gjetën financat për këtë përdorues.", "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }

                            financatGridView.DataSource = dt;

                            // Përshtat kolonat
                            financatGridView.Columns["Shuma"].HeaderText = "Shuma (€)";
                            financatGridView.Columns["Pershkrimi"].HeaderText = "Përshkrimi";
                            financatGridView.Columns["DataPageses"].HeaderText = "Data e Pagesës";

                            // Formato datën për të hequr orën
                            financatGridView.Columns["DataPageses"].DefaultCellStyle.Format = "yyyy-MM-dd";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ngarkimit të financave: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            Xhirollogarite xhirollogarite = new Xhirollogarite();
            xhirollogarite.ShowDialog();
        }
    }
}
