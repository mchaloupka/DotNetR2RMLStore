﻿using System.Collections.Generic;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using Slp.r2rml4net.Storage.Sparql.Algebra.Patterns;
using Slp.r2rml4net.Storage.Sparql.Utils;
using System.Linq;
using System.Reflection;
using TCode.r2rml4net.Mapping;

namespace Slp.r2rml4net.Storage.Mapping
{
    /// <summary>
    /// Mapping transformer
    /// </summary>
    public class MappingTransformer
        : BaseSparqlTransformer<QueryContext>
    {
        /// <summary>
        /// The mapping processor
        /// </summary>
        private readonly MappingProcessor _mappingProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingTransformer"/> class.
        /// </summary>
        /// <param name="mappingProcessor">The mapping processor.</param>
        public MappingTransformer(MappingProcessor mappingProcessor)
        {
            _mappingProcessor = mappingProcessor;
        }

        /// <summary>
        /// Processes the specified triple pattern.
        /// </summary>
        /// <param name="triplePattern">The triple pattern.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        protected override IGraphPattern Process(TriplePattern triplePattern, QueryContext data)
        {
            List<RestrictedTriplePattern> patterns = new List<RestrictedTriplePattern>();

            foreach (var tripleMap in _mappingProcessor.Mapping.TriplesMaps)
            {
                var subjectMap = tripleMap.SubjectMap;
                var graphMaps = subjectMap.GraphMaps.ToList();

                foreach (var predicateObjectMap in tripleMap.PredicateObjectMaps)
                {
                    var graphList = new List<IGraphMap>(graphMaps);
                    graphList.AddRange(predicateObjectMap.GraphMaps);

                    foreach (var predicateMap in predicateObjectMap.PredicateMaps)
                    {
                        foreach (var objectMap in predicateObjectMap.ObjectMaps)
                        {
                            ConstrainTriplePattern(triplePattern, patterns, tripleMap, subjectMap, predicateMap, objectMap,
                                graphList);
                        }
                        foreach (var refObjectMap in predicateObjectMap.RefObjectMaps)
                        {
                            ConstrainTriplePattern(triplePattern, patterns, tripleMap, subjectMap, predicateMap, refObjectMap,
                                graphList);
                        }
                    }
                }

                foreach (var classUri in subjectMap.Classes)
                {
                    ConstrainTriplePattern(triplePattern, patterns, tripleMap, subjectMap, classUri, graphMaps);
                }
            }

            if (patterns.Count == 0)
            {
                return new NotMatchingPattern(triplePattern.Variables);
            }
            else if (patterns.Count == 1)
            {
                return patterns[0];
            }
            else
            {
                return new UnionPattern(patterns);
            }
        }

        /// <summary>
        /// Constrains the triple pattern.
        /// </summary>
        /// <param name="triplePattern">The triple pattern.</param>
        /// <param name="patterns">The patterns.</param>
        /// <param name="tripleMap">The triple map.</param>
        /// <param name="subjectMap">The subject map.</param>
        /// <param name="classUri">The class URI.</param>
        /// <param name="graphMaps">The graph maps.</param>
        private void ConstrainTriplePattern(TriplePattern triplePattern, 
            List<RestrictedTriplePattern> patterns, ITriplesMap tripleMap, ISubjectMap subjectMap, 
            System.Uri classUri, List<IGraphMap> graphMaps)
        {
            if (graphMaps.Any())
            {
                patterns.AddRange(graphMaps.Select(graphMap => 
                    new RestrictedTriplePattern(triplePattern.SubjectPattern, 
                        triplePattern.PredicatePattern, triplePattern.ObjectPattern, tripleMap, 
                        subjectMap, new ClassPredicateMap(tripleMap.BaseUri), 
                        new ClassObjectMap(tripleMap.BaseUri, classUri), null, graphMap)));
            }
            else
            {
                patterns.Add(
                    new RestrictedTriplePattern(triplePattern.SubjectPattern,
                    triplePattern.PredicatePattern, triplePattern.ObjectPattern, tripleMap,
                    subjectMap, new ClassPredicateMap(tripleMap.BaseUri), 
                    new ClassObjectMap(tripleMap.BaseUri, classUri),
                    null, null));
            }
        }

        /// <summary>
        /// Constrains the triple pattern.
        /// </summary>
        /// <param name="triplePattern">The triple pattern.</param>
        /// <param name="patterns">The patterns.</param>
        /// <param name="tripleMap">The triple map.</param>
        /// <param name="subjectMap">The subject map.</param>
        /// <param name="predicateMap">The predicate map.</param>
        /// <param name="refObjectMap">The reference object map.</param>
        /// <param name="graphMaps">The graph maps.</param>
        private void ConstrainTriplePattern(TriplePattern triplePattern, 
            List<RestrictedTriplePattern> patterns, ITriplesMap tripleMap, ISubjectMap subjectMap, 
            IPredicateMap predicateMap, IRefObjectMap refObjectMap, List<IGraphMap> graphMaps)
        {
            if (graphMaps.Any())
            {
                patterns.AddRange(graphMaps.Select(graphMap =>
                    new RestrictedTriplePattern(triplePattern.SubjectPattern,
                        triplePattern.PredicatePattern, triplePattern.ObjectPattern, tripleMap,
                        subjectMap, predicateMap, null, refObjectMap, graphMap)));
            }
            else
            {
                patterns.Add(
                    new RestrictedTriplePattern(triplePattern.SubjectPattern,
                    triplePattern.PredicatePattern, triplePattern.ObjectPattern, tripleMap,
                    subjectMap, predicateMap, null, refObjectMap, null));
            }
        }

        /// <summary>
        /// Constrains the triple pattern.
        /// </summary>
        /// <param name="triplePattern">The triple pattern.</param>
        /// <param name="patterns">The patterns.</param>
        /// <param name="tripleMap">The triple map.</param>
        /// <param name="subjectMap">The subject map.</param>
        /// <param name="predicateMap">The predicate map.</param>
        /// <param name="objectMap">The object map.</param>
        /// <param name="graphMaps">The graph maps.</param>
        private void ConstrainTriplePattern(TriplePattern triplePattern, 
            List<RestrictedTriplePattern> patterns, ITriplesMap tripleMap, ISubjectMap subjectMap, 
            IPredicateMap predicateMap, IObjectMap objectMap, List<IGraphMap> graphMaps)
        {
            if (graphMaps.Any())
            {
                patterns.AddRange(graphMaps.Select(graphMap =>
                    new RestrictedTriplePattern(triplePattern.SubjectPattern,
                        triplePattern.PredicatePattern, triplePattern.ObjectPattern, tripleMap,
                        subjectMap, predicateMap, objectMap, null, graphMap)));
            }
            else
            {
                patterns.Add(
                    new RestrictedTriplePattern(triplePattern.SubjectPattern,
                    triplePattern.PredicatePattern, triplePattern.ObjectPattern, tripleMap,
                    subjectMap, predicateMap, objectMap, null, null));
            }
        }
    }
}