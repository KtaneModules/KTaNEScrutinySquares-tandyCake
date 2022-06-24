using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

public static class Ut
{

    /// <summary>
    /// Returns the number of times a given <typeparamref name="T"/> <paramref name="item"/> appears in <paramref name="source"/>.
    /// </summary>
    /// <param name="source"> The source from which count is taken from.</param>
    /// <param name="item">The item that is counted.</param>
	public static int CountOf<T>(this IEnumerable<T> source, T item)
    {
        if (source == null)
            throw new ArgumentNullException("source");
        if (item == null)
            throw new ArgumentNullException("item");
        int total = 0;
        foreach (T element in source)
            if (element.Equals(item))
                total++;
        return total;
    }

    /// <summary>
    ///     Returns the entries of <paramref name="source"/> which occur a total of <paramref name="count"/> times within <paramref name="source"/>.
    /// </summary>
    /// <param name="source">The source which is searched.</param>
    /// <param name="count">The target number of occurences.</param>
    public static IEnumerable<T> WhereCountIs<T>(this IEnumerable<T> source, int count)
    {
        if (source == null)
            throw new ArgumentNullException("source");
        Dictionary<T, int> counts = new Dictionary<T, int>();
        foreach (T item in source)
            counts.IncrementDictionary(item);
        foreach (T item in source)
            if (counts[item] == count)
                yield return item;
    }

    /// <summary>
    /// Determines whether or not <paramref name="source"/> has any entries which are equal.
    /// </summary>
    public static bool HasDuplicates<T>(this IEnumerable<T> source)
    {
        if (source == null)
            throw new ArgumentNullException("source");
        HashSet<T> prev = new HashSet<T>();
        foreach (T element in source)
            if (!prev.Add(element))
                return true;
        return false;
    }

    /// <returns>
    ///     Returns a random bool, either true or false.
    /// </returns>
    public static bool RandBool()
    {
        return Rnd.Range(0, 2) == 0;
    }

    /// <summary>
    ///     Returns the item in <paramref name="source"/> which has the highest value when compared using the given <paramref name="comparer"/>.
    ///     If multiple items have this value, the first will be chosen.
    /// </summary>
    /// <param name="source">The source from which values are taken.</param>
    /// <param name="comparer">The function applied to the values.</param>
    public static T MaxBy<T>(this IEnumerable<T> source, Func<T, int> comparer)
    {
        if (source == null)
            throw new ArgumentNullException("source");
        if (source.Count() == 0)
            throw new InvalidOperationException("Max operation cannot be performed on an empty set");
        var ordered = source.OrderBy(comparer);
        return source.First(x => x.Equals(ordered.Last()));
    }

