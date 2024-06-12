using Example.Modules.Schemas.Dtos;

namespace Example.Modules.Server.Domain.Elements.Repositories;

/// <summary>
///  The element repository for the server
/// </summary>
public class ElementRepository
{
    private readonly ICollection<ElementDto> _elements = new List<ElementDto>
    {
        new ElementDto
        {
            Id = 1,
            Name = "Труба - стальная",
            Category = "Трубы"
        },
        new ElementDto
        {
            Id = 2,
            Name = "Стена",
            Category = "Стены"
        },
        new ElementDto
        {
            Id = 3,
            Name = "Труба - стальная",
            Category = "Трубы"
        },
        new ElementDto
        {
            Id = 4,
            Name = "Труба - медь",
            Category = "Трубы"
        }
    };
    
    /// <summary>
    ///  Get all elements
    /// </summary>
    /// <returns> The elements. </returns>
    public ICollection<ElementDto> GetElements()
    {
        var elements = _elements;

        return elements;
    }

    /// <summary>
    ///  Get elements by category
    /// </summary>
    /// <param name="category"> The category. </param>
    /// <returns> The elements. </returns>
    public ICollection<ElementDto> GetElementsByCategory(string category)
    {
        if(string.IsNullOrEmpty(category)) 
            return _elements;
        
        var lowerCategory = category.ToLower();
            
        var result = _elements
            .Where(x => x.Category.ToLower().Contains(lowerCategory))
            .ToList();
        
        return result;
    }
}