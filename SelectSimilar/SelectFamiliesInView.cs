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
    class SelectFamiliesInView : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                if (!new Entitlements().Entitled(commandData, ref message, elements))
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

                HashSet<int> typeless = new HashSet<int>();
                //Get Family Ids from selection
                HashSet<int> typeids = new HashSet<int>();
                HashSet<int> catids = new HashSet<int>();
                IEnumerable<int> validtypeids;
                foreach (ElementId p in elids)
                {
                    if(uidoc.Document.GetElement(p).GetTypeId().IntegerValue == -1){
                        typeless.Add(p.IntegerValue);
                        continue;
                    }
                    if (uidoc.Document.GetElement(p) is FamilyInstance faminst)
                        validtypeids = faminst.Symbol.Family.GetFamilySymbolIds().Select(a => a.IntegerValue);
                    else
                        validtypeids = uidoc.Document.GetElement(p).GetValidTypes().Where(a => famnames.Contains(((ElementType)uidoc.Document.GetElement(a)).FamilyName)).Select(a => a.IntegerValue);

                    if (validtypeids.Count() > 0)
                        typeids.UnionWith(validtypeids);
                    else
                        catids.Add(uidoc.Document.GetElement(p).Category.Id.IntegerValue);
                }

                HashSet<string> typelessnames = new HashSet<string>();
                foreach(int p in typeless)
                {
                    Element e = uidoc.Document.GetElement(new ElementId(p));
                    typelessnames.Add(e.Name);
                }

                //Select elements
                List<ElementId> select = new FilteredElementCollector(uidoc.Document, uidoc.ActiveView.Id).Where(a => typeids.Contains(a.GetTypeId().IntegerValue) || (a.Category != null && catids.Contains(a.Category.Id.IntegerValue))).Select(a => a.Id).ToList();
                select = select.Union(new FilteredElementCollector(uidoc.Document, uidoc.ActiveView.Id).Where(a => typelessnames.Contains(a.Name)).Select(a => a.Id)).ToList();

                uidoc.Selection.SetElementIds(select);

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                MessageBox.Show( ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return Result.Failed;
            }
        }
    }
}
