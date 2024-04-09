namespace Lib.Iteraction.Services.Contracts;

/// <summary>
///  Interface IStructureDataConverter for convert System.Data.DataTable to IList<T/>
/// </summary>
public interface IStructureDataConverter
{
    /// <summary>
    ///  Convert System.Data.DataTable to IList<T/> 
    /// </summary>
    /// <param name="dataTable"> System.Data.DataTable</param>
    /// <typeparam name="T"> Object type</typeparam>
    /// <returns></returns>
    IList<T> ToObjects<T>(System.Data.DataTable dataTable) where T : new();
    /// <summary>
    ///  Convert IList<T/> to System.Data.DataTable
    /// </summary>
    /// <param name="objects"> IList<T/></param>
    /// <typeparam name="T"> Object type</typeparam>
    /// <returns></returns>
    System.Data.DataTable ToDataTable<T>(IEnumerable<T> objects);
}
