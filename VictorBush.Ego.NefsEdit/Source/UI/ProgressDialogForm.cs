// See LICENSE.txt for license information.

using VictorBush.Ego.NefsEdit.Services;
using VictorBush.Ego.NefsLib.Progress;

namespace VictorBush.Ego.NefsEdit.UI;

/// <summary>
/// Form that monitors progress reporting for the provided progress reporter.
/// </summary>
/// <remarks>
/// <para>
/// Usage: var progressDialog = new ProgressDialogForm(); var progressInfo = progressDialog.ProgressInfo;
///
/// // Show the modal dialog asynchronously (just using ShowDialog() would block // and prevent the async operation from
/// executing) var progressDialogTask = progressDialog.ShowDialogAsync();
///
/// // Do the async operation var task = await doSomethingAsync(progressInfo);
///
/// // Close the dialog progressDialog.Close();
///
/// //.
/// </para>
/// </remarks>
internal partial class ProgressDialogForm : Form
{
	private CancellationTokenSource cancelSource;

	/// <summary>
	/// Initializes a new instance of the <see cref="ProgressDialogForm"/> class.
	/// </summary>
	/// <param name="uiService">The UI service to use.</param>
	public ProgressDialogForm(IUiService uiService)
	{
		InitializeComponent();
		UiService = uiService ?? throw new ArgumentNullException(nameof(uiService));

		/* Setup cancellation */
		this.cancelSource = new CancellationTokenSource();

		/* Create a progress reporter */
		ProgressInfo = new NefsProgress(this.cancelSource.Token);
		ProgressInfo.ProgressChanged += OnProgress;
	}

	/// <summary>
	/// Gets the progress info used by the dialog.
	/// </summary>
	public NefsProgress ProgressInfo { get; }

	private IUiService UiService { get; }

	/// <summary>
	/// Sets the progress bar style.
	/// </summary>
	/// <param name="style">The style.</param>
	public void SetStyle(ProgressBarStyle style)
	{
		this.progressBar.Style = style;
	}

	private void CancelButton_Click(object? sender, EventArgs e)
	{
		/* Set the cancellation token to cancel */
		this.cancelSource.Cancel();
	}

	private void OnProgress(object? sender, NefsProgressEventArgs e)
	{
		/* Constrain the progress percentage to appropriate range */
		var value = Math.Min((int)(e.Progress * 100), this.progressBar.Maximum);
		value = Math.Max(value, 0);

		/* Update the form controls - must do on UI thread */
		UiService.Dispatcher.Invoke(() =>
		{
			this.progressBar.Value = value;
			this.statusLabel.Text = $"{e.Message}\r\n{e.SubMessage}";
		});
	}
}
