namespace Manbaa.Domain.Common;

/// <summary>
/// An entity that is searched by free text. <see cref="SearchText" /> holds a normalized copy of the searchable fields
/// and is rebuilt automatically whenever the entity is saved.
/// </summary>
public interface ISearchable
{
    string SearchText { get; set; }

    string BuildSearchText();
}
