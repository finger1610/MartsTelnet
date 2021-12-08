using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MartsTelnet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> _devices;
        List<string> _commands;
        List<string> _log;
        List<string> _fails;
        bool isRun = true;//флаг для остановки процесса внутри btnConnect_Click

        public MainWindow()
        {
            _devices = new List<string>();
            _log = new List<string>();
            _commands = new List<string>();
            _fails = new List<string>();
            InitializeComponent();
        }


        /// <summary>
        /// Проверяет заполненность полей для запуска скрипта
        /// </summary>
        private void checkComlete()
        {
            if (btnRun != null &&
            btnTestConnect != null &&
            chkBoxShowRes != null)
                if (txtoxLogin.Text != "" &&
                    txtboxPassword.Password != "" &&
                    _devices.Count != 0 &&
                    _commands.Count != 0)
                {
                    btnRun.IsEnabled = true;
                    btnTestConnect.IsEnabled = true;
                    chkBoxShowRes.IsEnabled = true;
                }
                else
                {
                    btnRun.IsEnabled = false;
                    btnTestConnect.IsEnabled = false;
                    chkBoxShowRes.IsEnabled = false;
                }
        }

        /// <summary>
        /// переключает доступность элементов
        /// </summary>
        /// <param name="status"> elements.IsEnabled = true </param>
        private void isEnableElements(bool status) 
        {
            txtoxLogin.IsEnabled = status;
            txtboxPassword.IsEnabled = status;

            txtboxIP.IsEnabled = status;
            btnFileDialog.IsEnabled = status;
            btncheckList.IsEnabled = status;

            txtboxPort.IsEnabled = status;
            btnAddComands.IsEnabled = status;

            btnTestConnect.IsEnabled = status;
            btnRun.IsEnabled = status;
            btnShowLog.IsEnabled = status;

        }
    

        //Изменения тектБоксов
        private void txtoxLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            checkComlete();

        }
        private void txtboxPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            checkComlete();
        }
        private void txtboxIP_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtboxIP.Text != "")
                btncheckList.IsEnabled = true;
            else
                btncheckList.IsEnabled = false;
            checkComlete();
        }
        private void txtboxPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            checkComlete();
        }

        //Buttons_click
        private void btnAddComands_Click(object sender, RoutedEventArgs e)
        {
            addComands addComands = new addComands(_commands);
            addComands.ShowDialog();
            if (_commands.Count != 0)
                lblComAdd.Visibility = Visibility.Visible;
            checkComlete();
        }
        private void btnShow_Click(object sender, RoutedEventArgs e)
        {
            ShowList showList = new ShowList(_log);
            showList.ShowDialog();
        }
        private void btnFileDialog_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new System.Windows.Forms.OpenFileDialog();
            var result = fileDialog.ShowDialog();
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    var file = fileDialog.FileName;
                    txtboxIP.Text = file;
                    txtboxIP.ToolTip = file;

                    //Считывание из файла
                    _devices.Clear();
                    try
                    {
                        using (FileStream fs = new FileStream(txtboxIP.Text, FileMode.Open))
                        {
                            using (StreamReader sr = new StreamReader(fs, System.Text.Encoding.UTF8))
                            {
                                while (!sr.EndOfStream)
                                {
                                    _devices.Add(sr.ReadLine());
                                }
                            }
                        }
                        checkComlete();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Возникла ошибка при чтении:\n" + ex.Message.ToString());
                    }


                    lblFindIP.Visibility = Visibility.Visible;
                    lblFindIP.Content = $"Найдено " + _devices.Count + " строк";
                    btncheckList.IsEnabled = true;
                    break;
                case System.Windows.Forms.DialogResult.Cancel:
                default:
                    txtboxIP.Text = "";
                    txtboxIP.ToolTip = null;

                    lblFindIP.Visibility = Visibility.Hidden;
                    btncheckList.IsEnabled = false;
                    break;
            }
        }
        private void btncheckList_Click(object sender, RoutedEventArgs e)
        {
            ShowList obj = new ShowList(_devices);
            obj.Show();
        }
        private void btnTestConnect_Click(object sender, RoutedEventArgs e)
        {
            isEnableElements(false);
            myTelnet session = new myTelnet(txtoxLogin.Text, txtboxPassword.Password, _devices[0]);
            session.addComands(_commands);
            session.runCommands(true);
            isEnableElements(true);
        }
        private async void btnRun_Click(object sender, RoutedEventArgs e)
        {
            //Обнуляем значения
            _log.Clear();
            _fails.Clear();
            prgBar.Value = 0;
            isRun = true;
            prgBar.Maximum = _devices.Count;
            lblProgress.Content = prgBar.Value + "/" + prgBar.Maximum;
            lblStatus.Content = "";
            lblStatus.Foreground = Brushes.Black;

            bool checkBox;
            //отключаем элементы управления
            isEnableElements(false);

            //включаем отображение прогресс бара и счетчика
            prgBar.Visibility = Visibility.Visible;
            lblProgress.Visibility = Visibility.Visible;
            btnStop.Visibility = Visibility.Visible;
            btnStop.IsEnabled = true;

            myTelnet session = new myTelnet(txtoxLogin.Text, txtboxPassword.Password,"",Convert.ToInt32(txtboxPort.Text));
            session.addComands(_commands);

            foreach (string device in _devices)
            {
                lblStatus.Content = device + " - Отправка...";
                if (!isRun)
                {
                    lblStatus.Content = device + " - Процесс остановлен";
                    MessageBox.Show(lblStatus.Content.ToString());
                    _log.Add((_log.Count + 1).ToString() + ")" + lblStatus.Content);

                    break;
                }

                session._ip = device;

                checkBox  = chkBoxShowRes.IsChecked.Value;//если кинуть напрямую в метод, то выдает ошибку доступа

                if (await Task.Run(() =>  session.runCommands(checkBox)))
                {
                    lblStatus.Content = device + " - Отправлено";
                    _log.Add((_log.Count + 1).ToString() + ") "+ session.log);
                    lblProgress.Content = _log.Count + "/" + prgBar.Maximum;
                }
                else
                {
                    lblStatus.Content = device + " - Ошибка";
                    _fails.Add(session.log);
                    _log.Add((_log.Count + 1).ToString() + ") " + session.log);
                    btnFails.Visibility = Visibility.Visible;
                    btnFails.Content = " Ошибки: " + _fails.Count().ToString();
                }
                prgBar.Value++;
            }


            isEnableElements(true);
            btnStop.Visibility = Visibility.Hidden;
            btnStop.IsEnabled = false;
            if (isRun)
            {
                lblStatus.Content = "Отправка завершена";
                lblStatus.Foreground = Brushes.Green;
            }
            if (_fails.Count != 0)
                btnFails.IsEnabled = true;

        }
        private void btnFails_Click(object sender, RoutedEventArgs e)
        {
            ShowList showList = new ShowList(_fails);
            showList.ShowDialog();
        }
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            isRun = false;
        }

    }
}
