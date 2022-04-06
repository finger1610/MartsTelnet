using System;
using System.Collections.Generic;
using PrimS.Telnet;
using System.Windows;
using System.Threading;

namespace MartsTelnet
{
    class myTelnet
    {
        string _user;
        string _password;
        int _port;
        public string _ip;
        List<string> _commands;
        List<string> _alterCommands;

        string _filter = string.Empty;
        public string _log = string.Empty;
        string _waitResult = string.Empty;
   
        bool _isDinamicChange = false;
        bool _isInvertDinChange = false;
        bool _isSelective = false;
        bool _isFindSelective = false;
        string _key = string.Empty;


        public myTelnet(string user, string password, string ip, int port, string filter,string key)
        {
            _user = user;
            _password = password;
            _ip = ip;
            _port = port;
            _commands = new List<string>();
            _alterCommands = new List<string>();
            _filter = filter;
            _key = key;
        }
        public myTelnet(string user, string password, string ip) : this(user, password, ip, 23, "","") { }

        public void addComands(List<string> commands)
        {
            _commands.Clear();
            foreach (string command in commands)
            {
                _commands.Add(command);
            }
        }
        public void addAltComands(string waitResult,List<string> commands, bool dinamicChange = false,bool invertDinChange = false)
        {
            _waitResult = waitResult;
            _isDinamicChange = dinamicChange;
            _isInvertDinChange = invertDinChange;
            _alterCommands.Clear();

            foreach (string command in commands)
            {
                _alterCommands.Add(command);
            }

        }
        public void waitResultInit(string wait,bool isSelectve)
        {
            _isSelective = isSelectve;
            _waitResult = wait;

        }
        public void addfilter(string filter)
        {
            _filter = filter;        
        }

        //автоматическая отпрпавка без вывода

        /// <summary>
        /// Проверяет наличие ответа на устройство
        /// </summary>
        /// <param name="client"></param>
        /// <param name="addLog">if true,then _log+=response </param>
        /// <param name="timeout"> timeout*50mc== timeout mc </param>
        /// <returns></returns>
        /// 


        bool findInProc(Client client)
        {
            
            foreach (string command in _alterCommands)
            {
                client.WriteLine(command);
                if (!response(client, true))
                    return false;
            }

            return true;
        }
        private bool response(Client client, bool addLog = false, bool filter =false,bool wait = false, int timeout = 100)
        {
            string tmp;
            //моя проверка на соединение.TimeOut == 100*50 mc - 5 sec
            for (int i = 0; i < timeout; ++i)
            {
                tmp = client.ReadAsync().Result;
                if (tmp != "")
                {
                    
                    //при обнаружении слова-фильтра
                    if (!tmp.Contains(_filter) && filter)
                    {
                        _log += "\"" + _filter + "\" не найден в процессе выполнения";
                        return false;
                    }


                    //для добавления команд на лету
             
                    if (wait&&_waitResult!="")
                    {
                        if (!_isSelective)//проверка на выборочную отправку дополнительных комманд
                        {
                            if (_alterCommands.Count != 0)
                            {

                                if (!_isInvertDinChange)//проверка на принцип выполнения. либо при обнаружении фрагмента, либо наоборот
                                {
                                    if (tmp.Contains(_waitResult))
                                        findInProc(client);
                                }
                                else
                                {
                                    if (!tmp.Contains(_waitResult))
                                        findInProc(client);
                                }
                            }
                        }
                        else
                        {
                            if (_isFindSelective)
                            {
                                _isFindSelective = false;
                                if (_alterCommands.Count != 0)
                                {

                                    if (!_isInvertDinChange)//код дублируется из if. Умнее не придумал...
                                    {
                                        if (tmp.Contains(_waitResult))
                                            findInProc(client);
                                    }
                                    else
                                    {
                                        if (!tmp.Contains(_waitResult))
                                            findInProc(client);
                                    }
                                }
                            }
                        }
                    }

                    
                    if (addLog)
                        _log += tmp;

                 
                    return true;
                }
                else
                    Thread.Sleep(50);
            }
            _log += "Time Out\n";
            return false;
        }

        private bool autorisacion (Client client)
        {
          
            if (!response(client, false,false,false, 200))
            {
                _log += "Error connect";
                return false;
            }
            //авторизация
            client.WriteLine(_user);
            if (!response(client))
                return false;
            client.WriteLine(_password);
            if (!response(client,false,true))
                return false;

            client.WriteLine("");
            client.ReadAsync();

            return true;
        }

        private bool send (Client client,List <string> commands, string dinComm)
        {
            string command;
            // Отправляем на устройство
            foreach (string commandFor in commands)
            {
                command = commandFor;//для удаления первых 3 символов ### 
                if (command.Length > 3)
                {
                    if (command.Substring(0, 3) == "###" && _isSelective)
                    {
                        _isFindSelective = true;//Включение добавления на лету после данной команды
                        command = command.Substring(3); 
                    }
                }
                //Условие для динамической замены
                if (_isDinamicChange && command.Contains(_key))
                    client.WriteLine(command.Replace(_key, dinComm));
                else
                    client.WriteLine(command);

                if (!response(client, true,false,true))
                    return false;
                
            }

            //поиск фаргмента после выполнения набора команд (смотри входное условие)
            if (_waitResult != ""&& _alterCommands.Count==0)
            {
                if (_log.Contains(_waitResult))
                {
                    string[] tmp = _log.Split("\n");
                    _log = _ip + " - ";

                    foreach (string row in tmp)
                    {
                        if (row.Contains(_waitResult))
                            _log += row + "\n";
                    }
                }
                else
                {
                    _log = _ip + "\" " + _waitResult + "\" не найден после ввода команд";
                    return false;
                }
            }
            return true;
        }

        public bool runCommands(string dinComm)
        {
            _log = _ip + "\n";
            try
            {
                TcpByteStream tcpByteStream = new TcpByteStream(_ip, _port);

                using (Client client = new Client(tcpByteStream, CancellationToken.None))
                {
                    if (!autorisacion(client))
                        return false;

                    if (!send(client,_commands, dinComm))
                        return false;

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
