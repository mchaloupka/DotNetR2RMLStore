﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slp.r2rml4net.Storage.Query;
using Slp.r2rml4net.Storage.Sparql;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Storage.Sql.Algebra.Condition;
using Slp.r2rml4net.Storage.Sql.Binders;
using VDS.RDF;

namespace Slp.r2rml4net.Storage.Sql
{
    public class ConditionBuilder
    {
        private ExpressionBuilder expressionBuilder;

        public ConditionBuilder()
        {
            this.expressionBuilder = new ExpressionBuilder();
        }

        public ICondition CreateEqualsCondition(QueryContext context, INode node, IBaseValueBinder valueBinder)
        {
            if (valueBinder is ValueBinder)
            {
                var leftOperand = expressionBuilder.CreateExpression(context, (ValueBinder)valueBinder);
                var rightOperand = expressionBuilder.CreateExpression(context, node);

                return new EqualsCondition(leftOperand, rightOperand);
            }
            else if (valueBinder is CollateValueBinder)
            {
                var orCondition = new OrCondition();
                var binders = ((CollateValueBinder)valueBinder).InnerBinders.ToArray();

                for (int curIndex = 0; curIndex < binders.Length; curIndex++)
                {
                    var andCondition = new AndCondition();

                    for (int prevIndex = 0; prevIndex < curIndex; prevIndex++)
                    {
                        andCondition.AddToCondition(CreateIsNullCondition(context, binders[prevIndex]));
                    }

                    andCondition.AddToCondition(CreateEqualsCondition(context, node, binders[curIndex]));
                    orCondition.AddToCondition(andCondition);
                }

                return orCondition;
            }
            else
                throw new Exception("Value binder can be only standard or collate");
        }

        public ICondition CreateEqualsCondition(QueryContext context, IBaseValueBinder firstValBinder, IBaseValueBinder secondValBinder)
        {
            if (firstValBinder is ValueBinder && secondValBinder is ValueBinder)
            {
                var leftOperand = expressionBuilder.CreateExpression(context, (ValueBinder)firstValBinder);
                var rightOperand = expressionBuilder.CreateExpression(context, (ValueBinder)secondValBinder);
                return new EqualsCondition(leftOperand, rightOperand);
            }
            else if (firstValBinder is CollateValueBinder)
            {
                var orCondition = new OrCondition();
                var binders = ((CollateValueBinder)firstValBinder).InnerBinders.ToArray();

                for (int curIndex = 0; curIndex < binders.Length; curIndex++)
                {
                    var andCondition = new AndCondition();

                    for (int prevIndex = 0; prevIndex < curIndex; prevIndex++)
                    {
                        andCondition.AddToCondition(CreateIsNullCondition(context, binders[prevIndex]));
                    }

                    andCondition.AddToCondition(CreateEqualsCondition(context, binders[curIndex], secondValBinder));
                    orCondition.AddToCondition(andCondition);
                }

                return orCondition;
            }
            else if (secondValBinder is CollateValueBinder)
            {
                return CreateEqualsCondition(context, secondValBinder, firstValBinder);
            }
            else
                throw new Exception("Value binder can be only standard or collate");
        }

        public ICondition CreateAlwaysTrueCondition(QueryContext context)
        {
            return new AlwaysTrueCondition();
        }

        public ICondition CreateAlwaysFalseCondition(QueryContext context)
        {
            return new AlwaysFalseCondition();
        }

        public ICondition CreateAndCondition(QueryContext context, IEnumerable<ICondition> conditions)
        {
            var and = new AndCondition();

            foreach (var cond in conditions)
            {
                and.AddToCondition(cond);
            }

            return and;
        }

        public ICondition CreateJoinEqualsCondition(QueryContext context, IBaseValueBinder firstValBinder, IBaseValueBinder secondValBinder)
        {
            if (firstValBinder.VariableName != secondValBinder.VariableName)
                return new AlwaysTrueCondition();

            var orCondition = new OrCondition();
            orCondition.AddToCondition(CreateIsNullCondition(context, firstValBinder));
            orCondition.AddToCondition(CreateIsNullCondition(context, secondValBinder));
            orCondition.AddToCondition(CreateEqualsCondition(context, firstValBinder, secondValBinder));
            return orCondition;
        }

        public ICondition CreateIsNullCondition(QueryContext context, IBaseValueBinder valueBinder)
        {
            if(valueBinder is ValueBinder)
            {
                var neededColumns = valueBinder.AssignedColumns.ToArray();

                if (neededColumns.Length == 0)
                    return CreateAlwaysFalseCondition(context);
                else if (neededColumns.Length == 1)
                    return CreateIsNullCondition(context, neededColumns[0]);
                else
                {
                    var orCondition = new OrCondition();
                    foreach (var column in neededColumns)
                    {
                        orCondition.AddToCondition(CreateIsNullCondition(context, column));
                    }
                    return orCondition;
                }
            }
            else if (valueBinder is CollateValueBinder)
            {
                var binders = ((CollateValueBinder)valueBinder).InnerBinders.ToArray();
                var andCondition = new AndCondition();

                foreach (var binder in binders)
                {
                    andCondition.AddToCondition(CreateIsNullCondition(context, binder));
                }

                return andCondition;
            }
            else
            {
                throw new Exception("Value binder can be only standard or collate");
            }
        }

        public ICondition CreateIsNullCondition(QueryContext context, ISqlColumn sqlColumn)
        {
            return new IsNullCondition(sqlColumn);
        }

        public ICondition CreateIsNotNullCondition(QueryContext context, ISqlColumn sqlColumn)
        {
            return new NotCondition(CreateIsNullCondition(context, sqlColumn));
        }
    }
}
