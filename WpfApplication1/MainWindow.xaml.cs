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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApplication1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ShapeChangeMouseEvent(object sender, MouseEventArgs e)
        {
            if (button1.IsMouseOver)
            {
                bool result = VisualStateManager.GoToState(button1, "RectangleState", true);
            }
            else
            {
                VisualStateManager.GoToState(button1, "EllipseState", true);
            }
        }

        private void ColorChangeMouseEvent(object sender, MouseEventArgs e)
        {
            if (button2.IsMouseOver)
            {
                VisualStateManager.GoToElementState(button2, "BlueState", true);
            }
            else
            {
                VisualStateManager.GoToElementState(button2, "OrangeState", true);
            }
        }
    }
}
