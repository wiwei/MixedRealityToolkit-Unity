using Microsoft.MixedReality.Toolkit.SDK.UX.Interactable.Events;
using Microsoft.MixedReality.Toolkit.SDK.UX.Interactable.States;
using Microsoft.MixedReality.Toolkit.SDK.UX.Interactable.Themes;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.MixedReality.Toolkit.SDK.UX.Interactable.ClassProvider
{
    public sealed class DefaultInteractableClassProvider : BaseInteractableClassProvider
    {
        private static readonly Dictionary<InteractableType, List<Type>> providedTypes = new Dictionary<InteractableType, List<Type>>()
        {
            {
                InteractableType.Event,
                new List<Type>()
                {
                    typeof(InteractableAudioReceiver),
                    typeof(InteractableOnClickReceiver),
                    typeof(InteractableOnFocusReceiver),
                    typeof(InteractableOnHoldReceiver),
                    typeof(InteractableOnPressReceiver),
                    typeof(InteractableOnToggleReceiver)
                }
            },
            {
                InteractableType.State,
                new List<Type>()
                {
                    typeof(InteractableStates)
                }
            },
            {
                InteractableType.Theme,
                new List<Type>()
                {
                    typeof(InteractableActivateTheme),
                    typeof(InteractableAnimatorTheme),
                    typeof(InteractableAudioTheme),
                    typeof(InteractableColorChildrenTheme),
                    typeof(InteractableColorTheme),
                    typeof(InteractableMaterialTheme),
                    typeof(InteractableOffsetTheme),
                    typeof(InteractableRotationTheme),
                    typeof(InteractableScaleTheme),
                    typeof(InteractableShaderTheme),
                    typeof(InteractableStringTheme),
                    typeof(InteractableTextureTheme),
                    typeof(ScaleOffsetColorTheme),
                }
            }
        };

        protected override Dictionary<InteractableType, List<Type>> GetInteractableTypeDictionary()
        {
            return providedTypes;
        }
    }
}
