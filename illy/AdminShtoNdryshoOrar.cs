using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Drawing;


namespace illy
{
    public partial class AdminShtoNdryshoOrar : Form
    {
        private string connectionString =
            "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";

        private int? orarID = null;

        private bool isDragging = false;
        private Point dragStartPoint;

        public AdminShtoNdryshoOrar(int? id = null)
        {
            InitializeComponent();
            this.orarID = id;

            this.MouseDown += Form2_MouseDown;
            this.MouseMove += Form2_MouseMove;
            this.MouseUp += Form2_MouseUp;


            NgarkoLendet();
            NgarkoGrupet();
            NgarkoDitet();

            if (orarID.HasValue)
            {
                NgarkoTeDhenat(orarID.Value);
                shtoButton.Visible = false;
                perditsoButton.Visible = true;
                this.Text = "Përditëso Orarin";
            }
            else
            {
                shtoButton.Visible = true;
                perditsoButton.Visible = false;
                this.Text = "Shto Orar të Ri";
            }
        }

        private void Form2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStartPoint = new Point(e.X, e.Y);
            }
        }

        private void Form2_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point p = PointToScreen(new Point(e.X, e.Y));
                this.Location = new Point(p.X - dragStartPoint.X, p.Y - dragStartPoint.Y);
            }
        }

        private void Form2_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void NgarkoLendet()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string query = "SELECT LendeID, EmriLendes FROM Lendet ORDER BY EmriLendes";

                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    lendaComboBox.DataSource = dt;
                    lendaComboBox.DisplayMember = "EmriLendes";
                    lendaComboBox.ValueMember = "LendeID";

                    lendaComboBox.DropDownWidth = 300;
                    lendaComboBox.MaxDropDownItems = 10;
                    lendaComboBox.IntegralHeight = false;
                    lendaComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gabim gjatë ngarkimit të lëndëve:\n\n" + ex.Message,
                    "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

                    grupiComboBox.DataSource = dt;
                    grupiComboBox.DisplayMember = "EmriGrupit";
                    grupiComboBox.ValueMember = "GrupID";

                    grupiComboBox.DropDownWidth = 200;
                    grupiComboBox.MaxDropDownItems = 10;
                    grupiComboBox.IntegralHeight = false;
                    grupiComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gabim gjatë ngarkimit të grupeve:\n\n" + ex.Message,
                    "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void NgarkoDitet()
        {
            ditaComboBox.Items.Clear();

            ditaComboBox.Items.AddRange(new[]
            {
                "E Hënë",
                "E Martë",
                "E Mërkurë",
                "E Enjte",
                "E Premte"
            });

            ditaComboBox.DropDownWidth = 150;
            ditaComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void NgarkoTeDhenat(int id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string query = "SELECT * FROM Oraret WHERE OrarID = @id";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", id);

                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                lendaComboBox.SelectedValue = r["LendeID"];
                                grupiComboBox.SelectedValue = r["GrupID"];

                                string dita = r["Dita"].ToString();

                                if (ditaComboBox.Items.Contains(dita))
                                    ditaComboBox.SelectedItem = dita;

                                TimeSpan fillimi = r["KohaFillimit"] as TimeSpan? ?? TimeSpan.Zero;
                                TimeSpan mbarimi = r["KohaMbarimit"] as TimeSpan? ?? TimeSpan.Zero;

                                kohaFillimitTextBox.Text = fillimi.ToString(@"hh\:mm");
                                kohaMbarimitTextBox.Text = mbarimi.ToString(@"hh\:mm");

                                llojiTextBox.Text = r["Lloji"]?.ToString() ?? "";
                                sallaTextBox.Text = r["Salla"]?.ToString() ?? "";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gabim gjatë ngarkimit të orarit:\n\n" + ex.Message,
                    "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidimFushat()
        {
            if (lendaComboBox.SelectedValue == null)
            {
                MessageBox.Show("Ju lutem zgjidhni një lëndë.",
                    "Kujdes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (grupiComboBox.SelectedValue == null)
            {
                MessageBox.Show("Ju lutem zgjidhni një grup.",
                    "Kujdes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (ditaComboBox.SelectedItem == null)
            {
                MessageBox.Show("Ju lutem zgjidhni një ditë.",
                    "Kujdes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(kohaFillimitTextBox.Text) ||
                string.IsNullOrWhiteSpace(kohaMbarimitTextBox.Text))
            {
                MessageBox.Show("Plotësoni orën e fillimit dhe të mbarimit.",
                    "Kujdes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            TimeSpan fillim = TimeSpan.Parse(kohaFillimitTextBox.Text);
            TimeSpan mbarim = TimeSpan.Parse(kohaMbarimitTextBox.Text);

            if (mbarim <= fillim)
            {
                MessageBox.Show("Koha e mbarimit duhet të jetë më e madhe se koha e fillimit.",
                    "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void shtoButton_Click(object sender, EventArgs e)
        {
            if (!ValidimFushat()) return;

            try
            {
                int lendeID = Convert.ToInt32(lendaComboBox.SelectedValue);
                int viti = 0, semestri = 0;

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string q = "SELECT Viti, Semestri FROM Lendet WHERE LendeID = @id";

                    using (SqlCommand cmd = new SqlCommand(q, con))
                    {
                        cmd.Parameters.AddWithValue("@id", lendeID);

                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                viti = Convert.ToInt32(r["Viti"]);
                                semestri = Convert.ToInt32(r["Semestri"]);
                            }
                        }
                    }
                }

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string query = @"INSERT INTO Oraret 
                    (LendeID, GrupID, Dita, KohaFillimit, KohaMbarimit, Lloji, Salla, Viti, Semestri)
                    VALUES (@lende,@grup,@dita,@fillim,@mbarim,@lloj,@salla,@viti,@semestri)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@lende", lendeID);
                        cmd.Parameters.AddWithValue("@grup", grupiComboBox.SelectedValue);
                        cmd.Parameters.AddWithValue("@dita", ditaComboBox.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@fillim", TimeSpan.Parse(kohaFillimitTextBox.Text));
                        cmd.Parameters.AddWithValue("@mbarim", TimeSpan.Parse(kohaMbarimitTextBox.Text));
                        cmd.Parameters.AddWithValue("@lloj", llojiTextBox.Text.Trim());
                        cmd.Parameters.AddWithValue("@salla", sallaTextBox.Text.Trim());
                        cmd.Parameters.AddWithValue("@viti", viti);
                        cmd.Parameters.AddWithValue("@semestri", semestri);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Orari u shtua me sukses!",
                    "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gabim gjatë shtimit të orarit:\n\n" + ex.Message,
                    "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void perditsoButton_Click(object sender, EventArgs e)
        {
            if (!ValidimFushat()) return;

            try
            {
                int lendeID = Convert.ToInt32(lendaComboBox.SelectedValue);
                int viti = 0, semestri = 0;

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string q = "SELECT Viti, Semestri FROM Lendet WHERE LendeID = @id";

                    using (SqlCommand cmd = new SqlCommand(q, con))
                    {
                        cmd.Parameters.AddWithValue("@id", lendeID);

                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                viti = Convert.ToInt32(r["Viti"]);
                                semestri = Convert.ToInt32(r["Semestri"]);
                            }
                        }
                    }
                }

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string query = @"UPDATE Oraret 
                    SET LendeID=@lende, GrupID=@grup, Dita=@dita,
                        KohaFillimit=@fillim, KohaMbarimit=@mbarim,
                        Lloji=@lloj, Salla=@salla, Viti=@viti, Semestri=@semestri
                    WHERE OrarID=@id";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", orarID.Value);
                        cmd.Parameters.AddWithValue("@lende", lendeID);
                        cmd.Parameters.AddWithValue("@grup", grupiComboBox.SelectedValue);
                        cmd.Parameters.AddWithValue("@dita", ditaComboBox.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@fillim", TimeSpan.Parse(kohaFillimitTextBox.Text));
                        cmd.Parameters.AddWithValue("@mbarim", TimeSpan.Parse(kohaMbarimitTextBox.Text));
                        cmd.Parameters.AddWithValue("@lloj", llojiTextBox.Text.Trim());
                        cmd.Parameters.AddWithValue("@salla", sallaTextBox.Text.Trim());
                        cmd.Parameters.AddWithValue("@viti", viti);
                        cmd.Parameters.AddWithValue("@semestri", semestri);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Orari u përditësua me sukses!",
                    "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gabim gjatë përditësimit të orarit:\n\n" + ex.Message,
                    "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}