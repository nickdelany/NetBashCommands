using System;
using System.Linq;
using System.Reflection;
using System.Text;
using NetBash;

namespace NetBashCommands.Reflection
{
    [WebCommand("assemblies", "Display loaded assemblies. Use ? for search options.")]
    public class AssembliesCommand : IWebCommand
    {
        private const string Help = 
@"nnn - assembly name starts with nnn
+nnn - assembly name contains nnn
!nnn - assembly name does not start with nnn
-nnn - assembly name does not contain nnn
a +b -c !d returns
names beginning with a and containing b 
that do not contain c nor start with d";

        public bool ReturnHtml
        {
            get { return false; }
        }

        public string Process(string[] args)
        {
            if (args.Length == 1 && args[0].Equals("?"))
                return Help;

            StringBuilder ret = new StringBuilder();

            var begins = args.Where(a => !(a.StartsWith("-") || a.StartsWith("+") || a.StartsWith("!"))).ToArray();
            var contains = args.Where(a => a.StartsWith("+")).ToArray();
            var notBegins = args.Where(a => a.StartsWith("!")).ToArray();
            var notContains = args.Where(a => a.StartsWith("-")).ToArray();

            foreach (AssemblyName assembly in 
                AppDomain.CurrentDomain.GetAssemblies()
                    .Select(assembly => assembly.GetName())
                    .Where(assembly => args.Length == 0 
                        || MatchesSearchTerm(begins, contains, notBegins, notContains, assembly.Name)))
            {
                ret.AppendLine(string.Format("{0}: {1}", assembly.Name, assembly.Version));
            }

            return ret.ToString();
        }

        private static bool MatchesSearchTerm(string[] begins, string[] contains, string[] notBegins, string[] notContains, string name)
        {
            string lower = name.ToLowerInvariant();

            return
                (begins.Length == 0 || begins.Any(a => lower.StartsWith(a.ToLowerInvariant())))
             && (contains.Length == 0 || contains.Any(a => lower.Contains(a.Substring(1).ToLowerInvariant())))
             && (notContains.Length == 0 || !notContains.Any(a => lower.Contains(a.Substring(1).ToLowerInvariant())))
             && (notBegins.Length == 0 || !notBegins.Any(a => lower.StartsWith(a.Substring(1).ToLowerInvariant())));
        }
    }
}
 