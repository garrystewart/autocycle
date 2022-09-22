namespace AutoCycle_Editor
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewImageColumn();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.captureVideoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.openCaptureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveCaptureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.startCycleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmdUsePrevious = new System.Windows.Forms.Button();
            this.cmdUseNext = new System.Windows.Forms.Button();
            this.cmdMakeNegative = new System.Windows.Forms.Button();
            this.cmdMakePositive = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2,
            this.Column3,
            this.Column4});
            this.dataGridView1.Location = new System.Drawing.Point(12, 27);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 25;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(620, 411);
            this.dataGridView1.TabIndex = 1;
            // 
            // Column1
            // 
            this.Column1.HeaderText = "Count";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            // 
            // Column2
            // 
            this.Column2.HeaderText = "Result";
            this.Column2.Name = "Column2";
            // 
            // Column3
            // 
            this.Column3.HeaderText = "Confidence";
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            // 
            // Column4
            // 
            this.Column4.HeaderText = "Image";
            this.Column4.Name = "Column4";
            this.Column4.ReadOnly = true;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 24);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.captureVideoToolStripMenuItem,
            this.toolStripSeparator1,
            this.openCaptureToolStripMenuItem,
            this.saveCaptureToolStripMenuItem,
            this.toolStripSeparator2,
            this.startCycleToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // captureVideoToolStripMenuItem
            // 
            this.captureVideoToolStripMenuItem.Name = "captureVideoToolStripMenuItem";
            this.captureVideoToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.captureVideoToolStripMenuItem.Text = "Capture Video";
            this.captureVideoToolStripMenuItem.Click += new System.EventHandler(this.captureVideoToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(146, 6);
            // 
            // openCaptureToolStripMenuItem
            // 
            this.openCaptureToolStripMenuItem.Name = "openCaptureToolStripMenuItem";
            this.openCaptureToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.openCaptureToolStripMenuItem.Text = "Open Capture";
            this.openCaptureToolStripMenuItem.Click += new System.EventHandler(this.openCaptureToolStripMenuItem_Click);
            // 
            // saveCaptureToolStripMenuItem
            // 
            this.saveCaptureToolStripMenuItem.Name = "saveCaptureToolStripMenuItem";
            this.saveCaptureToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.saveCaptureToolStripMenuItem.Text = "Save Capture";
            this.saveCaptureToolStripMenuItem.Click += new System.EventHandler(this.saveCaptureToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(146, 6);
            // 
            // startCycleToolStripMenuItem
            // 
            this.startCycleToolStripMenuItem.Name = "startCycleToolStripMenuItem";
            this.startCycleToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.startCycleToolStripMenuItem.Text = "Start Cycle";
            this.startCycleToolStripMenuItem.Click += new System.EventHandler(this.startCycleToolStripMenuItem_Click);
            // 
            // cmdUsePrevious
            // 
            this.cmdUsePrevious.Location = new System.Drawing.Point(638, 74);
            this.cmdUsePrevious.Name = "cmdUsePrevious";
            this.cmdUsePrevious.Size = new System.Drawing.Size(150, 23);
            this.cmdUsePrevious.TabIndex = 5;
            this.cmdUsePrevious.Text = "Use Previous";
            this.cmdUsePrevious.UseVisualStyleBackColor = true;
            this.cmdUsePrevious.Click += new System.EventHandler(this.cmdUsePrevious_Click);
            // 
            // cmdUseNext
            // 
            this.cmdUseNext.Location = new System.Drawing.Point(638, 103);
            this.cmdUseNext.Name = "cmdUseNext";
            this.cmdUseNext.Size = new System.Drawing.Size(150, 23);
            this.cmdUseNext.TabIndex = 6;
            this.cmdUseNext.Text = "Use Next";
            this.cmdUseNext.UseVisualStyleBackColor = true;
            this.cmdUseNext.Click += new System.EventHandler(this.cmdUseNext_Click);
            // 
            // cmdMakeNegative
            // 
            this.cmdMakeNegative.Location = new System.Drawing.Point(638, 132);
            this.cmdMakeNegative.Name = "cmdMakeNegative";
            this.cmdMakeNegative.Size = new System.Drawing.Size(150, 23);
            this.cmdMakeNegative.TabIndex = 7;
            this.cmdMakeNegative.Text = "Make Negative";
            this.cmdMakeNegative.UseVisualStyleBackColor = true;
            this.cmdMakeNegative.Click += new System.EventHandler(this.cmdMakeNegative_Click);
            // 
            // cmdMakePositive
            // 
            this.cmdMakePositive.Location = new System.Drawing.Point(638, 161);
            this.cmdMakePositive.Name = "cmdMakePositive";
            this.cmdMakePositive.Size = new System.Drawing.Size(150, 23);
            this.cmdMakePositive.TabIndex = 8;
            this.cmdMakePositive.Text = "Make Positive";
            this.cmdMakePositive.UseVisualStyleBackColor = true;
            this.cmdMakePositive.Click += new System.EventHandler(this.cmdMakePositive_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(638, 45);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(150, 23);
            this.textBox1.TabIndex = 12;
            this.textBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(638, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 15);
            this.label1.TabIndex = 10;
            this.label1.Text = "Manual Edit";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.cmdMakePositive);
            this.Controls.Add(this.cmdMakeNegative);
            this.Controls.Add(this.cmdUseNext);
            this.Controls.Add(this.cmdUsePrevious);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "AutoCycle Editor";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DataGridView dataGridView1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem captureVideoToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem openCaptureToolStripMenuItem;
        private ToolStripMenuItem saveCaptureToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem startCycleToolStripMenuItem;
        private DataGridViewTextBoxColumn Column1;
        private DataGridViewTextBoxColumn Column2;
        private DataGridViewTextBoxColumn Column3;
        private DataGridViewImageColumn Column4;
        private Button cmdUsePrevious;
        private Button cmdUseNext;
        private Button cmdMakeNegative;
        private Button cmdMakePositive;
        private TextBox textBox1;
        private Label label1;
    }
}