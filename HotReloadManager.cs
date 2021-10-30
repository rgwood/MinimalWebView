using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection.Metadata;
using MinimalWebView;
using System.Diagnostics;

[assembly: MetadataUpdateHandler(typeof(HotReloadManager))]

namespace MinimalWebView
{
    internal static class HotReloadManager
    {
        public static void ClearCache(Type[]? types)
        {
            Debug.WriteLine("ClearCache");
        }

        public static void UpdateApplication(Type[]? types)
        {
            Debug.WriteLine("UpdateApplication");
        }
    }
}
