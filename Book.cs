using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Security;

namespace WindowsFormsApplication1
{
    public class Book
    {
        private String _title;
        private String _author;
        private int _currentPage;
        private int _goodreadsId;

        public Book(String title, String author, int currentPage)
        {
            _title = title;
            _author = author;
            _currentPage = currentPage;
        }

        /* Getters and Setters */ 
        public String Title
        {
            get { return _title; }
            set { _title = value; }
        }

        public String Author
        {
            get { return _author; }
            set { _author = value; }
        }

        public int CurrentPage
        {
            get { return _currentPage; }
            set { _currentPage = value; }
        }

        public int GoodreadsId
        {
            get { return _goodreadsId; }
            set { _goodreadsId = value; }
        }

        public void setId()
        {
            // generate the url for the api call
            String url = "http://www.goodreads.com/api/book_url/" + SecurityElement.Escape(_title.Replace(" ", "%2B")) + "%2B" + SecurityElement.Escape(_author.Replace(" ", "%2B")) + "?key=eStfu8ZZXxins2tOdlVjUA";
            XmlTextReader textReader = new XmlTextReader(url);
            textReader.Read();

            while (textReader.Read())
            {
                textReader.MoveToElement();

                if (textReader.LocalName.Equals("book"))
                {
                    textReader.MoveToAttribute("id");
                    int id = 0;
                    Int32.TryParse(textReader.Value, out id);

                    if (id != 0)
                    {
                        // set the id
                        _goodreadsId = id;
                    }
                }
                if (textReader.LocalName.Equals("title"))
                {
                    // set the title based on the goodreads result
                    _title = textReader.ReadElementContentAsString();
                }
            }
        }
    }
}