    /// <summary>
    ///     Returns the item in <paramref name="source"/> which has the lowest value when compared using the given <paramref name="comparer"/>.
    ///     If multiple items have this value, the first will be chosen.
    /// </summary>
    /// <param name="source">The source from which items are taken.</param>
    /// <param name="comparer">The function applied to the items to obtain their values.</param>
    public static T MinBy<T>(this IEnumerable<T> source, Func<T, int> comparer)
    {
        if (source == null)
            throw new ArgumentNullException("source");

        if (source.Count() == 0)
            throw new InvalidOperationException("Min operation cannot be performed on an empty set");
        var ordered = source.OrderBy(comparer);
        return source.First(x => x.Equals(ordered.First()));
    }
    /// <summary>
    ///     Returns a modified form of <paramref name="source"/> with all instances of <paramref name="original"/> replaced with <paramref name="substitute"/>.
    /// </summary>
    public static IEnumerable<T> Replace<T>(this IEnumerable<T> source, T original, T substitute)
    {
        if (source == null)
            throw new ArgumentNullException("source");
        foreach (T element in source) 
        { 
            if (element.Equals(original))
                yield return substitute;
            else yield return element;
        }
    }
    /// <summary>
    ///     Returns a modified form of <paramref name="source"/>. All elements which have are a key of <paramref name="replacements"/> will be replaced with their corresponding value in <paramref name="replacements"/>.
    /// </summary>
    public static IEnumerable<T> Replace<T>(this IEnumerable<T> source, IDictionary<T, T> replacements)
    {
        if (source == null || replacements == null)
            throw new ArgumentNullException(source == null ? "source" : "replacements");
        foreach (T element in source)
        {
            if (replacements.ContainsKey(element))
                yield return replacements[element];
            else yield return element;
        }
    }
    /// <summary>
    ///     Returns a modified form of <paramref name="source"/> in which all elements that return true in <paramref name="condition"/> are replaced with <paramref name="replacement"/>.
    /// </summary>
    public static IEnumerable<T> Replace<T>(this IEnumerable<T> source, Func<T, bool> condition, T replacement)
    {
        if (source == null || condition == null)
            throw new ArgumentNullException(source == null ? "source" : "condition");
        foreach (T element in source)
        {
            if (condition(element))
                yield return replacement;
            else yield return element;
        }
    }
    /// <summary>
    ///     Returns a modified form of <paramref name="source"/> in which all elements that return true by <paramref name="condition"/> are replaced by their result when inputted into <paramref name="replacementSelector"/>.
    /// </summary>
    public static IEnumerable<T> Replace<T>(this IEnumerable<T> source, Func<T, bool> condition, Func<T, T> replacementSelector)
    {
        if (source == null || condition == null || replacementSelector == null)
            throw new ArgumentNullException(source == null ? "source" : condition == null ? "condition" : "replacementSelector");
        foreach (T element in source)
        {
            if (condition(element))
                yield return replacementSelector(element);
            else yield return element;
        }
    }
    /// <summary>
    ///     Replaces all elements equal to <paramref name="original"/> in <paramref name="source"/> with <paramref name="substitute"/>.
    /// </summary>
    public static void ReplaceInPlace<TSource, TElem>(this TSource source, TElem original, TElem substitute) where TSource : IList<TElem>
    {
        if (source == null)
            throw new ArgumentNullException("source");
        for (int i = 0; i < source.Count; i++){
            if (source[i].Equals(original))
                source[i] = substitute;
        }
    }
    /// <summary>
    ///     Replaces each element of <paramref name="source"/> which has a key in <paramref name="replacements"/> by its corresponding value in <paramref name="replacements"/>.
    /// </summary>
    public static void ReplaceInPlace<TSource, TElem>(this TSource source, IDictionary<TElem, TElem> replacements) where TSource : IList<TElem>
    {
        if (source == null || replacements == null)
            throw new ArgumentNullException(source == null ? "source" : "replacements");
        for (int i = 0; i < source.Count; i++)
            if (replacements.ContainsKey(source[i]))
                source[i] = replacements[source[i]];
    }
    /// <summary>
    ///     Replaces each element of <paramref name="source"/> which returns true in <paramref name="condition"/> with <paramref name="replacement"/>.
    /// </summary>
    public static void ReplaceInPlace<TSource, TElem>(this TSource source, Func<TElem, bool> condition, TElem replacement) where TSource : IList<TElem>
    {
        if (source == null || condition == null)
            throw new ArgumentNullException(source == null ? "source" : "condition");
        for (int i = 0; i < source.Count; i++)
            if (condition(source[i]))
                source[i] = replacement;
    }
    /// <summary>
    ///     Replaces each element of <paramref name="source"/> which returns true in <paramref name="condition"/> with the result of it being inputted into <paramref name="replacementSelector"/>.
    /// </summary>
    public static void ReplaceInPlace<TSource, TElem>(this TSource source, Func<TElem, bool> condition, Func<TElem, TElem> replacementSelector) where TSource : IList<TElem>
    {
        if (source == null || condition == null)
            throw new ArgumentNullException(source == null ? "source" : condition == null ? "condition" : "replacementSelector");
        for (int i = 0; i < source.Count; i++)
            if (condition(source[i]))
                source[i] = replacementSelector(source[i]);
    }

