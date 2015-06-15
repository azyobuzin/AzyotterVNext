using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Azyotter.ViewModels;

namespace Azyotter.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.vm = (MainViewModel)this.DataContext;
        }

        private MainViewModel vm;

        private void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            var listBox = (ListBox)sender;
            var tabVm = (TabViewModel)listBox.DataContext;
            listBox.InputBindings.AddRange(new[]
            {
                new KeyBinding(tabVm.FavoriteCommand, Key.S, ModifierKeys.Control),
                new KeyBinding(tabVm.RetweetCommand, Key.R, ModifierKeys.Alt)
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.tweetText.InputBindings.Add(new KeyBinding(this.vm.Tweeting.TweetCommand, Key.Return, ModifierKeys.Control));

            this.vm.Initialize();
        }
    }
}
