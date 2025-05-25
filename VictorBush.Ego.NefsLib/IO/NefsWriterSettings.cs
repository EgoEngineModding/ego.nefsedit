namespace VictorBush.Ego.NefsLib.IO;

public readonly record struct NefsWriterSettings(bool IsEncrypted, bool IsXorEncoded, bool IsLittleEndian);
