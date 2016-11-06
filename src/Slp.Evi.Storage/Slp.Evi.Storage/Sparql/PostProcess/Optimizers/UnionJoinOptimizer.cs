﻿using System;
using System.Collections.Generic;
using System.Linq;
using Slp.Evi.Storage.Common.Optimization.PatternMatching;
using Slp.Evi.Storage.Relational.Query.ValueBinders;
using Slp.Evi.Storage.Sparql.Algebra;
using Slp.Evi.Storage.Sparql.Algebra.Patterns;
using Slp.Evi.Storage.Sparql.Types;
using Slp.Evi.Storage.Sparql.Utils.CodeGeneration;
using Slp.Evi.Storage.Utils;
using TCode.r2rml4net.Mapping;
using VDS.RDF.Query.Patterns;
using SlpPatternItem = Slp.Evi.Storage.Common.Optimization.PatternMatching.PatternItem;
using PatternItem = VDS.RDF.Query.Patterns.PatternItem;

namespace Slp.Evi.Storage.Sparql.PostProcess.Optimizers
{
    /// <summary>
    /// The union / join optimization
    /// </summary>
    public class UnionJoinOptimizer
        : BaseSparqlOptimizer<object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnionJoinOptimizer"/> class.
        /// </summary>
        public UnionJoinOptimizer() 
            : base(new UnionJoinOptimizerImplementation())
        { }

