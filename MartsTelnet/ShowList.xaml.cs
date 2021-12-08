using System;
using System.Collections.Generic;
using System.IO;
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
        string _device = "10.222.8.10";
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

        private void btnSaveToFile_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new System.Windows.Forms.SaveFileDialog();
            fileDialog.FileName = "file";
            fileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Лог файлы(*.log)|*.log|Все файлы (*.*)|*.*" ;
            var result = fileDialog.ShowDialog();
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    var file = fileDialog.FileName;
  
                    //Считывание из файла
                    try
                    {
                        using (FileStream fs = new FileStream(file, FileMode.OpenOrCreate))
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
                        MessageBox.Show("Содержимое сохранено в файле " + file);
                        
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
