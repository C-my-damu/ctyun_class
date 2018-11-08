using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        static Thread ThreadClient = null;
        static Socket SocketClient = null;
        static bool flag_scr = true;
        static bool flag_cam = true;
        static string message = "";
        static int room_id = 0;
        static int student_id = 0;

        private void sendCMD(int i)//拍照/截图命令
        {
            if (i == 1)
            {
                if (flag_scr) ClientSendMsg("screen_" + room_id.ToString());
                else MessageBox.Show("讲台关闭了截屏权限");

            }
           
            if (i == 2)
            {
                if (flag_cam) ClientSendMsg("photo_" + room_id.ToString());
                else MessageBox.Show("讲台关闭了截图权限");
            }
           
        }
        private void sendLogin()//广播教室号登陆
        {
            ClientSendMsg("LoginStudent_" + room_id.ToString());
        }
        public void startTCP() {
            try
            {
                int port = 5500;
                string host = "127.0.0.1";//服务器端ip地址
                IPAddress ip = IPAddress.Parse(host);
                IPEndPoint ipe = new IPEndPoint(ip, port);
                SocketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    //客户端套接字连接到网络节点上，用的是Connect  
                    SocketClient.Connect(ipe);
                }
                catch (Exception)
                {
                    Thread.Sleep(1000);
                    MessageBox.Show("连接服务器失败！\r\n");
                    Application.Exit();
                    return;
                }

                ThreadClient = new Thread(Recv);
                ThreadClient.IsBackground = true;
                ThreadClient.Start();

                Thread.Sleep(1000);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }
        public static void Recv()
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

                    if (strRevMsg == "false true" || strRevMsg == "true false" ||//接收到权限变化指令
                        strRevMsg == "false false" || strRevMsg == "true true")
                    {
                        switch (strRevMsg)
                        {
                            case "false true":
                                {
                                    flag_scr = false;
                                    flag_cam = true;
                                    break;
                                }
                            case "true false":
                                {
                                    flag_scr = true;
                                    flag_cam = false;
                                    break;
                                }
                            case "false false":
                                {
                                    flag_scr = false;
                                    flag_cam = false;
                                    break;
                                }
                            case "true true":
                                {
                                    flag_scr = true;
                                    flag_cam = true;
                                    break;
                                }
                                
                        }
                        //stage = 1;
                        //ClientSendMsg("select id,name from classroom;");
                    }

                   
                        Console.WriteLine(strRevMsg + "\r\n");
                       
                }
                catch (Exception ex)
                {
                    Console.WriteLine("远程服务器已经中断连接！" + ex.Message + "\r\n");
                    break;
                }
            }
        }

        //发送字符信息到服务端的方法  
        public static void ClientSendMsg(string sendMsg)
        {
            //将输入的内容字符串转换为机器可以识别的字节数组     
            byte[] arrClientSendMsg = Encoding.UTF8.GetBytes(sendMsg);
            //调用客户端套接字发送字节数组     
            SocketClient.Send(arrClientSendMsg);
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label4.Text = DateTime.Now.ToLongDateString()+" "+DateTime.Now.ToLongTimeString();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            startTCP();
            Thread.Sleep(500);
            string a = "select * from student;";
            ClientSendMsg(a);
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Thread.Sleep(500);
            //if (ThreadClient.IsAlive)
            //    ThreadClient.Abort();
            if (SocketClient.Connected)
            {
                SocketClient.Disconnect(true);
                SocketClient.Close();
                SocketClient.Dispose();
            }
        }
    }
}
