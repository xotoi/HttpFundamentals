using System;

namespace HttpFundamentals.Constraints
{
    [Flags]
    public enum ConstraintType
    {
        FileConstraint = 1,
        UrlConstraint = 2
    }
}
