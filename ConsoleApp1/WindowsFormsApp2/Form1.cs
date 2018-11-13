﻿using System;
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

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
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
        private void rev(object obj)
        {
            TcpClient tcpClient = (TcpClient)obj;
            while (true) {
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
                                pathTemp = filePath.Replace("D:\\ctyunclass\\", "D:\\FILES");

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
                            tcpClient.Close();
                            TxtReceiveAddContent("接收成功");
                        }
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                    
                    
                }
            }
        }
        private void ReceiveFileFunc(object obj)
        {
            TcpListener tcpListener = obj as TcpListener;
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

        private void Form1_Shown(object sender, EventArgs e)
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Any, 6000);
            tcpListener.Start();
            txtReceive.Text = "开始侦听...";
            Thread thread = new Thread(ReceiveFileFunc);
            thread.Start(tcpListener);
            thread.IsBackground = true;
        }
    }
}
