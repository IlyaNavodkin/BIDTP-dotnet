using System.Data;
using BIDTP.Dotnet.Core.Iteraction.Dtos;
using BIDTP.Dotnet.Core.Iteraction.Enums;
using BIDTP.Dotnet.Core.Iteraction.Providers;

namespace Example.Server.Domain.DataTable;

public class DataTableController
{
    public static Task MutateUrTable( Context context )
    {
        var request = context.Request;
        
        var table = request.GetBody<System.Data.DataTable>();
        
        foreach (DataRow row in table.Rows)
        {
            row[0] = "MutateName";
            row[1] = 2;
            row[2] = true;
        }
        
        if (table.Columns.Count >= 3)
        {
            table.Columns.RemoveAt(2); 
        }
        
        var response = new Response( StatusCode.Success );
        
        response.SetBody(table);
        
        context.Response = response;

        return Task.CompletedTask;

    }
}