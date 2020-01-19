﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using System.Diagnostics;
using System.Collections.Specialized;
using System.IO;
using TimetablingWPF.StructureClasses;

namespace TimetablingWPF
{
    public static class TimetableStructure
    {
        public static void SetData(IList<TimetableStructureWeek> weeks)
        {
            Weeks = weeks;
            TotalSchedulable = 0;
            foreach (TimetableStructureWeek week in Weeks)
            {
                TotalSchedulable += week.TotalSchedulable;
            }
        }
        public static IList<TimetableStructureWeek> Weeks { get; private set; }
        public static int TotalSchedulable { get; private set; }
    }
    public class YearGroup
    {
        public YearGroup(string year)
        {
            Year = year;
        }
        private bool Committed;
        public InternalObservableCollection<Form> Forms { get; private set; } = new InternalObservableCollection<Form>();
        public string Year { get; set; }
        public int StorageIndex { get; set; }
        public void Commit()
        {
            if (!Committed)
            {
                ((IList)Application.Current.Properties[typeof(YearGroup)]).Add(this);
                Committed = true;
            }
        }
        public override string ToString()
        {
            return $"Year {Year}";
        }

        public override bool Equals(object obj)
        {
            return obj is YearGroup yg && yg.Year == Year;
        }

        public override int GetHashCode()
        {
            return Year.GetHashCode();
        }
        public static bool operator ==(YearGroup left, YearGroup right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(YearGroup left, YearGroup right)
        {
            return !left.Equals(right);
        }
    }
}
namespace TimetablingWPF.StructureClasses{
    public class TimetableStructureWeek
    {
        public TimetableStructureWeek(string name, IList<string> days, IList<string> periods, IList<int> unschedulable)
        {
            Name = name;
            DayNames = days;
            PeriodNames = periods;
            TotalPeriods = periods.Count * days.Count;
            TotalSchedulable = TotalPeriods;
            UnavailablePeriods = unschedulable;
            for (int i = 0; i < DayNames.Count; i++)
            {
                DaySchedulable.Add(0);
            }
            for (int i = 0; i < TotalPeriods; i++)
            {
                AllPeriods.Add(true);
            }
            foreach (int index in unschedulable)
            {
                AllPeriods[index] = false;
                TotalSchedulable--;
                DaySchedulable[index / PeriodNames.Count]++;
            }
        }
        public bool PeriodIsSchedulable(int day, int period)
        {
            return AllPeriods[IndexesToPeriodNum(day, period, PeriodNames.Count)];
        }
        public bool DayIsSchedulable(int day)
        {
            return DaySchedulable[day] != PeriodNames.Count;
        }
        public static int IndexesToPeriodNum(int day, int period, int numPeriods)
        {
            return day * numPeriods + period;
        }
        public string Name { get; }
        public IList<string> DayNames { get; }
        public IList<string> PeriodNames { get; }
        public IList<int> UnavailablePeriods { get; }
        public IList<bool> AllPeriods { get; } = new List<bool>();
        private IList<int> DaySchedulable { get; } = new List<int>();
        public int TotalPeriods { get; }
        public int TotalSchedulable { get; }
    }
}
