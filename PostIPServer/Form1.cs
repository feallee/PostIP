using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace PostIPServer
{
    public partial class Form1 : Form
    {
        readonly ConcurrentDictionary<string, string> buffer = new ConcurrentDictionary<string, string>();
        readonly BackgroundWorker worker = new BackgroundWorker();
        public Form1()
        {
            InitializeComponent();
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (textBox1.TextLength >= 65535) textBox1.Clear();
            textBox1.AppendText($"\r\n[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}]\t{e.UserState}");
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            HttpListener listener = null;
            while (!worker.CancellationPending)
            {
                try
                {
                    if (listener == null)
                    {
                        listener = new HttpListener();
                        listener.Prefixes.Add($"http://+:{toolStripTextBox1.Text.Trim()}/");
                        listener.Start();
                        worker.ReportProgress(0, $"状态\t服务已启动");
                    }
                    Listen(listener);
                }
                catch (Exception ex)
                {
                    worker.ReportProgress(0, $"错误\t{ex.Message}");
                    listener.Close();
                    listener = null;
                    worker.ReportProgress(0, $"状态\t服务错误停止");
                }
                Thread.Sleep(500);
            }
            if (listener != null) listener.Close();
            worker.ReportProgress(0, $"状态\t服务正常停止");
        }

        void Listen(HttpListener listener)
        {
            var context = listener.GetContext();
            var q1 = context.Request.QueryString.Get("action");
            var q2 = context.Request.QueryString.Get("token");
            string action = q1 == null ? "" : q1.ToLower();
            string token = q2 == null ? "" : q2.ToLower();
            string r;
            if (action == "__post__")
            {
                string format = HandleRequest(context.Request);
                string ip = context.Request.RemoteEndPoint.Address.ToString();
                string url = String.Format(format, ip);
                buffer[token] = url;
                r = $"更新\t[{token}]=>{url}";
                worker.ReportProgress(0, r);
                r = ip;
            }
            else
            {
                if (buffer.TryGetValue(token, out string url))
                {
                    context.Response.Redirect(url);
                    r = $"定向\t[{token}]=>{url}";
                    worker.ReportProgress(0, r);
                }
                else
                {
                    r = $"错误\t[{token}]=>没有找到对应的映射！";
                    worker.ReportProgress(0, r);
                    r = "没有找到对应的映射！";
                }
            }
            var d = context.Request.ContentEncoding.GetBytes(r);
            context.Response.OutputStream.Write(d, 0, d.Length);
            context.Response.OutputStream.Close();
        }

        static string HandleRequest(HttpListenerRequest request)
        {
            var byteList = new List<byte>();
            var byteArr = new byte[2048];
            int readLen = 0;
            int len = 0;
            //接收客户端传过来的数据并转成字符串类型
            do
            {
                readLen = request.InputStream.Read(byteArr, 0, byteArr.Length);
                len += readLen;
                byteList.AddRange(byteArr);
            } while (readLen != 0);
            return request.ContentEncoding.GetString(byteList.ToArray(), 0, len);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (worker.IsBusy)
            {
                worker.CancelAsync();
            }
            else
            {
                worker.RunWorkerAsync();
            }
        }


    }
}
