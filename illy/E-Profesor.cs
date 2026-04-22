using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace illy
{
    public partial class E_Profesor : Form
    {
        private int userId;
        private string connectionString =
        "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";


        public E_Profesor(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            SetupGridView();
            LoadNjoftimet();
        }

        private void SetupGridView()
        {
            eProfesorGridView.AutoGenerateColumns = true;
            eProfesorGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            eProfesorGridView.MultiSelect = false;
            eProfesorGridView.AllowUserToAddRows = false;
            eProfesorGridView.ReadOnly = true;

            // Aktivizo word wrapping për përmbajtjen dhe lëndën
            eProfesorGridView.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            eProfesorGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells; // Rregullo lartësinë e rreshtave automatikisht
        }

        private void LoadNjoftimet()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                        SELECT 
                            n.Titulli,
                            n.Permbajtja,
                            n.DataPublikimit,
                            l.EmriLendes AS Lenda,
                            u.Username AS Profesori
                        FROM Njoftimet n
                        JOIN Lendet l ON n.LendeID = l.LendeID
                        JOIN Userat u ON n.ProfesoriID = u.UserID
                        JOIN Userat s ON s.Viti = l.Viti 
                            AND s.Semestri = l.Semestri 
                            AND s.DrejtimID = l.DrejtimID 
                            AND s.NendrejtimID = l.NendrejtimID
                        WHERE s.UserID = @StudentID
                        ORDER BY n.DataPublikimit DESC";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@StudentID", userId);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable njoftimetTable = new DataTable();
                            adapter.Fill(njoftimetTable);

                            if (njoftimetTable.Rows.Count == 0)
                            {
                                MessageBox.Show("Nuk u gjetën njoftime për studentin e zgjedhur.", "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }

                            eProfesorGridView.DataSource = njoftimetTable;

                            // Përshtat kolonat
                            eProfesorGridView.Columns["Titulli"].HeaderText = "Titulli";
                            eProfesorGridView.Columns["Permbajtja"].HeaderText = "Përmbajtja";
                            eProfesorGridView.Columns["DataPublikimit"].HeaderText = "Data e Publikimit";
                            eProfesorGridView.Columns["Lenda"].HeaderText = "Lënda";
                            eProfesorGridView.Columns["Profesori"].HeaderText = "Profesori";

                            // Përshtat gjerësinë e kolonave
                            eProfesorGridView.Columns["Titulli"].Width = 150;
                            eProfesorGridView.Columns["Permbajtja"].Width = 300; // Më e gjerë për të shfaqur përmbajtjen
                            eProfesorGridView.Columns["DataPublikimit"].Width = 120;
                            eProfesorGridView.Columns["Lenda"].Width = 200; // Më e gjerë për të shfaqur lëndën
                            eProfesorGridView.Columns["Profesori"].Width = 120;

                            // Formato datën
                            if (eProfesorGridView.Columns.Contains("DataPublikimit"))
                            {
                                eProfesorGridView.Columns["DataPublikimit"].DefaultCellStyle.Format = "yyyy-MM-dd";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ngarkimit të njoftimeve: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
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