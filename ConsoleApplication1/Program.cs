using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Web;
using HtmlAgilityPack;
using System.IO;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            
            
            //GetBlogList();
            //var nodes = node.DescendantsAndSelf().Where( ).ToList();
        }

        static void GetBlogList()
        {
            string folder = AppDomain.CurrentDomain.BaseDirectory;
            WebClient client = new WebClient();
            client.Encoding = Encoding.UTF8;
            string str = client.DownloadString("http://lonlyline.blogbus.com/index_1.html");

            HtmlDocument hdoc = new HtmlDocument();
            hdoc.LoadHtml(str);

            HtmlNode node = hdoc.GetElementbyId("posts");

            var nodes = node.Descendants().Where((n) =>
            {
                var attr = n.Attributes.Where(at => at.Name == "class").FirstOrDefault();
                if (attr != null)
                {
                    if (attr.Value == "postHeader")
                        return true;
                }
                return false;
            }).ToList();

            List<Blog> blogs = new List<Blog>();
            if (nodes != null)
            {

                blogs = nodes.Select(n => {
                    Blog bl = new Blog();
                    bl.Date = n.Element("h3").InnerText;
                    bl.Title = n.Element("h2").Element("a").InnerText;
                    bl.Tag = n.Element("h2").Element("span").Element("a").InnerText;
                    bl.DetailLink = n.Element("h2").Element("a").Attributes["href"].Value;
                    bl.FileName = bl.DetailLink.Split('/').Last();
                    
                    return bl;
                }).ToList();
            }

            blogs.ForEach((bl) => {
                string blog = client.DownloadString(bl.DetailLink);
                GetBlogDetail(hdoc, blog, folder + bl.FileName);
            });
        }

        static void GetBlogDetail(HtmlDocument hdoc,string Blog,string fileName)
        {

            hdoc.LoadHtml(Blog);
            HtmlNode node = hdoc.GetElementbyId("posts");
            StreamWriter sw = new StreamWriter(new FileStream(fileName, FileMode.Create),Encoding.UTF8);
            
            sw.Write(node.OuterHtml);
            sw.Close();
        }
    }

    public class Blog
    {
        public string Date { get; set; }

        public string Title { get; set; }

        public string Category { get; set; }
        public string Tag { get; set; }

        public string DetailLink { get; set; }

        public string FileName { get; set; }
    }
}
