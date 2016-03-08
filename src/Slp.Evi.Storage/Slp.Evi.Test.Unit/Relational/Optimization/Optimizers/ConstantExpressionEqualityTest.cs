﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Slp.r2rml4net.Storage.Relational.Optimization.Optimizers;
using Slp.r2rml4net.Storage.Relational.Query.Conditions.Filter;
using Slp.r2rml4net.Storage.Relational.Query.Expressions;

namespace Slp.r2rml4net.Test.Unit.Relational.Optimization.Optimizers
{
    [TestClass]
    public class ConstantExpressionEqualityTest
        : BaseOptimizerTest<object>
    {
        private ConstantExpressionEqualityOptimizer _optimizer;

        [TestInitialize]
        public void TestInitialization()
        {
            _optimizer = new ConstantExpressionEqualityOptimizer();
        }

        [TestMethod]
        public void ConstantEquality_SameStrings_Prefix()
        {
            var queryContext = GenerateQueryContext();

            var left = new ConstantExpression("http://s.com/", queryContext);
            var right = new ConstantExpression("http://s.com/", queryContext);

            var condition = new EqualExpressionCondition(left, right);

            var result = _optimizer.TransformFilterCondition(condition, GetContext(queryContext));

            var expected = new AlwaysTrueCondition();

            AssertFilterConditionsEqual(expected, result);
        }

        [TestMethod]
        public void ConstantEquality_StringDifferents_Prefix()
        {
            var queryContext = GenerateQueryContext();

            var left = new ConstantExpression("http://s.com/", queryContext);
            var right = new ConstantExpression("http://s.com/2", queryContext);

            var condition = new EqualExpressionCondition(left, right);

            var result = _optimizer.TransformFilterCondition(condition, GetContext(queryContext));

            var expected = new AlwaysFalseCondition();

            AssertFilterConditionsEqual(expected, result);
        }

        [TestMethod]
        public void ConstantEquality_DifferentType_Prefix()
        {
            var queryContext = GenerateQueryContext();

            var left = new ConstantExpression("2", queryContext);
            var right = new ConstantExpression(2, queryContext);

            var condition = new EqualExpressionCondition(left, right);

            var result = _optimizer.TransformFilterCondition(condition, GetContext(queryContext));

            var expected = new AlwaysFalseCondition();

            AssertFilterConditionsEqual(expected, result);
        }
    }
}
