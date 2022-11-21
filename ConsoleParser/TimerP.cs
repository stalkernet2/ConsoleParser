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
        private static TimeSpan _triggerTimeToStart { get; set; }
        private static TimeSpan _triggerTimeToStop { get; set; }
        private static Parameters _parameters { get; set; }
        private static Timer _timer;

        public static void Init(Parameters param, TimeSpan startTime, TimeSpan stopTime)
        {
            _triggerTimeToStart = startTime;
            _triggerTimeToStop = stopTime;

            Logger.LogNewLine($"Запуск парсера по времени назначен на {_triggerTimeToStart}.");
            Logger.LogNewLine($"Выключение на {_triggerTimeToStop}.\n");

            _parameters = param;
            _timer = new Timer(new TimerCallback(CheckTimeToStart), new AutoResetEvent(false), 0, 1000);
        }

        private static void CheckTimeToStart(object state)
        {
            var timeNow = DateTime.Now;
            var timeNowSpan = new TimeSpan(timeNow.Hour, timeNow.Minute, timeNow.Second);
            if (_triggerTimeToStart.Hours <= timeNowSpan.Hours && _triggerTimeToStart.Minutes <= timeNowSpan.Minutes && _triggerTimeToStart.Seconds <= timeNowSpan.Seconds)
            {
                _timer.Dispose();
                Logger.LogNewLine("Запуск парсера");

                _timer = new Timer(new TimerCallback(CheckTimeToStop), new AutoResetEvent(false), 0, 1000);

                Parser.Start(_parameters);
            }
        }

        private static void CheckTimeToStop(object stat)
        {
            var timeNow = DateTime.Now;
            var timeNowSpan = new TimeSpan(timeNow.Hour, timeNow.Minute, timeNow.Second);
            if (_triggerTimeToStop.Hours <= timeNowSpan.Hours && _triggerTimeToStop.Minutes <= timeNowSpan.Minutes && _triggerTimeToStop.Seconds <= timeNowSpan.Seconds)
            {
                _timer.Dispose();
                Console.WriteLine("Этого текста не должно быть видно");
                Environment.Exit(0);
            }
        }
    }
}
