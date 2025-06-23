using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
using Wpf.Ui.Common.Interfaces;

namespace WorkTools.Views.Pages
{
    /// <summary>
    /// PLCPage.xaml 的交互逻辑
    /// </summary>
    public partial class PLCPage : INavigableView<ViewModels.PLCViewModel>
    {
        public static readonly string netId = ConfigurationManager.AppSettings["netId"].ToString();
        public static int port = int.Parse(ConfigurationManager.AppSettings["port"]);
        AdsClient adsClient = new AdsClient();

        public ViewModels.PLCViewModel ViewModel
        {
            get;
        }

        public PLCPage(ViewModels.PLCViewModel viewModel)
        {
            ViewModel = viewModel;

            InitializeComponent();
        }

        private void BtnReadPLCState_Click(object sender, RoutedEventArgs e)
        {
            if (adsClient.IsConnected)
            {
                try
                {
                    StateInfo state = adsClient.ReadState();
                    Log($"DeviceState:{state.DeviceState},AdsState:{state.AdsState}");
                }
                catch (Exception ex)
                {
                    Log($"Read plc state error:{ex.Message}");
                }
            }
            else
            {
                Log("PLC not connect");
            }
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
            try
            {
                adsClient?.Connect(netId, port);

                var errcode = adsClient.TryReadState(out StateInfo stateInfo);

                if (errcode == 0)
                {
                    Log($"PLC connect success!");
                }
                else
                {
                    Log($"PLC connect failed!");
                }
            }
            catch (Exception ex)
            {
                Log($"PLC connect error:{ex.Message}");
            }
        }

        private void BtnDisConnectPLC_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                adsClient?.Disconnect();
            }
            catch (Exception ex)
            {
                Log($"PLC Disconnect error:{ex.Message}");
            }
        }

        private void BtnReadVariables_Click(object sender, RoutedEventArgs e)
        {
            string address = TxtAddress.Text.Trim();
            if (adsClient.IsConnected)
            {
                try
                {
                    var value = adsClient.ReadValue(address);

                    Type type = value.GetType();

                    Log($"Addr:{address},Value:{value}");
                }
                catch (Exception ex)
                {
                    Log($"Read variable error,address:{address},{ex.Message}");
                }
            }
        }

        public T? Read<T>(string InstancePath)
        {
            if (!adsClient?.IsConnected ?? false)
            {
                Log("PLC not connected");
                return default;
            }

            T? value = default;

            try
            {
                if (adsClient.IsConnected)
                {
                    uint v1 = adsClient.CreateVariableHandle(InstancePath); // Short
                    value = adsClient.ReadAny<T>(v1);
                    adsClient.DeleteVariableHandle(v1);
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
            return value;
        }

        private void BtnGetAppVersion_Click(object sender, RoutedEventArgs e)
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            Log($"App Version:{version}");
        }

        private void BtnStartAcquireData_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnStopAcquireData_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
