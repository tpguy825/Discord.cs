namespace Discord.cs
{
    partial class CodeForm
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
            label1 = new Label();
            textBox1 = new TextBox();
            button1 = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(11, 9);
            label1.Name = "label1";
            label1.Size = new Size(176, 15);
            label1.TabIndex = 0;
            label1.Text = "Please enter your Discord token:";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(12, 27);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(216, 23);
            textBox1.TabIndex = 1;
            // 
            // button1
            // 
            button1.Location = new Point(153, 56);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 2;
            button1.Text = "Submit";
            button1.UseVisualStyleBackColor = true;
            // 
            // CodeForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(240, 86);
            Controls.Add(button1);
            Controls.Add(textBox1);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "CodeForm";
            Text = "Discord.cs";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        public TextBox textBox1;
        public Button button1;
    }
}