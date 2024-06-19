namespace Discord.cs
{
    partial class DiscordNetLog
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
            richTextBox1 = new RichTextBox();
            SuspendLayout();
            // 
            // richTextBox1
            // 
            richTextBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            richTextBox1.Location = new Point(3, 3);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(694, 494);
            richTextBox1.TabIndex = 0;
            richTextBox1.Text = "";
            // 
            // DiscordNetLog
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(700, 500);
            WindowState = FormWindowState.Minimized; // Minimize the window on startup
            Controls.Add(richTextBox1);
            Name = "DiscordNetLog";
            Text = "Discord.cs - Logs";
            ResumeLayout(false);
        }

        #endregion

        public RichTextBox richTextBox1;
    }
}