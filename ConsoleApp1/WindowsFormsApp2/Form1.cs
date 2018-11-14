using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        static TcpListener tcpListener = null;
        public delegate void TxtReceiveAddContentEventHandler(string txtValue);
        public void TxtReceiveAddContent(string txtValue)
        {
            if (txtReceive.InvokeRequired)
            {
                TxtReceiveAddContentEventHandler addContent = TxtReceiveAddContent;
                txtReceive.Invoke(addContent, new object[] { txtValue });
            }
            else
            {
                txtReceive.Text = txtValue + "\r\n" + txtReceive.Text;
            }
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }
        private void rev(object obj)//接收文件并分类归档
        {
            TcpClient tcpClient = obj as TcpClient;
            int t = 0;
            while (true)
            {
                //TxtReceiveAddContent(tcpClient.Connected.ToString());
                if (tcpClient.Connected)
                {
                    try
                    {
                        NetworkStream stream = tcpClient.GetStream();
                        if (stream != null && stream.DataAvailable)
                        {

                            byte[] fileNameLengthForValueByte = Encoding.Unicode.GetBytes((256).ToString("D11"));
                            byte[] fileNameLengByte = new byte[1024];
                            byte[] filePathLengByte = new byte[1024];
                            int fileNameLengthSize = stream.Read(fileNameLengByte, 0, fileNameLengthForValueByte.Length);
                            string fileNameLength = Encoding.Unicode.GetString(fileNameLengByte, 0, fileNameLengthSize);

                            TxtReceiveAddContent("文件名字符流的长度为：" + fileNameLength);

                            int fileNameLengthNum = Convert.ToInt32(fileNameLength);
                            byte[] fileNameByte = new byte[fileNameLengthNum];

                            int fileNameSize = stream.Read(fileNameByte, 0, fileNameLengthNum);
                            string fileName = Encoding.Unicode.GetString(fileNameByte, 0, fileNameSize);
                            TxtReceiveAddContent("文件名为：" + fileName);

                            int filePathLengthSize = stream.Read(filePathLengByte, 0, fileNameLengthForValueByte.Length);
                            string filePathLength = Encoding.Unicode.GetString(filePathLengByte, 0, filePathLengthSize);

                            int filePathLengthNum = Convert.ToInt32(filePathLength);
                            byte[] filePathByte = new byte[filePathLengthNum];
                            TxtReceiveAddContent("文件路径字符流的长度为：" + filePathLength);

                            int filePathSize = stream.Read(filePathByte, 0, filePathLengthNum);
                            string filePath = Encoding.Unicode.GetString(filePathByte, 0, filePathSize);
                            TxtReceiveAddContent("文件路径为：" + filePath);

                            string pathTemp = "";
                            if (filePath.StartsWith("D:\\ctyunclass\\"))
                            {
                                pathTemp = filePath.Replace("D:\\ctyunclass\\", "D:\\FILES\\");
                            }
                            string dirPath = pathTemp;
                            if (!Directory.Exists(dirPath))
                            {
                                Directory.CreateDirectory(dirPath);
                            }
                            FileStream fileStream = new FileStream(dirPath + "\\" + fileName, FileMode.Create, FileAccess.Write);
                            int fileReadSize = 0;
                            byte[] buffer = new byte[2048];
                            while ((fileReadSize = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                fileStream.Write(buffer, 0, fileReadSize);

                            }
                            fileStream.Flush();
                            fileStream.Close();
                            stream.Flush();
                            stream.Close();
                            //stream.Dispose();
                            tcpClient.Close();
                            TxtReceiveAddContent("接收成功");
                            
                            string week = "第" + getWeek().ToString() + "周";
                            string destpath = dirPath.Replace("temp", week+"\\"+fileName.Replace(".jpg",""));
                            string copyname = fileName.Replace("pic", DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + "-" + DateTime.Now.ToShortTimeString().Replace(":", "-") + "-" + DateTime.Now.Second.ToString());
                            copyname = fileName.Replace("src", DateTime.Now.Month.ToString()+"-"+DateTime.Now.Day.ToString() + "-" + DateTime.Now.ToShortTimeString().Replace(":", "-") + "-" + DateTime.Now.Second.ToString());
                            if (!Directory.Exists(destpath))
                            {
                                Directory.CreateDirectory(destpath);
                            }
                            TxtReceiveAddContent("copy to :"+destpath  + copyname);
                            File.Copy(dirPath + "\\" + fileName, destpath+copyname );
                        }
                    }
                    catch (Exception)
                    {
                        //tcpClient.Close();
                        TxtReceiveAddContent(t.ToString()+"断开连接");
                       // throw;
                    }
                    
                    
                }
                else
                {
                    tcpClient.Close();
                    tcpClient.Dispose();
                    break;

                }
            
            }
            TxtReceiveAddContent(t.ToString() + "断开连接,进程结束");
        }
        private int getWeek()
        {
            int week = 0;
            if (DateTime.Now.Month < 3)
            {
                week = (DateTime.Now.DayOfYear + 122) / 7;
            }
            else
            {
                if (DateTime.Now.Month<9)
                {
                    week = (DateTime.Now.DayOfYear - 59) / 7;               
                }
                else
                {
                    week = (DateTime.Now.DayOfYear - 243) / 7;    
                }
            }
            return week;
        }
        private void ReceiveFileFunc()//接受请求并启动接收文件线程
        {
            TcpClient tcpClient = null;
            while (true)
            {               
                try
                {
                    tcpClient = tcpListener.AcceptTcpClient();

                }
                catch (Exception) {
                    break;
                }
                TxtReceiveAddContent(tcpClient.Client.RemoteEndPoint.ToString() + "连接成功");
                Thread thread = new Thread(rev);
                thread.Start(tcpClient);
                thread.IsBackground = true;
            }
        }

        private void Form1_Shown(object sender, EventArgs e)//程序启动，拉起SQL后台
        {
            tcpListener = new TcpListener(IPAddress.Any, 6000);
            tcpListener.Start();
            txtReceive.Text = "开始侦听...";
            Thread thread = new Thread(ReceiveFileFunc);
            thread.Start();
            thread.IsBackground = true;
            Thread.Sleep(500);
            
            

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
