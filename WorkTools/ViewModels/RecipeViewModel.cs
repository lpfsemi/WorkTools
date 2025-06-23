using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CsvHelper;
using Microsoft.Win32;
using PropertyTools.Wpf.Shell32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Documents;
using WorkTools.Models;
using Wpf.Ui.Common.Interfaces;

namespace WorkTools.ViewModels
{
    public partial class RecipeViewModel : ObservableObject, INavigationAware
    {
        [ObservableProperty]
        private string _fileNames;
        [ObservableProperty]
        private string _FileDir;
        [RelayCommand]
        private void OnSelecteDirPath()
        {
            var folderDialog = new BrowseForFolderDialog
            {
                InitialFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (folderDialog.ShowDialog() != true)
            {
                return;
            }

            FileDir = folderDialog.SelectedFolder;

            if (!string.IsNullOrWhiteSpace(FileDir))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(FileDir);

                var files = directoryInfo.GetFiles().ToList();

                foreach (var file in files)
                {
                    List<RecipeStep> steps = new List<RecipeStep>();
                    using var reader = new StreamReader(file.FullName);
                    using var csv = new CsvReader(reader, CultureInfo.CurrentCulture);

                    var recipeBaseRecoeds = csv.GetRecords<RecipeStepBase>();

                    foreach (var item in recipeBaseRecoeds)
                    {
                        var step = new RecipeStep()
                        {
                            Id = item.Id,

                            Name = item.Name,

                            Function = item.Function,

                            Time = item.Time,

                            Temp = item.Temp,

                            O2 = item.O2,

                            N2 = item.N2,
                            Gas3 = item.Gas3,
                            Gas4 = item.Gas4,
                            Gas5 = item.Gas5,
                            RFPower = item.RFPower,
                            VacuumPressure = item.VacuumPressure,
                            EndPointDelect = item.EndPointDelect,
                            RF = item.RF,
                            FanOn = default
                        };
                        steps.Add(step);
                    }
                    csv.Dispose();
                    using var writer = new StreamWriter(file.FullName, false, System.Text.Encoding.UTF8);

                    using var csvw = new CsvWriter(writer, CultureInfo.CurrentCulture);
                    csvw.WriteRecords(steps);

                }
            }
        }
            public void OnNavigatedFrom()
            {

            }

            public void OnNavigatedTo()
            {

            }
        }
    }
