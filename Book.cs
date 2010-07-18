using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Security;

namespace WindowsFormsApplication1 {
	[Serializable()]
	public class Book {
		private String _title;
		private String _origTitle;  // used to ID books because the title will change
		private String _author;
		private int _currentPage;
		private int _goodreadsId;

		public Book(String title, String author, int currentPage) {
			_origTitle = title;
			_title = title;
			_author = author;
			_currentPage = currentPage;
			_goodreadsId = -1;          // this value will tell me if I read it from the settings or not
		}

		public Book(String origTitle, String title, String author, int currentPage, int goodreadsId) {
			_origTitle = origTitle;
			_title = title;
			_author = author;
			_currentPage = currentPage;
			_goodreadsId = goodreadsId;
		}

		/* Getters and Setters */
		public String OrigTitle {
			get { return _origTitle; }
		}

		public String Title {
			get { return _title; }
			set { _title = value; }
		}

		public String Author {
			get { return _author; }
			set { _author = value; }
		}

		public int CurrentPage {
			get { return _currentPage; }
			set { _currentPage = value; }
		}

		public int GoodreadsId {
			get { return _goodreadsId; }
			set { _goodreadsId = value; }
		}

		public override String ToString() {
			// had to make the divider something that probably wouldn't be in the title
			return _origTitle + "~" + _title + "~" + _author + "~" + _currentPage + "~" + _goodreadsId + "\n";
		}

		public void setId() {
			// we already have it - go away
			if (_goodreadsId != -1 && _goodreadsId != 0) {
				return;
			}

			Console.WriteLine("Let's ask the internet");

			// generate the url for the api call
			String url = "http://www.goodreads.com/api/book_url/" + SecurityElement.Escape(_origTitle.Replace(" ", "%2B")) + "%2B" + SecurityElement.Escape(_author.Replace(" ", "%2B")) + "?key=eStfu8ZZXxins2tOdlVjUA";
			XmlTextReader textReader = new XmlTextReader(url);
			textReader.Read();

			while (textReader.Read()) {
				textReader.MoveToElement();

				if (textReader.LocalName.Equals("book")) {
					textReader.MoveToAttribute("id");
					int id = 0;
					Int32.TryParse(textReader.Value, out id);

					if (id != 0) {
						// set the id
						_goodreadsId = id;
					}
				}
				if (textReader.LocalName.Equals("title")) {
					// set the title based on the goodreads result
					_title = textReader.ReadElementContentAsString();
				}
			}
		}
	}
}
