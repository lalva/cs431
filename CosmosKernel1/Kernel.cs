using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;

namespace CosmosKernel1
{
    public class Kernel : Sys.Kernel
    {
        public int month;
        public int dayOfMonth;
        public int year;
        public int hour;
        public int minute;
        public int second;

        public int monthOffset;
        public int dayOffset;
        public int yearOffset;
        public int hourOffset;
        public int minuteOffset;
        public int secondOffset;

        protected override void BeforeRun()
        {
            updateTime();
            monthOffset = 0;
            dayOffset = 0;
            yearOffset = 0;
            hourOffset = 0;
            minuteOffset = 0;
            secondOffset = 0;
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
        }

        protected override void Run()
        {
            Console.Write("Input: ");
            string input = Console.ReadLine();
            if (input.IndexOf("date") == 0)
            {
                if (input.IndexOf("date ") == 0)
                {
                    // setDate(input.Substring(5, input.Length - 4));
                }
                Console.WriteLine(getDate());
            }
            else if (input.IndexOf("time") == 0)
            {
                if (input.IndexOf("time ") == 0)
                {
                    // setTime(input.Substring(5, input.Length - 4));
                }
                Console.WriteLine(getTime());
            }
            else
            {
                Console.Write("Text typed: ");
                Console.WriteLine(input);
            }
        }

        protected void setTime(string userTime)
        {
            string[] times = userTime.Split(':');
            int userHour = Int32.Parse(times[0]);
            int userMinute = Int32.Parse(times[1]);
            int userSecond = Int32.Parse(times[2]);
            updateTime();
            hourOffset = hour - userHour;
            minuteOffset = minute - userMinute;
            secondOffset = second - userSecond;
        }
        protected void setDate(string userDate)
        {
            string[] dates = userDate.Split('/');
            int userMonth = Int32.Parse(dates[0]);
            int userDay = Int32.Parse(dates[1]);
            int userYear = Int32.Parse(dates[2]);
            updateTime();
            monthOffset = month - userMonth;
            dayOffset = dayOfMonth - userDay;
            yearOffset = year - userYear;
        }

        protected void updateTime()
        {
            hour = Cosmos.Hardware.RTC.Hour;
            minute = Cosmos.Hardware.RTC.Minute;
            second = Cosmos.Hardware.RTC.Second;
            month = Cosmos.Hardware.RTC.Month;
            dayOfMonth = Cosmos.Hardware.RTC.DayOfTheMonth;
            year = (Cosmos.Hardware.RTC.Century * 100) + Cosmos.Hardware.RTC.Year;
        }

        protected string getTime()
        {
            updateTime();
            return (hour - hourOffset).ToString() + ":" + (minute - minuteOffset).ToString() + ":" + (second - secondOffset).ToString();
        }

        protected string getDate()
        {
            updateTime();
            return (month - monthOffset).ToString() + "/" + (dayOfMonth - dayOffset).ToString() + "/" + (year - yearOffset).ToString();
        }
    }
}
