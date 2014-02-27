﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Sql.Algebra.Source
{
    public class SingleEmptyRowSource : ISqlSource
    {
        public string Name { get; set; }


        public IEnumerable<ISqlColumn> Columns
        {
            get { yield break; }
        }
    }
}
