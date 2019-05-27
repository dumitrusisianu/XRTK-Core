﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace XRTK.Definitions.SpatialAwarenessSystem
{
    /// <summary>
    /// A Spatial Mesh Object is the Spatial Awareness System's representation of a spatial object with mesh information.
    /// </summary>
    public struct SpatialMeshObject
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="gameObject"></param>
        public SpatialMeshObject(int id, GameObject gameObject) : this()
        {
            Id = id;
            Debug.Assert(gameObject != null);
            GameObject = gameObject;
            Anchor = new GameObject("Anchor");
            Anchor.transform.SetParent(gameObject.transform);
        }

        /// <summary>
        /// The id of the spatial mesh object.
        /// </summary>
        public int Id { get; internal set; }

        private GameObject gameObject;

        /// <summary>
        /// The <see cref="UnityEngine.GameObject"/> reference of the Spatial Mesh Object.
        /// </summary>
        public GameObject GameObject
        {
            get => gameObject;
            internal set
            {
                gameObject = value;

                Renderer = gameObject.GetComponent<MeshRenderer>();
                Filter = gameObject.GetComponent<MeshFilter>();
                Collider = gameObject.GetComponent<MeshCollider>();
            }
        }

        /// <summary>
        /// The spatial anchor of the <see cref="UnityEngine.GameObject"/> reference for the Spatial Mesh Object.
        /// </summary>
        /// <remarks>
        /// The anchor is used to correctly offset the spatial mesh if the user ever teleports or moves their playspace.
        /// </remarks>
        public GameObject Anchor { get; internal set; }

        /// <summary>
        /// The <see cref="UnityEngine.Mesh"/> reference for the Spatial Mesh Object.
        /// </summary>
        public Mesh Mesh
        {
            get => Filter.sharedMesh;
            internal set
            {
                // Reset the surface mesh collider to fit the updated mesh.
                // Unity tribal knowledge indicates that to change the mesh assigned to a
                // mesh collider and mesh filter, the mesh must first be set to null.  Presumably there
                // is a side effect in the setter when setting the shared mesh to null.
                Filter.sharedMesh = null;
                Filter.sharedMesh = value;
                Collider.sharedMesh = null;
                Collider.sharedMesh = Filter.sharedMesh;
            }
        }

        /// <summary>
        /// The <see cref="MeshRenderer"/> reference for the Spatial Mesh Object.
        /// </summary>
        public MeshRenderer Renderer { get; private set; }

        /// <summary>
        /// The <see cref="MeshFilter"/> reference for the Spatial Mesh Object.
        /// </summary>
        public MeshFilter Filter { get; private set; }

        /// <summary>
        /// The <see cref="MeshCollider"/> reference for the Spatial Mesh Object.
        /// </summary>
        public MeshCollider Collider { get; private set; }
    }
}