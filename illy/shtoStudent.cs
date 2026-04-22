using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging; // Shtuar për ImageFormat
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace illy
{
    public partial class shtoStudent : Form
    {
        private string connectionString =
            "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";
        private Image selectedImage; // Foto e re e zgjedhur nga përdoruesi (nëse ka)
        private byte[] existingPhoto; // Foto origjinale nga databaza (ruhet gjatë update)
        private string originalUsername;

        public shtoStudent()
        {
            InitializeComponent();
            LoadDrejtimet();
            LoadNendrejtimet();
            LoadGrupet();
            insertPictureBox.Click += InsertPictureBox_Click;
            shtoDateTimePicker.Value = DateTime.Today.AddYears(-18);
            shtoDateTimePicker.MaxDate = DateTime.Today.AddYears(-16);
            shtoDateTimePicker.MinDate = DateTime.Today.AddYears(-100);
        }

        // Konstruktori për update – me foto ekzistuese dhe fusha shtesë (backupCode, personalNumber bosh)
        public shtoStudent(string username, string phoneNumber, string email, string contract,
                           string drejtimi, string nendrejtimi, string grupi,
                           DateTime dateOfBirth, byte[] photo, string backupCode)
        {
            InitializeComponent();
            LoadDrejtimet();
            LoadNendrejtimet();
            LoadGrupet();
            originalUsername = username;
            textUsername.Text = username;
            textNumber.Text = phoneNumber;
            textEmail.Text = email;
            textContract.Text = contract;
            shtoDateTimePicker.Value = dateOfBirth;
            shtoDateTimePicker.MaxDate = DateTime.Today.AddYears(-16);
            shtoDateTimePicker.MinDate = DateTime.Today.AddYears(-100);

            // Fushat shtesë
            backTextBox.Text = backupCode; // Backup code shfaqet (plaintext)
            numriPersonalTextBox.Text = "••••••"; // Placeholder për numrin personal (nuk mund të de-hash-ohet)
            textPassword.Text = ""; // Password lëre bosh (ndryshohet vetëm nëse plotësohet i ri)

            // Ruaj foton ekzistuese nga databaza
            existingPhoto = photo;

            // Shfaq foton nëse ekziston
            if (photo != null && photo.Length > 0)
            {
                try
                {
                    using (MemoryStream ms = new MemoryStream(photo))
                    {
                        selectedImage = Image.FromStream(ms); // vendoset si selectedImage që të shfaqet
                        previewPictureBox.Image = selectedImage;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Gabim ngarkimi foto: " + ex.Message);
                    previewPictureBox.Image = null;
                    selectedImage = null;
                }
             
            }
            else
            {
                previewPictureBox.Image = null;
                selectedImage = null;
            }

            // Vendos ComboBox-et
            foreach (DataRowView item in drejtimiComboBox.Items)
            {
                if (item["EmriDrejtimit"].ToString() == drejtimi)
                {
                    drejtimiComboBox.SelectedItem = item;
                    break;
                }
            }

            // Thirr manualisht për të ringarkuar nëndrejtime pas drejtimit
            DrejtimiComboBox_SelectedIndexChanged(null, null);

            foreach (DataRowView item in nendrejtimiComboBox.Items)
            {
                if (item["EmriNendrejtimit"].ToString() == nendrejtimi)
                {
                    nendrejtimiComboBox.SelectedItem = item;
                    break;
                }
            }

            foreach (DataRowView item in grupComboBox.Items)
            {
                if (item["EmriGrupit"].ToString() == grupi)
                {
                    grupComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void LoadDrejtimet()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT DrejtimID, EmriDrejtimit FROM Drejtimet";
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, con))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        drejtimiComboBox.DataSource = dt;
                        drejtimiComboBox.DisplayMember = "EmriDrejtimit";
                        drejtimiComboBox.ValueMember = "DrejtimID";
                    }
                }
                if (drejtimiComboBox.Items.Count > 0)
                    drejtimiComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ngarkimit të drejtimeve: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadNendrejtimet()
        {
            try
            {
                drejtimiComboBox.SelectedIndexChanged -= DrejtimiComboBox_SelectedIndexChanged;
                drejtimiComboBox.SelectedIndexChanged += DrejtimiComboBox_SelectedIndexChanged;
                if (drejtimiComboBox.SelectedValue != null)
                {
                    DrejtimiComboBox_SelectedIndexChanged(null, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ngarkimit të nëndrejtimeve: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DrejtimiComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (drejtimiComboBox.SelectedValue != null && int.TryParse(drejtimiComboBox.SelectedValue.ToString(), out int drejtimId))
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();
                        string query = "SELECT NendrejtimID, EmriNendrejtimit FROM Nendrejtimet WHERE DrejtimID = @DrejtimID";
                        using (SqlDataAdapter adapter = new SqlDataAdapter(query, con))
                        {
                            adapter.SelectCommand.Parameters.AddWithValue("@DrejtimID", drejtimId);
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            nendrejtimiComboBox.DataSource = dt;
                            nendrejtimiComboBox.DisplayMember = "EmriNendrejtimit";
                            nendrejtimiComboBox.ValueMember = "NendrejtimID";
                        }
                    }
                    if (nendrejtimiComboBox.Items.Count > 0)
                        nendrejtimiComboBox.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Gabim gjatë ngarkimit të nëndrejtimeve: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadGrupet()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT GrupID, EmriGrupit FROM Grupet";
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, con))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        grupComboBox.DataSource = dt;
                        grupComboBox.DisplayMember = "EmriGrupit";
                        grupComboBox.ValueMember = "GrupID";
                    }
                }
                if (grupComboBox.Items.Count > 0)
                    grupComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ngarkimit të grupeve: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InsertPictureBox_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    selectedImage = Image.FromFile(ofd.FileName);
                    previewPictureBox.Image = selectedImage;
                }
            }
        }

        private byte[] ImageToByteArray(Image image)
        {
            if (image == null) return null;

            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    image.Save(ms, ImageFormat.Jpeg);
                    return ms.ToArray();
                }
                catch
                {
                    try
                    {
                        image.Save(ms, ImageFormat.Png);
                        return ms.ToArray();
                    }
                    catch
                    {
                        // Hiqur MessageBox si kërkesë
                        return null;
                    }
                }
            }
        }

        private byte[] GenerateSalt()
        {
            byte[] salt = new byte[16];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        private byte[] HashValue(string value, byte[] salt)
        {
            string saltAsString = System.Text.Encoding.Unicode.GetString(salt);
            string combined = value + saltAsString;
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = System.Text.Encoding.Unicode.GetBytes(combined);
                return sha256.ComputeHash(bytes);
            }
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            return !string.IsNullOrWhiteSpace(phoneNumber) && phoneNumber.Length >= 8 && phoneNumber.All(c => char.IsDigit(c) || c == '+' || c == ' ' || c == '-');
        }

        private bool IsValidEmail(string email)
        {
            return !string.IsNullOrWhiteSpace(email) && email.Contains("@") && email.Contains(".");
        }

        private bool IsValidDateOfBirth(DateTime dateOfBirth)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - dateOfBirth.Year;
            if (dateOfBirth > today.AddYears(-age)) age--;
            return dateOfBirth <= today && age >= 16;
        }

        private bool IsValidBackupCode(string backupCode)
        {
            return !string.IsNullOrWhiteSpace(backupCode) && backupCode.Length == 6 && backupCode.All(char.IsDigit);
        }

        private void ClearFields()
        {
            textUsername.Text = "";
            textPassword.Text = "";
            numriPersonalTextBox.Text = "";
            backTextBox.Text = "";
            textNumber.Text = "";
            textEmail.Text = "";
            textContract.Text = "";
            shtoDateTimePicker.Value = DateTime.Today.AddYears(-18);
            previewPictureBox.Image = null;
            selectedImage = null;
            existingPhoto = null;
            if (drejtimiComboBox.Items.Count > 0) drejtimiComboBox.SelectedIndex = 0;
            if (nendrejtimiComboBox.Items.Count > 0) nendrejtimiComboBox.SelectedIndex = 0;
            if (grupComboBox.Items.Count > 0) grupComboBox.SelectedIndex = 0;
            originalUsername = null;
        }

        private void shtoButton_Click(object sender, EventArgs e)
        {
            string username = textUsername.Text.Trim();
            string password = textPassword.Text.Trim();
            string personalNumber = numriPersonalTextBox.Text.Trim();
            string backupCode = backTextBox.Text.Trim();
            string phoneNumber = textNumber.Text.Trim();
            string email = textEmail.Text.Trim();
            string contract = textContract.Text.Trim();
            DateTime dateOfBirth = shtoDateTimePicker.Value;
            int? drejtimId = drejtimiComboBox.SelectedValue as int?;
            int? nendrejtimId = nendrejtimiComboBox.SelectedValue as int?;
            int? grupId = grupComboBox.SelectedValue as int?;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(personalNumber) || string.IsNullOrWhiteSpace(backupCode) ||
                string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(contract) || !drejtimId.HasValue || !nendrejtimId.HasValue || !grupId.HasValue)
            {
                MessageBox.Show("Ju lutem plotësoni të gjitha fushat!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!IsValidPhoneNumber(phoneNumber))
            {
                MessageBox.Show("Numri i telefonit duhet të jetë i vlefshëm!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!IsValidEmail(email))
            {
                MessageBox.Show("Email-i duhet të jetë i vlefshëm!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!IsValidDateOfBirth(dateOfBirth))
            {
                MessageBox.Show("Data e lindjes nuk është e vlefshme!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!IsValidBackupCode(backupCode))
            {
                MessageBox.Show("Backup Code duhet të jetë saktësisht 6 shifra digjitale!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            byte[] salt = GenerateSalt();
            byte[] hashedPassword = HashValue(password, salt);
            byte[] saltPN = GenerateSalt();
            byte[] hashedPersonalNumber = HashValue(personalNumber, saltPN);
            byte[] photoToSave = selectedImage != null ? ImageToByteArray(selectedImage) : null;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    string checkUniqueQuery = @"
                        SELECT COUNT(*) FROM Userat 
                        WHERE Username = @Username OR Email = @Email OR PhoneNumber = @PhoneNumber OR ContractNumber = @ContractNumber";
                    using (SqlCommand checkCmd = new SqlCommand(checkUniqueQuery, con))
                    {
                        checkCmd.Parameters.AddWithValue("@Username", username);
                        checkCmd.Parameters.AddWithValue("@Email", email);
                        checkCmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                        checkCmd.Parameters.AddWithValue("@ContractNumber", contract);
                        int count = (int)checkCmd.ExecuteScalar();
                        if (count > 0)
                        {
                            MessageBox.Show("Një nga fushat unik (username, email, phone, contract) ekziston tashmë!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    string insertQuery = @"
                        INSERT INTO Userat (
                            Username, Password, Salt, PhoneNumber, ContractNumber, Email,
                            PersonalNumber, Salt_PersonalNumber, BackupCode,
                            DrejtimID, NendrejtimID, GrupID, Photo, DataLindjes, RoleID, Viti, Semestri
                        ) VALUES (
                            @Username, @Password, @Salt, @PhoneNumber, @ContractNumber, @Email,
                            @PersonalNumber, @Salt_PersonalNumber, @BackupCode,
                            @DrejtimID, @NendrejtimID, @GrupID, @Photo, @DataLindjes, 1, 1, 1
                        )";
                    using (SqlCommand cmd = new SqlCommand(insertQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Password", hashedPassword);
                        cmd.Parameters.AddWithValue("@Salt", salt);
                        cmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                        cmd.Parameters.AddWithValue("@ContractNumber", contract);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@PersonalNumber", hashedPersonalNumber);
                        cmd.Parameters.AddWithValue("@Salt_PersonalNumber", saltPN);
                        cmd.Parameters.AddWithValue("@BackupCode", backupCode);
                        cmd.Parameters.AddWithValue("@DrejtimID", drejtimId.Value);
                        cmd.Parameters.AddWithValue("@NendrejtimID", nendrejtimId.Value);
                        cmd.Parameters.AddWithValue("@GrupID", grupId.Value);
                        cmd.Parameters.AddWithValue("@Photo", (object)photoToSave ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@DataLindjes", dateOfBirth);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Studenti u shtua me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearFields();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Gabim gjatë shtimit të studentit: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void perditsoButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(originalUsername))
            {
                MessageBox.Show("Ju lutem zgjidhni një student për ta përditësuar!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string username = textUsername.Text.Trim();
            string password = textPassword.Text.Trim();
            string personalNumber = numriPersonalTextBox.Text.Trim();
            string backupCode = backTextBox.Text.Trim();
            string phoneNumber = textNumber.Text.Trim();
            string email = textEmail.Text.Trim();
            string contract = textContract.Text.Trim();
            DateTime dateOfBirth = shtoDateTimePicker.Value;
            int? drejtimId = drejtimiComboBox.SelectedValue as int?;
            int? nendrejtimId = nendrejtimiComboBox.SelectedValue as int?;
            int? grupId = grupComboBox.SelectedValue as int?;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(phoneNumber) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(contract) ||
                !drejtimId.HasValue || !nendrejtimId.HasValue || !grupId.HasValue)
            {
                MessageBox.Show("Ju lutem plotësoni të gjitha fushat!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!IsValidPhoneNumber(phoneNumber))
            {
                MessageBox.Show("Numri i telefonit duhet të jetë i vlefshëm!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!IsValidEmail(email))
            {
                MessageBox.Show("Email-i duhet të jetë i vlefshëm!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!IsValidDateOfBirth(dateOfBirth))
            {
                MessageBox.Show("Data e lindjes nuk është e vlefshme!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!string.IsNullOrWhiteSpace(backupCode) && !IsValidBackupCode(backupCode))
            {
                MessageBox.Show("Backup Code duhet të jetë saktësisht 6 shifra digjitale nëse plotësohet!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            byte[] photoToSave = null;
            if (selectedImage != null)
            {
                photoToSave = ImageToByteArray(selectedImage); // Foto e re
            }
            else if (existingPhoto != null && existingPhoto.Length > 0)
            {
                photoToSave = existingPhoto; // Ruaj ekzistuesen direkt
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    string checkUniqueQuery = @"
                        SELECT COUNT(*) FROM Userat 
                        WHERE (Username = @Username OR Email = @Email OR PhoneNumber = @PhoneNumber OR ContractNumber = @ContractNumber)
                        AND Username <> @OriginalUsername";
                    using (SqlCommand checkCmd = new SqlCommand(checkUniqueQuery, con))
                    {
                        checkCmd.Parameters.AddWithValue("@Username", username);
                        checkCmd.Parameters.AddWithValue("@Email", email);
                        checkCmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                        checkCmd.Parameters.AddWithValue("@ContractNumber", contract);
                        checkCmd.Parameters.AddWithValue("@OriginalUsername", originalUsername);
                        int count = (int)checkCmd.ExecuteScalar();
                        if (count > 0)
                        {
                            MessageBox.Show("Një nga fushat unik (username, email, phone, contract) ekziston tashmë tek një user tjetër!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    string updateQuery = @"
                        UPDATE Userat SET
                            Username = @Username,
                            PhoneNumber = @PhoneNumber,
                            Email = @Email,
                            ContractNumber = @ContractNumber,
                            DrejtimID = @DrejtimID,
                            NendrejtimID = @NendrejtimID,
                            GrupID = @GrupID,
                            DataLindjes = @DataLindjes,
                            UpdatedAt = GETDATE()";

                    if (!string.IsNullOrWhiteSpace(password))
                    {
                        byte[] salt = GenerateSalt();
                        byte[] hashedPassword = HashValue(password, salt);
                        updateQuery += ", Password = @Password, Salt = @Salt";
                    }

                    if (!string.IsNullOrWhiteSpace(personalNumber) && personalNumber != "••••••")
                    {
                        byte[] saltPN = GenerateSalt();
                        byte[] hashedPersonalNumber = HashValue(personalNumber, saltPN);
                        updateQuery += ", PersonalNumber = @PersonalNumber, Salt_PersonalNumber = @Salt_PersonalNumber";
                    }

                    if (!string.IsNullOrWhiteSpace(backupCode))
                    {
                        updateQuery += ", BackupCode = @BackupCode";
                    }

                    if (photoToSave != null)
                    {
                        updateQuery += ", Photo = @Photo";
                    }

                    updateQuery += " WHERE Username = @OriginalUsername AND RoleID = 1";

                    using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@ContractNumber", contract);
                        cmd.Parameters.AddWithValue("@DrejtimID", drejtimId.Value);
                        cmd.Parameters.AddWithValue("@NendrejtimID", nendrejtimId.Value);
                        cmd.Parameters.AddWithValue("@GrupID", grupId.Value);
                        cmd.Parameters.AddWithValue("@DataLindjes", dateOfBirth);
                        cmd.Parameters.AddWithValue("@OriginalUsername", originalUsername);

                        if (!string.IsNullOrWhiteSpace(password))
                        {
                            byte[] salt = GenerateSalt();
                            byte[] hashedPassword = HashValue(password, salt);
                            cmd.Parameters.AddWithValue("@Password", hashedPassword);
                            cmd.Parameters.AddWithValue("@Salt", salt);
                        }

                        if (!string.IsNullOrWhiteSpace(personalNumber) && personalNumber != "••••••")
                        {
                            byte[] saltPN = GenerateSalt();
                            byte[] hashedPersonalNumber = HashValue(personalNumber, saltPN);
                            cmd.Parameters.AddWithValue("@PersonalNumber", hashedPersonalNumber);
                            cmd.Parameters.AddWithValue("@Salt_PersonalNumber", saltPN);
                        }

                        if (!string.IsNullOrWhiteSpace(backupCode))
                        {
                            cmd.Parameters.AddWithValue("@BackupCode", backupCode);
                        }

                        if (photoToSave != null)
                        {
                            cmd.Parameters.AddWithValue("@Photo", photoToSave);
                        }

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Studenti u përditësua me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearFields();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Nuk u përditësua asnjë student! Sigurohuni që të dhënat janë korrekte.", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Gabim gjatë përditësimit të studentit: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}