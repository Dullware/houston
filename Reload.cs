using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

public class Reload : System.Windows.Forms.Form
{
    public System.Windows.Forms.Label Question;
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.GroupBox groupBox1;
    public System.Windows.Forms.CheckBox undochanges;

    public Reload()
    {
        Init();

    }

    private void Init()
    {
        System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Reload));
        this.Question = new System.Windows.Forms.Label();
        this.button1 = new System.Windows.Forms.Button();
        this.button2 = new System.Windows.Forms.Button();
        this.undochanges = new System.Windows.Forms.CheckBox();
        this.groupBox1 = new System.Windows.Forms.GroupBox();
        this.SuspendLayout();
        // 
        // Question
        // 
        this.Question.Location = new System.Drawing.Point(16, 16);
        this.Question.Name = "Question";
        this.Question.Size = new System.Drawing.Size(312, 56);
        this.Question.TabIndex = 0;
        // 
        // button1
        // 
        this.button1.DialogResult = System.Windows.Forms.DialogResult.Yes;
        this.button1.Location = new System.Drawing.Point(248, 88);
        this.button1.Name = "button1";
        this.button1.TabIndex = 1;
        this.button1.Text = "Yes";
        // 
        // button2
        // 
        this.button2.DialogResult = System.Windows.Forms.DialogResult.No;
        this.button2.Location = new System.Drawing.Point(248, 128);
        this.button2.Name = "button2";
        this.button2.TabIndex = 2;
        this.button2.Text = "No";
        // 
        // undochanges
        // 
        this.undochanges.Location = new System.Drawing.Point(16, 128);
        this.undochanges.Name = "undochanges";
        this.undochanges.Size = new System.Drawing.Size(248, 24);
        this.undochanges.TabIndex = 3;
        this.undochanges.Text = "Undo all changes by the other application";
        // 
        // groupBox1
        // 
        this.groupBox1.Location = new System.Drawing.Point(8, 112);
        this.groupBox1.Name = "groupBox1";
        this.groupBox1.Size = new System.Drawing.Size(320, 48);
        this.groupBox1.TabIndex = 4;
        this.groupBox1.TabStop = false;
        // 
        // Reload
        // 
        this.AcceptButton = this.button1;
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.CancelButton = this.button2;
        this.ClientSize = new System.Drawing.Size(334, 188);
        this.Controls.Add(this.button2);
        this.Controls.Add(this.button1);
        this.Controls.Add(this.Question);
        this.Controls.Add(this.undochanges);
        this.Controls.Add(this.groupBox1);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
        this.Icon = new Icon(GetType(), "Houston.Houston.ico");
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "Reload";
        this.ShowInTaskbar = false;
        this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
        this.Text = Application.ProductName;
        this.ResumeLayout(false);

    }
}
