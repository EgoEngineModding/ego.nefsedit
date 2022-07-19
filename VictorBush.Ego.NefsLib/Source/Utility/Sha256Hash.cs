// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Source.Utility;

/// <summary>
/// A SHA-256 hash value.
/// </summary>
public struct Sha256Hash
{
	/// <summary>
	/// The size of a hash value in bytes.
	/// </summary>
	public const int Size = 0x20;

	/// <summary>
	/// Initializes a new instance of the <see cref="Sha256Hash"/> struct.
	/// </summary>
	/// <param name="value">The value of the hash.</param>
	public Sha256Hash(byte[] value)
	{
		if (value.Length != Size)
		{
			throw new ArgumentException("The SHA-256 hash value must be 32 bytes.");
		}

		Value = value;
	}

	/// <summary>
	/// The value of the hash.
	/// </summary>
	public byte[] Value { get; }

	public static bool operator !=(Sha256Hash a, Sha256Hash b) => !(a == b);

	public static bool operator ==(Sha256Hash a, Sha256Hash b) => a.Equals(b);

	/// <inheritdoc/>
	public override bool Equals(object obj) => obj is Sha256Hash hash && Value.SequenceEqual(hash.Value);

	/// <inheritdoc/>
	public override int GetHashCode() => Value.GetHashCode();

	/// <inheritdoc/>
	public override string ToString() => string.Join("", Value.Select(b => $"{b:X}"));
}
