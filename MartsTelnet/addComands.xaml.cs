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
    /// Логика взаимодействия для addComands.xaml
    /// </summary>
    public partial class addComands : Window
    {

        List<string> _commands;
        public addComands(List<string> commands)
        {
            _commands = commands;
            InitializeComponent();
            rtxtboxCommands.Selection.Text = "";
            foreach (string str in _commands)
            {
                rtxtboxCommands.Selection.Text += str + "\r ";
            }
            rtxtboxCommands.Selection.Text += "\n";
        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            _commands.Clear();
            string str = new TextRange(rtxtboxCommands.Document.ContentStart, rtxtboxCommands.Document.ContentEnd).Text;

            string[] lines = str.Split(new char[] { '\r', '\n' });
            var point = rtxtboxCommands.Document.ContentStart;

            foreach (var row in lines)
            {
                _commands.Add(row);
            }
            this.Close();
        }

    }
}
