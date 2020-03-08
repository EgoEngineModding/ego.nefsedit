// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Utility
{
    using System.Collections.Generic;
    using System.Windows.Forms;

    /// <summary>
    /// Extension methods for tree nodes.
    /// </summary>
    public static class TreeNodeExtensions
    {
        /// <summary>
        /// Gets all descendant nodes of a TreeNode.
        /// </summary>
        /// <remarks>See https://stackoverflow.com/questions/177277/how-to-get-a-list-of-all-child-nodes-in-a-treeview-in-net.</remarks>
        /// <param name="input">The TreeNode to get descendants off.</param>
        /// <returns>List of descendant tree nodes.</returns>
        public static IEnumerable<TreeNode> DescendantNodes(this TreeNode input)
        {
            foreach (TreeNode node in input.Nodes)
            {
                yield return node;
                foreach (var subnode in node.DescendantNodes())
                {
                    yield return subnode;
                }
            }
        }
    }
}
