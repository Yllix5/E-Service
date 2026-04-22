using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Drawing;

namespace illy
{
    public partial class PublikoNjoftime : Form
    {
        private int profesoriId;
        private string connectionString =
        "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";


        private bool isDragging = false;
        private Point dragStartPoint;

        public PublikoNjoftime(int profesoriId)
        {
            InitializeComponent();
            this.profesoriId = profesoriId;
            LoadLendet(); // Ngarko lëndët në ComboBox kur forma hapet

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

        private void LoadLendet()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT LendeID, EmriLendes, Viti, Semestri FROM Lendet WHERE ProfesoriID = @ProfesoriID";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@ProfesoriID", profesoriId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            lendaComboBox.Items.Clear();
                            while (reader.Read())
                            {
                                // Ruaj LendeID, Viti dhe Semestri në Tag, dhe shfaq EmriLendes në ComboBox
                                var lenda = new
                                {
                                    LendeID = reader.GetInt32(0),
                                    EmriLendes = reader.GetString(1),
                                    Viti = reader.GetInt32(2),
                                    Semestri = reader.GetInt32(3)
                                };
                                lendaComboBox.Items.Add(lenda);
                                lendaComboBox.DisplayMember = "EmriLendes";
                            }
                        }
                    }
                }

                if (lendaComboBox.Items.Count > 0)
                {
                    lendaComboBox.SelectedIndex = 0; // Zgjidh lëndën e parë automatikisht
                }
                else
                {
                    MessageBox.Show("Nuk keni lëndë të regjistruara!", "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    publikoButton.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ngarkimit të lëndëve: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void publikoButton_Click(object sender, EventArgs e)
        {
            string titulli = titulliTextBox.Text.Trim();
            string permbajtja = permbajtjaTextBox.Text.Trim();

            // Valido fushat
            if (string.IsNullOrWhiteSpace(titulli) || string.IsNullOrWhiteSpace(permbajtja))
            {
                MessageBox.Show("Ju lutem plotësoni të gjitha fushat!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (lendaComboBox.SelectedItem == null)
            {
                MessageBox.Show("Ju lutem zgjidhni një lëndë!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Merr LendeID nga lënda e zgjedhur
            var lenda = (dynamic)lendaComboBox.SelectedItem;
            int lendaId = lenda.LendeID;

            // Ruaj njoftimin në databazë
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "INSERT INTO Njoftimet (ProfesoriID, LendeID, Titulli, Permbajtja, DataPublikimit) " +
                                  "VALUES (@ProfesoriID, @LendeID, @Titulli, @Permbajtja, @DataPublikimit)";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@ProfesoriID", profesoriId);
                        cmd.Parameters.AddWithValue("@LendeID", lendaId);
                        cmd.Parameters.AddWithValue("@Titulli", titulli);
                        cmd.Parameters.AddWithValue("@Permbajtja", permbajtja);
                        cmd.Parameters.AddWithValue("@DataPublikimit", DateTime.Now);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Njoftimi u publikua me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close(); // Mbyll formën pas publikimit
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë publikimit të njoftimit: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PublikoNjoftime_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Dispose();
        }
    }
}