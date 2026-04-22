using System;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace illy
{
    public partial class Form3 : Form
    {
        private int userId;
        private string connectionString =
        "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";

        private Form currentForm = null;

        private bool isDragging = false;
        private Point dragStartPoint;

        public Form3(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            LoadUserData();
            this.Load += new EventHandler(Form3_Load);

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

        private void Form3_Load(object sender, EventArgs e)
        {
            BallinaForm ballinaForm = new BallinaForm(userId);
            ShowFormInPanel(ballinaForm);
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
                                usernameLabel.Text = reader["Username"].ToString();
                                if (reader["Photo"] != DBNull.Value)
                                {
                                    byte[] imageData = (byte[])reader["Photo"];
                                    profilePictureBox.Image = ByteArrayToImage(imageData);
                                }
                                else
                                {
                                    profilePictureBox.Image = null;
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
            MainPanel.Controls.Clear();
            MainPanel.Controls.Add(form);
            form.Show();
        }

        private void ballinaPictureBox_Click(object sender, EventArgs e)
        {
            BallinaForm ballinaForm = new BallinaForm(userId);
            ShowFormInPanel(ballinaForm);
        }

        private void profiliImPictureBox_Click(object sender, EventArgs e)
        {
            ProfiliIm profiliImForm = new ProfiliIm(userId);
            ShowFormInPanel(profiliImForm);
        }

        private void financatButton_Click(object sender, EventArgs e)
        {
            FinancatForm Financat = new FinancatForm(userId);
            ShowFormInPanel(Financat);
        }
        private void lendetButton_Click(object sender, EventArgs e)
        {
            LendetForm Lendet = new LendetForm(userId);
            ShowFormInPanel(Lendet);
        }

        private void eProfesorButton_Click(object sender, EventArgs e)
        {
            E_Profesor Profesori = new E_Profesor(userId);
            ShowFormInPanel(Profesori);
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            OrariForm Orari = new OrariForm(userId);
            ShowFormInPanel(Orari);
        }

        private void rezultatetButton_Click(object sender, EventArgs e)
        {
            RezultatetForm rezultatet = new RezultatetForm(userId);
            ShowFormInPanel(rezultatet);
        }

        private void provimetButton_Click(object sender, EventArgs e)
        {
            ProvimetForm provimetForm = new ProvimetForm(userId);
            ShowFormInPanel(provimetForm);
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

        private void gmailPicture_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://mail.google.com/a/universitetiaab.com");
        }

        private void notatButton_Click(object sender, EventArgs e)
        {
            NotatForm notatForm = new NotatForm(userId);
            ShowFormInPanel(notatForm);
        }

        private void guna2ControlBox1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}