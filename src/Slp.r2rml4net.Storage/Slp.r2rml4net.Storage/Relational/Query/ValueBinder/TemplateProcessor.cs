﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Slp.r2rml4net.Storage.Relational.Query.ValueBinder
{
    /// <summary>
    /// Template processor
    /// </summary>
    public class TemplateProcessor
    {
        /// <summary>
        /// The template replace regex
        /// </summary>
        private static readonly Regex TemplateReplaceRegex = new Regex(@"(?<N>\{)([^\{\}.]+)(?<-N>\})(?(N)(?!))");

        /// <summary>
        /// Gets the columns from template.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <returns>Columns used in template.</returns>
        public IEnumerable<string> GetColumnsFromTemplate(string template)
        {
            return ParseTemplate(template).OfType<ColumnTemplatePart>().Select(x => x.Column).Distinct();
        }

        /// <summary>
        /// Parses the template.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <returns>The template parts.</returns>
        public IEnumerable<ITemplatePart> ParseTemplate(string template)
        {
            var matches = TemplateReplaceRegex.Matches(template).OfType<Match>().OrderBy(x => x.Index);

            int curIndex = 0;

            foreach (var match in matches)
            {
                if (match.Index > curIndex)
                {
                    yield return new TextTemplatePart(template.Substring(curIndex, match.Index - curIndex));
                }

                yield return new ColumnTemplatePart(template.Substring(match.Index + 1, match.Length - 2));

                curIndex = match.Index + match.Length;
            }

            if (curIndex != template.Length)
            {
                yield return new TextTemplatePart(template.Substring(curIndex));
            }
        }

        /// <summary>
        /// Template column part
        /// </summary>
        public class ColumnTemplatePart : ITemplatePart
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ColumnTemplatePart"/> class.
            /// </summary>
            /// <param name="column">The column.</param>
            public ColumnTemplatePart(string column)
            {
                Column = column;
            }

            /// <summary>
            /// Gets the column.
            /// </summary>
            /// <value>The column.</value>
            public string Column { get; private set; }

            /// <summary>
            /// Gets a value indicating whether this instance is column.
            /// </summary>
            /// <value><c>true</c> if this instance is column; otherwise, <c>false</c>.</value>
            public bool IsColumn
            {
                get { return true; }
            }

            /// <summary>
            /// Gets a value indicating whether this instance is text.
            /// </summary>
            /// <value><c>true</c> if this instance is text; otherwise, <c>false</c>.</value>
            public bool IsText
            {
                get { return false; }
            }

            /// <summary>
            /// Gets the text.
            /// </summary>
            /// <value>The text.</value>
            /// <exception cref="System.Exception">Asked for text on ColumnTemplatePart</exception>
            public string Text
            {
                get { throw new Exception("Asked for text on ColumnTemplatePart"); }
            }
        }

        /// <summary>
        /// Template text part
        /// </summary>
        public class TextTemplatePart : ITemplatePart
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TextTemplatePart"/> class.
            /// </summary>
            /// <param name="text">The text.</param>
            public TextTemplatePart(string text)
            {
                Text = text;
            }

            /// <summary>
            /// Gets the text.
            /// </summary>
            /// <value>The text.</value>
            public string Text { get; private set; }

            /// <summary>
            /// Gets a value indicating whether this instance is column.
            /// </summary>
            /// <value><c>true</c> if this instance is column; otherwise, <c>false</c>.</value>
            public bool IsColumn
            {
                get { return false; }
            }

            /// <summary>
            /// Gets a value indicating whether this instance is text.
            /// </summary>
            /// <value><c>true</c> if this instance is text; otherwise, <c>false</c>.</value>
            public bool IsText
            {
                get { return true; }
            }

            /// <summary>
            /// Gets the column.
            /// </summary>
            /// <value>The column.</value>
            /// <exception cref="System.Exception">Asked for column on TextTemplatePart</exception>
            public string Column
            {
                get { throw new Exception("Asked for column on TextTemplatePart"); }
            }
        }
    }

    /// <summary>
    /// Template part
    /// </summary>
    public interface ITemplatePart
    {
        /// <summary>
        /// Gets a value indicating whether this instance is column.
        /// </summary>
        /// <value><c>true</c> if this instance is column; otherwise, <c>false</c>.</value>
        bool IsColumn { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is text.
        /// </summary>
        /// <value><c>true</c> if this instance is text; otherwise, <c>false</c>.</value>
        bool IsText { get; }

        /// <summary>
        /// Gets the column.
        /// </summary>
        /// <value>The column.</value>
        string Column { get; }

        /// <summary>
        /// Gets the text.
        /// </summary>
        /// <value>The text.</value>
        string Text { get; }
    }
}
