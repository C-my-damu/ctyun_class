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
        static bool flash_scr = false;
        static bool flash_cam = false;

        static bool newMsg = false;
        static string message = "";

        static string room_id = "";
        static string room_name = "";
        static string student_id = "";
        static string class_id = "";
        static string class_name = "";
        static string tempFile = "";

        
        private void sendLogin()//广播教室号登陆
        {
            ClientSendMsg("LoginStudent_" + room_id.ToString());
        }

        public int startTCP()//开启TCP链接
        {
            try
            {
                int port = 5500;
                string host = "117.80.86.174";//服务器端ip地址
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
                        strRevMsg.StartsWith("f-f") || strRevMsg.StartsWith("t-t")|| strRevMsg.StartsWith("fs") || strRevMsg.StartsWith("fc"))
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

        public bool Download(string url, string localfile)//下载文件方法
        {
            bool flag = false;
            long startPosition = 0; // 上次下载的文件起始位置
            FileStream writeStream; // 写入本地文件流对象

            // 判断要下载的文件夹是否存在
            if (File.Exists(localfile))
            {

                writeStream = File.OpenWrite(localfile);             // 存在则打开要下载的文件
                startPosition = writeStream.Length;                  // 获取已经下载的长度
                writeStream.Seek(startPosition, SeekOrigin.Current); // 本地文件写入位置定位
            }
            else
            {
                writeStream = new FileStream(localfile, FileMode.Create);// 文件不保存创建一个文件
                startPosition = 0;
            }


            try
            {
                HttpWebRequest myRequest = (HttpWebRequest)HttpWebRequest.Create(url);// 打开网络连接

                if (startPosition > 0)
                {
                    myRequest.AddRange((int)startPosition);// 设置Range值,与上面的writeStream.Seek用意相同,是为了定义远程文件读取位置
                }


                Stream readStream = myRequest.GetResponse().GetResponseStream();// 向服务器请求,获得服务器的回应数据流


                byte[] btArray = new byte[512];// 定义一个字节数据,用来向readStream读取内容和向writeStream写入内容
                int contentSize = readStream.Read(btArray, 0, btArray.Length);// 向远程文件读第一次

                while (contentSize > 0)// 如果读取长度大于零则继续读
                {
                    writeStream.Write(btArray, 0, contentSize);// 写入本地文件
                    contentSize = readStream.Read(btArray, 0, btArray.Length);// 继续向远程文件读取
                }

                //关闭流
                writeStream.Close();
                readStream.Close();

                flag = true;        //返回true下载成功
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                writeStream.Close();
                flag = false;       //返回false下载失败
            }

            return flag;
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)//时钟
        {
            label4.Text = DateTime.Now.ToLongDateString()+" "+DateTime.Now.ToLongTimeString();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            
        }

        private void Form1_Shown(object sender, EventArgs e)//初始化界面
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
            timer2.Enabled = true;
            
        }

        private void button2_Click(object sender, EventArgs e)//发送截图命令
        {
            ClientSendMsg("screen_"+room_id);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)//退出程序释放资源
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

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)//根据选择的学号加载姓名
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

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)//根据选择的教室加载课程
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

        private void button1_Click(object sender, EventArgs e)//签到按钮
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
                    string tempIP = string.Empty;
                    if (System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList.Length > 1)
                        tempIP = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList[1].ToString();
                    ClientSendMsg("sql- UPDATE `student` SET `ip` =  '!tempIP!'where(name='" + textBox1.Text + "') ");//注册学生端当前IP
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
                    ClientSendMsg("sql- select name from classroom where(name='" + textBox2.Text + "') ");//获得当前课程id
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
                    ClientSendMsg("sql- select * from choice where(id_student='" + student_id + "' and id_class='"+class_id+"') ");//查询是否第一次上这门课
                    while (!newMsg && t < 100)
                    {
                        Thread.Sleep(50);
                        t++;
                    }
                    if (t < 100)
                    {
                        if (message == "$")//无此课报名记录
                        {
                            ClientSendMsg("sql- INSERT INTO `choice` (`id_student`, `id_class`) VALUES (" + student_id + ", " + class_id + ");");//选课
                        }
                    }
                    

                    ClientSendMsg("sql- INSERT INTO `attend` (`id_student`, `id_room`,`date`) VALUES ("+student_id+", "+room_id+",'"+DateTime.Now.ToShortDateString()+" "+DateTime.Now.ToShortTimeString()+":"+DateTime.Now.Second.ToString()+"');");//登记上课记录
                    ClientSendMsg("login_" + room_id);//签到
                    class_name = textBox2.Text;
                    room_name = comboBox2.SelectedItem.ToString();
                    timer3.Enabled = true;
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

        private void button4_Click(object sender, EventArgs e)//退签按钮，并释放套接字
        {
            timer3.Enabled = false;
            ClientSendMsg("logout_" + room_id);
            Thread.Sleep(1000);
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

        private void button3_Click(object sender, EventArgs e)//发送拍照命令
        {
          
            ClientSendMsg("photo_"+room_id);
           
            
        }

        private void timer2_Tick(object sender, EventArgs e)//刷新权限
        {
            button2.Enabled = flag_scr;
            button3.Enabled = flag_cam;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            

        }

        private void timer3_Tick(object sender, EventArgs e)//刷新实时预览图
        {
            if (flag_scr)
            {
                try
                {
                    string subPath = "D:\\ctyunclass\\temp";
                    if (false == System.IO.Directory.Exists(subPath))
                    {
                        System.IO.Directory.CreateDirectory(subPath);
                    }



                    if (Download(Uri.EscapeUriString("http://117.80.86.174:88/" + class_name + "/temp/src.jpg"), subPath + "/src.jpg"))
                    {
                        Thread.Sleep(1000);
                        pictureBox2.Image = Image.FromFile(subPath + "/src.jpg");
                    }
                        
                }
                catch (Exception)
                {

                    throw;
                }
                flash_scr = false;
            }
            if (flash_cam)
            {
                try
                {
                    string subPath = "D:\\ctyunclass\\temp";
                    if (false == System.IO.Directory.Exists(subPath))
                    {
                        System.IO.Directory.CreateDirectory(subPath);
                    }



                    if (Download(Uri.EscapeUriString("http://117.80.86.174:88/" + class_name + "/temp/src.jpg"), subPath + "/cam.jpg"))
                    {
                        Thread.Sleep(1000);
                        pictureBox3.Image = Image.FromFile(subPath + "/cam.jpg");
                    }
                        
                }
                catch (Exception)
                {

                    throw;
                }
                flash_cam = false;
            }
            
        }
    }
}
