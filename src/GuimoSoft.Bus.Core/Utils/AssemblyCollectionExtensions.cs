using System.Collections.Generic;
using System.Reflection;
using GuimoSoft.Bus.Core.Internal;

namespace GuimoSoft.Bus.Core.Utils
{
    internal static class AssemblyCollectionExtensions
    {
        public static void TryAddAssembly(this ICollection<Assembly> assemblies, Assembly assembly)
        {
            lock (Singletons._lock)
            {
                if (!assemblies.Contains(assembly))
                    assemblies.Add(assembly);
            }
        }
    }
}
