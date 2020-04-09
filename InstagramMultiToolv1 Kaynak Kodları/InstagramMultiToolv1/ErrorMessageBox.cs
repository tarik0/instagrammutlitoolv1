using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InstagramMultiToolv1
{
    public partial class ErrorMessageBox : Form
    {
        public ErrorMessageBox(string msg, FormStartPosition pos)
        {
            InitializeComponent();
            this.StartPosition = pos;
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            label1.Text = msg;
        }

        private void ErrorMessageBox_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
