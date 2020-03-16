// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Utility
{
    using System.Collections;
    using System.Windows.Forms;

    /// <summary>
    /// This class is an implementation of the 'IComparer' interface.
    /// </summary>
    /// <remarks>
    /// Source: https://support.microsoft.com/en-us/help/319401/how-to-sort-a-listview-control-by-a-column-in-visual-c .
    /// </remarks>
    public class ListViewColumnSorter : IComparer
    {
        /// <summary>
        /// Case insensitive comparer object.
        /// </summary>
        private readonly CaseInsensitiveComparer objectCompare;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListViewColumnSorter"/> class.
        /// </summary>
        public ListViewColumnSorter()
        {
            // Initialize the column to '0'
            this.SortColumn = 0;

            // Initialize the sort order to 'none'
            this.Order = SortOrder.None;

            // Initialize the CaseInsensitiveComparer object
            this.objectCompare = new CaseInsensitiveComparer();
        }

        /// <summary>
        /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
        /// </summary>
        public SortOrder Order { get; set; }

        /// <summary>
        /// Gets or sets the number of the column to which to apply the sorting operation (Defaults
        /// to '0').
        /// </summary>
        public int SortColumn { get; set; }

        /// <summary>
        /// This method is inherited from the IComparer interface. It compares the two objects
        /// passed using a case insensitive comparison.
        /// </summary>
        /// <param name="x">First object to be compared.</param>
        /// <param name="y">Second object to be compared.</param>
        /// <returns>
        /// The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and
        /// positive if 'x' is greater than 'y'.
        /// </returns>
        public int Compare(object x, object y)
        {
            int compareResult;
            ListViewItem listviewX, listviewY;

            // Cast the objects to be compared to ListViewItem objects
            listviewX = (ListViewItem)x;
            listviewY = (ListViewItem)y;

            // Compare the two items
            compareResult = this.objectCompare.Compare(
                listviewX.SubItems[this.SortColumn].Text,
                listviewY.SubItems[this.SortColumn].Text);

            // Calculate correct return value based on object comparison
            if (this.Order == SortOrder.Ascending)
            {
                // Ascending sort is selected, return normal result of compare operation
                return compareResult;
            }
            else if (this.Order == SortOrder.Descending)
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
}
