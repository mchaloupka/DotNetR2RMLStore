﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Slp.r2rml4net.Storage.Sql.Algebra;
using Slp.r2rml4net.Test.Unit.Optimization.SqlAlgebra.Utils;

namespace Slp.r2rml4net.Test.Unit.Optimization.SqlAlgebra
{
    public class BaseConditionTest
    {
        public void AssertConditionsEqual(ICondition expected, ICondition actual)
        {
            var equalityAssert = new SqlAlgebraEqualityChecker();
            var checkResult = expected.Accept(equalityAssert, actual);
            Assert.IsTrue((bool)checkResult, "The conditions are not equal");
        }
    }
}
