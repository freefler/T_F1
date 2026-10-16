using Microsoft.Win32;
using System;
using System.Data;
using System.IO.Ports;
using System.Windows.Forms;

namespace THE_Ftry_project
{
    public partial class Form1 : Form
    {
        #region 事件与日志
        // 服务读到数据时被调用
        private void OnDataReceived(ushort[] data) { AppendLog($"[{DateTime.Now:HH:mm:ss}] {string.Join("_", data)}"); }
        // 服务出错时被调用
        private void OnError(string msg) { AppendLog($"[{DateTime.Now:HH:mm:ss}] 读取失败：{msg}"); }
        // 统一的日志写入方法（带跨线程保护）
        private void AppendLog(string text)
        { // ★ 事件可能从后台线程触发，不能直接碰控件，要"切回"UI 线程
            if (richTextBox1.InvokeRequired)
            { richTextBox1.BeginInvoke(new Action<string>(AppendLog), text); return; }
            richTextBox1.AppendText(text + "\r\n");
            richTextBox1.ScrollToCaret();
        }
        #endregion
        public string[] Select_por()
        {
            string[] ports = SerialPort.GetPortNames();
            if (ports.Length > 0)
            {
                comboBox9.Items.AddRange(ports);
                comboBox10.Items.AddRange(ports);
                return ports;
            }
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DEVICEMAP\SERIALCOMM"))
            {
                richTextBox1.AppendText("系统未识别到可用串口,尝试注册表获取...");
                if (key == null) { richTextBox1.AppendText("注册表未识别到可用串口"); return new string[0]; }
                var list = new System.Collections.Generic.List<string>();
                foreach (var name in key.GetValueNames())
                {
                    string portName = key.GetValue(name) as string;
                    if (!string.IsNullOrEmpty(portName)) list.Add(portName);
                }
                comboBox9.Items.AddRange(list.ToArray());
                comboBox10.Items.AddRange(list.ToArray());
                return list.ToArray();
            }
        }
        private void Initial()
        {
            label1.Text = "连接状态：未连接";
            label2.Text = "连接状态：未连接";
            comboBox1.Text = comboBox1.Items[6].ToString();
            comboBox5.Text = comboBox5.Items[6].ToString();
            comboBox2.Text = comboBox2.Items[0].ToString();
            comboBox6.Text = comboBox6.Items[0].ToString();
            comboBox3.Text = comboBox3.Items[0].ToString();
            comboBox7.Text = comboBox7.Items[0].ToString();
            comboBox4.Text = comboBox4.Items[0].ToString();
            comboBox8.Text = comboBox8.Items[0].ToString();
            string[] port_name = Select_por();
            if (port_name.Length == 0) return;
            comboBox9.Text = comboBox9.Items[0].ToString();
            comboBox10.Text = comboBox10.Items[1].ToString();
        }//初始化
        private ModbusRtuService _rtu;
        private ModbusSlaveService _slave;
        private bool _polling;

        public Form1()
        {
            InitializeComponent();
            _rtu = new ModbusRtuService();
            _rtu.DataReceived += OnDataReceived;
            _rtu.ErrorOccurred += OnError;
            _slave = new ModbusSlaveService();
            _slave.ErrorOccurred += OnError;
        }

        #region 串口配置

        private bool OffandON1 = false;
        private bool OffandON2 = false;

        private bool OpenSlaveFromUi()
        {
            if (comboBox10.SelectedItem == null)
            {
                MessageBox.Show("从站串口未选择");
                return false;
            }
            string portName = comboBox10.Text;
            int baudRate = Convert.ToInt32(comboBox5.Text);
            int dataBits = Convert.ToInt32(comboBox7.Text);
            Parity parity = (Parity)Enum.Parse(typeof(Parity), comboBox6.Text);
            StopBits stopBits = StopBits.One;
            switch (comboBox8.Text)
            {
                case "1": stopBits = StopBits.One; break;
                case "1.5": stopBits = StopBits.OnePointFive; break;
                case "2": stopBits = StopBits.Two; break;
            }
            return _slave.Start(portName, baudRate, parity, dataBits, stopBits);
        }//开串口