    /// <summary>
    ///     Removes each element in <paramref name="removeSet"/> from <paramref name="source"/>.
    /// </summary>
    public static void RemoveMany<T>(this List<T> source, IEnumerable<T> removeSet)
    {
        foreach (T item in removeSet)
            source.Remove(item);
    }
    /// <summary>
    ///     Removes each element in <paramref name="removeSet"/> from <paramref name="source"/>.
    /// </summary>
    public static void RemoveMany<T>(this List<T> source, params T[] removeSet)
    {
        source.RemoveMany(removeSet.AsEnumerable());
    }


    /// <summary>
    ///     Swaps the two elements in positions <paramref name="p1"/> and <paramref name="p2"/> in <paramref name="source"/>.
    /// </summary>
    public static void SwapPositions<T>(this IList<T> source, int p1, int p2)
    {
        T temp = source[p1];
        source[p1] = source[p2];
        source[p2] = temp;
    }

    /// <summary>
    ///     Converts an integer number <paramref name="input"/> into a string representing its ordinal form.
    /// </summary>
    /// <param name="input">The number to be converted.</param>
    /// <returns>The number's ordinal form.</returns>
    public static string Ordinal(int input)
    {
        if (input < 0)
            return string.Format("({0})th", input);
        if (input % 100 > 10 && input % 100 < 14 )
            return input + "th";
        else switch (input % 10)
            {
                case 1: return input + "st";
                case 2: return input + "nd";
                case 3: return input + "rd";
                default: return input + "th";
            }
    }

    /// <summary>
    /// Chooses a random item from <paramref name="source"/> which returns true when fed into <paramref name="condition"/>.
    /// </summary>
    /// <param name="source">The source from which items are taken from.</param>
    /// <param name="condition">The function which chosen items must obey.</param>
    /// <exception cref="InvalidOperationException"></exception>
    public static T PickRandom<T>(this IEnumerable<T> source, Func<T, bool> condition)
    {
        if (source == null)
            throw new ArgumentNullException("source");
        if (source.Count() == 0)
            throw new InvalidOperationException("Cannot pick an element from an empty set.");
        IEnumerable<T> filteredsource = source.Where(condition);
        if (filteredsource.Count() == 0)
            throw new InvalidOperationException("Filtered set contains no items.");
        return filteredsource.PickRandom();
    }
    /// <summary>
    /// Chooses a random item from <paramref name="source"/>.
    /// </summary>
    /// <param name="source">The source from which items are taken from.</param>
    /// <exception cref="InvalidOperationException"></exception>
    public static T PickRandom<T>(this IEnumerable<T> source)
    {
        if (source == null)
            throw new ArgumentNullException("source");
        if (source.Count() == 0)
            throw new InvalidOperationException("Cannot pick an element from an empty set.");
        if (source is IList<T>)
        {
            var list = source as IList<T>;
            return list[Rnd.Range(0, list.Count)];
        }
        else return source.ElementAt(Rnd.Range(0, source.Count()));
    }

    /// <summary>
    /// Takes a dictionary and creates a new dictionary in which the keys of the original are the values of the output, and vice versa.
    /// </summary>
    /// <typeparam name="TKey">The type of the original dictionary's keys.</typeparam>
    /// <typeparam name="TVal">The type of the original dictionary's values.</typeparam>
    /// <param name="dict">The dictionary to be inverted.</param>
    /// <returns>A new dictionary with the keys and values swapped.</returns>
    /// <exception cref="InvalidOperationException"></exception>"
    public static Dictionary<TVal, TKey> InvertDictionary<TKey, TVal>(this Dictionary<TKey, TVal> dict)
    {
        if (dict == null)
            throw new ArgumentException("dict");
        var output = new Dictionary<TVal, TKey>();
        if (dict.Values.HasDuplicates())
            throw new InvalidOperationException("Inputted dictionary has multiple entries with the same value, making it impossible to invert.");
        foreach (var pair in dict)
            output.Add(pair.Value, pair.Key);
        return output;
    }

