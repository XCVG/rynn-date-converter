﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RynnDateConverter
{
    public struct EarthFormatDate
    {
        public int Year;
        public int Month;
        public int Day;
        public int Hour;
        public int Minute;
        public int Second;

        public EarthFormatDate(int year, int month, int day, int hour, int minute, int second)
        {
            Year = year;
            Month = month;
            Day = day;
            Hour = hour;
            Minute = minute;
            Second = second;
        }
    }

    public struct RynnFormatDate
    {
        public int Era;
        public int Year;
        public int Month;
        public int Day;
        public int Hour;
        public int Minute;
        public int Second;
        public bool OnEraBoundary;

        //check bounds? why bother!
        public RynnFormatDate(int era, int year, int month, int day, int hour, int minute, int second)
        {
            Era = era;
            Year = year;
            Month = month;
            Day = day;
            Hour = hour;
            Minute = minute;
            Second = second;
            OnEraBoundary = false;
        }

        public RynnFormatDate(int era, int year, int month, int day, int hour, int minute, int second, bool onEraBoundary)
        {
            Era = era;
            Year = year;
            Month = month;
            Day = day;
            Hour = hour;
            Minute = minute;
            Second = second;
            OnEraBoundary = onEraBoundary;
        }
    }

    public sealed class MultiformatDate //fuck you if you want to extend it
    {
        private const int RYNN_DAYS_PER_YEAR = 350;
        private const int RYNN_LEAP_INTERVAL = 3;
        private const int RYNN_LEAP_MONTH = 3;
        private const int RYNN_ERA_5_DAYS_OS = 182242; //5CE001->5CE521, plus (521/3)=173 days from leap years, plus (26+28+16)=70 days, minus 1 (goes to offset point)
        private const int RYNN_ERA_4_DAYS = 303739; //4CE001->4CE868=867 years=303450 days, plus (868/3)=289 days from leap years (goes to end of 4CE867)
        private const int RYNN_ERA_3_DAYS = 330364; //3CE001->3CE944=943 years=330050 days, plus (944/3)=314 days from leap years (goes to end of 3CE943)
        private readonly int[] RYNN_MONTH_DAYS = {0, 26, 28, 27, 27, 27, 26, 26, 27, 27, 28, 27, 27, 27 };        

        private struct RynnMonthDays
        {
            public int Month;
            public int Days;

            public RynnMonthDays(int month, int days)
            {
                Month = month;
                Days = days;
            }
        }

        private readonly RynnFormatDate CommonPointRynn = new RynnFormatDate(5, 521, 3, 16, 0, 0, 0, true);
        private readonly DateTime CommonPoint = new DateTime(2160, 10, 21, 0, 0, 0, DateTimeKind.Utc); //TODO make ticks

        private DateTime InternalDate;

        public long Timestamp
        {
            get
            {
                return ConvertToUnixTime(InternalDate);
            }
        }

        public EarthFormatDate EarthDate
        {
            get
            {
                //structs are teh werid
                EarthFormatDate efd;
                efd.Year = InternalDate.Year;
                efd.Month = InternalDate.Month;
                efd.Day = InternalDate.Day;
                efd.Hour = InternalDate.Hour;
                efd.Minute = InternalDate.Minute;
                efd.Second = InternalDate.Second;
                return efd;
            }
        }

        public RynnFormatDate RynnDate
        {
            get
            {
                return GetRynnDateForDate();
            }
        }

        public MultiformatDate(EarthFormatDate fromDate) //fucking Objective-C influence
        {
            InternalDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, fromDate.Hour, fromDate.Minute, fromDate.Second, DateTimeKind.Utc);
            Console.WriteLine(InternalDate.ToString());
        }

        public MultiformatDate(RynnFormatDate fromDate)
        {
            InternalDate = GetDateForRynnDate(fromDate);
            Console.WriteLine(InternalDate.ToString());
        }

        public MultiformatDate(long fromTimestamp)
        {
            InternalDate = UnixTimeToDateTime(fromTimestamp);
            Console.WriteLine(InternalDate.ToString());
        }

        //UNIX converters stolen from http://www.fluxbytes.com/csharp/convert-datetime-to-unix-time-in-c/

        private static long ConvertToUnixTime(DateTime datetime)
        {
            DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return (long)(datetime - sTime).TotalSeconds;
        }

        private static DateTime UnixTimeToDateTime(long unixtime)
        {
            DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return sTime.AddSeconds(unixtime);
        }

        //actual Rynn conversion
        private DateTime GetDateForRynnDate(RynnFormatDate rfd)
        {
            if (!ValidateRynnDate(rfd))
                throw new ArgumentOutOfRangeException("rfd", rfd, "unacceptable date");

            int deltaDays = 0;

            //convert Rynn date by era
            if(rfd.Era == 6)
            {
                //6CE means we can calculate days, then offset
                int daysFromYear = GetRynnDaysForYearInterval(1, rfd.Year);
                int daysFromDate = GetRynnTotalDaysForMonthDays(new RynnMonthDays(rfd.Month, rfd.Day), (rfd.Year % RYNN_LEAP_INTERVAL == 0));

                int offsetDays = GetRynnTotalDaysForMonthDays(new RynnMonthDays(CommonPointRynn.Month, CommonPointRynn.Day), false);

                deltaDays = daysFromYear + daysFromDate - offsetDays;
            }
            else if(rfd.Era == 5)
            {
                //5CE means we go the other way but offset in a similar way
                int daysFromYear = GetRynnDaysForYearInterval(CommonPointRynn.Year, rfd.Year);
                int daysFromDate = GetRynnTotalDaysForMonthDays(new RynnMonthDays(rfd.Month, rfd.Day), (rfd.Year % RYNN_LEAP_INTERVAL == 0));

                int offsetDays = GetRynnTotalDaysForMonthDays(new RynnMonthDays(CommonPointRynn.Month, CommonPointRynn.Day), false);

                deltaDays = daysFromYear + daysFromDate - offsetDays;
            }
            else if(rfd.Era >= 3) 
            {
                //3CE or 4CE we use known day offsets and move from the beginning of the era
                //probably more error prone and broken

                int baseDayOffset = RYNN_ERA_5_DAYS_OS;

                if (rfd.Era == 4)
                    baseDayOffset += RYNN_ERA_4_DAYS;

                if (rfd.Era == 3)
                    baseDayOffset += (RYNN_ERA_4_DAYS + RYNN_ERA_3_DAYS);

                baseDayOffset -= 1; //I don't know why I need this but I have an OBO/rounding error somewhere

                int daysFromYear = GetRynnDaysForYearInterval(1, rfd.Year);
                int daysFromDate = GetRynnTotalDaysForMonthDays(new RynnMonthDays(rfd.Month, rfd.Day), (rfd.Year % RYNN_LEAP_INTERVAL == 0));

                deltaDays = (-baseDayOffset) + (daysFromYear + daysFromDate);
            }
            else
            {
                //unsolvable date
                throw new ArgumentOutOfRangeException("rfd.Era", "unsolvable era");
            }


            //run a calculation
            TimeSpan offset = new TimeSpan(deltaDays, 0, 0, 0);

            DateTime dt = CommonPoint + offset;

            return dt;
        }

        private int GetRynnDaysForYearInterval(int startYear, int endYear)
        {
            //get initial days
            int totalDays = (endYear - startYear) * RYNN_DAYS_PER_YEAR;

            //get num leap years from 0 to start year
            int toStartLeapYears = startYear / RYNN_LEAP_INTERVAL;

            //get num leap years from 0 to end year
            int toEndLeapYears = endYear / RYNN_LEAP_INTERVAL;

            //calculate
            int extraDays = toEndLeapYears - toStartLeapYears;

            return (totalDays + extraDays);
        }

        private RynnFormatDate GetRynnDateForDate()
        {
            RynnFormatDate rfd = new RynnFormatDate();

            //first, get days offset from alignment point
            int daysOffset = (InternalDate - CommonPoint).Days;

            Console.WriteLine(daysOffset);

            //if it's positive, it must be 6CE which is fairly easy to deal with
            if (daysOffset >= 0)
            {
                rfd.Era = 6;
                int years = daysOffset / RYNN_DAYS_PER_YEAR;
                years += 1; //a hack because eras overlap
                int extraDays = daysOffset % RYNN_DAYS_PER_YEAR;
                extraDays += GetRynnTotalDaysForMonthDays(new RynnMonthDays(CommonPointRynn.Month, CommonPointRynn.Day), false);
                extraDays -= 1; //this should fix it

                //handle leap years (sort of)
                extraDays -= years / RYNN_LEAP_INTERVAL;

                //don't go negative!
                if (extraDays < 0)
                {
                    years -= 1;
                    extraDays = RYNN_DAYS_PER_YEAR + extraDays;
                    if (years % RYNN_LEAP_INTERVAL == 0)
                        extraDays++;
                }

                //if the year is still 1, then we're on an era boundary!
                if (years == 1)
                {
                    rfd.OnEraBoundary = true;
                }

                //calculate actual month and days
                RynnMonthDays rmd = GetRynnMonthForDays(extraDays, (years % RYNN_LEAP_INTERVAL == 0));

                rfd.Year = years;
                rfd.Month = rmd.Month;
                rfd.Day = rmd.Days;
            }
            else if (daysOffset >= -RYNN_ERA_5_DAYS_OS)
            {
                rfd.Era = 5;

                //note that daysOffset is days before the offset point

                //get number of days after beginning of 5CE
                int daysInEra = RYNN_ERA_5_DAYS_OS + daysOffset; //because it's negative

                //Console.WriteLine(daysInEra);

                //calculate similar to 6CE
                int years = daysInEra / RYNN_DAYS_PER_YEAR;
                years += 1; //a hack because eras overlap
                int extraDays = daysInEra % RYNN_DAYS_PER_YEAR;

                //handle leap years (broken)
                extraDays -= years / RYNN_LEAP_INTERVAL;

                //don't go negative!
                if (extraDays < 0)
                {
                    years -= 1;
                    extraDays = RYNN_DAYS_PER_YEAR + extraDays;
                    if (years % RYNN_LEAP_INTERVAL == 0)
                        extraDays++;
                }

                Console.WriteLine(extraDays);

                //if the year is still 1, then we're on an era boundary!
                if (years == 1)
                {
                    rfd.OnEraBoundary = true;
                }

                //calculate actual month and days
                RynnMonthDays rmd = GetRynnMonthForDays(extraDays, (years % RYNN_LEAP_INTERVAL == 0));

                rfd.Year = years;
                rfd.Month = rmd.Month;
                rfd.Day = rmd.Days;

            }
            else if (daysOffset >= -(RYNN_ERA_5_DAYS_OS + RYNN_ERA_4_DAYS))
            {
                rfd.Era = 4;

                int daysInEra = (RYNN_ERA_5_DAYS_OS + RYNN_ERA_4_DAYS) + daysOffset; //because it's negative

                //literally copypasted from 5CE
                int years = daysInEra / RYNN_DAYS_PER_YEAR;
                years += 1; //a hack because eras overlap
                int extraDays = daysInEra % RYNN_DAYS_PER_YEAR;

                //handle leap years
                extraDays -= years / RYNN_LEAP_INTERVAL;

                //don't go negative!
                if (extraDays < 0)
                {
                    years -= 1;
                    extraDays = RYNN_DAYS_PER_YEAR + extraDays;
                    if (years % RYNN_LEAP_INTERVAL == 0)
                        extraDays++;
                }

                //if the year is still 1, then we're on an era boundary!
                if (years == 1)
                {
                    rfd.OnEraBoundary = true;
                }

                //calculate actual month and days
                RynnMonthDays rmd = GetRynnMonthForDays(extraDays, (years % RYNN_LEAP_INTERVAL == 0));

                rfd.Year = years;
                rfd.Month = rmd.Month;
                rfd.Day = rmd.Days;
            }
            else if (daysOffset >= -(RYNN_ERA_5_DAYS_OS + RYNN_ERA_4_DAYS + RYNN_ERA_3_DAYS))
            {
                rfd.Era = 3;

                int daysInEra = (RYNN_ERA_5_DAYS_OS + RYNN_ERA_4_DAYS + RYNN_ERA_3_DAYS) + daysOffset; //because it's negative

                //literally copypasted from 5CE
                int years = daysInEra / RYNN_DAYS_PER_YEAR;
                years += 1; //a hack because eras overlap
                int extraDays = daysInEra % RYNN_DAYS_PER_YEAR;

                //handle leap years
                extraDays -= years / RYNN_LEAP_INTERVAL;

                //don't go negative!
                if (extraDays < 0)
                {
                    years -= 1;
                    extraDays = RYNN_DAYS_PER_YEAR + extraDays;
                    if (years % RYNN_LEAP_INTERVAL == 0)
                        extraDays++;
                }

                //if the year is still 1, then we're on an era boundary!
                if (years == 1)
                {
                    rfd.OnEraBoundary = true;
                }

                //calculate actual month and days
                RynnMonthDays rmd = GetRynnMonthForDays(extraDays, (years % RYNN_LEAP_INTERVAL == 0));

                rfd.Year = years;
                rfd.Month = rmd.Month;
                rfd.Day = rmd.Days;
            }
            else
            {
                rfd.Era = 2; //ehehehee
            }

            return rfd;
        }

        private int GetRynnTotalDaysForMonthDays(RynnMonthDays rynnMonthDays, bool isLeapYear)
        {
            int[] monthDayArray = new int[RYNN_MONTH_DAYS.Length];
            Array.Copy(RYNN_MONTH_DAYS, monthDayArray, RYNN_MONTH_DAYS.Length);

            //leap year hack
            if (isLeapYear)
            {
                monthDayArray[3] += 1;
            }
            
            int totalDays = rynnMonthDays.Days;
            for (int month = 1; month < rynnMonthDays.Month; month++)
            {
                totalDays += monthDayArray[month];
            }

            Console.WriteLine(totalDays);

            return totalDays;
        }

        private RynnMonthDays GetRynnMonthForDays(int days, bool isLeapYear)
        {
            //because a year starts on the 1st, not the 0th
            days += 1;

            int[] monthDayArray = new int[RYNN_MONTH_DAYS.Length];
            Array.Copy(RYNN_MONTH_DAYS, monthDayArray, RYNN_MONTH_DAYS.Length);

            //leap year hack
            if(isLeapYear)
            {
                monthDayArray[3] += 1;
            }

            RynnMonthDays rmd = new RynnMonthDays();

            //a stupid way of doing it
            //it's broken and I don't know why
            for(int month = 1; month < monthDayArray.Length; month++)
            {
                //if days is <= the number of days in the month, it's the correct month
                if(days <= monthDayArray[month])
                {
                    rmd.Month = month;
                    break;
                }
                else
                {
                    //otherwise, subtract and continue
                    days -= monthDayArray[month];
                }
            }

            rmd.Days = days;

            return rmd;            
        }

        private bool ValidateRynnDate(RynnFormatDate rfd)
        {
            if (rfd.Era < 1 || rfd.Era > 6)
                return false;

            if (rfd.Year <= 0) //yes, it can be too high, but something else will break if that's the case
                return false;

            if (rfd.Month < 1 || rfd.Month >= RYNN_MONTH_DAYS.Length)
                return false;

            if (rfd.Day < 1 || rfd.Day > RYNN_MONTH_DAYS[rfd.Month])
                return false;

            if (rfd.Hour < 0 || rfd.Hour >= 24)
                return false;

            if (rfd.Minute < 0 || rfd.Minute >= 60)
                return false;

            if (rfd.Second < 0 || rfd.Second >= 60)
                return false;

            return true;
        }

    }
}
