using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESharpLibrary.Test
{
    public class TestDiscovery
    {
        public static IEnumerable<MethodDefinition> DiscoverTests(ModuleDefinition module)
        {
            foreach (var t in module.Types)
            {
                // skip our own generated tests
                if (t.Name.EndsWith("_ETest"))
                {
                    continue;
                }

                // only use explicit ETest classes
                if (!t.CustomAttributes.Any((a => a.AttributeType.Name == "ETestFixtureAttribute")))
                {
                    continue;
                }


                var allMethods = t.Methods;
                var tests = allMethods.Where(x => x.CustomAttributes.Any
                    (a => a.AttributeType.Name == "TestAttribute" /*NUnit*/ ||
                    a.AttributeType.Name == "TestMethodAttribute" /*MSTest*/));

                foreach (var test in tests)
                {
                    // don't use ignored tests
                    if (test.CustomAttributes.Any((a => a.AttributeType.Name == "IgnoreAttribute")))
                        continue;

                    yield return test;
                }
            }

        }
    }

}