using System;
using System.Collections.Generic;
using PrimS.Telnet;
using System.Windows;
using System.Threading;
using System.Threading.Tasks;

namespace MartsTelnet
{
    class myTelnet
    {
        string _user;
        string _password;
        int _port;
        public string _ip;
        List<string> _commands;
        string _filter = string.Empty;
        public string _log = string.Empty;
        public string _waitResult = string.Empty;

        public myTelnet(string user, string password, string ip, int port, string filter)
        {
            _user = user;
            _password = password;
            _ip = ip;
            _port = port;
            _commands = new List<string>();
            _filter = filter;
        }
        public myTelnet(string user, string password, string ip) : this(user, password, ip, 23, "") { }

        public void addComands(List<string> commands)
        {
            foreach (string command in commands)
            {
                _commands.Add(command);
            }
        }
        //автоматическая отпрпавка без вывода

        /// <summary>
        /// Проверяет наличие ответа на устройство
        /// </summary>
        /// <param name="client"></param>
        /// <param name="addLog">if true,then _log+=response </param>
        /// <param name="timeout"> timeout*50mc== timeout mc </param>
        /// <returns></returns>
        private bool response(Client client, bool addLog = false, bool separator = false, int timeout = 100)
        {
            string tmp;
            //моя проверка на соединение.TimeOut == 100*50 mc - 5 sec
            for (int i = 0; i < timeout; ++i)
            {
                tmp = client.ReadAsync().Result;
                if (tmp != "")
                {

                    if (addLog)
                        _log += tmp;

                    if (_filter != "" && separator)
                        if (!tmp.Contains(_filter))
                        {
                            _log += "\"" + _filter + "\" не найден после авторизации. Операция прервана";
                            return false;
                        }

                    return true;
                }
                else
                    Thread.Sleep(50);
            }
            _log += "Time Out\n";
            return false;
        }

        public bool runCommands(bool showMessage = false)
        {
            _log = _ip + "\n";
            try
            {
                TcpByteStream tcpByteStream = new TcpByteStream(_ip, _port);

                using (Client client = new Client(tcpByteStream, CancellationToken.None))
                {
                    if (!response(client, false, false, 200))
                    {
                        _log += "Error connect";
                        return false;
                    }
                    //авторизация
                    client.WriteLine(_user);
                    if (!response(client))
                        return false;
                    client.WriteLine(_password);
                    if (!response(client, false, true))
                        return false;

                    client.WriteLine("");
                    client.ReadAsync();

                    //Отправляем на устройство
                    foreach (string command in _commands)
                    {
                        client.WriteLine(command);
                        if (!response(client, true))
                            return false;
                    }

                    //ожидание результата на выходе
                    if (_waitResult != "")
                    {
                        if (_log.Contains(_waitResult))
                        {
                            string[] tmp = _log.Split("\n");
                            foreach(string row in tmp)
                            {
                                if (row.Contains(_waitResult))
                                {
                                    _log = _ip + " - " + row;
                                    break;
                                }
                            }
                        }    
                         else
                        {
                            _log =_ip+  "\"" + _waitResult + "\" не найден после ввода команд";
                            return false;
                        }
                    }

                    if (showMessage)
                        MessageBox.Show(_log);

                    client.Dispose();
                    tcpByteStream.Close();
                    tcpByteStream.Dispose();

                    return true;
                }
            }
            catch (Exception ex)
            {
                _log += ex.Message.ToString();
            }
            return false;
        }



    }

}
