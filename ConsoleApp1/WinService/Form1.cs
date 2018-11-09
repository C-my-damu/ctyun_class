﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using AForge;
using AForge.Controls;
using AForge.Video;
using AForge.Video.DirectShow;
using Size = System.Drawing.Size;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.CvEnum;
using System.Net.Sockets;
using System.Net;

namespace WinService
{
    public partial class Form1 : Form
    {

        static string filepath_scr = "";//待上传截屏文件的文件目录
        static string filepath_cam = "";//待上传照片文件的文件目录
     
        
        static bool flag_scr =true;//截屏权限
        static bool flag_cam = false;//相机权限
        static bool do_scr = false;//截屏请求
        static bool do_cam = false;//相机请求

        static string room_id = "";//教室码
        static string class_id = "";//课程码
        static int member = 0;//签到人次

        static string message = "";//tcp取回的信息
        static bool newMsg = false;//指示套接字接收是否已经刷新

        private FilterInfoCollection videoDevices;//存放摄像头列表
        private VideoCaptureDevice videoSource;//视频源
        private int Indexof = 0;//摄像头的当前选择项

        static Thread ThreadClient = null;//TCP线程
        static Socket SocketClient = null;//TCP客户端

        static ThreadStart threadStart = new ThreadStart(startUpload);//文件上传线程
        Thread clock0 = new Thread(threadStart);

        public static void startUpload()//上传文件线程，上传错误则抛出提示
        {
            int t = 0;
            while (uploadFile("file2update") != 1&&t<10) {
                t++;
            };
            if (t == 10) {
                MessageBox.Show("文件上传失败，请检查网络连接");
            }
        }

        public static int uploadFile(string file)//上传文件实体方法
        {
            //to do:获取时间，打时间戳，课程戳，写文件名，上传文件
            //1成功0失败
            return 1;
        }

        public void pushButton1()
        {
            button1.PerformClick();

        }

        public void pushButton2()
        {
            button2.PerformClick();

        }

        public static int connectToCtyun()//启动套接字连接服务器
        {
            try
            {
                int port = 5500;
                string host = "127.0.0.1";//服务器端ip地址
                IPAddress ip = IPAddress.Parse(host);
                IPEndPoint ipe = new IPEndPoint(ip, port);

                //定义一个套接字监听  
                SocketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    //客户端套接字连接到网络节点上，用的是Connect   
                    SocketClient.Connect(ipe);
                }
                catch (Exception)
                {
                    MessageBox.Show("连接失败！\r\n程序即将关闭");
                   // Thread.Sleep(1000);
                    //
                    return -1;
                }

                ThreadClient = new Thread(Recv);
                ThreadClient.IsBackground = true;
                ThreadClient.Start();

                Thread.Sleep(1000);
                //Console.WriteLine("请输入内容<按Enter键发送>：\r\n");
                //while (true)
                //{
                //    string sendStr = Console.ReadLine();
                //    ClientSendMsg(sendStr);
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // Console.ReadLine();
                return -1;
            }
            return 1;
        }

