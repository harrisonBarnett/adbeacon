using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;

namespace ADBeacon.Views
{
    /// <summary>
    /// Interaction logic for AddBroadcastItem.xaml
    /// </summary>
    public partial class AddBroadcastItem : Window
    {
        public AddBroadcastItem()
        {
            InitializeComponent();
        }

        private void confirmAddBtn_Click(object sender, RoutedEventArgs e)
        {
            if (addBroadcastLabel.Text == null || addBroadcastLabel?.Text.Trim().Length == 0
                || addBroadcastIntent.Text == null || addBroadcastIntent?.Text.Trim().Length == 0
                || addBroadcastCategory.Text == null || addBroadcastCategory?.Text.Trim().Length == 0)
            {
                MessageBox.Show("Kindly add a label, intent and category.");
            } else if (MainWindow.broadcasts.Where(b => b.Label == addBroadcastLabel.Text).Count() > 0) 
            {
                MessageBox.Show("This name is already in use.");
            }
            else
            {
                BroadcastItem newBroadcastItem = new BroadcastItem();
                newBroadcastItem.Label = addBroadcastLabel.Text.Trim();
                newBroadcastItem.Description = addBroadcastDescription?.Text.Trim();
                newBroadcastItem.Intent = addBroadcastIntent.Text.Trim();
                newBroadcastItem.Category = addBroadcastCategory.Text.Trim();

                if (addBroadcastExtras.Text != null && addBroadcastExtras.Text.Trim().Length > 0)
                {
                    try
                    {
                        List<Extra> extras = new List<Extra>();
                        var rows = addBroadcastExtras.Text.Split('\n');
                        foreach (var row in rows)
                        {
                            if (row.Trim().Length > 0)
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

                MainWindow.broadcasts.Add(newBroadcastItem);

                MainWindow.Save();

                //using (StreamWriter sw = new StreamWriter(MainWindow.filePath, false))
                //{
                //    sw.Write(JsonSerializer.Serialize(MainWindow.broadcasts));
                //}
                this.Close();
            }
        }
    }
}
