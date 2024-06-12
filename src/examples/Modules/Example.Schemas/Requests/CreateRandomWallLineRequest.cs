using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Example.Schemas.Dtos;

namespace Example.Schemas.Requests;

public class CreateRandomWallLineRequest
{
    public LineDto Line { get; set; }
}
