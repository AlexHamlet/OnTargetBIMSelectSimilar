using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SelectSimilar.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SelectSimilar
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SelectInstance : IExternalCommand
    {
        private UIDocument uidoc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                if (!new Entitlements().Entitled(commandData, ref message, elements))
                {
                    MessageBox.Show("Please Purchase this software through the Autodesk App Store or Contact sales@ontargetbim.com", "Error");
                    return Result.Failed;
                }

                uidoc = commandData.Application.ActiveUIDocument;

                List<ElementId> elids = uidoc.Selection.GetElementIds().ToList();
                if (elids.Count != 1)
                {
                    MessageBox.Show("You must select only one element", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return Result.Cancelled;
                }

                new wndSelectInstance(uidoc, uidoc.Document.GetElement(elids.First())).ShowDialog();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
                return Result.Failed;
            }
        }
    }
}
