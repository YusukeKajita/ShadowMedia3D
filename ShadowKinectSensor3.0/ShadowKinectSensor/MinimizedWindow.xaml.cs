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

namespace ShadowKinectSensor
{
    /// <summary>
    /// MinimizedWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MinimizedWindow : Window
    {
        public MainWindow mainwindow { set; get; }
        public MinimizedWindow()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += (s, e) => this.DragMove();
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.Hide();
            this.mainwindow.Show();
        }
    }
}
