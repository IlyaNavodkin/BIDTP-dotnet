using Example.Schemas.Dtos;

namespace Example.Schemas.Requests;

/// <summary>
///  Delete element request
/// </summary>
public class DeleteElementRequest
{
    /// <summary>
    ///  Element for delete
    /// </summary>
    public ElementDto Element { get; set; }
}