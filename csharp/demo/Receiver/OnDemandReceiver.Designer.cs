namespace Receiver
{
  partial class OnDemandReceiver
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
      this.stop = new System.Windows.Forms.Button();
      this.start = new System.Windows.Forms.Button();
      this.status = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // stop
      // 
      this.stop.Enabled = false;
      this.stop.Location = new System.Drawing.Point(94, 12);
      this.stop.Name = "stop";
      this.stop.Size = new System.Drawing.Size(75, 23);
      this.stop.TabIndex = 4;
      this.stop.Text = "Stop";
      this.stop.UseVisualStyleBackColor = true;
      // 
      // start
      // 
      this.start.Location = new System.Drawing.Point(13, 12);
      this.start.Name = "start";
      this.start.Size = new System.Drawing.Size(75, 23);
      this.start.TabIndex = 3;
      this.start.Text = "Start";
      this.start.UseVisualStyleBackColor = true;
      // 
      // status
      // 
      this.status.AutoSize = true;
      this.status.Location = new System.Drawing.Point(10, 53);
      this.status.Name = "status";
      this.status.Size = new System.Drawing.Size(87, 13);
      this.status.TabIndex = 6;
      this.status.Text = "status label";
      // 
      // OnDemandReceiver
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(180, 75);
      this.Controls.Add(this.status);
      this.Controls.Add(this.stop);
      this.Controls.Add(this.start);
      this.Name = "OnDemandReceiver";
      this.Text = "OnDemandReceiver";
      this.Load += new System.EventHandler(this.OnDemandReceiver_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button stop;
    private System.Windows.Forms.Button start;
    private System.Windows.Forms.Label status;

  }
}

