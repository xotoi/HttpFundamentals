using System;

namespace HttpFundamentals.Constraints
{
    public class CrossDomainTransitionConstraint : IConstraint
    {
        private readonly Uri _parentUri;
        private readonly CrossDomainTransition _availableTransition;

        public CrossDomainTransitionConstraint(CrossDomainTransition availableTransition, Uri parentUri)
        {
            switch (availableTransition)
            {
                case CrossDomainTransition.All:
                case CrossDomainTransition.CurrentDomainOnly:
                case CrossDomainTransition.DescendantUrlsOnly:
                    _availableTransition = availableTransition;
                    _parentUri = parentUri;
                    break;
                default:
                    throw new ArgumentException($"Unknown transition type: {availableTransition}");
            }
        }

        public ConstraintType ConstraintType => ConstraintType.UrlConstraint | ConstraintType.FileConstraint;

        public bool IsAcceptable(Uri uri)
        {
            switch (_availableTransition)
            {
                case CrossDomainTransition.All:
                    return true;
                case CrossDomainTransition.CurrentDomainOnly:
                    if (_parentUri.DnsSafeHost == uri.DnsSafeHost)
                    {
                        return true;
                    }
                    break;
                case CrossDomainTransition.DescendantUrlsOnly:
                    if (_parentUri.IsBaseOf(uri))
                    {
                        return true;
                    }
                    break;
            }

            return false;
        }
    }
}
