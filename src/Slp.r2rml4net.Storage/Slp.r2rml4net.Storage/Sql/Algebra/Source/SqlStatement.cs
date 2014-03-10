﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Source
{
    public class SqlStatement : ISqlOriginalDbSource
    {
        public string SqlQuery { get; private set; }

        public SqlStatement(string query)
        {
            this.SqlQuery = query;
        }

        private List<SqlTableColumn> columns;

        public ISqlColumn GetColumn(string columnName)
        {
            var col = columns.Where(x => x.Name == columnName).FirstOrDefault();

            if (col == null)
            {
                col = new SqlTableColumn(columnName, this);
                columns.Add(col);
            }

            return col;
        }

        public string Name { get; set; }


        public IEnumerable<ISqlColumn> Columns
        {
            get { return columns.AsEnumerable(); }
        }

        public object Accept(Operator.ISqlSourceVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}
