namespace Example.Modules.Schemas.Dtos
{
    public class LineDto
    {
        public string ElementId { get; set; }
        public string Guid { get; set; }
        public PointDto StartPoint { get; set; }
        public PointDto EndPoint { get; set; }
    }
}
