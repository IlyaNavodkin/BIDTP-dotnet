namespace Example.Schemas.Dtos;

/// <summary>
///  The element dto in dal layer
/// </summary>
public class ElementDto
{
    /// <summary>
    ///  The name of the element
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    ///  The id of the element
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    ///  The category of the element
    /// </summary>
    public string Category { get; set; }
}