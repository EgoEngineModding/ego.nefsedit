// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Header.Version160;
using VictorBush.Ego.NefsLib.Progress;

namespace VictorBush.Ego.NefsLib.IO;

internal class NefsWriterStrategy160 : NefsWriterStrategy160Base<NefsHeader160>
{
	/// <inheritdoc />
	protected override Task WriteHeaderAsync(EndianBinaryWriter writer, NefsHeader160 header, long primaryOffset, NefsProgress p)
	{
		throw new NotImplementedException();
	}
}
