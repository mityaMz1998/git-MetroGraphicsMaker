using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exceptions
{
    public enum GraphState
    {
        NONE,

        GOOD,

        WithOutWhite
    }

     public class TheGraphIsNotFeasible : Exception
    {
       public GraphState State { get; protected set; }

        public TheGraphIsNotFeasible(GraphState state)
        {
            State = state;
        }
    }
}
