using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.ApplicationModel.DataTransfer;
//using Windows.ApplicationModel.DataTransfer;

namespace Wpf_capitalHelper
{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            add_clickboard_event();
        }

        private void tb_input_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            tb_input.Text = "";
        }
        private void tb_input_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox? tb = sender as TextBox;
            if (tb == null) return;

            if (e.Key == Key.Return)//Enter key
            {
                btn_convert.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        private void btn_convert_Click(object sender, RoutedEventArgs e)
        {
            string output = "";
            string input_text = tb_input.Text;
            List<string> exclude_text = (File.ReadLines("exclude.txt")).ToList();


            //split to sigle word
            string[] word = input_text.Split(' ');

            //First word (need to be capital)
            //hyphen word progress
            string[] hyphen_word = word[0].Split('-');
            if (hyphen_word.Count() > 1)
            {
                string tmp = hyphen_word[0].Capitalize();
                for (int hi = 1; hi < hyphen_word.Count(); hi++)
                {
                    tmp += "-" + Capitalize_word_with_checking(hyphen_word[hi], exclude_text);
                }
                output += tmp;

            }
            else //word is single(without hyphen)
            {
                output += word[0].Capitalize();
            }

            bool colon_check = false;
            for (int i = 1; i < word.Count(); i++)
            {
                if(colon_check == true)//if preword have colon, Capital!
                {
                    output += " " + word[i].Capitalize();
                    colon_check = false;
                    continue;
                }
                hyphen_word = word[i].Split('-');
                if (hyphen_word.Count() > 1)
                {
                    //hyphen word progress
                    string tmp = Capitalize_word_with_checking(hyphen_word[0], exclude_text);
                    for (int hi = 1; hi < hyphen_word.Count(); hi++)
                    {
                        tmp += "-" + Capitalize_word_with_checking(hyphen_word[hi], exclude_text);
                    }
                    output += " " + tmp;
                }
                else //word is single(without hyphen)
                {
                    //check ":"
                    if (word[i].IndexOf(':') >= 0)
                        colon_check = true;
                    output += " " + Capitalize_word_with_checking(word[i], exclude_text);
                }
            }
            
            tb_output.Text = output;

            //Auto copy to clickboard
            if (cb_auto_copy_to_clickboard.IsChecked == true)
            {
                remove_clickboard_event();
                System.Windows.Clipboard.SetText(output);
                add_clickboard_event();
            }
        }

        private string Capitalize_word_with_checking(string input, List<string> exclude_text)
        {
            string rtn = input;
            if (exclude_text.Exists(x => x == input) == false)//if the word is not in the exclude keyword
            {
                rtn = input.Capitalize();
                //Trace.WriteLine(word[i].Capitalize());
            }
            return rtn;
        }


        #region Clickboard event
        public string Clipboard_text = "";
        //public static event System.EventHandler<object> ContentChanged;
        private void add_clickboard_event()
        {
            Windows.ApplicationModel.DataTransfer.Clipboard.ContentChanged += new EventHandler<object>(TrackClipboardChanges_EventHandler);
        }
        private void remove_clickboard_event()
        {
            Windows.ApplicationModel.DataTransfer.Clipboard.ContentChanged -= new EventHandler<object>(TrackClipboardChanges_EventHandler);
        }
        private async void TrackClipboardChanges_EventHandler(object sender, object e)
        {
            String text = "";
            DataPackageView dataPackageView = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                text = await dataPackageView.GetTextAsync();
                Clipboard_text = text;
                Trace.WriteLine("click board : \"" + text + "\"");
            }
            if (cb_auto_detect_copy.IsChecked == true)
                tb_input.Text = text;
            if (cb_auto_convert.IsChecked == true)
                btn_convert.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
        #endregion
    }


    static class Ex
    {
        public static string Capitalize(this string word)
        {
            return word.Substring(0, 1).ToUpper() + word.Substring(1).ToLower();
        }

    }


}



