// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MDSDK.PerformanceTesting
{
    public class PerformanceComparisonTest
    {
        private readonly int _iterations;

        private readonly int _warmupRuns;

        private readonly string _name;

        private readonly List<PerformanceMeasurementTest> _tests = new();

        public PerformanceComparisonTest(int iterations, int warmupRuns, [CallerMemberName] string name = null)
        {
            _iterations = iterations;
            _warmupRuns = warmupRuns;
            _name = name;
        }

        public void AddTest(string name, Action runAction)
        {
            _tests.Add(new PerformanceMeasurementTest(name, runAction));
        }

        public void Run()
        {
            Console.WriteLine($"Running {_name}");
            foreach (var test in _tests)
            {
                Console.Write("    ");
                test.MeasureAverageRunTime(_iterations, _warmupRuns);
            }
        }
    }
}
