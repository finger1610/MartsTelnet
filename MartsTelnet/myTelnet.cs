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

        public myTelnet(string user, string password, string ip, int port,string filter)
        {
            _user = user;
            _password = password;
            _ip = ip;
            _port = port;
            _commands = new List<string>();
            _filter = filter;
        }
        public myTelnet(string user, string password, string ip) : this(user, password, ip, 23,"") { }

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
            _log =_ip+ "\n";
            try
            {
                TcpByteStream tcpByteStream = new TcpByteStream(_ip, _port);

                    using (Client client = new Client(tcpByteStream, System.Threading.CancellationToken.None))
                    {
  
                        client.WriteLine(_user);
                        client.WriteLine(_password);
                        Thread.Sleep(1000);
                        log = _ip + "\n" + Task.Run(() => client.ReadAsync().Result).Result.ToString();

                    client.WriteLine("");
                    client.ReadAsync();

                   //Отправляем на устройство
                    foreach (string command in _commands)
                    { 
                       client.WriteLine(command);
                       if (!response(client,true))
                           return false;

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

                MessageBox.Show(ex.Message.ToString());

            }
            return false;
        }

      
   
    }

}
