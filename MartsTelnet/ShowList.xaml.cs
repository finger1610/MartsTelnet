using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MartsTelnet
{
    /// <summary>
    /// Логика взаимодействия для ShowList.xaml
    /// </summary>
    public partial class ShowList : Window
    {

        readonly string _message;
      readonly  List<string> _IP;
      readonly Dictionary<string,string> _IpPlus;
        bool _isOkCancel = true;

        public bool isOkCancel { get { return _isOkCancel; } }

        public ShowList(string name,Dictionary<string,string> ip)
        {
           this.Title = name;
            _IpPlus = new Dictionary<string, string>(ip);
            _IP = new List<string>();
            foreach (string str in _IpPlus.Keys)
            {
                _IP.Add(str);
            }
            InitializeComponent();
            showData();
        }

        
        public ShowList(string name, List<string> ip)
        {
            this.Title = name;
            _IP = ip;
            InitializeComponent();
            chkBoxShowKey.Visibility = Visibility.Hidden;
            chkBoxShowKey.IsEnabled= false;
            showData();
        }

        
        public ShowList(string name, string message) //Вместо MessageBox
        {
            this.Title = name;
            InitializeComponent();
            lstboxIP.Items.Add(message);
            chkBoxShowKey.Visibility = Visibility.Hidden;
            chkBoxShowKey.IsEnabled = false;
            btnCancel.Visibility = Visibility.Visible;
            btnCancel.IsEnabled = true;
           
         
        }



        private void showData() //для подмены листа с адресами на лист с адресами+значениями
        {
           // lstboxIP.Items.Clear();
            if (chkBoxShowKey.IsChecked.Value)
            {
                lstboxIP.ItemsSource = _IpPlus;
            }
            else
                lstboxIP.ItemsSource = _IP;
        }

        private void lstboxIP_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //Здесь реализую открытие CLI окна с последующем заходом на железку 
           
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            _isOkCancel = true;
            this.Close();
        }

        private void btnSaveToFile_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new System.Windows.Forms.SaveFileDialog();
            fileDialog.FileName = this.Title;
            fileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Лог файлы(*.log)|*.log|Все файлы (*.*)|*.*" ;
            var result = fileDialog.ShowDialog();
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK: 
                    //запись в файл
                    try
                    {
                        using (FileStream fs = new FileStream(fileDialog.FileName, FileMode.OpenOrCreate))
                        {
                            using (StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8))
                            {
                               foreach (string iter in _IP)
                               {
                                    sw.WriteLine(iter);
                                    sw.WriteLine("##############################");
                               }
                            }
                        }
                        MessageBox.Show("Содержимое сохранено в файле " + fileDialog.FileName);                 
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Возникла ошибка при записи:\n" + ex.Message.ToString());
                    }
                    break;
     
                default:

                    break;
            }
        }

        private void chkBoxShowKey_Checked(object sender, RoutedEventArgs e)
        {
            showData();
        }

        private void chkBoxShowKey_Unchecked(object sender, RoutedEventArgs e)
        {
            showData();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            _isOkCancel = false;
            this.Close();
        }
    }
    
}
