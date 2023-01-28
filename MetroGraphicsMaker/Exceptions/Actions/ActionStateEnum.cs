using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exceptions.Actions
{
    [Flags]
    public enum ActionState
    {
        NONE,

        DONE, //Успешно выполненная операция

        CANT_EXECUTE, //Операция не может быть выполнена и, как правило, запускается исключение

        CANCELLED //Успешно отмененная операция.
    }
}
