﻿using System;
using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;
using Slp.Evi.Storage.DBSchema;

namespace Slp.Evi.Test.Unit.Mocks
{
    /// <summary>
    /// Mock of <see cref="IDbSchemaProvider" />
    /// </summary>
    public class DbSchemaProviderMock : IDbSchemaProvider
    {
        /// <summary>
        /// The table info cache
        /// </summary>
        private readonly Dictionary<string, DatabaseTable> _tableCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbSchemaProviderMock"/> class.
        /// </summary>
        public DbSchemaProviderMock()
        {
            _tableCache = new Dictionary<string, DatabaseTable>();
        }

        /// <summary>
        /// Gets the table information.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>DatabaseTable.</returns>
        /// <exception cref="System.Exception">Table not found in database schema</exception>
        public DatabaseTable GetTableInfo(string tableName)
        {

            if (_tableCache.ContainsKey(tableName))
                return _tableCache[tableName];

            throw new Exception("Table not found in database schema");
        }

        /// <summary>
        /// Adds the database table information.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="tableInfo">The table information.</param>
        public void AddDatabaseTableInfo(string tableName, DatabaseTable tableInfo)
        {
            _tableCache.Add(tableName, tableInfo);
        }
    }
}
