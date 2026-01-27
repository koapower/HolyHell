using ObservableCollections;
using System;

public static class ObservableListExtensions
{
    public static T Find<T>(this ObservableList<T> list, Predicate<T> match)
    {
        if (match == null) throw new ArgumentNullException(nameof(match));

        foreach (var item in list)
        {
            if (match(item)) return item;
        }
        return default;
    }

    public static int RemoveAll<T>(this ObservableList<T> list, Predicate<T> match)
    {
        if (match == null) throw new ArgumentNullException(nameof(match));

        int count = 0;
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (match(list[i]))
            {
                list.RemoveAt(i);
                count++;
            }
        }
        return count;
    }
}