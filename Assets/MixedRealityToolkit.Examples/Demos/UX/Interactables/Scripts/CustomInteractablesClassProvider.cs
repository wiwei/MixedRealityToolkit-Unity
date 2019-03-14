using Microsoft.MixedReality.Toolkit.SDK.UX.Interactable.ClassProvider;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SDK.UX.Interactable.Events
{ 
    [CreateAssetMenu(menuName = "test")]
    public sealed class CustomInteractablesClassProvider : BaseInteractableClassProvider
    {
        private static readonly Dictionary<InteractableType, List<Type>> providedTypes = new Dictionary<InteractableType, List<Type>>()
        {
            {
                InteractableType.Event,
                new List<Type>() { typeof(CustomInteractablesReceiver) }
            }
        };

        protected override Dictionary<InteractableType, List<Type>> GetInteractableTypeDictionary()
        {
            return providedTypes;
        }
    }
}