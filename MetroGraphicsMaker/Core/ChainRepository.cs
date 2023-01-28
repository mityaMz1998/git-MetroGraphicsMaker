using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core;

namespace WpfApplication1.Core
{
    class ChainRepository
    {
        protected IEnumerable<Route> Routes;

        protected List<Chain> Chains;

        protected List<Link> Links;

        protected IEnumerable<InspectionPoint> Points;

        protected static Boolean HasDepot(Route route)
        {
            return route.prevRoute.nightStayPoint != null && route.prevRoute.nightStayPoint.depot != null;
        }

        protected static Boolean IsNotCheked(Route route)
        {
            return route.State != RouteState.CHECKED;
                // route.State == RouteState.DEFAULT || route.State == RouteState.VISITED;
        }



        public ChainRepository(IEnumerable<Route> routes, IEnumerable<InspectionPoint> points)
        {
            if (routes == null)
                throw new ArgumentNullException(/*nameof(routes)*/);

            if (points == null)
                throw new ArgumentNullException(/*nameof(points)*/);

            Routes = routes;
            Points = points;
            Chains = new List<Chain>();
        }

        public IEnumerable<Chain> GetAllChains()
        {
            var localChainOfRoutes = new List<Route>();

            while (Routes.Any(r => IsNotCheked(r)))
            {
                var currentRoute =
                    Routes.FirstOrDefault(r => IsNotCheked(r) && HasDepot(r));

                if (currentRoute == null) break;

                while (IsNotCheked(currentRoute)) // currentRoute.State != RouteState.CHECKED)
                {
                    var afterRepair = false;
                    do
                    {
                        if (currentRoute.State == RouteState.VISITED)
                        {
                            localChainOfRoutes.Add(currentRoute);
                            afterRepair = true;

                            currentRoute.State = RouteState.CHECKED;
                            if (HasDepot(currentRoute))
                                break;
                            currentRoute = currentRoute.nextRoute;
                        }

                        localChainOfRoutes.Add(currentRoute);
                        if (currentRoute.State == RouteState.DEFAULT && currentRoute.Repairs.Any(r => !r.isContinue))
                        {
                            currentRoute.State = RouteState.VISITED;
                            if (currentRoute.nextRoute.Repairs.Any(r => r.isContinue))
                                currentRoute.State = RouteState.CHECKED;
                            break;
                        }

                        #region repeat_region

                        currentRoute.State = RouteState.CHECKED;

                        if (HasDepot(currentRoute))
                            break;

                        currentRoute = currentRoute.nextRoute;

                        #endregion
                    } while (true);

                    //foreach (var point in localChainOfRoutes.SelectMany(r => r.depot.inspectionPoints))
                    //    points.Add(point);

                    // points.Add(MovementSchedule.colInspectionPoint.First(point => point.name.Equals("Новокосино"));

                    Chains.Add(new Chain(localChainOfRoutes) { IsAfterRepair = afterRepair });
                    localChainOfRoutes.Clear();
                }
            }
            return Chains;
        }

        public IEnumerable<Link> GetAllLinks()
        {
            if (Links == null)
            {
                foreach (var chain in Chains)
                    chain.PrepareRoutes();

                Links = Chains.SelectMany(chain => chain.Links).ToList();
            }
            return Links;
        }

        public IEnumerable<Chain> GetOrderedChains<TKey>(Func<Chain, TKey> criteria)
        {
            return Chains.OrderBy(criteria);
        }

        public IEnumerable<Chain> GetOrderedChains<T>(Func<Link, T> criteria)
        {
            throw new NotImplementedException();
        }
    }
}
