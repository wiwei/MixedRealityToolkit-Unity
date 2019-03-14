using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SDK.UX.Interactable.ClassProvider
{
    public abstract class BaseInteractableClassProvider : ScriptableObject
    {
        public enum InteractableType
        {
            None = 0,
            Event,
            State,
            Theme
        }

        public List<Type> GetTypes(InteractableType interactableType)
        {
            List<Type> types;
            if (!GetInteractableTypeDictionary().TryGetValue(interactableType, out types))
            {
                types = new List<Type>();
            }
            return types;
        }

        protected abstract Dictionary<InteractableType, List<Type>> GetInteractableTypeDictionary();
    }
}