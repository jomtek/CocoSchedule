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
using System.ComponentModel;
using CocoSchedule.Data;

namespace CocoSchedule.Forms
{
    /// <summary>
    /// Logique d'interaction pour TaskInformationWindow.xaml
    /// </summary>
    public partial class TaskInformationWindow : Window
    {
        private TaskDescription _description;

        public TaskInformationWindow(ref TaskDescription description)
        {
            InitializeComponent();
            _description = description;

            TitleTB.Text = description.TitleText;
            DescriptionRTB.Document.Blocks.Clear();
            DescriptionRTB.Document.Blocks.Add(new Paragraph(new Run(description.DescriptionText)));

            TitleTB.Focus();
        }

        private void SaveBTN_Click(object sender, RoutedEventArgs e)
        {
            _description.TitleText = TitleTB.Text.Trim();
            _description.DescriptionText = new TextRange(DescriptionRTB.Document.ContentStart, DescriptionRTB.Document.ContentEnd).Text.Trim();

            DialogResult = true;
            Close();
        }
    }
}
