namespace ChessProgram
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
            this.button2 = new System.Windows.Forms.Button();
            this.btnUndoMove = new System.Windows.Forms.Button();
            this.btnAddOpeningMove = new System.Windows.Forms.Button();
            this.cbOpeningBookName = new System.Windows.Forms.ComboBox();
            this.rtbMovePoints = new System.Windows.Forms.RichTextBox();
            this.rtbMoveStats = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(663, 314);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(98, 53);
            this.button1.TabIndex = 1;
            this.button1.Text = "New AI Game";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(663, 447);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(98, 53);
            this.button2.TabIndex = 2;
            this.button2.Text = "New Game Against Self";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // btnUndoMove
            // 
            this.btnUndoMove.Location = new System.Drawing.Point(663, 220);
            this.btnUndoMove.Name = "btnUndoMove";
            this.btnUndoMove.Size = new System.Drawing.Size(98, 56);
            this.btnUndoMove.TabIndex = 3;
            this.btnUndoMove.Text = "Undo";
            this.btnUndoMove.UseVisualStyleBackColor = true;
            this.btnUndoMove.Click += new System.EventHandler(this.btnUndoMove_Click);
            // 
            // btnAddOpeningMove
            // 
            this.btnAddOpeningMove.BackColor = System.Drawing.Color.LightGray;
            this.btnAddOpeningMove.Location = new System.Drawing.Point(663, 48);
            this.btnAddOpeningMove.Name = "btnAddOpeningMove";
            this.btnAddOpeningMove.Size = new System.Drawing.Size(98, 47);
            this.btnAddOpeningMove.TabIndex = 4;
            this.btnAddOpeningMove.Text = "Add Opening Move";
            this.btnAddOpeningMove.UseVisualStyleBackColor = false;
            this.btnAddOpeningMove.Click += new System.EventHandler(this.btnAddOpeningMove_Click);
            // 
            // cbOpeningBookName
            // 
            this.cbOpeningBookName.FormattingEnabled = true;
            this.cbOpeningBookName.Items.AddRange(new object[] {
            "Damiano Defence"});
            this.cbOpeningBookName.Location = new System.Drawing.Point(663, 12);
            this.cbOpeningBookName.Name = "cbOpeningBookName";
            this.cbOpeningBookName.Size = new System.Drawing.Size(98, 21);
            this.cbOpeningBookName.TabIndex = 5;
            this.cbOpeningBookName.Text = "Italian-Koltanowski Gambit";
            // 
            // rtbMovePoints
            // 
            this.rtbMovePoints.Location = new System.Drawing.Point(663, 521);
            this.rtbMovePoints.Name = "rtbMovePoints";
            this.rtbMovePoints.ReadOnly = true;
            this.rtbMovePoints.Size = new System.Drawing.Size(97, 145);
            this.rtbMovePoints.TabIndex = 6;
            this.rtbMovePoints.Text = "";
            // 
            // rtbMoveStats
            // 
            this.rtbMoveStats.Location = new System.Drawing.Point(663, 101);
            this.rtbMoveStats.Name = "rtbMoveStats";
            this.rtbMoveStats.ReadOnly = true;
            this.rtbMoveStats.Size = new System.Drawing.Size(97, 113);
            this.rtbMoveStats.TabIndex = 7;
            this.rtbMoveStats.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(773, 678);
            this.Controls.Add(this.rtbMoveStats);
            this.Controls.Add(this.rtbMovePoints);
            this.Controls.Add(this.cbOpeningBookName);
            this.Controls.Add(this.btnAddOpeningMove);
            this.Controls.Add(this.btnUndoMove);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseClick);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button btnUndoMove;
        private System.Windows.Forms.Button btnAddOpeningMove;
        private System.Windows.Forms.ComboBox cbOpeningBookName;
        private System.Windows.Forms.RichTextBox rtbMovePoints;
        private System.Windows.Forms.RichTextBox rtbMoveStats;
    }
}

