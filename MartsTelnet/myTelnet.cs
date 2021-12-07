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
        public string log ="";



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
                _commands.Add(command);
        }
  
        public void Close()
        {
            _commands = null;
        }
        public void testConnect()
        {

            try
            {
                TcpByteStream byteStream = new TcpByteStream(_ip, _port);
                if (byteStream.Connected)
                    using (Client client = new Client(byteStream, System.Threading.CancellationToken.None))
                    {
                        MessageBox.Show(client.ReadAsync().Result.ToString());
                        if (client.TryLoginAsync(_user, _password, 500, "\0", "\n").Result)
                        {
                            MessageBox.Show("Sucsess" + client.ReadAsync().Result.ToString());

                        }
                        else
                        {
                            byteStream.Close();
                            MessageBox.Show(client.ReadAsync().Result.ToString());
                            throw new Exception("fail");
                        }
                    }
                else MessageBox.Show("Fail Connected");
                byteStream.Close();
                //using (Client client = new Client(_ip, _port, System.Threading.CancellationToken.None))
                //{

                //   MessageBox.Show(client.ReadAsync().Result.ToString());
                //   if (client.TryLoginAsync(_user, _password, 2000).Result)
                //    {
                //        MessageBox.Show(client.ReadAsync().Result.ToString());
                //    }
                //   else
                //       throw new Exception("uncorect log\\pass");

                //    //Task.Delay(500);
                //    //MessageBox.Show(client.ReadAsync().Result.ToString());
                //    //client.WriteLine(_user);
                //    //client.WriteLine(_password);
                //    //MessageBox.Show(client.ReadAsync().Result.ToString());


                //   //    foreach (string command in _commands)
                //   //    {
                //   //        client.WriteLine(command);
                //   //        MessageBox.Show(client.ReadAsync().Result.ToString());
                //   //    }
                //   //MessageBox.Show(client.ReadAsync().Result.ToString());

                //    client.Dispose();

                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        public bool runCommands()
        {
            log = "";
            try
            {
                TcpByteStream tcpByteStream = new TcpByteStream(_ip, _port);
                Thread.Sleep(200);
                if (tcpByteStream.Connected)

                    using (Client client = new Client(tcpByteStream, System.Threading.CancellationToken.None))
                    {

                     //  if ( client.TryLoginAsync(_user, _password, 2000))
                     

                        client.WriteLine(_user);
                        client.WriteLine(_password);
                        Thread.Sleep(200);
                        client.ReadAsync();
                        //  client.WriteLine("show int");
                        Thread.Sleep(200);

                        client.WriteLine("");
                        //foreach (string command in _commands)
                        //{
                        // if (!tcpByteStream.Connected)
                        //   return false;
                        //    Thread.Sleep(100);
                        //    client.WriteLine(command);
                        //}
                        log = client.ReadAsync().Result;

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
