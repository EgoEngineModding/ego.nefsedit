// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Item;
using VictorBush.Ego.NefsLib.Progress;

namespace VictorBush.Ego.NefsLib.Header.Builder;

internal abstract class NefsHeaderBuilder
{
	private static readonly Dictionary<NefsVersion, NefsHeaderBuilder> Instances = new();

	public static NefsHeaderBuilder Get(NefsVersion version)
	{
		if (Instances.TryGetValue(version, out var inst))
		{
			return inst;
		}

		inst = version switch
		{
			NefsVersion.Version151 => new NefsHeaderBuilder151(),
			NefsVersion.Version160 => new NefsHeaderBuilder160(),
			NefsVersion.Version200 => new NefsHeaderBuilder200(),
			_ => throw new NotImplementedException($"Support for {version.ToPrettyString()} is not implemented.")
		};

		Instances.Add(version, inst);
		return inst;
	}

	public abstract INefsHeader Build(INefsHeader sourceHeader, NefsItemList items, NefsProgress p);

	public abstract uint ComputeDataOffset(INefsHeader sourceHeader, NefsItemList items);
}

/// <inheritdoc />
internal abstract class NefsHeaderBuilder<T> : NefsHeaderBuilder
	where T : INefsHeader
{
	/// <inheritdoc />
	public override uint ComputeDataOffset(INefsHeader sourceHeader, NefsItemList items)
	{
		return ComputeDataOffset(sourceHeader.As<T>(), items);
	}

	internal abstract uint ComputeDataOffset(T sourceHeader, NefsItemList items);

	/// <inheritdoc />
	public override INefsHeader Build(INefsHeader sourceHeader, NefsItemList items, NefsProgress p)
	{

		return Build(sourceHeader.As<T>(), items, p);
	}

	internal abstract T Build(T sourceHeader, NefsItemList items, NefsProgress p);
}
