using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;




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

        public MainWindow()
        {
            _devices = new List<string>();
            _commands = new List<string>();
            _fails = new List<string>();
            InitializeComponent();
        }



        void checkComblete()
        {
            if (txtoxLogin.Text != "" &&
                txtboxPassword.Password != "" &&
                _devices.Count != 0 &&
                _commands.Count != 0)
            {
                btnConnect.IsEnabled = true;
                btnTestConnect.IsEnabled = true;
            }
            else
            {
                btnConnect.IsEnabled = false;
                btnTestConnect.IsEnabled = false;
            }
        }


        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            addComands obj1 = new addComands(_commands);
            obj1.ShowDialog();
            checkComblete();
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
                        checkComblete();
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
            obj.ShowDialog();
        }

        private void txtboxIP_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtboxIP.Text != "")
                btncheckList.IsEnabled = true;
            else
                btncheckList.IsEnabled = false;
        }

        private void btnTestConnect_Click(object sender, RoutedEventArgs e)
        {
            myTelnet session = new myTelnet(txtoxLogin.Text, txtboxPassword.Password, _devices[0]);
            session.addComands(_commands);
            session.testConnect();

        }

        private void btnAddComands_Click(object sender, RoutedEventArgs e)
        {
            addComands addComands = new addComands(_commands);
            addComands.ShowDialog();
            if (_commands.Count != 0)
                lblComAdd.Visibility = Visibility.Visible;
            checkComblete();
        }

        private void btnShow_Click(object sender, RoutedEventArgs e)
        {
            ShowList showList = new ShowList(_log);
            showList.ShowDialog();
        }

        private async void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            _log = null;
            _fails.Clear();
            prgBar.Value = 0.0;
            prgBar.Maximum = _devices.Count;
            lblProgress.Content = prgBar.Value + "/" + prgBar.Maximum;


            prgBar.Visibility = Visibility.Visible;
            lblProgress.Visibility = Visibility.Visible;


            myTelnet session = new myTelnet(txtoxLogin.Text, txtboxPassword.Password,"",Convert.ToInt32(txtboxPort.Text));
            session.addComands(_commands);


            foreach (string device in _devices)
            {
                session._ip = device;

                if (await Task.Run(() => session.runCommands()))
                {
                    lblProgress.Content = session.log.Count + "/" + prgBar.Maximum;
                }
                else
                {
                    _fails.Add(device);
                    btnFails.Visibility = Visibility.Visible;
                    btnFails.Content = " Ошибки: " + _fails.Count().ToString();
                }
                prgBar.Value += 1.0;
            }

            _log = session.log;
            session.log = null;
            btnShowLog.IsEnabled = true;
            if (_fails.Count != 0)
                btnFails.IsEnabled = true;

        }

        private void PrgBar_ValueChanged(object sender, ProgressChangedEventArgs e)
        {
            prgBar.Value = e.ProgressPercentage;
        }

        private void btnFails_Click(object sender, RoutedEventArgs e)
        {

            ShowList showList = new ShowList(_fails);
            showList.ShowDialog();

        }




    }
}
