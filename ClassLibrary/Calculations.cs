using System;
using System.Collections.Generic;

namespace ClassLibrary
{
    public class Calculations
    {
        /// Возвращает массив свободных временных слотов заданной длительности.
        public static string[] AvailablePeriods(
            TimeSpan[] startTimes,
            int[] durations,
            TimeSpan beginWorkingTime,
            TimeSpan endWorkingTime,
            int consultationTime)
        {
            /// Валидация входных данных 
            if (consultationTime <= 0)
                return Array.Empty<string>();

            if (beginWorkingTime >= endWorkingTime)
                return Array.Empty<string>();

            int beginMin = (int)beginWorkingTime.TotalMinutes;
            int endMin = (int)endWorkingTime.TotalMinutes;

            if (endMin - beginMin < consultationTime)
                return Array.Empty<string>();

            /// Защита от null или несовпадающих массивов
            if (startTimes == null || durations == null)
                return Array.Empty<string>();

            int pairs = Math.Min(startTimes.Length, durations.Length);
            if (pairs == 0)
            {
                /// Весь день свободен – сразу нарезка слотов
                return SliceFreeInterval(beginMin, endMin, consultationTime);
            }

            /// Формирование занятых интервалов (обрезанных по рабочему дню)
            var busyIntervals = new List<(int start, int end)>();
            for (int i = 0; i < pairs; i++)
            {
                int start = (int)startTimes[i].TotalMinutes;
                int end = start + durations[i];

                /// Пересечение с рабочим днём
                int clippedStart = Math.Max(start, beginMin);
                int clippedEnd = Math.Min(end, endMin);
                if (clippedStart < clippedEnd)
                    busyIntervals.Add((clippedStart, clippedEnd));
            }

            /// Слияние пересекающихся занятых интервалов 
            busyIntervals.Sort((a, b) => a.start.CompareTo(b.start));
            var merged = new List<(int start, int end)>();
            foreach (var interval in busyIntervals)
            {
                if (merged.Count == 0 || interval.start > merged[merged.Count - 1].end)
                    merged.Add(interval);
                else
                {
                    var last = merged[merged.Count - 1];
                    merged[merged.Count - 1] = (last.start, Math.Max(last.end, interval.end));
                }
            }

            /// Построение свободных отрезков между занятыми 
            var freeIntervals = new List<(int start, int end)>();
            int current = beginMin;
            foreach (var (mStart, mEnd) in merged)
            {
                if (current < mStart)
                    freeIntervals.Add((current, mStart));
                current = Math.Max(current, mEnd);
            }
            if (current < endMin)
                freeIntervals.Add((current, endMin));

            /// Нарезка свободных отрезков на слоты 
            var result = new List<string>();
            foreach (var (fs, fe) in freeIntervals)
            {
                result.AddRange(SliceFreeInterval(fs, fe, consultationTime));
            }
            return result.ToArray();
        }


        /// Нарезает непрерывный интервал [start, end) на слоты длительностью slotMinutes.
        private static string[] SliceFreeInterval(int startMin, int endMin, int slotMinutes)
        {
            var slots = new List<string>();
            int pos = startMin;
            while (pos + slotMinutes <= endMin)
            {
                slots.Add(FormatTimeSlot(pos, pos + slotMinutes));
                pos += slotMinutes;
            }
            return slots.ToArray();
        }

        private static string FormatTimeSlot(int startMin, int endMin)
        {
            string ToTime(int m) => $"{m / 60:D2}:{m % 60:D2}";
            return $"{ToTime(startMin)}-{ToTime(endMin)}";
        }
    }
}