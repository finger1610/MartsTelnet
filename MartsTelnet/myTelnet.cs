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
        public async Task<bool> runCommands(bool showMessage = false)
        {
            log =_ip+ "\nОшибка соединения";
            try
            {
                TcpByteStream tcpByteStream = new TcpByteStream(_ip, _port);
                Thread.Sleep(200);
                if (tcpByteStream.Connected)
                    using (Client client = new Client(tcpByteStream, System.Threading.CancellationToken.None))
                    {

                       await client.TryLoginAsync(_user, _password, 2000).ConfigureAwait(true);
                        log = _ip + "\n" + client.ReadAsync().Result;
                            foreach (string command in _commands)
                            {
                                if (!tcpByteStream.Connected)
                                    return false;
                                client.WriteLine(command);
                                Thread.Sleep(50);
                            }

                            log = _ip + "\n" + Task.Run(() => client.ReadAsync()).ToString();
                            if (log == _ip + "\n")
                            {
                                log += "Оборудование недоступно";
                                return false;
                            }
                            if (showMessage)
                                MessageBox.Show(log);

                            client.Dispose();

                            tcpByteStream.Close();
                            tcpByteStream.Dispose();

                            return true;
                    }
                    
                        //client.WriteLine(_user);
                        //client.WriteLine(_password);
                        //Thread.Sleep(100);
                        //log = _ip + "\n" +(await Task.Run(() => client.ReadAsync().Result.ToString()));


                        //client.WriteLine("");
                 

                       

                       
                    
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message.ToString());

            }
            return false;
        }

      
   
    }

}
