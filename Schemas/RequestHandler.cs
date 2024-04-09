using System.Collections.ObjectModel;
using System.Data;
using System.Text.Json;
using Lib;
using Lib.Iteraction.Request;
using Lib.Iteraction.RequestServer;
using Lib.Iteraction.Response;

namespace Schemas;

public class RequestHandler : IRequestHandler
{
    public async Task<ResponseBase> ServeRequest(RequestBase request)
    {
        if (request.Headers["Route"] != "getNewComponents") return new Response(StatusCode.ServerError);


        var dataTable = new DataTable();
        
        var computer1 = new Computer { Id = 1, Name = "MSI CAL" };
        var computer2 = new Computer { Id = 2, Name = "GOGABYTE ZERO" };
        var computers = new ObservableCollection<Computer>();
        
        computers.Add(computer1);
        computers.Add(computer2);
        
        var computer4 = new Computer { Id = 228, Name = "BOBER" };
        var computer5 = new Computer { Id = 1488, Name = "OREO" };
        var computers2 = new ObservableCollection<Computer>();
        
        computers.Add(computer4);
        computers.Add(computer5);
        
        dataTable.Columns.Add("Id", typeof(int));
        dataTable.Columns.Add("Computers", typeof(ObservableCollection<Computer>));
        
        dataTable.Rows.Add(1, computers );
        dataTable.Rows.Add(2, computers2 );
        
        
        var result = new Result
        {
            Data = new List<Component>
            {
                new Component { Id = 1, Name = "Rtx 4070" },
                new Component { Id = 2, Name = "Rtx 4080" },
            },
            DataTableDiscontComputers = dataTable
        };
        
        
        var response = new Response(StatusCode.Success)
        {
            Body = JsonSerializer.Serialize(result)
        };
        
        return response;
        
    }
}