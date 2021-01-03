// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Diagnostics;

namespace MDSDK.PerformanceTesting
{
    public sealed class PerformanceMeasurementTest
    {
        private readonly string _name;

        private readonly Action _runAction;

        public PerformanceMeasurementTest(string name, Action runAction)
        {
            _name = name;
            _runAction = runAction;
        }

        public TimeSpan MeasureAverageRunTime(int iterations, int warmupRuns)
        {
            Console.Write($"Running {_name}...");
            
            for (var i = 0; i < warmupRuns; i++)
            {
                _runAction.Invoke();
            }

            var stopwatch = Stopwatch.StartNew();

            for (var i = 0; i < iterations; i++)
            {
                _runAction.Invoke();
            }

            var averageRunTime = stopwatch.Elapsed / iterations;
            
            Console.WriteLine($"average run time = {averageRunTime.TotalMilliseconds} ms");

            return averageRunTime;
        }
    }
}
