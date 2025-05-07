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


namespace ArcGISPro2GoogleEarthPro
{
    /// <summary>
    /// Interaction logic for ToolViewerView.xaml
    /// </summary>
    public partial class ToolViewerView : UserControl
    {
        public ToolViewerView()
        {
            InitializeComponent();
        }
        public static AEVariables AEV = AEVariables.GetInstance();

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            DataContext = AEV;
        }
    }
}
