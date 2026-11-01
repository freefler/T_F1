using ClosedXML.Excel;
using DocumentFormat.OpenXml.Math;
using MY_Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.IO.Ports.SerialPort;
namespace R_text
{
    public partial class Form1 : Form
    {
        #region sin
        public Form1()
        {
            InitializeComponent();
            panel1.Pan_EnableDoubleBuffering();
            timer1.Interval = 30;
            timer1.Start();
            timer2.Interval = 1000;
            timer2.Start();
            panel1.Paint += panel1_Paint;
            button1.Text = "暂停";
            Port_name();
            _serial = new SerialSoruce();
            _serial.Log += m => AppendLog(m);
            _log += m =>
            {
                if (listBox1.InvokeRequired)
                    listBox1.BeginInvoke(new Action(() => AppendLog(m)));
                else
                    AppendLog(m);
            };

            _serial.ValuePrased += v =>
            {
                this.BeginInvoke(new Action(() =>
                {
                    list1_sin.Add(v);
                    if (list1_sin.Count > 150) list1_sin.RemoveAt(0);
                    panel1.Invalidate();
                }));
            };
        }
        private void AppendLog(string m)
        {
            listBox1.BeginUpdate();
            listBox1.Items.Add(m);
            listBox1.TopIndex = list1_sin != null ? listBox1.Items.Count - 1 : listBox1.Items.Count - 1;
            while (listBox1.Items.Count > 200) listBox1.Items.RemoveAt(0);
            listBox1.EndUpdate();
        }
        public void Port_name()
        {
            string[] port = SerialPort.GetPortNames();
            comboBox1.Items.AddRange(port);
            comboBox1.Text = comboBox1.Items[0]?.ToString();
        }
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            using (Pen pen = new Pen(Color.Black, 2))
            {
                Graphics g = e.Graphics;
                g.Clear(Color.White);
                if (list1_sin.Count < 2) return;
                Point[] pi = new Point[list1_sin.Count];
                for (int i = 0; i < list1_sin.Count; i++)
                {
                    int x = (int)(i * (panel1.Width / 150.0));
                    int y = (int)(panel1.Height - (list1_sin[i] / 100.0) * panel1.Height);
                    pi[i] = new Point(x, y);
                }
                g.DrawLines(pen, pi);
            }
           
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_serial == null || !_serial.IS_Open)
            {
                if (timer1.Enabled)
                { button1.Text = "开始"; timer1.Stop(); timer2.Stop(); }
                else { button1.Text = "暂停"; timer1.Start(); timer2.Start(); }
            }
            else { MessageBox.Show("串口未关闭"); }
          
           
        }
        public List<double> list1_sin = new List<double>();
        public int step = 0;
        public Random random1 = new Random();
        private event Action<string> _log;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_serial == null || !_serial.IS_Open)
            {
                var data = 50 + Math.Sin(step * 0.1) * 20 + random1.Next(0, 20) * 0.1;
                list1_sin.Add(data);
                dat1.Append(data);
                step++;
                if (list1_sin.Count > 150) { list1_sin.RemoveAt(0); } else if (step > 999) step = 0; 
            }
            panel1.Invalidate();
        }
        private StringBuilder dat1 = new StringBuilder();
        private void timer2_Tick(object sender, EventArgs e)
        {
            if (list1_sin.Count == 0) return;
            double max = list1_sin.Max(), min = list1_sin.Min(), avg = list1_sin.Average();
            _log?.Invoke($"[{DateTime.Now:HH:mm:ss}] 点数:{list1_sin.Count} 当前:{list1_sin.Last():F2} 最大:{max:F2} 最小:{min:F2} 平均:{avg:F2}");

        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                timer1.Stop();
                timer2.Stop();
                button1.Text = "开始";
                dlg.Filter = "Excel 文件|*.xlsx";
                dlg.FileName = $"采集数据表{DateTime.Now:yy-MM-dd}.xlsx";
                if (dlg.ShowDialog() != DialogResult.OK)
                {
                    timer1.Start();
                    timer2.Start();
                    button1.Text = "暂停";
                    return;
                }
                using (XLWorkbook wb = new XLWorkbook())
                {
                    IXLWorksheet ws = wb.Worksheets.Add("sin数据");
                    ws.Cell(1, 1).Value = "序号";
                    ws.Cell(1, 2).Value = "数据";
                    for (int i = 0; i < list1_sin.Count; i++)
                    {
                        ws.Cell(i + 2, 1).Value = i + 1;
                        ws.Cell(i + 2, 2).Value = list1_sin[i];
                    }
                    wb.SaveAs(dlg.FileName);
                }
                MessageBox.Show("已导出 " + list1_sin.Count + " 个点");
            }
        }
        #endregion

        private SerialSoruce _serial;

        private void button3_Click(object sender, EventArgs e)
        {
            if (_serial.IS_Open) { 
                _serial.Disconnect();
                button3.Text="打开串口"; timer1.Start(); timer2.Start(); button1.Text = "暂停"; }
            else { _serial.Connect(comboBox1.SelectedItem.ToString(), 9600);
                button3.Text="关闭串口"; timer1.Stop(); timer2.Stop(); button1.Text = "开始"; }
            
        }
    }

    public class SerialSoruce : IDisposable
    {
        private SerialPort _sp;
        private Timer _watch = new Timer(); // 看门狗
        private DateTime _lastRecv = DateTime.MinValue;
        //  环形缓冲\先攒再拆包
        private byte[] bytes_data = new byte[4096];
        private int _head, _tail;
        public event Action<double> ValuePrased;// 解析数值
        public event Action<string> Log; // 日志
        public bool IS_Open => _sp != null && _sp.IsOpen;
        public void Connect(string port, int baud)
        {
            _sp = new SerialPort(port, baud)
            { NewLine = "\n" };
            _sp.DataReceived += OnData;
            _sp.Open();

            _lastRecv = DateTime.Now;
            _watch.Interval = 1000;
            _watch.Tick += Watch_Tick;

            Log?.Invoke($"[{DateTime.Now:HH:mm:ss}]打开串口{port} @{baud}");

        }
        private void OnData(object s, SerialDataReceivedEventArgs e)
        {
            if (_sp == null || !_sp.IsOpen) return;
            int n = _sp.BytesToRead;
            byte[] tmp = new byte[n];
            _sp.Read(tmp, 0, n);
            foreach (byte b in tmp) WriteRing(b);//数据滚动
            _lastRecv = DateTime.Now;
            ParseLines();
        }
        private void Watch_Tick(object s, EventArgs e)
        {
            if (_sp == null) return;
            if ((DateTime.Now - _lastRecv).TotalSeconds > 3)
            {
                Log?.Invoke($"[{DateTime.Now:HH:mm:ss}]超时");
                try { _sp.Close(); _sp.Open(); _lastRecv = DateTime.Now; }
                catch (Exception ex) { Log?.Invoke($"[{DateTime.Now:HH:mm:ss}]串口错误:连接失败" + ex.Message); }
            }

        }
        private void WriteRing(byte b)
        {
            bytes_data[_head] = b;
            _head = (_head + 1) % bytes_data.Length;
            if (_head == _tail) _tail = (_tail + 1) % bytes_data.Length;
        }
        private void ParseLines()
        {
            var sb = new StringBuilder();
            while (_tail != _head)
            {
                byte b = bytes_data[_tail];
                _tail = (_tail + 1) % bytes_data.Length;
                if (b == (byte)'\n')
                {
                    if (double.TryParse(sb.ToString().Trim(), out double value))
                    {
                        ValuePrased?.Invoke(value);
                        sb.Clear();
                    }
                }
                else if (b != (byte)'\n')
                { sb.Append((char)b); }
            }

        }

        public void Dispose() => Disconnect();
        public void Disconnect()
        {
            _watch.Stop();
            if (_sp != null && _sp.IsOpen)
            {
                _sp.Close();
                _sp.Dispose();
                _sp = null;
            }
            Log?.Invoke($"[{DateTime.Now:HH:mm:ss}]关闭串口");
        }
    }

}
