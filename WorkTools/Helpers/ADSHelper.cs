using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.Ads.SumCommand;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.TypeSystem;

namespace WorkTools.Helpers
{
    public class ADSHelper
    {
        public static readonly string netId = ConfigurationManager.AppSettings["netId"].ToString();
        public static int port = int.Parse(ConfigurationManager.AppSettings["port"]);
        private static AdsClient adsClient;
        private static ADSHelper Helper { get; set; }
        private static object _lock = new object();
        private ADSHelper()
        {
        }

        private static bool Connect()
        {
            adsClient = new AdsClient();
            adsClient?.Connect(netId, port);
            return IsRun();
        }

        public static bool IsRun()
        {
            try
            {
                StateInfo? stateInfo = adsClient?.ReadState();
                if (stateInfo is StateInfo state && (state.AdsState == AdsState.Run || state.AdsState == AdsState.Start || state.AdsState == AdsState.Init))
                {
                    GetAllDefaults();
                    return true;
                }
                else
                {
                    if (!needGetAllDefaults)
                        needGetAllDefaults = true;
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static ADSHelper Instance()
        {
            if (Helper != null)
            {
                return Helper;
            }

            lock (_lock)
            {
                Helper ??= new ADSHelper();
                Connect();
            }
            return Helper;
        }

        public static bool Connected
        {
            get { return adsClient?.IsConnected ?? false; }
        }

        public bool ReConnect()
        {
            if (Connected)
            {
                return true;
            }

            return Connect();
        }

        private static bool needGetAllDefaults = true;

        private static ISymbolCollection<ISymbol> allSymbols;
        private static Dictionary<string, DataType> allDataTypes; //

        private static void GetAllDefaults()
        {
            if (needGetAllDefaults || allSymbols == null || allSymbols.Count == 0 || allDataTypes == null || allDataTypes.Count == 0)
            {
                //ADS Client Load symbolic and DataType information 初始化
                //SymbolLoaderSettings settings = new SymbolLoaderSettings(SymbolsLoadMode.VirtualTree, ValueAccessMode.IndexGroupOffsetPreferred);
                //ISymbolLoader loader = SymbolLoaderFactory.Create(AdsClient, settings);

                ISymbolLoader loader = SymbolLoaderFactory.Create(adsClient, SymbolLoaderSettings.Default);
                allSymbols = loader.Symbols;
                allDataTypes = loader.DataTypes.ToDictionary(i => i.Name, i => (DataType)i);

                if (allSymbols.Count > 0 && allDataTypes.Count > 0)
                    needGetAllDefaults = false;
            }
        }

        private SymbolCollection GetSymbol(List<string> Symbols)
        {
            SymbolCollection symbolCollection = new SymbolCollection();

            try
            {
                foreach (string symbol in Symbols)
                {
                    if (!symbolCollection.Contains(allSymbols[symbol]))
                    {
                        symbolCollection.Add(allSymbols[symbol]);
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return symbolCollection;
        }

        public List<(string address, object value)> BatchRead(List<string> InstancePaths)
        {
            List<(string address, object value)> result = new();
            SymbolCollection symbols = GetSymbol(InstancePaths);

            if (symbols.Count <= 0)
            {
                return result;
            }
            SumSymbolRead readCommand = new SumSymbolRead(adsClient, symbols);
            try
            {
                object[] values = readCommand.Read();
                int addrCount = symbols.Count;
                if (addrCount == values?.Length)
                {
                    for (int i = 0; i < addrCount; i++)
                    {
                        result.Add((InstancePaths[i], values[i]));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return result;
        }


        public T? Read<T>(string InstancePath)
        {
            if (!adsClient?.IsConnected ?? false)
            {
                Connect();
                if (!IsRun())
                {
                    return default;
                }
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
            }
            return value;
        }
    }
}
