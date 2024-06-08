﻿using System;
using Example.Schemas.Dtos;
using Example.Schemas.Requests;

namespace Example.Services.Generate
{
    public class RandomPointGenerateService
    {
        private static Random _random = new Random();

        public static WallLineRequest GeneratePointsWithMinDistance(double minDistance = 0.3, double maxDistance = 6)
        {
            PointDto startPoint;
            PointDto endPoint;

            double distance;
            do
            {
                startPoint = GenerateRandomPoint(maxDistance);
                endPoint = GenerateRandomPoint(maxDistance);

                distance = CalculateDistance(startPoint, endPoint);
            } while (distance < minDistance);

            return new WallLineRequest { StartPoint = startPoint, EndPoint = endPoint };
        }

        private static PointDto GenerateRandomPoint(double maxDistance)
        {
            return new PointDto
            {
                X = (_random.NextDouble() * 2 - 1) * maxDistance,
                Y = (_random.NextDouble() * 2 - 1) * maxDistance
            };
        }

        private static double CalculateDistance(PointDto point1, PointDto point2)
        {
            return Math.Sqrt(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2));
        }
    }

}