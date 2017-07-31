using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace VictorBush.Ego.NefsEdit.UI
{
    public partial class PropertyGridForm : DockContent
    {
        public PropertyGridForm()
        {
            InitializeComponent();
        }

        public void SetSelectedObject(Object obj)
        {
            propertyGrid.SelectedObject = obj;
        }
    }
}
