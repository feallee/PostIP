using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace PostIP
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                string url = HttpApi(textBox1.Text.Trim(), textBox2.Text.Trim());
                if (textBox3.TextLength >= 65535) textBox3.Clear();
                textBox3.AppendText($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] 映射地址:{url}\r\n");

            }
            catch (Exception ex)
            {
                if (textBox3.TextLength >= 65535) textBox3.Clear();
                textBox3.AppendText($"在[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}]产生错误：\r\n");
                textBox3.AppendText($"{ex.Message}\r\n");
            }
        }
         static string HttpApi(string url, string data)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);//webrequest请求api地址  
            request.Accept = "text/html,application/xhtml+xml,*/*";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            byte[] buffer = Encoding.Default.GetBytes(data);
            request.ContentLength = buffer.Length;
            var stream = request.GetRequestStream();
            stream.Write(buffer, 0, buffer.Length);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            var responseStream = response.GetResponseStream();


            var streamReader = new StreamReader(responseStream, Encoding.Default);
            string retString = streamReader.ReadToEnd();

            streamReader.Close();
            responseStream.Close();
            return retString;
        }



        private void button1_Click(object sender, EventArgs e)
        {
            timer1_Tick(this, null);
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
