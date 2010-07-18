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
using System.Net;



namespace WindowsFormsApplication1 {
	public partial class Form1 : Form {
		public List<Book> readBooks = new List<Book>();
		public string userId = "";

		public Form1() {
			InitializeComponent();
		}

		public void loadBooks() {
			String settingsBooksString = Properties.Settings.Default.Books;
			String[] booksFromSettings = settingsBooksString.Split('\n');
			String[] curBook;
			String[] origTitles = new String[booksFromSettings.Length];

			if (!settingsBooksString.Equals("")) {
				int i = 0;
				// read in books from application settings
				foreach (String s in booksFromSettings) {
					if (s != "") {
						curBook = s.Split('~');
						readBooks.Add(new Book(curBook[0], curBook[1], curBook[2], Convert.ToInt32(curBook[3]), Convert.ToInt32(curBook[4])));
						origTitles[i] = curBook[0];  // storing the titles for look up later
						i++;
					}
				}
			}

			// open up the readers XML data about the users read books
			XmlTextReader textReader = new XmlTextReader("J:\\database\\cache\\media.xml");

			try {
				textReader.Read();
			} catch (Exception e) {
				MessageBox.Show("No Valid Ebook Reader found.");
				Console.WriteLine(e.ToString());
			}

			while (textReader.Read()) {
				textReader.MoveToElement();

				if (textReader.LocalName.Equals("text")) {
					textReader.MoveToAttribute("author");
					String author = textReader.Value;

					textReader.MoveToAttribute("page");
					int curPage = 0;
					Int32.TryParse(textReader.Value, out curPage);

					textReader.MoveToAttribute("title");
					String title = textReader.Value;

					if (curPage > 0 && !origTitles.Contains(title)) {
						readBooks.Add(new Book(title, author, curPage));
					} else if (curPage > 0) {
						// update page number
						readBooks.ElementAt(Array.IndexOf(origTitles, title)).CurrentPage = curPage;
					} else {
						// do nothing
					}

				}
			}

			String booksString = "";

			foreach (Book bookItem in readBooks) {
				bookItem.setId();
				if (bookItem.GoodreadsId != 0) {
					bookList.Items.Add(bookItem.Title + " on page " + bookItem.CurrentPage);
					bookList.SetItemChecked(bookList.Items.Count - 1, true);
				}

				booksString += bookItem.ToString();

			}

			Properties.Settings.Default.Books = booksString;
			Properties.Settings.Default.Save();
		}

		private void Form1_Load(object sender, EventArgs e) {

		}

		private void Form1_Shown(object sender, EventArgs e) {
			loadBooks();
		}

		private void pictureBox1_Click(object sender, EventArgs e) {
			submitStatuses();
		}



		private void label1_Click(object sender, EventArgs e) {
			loginUser();
		}

		private void pictureBox2_Click(object sender, EventArgs e) {
			loginUser();
		}

		private void submitStatuses() {
			if (userId.Equals("")) {
				loginUser();
			}

			for(int i = 0; i < bookList.Items.Count; i++) {
				Book curBook = readBooks[i];

				if (bookList.GetItemChecked(i)) {
					// add the book to the currently reading shelf 
					addBookToShelf(curBook);
					// submit this status
					submitBookStatus(curBook);
				}
			}
		}

		private void addBookToShelf(Book book) {
			OAuth oAuth = new OAuth();

			string shelfURL = oAuth.getOAuthDataUrl("http://www.goodreads.com/shelf/add_to_shelf.xml");
			HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(shelfURL);
			httpRequest.Method = "POST";
			httpRequest.ContentType = "application/x-www-form-urlencoded";
			string postData = "book_id=" + book.GoodreadsId + "&name=currently-reading";
			httpRequest.ContentLength = postData.Length;
			StreamWriter stOut = new StreamWriter(httpRequest.GetRequestStream(), System.Text.Encoding.ASCII);
			stOut.Write(postData);
			stOut.Close();
			StreamReader stIn = new StreamReader(httpRequest.GetResponse().GetResponseStream());
			string strResponse = stIn.ReadToEnd();
			stIn.Close();

			Console.WriteLine(strResponse);
		}

		private void submitBookStatus(Book book) {
			OAuth oAuth = new OAuth();

			string statusURL = oAuth.getOAuthDataUrl("http://www.goodreads.com/user_status.xml");
			HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(statusURL);
			httpRequest.Method = "POST";
			httpRequest.ContentType = "application/x-www-form-urlencoded";
			string postData = "user_status[book_id]=" + book.GoodreadsId + "&user_status[page]=" + book.CurrentPage + "&user_status[body]=Added%20By%20Bibliomania";
			httpRequest.ContentLength = postData.Length;
			StreamWriter stOut = new StreamWriter(httpRequest.GetRequestStream(), System.Text.Encoding.ASCII);
			stOut.Write(postData);
			stOut.Close();
			StreamReader stIn = new StreamReader(httpRequest.GetResponse().GetResponseStream());
			string strResponse = stIn.ReadToEnd();
			stIn.Close();

			Console.WriteLine(strResponse);
		}

		private void loginUser() {
			OAuth oAuth = new OAuth();
			oAuth.getOAuthToken();
			string userXML = oAuth.getOAuthDataUrl("http://www.goodreads.com/api/auth_user");

			// grab the user name and user id
			XmlTextReader textReader = new XmlTextReader(userXML);

			while (textReader.Read()) {
				textReader.MoveToElement();

				if (textReader.LocalName.Equals("user")) {
					textReader.MoveToAttribute("id");
					userId = textReader.Value;
				}

				if (textReader.LocalName.Equals("name")) {
					label1.Text = "hi, " + textReader.ReadElementContentAsString();
				}

			}
		}


	}
}
