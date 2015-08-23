﻿// This is generated code, do not edit!!!
using System;

using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.Conditions.Source;
namespace Slp.r2rml4net.Storage.Relational.Utils.CodeGeneration
{
    /// <summary>
    /// Base generated transformer for <see cref="ISourceConditionVisitor" />
    /// </summary>
    /// <typeparam name="T">Type of parameter passed to process</typeparam>
    /// <typeparam name="TR">Type of the transformation result</typeparam>
    /// <typeparam name="T1">Type of the transformation result when processing <see cref="IFilterCondition" /></typeparam>
    /// <typeparam name="T2">Type of the transformation result when processing <see cref="ICalculusSource" /></typeparam>
    public abstract class BaseSourceConditionTransformerG<T, TR, T1, T2>
        : BaseFilterConditionTransformerG<T, T1, T2>, ISourceConditionVisitor
    {
        /// <summary>
        /// Transforms the <see cref="ISourceCondition" />.
        /// </summary>
        /// <param name="instance">The instance to tranform.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformed calculus source.</returns>
        public TR TransformSourceCondition(ISourceCondition instance, T data)
        {
            return (TR)instance.Accept(this, data);
        }
        /// <summary>
        /// Decides whether we should use standard or fallback transformation for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed</param>
        /// <param name="data">The passed data</param>
        /// <returns><c>true</c> if transformation should process standardly, <c>false</c> the fallback should be used.</returns>
        protected virtual bool CommonShouldTransform(ISourceCondition toTransform, T data)
        {
            return true;
        }

        /// <summary>
        /// Postprocess for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The postprocessed transformation result</returns>
        protected virtual TR CommonPostTransform(TR transformed, ISourceCondition toTransform, T data)
        {
            return transformed;
        }

        /// <summary>
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected virtual TR CommonFallbackTransform(ISourceCondition toTransform, T data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Visits <see cref="TupleFromSourceCondition" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(TupleFromSourceCondition toVisit, object data)
        {
            var tData = (T)data;
            if(ShouldTransform(toVisit, tData))
            {
                var transformed = Transform(toVisit, tData);
                return PostTransform(transformed, toVisit, tData);
            }
            else
            {
                return FallbackTransform(toVisit, tData);
            }
        }

        /// <summary>
        /// Process the <see cref="TupleFromSourceCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected abstract TR Transform(TupleFromSourceCondition toTransform, T data);

        /// <summary>
        /// Preprocess for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed</param>
        /// <param name="data">The passed data</param>
        /// <returns><c>true</c> if transformation should continue, <c>false</c> the fallback should be used.</returns>
        protected virtual bool ShouldTransform(TupleFromSourceCondition toTransform, T data)
        {
            return CommonShouldTransform(toTransform, data);
        }

        /// <summary>
        /// Postprocess for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The postprocessed transformation result</returns>
        protected virtual TR PostTransform(TR transformed, TupleFromSourceCondition toTransform, T data)
        {
            return CommonPostTransform(transformed, toTransform, data);
        }

        /// <summary>
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected virtual TR FallbackTransform(TupleFromSourceCondition toTransform, T data)
        {
            return CommonFallbackTransform(toTransform, data);
        }

        /// <summary>
        /// Visits <see cref="UnionedSourcesCondition" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(UnionedSourcesCondition toVisit, object data)
        {
            var tData = (T)data;
            if(ShouldTransform(toVisit, tData))
            {
                var transformed = Transform(toVisit, tData);
                return PostTransform(transformed, toVisit, tData);
            }
            else
            {
                return FallbackTransform(toVisit, tData);
            }
        }

        /// <summary>
        /// Process the <see cref="UnionedSourcesCondition"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected abstract TR Transform(UnionedSourcesCondition toTransform, T data);

        /// <summary>
        /// Preprocess for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed</param>
        /// <param name="data">The passed data</param>
        /// <returns><c>true</c> if transformation should continue, <c>false</c> the fallback should be used.</returns>
        protected virtual bool ShouldTransform(UnionedSourcesCondition toTransform, T data)
        {
            return CommonShouldTransform(toTransform, data);
        }

        /// <summary>
        /// Postprocess for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The postprocessed transformation result</returns>
        protected virtual TR PostTransform(TR transformed, UnionedSourcesCondition toTransform, T data)
        {
            return CommonPostTransform(transformed, toTransform, data);
        }

        /// <summary>
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected virtual TR FallbackTransform(UnionedSourcesCondition toTransform, T data)
        {
            return CommonFallbackTransform(toTransform, data);
        }

    }
}
