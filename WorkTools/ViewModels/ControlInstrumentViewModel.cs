﻿using CommunityToolkit.Mvvm.ComponentModel;
using WorkTools.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Wpf.Ui.Common.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics.Metrics;
using Microsoft.Win32;

namespace WorkTools.ViewModels
{
    public partial class ControlInstrumentViewModel : ObservableObject, INavigationAware
    {
        private bool _isInitialized = false;

        [ObservableProperty]
        private IEnumerable<DataColor> _colors;

        public void OnNavigatedTo()
        {
            if (!_isInitialized)
                InitializeViewModel();
        }

        public void OnNavigatedFrom()
        {
        }


        private void InitializeViewModel()
        {
            var random = new Random();
            var colorCollection = new List<DataColor>();

            for (int i = 0; i < 8192; i++)
                colorCollection.Add(new DataColor
                {
                    Color = new SolidColorBrush(Color.FromArgb(
                        (byte)200,
                        (byte)random.Next(0, 250),
                        (byte)random.Next(0, 250),
                        (byte)random.Next(0, 250)))
                });

            Colors = colorCollection;

            _isInitialized = true;
        }
    }

}
