using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace illy
{
    public partial class ShtoProfessor : Form
    {
        private string connectionString =
            "Server=localhost\\SQLEXPRESS;Database=Projekti;Integrated Security=True;MultipleActiveResultSets=True;";

        private Image selectedImage;
        private byte[] existingPhoto;
        private string originalUsername;

        // Konstruktor për SHTIM të ri
        public ShtoProfessor()
        {
            InitializeComponent();
            LoadDrejtimet();
            LoadNendrejtimet();
            insertPictureBox.Click += InsertPictureBox_Click;
            shtoDateTimePicker.Value = DateTime.Today.AddYears(-30);
            shtoDateTimePicker.MaxDate = DateTime.Today.AddYears(-25);
            shtoDateTimePicker.MinDate = DateTime.Today.AddYears(-80);
            shtoButton.Visible = true;
            perditsoButton.Visible = false;
        }

        // Konstruktor për EDITIM (vetëm me emër – të dhënat ngarkohen nga DB)
        public ShtoProfessor(string usernameForEdit)
        {
            InitializeComponent();
            LoadDrejtimet();
            LoadNendrejtimet();

            originalUsername = usernameForEdit;

            // Ngarko të dhënat nga DB
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"
                        SELECT PhoneNumber, Email, ContractNumber, DataLindjes, DrejtimID, NendrejtimID,
                               Photo, BackupCode, PersonalNumber
                        FROM Userat 
                        WHERE Username = @Username AND RoleID = 2";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Username", usernameForEdit);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                textUsername.Text = usernameForEdit;
                                textNumber.Text = reader["PhoneNumber"]?.ToString() ?? "";
                                textEmail.Text = reader["Email"]?.ToString() ?? "";
                                textContract.Text = reader["ContractNumber"]?.ToString() ?? "";
                                shtoDateTimePicker.Value = reader["DataLindjes"] as DateTime? ?? DateTime.Today.AddYears(-30);
                                backTextBox.Text = reader["BackupCode"]?.ToString() ?? "";
                                numriPersonalTextBox.Text = reader["PersonalNumber"] != DBNull.Value ? "••••••••••" : "";

                                existingPhoto = reader["Photo"] as byte[];
                                if (existingPhoto != null && existingPhoto.Length > 0)
                                {
                                    using (MemoryStream ms = new MemoryStream(existingPhoto))
                                    {
                                        selectedImage = Image.FromStream(ms);
                                        previewPictureBox.Image = selectedImage;
                                    }
                                }

                                int? drejtimId = reader["DrejtimID"] as int?;
                                int? nendrejtimId = reader["NendrejtimID"] as int?;

                                if (drejtimId.HasValue)
                                {
                                    drejtimiComboBox.SelectedValue = drejtimId.Value;
                                    DrejtimiComboBox_SelectedIndexChanged(null, null);
                                }

                                if (nendrejtimId.HasValue)
                                    nendrejtimiComboBox.SelectedValue = nendrejtimId.Value;
                            }
                            else
                            {
                                MessageBox.Show("Profesor nuk u gjet!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                this.Close();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim ngarkimi: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }

            shtoButton.Visible = false;
            perditsoButton.Visible = true;
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
                if (drejtimiComboBox.Items.Count > 0) drejtimiComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim drejtimet: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadNendrejtimet()
        {
            drejtimiComboBox.SelectedIndexChanged -= DrejtimiComboBox_SelectedIndexChanged;
            drejtimiComboBox.SelectedIndexChanged += DrejtimiComboBox_SelectedIndexChanged;

            if (drejtimiComboBox.SelectedValue != null)
                DrejtimiComboBox_SelectedIndexChanged(null, null);
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
                    if (nendrejtimiComboBox.Items.Count > 0) nendrejtimiComboBox.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Gabim nëndrejtimet: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
                    catch { return null; }
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
            return dateOfBirth <= today && age >= 25;
        }

        private bool IsValidBackupCode(string backupCode)
        {
            return !string.IsNullOrWhiteSpace(backupCode) && backupCode.Length == 6 && backupCode.All(char.IsDigit);
        }

        // Vetëm një metodë ClearFields (kjo e rregullon gabimin "ambiguous" dhe "already defines")
        private void ClearFields()
        {
            textUsername.Text = "";
            textPassword.Text = "";
            numriPersonalTextBox.Text = "";
            backTextBox.Text = "";
            textNumber.Text = "";
            textEmail.Text = "";
            textContract.Text = "";
            shtoDateTimePicker.Value = DateTime.Today.AddYears(-30);
            previewPictureBox.Image = null;
            selectedImage = null;
            existingPhoto = null;
            if (drejtimiComboBox.Items.Count > 0) drejtimiComboBox.SelectedIndex = 0;
            if (nendrejtimiComboBox.Items.Count > 0) nendrejtimiComboBox.SelectedIndex = 0;
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

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(personalNumber) || string.IsNullOrWhiteSpace(backupCode) ||
                string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(contract) || !drejtimId.HasValue || !nendrejtimId.HasValue)
            {
                MessageBox.Show("Plotësoni të gjitha fushat!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!IsValidPhoneNumber(phoneNumber)) { MessageBox.Show("Numri telefonit i pavlefshëm!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            if (!IsValidEmail(email)) { MessageBox.Show("Email i pavlefshëm!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            if (!IsValidDateOfBirth(dateOfBirth)) { MessageBox.Show("Data lindjes e pavlefshme!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            if (!IsValidBackupCode(backupCode)) { MessageBox.Show("Backup Code 6 shifra!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

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

                    string checkQuery = @"SELECT COUNT(*) FROM Userat WHERE Username = @Username OR Email = @Email OR PhoneNumber = @Phone OR ContractNumber = @Contract";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                    {
                        checkCmd.Parameters.AddWithValue("@Username", username);
                        checkCmd.Parameters.AddWithValue("@Email", email);
                        checkCmd.Parameters.AddWithValue("@Phone", phoneNumber);
                        checkCmd.Parameters.AddWithValue("@Contract", contract);
                        if ((int)checkCmd.ExecuteScalar() > 0)
                        {
                            MessageBox.Show("Një fushë unik ekziston tashmë!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    string insertQuery = @"
                        INSERT INTO Userat (Username, Password, Salt, PhoneNumber, ContractNumber, Email,
                                            PersonalNumber, Salt_PersonalNumber, BackupCode,
                                            DrejtimID, NendrejtimID, Photo, DataLindjes, RoleID)
                        VALUES (@Username, @Password, @Salt, @Phone, @Contract, @Email,
                                @Personal, @SaltPN, @Backup, @Drejtim, @Nendrejtim, @Photo, @Data, 2)";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Password", hashedPassword);
                        cmd.Parameters.AddWithValue("@Salt", salt);
                        cmd.Parameters.AddWithValue("@Phone", phoneNumber);
                        cmd.Parameters.AddWithValue("@Contract", contract);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Personal", hashedPersonalNumber);
                        cmd.Parameters.AddWithValue("@SaltPN", saltPN);
                        cmd.Parameters.AddWithValue("@Backup", backupCode);
                        cmd.Parameters.AddWithValue("@Drejtim", drejtimId.Value);
                        cmd.Parameters.AddWithValue("@Nendrejtim", nendrejtimId.Value);
                        cmd.Parameters.AddWithValue("@Photo", (object)photoToSave ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Data", dateOfBirth);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Profesor u shtua me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearFields();
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Gabim shtimi: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void perditsoButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(originalUsername))
            {
                MessageBox.Show("Asnjë profesor i zgjedhur!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(phoneNumber) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(contract) ||
                !drejtimId.HasValue || !nendrejtimId.HasValue)
            {
                MessageBox.Show("Plotësoni fushat e detyrueshme!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!IsValidPhoneNumber(phoneNumber)) { MessageBox.Show("Numri telefonit i pavlefshëm!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            if (!IsValidEmail(email)) { MessageBox.Show("Email i pavlefshëm!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            if (!IsValidDateOfBirth(dateOfBirth)) { MessageBox.Show("Data lindjes e pavlefshme!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            if (!string.IsNullOrWhiteSpace(backupCode) && !IsValidBackupCode(backupCode)) { MessageBox.Show("Backup Code 6 shifra!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

            byte[] photoToSave = null;
            if (selectedImage != null)
            {
                photoToSave = ImageToByteArray(selectedImage);
            }
            else if (existingPhoto != null && existingPhoto.Length > 0)
            {
                photoToSave = existingPhoto;
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    string checkQuery = @"
                        SELECT COUNT(*) FROM Userat
                        WHERE (Username = @Username OR Email = @Email OR PhoneNumber = @Phone OR ContractNumber = @Contract)
                        AND Username <> @OriginalUsername";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                    {
                        checkCmd.Parameters.AddWithValue("@Username", username);
                        checkCmd.Parameters.AddWithValue("@Email", email);
                        checkCmd.Parameters.AddWithValue("@Phone", phoneNumber);
                        checkCmd.Parameters.AddWithValue("@Contract", contract);
                        checkCmd.Parameters.AddWithValue("@OriginalUsername", originalUsername);
                        if ((int)checkCmd.ExecuteScalar() > 0)
                        {
                            MessageBox.Show("Fushë unik ekziston tashmë!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                            DataLindjes = @DataLindjes,
                            UpdatedAt = GETDATE()";

                    if (!string.IsNullOrWhiteSpace(password))
                    {
                        byte[] salt = GenerateSalt();
                        byte[] hashedPassword = HashValue(password, salt);
                        updateQuery += ", Password = @Password, Salt = @Salt";
                    }

                    if (!string.IsNullOrWhiteSpace(personalNumber) && personalNumber != "••••••••••")
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

                    updateQuery += " WHERE Username = @OriginalUsername AND RoleID = 2";

                    using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@ContractNumber", contract);
                        cmd.Parameters.AddWithValue("@DrejtimID", drejtimId.Value);
                        cmd.Parameters.AddWithValue("@NendrejtimID", nendrejtimId.Value);
                        cmd.Parameters.AddWithValue("@DataLindjes", dateOfBirth);
                        cmd.Parameters.AddWithValue("@OriginalUsername", originalUsername);

                        if (!string.IsNullOrWhiteSpace(password))
                        {
                            byte[] salt = GenerateSalt();
                            byte[] hashedPassword = HashValue(password, salt);
                            cmd.Parameters.AddWithValue("@Password", hashedPassword);
                            cmd.Parameters.AddWithValue("@Salt", salt);
                        }

                        if (!string.IsNullOrWhiteSpace(personalNumber) && personalNumber != "••••••••••")
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
                            MessageBox.Show("Profesor u përditësua me sukses!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearFields();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Nuk u përditësua asnjë profesor!", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Gabim përditësimi: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}