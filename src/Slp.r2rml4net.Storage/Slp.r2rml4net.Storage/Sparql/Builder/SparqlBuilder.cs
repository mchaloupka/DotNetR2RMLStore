﻿using System;
using System.Collections.Generic;
using System.Linq;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Sparql.Algebra.Modifiers;
using Slp.r2rml4net.Storage.Sparql.Algebra.Patterns;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace Slp.r2rml4net.Storage.Sparql.Builder
{
    /// <summary>
    /// SPARQL algebra builder.
    /// </summary>
    public class SparqlBuilder
    {
        /// <summary>
        /// Processes the specified context.
        /// </summary>
        /// <param name="context">The query context.</param>
        /// <returns>The SPARQL query.</returns>
        /// <exception cref="System.Exception">Cannot handle unknown query type</exception>
        public ISparqlQuery Process(QueryContext context)
        {
            switch (context.OriginalQuery.QueryType)
            {
                case SparqlQueryType.Ask:
                    return ProcessAsk(context);
                case SparqlQueryType.Construct:
                    return ProcessConstruct(context);
                case SparqlQueryType.Describe:
                case SparqlQueryType.DescribeAll:
                    return ProcessDescribe(context);
                case SparqlQueryType.Select:
                case SparqlQueryType.SelectAll:
                case SparqlQueryType.SelectAllDistinct:
                case SparqlQueryType.SelectAllReduced:
                case SparqlQueryType.SelectDistinct:
                case SparqlQueryType.SelectReduced:
                    return ProcessSelect(context);
                default:
                    throw new Exception("Cannot handle unknown query type");
            }
        }

        /// <summary>
        /// Processes the ask query.
        /// </summary>
        /// <param name="context">The query context.</param>
        /// <returns>The SPARQL query.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private ISparqlQuery ProcessAsk(QueryContext context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Processes the construct query.
        /// </summary>
        /// <param name="context">The query context.</param>
        /// <returns>The SPARQL query.</returns>
        private ISparqlQuery ProcessConstruct(QueryContext context)
        {
            return ProcessAlgebra(context.OriginalAlgebra, context);
        }

        /// <summary>
        /// Processes the describe query.
        /// </summary>
        /// <param name="context">The query context.</param>
        /// <returns>The SPARQL query.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private ISparqlQuery ProcessDescribe(QueryContext context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Processes the select query.
        /// </summary>
        /// <param name="context">The query context.</param>
        /// <returns>The SPARQL query.</returns>
        private ISparqlQuery ProcessSelect(QueryContext context)
        {
            return ProcessAlgebra(context.OriginalAlgebra, context);
        }

        /// <summary>
        /// Processes the algebra.
        /// </summary>
        /// <param name="originalAlgebra">The original algebra.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The SPARQL query.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private ISparqlQuery ProcessAlgebra(ISparqlAlgebra originalAlgebra, QueryContext context)
        {
            if (originalAlgebra is Select)
            {
                var orSel = (Select)originalAlgebra;
                var innerAlgebra = ProcessAlgebra(orSel.InnerAlgebra, context);

                if (!orSel.IsSelectAll)
                {
                    return context.Optimizers.Optimize(new SelectModifier(innerAlgebra, orSel.SparqlVariables.Select(x => x.Name).ToList()));
                }
                else
                {
                    return context.Optimizers.Optimize(new SelectModifier(innerAlgebra, innerAlgebra.Variables));
                }
            }
            else if (originalAlgebra is IBgp)
            {
                var orBgp = (IBgp)originalAlgebra;
                return context.Optimizers.Optimize(ProcessTriplePatterns(orBgp.TriplePatterns, context));
            }
            else if (originalAlgebra is Union)
            {
                var orUnion = (Union)originalAlgebra;
                var left = (IGraphPattern)ProcessAlgebra(orUnion.Lhs, context);
                var right = (IGraphPattern)ProcessAlgebra(orUnion.Rhs, context);
                return context.Optimizers.Optimize(new UnionPattern(new IGraphPattern[] { left, right }));
            }

            throw new NotImplementedException();

            // http://www.dotnetrdf.org/api/~VDS.RDF.Query.Algebra.html
            //Ask
            //AskAnyTriples
            //AskBgp
            //AskUnion
            //BaseArbitraryLengthPathOperator
            //BaseMultiset
            //BasePathOperator
            //BaseSet
            //Bgp
            //Bindings
            //Distinct
            //ExistsJoin
            //Extend
            //Filter
            //FilteredProduct
            //FullTextQuery
            //Graph
            //GroupBy
            //GroupMultiset
            //Having
            //IdentityFilter
            //IdentityMultiset
            //Join
            //LazyBgp
            //LazyUnion
            //LeftJoin
            //Minus
            //Multiset
            //NegatedPropertySet
            //NullMultiset
            //NullOperator
            //OneOrMorePath
            //OrderBy
            //ParallelJoin
            //ParallelUnion
            //PartitionedMultiset
            //PropertyFunction
            //PropertyPath
            //Reduced
            //SameTermFilter
            //Select
            //SelectDistinctGraphs
            //Service
            //Set
            //SetDistinctnessComparer
            //SingleValueRestrictionFilter
            //Slice
            //SubQuery
            //Table
            //Union
            //VariableRestrictionFilter
            //ZeroLengthPath
            //ZeroOrMorePath

        }

        private IGraphPattern ProcessTriplePatterns(IEnumerable<ITriplePattern> triplePatterns, QueryContext context)
        {
            List<IGraphPattern> joinedQueries = new List<IGraphPattern>();

            foreach (var triplePattern in triplePatterns.OfType<VDS.RDF.Query.Patterns.TriplePattern>())
            {
                var processed = new Algebra.Patterns.TriplePattern(triplePattern.Subject, triplePattern.Predicate,
                    triplePattern.Object);
                joinedQueries.Add(processed);
            }

            IGraphPattern currentQuery;
            if (joinedQueries.Count == 0)
            {
                currentQuery = new EmptyPattern();
            }
            else if (joinedQueries.Count == 1)
            {
                currentQuery = joinedQueries[0];
            }
            else
            {
                currentQuery = new JoinPattern(joinedQueries);
            }

            // TODO:
            //    http://www.dotnetrdf.org/api/dotNetRDF~VDS.RDF.Query.Patterns.ITriplePattern.html
            //    //BindPattern
            //    //FilterPattern
            //    //LetPattern
            //    //PropertyFunctionPattern
            //    //PropertyPathPattern
            //    //SubQueryPattern
            //    //TriplePattern

            return context.Mapping.ProcessPattern(currentQuery, context);
        }
    }
}
