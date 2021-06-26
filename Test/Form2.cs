using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private string _text;
        public string text {
            get { return _text; }
            set { _text = value; }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.text = label1.Text;
            this.Close();
        }
    }
}
