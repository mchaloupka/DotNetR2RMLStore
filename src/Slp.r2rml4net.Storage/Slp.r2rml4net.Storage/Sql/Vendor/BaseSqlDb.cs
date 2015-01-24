﻿using System.Linq;
using Slp.r2rml4net.Storage.Bootstrap;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Operator;
using Slp.r2rml4net.Storage.Sql.Algebra.Source;

namespace Slp.r2rml4net.Storage.Sql.Vendor
{
    /// <summary>
    /// Base database representation
    /// </summary>
    public abstract class BaseSqlDb : ISqlDb
    {
        /// <summary>
        /// The query builder
        /// </summary>
        private readonly BaseSqlQueryBuilder _queryBuilder;

        /// <summary>
        /// The name generator
        /// </summary>
        private readonly BaseSqlNameGenerator _nameGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseSqlDb"/> class.
        /// </summary>
        protected BaseSqlDb(ISqlDbFactory factory)
        {
            _queryBuilder = factory.CreateSqlQueryBuilder();
            _nameGenerator = factory.CreateNameGenerator();
        }

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The query result reader.</returns>
        public abstract IQueryResultReader ExecuteQuery(string query, QueryContext context);

        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="sqlAlgebra">The SQL algebra.</param>
        /// <param name="context">The query context.</param>
        /// <returns>The SQL query</returns>
        public string GenerateQuery(INotSqlOriginalDbSource sqlAlgebra, QueryContext context)
        {
            _nameGenerator.GenerateNames(sqlAlgebra, context);

            return _queryBuilder.GenerateQuery(sqlAlgebra, context);
        }

        /// <summary>
        /// Determines whether the specified columns can be unioned.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="other">The other column.</param>
        /// <returns><c>true</c> if the specified columns can be unioned; otherwise, <c>false</c>.</returns>
        public bool CanBeUnioned(ISqlColumn column, ISqlColumn other)
        {
            if(column is SqlSelectColumn)
            {
                return CanBeUnioned(((SqlSelectColumn)column).OriginalColumn, other);
            }
            else if(column is SqlUnionColumn)
            {
                var unColumn = (SqlUnionColumn)column;
                var firstCol = unColumn.OriginalColumns.FirstOrDefault();

                if (firstCol != null)
                    return CanBeUnioned(firstCol, other);
                else
                    return false;
            }
            else if(column is SqlExpressionColumn)
            {
                // TODO: Determine sql type
                return false;
            }
            else if(column is SqlTableColumn && other is SqlTableColumn)
            {
                // TODO: Make it accordingly to type
                if(column.Source is SqlTable && other.Source is SqlTable)
                {
                    var columnTableName = ((SqlTable)column.Source).TableName;
                    var otherTableName = ((SqlTable)other.Source).TableName;

                    return DataReaderWrapper.GetTableNameUnquoted(columnTableName) == DataReaderWrapper.GetTableNameUnquoted(otherTableName) && (DataReaderWrapper.GetColumnNameUnquoted(((SqlTableColumn)column).OriginalName) == DataReaderWrapper.GetColumnNameUnquoted(((SqlTableColumn)other).OriginalName));
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return CanBeUnioned(other, column);
            }
        }
    }
}
