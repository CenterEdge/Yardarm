using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Helpers
{
    public static class DocumentationSyntaxHelpers
    {
        private static readonly char[] _newLineChars = {'\r', '\n'};

        public static SyntaxTrivia BuildXmlCommentTrivia(params XmlElementSyntax[] elements)
        {
            var nodes = new XmlNodeSyntax[elements.Length * 2 + 1];

            nodes[0] = XmlText(XmlTextLiteral(TriviaList(DocumentationCommentExterior("///")), " ", " ", TriviaList()));

            for (int i = 0; i < elements.Length; i++)
            {
                nodes[i*2 + 1] = elements[i];
                nodes[i*2 + 2] = i < elements.Length - 1
                    ? InteriorNewLine()
                    : XmlText(XmlTextNewLine(TriviaList(), Environment.NewLine, Environment.NewLine, TriviaList()));
            }

            return Trivia(DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia, List(nodes)));
        }

        /// <summary>
        /// Builds a summary element. Assumes a leading /// comment, and does not add a newline at the end
        /// </summary>
        public static XmlElementSyntax BuildSummaryElement(string content) =>
            XmlSummaryElement(
                content.Split(_newLineChars, StringSplitOptions.RemoveEmptyEntries)
                    .SelectMany(ContentLine)
                    .Concat(ContentLine(""))
                    .ToArray());

        /// <summary>
        /// Builds a remarks element. Assumes a leading /// comment, and does not add a newline at the end
        /// </summary>
        public static XmlElementSyntax BuildRemarksElement(string content) =>
            XmlRemarksElement(
                content.Split(_newLineChars, StringSplitOptions.RemoveEmptyEntries)
                    .SelectMany(ContentLine)
                    .Concat(ContentLine(""))
                    .ToArray());

        /// <summary>
        /// Line of content text with a preceding newline
        /// </summary>
        private static XmlNodeSyntax[] ContentLine(string content) => new[]
        {
            InteriorNewLine(), XmlText(XmlTextLiteral(content))
        };

        /// <summary>
        /// Builds a new line with leading comment characters for the next line
        /// </summary>
        /// <returns></returns>
        private static XmlNodeSyntax InteriorNewLine() => XmlText(XmlTextNewLine(Environment.NewLine, true));
    }
}
