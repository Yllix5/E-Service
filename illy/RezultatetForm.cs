using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient; 

namespace illy
{
    public partial class RezultatetForm : Form
    {
        private int _userId;
        private string connectionString =
        "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";


        public RezultatetForm(int userId)
        {
            InitializeComponent();
            _userId = userId; 
            LoadRezultatet(); 
        }

        private void LoadRezultatet()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Query për të marrë të dhënat nga Rezultatet, Lendet dhe Userat (për profesorin)
                    string query = @"
                        SELECT 
                            L.EmriLendes AS [Lënda],
                            U.Username AS [Profesori],
                            R.Pershkrimi AS [Kolokviumi],
                            R.DataRegjistrimit AS [Data],
                            R.Pershkrimi AS [Shënim Shtesë],
                            R.Piket AS [Pikët]
                        FROM Rezultatet R
                        JOIN Lendet L ON R.LendeID = L.LendeID
                        JOIN Userat U ON R.ProfesoriID = U.UserID
                        WHERE R.StudentiID = @UserID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", _userId); // Filtro sipas studentit

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            // Lidh DataTable me GridView
                            RezultatetGridView.DataSource = dt;

                            // Përshtat kolonat (opsionale, për estetikë)
                            RezultatetGridView.Columns["Lënda"].Width = 150;
                            RezultatetGridView.Columns["Profesori"].Width = 120;
                            RezultatetGridView.Columns["Kolokviumi"].Width = 120;
                            RezultatetGridView.Columns["Data"].Width = 100;
                            RezultatetGridView.Columns["Shënim Shtesë"].Width = 150;
                            RezultatetGridView.Columns["Pikët"].Width = 80;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gabim gjatë ngarkimit të rezultateve: " + ex.Message, "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
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