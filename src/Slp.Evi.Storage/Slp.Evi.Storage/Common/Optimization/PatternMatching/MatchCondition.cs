using System;
using System.Globalization;
using DatabaseSchemaReader.DataSchema;
using Slp.Evi.Storage.Query;

namespace Slp.Evi.Storage.Common.Optimization.PatternMatching
{
    /// <summary>
    /// The match condition
    /// </summary>
    public class MatchCondition
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="MatchCondition"/> class from being created.
        /// </summary>
        private MatchCondition()
        {
            IsAlwaysFalse = false;
            LeftPattern = null;
            RightPattern = null;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is always false.
        /// </summary>
        /// <value><c>true</c> if this instance is always false; otherwise, <c>false</c>.</value>
        public bool IsAlwaysFalse { get; private set; }

        /// <summary>
        /// Gets the left pattern.
        /// </summary>
        /// <remarks>
        /// Do not get this value if <see cref="IsAlwaysFalse"/> is <c>true</c>.
        /// </remarks>
        /// <value>The left pattern.</value>
        public Pattern LeftPattern { get; private set; }

        /// <summary>
        /// Gets the right pattern.
        /// </summary>
        /// <remarks>
        /// Do not get this value if <see cref="IsAlwaysFalse"/> is <c>true</c>.
        /// </remarks>
        /// <value>The right pattern.</value>
        public Pattern RightPattern { get; private set; }

        /// <summary>
        /// Creates the always false condition.
        /// </summary>
        public static MatchCondition CreateAlwaysFalseCondition()
        {
            return new MatchCondition()
            {
                IsAlwaysFalse = true
            };
        }

        /// <summary>
        /// Creates the condition.
        /// </summary>
        /// <param name="leftPattern">The left pattern.</param>
        /// <param name="rightPattern">The right pattern.</param>
        public static MatchCondition CreateCondition(Pattern leftPattern, Pattern rightPattern)
        {
            if (!CanEqualToOther(leftPattern, rightPattern) || !CanEqualToOther(rightPattern, leftPattern))
            {
                return new MatchCondition()
                {
                    IsAlwaysFalse = true
                };
            }

            return new MatchCondition()
            {
                LeftPattern = leftPattern,
                RightPattern = rightPattern
            };
        }

        private static bool CanEqualToOther(Pattern first, Pattern other)
        {
            if (first.PatternItems.Length > 0)
            {
                var firstItem = first.PatternItems[0];
                if (firstItem.IsColumn && firstItem.DataType != null && firstItem.DataType.IsNumeric)
                {
                    if (other.PatternItems.Length == 0)
                    {
                        return false;
                    }
                    else
                    {
                        var otherFirstItem = other.PatternItems[0];
                        if (otherFirstItem.IsConstant)
                        {
                            if (otherFirstItem.Text.Length > 0)
                            {
                                var category = char.GetUnicodeCategory(otherFirstItem.Text[0]);

                                switch (category)
                                {
                                    case UnicodeCategory.DecimalDigitNumber:
                                    case UnicodeCategory.MathSymbol:
                                        break;
                                    default:
                                        return false;
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}