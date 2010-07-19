using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using System.Net;
using System.Xml;
using OAuth;

namespace WindowsFormsApplication1 {
	// Contains the methods used for accessing the goodreads API 
	class GoodreadsAPI {

		// submits a status update for a book to goodreads
		public static void submitBookStatus(Book book) {
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

		// logs the user in and accesses their userId and userName
		public static void loginUser(out string userId, out string userName) {
			OAuth oAuth = new OAuth();
			oAuth.getOAuthToken();
			string userXML = oAuth.getOAuthDataUrl("http://www.goodreads.com/api/auth_user");
			userId = "";
			userName = "";

			// grab the user name and user id
			XmlTextReader textReader = new XmlTextReader(userXML);

			while (textReader.Read()) {
				textReader.MoveToElement();

				if (textReader.LocalName.Equals("user")) {
					textReader.MoveToAttribute("id");
					userId = textReader.Value;
				}

				if (textReader.LocalName.Equals("name")) {
					userName = textReader.ReadElementContentAsString();
				}

			}
		}

		// adds a book to the currently reading shelf (statuses cannot be added for books not on this shelf)
		public static void addBookToShelf(Book book) {
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

	}
}
