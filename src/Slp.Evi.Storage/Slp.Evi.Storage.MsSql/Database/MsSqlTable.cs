﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DatabaseSchemaReader.DataSchema;
using Slp.Evi.Common.Database;
using Slp.Evi.Common.Types;

namespace Slp.Evi.Storage.MsSql.Database
{
    public class MsSqlTable
        : ISqlTableSchema
    {
        private readonly Dictionary<string, MsSqlColumn> _columns;

        public MsSqlTable(string tableName, IEnumerable<MsSqlColumn> columns, DatabaseConstraint tableSchemaPrimaryKey, List<DatabaseConstraint> tableSchemaUniqueKeys)
        {
            Name = tableName;
            _columns = columns.ToDictionary(column => column.Name);
            
            var primaryKey = tableSchemaPrimaryKey?.Columns;
            var uniqueKeys = tableSchemaUniqueKeys.Select(x => x.Columns).ToList();
            Keys = new[] {primaryKey}.Union(uniqueKeys).Where(x => x != null).ToArray();
        }

        /// <inheritdoc />
        public ISqlColumnSchema GetColumn(string columnName)
        {
            if (_columns.TryGetValue(columnName, out var column))
            {
                return column;
            }
            else
            {
                throw new ArgumentException($"Column ${columnName} was not found in the table ${Name}");
            }
        }

        public string Name { get; }

        /// <inheritdoc />
        public IEnumerable<string> Columns => _columns.Select(x => x.Key);

        /// <inheritdoc />
        public IEnumerable<IEnumerable<string>> Keys { get; }

        public static MsSqlTable CreateFromDatabase(DatabaseTable tableSchema)
        {
            var columns = tableSchema.Columns.Select(MsSqlColumn.CreateFromDatabase);
            return new MsSqlTable(tableSchema.Name, columns, tableSchema.PrimaryKey, tableSchema.UniqueKeys);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Table({Name})";
        }
    }
}
