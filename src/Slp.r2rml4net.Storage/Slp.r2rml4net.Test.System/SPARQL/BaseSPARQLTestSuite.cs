﻿using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Slp.r2rml4net.Storage;
using Slp.r2rml4net.Storage.Bootstrap;
using Slp.r2rml4net.Storage.Database;
using TCode.r2rml4net.Mapping.Fluent;
using VDS.RDF;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;

namespace Slp.r2rml4net.Test.System.SPARQL
{
    public abstract class BaseSPARQLTestSuite
    {
        protected static string GetPath(string dataFile)
        {
            var path = string.Format(".\\SPARQL\\SPARQL_TestSuite\\{0}", dataFile);
            return path;
        }

        protected static R2RMLStorage InitializeDataset(string dataset, ISqlDatabase sqlDb, IR2RMLStorageFactory storageFactory)
        {
            var datasetFile = GetPath(@"Data\Datasets\" + dataset);

            var doc = XDocument.Load(datasetFile);
            var sqlCommands = doc.Root
                .Elements()
                .Where(x => x.Name == "sql")
                .Single()
                .Elements();

            foreach (var command in sqlCommands)
            {
                if (command.Name == "table")
                    CreateTable(sqlDb, command);
                else if (command.Name == "query")
                    ExecuteQuery(sqlDb, command);
                else
                    throw new Exception(String.Format("Unknown sql command {1} when creating dataset {0}", dataset, command.Name));
            }

            var mappingString = doc.Root.Elements().Where(x => x.Name == "mapping").Single().Value;
            var mapping = R2RMLLoader.Load(mappingString);
            return new R2RMLStorage(sqlDb, mapping, storageFactory);
        }

        private static void ExecuteQuery(ISqlDatabase sqlDb, XElement query)
        {
            sqlDb.ExecuteQuery(query.Value);
        }

        private static void CreateTable(ISqlDatabase sqlDb, XElement table)
        {
            var tableName = table.Attribute("name").Value;
            sqlDb.ExecuteQuery(String.Format("IF OBJECT_ID(\'{0}\', 'U') IS NOT NULL DROP TABLE {0}", tableName));

            StringBuilder sb = new StringBuilder();
            sb.Append("CREATE TABLE ");
            sb.Append(tableName);
            sb.Append(" (");

            bool first = true;

            foreach (var tablePart in table.Elements())
            {
                if (!first)
                {
                    sb.Append(", ");
                }
                else
                {
                    first = false;
                }

                if (tablePart.Name == "column")
                {
                    sb.Append(tablePart.Attribute("name").Value);
                    sb.Append(' ');
                    sb.Append(tablePart.Attribute("type").Value);
                    sb.Append(' ');

                    if (tablePart.Attribute("nullable").Value == "true")
                    {
                        sb.Append("NULL");
                    }
                    else
                    {
                        sb.Append("NOT NULL");
                    }
                }
                else
                    throw new Exception(String.Format("Unknown table part {1} when creating table {0}", tableName, tablePart.Name));
            }

            sb.Append(")");

            sqlDb.ExecuteQuery(sb.ToString());
        }

        protected void AssertBagEqual(XDocument expected, object result)
        {
            var expectedSet = ParseSparqlResultSetXmlResultFile(expected);

            Assert.IsTrue(result is SparqlResultSet);

            var resultSet = (SparqlResultSet)result;

            CollectionAssert.AreEqual(expectedSet.Variables.ToArray(), resultSet.Variables.ToArray());

            CollectionAssert.AreEquivalent(expectedSet.Results, resultSet.Results);
        }

        private SparqlResultSet ParseSparqlResultSetXmlResultFile(XDocument expected)
        {
            XNamespace ns = "http://www.w3.org/2005/sparql-results#";

            var variableNodes = expected
                .Descendants()
                .Where(x => x.Name.Namespace == ns && x.Name.LocalName == "head")
                .Descendants()
                .Where(x => x.Name.Namespace == ns && x.Name.LocalName == "variable")
                .Select(x => x.Attribute("name"))
                .Where(x => x != null)
                .Select(x => x.Value);

            var resultSet = new SparqlResultSet();
            var handler = new ResultSetHandler(resultSet);

            handler.StartResults();

            foreach (var variable in variableNodes)
            {
                handler.HandleVariable(variable);
            }

            var resultNodes = expected
                .Descendants()
                .Where(x => x.Name.Namespace == ns && x.Name.LocalName == "results")
                .Descendants()
                .Where(x => x.Name.Namespace == ns && x.Name.LocalName == "result");

            foreach (var resultNode in resultNodes)
            {
                var bindings = resultNode
                    .Descendants()
                    .Where(x => x.Name.Namespace == ns && x.Name.LocalName == "binding")
                    .Where(x => x.Attribute("name") != null);


                var set = new Set();

                foreach (var binding in bindings)
                {
                    var varNane = binding.Attribute("name").Value;
                    var value = GetSparqlValue(binding, handler);

                    set.Add(varNane, value);
                }

                handler.HandleResult(new SparqlResult(set));
            }

            handler.EndResults(true);

            return resultSet;
        }

        private INode GetSparqlValue(XElement binding, ISparqlResultsHandler resultSetHandler)
        {
            if (binding.Nodes().OfType<XElement>().Count() == 1)
            {
                var element = binding.Nodes().OfType<XElement>().First();

                if (element.Name.LocalName == "uri")
                {
                    string uri = element.Value;
                    return resultSetHandler.CreateUriNode(new Uri(uri));
                }
            }

            throw new NotImplementedException();
        }
    }
}
