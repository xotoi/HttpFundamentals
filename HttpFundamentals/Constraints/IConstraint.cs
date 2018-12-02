using System;

namespace HttpFundamentals.Constraints
{
    public interface IConstraint
    {
        ConstraintType ConstraintType { get; }
        bool IsAcceptable(Uri uri);
    }
}
