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
    /// Служба для создания кнопки на элементе интерфейса пользователя, таком как панель ленты инструментов.
    /// </summary>
    public class UiComponentCreateUtil 
    {
        private static readonly Lazy<UiComponentCreateUtil> Instance = 
            new Lazy<UiComponentCreateUtil>(() => new UiComponentCreateUtil());

        /// <summary>
        ///  Возвращает экземпляр синглтона
        /// </summary>q
        /// <returns> Экземпляр синглтона</returns>
        public static UiComponentCreateUtil GetInstance () => Instance.Value;
        
        /// <summary>
        /// Создает кнопку с указанными свойствами и добавляет ее на указанную панель ленты инструментов.
        /// </summary>
        /// <typeparam name="T">Тип внешней команды, которую выполнит кнопка.</typeparam>
        /// <param name="smallIcon">Источник изображения для маленькой иконки</param>
        /// <param name="largeIcon">Изображение для отображения на кнопке.</param>
        /// <param name="buttonName">Имя кнопки.</param>
        /// <param name="toolTip">Текст для отображения всплывающей подсказки при наведении на кнопку.</param>
        /// <param name="toolTipImage">Источник изображения для всплывающей подсказки</param>
        /// <param name="longDescription">Описание для всплывающей подсказки</param>
        /// <remarks>
        /// Параметр типа T должен реализовывать интерфейс IExternalCommand.
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
        /// Инициализирует панель ленты инструментов для интерфейса пользователя управляемого приложения.
        /// </summary>
        /// <param name="application">Управляемое приложение.</param>
        /// <param name="ribbonTab">Лента инструментов, к которой будет добавлена панель.</param>
        /// <param name="panelName">Имя панели.</param>
        /// <param name="panelTitle">Заголовок панели.</param>
        /// <returns>Инициализированная панель ленты инструментов.</returns>
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
        /// Инициализирует вкладку ленты инструментов для интерфейса пользователя управляемого приложения.
        /// </summary>
        /// <param name="application">Управляемое приложение.</param>
        /// <param name="tabPanelName">Имя вкладки.</param>
        /// <returns>Инициализированная вкладка ленты инструментов.</returns>
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

