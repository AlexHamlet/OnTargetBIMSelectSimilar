using Autodesk.Revit.DB;
using System.Windows.Controls;

namespace SelectSimilar
{
    class ParameterCheckBox :CheckBox
    {
        public Parameter Parameter { get; }

        public ParameterCheckBox(Parameter parameter, string displaystring)
        {
            Parameter = parameter;
            Content = displaystring;
        }
    }
}
