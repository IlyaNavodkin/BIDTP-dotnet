using Example.Schemas.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Client.Services
{
    public class RandomService
    {
        private static Random _random = new Random();

        public static LineDto GenerateRandomLine(double maxDistance)
        {
            var point1 = new PointDto
            {
                X = (_random.NextDouble() * 2 - 1) * maxDistance,
                Y = (_random.NextDouble() * 2 - 1) * maxDistance
            };

            var point2 = new PointDto
            {
                X = (_random.NextDouble() * 2 - 1) * maxDistance,
                Y = (_random.NextDouble() * 2 - 1) * maxDistance
            };

            return new LineDto
            {
                StartPoint = point1,
                EndPoint = point2
            };
        }

        public static double CalculateDistance(PointDto point1, PointDto point2)
        {
            return Math.Sqrt(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2));
        }
    }
}
