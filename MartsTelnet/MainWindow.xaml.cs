using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

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

        //для секундомера
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        Stopwatch stopWatch = new Stopwatch();
        string curentTime = string.Empty;

        public MainWindow()
        {
            _devices = new List<string>();
            _log = new List<string>();
            _commands = new List<string>();
            _fails = new List<string>();
            InitializeComponent();
            dispatcherTimer.Tick += new EventHandler(dt_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
        }

        //для секундомера
        void dt_Tick(object sender,EventArgs e)
        {
            if (stopWatch.IsRunning)
            {
                TimeSpan ts = stopWatch.Elapsed;
                curentTime = string.Format("{0:00}:{1:00}:{2:00}",
                    ts.Minutes, ts.Seconds, ts.Milliseconds/10);
                lblClock.Content = curentTime;
            }
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
            txtboxFilter.IsEnabled = status;    
            txtboxIP.IsEnabled = status;
            btnFileDialog.IsEnabled = status;
            btncheckList.IsEnabled = status;

            txtboxPort.IsEnabled = status;
            btnAddComands.IsEnabled = status;

            btnTestConnect.IsEnabled = status;
            btnRun.IsEnabled = status;
            btnShowLog.IsEnabled = status;

            btnReset.IsEnabled = status;
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
            ShowList showList = new ShowList("Log", _log);
            showList.ShowDialog();
        }
        private void btnFileDialog_Click(object sender, RoutedEventArgs e)
        {
           
                var fileDialog = new System.Windows.Forms.OpenFileDialog();
                var result = fileDialog.ShowDialog();

            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    txtboxIP.Text = fileDialog.FileName;
                    txtboxIP.ToolTip = fileDialog.FileName;

                    //Считывание из файла
                    _devices.Clear();
                    string tmp = string.Empty;
                    try
                    {
                        using (FileStream fs = new FileStream(txtboxIP.Text, FileMode.Open))
                        {
                            using (StreamReader sr = new StreamReader(fs, System.Text.Encoding.UTF8))
                            {
                                while (!sr.EndOfStream)
                                {
                                    //Добавить проверку на наличие IP адреса
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
            ShowList obj = new ShowList("List IP addresses",_devices);
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
            btnFails.Visibility = Visibility.Hidden;

            lblProgress.Content = prgBar.Value + "/" + prgBar.Maximum;
            lblStatus.Content = "";
            lblStatus.Foreground = Brushes.Black;
            //clock
            stopWatch.Reset();
            lblClock.Content = "00:00:00";

            bool checkBox;
            //отключаем элементы управления
            isEnableElements(false);

            //включаем отображение прогресс бара и счетчика
            prgBar.Visibility = Visibility.Visible;
            lblProgress.Visibility = Visibility.Visible;
            btnStop.Visibility = Visibility.Visible;
            btnStop.IsEnabled = true;

            myTelnet session = new myTelnet(txtoxLogin.Text, txtboxPassword.Password,"",Convert.ToInt32(txtboxPort.Text),txtboxFilter.Text);
            session.addComands(_commands);

            stopWatch.Start();
            dispatcherTimer.Start();

            foreach (string device in _devices)
            {
                lblStatus.Content = device + " - Отправка...";
                if (!isRun)
                {
                    stopWatch.Stop();
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
                    lblProgress.Content = (_log.Count + 1 - _fails.Count).ToString() + "/" + prgBar.Maximum;
                }
                else
                {
                    lblStatus.Content = device + " - Ошибка";
                    _fails.Add(session._log);
                    btnFails.Visibility = Visibility.Visible;
                    btnFails.Content = " Ошибки: " + _fails.Count().ToString();
                }
                _log.Add((_log.Count + 1).ToString() + ") " + session._log);
                prgBar.Value++;
            }

            if (stopWatch.IsRunning)
            {
                stopWatch.Stop();
            }
          //  elapsedtimeitem.Items.Add(curentTime);
            

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
            ShowList showList = new ShowList("Errors",_fails);
            showList.Show();
        }
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            isRun = false;
        }
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            txtoxLogin.Text = "";
            txtboxPassword.Password = "";
            txtboxIP.Text = "";
            txtboxPort.Text = "23";
            txtboxFilter.Text = "";
            _commands.Clear();
            _devices.Clear();
            _fails.Clear();
            _log.Clear();

            btnStop.Visibility = Visibility.Hidden;
            btnFails.Visibility = Visibility.Hidden;
            btnShowLog.IsEnabled = false;

            prgBar.Visibility = Visibility.Hidden;
            lblComAdd.Visibility = Visibility.Hidden;
            lblProgress.Content = "";
            lblStatus.Content = "";
            lblFindIP.Content = "";
            
            lblClock.Content = "";
            checkComlete();
        }
    }
}
