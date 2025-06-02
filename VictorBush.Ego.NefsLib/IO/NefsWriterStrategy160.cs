// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Header.Version160;
using VictorBush.Ego.NefsLib.Progress;

namespace VictorBush.Ego.NefsLib.IO;

internal class NefsWriterStrategy160 : NefsWriterStrategy160Base<Nefs160Header>
{
	/// <inheritdoc />
	protected override Task WriteHeaderAsync(EndianBinaryWriter writer, Nefs160Header header, long primaryOffset, NefsProgress p)
	{
		throw new NotImplementedException();
	}
}
