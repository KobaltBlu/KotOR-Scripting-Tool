namespace NWN_Script
{
    partial class FindAndReplace
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FindAndReplace));
            this.lbl_FindWhat = new System.Windows.Forms.Label();
            this.lbl_ReplaceWith = new System.Windows.Forms.Label();
            this.textBox_FindWhat = new System.Windows.Forms.TextBox();
            this.textBox_ReplaceWith = new System.Windows.Forms.TextBox();
            this.btn_replace = new System.Windows.Forms.Button();
            this.btn_ReplaceAll = new System.Windows.Forms.Button();
            this.btn_Close = new System.Windows.Forms.Button();
            this.checkMatchCase = new System.Windows.Forms.CheckBox();
            this.checkInSelection = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbl_FindWhat
            // 
            this.lbl_FindWhat.AutoSize = true;
            this.lbl_FindWhat.Location = new System.Drawing.Point(13, 55);
            this.lbl_FindWhat.Name = "lbl_FindWhat";
            this.lbl_FindWhat.Size = new System.Drawing.Size(56, 13);
            this.lbl_FindWhat.TabIndex = 0;
            this.lbl_FindWhat.Text = "Find what:";
            // 
            // lbl_ReplaceWith
            // 
            this.lbl_ReplaceWith.AutoSize = true;
            this.lbl_ReplaceWith.Location = new System.Drawing.Point(13, 95);
            this.lbl_ReplaceWith.Name = "lbl_ReplaceWith";
            this.lbl_ReplaceWith.Size = new System.Drawing.Size(72, 13);
            this.lbl_ReplaceWith.TabIndex = 1;
            this.lbl_ReplaceWith.Text = "Replace with:";
            // 
            // textBox_FindWhat
            // 
            this.textBox_FindWhat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_FindWhat.Location = new System.Drawing.Point(13, 72);
            this.textBox_FindWhat.Name = "textBox_FindWhat";
            this.textBox_FindWhat.Size = new System.Drawing.Size(361, 20);
            this.textBox_FindWhat.TabIndex = 2;
            // 
            // textBox_ReplaceWith
            // 
            this.textBox_ReplaceWith.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_ReplaceWith.Location = new System.Drawing.Point(13, 112);
            this.textBox_ReplaceWith.Name = "textBox_ReplaceWith";
            this.textBox_ReplaceWith.Size = new System.Drawing.Size(361, 20);
            this.textBox_ReplaceWith.TabIndex = 3;
            // 
            // btn_replace
            // 
            this.btn_replace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_replace.Location = new System.Drawing.Point(16, 159);
            this.btn_replace.Name = "btn_replace";
            this.btn_replace.Size = new System.Drawing.Size(75, 23);
            this.btn_replace.TabIndex = 4;
            this.btn_replace.Text = "Replace";
            this.btn_replace.UseVisualStyleBackColor = true;
            this.btn_replace.Click += new System.EventHandler(this.btn_replace_Click);
            // 
            // btn_ReplaceAll
            // 
            this.btn_ReplaceAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_ReplaceAll.Location = new System.Drawing.Point(4, 9);
            this.btn_ReplaceAll.Name = "btn_ReplaceAll";
            this.btn_ReplaceAll.Size = new System.Drawing.Size(75, 23);
            this.btn_ReplaceAll.TabIndex = 5;
            this.btn_ReplaceAll.Text = "Replace All";
            this.btn_ReplaceAll.UseVisualStyleBackColor = true;
            this.btn_ReplaceAll.Click += new System.EventHandler(this.btn_ReplaceAll_Click);
            // 
            // btn_Close
            // 
            this.btn_Close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Close.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_Close.Location = new System.Drawing.Point(299, 159);
            this.btn_Close.Name = "btn_Close";
            this.btn_Close.Size = new System.Drawing.Size(75, 23);
            this.btn_Close.TabIndex = 6;
            this.btn_Close.Text = "Close";
            this.btn_Close.UseVisualStyleBackColor = true;
            this.btn_Close.Click += new System.EventHandler(this.btn_Close_Click);
            // 
            // checkMatchCase
            // 
            this.checkMatchCase.AutoSize = true;
            this.checkMatchCase.Location = new System.Drawing.Point(16, 138);
            this.checkMatchCase.Name = "checkMatchCase";
            this.checkMatchCase.Size = new System.Drawing.Size(82, 17);
            this.checkMatchCase.TabIndex = 7;
            this.checkMatchCase.Text = "Match case";
            this.checkMatchCase.UseVisualStyleBackColor = true;
            this.checkMatchCase.CheckedChanged += new System.EventHandler(this.checkMatchCase_CheckedChanged);
            // 
            // checkInSelection
            // 
            this.checkInSelection.AutoSize = true;
            this.checkInSelection.BackColor = System.Drawing.SystemColors.Control;
            this.checkInSelection.Location = new System.Drawing.Point(87, 14);
            this.checkInSelection.Name = "checkInSelection";
            this.checkInSelection.Size = new System.Drawing.Size(80, 17);
            this.checkInSelection.TabIndex = 8;
            this.checkInSelection.Text = "In selection";
            this.checkInSelection.UseVisualStyleBackColor = false;
            this.checkInSelection.CheckedChanged += new System.EventHandler(this.checkInSelection_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.checkInSelection);
            this.groupBox1.Controls.Add(this.btn_ReplaceAll);
            this.groupBox1.Location = new System.Drawing.Point(96, 150);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(172, 36);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            // 
            // FindAndReplace
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_Close;
            this.ClientSize = new System.Drawing.Size(386, 194);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.checkMatchCase);
            this.Controls.Add(this.btn_Close);
            this.Controls.Add(this.btn_replace);
            this.Controls.Add(this.textBox_ReplaceWith);
            this.Controls.Add(this.textBox_FindWhat);
            this.Controls.Add(this.lbl_ReplaceWith);
            this.Controls.Add(this.lbl_FindWhat);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FindAndReplace";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Find and Replace";
            this.Load += new System.EventHandler(this.FindAndReplace_Load);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FindAndReplace_KeyPress);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbl_FindWhat;
        private System.Windows.Forms.Label lbl_ReplaceWith;
        private System.Windows.Forms.TextBox textBox_FindWhat;
        private System.Windows.Forms.TextBox textBox_ReplaceWith;
        private System.Windows.Forms.Button btn_replace;
        private System.Windows.Forms.Button btn_ReplaceAll;
        private System.Windows.Forms.Button btn_Close;
        private System.Windows.Forms.CheckBox checkMatchCase;
        private System.Windows.Forms.CheckBox checkInSelection;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}