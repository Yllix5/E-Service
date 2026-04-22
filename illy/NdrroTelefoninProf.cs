using System;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

namespace illy
{
    public partial class NdrroTelefoninProf : Form
    {
        private int userId;
        private KryefaqjaForm kryefaqjaForm; // Referencë për KryefaqjaForm
        private string connectionString =
        "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";


        private bool isDragging = false;
        private Point dragStartPoint;

        public NdrroTelefoninProf(int userId, KryefaqjaForm kryefaqjaForm)
        {
            InitializeComponent();
            this.userId = userId;
            this.kryefaqjaForm = kryefaqjaForm; // Ruaj referencën e KryefaqjaForm

            this.MouseDown += Form2_MouseDown;
            this.MouseMove += Form2_MouseMove;
            this.MouseUp += Form2_MouseUp;
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

        private void nderrojeButton_Click(object sender, EventArgs e)
        {
            // Merr numrat nga TextBox-et
            string numriIRi = numriTextBox.Text.Trim();
            string perseritNumrin = perseritTextBox.Text.Trim();

            // Valido fushat
            if (string.IsNullOrWhiteSpace(numriIRi) || string.IsNullOrWhiteSpace(perseritNumrin))
            {
                MessageBox.Show("Ju lutem plotësoni të gjitha fushat!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Kontrollo nëse numrat përputhen
            if (numriIRi != perseritNumrin)
            {
                MessageBox.Show("Numrat e telefonit nuk përputhen! Ju lutem sigurohuni që numri i ri dhe përsëritja të jenë të njëjta.", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Valido formatin e numrit të telefonit (opsionale, mund ta personalizosh sipas nevojës)
            if (!IsValidPhoneNumber(numriIRi))
            {
                MessageBox.Show("Numri i telefonit nuk është në formatin e duhur! Ju lutem përdorni një format të vlefshëm (p.sh., +383xxxxxxxx).", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Përditëso numrin e telefonit në databazë
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "UPDATE Userat SET PhoneNumber = @PhoneNumber WHERE UserID = @UserID";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@PhoneNumber", numriIRi);
                        cmd.Parameters.AddWithValue("@UserID", userId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Numri i telefonit u ndryshua me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            kryefaqjaForm.LoadUserData(); // Përditëso të dhënat në KryefaqjaForm
                            this.Close(); // Mbyll formën pas suksesit
                        }
                        else
                        {
                            MessageBox.Show("Përdoruesi nuk u gjet në databazë!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ndryshimit të numrit të telefonit: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            // Valido formatin e numrit të telefonit
            if (phoneNumber.Length < 8 || phoneNumber.Length > 15)
                return false;

            return phoneNumber.StartsWith("+") && phoneNumber.Substring(1).All(char.IsDigit);
        }

        private void NdrroTelefoninProf_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Dispose();
        }
    }
}