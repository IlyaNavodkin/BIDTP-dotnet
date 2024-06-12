using Example.Schemas.Dtos;

namespace Example.Schemas.Requests;

public class DeleteElementRequest
{
    public ElementDto Element { get; set; }
}