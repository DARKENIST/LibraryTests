using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class Calculations
    {
        public static string[] AvailablePeriods(TimeSpan[] startTimes, int[] durations, TimeSpan beginWorkingTime, TimeSpan endWorkingTime, int consultationTime)
        {
            // Защита от некорректных входных данных
            if (consultationTime <= 0)
                return Array.Empty<string>();

            int beginMin = (int)beginWorkingTime.TotalMinutes;
            int endMin = (int)endWorkingTime.TotalMinutes;

            if (beginMin >= endMin || consultationTime > (endMin - beginMin))
                return Array.Empty<string>();

            // Формируем занятые интервалы, обрезанные рабочим днём
            var busyIntervals = new List<(int start, int end)>();
            for (int i = 0; i < startTimes.Length; i++)
            {
                int start = (int)startTimes[i].TotalMinutes;
                int end = start + durations[i];

                // Учитываем только пересечение с рабочим днём
                if (end > beginMin && start < endMin)
                {
                    int clippedStart = Math.Max(start, beginMin);
                    int clippedEnd = Math.Min(end, endMin);
                    busyIntervals.Add((clippedStart, clippedEnd));
                }
            }

            // Свободные непрерывные отрезки
            List<(int start, int end)> freePeriods;

            if (busyIntervals.Count == 0)
            {
                freePeriods = new List<(int, int)> { (beginMin, endMin) };
            }
            else
            {
                // Слияние пересекающихся интервалов
                busyIntervals.Sort((a, b) => a.start.CompareTo(b.start));
                var merged = new List<(int start, int end)> { busyIntervals[0] };

                for (int i = 1; i < busyIntervals.Count; i++)
                {
                    var last = merged[merged.Count - 1];
                    var curr = busyIntervals[i];
                    if (curr.start <= last.end)
                        merged[merged.Count - 1] = (last.start, Math.Max(last.end, curr.end));
                    else
                        merged.Add(curr);
                }

                // Построение свободных отрезков между занятыми
                freePeriods = new List<(int start, int end)>();
                int cur = beginMin;
                foreach (var (mStart, mEnd) in merged)
                {
                    if (cur < mStart)
                        freePeriods.Add((cur, mStart));
                    cur = Math.Max(cur, mEnd);
                }
                if (cur < endMin)
                    freePeriods.Add((cur, endMin));
            }

            // Нарезка свободных отрезков на окна длительностью consultationTime
            var slots = new List<string>();
            foreach (var (fs, fe) in freePeriods)
            {
                int pos = fs;
                while (pos + consultationTime <= fe)
                {
                    int slotEnd = pos + consultationTime;
                    slots.Add(FormatSlot(pos, slotEnd));
                    pos += consultationTime;
                }
            }

            return slots.ToArray();
        }
        
        private static string FormatSlot(int startMin, int endMin)
        {
            string ToTime(int minutes) =>
                $"{minutes / 60:D2}:{minutes % 60:D2}";

            return $"{ToTime(startMin)}-{ToTime(endMin)}";
        }
    }
}
