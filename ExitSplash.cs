using System;
using System.Drawing;
using System.Windows.Forms;

public class ExitSplash : Form
{
    public ExitSplash()
    {
        FormBorderStyle = FormBorderStyle.Fixed3D;
        BackColor = SystemColors.Window;
        BackColor = Color.FromArgb(255, 204, 153);
        Font = new Font(Font.FontFamily, 12, FontStyle.Bold);
        ControlBox = false;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(410, 200);

        Label lb;
        lb = new Label();
        lb.Parent = this;
        lb.AutoSize = true;
        lb.Location = new Point(40, 50);
        lb.Text = string.Format("{0} is terminating all running jobs...\r\n\r\nPlease wait.",Application.ProductName);

    }
}
