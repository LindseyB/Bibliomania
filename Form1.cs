using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using OAuth;


namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public List<Book> readBooks = new List<Book>();

        public Form1()
        {
            InitializeComponent();

        }

        public void loadBooks()
        {
            String settingsBooksString = Properties.Settings.Default.Books;
            String[] booksFromSettings = settingsBooksString.Split('\n');
            String[] curBook;
            String[] origTitles = new String[booksFromSettings.Length];

            if (!settingsBooksString.Equals(""))
            {
                int i = 0;
                // read in books from application settings
                foreach (String s in booksFromSettings)
                {
                    if (s != "")
                    {
                        curBook = s.Split('~');
                        readBooks.Add(new Book(curBook[0], curBook[1], curBook[2], Convert.ToInt32(curBook[3]), Convert.ToInt32(curBook[4])));
                        origTitles[i] = curBook[0];  // storing the titles for look up later
                        i++;
                    }
                }
            }

            Console.WriteLine("Settings: " + settingsBooksString);

            // open up the readers XML data about the users read books
            XmlTextReader textReader = new XmlTextReader("tests\\media.xml");

            try
            {
                textReader.Read();
            }
            catch (Exception e)
            {
                MessageBox.Show("No Valid Ebook Reader found.");
                Console.WriteLine(e.ToString());
            }

            while (textReader.Read())
            {
                textReader.MoveToElement();

                if (textReader.LocalName.Equals("text"))
                {
                    textReader.MoveToAttribute("author");
                    String author = textReader.Value;

                    textReader.MoveToAttribute("page");
                    int curPage = 0;
                    Int32.TryParse(textReader.Value, out curPage);

                    textReader.MoveToAttribute("title");
                    String title = textReader.Value;

                    if (curPage > 0 && !origTitles.Contains(title))
                    {
                        readBooks.Add(new Book(title, author, curPage));
                    }
                    else if (curPage > 0)
                    {
                        // update page number
                        readBooks.ElementAt(Array.IndexOf(origTitles, title)).CurrentPage = curPage;
                    }
                    else
                    {
                        // do nothing
                    }

                }
            }

            String booksString = "";

            foreach (Book bookItem in readBooks)
            {
                bookItem.setId();
                if (bookItem.GoodreadsId != 0)
                {
                    bookList.Items.Add(bookItem.Title + " on page " + bookItem.CurrentPage);
                    bookList.SetItemChecked(bookList.Items.Count - 1, true);
                }

                booksString += bookItem.ToString();

            }

            Properties.Settings.Default.Books = booksString;
            Properties.Settings.Default.Save();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            loadBooks();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            getOAuth();
        }

        public void getOAuth()
        {
            string consumerKey = "eStfu8ZZXxins2tOdlVjUA";
            string consumerSecret = "A773YHERJAWODRasHREh9GOjFTEuGdGQvu1cfbXFfM";

            Uri uri = new Uri("http://goodreads.com/oauth/request_token");
            OAuthBase oAuth = new OAuthBase();
            string nonce = oAuth.GenerateNonce();
            string timeStamp = oAuth.GenerateTimeStamp();
            string normalizedUrl;
            string normalizedRequestParameters;
            string sig = oAuth.GenerateSignature(uri, consumerKey, consumerSecret, null, null, "GET", timeStamp, nonce, out normalizedUrl, out normalizedRequestParameters);

            sig = HttpUtility.UrlEncode(sig);

            StringBuilder sb = new StringBuilder(uri.ToString());
            sb.AppendFormat("?oauth_consumer_key={0}&", consumerKey);
            /*sb.AppendFormat("oauth_nonce={0}&", nonce);
            sb.AppendFormat("oauth_timestamp={0}&", timeStamp);
            sb.AppendFormat("oauth_signature_method={0}&", "HMAC-SHA1");
            sb.AppendFormat("oauth_version={0}&", "1.0");*/
            sb.AppendFormat("oauth_signature={0}", sig);

            System.Diagnostics.Debug.WriteLine(sb.ToString()); 
        }

        private void label1_Click(object sender, EventArgs e)
        {
            getOAuth();
        }
    }
}
