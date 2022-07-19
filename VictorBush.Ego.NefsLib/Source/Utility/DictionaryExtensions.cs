// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Utility;

/// <summary>
/// Extension methods for dictionaries.
/// </summary>
public static class DictionaryExtensions
{
	/// <summary>
	/// Gets the value from a dictionary or returns the default value if doesn't exist.
	/// </summary>
	/// <typeparam name="TKey">The type of key.</typeparam>
	/// <typeparam name="TValue">The type of value.</typeparam>
	/// <param name="dictionary">The dictionary.</param>
	/// <param name="key">The key.</param>
	/// <returns>The value.</returns>
	public static TValue? GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
	{
		if (dictionary.TryGetValue(key, out var val))
		{
			return val;
		}

		return default;
	}
}
