using System;

namespace Core
{
    public class Link : ICloneable
    {
        /// <summary>
        /// Первый
        /// </summary>
        public Route FirstRoute;

        /// <summary>
        /// Второй маршрут. Может иметь значение null.
        /// </summary>
        public Route SecondRoute = null;

        /// <summary>
        /// 
        /// </summary>
        public AdvancedRepairCandidate Candidate = null;

        /// <summary>
        /// Флаг, показывающий назначался ли на данную цепочку кандидат.
        /// </summary>
        public Boolean IsCheked = false;

        /// <summary>
        /// Родительская цепочка.
        /// </summary>
        public Chain ParentChain;

        public Boolean ContainsRepair()
        {
            return (FirstRoute.Times.Count > 0 || SecondRoute != null && SecondRoute.Times.Count > 0);
        }

        /// <summary>
        /// Возвращает время без ремонта для данного звена согласно флагу.
        /// </summary>
        /// <param name="beforeRepair">Истина - от начала до осмотра/ремонта, ложь - до конца движения.</param>
        /// <returns>Колличество метросекунд, которое проработал состав на линии без осмотров.</returns>
        public Int32 TimeBetweenRepair(Boolean beforeRepair)
        {
            return FirstRoute.TimeWithoutRepair(beforeRepair) + (SecondRoute == null
                ? 0
                : SecondRoute.TimeWithoutRepair(false));
        }

        public override String ToString()
        {
            return String.Format("{4}Chain: {6} [{5}]{4}{4}First: {0}{4}{7}Times:{4}{1}{4}Second: {2}{4}{7}Times:{4}{3}{4}============================",
                FirstRoute.number.ToString("№000"), FirstRoute.GetWindows(),
                SecondRoute != null ? SecondRoute.number.ToString("№000") : "null",
                SecondRoute != null ? SecondRoute.GetWindows() : "null",
                Environment.NewLine,
                ParentChain.TimeWithoutRepair,
                ParentChain.ToString(),
                '\t');
        }

        public object Clone()
        {
            var localLink = new Link
            {
                FirstRoute = this.FirstRoute.Clone() as Route,
                SecondRoute = this.SecondRoute != null ? this.SecondRoute.Clone() as Route : null,
                ParentChain = this.ParentChain
            };
            return localLink as Object;
        }
    }
}