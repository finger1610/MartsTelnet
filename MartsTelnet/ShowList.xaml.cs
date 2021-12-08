using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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
    /// Логика взаимодействия для ShowList.xaml
    /// </summary>
    public partial class ShowList : Window
    {
      //  string _device = "10.222.8.10";
        readonly List<string> _IP;

        public ShowList(List<string> ip)
        {
            _IP = ip;
            InitializeComponent();
            lstboxIP.ItemsSource = _IP;
        }

        

        private void lstboxIP_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //Здесь реализую открытие CLI окна с последующем заходом на железку 
           
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
