namespace Receiver
{
  partial class Receiver
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
      this.output = new System.Windows.Forms.TextBox();
      this.receive = new System.Windows.Forms.Button();
      this.stop = new System.Windows.Forms.Button();
      this.start = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // output
      // 
      this.output.Location = new System.Drawing.Point(13, 13);
      this.output.Multiline = true;
      this.output.Name = "output";
      this.output.ReadOnly = true;
      this.output.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.output.Size = new System.Drawing.Size(269, 93);
      this.output.TabIndex = 0;
      // 
      // receive
      // 
      this.receive.Location = new System.Drawing.Point(450, 11);
      this.receive.Name = "receive";
      this.receive.Size = new System.Drawing.Size(75, 23);
      this.receive.TabIndex = 5;
      this.receive.Text = "Receive";
      this.receive.UseVisualStyleBackColor = true;
      // 
      // stop
      // 
      this.stop.Enabled = false;
      this.stop.Location = new System.Drawing.Point(369, 11);
      this.stop.Name = "stop";
      this.stop.Size = new System.Drawing.Size(75, 23);
      this.stop.TabIndex = 4;
      this.stop.Text = "Stop";
      this.stop.UseVisualStyleBackColor = true;
      // 
      // start
      // 
      this.start.Location = new System.Drawing.Point(288, 11);
      this.start.Name = "start";
      this.start.Size = new System.Drawing.Size(75, 23);
      this.start.TabIndex = 3;
      this.start.Text = "Start";
      this.start.UseVisualStyleBackColor = true;
      // 
      // Receiver
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(534, 118);
      this.Controls.Add(this.receive);
      this.Controls.Add(this.stop);
      this.Controls.Add(this.start);
      this.Controls.Add(this.output);
      this.Name = "Receiver";
      this.Text = "Receiver";
      this.Load += new System.EventHandler(this.Receiver_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox output;
    private System.Windows.Forms.Button receive;
    private System.Windows.Forms.Button stop;
    private System.Windows.Forms.Button start;

  }
}

