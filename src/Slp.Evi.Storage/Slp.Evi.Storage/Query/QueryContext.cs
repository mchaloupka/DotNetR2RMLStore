﻿using System;
using System.Collections.Generic;
using Slp.r2rml4net.Storage.Bootstrap;
using Slp.r2rml4net.Storage.Database;
using Slp.r2rml4net.Storage.Database.Base;
using Slp.r2rml4net.Storage.DBSchema;
using Slp.r2rml4net.Storage.Mapping;
using Slp.r2rml4net.Storage.Relational.Query;
using Slp.r2rml4net.Storage.Sparql.Algebra;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;

namespace Slp.r2rml4net.Storage.Query
{
    /// <summary>
    /// The query context.
    /// </summary>
    public class QueryContext
    {
        /// <summary>
        /// The blank nodes subjects.
        /// </summary>
        private readonly Dictionary<string, INode> _blankNodesSubjects;

        /// <summary>
        /// The blank nodes objects.
        /// </summary>
        private readonly Dictionary<string, INode> _blankNodesObjects;

        /// <summary>
        /// The used variables
        /// </summary>
        private readonly HashSet<string> _usedVariables;

        /// <summary>
        /// The query naming helpers
        /// </summary>
        private readonly QueryNamingHelpers _queryNamingHelpers;

        /// <summary>
        /// The optimizers
        /// </summary>
        private readonly Optimizers _optimizers;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryContext" /> class.
        /// </summary>
        /// <param name="originalQuery">The original query.</param>
        /// <param name="mapping">The mapping.</param>
        /// <param name="db">The database.</param>
        /// <param name="schemaProvider">The schema provider.</param>
        /// <param name="nodeFactory">The node factory.</param>
        /// <param name="factory">The storage factory</param>
        public QueryContext(SparqlQuery originalQuery, MappingProcessor mapping, ISqlDatabase db, IDbSchemaProvider schemaProvider, INodeFactory nodeFactory, IR2RMLStorageFactory factory)
        {
            OriginalQuery = originalQuery;
            OriginalAlgebra = originalQuery.ToAlgebra();
            NodeFactory = nodeFactory;
            Db = db;
            Mapping = mapping;
            SchemaProvider = schemaProvider;
            _blankNodesSubjects = new Dictionary<string, INode>();
            _blankNodesObjects = new Dictionary<string, INode>();
            _usedVariables = new HashSet<string>(OriginalAlgebra.Variables);
            _queryNamingHelpers = new QueryNamingHelpers(this);
            _optimizers = new Optimizers(factory, this);
        }

        /// <summary>
        /// Gets the schema provider.
        /// </summary>
        /// <value>The schema provider.</value>
        public IDbSchemaProvider SchemaProvider { get; private set; }

        /// <summary>
        /// Gets the original query.
        /// </summary>
        /// <value>The original query.</value>
        public SparqlQuery OriginalQuery { get; private set; }

        /// <summary>
        /// Gets the mapping.
        /// </summary>
        /// <value>The mapping.</value>
        public MappingProcessor Mapping { get; private set; }

        /// <summary>
        /// Gets the node factory.
        /// </summary>
        /// <value>The node factory.</value>
        public INodeFactory NodeFactory { get; private set; }

        /// <summary>
        /// Gets the original algebra.
        /// </summary>
        /// <value>The original algebra.</value>
        public ISparqlAlgebra OriginalAlgebra { get; private set; }

        /// <summary>
        /// Gets the database.
        /// </summary>
        /// <value>The database.</value>
        public ISqlDatabase Db { get; private set; }

        /// <summary>
        /// Gets the query naming helpers.
        /// </summary>
        /// <value>The query naming helpers.</value>
        public QueryNamingHelpers QueryNamingHelpers
        {
            get { return _queryNamingHelpers; }
        }

        /// <summary>
        /// The optimizers
        /// </summary>
        public Optimizers Optimizers
        {
            get { return _optimizers; }
        }

        /// <summary>
        /// Gets the blank node subject for value.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="value">The value.</param>
        /// <returns>Node.</returns>
        public INode GetBlankNodeSubjectForValue(INodeFactory factory, object value)
        {
            var sVal = value.ToString();

            if (!_blankNodesSubjects.ContainsKey(sVal))
            {
                _blankNodesSubjects.Add(sVal, factory.CreateBlankNode());
            }

            return _blankNodesSubjects[sVal];
        }

        /// <summary>
        /// Gets the blank node object for value.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="value">The value.</param>
        /// <returns>Node.</returns>
        public INode GetBlankNodeObjectForValue(INodeFactory factory, object value)
        {
            var sVal = value.ToString();

            if (!_blankNodesObjects.ContainsKey(sVal))
            {
                _blankNodesObjects.Add(sVal, factory.CreateBlankNode());
            }

            return _blankNodesObjects[sVal];
        }

        /// <summary>
        /// Creates the sparql variable.
        /// </summary>
        /// <returns>The variable name.</returns>
        public string CreateSparqlVariable()
        {
            int counter = 1;
            string varName;

            do
            {
                varName = string.Format("_:context-autos{0}", counter++);
            } while (_usedVariables.Contains(varName));

            _usedVariables.Add(varName);
            return varName;
        }
    }
}
