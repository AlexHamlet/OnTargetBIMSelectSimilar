using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using Grid = System.Windows.Controls.Grid;

namespace SelectSimilar.UI
{
    /// <summary>
    /// Interaction logic for SelectInstance.xaml
    /// </summary>
    public partial class wndSelectInstance : Window
    {
        private UIDocument uidoc;
        private ElementId category;
        private int symbol;
        private Element Element;
        private List<ParameterCheckBox> parameterCheckBoxes;
        public wndSelectInstance(UIDocument uidoc, Element element)
        {
            try
            {
                InitializeComponent();
                this.uidoc = uidoc;
                Element = element;
                if (!uidoc.Document.IsFamilyDocument)
                    category = element.Category.Id;

                symbol = element.GetTypeId().IntegerValue;

                stpnlcatfamtypetitle.Children.Add(new CheckBox() { Name = "cbCategory", IsChecked = true, IsEnabled = false, Content = "Category:", Height = 16 });
                stpnlcatfamtypetitle.Children.Add(new CheckBox() { Name = "cbFamily", IsChecked = true, IsEnabled = false, Content = "Family:", Height = 16 });
                stpnlcatfamtypetitle.Children.Add(new CheckBox() { Name = "cbType", IsChecked = true, IsEnabled = false, Content = "Type:", Height = 16 });

                stpnlcatfamtype.Children.Add(new Label() { Margin = new Thickness(2, 0, 0, 0), Content = element.Category != null ? element.Category.Name : "", FontSize = 12, Height = 16, Padding = new Thickness(0), VerticalContentAlignment = VerticalAlignment.Center });

                if (element.GetTypeId().IntegerValue != -1)
                {
                    stpnlcatfamtype.Children.Add(new Label() { Margin = new Thickness(2, 0, 0, 0), Content = ((ElementType)uidoc.Document.GetElement(uidoc.Document.GetElement(element.GetTypeId()).Id)).FamilyName, FontSize = 12, Height = 16, Padding = new Thickness(0), VerticalContentAlignment = VerticalAlignment.Center });
                    stpnlcatfamtype.Children.Add(new Label() { Margin = new Thickness(2, 0, 0, 0), Content = uidoc.Document.GetElement(uidoc.Document.GetElement(element.GetTypeId()).Id).Name, FontSize = 12, Height = 16, Padding = new Thickness(0), VerticalContentAlignment = VerticalAlignment.Center });
                }
                else
                {
                    if (element is RevisionCloud rc)
                    {
                        stpnlcatfamtype.Children.Add(new Label() { Margin = new Thickness(2, 0, 0, 0), Content = "Revision Clouds", FontSize = 12, Height = 16, Padding = new Thickness(0), VerticalContentAlignment = VerticalAlignment.Center });
                        stpnlcatfamtype.Children.Add(new Label() { Margin = new Thickness(2, 0, 0, 0), Content = uidoc.Document.GetElement(rc.RevisionId).Name, FontSize = 12, Height = 16, Padding = new Thickness(0), VerticalContentAlignment = VerticalAlignment.Center });
                    }
                    else if (element is CurveElement ce)
                    {
                        stpnlcatfamtype.Children.Add(new Label() { Margin = new Thickness(2, 0, 0, 0), Content = element.Name, FontSize = 12, Height = 16, Padding = new Thickness(0), VerticalContentAlignment = VerticalAlignment.Center });
                        stpnlcatfamtype.Children.Add(new Label() { Margin = new Thickness(2, 0, 0, 0), Content = ce.LineStyle.Name, FontSize = 12, Height = 16, Padding = new Thickness(0), VerticalContentAlignment = VerticalAlignment.Center });
                    }
                    else
                        stpnlcatfamtype.Children.Add(new Label() { Margin = new Thickness(2, 0, 0, 0), Content = element.Name, FontSize = 12, Height = 16, Padding = new Thickness(0), VerticalContentAlignment = VerticalAlignment.Center });
                }

                parameterCheckBoxes = new List<ParameterCheckBox>();
                ParameterCheckBox pcb;
                Label value;
                int row = 0;
                Border border;
                foreach (Parameter p in element.GetOrderedParameters().OrderBy(a => a.Definition.Name))
                {
                    if (p.StorageType == StorageType.None)
                        continue;

                    grdParams.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(16) });

                    pcb = new ParameterCheckBox(p, string.Format("{0}: ", p.Definition.Name));
                    parameterCheckBoxes.Add(pcb);
                    border = new Border() { Child = pcb, BorderThickness = new Thickness(0, 0, 0, 1), BorderBrush = new SolidColorBrush(Color.FromRgb(150, 150, 150)) };
                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 0);
                    grdParams.Children.Add(border);

