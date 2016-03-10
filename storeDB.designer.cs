namespace Iris_Matching_System
{
    partial class storeDB
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
            this.label1 = new System.Windows.Forms.Label();
            this.stroeDB = new System.Windows.Forms.Button();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.browseDB = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "DB path";
            // 
            // stroeDB
            // 
            this.stroeDB.Location = new System.Drawing.Point(12, 70);
            this.stroeDB.Name = "stroeDB";
            this.stroeDB.Size = new System.Drawing.Size(75, 23);
            this.stroeDB.TabIndex = 4;
            this.stroeDB.Text = "store DB";
            this.stroeDB.UseVisualStyleBackColor = true;
            this.stroeDB.Click += new System.EventHandler(this.stroeDB_Click);
            // 
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(74, 13);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(293, 20);
            this.txtPath.TabIndex = 3;
            this.txtPath.Text = "C:\\Documents and Settings\\uchiha sasuke\\Desktop\\072\\project\\resources\\casia\\CASIA" +
                "-IrisV3\\CASIA-IrisV3-Interval";
            // 
            // browseDB
            // 
            this.browseDB.Location = new System.Drawing.Point(384, 12);
            this.browseDB.Name = "browseDB";
            this.browseDB.Size = new System.Drawing.Size(75, 23);
            this.browseDB.TabIndex = 6;
            this.browseDB.Text = "browse";
            this.browseDB.UseVisualStyleBackColor = true;
            this.browseDB.Click += new System.EventHandler(this.browseDB_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(71, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(233, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "locate the path of  \"CASIA-IrisV3-Interval\" folder";
            // 
            // storeDB
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(482, 115);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.browseDB);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.stroeDB);
            this.Controls.Add(this.txtPath);
            this.Name = "storeDB";
            this.Text = "storeDB";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button stroeDB;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Button browseDB;
        private System.Windows.Forms.Label label2;
    }
}