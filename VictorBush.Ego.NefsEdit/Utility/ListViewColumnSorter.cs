// See LICENSE.txt for license information.

using System.Collections;
using System.Globalization;

namespace VictorBush.Ego.NefsEdit.Utility;

/// <summary>
/// This class is an implementation of the 'IComparer' interface.
/// </summary>
/// <remarks>
/// Source: https://support.microsoft.com/en-us/help/319401/how-to-sort-a-listview-control-by-a-column-in-visual-c .
/// </remarks>
public class ListViewColumnSorter : IComparer
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ListViewColumnSorter"/> class.
	/// </summary>
	public ListViewColumnSorter()
	{
		// Initialize the column to '0'
		SortColumn = 0;

		// Initialize the sort order to 'none'
		Order = SortOrder.None;
	}

	/// <summary>
	/// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
	/// </summary>
	public SortOrder Order { get; set; }

	/// <summary>
	/// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
	/// </summary>
	public int SortColumn { get; set; }

	/// <summary>
	/// This method is inherited from the IComparer interface. It compares the two objects passed using a case
	/// insensitive comparison.
	/// </summary>
	/// <param name="x">First object to be compared.</param>
	/// <param name="y">Second object to be compared.</param>
	/// <returns>
	/// The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'.
	/// </returns>
	public int Compare(object? x, object? y)
	{
		int compareResult;
		ListViewItem? listviewX, listviewY;

		// Cast the objects to be compared to ListViewItem objects
		listviewX = x as ListViewItem;
		listviewY = y as ListViewItem;

		var xText = listviewX?.SubItems[SortColumn].Text;
		var yText = listviewY?.SubItems[SortColumn].Text;

		// Compare as uint if possible, otherwise sort by text
		if (uint.TryParse(xText, NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out var xInt)
			&& uint.TryParse(yText, NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out var yInt))
		{
			compareResult = xInt.CompareTo(yInt);
		}
		else
		{
			compareResult = string.CompareOrdinal(
				listviewX?.SubItems[SortColumn].Text,
				listviewY?.SubItems[SortColumn].Text);
		}

		// Calculate correct return value based on object comparison
		if (Order == SortOrder.Ascending)
		{
			// Ascending sort is selected, return normal result of compare operation
			return compareResult;
		}
		else if (Order == SortOrder.Descending)
		{
			// Descending sort is selected, return negative result of compare operation
			return -compareResult;
		}
		else
		{
			// Return '0' to indicate they are equal
			return 0;
		}
	}
}
