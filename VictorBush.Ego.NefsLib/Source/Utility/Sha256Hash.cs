// See LICENSE.txt for license information.

using System.Runtime.CompilerServices;

namespace VictorBush.Ego.NefsLib.Utility;

/// <summary>
/// A SHA-256 hash value.
/// </summary>
[InlineArray(Size)]
public struct Sha256Hash : IEquatable<Sha256Hash>
{
	/// <summary>
	/// The size of a hash value in bytes.
	/// </summary>
	public const int Size = 0x20;

	private byte element;

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

		value.CopyTo(this);
	}

	public static bool operator !=(Sha256Hash left, Sha256Hash right)
	{
		return !(left == right);
	}

	public static bool operator ==(Sha256Hash left, Sha256Hash right)
	{
		return left.Equals(right);
	}

	/// <inheritdoc />
	public bool Equals(Sha256Hash other)
	{
		ReadOnlySpan<byte> self = this;
		return self.SequenceEqual(other);
	}

	/// <inheritdoc />
	public override bool Equals(object? obj)
	{
		return obj is Sha256Hash other && Equals(other);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		var h = new HashCode();
		h.AddBytes(this);
		return h.ToHashCode();
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return Convert.ToHexString(this);
	}
}