    /// <summary>
    ///     Linearly interpolates from <paramref name="start"/> to <paramref name="end"/> while <paramref name="t"/> is less than 0.5, and then interpolates from <paramref name="end"/> to <paramref name="start"/> while <paramref name="t"/> is greater than 0.5.
    /// </summary>
    /// <param name="start">The initial value to be interpolated. This value will be equal to the output when <paramref name="t"/> == 0 or <paramref name="t"/> == 1.</param>
    /// <param name="end">The end value to be interpolated to. This value will be equal to the output when <paramref name="t"/></param>
    /// <param name="t">The interpolant; within 0.0 to 1.0.</param>
    /// <returns>The result of the interpolation.</returns>
    public static float InOutLerp(float start, float end, float t)
    {
        if (t < 0)
            t = 0;
        if (t > 1)
            t = 1;
        if (t <= 0.5)
            return Mathf.Lerp(start, end, t * 2);
        else return Mathf.Lerp(end, start, t * 2 - 1);
    }

    

    /// <summary>
    ///     Decomposes a rectangular grid into multiple rows and sends each as a log message such that the logging is compatible with the Logfile Analyzer.
    /// </summary>
    /// <param name="displayName">The module's display name, as specified in KMBombModule.ModuleDisplayName"</param>
    /// <param name="moduleIdNumber">A number unique among all instances of <paramref name="displayName"/>.</param>
    /// <param name="grid">The grid to be logged. The grid must be rectangular.</param>
    /// <param name="height">The number of rows in the grid. The output will consist of this many messages.</param>
    /// <param name="width">The number of items per row of the grid.</param>
    /// <param name="separator">A string to separate each entry of the grid</param>
    /// <exception cref="ArgumentOutOfRangeException">Occurs when the stated width or height of the grid is 0 or fewer.</exception>
    /// <exception cref="ArgumentNullException">Occurs when the entered grid is null.</exception>
    public static void LogGrid(string displayName, int moduleIdNumber, object[] grid, int height, int width, string separator = " ")
    {
        if (grid == null)
            throw new ArgumentNullException("Received grid has a null value");
        if (height <= 0)
            throw new ArgumentOutOfRangeException(string.Format("Unexpected value of height, expected a positive value, received {0}.", height));
        if (width <= 0)
            throw new ArgumentOutOfRangeException(string.Format("Unexpected value of width, expected a positive value, received {0}.", width));
        for (int row = 0; row < height; row++)
            Debug.LogFormat("[{0} #{1}] {2}", displayName, moduleIdNumber, string.Join(separator, Enumerable.Range(row * height, width).Select(x => grid[x].ToString()).ToArray()));
    }
    /// <summary>
    ///     Decomposes a rectangular grid of integers into multiple rows and sends each as a log message such that the logging is compatible with the Logfile Analyzer.<br></br>Each integer is indexed into <paramref name="itemSet"/> to obtain a string to represent each integer.
    /// </summary>
    /// <param name="displayName">The module's display name, as specified in KMBombModule.ModuleDisplayName"</param>
    /// <param name="moduleIdNumber">A number unique among all instances of <paramref name="displayName"/>.</param>
    /// <param name="height">The number of rows in the grid. The output will consist of this many messages.</param>
    /// <param name="width">The number of items per row of the grid.</param>
    /// <param name="itemSet">The set of objects to index each entry of <paramref name="grid"/> into.</param>
    /// <param name="separator">A string to separate each entry of the grid</param>
    /// <param name="offset">A value added to each value of <paramref name="grid"/> before it is indexed into <paramref name="itemSet"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">Occurs when the stated width or height of the grid is 0 or fewer.</exception>
    /// <exception cref="ArgumentNullException">Occurs when the entered grid is null.</exception>
    public static void LogIntegerGrid(string displayName, int moduleIdNumber, int[] grid, int height, int width, object[] itemSet, string separator = " ", int offset = 0)
    {
        if (grid == null)
            throw new ArgumentNullException("Received grid has a null value");
        if (height <= 0)
            throw new ArgumentOutOfRangeException(string.Format("Unexpected value of height, expected a positive value, received {0}.", height));
        if (width <= 0)
            throw new ArgumentOutOfRangeException(string.Format("Unexpected value of width, expected a positive value, received {0}.", width));
        grid = grid.Select(x => x + offset).ToArray();
        if (grid.Any(x => x < 0))
            throw new IndexOutOfRangeException(string.Format("Unexpected value {0} found within the grid after applying offset. Values cannot be less than 0.",
                grid.First(x => x < 0)));
        if (grid.Any(x => x >= itemSet.Count()))
            throw new IndexOutOfRangeException(string.Format("Unexpected value {0} found within the grid after applying offset. Values cannot be greater than 1 fewer than the length of charSet ({1}).",
                grid.First(x => x >= itemSet.Count()), itemSet.Count()));
        for (int row = 0; row < height; row++)
            Debug.LogFormat("[{0} #{1}] {2}", displayName, moduleIdNumber, string.Join(separator, Enumerable.Range(row * height, width).Select(x => itemSet[grid[x] + offset].ToString()).ToArray()));
    }
    /// <summary>
    ///     Takes an IEnumerable <paramref name="source"/> and cyclically shifts its entries to the right by <paramref name="rightShift"/>.<br></br>If the value of <paramref name="rightShift"/> is less than 0, <paramref name="source"/> will be shifted to the left by its absolute value. 
    /// </summary>
    /// <typeparam name="T">The type of the source.</typeparam>
    /// <typeparam name="TElem">The type of the source's items.</typeparam>
    /// <param name="source">The source to be shifted.</param>
    /// <param name="rightShift">The number of places to shift the source right by. If this value is negative, the source will be shifted <em>left</em> by its absolute value</param>
    public static T ShiftRight<T, TElem>(this T source, int rightShift) where T : IEnumerable<TElem>
    {
        if (source == null)
            throw new ArgumentNullException("source unexpected null value");
        if (rightShift < 0)
            return (T)source.Skip(-rightShift).Concat(source.Take(-rightShift));
        else return (T)source.Skip(source.Count() - rightShift).Concat(source.Take(rightShift));
    }