        private bool OpenMasterFromUi()
        {
            if (comboBox9.SelectedItem == null)
            {
                MessageBox.Show("主站串口未选择");
                return false;
            }
            string portName = comboBox9.Text;
            int baudRate = Convert.ToInt32(comboBox1.Text);
            int dataBits = Convert.ToInt32(comboBox3.Text);
            Parity parity = (Parity)Enum.Parse(typeof(Parity), comboBox2.Text);
            StopBits stopBits = StopBits.One;
            switch (comboBox4.Text)
            {
                case "1": stopBits = StopBits.One; break;
                case "1.5": stopBits = StopBits.OnePointFive; break;
                case "2": stopBits = StopBits.Two; break;
            }
            return _rtu.Open(portName, baudRate, parity, dataBits, stopBits);
        }//new开串口
        #endregion

        #region 主从站设置


        private void CloseMaster()
        {
            _rtu.StopPoll();
            _rtu.Close();
            _polling = false;
            button4.Text = "开始轮询";
        }
        private void CloseSlave()
        {
            _slave.Dispose();
        }
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            Initial();
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            CloseMaster();
            CloseSlave();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (!OffandON1)
            {
                OpenMasterFromUi();
                if (_rtu.IsOpen)
                {
                    label1.Text = "连接状态：已连接";
                    OffandON1 = true;
                    button1.Text = "关闭串口";
                }
                else { MessageBox.Show("打开失败"); }

            }
            else
            {
                CloseMaster();
                if (!_rtu.IsOpen)
                {
                    label1.Text = "连接状态：未连接";
                    OffandON1 = false;
                    button1.Text = "打开串口";
                }
                else { MessageBox.Show("关闭失败"); }

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!OffandON2)
            {
                OpenSlaveFromUi();
                if (_slave.IsOpen)
                {
                    label2.Text = "连接状态：已连接";
                    OffandON2 = true;
                    button2.Text = "关闭串口";
                }
                else { MessageBox.Show("打开失败"); }

            }
            else
            {
                CloseSlave();
                if (!_slave.IsOpen)
                {
                    label2.Text = "连接状态：未连接";
                    OffandON2 = false;
                    button2.Text = "打开串口";
                }
                else { MessageBox.Show("关闭失败"); }
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            try
            {
                ushort[] data = await _rtu.ReadAsync(1, 0, 2);
                richTextBox1.AppendText($"[{DateTime.Now:HH:mm:ss}]message:{string.Join("_", data)}\r\n");
            }
            catch (Exception ex) { richTextBox1.AppendText($"[{DateTime.Now:HH:mm:ss}]+读取失败:{ex.Message}\r\n"); }

        }//listen

        #region 轮询
        private void button4_Click(object sender, EventArgs e)
        {

            if (!_rtu.IsOpen)
            {
                richTextBox1.AppendText("请先打开主站串口\r\n");
                return;
            }
            if (!_slave.IsOpen)
            {
                richTextBox1.AppendText("警告：从站未启动，读取会超时\r\n");
            }

            if (_polling)
            {
                _rtu.StopPoll();
                _polling = false;
                button4.Text = "开始轮询";
                richTextBox1.AppendText("=== 停止轮询 ===\r\n");
            }
            else
            {
                _rtu.StartPoll(1, 0, 2, 1000);   // 站号1，地址0，读2个，每1000ms一次
                _polling = true;
                button4.Text = "停止轮询";
                richTextBox1.AppendText("=== 开始轮询 ===\r\n");
            }

        }//轮询
        #endregion

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = DBhelper.QueryHistorty(100);
                richTextBox2.Clear();
                foreach (DataRow row in dt.Rows)
                { 
                    richTextBox2.AppendText($" {row["TagName"]}: {row["TagValue"]}—时间：{row["CollectTime"]}\r\n");
                }

            } 
            catch (Exception ex)
            { richTextBox2.AppendText("查询失败：" + ex.Message + "\r\n"); }
        }
        public string[] Insert_Themes() 
        {


            return null;
        }

        private void button7_Click(object sender, EventArgs e)
        {

        }
    }
}
