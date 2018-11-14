using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace ConsoleApp2
{
    class Program
    {
        static Socket SocketWatch = null;
        //定义一个集合，存储客户端信息
        static Dictionary<string, Socket> ClientConnectionItems = new Dictionary<string, Socket> { };
        static MySqlConnection localSql = new MySqlConnection("Database=ctyun_class;Data Source=127.0.0.1;User Id=pc-test;Password=damu19950313_");
        static void Main(string[] args)
        {
            //MySqlConnection localSql = null;
            try
            {
                //localSql = new MySqlConnection("Database=ctyun_class;Data Source=117.80.86.174;User Id=pc-test;Password=damu19950313_");
                localSql.Open();
                //MySqlCommand mycmd = new MySqlCommand("select * from classroom", localSql);
                //Console.WriteLine(mycmd.CommandText);
            }
            catch (Exception)
            {
                Console.WriteLine("can not open database!\r\n");
                throw;
            }
            //localSql.Open();
            

            IPAddress ip = IPAddress.Any;
            int port = 5500;

            //将IP地址和端口号绑定到网络节点point上  
            IPEndPoint ipe = new IPEndPoint(ip, port);

            //定义一个套接字用于监听客户端发来的消息，包含三个参数（IP4寻址协议，流式连接，Tcp协议）  
            SocketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //监听绑定的网络节点  
            SocketWatch.Bind(ipe);
            //将套接字的监听队列长度限制为20  
            SocketWatch.Listen(20);


            //负责监听客户端的线程:创建一个监听线程  
            Thread threadwatch = new Thread(WatchConnecting);
            //将窗体线程设置为与后台同步，随着主线程结束而结束  
            threadwatch.IsBackground = true;
            //启动线程     
            threadwatch.Start();

            Console.WriteLine("开启监听......");
            Console.WriteLine("点击输入任意数据回车退出程序......");
            Console.ReadKey();

            SocketWatch.Close();
            localSql.Close();
        }
        static void WatchConnecting()
        {
            Socket connection = null;

            //持续不断监听客户端发来的请求     
            while (true)
            {
                try
                {
                    connection = SocketWatch.Accept();
                }
                catch (Exception ex)
                {
                    //提示套接字监听异常     
                    Console.WriteLine(ex.Message);
                    break;
                }

                //客户端网络结点号  
                string remoteEndPoint = connection.RemoteEndPoint.ToString();
                //添加客户端信息  
                ClientConnectionItems.Add(remoteEndPoint, connection);
                //显示与客户端连接情况
                Console.WriteLine("\r\n[客户端\"" + remoteEndPoint + "\"建立连接成功！ 客户端数量：" + ClientConnectionItems.Count + "]");

                //获取客户端的IP和端口号  
                IPAddress clientIP = (connection.RemoteEndPoint as IPEndPoint).Address;
                int clientPort = (connection.RemoteEndPoint as IPEndPoint).Port;

                //让客户显示"连接成功的"的信息  
                string sendmsg = "[" + "本地IP：" + clientIP + " 本地端口：" + clientPort.ToString() + " 连接服务端成功！]";
                byte[] arrSendMsg = Encoding.UTF8.GetBytes(sendmsg);
                connection.Send(arrSendMsg);

                //创建一个通信线程      
                Thread thread = new Thread(recv);
                //设置为后台线程，随着主线程退出而退出 
                thread.IsBackground = true;
                //启动线程     
                thread.Start(connection);
            }
        }

        /// <summary>
        /// 接收客户端发来的信息，客户端套接字对象
        /// </summary>
        /// <param name="socketclientpara"></param>    
        static void recv(object socketclientpara)
        {
            Socket socketServer = socketclientpara as Socket;
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId.ToString());
            bool f = true;
            //MySqlConnection localSql = new MySqlConnection("Database=ctyun_class;Data Source=117.80.86.174;User Id=pc-test;Password=damu19950313_");
            while (f)
            {
                //创建一个内存缓冲区，其大小为1024*1024字节  即1M     
                byte[] arrServerRecMsg = new byte[1024 * 1024];
                //将接收到的信息存入到内存缓冲区，并返回其字节数组的长度   
                MySqlCommand mycmd = localSql.CreateCommand();
                try
                {
                    int length = socketServer.Receive(arrServerRecMsg);

                    //将机器接受到的字节数组转换为人可以读懂的字符串     
                    string strSRecMsg = Encoding.UTF8.GetString(arrServerRecMsg, 0, length);
                    //MySqlCommand mycmd = localSql.CreateCommand();
                    Console.WriteLine(":"+strSRecMsg);
                    //  MySqlDataAdapter adap = new MySqlDataAdapter(mycmd);
                    // DataSet ds = new DataSet();
                    // adap.Fill(ds);
                    //if (strSRecMsg != null&& strSRecMsg != "")
                    //{ 
                    string temp = strSRecMsg;
                    if (temp == "room"||temp=="close" || temp == ""||temp=="ping")
                    {
                        switch (temp)
                        {
                            case "room": {
                                    socketServer.Send(Encoding.UTF8.GetBytes("room0"));
                                    break;
                                }
                            case "close":
                                {
                                    string sql = string.Format("update classroom set class_now = '无课程' where (ip = '" + socketServer.RemoteEndPoint.ToString() + "')");
                                    mycmd.CommandText = sql;
                                    mycmd.CommandType = CommandType.Text;
                                    socketServer.Shutdown(SocketShutdown.Both);
                                    ClientConnectionItems.Remove(socketServer.RemoteEndPoint.ToString());
                                    Console.WriteLine("\r\n[客户端\"" + socketServer.RemoteEndPoint + "\"已经中断连接！ 客户端数量：" + ClientConnectionItems.Count + "]");
                                    f = false;
                                    mycmd.Dispose();
                                    break;
                                }
                            case "":
                                {
                                    string sql = string.Format("update classroom set class_now = '无课程' where (ip = '" + socketServer.RemoteEndPoint.ToString() + "')");
                                    mycmd.CommandText = sql;
                                    mycmd.CommandType = CommandType.Text;
                                    socketServer.Shutdown(SocketShutdown.Both);
                                    ClientConnectionItems.Remove(socketServer.RemoteEndPoint.ToString());
                                    Console.WriteLine("\r\n[客户端\"" + socketServer.RemoteEndPoint + "\"已经中断连接！ 客户端数量：" + ClientConnectionItems.Count + "]");
                                    f = false;
                                    mycmd.Dispose();
                                    break;
                                }
                            case "ping"://处理心跳包保持连接
                                {
                                    Console.WriteLine("\r\n[客户端" + socketServer.RemoteEndPoint + "ping]");
                                    break;
                                }
                        }         
                    }
                    else {
                        if (temp.StartsWith("sql-"))//处理数据库命令
                        {
                            if (localSql.State == ConnectionState.Closed)
                            {
                                try
                                {                            
                                    localSql.Open();                                    
                                }
                                catch (Exception)
                                {
                                    Console.WriteLine("can not open database!\r\n");
                                    throw;
                                }
                            }
                            temp = temp.Replace("sql-", "");
                            temp = temp.Replace("!tempIP!", socketServer.RemoteEndPoint.ToString());
                            Console.WriteLine("SQL  " + temp);
                            string sql = string.Format(temp);
                            mycmd.CommandText = temp;
                            mycmd.CommandType = CommandType.Text;
                            if (localSql.State.ToString() != "Open")
                            {
                                localSql.Open();
                            }
                            MySqlDataReader sdr = mycmd.ExecuteReader();
                            int i = 0;
                            string resql = "";
                            while (sdr.Read())
                            {
                                for (int t = 0; t < sdr.FieldCount; t++)
                                { resql = resql + sdr[t].ToString() + "$"; }

                                i++;
                            }
                            if (i == 0 && temp.StartsWith("select"))
                            {
                                resql = "$";
                            }
                            Console.WriteLine(resql);
                            sdr.Close();
                            mycmd.Cancel();
                            mycmd.Dispose();
                            socketServer.Send(Encoding.UTF8.GetBytes(resql));
                            //Thread.Sleep(100);
                        }
                        if(temp.StartsWith("photo_"))//转发拍照指令
                        {
                            string temp0 = "select ip from classroom where (id='"+temp.Replace("photo_","")+"' )";
                            Console.WriteLine("SQL-in  " + temp0);
                            string sql = string.Format(temp0);
                            mycmd.CommandText = temp0;
                            mycmd.CommandType = CommandType.Text;
                            if (localSql.State.ToString() != "Open")
                            {
                                localSql.Open();
                            }
                            MySqlDataReader sdr = mycmd.ExecuteReader();
                            int i = 0;                            
                            while (sdr.Read())
                            {
                                string ip_temp=sdr[0].ToString();
                                foreach (var socketTemp in ClientConnectionItems)
                                {
                                    if(socketTemp.Value.RemoteEndPoint.ToString()==ip_temp)
                                    socketTemp.Value.Send(Encoding.UTF8.GetBytes("photo"));
                                }
                                i++;
                            }
                            sdr.Close();
                            mycmd.Cancel();
                            mycmd.Dispose();
                        }
                        if (temp.StartsWith("screen_"))//转发截图指令
                        {
                            string temp0 = "select ip from classroom where (id='" + temp.Replace("screen_", "") + "' )";
                            Console.WriteLine("SQL-in  " + temp0);
                            string sql = string.Format(temp0);
                            mycmd.CommandText = temp0;
                            mycmd.CommandType = CommandType.Text;
                            if (localSql.State.ToString() != "Open")
                            {
                                localSql.Open();
                            }
                            MySqlDataReader sdr = mycmd.ExecuteReader();
                            int i = 0;
                            while (sdr.Read())
                            {
                                string ip_temp = sdr[0].ToString();
                                foreach (var socketTemp in ClientConnectionItems)
                                {
                                    if (socketTemp.Value.RemoteEndPoint.ToString() == ip_temp)
                                        socketTemp.Value.Send(Encoding.UTF8.GetBytes("screen"));
                                }
                                i++;
                            }
                            sdr.Close();
                            mycmd.Cancel();
                            mycmd.Dispose();
                        }
                        if (temp.StartsWith("login_"))//转发签到
                        {
                            string temp0 = "select ip from classroom where (id='" + temp.Replace("login_", "") + "' )";
                            Console.WriteLine("SQL-in  " + temp0);
                            string sql = string.Format(temp0);
                            mycmd.CommandText = temp0;
                            mycmd.CommandType = CommandType.Text;
                            if (localSql.State.ToString() != "Open")
                            {
                                localSql.Open();
                            }
                            MySqlDataReader sdr = mycmd.ExecuteReader();
                            int i = 0;
                            while (sdr.Read())
                            {
                                string ip_temp = sdr[0].ToString();
                                foreach (var socketTemp in ClientConnectionItems)
                                {
                                    if (socketTemp.Value.RemoteEndPoint.ToString() == ip_temp)
                                        socketTemp.Value.Send(Encoding.UTF8.GetBytes("login"));
                                }
                                i++;
                            }
                            sdr.Close();
                            mycmd.Cancel();
                            mycmd.Dispose();
                        }
                        if (temp.StartsWith("logout_"))//转发退签
                        {
                            string temp0 = "select ip from classroom where (id='" + temp.Replace("logout_", "") + "' )";
                            Console.WriteLine("SQL-in  " + temp0);
                            string sql = string.Format(temp0);
                            mycmd.CommandText = temp0;
                            mycmd.CommandType = CommandType.Text;
                            if (localSql.State.ToString() != "Open")
                            {
                                localSql.Open();
                            }
                            MySqlDataReader sdr = mycmd.ExecuteReader();
                            int i = 0;
                            while (sdr.Read())
                            {
                                string ip_temp = sdr[0].ToString();
                                foreach (var socketTemp in ClientConnectionItems)
                                {
                                    if (socketTemp.Value.RemoteEndPoint.ToString() == ip_temp)
                                        socketTemp.Value.Send(Encoding.UTF8.GetBytes("logout"));
                                }
                                i++;
                            }
                            sdr.Close();
                            mycmd.Cancel();
                            mycmd.Dispose();
                        }
                        if (temp.StartsWith("flag_"))//广播权限变更
                        {
                            string room_id_t = temp.Remove(0, 8);
                            if (ClientConnectionItems.Count > 0)
                            {
                                foreach (var socketTemp in ClientConnectionItems)
                                {

                                    socketTemp.Value.Send(Encoding.UTF8.GetBytes(temp.Replace("flag_", "")));
                                }
                            }                                                 
                        }
                    }
                }
                catch (Exception e)
                {
                    string sql = string.Format("update classroom set class_now = '无课程' where (ip = '" + socketServer.RemoteEndPoint.ToString() + "')");
                    mycmd.CommandText = sql;
                    mycmd.CommandType = CommandType.Text;
                    socketServer.Shutdown(SocketShutdown.Both);
                    ClientConnectionItems.Remove(socketServer.RemoteEndPoint.ToString());
                    Console.WriteLine(e.ToString());
                    ClientConnectionItems.Remove(socketServer.RemoteEndPoint.ToString());
                    //提示套接字监听异常  
                    Console.WriteLine("\r\n[客户端\"" + socketServer.RemoteEndPoint + "\"已经中断连接！ 客户端数量：" + ClientConnectionItems.Count + "]");
                    //关闭之前accept出来的和客户端进行通信的套接字 
                    Console.WriteLine(Thread.CurrentThread.ManagedThreadId.ToString()+"end");
                    localSql.Close();
                    socketServer.Shutdown(SocketShutdown.Both);
                    socketServer.Close();
                    socketServer.Dispose();
                    break;
                }
            }

        }
    }
}
