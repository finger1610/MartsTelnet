using System;
using System.Collections.Generic;
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
        Dictionary<string, string> _devices;//коллекция для сохрания ip адреса и строки под замену в комманде (динамическая замена)
        List<string> _commands;
        List<string> _additCommands;
        List<string> _log; //общий лог со всей инфой
        List<string> _logSuccess;//лог успешных заходов
        List<string> _fails;//лог ошибок
        bool isRun = true;//флаг для остановки процесса внутри btnConnect_Click
        string _key = "%%%";

        List<uint> _timings;


        //для секундомера
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        Stopwatch stopWatch = new Stopwatch();
        string curentTime = string.Empty;

        public MainWindow()
        {
            _devices = new Dictionary<string, string>();
            _log = new List<string>();
            _commands = new List<string>();
            _additCommands = new List<string>();

            _fails = new List<string>();

            _logSuccess = new List<string>();

            _timings = new List<uint>();

            InitializeComponent();
            chkBoxDinChangeToopTip.Content = "Будет менять фрагмент " + _key + " на на текст из файла с IP адресами";

            dispatcherTimer.Tick += new EventHandler(dt_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
        }

        //для секундомера
        void dt_Tick(object sender, EventArgs e)
        {
            if (stopWatch.IsRunning)
            {
                TimeSpan ts = stopWatch.Elapsed;
                curentTime = string.Format("{0:00}:{1:00}:{2:00}:{3:00}",
                   ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
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
                    _devices.Count != 0)
                //  && _commands.Count != 0) //закомитил для возможности запуска программы без команд (т.е тупо авторизация)
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

            //Проверка на наличие ключа
            if (_commands.Count != 0)
            {
                lblComAdd.Visibility = Visibility.Visible;
                foreach (string str in _commands)
                {
                    if (str.Contains(_key))
                    {
                        chkBoxDinChange.IsEnabled = true;
                        break;
                    }

                    chkBoxDinChange.IsEnabled = false;

                }
            }
            else
                lblComAdd.Visibility = Visibility.Hidden;
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

            txtboxFilter.IsEnabled = status;
            txtboxWait.IsEnabled = status;
            btnCommandAdterfind.IsEnabled = status;


            btnFileDialog.IsEnabled = status;
            btncheckList.IsEnabled = status;

            txtboxPort.IsEnabled = status;
            btnAddComands.IsEnabled = status;
            chkBoxSelective.IsEnabled = status;
            chkBoxDinChange.IsEnabled = status;

            btnTestConnect.IsEnabled = status;
            btnRun.IsEnabled = status;
            btnShowLog.IsEnabled = status;
            btnShowLogSuccess.IsEnabled = status;
            btnFails.IsEnabled = status;
            chkBoxSaveLogs.IsEnabled = status;

            btnReset.IsEnabled = status;
            btnSettings.IsEnabled = status;
        }
        private void saveLog()
        {
            string[] tmp = txtboxIP.Text.Split("\\");
            string directory = tmp[tmp.Length - 1] + "_log ";


            var fileDialog = new System.Windows.Forms.FolderBrowserDialog();
            fileDialog.Description = "Выбор места для папки лога " + directory;
            switch (fileDialog.ShowDialog())
            {
                case System.Windows.Forms.DialogResult.OK:
                    directory = fileDialog.SelectedPath + "\\" + directory;

                    if (txtboxFilter.Text != "")
                        directory += "Filter-" + txtboxFilter.Text;

                    if (txtboxWait.Text != "")
                        directory += " Wait-" + txtboxWait.Text;


                    try
                    {
                        if (!Directory.Exists(directory))
                            Directory.CreateDirectory(directory);

                        directory = directory.TrimEnd(' ');
                        if (_log.Count != 0)
                        {
                            using (FileStream fs = new FileStream(directory + "\\" + btnShowLog.Content.ToString() + ".txt", FileMode.Create))
                            {
                                using (StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8))
                                {
                                    foreach (string iter in _log)
                                    {
                                        sw.WriteLine(iter);
                                        sw.WriteLine("##############################");
                                    }
                                }
                            }
                        }
                        if (_logSuccess.Count != 0)
                        {
                            using (FileStream fs = new FileStream(directory + "\\" + btnShowLogSuccess.Content.ToString() + ".txt", FileMode.Create))
                            {

                                using (StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8))
                                {
                                    foreach (string iter in _logSuccess)
                                    {
                                        sw.WriteLine(iter);
                                        sw.WriteLine("##############################");
                                    }
                                }
                            }
                        }
                        if (_fails.Count != 0)
                            using (FileStream fs = new FileStream(directory + "\\" + "Errors Log.txt", FileMode.Create))
                            {

                                using (StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8))
                                {
                                    foreach (string iter in _fails)
                                    {
                                        sw.WriteLine(iter);
                                        sw.WriteLine("##############################");
                                    }
                                }
                            }
                        MessageBox.Show("Лог сохранен в папке: " + directory);

                    }

                    catch (ArgumentException ex)
                    {
                        MessageBox.Show("Argument" + ex.Message.ToString());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Возникла ошибка при записи:\n" + ex.Message.ToString());
                    }
                    break;
            }

        }

       async private void readFile()
        {

            lblFindIP.Visibility = Visibility.Visible;
            lblFindIP.Content = "Считывание";

            //Считывание из файла
            _devices.Clear();

            string tmp;
            string ipTmp;
            string valueTmp;

            //резулярное выражение для поиска ip в строке
            System.Text.RegularExpressions.Regex RegIp = new System.Text.RegularExpressions.Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");

            try
            {
              await  using (FileStream fs = new FileStream(txtboxIP.Text, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs, System.Text.Encoding.UTF8))
                    {

                        while (!sr.EndOfStream)
                        {
                            lblFindIP.Content = "Считывание.";
                            tmp = sr.ReadLine();
                            if (RegIp.IsMatch(tmp))
                            {
                                ipTmp = RegIp.Match(tmp).ToString();
                                valueTmp = tmp.Replace(ipTmp + ' ', "");

                                lblFindIP.Content = "Считывание..";
                                if (!_devices.ContainsKey(ipTmp)) //Проверка на задвоение IP адреса
                                {
                                    if (valueTmp != ipTmp)
                                        _devices.Add(ipTmp, valueTmp);
                                    else
                                        _devices.Add(ipTmp, "");
                                }
                            }
                            lblFindIP.Content = "Считывание...";
                        }
                    }
                }
                if (_devices.Count != 0)
                {
                    btncheckList.IsEnabled = true;
                }
                else
                    btncheckList.IsEnabled = false;
                checkComlete();
            }

            catch (ArgumentException ex)
            {
                MessageBox.Show("Argument" + ex.Message.ToString());
            }
            //
            catch (Exception ex)
            {
                MessageBox.Show("Возникла ошибка при чтении:\n" + ex.Message.ToString());
            }


            lblFindIP.Content = $"Найдено " + _devices.Count + " адресов";
            btncheckList.IsEnabled = true;


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

        private void txtboxIP_TouchEnter(object sender, System.Windows.Input.TouchEventArgs e)
        {
            readFile();
        }

        private void txtboxPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            checkComlete();
        }

        private void txtboxWait_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtboxWait.Text != "")
            {
                btnCommandAdterfind.IsEnabled = true;
                chkBoxSelective.IsEnabled = true;
            }
            else
            {
                btnCommandAdterfind.IsEnabled = false;
                chkBoxSelective.IsEnabled = false;
            }
        }



        //Buttons_click

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            Settings settings = new Settings(_timings);
            settings.ShowDialog();
        }

        private void btnAddComands_Click(object sender, RoutedEventArgs e)
        {
            addComands addComands = new addComands(_commands);
            addComands.ShowDialog();

            checkComlete();
        }

        private void btnCommandAdterfind_Click(object sender, RoutedEventArgs e)
        {
            addComands addComands = new addComands(_additCommands);
            addComands.ShowDialog();
            if (_additCommands.Count != 0)
                chkBoxInvertDisChange.IsEnabled = true;
            else
                chkBoxInvertDisChange.IsEnabled = false;

        }

        private void btnShow_Click(object sender, RoutedEventArgs e)
        {
            ShowList showList = new ShowList(btnShowLog.Content.ToString(), _log);
            showList.ShowDialog();
        }
        private void btnShowLogSuccess_Click(object sender, RoutedEventArgs e)
        {
            ShowList showList = new ShowList(btnShowLogSuccess.Content.ToString(), _logSuccess);
            showList.ShowDialog();

        }
        private void btnFails_Click(object sender, RoutedEventArgs e)
        {
            ShowList showList = new ShowList("Errors", _fails);
            showList.ShowDialog();
        }

        private void btnFileDialog_Click(object sender, RoutedEventArgs e)
        {

            System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();

            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtboxIP.Text = fileDialog.FileName;
                txtboxIP.ToolTip = fileDialog.FileName;

                readFile();

            }
            else
            {
                txtboxIP.Text = string.Empty;
                txtboxIP.ToolTip = null;

                lblFindIP.Visibility = Visibility.Hidden;
                btncheckList.IsEnabled = false;
            }

        }
        private void btncheckList_Click(object sender, RoutedEventArgs e)
        {
            ShowList obj = new ShowList("List IP addresses", _devices);
            obj.Show();
  
        }
        private void btnTestConnect_Click(object sender, RoutedEventArgs e)
        {
            //Обнуляем значения
            _log.Clear();
            _logSuccess.Clear();
            _fails.Clear();

            isEnableElements(false);
            myTelnet session = new myTelnet(txtoxLogin.Text, txtboxPassword.Password, _devices.Keys.First());
            session.addComands(_commands);
            if (_timings.Count > 0) 
            {
                session.timingLogin = (int)_timings[0];
                session.timingCommands= (int)_timings[1];
            }
            if (_additCommands.Count != 0)
                session.addAltComands(txtboxWait.Text, _additCommands, chkBoxInvertDisChange.IsChecked.Value);

            if (txtboxFilter.Text != "")
                session.addfilter(txtboxFilter.Text);

            session.isDinChange = chkBoxDinChange.IsChecked.Value;

            if (txtboxWait.Text != "")
                session.waitResultInit(txtboxWait.Text, chkBoxSelective.IsChecked.Value);

            session.runCommands(_devices.Keys.First(),_devices.Values.First());
            //  MessageBox.Show(session.log);
            ShowList obj = new ShowList("Resplnce", session.log);
            obj.Show();

            isEnableElements(true);
            checkComlete();
        }


        //Основная кнопка. Тут всё запускается
        private async void btnRun_Click(object sender, RoutedEventArgs e)
        {
            //Обнуляем значения
            _log.Clear();
            _logSuccess.Clear();
            _fails.Clear();


            prgBar.Value = 0;
            isRun = true;
            prgBar.Maximum = _devices.Count;
            btnFails.Visibility = Visibility.Hidden;

            lblProgress.Content = prgBar.Value + "/" + prgBar.Maximum;
            lblStatus.Content = string.Empty;
            lblStatus.Foreground = Brushes.Black;
            //clock
            stopWatch.Reset();
            lblClock.Content = "00:00:00";

            //первая запись в логе с системной инфой
            string commands = string.Empty;
            string altcommands = string.Empty;
            foreach (string str in _commands)
                commands += str + "\n";
            foreach (string str in _additCommands)
                altcommands += str + "\n";


            _log.Add("Колличество устройств: " + _devices.Count +
              "\nКоманды на отправку:\n" + commands +
              (chkBoxDinChange.IsChecked.Value ? "\nВключена динамическая замена " + _key : string.Empty) +
             "\nПорт: " + txtboxPort.Text +
              (txtboxFilter.Text != "" ? "\nФильтр: " + txtboxFilter.Text : string.Empty) +
              (txtboxWait.Text != "" ? "\nОжидание вывода: " + txtboxWait.Text : string.Empty) + "\n" +
              (chkBoxInvertDisChange.IsChecked.Value ? "\nВключена инверсия действия от ожидания" : string.Empty) +
              (_additCommands.Count != 0 ? "\nДействие: " + altcommands : string.Empty) + "\n");
            _logSuccess.Add("Лог успешных проходов\n");


            //отключаем элементы управления
            isEnableElements(false);

            //включаем отображение прогресс бара и счетчика
            prgBar.Visibility = Visibility.Visible;
            lblProgress.Visibility = Visibility.Visible;
            btnStop.Visibility = Visibility.Visible;
            btnStop.IsEnabled = true;

            //инициализируем данные для работы c Telnet
            myTelnet session = new myTelnet(txtoxLogin.Text, txtboxPassword.Password, "", Convert.ToInt32(txtboxPort.Text),
                                           txtboxFilter.Text != "" ? txtboxFilter.Text : string.Empty, _key);
            if (_timings.Count > 0)
            {
                session.timingLogin = (int)_timings[0];
                session.timingCommands = (int)_timings[1];
            }
            session.addComands(_commands);


            if (txtboxFilter.Text != "")
                session.addfilter(txtboxFilter.Text);

            if (txtboxWait.Text != "")
                session.waitResultInit(txtboxWait.Text, chkBoxSelective.IsChecked.Value);

            session.isDinChange = chkBoxDinChange.IsChecked.Value;

            if (_additCommands.Count != 0)
                session.addAltComands(txtboxWait.Text, _additCommands, chkBoxInvertDisChange.IsChecked.Value);

 

            string ip;

            //запуск работы цикла
            stopWatch.Start();
            dispatcherTimer.Start();
            foreach (KeyValuePair<string, string> pair in _devices)
            {
                ip = pair.Key;

                lblStatus.Content = ip + " - Отправка...";
                if (!isRun)
                {
                    stopWatch.Stop();
                    lblStatus.Content = ip + " - Процесс остановлен";
                    MessageBox.Show(lblStatus.Content.ToString());
                    _log.Add(_log.Count + ")" + lblStatus.Content);
                    break;
                }


               if (await Task.Run(() => session.runCommands(ip,pair.Value)))
               {
                    lblStatus.Content = ip + " - Отправлено";
                    lblProgress.Content = (_log.Count - _fails.Count).ToString() + "/" + prgBar.Maximum;
                    _logSuccess.Add(_logSuccess.Count + ")" + session.log);
               }
                else
                {
                    lblStatus.Content = ip + " - Ошибка";
                    _fails.Add(_fails.Count + 1 + ")" + session.log);
                    btnFails.Visibility = Visibility.Visible;
                    btnFails.Content = " Ошибки: " + _fails.Count();
                }


                if (chkBoxShowRes.IsChecked.Value)
                {
                  stopWatch.Stop();

                    //if (MessageBox.Show(session.log, "Лог " + ip, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                    ShowList obj = new ShowList("Responce", session.log);
                    obj.ShowDialog();
                    isRun = obj.isOkCancel;
                   stopWatch.Start();
                }

                _log.Add(_log.Count + ") " + session.log);
                prgBar.Value++;
            }

            if (stopWatch.IsRunning) stopWatch.Stop();
            
            //  elapsedtimeitem.Items.Add(curentTime);

            _log[0] += "Выполнено успешно: " + (_log.Count - 1 - _fails.Count) + "\nВремя выполнения: " + lblClock.Content + "\n" +
                (!isRun ? "Прерывание вручную" : "Остановка по окончанию адресов");
            _logSuccess[0] += "Колличество устройств: " + (_logSuccess.Count - 1);
            isEnableElements(true);
            checkComlete();
            btnStop.Visibility = Visibility.Hidden;
            btnStop.IsEnabled = false;
            if (isRun)
            {
                lblStatus.Content = "Отправка завершена";
                lblStatus.Foreground = Brushes.Green;
            }
            if (_fails.Count != 0)
                btnFails.IsEnabled = true;

            if (chkBoxSaveLogs.IsChecked.Value)
                saveLog();


        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            isRun = false;
        }
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            txtoxLogin.Text = string.Empty;
            txtboxPassword.Password = string.Empty;
            txtboxIP.Text = string.Empty;
            txtboxPort.Text = "23";
            txtboxFilter.Text = string.Empty;
            txtboxWait.Text = string.Empty;
            _commands.Clear();
            _devices.Clear();
            _fails.Clear();
            _log.Clear();
            chkBoxDinChange.IsChecked = false;
            chkBoxDinChange.IsEnabled = false;

            btnStop.Visibility = Visibility.Hidden;
            btnFails.Visibility = Visibility.Hidden;
            btnShowLog.IsEnabled = false;

            prgBar.Visibility = Visibility.Hidden;
            lblComAdd.Visibility = Visibility.Hidden;
            lblProgress.Content = string.Empty;
            lblStatus.Content = string.Empty;
            lblFindIP.Content = string.Empty;

            lblClock.Content = string.Empty;
            checkComlete();
        }
        private void chkBoxSaveLogs_Checked(object sender, RoutedEventArgs e)
        {
            if (_log.Count != 0)
                saveLog();
        }

    }
}
