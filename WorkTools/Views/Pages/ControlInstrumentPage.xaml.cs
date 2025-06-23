using NModbus;
using NModbus.Device;
using NModbus.Extensions.Enron;
using NModbus.IO;
using NModbus.Serial;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Configuration;
using System.IO.Ports;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TwinCAT.Ads;
using TwinCAT.TypeSystem;
using Wpf.Ui.Common.Interfaces;

namespace WorkTools.Views.Pages
{
    /// <summary>
    /// ControlInstrumentPage.xaml 的交互逻辑
    /// </summary>
    public partial class ControlInstrumentPage : INavigableView<ViewModels.ControlInstrumentViewModel>
    {
        public static readonly byte slaveAddress = byte.Parse(ConfigurationManager.AppSettings["slaveAddress"]);
        public static readonly string rtuComName = Convert.ToString(ConfigurationManager.AppSettings["rtuComName"]);
        public static readonly int rtuBaudRate = int.Parse(ConfigurationManager.AppSettings["rtuBaudRate"]);
        public static readonly int rtuDataBits = int.Parse(ConfigurationManager.AppSettings["rtuDataBits"]);
        public static readonly int rtuParity = int.Parse(ConfigurationManager.AppSettings["rtuParity"]);
        public static readonly int rtuStopBits = int.Parse(ConfigurationManager.AppSettings["rtuStopBits"]);
        public static readonly int rtuReadTimeout = int.Parse(ConfigurationManager.AppSettings["rtuReadTimeout"]);
        public static readonly int rtuWriteTimeout = int.Parse(ConfigurationManager.AppSettings["rtuWriteTimeout"]);

        public ViewModels.ControlInstrumentViewModel ViewModel
        {
            get;
        }

        public ControlInstrumentPage(ViewModels.ControlInstrumentViewModel viewModel)
        {
            ViewModel = viewModel;

            InitializeComponent();
        }

        IModbusMaster _modbusSerialRtuMaster;
        SerialPort port;
        private void BtnReadPLCState_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Log(string info)
        {
            Dispatcher.Invoke(() =>
            {
                LvLog.Items.Add($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}---{info}");
            });
        }

