// See LICENSE.txt for license information.

using WeifenLuo.WinFormsUI.Docking;

namespace VictorBush.Ego.NefsEdit.UI;

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
		InitializeComponent();
	}

	/// <summary>
	/// Refreshes the properties list.
	/// </summary>
	public void RefreshGrid()
	{
		this.propertyGrid.Refresh();
	}

	/// <summary>
	/// Sets the selected object in the property grid.
	/// </summary>
	/// <param name="obj">The object to display in the property grid.</param>
	public void SetSelectedObject(object? obj)
	{
		this.propertyGrid.SelectedObject = obj;
	}
}
