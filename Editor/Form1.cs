using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Editor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Sender.WaveOpen();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e) {
            Sender.WaveClose();
        }

        private void button1_Click(object sender, EventArgs e) {
            Sender.LoadDLS("C:\\Users\\user\\Desktop\\あいうえお");
        }
    }
}
