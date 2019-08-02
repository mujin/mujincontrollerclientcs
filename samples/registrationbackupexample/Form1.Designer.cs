namespace RegistrationBackup
{
    partial class Form1
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
            this.button1 = new System.Windows.Forms.Button();
            this.helloWorldLabel = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.username_input = new System.Windows.Forms.TextBox();
            this.controllerusername = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.password_input = new System.Windows.Forms.TextBox();
            this.ftpusername_input = new System.Windows.Forms.TextBox();
            this.ftppassword_input = new System.Windows.Forms.TextBox();
            this.ftphost_input = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.controllerurl_input = new System.Windows.Forms.TextBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.ftpremotePath_input = new System.Windows.Forms.TextBox();
            this.ftpport_input = new System.Windows.Forms.TextBox();
            this.radioButton1_backup = new System.Windows.Forms.RadioButton();
            this.radioButton2_syncmasterfile = new System.Windows.Forms.RadioButton();
            this.label7 = new System.Windows.Forms.Label();
            this.masterFilePath_input = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(280, 387);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(97, 28);
            this.button1.TabIndex = 2;
            this.button1.Text = "start";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // helloWorldLabel
            // 
            this.helloWorldLabel.AutoSize = true;
            this.helloWorldLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.helloWorldLabel.Location = new System.Drawing.Point(163, 31);
            this.helloWorldLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.helloWorldLabel.Name = "helloWorldLabel";
            this.helloWorldLabel.Size = new System.Drawing.Size(436, 26);
            this.helloWorldLabel.TabIndex = 3;
            this.helloWorldLabel.Text = "Mujin Registration Backup Sample Program";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(100, 439);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(459, 23);
            this.progressBar1.TabIndex = 5;
            this.progressBar1.Click += new System.EventHandler(this.ProgressBar1_Click);
            // 
            // username_input
            // 
            this.username_input.Location = new System.Drawing.Point(178, 156);
            this.username_input.Name = "username_input";
            this.username_input.Size = new System.Drawing.Size(137, 20);
            this.username_input.TabIndex = 6;
            this.username_input.TextChanged += new System.EventHandler(this.TextBox1_TextChanged);
            // 
            // controllerusername
            // 
            this.controllerusername.AutoSize = true;
            this.controllerusername.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.controllerusername.Location = new System.Drawing.Point(3, 152);
            this.controllerusername.Name = "controllerusername";
            this.controllerusername.Size = new System.Drawing.Size(173, 24);
            this.controllerusername.TabIndex = 7;
            this.controllerusername.Text = "controllerusername";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(7, 190);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(169, 24);
            this.label1.TabIndex = 8;
            this.label1.Text = "controllerpassword";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(330, 226);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(136, 24);
            this.label2.TabIndex = 9;
            this.label2.Text = "FTP username";
            this.label2.Click += new System.EventHandler(this.Label2_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(333, 266);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(133, 24);
            this.label3.TabIndex = 10;
            this.label3.Text = "FTP Password";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(376, 110);
            this.label4.Name = "label4";
            this.label4.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.label4.Size = new System.Drawing.Size(89, 24);
            this.label4.TabIndex = 11;
            this.label4.Text = "FTP Host";
            this.label4.Click += new System.EventHandler(this.Label4_Click);
            // 
            // password_input
            // 
            this.password_input.Location = new System.Drawing.Point(178, 194);
            this.password_input.Name = "password_input";
            this.password_input.Size = new System.Drawing.Size(137, 20);
            this.password_input.TabIndex = 12;
            this.password_input.TextChanged += new System.EventHandler(this.Password_input_TextChanged);
            // 
            // ftpusername_input
            // 
            this.ftpusername_input.Location = new System.Drawing.Point(476, 231);
            this.ftpusername_input.Name = "ftpusername_input";
            this.ftpusername_input.Size = new System.Drawing.Size(137, 20);
            this.ftpusername_input.TabIndex = 13;
            this.ftpusername_input.TextChanged += new System.EventHandler(this.Ftpusername_TextChanged);
            // 
            // ftppassword_input
            // 
            this.ftppassword_input.Location = new System.Drawing.Point(476, 271);
            this.ftppassword_input.Name = "ftppassword_input";
            this.ftppassword_input.Size = new System.Drawing.Size(137, 20);
            this.ftppassword_input.TabIndex = 14;
            // 
            // ftphost_input
            // 
            this.ftphost_input.Location = new System.Drawing.Point(476, 114);
            this.ftphost_input.Name = "ftphost_input";
            this.ftphost_input.Size = new System.Drawing.Size(137, 20);
            this.ftphost_input.TabIndex = 15;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(67, 114);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(109, 24);
            this.label5.TabIndex = 16;
            this.label5.Text = "controllerurl";
            // 
            // controllerurl_input
            // 
            this.controllerurl_input.Location = new System.Drawing.Point(178, 118);
            this.controllerurl_input.Name = "controllerurl_input";
            this.controllerurl_input.Size = new System.Drawing.Size(137, 20);
            this.controllerurl_input.TabIndex = 17;
            this.controllerurl_input.Text = "http://";
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(619, 118);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(224, 344);
            this.richTextBox1.TabIndex = 18;
            this.richTextBox1.Text = "";
            this.richTextBox1.TextChanged += new System.EventHandler(this.RichTextBox1_TextChanged);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(619, 86);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 19;
            this.textBox1.Text = "Task Result";
            this.textBox1.TextChanged += new System.EventHandler(this.TextBox1_TextChanged_1);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(316, 190);
            this.label6.Name = "label6";
            this.label6.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.label6.Size = new System.Drawing.Size(154, 24);
            this.label6.TabIndex = 20;
            this.label6.Text = "FTP RemotePath";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(376, 152);
            this.label14.Name = "label14";
            this.label14.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.label14.Size = new System.Drawing.Size(84, 24);
            this.label14.TabIndex = 21;
            this.label14.Text = "FTP Port";
            // 
            // ftpremotePath_input
            // 
            this.ftpremotePath_input.Location = new System.Drawing.Point(476, 189);
            this.ftpremotePath_input.Name = "ftpremotePath_input";
            this.ftpremotePath_input.Size = new System.Drawing.Size(137, 20);
            this.ftpremotePath_input.TabIndex = 22;
            // 
            // ftpport_input
            // 
            this.ftpport_input.Location = new System.Drawing.Point(476, 152);
            this.ftpport_input.Name = "ftpport_input";
            this.ftpport_input.Size = new System.Drawing.Size(137, 20);
            this.ftpport_input.TabIndex = 23;
            this.ftpport_input.Text = "21";
            // 
            // radioButton1_backup
            // 
            this.radioButton1_backup.AutoSize = true;
            this.radioButton1_backup.Checked = true;
            this.radioButton1_backup.Location = new System.Drawing.Point(80, 316);
            this.radioButton1_backup.Name = "radioButton1_backup";
            this.radioButton1_backup.Size = new System.Drawing.Size(62, 17);
            this.radioButton1_backup.TabIndex = 24;
            this.radioButton1_backup.TabStop = true;
            this.radioButton1_backup.Text = "Backup";
            this.radioButton1_backup.UseVisualStyleBackColor = true;
            this.radioButton1_backup.CheckedChanged += new System.EventHandler(this.RadioButton1_CheckedChanged);
            // 
            // radioButton2_syncmasterfile
            // 
            this.radioButton2_syncmasterfile.AutoSize = true;
            this.radioButton2_syncmasterfile.Location = new System.Drawing.Point(80, 351);
            this.radioButton2_syncmasterfile.Name = "radioButton2_syncmasterfile";
            this.radioButton2_syncmasterfile.Size = new System.Drawing.Size(100, 17);
            this.radioButton2_syncmasterfile.TabIndex = 25;
            this.radioButton2_syncmasterfile.Text = "Sync MasterFile";
            this.radioButton2_syncmasterfile.UseVisualStyleBackColor = true;
            this.radioButton2_syncmasterfile.CheckedChanged += new System.EventHandler(this.RadioButton2_CheckedChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(38, 231);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(134, 24);
            this.label7.TabIndex = 26;
            this.label7.Text = "masterFilePath";
            this.label7.Click += new System.EventHandler(this.Label7_Click);
            // 
            // masterFilePath_input
            // 
            this.masterFilePath_input.Location = new System.Drawing.Point(178, 235);
            this.masterFilePath_input.Name = "masterFilePath_input";
            this.masterFilePath_input.Size = new System.Drawing.Size(137, 20);
            this.masterFilePath_input.TabIndex = 27;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(855, 517);
            this.Controls.Add(this.masterFilePath_input);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.radioButton2_syncmasterfile);
            this.Controls.Add(this.radioButton1_backup);
            this.Controls.Add(this.ftpport_input);
            this.Controls.Add(this.ftpremotePath_input);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.controllerurl_input);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.ftphost_input);
            this.Controls.Add(this.ftppassword_input);
            this.Controls.Add(this.ftpusername_input);
            this.Controls.Add(this.password_input);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.controllerusername);
            this.Controls.Add(this.username_input);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.helloWorldLabel);
            this.Controls.Add(this.button1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "Mujin Registration Backup Sample";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label helloWorldLabel;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.TextBox username_input;
        private System.Windows.Forms.Label controllerusername;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox password_input;
        private System.Windows.Forms.TextBox ftpusername_input;
        private System.Windows.Forms.TextBox ftppassword_input;
        private System.Windows.Forms.TextBox ftphost_input;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox controllerurl_input;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox ftpremotePath_input;
        private System.Windows.Forms.TextBox ftpport_input;
        private System.Windows.Forms.RadioButton radioButton1_backup;
        private System.Windows.Forms.RadioButton radioButton2_syncmasterfile;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox masterFilePath_input;
    }
}

