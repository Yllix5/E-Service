using System;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace illy
{
    public partial class ProfessorDashboard : Form
    {
        private int userId;
        private string connectionString =
        "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";

        private Form currentForm = null; // Variabël për të mbajtur gjurmët e formës aktuale

        private bool isDragging = false;
        private Point dragStartPoint;

        public ProfessorDashboard(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            LoadUserData();
            this.Load += new EventHandler(ProfessorDashboard_Load); // Ngarko formën fillestare kur hapet

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

        private void ProfessorDashboard_Load(object sender, EventArgs e)
        {
            // Shfaq KryefaqjaForm si formën fillestare kur hapet ProfessorDashboard
            KryefaqjaForm kryefaqjaForm = new KryefaqjaForm(userId);
            ShowFormInPanel(kryefaqjaForm);
        }

        private void LoadUserData()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    string query = "SELECT Username, Photo FROM Userat WHERE UserID = @userId";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Shfaq emrin e përdoruesit
                                usernameLabel.Text = reader["Username"].ToString();

                                // Shfaq foton vetëm nëse ka një foto në databazë
                                if (reader["Photo"] != DBNull.Value)
                                {
                                    byte[] imageData = (byte[])reader["Photo"];
                                    profilePictureBox.Image = ByteArrayToImage(imageData);
                                }
                                else
                                {
                                    profilePictureBox.Image = null; // Nuk shfaq foto nëse nuk ka
                                }
                            }
                            else
                            {
                                MessageBox.Show("Përdoruesi nuk u gjet!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Gabim gjatë ngarkimit të të dhënave: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private Image ByteArrayToImage(byte[] byteArrayIn)
        {
            using (MemoryStream ms = new MemoryStream(byteArrayIn))
            {
                return Image.FromStream(ms);
            }
        }

        // Metoda për të shfaqur format brenda professorPanel
        private void ShowFormInPanel(Form form)
        {
            if (currentForm != null)
            {
                currentForm.Close();
                currentForm.Dispose();
            }

            currentForm = form;
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            professorPanel.Controls.Clear();
            professorPanel.Controls.Add(form);
            form.Show();
        }

        // Ngjarja për butonin "Kryefaqja"
        private void kryefaqjaButton_Click(object sender, EventArgs e)
        {
            KryefaqjaForm kryefaqjaForm = new KryefaqjaForm(userId);
            ShowFormInPanel(kryefaqjaForm);
        }

        // Ngjarja për butonin "Shto material"
        private void shtoMaterialButton_Click(object sender, EventArgs e)
        {
            ShtoMaterialForm shtoMaterialForm = new ShtoMaterialForm(userId);
            ShowFormInPanel(shtoMaterialForm);
        }

        private void eStudenti_Click(object sender, EventArgs e)
        {
            Estudenti estudenti = new Estudenti(userId);
            ShowFormInPanel(estudenti);
        }

        private void vleresimetButton_Click(object sender, EventArgs e)
        {
            VleresoStudentin VS = new VleresoStudentin(userId);
            ShowFormInPanel(VS);
        }

        private void ShtoNotenButton_Click(object sender, EventArgs e)
        {
            ShtoNoten Shto = new ShtoNoten(userId);
            ShowFormInPanel(Shto);
        }

        private void gmailPicture_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://mail.google.com/a/universitetiaab.com");
        }

        private void logoutPicture_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Dëshiron të dalesh nga llogaria?",
                "Logout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                NotifyHelper.ClearCurrentForm();
                Form1 form1 = new Form1();
                form1.Show();
                this.Close();
            }
        }

        private void provimetButton_Click(object sender, EventArgs e)
        {
            ProvimetProfessor PP = new ProvimetProfessor(userId);
            ShowFormInPanel(PP);
        }

        private void guna2ControlBox1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}