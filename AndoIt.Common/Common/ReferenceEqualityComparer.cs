using System.Collections.Generic;

namespace AndoIt.Common.Core.Common
{
    /// <summary>
    /// Comparador para evitar referencias circulares en HashSet
    /// </summary>
    public class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public new bool Equals(object x, object y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(object obj)
        {
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
    }
}
