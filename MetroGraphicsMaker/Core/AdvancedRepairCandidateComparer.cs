using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class AdvancedRepairCandidateComparer : IEqualityComparer<AdvancedRepairCandidate>
    {
        public bool Equals(AdvancedRepairCandidate x, AdvancedRepairCandidate y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(AdvancedRepairCandidate obj)
        {
            return obj.GetHashCode();
        }
    }
}
