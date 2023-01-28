using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Converters;
using Messages;

namespace Core
{
    public class Wagon
    {
        public UInt16 number;

        public WagonType type;

        public Dictionary<RepairType, Int32> timesWithoutRepair;

        public Train train;

        public Wagon(DataRow row)
        {
            number = Convert.ToUInt16(row["Номер"].ToString());

            timesWithoutRepair = new Dictionary<RepairType, Int32>();
        }

        public void Initialize(DataRow row)
        {
            var typeCode = Convert.ToUInt32(row["Тип"].ToString());
            type = MovementSchedule.colWagonType.SingleOrDefault(t => t.code == typeCode);

            if (type == null)
                Error.showErrorMessage(new WagonType {code = typeCode}, this);

            var times = row["Времена"].ToString().Split(new[] {';'});
            foreach (var s in times)
            {
                var pair = s.Split(new[] {"-->"}, StringSplitOptions.RemoveEmptyEntries);
                var key = MovementSchedule.colRepairType.SingleOrDefault(t => t.name.Equals(pair[0]));
                var value = TimeConverter.TimeToSeconds(Convert.ToDateTime(pair[1]));
                if (key != null)
                    timesWithoutRepair.Add(key, value);
                else
                    Error.showErrorMessage(new RepairType {name = pair[0]}, this);
            }

            var trainCode = Convert.ToUInt32(row["Состав"].ToString());
            train = MovementSchedule.colTrain.SingleOrDefault(t => t.code == trainCode);
            if (train != null)
                train.wagons.Add(this);
            else
                Error.showErrorMessage(new Train {code = trainCode}, this);
        }
    }
}