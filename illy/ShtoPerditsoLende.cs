using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Drawing;

namespace illy
{
    public partial class ShtoPerditsoLende : Form
    {
        private string connectionString =
            "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";

        private int? lendeID = null; // null = shtim i ri, jo null = përditësim

        private bool isDragging = false;
        private Point dragStartPoint;

        public ShtoPerditsoLende(int? idLende = null)
        {
            InitializeComponent();
            this.lendeID = idLende;

            this.MouseDown += Form2_MouseDown;
            this.MouseMove += Form2_MouseMove;
            this.MouseUp += Form2_MouseUp;

            NgarkoDrejtimet();
            NgarkoVitetDheSemestrat();

            // Nëse është përditësim, ngarko të dhënat
            if (lendeID.HasValue)
            {
                NgarkoTeDhenat(lendeID.Value);
                shtoButton.Visible = false;
                perditsoButton.Visible = true;
                this.Text = "Përditëso Lëndën";
            }
            else
            {
                shtoButton.Visible = true;
                perditsoButton.Visible = false;
                this.Text = "Shto Lëndë të Re";
            }

            // Event për të ngarkuar nëndrejtimet kur ndryshon drejtimi
            drejtimiComboBox.SelectedIndexChanged += drejtimiComboBox_SelectedIndexChanged;
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

        private void NgarkoDrejtimet()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT DrejtimID, EmriDrejtimit FROM Drejtimet ORDER BY EmriDrejtimit";
                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    drejtimiComboBox.DataSource = dt;
                    drejtimiComboBox.DisplayMember = "EmriDrejtimit";
                    drejtimiComboBox.ValueMember = "DrejtimID";
                    drejtimiComboBox.DropDownWidth = 250; // gjatësi standarde
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gabim gjatë ngarkimit të drejtimit:\n" + ex.Message, "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void NgarkoNendrejtimet(int drejtimID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT NendrejtimID, EmriNendrejtimit FROM Nendrejtimet WHERE DrejtimID = @id ORDER BY EmriNendrejtimit";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", drejtimID);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        nendrejtimiComboBox.DataSource = dt;
                        nendrejtimiComboBox.DisplayMember = "EmriNendrejtimit";
                        nendrejtimiComboBox.ValueMember = "NendrejtimID";
                        nendrejtimiComboBox.DropDownWidth = 250;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gabim gjatë ngarkimit të nëndrejtimit:\n" + ex.Message, "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void drejtimiComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (drejtimiComboBox.SelectedValue != null && int.TryParse(drejtimiComboBox.SelectedValue.ToString(), out int drejtimID))
            {
                NgarkoNendrejtimet(drejtimID);
            }
        }

        private void NgarkoVitetDheSemestrat()
        {
            // Viti: 1,2,3
            vitiComboBox.Items.AddRange(new object[] { 1, 2, 3 });
            vitiComboBox.DropDownWidth = 100;

            // Semestri: 1-6
            semestriComboBox.Items.AddRange(new object[] { 1, 2, 3, 4, 5, 6 });
            semestriComboBox.DropDownWidth = 100;
        }

        private void NgarkoTeDhenat(int id)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                        SELECT 
                            EmriLendes, 
                            DrejtimID, 
                            NendrejtimID, 
                            Viti, 
                            Semestri, 
                            ECTS
                        FROM Lendet 
                        WHERE LendeID = @id";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                textLenda.Text = r["EmriLendes"].ToString();
                                drejtimiComboBox.SelectedValue = r["DrejtimID"];
                                // Nëndrejtimi ngarkohet automatikisht nga event-i
                                nendrejtimiComboBox.SelectedValue = r["NendrejtimID"];
                                vitiComboBox.SelectedItem = r["Viti"];
                                semestriComboBox.SelectedItem = r["Semestri"];
                                ectsTextBox.Text = r["ECTS"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gabim gjatë ngarkimit të të dhënave:\n" + ex.Message, "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void shtoButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textLenda.Text))
            {
                MessageBox.Show("Plotësoni emrin e lëndës!", "Kujdes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (drejtimiComboBox.SelectedValue == null || nendrejtimiComboBox.SelectedValue == null ||
                vitiComboBox.SelectedItem == null || semestriComboBox.SelectedItem == null ||
                string.IsNullOrWhiteSpace(ectsTextBox.Text))
            {
                MessageBox.Show("Plotësoni të gjitha fushat!", "Kujdes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(ectsTextBox.Text, out int ects) || ects <= 0)
            {
                MessageBox.Show("ECTS duhet të jetë numër pozitiv!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                        INSERT INTO Lendet 
                        (EmriLendes, DrejtimID, NendrejtimID, Viti, Semestri, ECTS)
                        VALUES (@emri, @drejtim, @nendrejtim, @viti, @semestri, @ects)";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@emri", textLenda.Text.Trim());
                        cmd.Parameters.AddWithValue("@drejtim", drejtimiComboBox.SelectedValue);
                        cmd.Parameters.AddWithValue("@nendrejtim", nendrejtimiComboBox.SelectedValue);
                        cmd.Parameters.AddWithValue("@viti", vitiComboBox.SelectedItem);
                        cmd.Parameters.AddWithValue("@semestri", semestriComboBox.SelectedItem);
                        cmd.Parameters.AddWithValue("@ects", ects);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Lënda u shtua me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gabim gjatë shtimit të lëndës:\n" + ex.Message, "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void perditsoButton_Click(object sender, EventArgs e)
        {
            if (lendeID == null) return;

            if (string.IsNullOrWhiteSpace(textLenda.Text))
            {
                MessageBox.Show("Plotësoni emrin e lëndës!", "Kujdes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (drejtimiComboBox.SelectedValue == null || nendrejtimiComboBox.SelectedValue == null ||
                vitiComboBox.SelectedItem == null || semestriComboBox.SelectedItem == null ||
                string.IsNullOrWhiteSpace(ectsTextBox.Text))
            {
                MessageBox.Show("Plotësoni të gjitha fushat!", "Kujdes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(ectsTextBox.Text, out int ects) || ects <= 0)
            {
                MessageBox.Show("ECTS duhet të jetë numër pozitiv!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                        UPDATE Lendet 
                        SET EmriLendes = @emri, 
                            DrejtimID = @drejtim, 
                            NendrejtimID = @nendrejtim, 
                            Viti = @viti, 
                            Semestri = @semestri, 
                            ECTS = @ects
                        WHERE LendeID = @id";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@emri", textLenda.Text.Trim());
                        cmd.Parameters.AddWithValue("@drejtim", drejtimiComboBox.SelectedValue);
                        cmd.Parameters.AddWithValue("@nendrejtim", nendrejtimiComboBox.SelectedValue);
                        cmd.Parameters.AddWithValue("@viti", vitiComboBox.SelectedItem);
                        cmd.Parameters.AddWithValue("@semestri", semestriComboBox.SelectedItem);
                        cmd.Parameters.AddWithValue("@ects", ects);
                        cmd.Parameters.AddWithValue("@id", lendeID.Value);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Lënda u përditësua me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gabim gjatë përditësimit të lëndës:\n" + ex.Message, "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}