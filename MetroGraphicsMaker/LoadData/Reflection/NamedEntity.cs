using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfApplication1.LoadData.Reflection
{
    class NamedEntity
    {
        protected String name;

        public String Name
        {
            get
            {
                return name;
            }
            protected set
            {
                if (value == null)
                    return;

                var trimmed = value.Trim();
                if (trimmed.Equals(String.Empty))
                    return;

                name = trimmed;
            }
        }
    }
}
