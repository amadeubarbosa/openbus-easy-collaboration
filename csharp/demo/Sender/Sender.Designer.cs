namespace Sender
{
    partial class Sender
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
      this.start = new System.Windows.Forms.Button();
      this.stop = new System.Windows.Forms.Button();
      this.send = new System.Windows.Forms.Button();
      this.input = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // button1
      // 
      this.start.Location = new System.Drawing.Point(147, 12);
      this.start.Name = "Start";
      this.start.Size = new System.Drawing.Size(75, 23);
      this.start.TabIndex = 0;
      this.start.Text = "Start";
      this.start.UseVisualStyleBackColor = true;
      // 
      // button2
      // 
      this.stop.Enabled = false;
      this.stop.Location = new System.Drawing.Point(228, 12);
      this.stop.Name = "Stop";
      this.stop.Size = new System.Drawing.Size(75, 23);
      this.stop.TabIndex = 1;
      this.stop.Text = "Stop";
      this.stop.UseVisualStyleBackColor = true;
      // 
      // button3
      // 
      this.send.Location = new System.Drawing.Point(309, 12);
      this.send.Name = "Send";
      this.send.Size = new System.Drawing.Size(75, 23);
      this.send.TabIndex = 2;
      this.send.Text = "Send";
      this.send.UseVisualStyleBackColor = true;
      // 
      // textBox1
      // 
      this.input.Location = new System.Drawing.Point(12, 12);
      this.input.Name = "Input";
      this.input.Size = new System.Drawing.Size(129, 20);
      this.input.TabIndex = 3;
      // 
      // Sender
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(396, 50);
      this.Controls.Add(this.input);
      this.Controls.Add(this.send);
      this.Controls.Add(this.stop);
      this.Controls.Add(this.start);
      this.Name = "Sender";
      this.Text = "Sender";
      this.Load += new System.EventHandler(this.Sender_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button start;
        private System.Windows.Forms.Button stop;
        private System.Windows.Forms.Button send;
        private System.Windows.Forms.TextBox input;
    }
}

