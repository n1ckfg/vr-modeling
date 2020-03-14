﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace libigl
{
    /// <summary>
    /// Contains all runtime C++ function declarations.
    /// These functions can only be called in play mode, as the dll is unloaded otherwise for easier rebuilds.
    /// C# to C++ Communication with marshalling attributes
    /// </summary>
    public static class Native
    {
        private const string dllName = "libigl-interface";
        private static bool initialized = false;

        public static VertexAttributeDescriptor[] VertexBufferLayout = new[]
        {
            //Note! Specify that the position is the only attribute in the first stream, else values will be interleaved
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, 1)
            // new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4, 1),
            // new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, 1),
            // new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 2, 1)
        };
        
        public static void Initialize()
        {
            if (!initialized)
                Initialize(Application.dataPath + "/Models/", NativeCallbacks.DebugLog);
            initialized = true;
        }

        /// <summary>
        /// Reset plugin, allows initialization again.
        /// Call this when plugin is unloaded, end of play mode when using DllManipulator
        /// </summary>
        public static void Dispose()
        {
            initialized = false;
        }

        [DllImport(dllName, ExactSpelling = true, CharSet = CharSet.Ansi)]
        private static extern void Initialize([In] string modelRootp,
            [In] NativeCallbacks.StringCallback debugCallback);

        [DllImport(dllName, ExactSpelling = true)]
        public static extern unsafe void TranslateMesh([In] float* VPtr, int VSize,
            [In /*, MarshalAs(UnmanagedType.Struct)*/]
            Vector3 directionArr);
    }
}
