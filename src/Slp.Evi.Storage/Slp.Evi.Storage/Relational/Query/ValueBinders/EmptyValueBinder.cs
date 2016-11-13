using System.Collections.Generic;
using System.Diagnostics;
using Slp.Evi.Storage.Database;
using Slp.Evi.Storage.Query;
using VDS.RDF;

namespace Slp.Evi.Storage.Relational.Query.ValueBinders
{
    /// <summary>
    /// The empty value binder
    /// </summary>
    public class EmptyValueBinder 
        : IValueBinder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyValueBinder"/> class.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        public EmptyValueBinder(string variableName)
        {
            VariableName = variableName;
        }

        /// <summary>
        /// Loads the node.
        /// </summary>
        /// <param name="nodeFactory">The node factory.</param>
        /// <param name="rowData">The row data.</param>
        /// <param name="context">The context.</param>
        /// <returns>INode.</returns>
        public INode LoadNode(INodeFactory nodeFactory, IQueryResultRow rowData, IQueryContext context)
        {
            return null;
        }

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        /// <value>The name of the variable.</value>
        public string VariableName { get; }

        /// <summary>
        /// Gets the needed calculus variables to calculate the value.
        /// </summary>
        /// <value>The needed calculus variables.</value>
        public IEnumerable<ICalculusVariable> NeededCalculusVariables => new ICalculusVariable[] {};

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        /// <param name="data">The data.</param>
        /// <returns>The returned value from visitor.</returns>
        [DebuggerStepThrough]
        public object Accept(IValueBinderVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }
    }
}