        private void BtnClearLog_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                LvLog.Items.Clear();
            });
        }

        private void BtnConnectPLC_Click(object sender, RoutedEventArgs e)
        {
            SerialPort newPort = CreatePort(port);

            SerialPortAdapter adapter = new SerialPortAdapter(newPort);
            var factory = new ModbusFactory();
            _modbusSerialRtuMaster = factory.CreateRtuMaster(adapter);
        }

        private SerialPort CreatePort(SerialPort port)
        {
            try
            {
                if (port?.IsOpen ?? false)
                {
                    port.DiscardInBuffer();
                    port.DiscardOutBuffer();
                    port.Close();
                    port.Dispose();
                }
                port ??= new SerialPort(rtuComName);
                port.BaudRate = rtuBaudRate;
                port.DataBits = rtuDataBits;
                port.Parity = (Parity)rtuParity;
                port.StopBits = (StopBits)rtuStopBits;
                port.ReadTimeout = rtuReadTimeout;
                port.WriteTimeout = rtuWriteTimeout;
                port.Open();

                Log($"Serial port {port.PortName} open success");
            }
            catch (Exception ex)
            {
                Log($"Serial port {port.PortName} open failed:{ex.Message}");
            }
            return port;
        }

        private void BtnDisConnectPLC_Click(object sender, RoutedEventArgs e)
        {
            DisConnect();
        }

        private void DisConnect()
        {
            if (port?.IsOpen ?? false)
            {
                port.DiscardInBuffer();
                port.DiscardOutBuffer();
                port.Close();
                port.Dispose();
            }
            _modbusSerialRtuMaster?.Dispose();
        }

        private void BtnReadVariables_Click(object sender, RoutedEventArgs e)
        {
            string address = TxtAddress.Text.Trim();

            _modbusSerialRtuMaster.ReadHoldingRegisters(slaveAddress, 0, 123);

        }

        private void BtnGetAppVersion_Click(object sender, RoutedEventArgs e)
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            Log($"App Version:{version}");
        }

        private void BtnWriteBool_Click(object sender, RoutedEventArgs e)
        {
            bool result1 = ushort.TryParse(TxtAddress.Text.Trim(), out ushort address);
            bool result2 = bool.TryParse(TxtAddress.Text.Trim(), out bool value);

            if (result1 && result2)
            {
                try
                {
                    _modbusSerialRtuMaster.WriteSingleCoil(slaveAddress, address, value);
                    Log($"Write Val:{value} to address {address} success");
                }
                catch (Exception ex)
                {
                    Log($"Write Val:{value} to address {address} failed,{ex.Message}");
                }
            }
        }

        private void BtnWriteByte_Click(object sender, RoutedEventArgs e)
        {
        }

        private void BtnWriteuint16_Click(object sender, RoutedEventArgs e)
        {
            bool result1 = ushort.TryParse(TxtAddress.Text.Trim(), out ushort address);
            bool result2 = ushort.TryParse(TxtValue.Text.Trim(), out ushort value);

            if (result1 && result2)
            {
                try
                {
                    _modbusSerialRtuMaster.WriteSingleRegister(slaveAddress, address, value);
                    Log($"Write Val:{value} to address {address} success");
                }
                catch (Exception ex)
                {
                    Log($"Write Val:{value} to address {address} failed,{ex.Message}");
                }
            }
        }

        private void BtnWriteint16_Click(object sender, RoutedEventArgs e)
        {
            bool result1 = ushort.TryParse(TxtAddress.Text.Trim(), out ushort address);
            bool result2 = ushort.TryParse(TxtValue.Text.Trim(), out ushort value);

            if (result1 && result2)
            {
                try
                {
                    _modbusSerialRtuMaster.WriteSingleRegister(slaveAddress, address, value);
                    Log($"Write Val:{value} to address {address} success");
                }
                catch (Exception ex)
                {
                    Log($"Write Val:{value} to address {address} failed,{ex.Message}");
                }
            }
        }

        private void BtnWriteuint_Click(object sender, RoutedEventArgs e)
        {
            bool result1 = ushort.TryParse(TxtAddress.Text.Trim(), out ushort address);
            bool result2 = UInt32.TryParse(TxtValue.Text.Trim(), out uint value);

            if (result1 && result2)
            {
                try
                {
                    _modbusSerialRtuMaster.WriteMultipleRegisters(slaveAddress, address, UInt32ToUshortArray(value));
                    Log($"Write Val:{value} to address {address} success");
                }
                catch (Exception ex)
                {
                    Log($"Write Val:{value} to address {address} failed,{ex.Message}");
                }
            }
        }

        public static ushort[] UInt32ToUshortArray(uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            ushort[] result = new ushort[2];
            result[0] = BitConverter.ToUInt16(bytes, 0); // 前两个字节
            result[1] = BitConverter.ToUInt16(bytes, 2); // 后两个字节
            return result;
        }

        public static ushort[] Int32ToUshortArray(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            ushort[] result = new ushort[2];
            result[0] = BitConverter.ToUInt16(bytes, 0); // 前两个字节
            result[1] = BitConverter.ToUInt16(bytes, 2); // 后两个字节
            return result;
        }

        public static ushort[] FloatToUshortArray(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            ushort[] result = new ushort[2];
            result[0] = BitConverter.ToUInt16(bytes, 0); // 前两个字节
            result[1] = BitConverter.ToUInt16(bytes, 2); // 后两个字节
            return result;
        }

        public static ushort[] DoubleToUshortArray(double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            ushort[] result = new ushort[4];
            for (int i = 0; i < bytes.Length; i += 2)
            {
                result[i / 2] = BitConverter.ToUInt16(bytes);
            }
            return result;
        }

        private void BtnWriteint_Click(object sender, RoutedEventArgs e)
        {
            bool result1 = ushort.TryParse(TxtAddress.Text.Trim(), out ushort address);
            bool result2 = Int32.TryParse(TxtValue.Text.Trim(), out int value);

            if (result1 && result2)
            {
                try
                {
                    _modbusSerialRtuMaster.WriteMultipleRegisters(slaveAddress, address, Int32ToUshortArray(value));
                    Log($"Write Val:{value} to address {address} success");
                }
                catch (Exception ex)
                {
                    Log($"Write Val:{value} to address {address} failed,{ex.Message}");
                }
            }
        }

        private void BtnWriteFloat_Click(object sender, RoutedEventArgs e)
        {
            bool result1 = ushort.TryParse(TxtAddress.Text.Trim(), out ushort address);
            bool result2 = float.TryParse(TxtValue.Text.Trim(), out float value);

            if (result1 && result2)
            {
                try
                {
                    _modbusSerialRtuMaster.WriteMultipleRegisters(slaveAddress, address, FloatToUshortArray(value));
                    Log($"Write Val:{value} to address {address} success");
                }
                catch (Exception ex)
                {
                    Log($"Write Val:{value} to address {address} failed,{ex.Message}");
                }
            }
        }

        private void BtnWriteDouble_Click(object sender, RoutedEventArgs e)
        {
            bool result1 = ushort.TryParse(TxtAddress.Text.Trim(), out ushort address);
            bool result2 = float.TryParse(TxtValue.Text.Trim(), out float value);

            if (result1 && result2)
            {
                try
                {
                    _modbusSerialRtuMaster.WriteMultipleRegisters(slaveAddress, address, DoubleToUshortArray(value));
                    Log($"Write Val:{value} to address {address} success");
                }
                catch (Exception ex)
                {
                    Log($"Write Val:{value} to address {address} failed,{ex.Message}");
                }
            }
        }

        private void BtnReadBool_Click(object sender, RoutedEventArgs e)
        {
            bool result1 = ushort.TryParse(TxtAddress.Text.Trim(), out ushort address);

            if (result1)
            {
                try
                {
                    var value = _modbusSerialRtuMaster.ReadCoils(slaveAddress, address, 1);
                    Log($"Read address {address},Value is {value} success");
                }
                catch (Exception ex)
                {
                    Log($"Read address {address} failed,{ex.Message}");
                }
            }
        }

        private void BtnReadByte_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnReaduint16_Click(object sender, RoutedEventArgs e)
        {
            bool result1 = ushort.TryParse(TxtAddress.Text.Trim(), out ushort address);

            if (result1)
            {
                try
                {
                    var value = _modbusSerialRtuMaster.ReadHoldingRegisters(slaveAddress, address, 1);
                    var result = Convert.ToUInt16(value);

                    Log($"Read address {address},Value is {result} success");
                }
                catch (Exception ex)
                {
                    Log($"Read address {address} failed,{ex.Message}");
                }
            }
        }

        private void BtnReadint16_Click(object sender, RoutedEventArgs e)
        {
            bool result1 = ushort.TryParse(TxtAddress.Text.Trim(), out ushort address);

            if (result1)
            {
                try
                {
                    var value = _modbusSerialRtuMaster.ReadHoldingRegisters(slaveAddress, address, 1);
                    var result = Convert.ToInt16(value);

                    Log($"Read address {address},Value is {result} success");
                }
                catch (Exception ex)
                {
                    Log($"Read address {address} failed,{ex.Message}");
                }
            }
        }

        private void BtnReaduint_Click(object sender, RoutedEventArgs e)
        {
            bool result1 = ushort.TryParse(TxtAddress.Text.Trim(), out ushort address);

            if (result1)
            {
                try
                {
                    var value = _modbusSerialRtuMaster.ReadHoldingRegisters(slaveAddress, address, 2);
                    var result = NModbus.Utility.ModbusUtility.GetUInt32(value[0], value[1]);

                    Log($"Read address {address},Value is {result} success");
                }
                catch (Exception ex)
                {
                    Log($"Read address {address} failed,{ex.Message}");
                }
            }
        }

        private void BtnReadint_Click(object sender, RoutedEventArgs e)
        {
            bool result1 = ushort.TryParse(TxtAddress.Text.Trim(), out ushort address);

            if (result1)
            {
                try
                {
                    var value = _modbusSerialRtuMaster.ReadHoldingRegisters(slaveAddress, address, 2);
                    var result = Convert.ToInt32(value);

                    Log($"Read address {address},Value is {result} success");
                }
                catch (Exception ex)
                {
                    Log($"Read address {address} failed,{ex.Message}");
                }
            }
        }

        private void BtnReadFloat_Click(object sender, RoutedEventArgs e)
        {
            bool result1 = ushort.TryParse(TxtAddress.Text.Trim(), out ushort address);

            if (result1)
            {
                try
                {
                    var value = _modbusSerialRtuMaster.ReadHoldingRegisters(slaveAddress, address,2);
                    var result = NModbus.Utility.ModbusUtility.GetSingle(value[0], value[1]);

                    Log($"Read address {address},Value is {result} success");
                }
                catch (Exception ex)
                {
                    Log($"Read address {address} failed,{ex.Message}");
                }
            }
        }

        private void BtnReadDouble_Click(object sender, RoutedEventArgs e)
        {
            bool result1 = ushort.TryParse(TxtAddress.Text.Trim(), out ushort address);

            if (result1)
            {
                try
                {
                    var value = _modbusSerialRtuMaster.ReadHoldingRegisters(slaveAddress, address, 4);
                    var result = NModbus.Utility.ModbusUtility.GetDouble(value[3], value[2], value[1], value[0]);

                    Log($"Read address {address},Value is {result} success");
                }
                catch (Exception ex)
                {
                    Log($"Read address {address} failed,{ex.Message}");
                }
            }
        }
    }

}
