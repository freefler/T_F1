using Modbus.Data;
using Modbus.Device;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace THE_Ftry_project
{
    internal class ModbusSlaveService
    {
        private SerialPort _port2;
        private ModbusSerialSlave _slave;
        //收集错误信息
        public event Action<string> ErrorOccurred;
        //
      
        public bool Start(string potName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            Close();
            //打开
            try
            {
                _port2 = new SerialPort(potName, baudRate, parity, dataBits, stopBits)
                {
                    ReadTimeout = 1000,
                    WriteTimeout = 1000
                };
                _port2.Open();
                _slave = ModbusSerialSlave.CreateRtu(1, _port2);
                _slave.DataStore = DataStoreFactory.CreateDefaultDataStore();
                _slave.DataStore.HoldingRegisters[1] = 464;
                _slave.DataStore.HoldingRegisters[2] = 88;
                Task.Run(() =>
                {
                    try { _slave.Listen(); }
                    catch (Exception ex) { }
                    
                });
                return true;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke("打开串口失败：" + ex.Message);
                Close();
                return false;
            }

        }
        public bool IsOpen => _port2 != null && _port2.IsOpen;

        private void Close()
        {
            if (_slave != null)
            {
                try { _slave.Dispose(); } catch { }
                _slave = null;
            }
            if (_port2 != null)
            {
                try { _port2.Close(); } catch { }
                try { _port2.Dispose(); } catch { }
                _port2 = null;
            }
        }

        public void IDisposable()
        {
            Close();
        }
    }
}
