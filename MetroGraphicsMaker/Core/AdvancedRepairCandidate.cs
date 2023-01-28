using Converters;
using System;

namespace Core
{
    /// <summary>
    /// 
    /// </summary>
    public class AdvancedRepairCandidate : ICloneable
    {
        private readonly RepairCandidate _candidate;

        private Int32 routeIndex;

        private Int32 repairIndex;

        public Int32 BeginTime()
        {
            return _candidate.BeginTime;
        }

        public InspectionPoint Point()
        {
            return _candidate.Point;
        }

        private readonly Route _route;

        public Route Route()
        {
            return _route;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="candidate"></param>
        /// <param name="route"></param>
        /// <param name="repIndex"></param>
        /// <param name="roIndex"></param>
        public AdvancedRepairCandidate(RepairCandidate candidate, Route route, Int32 repIndex = 0, Int32 roIndex = 0)
        {
            _candidate = candidate;

            _route = route;

            repairIndex = repIndex;

            routeIndex = roIndex;
        }

        public String GetPair()
        {
            return String.Format("{0}-{1}", _route.number, _candidate.Code);
            //return String.Format("{0}-{1}", routeIndex, repairIndex);
        }

        public override String ToString()
        {
            return String.Format("{0} [{1}] / {2} / №{3:D3}", _candidate.BeginTime, TimeConverter.SecondsToString(_candidate.BeginTime), _candidate.Point.name, _route.number);
        }


        public override bool Equals(object obj)
        {
            var other = obj as AdvancedRepairCandidate;
            return other != null && other._candidate.BeginTime == _candidate.BeginTime && other._candidate.Point != null &&
                   _candidate.Point != null && other._candidate.Point.code == _candidate.Point.code;
            //&& _route.number == other._route.number;
        }

        public override Int32 GetHashCode()
        {
            return _candidate.Point.GetHashCode() + _candidate.BeginTime;// + Convert.ToInt32(_route.number);
        }

        public object Clone()
        {
            return new AdvancedRepairCandidate(_candidate, _route);
        }
    }
}
