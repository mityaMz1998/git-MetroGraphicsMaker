using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exceptions.Actions
{
    public class TheOperationIsNotFeasible : Exception
    {
        public ActionState State { get; protected set; }

        public TheOperationIsNotFeasible(ActionState state)
        {
            State = state;
        }
    }
}
