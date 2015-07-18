﻿// This is generated code, do not edit!!!

using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Relational.Query.Expression;

namespace Slp.r2rml4net.Storage.Relational.Utils.CodeGeneration
{
    /// <summary>
    /// Base generated transformer for <see cref="IExpressionVisitor" />
    /// </summary>
    /// <typeparam name="T">Type of parameter passed to process</typeparam>
    /// <typeparam name="TR">Type of the transformation result</typeparam>
    /// <typeparam name="T1">Type of the transformation result when processing <see cref="ICondition" /></typeparam>
    /// <typeparam name="T2">Type of the transformation result when processing <see cref="ICalculusSource" /></typeparam>
    public abstract class BaseExpressionTransformerG<T, TR, T1, T2>
        : BaseConditionTransformerG<T, T1, T2>, IExpressionVisitor
    {
        /// <summary>
        /// Transforms the <see cref="IExpression" />.
        /// </summary>
        /// <param name="instance">The instance to tranform.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformed calculus source.</returns>
        public TR Transform(IExpression instance, T data)
        {
            return (TR)instance.Accept(this, data);
        }

        /// <summary>
        /// Postprocess for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The postprocessed transformation result</returns>
        protected virtual TR CommonPostTransform(TR transformed, IExpression toTransform, T data)
        {
            return transformed;
        }

        /// <summary>
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected abstract TR CommonFallbackTransform(IExpression toTransform, T data);

        /// <summary>
        /// Visits <see cref="ColumnExpression" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(ColumnExpression toVisit, object data)
        {
            var tData = (T)data;
            if(PreTransform(ref toVisit, tData))
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
        /// Process the <see cref="ColumnExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected abstract TR Transform(ColumnExpression toTransform, T data);

        /// <summary>
        /// Preprocess for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed</param>
        /// <param name="data">The passed data</param>
        /// <returns><c>true</c> if transformation should continue, <c>false</c> the fallback should be used.</returns>
        protected virtual bool PreTransform(ref ColumnExpression toTransform, T data)
        {
            return CommonPreTransform(ref toTransform, data);
        }

        /// <summary>
        /// Postprocess for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The postprocessed transformation result</returns>
        protected virtual TR PostTransform(TR transformed, ColumnExpression toTransform, T data)
        {
            return CommonPostTransform(transformed, toTransform, data);
        }

        /// <summary>
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected virtual TR FallbackTransform(ColumnExpression toTransform, T data)
        {
            return CommonFallbackTransform(toTransform, data);
        }

        /// <summary>
        /// Visits <see cref="ConcatenationExpression" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(ConcatenationExpression toVisit, object data)
        {
            var tData = (T)data;
            if(PreTransform(ref toVisit, tData))
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
        /// Process the <see cref="ConcatenationExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected abstract TR Transform(ConcatenationExpression toTransform, T data);

        /// <summary>
        /// Preprocess for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed</param>
        /// <param name="data">The passed data</param>
        /// <returns><c>true</c> if transformation should continue, <c>false</c> the fallback should be used.</returns>
        protected virtual bool PreTransform(ref ConcatenationExpression toTransform, T data)
        {
            return CommonPreTransform(ref toTransform, data);
        }

        /// <summary>
        /// Postprocess for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The postprocessed transformation result</returns>
        protected virtual TR PostTransform(TR transformed, ConcatenationExpression toTransform, T data)
        {
            return CommonPostTransform(transformed, toTransform, data);
        }

        /// <summary>
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected virtual TR FallbackTransform(ConcatenationExpression toTransform, T data)
        {
            return CommonFallbackTransform(toTransform, data);
        }

        /// <summary>
        /// Visits <see cref="ConstantExpression" />
        /// </summary>
        /// <param name="toVisit">The visited instance</param>
        /// <param name="data">The passed data</param>
        /// <returns>The returned data</returns>
        public object Visit(ConstantExpression toVisit, object data)
        {
            var tData = (T)data;
            if(PreTransform(ref toVisit, tData))
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
        /// Process the <see cref="ConstantExpression"/>
        /// </summary>
        /// <param name="toTransform">The instance to process</param>
        /// <param name="data">The passed data</param>
        /// <returns>The transformation result</returns>
        protected abstract TR Transform(ConstantExpression toTransform, T data);

        /// <summary>
        /// Preprocess for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed</param>
        /// <param name="data">The passed data</param>
        /// <returns><c>true</c> if transformation should continue, <c>false</c> the fallback should be used.</returns>
        protected virtual bool PreTransform(ref ConstantExpression toTransform, T data)
        {
            return CommonPreTransform(ref toTransform, data);
        }

        /// <summary>
        /// Postprocess for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The postprocessed transformation result</returns>
        protected virtual TR PostTransform(TR transformed, ConstantExpression toTransform, T data)
        {
            return CommonPostTransform(transformed, toTransform, data);
        }

        /// <summary>
        /// Fallback variant for the transformation.
        /// </summary>
        /// <param name="toTransform">Instance to be transformed.</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The transformation result</returns>
        protected virtual TR FallbackTransform(ConstantExpression toTransform, T data)
        {
            return CommonFallbackTransform(toTransform, data);
        }

    }
}
