using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace labelImage
{
    /// <summary>
    /// FillContentBox.xaml 的交互逻辑
    /// </summary>
    public partial class FillContentBox : UserControl
    {

        public string termNumber;
        public string name;

        public FillContentBox()
        {
            InitializeComponent();

            List<TermsNumberClass> termsNumberList = new List<TermsNumberClass>();

            termsNumberList.Add(new TermsNumberClass { Color = "Red",  Name = "1期", Value = "1" });
            termsNumberList.Add(new TermsNumberClass { Color = "Orange", Name = "2期", Value = "2" });
            termsNumberList.Add(new TermsNumberClass { Color = "Yellow", Name = "3期", Value = "3" });
            termsNumberList.Add(new TermsNumberClass { Color = "Green", Name = "4期", Value = "4" });
            termsNumberList.Add(new TermsNumberClass { Color = "Blue", Name = "5期", Value = "5" });
            termsNumberList.Add(new TermsNumberClass { Color = "Indigo", Name = "6期", Value = "6" });
            termsNumberList.Add(new TermsNumberClass { Color = "Purple", Name = "7期", Value = "7" });
            termNumberBox.ItemsSource = termsNumberList;
            termNumberBox.SelectedIndex = 0;

        }

        private void okButtonClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("ok");
            name = nameTextBox.Text.Length <= 0 ? "default" : nameTextBox.Text;
            TermsNumberClass termsNumberClass = termNumberBox.SelectedItem as TermsNumberClass;
            termNumber = termsNumberClass.Value;
            Console.WriteLine(name);
        }

        private void cancelButtonClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("cancel");
        }

        private void termsNumberBoxClick(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine("123");
        }
    }


    public class TermsNumberClass
    {
        public string Color { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
