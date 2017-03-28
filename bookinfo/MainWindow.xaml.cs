using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Net;

namespace bookinfo
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public class BookInfo
    {
        public string Auther { get; set; }
        public string Title { get; set; }
        public string TitleTrans { get; set; }
        public string ISBN { get; set; }
        public string seriesTitle { get; set; }
        public string Publisher { get; set; }
        public string Volume { get; set; }
    }
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            textBox.Text = null;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if(textBox.Text == null)
            {
                MessageBox.Show("ISBNが入力されていません");
                this.Close();
            }
            string baseurl = "http://iss.ndl.go.jp/api/opensearch?isbn=";
            string isbn = textBox.Text;
            string url = baseurl + isbn;
            XNamespace p = @"http://pinga.mu/terms/";
            XNamespace media = @"http://search.yahoo.com/mrss/";
            XNamespace dcndl = @"http://ndl.go.jp/dcndl/terms/";
            XNamespace dc = @"http://purl.org/dc/elements/1.1/";
            XNamespace opensearch = @"http://a9.com/-/spec/opensearchrss/1.0/";
            XElement booksearch = XElement.Load(url);
            XElement booksearchChannel = booksearch.Element("channel");
            XElement booksearchElement = booksearchChannel.Element("item");
            //label.Content = booksearchElement.Element(dc + "creator").Value;
            BookInfo bookinfo = new BookInfo();
            bookinfo.Auther = booksearchElement.Element(dc + "creator").Value;
            bookinfo.Title = booksearchElement.Element(dc + "title").Value;
            bookinfo.TitleTrans = booksearchElement.Element(dcndl + "titleTranscription").Value;
            bookinfo.seriesTitle = booksearchElement.Element(dcndl + "seriesTitle").Value;
            bookinfo.ISBN = isbn;
            bookinfo.Publisher = booksearchElement.Element(dc + "publisher").Value;
            if(booksearchElement.Element(dc + "volume") != null)
            {
                bookinfo.Volume = booksearchElement.Element(dc + "volume").Value;
            }
            else
            {
                bookinfo.Volume = "0";
            }
            using (var ms = new MemoryStream())
            using (var sr = new StreamReader(ms))
            {
                var serializer = new DataContractJsonSerializer(typeof(BookInfo));
                serializer.WriteObject(ms, bookinfo);
                ms.Position = 0;
                var json = sr.ReadToEnd();
                label.Content = json;
                //Json put
                string put_url = "http://localhost:5984/bookinfo/" + isbn;
                WebRequest req = WebRequest.Create(put_url);
                req.Method = "PUT";
                byte[] byteArray = Encoding.UTF8.GetBytes(json);
                req.ContentType = "text/json";
                req.ContentLength = byteArray.Length;
                Stream dataStream = req.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                //Retrun
                WebResponse res = req.GetResponse();
                MessageBox.Show(((HttpWebResponse)res).StatusDescription);
                dataStream = res.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string resFromServer = reader.ReadToEnd();
                MessageBox.Show(resFromServer);
                reader.Close();
                dataStream.Close();
                res.Close();
            }


        }
    }
}
