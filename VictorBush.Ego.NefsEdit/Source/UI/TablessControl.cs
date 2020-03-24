// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.UI
{
    using System;
    using System.Windows.Forms;

    /// <summary>
    /// A tab control that hides its tab buttons at run time.
    /// </summary>
    /// <remarks>
    /// https://stackoverflow.com/questions/6953487/hide-tab-header-on-c-sharp-tabcontrol.
    /// </remarks>
    public class TablessControl : TabControl
    {
        /// <inheritdoc/>
        protected override void WndProc(ref Message m)
        {
            // Hide tabs by trapping the TCM_ADJUSTRECT message
            if (m.Msg == 0x1328 && !this.DesignMode)
            {
                m.Result = (IntPtr)1;
            }
            else
            {
                base.WndProc(ref m);
            }
        }
    }
}
