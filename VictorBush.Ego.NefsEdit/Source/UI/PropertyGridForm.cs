// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.UI
{
    using System;
    using WeifenLuo.WinFormsUI.Docking;

    /// <summary>
    /// Form that contains the property grid.
    /// </summary>
    public partial class PropertyGridForm : DockContent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyGridForm"/> class.
        /// </summary>
        public PropertyGridForm()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Sets the selected object in the property grid.
        /// </summary>
        /// <param name="obj">The object to display in the property grid.</param>
        public void SetSelectedObject(Object obj)
        {
            this.propertyGrid.SelectedObject = obj;
        }
    }
}
