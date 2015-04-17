using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

public class PickTime : System.Windows.Forms.Form
{
    public System.Windows.Forms.DateTimePicker dateTimePicker1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOK;

    public PickTime()
    {
        Init();

        //dateTimePicker1.MinDate = DateTime.Today;
        dateTimePicker1.Value = new DateTime(dateTimePicker1.Value.Year, dateTimePicker1.Value.Month, dateTimePicker1.Value.Day, dateTimePicker1.Value.Hour, dateTimePicker1.Value.Minute + 1, 0, 0);
    }

    private void Init()
    {
        this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
        this.label1 = new System.Windows.Forms.Label();
        this.btnCancel = new System.Windows.Forms.Button();
        this.btnOK = new System.Windows.Forms.Button();
        this.SuspendLayout();
        // 
        // dateTimePicker1
        // 
        this.dateTimePicker1.CustomFormat = "HH:mm @ddddMMM dd, yyyy";
        this.dateTimePicker1.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
        this.dateTimePicker1.Location = new System.Drawing.Point(32, 64);
        this.dateTimePicker1.Name = "dateTimePicker1";
        this.dateTimePicker1.ShowUpDown = true;
        this.dateTimePicker1.Size = new System.Drawing.Size(232, 20);
        this.dateTimePicker1.TabIndex = 0;
        this.dateTimePicker1.ValueChanged += new System.EventHandler(this.dateTimePicker1_ValueChanged);
        // 
        // label1
        // 
        this.label1.Location = new System.Drawing.Point(32, 40);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(168, 23);
        this.label1.TabIndex = 1;
        this.label1.Text = "Select a time to start the job:";
        // 
        // btnCancel
        // 
        this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.btnCancel.Location = new System.Drawing.Point(200, 136);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.TabIndex = 2;
        this.btnCancel.Text = "Cancel";
        // 
        // btnOK
        // 
        this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.btnOK.Location = new System.Drawing.Point(112, 136);
        this.btnOK.Name = "btnOK";
        this.btnOK.TabIndex = 3;
        this.btnOK.Text = "OK";
        // 
        // PickTime
        // 
        this.AcceptButton = this.btnOK;
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.CancelButton = this.btnCancel;
        this.ClientSize = new System.Drawing.Size(296, 182);
        this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.btnOK,
																		  this.btnCancel,
																		  this.dateTimePicker1,
																		  this.label1});
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "PickTime";
        this.ShowInTaskbar = false;
        this.Text = "PickTime";
        this.ResumeLayout(false);

    }

    private void dateTimePicker1_ValueChanged(object sender, System.EventArgs e)
    {
        btnOK.Enabled = DateTime.Compare(DateTime.Now, dateTimePicker1.Value) < 0;
    }
}
