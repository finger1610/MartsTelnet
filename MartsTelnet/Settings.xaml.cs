using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MartsTelnet
{
    /// <summary>
    /// Логика взаимодействия для Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        List<uint> _timings;
        public Settings(List <uint> timings)
        {
            _timings = timings;

            InitializeComponent();
           

            if (_timings.Count == 2)
            {
                txtBoxResponseLogPass.Text = _timings[0].ToString();
                txtBoxResponseCommand.Text = _timings[1].ToString();
            }
            else
            {
                txtBoxResponseLogPass.Text = "10";
                txtBoxResponseCommand.Text = "10";
            }
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            _timings.Clear();
            _timings.Add(Convert.ToUInt32(txtBoxResponseLogPass.Text.ToString()));
            _timings.Add(Convert.ToUInt32(txtBoxResponseCommand.Text.ToString()));
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            _timings.Clear();
            this.Close();
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            txtBoxResponseLogPass.Text = "10";
            txtBoxResponseCommand.Text = "10";
        }
    }
}
