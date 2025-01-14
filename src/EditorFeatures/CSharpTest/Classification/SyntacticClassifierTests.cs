﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editor.UnitTests.Workspaces;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.Classification
{
    public partial class SyntacticClassifierTests : AbstractCSharpClassifierTests
    {
        internal override IEnumerable<ClassifiedSpan> GetClassificationSpans(string code, TextSpan textSpan, CSharpParseOptions options)
        {
            using (var workspace = CSharpWorkspaceFactory.CreateWorkspaceFromFile(code, parseOptions: options))
            {
                var snapshot = workspace.Documents.First().TextBuffer.CurrentSnapshot;
                var document = workspace.CurrentSolution.Projects.First().Documents.First();
                var tree = document.GetSyntaxTreeAsync().PumpingWaitResult();

                var service = document.GetLanguageService<IClassificationService>();
                var result = new List<ClassifiedSpan>();
                service.AddSyntacticClassifications(tree, textSpan, result, CancellationToken.None);

                return result;
            }
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void VarAtTypeMemberLevel()
        {
            Test(@"class C { var foo }",
                Keyword("class"),
                Class("C"),
                Punctuation.OpenCurly,
                Identifier("var"),
                Identifier("foo"),
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void VarAsLocalVariableType()
        {
            TestInMethod("var foo = 42",
                Keyword("var"),
                Identifier("foo"),
                Operators.Equals,
                Number("42"));
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void VarOptimisticallyColored()
        {
            TestInMethod("var",
                Keyword("var"));
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void VarNotColoredInClass()
        {
            TestInClass("var",
                Identifier("var"));
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void VarInsideLocalAndExpressions()
        {
            TestInMethod(@"var var = (var)var as var;",
                Keyword("var"),
                Identifier("var"),
                Operators.Equals,
                Punctuation.OpenParen,
                Identifier("var"),
                Punctuation.CloseParen,
                Identifier("var"),
                Keyword("as"),
                Identifier("var"),
                Punctuation.Semicolon);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void VarAsMethodParameter()
        {
            Test(@"class C { void M(var v) { } }",
                Keyword("class"),
                Class("C"),
                Punctuation.OpenCurly,
                Keyword("void"),
                Identifier("M"),
                Punctuation.OpenParen,
                Identifier("var"),
                Identifier("v"),
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void YieldYield()
        {
            Test(@"using System.Collections.Generic;
class yield { 
    IEnumerable<yield> M() { 
        yield yield = new yield(); 
        yield return yield; } }",
                Keyword("using"),
                Identifier("System"),
                Operators.Dot,
                Identifier("Collections"),
                Operators.Dot,
                Identifier("Generic"),
                Punctuation.Semicolon,
                Keyword("class"),
                Class("yield"),
                Punctuation.OpenCurly,
                Identifier("IEnumerable"),
                Punctuation.OpenAngle,
                Identifier("yield"),
                Punctuation.CloseAngle,
                Identifier("M"),
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Identifier("yield"),
                Identifier("yield"),
                Operators.Equals,
                Keyword("new"),
                Identifier("yield"),
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.Semicolon,
                Keyword("yield"),
                Keyword("return"),
                Identifier("yield"),
                Punctuation.Semicolon,
                Punctuation.CloseCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void YieldReturn()
        {
            TestInMethod("yield return 42",
                Keyword("yield"),
                Keyword("return"),
                Number("42"));
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void YieldFixed()
        {
            TestInMethod(@"yield return this.items[0]; yield break; fixed (int* i = 0) { }",
                Keyword("yield"),
                Keyword("return"),
                Keyword("this"),
                Operators.Dot,
                Identifier("items"),
                Punctuation.OpenBracket,
                Number("0"),
                Punctuation.CloseBracket,
                Punctuation.Semicolon,
                Keyword("yield"),
                Keyword("break"),
                Punctuation.Semicolon,
                Keyword("fixed"),
                Punctuation.OpenParen,
                Keyword("int"),
                Operators.Star,
                Identifier("i"),
                Operators.Equals,
                Number("0"),
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void PartialClass()
        {
            Test("public partial class Foo",
                Keyword("public"),
                Keyword("partial"),
                Keyword("class"),
                Class("Foo"));
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void PartialMethod()
        {
            TestInClass("public partial void M() { }",
                Keyword("public"),
                Keyword("partial"),
                Keyword("void"),
                Identifier("M"),
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        /// <summary>
        /// Partial is only valid in a type declaration
        /// </summary>
        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        [WorkItem(536313)]
        public void PartialAsLocalVariableType()
        {
            TestInMethod("partial p1 = 42;",
                Identifier("partial"),
                Identifier("p1"),
                Operators.Equals,
                Number("42"),
                Punctuation.Semicolon);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void PartialClassStructInterface()
        {
            Test(@"partial class T1 { }
partial struct T2 { }
partial interface T3 { }",
                Keyword("partial"),
                Keyword("class"),
                Class("T1"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Keyword("partial"),
                Keyword("struct"),
                Struct("T2"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Keyword("partial"),
                Keyword("interface"),
                Interface("T3"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        private static readonly string[] s_contextualKeywordsOnlyValidInMethods = new string[] { "where", "from", "group", "join", "select", "into", "let", "by", "orderby", "on", "equals", "ascending", "descending" };

        /// <summary>
        /// Check for items only valid within a method declaration
        /// </summary>
        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void ContextualKeywordsOnlyValidInMethods()
        {
            foreach (var kw in s_contextualKeywordsOnlyValidInMethods)
            {
                TestInNamespace(kw + " foo",
                    Identifier(kw),
                    Identifier("foo"));
            }
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void VerbatimStringLiterals1()
        {
            TestInMethod(@"@""foo""",
                Verbatim(@"@""foo"""));
        }

        /// <summary>
        /// Should show up as soon as we get the @\" typed out
        /// </summary>
        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void VerbatimStringLiterals2()
        {
            Test(@"@""",
                Verbatim(@"@"""));
        }

        /// <summary>
        /// Parser does not currently support strings of this type
        /// </summary>
        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void VerbatimStringLiteral3()
        {
            Test(@"foo @""",
                Identifier("foo"),
                Verbatim(@"@"""));
        }

        /// <summary>
        /// Uncompleted ones should span new lines
        /// </summary>
        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void VerbatimStringLiteral4()
        {
            var code = @"

@"" foo bar 

";
            Test(code,
                Verbatim(@"@"" foo bar 

"));
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void VerbatimStringLiteral5()
        {
            var code = @"

@"" foo bar
and 
on a new line "" 
more stuff";
            TestInMethod(code,
                Verbatim(@"@"" foo bar
and 
on a new line """),
                Identifier("more"),
                Identifier("stuff"));
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void VerbatimStringLiteral6()
        {
            Test(@"string s = @""""""/*"";",
                Keyword("string"),
                Identifier("s"),
                Operators.Equals,
                Verbatim(@"@""""""/*"""),
                Punctuation.Semicolon);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void StringLiteral1()
        {
            Test(@"""foo""",
                String(@"""foo"""));
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void StringLiteral2()
        {
            Test(@"""""",
                String(@""""""));
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void CharacterLiteral1()
        {
            var code = @"'f'";
            TestInMethod(code,
                String("'f'"));
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void LinqFrom1()
        {
            var code = @"from it in foo";
            TestInExpression(code,
                Keyword("from"),
                Identifier("it"),
                Keyword("in"),
                Identifier("foo"));
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void LinqFrom2()
        {
            var code = @"from it in foo.Bar()";
            TestInExpression(code,
                Keyword("from"),
                Identifier("it"),
                Keyword("in"),
                Identifier("foo"),
                Operators.Dot,
                Identifier("Bar"),
                Punctuation.OpenParen,
                Punctuation.CloseParen);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void LinqFrom3()
        {
            // query expression are not statement expressions, but the parser parses them anyways to give better errors
            var code = @"from it in ";
            TestInMethod(code,
                Keyword("from"),
                Identifier("it"),
                Keyword("in"));
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void LinqFrom4()
        {
            var code = @"from it in ";
            TestInExpression(code,
                Keyword("from"),
                Identifier("it"),
                Keyword("in"));
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void LinqWhere1()
        {
            var code = "from it in foo where it > 42";
            TestInExpression(code,
                Keyword("from"),
                Identifier("it"),
                Keyword("in"),
                Identifier("foo"),
                Keyword("where"),
                Identifier("it"),
                Operators.GreaterThan,
                Number("42"));
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void LinqWhere2()
        {
            var code = @"from it in foo where it > ""bar""";
            TestInExpression(code,
                Keyword("from"),
                Identifier("it"),
                Keyword("in"),
                Identifier("foo"),
                Keyword("where"),
                Identifier("it"),
                Operators.GreaterThan,
                String(@"""bar"""));
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void VarContextualKeywordAtNamespaceLevel()
        {
            var code = @"var foo = 2;";
            Test(code,
                code,
                Identifier("var"),
                Identifier("foo"),
                Operators.Equals,
                Number("2"),
                Punctuation.Semicolon);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void LinqKeywordsAtNamespaceLevel()
        {
            // the contextual keywords are actual keywords since we parse top level field declaration and only give a semantic error
            Test(@"object foo = from foo in foo 
join foo in foo 
on foo equals foo 
group foo by foo 
into foo 
let foo = foo 
where foo 
orderby foo ascending, 
foo descending 
select foo;",
                Keyword("object"),
                Identifier("foo"),
                Operators.Equals,
                Keyword("from"),
                Identifier("foo"),
                Keyword("in"),
                Identifier("foo"),
                Keyword("join"),
                Identifier("foo"),
                Keyword("in"),
                Identifier("foo"),
                Keyword("on"),
                Identifier("foo"),
                Keyword("equals"),
                Identifier("foo"),
                Keyword("group"),
                Identifier("foo"),
                Keyword("by"),
                Identifier("foo"),
                Keyword("into"),
                Identifier("foo"),
                Keyword("let"),
                Identifier("foo"),
                Operators.Equals,
                Identifier("foo"),
                Keyword("where"),
                Identifier("foo"),
                Keyword("orderby"),
                Identifier("foo"),
                Keyword("ascending"),
                Punctuation.Comma,
                Identifier("foo"),
                Keyword("descending"),
                Keyword("select"),
                Identifier("foo"),
                Punctuation.Semicolon);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void ContextualKeywordsAsFieldName()
        {
            Test(@"class C { int yield, get, set, value, add, remove, global, partial, where, alias; }",
                Keyword("class"),
                Class("C"),
                Punctuation.OpenCurly,
                Keyword("int"),
                Identifier("yield"),
                Punctuation.Comma,
                Identifier("get"),
                Punctuation.Comma,
                Identifier("set"),
                Punctuation.Comma,
                Identifier("value"),
                Punctuation.Comma,
                Identifier("add"),
                Punctuation.Comma,
                Identifier("remove"),
                Punctuation.Comma,
                Identifier("global"),
                Punctuation.Comma,
                Identifier("partial"),
                Punctuation.Comma,
                Identifier("where"),
                Punctuation.Comma,
                Identifier("alias"),
                Punctuation.Semicolon,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void LinqKeywordsInFieldInitializer()
        {
            Test(@"class C { int a = from a in a 
join a in a 
on a equals a 
group a by a 
into a 
let a = a 
where a 
orderby a ascending, 
a descending 
select a; }",
                Keyword("class"),
                Class("C"),
                Punctuation.OpenCurly,
                Keyword("int"),
                Identifier("a"),
                Operators.Equals,
                Keyword("from"),
                Identifier("a"),
                Keyword("in"),
                Identifier("a"),
                Keyword("join"),
                Identifier("a"),
                Keyword("in"),
                Identifier("a"),
                Keyword("on"),
                Identifier("a"),
                Keyword("equals"),
                Identifier("a"),
                Keyword("group"),
                Identifier("a"),
                Keyword("by"),
                Identifier("a"),
                Keyword("into"),
                Identifier("a"),
                Keyword("let"),
                Identifier("a"),
                Operators.Equals,
                Identifier("a"),
                Keyword("where"),
                Identifier("a"),
                Keyword("orderby"),
                Identifier("a"),
                Keyword("ascending"),
                Punctuation.Comma,
                Identifier("a"),
                Keyword("descending"),
                Keyword("select"),
                Identifier("a"),
                Punctuation.Semicolon,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void LinqKeywordsAsTypeName()
        {
            Test(@"class var { }
struct from { }
interface join { }
enum on { }
delegate equals { }
class group { }
class by { }
class into { }
class let { }
class where { }
class orderby { }
class ascending { }
class descending { }
class select { }",
                Keyword("class"),
                Class("var"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Keyword("struct"),
                Struct("from"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Keyword("interface"),
                Interface("join"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Keyword("enum"),
                Enum("on"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Keyword("delegate"),
                Identifier("equals"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Keyword("class"),
                Class("group"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Keyword("class"),
                Class("by"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Keyword("class"),
                Class("into"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Keyword("class"),
                Class("let"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Keyword("class"),
                Class("where"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Keyword("class"),
                Class("orderby"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Keyword("class"),
                Class("ascending"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Keyword("class"),
                Class("descending"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Keyword("class"),
                Class("select"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void LinqKeywordsAsMethodParameters()
        {
            Test(@"class C { orderby M(var foo, from foo, join foo, on foo, equals foo, group foo, by foo, into foo, let foo, where foo, orderby foo, ascending foo, descending foo, select foo) { } }",
                Keyword("class"),
                Class("C"),
                Punctuation.OpenCurly,
                Identifier("orderby"),
                Identifier("M"),
                Punctuation.OpenParen,
                Identifier("var"),
                Identifier("foo"),
                Punctuation.Comma,
                Identifier("from"),
                Identifier("foo"),
                Punctuation.Comma,
                Identifier("join"),
                Identifier("foo"),
                Punctuation.Comma,
                Identifier("on"),
                Identifier("foo"),
                Punctuation.Comma,
                Identifier("equals"),
                Identifier("foo"),
                Punctuation.Comma,
                Identifier("group"),
                Identifier("foo"),
                Punctuation.Comma,
                Identifier("by"),
                Identifier("foo"),
                Punctuation.Comma,
                Identifier("into"),
                Identifier("foo"),
                Punctuation.Comma,
                Identifier("let"),
                Identifier("foo"),
                Punctuation.Comma,
                Identifier("where"),
                Identifier("foo"),
                Punctuation.Comma,
                Identifier("orderby"),
                Identifier("foo"),
                Punctuation.Comma,
                Identifier("ascending"),
                Identifier("foo"),
                Punctuation.Comma,
                Identifier("descending"),
                Identifier("foo"),
                Punctuation.Comma,
                Identifier("select"),
                Identifier("foo"),
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void LinqKeywordsInLocalVariableDeclarations()
        {
            Test(@"class C { void M() {
    var foo = (var)foo as var;
    from foo = (from)foo as from;
    join foo = (join)foo as join;
    on foo = (on)foo as on;
    equals foo = (equals)foo as equals;
    group foo = (group)foo as group;
    by foo = (by)foo as by;
    into foo = (into)foo as into;
    orderby foo = (orderby)foo as orderby;
    ascending foo = (ascending)foo as ascending;
    descending foo = (descending)foo as descending;
    select foo = (select)foo as select;
} }",
                Keyword("class"),
                Class("C"),
                Punctuation.OpenCurly,
                Keyword("void"),
                Identifier("M"),
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Keyword("var"),
                Identifier("foo"),
                Operators.Equals,
                Punctuation.OpenParen,
                Identifier("var"),
                Punctuation.CloseParen,
                Identifier("foo"),
                Keyword("as"),
                Identifier("var"),
                Punctuation.Semicolon,
                Identifier("from"),
                Identifier("foo"),
                Operators.Equals,
                Punctuation.OpenParen,
                Identifier("from"),
                Punctuation.CloseParen,
                Identifier("foo"),
                Keyword("as"),
                Identifier("from"),
                Punctuation.Semicolon,
                Identifier("join"),
                Identifier("foo"),
                Operators.Equals,
                Punctuation.OpenParen,
                Identifier("join"),
                Punctuation.CloseParen,
                Identifier("foo"),
                Keyword("as"),
                Identifier("join"),
                Punctuation.Semicolon,
                Identifier("on"),
                Identifier("foo"),
                Operators.Equals,
                Punctuation.OpenParen,
                Identifier("on"),
                Punctuation.CloseParen,
                Identifier("foo"),
                Keyword("as"),
                Identifier("on"),
                Punctuation.Semicolon,
                Identifier("equals"),
                Identifier("foo"),
                Operators.Equals,
                Punctuation.OpenParen,
                Identifier("equals"),
                Punctuation.CloseParen,
                Identifier("foo"),
                Keyword("as"),
                Identifier("equals"),
                Punctuation.Semicolon,
                Identifier("group"),
                Identifier("foo"),
                Operators.Equals,
                Punctuation.OpenParen,
                Identifier("group"),
                Punctuation.CloseParen,
                Identifier("foo"),
                Keyword("as"),
                Identifier("group"),
                Punctuation.Semicolon,
                Identifier("by"),
                Identifier("foo"),
                Operators.Equals,
                Punctuation.OpenParen,
                Identifier("by"),
                Punctuation.CloseParen,
                Identifier("foo"),
                Keyword("as"),
                Identifier("by"),
                Punctuation.Semicolon,
                Identifier("into"),
                Identifier("foo"),
                Operators.Equals,
                Punctuation.OpenParen,
                Identifier("into"),
                Punctuation.CloseParen,
                Identifier("foo"),
                Keyword("as"),
                Identifier("into"),
                Punctuation.Semicolon,
                Identifier("orderby"),
                Identifier("foo"),
                Operators.Equals,
                Punctuation.OpenParen,
                Identifier("orderby"),
                Punctuation.CloseParen,
                Identifier("foo"),
                Keyword("as"),
                Identifier("orderby"),
                Punctuation.Semicolon,
                Identifier("ascending"),
                Identifier("foo"),
                Operators.Equals,
                Punctuation.OpenParen,
                Identifier("ascending"),
                Punctuation.CloseParen,
                Identifier("foo"),
                Keyword("as"),
                Identifier("ascending"),
                Punctuation.Semicolon,
                Identifier("descending"),
                Identifier("foo"),
                Operators.Equals,
                Punctuation.OpenParen,
                Identifier("descending"),
                Punctuation.CloseParen,
                Identifier("foo"),
                Keyword("as"),
                Identifier("descending"),
                Punctuation.Semicolon,
                Identifier("select"),
                Identifier("foo"),
                Operators.Equals,
                Punctuation.OpenParen,
                Identifier("select"),
                Punctuation.CloseParen,
                Identifier("foo"),
                Keyword("as"),
                Identifier("select"),
                Punctuation.Semicolon,
                Punctuation.CloseCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void LinqKeywordsAsFieldNames()
        {
            Test(@"class C { int var, from, join, on, into, equals, let, orderby, ascending, descending, select, group, by, partial; }",
                Keyword("class"),
                Class("C"),
                Punctuation.OpenCurly,
                Keyword("int"),
                Identifier("var"),
                Punctuation.Comma,
                Identifier("from"),
                Punctuation.Comma,
                Identifier("join"),
                Punctuation.Comma,
                Identifier("on"),
                Punctuation.Comma,
                Identifier("into"),
                Punctuation.Comma,
                Identifier("equals"),
                Punctuation.Comma,
                Identifier("let"),
                Punctuation.Comma,
                Identifier("orderby"),
                Punctuation.Comma,
                Identifier("ascending"),
                Punctuation.Comma,
                Identifier("descending"),
                Punctuation.Comma,
                Identifier("select"),
                Punctuation.Comma,
                Identifier("group"),
                Punctuation.Comma,
                Identifier("by"),
                Punctuation.Comma,
                Identifier("partial"),
                Punctuation.Semicolon,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void LinqKeywordsAtFieldLevelInvalid()
        {
            Test(@"class C { string Property { from a in a 
join a in a 
on a equals a 
group a by a 
into a 
let a = a 
where a 
orderby a ascending, 
a descending 
select a; } }",
                Keyword("class"),
                Class("C"),
                Punctuation.OpenCurly,
                Keyword("string"),
                Identifier("Property"),
                Punctuation.OpenCurly,
                Identifier("from"),
                Identifier("a"),
                Keyword("in"),
                Identifier("a"),
                Identifier("join"),
                Identifier("a"),
                Keyword("in"),
                Identifier("a"),
                Identifier("on"),
                Identifier("a"),
                Identifier("equals"),
                Identifier("a"),
                Identifier("group"),
                Identifier("a"),
                Identifier("by"),
                Identifier("a"),
                Identifier("into"),
                Identifier("a"),
                Identifier("let"),
                Identifier("a"),
                Operators.Equals,
                Identifier("a"),
                Identifier("where"),
                Identifier("a"),
                Identifier("orderby"),
                Identifier("a"),
                Identifier("ascending"),
                Punctuation.Comma,
                Identifier("a"),
                Identifier("descending"),
                Identifier("select"),
                Identifier("a"),
                Punctuation.Semicolon,
                Punctuation.CloseCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void CommentSingle()
        {
            var code = "// foo";

            Test(code,
                Comment("// foo"));
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void CommentAsTrailingTrivia1()
        {
            var code = "class Bar { // foo";
            Test(code,
                Keyword("class"),
                Class("Bar"),
                Punctuation.OpenCurly,
                Comment("// foo"));
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void CommentAsLeadingTrivia1()
        {
            var code = @"
class Bar { 
  // foo
  void Method1() { }
}";
            Test(code,
                Keyword("class"),
                Class("Bar"),
                Punctuation.OpenCurly,
                Comment("// foo"),
                Keyword("void"),
                Identifier("Method1"),
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void ShebangAsFirstCommentInScript()
        {
            var code = @"#!/usr/bin/env scriptcs
System.Console.WriteLine();";
            var expected = new[]
            {
                Comment("#!/usr/bin/env scriptcs"),
                Identifier("System"),
                Operators.Dot,
                Identifier("Console"),
                Operators.Dot,
                Identifier("WriteLine"),
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.Semicolon
            };
            Test(code, code, expected, Options.Script);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void ShebangAsFirstCommentInNonScript()
        {
            var code = @"#!/usr/bin/env scriptcs
System.Console.WriteLine();";
            var expected = new[]
            {
                PPKeyword("#"),
                PPText("!/usr/bin/env scriptcs"),
                Identifier("System"),
                Operators.Dot,
                Identifier("Console"),
                Operators.Dot,
                Identifier("WriteLine"),
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.Semicolon
            };
            Test(code, code, expected, Options.Regular);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void ShebangNotAsFirstCommentInScript()
        {
            var code = @" #!/usr/bin/env scriptcs
System.Console.WriteLine();";
            var expected = new[]
            {
                PPKeyword("#"),
                PPText("!/usr/bin/env scriptcs"),
                Identifier("System"),
                Operators.Dot,
                Identifier("Console"),
                Operators.Dot,
                Identifier("WriteLine"),
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.Semicolon
            };
            Test(code, code, expected, Options.Script);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void CommentAsMethodBodyContent()
        {
            var code = @"
class Bar { 
  void Method1() {
// foo
}
}";
            Test(code,
                Keyword("class"),
                Class("Bar"),
                Punctuation.OpenCurly,
                Keyword("void"),
                Identifier("Method1"),
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Comment("// foo"),
                Punctuation.CloseCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void CommentMix1()
        {
            Test(@"// comment1 /*
class cl { }
//comment2 */",
                Comment("// comment1 /*"),
                Keyword("class"),
                Class("cl"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Comment("//comment2 */"));
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void CommentMix2()
        {
            TestInMethod(@"/**/int /**/i = 0;",
                Comment("/**/"),
                Keyword("int"),
                Comment("/**/"),
                Identifier("i"),
                Operators.Equals,
                Number("0"),
                Punctuation.Semicolon);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void XmlDocCommentOnClass()
        {
            var code = @"
/// <summary>something</summary>
class Bar { }";
            Test(code,
                XmlDoc.Delimiter("///"),
                XmlDoc.Text(" "),
                XmlDoc.Delimiter("<"),
                XmlDoc.Name("summary"),
                XmlDoc.Delimiter(">"),
                XmlDoc.Text("something"),
                XmlDoc.Delimiter("</"),
                XmlDoc.Name("summary"),
                XmlDoc.Delimiter(">"),
                Keyword("class"),
                Class("Bar"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void XmlDocCommentOnClassWithIndent()
        {
            var code = @"
    /// <summary>
    /// something
    /// </summary>
    class Bar { }";
            Test(code,
                XmlDoc.Delimiter("///"),
                XmlDoc.Text(" "),
                XmlDoc.Delimiter("<"),
                XmlDoc.Name("summary"),
                XmlDoc.Delimiter(">"),
                XmlDoc.Delimiter("///"),
                XmlDoc.Text(" something"),
                XmlDoc.Delimiter("///"),
                XmlDoc.Text(" "),
                XmlDoc.Delimiter("</"),
                XmlDoc.Name("summary"),
                XmlDoc.Delimiter(">"),
                Keyword("class"),
                Class("Bar"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void XmlDocComment_EntityReference()
        {
            var code = @"
/// <summary>&#65;</summary>
class Bar { }";
            Test(code,
                XmlDoc.Delimiter("///"),
                XmlDoc.Text(" "),
                XmlDoc.Delimiter("<"),
                XmlDoc.Name("summary"),
                XmlDoc.Delimiter(">"),
                XmlDoc.EntityReference("&#65;"),
                XmlDoc.Delimiter("</"),
                XmlDoc.Name("summary"),
                XmlDoc.Delimiter(">"),
                Keyword("class"),
                Class("Bar"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void XmlDocComment_ExteriorTriviaInsideCloseTag()
        {
            var code = @"
/// <summary>something</
/// summary>
class Bar { }";
            Test(code,
                XmlDoc.Delimiter("///"),
                XmlDoc.Text(" "),
                XmlDoc.Delimiter("<"),
                XmlDoc.Name("summary"),
                XmlDoc.Delimiter(">"),
                XmlDoc.Text("something"),
                XmlDoc.Delimiter("</"),
                XmlDoc.Delimiter("///"),
                XmlDoc.Name(" "),
                XmlDoc.Name("summary"),
                XmlDoc.Delimiter(">"),
                Keyword("class"),
                Class("Bar"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WorkItem(531155)]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void XmlDocComment_ExteriorTriviaInsideCRef()
        {
            var code = @"
/// <see cref=""System.
/// Int32""/>
class C
{
}";
            Test(code,
                XmlDoc.Delimiter("///"),
                XmlDoc.Text(" "),
                XmlDoc.Delimiter("<"),
                XmlDoc.Name("see"),
                XmlDoc.AttributeName(" "),
                XmlDoc.AttributeName("cref"),
                XmlDoc.Delimiter("="),
                XmlDoc.AttributeQuotes("\""),
                Identifier("System"),
                Operators.Dot,
                XmlDoc.Delimiter("///"),
                Identifier("Int32"),
                XmlDoc.AttributeQuotes("\""),
                XmlDoc.Delimiter("/>"),
                Keyword("class"),
                Class("C"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void XmlDocCommentOnClassWithExteriorTrivia()
        {
            var code = @"
/// <summary>
/// something
/// </summary>
class Bar { }";
            Test(code,
                XmlDoc.Delimiter("///"),
                XmlDoc.Text(" "),
                XmlDoc.Delimiter("<"),
                XmlDoc.Name("summary"),
                XmlDoc.Delimiter(">"),
                XmlDoc.Delimiter("///"),
                XmlDoc.Text(" something"),
                XmlDoc.Delimiter("///"),
                XmlDoc.Text(" "),
                XmlDoc.Delimiter("</"),
                XmlDoc.Name("summary"),
                XmlDoc.Delimiter(">"),
                Keyword("class"),
                Class("Bar"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void XmlDocComment_ExteriorTriviaNoText()
        {
            var code =
@"///<summary>
///something
///</summary>
class Bar { }";
            Test(code,
                XmlDoc.Delimiter("///"),
                XmlDoc.Delimiter("<"),
                XmlDoc.Name("summary"),
                XmlDoc.Delimiter(">"),
                XmlDoc.Delimiter("///"),
                XmlDoc.Text("something"),
                XmlDoc.Delimiter("///"),
                XmlDoc.Delimiter("</"),
                XmlDoc.Name("summary"),
                XmlDoc.Delimiter(">"),
                Keyword("class"),
                Class("Bar"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void XmlDocComment_EmptyElement()
        {
            var code = @"
/// <summary />
class Bar { }";
            Test(code,
                XmlDoc.Delimiter("///"),
                XmlDoc.Text(" "),
                XmlDoc.Delimiter("<"),
                XmlDoc.Name("summary"),
                XmlDoc.Delimiter(" "),
                XmlDoc.Delimiter("/>"),
                Keyword("class"),
                Class("Bar"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void XmlDocComment_Attribute()
        {
            var code = @"
/// <summary attribute=""value"">something</summary>
class Bar { }";

            Test(code,
                XmlDoc.Delimiter("///"),
                XmlDoc.Text(" "),
                XmlDoc.Delimiter("<"),
                XmlDoc.Name("summary"),
                XmlDoc.AttributeName(" "),
                XmlDoc.AttributeName("attribute"),
                XmlDoc.Delimiter("="),
                XmlDoc.AttributeQuotes(@""""),
                XmlDoc.AttributeValue(@"value"),
                XmlDoc.AttributeQuotes(@""""),
                XmlDoc.Delimiter(">"),
                XmlDoc.Text("something"),
                XmlDoc.Delimiter("</"),
                XmlDoc.Name("summary"),
                XmlDoc.Delimiter(">"),
                Keyword("class"),
                Class("Bar"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void XmlDocComment_AttributeInEmptyElement()
        {
            var code = @"
/// <summary attribute=""value"" />
class Bar { }";
            Test(code,
                XmlDoc.Delimiter("///"),
                XmlDoc.Text(" "),
                XmlDoc.Delimiter("<"),
                XmlDoc.Name("summary"),
                XmlDoc.AttributeName(" "),
                XmlDoc.AttributeName("attribute"),
                XmlDoc.Delimiter("="),
                XmlDoc.AttributeQuotes(@""""),
                XmlDoc.AttributeValue(@"value"),
                XmlDoc.AttributeQuotes(@""""),
                XmlDoc.Delimiter(" "),
                XmlDoc.Delimiter("/>"),
                Keyword("class"),
                Class("Bar"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void XmlDocComment_ExtraSpaces()
        {
            var code = @"
///   <   summary   attribute    =   ""value""     />
class Bar { }";
            Test(code,
                XmlDoc.Delimiter("///"),
                XmlDoc.Text("   "),
                XmlDoc.Delimiter("<"),
                XmlDoc.Name("   "),
                XmlDoc.Name("summary"),
                XmlDoc.AttributeName("   "),
                XmlDoc.AttributeName("attribute"),
                XmlDoc.Delimiter("    "),
                XmlDoc.Delimiter("="),
                XmlDoc.AttributeQuotes("   "),
                XmlDoc.AttributeQuotes(@""""),
                XmlDoc.AttributeValue(@"value"),
                XmlDoc.AttributeQuotes(@""""),
                XmlDoc.Delimiter("     "),
                XmlDoc.Delimiter("/>"),
                Keyword("class"),
                Class("Bar"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void XmlDocComment_XmlComment()
        {
            var code = @"
///<!--comment-->
class Bar { }";
            Test(code,
                XmlDoc.Delimiter("///"),
                XmlDoc.Delimiter("<!--"),
                XmlDoc.Comment("comment"),
                XmlDoc.Delimiter("-->"),
                Keyword("class"),
                Class("Bar"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void XmlDocComment_XmlCommentWithExteriorTrivia()
        {
            var code = @"
///<!--first
///second-->
class Bar { }";
            Test(code,
                XmlDoc.Delimiter("///"),
                XmlDoc.Delimiter("<!--"),
                XmlDoc.Comment("first"),
                XmlDoc.Delimiter("///"),
                XmlDoc.Comment("second"),
                XmlDoc.Delimiter("-->"),
                Keyword("class"),
                Class("Bar"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void XmlDocComment_XmlCommentInElement()
        {
            var code = @"
///<summary><!--comment--></summary>
class Bar { }";
            Test(code,
                XmlDoc.Delimiter("///"),
                XmlDoc.Delimiter("<"),
                XmlDoc.Name("summary"),
                XmlDoc.Delimiter(">"),
                XmlDoc.Delimiter("<!--"),
                XmlDoc.Comment("comment"),
                XmlDoc.Delimiter("-->"),
                XmlDoc.Delimiter("</"),
                XmlDoc.Name("summary"),
                XmlDoc.Delimiter(">"),
                Keyword("class"),
                Class("Bar"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void MultilineXmlDocComment_ExteriorTrivia()
        {
            var code =
@"/**<summary>
*comment
*</summary>*/
class Bar { }";
            Test(code,
                XmlDoc.Delimiter("/**"),
                XmlDoc.Delimiter("<"),
                XmlDoc.Name("summary"),
                XmlDoc.Delimiter(">"),
                XmlDoc.Delimiter("*"),
                XmlDoc.Text("comment"),
                XmlDoc.Delimiter("*"),
                XmlDoc.Delimiter("</"),
                XmlDoc.Name("summary"),
                XmlDoc.Delimiter(">"),
                XmlDoc.Delimiter("*/"),
                Keyword("class"),
                Class("Bar"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void XmlDocComment_CDataWithExteriorTrivia()
        {
            var code = @"
///<![CDATA[first
///second]]>
class Bar { }";
            Test(code,
                XmlDoc.Delimiter("///"),
                XmlDoc.Delimiter("<![CDATA["),
                XmlDoc.CDataSection("first"),
                XmlDoc.Delimiter("///"),
                XmlDoc.CDataSection("second"),
                XmlDoc.Delimiter("]]>"),
                Keyword("class"),
                Class("Bar"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void XmlDocComment_ProcessingDirective()
        {
            Test(@"/// <summary><?foo
/// ?></summary>
public class Program
{
    static void Main() { }
}
",
                XmlDoc.Delimiter("///"),
                XmlDoc.Text(" "),
                XmlDoc.Delimiter("<"),
                XmlDoc.Name("summary"),
                XmlDoc.Delimiter(">"),
                XmlDoc.ProcessingInstruction("<?"),
                XmlDoc.ProcessingInstruction("foo"),
                XmlDoc.Delimiter("///"),
                XmlDoc.ProcessingInstruction(" "),
                XmlDoc.ProcessingInstruction("?>"),
                XmlDoc.Delimiter("</"),
                XmlDoc.Name("summary"),
                XmlDoc.Delimiter(">"),
                Keyword("public"),
                Keyword("class"),
                Class("Program"),
                Punctuation.OpenCurly,
                Keyword("static"),
                Keyword("void"),
                Identifier("Main"),
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        [WorkItem(536321)]
        public void KeywordTypeParameters()
        {
            var code = @"class C<int> { }";
            Test(code,
                Keyword("class"),
                Class("C"),
                Punctuation.OpenAngle,
                Keyword("int"),
                Punctuation.CloseAngle,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        [WorkItem(536853)]
        public void TypeParametersWithAttribute()
        {
            var code = @"class C<[Attr] T> { }";
            Test(code,
                Keyword("class"),
                Class("C"),
                Punctuation.OpenAngle,
                Punctuation.OpenBracket,
                Identifier("Attr"),
                Punctuation.CloseBracket,
                TypeParameter("T"),
                Punctuation.CloseAngle,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void ClassTypeDeclaration1()
        {
            var code = "class C1 { } ";
            Test(code,
                Keyword("class"),
                Class("C1"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void ClassTypeDeclaration2()
        {
            var code = "class ClassName1 { } ";
            Test(code,
                Keyword("class"),
                Class("ClassName1"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void StructTypeDeclaration1()
        {
            var code = "struct Struct1 { }";
            Test(code,
                Keyword("struct"),
                Struct("Struct1"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void InterfaceDeclaration1()
        {
            var code = "interface I1 { }";
            Test(code,
                Keyword("interface"),
                Interface("I1"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void EnumDeclaration1()
        {
            var code = "enum Weekday { }";
            Test(code,
                Keyword("enum"),
                Enum("Weekday"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WorkItem(4302, "DevDiv_Projects/Roslyn")]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void ClassInEnum()
        {
            var code = "enum E { Min = System.Int32.MinValue }";
            Test(code,
                Keyword("enum"),
                Enum("E"),
                Punctuation.OpenCurly,
                Identifier("Min"),
                Operators.Equals,
                Identifier("System"),
                Operators.Dot,
                Identifier("Int32"),
                Operators.Dot,
                Identifier("MinValue"),
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void DelegateDeclaration1()
        {
            var code = "delegate void Action();";
            Test(code,
                Keyword("delegate"),
                Keyword("void"),
                Delegate("Action"),
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.Semicolon);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void GenericTypeArgument()
        {
            TestInMethod("C<T>", "M", "default(T)",
                Keyword("default"),
                Punctuation.OpenParen,
                Identifier("T"),
                Punctuation.CloseParen);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void GenericParameter()
        {
            var code = "class C1<P1> {}";
            Test(code,
                Keyword("class"),
                Class("C1"),
                Punctuation.OpenAngle,
                TypeParameter("P1"),
                Punctuation.CloseAngle,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void GenericParameters()
        {
            var code = "class C1<P1,P2> {}";
            Test(code,
                Keyword("class"),
                Class("C1"),
                Punctuation.OpenAngle,
                TypeParameter("P1"),
                Punctuation.Comma,
                TypeParameter("P2"),
                Punctuation.CloseAngle,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void GenericParameter_Interface()
        {
            var code = "interface I1<P1> {}";
            Test(code,
                Keyword("interface"),
                Interface("I1"),
                Punctuation.OpenAngle,
                TypeParameter("P1"),
                Punctuation.CloseAngle,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void GenericParameter_Struct()
        {
            var code = "struct S1<P1> {}";
            Test(code,
                Keyword("struct"),
                Struct("S1"),
                Punctuation.OpenAngle,
                TypeParameter("P1"),
                Punctuation.CloseAngle,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void GenericParameter_Delegate()
        {
            var code = "delegate void D1<P1> {}";
            Test(code,
                Keyword("delegate"),
                Keyword("void"),
                Delegate("D1"),
                Punctuation.OpenAngle,
                TypeParameter("P1"),
                Punctuation.CloseAngle,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void GenericParameter_Method()
        {
            TestInClass(@"T M<T>(T t) { return default(T); }",
                Identifier("T"),
                Identifier("M"),
                Punctuation.OpenAngle,
                TypeParameter("T"),
                Punctuation.CloseAngle,
                Punctuation.OpenParen,
                Identifier("T"),
                Identifier("t"),
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Keyword("return"),
                Keyword("default"),
                Punctuation.OpenParen,
                Identifier("T"),
                Punctuation.CloseParen,
                Punctuation.Semicolon,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void TernaryExpression()
        {
            TestInExpression("true ? 1 : 0",
                Keyword("true"),
                Operators.QuestionMark,
                Number("1"),
                Operators.Colon,
                Number("0"));
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void BaseClass()
        {
            Test("class C : B { }",
                Keyword("class"),
                Class("C"),
                Punctuation.Colon,
                Identifier("B"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void Label()
        {
            TestInMethod("foo:",
                Identifier("foo"),
                Punctuation.Colon);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void Attribute()
        {
            Test("[assembly: Foo]",
                Punctuation.OpenBracket,
                Keyword("assembly"),
                Punctuation.Colon,
                Identifier("Foo"),
                Punctuation.CloseBracket);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void TestAngleBracketsOnGenericConstraints_Bug932262()
        {
            Test(@"class C<T> where T : A<T> { }",
                Keyword("class"),
                Class("C"),
                Punctuation.OpenAngle,
                TypeParameter("T"),
                Punctuation.CloseAngle,
                Keyword("where"),
                Identifier("T"),
                Punctuation.Colon,
                Identifier("A"),
                Punctuation.OpenAngle,
                Identifier("T"),
                Punctuation.CloseAngle,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void TestYieldPositive()
        {
            TestInMethod(
@"yield return foo;",

                Keyword("yield"),
                Keyword("return"),
                Identifier("foo"),
                Punctuation.Semicolon);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void TestYieldNegative()
        {
            TestInMethod(
@"int yield;",

                Keyword("int"),
                Identifier("yield"),
                Punctuation.Semicolon);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void TestFromPositive()
        {
            TestInExpression(
@"from x in y",

                Keyword("from"),
                Identifier("x"),
                Keyword("in"),
                Identifier("y"));
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void TestFromNegative()
        {
            TestInMethod(
@"int from;",

                Keyword("int"),
                Identifier("from"),
                Punctuation.Semicolon);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void AttributeTargetSpecifiersModule()
        {
            Test(@"[module: Obsolete]",
                Punctuation.OpenBracket,
                Keyword("module"),
                Punctuation.Colon,
                Identifier("Obsolete"),
                Punctuation.CloseBracket);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void AttributeTargetSpecifiersAssembly()
        {
            Test(@"[assembly: Obsolete]",
                Punctuation.OpenBracket,
                Keyword("assembly"),
                Punctuation.Colon,
                Identifier("Obsolete"),
                Punctuation.CloseBracket);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void AttributeTargetSpecifiersOnDelegate()
        {
            TestInClass(@"[type: A] [return: A] delegate void M();",
                Punctuation.OpenBracket,
                Keyword("type"),
                Punctuation.Colon,
                Identifier("A"),
                Punctuation.CloseBracket,
                Punctuation.OpenBracket,
                Keyword("return"),
                Punctuation.Colon,
                Identifier("A"),
                Punctuation.CloseBracket,
                Keyword("delegate"),
                Keyword("void"),
                Delegate("M"),
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.Semicolon);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void AttributeTargetSpecifiersOnMethod()
        {
            TestInClass(@"[return: A] [method: A] void M() { }",
                Punctuation.OpenBracket,
                Keyword("return"),
                Punctuation.Colon,
                Identifier("A"),
                Punctuation.CloseBracket,
                Punctuation.OpenBracket,
                Keyword("method"),
                Punctuation.Colon,
                Identifier("A"),
                Punctuation.CloseBracket,
                Keyword("void"),
                Identifier("M"),
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void AttributeTargetSpecifiersOnCtor()
        {
            Test(@"class C { [method: A] C() { } }",
                Keyword("class"),
                Class("C"),
                Punctuation.OpenCurly,
                Punctuation.OpenBracket,
                Keyword("method"),
                Punctuation.Colon,
                Identifier("A"),
                Punctuation.CloseBracket,
                Identifier("C"),
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void AttributeTargetSpecifiersOnDtor()
        {
            Test(@"class C {  [method: A] ~C() { } }",
                Keyword("class"),
                Class("C"),
                Punctuation.OpenCurly,
                Punctuation.OpenBracket,
                Keyword("method"),
                Punctuation.Colon,
                Identifier("A"),
                Punctuation.CloseBracket,
                Operators.Text("~"),
                Identifier("C"),
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void AttributeTargetSpecifiersOnOperator()
        {
            TestInClass(@"[method: A] [return: A] static T operator +(T a, T b) { }",
                Punctuation.OpenBracket,
                Keyword("method"),
                Punctuation.Colon,
                Identifier("A"),
                Punctuation.CloseBracket,
                Punctuation.OpenBracket,
                Keyword("return"),
                Punctuation.Colon,
                Identifier("A"),
                Punctuation.CloseBracket,
                Keyword("static"),
                Identifier("T"),
                Keyword("operator"),
                Operators.Text("+"),
                Punctuation.OpenParen,
                Identifier("T"),
                Identifier("a"),
                Punctuation.Comma,
                Identifier("T"),
                Identifier("b"),
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void AttributeTargetSpecifiersOnEventDeclaration()
        {
            TestInClass(@"[event: A] event A E {
[param: Test]
[method: Test]
add { }
[param: Test]
[method: Test]
remove { } }
",
                Punctuation.OpenBracket,
                Keyword("event"),
                Punctuation.Colon,
                Identifier("A"),
                Punctuation.CloseBracket,
                Keyword("event"),
                Identifier("A"),
                Identifier("E"),
                Punctuation.OpenCurly,
                Punctuation.OpenBracket,
                Keyword("param"),
                Punctuation.Colon,
                Identifier("Test"),
                Punctuation.CloseBracket,
                Punctuation.OpenBracket,
                Keyword("method"),
                Punctuation.Colon,
                Identifier("Test"),
                Punctuation.CloseBracket,
                Keyword("add"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Punctuation.OpenBracket,
                Keyword("param"),
                Punctuation.Colon,
                Identifier("Test"),
                Punctuation.CloseBracket,
                Punctuation.OpenBracket,
                Keyword("method"),
                Punctuation.Colon,
                Identifier("Test"),
                Punctuation.CloseBracket,
                Keyword("remove"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void AttributeTargetSpecifiersOnPropertyAccessors()
        {
            TestInClass(@"int P {
[return: T]
[method: T]
get { }
[param: T]
[method: T]
set{ } }",
                Keyword("int"),
                Identifier("P"),
                Punctuation.OpenCurly,
                Punctuation.OpenBracket,
                Keyword("return"),
                Punctuation.Colon,
                Identifier("T"),
                Punctuation.CloseBracket,
                Punctuation.OpenBracket,
                Keyword("method"),
                Punctuation.Colon,
                Identifier("T"),
                Punctuation.CloseBracket,
                Keyword("get"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Punctuation.OpenBracket,
                Keyword("param"),
                Punctuation.Colon,
                Identifier("T"),
                Punctuation.CloseBracket,
                Punctuation.OpenBracket,
                Keyword("method"),
                Punctuation.Colon,
                Identifier("T"),
                Punctuation.CloseBracket,
                Keyword("set"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void AttributeTargetSpecifiersOnIndexers()
        {
            TestInClass(@"[property: A] int this[int i] { get; set; }",
                Punctuation.OpenBracket,
                Keyword("property"),
                Punctuation.Colon,
                Identifier("A"),
                Punctuation.CloseBracket,
                Keyword("int"),
                Keyword("this"),
                Punctuation.OpenBracket,
                Keyword("int"),
                Identifier("i"),
                Punctuation.CloseBracket,
                Punctuation.OpenCurly,
                Keyword("get"),
                Punctuation.Semicolon,
                Keyword("set"),
                Punctuation.Semicolon,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void AttributeTargetSpecifiersOnIndexerAccessors()
        {
            TestInClass(@"int this[int i] {
[return: T] 
[method: T]
get { }
[param: T]
[method: T]
set { } }",
                Keyword("int"),
                Keyword("this"),
                Punctuation.OpenBracket,
                Keyword("int"),
                Identifier("i"),
                Punctuation.CloseBracket,
                Punctuation.OpenCurly,
                Punctuation.OpenBracket,
                Keyword("return"),
                Punctuation.Colon,
                Identifier("T"),
                Punctuation.CloseBracket,
                Punctuation.OpenBracket,
                Keyword("method"),
                Punctuation.Colon,
                Identifier("T"),
                Punctuation.CloseBracket,
                Keyword("get"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Punctuation.OpenBracket,
                Keyword("param"),
                Punctuation.Colon,
                Identifier("T"),
                Punctuation.CloseBracket,
                Punctuation.OpenBracket,
                Keyword("method"),
                Punctuation.Colon,
                Identifier("T"),
                Punctuation.CloseBracket,
                Keyword("set"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void AttributeTargetSpecifiersOnField()
        {
            TestInClass(@"[field: A]
const int a = 0;",
                Punctuation.OpenBracket,
                Keyword("field"),
                Punctuation.Colon,
                Identifier("A"),
                Punctuation.CloseBracket,
                Keyword("const"),
                Keyword("int"),
                Identifier("a"),
                Operators.Equals,
                Number("0"),
                Punctuation.Semicolon);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void TestAllKeywords()
        {
            Test(@"using System;
#region TaoRegion
namespace MyNamespace
{
    abstract class Foo : Bar
    {
        bool foo = default(bool);
        byte foo1;
        char foo2;
        const int foo3 = 999;
        decimal foo4;
        delegate void D();
        double foo5;
        enum MyEnum { one, two, three };
        event D MyEvent;
        float foo6;
        static int x;
        long foo7;
        sbyte foo8;
        short foo9;
        int foo10 = sizeof(int);
        string foo11;
        uint foo12;
        ulong foo13;
        volatile ushort foo14;
        struct SomeStruct { }
        protected virtual void someMethod() { }
        public Foo(int i)
        {
            bool var = i is int;
            try
            {
                while (true)
                {
                    continue;
                    break;
                }
                switch (foo)
                {
                    case true:
                        break;
                    default:
                        break;
                }
            }
            catch (System.Exception) { }
            finally { }
            checked
            {
                int i2 = 10000;
                i2++;
            }
            do { } while (true);
            if (false) { }
            else { }
            unsafe
            {
                fixed (int* p = &x) { }
                char* buffer = stackalloc char[16];
            }
            for (int i1 = 0; i1 < 10; i1++)
            { }
            System.Collections.ArrayList al = new System.Collections.ArrayList();
            foreach (object o in al)
            {
                object o1 = o;
            }
            lock (this) { }
        }
        Foo method(Bar i, out int z)
        {
            z = 5;
            return i as Foo;
        }
        public static explicit operator Foo(int i)
        {
            return new Baz(1);
        }
        public static implicit operator Foo(double x)
        {
            return new Baz(1);
        }
        public extern void doSomething();
        internal void method2(object o)
        {
            if (o == null)
                goto Output;
            if (o is Baz)
                return;
            else
                throw new System.Exception();
        Output:
            Console.WriteLine(""Finished"");
        }
    }
    sealed class Baz : Foo
    {
        readonly int field;
        public Baz(int i)
            : base(i) { }
        public void someOtherMethod(ref int i, System.Type c)
        {
            int f = 1;
            someOtherMethod(ref f, typeof(int));
        }
        protected override void someMethod()
        {
            unchecked
            {
                int i = 1;
                i++;
            }
        }
        private void method(params object[] args) { }
    }
    interface Bar { }
}
#endregion TaoRegion",
                Keyword("using"),
                Identifier("System"),
                Punctuation.Semicolon,
                PPKeyword("#"),
                PPKeyword("region"),
                PPText("TaoRegion"),
                Keyword("namespace"),
                Identifier("MyNamespace"),
                Punctuation.OpenCurly,
                Keyword("abstract"),
                Keyword("class"),
                Class("Foo"),
                Punctuation.Colon,
                Identifier("Bar"),
                Punctuation.OpenCurly,
                Keyword("bool"),
                Identifier("foo"),
                Operators.Equals,
                Keyword("default"),
                Punctuation.OpenParen,
                Keyword("bool"),
                Punctuation.CloseParen,
                Punctuation.Semicolon,
                Keyword("byte"),
                Identifier("foo1"),
                Punctuation.Semicolon,
                Keyword("char"),
                Identifier("foo2"),
                Punctuation.Semicolon,
                Keyword("const"),
                Keyword("int"),
                Identifier("foo3"),
                Operators.Equals,
                Number("999"),
                Punctuation.Semicolon,
                Keyword("decimal"),
                Identifier("foo4"),
                Punctuation.Semicolon,
                Keyword("delegate"),
                Keyword("void"),
                Delegate("D"),
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.Semicolon,
                Keyword("double"),
                Identifier("foo5"),
                Punctuation.Semicolon,
                Keyword("enum"),
                Enum("MyEnum"),
                Punctuation.OpenCurly,
                Identifier("one"),
                Punctuation.Comma,
                Identifier("two"),
                Punctuation.Comma,
                Identifier("three"),
                Punctuation.CloseCurly,
                Punctuation.Semicolon,
                Keyword("event"),
                Identifier("D"),
                Identifier("MyEvent"),
                Punctuation.Semicolon,
                Keyword("float"),
                Identifier("foo6"),
                Punctuation.Semicolon,
                Keyword("static"),
                Keyword("int"),
                Identifier("x"),
                Punctuation.Semicolon,
                Keyword("long"),
                Identifier("foo7"),
                Punctuation.Semicolon,
                Keyword("sbyte"),
                Identifier("foo8"),
                Punctuation.Semicolon,
                Keyword("short"),
                Identifier("foo9"),
                Punctuation.Semicolon,
                Keyword("int"),
                Identifier("foo10"),
                Operators.Equals,
                Keyword("sizeof"),
                Punctuation.OpenParen,
                Keyword("int"),
                Punctuation.CloseParen,
                Punctuation.Semicolon,
                Keyword("string"),
                Identifier("foo11"),
                Punctuation.Semicolon,
                Keyword("uint"),
                Identifier("foo12"),
                Punctuation.Semicolon,
                Keyword("ulong"),
                Identifier("foo13"),
                Punctuation.Semicolon,
                Keyword("volatile"),
                Keyword("ushort"),
                Identifier("foo14"),
                Punctuation.Semicolon,
                Keyword("struct"),
                Struct("SomeStruct"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Keyword("protected"),
                Keyword("virtual"),
                Keyword("void"),
                Identifier("someMethod"),
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Keyword("public"),
                Identifier("Foo"),
                Punctuation.OpenParen,
                Keyword("int"),
                Identifier("i"),
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Keyword("bool"),
                Identifier("var"),
                Operators.Equals,
                Identifier("i"),
                Keyword("is"),
                Keyword("int"),
                Punctuation.Semicolon,
                Keyword("try"),
                Punctuation.OpenCurly,
                Keyword("while"),
                Punctuation.OpenParen,
                Keyword("true"),
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Keyword("continue"),
                Punctuation.Semicolon,
                Keyword("break"),
                Punctuation.Semicolon,
                Punctuation.CloseCurly,
                Keyword("switch"),
                Punctuation.OpenParen,
                Identifier("foo"),
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Keyword("case"),
                Keyword("true"),
                Punctuation.Colon,
                Keyword("break"),
                Punctuation.Semicolon,
                Keyword("default"),
                Punctuation.Colon,
                Keyword("break"),
                Punctuation.Semicolon,
                Punctuation.CloseCurly,
                Punctuation.CloseCurly,
                Keyword("catch"),
                Punctuation.OpenParen,
                Identifier("System"),
                Operators.Dot,
                Identifier("Exception"),
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Keyword("finally"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Keyword("checked"),
                Punctuation.OpenCurly,
                Keyword("int"),
                Identifier("i2"),
                Operators.Equals,
                Number("10000"),
                Punctuation.Semicolon,
                Identifier("i2"),
                Operators.Text("++"),
                Punctuation.Semicolon,
                Punctuation.CloseCurly,
                Keyword("do"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Keyword("while"),
                Punctuation.OpenParen,
                Keyword("true"),
                Punctuation.CloseParen,
                Punctuation.Semicolon,
                Keyword("if"),
                Punctuation.OpenParen,
                Keyword("false"),
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Keyword("else"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Keyword("unsafe"),
                Punctuation.OpenCurly,
                Keyword("fixed"),
                Punctuation.OpenParen,
                Keyword("int"),
                Operators.Star,
                Identifier("p"),
                Operators.Equals,
                Operators.Text("&"),
                Identifier("x"),
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Keyword("char"),
                Operators.Star,
                Identifier("buffer"),
                Operators.Equals,
                Keyword("stackalloc"),
                Keyword("char"),
                Punctuation.OpenBracket,
                Number("16"),
                Punctuation.CloseBracket,
                Punctuation.Semicolon,
                Punctuation.CloseCurly,
                Keyword("for"),
                Punctuation.OpenParen,
                Keyword("int"),
                Identifier("i1"),
                Operators.Equals,
                Number("0"),
                Punctuation.Semicolon,
                Identifier("i1"),
                Operators.LessThan,
                Number("10"),
                Punctuation.Semicolon,
                Identifier("i1"),
                Operators.Text("++"),
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Identifier("System"),
                Operators.Dot,
                Identifier("Collections"),
                Operators.Dot,
                Identifier("ArrayList"),
                Identifier("al"),
                Operators.Equals,
                Keyword("new"),
                Identifier("System"),
                Operators.Dot,
                Identifier("Collections"),
                Operators.Dot,
                Identifier("ArrayList"),
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.Semicolon,
                Keyword("foreach"),
                Punctuation.OpenParen,
                Keyword("object"),
                Identifier("o"),
                Keyword("in"),
                Identifier("al"),
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Keyword("object"),
                Identifier("o1"),
                Operators.Equals,
                Identifier("o"),
                Punctuation.Semicolon,
                Punctuation.CloseCurly,
                Keyword("lock"),
                Punctuation.OpenParen,
                Keyword("this"),
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Punctuation.CloseCurly,
                Identifier("Foo"),
                Identifier("method"),
                Punctuation.OpenParen,
                Identifier("Bar"),
                Identifier("i"),
                Punctuation.Comma,
                Keyword("out"),
                Keyword("int"),
                Identifier("z"),
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Identifier("z"),
                Operators.Equals,
                Number("5"),
                Punctuation.Semicolon,
                Keyword("return"),
                Identifier("i"),
                Keyword("as"),
                Identifier("Foo"),
                Punctuation.Semicolon,
                Punctuation.CloseCurly,
                Keyword("public"),
                Keyword("static"),
                Keyword("explicit"),
                Keyword("operator"),
                Identifier("Foo"),
                Punctuation.OpenParen,
                Keyword("int"),
                Identifier("i"),
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Keyword("return"),
                Keyword("new"),
                Identifier("Baz"),
                Punctuation.OpenParen,
                Number("1"),
                Punctuation.CloseParen,
                Punctuation.Semicolon,
                Punctuation.CloseCurly,
                Keyword("public"),
                Keyword("static"),
                Keyword("implicit"),
                Keyword("operator"),
                Identifier("Foo"),
                Punctuation.OpenParen,
                Keyword("double"),
                Identifier("x"),
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Keyword("return"),
                Keyword("new"),
                Identifier("Baz"),
                Punctuation.OpenParen,
                Number("1"),
                Punctuation.CloseParen,
                Punctuation.Semicolon,
                Punctuation.CloseCurly,
                Keyword("public"),
                Keyword("extern"),
                Keyword("void"),
                Identifier("doSomething"),
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.Semicolon,
                Keyword("internal"),
                Keyword("void"),
                Identifier("method2"),
                Punctuation.OpenParen,
                Keyword("object"),
                Identifier("o"),
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Keyword("if"),
                Punctuation.OpenParen,
                Identifier("o"),
                Operators.DoubleEquals,
                Keyword("null"),
                Punctuation.CloseParen,
                Keyword("goto"),
                Identifier("Output"),
                Punctuation.Semicolon,
                Keyword("if"),
                Punctuation.OpenParen,
                Identifier("o"),
                Keyword("is"),
                Identifier("Baz"),
                Punctuation.CloseParen,
                Keyword("return"),
                Punctuation.Semicolon,
                Keyword("else"),
                Keyword("throw"),
                Keyword("new"),
                Identifier("System"),
                Operators.Dot,
                Identifier("Exception"),
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.Semicolon,
                Identifier("Output"),
                Punctuation.Colon,
                Identifier("Console"),
                Operators.Dot,
                Identifier("WriteLine"),
                Punctuation.OpenParen,
                String(@"""Finished"""),
                Punctuation.CloseParen,
                Punctuation.Semicolon,
                Punctuation.CloseCurly,
                Punctuation.CloseCurly,
                Keyword("sealed"),
                Keyword("class"),
                Class("Baz"),
                Punctuation.Colon,
                Identifier("Foo"),
                Punctuation.OpenCurly,
                Keyword("readonly"),
                Keyword("int"),
                Identifier("field"),
                Punctuation.Semicolon,
                Keyword("public"),
                Identifier("Baz"),
                Punctuation.OpenParen,
                Keyword("int"),
                Identifier("i"),
                Punctuation.CloseParen,
                Punctuation.Colon,
                Keyword("base"),
                Punctuation.OpenParen,
                Identifier("i"),
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Keyword("public"),
                Keyword("void"),
                Identifier("someOtherMethod"),
                Punctuation.OpenParen,
                Keyword("ref"),
                Keyword("int"),
                Identifier("i"),
                Punctuation.Comma,
                Identifier("System"),
                Operators.Dot,
                Identifier("Type"),
                Identifier("c"),
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Keyword("int"),
                Identifier("f"),
                Operators.Equals,
                Number("1"),
                Punctuation.Semicolon,
                Identifier("someOtherMethod"),
                Punctuation.OpenParen,
                Keyword("ref"),
                Identifier("f"),
                Punctuation.Comma,
                Keyword("typeof"),
                Punctuation.OpenParen,
                Keyword("int"),
                Punctuation.CloseParen,
                Punctuation.CloseParen,
                Punctuation.Semicolon,
                Punctuation.CloseCurly,
                Keyword("protected"),
                Keyword("override"),
                Keyword("void"),
                Identifier("someMethod"),
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Keyword("unchecked"),
                Punctuation.OpenCurly,
                Keyword("int"),
                Identifier("i"),
                Operators.Equals,
                Number("1"),
                Punctuation.Semicolon,
                Identifier("i"),
                Operators.Text("++"),
                Punctuation.Semicolon,
                Punctuation.CloseCurly,
                Punctuation.CloseCurly,
                Keyword("private"),
                Keyword("void"),
                Identifier("method"),
                Punctuation.OpenParen,
                Keyword("params"),
                Keyword("object"),
                Punctuation.OpenBracket,
                Punctuation.CloseBracket,
                Identifier("args"),
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Punctuation.CloseCurly,
                Keyword("interface"),
                Interface("Bar"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Punctuation.CloseCurly,
                PPKeyword("#"),
                PPKeyword("endregion"),
                PPText("TaoRegion"));
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void TestAllOperators()
        {
            Test(@"using IO = System.IO;
public class Foo<T>
{
    public void method()
    {
        int[] a = new int[5];
        int[] var = { 1, 2, 3, 4, 5 };
        int i = a[i];
        Foo<T> f = new Foo<int>();
        f.method();
        i = i + i - i * i / i % i & i | i ^ i;
        bool b = true & false | true ^ false;
        b = !b;
        i = ~i;
        b = i < i && i > i;
        int? ii = 5;
        int f = true ? 1 : 0;
        i++;
        i--;
        b = true && false || true;
        i << 5;
        i >> 5;
        b = i == i && i != i && i <= i && i >= i;
        i += 5.0;
        i -= i;
        i *= i;
        i /= i;
        i %= i;
        i &= i;
        i |= i;
        i ^= i;
        i <<= i;
        i >>= i;
        object s = x => x + 1;
        Point point;
        unsafe
        {
            Point* p = &point;
            p->x = 10;
        }
        IO::BinaryReader br = null;
    }
}",
                Keyword("using"),
                Identifier("IO"),
                Operators.Equals,
                Identifier("System"),
                Operators.Dot,
                Identifier("IO"),
                Punctuation.Semicolon,
                Keyword("public"),
                Keyword("class"),
                Class("Foo"),
                Punctuation.OpenAngle,
                TypeParameter("T"),
                Punctuation.CloseAngle,
                Punctuation.OpenCurly,
                Keyword("public"),
                Keyword("void"),
                Identifier("method"),
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Keyword("int"),
                Punctuation.OpenBracket,
                Punctuation.CloseBracket,
                Identifier("a"),
                Operators.Equals,
                Keyword("new"),
                Keyword("int"),
                Punctuation.OpenBracket,
                Number("5"),
                Punctuation.CloseBracket,
                Punctuation.Semicolon,
                Keyword("int"),
                Punctuation.OpenBracket,
                Punctuation.CloseBracket,
                Identifier("var"),
                Operators.Equals,
                Punctuation.OpenCurly,
                Number("1"),
                Punctuation.Comma,
                Number("2"),
                Punctuation.Comma,
                Number("3"),
                Punctuation.Comma,
                Number("4"),
                Punctuation.Comma,
                Number("5"),
                Punctuation.CloseCurly,
                Punctuation.Semicolon,
                Keyword("int"),
                Identifier("i"),
                Operators.Equals,
                Identifier("a"),
                Punctuation.OpenBracket,
                Identifier("i"),
                Punctuation.CloseBracket,
                Punctuation.Semicolon,
                Identifier("Foo"),
                Punctuation.OpenAngle,
                Identifier("T"),
                Punctuation.CloseAngle,
                Identifier("f"),
                Operators.Equals,
                Keyword("new"),
                Identifier("Foo"),
                Punctuation.OpenAngle,
                Keyword("int"),
                Punctuation.CloseAngle,
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.Semicolon,
                Identifier("f"),
                Operators.Dot,
                Identifier("method"),
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.Semicolon,
                Identifier("i"),
                Operators.Equals,
                Identifier("i"),
                Operators.Text("+"),
                Identifier("i"),
                Operators.Text("-"),
                Identifier("i"),
                Operators.Star,
                Identifier("i"),
                Operators.Text("/"),
                Identifier("i"),
                Operators.Text("%"),
                Identifier("i"),
                Operators.Text("&"),
                Identifier("i"),
                Operators.Text("|"),
                Identifier("i"),
                Operators.Text("^"),
                Identifier("i"),
                Punctuation.Semicolon,
                Keyword("bool"),
                Identifier("b"),
                Operators.Equals,
                Keyword("true"),
                Operators.Text("&"),
                Keyword("false"),
                Operators.Text("|"),
                Keyword("true"),
                Operators.Text("^"),
                Keyword("false"),
                Punctuation.Semicolon,
                Identifier("b"),
                Operators.Equals,
                Operators.Exclamation,
                Identifier("b"),
                Punctuation.Semicolon,
                Identifier("i"),
                Operators.Equals,
                Operators.Text("~"),
                Identifier("i"),
                Punctuation.Semicolon,
                Identifier("b"),
                Operators.Equals,
                Identifier("i"),
                Operators.LessThan,
                Identifier("i"),
                Operators.DoubleAmpersand,
                Identifier("i"),
                Operators.GreaterThan,
                Identifier("i"),
                Punctuation.Semicolon,
                Keyword("int"),
                Operators.QuestionMark,
                Identifier("ii"),
                Operators.Equals,
                Number("5"),
                Punctuation.Semicolon,
                Keyword("int"),
                Identifier("f"),
                Operators.Equals,
                Keyword("true"),
                Operators.QuestionMark,
                Number("1"),
                Operators.Colon,
                Number("0"),
                Punctuation.Semicolon,
                Identifier("i"),
                Operators.Text("++"),
                Punctuation.Semicolon,
                Identifier("i"),
                Operators.Text("--"),
                Punctuation.Semicolon,
                Identifier("b"),
                Operators.Equals,
                Keyword("true"),
                Operators.DoubleAmpersand,
                Keyword("false"),
                Operators.DoublePipe,
                Keyword("true"),
                Punctuation.Semicolon,
                Identifier("i"),
                Operators.Text("<<"),
                Number("5"),
                Punctuation.Semicolon,
                Identifier("i"),
                Operators.Text(">>"),
                Number("5"),
                Punctuation.Semicolon,
                Identifier("b"),
                Operators.Equals,
                Identifier("i"),
                Operators.DoubleEquals,
                Identifier("i"),
                Operators.DoubleAmpersand,
                Identifier("i"),
                Operators.ExclamationEquals,
                Identifier("i"),
                Operators.DoubleAmpersand,
                Identifier("i"),
                Operators.Text("<="),
                Identifier("i"),
                Operators.DoubleAmpersand,
                Identifier("i"),
                Operators.Text(">="),
                Identifier("i"),
                Punctuation.Semicolon,
                Identifier("i"),
                Operators.Text("+="),
                Number("5.0"),
                Punctuation.Semicolon,
                Identifier("i"),
                Operators.Text("-="),
                Identifier("i"),
                Punctuation.Semicolon,
                Identifier("i"),
                Operators.Text("*="),
                Identifier("i"),
                Punctuation.Semicolon,
                Identifier("i"),
                Operators.Text("/="),
                Identifier("i"),
                Punctuation.Semicolon,
                Identifier("i"),
                Operators.Text("%="),
                Identifier("i"),
                Punctuation.Semicolon,
                Identifier("i"),
                Operators.Text("&="),
                Identifier("i"),
                Punctuation.Semicolon,
                Identifier("i"),
                Operators.Text("|="),
                Identifier("i"),
                Punctuation.Semicolon,
                Identifier("i"),
                Operators.Text("^="),
                Identifier("i"),
                Punctuation.Semicolon,
                Identifier("i"),
                Operators.Text("<<="),
                Identifier("i"),
                Punctuation.Semicolon,
                Identifier("i"),
                Operators.Text(">>="),
                Identifier("i"),
                Punctuation.Semicolon,
                Keyword("object"),
                Identifier("s"),
                Operators.Equals,
                Identifier("x"),
                Operators.Text("=>"),
                Identifier("x"),
                Operators.Text("+"),
                Number("1"),
                Punctuation.Semicolon,
                Identifier("Point"),
                Identifier("point"),
                Punctuation.Semicolon,
                Keyword("unsafe"),
                Punctuation.OpenCurly,
                Identifier("Point"),
                Operators.Star,
                Identifier("p"),
                Operators.Equals,
                Operators.Text("&"),
                Identifier("point"),
                Punctuation.Semicolon,
                Identifier("p"),
                Operators.Text("->"),
                Identifier("x"),
                Operators.Equals,
                Number("10"),
                Punctuation.Semicolon,
                Punctuation.CloseCurly,
                Identifier("IO"),
                Operators.Text("::"),
                Identifier("BinaryReader"),
                Identifier("br"),
                Operators.Equals,
                Keyword("null"),
                Punctuation.Semicolon,
                Punctuation.CloseCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void TestPartialMethodWithNamePartial()
        {
            Test(@"partial class C
{
    partial void partial(string bar);
    partial void partial(string baz) {}
    partial int Foo();
    partial int Foo() {}
    public partial void 
    partial void
}
",
                Keyword("partial"),
                Keyword("class"),
                Class("C"),
                Punctuation.OpenCurly,
                Keyword("partial"),
                Keyword("void"),
                Identifier("partial"),
                Punctuation.OpenParen,
                Keyword("string"),
                Identifier("bar"),
                Punctuation.CloseParen,
                Punctuation.Semicolon,
                Keyword("partial"),
                Keyword("void"),
                Identifier("partial"),
                Punctuation.OpenParen,
                Keyword("string"),
                Identifier("baz"),
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Keyword("partial"),
                Keyword("int"),
                Identifier("Foo"),
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.Semicolon,
                Keyword("partial"),
                Keyword("int"),
                Identifier("Foo"),
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Keyword("public"),
                Keyword("partial"),
                Keyword("void"),
                Identifier("partial"),
                Keyword("void"),
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void ValueInSetterAndAnonymousTypePropertyName()
        {
            Test(@"class C { int P { set { var t = new { value = value }; } } }",
                Keyword("class"),
                Class("C"),
                Punctuation.OpenCurly,
                Keyword("int"),
                Identifier("P"),
                Punctuation.OpenCurly,
                Keyword("set"),
                Punctuation.OpenCurly,
                Keyword("var"),
                Identifier("t"),
                Operators.Equals,
                Keyword("new"),
                Punctuation.OpenCurly,
                Identifier("value"),
                Operators.Equals,
                Identifier("value"),
                Punctuation.CloseCurly,
                Punctuation.Semicolon,
                Punctuation.CloseCurly,
                Punctuation.CloseCurly,
                Punctuation.CloseCurly);
        }

        [WorkItem(538680)]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void TestValueInLabel()
        {
            Test(@"class C
{
    int X
    {
        set { value:; }
    }
}
",
                Keyword("class"),
                Class("C"),
                Punctuation.OpenCurly,
                Keyword("int"),
                Identifier("X"),
                Punctuation.OpenCurly,
                Keyword("set"),
                Punctuation.OpenCurly,
                Identifier("value"),
                Punctuation.Colon,
                Punctuation.Semicolon,
                Punctuation.CloseCurly,
                Punctuation.CloseCurly,
                Punctuation.CloseCurly);
        }

        [WorkItem(541150)]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void TestGenericVar()
        {
            Test(@"using System;
 
static class Program
{
    static void Main()
    {
        var x = 1;
    }
}
 
class var<T> { }
",
                Keyword("using"),
                Identifier("System"),
                Punctuation.Semicolon,
                Keyword("static"),
                Keyword("class"),
                Class("Program"),
                Punctuation.OpenCurly,
                Keyword("static"),
                Keyword("void"),
                Identifier("Main"),
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Keyword("var"),
                Identifier("x"),
                Operators.Equals,
                Number("1"),
                Punctuation.Semicolon,
                Punctuation.CloseCurly,
                Punctuation.CloseCurly,
                Keyword("class"),
                Class("var"),
                Punctuation.OpenAngle,
                TypeParameter("T"),
                Punctuation.CloseAngle,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WorkItem(541154)]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void TestInaccessibleVar()
        {
            Test(@"using System;
 
class A
{
    private class var { }
}
 
class B : A
{
    static void Main()
    {
        var x = 1;
    }
}
",
                Keyword("using"),
                Identifier("System"),
                Punctuation.Semicolon,
                Keyword("class"),
                Class("A"),
                Punctuation.OpenCurly,
                Keyword("private"),
                Keyword("class"),
                Class("var"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Punctuation.CloseCurly,
                Keyword("class"),
                Class("B"),
                Punctuation.Colon,
                Identifier("A"),
                Punctuation.OpenCurly,
                Keyword("static"),
                Keyword("void"),
                Identifier("Main"),
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Keyword("var"),
                Identifier("x"),
                Operators.Equals,
                Number("1"),
                Punctuation.Semicolon,
                Punctuation.CloseCurly,
                Punctuation.CloseCurly);
        }

        [WorkItem(541613)]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void TestEscapedVar()
        {
            Test(@"class Program
{
    static void Main(string[] args)
    {
        @var v = 1;
    }
}",
                Keyword("class"),
                Class("Program"),
                Punctuation.OpenCurly,
                Keyword("static"),
                Keyword("void"),
                Identifier("Main"),
                Punctuation.OpenParen,
                Keyword("string"),
                Punctuation.OpenBracket,
                Punctuation.CloseBracket,
                Identifier("args"),
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Identifier("@var"),
                Identifier("v"),
                Operators.Equals,
                Number("1"),
                Punctuation.Semicolon,
                Punctuation.CloseCurly,
                Punctuation.CloseCurly);
        }

        [WorkItem(542432)]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void TestVar()
        {
            Test(@"class Program
{
    class var<T> { }
    static var<int> GetVarT() { return null; }
    static void Main()
    {
        var x = GetVarT();
        var y = new var<int>();
    }
}",
                Keyword("class"),
                Class("Program"),
                Punctuation.OpenCurly,
                Keyword("class"),
                Class("var"),
                Punctuation.OpenAngle,
                TypeParameter("T"),
                Punctuation.CloseAngle,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Keyword("static"),
                Identifier("var"),
                Punctuation.OpenAngle,
                Keyword("int"),
                Punctuation.CloseAngle,
                Identifier("GetVarT"),
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Keyword("return"),
                Keyword("null"),
                Punctuation.Semicolon,
                Punctuation.CloseCurly,
                Keyword("static"),
                Keyword("void"),
                Identifier("Main"),
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Keyword("var"),
                Identifier("x"),
                Operators.Equals,
                Identifier("GetVarT"),
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.Semicolon,
                Keyword("var"),
                Identifier("y"),
                Operators.Equals,
                Keyword("new"),
                Identifier("var"),
                Punctuation.OpenAngle,
                Keyword("int"),
                Punctuation.CloseAngle,
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.Semicolon,
                Punctuation.CloseCurly,
                Punctuation.CloseCurly);
        }

        [WorkItem(543123)]
        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void TestVar2()
        {
            Test(@"class Program
{
    void Main(string[] args)
    {
        foreach (var v in args) { }
    }
}
",
                Keyword("class"),
                Class("Program"),
                Punctuation.OpenCurly,
                Keyword("void"),
                Identifier("Main"),
                Punctuation.OpenParen,
                Keyword("string"),
                Punctuation.OpenBracket,
                Punctuation.CloseBracket,
                Identifier("args"),
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Keyword("foreach"),
                Punctuation.OpenParen,
                Identifier("var"),
                Identifier("v"),
                Keyword("in"),
                Identifier("args"),
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Punctuation.CloseCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void InterpolatedStrings1()
        {
            var code = @"
var x = ""World"";
var y = $""Hello, {x}"";
";
            TestInMethod(code,
                Keyword("var"),
                Identifier("x"),
                Operators.Equals,
                String("\"World\""),
                Punctuation.Semicolon,
                Keyword("var"),
                Identifier("y"),
                Operators.Equals,
                String("$\""),
                String("Hello, "),
                Punctuation.OpenCurly,
                Identifier("x"),
                Punctuation.CloseCurly,
                String("\""),
                Punctuation.Semicolon);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void InterpolatedStrings2()
        {
            var code = @"
var a = ""Hello"";
var b = ""World"";
var c = $""{a}, {b}"";
";
            TestInMethod(code,
                Keyword("var"),
                Identifier("a"),
                Operators.Equals,
                String("\"Hello\""),
                Punctuation.Semicolon,
                Keyword("var"),
                Identifier("b"),
                Operators.Equals,
                String("\"World\""),
                Punctuation.Semicolon,
                Keyword("var"),
                Identifier("c"),
                Operators.Equals,
                String("$\""),
                Punctuation.OpenCurly,
                Identifier("a"),
                Punctuation.CloseCurly,
                String(", "),
                Punctuation.OpenCurly,
                Identifier("b"),
                Punctuation.CloseCurly,
                String("\""),
                Punctuation.Semicolon);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void InterpolatedStrings3()
        {
            var code = @"
var a = ""Hello"";
var b = ""World"";
var c = $@""{a}, {b}"";
";
            TestInMethod(code,
                Keyword("var"),
                Identifier("a"),
                Operators.Equals,
                String("\"Hello\""),
                Punctuation.Semicolon,
                Keyword("var"),
                Identifier("b"),
                Operators.Equals,
                String("\"World\""),
                Punctuation.Semicolon,
                Keyword("var"),
                Identifier("c"),
                Operators.Equals,
                Verbatim("$@\""),
                Punctuation.OpenCurly,
                Identifier("a"),
                Punctuation.CloseCurly,
                Verbatim(", "),
                Punctuation.OpenCurly,
                Identifier("b"),
                Punctuation.CloseCurly,
                Verbatim("\""),
                Punctuation.Semicolon);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void ExceptionFilter1()
        {
            var code = @"
try
{
}
catch when (true)
{
}
";
            TestInMethod(code,
                Keyword("try"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Keyword("catch"),
                Keyword("when"),
                Punctuation.OpenParen,
                Keyword("true"),
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void ExceptionFilter2()
        {
            var code = @"
try
{
}
catch (System.Exception) when (true)
{
}
";
            TestInMethod(code,
                Keyword("try"),
                Punctuation.OpenCurly,
                Punctuation.CloseCurly,
                Keyword("catch"),
                Punctuation.OpenParen,
                Identifier("System"),
                Operators.Dot,
                Identifier("Exception"),
                Punctuation.CloseParen,
                Keyword("when"),
                Punctuation.OpenParen,
                Keyword("true"),
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Punctuation.CloseCurly);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void OutVar()
        {
            var code = @"
F(out var);";
            TestInMethod(code,
                Identifier("F"),
                Punctuation.OpenParen,
                Keyword("out"),
                Identifier("var"),
                Punctuation.CloseParen,
                Punctuation.Semicolon);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void ReferenceDirective()
        {
            var code = @"
#r ""file.dll""";
            Test(code,
                PPKeyword("#"),
                PPKeyword("r"),
                String("\"file.dll\""));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void LoadDirective()
        {
            var code = @"
#load ""file.csx""";
            Test(code,
                PPKeyword("#"),
                PPKeyword("load"),
                String("\"file.csx\""));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void IncompleteAwaitInNonAsyncContext()
        {
            var code = @"
void M()
{
    var x = await
}";
            TestInClass(code,
                Keyword("void"),
                Identifier("M"),
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Keyword("var"),
                Identifier("x"),
                Operators.Equals,
                Keyword("await"),
                Punctuation.CloseCurly);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Classification)]
        public void CompleteAwaitInNonAsyncContext()
        {
            var code = @"
void M()
{
    var x = await;
}";
            TestInClass(code,
                Keyword("void"),
                Identifier("M"),
                Punctuation.OpenParen,
                Punctuation.CloseParen,
                Punctuation.OpenCurly,
                Keyword("var"),
                Identifier("x"),
                Operators.Equals,
                Identifier("await"),
                Punctuation.Semicolon,
                Punctuation.CloseCurly);
        }
    }
}
