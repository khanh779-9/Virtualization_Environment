using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Virtualization_Environment
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(openFileDialog1.ShowDialog()== DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(textBox1.Text == "")
            {
                MessageBox.Show("Please select an executable file");
                return;
            }

            string exePath = textBox1.Text;
            if(!System.IO.File.Exists(exePath))
            {
                MessageBox.Show("File not found");
                return;
            }
            
            if(!Directory.Exists("C:\\SandBox_K"))
            {
                Directory.CreateDirectory("C:\\SandBox_K");
            }    
          
            Helper.VirtualEnvironment.RunInSandbox(exePath, "C:\\SandBox_K");
            MessageBox.Show("Process has been executed in sandbox");


        }
    }
}
