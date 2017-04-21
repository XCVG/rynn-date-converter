using System;
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
        int Era;
        int Year;
        int Month;
        int Day;
        int Hour;
        int Minute;
        int Second;

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
        }
    }

    public sealed class MultiformatDate //fuck you if you want to extend it
    {

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
                throw new NotImplementedException();
            }
        }

        public MultiformatDate(EarthFormatDate fromDate) //fucking Objective-C influence
        {
            InternalDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, fromDate.Hour, fromDate.Minute, fromDate.Second, DateTimeKind.Utc);
            Console.WriteLine(InternalDate.ToString());
        }

        public MultiformatDate(RynnFormatDate fromDate)
        {
            //this is gonna suck
            throw new NotImplementedException();
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

    }
}
