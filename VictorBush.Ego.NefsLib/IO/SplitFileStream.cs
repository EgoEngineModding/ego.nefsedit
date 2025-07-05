// See LICENSE.txt for license information.

using System.IO.Abstractions;
using VictorBush.Ego.NefsLib.DataSource;

namespace VictorBush.Ego.NefsLib.IO;

/// <summary>
/// Represents a split volume data stream.
/// </summary>
internal class SplitFileStream : Stream
{
	private readonly NefsVolumeSource volume;
	private readonly IFileSystem fileSystem;
	private readonly FileStreamOptions options;

	private FileSystemStream currentStream;
	private bool isOpen;
	private int fileNumber;
	private long fileStartPosition;

	public override bool CanRead => this.currentStream.CanRead;
	public override bool CanSeek => this.currentStream.CanSeek;
	public override bool CanWrite => this.currentStream.CanWrite;
	public override long Length
	{
		get
		{
			// Get the last file number
			var lastNumber = this.fileNumber;
			var lastPath = this.currentStream.Name;
			for (var i = lastNumber + 1; i <= 999; ++i)
			{
				var pathAtFileNumber = this.volume.GetPathAtFileNumber(i);
				if (!this.fileSystem.File.Exists(pathAtFileNumber))
				{
					break;
				}

				lastNumber = i;
				lastPath = pathAtFileNumber;
			}

			var lastFileStartPosition = lastNumber * this.volume.SplitSize;
			return lastNumber == this.fileNumber
				? this.volume.DataOffset + this.fileStartPosition + this.currentStream.Length
				: this.volume.DataOffset + lastFileStartPosition + this.fileSystem.FileInfo.New(lastPath).Length;
		}
	}

	public override long Position
	{
		get => this.volume.DataOffset + this.fileStartPosition + this.currentStream.Position;
		set
		{
			ArgumentOutOfRangeException.ThrowIfNegative(value);
			Seek(value, SeekOrigin.Begin);
		}
	}

	public SplitFileStream(NefsVolumeSource volume, IFileSystem fileSystem, FileStreamOptions options)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(volume.SplitSize);
		if (options.Mode is not (FileMode.Open or FileMode.OpenOrCreate))
		{
			throw new ArgumentException("Invalid file mode. Only Open or OpenOrCreate is supported.", nameof(options));
		}

		this.volume = volume;
		this.fileSystem = fileSystem;
		this.options = options;

