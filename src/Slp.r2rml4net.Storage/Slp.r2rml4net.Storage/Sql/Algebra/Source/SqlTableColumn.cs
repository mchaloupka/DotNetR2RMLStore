﻿using DatabaseSchemaReader.DataSchema;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Source
{
    /// <summary>
    /// SQL table column.
    /// </summary>
    public class SqlTableColumn : IOriginalSqlColumn
    {
        /// <summary>
        /// The SQL column type
        /// </summary>
        public DataType SqlColumnType { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlTableColumn"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="source">The source.</param>
        /// <param name="sqlColumnType">SQL column type</param>
        public SqlTableColumn(string name, ISqlSource source, DataType sqlColumnType)
        {
            SqlColumnType = sqlColumnType;
            OriginalName = name;
            Source = source;
        }

        /// <summary>
        /// Gets the name of the original.
        /// </summary>
        /// <value>The name of the original.</value>
        public string OriginalName { get; private set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>The source.</value>
        public ISqlSource Source { get; private set; }
    }
}