        /// <summary>
        /// The implementation class for <see cref="UnionJoinOptimizer"/>
        /// </summary>
        public class UnionJoinOptimizerImplementation
            : BaseSparqlOptimizerImplementation<object>
        {
            /// <summary>
            /// Process the <see cref="JoinPattern"/>
            /// </summary>
            /// <param name="toTransform">The instance to process</param>
            /// <param name="data">The passed data</param>
            /// <returns>The transformation result</returns>
            protected override IGraphPattern Transform(JoinPattern toTransform, OptimizationContext data)
            {
                List<IGraphPattern> childPatterns = new List<IGraphPattern>();
                List<UnionPattern> childUnionPatterns = new List<UnionPattern>();


                foreach (var pattern in toTransform.JoinedGraphPatterns)
                { 
                    ProcessJoinChild(childPatterns, childUnionPatterns, pattern);
                }

                var cartesianProducts = CreateCartesian(childPatterns, childUnionPatterns,
                    data);

                List<JoinPattern> resultJoinPatterns = cartesianProducts.Select(cartesianProduct => new JoinPattern(cartesianProduct.ToList())).ToList();

                if (resultJoinPatterns.Count == 0)
                {
                    return new NotMatchingPattern();
                }
                else if (resultJoinPatterns.Count == 1)
                {
                    return resultJoinPatterns[0];
                }
                else
                {
                    return new UnionPattern(resultJoinPatterns);
                }
            }

            /// <summary>
            /// Processes children of the <see cref="JoinPattern"/>
            /// </summary>
            /// <param name="childPatterns">All child patterns of a type different from <see cref="UnionPattern"/> and <see cref="JoinPattern"/></param>
            /// <param name="childUnionPatterns">All child patterns of a type <see cref="UnionPattern"/></param>
            /// <param name="joinedGraphPattern">The child to be processed</param>
            private void ProcessJoinChild(List<IGraphPattern> childPatterns, List<UnionPattern> childUnionPatterns, IGraphPattern joinedGraphPattern)
            {
                if (joinedGraphPattern is UnionPattern)
                {
                    childUnionPatterns.Add((UnionPattern)joinedGraphPattern);
                }
                else if (joinedGraphPattern is JoinPattern)
                {
                    foreach (var innerJoinedGraphPattern in ((JoinPattern)joinedGraphPattern).JoinedGraphPatterns)
                    {
                        ProcessJoinChild(childPatterns, childUnionPatterns, innerJoinedGraphPattern);
                    }
                }
                else
                {
                    childPatterns.Add(joinedGraphPattern);
                }
            }

            /// <summary>
            /// Creates the Cartesian product
            /// </summary>
            /// <param name="childPatterns">All child patterns of a type different from <see cref="UnionPattern"/> and <see cref="JoinPattern"/></param>
            /// <param name="childUnionPatterns">All child patterns of a type <see cref="UnionPattern"/></param>
            /// <param name="data">The context</param>
            /// <returns></returns>
            private IEnumerable<IEnumerable<IGraphPattern>> CreateCartesian(List<IGraphPattern> childPatterns, List<UnionPattern> childUnionPatterns, OptimizationContext data)
            {
                var leftCartesian = new CartesianResult();
                bool leftOk = true;

                foreach (var childPattern in childPatterns)
                {
                    if (childPattern is RestrictedTriplePattern)
                    {
                        var triplePattern = (RestrictedTriplePattern) childPattern;

                        if (!leftCartesian.VerifyTriplePattern(triplePattern, data))
                        {
                            leftOk = false;
                            break;
                        }

                        leftCartesian.AddTriplePatternInfo(triplePattern);
                    }

                    leftCartesian.Queries.Add(childPattern);
                }

                var currentCartesians = new List<CartesianResult>();

                if (leftOk)
                {
                    currentCartesians.Add(leftCartesian);

                    currentCartesians = childUnionPatterns
                        .Aggregate(currentCartesians, 
                        (current, childUnionPattern) => ProcessCartesian(current, childUnionPattern, data));
                }

                return currentCartesians.Select(x => x.Queries);
            }

            /// <summary>
            /// Processes the current Cartesian product
            /// </summary>
            /// <param name="currentCartesians">Current Cartesian products</param>
            /// <param name="childUnionPattern">The <see cref="UnionPattern"/> to process</param>
            /// <param name="data">The context</param>
            /// <returns></returns>
            private List<CartesianResult> ProcessCartesian(List<CartesianResult> currentCartesians, UnionPattern childUnionPattern, OptimizationContext data)
            {
                List<CartesianResult> result = new List<CartesianResult>();

                foreach (var currentCartesian in currentCartesians)
                {
                    foreach (var unionedGraphPattern in childUnionPattern.UnionedGraphPatterns)
                    {
                        var triplePattern = unionedGraphPattern as RestrictedTriplePattern;
                        if (triplePattern != null)
                        {
                            if (!currentCartesian.VerifyTriplePattern(triplePattern, data))
                            {
                                continue;
                            }
                        }

                        var newCartesian = currentCartesian.Clone();

                        if (triplePattern != null)
                        {
                            newCartesian.AddTriplePatternInfo(triplePattern);
                        }

                        newCartesian.Queries.Add(unionedGraphPattern);
                        result.Add(newCartesian);
                    }
                }

                return result;
            }

            /// <summary>
            /// Cartesian result
            /// </summary>
            private class CartesianResult
            {
                /// <summary>
                /// Initializes a new instance of the <see cref="CartesianResult"/> class.
                /// </summary>
                public CartesianResult()
                {
                    _variables = new Dictionary<string, List<ITermMap>>();
                    Queries = new List<IGraphPattern>();
                    _patternComparer = new PatternComparer();
                }

                /// <summary>
                /// Clones this instance.
                /// </summary>
                /// <returns>The cloned instance.</returns>
                public CartesianResult Clone()
                {
                    var cr = new CartesianResult();

                    foreach (var q in Queries)
                    {
                        cr.Queries.Add(q);
                    }

                    foreach (var variable in _variables.Keys)
                    {
                        cr._variables[variable] = new List<ITermMap>();

                        foreach (var termMap in _variables[variable])
                        {
                            cr._variables[variable].Add(termMap);
                        }
                    }

                    return cr;
                }

                /// <summary>
                /// Gets the variables mappings.
                /// </summary>
                /// <value>The variables.</value>
                private readonly Dictionary<string, List<ITermMap>> _variables;

                /// <summary>
                /// Gets the queries.
                /// </summary>
                /// <value>The queries.</value>
                public List<IGraphPattern> Queries { get; }

                /// <summary>
                /// Adds the information from the passed <see cref="RestrictedTriplePattern"/> to the <see cref="CartesianResult"/>
                /// </summary>
                /// <param name="triplePattern">The passed <see cref="RestrictedTriplePattern"/> to process</param>
                public void AddTriplePatternInfo(RestrictedTriplePattern triplePattern)
                {
                    AddPatternInfo(triplePattern.SubjectPattern, triplePattern.SubjectMap);
                    AddPatternInfo(triplePattern.PredicatePattern, triplePattern.PredicateMap);

                    if (triplePattern.ObjectMap != null)
                    {
                        AddPatternInfo(triplePattern.ObjectPattern, triplePattern.ObjectMap);
                    }
                    else if (triplePattern.RefObjectMap != null)
                    {
                        var parentMap = triplePattern.RefObjectMap.ParentTriplesMap;
                        AddPatternInfo(triplePattern.ObjectPattern, parentMap.SubjectMap);
                    }
                    else
                    {
                        throw new Exception("There must be an object map or ref object map");
                    }
                }

                /// <summary>
                /// Adds the pattern information
                /// </summary>
                /// <param name="pattern">The pattern</param>
                /// <param name="termMap">The mapping for the pattern</param>
                private void AddPatternInfo(PatternItem pattern, ITermMap termMap)
                {
                    if (pattern is VariablePattern)
                    {
                        AddVariableInfo(((VariablePattern)pattern).VariableName, termMap);
                    }
                    else if (pattern is BlankNodePattern)
                    {
                        AddVariableInfo(((BlankNodePattern)pattern).ID, termMap);
                    }
                }

                /// <summary>
                /// Adds the information for the variable
                /// </summary>
                /// <param name="variableName">The variable name</param>
                /// <param name="termMap">The mapping for the variable</param>
                private void AddVariableInfo(string variableName, ITermMap termMap)
                {
                    if (!_variables.ContainsKey(variableName))
                    {
                        _variables.Add(variableName, new List<ITermMap>());
                    }

                    _variables[variableName].Add(termMap);
                }

                /// <summary>
                /// Verifies whether the <see cref="RestrictedTriplePattern"/> can be added to the <see cref="CartesianResult"/>
                /// </summary>
                /// <param name="triplePattern">The passed <see cref="RestrictedTriplePattern"/> to process</param>
                /// <param name="data">The context</param>
                /// <returns>Returns <c>true</c> if the <paramref name="triplePattern"/> can be added to the <see cref="CartesianResult"/>; <c>false</c> otherwise</returns>
                public bool VerifyTriplePattern(RestrictedTriplePattern triplePattern, OptimizationContext data)
                {
                    bool ok =
                        VerifyPatternInfo(triplePattern.SubjectPattern, triplePattern.SubjectMap, data.Context.TypeCache)
                        && VerifyPatternInfo(triplePattern.PredicatePattern, triplePattern.PredicateMap, data.Context.TypeCache);

                    if (ok)
                    {
                        if (triplePattern.ObjectMap != null)
                        {
                            ok = VerifyPatternInfo(triplePattern.ObjectPattern, triplePattern.ObjectMap, data.Context.TypeCache);
                        }
                        else if (triplePattern.RefObjectMap != null)
                        {
                            var parentMap = triplePattern.RefObjectMap.ParentTriplesMap;
                            ok = VerifyPatternInfo(triplePattern.ObjectPattern, parentMap.SubjectMap, data.Context.TypeCache);
                        }
                        else
                        {
                            throw new Exception("There must be an object map or ref object map");
                        }
                    }

                    return ok;
                }

                /// <summary>
                /// Verifies the pattern information
                /// </summary>
                /// <param name="pattern">The pattern</param>
                /// <param name="termMap">The mapping for the pattern</param>
                /// <param name="typeCache">The type cache</param>
                private bool VerifyPatternInfo(PatternItem pattern, ITermMap termMap, TypeCache typeCache)
                {
                    if (pattern is VariablePattern)
                    {
                        return VerifyVariableInfo(((VariablePattern)pattern).VariableName, termMap, typeCache);
                    }
                    else if (pattern is BlankNodePattern)
                    {
                        return VerifyVariableInfo(((BlankNodePattern) pattern).ID, termMap, typeCache);
                    }
                    else
                    {
                        return true;
                    }
                }

                /// <summary>
                /// Verifies the information for the variable
                /// </summary>
                /// <param name="variableName">The variable name</param>
                /// <param name="termMap">The mapping for the variable</param>
                /// <param name="typeCache">The type cache</param>
                private bool VerifyVariableInfo(string variableName, ITermMap termMap, TypeCache typeCache)
                {
                    if (_variables.ContainsKey(variableName))
                    {
                        var storedTermMaps = _variables[variableName];

                        foreach (var storedTermMap in storedTermMaps)
                        {
                            if (!CanMatch(termMap, storedTermMap, typeCache))
                            {
                                return false;
                            }
                        }
                    }

                    return true;
                }

                /// <summary>
                /// The template processor
                /// </summary>
                private readonly TemplateProcessor _templateProcessor = new TemplateProcessor();

                private readonly PatternComparer _patternComparer;

                /// <summary>
                /// Determines whether the first mapping can match the second mapping.
                /// </summary>
                /// <param name="first">The first mapping.</param>
                /// <param name="second">The second mapping.</param>
                /// <param name="typeCache">The type cache</param>
                /// <returns><c>true</c> if first mapping can match the second one; otherwise, <c>false</c>.</returns>
                private bool CanMatch(ITermMap first, ITermMap second, TypeCache typeCache)
                {
                    //var firstType = typeCache.GetValueType(first);
                    //var secondType = typeCache.GetValueType(second);

                    //if (firstType != secondType)
                    //{
                    //    return false;
                    //}

                    var result = CanMatchFunction(first,
                            constantUriFunc: x => CanMatch(x, second),
                            constantLiteralFunc: x => CanMatch(x, second),
                            columnFunc: x => CanColumnMatch(x, second),
                            templateFunc: x => CanTemplateMatch(x, second));

                    return result;
                }

                /// <summary>
                /// Determines whether the first template mapping can match the second mapping.
                /// </summary>
                /// <param name="first">The first template mapping.</param>
                /// <param name="second">The second mapping.</param>
                /// <returns><c>true</c> if first mapping can match the second one; otherwise, <c>false</c>.</returns>
                private bool CanTemplateMatch(ITermMap first, ITermMap second)
                {
                    return CanMatchFunction(second,
                        constantUriFunc: x => CanMatchTemplate(x, first),
                        constantLiteralFunc: x => CanMatchTemplate(x, first),
                        columnFunc: x => CanTemplateMatchColumn(first, x),
                        templateFunc: x => CanTemplatesMatch(first, x));
                }

                /// <summary>
                /// Determines whether the first column mapping can match the second mapping.
                /// </summary>
                /// <param name="first">The first column mapping.</param>
                /// <param name="second">The second mapping.</param>
                /// <returns><c>true</c> if first mapping can match the second one; otherwise, <c>false</c>.</returns>
                private bool CanColumnMatch(ITermMap first, ITermMap second)
                {
                    return CanMatchFunction(second,
                        constantUriFunc: x => CanMatchColumn(x, first),
                        constantLiteralFunc: x => CanMatchColumn(x, first),
                        columnFunc: x => CanColumnsMatch(first, x),
                        templateFunc: x => CanTemplateMatchColumn(x, first));
                }

                /// <summary>
                /// Determines whether the first literal mapping can match the second mapping.
                /// </summary>
                /// <param name="literal">The literal mapping.</param>
                /// <param name="second">The second mapping.</param>
                /// <returns><c>true</c> if first mapping can match the second one; otherwise, <c>false</c>.</returns>
                private bool CanMatch(string literal, ITermMap second)
                {
                    return CanMatchFunction(second,
                        constantUriFunc: x => CanMatch(literal, x),
                        constantLiteralFunc: x => CanMatch(literal, x),
                        columnFunc: x => CanMatchColumn(literal, x),
                        templateFunc: x => CanMatchTemplate(literal, x)
                        );
                }

                /// <summary>
                /// Determines whether the first URI mapping can match the second mapping.
                /// </summary>
                /// <param name="uri">The first URI mapping.</param>
                /// <param name="second">The second mapping.</param>
                /// <returns><c>true</c> if first mapping can match the second one; otherwise, <c>false</c>.</returns>
                private bool CanMatch(Uri uri, ITermMap second)
                {
                    return CanMatchFunction(second,
                        constantUriFunc: x => CanMatch(uri, x),
                        constantLiteralFunc: x => CanMatch(x, uri),
                        columnFunc: x => CanMatchColumn(uri, x),
                        templateFunc: x => CanMatchTemplate(uri, x));
                }

                /// <summary>
                /// Determines whether the URIs can match.
                /// </summary>
                /// <param name="firstUri">The first URI.</param>
                /// <param name="secondUri">The second URI.</param>
                /// <returns><c>true</c> if the URIs can match; otherwise, <c>false</c>.</returns>
                private bool CanMatch(Uri firstUri, Uri secondUri)
                {
                    return firstUri.UriEquals(secondUri);
                }

                /// <summary>
                /// Determines whether the literals can match.
                /// </summary>
                /// <param name="firstLiteral">The first literal.</param>
                /// <param name="secondLiteral">The second literal.</param>
                /// <returns><c>true</c> if the literals can match; otherwise, <c>false</c>.</returns>
                private bool CanMatch(string firstLiteral, string secondLiteral)
                {
                    return firstLiteral == secondLiteral;
                }

                /// <summary>
                /// Determines whether the literal can match uri.
                /// </summary>
                /// <param name="literal">The first literal.</param>
                /// <param name="uri">The second URI.</param>
                /// <returns><c>false</c>.</returns>
                private bool CanMatch(string literal, Uri uri)
                {
                    return false;
                }

                /// <summary>
                /// Determines whether the template mappings can match.
                /// </summary>
                /// <param name="firstTemplateTermMap">The first template mapping.</param>
                /// <param name="secondTemplateTermMap">The second template mapping.</param>
                /// <returns><c>true</c> if the template mappings can match; otherwise, <c>false</c>.</returns>
                private bool CanTemplatesMatch(ITermMap firstTemplateTermMap, ITermMap secondTemplateTermMap)
                {
                    if ((firstTemplateTermMap.TermType.IsLiteral && secondTemplateTermMap.TermType.IsURI) || (firstTemplateTermMap.TermType.IsURI && secondTemplateTermMap.TermType.IsLiteral))
                        return false;

                    return TemplatesMatchCheck(firstTemplateTermMap.Template, secondTemplateTermMap.Template, firstTemplateTermMap.TermType.IsURI);
                }

                /// <summary>
                /// Determines whether the template mapping can match the column mapping.
                /// </summary>
                /// <param name="firstTemplateTermMap">The first template mapping.</param>
                /// <param name="secondColumnTermMap">The second column mapping.</param>
                /// <returns><c>true</c> if the mappings can match; otherwise, <c>false</c>.</returns>
                private bool CanTemplateMatchColumn(ITermMap firstTemplateTermMap, ITermMap secondColumnTermMap)
                {
                    if ((firstTemplateTermMap.TermType.IsLiteral && secondColumnTermMap.TermType.IsURI) || (firstTemplateTermMap.TermType.IsURI && secondColumnTermMap.TermType.IsLiteral))
                        return false;

                    return true;
                }

                /// <summary>
                /// Determines whether the literal can match the template mapping.
                /// </summary>
                /// <param name="literal">The literal.</param>
                /// <param name="templateTermMap">The template mapping.</param>
                /// <returns><c>true</c> if the mappings can match; otherwise, <c>false</c>.</returns>
                private bool CanMatchTemplate(string literal, ITermMap templateTermMap)
                {
                    if (templateTermMap.TermType.IsURI)
                        return false;

                    return TemplateMatchCheck(templateTermMap.Template, literal, false);
                }

                /// <summary>
                /// Determines whether the URI can match the template mapping.
                /// </summary>
                /// <param name="uri">The URI.</param>
                /// <param name="templateTermMap">The template mapping.</param>
                /// <returns><c>true</c> if the mappings can match; otherwise, <c>false</c>.</returns>
                private bool CanMatchTemplate(Uri uri, ITermMap templateTermMap)
                {
                    if (templateTermMap.TermType.IsLiteral)
                        return false;

                    return TemplateMatchCheck(templateTermMap.Template, uri.ToString(), true);
                }

                /// <summary>
                /// Determines whether the column mappings can match.
                /// </summary>
                /// <param name="firstColumnTermMap">The first column mapping.</param>
                /// <param name="secondColumnTermMap">The second column mapping.</param>
                /// <returns><c>true</c> if the mappings can match; otherwise, <c>false</c>.</returns>
                private bool CanColumnsMatch(ITermMap firstColumnTermMap, ITermMap secondColumnTermMap)
                {
                    bool ok = !(firstColumnTermMap.TermType.IsLiteral && secondColumnTermMap.TermType.IsURI) || (firstColumnTermMap.TermType.IsURI && secondColumnTermMap.TermType.IsLiteral);

                    // NOTE: Maybe type checking

                    return ok;
                }

                /// <summary>
                /// Determines whether the literal can match the column mapping.
                /// </summary>
                /// <param name="literal">The literal.</param>
                /// <param name="columnTermMap">The column mapping.</param>
                /// <returns><c>true</c> if the mappings can match; otherwise, <c>false</c>.</returns>
                private bool CanMatchColumn(string literal, ITermMap columnTermMap)
                {
                    if (columnTermMap.TermType.IsLiteral)
                        return true;
                    else
                        return false;
                }

                /// <summary>
                /// Determines whether the URI can match the column mapping.
                /// </summary>
                /// <param name="uri">The URI.</param>
                /// <param name="columnTermMap">The column mapping.</param>
                /// <returns><c>true</c> if the mappings can match; otherwise, <c>false</c>.</returns>
                private bool CanMatchColumn(Uri uri, ITermMap columnTermMap)
                {
                    if (columnTermMap.TermType.IsURI)
                        return true;
                    else
                        return false;
                }

                /// <summary>
                /// The can match function.
                /// </summary>
                /// <typeparam name="T"></typeparam>
                /// <param name="decider">The mapping chosen as decider which function will be used.</param>
                /// <param name="constantUriFunc">The constant URI function.</param>
                /// <param name="constantLiteralFunc">The constant literal function.</param>
                /// <param name="columnFunc">The column function.</param>
                /// <param name="templateFunc">The template function.</param>
                /// <exception cref="System.Exception">
                /// Object map must be an IRI or Literal
                /// or
                /// Constant value term must be uri valued or an object map
                /// or
                /// Term must be constant, column or template valued
                /// </exception>
                private T CanMatchFunction<T>(ITermMap decider, Func<Uri, T> constantUriFunc, Func<string, T> constantLiteralFunc, Func<ITermMap, T> columnFunc, Func<ITermMap, T> templateFunc)
                {
                    if (decider.IsConstantValued)
                    {
                        if (decider is IUriValuedTermMap)
                        {
                            var uri = ((IUriValuedTermMap)decider).URI;
                            return constantUriFunc(uri);
                        }
                        else if (decider is IObjectMap)
                        {
                            var oMap = (IObjectMap)decider;

                            if (oMap.URI != null)
                            {
                                return constantUriFunc(oMap.URI);
                            }
                            else if (oMap.Literal != null)
                            {
                                return constantLiteralFunc(oMap.Literal);
                            }
                            else
                                throw new Exception("Object map must be an IRI or Literal");
                        }
                        else
                            throw new Exception("Constant value term must be uri valued or an object map");
                    }
                    else if (decider.IsColumnValued)
                    {
                        return columnFunc(decider);
                    }
                    else if (decider.IsTemplateValued)
                    {
                        return templateFunc(decider);
                    }
                    else
                        throw new Exception("Term must be constant, column or template valued");
                }

                /// <summary>
                /// Can the template match the value.
                /// </summary>
                /// <param name="template">The template.</param>
                /// <param name="value">The value.</param>
                /// <param name="isIri">if set to <c>true</c> the values are IRI.</param>
                /// <returns><c>true</c> if the template can match the value, <c>false</c> otherwise.</returns>
                private bool TemplateMatchCheck(string template, string value, bool isIri)
                {
                    var templateParts =
                        _templateProcessor.ParseTemplate(template)
                        .Select(SlpPatternItem.FromTemplatePart);

                    var leftPattern = new Pattern(isIri, templateParts);
                    var rightPattern = new Pattern(isIri, new[] {new SlpPatternItem(value)});

                    var result = _patternComparer.Compare(leftPattern, rightPattern);
                    return !result.NeverMatch;
                }

                /// <summary>
                /// Can the templates match.
                /// </summary>
                /// <param name="firstTemplate">The first template.</param>
                /// <param name="secondTemplate">The second template.</param>
                /// <param name="isIri">if set to <c>true</c> the values are IRI.</param>
                /// <returns><c>true</c> if the templates can match, <c>false</c> otherwise.</returns>
                private bool TemplatesMatchCheck(string firstTemplate, string secondTemplate, bool isIri)
                {
                    var leftTemplateParts =
                       _templateProcessor.ParseTemplate(firstTemplate)
                       .Select(SlpPatternItem.FromTemplatePart);

                    var leftPattern = new Pattern(isIri, leftTemplateParts);

                    var rightTemplateParts =
                       _templateProcessor.ParseTemplate(secondTemplate)
                       .Select(SlpPatternItem.FromTemplatePart);

                    var rightPattern = new Pattern(isIri, rightTemplateParts);

                    var result = _patternComparer.Compare(leftPattern, rightPattern);
                    return !result.NeverMatch;
                }
            }
        }
    }
}
