using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;

namespace SchoolPlayer
{
    public partial class Form1 : Form
    {
        WebBrowser wb = new WebBrowser();
        string set = "";

        public Form1()
        {
            InitializeComponent();

            

            set = System.Environment.CurrentDirectory + "\\set.ini";
            /*
            if (!File.Exists(set)) {
                File.Create(set);
            }
            */
            textBox_path.Text = ContentValue("Set", "path");
        }


        /// <summary>
        /// 写入INI文件
        /// </summary>
        /// <param name="section">节点名称[如[TypeName]]</param>
        /// <param name="key">键</param>
        /// <param name="val">值</param>
        /// <param name="filepath">文件路径</param>
        /// <returns></returns>
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filepath);
        /// <summary>
        /// 读取INI文件
        /// </summary>
        /// <param name="section">节点名称</param>
        /// <param name="key">键</param>
        /// <param name="def">值</param>
        /// <param name="retval">stringbulider对象</param>
        /// <param name="size">字节大小</param>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retval, int size, string filePath);

        private string strFilePath = Application.StartupPath + "\\FileConfig.ini";//获取INI文件路径


        /// <summary>
        /// 自定义读取INI文件中的内容方法
        /// </summary>
        /// <param name="Section">键</param>
        /// <param name="key">值</param>
        /// <returns></returns>
        private string ContentValue(string Section, string key)
        {

            StringBuilder temp = new StringBuilder(1024);
            GetPrivateProfileString(Section, key, "", temp, 1024, set);
            return temp.ToString();
        }

        /// <summary>
        /// 下载页面，获取url进行下载
        /// </summary>
        /// <param name="movie_url"></param>
        private void DownloadPage(string movie_url) {
            try
            {
                wb.DocumentCompleted += wb_DocumentCompleted;
                wb.Url = new Uri(movie_url);
            }
            catch (Exception e) {
                MessageBox.Show("网址有问题");
            }
        }

        private void wb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {           
            foreach (HtmlElement he in wb.Document.GetElementsByTagName("param"))
            {
                if (he.GetAttribute("name") == "url")
                {
                    wb.Dispose();
                    DownloadMovie(he.GetAttribute("value"));
                }
            }
        }

        /// <summary>
        /// 下载电影
        /// </summary>
        /// <param name="StringUrl">电影下载地址</param>
        private void DownloadMovie(string StringUrl) {

            string path = textBox_path.Text;

            // 获取电影的文件名称
            String MovieName = StringUrl.Split('/')[StringUrl.Split('/').Length - 1];
            // 获取Host
            String host = StringUrl.Split('/')[2];

            Uri url = new Uri(StringUrl + "?agent=powerpxp&session=-1392595589");

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;

            request.Method = "GET";

            request.UserAgent = "Novasoft NetPlayer/4.0";

            request.Host = host;


            FileStream f = null;
            try
            {
                f = new FileStream(path + MovieName, FileMode.Create);
            }
            catch (Exception e)
            {
                MessageBox.Show("路径有问题。");
            }

            using (HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse())
            {

                long dataLengthHaveRead  = 0;
                long dataLength  = webResponse.ContentLength;

                //MessageBox.Show(webResponse.StatusCode.ToString());

                if (webResponse.StatusCode.ToString().Equals("OK"))
                {
                    WritePrivateProfileString("Set", "path", folderBrowserDialog.SelectedPath, set);
                }else{
                    MessageBox.Show("获取不了的说~~");
                }

                using (Stream stream = webResponse.GetResponseStream())
                {

                    const long ChunkSize = 102400; //100K 每次读取文件，只读取100K，这样可以缓解服务器的压力
                    byte[] buffer = new byte[ChunkSize];

                    while (dataLength - dataLengthHaveRead > 0)
                    {
                        Application.DoEvents();
                        int lengthRead = stream.Read(buffer, 0, Convert.ToInt32(ChunkSize));
                        dataLengthHaveRead += lengthRead;
                        double pro = Math.Round(Convert.ToDouble(dataLengthHaveRead) / dataLength, 3) * 100;
                        progressBar_download.Value = Convert.ToInt32(pro);
                        label_download_info.Text = pro + "%";
                        //MessageBox.Show(Convert.ToDouble(dataLengthHaveRead) / dataLength * 100 + " " + dataLength, "" + lengthRead);
                        f.Write(buffer, 0, lengthRead);
                        f.Flush();
                    }
                    f.Close();
                    MessageBox.Show("下载完成~！ > <");
                    wb = new WebBrowser();
                    progressBar_download.Value = 0;
                }
            }

        }

        private void button_download_Click(object sender, EventArgs e)
        {
            DownloadPage(textBox_url.Text);
        }

        private void button_folder_Click(object sender, EventArgs e)
        {

            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                string path = folderBrowserDialog.SelectedPath;
                textBox_path.Text = path.EndsWith("\\") ? path : path+"\\";
            }
        }





    }
}
