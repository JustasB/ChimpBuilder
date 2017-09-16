using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace ChimpBuilderTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            var chimpHTML = new HtmlDocument();
            chimpHTML.LoadHtml(txtChimp.Text);

            //Remove top mailchimp comment
            chimpHTML
                .DocumentNode
                .ChildNodes
                .First(n => n.Name == "#comment" && n.InnerHtml.Contains("saved from"))
                .Remove();

            //Remove mc logo - the parent a tag of img with src containing "Monkey"
            chimpHTML
                .DocumentNode
                .Descendants("img")
                .First(img => img.Attributes["src"].Value.Contains("Monkey"))
                .ParentNode
                .Remove();

            //remove foreign font css reference
            chimpHTML
                .DocumentNode
                .Descendants("link")
                .First()
                .Remove();

            //Remove update preferences and replace unsubscribe url
            var preferencesTag = chimpHTML
                .DocumentNode
                .Descendants("a")
                .First(a => a.Attributes["href"].Value.Contains("profile?u"));

            //Unsub link
            preferencesTag.NextSibling.NextSibling.Attributes["href"].Value = "{{ settings.site.unsubscribe_url }}";
            
            //Remove prefs and "or"
            preferencesTag.NextSibling.Remove(); //remove "or" word after link
            preferencesTag.Remove();

            //Remove add to address book
            chimpHTML
                .DocumentNode
                .Descendants("a")
                .First(a => a.Attributes["class"] != null && a.Attributes["class"].Value == "hcard-download")
                .Remove();

            var themeFilesHTML = new HtmlDocument();
            themeFilesHTML.LoadHtml(txtThemeFiles.Text);

            var templateBody = chimpHTML.DocumentNode;

            var chimpImages = templateBody
                .Descendants("img")
                .Select(img => new {
                    element = img,
                    file = new FileInfo(img.Attributes["src"].Value.Replace("https://", "")).Name
                })
                .ToList();

            var themeImages = themeFilesHTML
                .DocumentNode
                .Descendants("a")
                .Select(a => new {
                    element = a,
                    href = a.Attributes["href"]?.Value })
                .ToList();

            chimpImages
                .ForEach(imgChimp =>
                {
                    imgChimp.element.Attributes["src"].Value = themeImages
                        .First(imgNB => imgNB.href != null && imgNB.href.Contains(imgChimp.file))
                        .href;
                });

            

            //Replace name
            var NBhtml = templateBody
                .InnerHtml
                .Replace("&lt;&lt;NAME&gt;&gt;", "{{ recipient.first_name_or_friend }}") //Replace name
                ;

            //Add NB body tag
            txtNBhtml.Text = NBhtml + "{{ body }}";
        }
    }
}
