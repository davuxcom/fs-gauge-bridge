﻿using BridgeClient.DataModel;
using BridgeClient.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BridgeClient
{
    public partial class App : Application
    {
        private SimpleHTTPServer _server;

        private Window _logWindow = null;
        private Window _varWindow = null;
        private LogWindowViewModel _logVm = new LogWindowViewModel();
        private SimConnectViewModel _simConnect;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var tracer = new UITraceListener();
            Trace.Listeners.Add(tracer);
            tracer.Message += (msg) => _logVm.AddMessage(msg);

            try
            {
                var settings = JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText("settings.json"));
                var vfs = new VFS(settings.VFS);

                CfgManager.Initialize(vfs);

                _simConnect = new SimConnectViewModel();

                var mainWindowViewModel = new MainWindowViewModel(new RelayCommand(OpenLog), new RelayCommand(OpenVarList));
                mainWindowViewModel.SimConnect = _simConnect;

                var window = new MainWindow { DataContext = mainWindowViewModel };
                window.Show();

                // Access is denied
                // netsh http add urlacl url="http://+:4200/" user=everyone
                _server = new SimpleHTTPServer(vfs, settings.Webserver);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\n\n{ex}");
            }
        }

        internal void Navigate(int index)
        {
            ProcessHelper.Start($"{_server.Url}Pages/VCockpit/Core/VCockpit.html?id={index}");
        }

        private void OpenLog()
        {
            if (_logWindow != null)
            {
                _logWindow.Topmost = true;
                _logWindow.Topmost = false;
            }
            else
            {
                _logWindow = new LogWindow { DataContext = _logVm };
                _logWindow.Closed += (_, __) => _logWindow = null;
                _logWindow.Show();
            }
        }

        private void OpenVarList()
        {
            if (_varWindow != null)
            {
                _varWindow.Topmost = true;
                _varWindow.Topmost = false;
            }
            else
            {
                _varWindow = new VariableListWindow { DataContext = new VariableListWindowViewModel(_simConnect) };
                _varWindow.Closed += (_, __) => _varWindow = null;
                _varWindow.Show();
            }
        }
    }
}
