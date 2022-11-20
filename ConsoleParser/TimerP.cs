using ConsoleParser.Parse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleParser
{
    public class TimerP
    {
        private static TimeSpan s_triggerTimeToStart { get; set; }
        private static TimeSpan s_triggerTimeToStop { get; set; }
        private static Parameters s_parameters { get; set; }
        private static Timer s_timer { get; set; }
        private static Thread s_thread { get; set; }


        public static void Init(Parameters param, TimeSpan startTime, TimeSpan stopTime)
        {
            s_triggerTimeToStart = startTime;
            s_triggerTimeToStop = stopTime;

            Logger.LogNewLine($"Парсер должен запуститься в {s_triggerTimeToStart}");
            Logger.LogNewLine($"Выключиться в {s_triggerTimeToStop}");

            s_parameters = param;
            s_timer = new Timer(new TimerCallback(CheckTimeToStart), new AutoResetEvent(false), 0, 1000);
        }

        private static void CheckTimeToStart(object state)
        {
            var timeNow = DateTime.Now;
            var timeNowSpan = new TimeSpan(timeNow.Hour, timeNow.Minute, timeNow.Second);
            if (s_triggerTimeToStart.Hours <= timeNowSpan.Hours && s_triggerTimeToStart.Minutes <= timeNowSpan.Minutes && s_triggerTimeToStart.Seconds <= timeNowSpan.Seconds)
            {
                s_timer.Dispose();
                Logger.LogNewLine("Запуск парсера");

                s_timer = new Timer(new TimerCallback(CheckTimeToStop), new AutoResetEvent(false), 0, 1000);

                Parser.Start(s_parameters);
            }
        }

        private static void CheckTimeToStop(object stat)
        {
            var timeNow = DateTime.Now;
            var timeNowSpan = new TimeSpan(timeNow.Hour, timeNow.Minute, timeNow.Second);
            if (s_triggerTimeToStop.Hours <= timeNowSpan.Hours && s_triggerTimeToStop.Minutes <= timeNowSpan.Minutes && s_triggerTimeToStop.Seconds <= timeNowSpan.Seconds)
            {
                s_timer.Dispose();
                Console.WriteLine("Этого текста не должно быть видно");
                Environment.Exit(0);
            }
        }
    }
}
