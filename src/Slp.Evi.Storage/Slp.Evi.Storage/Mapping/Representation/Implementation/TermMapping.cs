﻿using System;
using TCode.r2rml4net.Mapping;

namespace Slp.Evi.Storage.Mapping.Representation.Implementation
{
    /// <summary>
    /// Represents a base class for implementations of <see cref="ITermMapping"/>.
    /// </summary>
    public abstract class TermMapping
        : ITermMapping
    {
        /// <summary>
        /// Fills an instance of <see cref= "TermMapping" /> from <see cref="ITermMap"/>.
        /// </summary>
        protected static void Fill(TermMapping tm, ITermMap termMap, TriplesMapping parentTriplesMapping, RepresentationCreationContext creationContext)
        {
            tm.TriplesMap = parentTriplesMapping;
            tm.TermType = TermTypeInformation.Create(termMap, creationContext);

            if (termMap.IsConstantValued)
            {
                tm.IsConstantValued = true;

                if (termMap is IUriValuedTermMap uriValued)
                {
                    tm.Iri = uriValued.URI;
                }
                else if (termMap is IObjectMap objectMap)
                {
                    tm.Iri = objectMap.URI;
                }
            }
            else if (termMap.IsColumnValued)
            {
                tm.IsColumnValued = true;
                tm.ColumnName = termMap.ColumnName;
            }
            else if (termMap.IsTemplateValued)
            {
                tm.IsTemplateValued = true;
                tm.Template = termMap.Template;
            }

            tm.BaseIri = termMap.BaseUri;
        }

        /// <inheritdoc />
        public ITriplesMapping TriplesMap { get; private set; }

        /// <inheritdoc />
        public ITermTypeInformation TermType { get; private set; }

        /// <inheritdoc />
        public bool IsConstantValued { get; private set; }

        /// <inheritdoc />
        public bool IsColumnValued { get; private set; }

        /// <inheritdoc />
        public bool IsTemplateValued { get; private set; }

        /// <inheritdoc />
        public string ColumnName { get; private set; }

        /// <inheritdoc />
        public string Template { get; private set; }

        /// <inheritdoc />
        public Uri BaseIri { get; private set; }

        /// <inheritdoc />
        public Uri Iri { get; private set; }
    }
}