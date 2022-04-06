using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;


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
                rtxtboxCommands.Selection.Text += str + "\r";
            }
            rtxtboxCommands.Selection.Text += "\n";
        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            _commands.Clear();
            string str = new TextRange(rtxtboxCommands.Document.ContentStart, rtxtboxCommands.Document.ContentEnd).Text;

            string[] lines = str.Split(new char[] { '\r', '\n' }); 
            foreach (string row in lines)
            {
                if (row != "")
                    _commands.Add(row);
            }
            _commands.Add("");
            this.Close();
        }

        private void btnFormat_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            _commands.Clear();
            this.Close();
        }
    }
}
