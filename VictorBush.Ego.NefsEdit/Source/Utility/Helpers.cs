using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VictorBush.Ego.NefsEdit.Utility
{
    public static class Helpers
    {
        /// <summary>
        /// Gets all descendant nodes of a TreeNode.
        /// </summary>
        /// <remarks>
        /// See https://stackoverflow.com/questions/177277/how-to-get-a-list-of-all-child-nodes-in-a-treeview-in-net.
        /// </remarks>
        /// <param name="input">The TreeNode to get descendants off.</param>
        /// <returns></returns>
        public static IEnumerable<TreeNode> DescendantNodes(this TreeNode input)
        {
            foreach (TreeNode node in input.Nodes)
            {
                yield return node;
                foreach (var subnode in node.DescendantNodes())
                    yield return subnode;
            }
        }
    }
}
