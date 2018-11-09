﻿using System;
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

        static bool newMsg = false;
        static string message = "";

        static string room_id = "";
        static string student_id = "";
        static string class_id = "";
        static string class_name = "";

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
        public int startTCP() {
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

                    if (strRevMsg.StartsWith("f-t") || strRevMsg.StartsWith("t-f") ||//接收到权限变化指令
                        strRevMsg.StartsWith("f-f") || strRevMsg.StartsWith("t-t"))
                    {
                        if (strRevMsg.EndsWith(class_name))
                        {
                            string s = strRevMsg.Replace(class_name, "");
                            switch (s)
                            {
                                case "f-t":
                                    {
                                        flag_scr = false;
                                        flag_cam = true;
                                        break;
                                    }
                                case "t-f":
                                    {
                                        flag_scr = true;
                                        flag_cam = false;
                                        break;
                                    }
                                case "f-f":
                                    {
                                        flag_scr = false;
                                        flag_cam = false;
                                        break;
                                    }
                                case "t-t":
                                    {
                                        flag_scr = true;
                                        flag_cam = true;
                                        break;
                                    }

                            }
                        }
                        //stage = 1;
                        //ClientSendMsg("select id,name from classroom;");
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

        //发送字符信息到服务端的方法  
        public static void ClientSendMsg(string sendMsg)
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
           if(-1!= startTCP())
            {
                Thread.Sleep(500);
                string a = "select * from student;";
                ClientSendMsg(a);


                comboBox1.Items.Clear();
                newMsg = false;
                Console.WriteLine(newMsg.ToString());
                int t = 0;
                ClientSendMsg("sql- select classnumber from student;");
                while (!newMsg && t < 100)
                {
                    Thread.Sleep(50);
                    t++;
                }
                if (t < 100)
                {
                    string[] a1 = message.Split('$');
                    comboBox1.Items.AddRange(a1);

                }
                comboBox1.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                comboBox1.AutoCompleteSource = AutoCompleteSource.ListItems;

                Thread.Sleep(500);

                comboBox2.Items.Clear();
                newMsg = false;
                Console.WriteLine(newMsg.ToString());
                t = 0;
                ClientSendMsg("sql- select name from classroom;");
                while (!newMsg && t < 100)
                {
                    Thread.Sleep(50);
                    t++;
                }
                if (t < 100)
                {
                    string[] a1 = message.Split('$');
                    comboBox2.Items.AddRange(a1);

                }
                comboBox2.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                comboBox2.AutoCompleteSource = AutoCompleteSource.ListItems;
            }
            else
            {
                Application.Exit();
            }

            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ClientSendMsg("screen_"+room_id);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Thread.Sleep(500);
            if (ThreadClient!=null && ThreadClient.IsAlive)
                ThreadClient.Abort();
            if (SocketClient!=null && SocketClient.Connected)
            {
                SocketClient.Disconnect(true);
                SocketClient.Close();
                SocketClient.Dispose();
            }
        }

        

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Text = "";
            newMsg = false;
            Console.WriteLine(newMsg.ToString());
            int t = 0;            
            ClientSendMsg("sql- select name from student where(classnumber='" + comboBox1.SelectedItem.ToString() + "');");
            while (!newMsg && t < 100)
            {
                Thread.Sleep(50);
                t++;
            }
            if (t < 100)
            {
                string[] a = message.Split('$');
                textBox1.Text = (a[0]);

            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox2.Text = "";
            newMsg = false;
            Console.WriteLine(newMsg.ToString());
            int t = 0;
            ClientSendMsg("sql- select class_now from classroom where(name='" + comboBox2.SelectedItem.ToString() + "');");
            while (!newMsg && t < 100)
            {
                Thread.Sleep(50);
                t++;
            }
            if (t < 100)
            {
                string[] a = message.Split('$');
                textBox2.Text = (a[0]);

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!SocketClient.Connected)
            {
                Thread.Sleep(1000);
                if (-1 == startTCP())//若连接异常推出程序
                {
                    MessageBox.Show("连接服务器失败！");
                    Thread.Sleep(1000);
                    Application.Exit();
                }
                else
                {
                    MessageBox.Show("连接服务器成功！");
                }
            }
            if (comboBox1.SelectedIndex.ToString() != "" && comboBox2.SelectedItem.ToString() != "")
            {
                DialogResult dr = MessageBox.Show("学号：" + comboBox1.SelectedItem.ToString()+"  姓名："+textBox1.Text + "\n\r教室：" + comboBox2.SelectedItem.ToString()+"  课程："+textBox2.Text + "\n\r是否确认？", "取消", MessageBoxButtons.OKCancel);
                if (dr == DialogResult.OK)
                {
                    ClientSendMsg("sql- UPDATE `student` SET `ip` = '" + SocketClient.LocalEndPoint.ToString() + "'where(name='" + textBox1.Text + "') ");//注册学生端当前IP
                    newMsg = false;
                    int t = 0;
                    ClientSendMsg("sql- select id from student where(classnumber='" + comboBox1.SelectedItem.ToString() + "') ");//获得当前学生id
                    while (!newMsg && t < 100)
                    {
                        Thread.Sleep(50);
                        t++;
                    }
                    if (t < 100)
                    {
                        student_id = message.Split('$')[0];
                        Console.WriteLine("student_id:" + student_id + "\n\r");
                    }
                    //Thread.Sleep(500);
                    t = 0;
                    newMsg = false;
                    ClientSendMsg("sql- select id from classroom where(name='" + comboBox2.SelectedItem.ToString() + "') ");//获得当前教室id
                    while (!newMsg && t < 100)
                    {
                        Thread.Sleep(50);
                        t++;
                    }
                    if (t < 100)
                    {
                        room_id = message.Split('$')[0];
                        Console.WriteLine("room_id:" + room_id + "\n\r");
                    }
                    //Thread.Sleep(500);
                    t = 0;
                    newMsg = false;
                    ClientSendMsg("sql- select id from class where(name='" + textBox2.Text + "') ");//获得当前课程id
                    while (!newMsg && t < 100)
                    {
                        Thread.Sleep(50);
                        t++;
                    }
                    if (t < 100)
                    {
                        class_id = message.Split('$')[0];
                        Console.WriteLine("class_id:" + class_id + "\n\r");
                    }
                    //Thread.Sleep(500);
                    t = 0;
                    newMsg = false;
                    ClientSendMsg("sql- select * from choice where(id_student='" + student_id + "' and id_class='"+class_id+"') ");//获得当前课程id
                    while (!newMsg && t < 100)
                    {
                        Thread.Sleep(50);
                        t++;
                    }
                    if (t < 100)
                    {
                        if (message == "$")
                        {
                            ClientSendMsg("sql- INSERT INTO `choice` (`id_student`, `id_class`) VALUES (" + student_id + ", " + class_id + ");");
                        }
                    }
                    

                    ClientSendMsg("sql- INSERT INTO `attend` (`id_student`, `id_room`,`date`) VALUES ("+student_id+", "+room_id+",'"+DateTime.Now.ToShortDateString()+" "+DateTime.Now.ToShortTimeString()+":"+DateTime.Now.Second.ToString()+"');");
                    ClientSendMsg("login_" + room_id);
                    class_name = comboBox2.SelectedIndex.ToString();
                    comboBox1.Enabled = false;
                    comboBox2.Enabled = false;
                }
                else
                {
                    //do nothing
                }


            }
            else
                MessageBox.Show("当前教室或课程未选择！");
            button4.Enabled = true;
            button1.Enabled = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (ThreadClient.IsAlive)
                ThreadClient.Abort();
            if (SocketClient.Connected)
            {
                ClientSendMsg("close");
                SocketClient.Shutdown(SocketShutdown.Both);

                SocketClient.Close();
                SocketClient.Dispose();
            }

            
            button1.Enabled = true;
            button4.Enabled = false;
            comboBox2.Enabled = true;
            comboBox1.Enabled = true;
        
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ClientSendMsg("photo_"+room_id);
        }
    }
}
