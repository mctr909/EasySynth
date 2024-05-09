using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EasySynth.Forms
{
	public partial class Main : Form
	{
		public Main()
		{
			InitializeComponent();
		}

		private void Main_Load(object sender, EventArgs e)
		{
			Playback.Open(44100, 256);
		}

		private void Main_FormClosing(object sender, FormClosingEventArgs e)
		{
			Playback.Close();
		}
	}
}
