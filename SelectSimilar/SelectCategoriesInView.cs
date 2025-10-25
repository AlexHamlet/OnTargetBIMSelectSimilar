using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SelectSimilar
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class SelectCategoriesInView : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIDocument uidoc = commandData.Application.ActiveUIDocument;

                if (uidoc.Document.IsFamilyDocument)
                {
                    MessageBox.Show("Select Categories is not available in a family view.", "Notice");
                    return Result.Cancelled;
                }

                //Get Selection
                List<ElementId> elids = uidoc.Selection.GetElementIds().ToList();
                if (elids.Count == 0)
                {
                    MessageBox.Show("Please Select at least one element", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return Result.Cancelled;
                }

                //Get Family Ids from selection
                HashSet<int> catids = new HashSet<int>();
                foreach (ElementId p in elids)
                {
                    int id = uidoc.Document.GetElement(p).Category.Id.IntegerValue;
                    if (id != -1)
                        catids.Add(id);
                }
                //Select elements
                uidoc.Selection.SetElementIds(new FilteredElementCollector(uidoc.Document, uidoc.ActiveView.Id).Where(a => a.Category != null && catids.Contains(a.Category.Id.IntegerValue)).Select(a => a.Id).ToList());

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return Result.Failed;
            }
        }
    }
}
