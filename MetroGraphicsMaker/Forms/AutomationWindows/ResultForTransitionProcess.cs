using System;

namespace Forms.AutomationWindows
{
    public class ResultForTransitionProcess
    {
        public Int32 StartTimeTransitionProcess { get; set; }
        public Int32 EndTimeTransitionProcess { get; set; }
        public Int32 TrainsByFirstWay { get; set; }
        public Int32 TrainsBySecondWay { get; set; }
        public Int32[] Results { get; set; }
        public Int32 CommonFactorResult { get; set; }
        public ResultForTransitionProcess()
        {

        }

        public ResultForTransitionProcess(ResultForMyDataGrid value)
        {
            StartTimeTransitionProcess = Converters.TimeConverter.StringToSeconds(value.StartingTimeDataGrid);

            EndTimeTransitionProcess = Converters.TimeConverter.StringToSeconds(value.EndingTimeDataGrid);

            TrainsByFirstWay = value.PairsByFirstWay;

            TrainsBySecondWay = value.PairsBySecondWay;

            Results = new Int32[TrainsByFirstWay + TrainsBySecondWay];

            CommonFactorResult = 1;

        }
    }
}