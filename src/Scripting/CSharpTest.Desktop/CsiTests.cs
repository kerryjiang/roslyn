﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
extern alias PortableTestUtils;

using System;
using Roslyn.Test.Utilities;
using Roslyn.Utilities;
using Xunit;
using TestBase = PortableTestUtils::Roslyn.Test.Utilities.TestBase;
using AssertEx = PortableTestUtils::Roslyn.Test.Utilities.AssertEx;

namespace Microsoft.CodeAnalysis.Scripting.Hosting.CSharp.UnitTests
{
    public class CsiTests : TestBase
    {
        private string CsiPath => typeof(Csi).Assembly.Location;

        /// <summary>
        /// csi should use the current working directory of its environment to resolve relative paths specified on command line.
        /// </summary>
        [Fact]
        public void CurrentWorkingDirectory1()
        {
            var dir = Temp.CreateDirectory();
            dir.CreateFile("a.csx").WriteAllText(@"Console.Write(Environment.CurrentDirectory + ';' + typeof(C).Name);");
            dir.CreateFile("C.dll").WriteAllBytes(TestResources.General.C1);
            
            var result = ProcessUtilities.Run(CsiPath, "/r:C.dll a.csx", workingDirectory: dir.Path);
            AssertEx.AssertEqualToleratingWhitespaceDifferences(dir.Path + ";C", result.Output);
            Assert.False(result.ContainsErrors);
        }

        /// <summary>
        /// csi does NOT use LIB environment variable to populate reference search paths.
        /// </summary>
        [Fact]
        public void ReferenceSearchPaths_LIB()
        {
            var cwd = Temp.CreateDirectory();
            cwd.CreateFile("a.csx").WriteAllText(@"Console.Write(typeof(C).Name);");

            var dir = Temp.CreateDirectory();
            dir.CreateFile("C.dll").WriteAllBytes(TestResources.General.C1);

            var result = ProcessUtilities.Run(CsiPath, "/r:C.dll a.csx", workingDirectory: cwd.Path, additionalEnvironmentVars: new[] { KeyValuePair.Create("LIB", dir.Path) });

            // error CS0006: Metadata file 'C.dll' could not be found
            Assert.True(result.Output.StartsWith("error CS0006", StringComparison.Ordinal));
            Assert.True(result.ContainsErrors);
        }

        /// <summary>
        /// csi does use SDK path (FX dir)
        /// </summary>
        [Fact]
        public void ReferenceSearchPaths_Sdk()
        {
            var cwd = Temp.CreateDirectory();
            cwd.CreateFile("a.csx").WriteAllText(@"Console.Write(typeof(DataSet).Name);");

            var result = ProcessUtilities.Run(CsiPath, "/r:System.Data.dll /u:System.Data;System a.csx", workingDirectory: cwd.Path);

            AssertEx.AssertEqualToleratingWhitespaceDifferences("DataSet", result.Output);
            Assert.False(result.ContainsErrors);
        }

        [Fact]
        public void DefaultUsings()
        {
            var source = @"
dynamic d = new ExpandoObject();
Process p = new Process();
Expression<Func<int>> e = () => 1;
var squares = from x in new[] { 1, 2, 3 } select x * x;
var sb = new StringBuilder();
var list = new List<int>();
var stream = new MemoryStream();
await Task.Delay(10);

Console.Write(""OK"");
";

            var cwd = Temp.CreateDirectory();
            cwd.CreateFile("a.csx").WriteAllText(source);

            var result = ProcessUtilities.Run(CsiPath, "a.csx", workingDirectory: cwd.Path);

            AssertEx.AssertEqualToleratingWhitespaceDifferences("OK", result.Output);
            Assert.False(result.ContainsErrors);
        }
    }
}
