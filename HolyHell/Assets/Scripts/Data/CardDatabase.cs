using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Sort fields available in deck-building card database.
/// </summary>
public enum SortField
{
    Id,
    Name,
    AngelValue,
    DemonValue,
    Rarity,
    Cost
}

/// <summary>
/// Provides search, filter, and sort operations over the card table.
/// Does not mutate the underlying table data.
/// </summary>
public class CardDatabase
{
    private readonly ITableManager tableManager;

    public CardDatabase(ITableManager tableManager)
    {
        this.tableManager = tableManager;
    }

    // Returns all cards from the table
    public IReadOnlyList<CardRow> GetAll()
    {
        return tableManager.GetTable<CardRow>().DataList;
    }

    /// <summary>
    /// Returns cards whose DisplayName or Description contains the query string
    /// (case-insensitive). Empty/null query returns all cards.
    /// </summary>
    public List<CardRow> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return GetAll().ToList();

        string lower = query.ToLowerInvariant();
        return GetAll()
            .Where(c =>
                (c.DisplayName != null && c.DisplayName.ToLowerInvariant().Contains(lower)) ||
                (c.Description != null && c.Description.ToLowerInvariant().Contains(lower)))
            .ToList();
    }

    /// <summary>
    /// Returns cards that pass the given filter. Null or empty filter returns all cards.
    /// </summary>
    public List<CardRow> Filter(DeckBuildFilter filter)
    {
        if (filter == null || filter.IsEmpty)
            return GetAll().ToList();

        return GetAll().Where(filter.Matches).ToList();
    }

    /// <summary>
    /// Returns cards matching both search query and filter.
    /// </summary>
    public List<CardRow> SearchAndFilter(string query, DeckBuildFilter filter)
    {
        IEnumerable<CardRow> result = GetAll();

        if (!string.IsNullOrWhiteSpace(query))
        {
            string lower = query.ToLowerInvariant();
            result = result.Where(c =>
                (c.DisplayName != null && c.DisplayName.ToLowerInvariant().Contains(lower)) ||
                (c.Description != null && c.Description.ToLowerInvariant().Contains(lower)));
        }

        if (filter != null && !filter.IsEmpty)
            result = result.Where(filter.Matches);

        return result.ToList();
    }

    /// <summary>
    /// Returns a new sorted list without modifying the source collection.
    /// </summary>
    public List<CardRow> Sort(IEnumerable<CardRow> cards, SortField field, bool ascending)
    {
        IOrderedEnumerable<CardRow> ordered = field switch
        {
            SortField.Id          => ascending ? cards.OrderBy(c => c.Id)                  : cards.OrderByDescending(c => c.Id),
            SortField.Name        => ascending ? cards.OrderBy(c => c.DisplayName)          : cards.OrderByDescending(c => c.DisplayName),
            SortField.AngelValue  => ascending ? cards.OrderBy(c => c.AngelGaugeIncrease)   : cards.OrderByDescending(c => c.AngelGaugeIncrease),
            SortField.DemonValue  => ascending ? cards.OrderBy(c => c.DemonGaugeIncrease)   : cards.OrderByDescending(c => c.DemonGaugeIncrease),
            SortField.Rarity      => ascending ? cards.OrderBy(c => c.Rarity)               : cards.OrderByDescending(c => c.Rarity),
            SortField.Cost        => ascending ? cards.OrderBy(c => c.ActionCost)            : cards.OrderByDescending(c => c.ActionCost),
            _                     => ascending ? cards.OrderBy(c => c.Id)                   : cards.OrderByDescending(c => c.Id)
        };

        return ordered.ToList();
    }
}
