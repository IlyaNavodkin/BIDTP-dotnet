using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Example.Schemas.Dtos;

namespace Example.Schemas.Requests
{

    public class WallPointsRequest
    {
        public PointDto StartPoint { get; set; }
        public PointDto EndPoint { get; set; }
    }
}
