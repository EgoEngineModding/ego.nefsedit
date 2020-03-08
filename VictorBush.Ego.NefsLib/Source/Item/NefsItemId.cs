// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Item
{
    using System;

    /// <summary>
    /// A unique identifier for an item in a NeFS archive.
    /// </summary>
    public struct NefsItemId : IComparable<NefsItemId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NefsItemId"/> struct.
        /// </summary>
        /// <param name="value">The value of the id.</param>
        public NefsItemId(UInt32 value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets or sets the value of this id.
        /// </summary>
        public UInt32 Value { get; }

        /// <summary>
        /// Checks if two ids are not equal.
        /// </summary>
        /// <param name="a">The first id.</param>
        /// <param name="b">The second id.</param>
        /// <returns>True if the ids are not equal.</returns>
        public static bool operator !=(NefsItemId a, NefsItemId b) => !(a == b);

        /// <summary>
        /// Checks if two ids are equal.
        /// </summary>
        /// <param name="a">The first id.</param>
        /// <param name="b">The second id.</param>
        /// <returns>True if the ids are equal.</returns>
        public static bool operator ==(NefsItemId a, NefsItemId b) => a.Value == b.Value;

        /// <inheritdoc/>
        public Int32 CompareTo(NefsItemId other)
        {
            return this.Value.CompareTo(other.Value);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is NefsItemId id && id == this;

        /// <inheritdoc/>
        public override int GetHashCode() => this.Value.GetHashCode();
    }
}
