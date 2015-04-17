using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

public class Preferences : System.Windows.Forms.Form
{
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox maxJobs;
    private System.Windows.Forms.StatusBar statusBar1;

    public Preferences()
    {
        Init();

    }


    private void Init()
    {
        System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Preferences));
        this.button1 = new System.Windows.Forms.Button();
        this.button2 = new System.Windows.Forms.Button();
        this.label1 = new System.Windows.Forms.Label();
        this.maxJobs = new System.Windows.Forms.TextBox();
        this.statusBar1 = new System.Windows.Forms.StatusBar();
        this.SuspendLayout();
        // 
        // button1
        // 
        this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.button1.Location = new System.Drawing.Point(240, 208);
        this.button1.Name = "button1";
        this.button1.TabIndex = 0;
        this.button1.Text = "OK";
        // 
        // button2
        // 
        this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.button2.Location = new System.Drawing.Point(320, 208);
        this.button2.Name = "button2";
        this.button2.TabIndex = 1;
        this.button2.Text = "Cancel";
        // 
        // label1
        // 
        this.label1.Location = new System.Drawing.Point(8, 8);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(208, 23);
        this.label1.TabIndex = 2;
        this.label1.Text = "Maximum number of concurrent jobs:";
        this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // maxJobs
        // 
        this.maxJobs.Location = new System.Drawing.Point(320, 8);
        this.maxJobs.Name = "maxJobs";
        this.maxJobs.Size = new System.Drawing.Size(64, 20);
        this.maxJobs.TabIndex = 3;
        this.maxJobs.Text = "2";
        this.maxJobs.TextChanged += new System.EventHandler(this.maxJobs_TextChanged);
        // 
        // statusBar1
        // 
        this.statusBar1.Location = new System.Drawing.Point(0, 244);
        this.statusBar1.Name = "statusBar1";
        this.statusBar1.Size = new System.Drawing.Size(398, 22);
        this.statusBar1.SizingGrip = false;
        this.statusBar1.TabIndex = 4;
        // 
        // Preferences
        // 
        this.AcceptButton = this.button1;
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.CancelButton = this.button2;
        this.ClientSize = new System.Drawing.Size(398, 266);
        this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.statusBar1,
																		  this.maxJobs,
																		  this.label1,
																		  this.button2,
																		  this.button1});
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "Preferences";
        this.ShowInTaskbar = false;
        this.Text = "Preferences";
        this.ResumeLayout(false);

    }

    private bool OK()
    {
        bool OK = true;
        int maxnJobs;
        try
        {
            maxnJobs = int.Parse(maxJobs.Text);
            if (maxnJobs < 0 || maxnJobs > 10) OK = false;
        }
        catch
        {
            OK = false;
        }

        if (!OK)
        {
            label1.Text = "Check the value for Maximum number of concurrent jobs";
        }

        if (OK)
        {
            label1.Text = "";
        }
        return OK;
    }

    private void maxJobs_TextChanged(object sender, System.EventArgs e)
    {
        button1.Enabled = OK();
    }
}
