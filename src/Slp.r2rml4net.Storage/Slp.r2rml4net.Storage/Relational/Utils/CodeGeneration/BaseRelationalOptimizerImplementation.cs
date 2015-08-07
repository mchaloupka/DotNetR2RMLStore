﻿using Slp.r2rml4net.Storage.Relational.Optimization;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.Conditions;
using Slp.r2rml4net.Storage.Relational.Query.Expressions;
using Slp.r2rml4net.Storage.Relational.Query.Sources;

namespace Slp.r2rml4net.Storage.Relational.Utils.CodeGeneration
{
    /// <summary>
    /// The base class for relational optimizers
    /// </summary>
    /// <typeparam name="T">Type of parameter passed to process</typeparam>
    public class BaseRelationalOptimizerImplementation<T>
        : BaseExpressionTransformerG<BaseRelationalOptimizer<T>.OptimizationContext, IExpression,IAssignmentCondition, ISourceCondition, IFilterCondition, ICalculusSource>
    {

        /// <summary>
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected override IExpression CommonFallbackTransform(IExpression toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="ColumnExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IExpression Transform(ColumnExpression toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="ConcatenationExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IExpression Transform(ConcatenationExpression toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="ConstantExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IExpression Transform(ConstantExpression toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected override IAssignmentCondition CommonFallbackTransform(IAssignmentCondition toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected override ISourceCondition CommonFallbackTransform(ISourceCondition toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="TupleFromSourceCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ISourceCondition Transform(TupleFromSourceCondition toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected override IFilterCondition CommonFallbackTransform(IFilterCondition toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="AlwaysFalseCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IFilterCondition Transform(AlwaysFalseCondition toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="AlwaysTrueCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IFilterCondition Transform(AlwaysTrueCondition toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="ConjunctionCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IFilterCondition Transform(ConjunctionCondition toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="DisjunctionCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IFilterCondition Transform(DisjunctionCondition toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="EqualExpressionCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IFilterCondition Transform(EqualExpressionCondition toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="EqualVariablesCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IFilterCondition Transform(EqualVariablesCondition toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="IsNullCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IFilterCondition Transform(IsNullCondition toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="NegationCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override IFilterCondition Transform(NegationCondition toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected override ICalculusSource CommonFallbackTransform(ICalculusSource toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="CalculusModel"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ICalculusSource Transform(CalculusModel toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }

        /// <summary>
        /// Process the <see cref="SqlTable"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected override ICalculusSource Transform(SqlTable toTransform, BaseRelationalOptimizer<T>.OptimizationContext data)
        {
            return toTransform;
        }
    }
}
