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
        public List<Book> books;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
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
                        bookList.Items.Add(title + " on page " + curPage);
                        //books.Add(new Book(title, author, curPage));
                    }

                }
            }

            
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
