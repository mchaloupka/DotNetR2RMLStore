﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Operator;
using Slp.r2rml4net.Storage.Sql.Binders;
using Slp.r2rml4net.Storage.Sql.Algebra.Utils;
using Slp.r2rml4net.Storage.Sql.Algebra.Source;

namespace Slp.r2rml4net.Storage.Sql.Binders.Utils
{
    public static class ValueBinderExtensions
    {
        public static IBaseValueBinder GetSelectValueBinder(this IBaseValueBinder binder, SqlSelectOp select, QueryContext context)
        {
            var newBinder = (IBaseValueBinder)binder.Clone();

            foreach (var col in newBinder.AssignedColumns.ToArray())
            {
                newBinder.ReplaceAssignedColumn(col, GetSelectColumn(select, col, context));
            }

            return newBinder;
        }

        private static ISqlColumn GetSelectColumn(SqlSelectOp select, ISqlColumn column, QueryContext context)
        {
            if(column is SqlSelectColumn)
            {
                return select.GetSelectColumn(column);
            }
            else if(column is SqlUnionColumn)
            {
                if(column.Source == null) 
                {
                    throw new Exception("Can't get select column from created column");
                }

                return select.GetSelectColumn(column);
            }
            else if(column is SqlExpressionColumn)
            {
                if(column.Source == null)
                {
                    var expression = (IExpression)((SqlExpressionColumn)column).Expression.Clone();

                    foreach (var colRef in expression.GetAllReferencedColumns().ToArray())
                    {
                        expression.ReplaceColumnReference(colRef, GetOriginalColumn(colRef, context));
                    }

                    return select.GetExpressionColumn(expression);
                }
                else
                {
                    return select.GetSelectColumn(column);
                }
            }
            else if (column is SqlTableColumn)
            {
                return select.GetSelectColumn(column);
            }
            else
            {
                throw new Exception("Can't get select column");
            }
        }

        public static IBaseValueBinder GetOriginalValueBinder(this IBaseValueBinder binder, QueryContext context)
        {
            var newBinder = (IBaseValueBinder)binder.Clone();

            foreach (var oldColumn in newBinder.AssignedColumns.ToArray())
            {
                var originalColumn = GetOriginalColumn(oldColumn, context);
                newBinder.ReplaceAssignedColumn(oldColumn, originalColumn);
            }

            return newBinder;
        }

        private static ISqlColumn GetOriginalColumn(ISqlColumn oldColumn, QueryContext context)
        {
            if (oldColumn is SqlSelectColumn)
            {
                return ((SqlSelectColumn)oldColumn).OriginalColumn;
            }
            else if (oldColumn is SqlUnionColumn)
            {
                var unColumn = (SqlUnionColumn)oldColumn;

                var newColumn = new SqlUnionColumn(null);

                foreach (var subCol in unColumn.OriginalColumns)
                {
                    newColumn.AddColumn(GetOriginalColumn(subCol, context));
                }

                return newColumn;
            }
            else if (oldColumn is SqlExpressionColumn)
            {
                var exprColumn = (SqlExpressionColumn)oldColumn;

                var expression = (IExpression)exprColumn.Expression.Clone();
                var columnReferences = expression.GetAllReferencedColumns().ToArray();

                foreach (var colRef in columnReferences)
                {
                    expression.ReplaceColumnReference(colRef, GetOriginalColumn(colRef, context));
                }

                return new SqlExpressionColumn(expression, null);
            }
            else
            {
                throw new Exception("Can't get original column");
            }
        }
    }
}