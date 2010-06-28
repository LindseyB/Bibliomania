using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            List<Book> readBooks = new List<Book>();           

            XmlTextReader textReader = new XmlTextReader("J:\\database\\cache\\media.xml");
            textReader.Read();

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

                    if (curPage > 0)
                    {
                        readBooks.Add(new Book(title, author, curPage));
                    }

                }
            }

            foreach (Book bookItem in readBooks)
            {
                bookItem.setId();
                if (bookItem.GoodreadsId != 0)
                {
                    bookList.Items.Add(bookItem.Title + " on page " + bookItem.CurrentPage);
                }
            }

            
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
