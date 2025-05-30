﻿using DecisionTableMakerApp.ViewModel;
using System.Windows;

namespace DecisionTableMakerApp.View
{
    public partial class ExportInfoWindow : Window
    {
        private ExportInfoWindowViewModel _vm;
        public string Inspection { get => _vm.InspectionText.Value; }

        public ExportInfoWindow()
        {
            InitializeComponent();

            _vm = new ExportInfoWindowViewModel();
            DataContext = _vm;

            // 起動時に検査観点のテキストボックスにフォーカスを設定
            Loaded += (s, e) => FocusFirstTextBox();
        }

        private void FocusFirstTextBox()
        {
            InspectionTextBox.Focus();
        }
    }
}
