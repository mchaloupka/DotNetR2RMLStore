﻿using Slp.Evi.Storage.Query;
using Slp.Evi.Storage.Sparql.Algebra;
using Slp.Evi.Storage.Sparql.Utils;
using Slp.Evi.Storage.Sparql.Utils.CodeGeneration;

namespace Slp.Evi.Storage.Sparql.PostProcess.Optimizers
{
    /// <summary>
    /// The base class for SPARQL optimization
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseSparqlOptimizer<T>
        : BaseSparqlTransformer<BaseSparqlOptimizer<T>.OptimizationContext>, ISparqlPostProcess
    {
        /// <summary>
        /// The optimizer implementation
        /// </summary>
        private readonly BaseSparqlOptimizerImplementation<T> _optimizerImplementation;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseSparqlOptimizer{T}"/> class.
        /// </summary>
        /// <param name="optimizerImplementation">The optimizer implementation.</param>
        protected BaseSparqlOptimizer(BaseSparqlOptimizerImplementation<T> optimizerImplementation)
        {
            _optimizerImplementation = optimizerImplementation;
        }

        /// <summary>
        /// Gets the optimizer implementation.
        /// </summary>
        /// <value>The optimizer implementation.</value>
        protected BaseSparqlOptimizerImplementation<T> OptimizerImplementation => _optimizerImplementation;

        /// <summary>
        /// The optimization context
        /// </summary>
        public class OptimizationContext
        {
            /// <summary>
            /// Gets or sets the query context.
            /// </summary>
            public IQueryContext Context { get; set; }

            /// <summary>
            /// Gets or sets the data.
            /// </summary>
            public T Data { get; set; }
        }

        /// <summary>
        /// Optimizes the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="context">The context.</param>
        public ISparqlQuery Process(ISparqlQuery query, IQueryContext context)
        {
            return TransformSparqlQuery(query, new OptimizationContext()
            {
                Context = context,
                Data = CreateInitialData()
            });
        }

        /// <summary>
        /// Creates the initial data.
        /// </summary>
        protected virtual T CreateInitialData()
        {
            return default(T);
        }

        /// <summary>
        /// Postprocess for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The postprocessed transformation result</returns>
        protected override IGraphPattern CommonPostTransform(IGraphPattern transformed, IGraphPattern toTransform, OptimizationContext data)
        {
            return base.CommonPostTransform(_optimizerImplementation.TransformGraphPattern(transformed, data), toTransform, data);
        }

        /// <summary>
        /// Postprocess for the transformation.
        /// </summary>
        /// <param name="transformed">The transformation result.</param>
        /// <param name="toTransform">The transformed instance</param>
        /// <param name="data">The passed data.</param>
        /// <returns>The postprocessed transformation result</returns>
        protected override ISparqlQuery CommonPostTransform(ISparqlQuery transformed, IModifier toTransform, OptimizationContext data)
        {
            return base.CommonPostTransform(_optimizerImplementation.TransformSparqlQuery(transformed, data), toTransform, data);
        }
    }
}
