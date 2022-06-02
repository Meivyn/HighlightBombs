using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HighlightBombs
{
    //Original File modified for use with this mod

    //
    //  Outline.cs
    //  QuickOutline
    //
    //  Created by Chris Nolet on 3/30/18.
    //  Copyright © 2018 Chris Nolet. All rights reserved.
    //

    [DisallowMultipleComponent]

    public class Outline : MonoBehaviour
    {
        private static HashSet<Mesh> registeredMeshes = new HashSet<Mesh>();

        internal static Material outlineMaskMaterialSource;
        internal static Material outlineFillMaterialSource;

        public enum Mode
        {
            OutlineAll,
            OutlineVisible,
            OutlineHidden,
            OutlineAndSilhouette,
            SilhouetteOnly
        }

        public Mode OutlineMode
        {
            get { return outlineMode; }
            set
            {
                outlineMode = value;
                needsUpdate = true;
            }
        }

        public Color OutlineColor
        {
            get { return outlineColor; }
            set
            {
                outlineColor = value;
                needsUpdate = true;
            }
        }

        public float OutlineWidth
        {
            get { return outlineWidth; }
            set
            {
                outlineWidth = value;
                needsUpdate = true;
            }
        }

        [Serializable]
        private class ListVector3
        {
            public List<Vector3> data;
        }

        [SerializeField]
        private Mode outlineMode;

        [SerializeField]
        private Color outlineColor = Color.white;

        [SerializeField, Range(0f, 10f)]
        private float outlineWidth = 2f;

        [Header("Optional")]

        [SerializeField, Tooltip("Precompute enabled: Per-vertex calculations are performed in the editor and serialized with the object. "
        + "Precompute disabled: Per-vertex calculations are performed at runtime in Awake(). This may cause a pause for large meshes.")]
        private bool precomputeOutline;

        [SerializeField, HideInInspector]
        private List<Mesh> bakeKeys = new List<Mesh>();

        [SerializeField, HideInInspector]
        private List<ListVector3> bakeValues = new List<ListVector3>();

        private Renderer[] renderers;


        private Material outlineMaskMaterial;
        private Material outlineFillMaterial;

        private bool needsUpdate;

        void Awake()
        {

            // Cache renderers
            ReInitRenderers();

            // Instantiate outline materials
            outlineMaskMaterial = Instantiate(outlineMaskMaterialSource);
            outlineFillMaterial = Instantiate(outlineFillMaterialSource);

            outlineMaskMaterial.name = "OutlineMask (Instance)";
            outlineFillMaterial.name = "OutlineFill (Instance)";

            // Retrieve or generate smooth normals
            LoadSmoothNormals();

            // Apply material properties immediately
            needsUpdate = true;
        }

        void OnEnable()
        {
            foreach (var renderer in renderers)
            {

                // Append outline shaders
                var materials = renderer.sharedMaterials.ToList();

                materials.Add(outlineMaskMaterial);
                materials.Add(outlineFillMaterial);

                renderer.materials = materials.ToArray();
            }
        }

        void OnValidate()
        {

            // Update material properties
            needsUpdate = true;

            // Clear cache when baking is disabled or corrupted
            if (!precomputeOutline && bakeKeys.Count != 0 || bakeKeys.Count != bakeValues.Count)
            {
                bakeKeys.Clear();
                bakeValues.Clear();
            }

            // Generate smooth normals when baking is enabled
            if (precomputeOutline && bakeKeys.Count == 0)
            {
                Bake();
            }
        }

        public void CheckRenderersValidity()
        {
            //   if(renderers == null || renderers.Count() < 1 || !renderers[0].gameObject.transform.IsDescendantOf(gameObject.transform))
            {

                ReInitRenderers();
            }
        }
        void ReInitRenderers()
        {
            //Remove Outline Materials if existing from old renderers
            OnDisable();
            //Re-grab renderers from gameobject
            renderers = GetComponentsInChildren<MeshRenderer>();
            if (renderers.Length > 1)
                renderers = renderers.Where(x => x.gameObject.name != "NoteArrowGlow" && x.gameObject.name != "NoteCircleGlow").ToArray();
        }
        void Update()
        {
            if (needsUpdate)
            {
                needsUpdate = false;

                UpdateMaterialProperties();
            }
        }

        void OnDisable()
        {
            if (renderers == null) return;
            foreach (var renderer in renderers)
            {

                // Remove outline shaders
                var materials = renderer.sharedMaterials.ToList();

                materials.Remove(outlineMaskMaterial);
                materials.Remove(outlineFillMaterial);

                renderer.materials = materials.ToArray();
            }
        }

        void OnDestroy()
        {

            // Destroy material instances
            Destroy(outlineMaskMaterial);
            Destroy(outlineFillMaterial);
        }

        void Bake()
        {

            // Generate smooth normals for each mesh
            var bakedMeshes = new HashSet<Mesh>();

            foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
            {

                // Skip duplicates
                if (!bakedMeshes.Add(meshFilter.sharedMesh))
                {
                    continue;
                }

                // Serialize smooth normals
                var smoothNormals = SmoothNormals(meshFilter.sharedMesh);

                bakeKeys.Add(meshFilter.sharedMesh);
                bakeValues.Add(new ListVector3() { data = smoothNormals });
            }
        }

        void LoadSmoothNormals()
        {
            return;
            // Retrieve or generate smooth normals
            foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
            {

                // Skip if smooth normals have already been adopted
                if (!registeredMeshes.Add(meshFilter.sharedMesh))
                {
                    continue;
                }

                // Retrieve or generate smooth normals
                var index = bakeKeys.IndexOf(meshFilter.sharedMesh);
                var smoothNormals = (index >= 0) ? bakeValues[index].data : SmoothNormals(meshFilter.sharedMesh);

                // Store smooth normals in UV3
                meshFilter.sharedMesh.SetUVs(3, smoothNormals);
            }

            // Clear UV3 on skinned mesh renderers
            foreach (var skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (registeredMeshes.Add(skinnedMeshRenderer.sharedMesh))
                {
                    skinnedMeshRenderer.sharedMesh.uv4 = new Vector2[skinnedMeshRenderer.sharedMesh.vertexCount];
                }
            }
        }

        List<Vector3> SmoothNormals(Mesh mesh)
        {
            return new List<Vector3>();
            // Group vertices by location
            var groups = mesh.vertices.Select((vertex, index) => new KeyValuePair<Vector3, int>(vertex, index)).GroupBy(pair => pair.Key);

            // Copy normals to a new list
            var smoothNormals = new List<Vector3>(mesh.normals);

            // Average normals for grouped vertices
            foreach (var group in groups)
            {

                // Skip single vertices
                if (group.Count() == 1)
                {
                    continue;
                }

                // Calculate the average normal
                var smoothNormal = Vector3.zero;

                foreach (var pair in group)
                {
                    smoothNormal += mesh.normals[pair.Value];
                }

                smoothNormal.Normalize();

                // Assign smooth normal to each vertex
                foreach (var pair in group)
                {
                    smoothNormals[pair.Value] = smoothNormal;
                }
            }

            return smoothNormals;
        }

        void UpdateMaterialProperties()
        {

            // Apply properties according to mode
            outlineFillMaterial.SetColor("_OutlineColor", outlineColor);

            switch (outlineMode)
            {
                case Mode.OutlineAll:
                    outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                    outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                    outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
                    break;

                case Mode.OutlineVisible:
                    outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                    outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                    outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
                    break;

                case Mode.OutlineHidden:
                    outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                    outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Greater);
                    outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
                    break;

                case Mode.OutlineAndSilhouette:
                    outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                    outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                    outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
                    break;

                case Mode.SilhouetteOnly:
                    outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                    outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Greater);
                    outlineFillMaterial.SetFloat("_OutlineWidth", 0f);
                    break;
            }
        }
    }
}
