using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Microsoft.MixedReality.Toolkit.SDK.UX.Interactable.ClassProvider.BaseInteractableClassProvider;

namespace Microsoft.MixedReality.Toolkit.SDK.UX.Interactable.ClassProvider
{
    public sealed class InteractableClassLoader
    {
        private static readonly object loaderLock = new object();
        private static bool loaded = false;
        private static Dictionary<InteractableType, List<Type>> cache = 
            new Dictionary<InteractableType, List<Type>>();

        public static List<Type> GetTypes(InteractableType interactableType)
        {
            EnsureInitialized();
            return cache[interactableType];
        }

        private static void EnsureInitialized()
        {
            if (!loaded)
            {
                var classProviders = Resources.FindObjectsOfTypeAll<ScriptableObject>().OfType<BaseInteractableClassProvider>();
                Dictionary<InteractableType, List<Type>> localCache = new Dictionary<InteractableType, List<Type>>();
                foreach (InteractableType type in Enum.GetValues(typeof(InteractableType)))
                {
                    // HashSet is used to ensure no duplicates in the case that multiple class
                    // providers proffer the same types.
                    HashSet<Type> types = new HashSet<Type>();
                    foreach (var classProvider in classProviders)
                    {
                        types.UnionWith(classProvider.GetTypes(type));
                    }
                    localCache[type] = types.ToList<Type>();
                }

                lock (loaderLock)
                {
                    if (!loaded)
                    {
                        cache = localCache;
                        loaded = true;
                    }
                }
            }
        }
    }
}
