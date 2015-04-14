using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.ObjectModel;
using System.Threading;
using Winform = System.Windows.Forms;
using System.Xml.Serialization;

namespace GetBusBlog
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _baseURL;
        private WebClient _client;
        private ObservableCollection<Blog> _blogs;
        private string _folder;
        private string _fileName;
        private FileStream _fs;
        private CancellationTokenSource tokenSource;
        private CancellationToken _token;
        public MainWindow()
        {
            InitializeComponent();
            _blogs = new ObservableCollection<Blog>();
            _client = new WebClient();
            _client.Encoding = Encoding.UTF8;
            lb_Date.DataContext = _blogs;
            
        }

        private async void btn_Analysis_Click(object sender, RoutedEventArgs e)
        {
            Winform.SaveFileDialog sfd = new Winform.SaveFileDialog();
            sfd.InitialDirectory = _folder;
            sfd.Filter = "XML Files | *.xml";

            if(sfd.ShowDialog()== Winform.DialogResult.OK)
            {
                _fileName = sfd.FileName;
                FileInfo fi = new FileInfo(_fileName);
                _folder = fi.DirectoryName;
            }
            try
            {
                tokenSource = new CancellationTokenSource();
                _token = tokenSource.Token;
                if(_fs!=null)
                {
                    _fs.Close();
                }
                _blogs.Clear();
                _baseURL = tb_URL.Text;
                btn_Analysis.IsEnabled = false;
                btn_Open.IsEnabled = false;
                int pageCount = await getPageCount();
                pb.Maximum = pageCount;
                for (int i = 1; i <= pageCount; i++)
                {
                    if(_token.IsCancellationRequested)
                    {
                        _token.ThrowIfCancellationRequested();
                    }
                    await getBlogList(i);
                    pb.Value = i;
                }
                await saveBlogs(_fileName);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                btn_Analysis.IsEnabled = true;
                btn_Open.IsEnabled = true;
            }
            
            
        }

        private Task getBlogList(int index)
        {
            return Task.Run(() =>
            {
                try
                {


                    string url = _baseURL + "/index_" + index + ".html";
                    string str = _client.DownloadString(url);
                    HtmlDocument hdoc = new HtmlDocument();
                    hdoc.LoadHtml(str);

                    HtmlNode node = hdoc.GetElementbyId("posts");
                    HtmlDocument detail = new HtmlDocument();
                    //List<Blog> blogs = new List<Blog>();

                    foreach (var element in node.Descendants("li"))
                    {

                        Blog bl = new Blog();
                        foreach (var element1 in element.Descendants("div"))
                        {
                            if (element1.Attributes["class"] != null && element1.Attributes["class"].Value.ToLower() == "postheader")
                            {
                                getBlogHeader(element1, bl);
                            }
                            if (element1.Attributes["class"] != null && element1.Attributes["class"].Value.ToLower() == "postfooter")
                            {
                                getBlogFooter(element1, bl);
                            }
                        }

                        string blogDetail = _client.DownloadString(bl.DetailLink);
                        getBlogDetail(detail, blogDetail, bl.FileName);
                        lb_Date.Dispatcher.BeginInvoke(new Action(() => { _blogs.Add(bl); }));
                        
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            });
            

        }


        private void getBlogHeader(HtmlNode node,Blog blog)
        {
            blog.Date = node.Element("h3").InnerText;
            blog.Title= node.Element("h2").Element("a").InnerText;
            blog.DetailLink = node.Element("h2").Element("a").Attributes["href"].Value;
            blog.FileName = _folder + @"\" + blog.DetailLink.Split('/').Last();
            blog.Category = node.Element("h2").Element("span") == null ? "" : node.Element("h2").Element("span").Element("a").InnerText;
        }

        private void getBlogFooter(HtmlNode node,Blog blog)
        {
            foreach(var n in node.Descendants("div"))
            {
                if (n.Attributes["class"] != null && n.Attributes["class"].Value.ToLower() == "tags")
                {
                    blog.Tag = n.Element("a") == null ? "" : n.Element("a").InnerText;
                }
                if (n.Attributes["class"] != null && n.Attributes["class"].Value.ToLower() == "menubar")
                {
                    foreach(var n1 in n.Descendants("span"))
                    {
                        if(n1.Attributes["class"]!=null && n1.Attributes["class"].Value.ToLower() == "author")
                        {
                            blog.Author = n1.Element("a").InnerText;
                        }
                        if(n1.Attributes["class"]!=null && n1.Attributes["class"].Value.ToLower() == "time")
                        {
                            blog.Time = n1.InnerText;
                        }
                    }
                }
            }
            
        }

        private void getBlogDetail(HtmlDocument hdoc, string Blog, string fileName)
        {

            hdoc.LoadHtml(Blog);
            HtmlNode node = hdoc.GetElementbyId("posts");
            StreamWriter sw = new StreamWriter(new FileStream(fileName, FileMode.Create), Encoding.UTF8);

            sw.Write(node.OuterHtml);
            sw.Close();
        }

        private Task<int> getPageCount()
        {
            return Task.Run<int>(() => {
                string str = _client.DownloadString(_baseURL);
                HtmlDocument hdoc = new HtmlDocument();
                hdoc.LoadHtml(str);
                HtmlNode node = hdoc.GetElementbyId("content");
                var navNode = node.Descendants().Where((n) =>
                {
                    var attr = n.Attributes.Where(at => at.Name == "class").FirstOrDefault();
                    if (attr != null)
                    {
                        if (attr.Value == "pageNavi")
                            return true;
                    }
                    return false;
                }).FirstOrDefault();
                if (navNode != null)
                {
                    return int.Parse(Regex.Replace(navNode.FirstChild.InnerText, @"[^\d.]*", ""));

                }
                return 0;
            });
            
        }

        private void lb_Date_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Blog blog = ((ListBox)sender).SelectedItem as Blog;
                if (blog != null)
                {
                    if (_fs != null)
                        _fs.Close();
                    _fs = new FileStream(blog.FileName, FileMode.Open);

                    //StreamReader reader = new StreamReader(new FileStream(blog.FileName, FileMode.Open),Encoding.Default);
                    //string blogStr = reader.ReadToEnd();
                    //reader.Close();

                    wb_Content.NavigateToStream(_fs);

                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            if(tokenSource!=null)
                tokenSource.Cancel();
            btn_Analysis.IsEnabled = true;
            btn_Open.IsEnabled = true;
        }

        private Task saveBlogs(string fileName)
        {
            return Task.Run(() => { 
                if(_blogs != null)
                {
                    try
                    {
                        XmlSerializer xs = new XmlSerializer(typeof(ObservableCollection<Blog>));
                        xs.Serialize(new FileStream(fileName, FileMode.Create), _blogs);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            });
        }

        private Task openBlogs(string fileName)
        {
            return Task.Run(() => {
                try
                {
                    XmlSerializer xs = new XmlSerializer(typeof(ObservableCollection<Blog>));
                    _blogs = xs.Deserialize(new FileStream(fileName, FileMode.Open)) as ObservableCollection<Blog>;
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        private async void btn_Open_Click(object sender, RoutedEventArgs e)
        {
            Winform.OpenFileDialog ofd = new Winform.OpenFileDialog();
            ofd.Filter = "XML Files | *.xml";
            if(ofd.ShowDialog() == Winform.DialogResult.OK)
            {
                _fileName = ofd.FileName;
                await openBlogs(_fileName);
                if (_blogs == null)
                    _blogs = new ObservableCollection<Blog>();
                lb_Date.DataContext = _blogs;
            }
        }
    }
}