                    value = new Label() { Content = p.AsValueString() ?? p.AsString(), FontSize = 12, Height = 16, Margin = new Thickness(2, 0, 0, 0), Padding = new Thickness(0), VerticalContentAlignment = VerticalAlignment.Center };
                    border = new Border() { Child = value, BorderThickness = new Thickness(0, 0, 0, 1), BorderBrush = new SolidColorBrush(Color.FromRgb(150, 150, 150)) };
                    Grid.SetRow(border, row++);
                    Grid.SetColumn(border, 2);
                    grdParams.Children.Add(border);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FilteredElementCollector fec;
                if (rdbtnView.IsChecked ?? false)
                    fec = new FilteredElementCollector(uidoc.Document, uidoc.ActiveView.Id);
                else
                    fec = new FilteredElementCollector(uidoc.Document).WhereElementIsNotElementType();

                List<Element> elements;
                if (uidoc.Document.IsFamilyDocument) { 
                elements = fec.Where(a =>
                {
                    if (a.GetTypeId().IntegerValue != -1)
                    {
                        if (a.GetTypeId().IntegerValue != symbol)
                            return false;
                    }
                    else
                    {
                        if (!a.Name.Equals(Element.Name)) return false;
                    }

                    return true;
                }
                     ).ToList();
                }
                else
                {

                     elements = fec.Where(a =>
                    {
                        if (a.Category == null) return false;
                        if (a.Category.Id.IntegerValue != category.IntegerValue) return false;
                        if (a.GetTypeId().IntegerValue != -1)
                        {
                            if (a.GetTypeId().IntegerValue != symbol)
                                return false;
                        }
                        else
                        {
                            if (!a.Name.Equals(Element.Name)) return false;
                        }

                        return true;
                    }
                     ).ToList();
                }

                foreach (ParameterCheckBox p in parameterCheckBoxes)
                {
                    if (p.IsChecked ?? false)
                    {
                        switch (p.Parameter.StorageType)
                        {
                            case StorageType.Integer:
                                elements = elements.Where(a => a.LookupParameter(p.Parameter.Definition.Name) != null && a.LookupParameter(p.Parameter.Definition.Name).AsInteger() == Element.LookupParameter(p.Parameter.Definition.Name).AsInteger()).ToList();
                                break;
                            case StorageType.Double:
                                elements = elements.Where(a => a.LookupParameter(p.Parameter.Definition.Name) != null && a.LookupParameter(p.Parameter.Definition.Name).AsDouble() == Element.LookupParameter(p.Parameter.Definition.Name).AsDouble()).ToList();
                                break;
                            case StorageType.String:
                                elements = elements.Where(a => a.LookupParameter(p.Parameter.Definition.Name) != null && a.LookupParameter(p.Parameter.Definition.Name).AsString() == Element.LookupParameter(p.Parameter.Definition.Name).AsString()).ToList();
                                break;
                            case StorageType.ElementId:
                                elements = elements.Where(a => a.LookupParameter(p.Parameter.Definition.Name) != null && a.LookupParameter(p.Parameter.Definition.Name).AsElementId().IntegerValue == Element.LookupParameter(p.Parameter.Definition.Name).AsElementId().IntegerValue).ToList();
                                break;
                            default:
                                break;
                        }
                    }
                }
                uidoc.Selection.SetElementIds(elements.Select(a => a.Id).ToList());
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
