using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
