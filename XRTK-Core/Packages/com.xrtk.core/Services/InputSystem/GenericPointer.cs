﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using UnityEngine;
using XRTK.Definitions.Physics;
using XRTK.Interfaces.Providers.Controllers;
using XRTK.Interfaces.InputSystem;
using XRTK.Interfaces.InputSystem.Handlers;
using XRTK.Interfaces.Physics;
using XRTK.Interfaces.TeleportSystem;

namespace XRTK.Services.InputSystem.Pointers
{
    /// <summary>
    /// Base Class for pointers that don't inherit from MonoBehaviour.
    /// </summary>
    public class GenericPointer : IMixedRealityPointer
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="pointerName"></param>
        /// <param name="inputSourceParent"></param>
        public GenericPointer(string pointerName, IMixedRealityInputSource inputSourceParent)
        {
            PointerId = MixedRealityToolkit.InputSystem.FocusProvider.GenerateNewPointerId();
            PointerName = pointerName;
            this.inputSourceParent = inputSourceParent;
        }

        /// <inheritdoc />
        public virtual IMixedRealityController Controller
        {
            get => controller;
            set
            {
                controller = value;
                inputSourceParent = controller.InputSource;
            }
        }

        private IMixedRealityController controller;

        /// <inheritdoc />
        public uint PointerId { get; }

        /// <inheritdoc />
        public string PointerName { get; set; }

        /// <inheritdoc />
        public virtual IMixedRealityInputSource InputSourceParent
        {
            get => inputSourceParent;
            protected set => inputSourceParent = value;
        }

        private IMixedRealityInputSource inputSourceParent;

        /// <inheritdoc />
        public IMixedRealityCursor BaseCursor { get; set; }

        /// <inheritdoc />
        public ICursorModifier CursorModifier { get; set; }

        /// <inheritdoc />
        public IMixedRealityTeleportHotSpot TeleportHotSpot { get; set; }

        /// <inheritdoc />
        public bool IsInteractionEnabled { get; set; }

        /// <inheritdoc />
        public bool IsFocusLocked { get; set; }

        /// <inheritdoc />
        public bool SyncPointerTargetPosition { get; set; }

        /// <inheritdoc />
        public virtual float PointerExtent { get; set; } = 10f;

        /// <inheritdoc />
        public virtual float DefaultPointerExtent { get; } = 10f;

        /// <inheritdoc />
        public RayStep[] Rays { get; protected set; } = { new RayStep(Vector3.zero, Vector3.forward) };

        /// <inheritdoc />
        public LayerMask[] PrioritizedLayerMasksOverride { get; set; }

        /// <inheritdoc />
        public IMixedRealityFocusHandler FocusHandler { get; set; }

        /// <inheritdoc />
        public IMixedRealityInputHandler InputHandler { get; set; }
        
        /// <inheritdoc />
        public IPointerResult Result { get; set; }

        /// <inheritdoc />
        public IBaseRayStabilizer RayStabilizer { get; set; }

        /// <inheritdoc />
        public RaycastMode RaycastMode { get; set; } = RaycastMode.Simple;

        /// <inheritdoc />
        public float SphereCastRadius { get; set; }

        /// <inheritdoc />
        public float PointerOrientation { get; } = 0f;

        /// <inheritdoc />
        public virtual void OnPreRaycast()
        {
            if (TryGetPointingRay(out var pointingRay))
            {
                Rays[0].CopyRay(pointingRay, PointerExtent);
            }

            if (RayStabilizer != null)
            {
                RayStabilizer.UpdateStability(Rays[0].Origin, Rays[0].Direction);
                Rays[0].CopyRay(RayStabilizer.StableRay, PointerExtent);
            }
        }

        /// <inheritdoc />
        public virtual void OnPostRaycast() { }

        /// <inheritdoc />
        public virtual bool TryGetPointerPosition(out Vector3 position)
        {
            position = Vector3.zero;
            return false;
        }

        /// <inheritdoc />
        public virtual bool TryGetPointingRay(out Ray pointingRay)
        {
            pointingRay = default;
            return false;
        }

        /// <inheritdoc />
        public virtual bool TryGetPointerRotation(out Quaternion rotation)
        {
            rotation = Quaternion.identity;
            return false;
        }

        #region IEquality Implementation

        public static bool Equals(IMixedRealityPointer left, IMixedRealityPointer right)
        {
            return left.Equals(right);
        }

        bool IEqualityComparer.Equals(object left, object right)
        {
            return left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) { return false; }
            if (ReferenceEquals(this, obj)) { return true; }
            if (obj.GetType() != GetType()) { return false; }

            return Equals((IMixedRealityPointer)obj);
        }

        private bool Equals(IMixedRealityPointer other)
        {
            return other != null && PointerId == other.PointerId && string.Equals(PointerName, other.PointerName);
        }

        int IEqualityComparer.GetHashCode(object obj)
        {
            return obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 0;
                hashCode = (hashCode * 397) ^ (int)PointerId;
                hashCode = (hashCode * 397) ^ (PointerName != null ? PointerName.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion IEquality Implementation
    }
}