		this.fileNumber = -1;
		this.currentStream = null!;
		UpdateFileStream(0);
		this.isOpen = true;
	}

	private void UpdateFileStream(int number)
	{
		if (this.fileNumber == number)
		{
			return;
		}

		var oldStream = this.currentStream;
		oldStream?.Flush();
		var stream = this.fileSystem.FileStream.New(this.volume.GetPathAtFileNumber(number), this.options);
		this.currentStream = stream;
		this.fileNumber = number;
		this.fileStartPosition = number * this.volume.SplitSize;
		oldStream?.Dispose();
	}

	private async ValueTask UpdateFileStreamAsync(int number)
	{
		if (this.fileNumber == number)
		{
			return;
		}

		var oldStream = this.currentStream;
		if (oldStream is not null)
		{
			await oldStream.FlushAsync().ConfigureAwait(false);
		}

		var stream = this.fileSystem.FileStream.New(this.volume.GetPathAtFileNumber(number), this.options);
		this.currentStream = stream;
		this.fileNumber = number;
		this.fileStartPosition = number * this.volume.SplitSize;
		if (oldStream is not null)
		{
			await oldStream.DisposeAsync().ConfigureAwait(false);
		}
	}

	private bool FileExistsAtNumber(int number)
	{
		return this.fileSystem.File.Exists(this.volume.GetPathAtFileNumber(number));
	}

	public override void Flush()
	{
		this.currentStream.Flush();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		return Read(buffer.AsSpan(offset, count));
	}

	public override int Read(Span<byte> buffer)
	{
		while (true)
		{
			if (this.currentStream.Position + buffer.Length <= this.currentStream.Length)
			{
				return this.currentStream.Read(buffer);
			}

			var bytesRead = this.currentStream.Read(buffer);
			if (bytesRead > 0)
			{
				return bytesRead;
			}

			if (!FileExistsAtNumber(this.fileNumber + 1))
			{
				return 0;
			}

			UpdateFileStream(this.fileNumber + 1);
		}
	}

	public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		return ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
	}

	public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
	{
		if (this.currentStream.Position + buffer.Length <= this.currentStream.Length)
		{
			return this.currentStream.ReadAsync(buffer, cancellationToken);
		}

		return ReadImpl();

		async ValueTask<int> ReadImpl()
		{
			var bytesRead = await this.currentStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
			if (bytesRead > 0)
			{
				return bytesRead;
			}

			if (!FileExistsAtNumber(this.fileNumber + 1))
			{
				return 0;
			}

			await UpdateFileStreamAsync(this.fileNumber + 1).ConfigureAwait(false);
			return await ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
		}
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		if (origin is < SeekOrigin.Begin or > SeekOrigin.End)
		{
			throw new ArgumentException("Invalid seek origin.", nameof(origin));
		}

		EnsureNotClosed();
		EnsureCanSeek();

		var pos = origin switch
		{
			SeekOrigin.Begin => offset,
			SeekOrigin.End => Length + offset,
			_ => Position + offset // SeekOrigin.Current
		};

		ArgumentOutOfRangeException.ThrowIfLessThan(pos, this.volume.DataOffset, nameof(offset));
		var targetFileNumber = this.volume.GetFileNumberAtPosition(pos);
		if (targetFileNumber > this.fileNumber)
		{
			// Seek must run through all files on route to target
			for (var i = this.fileNumber; i < targetFileNumber; ++i)
			{
				this.currentStream.Seek(this.volume.SplitSize, SeekOrigin.Begin);
				UpdateFileStream(i + 1);
			}
		}
		else if (targetFileNumber < this.fileNumber)
		{
			UpdateFileStream(targetFileNumber);
		}

		this.currentStream.Seek(pos - this.fileStartPosition, SeekOrigin.Begin);
		return Position;
	}

	public override void SetLength(long value)
	{
		throw new NotImplementedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		Write(buffer.AsSpan(offset, count));
	}

	public override void Write(ReadOnlySpan<byte> buffer)
	{
		while (true)
		{
			if (this.currentStream.Position + buffer.Length <= this.volume.SplitSize)
			{
				this.currentStream.Write(buffer);
				return;
			}

			var bytesToWrite = Convert.ToInt32(this.volume.SplitSize - this.currentStream.Position);
			this.currentStream.Write(buffer[..bytesToWrite]);
			buffer = buffer[bytesToWrite..];

			UpdateFileStream(this.fileNumber + 1);
		}
	}

	public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	{
		return WriteAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
	}

	public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
	{
		if (this.currentStream.Position + buffer.Length <= this.volume.SplitSize)
		{
			return this.currentStream.WriteAsync(buffer, cancellationToken);
		}

		return WriteImpl();

		async ValueTask WriteImpl()
		{
			var bytesToWrite = Convert.ToInt32(this.volume.SplitSize - this.currentStream.Position);
			await this.currentStream.WriteAsync(buffer[..bytesToWrite], cancellationToken).ConfigureAwait(false);
			buffer = buffer[bytesToWrite..];

			await UpdateFileStreamAsync(this.fileNumber + 1).ConfigureAwait(false);
			await WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (!disposing)
		{
			return;
		}

		this.isOpen = false;
		this.currentStream?.Dispose();
	}

	private void EnsureNotClosed()
	{
		ObjectDisposedException.ThrowIf(!this.isOpen, this);
	}

	private void EnsureCanSeek()
	{
		if (!CanSeek)
		{
			// let underlying stream throw
			this.currentStream.Seek(0,  SeekOrigin.Begin);
		}
	}
}
