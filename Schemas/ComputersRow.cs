using System.Collections.ObjectModel;

namespace Schemas;

public class ComputersRow
{
    public  ObservableCollection<Computer> Computers { get; set; }
    public  int Id { get; set; }
}