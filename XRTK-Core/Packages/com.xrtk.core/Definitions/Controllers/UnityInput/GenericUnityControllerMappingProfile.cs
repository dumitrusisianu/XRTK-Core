﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Utilities;
using XRTK.Providers.Controllers;
using XRTK.Providers.Controllers.UnityInput;

namespace XRTK.Definitions.Controllers.UnityInput.Profiles
{
    [CreateAssetMenu(menuName = "Mixed Reality Toolkit/Input System/Controller Mappings/Generic Unity Controller Mapping Profile", fileName = "GenericUnityControllerMappingProfile")]
    public class GenericUnityControllerMappingProfile : BaseMixedRealityControllerMappingProfile
    {
        /// <inheritdoc />
        public override SupportedControllerType ControllerType => SupportedControllerType.GenericUnity;

        protected override void Awake()
        {
            if (!HasSetupDefaults)
            {
                ControllerMappings = new[]
                {
                    new MixedRealityControllerMapping("Generic Unity Controller", typeof(GenericJoystickController), Handedness.None, true),
                };
            }

            base.Awake();
        }
    }
}