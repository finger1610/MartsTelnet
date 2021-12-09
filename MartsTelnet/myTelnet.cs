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
        public string log = "";

        public myTelnet(string user, string password, string ip, int port)
        {
            _user = user;
            _password = password;
            _ip = ip;
            _port = port;
            _commands = new List<string>();
        }
        public myTelnet(string user, string password, string ip) : this(user, password, ip, 23) { }

        public void addComands(List<string> commands)
        {
            foreach (string command in commands)
            {
                _commands.Add(command);
            }
        }
        //автоматическая отпрпавка без вывода
        public bool runCommands(bool showMessage = false)
        {
            log =_ip+ "\nОшибка соединения";
            try
            {
                TcpByteStream tcpByteStream = new TcpByteStream(_ip, _port);
                Thread.Sleep(1000);
                if (tcpByteStream.Connected)

                    using (Client client = new Client(tcpByteStream, System.Threading.CancellationToken.None))
                    {
  
                        client.WriteLine(_user);
                        client.WriteLine(_password);
                        Thread.Sleep(1000);
                        log = _ip + "\n" + Task.Run(() => client.ReadAsync().Result).Result.ToString();

                        client.WriteLine("");
                        foreach (string command in _commands)
                        {
                            if (!tcpByteStream.Connected)
                                return false;
                            client.WriteLine(command);
                            Thread.Sleep(50);
                        }

                        log = _ip + "\n" + Task.Run(()=> client.ReadAsync().Result).Result.ToString();

                        if (log == _ip + "\n")
                        {
                            log += "Ошибка лога";
                            return false;
                        }
                        if (showMessage)
                            MessageBox.Show(log);

                        client.Dispose();

                        tcpByteStream.Close();
                        tcpByteStream.Dispose();

                        return true;
                    }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message.ToString());

            }
            return false;
        }

      
   
    }

}
