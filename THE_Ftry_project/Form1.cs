using Microsoft.Win32;
using Modbus.Data;
using Modbus.Device;
using System;
using System.Data;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace THE_Ftry_project
{
    public partial class Form1 : Form
    {
            #region 事件与日志
        // 服务读到数据时被调用
        private void OnDataReceived(ushort[] data) { AppendLog($"[{DateTime.Now:HH:mm:ss}] {string.Join("_", data)}");}
        // 服务出错时被调用
        private void OnError(string msg){ AppendLog($"[{DateTime.Now:HH:mm:ss}] 读取失败：{msg}"); }
        // 统一的日志写入方法（带跨线程保护）
        private void AppendLog(string text)
        { // ★ 事件可能从后台线程触发，不能直接碰控件，要"切回"UI 线程
            if (richTextBox1.InvokeRequired)
            { richTextBox1.BeginInvoke(new Action<string>(AppendLog), text);return; }
            richTextBox1.AppendText(text + "\r\n");
            richTextBox1.ScrollToCaret();
        }
             #endregion
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
            RegistryKey keycon = Registry.LocalMachine.OpenSubKey(@"Hardware\DeviceMap\SerialComm");
            string[] sSubkey = keycon.GetValueNames();
            foreach (var n in sSubkey)
            {
                string porName = (string)keycon.GetValue(n);
                comboBox9.Items.Add(porName);
                comboBox10.Items.Add(porName);
            }
            comboBox9.Text = comboBox9.Items[0].ToString();
            comboBox10.Text = comboBox10.Items[1].ToString();
            // port1 = new SerialPort();   // 【已废弃】主站串口已交给 ModbusRtuService 管理
            port2 = new SerialPort();      // 从站串口暂时还留在 Form1 里
            // master1 = null;             // 【已废弃】
            slave2 = null;

        }//初始化
        private ModbusRtuService _rtu;
        private bool _polling;

        public Form1()
        {
            InitializeComponent();
            _rtu = new ModbusRtuService();
            _rtu.DataReceived += OnDataReceived;
            _rtu.ErrorOccurred += OnError;
        }

        #region 串口配置
        private SerialPort port1;
        private SerialPort port2;
        private bool OffandON1 = false;
        private bool OffandON2 = false;
        #region 【已废弃·可折叠】旧的开串口方式 Open1
        /* ================================================================
           主站的串口和 Modbus 对象已经全部搬到 ModbusRtuService 里了，
           这个方法不再使用，保留在这里供对照学习，以后可以整个删掉。
        public void Open1()
        {
            try
            {
                port1.PortName = comboBox9.SelectedItem.ToString();
                port1.BaudRate = Convert.ToInt32(comboBox1.SelectedItem.ToString());
                port1.DataBits = Convert.ToInt32(comboBox3.SelectedItem.ToString());
                switch (comboBox2.SelectedIndex)
                {
                    case 0: port1.Parity = Parity.None; break;
                    case 1: port1.Parity = Parity.Odd; break;
                    case 2: port1.Parity = Parity.Even; break;
                    case 3: port1.Parity = Parity.Mark; break;
                    case 4: port1.Parity = Parity.Space; break;
                }
                switch (comboBox4.SelectedIndex)
                {
                    case 0: port1.StopBits = StopBits.One; break;
                    case 1: port1.StopBits = StopBits.OnePointFive; break;
                    case 2: port1.StopBits = StopBits.Two; break;
                }
                port1.Open();
                if (port1.IsOpen && master1 == null)
                {
                    SendData();
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("串口打开失败：" + ex.Message);

            }
        }//开串口
        ================================================================ */
        #endregion

        private void Open2()
        {
            try
            {
                port2.PortName = comboBox10.SelectedItem.ToString();
                port2.BaudRate = Convert.ToInt32(comboBox5.SelectedItem.ToString());
                port2.DataBits = Convert.ToInt32(comboBox7.SelectedItem.ToString());
                switch (comboBox6.SelectedIndex)
                {
                    case 0: port2.Parity = Parity.None; break;
                    case 1: port2.Parity = Parity.Odd; break;
                    case 2: port2.Parity = Parity.Even; break;
                    case 3: port2.Parity = Parity.Mark; break;
                    case 4: port2.Parity = Parity.Space; break;
                }
                switch (comboBox8.SelectedIndex)
                {

                    case 0: port2.StopBits = StopBits.One; break;
                    case 1: port2.StopBits = StopBits.OnePointFive; break;
                    case 2: port2.StopBits = StopBits.Two; break;
                }
                port2.Open();
                if (port2.IsOpen && slave2 == null)
                {
                    ListenData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("串口打开失败：" + ex.Message);

            }
        }//开串口

        private bool OpenMasterFromUi() 
        {
            if (comboBox9.SelectedItem==null) 
            {
                MessageBox.Show("串口未选择");
                return false;
            }       
            string portName = comboBox9.Text;
            int baudRate= Convert.ToInt32(comboBox1.Text);
            int dataBits= Convert.ToInt32(comboBox3.Text);
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
       
        private ModbusSerialSlave slave2;    // 从站（虚拟PLC）
        private void ListenData()
        {

            slave2 = ModbusSerialSlave.CreateRtu(1, port2);
            slave2.DataStore = DataStoreFactory.CreateDefaultDataStore();
            slave2.DataStore.HoldingRegisters[1] = 253;
            slave2.DataStore.HoldingRegisters[2] = 66;
            Task.Run(() =>
            {
                try
                {
                    slave2.Listen();   // 这个方法正常情况下永不返回
                }
                catch (Exception)
                {
                    // 关闭串口 / Dispose 时，这里必然会抛异常
                    // 这是正常退出，不需要处理，留个空 catch 防止程序崩掉
                }
            });

        }//从站设置
        #region 【已废弃·可折叠】旧的主站设置 SendData
        /* ================================================================
        private void SendData()
        {
            master1 = ModbusSerialMaster.CreateRtu(port1);
            master1.Transport.ReadTimeout = 1000;
            master1.Transport.Retries = 0;
        }//主站设置
        ================================================================ */
        #endregion

        private void CloseMaster()
        {
            #region 废弃代码
            //try
            //{
            //    if (master1 != null)
            //    {
            //        master1.Dispose();
            //        master1 = null;
            //    }//主
            //}
            //catch (Exception ex) { MessageBox.Show("主站错误:" + ex.Message); }//关主站
            //if (port1 != null && port1.IsOpen)
            //{ port1.Close(); }
            #endregion
            _rtu.StopPoll();
            _rtu.Close();
            _polling = false;
            button4.Text = "开始轮询";
        }
        private void CloseSlave()
        {
            try
            {
                if (slave2 != null)
                {
                    slave2.Dispose();
                    slave2 = null;
                }//主
            }
            catch (Exception ex) { MessageBox.Show("从站错误:" + ex.Message); }//关从站
            if (port2 != null && port2.IsOpen)
            { port2.Close(); }
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
                Open2();
                if (port2.IsOpen)
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
                if (!port2.IsOpen)
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
            #region 往期代码
            //if (master1 == null) { richTextBox1.AppendText("请先打开主站串口\r\n"); return; }
            //ushort[] listendata = null;
            //try { listendata = master1.ReadHoldingRegisters(1, 0, 2); }
            //catch (Exception ex) { richTextBox1.AppendText($"[{DateTime.Now:HH:mm:ss}] 读取失败：{ex.Message}\r\n"); return; }

            //try
            //{
            //    richTextBox1.AppendText($"[{DateTime.Now:HH:mm:ss}] message：{string.Join("_", listendata)}\r\n");
            //}

            //catch (Exception ex) { richTextBox1.AppendText($"[{DateTime.Now:HH:mm:ss}] 读取失败：{ex.Message}\r\n"); }
            #endregion
            try { ushort[] data = await _rtu.ReadAsync(1,0,2);
                richTextBox1.AppendText($"[{DateTime.Now:HH:mm:ss}]message:{string.Join("_",data)}\r\n");
            }
            catch (Exception ex){ richTextBox1.AppendText($"[{DateTime.Now:HH:mm:ss}]+读取失败:{ ex.Message}\r\n"); }



        }//listen

        #region 轮询
       
        private void button4_Click(object sender, EventArgs e)
        {
            #region 往期代码
            //if (!Star_Pol)
            //{
            //    if (master1 == null)
            //    { richTextBox1.AppendText("请先打开主站串口\r\n"); return; }
            //    if (slave2 == null)
            //    { richTextBox1.AppendText("警告：从站未启动，读取会超时\r\n"); }
            //    timerPoll.Start();

            //    richTextBox1.AppendText("=== 开始轮询 ===\r\n");
            //    button4.Text = "暂停轮询";
            //    Star_Pol = true;
            //}
            //else
            //{
            //    timerPoll.Stop();  richTextBox1.AppendText("=== 停止轮询 ===\r\n"); 
            //    Star_Pol=false;
            //    button4.Text = "开始轮询";
            //}
            #endregion
            if (!_rtu.IsOpen)
            {
                richTextBox1.AppendText("请先打开主站串口\r\n");
                return;
            }
            if (slave2 == null)
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
        #region 【已废弃·可折叠】旧的轮询实现 timerPoll_Tick
        /* ================================================================
           轮询循环已经搬到 ModbusRtuService.StartPoll() 里了，
           那里有自己的后台循环和 CancellationToken，不再依赖 Timer 控件。
           窗体上的 timerPoll 组件可以删掉了。
        private bool _isReading = false;
        private readonly object _rtuLock = new object();
        private async void timerPoll_Tick(object sender, EventArgs e)
        {
            var m = master1;
            if (m == null) return;
            if (_isReading == true) return;
            _isReading = true;
            try
            {
                ushort[] data = await Task.Run(() =>
                {
                    lock (_rtuLock)
                    {
                        return m.ReadHoldingRegisters(1, 0, 2);
                    }
                });

                richTextBox1.AppendText($"[{DateTime.Now:HH:mm:ss}]{string.Join("_",data)}\r\n");
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText($"[{DateTime.Now:HH:mm:ss}] 读取失败：{ex.Message}\r\n");
            }finally { _isReading = false; }

        }
        ================================================================ */
        #endregion

        #endregion
    }
}
