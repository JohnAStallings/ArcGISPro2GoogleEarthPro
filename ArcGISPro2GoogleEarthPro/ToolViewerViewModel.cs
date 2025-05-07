using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.KnowledgeGraph;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ArcGISPro2GoogleEarthPro
{
    internal class ToolViewerViewModel : DockPane
    {   
      private const string _dockPaneID = "ArcGISPro2GoogleEarthPro_ToolViewer";
 
      protected ToolViewerViewModel() { }  

      /// <summary>
      /// Show the DockPane.
      /// </summary>
      internal static void Show()
      {        
        DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
        if (pane == null)
          return;

        pane.Activate();
      }
        internal static void Close()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null) return;
            pane.Hide();
        }
        internal static bool IsShown()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null) return false;
            bool visible = pane.IsVisible;
            return visible;
        }
        
    }

  public class AEVariables : INotifyPropertyChanged
    {
        private static AEVariables Instance = new AEVariables();
        private AEVariables() { }
        public static AEVariables GetInstance() { Instance = new AEVariables(); return Instance; }


        //Local
        private object _viewer = null;
        private PageArc2Earth _pageArc2Earth = null;

        //Public
        public object Viewer { get { return _viewer; } set { _viewer = value; NotifyPropertyChanged(); } }
        public PageArc2Earth pageArc2Earth { get { return _pageArc2Earth; } set { _pageArc2Earth = value; NotifyPropertyChanged(); } }

        private void NewArc2Earth() { _pageArc2Earth = new PageArc2Earth(); }
        public void DiscardArc2Earth() { if (_pageArc2Earth != null) { _pageArc2Earth = null; } }
        public void ShowArc2Earth() { NewArc2Earth(); Viewer = pageArc2Earth; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    }
	
}
