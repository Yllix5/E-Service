using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace illy
{
    public partial class AdminOrari : Form
    {
        private string connectionString =
            "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";

        public AdminOrari(int userId)
        {
            InitializeComponent();

            vitiComboBox.DropDownWidth = 120;
            semestriComboBox.DropDownWidth = 120;
            grupiComboBox.DropDownWidth = 180;

            NgarkoVitet();
            NgarkoGrupet();
            NgarkoOraret();

            vitiComboBox.SelectedIndexChanged += vitiComboBox_SelectedIndexChanged;
            semestriComboBox.SelectedIndexChanged += (s, e) => NgarkoOraret();
            grupiComboBox.SelectedIndexChanged += (s, e) => NgarkoOraret();
        }

        private void NgarkoVitet()
        {
            vitiComboBox.Items.Clear();
            vitiComboBox.Items.AddRange(new object[] { 1, 2, 3 });
            vitiComboBox.SelectedIndex = 0;

            // FIX: Thirr manualisht mbushjen e semestrit për vitin default (1)
            NgarkoSemestrin();
        }

        private void NgarkoSemestrin()
        {
            if (vitiComboBox.SelectedItem == null) return;

            int viti = Convert.ToInt32(vitiComboBox.SelectedItem);
            semestriComboBox.Items.Clear();

            if (viti == 1) semestriComboBox.Items.AddRange(new object[] { 1, 2 });
            else if (viti == 2) semestriComboBox.Items.AddRange(new object[] { 3, 4 });
            else if (viti == 3) semestriComboBox.Items.AddRange(new object[] { 5, 6 });

            if (semestriComboBox.Items.Count > 0)
                semestriComboBox.SelectedIndex = 0;
        }

        private void vitiComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            NgarkoSemestrin();
            NgarkoOraret();
        }

        private void NgarkoGrupet()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT GrupID, EmriGrupit FROM Grupet ORDER BY EmriGrupit";
                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    DataRow all = dt.NewRow();
                    all["GrupID"] = 0;
                    all["EmriGrupit"] = "Të gjitha grupet";
                    dt.Rows.InsertAt(all, 0);

                    grupiComboBox.DataSource = dt;
                    grupiComboBox.DisplayMember = "EmriGrupit";
                    grupiComboBox.ValueMember = "GrupID";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gabim gjatë ngarkimit të grupeve:\n" + ex.Message, "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void NgarkoOraret()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    int? viti = vitiComboBox.SelectedItem as int?;
                    int? semestri = semestriComboBox.SelectedItem as int?;
                    int grup = grupiComboBox.SelectedValue is int g ? g : 0;

                    string sql = @"
                        SELECT 
                            o.OrarID,
                            l.EmriLendes AS Lënda,
                            g.EmriGrupit AS Grupi,
                            o.Dita,
                            CONVERT(varchar(5), o.KohaFillimit, 108) AS 'Ora Fillimi',
                            CONVERT(varchar(5), o.KohaMbarimit, 108) AS 'Ora Mbarimi',
                            o.Lloji,
                            o.Salla,
                            o.Viti,
                            o.Semestri
                        FROM Oraret o
                        LEFT JOIN Lendet l ON o.LendeID = l.LendeID
                        LEFT JOIN Grupet g ON o.GrupID = g.GrupID
                        WHERE 1=1";

                    if (viti.HasValue) sql += " AND o.Viti = @viti";
                    if (semestri.HasValue) sql += " AND o.Semestri = @semestri";
                    if (grup > 0) sql += " AND o.GrupID = @grup";

                    sql += " ORDER BY o.Viti, o.Semestri, o.Dita, o.KohaFillimit";

                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        if (viti.HasValue) cmd.Parameters.AddWithValue("@viti", viti.Value);
                        if (semestri.HasValue) cmd.Parameters.AddWithValue("@semestri", semestri.Value);
                        if (grup > 0) cmd.Parameters.AddWithValue("@grup", grup);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        shfaqOrarinGridView.DataSource = dt;

                        if (shfaqOrarinGridView.Columns["OrarID"] != null)
                            shfaqOrarinGridView.Columns["OrarID"].Visible = false;

                        shfaqOrarinGridView.AutoResizeColumns();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gabim gjatë ngarkimit të orareve:\n" + ex.Message, "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShtoButton_Click(object sender, EventArgs e)
        {
            AdminShtoNdryshoOrar forma = new AdminShtoNdryshoOrar(null);
            if (forma.ShowDialog() == DialogResult.OK)
                NgarkoOraret();
        }

        private void PërditsoButton_Click(object sender, EventArgs e)
        {
            if (shfaqOrarinGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Zgjidh një orar!", "Kujdes");
                return;
            }
            int orarID = Convert.ToInt32(shfaqOrarinGridView.SelectedRows[0].Cells["OrarID"].Value);
            AdminShtoNdryshoOrar forma = new AdminShtoNdryshoOrar(orarID);
            if (forma.ShowDialog() == DialogResult.OK)
                NgarkoOraret();
        }

        private void FshijeButton_Click(object sender, EventArgs e)
        {
            if (shfaqOrarinGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Zgjidh një orar!", "Kujdes");
                return;
            }

            string lenda = shfaqOrarinGridView.SelectedRows[0].Cells["Lënda"].Value?.ToString() ?? "Lëndë e panjohur";
            string dita = shfaqOrarinGridView.SelectedRows[0].Cells["Dita"].Value?.ToString() ?? "";
            string oraFillim = shfaqOrarinGridView.SelectedRows[0].Cells["Ora Fillimi"].Value?.ToString() ?? "";
            string oraMbarim = shfaqOrarinGridView.SelectedRows[0].Cells["Ora Mbarimi"].Value?.ToString() ?? "";

            string mesazhi = $"Dëshiron të fshish këtë orar?\n\n" +
                             $"Lënda: {lenda}\n" +
                             $"Dita: {dita}\n" +
                             $"Ora: {oraFillim} - {oraMbarim}";

            if (MessageBox.Show(mesazhi, "Konfirmim Fshirje",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            int orarID = Convert.ToInt32(shfaqOrarinGridView.SelectedRows[0].Cells["OrarID"].Value);

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "DELETE FROM Oraret WHERE OrarID = @id";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", orarID);
                        cmd.ExecuteNonQuery();
                    }
                }
                NgarkoOraret();
                MessageBox.Show("Orari u fshi me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gabim gjatë fshirjes:\n" + ex.Message, "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}