        public static void Recv()//接收信息
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
                    if(strRevMsg=="photo"|| strRevMsg == "screen" || strRevMsg == "login")
                    {
                        switch (strRevMsg)
                        {
                            case "screen":
                                {
                                    do_scr = true;
                                    break;
                                }
                            case "photo":
                                {
                                    do_cam = true;
                                    break;
                                }
                            case "login":
                                {
                                    member++;
                                    break;
                                }
                            case "logout":
                                {
                                    if (member>0)
                                    member--;
                                    break;
                                }
                        }

                    }
                    else
                    {
                        message = strRevMsg;//将获得的数据放在缓存
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
       
        public static void ClientSendMsg(string sendMsg)//发送字符信息到服务端的方法  
        {
            //将输入的内容字符串转换为机器可以识别的字节数组     
            byte[] arrClientSendMsg = Encoding.UTF8.GetBytes(sendMsg);
            //调用客户端套接字发送字节数组  
            try
            {
                SocketClient.Send(arrClientSendMsg);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                //throw;
            }
            
        }

        private Bitmap GetScreenCapture()//截图
        {
            Rectangle tScreenRect = new Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Bitmap tSrcBmp = new Bitmap(tScreenRect.Width, tScreenRect.Height); // 用于屏幕原始图片保存
            Graphics gp = Graphics.FromImage(tSrcBmp);
            gp.CopyFromScreen(0, 0, 0, 0, tScreenRect.Size);
            gp.DrawImage(tSrcBmp, 0, 0, tScreenRect, GraphicsUnit.Pixel);
            return tSrcBmp;
        }

        private void Camlist()//获取摄像头列表
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (videoDevices.Count == 0)
            {
                MessageBox.Show("未找到摄像头设备");
            }
            foreach (FilterInfo device in videoDevices)
            {
                Cameralist.Items.Add(device.Name);
                Cameralist.SelectedIndex = 0;
            }
        }

        private void startCam()//开启摄像头
        {
            Indexof = Cameralist.SelectedIndex;
            if (Indexof < 0)
            {
                MessageBox.Show("请选择一个摄像头");
                return;
            }
           videoSourcePlayer1.Visible = true;//videoDevices[Indexof]确定出用哪个摄像头了。
            videoSource = new VideoCaptureDevice(videoDevices[Indexof].MonikerString);
            videoSourcePlayer1.VideoSource = videoSource;
            videoSourcePlayer1.Start();          
        }

        private void closeCam()//关闭摄像头
        {
            Indexof = Cameralist.SelectedIndex;         
            videoSourcePlayer1.Visible = false;
            videoSource.Stop();           
            videoSourcePlayer1.Stop();

        }

        public static Bitmap ResizeUsingEmguCV(Bitmap original, int newWidth, int newHeight)//图片插值缩放
        {
            try
            {
                Emgu.CV.Image<Emgu.CV.Structure.Rgb, byte> image =
                    new Emgu.CV.Image<Emgu.CV.Structure.Rgb, byte>(original);
                Emgu.CV.Image<Emgu.CV.Structure.Rgb, byte> newImage = image.Resize(
                    newWidth, newHeight, Emgu.CV.CvEnum.Inter.Cubic);
                return newImage.Bitmap;
            }
            catch
            {
                return null;
            }
        }

        private Bitmap getPhoto()//拍照
        {
                 Bitmap b = new Bitmap( videoSourcePlayer1.GetCurrentVideoFrame());
            return b;

        }

        private void sendFlag()//广播教室号和权限
        {
            string flag = "";
            switch (flag_scr)
            {
                case true:
                    {
                        flag += "t";
                        break;
                    }
                case false:
                    {
                        flag += "f";
                        break;
                    }
            }
            switch (flag_cam)
            {
                case true:
                    {
                        flag += "-t";
                        break;
                    }
                case false:
                    {
                        flag += "-f";
                        break;
                    }
            }
            ClientSendMsg("flag_"+flag+comboBox2.SelectedItem.ToString());
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)//初始化界面，载入预设
        {
            timer1.Enabled = true;
            Camlist();

            //button4.BackColor = Color.Blue;
            button5.BackColor = Color.Red;
            button6.BackColor = Color.Blue;

        }

        private void timer1_Tick(object sender, EventArgs e)//时钟
        {
            
            label4.Text = DateTime.Now.ToString();
        }

        private void button6_Click(object sender, EventArgs e)//控制截屏权限
        {
            flag_scr = !flag_scr;
            button1.Enabled = !button1.Enabled;
            if (flag_scr)
                button6.BackColor = Color.Blue;
            else
                button6.BackColor = Color.Red;
            sendFlag();
        }

