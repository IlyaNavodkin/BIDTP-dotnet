using System.Data;
using Lib.Iteraction.Services.Contracts;

namespace Lib.Iteraction.Services;

    /// <inheritdoc />
    public class StructureDataConverter : IStructureDataConverter
    {
        /// <inheritdoc />
        public IList<T> ToObjects<T>(System.Data.DataTable dataTable) 
            where T : new()
        {
            var resultList = new List<T>();

            var properties = typeof(T).GetProperties();

            foreach (DataRow row in dataTable.Rows)
            {
                var obj = new T();

                foreach (var property in properties)
                {
                    var column = dataTable.Columns[property.Name];
                    
                    if (column == null) continue;
                    
                    var columnDataType = column.DataType;
                        
                    var rowValue = row[property.Name];

                    if (rowValue == DBNull.Value) continue;
                    
                    if (property.PropertyType.IsEnum)
                    {
                        var enumValue = Enum.Parse(property.PropertyType, rowValue.ToString());
                        property.SetValue(obj, enumValue);
                    }
                    else
                    {
                        var value = Convert.ChangeType(rowValue, columnDataType);
                        property.SetValue(obj, value);
                    }
                }
                
                resultList.Add(obj);
            }

            return resultList;
        }

        /// <inheritdoc />
        public System.Data.DataTable ToDataTable<T>(IEnumerable<T> objects)
        {
            var dataTable = new System.Data.DataTable();

            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                dataTable.Columns.Add(property.Name, 
                    Nullable.GetUnderlyingType(property.PropertyType) ??
                    property.PropertyType);
            }

            foreach (var obj in objects)
            {
                var newRow = dataTable.NewRow();
                foreach (var property in properties)
                {
                    var value = property.GetValue(obj);

                    newRow[property.Name] = value ?? DBNull.Value;
                }
                dataTable.Rows.Add(newRow);
            }

            return dataTable;
        }
    }
