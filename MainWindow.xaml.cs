using ADBeacon.Views;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ADBeacon
{
    public partial class MainWindow : Window
    {
        private PowerShell _ps;
        public static string filePath;

        public static string registryKey;
        public static string registryValue;

        private static string _deviceId;

        public static ObservableCollection<BroadcastItem> broadcasts;
        public MainWindow()
        {
            InitializeComponent();

            _ps = PowerShell.Create();

            UpdateDeviceList();

            broadcasts = new ObservableCollection<BroadcastItem>();
            broadcasts.CollectionChanged += OnBroadcastsChanged;

            const string userRoot = "HKEY_CURRENT_USER";
            const string subkey = "ADBeacon";
            registryKey = "HKEY_CURRENT_USER\\ADBeacon";
            registryValue = "Broadcasts";

            var result = Registry.GetValue(registryKey, registryValue, null);
            if (result != null)
            {
                broadcasts = new ObservableCollection<BroadcastItem>(JsonSerializer.Deserialize<List<BroadcastItem>>(result.ToString()));
            }

            broadcastBtnList.ItemsSource = broadcasts;

        }

        private void OnBroadcastsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            broadcastBtnList.ItemsSource = broadcasts;
        }


        private void broadcastBtn_Click(object sender, RoutedEventArgs e)
        {
            // Set our broadcast info
            Button btn = sender as Button;
            TextBlock label = (TextBlock)btn.Content;

            var broadcast = broadcasts.Where(b => b.Label == label.Text).FirstOrDefault();

            resultIntent.Text = $"INTENT: {broadcast.Intent}\n";
            resultCategory.Text = $"CATEGORY: {broadcast.Category}\n";
            StringBuilder esSb = new StringBuilder();
            esSb.AppendLine("EXTRAS:");
            foreach (var extra in broadcast.Extras)
            {
                var value = Regex.Replace(extra.Value, @"\\ ", " ");
                esSb.AppendLine($"{extra.Key}, {value}");
            }
            resultExtras.Text = esSb.ToString();

            try
            {
                // Attempt find devices
                _ps.Commands.Clear();
                _ps.AddCommand("Invoke-Expression");
                _ps.AddArgument("adb devices");

                var res = _ps.Invoke();
                if (res.Count <= 1)
                {
                    // do something?
                } else
                {
                    _ps.Commands.Clear();
                    adbResponse.Children.Clear();
                    adbResponse.Children.Add(new TextBlock { 
                        Text = "RESPONSE:",
                        Foreground = new SolidColorBrush(Colors.White)
                    });

                    var argsBuilder = new StringBuilder();
                    argsBuilder.Append("adb ");
                    if (_deviceId != null && _deviceId.Trim().Length > 0)
                    {
                        argsBuilder.Append($"-s {_deviceId} ");
                    }
                    argsBuilder.Append($"shell am broadcast -a {broadcast.Intent} -c {broadcast.Category}");

                    if (broadcast.Extras.Count > 0)
                    {
                        foreach (var extra in broadcast.Extras)
                        {
                            argsBuilder.Append($" --es {extra.Key} {extra.Value}");
                        }
                    }

                    //Console.WriteLine(argsBuilder.ToString());


                    _ps.AddCommand("Invoke-Expression");
                    _ps.AddArgument(argsBuilder.ToString());

                    res = _ps.Invoke();
                    foreach (var item in res)
                    {
                        TextBlock text = new TextBlock();
                        text.Text = item.ToString();
                        text.Foreground = new SolidColorBrush(Colors.White);
                        text.Margin = new Thickness(0,0,0,10);
                        text.TextWrapping = TextWrapping.Wrap;

                        adbResponse.Children.Add(text);
                    }
                }


            } catch (Exception ex)
            {
                // do something?
            }
           
        }

        private void addBroadcastBtn_Click(object sender, RoutedEventArgs e)
        {
            var page = new AddBroadcastItem();
            page.Show();
        }

        private void broadcastBtnContextUpdate_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var ctxMenu = menuItem.Parent as ContextMenu;
            var btn = ctxMenu.PlacementTarget as Button;

            TextBlock label = (TextBlock)btn.Content;

            var broadcast = broadcasts.Where(b => b.Label == label.Text).FirstOrDefault();

            var page = new EditBroadcastItem(broadcast);
            page.Show();
        }

        private void broadcastBtnContextDelete_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var ctxMenu = menuItem.Parent as ContextMenu;
            var btn = ctxMenu.PlacementTarget as Button;

            TextBlock label = (TextBlock)btn.Content;

            var broadcast = broadcasts.ToList().Where(b => b.Label == label.Text).FirstOrDefault();
            broadcasts.Remove(broadcast);

            Save();

        }

        public static void Save()
        {
            Registry.SetValue(registryKey, registryValue, JsonSerializer.Serialize(broadcasts));
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            _deviceId = textBox.Text;
        }

        public void UpdateDeviceList ()
        {
            _ps.Commands.Clear();
            _ps.AddCommand("Invoke-Expression");
            _ps.AddArgument("adb devices");
            var res = _ps.Invoke();
            if (res.Count > 0)
            {
                deviceListStack.Children.Clear();

                for (int i = 1; i < res.Count; i++)
                {

                    var row = res[i].ToString().Split('\t')[0];
                    if (row.Trim().Length > 0 )
                    {

                        var deviceBtn = new Button();
                        deviceBtn.Content = row;
                        deviceBtn.ToolTip = row;
                        deviceBtn.Style = Application.Current.FindResource("MaterialDesignFlatButton") as Style;
                        deviceBtn.Click += (s, e) =>
                        {
                            deviceTextBox.Text = row;
                            _deviceId = row;

                        };
                        deviceListStack.Children.Add(deviceBtn);
                    }
                   
                }
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UpdateDeviceList();
        }
    }
}
