using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Threading;

using Newtonsoft.Json;

namespace SimpleCpuBench
{
	static class Program
	{
		static void Main(string[] args)
		{
			var curProcess = Process.GetCurrentProcess();
			curProcess.PriorityClass = ProcessPriorityClass.RealTime;
			Thread.CurrentThread.Priority = ThreadPriority.Highest;

			var text1 = File.ReadAllText(@"Data\sqlite3.c");
			Measure("foreach", 10_000, () => ReadText1(text1));
			Measure("for", 10_000, () => ReadText2(text1));

			var text2 = File.ReadAllText(@"Data\data.json");
			Measure("json", 10_000, () => ReadJson(text2));
		}

		static object ReadText1(string text)
		{
			var res = char.MinValue;
			foreach (var ch in text)
			{
				res ^= ch;
			}
			return res;
		}

		static object ReadText2(string text)
		{
			var res = char.MinValue;
			var i = 0;
			var length = text.Length;
			while (i != length)
			{
				var ch = text[i++];
				res ^= ch;
			}
			return res;
		}

		static object ReadJson(string text)
		{
			var res = JsonConvert.DeserializeObject(text);
			return res;
		}

		static void Measure(string name, int repeatCount, Func<object> action)
		{
			for (var i = 0; i < 10; i++)
			{
				_temp = action();
			}

			var measures = new List<double>();

			for (var i = 0; i < repeatCount; i++)
			{
				var watch = Stopwatch.StartNew();
				_temp = action();
				watch.Stop();
				measures.Add(watch.Elapsed.TotalSeconds);

				CollectAll();
			}

			measures.Sort();

			Console.WriteLine($"----- {name} -----");

			var totalTime = measures.Sum();
			measures.Sort();
			var medianTime = measures[measures.Count / 2];

			Console.WriteLine($"Minimal time: {measures[0]:0.000000} secs");
			Console.WriteLine($"Median time: {medianTime:0.000000} secs");
			Console.WriteLine($"Maximum time: {measures.Last():0.000000} secs");
			Console.WriteLine($"Total time: {totalTime:0.000000} secs");

			Console.WriteLine();
		}

		static void CollectAll()
		{
			GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
		}

		static volatile object _temp;
	}
}
