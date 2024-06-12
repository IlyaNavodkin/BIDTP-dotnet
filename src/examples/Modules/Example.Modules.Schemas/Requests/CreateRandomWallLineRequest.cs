using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Example.Modules.Schemas.Dtos;

namespace Example.Modules.Schemas.Requests;

public class CreateRandomWallLineRequest
{
    public LineDto Line { get; set; }
}
