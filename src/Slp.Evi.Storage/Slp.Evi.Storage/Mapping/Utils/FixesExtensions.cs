﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TCode.r2rml4net.Mapping;
using VDS.RDF;

namespace Slp.Evi.Storage.Mapping.Utils
{
    /// <summary>
    /// Extension class for r2rml4net fixes
    /// </summary>
    public static class FixesExtensions
    {
        private const string RrPrefix = "http://www.w3.org/ns/r2rml#";
        private const string RrJoinCondition = RrPrefix + "joinCondition";
        private const string RrChild = RrPrefix + "child";
        private const string RrParent = RrPrefix + "parent";

        /// <summary>
        /// Gets the join conditions.
        /// </summary>
        /// <param name="refObjectPattern">The reference object pattern.</param>
        /// <returns>IEnumerable&lt;JoinCondition&gt;.</returns>
        public static IEnumerable<JoinCondition> GetJoinConditions(this IRefObjectMap refObjectPattern)
        {
            var mapping = (IGraph)refObjectPattern.GetType().GetProperty("R2RMLMappings", BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic).GetValue(refObjectPattern);
            var node = refObjectPattern.Node;

            var joinConditions = mapping.GetTriplesWithSubject(node).WithPredicate(mapping.CreateUriNode(new Uri(RrJoinCondition))).Select(x => x.Object);

            foreach (var joinCondition in joinConditions)
            {
                var inner = mapping.GetTriplesWithSubject(joinCondition);
                var child = inner.WithPredicate(mapping.CreateUriNode(new Uri(RrChild))).Select(x => x.Object).OfType<ILiteralNode>().Select(x => x.Value).First();
                var parent = inner.WithPredicate(mapping.CreateUriNode(new Uri(RrParent))).Select(x => x.Object).OfType<ILiteralNode>().Select(x => x.Value).First();

                yield return new JoinCondition(child, parent);
            }


            //return refObjectPattern.JoinConditions;
        }
    }
}
