using System.Windows.Forms;

namespace chatcatcher
{
    partial class MainForm
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

        // 在 InitializeComponent 方法中初始化控件對象並設置其屬性
        private void InitializeComponent()
        {
            txtChat = new TextBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            btnOpenFile = new Button();
            btn = new Button();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // txtChat
            // 
            txtChat.Location = new Point(3, 31);
            txtChat.Multiline = true;
            txtChat.Name = "txtChat";
            txtChat.ReadOnly = true;
            txtChat.ScrollBars = ScrollBars.Vertical;
            txtChat.Size = new Size(500, 20);
            txtChat.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(btnOpenFile, 0, 0);
            tableLayoutPanel1.Controls.Add(btn, 0, 2);
            tableLayoutPanel1.Controls.Add(txtChat, 0, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(4, 3, 4, 3);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 80F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            tableLayoutPanel1.Size = new Size(569, 287);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // btnOpenFile
            // 
            btnOpenFile.Location = new Point(3, 3);
            btnOpenFile.Name = "btnOpenFile";
            btnOpenFile.Size = new Size(102, 22);
            btnOpenFile.TabIndex = 3;
            btnOpenFile.Text = "選擇連接參數";
            btnOpenFile.UseVisualStyleBackColor = true;
            btnOpenFile.Click += btnOpenFile_Click;
            // 
            // btn
            // 
            btn.Location = new Point(3, 260);
            btn.Name = "btn";
            btn.Size = new Size(50, 21);
            btn.TabIndex = 2;
            btn.Text = "開始連結";
            btn.UseVisualStyleBackColor = true;
            btn.Click += btn_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(569, 287);
            Controls.Add(tableLayoutPanel1);
            Margin = new Padding(4, 3, 4, 3);
            Name = "MainForm";
            Text = "Chat Catcher";
            Load += MainForm_Load;
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private Button btn;
        private TextBox txtChat;
        private Button btnOpenFile;
    }
}