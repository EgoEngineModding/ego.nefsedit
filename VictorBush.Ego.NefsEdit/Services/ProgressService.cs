// See LICENSE.txt for license information.

using Microsoft.Extensions.Logging;
using VictorBush.Ego.NefsEdit.UI;
using VictorBush.Ego.NefsEdit.Utility;
using VictorBush.Ego.NefsLib.Progress;

namespace VictorBush.Ego.NefsEdit.Services;

/// <summary>
/// Progress service implementation.
/// </summary>
internal class ProgressService : IProgressService
{
	private static readonly ILogger Log = LogHelper.GetLogger();

	/// <summary>
	/// Initializes a new instance of the <see cref="ProgressService"/> class.
	/// </summary>
	/// <param name="uiService">The UI service to use.</param>
	public ProgressService(IUiService uiService)
	{
		UiService = uiService ?? throw new ArgumentNullException(nameof(uiService));
	}

	private IUiService UiService { get; }

	/// <inheritdoc/>
	public async Task RunModalTaskAsync(Func<NefsProgress, Task> task)
	{
		// Create a progress dialog
		var progressForm = new ProgressDialogForm(UiService);

		// Show the progress dialog. Don't await this call. Need to allow dialog to show modally, but want to continue execution.
		var progressFormTask = progressForm.ShowDialogAsync();

		// Run the task
		try
		{
			await task(progressForm.ProgressInfo);
		}
		catch (Exception ex)
		{
			Log.LogError(ex.Message);
		}

		// Close the progress dialog
		progressForm.Close();
	}
}
