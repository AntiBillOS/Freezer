using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.IO;
namespace WpfRefrigerator
{
    /// <summary>
    /// Interaction logic for Journal.xaml
    /// </summary>
    public partial class Journal : Window
    {
        public Journal()
        {
            InitializeComponent();
        }
        private void Read_File()
        {
            MainWindow s = this.Owner as MainWindow;
            foreach (var item in s.RfEvents)//через итератов класса получаем информацию об событиях
            {
                JournalInfo.Text += item.ToString() + '\n';
            }
        }
        private void Load(object sender, RoutedEventArgs e)
        {
            Read_File();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainWindow s = this.Owner as MainWindow;
            //системное окно сохраненния файла, формата txt
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Document"; // Default file name
            dlg.DefaultExt = ".text"; // Default file extension
            dlg.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension
            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();
            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                using (var news = new StreamWriter(filename, false))
                {
                    foreach (var item in s.RfEvents)
                    {
                        news.WriteLine(item);
                    }
                }
            }
        }
    }
}
