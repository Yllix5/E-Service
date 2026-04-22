using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace illy
{
    public partial class BallinaForm : Form
    {
        private int userId;
        private string connectionString =
            "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";
        private int[] lendeIds = new int[5];

        public BallinaForm(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            LoadLendet();
        }

        private void LoadLendet()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Merr DrejtimID, NendrejtimID, Viti dhe Semestri nga tabela Userat
                    string userQuery = "SELECT DrejtimID, NendrejtimID, Viti, Semestri FROM Userat WHERE UserID = @UserID";
                    int drejtimId = 0, nendrejtimId = 0, viti = 1, semestri = 0;
                    using (SqlCommand userCmd = new SqlCommand(userQuery, con))
                    {
                        userCmd.Parameters.AddWithValue("@UserID", userId);
                        using (SqlDataReader reader = userCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                drejtimId = reader["DrejtimID"] != DBNull.Value ? reader.GetInt32(0) : 0;
                                nendrejtimId = reader["NendrejtimID"] != DBNull.Value ? reader.GetInt32(1) : 0;
                                viti = reader["Viti"] != DBNull.Value ? reader.GetInt32(2) : 1;
                                semestri = reader["Semestri"] != DBNull.Value ? reader.GetInt32(3) : 0;
                            }
                            else
                            {
                                MessageBox.Show("Përdoruesi nuk u gjet në databazë!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }
                    }

                    if (drejtimId == 0 || nendrejtimId == 0)
                    {
                        MessageBox.Show("Drejtimi ose nëndrejtimi i studentit nuk është përcaktuar!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    if (viti == 0)
                    {
                        MessageBox.Show("Viti i studentit nuk është përcaktuar!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    if (semestri == 0)
                    {
                        MessageBox.Show("Semestri i studentit nuk është përcaktuar!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    Label[] lendaLabels = { lenda1Label, lenda2Label, lenda3Label, lenda4Label, lenda5Label };
                    Guna2Button[] materialetButtons = { materialet1Button, materialet2Button, materialet3Button, materialet4Button, materialet5Button };
                    Guna2Button[] detyratButtons = { detyrat1Button, detyrat2Button, detyrat3Button, detyrat4Button, detyrat5Button };
                    List<(int LendeID, string EmriLendes)> lendetList = new List<(int, string)>();

                    // Filtro lëndët sipas DrejtimID, NendrejtimID, Viti dhe Semestri
                    string lendetQuery = "SELECT LendeID, EmriLendes FROM Lendet WHERE DrejtimID = @DrejtimID AND NendrejtimID = @NendrejtimID AND Viti = @Viti AND Semestri = @Semestri";
                    using (SqlCommand lendetCmd = new SqlCommand(lendetQuery, con))
                    {
                        lendetCmd.Parameters.AddWithValue("@DrejtimID", drejtimId);
                        lendetCmd.Parameters.AddWithValue("@NendrejtimID", nendrejtimId);
                        lendetCmd.Parameters.AddWithValue("@Viti", viti);
                        lendetCmd.Parameters.AddWithValue("@Semestri", semestri);
                        using (SqlDataReader reader = lendetCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int lendeId = reader.GetInt32(0);
                                string emriLendes = reader.GetString(1);
                                lendetList.Add((lendeId, emriLendes));
                            }
                        }
                    }

                    int index = 0;
                    foreach (var lende in lendetList)
                    {
                        if (index >= lendaLabels.Length) break;
                        lendaLabels[index].Text = lende.EmriLendes;
                        lendeIds[index] = lende.LendeID;

                        // Numri i materialeve të zakonshme (Ligjerata + Ushtrime)
                        string materialetQuery = @"
                            SELECT COUNT(*)
                            FROM Materialet
                            WHERE LendeID = @LendeID
                              AND Tipi IN ('Ligjerata', 'Ushtrime')";
                        using (SqlCommand materialetCmd = new SqlCommand(materialetQuery, con))
                        {
                            materialetCmd.Parameters.AddWithValue("@LendeID", lende.LendeID);
                            int numriMaterialeve = (int)materialetCmd.ExecuteScalar();
                            materialetButtons[index].Text = numriMaterialeve.ToString();
                        }

                        // Numri i detyrave (vetëm Tipi = 'Detyrë')
                        string detyratQuery = @"
                            SELECT COUNT(*)
                            FROM Materialet
                            WHERE LendeID = @LendeID
                              AND TRIM(UPPER(Tipi)) = 'DETYRË'";
                        using (SqlCommand detyratCmd = new SqlCommand(detyratQuery, con))
                        {
                            detyratCmd.Parameters.AddWithValue("@LendeID", lende.LendeID);
                            int numriDetyrave = (int)detyratCmd.ExecuteScalar();
                            detyratButtons[index].Text = numriDetyrave.ToString();
                        }

                        index++;
                    }

                    for (int i = index; i < lendaLabels.Length; i++)
                    {
                        lendaLabels[i].Text = "";
                        materialetButtons[i].Text = "0";
                        detyratButtons[i].Text = "0";
                        lendeIds[i] = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ngarkimit të lëndëve: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void materialet1Button_Click(object sender, EventArgs e)
        {
            if (lendeIds[0] != 0)
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT COUNT(*) FROM Materialet WHERE LendeID = @LendeID AND (Pershkrimi NOT LIKE '%Detyrë%' OR Pershkrimi IS NULL)";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@LendeID", lendeIds[0]);
                        int numriMaterialeve = (int)cmd.ExecuteScalar();
                        if (numriMaterialeve > 0)
                        {
                            MaterialetListForm materialetListForm = new MaterialetListForm(lendeIds[0], userId, false);
                            materialetListForm.ShowDialog();
                        }
                        else
                        {
                            MessageBox.Show("Nuk ka materiale për këtë lëndë!", "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }

        private void materialet2Button_Click(object sender, EventArgs e)
        {
            if (lendeIds[1] != 0)
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT COUNT(*) FROM Materialet WHERE LendeID = @LendeID AND (Pershkrimi NOT LIKE '%Detyrë%' OR Pershkrimi IS NULL)";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@LendeID", lendeIds[1]);
                        int numriMaterialeve = (int)cmd.ExecuteScalar();
                        if (numriMaterialeve > 0)
                        {
                            MaterialetListForm materialetListForm = new MaterialetListForm(lendeIds[1], userId, false);
                            materialetListForm.ShowDialog();
                        }
                        else
                        {
                            MessageBox.Show("Nuk ka materiale për këtë lëndë!", "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }

        private void materialet3Button_Click(object sender, EventArgs e)
        {
            if (lendeIds[2] != 0)
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT COUNT(*) FROM Materialet WHERE LendeID = @LendeID AND (Pershkrimi NOT LIKE '%Detyrë%' OR Pershkrimi IS NULL)";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@LendeID", lendeIds[2]);
                        int numriMaterialeve = (int)cmd.ExecuteScalar();
                        if (numriMaterialeve > 0)
                        {
                            MaterialetListForm materialetListForm = new MaterialetListForm(lendeIds[2], userId, false);
                            materialetListForm.ShowDialog();
                        }
                        else
                        {
                            MessageBox.Show("Nuk ka materiale për këtë lëndë!", "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }

        private void materialet4Button_Click(object sender, EventArgs e)
        {
            if (lendeIds[3] != 0)
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT COUNT(*) FROM Materialet WHERE LendeID = @LendeID AND (Pershkrimi NOT LIKE '%Detyrë%' OR Pershkrimi IS NULL)";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@LendeID", lendeIds[3]);
                        int numriMaterialeve = (int)cmd.ExecuteScalar();
                        if (numriMaterialeve > 0)
                        {
                            MaterialetListForm materialetListForm = new MaterialetListForm(lendeIds[3], userId, false);
                            materialetListForm.ShowDialog();
                        }
                        else
                        {
                            MessageBox.Show("Nuk ka materiale për këtë lëndë!", "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }

        private void materialet5Button_Click(object sender, EventArgs e)
        {
            if (lendeIds[4] != 0)
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT COUNT(*) FROM Materialet WHERE LendeID = @LendeID AND (Pershkrimi NOT LIKE '%Detyrë%' OR Pershkrimi IS NULL)";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@LendeID", lendeIds[4]);
                        int numriMaterialeve = (int)cmd.ExecuteScalar();
                        if (numriMaterialeve > 0)
                        {
                            MaterialetListForm materialetListForm = new MaterialetListForm(lendeIds[4], userId, false);
                            materialetListForm.ShowDialog();
                        }
                        else
                        {
                            MessageBox.Show("Nuk ka materiale për këtë lëndë!", "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }

        private void detyrat1Button_Click(object sender, EventArgs e)
        {
            if (lendeIds[0] != 0)
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT COUNT(*) FROM Materialet WHERE LendeID = @LendeID AND TRIM(UPPER(Tipi)) = 'DETYRË'";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@LendeID", lendeIds[0]);
                        int numriDetyrave = (int)cmd.ExecuteScalar();
                        if (numriDetyrave > 0)
                        {
                            DetyratList detyratListForm = new DetyratList(lendeIds[0], userId);
                            detyratListForm.ShowDialog();
                        }
                        else
                        {
                            MessageBox.Show("Nuk ka detyra për këtë lëndë!", "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }

        private void detyrat2Button_Click(object sender, EventArgs e)
        {
            if (lendeIds[1] != 0)
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT COUNT(*) FROM Materialet WHERE LendeID = @LendeID AND TRIM(UPPER(Tipi)) = 'DETYRË'";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@LendeID", lendeIds[1]);
                        int numriDetyrave = (int)cmd.ExecuteScalar();
                        if (numriDetyrave > 0)
                        {
                            DetyratList detyratListForm = new DetyratList(lendeIds[1], userId);
                            detyratListForm.ShowDialog();
                        }
                        else
                        {
                            MessageBox.Show("Nuk ka detyra për këtë lëndë!", "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }

        private void detyrat3Button_Click(object sender, EventArgs e)
        {
            if (lendeIds[2] != 0)
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT COUNT(*) FROM Materialet WHERE LendeID = @LendeID AND TRIM(UPPER(Tipi)) = 'DETYRË'";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@LendeID", lendeIds[2]);
                        int numriDetyrave = (int)cmd.ExecuteScalar();
                        if (numriDetyrave > 0)
                        {
                            DetyratList detyratListForm = new DetyratList(lendeIds[2], userId);
                            detyratListForm.ShowDialog();
                        }
                        else
                        {
                            MessageBox.Show("Nuk ka detyra për këtë lëndë!", "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }

        private void detyrat4Button_Click(object sender, EventArgs e)
        {
            if (lendeIds[3] != 0)
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT COUNT(*) FROM Materialet WHERE LendeID = @LendeID AND TRIM(UPPER(Tipi)) = 'DETYRË'";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@LendeID", lendeIds[3]);
                        int numriDetyrave = (int)cmd.ExecuteScalar();
                        if (numriDetyrave > 0)
                        {
                            DetyratList detyratListForm = new DetyratList(lendeIds[3], userId);
                            detyratListForm.ShowDialog();
                        }
                        else
                        {
                            MessageBox.Show("Nuk ka detyra për këtë lëndë!", "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }

        private void detyrat5Button_Click(object sender, EventArgs e)
        {
            if (lendeIds[4] != 0)
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT COUNT(*) FROM Materialet WHERE LendeID = @LendeID AND TRIM(UPPER(Tipi)) = 'DETYRË'";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@LendeID", lendeIds[4]);
                        int numriDetyrave = (int)cmd.ExecuteScalar();
                        if (numriDetyrave > 0)
                        {
                            DetyratList detyratListForm = new DetyratList(lendeIds[4], userId);
                            detyratListForm.ShowDialog();
                        }
                        else
                        {
                            MessageBox.Show("Nuk ka detyra për këtë lëndë!", "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            ReferentForm FormaRef = new ReferentForm();
            FormaRef.ShowDialog();
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            PerkrahjaTeknike PTK = new PerkrahjaTeknike();
            PTK.ShowDialog();
        }
    }
}