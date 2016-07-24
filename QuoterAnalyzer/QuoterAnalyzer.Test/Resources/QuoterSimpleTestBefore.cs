using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuoterAnalyzer.Test.Resources
{
    class QuoterSimpleTest
    {
        void Test()
        {
            var expression = SyntaxFactory.ParseExpression("2 + 2");
        }
    }
}
