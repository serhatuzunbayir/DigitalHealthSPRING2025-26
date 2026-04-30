using DigitalHealthTrainer.Data;
using Npgsql;

namespace DigitalHealthTrainer.Forms
{
    public class TestConnectionForm : Form
    {
        private Button btnTest;
        private Label lblResult;
        private ListBox lstTables;

        public TestConnectionForm()
        {
            this.Text = "Veritabanı Bağlantı Testi";
            this.Size = new Size(450, 350);
            this.StartPosition = FormStartPosition.CenterScreen;

            btnTest = new Button
            {
                Text = "Bağlantıyı Test Et",
                Location = new Point(20, 20),
                Size = new Size(200, 40),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnTest.Click += BtnTest_Click;

            lblResult = new Label
            {
                Text = "Henüz test edilmedi.",
                Location = new Point(20, 75),
                Size = new Size(400, 25),
                Font = new Font("Segoe UI", 10)
            };

            lstTables = new ListBox
            {
                Location = new Point(20, 110),
                Size = new Size(390, 180),
                Font = new Font("Consolas", 10)
            };

            this.Controls.AddRange(new Control[] { btnTest, lblResult, lstTables });
        }

        private void BtnTest_Click(object? sender, EventArgs e)
        {
            lstTables.Items.Clear();
            lblResult.ForeColor = Color.Black;
            lblResult.Text = "Bağlanılıyor...";

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                conn.Open();

                lblResult.ForeColor = Color.Green;
                lblResult.Text = "Bağlantı başarılı!";

                // Tabloları listele
                string query = @"SELECT table_name
                                 FROM information_schema.tables
                                 WHERE table_schema = 'public'
                                 ORDER BY table_name";

                using var cmd = new NpgsqlCommand(query, conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lstTables.Items.Add(reader.GetString(0));
                }

                if (lstTables.Items.Count == 0)
                {
                    lstTables.Items.Add("(Henüz tablo oluşturulmamış)");
                }
            }
            catch (Exception ex)
            {
                lblResult.ForeColor = Color.Red;
                lblResult.Text = "Bağlantı başarısız!";
                lstTables.Items.Add("Hata: " + ex.Message);
            }
        }
    }
}
