namespace illy
{
    partial class TranskriptaNotave
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TranskriptaNotave));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.ShfaqTranskriptenGridView = new Guna.UI2.WinForms.Guna2DataGridView();
            this.kerkoTextBox = new Guna.UI2.WinForms.Guna2TextBox();
            this.ruajePDFButton = new Guna.UI2.WinForms.Guna2Button();
            ((System.ComponentModel.ISupportInitialize)(this.ShfaqTranskriptenGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(48, 55);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(252, 16);
            this.label1.TabIndex = 33;
            this.label1.Text = "Këtu mund të shihni transkriptat e notave...";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Calibri", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(40)))), ((int)(((byte)(41)))));
            this.label2.Location = new System.Drawing.Point(46, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(191, 26);
            this.label2.TabIndex = 32;
            this.label2.Text = "Transkripta e Notave";
            // 
            // ShfaqTranskriptenGridView
            // 
            this.ShfaqTranskriptenGridView.AllowUserToAddRows = false;
            this.ShfaqTranskriptenGridView.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.White;
            this.ShfaqTranskriptenGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(40)))), ((int)(((byte)(41)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(40)))), ((int)(((byte)(41)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.ShfaqTranskriptenGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.ShfaqTranskriptenGridView.ColumnHeadersHeight = 40;
            this.ShfaqTranskriptenGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(69)))), ((int)(((byte)(94)))));
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(229)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(69)))), ((int)(((byte)(94)))));
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.ShfaqTranskriptenGridView.DefaultCellStyle = dataGridViewCellStyle3;
            this.ShfaqTranskriptenGridView.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(229)))), ((int)(((byte)(255)))));
            this.ShfaqTranskriptenGridView.Location = new System.Drawing.Point(12, 144);
            this.ShfaqTranskriptenGridView.Name = "ShfaqTranskriptenGridView";
            this.ShfaqTranskriptenGridView.ReadOnly = true;
            this.ShfaqTranskriptenGridView.RowHeadersVisible = false;
            this.ShfaqTranskriptenGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.White;
            this.ShfaqTranskriptenGridView.RowsDefaultCellStyle = dataGridViewCellStyle4;
            this.ShfaqTranskriptenGridView.RowTemplate.Height = 40;
            this.ShfaqTranskriptenGridView.Size = new System.Drawing.Size(871, 341);
            this.ShfaqTranskriptenGridView.TabIndex = 48;
            this.ShfaqTranskriptenGridView.ThemeStyle.AlternatingRowsStyle.BackColor = System.Drawing.Color.White;
            this.ShfaqTranskriptenGridView.ThemeStyle.AlternatingRowsStyle.Font = null;
            this.ShfaqTranskriptenGridView.ThemeStyle.AlternatingRowsStyle.ForeColor = System.Drawing.Color.Empty;
            this.ShfaqTranskriptenGridView.ThemeStyle.AlternatingRowsStyle.SelectionBackColor = System.Drawing.Color.Empty;
            this.ShfaqTranskriptenGridView.ThemeStyle.AlternatingRowsStyle.SelectionForeColor = System.Drawing.Color.Empty;
            this.ShfaqTranskriptenGridView.ThemeStyle.BackColor = System.Drawing.Color.White;
            this.ShfaqTranskriptenGridView.ThemeStyle.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(229)))), ((int)(((byte)(255)))));
            this.ShfaqTranskriptenGridView.ThemeStyle.HeaderStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(88)))), ((int)(((byte)(255)))));
            this.ShfaqTranskriptenGridView.ThemeStyle.HeaderStyle.BorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.ShfaqTranskriptenGridView.ThemeStyle.HeaderStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ShfaqTranskriptenGridView.ThemeStyle.HeaderStyle.ForeColor = System.Drawing.Color.White;
            this.ShfaqTranskriptenGridView.ThemeStyle.HeaderStyle.HeaightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            this.ShfaqTranskriptenGridView.ThemeStyle.HeaderStyle.Height = 40;
            this.ShfaqTranskriptenGridView.ThemeStyle.ReadOnly = true;
            this.ShfaqTranskriptenGridView.ThemeStyle.RowsStyle.BackColor = System.Drawing.Color.White;
            this.ShfaqTranskriptenGridView.ThemeStyle.RowsStyle.BorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.ShfaqTranskriptenGridView.ThemeStyle.RowsStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ShfaqTranskriptenGridView.ThemeStyle.RowsStyle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(69)))), ((int)(((byte)(94)))));
            this.ShfaqTranskriptenGridView.ThemeStyle.RowsStyle.Height = 40;
            this.ShfaqTranskriptenGridView.ThemeStyle.RowsStyle.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(229)))), ((int)(((byte)(255)))));
            this.ShfaqTranskriptenGridView.ThemeStyle.RowsStyle.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(69)))), ((int)(((byte)(94)))));
            // 
            // kerkoTextBox
            // 
            this.kerkoTextBox.Animated = true;
            this.kerkoTextBox.AutoRoundedCorners = true;
            this.kerkoTextBox.BorderColor = System.Drawing.Color.DodgerBlue;
            this.kerkoTextBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.kerkoTextBox.DefaultText = "";
            this.kerkoTextBox.DisabledState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(208)))), ((int)(((byte)(208)))));
            this.kerkoTextBox.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.kerkoTextBox.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.kerkoTextBox.DisabledState.PlaceholderForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.kerkoTextBox.FocusedState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.kerkoTextBox.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.kerkoTextBox.ForeColor = System.Drawing.Color.Black;
            this.kerkoTextBox.HoverState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.kerkoTextBox.IconLeft = ((System.Drawing.Image)(resources.GetObject("kerkoTextBox.IconLeft")));
            this.kerkoTextBox.Location = new System.Drawing.Point(140, 101);
            this.kerkoTextBox.Name = "kerkoTextBox";
            this.kerkoTextBox.PlaceholderForeColor = System.Drawing.Color.Black;
            this.kerkoTextBox.PlaceholderText = "Kërko sipas kontratës së studentit";
            this.kerkoTextBox.SelectedText = "";
            this.kerkoTextBox.Size = new System.Drawing.Size(594, 37);
            this.kerkoTextBox.TabIndex = 67;
            // 
            // ruajePDFButton
            // 
            this.ruajePDFButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ruajePDFButton.DisabledState.BorderColor = System.Drawing.Color.DarkGray;
            this.ruajePDFButton.DisabledState.CustomBorderColor = System.Drawing.Color.DarkGray;
            this.ruajePDFButton.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(169)))), ((int)(((byte)(169)))), ((int)(((byte)(169)))));
            this.ruajePDFButton.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.ruajePDFButton.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(40)))), ((int)(((byte)(41)))));
            this.ruajePDFButton.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ruajePDFButton.ForeColor = System.Drawing.Color.White;
            this.ruajePDFButton.Image = ((System.Drawing.Image)(resources.GetObject("ruajePDFButton.Image")));
            this.ruajePDFButton.Location = new System.Drawing.Point(377, 497);
            this.ruajePDFButton.Name = "ruajePDFButton";
            this.ruajePDFButton.Size = new System.Drawing.Size(142, 30);
            this.ruajePDFButton.TabIndex = 68;
            this.ruajePDFButton.Text = "Ruaje si PDF";
            this.ruajePDFButton.Click += new System.EventHandler(this.ruajePDFButton_Click);
            // 
            // TranskriptaNotave
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(895, 539);
            this.Controls.Add(this.ruajePDFButton);
            this.Controls.Add(this.kerkoTextBox);
            this.Controls.Add(this.ShfaqTranskriptenGridView);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "TranskriptaNotave";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TranskriptaNotave";
            ((System.ComponentModel.ISupportInitialize)(this.ShfaqTranskriptenGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private Guna.UI2.WinForms.Guna2DataGridView ShfaqTranskriptenGridView;
        private Guna.UI2.WinForms.Guna2TextBox kerkoTextBox;
        private Guna.UI2.WinForms.Guna2Button ruajePDFButton;
    }
}