using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace illy
{
    public partial class StatistikaForm : Form
    {
        private string connectionString =
            "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";

        private int adminUserId;

        public StatistikaForm(int userId)
        {
            this.adminUserId = userId;
            InitializeComponent();

            // Sigurohu që StatistikaPanel ekziston në designer
            if (StatistikaPanel == null)
            {
                MessageBox.Show("Paneli 'StatistikaPanel' nuk u gjet! Shtoje në designer.", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Krijo dhe vendos 5 diagrame brenda panelit
            KrijoDiagrameNePanel();

            // Ngarko të dhënat kur hapet forma
            this.Load += StatistikaForm_Load;
        }

        private void KrijoDiagrameNePanel()
        {
            StatistikaPanel.Controls.Clear();

            int width = StatistikaPanel.Width / 3;   // ~272
            int height = StatistikaPanel.Height / 2; // ~202

            // 1. Pie Chart - Userat Total vs Aktivë
            Chart chartUserat = KrijoChart(width, height, "Userat Total vs Aktivë");
            chartUserat.Location = new Point(0, 0);
            StatistikaPanel.Controls.Add(chartUserat);

            // 2. Bar Chart - Studentë që shkarkojnë
            Chart chartShkarkime = KrijoChart(width, height, "Studentë që Shkarkojnë Materiale");
            chartShkarkime.Location = new Point(width, 0);
            StatistikaPanel.Controls.Add(chartShkarkime);

            // 3. Column Chart - Ndryshime Password
            Chart chartPassword = KrijoChart(width, height, "Ndryshime Password (Total)");
            chartPassword.Location = new Point(width * 2, 0);
            StatistikaPanel.Controls.Add(chartPassword);

            // 4. Line Chart - Aktivitetet (login trend)
            Chart chartAktivitete = KrijoChart(width * 2, height, "Aktivitetet e Fundit (Login Trend)");
            chartAktivitete.Location = new Point(0, height);
            StatistikaPanel.Controls.Add(chartAktivitete);

            // 5. Doughnut Chart - Financat
            Chart chartFinancat = KrijoChart(width, height, "Financat e Paguara");
            chartFinancat.Location = new Point(width * 2, height);
            StatistikaPanel.Controls.Add(chartFinancat);
        }

        private Chart KrijoChart(int w, int h, string titulli)
        {
            Chart c = new Chart
            {
                Size = new Size(w - 10, h - 10),
                BackColor = Color.WhiteSmoke,
                BorderSkin = new BorderSkin { SkinStyle = BorderSkinStyle.Emboss }
            };

            ChartArea area = new ChartArea();
            c.ChartAreas.Add(area);

            Title t = new Title(titulli, Docking.Top, new Font("Segoe UI", 11, FontStyle.Bold), Color.DarkBlue);
            c.Titles.Add(t);

            return c;
        }

        private void StatistikaForm_Load(object sender, EventArgs e)
        {
            NgarkoDheShfaqStatistikat();
        }

        private void NgarkoDheShfaqStatistikat()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // 1. Userat Total vs Aktivë
                    string q1 = @"
                        SELECT 
                            (SELECT COUNT(*) FROM Userat) AS Total,
                            (SELECT COUNT(DISTINCT UserID) FROM LoginLogs 
                             WHERE LoginTime >= DATEADD(DAY, -30, GETDATE())) AS Aktive";
                    using (SqlCommand cmd = new SqlCommand(q1, con))
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            int total = r.GetInt32(0);
                            int aktive = r.GetInt32(1);
                            Chart c = (Chart)StatistikaPanel.Controls[0];
                            KrijoPieChart(c, new[] { "Aktivë", "Jo-aktivë" }, new[] { aktive, total - aktive });
                        }
                    }

                    // 2. Studentë që shkarkojnë
                    string q2 = @"
                        SELECT 
                            (SELECT COUNT(*) FROM Userat WHERE RoleID = 1) AS TotalStudente,
                            (SELECT COUNT(DISTINCT StudentiID) FROM ShkarkimetMaterialeve) AS Shkarkues";
                    using (SqlCommand cmd = new SqlCommand(q2, con))
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            int total = r.GetInt32(0);
                            int shk = r.GetInt32(1);
                            Chart c = (Chart)StatistikaPanel.Controls[1];
                            KrijoBarChart(c, new[] { "Shkarkues", "Jo-shkarkues" }, new[] { shk, total - shk });
                        }
                    }

                    // 3. Ndryshime Password (vetëm total për thjeshtësi)
                    string q3 = "SELECT COUNT(DISTINCT UserID) FROM PasswordChangeLogs";
                    using (SqlCommand cmd = new SqlCommand(q3, con))
                    {
                        int numri = (int)cmd.ExecuteScalar();
                        Chart c = (Chart)StatistikaPanel.Controls[2];
                        KrijoColumnChart(c, new[] { "Ndryshime Password" }, new[] { numri });
                    }

                    // 4. Aktivitetet - Line Chart (5 ditët e fundit)
                    string q4 = @"
                        SELECT TOP 5 
                            CONVERT(date, LoginTime) AS Data,
                            COUNT(*) AS NumriLogin
                        FROM LoginLogs
                        WHERE LoginTime >= DATEADD(DAY, -5, GETDATE())
                        GROUP BY CONVERT(date, LoginTime)
                        ORDER BY Data DESC";
                    using (SqlDataAdapter da = new SqlDataAdapter(q4, con))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        Chart c = (Chart)StatistikaPanel.Controls[3];
                        KrijoLineChart(c, dt, "Data", "NumriLogin");
                    }

                    // 5. Financat - Doughnut
                    string q5 = @"
                        SELECT 
                            ISNULL(SUM(Shuma), 0) AS TotalPaguar,
                            COUNT(*) AS NumriPagesave
                        FROM Financat";
                    using (SqlCommand cmd = new SqlCommand(q5, con))
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            decimal total = r.GetDecimal(0);
                            int pagesa = r.GetInt32(1);
                            Chart c = (Chart)StatistikaPanel.Controls[4];
                            KrijoDoughnutChart(c, new[] { "Paguar", "Pagesa" }, new[] { (double)total, pagesa });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gabim gjatë ngarkimit të statistikave:\n" + ex.Message, "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Ndihmës për Pie Chart
        private void KrijoPieChart(Chart chart, string[] labels, int[] values)
        {
            chart.Series.Clear();
            Series s = new Series("Pie") { ChartType = SeriesChartType.Pie };
            for (int i = 0; i < labels.Length; i++)
            {
                s.Points.AddXY(labels[i], values[i]);
            }
            chart.Series.Add(s);
            chart.ChartAreas[0].Area3DStyle.Enable3D = true;
        }

        // Bar Chart
        private void KrijoBarChart(Chart chart, string[] labels, int[] values)
        {
            chart.Series.Clear();
            Series s = new Series("Bar") { ChartType = SeriesChartType.Bar };
            for (int i = 0; i < labels.Length; i++)
            {
                s.Points.AddXY(labels[i], values[i]);
            }
            chart.Series.Add(s);
        }

        // Column Chart (vetëm një kolonë)
        private void KrijoColumnChart(Chart chart, string[] labels, int[] values)
        {
            chart.Series.Clear();
            Series s = new Series("Column") { ChartType = SeriesChartType.Column };
            for (int i = 0; i < labels.Length; i++)
            {
                s.Points.AddXY(labels[i], values[i]);
            }
            chart.Series.Add(s);
        }

        // Line Chart për trend
        private void KrijoLineChart(Chart chart, DataTable dt, string xColumn, string yColumn)
        {
            chart.Series.Clear();
            Series s = new Series("Line") { ChartType = SeriesChartType.Line, BorderWidth = 3 };
            foreach (DataRow row in dt.Rows)
            {
                s.Points.AddXY(row[xColumn], row[yColumn]);
            }
            chart.Series.Add(s);
            chart.ChartAreas[0].AxisX.LabelStyle.Angle = -45;
        }

        // Doughnut Chart për financat
        private void KrijoDoughnutChart(Chart chart, string[] labels, double[] values)
        {
            chart.Series.Clear();
            Series s = new Series("Doughnut") { ChartType = SeriesChartType.Doughnut };
            for (int i = 0; i < labels.Length; i++)
            {
                s.Points.AddXY(labels[i], values[i]);
            }
            chart.Series.Add(s);
            chart.ChartAreas[0].Area3DStyle.Enable3D = true;
        }
    }
}