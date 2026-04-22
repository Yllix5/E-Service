using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Drawing;

namespace illy
{
    public partial class caktoProfessor : Form
    {
        private string connectionString =
            "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";

        private int? lendeIDPerditesim = null; // null = caktim i ri, jo null = përditësim

        private bool isDragging = false;
        private Point dragStartPoint;

        public caktoProfessor(int? lendeID = null)
        {
            InitializeComponent();
            this.lendeIDPerditesim = lendeID;

            this.MouseDown += Form2_MouseDown;
            this.MouseMove += Form2_MouseMove;
            this.MouseUp += Form2_MouseUp;

            NgarkoProfesoret();
            NgarkoLendet();

            professoriComboBox.DropDownHeight = 200;
            professoriComboBox.DropDownStyle = ComboBoxStyle.DropDownList;

            lendaComboBox.DropDownHeight = 200;
            lendaComboBox.DropDownStyle = ComboBoxStyle.DropDownList;

            // Nëse është përditësim, shfaq të dhënat ekzistuese
            if (lendeID.HasValue)
            {
                NgarkoTeDhenatPerditesim(lendeID.Value);
                caktoButton.Visible = false;        // fsheh butonin e caktimit të ri
                perditsoButton.Visible = true;      // shfaq butonin e përditësimit
                this.Text = "Përditëso Profesor për Lëndë";
            }
            else
            {
                caktoButton.Visible = true;
                perditsoButton.Visible = false;
                this.Text = "Cakto Profesor për Lëndë";
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

        private void NgarkoProfesoret()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT UserID, Username FROM Userat WHERE RoleID = 2 ORDER BY Username"; // RoleID=2 = Profesor
                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    professoriComboBox.DataSource = dt;
                    professoriComboBox.DisplayMember = "Username";
                    professoriComboBox.ValueMember = "UserID";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gabim gjatë ngarkimit të profesorëve:\n" + ex.Message, "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gabim gjatë ngarkimit të lëndëve:\n" + ex.Message, "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Nëse është përditësim, precakto profesorin dhe lëndën aktuale
        private void NgarkoTeDhenatPerditesim(int lendeID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                        SELECT l.EmriLendes, l.ProfesoriID
                        FROM Lendet l
                        WHERE l.LendeID = @id";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", lendeID);
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                lendaComboBox.SelectedValue = lendeID;
                                lendaComboBox.Enabled = false; // mos lejo ndryshim të lëndës gjatë përditësimit

                                if (!r.IsDBNull(1))
                                {
                                    professoriComboBox.SelectedValue = r.GetInt32(1);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gabim gjatë ngarkimit të të dhënave për përditësim:\n" + ex.Message, "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Cakto profesor të ri për lëndë
        private void caktoButton_Click(object sender, EventArgs e)
        {
            if (professoriComboBox.SelectedValue == null || lendaComboBox.SelectedValue == null)
            {
                MessageBox.Show("Zgjidh një profesor dhe një lëndë!", "Kujdes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int profesoriID = Convert.ToInt32(professoriComboBox.SelectedValue);
            int lendeID = Convert.ToInt32(lendaComboBox.SelectedValue);

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Kontrollo nëse lënda ka tashmë profesor
                    string checkQuery = "SELECT ProfesoriID FROM Lendet WHERE LendeID = @id";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                    {
                        checkCmd.Parameters.AddWithValue("@id", lendeID);
                        object existing = checkCmd.ExecuteScalar();
                        if (existing != DBNull.Value)
                        {
                            if (MessageBox.Show("Kjo lëndë ka tashmë një profesor. Dëshiron ta zëvendësosh?", "Konfirmim", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                                return;
                        }
                    }

                    // Cakto / përditëso profesorin
                    string updateQuery = "UPDATE Lendet SET ProfesoriID = @profID WHERE LendeID = @lendeID";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@profID", profesoriID);
                        cmd.Parameters.AddWithValue("@lendeID", lendeID);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Profesori u caktua me sukses për lëndën!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gabim gjatë caktimit të profesorit:\n" + ex.Message, "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Përditëso profesorin e lëndës
        private void perditsoButton_Click(object sender, EventArgs e)
        {
            if (professoriComboBox.SelectedValue == null || lendaComboBox.SelectedValue == null)
            {
                MessageBox.Show("Zgjidh një profesor!", "Kujdes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int profesoriID = Convert.ToInt32(professoriComboBox.SelectedValue);
            int lendeID = Convert.ToInt32(lendaComboBox.SelectedValue);

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "UPDATE Lendet SET ProfesoriID = @profID WHERE LendeID = @lendeID";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@profID", profesoriID);
                        cmd.Parameters.AddWithValue("@lendeID", lendeID);
                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            MessageBox.Show("Profesori u përditësua me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Asnjë ndryshim nuk u bë – kontrollo ID-në e lëndës.", "Kujdes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gabim gjatë përditësimit:\n" + ex.Message, "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}