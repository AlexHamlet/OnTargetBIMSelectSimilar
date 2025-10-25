using Autodesk.Revit.UI;
using Autodesk.Windows;
using RestSharp;
using RestSharp.Serialization.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SelectSimilar
{
    class SelectSimilarExtApp : IExternalApplication
    {


        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                string tabname = "OTBIM";
                try
                {
                    application.CreateRibbonTab(tabname);
                }
                catch (Exception) { }
                Autodesk.Revit.UI.RibbonPanel rpUtility = application.CreateRibbonPanel(tabname, "Select Similar");

                PulldownButtonData ssbtn = new PulldownButtonData("OTBSelectSimilar", "Select Similar");
                ssbtn.LargeImage = PngImageSource("SelectSimilar.Resources.SS32.png");
                ssbtn.Image = PngImageSource("SelectSimilar.Resources.SS16.png");

                PushButtonData scivbtn = new PushButtonData("OTBSelectCategoryinView", "Categories in view", localpath("SelectSimilar.dll"), "SelectSimilar.SelectCategoriesInView");
                PushButtonData scipbtn = new PushButtonData("OTBSelectCategoryinProject", "Categories in project", localpath("SelectSimilar.dll"), "SelectSimilar.SelectCategoriesInProject");
                PushButtonData sfivbtn = new PushButtonData("OTBSelectFamilyinView", "Families in view", localpath("SelectSimilar.dll"), "SelectSimilar.SelectFamiliesInView");
                PushButtonData sfipbtn = new PushButtonData("OTBSelectFamilyinProj", "Families in project", localpath("SelectSimilar.dll"), "SelectSimilar.SelectFamiliesInProject");
                PushButtonData stivbtn = new PushButtonData("OTBSelectTypeinView", "Types in view", localpath("SelectSimilar.dll"), "SelectSimilar.SelectTypesInView");
                PushButtonData stipbtn = new PushButtonData("OTBSelectTypeinProj", "Types in project", localpath("SelectSimilar.dll"), "SelectSimilar.SelectTypesInProject");
                PushButtonData sibtn = new PushButtonData("OTBSelectInstance", "Select Instances", localpath("SelectSimilar.dll"), "SelectSimilar.SelectInstance");
                PushButtonData abt = new PushButtonData("OTBAbout", "Privacy Policy", localpath("SelectSimilar.dll"), "SelectSimilar.About");

                scivbtn.SetContextualHelp(SSContextualHelp());
                scivbtn.ToolTip = "Select one or more elements before running this command";
                scipbtn.SetContextualHelp(SSContextualHelp());
                scipbtn.ToolTip = "Select one or more elements before running this command";
                sfivbtn.SetContextualHelp(SSContextualHelp());
                sfivbtn.ToolTip = "Select one or more elements before running this command";
                sfipbtn.SetContextualHelp(SSContextualHelp());
                sfipbtn.ToolTip = "Select one or more elements before running this command";
                stivbtn.SetContextualHelp(SSContextualHelp());
                stivbtn.ToolTip = "Select one or more elements before running this command";
                stipbtn.SetContextualHelp(SSContextualHelp());
                stipbtn.ToolTip = "Select one or more elements before running this command";
                sibtn.SetContextualHelp(SSContextualHelp());
                sibtn.ToolTip = "Select a single element before running this command";
                abt.SetContextualHelp(SSContextualHelp());


                PulldownButton ssbtnpd = rpUtility.AddItem(ssbtn) as PulldownButton;
                ssbtnpd.AddPushButton(scivbtn);
                ssbtnpd.AddPushButton(scipbtn);
                ssbtnpd.AddPushButton(sfivbtn);
                ssbtnpd.AddPushButton(sfipbtn);
                ssbtnpd.AddPushButton(stivbtn);
                ssbtnpd.AddPushButton(stipbtn);
                ssbtnpd.AddPushButton(sibtn);
                ssbtnpd.AddPushButton(abt);

                //application.ControlledApplication.ApplicationInitialized += OnApplicationInitialized;

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return Result.Failed;
            }
        }

        private ContextualHelp SSContextualHelp()
        {
            return new ContextualHelp(ContextualHelpType.Url, "https://apps.autodesk.com/ACD/en/Detail/HelpDoc?appId=955852388506253137&appLang=en&os=Win64&mode=preview");
        }

        private void MoveRibbonItemsBetweenTabPanels(
        string sourceTabName,
        string sourcePanelName,
        string targetTabName,
        string targetPanelName)
        {
            try
            {
                Autodesk.Windows.RibbonPanel sourceRibbonPanel = null;
                bool isBreak = false;
                Autodesk.Windows.RibbonTabCollection tabs = Autodesk.Windows.ComponentManager.Ribbon.Tabs;
                // Get source ribbon panel
                foreach (Autodesk.Windows.RibbonTab tab in tabs)
                {
                    if (tab.Id.Equals(sourceTabName, StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (Autodesk.Windows.RibbonPanel panel in tab.Panels)
                        {
                            if (panel.Source.AutomationName.Equals(sourcePanelName, StringComparison.OrdinalIgnoreCase))
                            {
                                sourceRibbonPanel = panel;
                                // Remove this panel of out its ribbon tab
                                tab.Panels.Remove(panel);
                                isBreak = true;
                                break;
                            }
                        }
                        if (isBreak) break;
                    }
                }
                // Copy items from this source ribbon panel to another ribbon panel (in different tab)
                if (sourceRibbonPanel != null)
                {
                    foreach (Autodesk.Windows.RibbonTab tab in tabs)
                    {
                        if (tab.Id.Equals(targetTabName, StringComparison.OrdinalIgnoreCase))
                        {
                            tab.Panels.Add(sourceRibbonPanel);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }

        void OnApplicationInitialized(object sender, Autodesk.Revit.DB.Events.ApplicationInitializedEventArgs e)
        {
            try
            {
                MoveRibbonItemsBetweenTabPanels("Add-Ins", "Select Similar", "Modify", "Select Similar");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Get a dll relative to this one.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string localpath(string file)
        {
            return string.Format("{0}\\{1}", AssemblyDirectory, file);
        }

        /// <summary>
        /// Get the assembly directory for the addin.  Useful for finding dlls relative to this one.
        /// </summary>
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        /// <summary>
        /// Get an embedded resource file from the dll.
        /// </summary>
        /// <param name="embeddedPath"></param>
        /// <returns>An imagesource corresponding to the given path</returns>
        private ImageSource PngImageSource(string embeddedPath)
        {
            Stream stream = this.GetType().Assembly.GetManifestResourceStream(embeddedPath);
            var decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

            return decoder.Frames[0];
        }
    }
}
