using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace illy
{
    public partial class TranskriptaNotave : Form
    {
        private string connectionString =
            "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";

        private int adminUserId;

        // Klasë e thjeshtë për të ruajtur informacionin e studentit (në vend të dynamic)
        private class StudentInfo
        {
            public string Emri { get; set; }
            public string Kontrata { get; set; }
            public string Drejtimi { get; set; }
            public string Nendrejtimi { get; set; }
        }

        public TranskriptaNotave(int adminUserId)
        {
            InitializeComponent();
            this.adminUserId = adminUserId;

            ShfaqGridinBosh();
            kerkoTextBox.Focus();

            // Kërko live gjatë shkrimit
            kerkoTextBox.TextChanged += kerkoTextBox_TextChanged;
        }

        private void kerkoTextBox_TextChanged(object sender, EventArgs e)
        {
            string kontrata = kerkoTextBox.Text.Trim();

            // Nëse është shumë shkurtër, pastro grid-in
            if (string.IsNullOrWhiteSpace(kontrata) || kontrata.Length < 3)
            {
                ShfaqGridinBosh();
                return;
            }

            NgarkoTranskripten(kontrata);
        }

        private void ShfaqGridinBosh()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Lenda", typeof(string));
            dt.Columns.Add("Profesori", typeof(string));
            dt.Columns.Add("NotaPërfundimtare", typeof(int));

            ShfaqTranskriptenGridView.DataSource = dt;

            if (ShfaqTranskriptenGridView.Columns.Contains("Lenda"))
            {
                ShfaqTranskriptenGridView.Columns["Lenda"].HeaderText = "Lënda";
                ShfaqTranskriptenGridView.Columns["Profesori"].HeaderText = "Profesori";
                ShfaqTranskriptenGridView.Columns["NotaPërfundimtare"].HeaderText = "Nota Përfundimtare";
            }
        }

        private void NgarkoTranskripten(string kontrata)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Merr informacionin e studentit
                    string userQuery = @"
                        SELECT u.UserID, u.Username, u.ContractNumber,
                               d.EmriDrejtimit, n.EmriNendrejtimit
                        FROM Userat u
                        LEFT JOIN Drejtimet d ON u.DrejtimID = d.DrejtimID
                        LEFT JOIN Nendrejtimet n ON u.NendrejtimID = n.NendrejtimID
                        WHERE u.ContractNumber = @Kontrata";

                    int studentiID = 0;
                    string emri = "", kontrataDB = "", drejtimi = "", nendrejtimi = "";

                    using (SqlCommand cmd = new SqlCommand(userQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Kontrata", kontrata);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                studentiID = reader.GetInt32(0);
                                emri = reader["Username"].ToString();
                                kontrataDB = reader["ContractNumber"].ToString();
                                drejtimi = reader["EmriDrejtimit"] != DBNull.Value ? reader["EmriDrejtimit"].ToString() : "Nuk është përcaktuar";
                                nendrejtimi = reader["EmriNendrejtimit"] != DBNull.Value ? reader["EmriNendrejtimit"].ToString() : "";
                            }
                            else
                            {
                                ShfaqGridinBosh();
                                return; // Pa mesazh gjatë kërkimit live
                            }
                        }
                    }

                    // Query e re: UNION për të marrë të gjitha notat perfundimtare
                    string transkriptaQuery = @"
                        -- Notat perfundimtare nga tabela NotatPerfundimtare (prioritet)
                        SELECT 
                            l.EmriLendes AS Lenda,
                            prof.Username AS Profesori,
                            n.NotaPerfundimtare AS NotaPërfundimtare
                        FROM NotatPerfundimtare n
                        INNER JOIN Lendet l ON n.LendeID = l.LendeID
                        INNER JOIN Userat prof ON l.ProfesoriID = prof.UserID
                        WHERE n.StudentiID = @StudentiID

                        UNION

                        -- Notat nga Provimet vetëm nëse nuk ka notë perfundimtare
                        SELECT 
                            l.EmriLendes AS Lenda,
                            prof.Username AS Profesori,
                            p.Nota AS NotaPërfundimtare
                        FROM Provimet p
                        INNER JOIN Lendet l ON p.LendeID = l.LendeID
                        INNER JOIN Userat prof ON p.ProfesoriID = prof.UserID
                        LEFT JOIN NotatPerfundimtare n ON p.LendeID = n.LendeID AND p.StudentiID = n.StudentiID
                        WHERE p.StudentiID = @StudentiID 
                          AND n.NotaID IS NULL   -- shmang dublikimin

                        ORDER BY Lenda";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(transkriptaQuery, con))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@StudentiID", studentiID);

                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        if (dt.Rows.Count == 0)
                        {
                            ShfaqGridinBosh();
                            return;
                        }

                        ShfaqTranskriptenGridView.DataSource = dt;

                        ShfaqTranskriptenGridView.Columns["Lenda"].HeaderText = "Lënda";
                        ShfaqTranskriptenGridView.Columns["Profesori"].HeaderText = "Profesori";
                        ShfaqTranskriptenGridView.Columns["NotaPërfundimtare"].HeaderText = "Nota Përfundimtare";

                        // Ruaj informacionin për PDF
                        Tag = new StudentInfo
                        {
                            Emri = emri,
                            Kontrata = kontrataDB,
                            Drejtimi = drejtimi,
                            Nendrejtimi = nendrejtimi
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                // Vetëm log për kërkim live – mos shfaq popup çdo herë
                System.Diagnostics.Debug.WriteLine("Gabim në NgarkoTranskripten: " + ex.Message);
                ShfaqGridinBosh();
            }
        }

        private void ruajePDFButton_Click(object sender, EventArgs e)
        {
            if (ShfaqTranskriptenGridView.DataSource == null || ShfaqTranskriptenGridView.Rows.Count == 0)
            {
                MessageBox.Show("Nuk ka transkriptë për tu ruajtur! Kërko një student fillimisht.", "Kujdes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string kontrata = kerkoTextBox.Text.Trim();
            string fileName = $"Transkripta_{kontrata.Replace("/", "-").Replace("\\", "-")}_{DateTime.Now:yyyyMMdd}.pdf";

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files|*.pdf",
                Title = "Ruaj Transkriptën si PDF",
                FileName = fileName
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (FileStream fs = new FileStream(saveFileDialog.FileName, FileMode.Create))
                    {
                        Document document = new Document(PageSize.A4, 30, 30, 40, 40);
                        PdfWriter writer = PdfWriter.GetInstance(document, fs);
                        document.Open();

                        Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18f);
                        Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14f);
                        Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11f);

                        Paragraph title = new Paragraph("Transkripta e Notave", titleFont);
                        title.Alignment = Element.ALIGN_CENTER;
                        document.Add(title);
                        document.Add(new Paragraph(" "));

                        StudentInfo studentInfo = (StudentInfo)Tag;

                        document.Add(new Paragraph("Student: " + studentInfo.Emri, headerFont));
                        document.Add(new Paragraph("Numri i kontratës: " + studentInfo.Kontrata, normalFont));
                        document.Add(new Paragraph("Drejtimi: " + studentInfo.Drejtimi, normalFont));

                        if (!string.IsNullOrEmpty(studentInfo.Nendrejtimi))
                            document.Add(new Paragraph("Nëndrejtimi: " + studentInfo.Nendrejtimi, normalFont));

                        document.Add(new Paragraph("Data e gjenerimit: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"), normalFont));
                        document.Add(new Paragraph(" "));

                        PdfPTable table = new PdfPTable(3);
                        table.WidthPercentage = 100;
                        table.SetWidths(new float[] { 40f, 30f, 30f });

                        string[] headers = { "Lënda", "Profesori", "Nota Përfundimtare" };
                        foreach (string h in headers)
                        {
                            PdfPCell cell = new PdfPCell(new Phrase(h, headerFont));
                            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                            cell.HorizontalAlignment = Element.ALIGN_CENTER;
                            cell.Padding = 5;
                            table.AddCell(cell);
                        }

                        foreach (DataGridViewRow row in ShfaqTranskriptenGridView.Rows)
                        {
                            if (row.IsNewRow) continue;

                            table.AddCell(new Phrase(row.Cells["Lenda"].Value != null ? row.Cells["Lenda"].Value.ToString() : "", normalFont));
                            table.AddCell(new Phrase(row.Cells["Profesori"].Value != null ? row.Cells["Profesori"].Value.ToString() : "", normalFont));
                            table.AddCell(new Phrase(row.Cells["NotaPërfundimtare"].Value != null ? row.Cells["NotaPërfundimtare"].Value.ToString() : "", normalFont));
                        }

                        document.Add(table);
                        document.Close();
                        writer.Close();

                        MessageBox.Show("Transkripta u ruajt si PDF!\n" + saveFileDialog.FileName, "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        kerkoTextBox.Clear();
                        ShfaqGridinBosh();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gabim gjatë krijimit të PDF:\n" + ex.Message, "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}