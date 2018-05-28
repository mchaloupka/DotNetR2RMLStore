﻿using System.Collections.Generic;
using System.Diagnostics;
using Slp.Evi.Storage.Mapping.Representation;
using TCode.r2rml4net.Mapping;
using VDS.RDF.Query.Patterns;

namespace Slp.Evi.Storage.Sparql.Algebra.Patterns
{
    /// <summary>
    /// Triple pattern
    /// </summary>
    public class RestrictedTriplePattern
        : IGraphPattern
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RestrictedTriplePattern"/> class.
        /// </summary>
        /// <param name="subjectPattern">The subject pattern.</param>
        /// <param name="predicatePattern">The predicate pattern.</param>
        /// <param name="objectPattern">The object pattern.</param>
        /// <param name="tripleMap">The triple map.</param>
        /// <param name="subjectMap">The subject map.</param>
        /// <param name="predicateMap">The predicate map.</param>
        /// <param name="objectMap">The object map.</param>
        /// <param name="refObjectMap">The reference object map</param>
        /// <param name="graphMap">The graph map.</param>
        public RestrictedTriplePattern(PatternItem subjectPattern, PatternItem predicatePattern,
            PatternItem objectPattern, ITriplesMapping tripleMap, ISubjectMapping subjectMap,
            IPredicateMapping predicateMap, IObjectMapping objectMap, IRefObjectMapping refObjectMap,
            IGraphMapping graphMap)
        {
            SubjectPattern = subjectPattern;
            PredicatePattern = predicatePattern;
            ObjectPattern = objectPattern;

            TripleMap = tripleMap;
            SubjectMap = subjectMap;
            PredicateMap = predicateMap;
            ObjectMap = objectMap;
            RefObjectMap = refObjectMap;
            GraphMap = graphMap;

            var variables = new List<string>();

            AddToVariables(SubjectPattern, variables);
            AddToVariables(PredicatePattern, variables);
            AddToVariables(ObjectPattern, variables);

            Variables = variables;
        }

        /// <summary>
        /// Gets the graph map.
        /// </summary>
        /// <value>The graph map.</value>
        public IGraphMapping GraphMap { get; private set; }

        /// <summary>
        /// Gets the object map.
        /// </summary>
        /// <value>The object map.</value>
        public IObjectMapping ObjectMap { get; private set; }

        /// <summary>
        /// Gets the reference object map.
        /// </summary>
        /// <value>The reference object map.</value>
        public IRefObjectMapping RefObjectMap { get; private set; }

        /// <summary>
        /// Gets the predicate map.
        /// </summary>
        /// <value>The predicate map.</value>
        public IPredicateMapping PredicateMap { get; private set; }

        /// <summary>
        /// Gets or sets the subject map.
        /// </summary>
        /// <value>The subject map.</value>
        public ISubjectMapping SubjectMap { get; private set; }

        /// <summary>
        /// Gets the triple map.
        /// </summary>
        /// <value>The triple map.</value>
        public ITriplesMapping TripleMap { get; private set; }

        /// <summary>
        /// Adds to variables list.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <param name="variables">The variables list.</param>
        private void AddToVariables(PatternItem pattern, List<string> variables)
        {
            if (pattern is VariablePattern variablePattern)
            {
                variables.Add(variablePattern.VariableName);
            }
        }

        /// <summary>
        /// Gets the SPARQL variables.
        /// </summary>
        /// <value>The variables.</value>
        public IEnumerable<string> Variables { get; }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(IGraphPatternVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <summary>
        /// Gets the subject pattern.
        /// </summary>
        /// <value>The subject pattern.</value>
        public PatternItem SubjectPattern { get; }

        /// <summary>
        /// Gets the predicate pattern.
        /// </summary>
        /// <value>The predicate pattern.</value>
        public PatternItem PredicatePattern { get; }

        /// <summary>
        /// Gets the object pattern.
        /// </summary>
        /// <value>The object pattern.</value>
        public PatternItem ObjectPattern { get; }

        /// <summary>
        /// Gets the set of always bound variables.
        /// </summary>
        public IEnumerable<string> AlwaysBoundVariables => Variables;
    }
}
