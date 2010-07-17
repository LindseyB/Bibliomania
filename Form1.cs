﻿using System;
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
using System.Net;
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
            getOAuthToken();
        }

        public void getOAuthToken()
        {
            string consumerKey = "eStfu8ZZXxins2tOdlVjUA";
            string consumerSecret = "A773YHERJAWODRasHREh9GOjFTEuGdGQvu1cfbXFfM";  // shhh! It's a secret

            // please note that the url needs to be EXACTLY the url it needs to / will hit
            // this means if the page forwards foo.bar to www.foo.bar the uri needs to be specified as www.foo.bar
            Uri uri = new Uri("http://www.goodreads.com/oauth/request_token");
            OAuthBase oAuth = new OAuthBase();
            
            string nonce = oAuth.GenerateNonce();
            string timeStamp = oAuth.GenerateTimeStamp();
            string normalizedUrl;
            string normalizedRequestParameters;
            string sig = oAuth.GenerateSignature(uri, consumerKey, consumerSecret, null, null, "GET", timeStamp, nonce, out normalizedUrl, out normalizedRequestParameters);

            sig = HttpUtility.UrlEncode(sig);

            string request_url = normalizedUrl + "?" + normalizedRequestParameters + "&oauth_signature=" + sig;
            string oauthToken;
            string oauthTokenSecret;

            oauthTokenReq(request_url, out oauthToken, out oauthTokenSecret);

            Console.WriteLine("token: " + oauthToken + " secret: " + oauthTokenSecret);
            
            // go get authorized
            uri = new Uri("http://www.goodreads.com/oauth/authorize");
            string oauthURL = uri.ToString() + "?oauth_token=" + oauthToken;
            // open a browser and allow the user to authorize 
            System.Diagnostics.Process.Start(oauthURL);

            // Instead of sleeping prompt the user to verify that they entered their credentials before proceeding to the next step
            if (MessageBox.Show("Did you allow Bibliomania access?", "Confirm Access", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                uri = new Uri("http://www.goodreads.com/oauth/access_token");
                nonce = oAuth.GenerateNonce();
                timeStamp = oAuth.GenerateTimeStamp();
                // this time we need our oauth token and oauth token secret
                sig = oAuth.GenerateSignature(uri, consumerKey, consumerSecret, oauthToken, oauthTokenSecret, "GET", timeStamp, nonce, out normalizedUrl, out normalizedRequestParameters);
                sig = HttpUtility.UrlEncode(sig);

                // notice that the sig is always being appended to the end
                string accessUrl = normalizedUrl + "?" + normalizedRequestParameters + "&oauth_signature=" + sig;

                oauthTokenReq(accessUrl, out oauthToken, out oauthTokenSecret);
                Console.WriteLine("ouathToken: " + oauthToken + "oauthTokenSecret:" + oauthTokenSecret);
            }
        }

        private void oauthTokenReq(string url, out string oauthTok, out string oauthTokSecret)
        {
            Console.WriteLine(url);

            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(url);
            Stream responseStream;
            
            try
            {
                responseStream = httpRequest.GetResponse().GetResponseStream();
            }
            catch (WebException e)
            {
                Console.WriteLine(e.Message);
                oauthTok = "";
                oauthTokSecret = "";
                return;
            }

            byte[] buf = new byte[1024];
            int count = 0;
            StringBuilder sb = new StringBuilder("");
            string tempString;

            do
            {
                // fill the buffer with data
                count = responseStream.Read(buf, 0, buf.Length);

                // make sure we read some data
                if (count != 0)
                {
                    // translate from bytes to ASCII text
                    tempString = Encoding.ASCII.GetString(buf, 0, count);

                    // continue building the string
                    sb.Append(tempString);
                }
            }
            while (count > 0);

            oauthTok = sb.ToString().Substring(12, sb.ToString().IndexOf('&') - 12);
            oauthTokSecret = sb.ToString().Substring(sb.ToString().IndexOf('&') + 20);
        }

        private void label1_Click(object sender, EventArgs e)
        {
            getOAuthToken();
        }
    }
}
