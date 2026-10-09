using Modbus.Device;
using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace THE_Ftry_project
{
    /// <summary>
    /// Modbus RTU 通信服务：只负责串口和协议，不碰任何界面控件
    /// </summary>
    public class ModbusRtuService : IDisposable
    {
        // ==================== 事件（委托的核心用法）====================
        // 含义：数据读到了，我"通知"所有订阅者，但不关心你们拿去干嘛
        // Action<ushort[]> 就是一个"接受 ushort[] 参数、没有返回值"的委托类型
        public event Action<ushort[]> DataReceived;

        // 出错了也通知一下（参数是错误信息）
        public event Action<string> ErrorOccurred;

        // ==================== 私有字段 ====================
        private SerialPort _port;
        private ModbusSerialMaster _master;
        private readonly object _lock = new object();   // 串口独占锁
        private CancellationTokenSource _cts;           // 用来停止轮询循环
        public bool IsOpen => _port != null && _port.IsOpen;

        // ==================== 打开串口 ====================
        public bool Open(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            Close();   // 先清理旧的
            try
            {
                _port = new SerialPort(portName, baudRate, parity, dataBits, stopBits)
                {
                    ReadTimeout = 1000,
                    WriteTimeout = 1000
                };
                _port.Open();

                _master = ModbusSerialMaster.CreateRtu(_port);
                _master.Transport.ReadTimeout = 1000;
                _master.Transport.Retries = 0;

                return true;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke("打开串口失败：" + ex.Message);
                Close();
                return false;
            }
        }

        // ==================== 关闭，释放全部资源 ====================
        public void Close()
        {
            StopPoll();

            // 顺序很重要：先 Dispose 上层的 Modbus 对象，再关底层串口
            if (_master != null)
            {
                try { _master.Dispose(); } catch { }
                _master = null;
            }
            if (_port != null)
            {
                try { if (_port.IsOpen) _port.Close(); } catch { }
                try { _port.Dispose(); } catch { }
                _port = null;
            }
        }

        // ==================== 单次读取 ====================
        public async Task<ushort[]> ReadAsync(byte slaveId, ushort startAddress, ushort count)
        {
            var m = _master;
            if (m == null) throw new InvalidOperationException("串口未打开");

            // Task.Run 把阻塞读写扔到线程池，调用方的线程（UI）不会被卡住
            // lock 保证同一时刻只有一个读写操作，防止 RTU 帧在串口上撞车
            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    return m.ReadHoldingRegisters(slaveId, startAddress, count);
                }
            });
        }

        // ==================== 启动连续轮询 ====================
        /// <summary>
        /// 后台循环每隔 intervalMs 毫秒读一次，读到就触发 DataReceived 事件
        /// </summary>
        public void StartPoll(byte slaveId, ushort startAddress, ushort count, int intervalMs)
        {
            StopPoll();

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            Task.Run(async () =>
            {
                // 只要没收到"取消"指令，就一直读
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var m = _master;
                        if (m != null)
                        {
                            ushort[] data = await Task.Run(() =>
                            {
                                lock (_lock)
                                {
                                    return m.ReadHoldingRegisters(slaveId, startAddress, count);
                                }
                            }, token);

                            // ★ 核心：读到数据就"喊一声"，谁订阅了谁就去处理
                            DataReceived?.Invoke(data);
                        }
                    }
                    catch (Exception ex)
                    {
                        // 掉線、超时、站号错，统统转成 ErrorOccurred 事件送出去
                        if (!token.IsCancellationRequested)
                            ErrorOccurred?.Invoke(ex.Message);
                    }

                    try
                    {
                        await Task.Delay(intervalMs, token);   // 等一个周期
                    }
                    catch
                    {
                        return;   // 被取消了，直接退出循环
                    }
                }
            }, token);
        }

        // ==================== 停止轮询 ====================
        public void StopPoll()
        {
            try
            {
                if (_cts != null)
                {
                    _cts.Cancel();
                    _cts.Dispose();
                    _cts = null;
                }
            }
            catch { }
        }

        public void Dispose()
        {
            StopPoll();
            Close();
        }



     
    }
}
