using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Messages;

namespace Core
{
    public class Train
    {
        public UInt32 code;

        public List<Wagon> wagons;

        public Route route;

        public Train()
        {
        }

        public Train(DataRow row)
        {
            code = Convert.ToUInt32(row["Код"].ToString());

            wagons = new List<Wagon>();
        }

        public void Initialize(DataRow row)
        {
            var routeNumber = Convert.ToUInt32(row["Маршрут"].ToString());
            route = MovementSchedule.colRoute.SingleOrDefault(r => r.number == routeNumber);
            if (route != null)
                route.train = this;
            else
                Error.showErrorMessage(new Route {number = routeNumber}, this);

            //wagons = mdlData.colWagon.Where(w => w.train == this).ToList();
        }
    }
}
