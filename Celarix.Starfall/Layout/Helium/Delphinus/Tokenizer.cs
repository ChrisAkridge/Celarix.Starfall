using AngleSharp.Dom;
using Celarix.Starfall.Layout.Helium.Delphinus.Tokens;
using OpenTK.Windowing.Common.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Delphinus
{
    internal static class Tokenizer
    {
        public static List<Token> Tokenize(string xml)
        {
            var parser = new AngleSharp.Xml.Parser.XmlParser();
            var parsedXml = parser.ParseDocument(xml);
            var tokens = new List<Token>();
            var context = StyleContext.StartingContext;

            TokenizeNode(parsedXml, context, tokens);
            return tokens;
        }

        private static void TokenizeNode(INode node, StyleContext context, IList<Token> output)
        {
            if (node is IText textNode)
            {
                var text = textNode.Text;
                
                // Replace all whitespace characters with U+0020 SPACE, including newlines
                text = string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

                // Separate into words and spaces
                var wordsAndSpaces = text.Split((char[]?)null, StringSplitOptions.None);
                foreach (var wordOrSpace in wordsAndSpaces)
                {
                    if (string.IsNullOrEmpty(wordOrSpace))
                    {
                        output.Add(new Space(context));
                    }
                    else
                    {
                        output.Add(new GlyphRun(wordOrSpace, context));
                    }
                }
            }
            else if (node is IElement element)
            {
                var tagName = element.TagName.ToLower();
                StyleContext newContext = ApplyAttributes(context, element);
                switch (tagName)
                {
                    case "p":
                        // p is the only block-level element (mathblock is just a special case of p),
                        // so we always close the previous paragraph even if there isn't one. later
                        // stages will ignore a mismatched closing paragraph token, so this is fine.
                        output.Add(new CloseParagraph());
                        output.Add(new OpenParagraph(newContext));
                        foreach (var child in element.ChildNodes) { TokenizeNode(child, newContext, output); }
                        output.Add(new CloseParagraph());
                        break;
                    case "mathblock":
                        // Much the same as p, but override halign and valign regardless of what's
                        // in the attributes.
                        output.Add(new CloseParagraph());
                        var mathContext = newContext with
                        {
                            HAlign = "center",
                            VAlign = "middle"
                        };
                        output.Add(new OpenParagraph(mathContext));
                        foreach (var child in element.ChildNodes) { TokenizeNode(child, newContext, output); }
                        output.Add(new CloseParagraph());
                        break;
                    case "span":
                        foreach (var child in element.ChildNodes)
                        {
                            TokenizeNode(child, newContext, output);
                        }
                        break;
                    case "br":
                        output.Add(new ForcedBreak());
                        break;
                    case "hr":
                        output.Add(new HorizontalRule(newContext));
                        break;
                    case "img":
                        output.Add(new ImageToken(newContext, element.Attributes.GetNamedItem("src")?.Value ?? ""));
                        break;
                    case "sup":
                        var sup = new AtomicBox(newContext, scaleFactor: 0.8, verticalOffset: -0.5, horizontalOffset: 0);
                        foreach (var child in element.ChildNodes)
                        {
                            TokenizeNode(child, newContext, sup.Children);
                        }
                        output.Add(sup);
                        break;
                    case "sub":
                        var sub = new AtomicBox(newContext, scaleFactor: 0.8, verticalOffset: 0.5, horizontalOffset: 0);
                        foreach (var child in element.ChildNodes)
                        {
                            TokenizeNode(child, newContext, sub.Children);
                        }
                        output.Add(sub);
                        break;
                    case "root":
                        TokenizeRootElement(element, newContext, output);
                        break;
                    case "frac":
                    case "fraction":
                        TokenizeFractionElement(element, newContext, output);
                        break;
                    case "sum":
                    case "prod":
                    case "int":
                    case "limit":
                        TokenizeLargeOperatorElement(element, newContext, output);
                        break;
                    default:
                        // Treat everything else like span.
                        foreach (var child in element.ChildNodes)
                        {
                            TokenizeNode(child, newContext, output);
                        }
                        break;
                }
            }
        }

        private static void TokenizeRootElement(IElement root, StyleContext context, IList<Token> output)
        {
            // No, not the root element, a <root> element
            // You know, like square roots
            // They need an <index> and <radicand> child element. If not provided, nothing appears.
            // If provided multiple times, only the first of each is used. Other children are ignored.
            var children = root.Children;
            var indexElement = children.FirstOrDefault(c => c.TagName.Equals("index", StringComparison.OrdinalIgnoreCase));
            var radicandElement = children.FirstOrDefault(c => c.TagName.Equals("radicand", StringComparison.OrdinalIgnoreCase));
            AtomicBox? indexBox = null;
            AtomicBox? radicandBox = null;

            if (indexElement != null)
            {
                indexBox = new AtomicBox(context, scaleFactor: 0.5, verticalOffset: -0.5, horizontalOffset: 0.2);
                TokenizeNode(indexElement, context, indexBox.Children);
            }

            if (radicandElement != null)
            {
                radicandBox = new AtomicBox(context, scaleFactor: 0.9, verticalOffset: 0.1, horizontalOffset: 0.8);
                TokenizeNode(radicandElement, context, radicandBox.Children);
            }

            output.Add(new RootElement(context, indexBox, radicandBox));
        }

        private static void TokenizeFractionElement(IElement fraction, StyleContext context, IList<Token> output)
        {
            // A <fraction> element needs a <numerator> and <denominator> child element. If not provided, nothing appears.
            // If provided multiple times, only the first of each is used. Other children are ignored.
            var children = fraction.Children;
            var numeratorElement = children.FirstOrDefault(c => c.TagName.Equals("numerator", StringComparison.OrdinalIgnoreCase));
            var denominatorElement = children.FirstOrDefault(c => c.TagName.Equals("denominator", StringComparison.OrdinalIgnoreCase));
            AtomicBox? numeratorBox = null;
            AtomicBox? denominatorBox = null;
            if (numeratorElement != null)
            {
                numeratorBox = new AtomicBox(context, scaleFactor: 0.8, verticalOffset: -0.5, horizontalOffset: 0);
                TokenizeNode(numeratorElement, context, numeratorBox.Children);
            }
            if (denominatorElement != null)
            {
                denominatorBox = new AtomicBox(context, scaleFactor: 0.8, verticalOffset: 0.5, horizontalOffset: 0);
                TokenizeNode(denominatorElement, context, denominatorBox.Children);
            }
            output.Add(new FractionElement(context, numeratorBox, denominatorBox));
        }

        private static void TokenizeLargeOperatorElement(IElement largeOperator, StyleContext context, IList<Token> output)
        {
            // This element needs an <upper>, <lower>, and <expr> child element. If not provided,
            // that part of the operator is left blank. If provided multiple times, only the first of
            // each is used. Other children are ignored.
            var kind = largeOperator.TagName.ToLowerInvariant() switch
            {
                "sum" => LargeOperatorKind.Sum,
                "prod" => LargeOperatorKind.Product,
                "int" => LargeOperatorKind.Integral,
                "limit" => LargeOperatorKind.Limit,
                _ => LargeOperatorKind.Sum
            };
            var children = largeOperator.Children;
            var upperElement = children.FirstOrDefault(c => c.TagName.Equals("upper", StringComparison.OrdinalIgnoreCase));
            var lowerElement = children.FirstOrDefault(c => c.TagName.Equals("lower", StringComparison.OrdinalIgnoreCase));
            var exprElement = children.FirstOrDefault(c => c.TagName.Equals("expr", StringComparison.OrdinalIgnoreCase));
            AtomicBox? upperBox = null;
            AtomicBox? lowerBox = null;
            AtomicBox? exprBox = null;
            if (upperElement != null)
            {
                upperBox = new AtomicBox(context, scaleFactor: 0.5, verticalOffset: -0.8, horizontalOffset: kind == LargeOperatorKind.Integral ? 0.5 : 0);
                TokenizeNode(upperElement, context, upperBox.Children);
            }
            if (lowerElement != null)
            {
                lowerBox = new AtomicBox(context, scaleFactor: 0.5, verticalOffset: 0.8, horizontalOffset: kind == LargeOperatorKind.Integral ? 0.3 : 0);
                TokenizeNode(lowerElement, context, lowerBox.Children);
            }
            if (exprElement != null)
            {
                exprBox = new AtomicBox(context, scaleFactor: 1, verticalOffset: 0, horizontalOffset: 1.5);
                TokenizeNode(exprElement, context, exprBox.Children);
            }
            output.Add(new LargeOperatorElement(context, kind, upperBox, lowerBox, exprBox));
        }

        

        private static StyleContext ApplyAttributes(StyleContext context, IElement element)
        {
            var attributes = element.Attributes;
            var fontFamily = attributes.GetNamedItem("fontfamily")?.Value ?? context.FontFamily;
            var fontSize = attributes.GetNamedItem("fontsize")?.Value ?? context.FontSize;
            var bgColor = attributes.GetNamedItem("bgcolor")?.Value ?? context.BgColor;
            var color = attributes.GetNamedItem("color")?.Value ?? context.Color;
            var id = attributes.GetNamedItem("id")?.Value ?? context.Id;
            var classes = attributes.GetNamedItem("class")?.Value ?? context.Classes;
            var hAlign = attributes.GetNamedItem("halign")?.Value ?? context.HAlign;
            var vAlign = attributes.GetNamedItem("valign")?.Value ?? context.VAlign;
            var lineMargin = attributes.GetNamedItem("linemargin")?.Value ?? context.LineMargin;

            return new StyleContext(
                FontFamily: fontFamily,
                FontSize: fontSize,
                BgColor: bgColor,
                Color: color,
                Id: id,
                Classes: classes,
                HAlign: hAlign,
                VAlign: vAlign,
                LineMargin: lineMargin
            );
        }
    }
}
