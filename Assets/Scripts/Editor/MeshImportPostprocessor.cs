﻿using UnityEditor;
using UnityEngine;

namespace libigl
{
    public class MeshImportPostprocessor : AssetPostprocessor
    {
        /// <summary>
        /// Called whenever a model is finished importing by the Unity importer.
        /// Custom importers will not call this
        /// </summary>
        /// <param name="g"></param>
        private void OnPostprocessModel(GameObject g)
        {
            //Get all meshes associated with this model/gameobject and reformat them
            var meshFilters = g.GetComponentsInChildren<MeshFilter>();
            if (meshFilters.Length == 0)
                return;

            for (int i = 0; i < meshFilters.Length; i++)
            {
                var mesh = meshFilters[i].sharedMesh;
                if (!mesh)
                    continue;

                // var oldLayout = mesh.GetVertexAttributes();
                mesh.SetVertexBufferParams(mesh.vertexCount, Native.VertexBufferLayout);
                Debug.Log("Converted VertexBufferLayout for: " + mesh.name);
            }
        }
    }
}