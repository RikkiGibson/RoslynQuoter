using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using QuoterAnalyzer;
using System.IO;

namespace QuoterAnalyzer.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {

        //No diagnostics expected to show up
        [TestMethod]
        public void TestMethod1()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void TestMethod2()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "QuoterAnalyzer",
                Message = String.Format("Type name '{0}' contains lowercase letters", "TypeName"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 11, 15)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TYPENAME
        {   
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        [TestMethod]
        public void TestMethod3()
        {
            var input = File.ReadAllText("Resources/QuoterSimpleTestBefore.cs");
            var expectedOutput = File.ReadAllText("Resources/QuoterSimpleTestAfter.cs");
            VerifyCSharpDiagnostic(input, new DiagnosticResult
            {
                Id = QuoterAnalyzerAnalyzer.DiagnosticId,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 14, 30) },
                Message = "SyntaxFactory.Parse argument can be quoted",
                Severity = DiagnosticSeverity.Info
            });
            VerifyCSharpFix(input, expectedOutput);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new QuoterAnalyzerCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new QuoterAnalyzerAnalyzer();
        }
    }
}