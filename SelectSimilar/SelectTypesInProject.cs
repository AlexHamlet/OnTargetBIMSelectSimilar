using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SelectSimilar
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class SelectTypesInProject : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                if(!new Entitlements().Entitled(commandData,ref message, elements))
                {
                    MessageBox.Show("Please Purchase this software through the Autodesk App Store or Contact sales@ontargetbim.com", "Error");
                    return Result.Failed;
                }

                UIDocument uidoc = commandData.Application.ActiveUIDocument;

                //Get Selection
                List<ElementId> elids = uidoc.Selection.GetElementIds().ToList();
                if (elids.Count == 0)
                {
                    MessageBox.Show("Please Select at least one element", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return Result.Cancelled;
                }

                HashSet<string> famnames = new HashSet<string>();
                foreach (var p in elids)
                {
                    Element e = uidoc.Document.GetElement(p);
                    if (e.GetTypeId().IntegerValue != -1)
                        famnames.Add(((ElementType)uidoc.Document.GetElement(e.GetTypeId())).FamilyName);
                }

                //special cases
                HashSet<int> linestyle = new HashSet<int>();
                HashSet<int> revisioncloud = new HashSet<int>();
                //typeless
                HashSet<int> typeless = new HashSet<int>();
                //Get Family Ids from selection
                HashSet<int> typeids = new HashSet<int>();
                HashSet<int> catids = new HashSet<int>();
                Element element;
                foreach (ElementId p in elids)
                {
                    element = uidoc.Document.GetElement(p);
                    if (element.GetTypeId().IntegerValue != -1)
                        typeids.Add(element.GetTypeId().IntegerValue);
                    else
                    {
                        typeless.Add(p.IntegerValue);
                        if (element is RevisionCloud)
                            revisioncloud.Add(p.IntegerValue);
                        if (element is CurveElement ce)
                            linestyle.Add(ce.LineStyle.Id.IntegerValue);
                        //catids.Add(element.Category.Id.IntegerValue);
                    }
                }

                HashSet<string> typelessnames = new HashSet<string>();
                foreach (int p in typeless)
                {
                    Element e = uidoc.Document.GetElement(new ElementId(p));
                    typelessnames.Add(e.Name);
                }

                //Select elements
                List<ElementId> select = new FilteredElementCollector(uidoc.Document).WhereElementIsNotElementType().Where(a => typeids.Contains(a.GetTypeId().IntegerValue) || (a.Category != null && catids.Contains(a.Category.Id.IntegerValue))).Select(a => a.Id).ToList();
                select = select.Union(new FilteredElementCollector(uidoc.Document).WhereElementIsNotElementType()
                    .Where(
                    a =>
                    {
                        if (typelessnames.Contains(a.Name))
                        {
                            if (a is RevisionCloud rc)
                                if (revisioncloud.Contains(rc.RevisionId.IntegerValue))
                                    return true;

                            if (a is CurveElement ce)
                                if (linestyle.Contains(ce.LineStyle.Id.IntegerValue))
                                    return true;
                                else return false;

                            return true;
                        }
                        return false;
                    }
                    ).Select(a => a.Id)).ToList();

                uidoc.Selection.SetElementIds(select);

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
