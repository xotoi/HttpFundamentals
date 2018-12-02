using System;
using System.Collections.Generic;
using System.Linq;

namespace HttpFundamentals.Constraints
{
    public class FileTypesConstraint : IConstraint
    {
        private readonly IEnumerable<string> _acceptableExtensions;

        public FileTypesConstraint(IEnumerable<string> acceptableExtensions)
        {
            _acceptableExtensions = acceptableExtensions;
        }

        public ConstraintType ConstraintType => ConstraintType.FileConstraint;

        public bool IsAcceptable(Uri uri)
        {
            return _acceptableExtensions.Any(e => uri.Segments.Last().EndsWith(e));
        }
    }
}
