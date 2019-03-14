﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Core.Definitions.Devices;
using Microsoft.MixedReality.Toolkit.Core.Definitions.Utilities;
using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.MixedReality.Toolkit.Core.Attributes
{
    /// <summary>
    /// Attach to a controller device class to make it show up in the controller mapping profile.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MixedRealityControllerAttribute : Attribute
    {
        /// <summary>
        /// The SupportedControllerType to which the controller device belongs to.
        /// </summary>
        public SupportedControllerType SupportedControllerType { get; }

        /// <summary>
        /// List of handedness values supported by the respective controller.
        /// </summary>
        public Handedness[] SupportedHandedness { get; }

        /// <summary>
        /// Path to image file used when displaying an icon in the UI.
        /// </summary>
        public string TexturePath { get; }

        /// <summary>
        /// Additional flags for configuring controller capabilities.
        /// </summary>
        public MixedRealityControllerConfigurationFlags Flags { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="supportedControllerType"></param>
        /// <param name="supportedHandedness"></param>
        /// <param name="texturePath"></param>
        /// <param name="flags"></param>
        public MixedRealityControllerAttribute(
            SupportedControllerType supportedControllerType, 
            Handedness[] supportedHandedness,
            string texturePath = "", 
            MixedRealityControllerConfigurationFlags flags = 0)
        {
            SupportedControllerType = supportedControllerType;
            SupportedHandedness = supportedHandedness;
            TexturePath = texturePath;
            Flags = flags;
        }

        /// <summary>
        /// Convenience function for retrieving the attribute given a certain class type.
        /// </summary>
        /// <remarks>
        /// This function is only available in a UnityEditor context.
        /// </remarks>
        public static MixedRealityControllerAttribute Find(Type type)
        {
#if WINDOWS_UWP && !ENABLE_IL2CPP
            // Type.GetCustomAttributes() doesn't exist on UWP .NET, so we have to indirectly use
            // the TypeInfo instead.
            return type.GetTypeInfo().GetCustomAttributes(typeof(MixedRealityControllerAttribute), true).FirstOrDefault() as MixedRealityControllerAttribute;
#else
            return type.GetCustomAttributes(typeof(MixedRealityControllerAttribute), true).FirstOrDefault() as MixedRealityControllerAttribute;
#endif
        }
    }
}
