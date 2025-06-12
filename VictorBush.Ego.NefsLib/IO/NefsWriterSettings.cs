namespace VictorBush.Ego.NefsLib.IO;

public readonly record struct NefsWriterSettings
{
	public bool IsEncrypted { get; init; }
	public bool IsXorEncoded { get; init; }
	public bool IsLittleEndian { get; init; } = BitConverter.IsLittleEndian;

	public NefsWriterSettings(bool IsEncrypted, bool IsXorEncoded, bool IsLittleEndian)
	{
		this.IsEncrypted = IsEncrypted;
		this.IsXorEncoded = IsXorEncoded;
		this.IsLittleEndian = IsLittleEndian;
	}
}
