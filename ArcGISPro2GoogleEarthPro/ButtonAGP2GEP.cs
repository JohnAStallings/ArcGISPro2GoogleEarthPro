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

namespace ArcGISPro2GoogleEarthPro
{
	internal class ButtonAGP2GEP : Button
	{
        protected override void OnClick()
        {
            if (MapView.Active == null) return;
            if (MapView.Active.IsReady == false) return;

            if (ToolViewerViewModel.IsShown()) { ToolViewerViewModel.Close(); }
            else { ToolViewerViewModel.Show(); }
            ToolViewerView.AEV.ShowArc2Earth();
        }
	}
}
