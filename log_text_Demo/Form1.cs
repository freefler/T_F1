using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace log_text
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public void WriteLog_nw()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 兜底：任何地方没捕获的异常，都会跑到这里
            Application.ThreadException += (s, e) =>
            {
                Log.WriteLog_Error("程序异常", e.Exception);
                MessageBox.Show("出错了：" + e.Exception.Message);
            };

            Application.Run(new Form1());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Log.WriteLog_Info("点击事件（按钮1）");
        
        }

        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            richTextBox1.Text = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory+ DateTime.Now.ToString("yyyy-MM-dd") + ".log");
        }
    }
}
