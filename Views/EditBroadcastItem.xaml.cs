using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;

namespace ADBeacon.Views
{
    public partial class EditBroadcastItem : Window
    {
        BroadcastItem _currentBroadcast;
        public EditBroadcastItem(BroadcastItem broadcast)
        {
            InitializeComponent();

            _currentBroadcast = broadcast;
            editBroadcastLabel.Text = _currentBroadcast.Label;
            editBroadcastDescription.Text = _currentBroadcast.Description;
            editBroadcastIntent.Text = _currentBroadcast.Intent;
            editBroadcastCategory.Text = _currentBroadcast.Category;
            var sb = new StringBuilder();
            foreach (var extra in _currentBroadcast.Extras)
            {
                var value = Regex.Replace(extra.Value, @"\\ ", " ");
                sb.AppendLine($"{extra.Key},{value}");
            }
            editBroadcastExtras.Text = sb.ToString();
        }

        private void confirmEditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (editBroadcastLabel.Text == null || editBroadcastLabel?.Text.Trim().Length == 0
                || editBroadcastIntent.Text == null || editBroadcastIntent?.Text.Trim().Length == 0
                || editBroadcastCategory.Text == null || editBroadcastCategory?.Text.Trim().Length == 0)
            {
                MessageBox.Show("Kindly add a label, intent and category.");
            }
            else
            {
                BroadcastItem newBroadcastItem = new BroadcastItem();
                newBroadcastItem.Label = editBroadcastLabel.Text.Trim();
                newBroadcastItem.Description = editBroadcastDescription?.Text.Trim();
                newBroadcastItem.Intent = editBroadcastIntent.Text.Trim();
                newBroadcastItem.Category = editBroadcastCategory.Text.Trim();

                if (editBroadcastExtras.Text != null && editBroadcastExtras.Text.Trim().Length > 0)
                {
                    try
                    {
                        List<Extra> extras = new List<Extra>();
                        var rows = editBroadcastExtras.Text.Split('\n');
                        foreach (var row in rows)
                        {
                            if(row.Trim().Length > 0)
                            {
                                var split = row.Split(',');
                                var key = split[0].Trim();
                                var value = Regex.Replace(split[1].Trim(), " ", @"\ ");

                                extras.Add(new Extra
                                {
                                    Key = key,
                                    Value = value
                                });
                            }

                        }
                        newBroadcastItem.Extras = extras;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Something went wrong: {ex.Message}");
                    }
                }

                MainWindow.broadcasts.Remove(_currentBroadcast);
                MainWindow.broadcasts.Add(newBroadcastItem);

                MainWindow.Save();

                this.Close();
            }
        }
    }
}
