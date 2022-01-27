using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace MartsTelnet
{
    /// <summary>
    /// Логика взаимодействия для ShowList.xaml
    /// </summary>
    public partial class ShowList : Window
    {
        string _device = "10.222.8.10";
        readonly List<string> _IP;

        public ShowList(string name,List<string> ip)
        {
           this.Title = name;
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

        private void btnSaveToFile_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new System.Windows.Forms.SaveFileDialog();
            fileDialog.FileName = this.Title;
            fileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Лог файлы(*.log)|*.log|Все файлы (*.*)|*.*" ;
            var result = fileDialog.ShowDialog();
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    var fileName = fileDialog.FileName;
  
                    //Считывание из файла
                    try
                    {
                        using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
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
                        MessageBox.Show("Содержимое сохранено в файле " + fileName);                 
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
    }
    
}
