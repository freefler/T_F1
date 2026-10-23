using System;
using System.Windows.Forms;

namespace Com_sql
{
    public partial class Form1 : Form
    {
        DBhelper str = new DBhelper("Data Source=DESKTOP-MD6I2MI\\SQLEXPRESS;" +
                "Initial Catalog=SCADA_Demo;" +
            "Integrated Security=True;TrustServerCertificate=True");

        public Form1()
        {
            InitializeComponent();
        }

        public void OpendSQL()
        {


            if (str.connStr == null)
            {
                label3.Text = "未输入连接字符串"; return;
            }
            try
            {
                if (str.TestConnection())
                {
                    label3.Text = "连接数据库成功";
                    ///连接数据库
                    dataGridView1.DataSource = null;
                    var dataTable = str.ConnectToData();
                    dataGridView1.DataSource = dataTable;

                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }

            }
            catch (Exception ex)
            {
                label3.Text = "连接数据库失败" + ex.Message;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Text = str.connStr;
            dataGridView1.ReadOnly = true;                  // 防止误改
            dataGridView1.AllowUserToAddRows = false;       // 去掉底部空白行
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            OpendSQL();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (str.TestConnection())
            {
                if (textBox2.Text == "") { MessageBox.Show("未输入数据"); return; }
                try
                {
                    int n = str.Inserdata("温度", Convert.ToDouble(textBox2.Text));
                    OpendSQL();
                    MessageBox.Show($"插入数据成功，影响行数：{n}");

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"插入数据失败：{ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("请先连接数据库");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (str.TestConnection())
            {
                if (textBox3.Text == "") { MessageBox.Show("未输入Id"); return; }
                if (textBox4.Text == "") { MessageBox.Show("未输入修改数据"); return; }
                try
                {
                    int n = str.UpdateData(Convert.ToInt32(textBox3.Text), "温度", Convert.ToDouble(textBox4.Text));
                    OpendSQL();
                    MessageBox.Show($"修改数据成功，影响行数：{n}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"插入数据失败：{ex.Message}");
                }
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (str.TestConnection())
            {
                if (textBox6.Text == "") { MessageBox.Show("未输入Id"); return; }
                try
                {
                    int id = Convert.ToInt32(textBox6.Text);
                    if (MessageBox.Show($"确定要删除 Id={id} 的数据吗？", "确认删除",
                     MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                    int n = str.DeleteData(id);
                    OpendSQL();
                    MessageBox.Show($"删除数据成功，影响行数：{n}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"删除数据失败：{ex.Message}");
                }
            }
        }
    }
}
