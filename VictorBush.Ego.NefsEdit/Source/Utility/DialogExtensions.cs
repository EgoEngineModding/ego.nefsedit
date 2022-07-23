// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsEdit.Utility;

/// <summary>
/// Extension to allow the form to be a modal dialog while perform async operations.
/// See: https://stackoverflow.com/questions/33406939/async-showdialog.
/// </summary>
public static class DialogExtensions
{
	/// <summary>
	/// Allows opening a modal dialog asynchronously.
	/// </summary>
	/// <param name="this">The dialog.</param>
	/// <returns>The dialog result.</returns>
	public static async Task<DialogResult> ShowDialogAsync(this Form @this)
	{
		await Task.Yield();
		if (@this.IsDisposed)
		{
			return DialogResult.OK;
		}

		return @this.ShowDialog();
	}
}