        private void button5_Click(object sender, EventArgs e)//控制拍照权限
        {
            //timer2.Enabled = !timer2.Enabled;
            flag_cam = !flag_cam;
            button2.Enabled = !button2.Enabled;
            if (flag_cam)
            {
                button5.BackColor = Color.Blue;
                //pictureBox2.Visible = true;
                startCam();
                timer2.Enabled = true;
            }
            else
            {
                button5.BackColor = Color.Red;
                //pictureBox2.Visible = false;
                timer2.Enabled = false;
                closeCam();
            }
            Thread.Sleep(2500);
            sendFlag();
        }

        private void button1_Click(object sender, EventArgs e)//截图按钮
        {
            string name = System.DateTime.Now.Hour.ToString()+"_"+ System.DateTime.Now.Minute.ToString() + "_" + System.DateTime.Now.Second.ToString() ;
            string date = System.DateTime.Now.ToLongDateString();
            FormWindowState t = WindowState;
            WindowState = FormWindowState.Minimized;
            Thread.Sleep(200);
            Bitmap b = GetScreenCapture();
            Graphics g = Graphics.FromImage(b);
            g.CopyFromScreen(0, 0, 0, 0, new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height));
            try
            {
                string subPath = "D:/ctyunclass/" + class_id + "/src/"+date+"/";
                if (false == System.IO.Directory.Exists(subPath))
                {
                    System.IO.Directory.CreateDirectory(subPath);
                }
                b.Save(subPath + name + ".jpg");
            }
            catch (Exception)
            {
                WindowState = t;
                MessageBox.Show("截屏保存失败，请确认软件权限或关闭截图功能");
                throw;
            }
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
                pictureBox1.Image = null;
            }
            pictureBox1.Image = b;
            WindowState = t;
            //Clipboard.SetImage(b);
            this.Cursor = Cursors.Default;
        }

        private void button2_Click(object sender, EventArgs e)//拍照按钮
        {
            string name = System.DateTime.Now.Hour.ToString() + "_" + System.DateTime.Now.Minute.ToString() + "_" + System.DateTime.Now.Second.ToString();
            string date = System.DateTime.Now.ToLongDateString();
            if (videoSourcePlayer1.IsRunning == true)
            {
                Bitmap b= getPhoto();
                Bitmap bb = ResizeUsingEmguCV(b, 1280, 720);
                try
                {
                    string subPath = "D:/ctyunclass/" + class_id + "/pic/" + date + "/";
                    if (false == System.IO.Directory.Exists(subPath))
                    {
                        System.IO.Directory.CreateDirectory(subPath);
                    }
                    bb.Save(subPath + name + ".jpg");
                    pictureBox2.Visible = true;
                    pictureBox2.Image = b;
                   
                   // videoSourcePlayer1.Stop();
                    videoSourcePlayer1.Visible = false;                   
                    Thread.Sleep(2000);
                    videoSourcePlayer1.Visible = true;
                    //videoSourcePlayer1.Start();
                    pictureBox2.Image.Dispose();
                    pictureBox2.Image = null;
                    pictureBox2.Visible = false;
                    
                }
                catch (Exception)
                {
                    MessageBox.Show("截屏保存失败，请确认软件权限或关闭拍照功能");
                    throw;
                }
            }
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)//程序结束释放资源
        {
            if (ThreadClient != null && ThreadClient.IsAlive)
            {
                ThreadClient.Abort();
            }
                if (SocketClient!=null&&SocketClient.Connected)
                {
                   // ClientSendMsg("close");
                    SocketClient.Shutdown(SocketShutdown.Both);

                    SocketClient.Close();
                    SocketClient.Dispose();
                }

                videoSourcePlayer1.Stop();
                if (videoSource != null && videoSource.IsRunning)
                {
                    videoSource.Stop();
                }
                //to do:断开与数据库的连接
            
            Thread.Sleep(1000);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
           // Thread.Sleep(1000);
            if (-1 == connectToCtyun())//若连接异常推出程序
            {
               // Thread.Sleep(1000);
                Application.Exit();
            }
            Thread.Sleep(1000);
            //ClientSendMsg("room");
        }//窗体控件初始化

        private void comboBox2_DropDown(object sender, EventArgs e)
        {
            comboBox2.Items.Clear();
            newMsg = false;
            Console.WriteLine(newMsg.ToString());
            int t = 0;
            ClientSendMsg("sql-select name from classroom;");
            while (!newMsg&&t<100)
            {
                Thread.Sleep(50);             
                t++;
            }
            if (t < 100)
            {
                string[] a = message.Split('$');
                comboBox2.Items.AddRange(a);
            }
        }//教室下拉菜单

        private void comboBox3_DropDown(object sender, EventArgs e)
        {
            comboBox3.Items.Clear();
            newMsg = false;
            Console.WriteLine(newMsg.ToString());
            int t = 0;
            ClientSendMsg("sql-select name from class;");
            while (!newMsg && t < 100)
            {
                Thread.Sleep(50);              
                t++;
            }
            if (t < 100)
            {
                string[] a = message.Split('$');
                comboBox3.Items.AddRange(a);
            }
        }//课程下拉菜单

        private void button3_Click(object sender, EventArgs e)//上课按钮，将客户端注册到服务器
        {
            if (!SocketClient.Connected)
            {
                Thread.Sleep(1000);
                if (-1 == connectToCtyun())//若连接异常推出程序
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
            if (comboBox2.SelectedIndex.ToString() != "" && comboBox3.SelectedItem.ToString() != "")
            {
                DialogResult dr = MessageBox.Show("当前教室：" + comboBox2.SelectedItem.ToString() + "\n\r当前课程：" + comboBox3.SelectedItem.ToString() + "\n\r是否确认？", "取消", MessageBoxButtons.OKCancel);
                if (dr == DialogResult.OK)
                {
                    ClientSendMsg("sql- UPDATE `classroom` SET `ip` = '"+SocketClient.LocalEndPoint.ToString()+"', `class_now` = '"+comboBox3.SelectedItem.ToString()+"'where(name='"+comboBox2.SelectedItem.ToString()+"') ");
                    newMsg = false;
                    int t = 0;
                    ClientSendMsg("sql- select id from classroom where(name='" + comboBox2.SelectedItem.ToString() + "') ");
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
                    Thread.Sleep(500);
                    t = 0;
                    newMsg = false;
                    ClientSendMsg("sql- select id from class where(name='" + comboBox3.SelectedItem.ToString() + "') ");
                    while (!newMsg && t < 100)
                    {
                        Thread.Sleep(50);
                        t++;
                    }
                    if (t < 100)
                    {
                        class_id = message.Split('$')[0];
                        Console.WriteLine("class_id:" + room_id + "\n\r");
                    }
                    comboBox2.Enabled = false;
                    comboBox3.Enabled = false;
                }
                else
                {
                    //do nothing
                }

                
            }
            else
            MessageBox.Show("当前教室或课程未选择！");
            button4.Enabled = true;
            button3.Enabled = false;
        }

        private void timer2_Tick(object sender, EventArgs e)//刷新签到人数并检测是否需要截图拍照
        {
            label8.Text = member.ToString();
            if (do_scr)
            {
                button1.PerformClick();
                do_scr = false;
            }
            if (do_cam)
            {
                button2.PerformClick();
                do_cam = false;
            }
        }

        private void button4_Click(object sender, EventArgs e)//下课按钮，清空签到人数，释放套接字
        {
            member = 0;
            
            if (ThreadClient.IsAlive)
                ThreadClient.Abort();
            if (SocketClient.Connected)
            {
                ClientSendMsg("close");
                SocketClient.Shutdown(SocketShutdown.Both);

                SocketClient.Close();
                SocketClient.Dispose();
            }

            videoSourcePlayer1.Stop();
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.Stop();
            }
            button3.Enabled = true;
            button4.Enabled = false;
            comboBox2.Enabled = true;
            comboBox3.Enabled = true;
        }
   
    }
}
