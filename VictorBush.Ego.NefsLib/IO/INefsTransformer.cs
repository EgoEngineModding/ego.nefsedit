// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.DataSource;
using VictorBush.Ego.NefsLib.Item;
using VictorBush.Ego.NefsLib.Progress;

namespace VictorBush.Ego.NefsLib.IO;

/// <summary>
/// Handles compression and encryption for NeFS item data.
/// </summary>
public interface INefsTransformer
{
	/// <summary>
	/// Reverts a transform on a stream of data (i.e., extract, unencrypt, etc).
	/// </summary>
	/// <param name="input">The input stream to detransform.</param>
	/// <param name="inputOffset">The absolute offset from the beginning of the input stream to detransform.</param>
	/// <param name="output">The output stream to write to.</param>
	/// <param name="outputOffset">The absolute offset from the beginning of the output stream to write to.</param>
	/// <param name="extractedSize">The expected size of the detransformed data.</param>
	/// <param name="chunks">The list of chunks the input stream contains.</param>
	/// <param name="p">Progress info.</param>
	Task DetransformAsync(
		Stream input,
		long inputOffset,
		Stream output,
		long outputOffset,
		uint extractedSize,
		IReadOnlyList<NefsDataChunk> chunks,
		NefsProgress p);

	/// <summary>
	/// Reverts a transform of a chunk of data (i.e., extract, unencrypt, etc).
	/// </summary>
	/// <param name="input">The input stream that contains the chunk.</param>
	/// <param name="output">The output stream to write to.</param>
	/// <param name="chunk">Chunk metadata.</param>
	/// <param name="maxOutputSize">Max size of the output chunk. Any data beyond this size will be discarded.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The size of the detransformed chunk.</returns>
	Task<uint> DetransformChunkAsync(
		Stream input,
		Stream output,
		NefsDataChunk chunk,
		uint maxOutputSize,
		NefsProgress p);

	/// <summary>
	/// Reverts a transform on an input file (i.e., extract, unencrypt, etc).
	/// </summary>
	/// <param name="inputFile">The file path that contains the data to detransform.</param>
	/// <param name="inputOffset">The absolute offset from the beginning of the input file to detransform.</param>
	/// <param name="outputFile">The output file path to write to.</param>
	/// <param name="outputOffset">The absolute offset from the beginning of the output file to write to.</param>
	/// <param name="extractedSize">The expected size of the detransformed data.</param>
	/// <param name="chunks">The list of chunks the input stream contains.</param>
	/// <param name="p">Progress info.</param>
	Task DetransformFileAsync(
		string inputFile,
		long inputOffset,
		string outputFile,
		long outputOffset,
		uint extractedSize,
		IReadOnlyList<NefsDataChunk> chunks,
		NefsProgress p);

	/// <summary>
	/// Reverts a transform on an input data source (i.e., extract, unencrypt, etc).
	/// </summary>
	/// <param name="dataSource">The source containing the data to detransform.</param>
	/// <param name="outputFile">The output file path to write to.</param>
	/// <param name="outputOffset">The absolute offset from the beginning of the output file to write to.</param>
	/// <param name="p">Progress info.</param>
	Task DetransformAsync(
		INefsDataSource dataSource,
		string outputFile,
		long outputOffset,
		NefsProgress p);

	/// <summary>
	/// Transforms data from an input stream and copies it into the output stream.
	/// </summary>
	/// <param name="input">The stream containing data to transform.</param>
	/// <param name="inputOffset">The absolute offset from the beginning of the input stream to transform.</param>
	/// <param name="inputLength">The number of bytes to transform.</param>
	/// <param name="output">The stream to write the trasnformed data to.</param>
	/// <param name="outputOffset">The absolute offset from the beginning of the output stream to write to.</param>
	/// <param name="transform">The transformation to apply to the data.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>Size information for the transformed data.</returns>
	Task<NefsItemSize> TransformAsync(
		Stream input,
		long inputOffset,
		uint inputLength,
		Stream output,
		long outputOffset,
		NefsDataTransform transform,
		NefsProgress p);

	/// <summary>
	/// Transforms a chunk of data from the input stream and copies it to the output stream.
	/// </summary>
	/// <param name="input">The input stream with the chunk of data.</param>
	/// <param name="inputChunkSize">The size of the data to transform.</param>
	/// <param name="output">The output stream to copy the transformed data to.</param>
	/// <param name="transform">The transformation to apply.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The size (in bytes) of the transformed chunk.</returns>
	Task<(uint size, ushort checksum)> TransformChunkAsync(Stream input,
		uint inputChunkSize,
		Stream output,
		NefsDataTransform transform,
		NefsProgress p);

	/// <summary>
	/// Transforms an entire file to a specified output file.
	/// </summary>
	/// <param name="inputFile">The path to the input file.</param>
	/// <param name="outputFile">The output file to write to.</param>
	/// <param name="transform">The transformation to apply to the file.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>Size information for the transformed file.</returns>
	Task<NefsItemSize> TransformFileAsync(
		string inputFile,
		string outputFile,
		NefsDataTransform transform,
		NefsProgress p);

	/// <summary>
	/// Transforms an input data source to a specified output file. The transform used is specified by the data source.
	/// </summary>
	/// <param name="input">The input data to compress.</param>
	/// <param name="outputFile">The output file to write to.</param>
	/// <param name="transform">The transformation to apply to the file.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>Size information for the transformed file.</returns>
	Task<NefsItemSize> TransformFileAsync(
		INefsDataSource input,
		string outputFile,
		NefsDataTransform transform,
		NefsProgress p);

	/// <summary>
	/// Transforms data from a file on disk to an output stream.
	/// </summary>
	/// <param name="inputFile">The path to the input file.</param>
	/// <param name="inputOffset">The absolute offset from the beginning of the input file to transform.</param>
	/// <param name="inputLength">The number of bytes to transform.</param>
	/// <param name="output">The output stream to write to.</param>
	/// <param name="outputOffset">The absolute offset from the beginning of the output stream to write to.</param>
	/// <param name="transform">The transformation to apply to the file.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>Size information for the transformed file.</returns>
	Task<NefsItemSize> TransformFileAsync(
		string inputFile,
		long inputOffset,
		uint inputLength,
		Stream output,
		long outputOffset,
		NefsDataTransform transform,
		NefsProgress p);

	/// <summary>
	/// Transforms data from a file on disk to a specified output file.
	/// </summary>
	/// <param name="inputFile">The path to the input file.</param>
	/// <param name="inputOffset">The absolute offset from the beginning of the input file to transform.</param>
	/// <param name="inputLength">The number of bytes to transform.</param>
	/// <param name="outputFile">The output file to write to.</param>
	/// <param name="outputOffset">The absolute offset from the beginning of the output stream to write to.</param>
	/// <param name="transform">The transformation to apply to the file.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>Size information for the transformed file.</returns>
	Task<NefsItemSize> TransformFileAsync(
		string inputFile,
		long inputOffset,
		uint inputLength,
		string outputFile,
		long outputOffset,
		NefsDataTransform transform,
		NefsProgress p);
}
