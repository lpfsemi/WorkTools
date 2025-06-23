using CommunityToolkit.Mvvm.ComponentModel;
using WorkTools.Models;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using Wpf.Ui.Common.Interfaces;
using WorkTools.Models;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.IO;
using System.Collections.ObjectModel;
using CsvHelper;
using System.Globalization;
using System.Runtime.InteropServices;
using MiniExcelLibs;
using ScottPlot.WPF;
using OpenTK.Graphics.OpenGL;
using Wpf.Ui.Controls;
using WorkTools.Helpers;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Windows.Threading;
using ScottPlot.Plottables;
using System.Windows.Documents;
using System.Linq;
using System.Dynamic;
using System.Data;
using System.Text;
using System.Windows;
using WorkTools.Log;
using System.Net.Sockets;
using OfficeOpenXml;
using ClosedXML.Excel;
using Microsoft.VisualBasic.FileIO;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using static ClosedXML.Excel.XLPredefinedFormat;
using OpenTK.Compute.OpenCL;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace WorkTools.ViewModels
{
    public partial class ChartInfo : ObservableObject
    {
        [ObservableProperty]
        private string _name;
        [ObservableProperty]
        private bool _isVisiable;
    }

    public partial class DataInfo : ObservableObject
    {


    }
    public partial class DataViewModel : ObservableObject, INavigationAware
    {
        private bool _isInitialized = false;
        private List<Symbol> _symbols = new List<Symbol>();
        private List<string> InstancePaths = new List<string>();
        private int _maxDataCount = 3600;

        private DispatcherTimer _timer;
        List<(string Name, List<dynamic>)> Signals = new List<(string Name, List<dynamic>)>();

        [ObservableProperty]
        private ObservableCollection<ChartInfo> _signalInfos = new ObservableCollection<ChartInfo>();

        //[ObservableProperty]
        //private WpfPlot _plotControl = new WpfPlot();
        public WpfPlot PlotControl { get; } = new WpfPlot();

        [ObservableProperty]
        private int _interval = 1000;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private IEnumerable<DataFileInfo> _dataFileInfos;

        [ObservableProperty]
        private IEnumerable<DataColor> _colors;

        [ObservableProperty]
        private string _filePath;

        [ObservableProperty]
        private int _count;

        [ObservableProperty]
        private Visibility _configVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private string _btnTitle = "☎";

        [ObservableProperty]
        private ObservableCollection<(string Name, object Value)> _datas = new();

        private void UpdateChart(List<(string address, object value)> values)
        {
            PlotControl.Plot.Clear<Signal>();
            int signalCount = Signals.Count;
            if (Signals[0].Item2.Count > _maxDataCount)
            {
                Signals[0].Item2.RemoveAt(0);
            }
            Signals[0].Item2.Add(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            Count = Signals[0].Item2.Count;
            Datas.Clear();
            for (int i = 1; i < signalCount; i++)
            {
                if (Signals[i].Item2.Count > _maxDataCount)
                {
                    Signals[i].Item2.RemoveAt(0);
                }
                Signals[i].Item2.Add(values[i - 1].value);
                var value = Signals[i].Item2.TakeLast(301);
                if (SignalInfos.FirstOrDefault(a => Signals[i].Name == a.Name).IsVisiable)
                {
                    Signal signal = PlotControl.Plot.Add.Signal(value.ToList());
                    signal.LegendText = Signals[i].Name;
                    Datas.Add((Signals[i].Name, values[i - 1].value));
                }
            }
            PlotControl.Plot.Axes.AutoScale();
            PlotControl.Plot.Axes.AutoScaleExpandY();
            PlotControl.Plot.Axes.AutoScaleExpandX();
            PlotControl.Plot.Axes.SetLimits(left: 0, bottom: 0, right: 300);

            PlotControl.Refresh();
        }

        private void _timer_Tick(object? sender, EventArgs e)
        {
            var values = ADSHelper.Instance().BatchRead(InstancePaths);
            UpdateChart(values);
        }

        private List<(string Name, List<dynamic>)> CreateSignal(List<Symbol> symbols)
        {
            List<(string Name, List<dynamic>)> signals = new() { ("Time", new List<dynamic>()) };
            foreach (var item in symbols)
            {
                if (item.CanRead)
                {
                    signals.Add((item.Name, new List<dynamic>()));
                    SignalInfos.Add(new ChartInfo() { Name = item.Name, IsVisiable = item.IsVisiable });
                }
            }
            return signals;
        }

        public void OnNavigatedTo()
        {
            if (!_isInitialized)
                InitializeViewModel();
        }

        public void OnNavigatedFrom()
        {
        }

        [RelayCommand]
        private void OnSelecteFilePath()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.ShowDialog();

            FilePath = openFileDialog.FileName;
        }

        [RelayCommand]
        private async void OnConverToExcelXLSX()
        {
            if ((!string.IsNullOrWhiteSpace(FilePath)) && File.Exists(FilePath))
            {
                try
                {
                    IsBusy = true;
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    FileInfo fileInfo = new FileInfo(FilePath);
                    string fileName = fileInfo.Name[..^4];

                    string fileDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "DataFromHello");
                    if (!Directory.Exists(fileDir))
                    {
                        Directory.CreateDirectory(fileDir);
                    }

                    string savepath = Path.Combine(fileDir, $"{fileName}.xlsx");
                    if (File.Exists(savepath))
                    {
                        File.Delete(savepath);
                    }

                    string outputpath = Path.Combine(fileDir, $"{fileName}_Number.xlsx");

                  //  await ConvertData(savepath, outputpath);
                    ConvertCSVToExcelXLSX(FilePath, savepath);
                  //  ConvertCSVToXls(FilePath, savepath);
                    stopwatch.Stop();
                    System.Windows.MessageBox.Show($"数据转换成功！耗时：{stopwatch.Elapsed.TotalSeconds}s");
                }
                catch (Exception ex)
                {
                    Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                    messageBox.Show("提示", $"数据转换失败：{ex.Message}");
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        [RelayCommand]
        private async void OnConverToExcelXLS()
        {
            if ((!string.IsNullOrWhiteSpace(FilePath)) && File.Exists(FilePath))
            {
                try
                {
                    IsBusy = true;
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    FileInfo fileInfo = new FileInfo(FilePath);
                    string fileName = fileInfo.Name[..^4];

                    string fileDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "DataFromHello");
                    if (!Directory.Exists(fileDir))
                    {
                        Directory.CreateDirectory(fileDir);
                    }

                    string savepath = Path.Combine(fileDir, $"{fileName}.xls");
                    if (File.Exists(savepath))
                    {
                        File.Delete(savepath);
                    }

                    string outputpath = Path.Combine(fileDir, $"{fileName}_Number.xlsx");

                    //  await ConvertData(savepath, outputpath);
                 //   ConvertCSVToExcelXLSX(FilePath, savepath);
                    ConvertCSVToXls(FilePath, savepath);
                    stopwatch.Stop();
                    System.Windows.MessageBox.Show($"数据转换成功！耗时：{stopwatch.Elapsed.TotalSeconds}s");
                }
                catch (Exception ex)
                {
                    Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                    messageBox.Show("提示", $"数据转换失败：{ex.Message}");
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        private async Task ConvertData(string savepath, string outputPath)
        {
            await Task.Run(async () =>
             {
                 MiniExcel.ConvertCsvToXlsx(FilePath, savepath);
                 MiniExcel.ConvertCsvToXlsx(FilePath, savepath);

                 ConvertTextToNumber(savepath, outputPath);
             });
        }

        private void ConvertCSVToExcelXLSX(string csvFilePath, string excelFilePath)
        {
            using (var workBook = new XLWorkbook())
            {
                var workSheet = workBook.Worksheets.Add("Sheet1");
                using (var parser = new TextFieldParser(csvFilePath))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    parser.HasFieldsEnclosedInQuotes = true;
                    int row = 1;
                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();

                        for (int col = 1; col < fields.Length; col++)
                        {
                            //  workSheet.Cell(row, col).Style.NumberFormat.SetFormat("General");
                            workSheet.Cell(row, col).Value = fields[col];

                        }
                        row++;
                    }
                }
                workBook.SaveAs(excelFilePath);
            }
        }

        private static void ConvertCSVToXls(string csvPath, string xlsPath)
        {
            string[] csvLines = File.ReadAllLines(csvPath);

            HSSFWorkbook workbook = new HSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("Data");

            for (int rowIndex = 0; rowIndex < csvLines.Length; rowIndex++)
            {
                IRow row = sheet.CreateRow(rowIndex);
                string[] rowData = csvLines[rowIndex].Split(',');
                for (int colIndex = 0; colIndex < rowData.Length; colIndex++)
                {
                    row.CreateCell(colIndex).SetCellValue(rowData[colIndex]);
                }
            }

            using (FileStream fs = new FileStream(xlsPath, FileMode.OpenOrCreate))
            {
                workbook.Write(fs);
            }

        }

        private static void ConvertTextToNumber(string inputPath, string outputPath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(new FileInfo(inputPath)))
            {
                var workSheet = package.Workbook.Worksheets[0];
                int rows = workSheet.Dimension.End.Row;
                int cols = workSheet.Dimension.End.Column;
                for (int row = 1; row <= rows; row++)
                {
                    for (int col = 1; col <= cols; col++)
                    {
                        var cell = workSheet.Cells[row, col];
                        if (cell is null)
                        {
                            continue;
                        }

                        //   string claeanValue = cell.Text.Replace(",", "");
                        if (double.TryParse(cell.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double number))
                        {
                            cell.Value = number;
                            cell.Style.Numberformat.Format = "General";
                        }
                    }
                }
                package.SaveAs(new FileInfo(outputPath));
            }
        }

        private static bool IsNumberString(string value)
        {
            return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _);
        }

        [RelayCommand]
        private void StartAcquire()
        {
            if (!_timer.IsEnabled)
            {
                _timer.Interval = TimeSpan.FromMilliseconds(Interval);
                _timer.Start();
            }
        }

        [RelayCommand]
        private void StopAcquire()
        {
            if (_timer.IsEnabled)
            {
                _timer.Stop();
            }
        }

        [RelayCommand]
        private void SaveData()
        {
            if (Signals.Count > 0)
            {
                string fileName = $"Data_{System.DateTime.Now:yyyyMMddHHmmss}.csv";
                if (!Directory.Exists(Constants.Path_Data_File_Dir))
                {
                    Directory.CreateDirectory(Constants.Path_Data_File_Dir);
                }
                string filePath = Path.Combine(Constants.Path_Data_File_Dir, fileName);

                FileHelper.WriteDynaminCvs(filePath, Signals);
            }
        }

        [RelayCommand]
        private void ClearCacheData()
        {
            if (!_timer.IsEnabled)
            {
                foreach (var item in Signals)
                {
                    item.Item2.Clear();
                }
                Count = 0;
                PlotControl.Plot.Clear<Signal>();
                PlotControl.Refresh();
            }
        }

        [RelayCommand]
        private void SaveChartConfig(Visibility? visibility)
        {
            ConfigVisibility = visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            BtnTitle = ConfigVisibility == Visibility.Visible ? "☏" : "☎";
        }

        private void InitializeViewModel()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(_interval)
            };
            _timer.Tick += _timer_Tick;
            _symbols = FileHelper.LoadParamConfig<List<Symbol>>();
            InstancePaths.Clear();
            foreach (var item in _symbols)
            {
                if (item.CanRead)
                {
                    InstancePaths.Add(item.Path);
                }
            }
            Signals = CreateSignal(_symbols);
            PlotControl.Plot.Legend.Alignment = ScottPlot.Alignment.UpperLeft;
            PlotControl.Plot.Legend.BackgroundColor = ScottPlot.Colors.Transparent;
            PlotControl.Plot.Legend.ShadowColor = ScottPlot.Colors.Transparent;
            PlotControl.Plot.DataBackground.Color = ScottPlot.Colors.Transparent;
            PlotControl.Plot.FigureBackground.Color = ScottPlot.Colors.WhiteSmoke;
            _isInitialized = true;
        }
    }
}