    /// <summary>
    ///     Splits an IEnumerable into several smaller IEnumerables with the same size.<br></br>If <paramref name="ignoreThrow"/> is true, the method will allow the last IEnumerable to have a size smaller than the <paramref name="groupSize"/>.
    /// </summary>
    /// <param name="source">The source to be split.</param>
    /// <param name="groupSize">The size of each resulting IEnumerable</param>
    /// <param name="ignoreThrow">If this value is true, a check for all groups being equal will be bypassed, allowing the final group to have a value less than the <paramref name="groupSize"/></param>
    public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int groupSize, bool ignoreThrow = false)
    {
        if (source == null)
            throw new ArgumentNullException("source unexpected null value");
        if (groupSize <= 0)
            throw new ArgumentOutOfRangeException("Group size must be positive, received value " + groupSize);
        if (source.Count() % groupSize != 0 && !ignoreThrow)
            throw new ArgumentException("Group size is not a multiple of the source's count.");
        int fullPartitionCount = (source.Count() / groupSize);
        for (int i = 0; i < fullPartitionCount; i++)
            yield return source.Skip(groupSize * i).Take(groupSize);
        yield return source.Skip(fullPartitionCount * groupSize);
    }
    /// <summary>
    ///     Increments the value in <paramref name="dict"/> with the key of <paramref name="item"/>. If no such key is present, adds it to the dictionary with a value of 1.
    /// </summary>
    public static void IncrementDictionary<TKey>(this Dictionary<TKey, int> dict, TKey item)
    {
        if (dict == null)
            throw new ArgumentNullException("dict unexpected null value");
        if (dict.ContainsKey(item))
            dict[item]++;
        else dict.Add(item, 1);
    }
}
