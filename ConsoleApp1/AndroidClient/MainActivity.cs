using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static Android.AccessibilityServices.GestureDescription;
using Android.Content;
//using System.Windows.Forms;

namespace AndroidClient
{
    [Activity(Label = "天翼云课堂学生版", Theme = "@style/AppTheme", MainLauncher = true)]



    public class MainActivity : AppCompatActivity
    {

        static Thread ThreadClient = null;
        static Socket SocketClient = null;

        static bool flag_scr = false;
        static bool flag_cam = false;
        static bool flash_scr = false;
        static bool flash_cam = false;

        static bool newMsg = false;
        static string message = "";

        static string room_id = "";
        static string room_name = "";
        static string student_id = "";
        static string class_id = "";
        static string class_name = "";

        public int startTCP()//开启TCP链接
        {
            try
            {
                int port = 5500;
                string host = "117.80.86.174";//服务器端ip地址
                //string host2 = "127.0.0.1";//本地调试用ip
                IPAddress ip = IPAddress.Parse(host);
                IPEndPoint ipe = new IPEndPoint(ip, port);
                SocketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    //客户端套接字连接到网络节点上，用的是Connect  
                    SocketClient.Connect(ipe);
                    timer4.Enabled = true;
                }
                catch (Exception)
                {
                    Thread.Sleep(1000);
                    Android.App.AlertDialog.Builder builder = new Android.App.AlertDialog.Builder(MainActivity.this);
                    builder.SetTitle("错误");
                    builder.SetMessage("连接服务器失败！");
                    builder.SetPositiveButton("是",new DialogInterface );
                    builder.Show();
                    
                    Application.Dispose();
                    return -1;
                }

                ThreadClient = new Thread(Recv);
                ThreadClient.IsBackground = true;
                ThreadClient.Start();

                Thread.Sleep(1000);
                return 1;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
        }

        public static void Recv()//接收指令和数据库返回值
        {

            //持续监听服务端发来的消息 
            while (true)
            {
                try
                {
                    //定义一个1M的内存缓冲区，用于临时性存储接收到的消息  
                    byte[] arrRecvmsg = new byte[1024 * 1024];

                    //将客户端套接字接收到的数据存入内存缓冲区，并获取长度  
                    int length = SocketClient.Receive(arrRecvmsg);

                    //将套接字获取到的字符数组转换为人可以看懂的字符串  
                    string strRevMsg = Encoding.UTF8.GetString(arrRecvmsg, 0, length);
                    Console.WriteLine(strRevMsg);
                    if (strRevMsg.StartsWith("f-t") || strRevMsg.StartsWith("t-f") ||//接收到权限变化指令
                        strRevMsg.StartsWith("f-f") || strRevMsg.StartsWith("t-t") || strRevMsg.StartsWith("fs") || strRevMsg.StartsWith("fc"))
                    {
                        if (strRevMsg.EndsWith(room_name))
                        {
                            string s = strRevMsg.Replace(room_name, "$");
                            Console.WriteLine(s);
                            switch (s)
                            {
                                case "f-t$":
                                    {
                                        flag_scr = false;
                                        flag_cam = true;
                                        break;
                                    }
                                case "t-f$":
                                    {
                                        flag_scr = true;
                                        flag_cam = false;
                                        break;
                                    }
                                case "f-f$":
                                    {
                                        flag_scr = false;
                                        flag_cam = false;
                                        break;
                                    }
                                case "t-t$":
                                    {
                                        flag_scr = true;
                                        flag_cam = true;
                                        break;
                                    }
                                case "fs$":
                                    {
                                        flash_scr = true;
                                        break;
                                    }
                                case "fc$":
                                    {
                                        flash_cam = true;
                                        break;
                                    }

                            }
                        }
                    }
                    else
                    {
                        message = strRevMsg;
                        newMsg = true;
                        Console.WriteLine(message + "\r\n");
                    }




                }
                catch (Exception ex)
                {

                    Console.WriteLine("远程服务器已经中断连接！" + ex.Message + "\r\n");
                    break;
                }
            }
        }

        public static void ClientSendMsg(string sendMsg)//发送套接字方法
        {
            try
            {
                //将输入的内容字符串转换为机器可以识别的字节数组     
                byte[] arrClientSendMsg = Encoding.UTF8.GetBytes(sendMsg);
                //调用客户端套接字发送字节数组     
                SocketClient.Send(arrClientSendMsg);
            }
            catch (Exception)
            {

                //throw;
            }
            //将输入的内容字符串转换为机器可以识别的字节数组     

        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

        }
    }
}