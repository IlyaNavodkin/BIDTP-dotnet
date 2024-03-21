using System;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using Autodesk.Revit.UI;
using Autodesk.Windows;
using RibbonPanel = Autodesk.Revit.UI.RibbonPanel;

namespace Example.Server.Revit.Utils
{
    /// <summary>
    ///  The ui component create util  
    /// </summary>
    public class UiComponentCreateUtil 
    {
        private static readonly Lazy<UiComponentCreateUtil> Instance = 
            new Lazy<UiComponentCreateUtil>(() => new UiComponentCreateUtil());

        /// <summary>
        ///  Get the instance of the singleton
        /// </summary>q
        /// <returns> The instance. </returns>
        public static UiComponentCreateUtil GetInstance () => Instance.Value;
        
        /// <summary>
        ///  Creates a push button data
        /// </summary>
        /// <typeparam name="T">The type of the external command .</typeparam>
        /// <param name="smallIcon"> Small icon.</param>
        /// <param name="largeIcon"> Large icon.</param>
        /// <param name="buttonName"> The name of the button .</param>
        /// <param name="toolTip"> The tool tip .</param>
        /// <param name="toolTipImage"> The tool tip image. </param>
        /// <param name="longDescription"> The long description. </param>
        /// <remarks>
        /// T is the type of the external command
        /// </remarks>
        public PushButtonData CreatePushButton<T>(string buttonName,
            ImageSource smallIcon = null, ImageSource largeIcon = null, string toolTip = null, ImageSource toolTipImage = null, 
            string longDescription = null) 
            where T : IExternalCommand
        {
            var pushButtonName = buttonName;
            var externalCommandName = typeof(T).Name;
            var className = typeof(T).FullName;

            var pushButtonData = new PushButtonData(externalCommandName,
                pushButtonName, Assembly.GetAssembly(typeof(T)).Location, className);
        
            pushButtonData.LargeImage = largeIcon;
            pushButtonData.Image = smallIcon;
            pushButtonData.ToolTip = toolTip;
            pushButtonData.ToolTipImage = toolTipImage;
            pushButtonData.LongDescription = longDescription;
            
            return pushButtonData;
        }
        
        /// <summary>
        ///  Creates a ribbon panel
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="ribbonTab">The ribbon tab .</param>
        /// <param name="panelName"> The name of the panel .</param>
        /// <param name="panelTitle"> The title of the panel .</param>
        /// <returns> The ribbon panel .</returns>
        public RibbonPanel CreateRibbonPanel(UIControlledApplication application, RibbonTab ribbonTab, 
            string panelName,string panelTittle)
        {
            RibbonPanel ribbonPanel = null;
            foreach (var internalRibbonPanel in ribbonTab.Panels)
            {
                var ribbonSource = internalRibbonPanel.Source;
                if (ribbonSource.Name != panelName) continue;
                ribbonPanel = application.GetRibbonPanels(ribbonTab.Name)
                    .FirstOrDefault(name => name.Name == panelName);
                break;
            }
    
            if (ribbonPanel is null)
            {
                ribbonPanel = application.CreateRibbonPanel(ribbonTab.Name, panelName);
                ribbonPanel.Title = panelTittle;
            }
    
            return ribbonPanel;
        }
    
        /// <summary>
        ///  Creates a ribbon tab .
        /// </summary>
        /// <param name="application">The application .</param>
        /// <param name="tabPanelName"> The name of the tab .</param>
        /// <returns> The ribbon tab .</returns>
        public RibbonTab CreateRibbonTab(UIControlledApplication application, string tabPanelName)
        {
            var ribbonControlTabs = ComponentManager.Ribbon.Tabs;
            var ribbonTab = ribbonControlTabs.FirstOrDefault(tab => tab.Name == tabPanelName);
    
            if (ribbonTab is null)
            {
                application.CreateRibbonTab(tabPanelName);
                ribbonTab = ribbonControlTabs.FirstOrDefault(tab => tab.Name == tabPanelName);
            }
            
            return ribbonTab;
        }
    }       
}

