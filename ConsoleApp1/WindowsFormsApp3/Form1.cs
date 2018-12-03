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
using MySql.Data;
using MySql.Data.MySqlClient;

namespace WindowsFormsApp3
{
    public partial class Form1 : Form
    {
        static Thread ThreadClient = null;
        static Socket SocketClient = null;
        static MySqlConnection localSql = null;
        int t = 0;
        string path = "";

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.ShowDialog();
            path = fileDialog.FileName;
            StreamReader file = new StreamReader(path,Encoding.GetEncoding("GB2312"));
            string data = file.ReadLine();
            label5.Text = data;
            file.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == "" || textBox1.Text == "" || textBox5.Text == "" || textBox6.Text == "")
            {
                MessageBox.Show("信息填写不全");
                return;
            }
            localSql = new MySqlConnection("Database="+textBox2.Text+ ";Data Source=" + textBox1.Text + ";User Id=" + textBox5.Text + ";Password=" + textBox6.Text);
            try
            {
                //localSql = new MySqlConnection("Database=ctyun_class;Data Source=117.80.86.174;User Id=pc-test;Password=damu19950313_");
                localSql.Open();
                MessageBox.Show("connected!");               
               
            }
            catch (Exception)
            {
                MessageBox.Show("can not open database!\r\n");
                throw;
            }
        }

        void upload()
        {
            StreamReader file = new StreamReader(path, Encoding.GetEncoding("GB2312"));
            MySqlCommand mycmd = localSql.CreateCommand();
            string data = file.ReadLine();
            int n = 0;
            string temp0 = "";
            while (!file.EndOfStream)
            {

                string temp = file.ReadLine();
                temp0 = "INSERT INTO " + textBox4.Text + " (`" + data.Replace(",", "`,`") + "`,`id`) VALUES ('" + temp.Replace(",", "','").Replace("\"", "") + "','" + t.ToString() + "');\n\r";
                mycmd.CommandText += temp0;
                mycmd.CommandType = CommandType.Text;                
                t++;
                n++;
                if (n == 500)
                {
                    MySqlDataReader sdr = null;
                    n = 0;
                    try
                    {
                        sdr = mycmd.ExecuteReader();
                        sdr.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(temp0);
                        Console.WriteLine("error:" + ex.Message + "in " + t.ToString());

                    }
                    Thread.Sleep(100);
                    //label7.Text = t.ToString();
                    mycmd.Cancel();
                    mycmd.Dispose();
                    mycmd = localSql.CreateCommand();
                }

            }
            MySqlDataReader sdr1 = null;
            try
            {
                sdr1 = mycmd.ExecuteReader();
                sdr1.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(temp0);
                Console.WriteLine("error:" + ex.Message + "in " + t.ToString());

            }
            mycmd.Cancel();
            mycmd.Dispose();

        }



        private void button2_Click(object sender, EventArgs e)
        {
            Thread thread = new Thread(upload);
            thread.Start();
            timer1.Enabled = true;
            //StreamReader file = new StreamReader(path,Encoding.GetEncoding("GB2312"));
            //MySqlCommand mycmd = localSql.CreateCommand();
            //string data = file.ReadLine();
            //int t = 0;
            //string temp0 = "";
            //while (!file.EndOfStream)
            //{

            //    string temp = file.ReadLine();
            //    temp0 = "INSERT INTO " + textBox4.Text + " (`" + data.Replace(",","`,`")+"`,`id`) VALUES ('"+temp.Replace(",","','").Replace("\"","") + "','"+t.ToString()+"');\n\r";


            //    string sql = string.Format(temp0);
            //    mycmd.CommandText = temp0;
            //    mycmd.CommandType = CommandType.Text;
            //    MySqlDataReader sdr = null;
            //    try
            //    {
            //        sdr = mycmd.ExecuteReader();
            //        sdr.Close();
            //    }
            //    catch (Exception ex )
            //    {
            //        Console.WriteLine(temp0);
            //        Console.WriteLine("error:" + ex.Message + "in " + t.ToString());

            //    }                

            //    t++;
            //    if(t%500==0)
            //    {
            //        try
            //        {
            //            sdr = mycmd.ExecuteReader();
            //            sdr.Close();
            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine(temp0);
            //            Console.WriteLine("error:" + ex.Message + "in " + t.ToString());

            //        }
            //        Thread.Sleep(500);
            //        label7.Text = t.ToString();
            //        mycmd.Cancel();
            //        mycmd.Dispose();
            //        mycmd = localSql.CreateCommand();
            //    }

        //}
        //    MySqlDataReader sdr1 = null;
        //    try
        //    {
        //        sdr1 = mycmd.ExecuteReader();
        //        sdr1.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(temp0);
        //        Console.WriteLine("error:" + ex.Message + "in " + t.ToString());

        //    }
        //    mycmd.Cancel();
        //    mycmd.Dispose();

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label2.Text = t.ToString();
        }
    }
}
