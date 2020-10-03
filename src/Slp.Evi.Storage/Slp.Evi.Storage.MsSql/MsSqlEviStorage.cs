﻿using Slp.Evi.Storage.Common;
using Slp.Evi.Storage.MsSql.Database;
using TCode.r2rml4net;

namespace Slp.Evi.Storage.MsSql
{
    public sealed class MsSqlEviStorage
        : EviStorage<MsSqlQuery>
    {
        public MsSqlEviStorage(IR2RML mapping, string connectionString, int queryTimeout) 
            : base(mapping, new MsSqlDatabase(connectionString, queryTimeout))
        { }

        public MsSqlEviStorage(IR2RML mapping, MsSqlDatabase database)
            : base(mapping, database)
        { }
    }
}
