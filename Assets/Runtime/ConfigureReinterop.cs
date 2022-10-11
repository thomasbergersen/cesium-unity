﻿using Reinterop;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CesiumForUnity
{
    [Reinterop]
    internal partial class ConfigureReinterop
    {
        // The output path for generated C++ files.
        // If this is relative, it is relative to the this file.
#if UNITY_EDITOR
        public const string CppOutputPath = "../native~/Runtime/generated-Editor";
#elif UNITY_ANDROID
        public const string CppOutputPath = "../native~/Runtime/generated-Android";
#elif UNITY_IOS
        public const string CppOutputPath = "../native~/Runtime/generated-iOS";
#elif UNITY_64
        public const string CppOutputPath = "../native~/Runtime/generated-Standalone";
#else
        public const string CppOutputPath = "../native~/Runtime/generated-Unknown";
#endif

        // The namespace with which to prefix all C# namespaces. For example, if this
        // property is set to "DotNet", then anything in the "System" namespace in C#
        // will be found in the "DotNet::System" namespace in C++.
        public const string BaseNamespace = "DotNet";

        // The name of the DLL or SO containing the C++ code.
        public const string NativeLibraryName = "CesiumForUnityNative-Runtime";

        // Comma-separated types to treat as non-blittable, even if their fields would
        // otherwise cause Reinterop to treat them as blittable.
        public const string NonBlittableTypes = "Unity.Collections.LowLevel.Unsafe.AtomicSafetyHandle,Unity.Collections.NativeArray,UnityEngine.MeshData,UnityEngine.MeshDataArray";

        public void ExposeToCPP()
        {
            Camera c = Camera.main;
            Transform t = c.transform;
            Vector3 u = t.up;
            Vector3 f = t.forward;

            t.position = new Vector3();
            Vector3 p = t.position;
            float x = p.x;
            float y = p.y;
            float z = p.z;
            c.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
            float fov = c.fieldOfView;
            int pixelHeight = c.pixelHeight;
            int pixelWidth = c.pixelWidth;
            float aspect = c.aspect;
            //IFormattable f = new Vector3();
            //IEquatable<Vector3> f2 = new Vector3();

            GameObject go = new GameObject();
            go.name = go.name;
            go = new GameObject("name");
            go.SetActive(go.activeSelf);
            Transform transform = go.transform;
            transform.parent = transform.parent;
            transform.position = transform.position;
            transform.rotation = transform.rotation;
            transform.localScale = transform.localScale;
            Matrix4x4 m = transform.localToWorldMatrix;

            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();

            go.transform.GetChild(go.transform.childCount - 1);
            go.transform.DetachChildren();
            go.hideFlags = HideFlags.DontSave;

            Texture2D texture2D = new Texture2D(256, 256, TextureFormat.RGBA32, false, false);
            texture2D.LoadRawTextureData(IntPtr.Zero, 0);
            NativeArray<byte> textureBytes = texture2D.GetRawTextureData<byte>();

            unsafe
            {
                NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(textureBytes);
            }

            int textureBytesLength = textureBytes.Length;
            texture2D.Apply(true, true);
            texture2D.wrapMode = TextureWrapMode.Clamp;
            texture2D.anisoLevel = 16;
            texture2D.filterMode = FilterMode.Trilinear;
            Texture texture = texture2D;

            Mesh mesh = new Mesh();
            Mesh[] meshes = new[] { mesh };
            mesh = meshes[0];
            int meshesLength = meshes.Length;
            mesh.SetVertices(new NativeArray<Vector3>());
            mesh.SetNormals(new NativeArray<Vector3>());
            mesh.SetUVs(0, new NativeArray<Vector2>());
            mesh.SetIndices(new NativeArray<int>(), MeshTopology.Triangles, 0, true, 0);
            mesh.RecalculateBounds();
            int instanceID = mesh.GetInstanceID();

            MeshCollider meshCollider = go.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;

            Debug.Log("Logging");

            MeshRenderer meshRenderer = new MeshRenderer();
            GameObject meshGameObject = meshRenderer.gameObject;
            meshRenderer.material = UnityEngine.Object.Instantiate(meshRenderer.material);
            meshRenderer.material.SetTexture("name", texture2D);
            meshRenderer.material.SetFloat("name", 1.0f);
            meshRenderer.material.SetVector("name", new Vector4());
            meshRenderer.sharedMaterial = meshRenderer.sharedMaterial;
            UnityEngine.Object.Destroy(meshGameObject);
            UnityEngine.Object.DestroyImmediate(meshGameObject);

            MeshFilter meshFilter = new MeshFilter();
            meshFilter.mesh = mesh;
            meshFilter.sharedMesh = mesh;

            Resources.Load<Material>("name");

            byte b;
            unsafe
            {
                string s = Encoding.UTF8.GetString(&b, 0);
            }

            NativeArray<Vector3> nav = new NativeArray<Vector3>(1, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            NativeArray<Vector2> nav2 = new NativeArray<Vector2>(1, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            NativeArray<int> nai = new NativeArray<int>(1, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            unsafe
            {
                NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(nav);
                NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(nav2);
                NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(nai);
            }

            nav.Dispose();
            nav2.Dispose();
            nai.Dispose();

            string temporaryCachePath = Application.temporaryCachePath;
            bool isEditor = Application.isEditor;

            Marshal.FreeCoTaskMem(Marshal.StringToCoTaskMemUTF8("hi"));

            UnityWebRequest request = UnityWebRequest.Get("url");
            bool isDone = request.isDone;
            string e = request.error;
            string method = request.method;
            string url = request.url;
            request.downloadHandler = new NativeDownloadHandler();
            request.SetRequestHeader("name", "value");
            request.GetResponseHeader("name");
            request.downloadHandler.Dispose();
            long responseCode = request.responseCode;
            UnityWebRequestAsyncOperation op = request.SendWebRequest();
            //Action<AsyncOperation> foo = (ao) => { };
            //var asdfx = foo + foo;
            op.completed += o => { };

            Task.Run(() => { });

            Cesium3DTileset tileset = new Cesium3DTileset();
            tileset.tilesetSource = tileset.tilesetSource;
            tileset.url = tileset.url;
            tileset.ionAssetID = tileset.ionAssetID;
            tileset.ionAccessToken = tileset.ionAccessToken;
            tileset.logSelectionStats = tileset.logSelectionStats;
            tileset.opaqueMaterial = tileset.opaqueMaterial;
            tileset.enabled = tileset.enabled;
            tileset.maximumScreenSpaceError = tileset.maximumScreenSpaceError;
            tileset.preloadAncestors = tileset.preloadAncestors;
            tileset.preloadSiblings = tileset.preloadSiblings;
            tileset.forbidHoles = tileset.forbidHoles;
            tileset.maximumSimultaneousTileLoads = tileset.maximumSimultaneousTileLoads;
            tileset.maximumCachedBytes = tileset.maximumCachedBytes;
            tileset.loadingDescendantLimit = tileset.loadingDescendantLimit;
            tileset.enableFrustumCulling = tileset.enableFrustumCulling;
            tileset.enableFogCulling = tileset.enableFogCulling;
            tileset.enforceCulledScreenSpaceError = tileset.enforceCulledScreenSpaceError;
            tileset.culledScreenSpaceError = tileset.culledScreenSpaceError;
            tileset.useLodTransitions = tileset.useLodTransitions;
            tileset.lodTransitionLength = tileset.lodTransitionLength;
            tileset.generateSmoothNormals = tileset.generateSmoothNormals;
            tileset.createPhysicsMeshes = tileset.createPhysicsMeshes;
            tileset.suspendUpdate = tileset.suspendUpdate;
            tileset.previousSuspendUpdate = tileset.previousSuspendUpdate;
            tileset.updateInEditor = tileset.updateInEditor;

            Cesium3DTileset tilesetFromGameObject = go.GetComponent<Cesium3DTileset>();
            MeshRenderer meshRendererFromGameObject = go.GetComponent<MeshRenderer>();
            CesiumIonRasterOverlay ionOverlay = go.GetComponent<CesiumIonRasterOverlay>();
            ionOverlay.ionAssetID = ionOverlay.ionAssetID;
            ionOverlay.ionAccessToken = ionOverlay.ionAccessToken;
            CesiumRasterOverlay baseOverlay = ionOverlay;
            CesiumRasterOverlay overlay = go.GetComponent<CesiumRasterOverlay>();
            baseOverlay.AddToTileset();
            baseOverlay.RemoveFromTileset();

            List<CesiumRasterOverlay> overlays = new List<CesiumRasterOverlay>();
            go.GetComponents<CesiumRasterOverlay>(overlays);
            for (int i = 0; i < overlays.Count; ++i)
            {
                CesiumRasterOverlay anOverlay = overlays[i];
            }

            CesiumRasterOverlay[] overlaysArray = go.GetComponents<CesiumRasterOverlay>();
            int len = overlaysArray.Length;
            CesiumRasterOverlay first = overlaysArray[0];

            MonoBehaviour mb = tileset;
            mb.StartCoroutine(new NativeCoroutine(endIteration => endIteration).GetEnumerator());

            CesiumGeoreference georeference = go.AddComponent<CesiumGeoreference>();
            georeference = go.GetComponent<CesiumGeoreference>();
            georeference.longitude = georeference.longitude;
            georeference.latitude = georeference.latitude;
            georeference.height = georeference.height;

            CesiumGeoreference inParent = go.GetComponentInParent<CesiumGeoreference>();
            inParent.UpdateOrigin();
            inParent.changed += () => { };

            float time = Time.deltaTime;

            GameObject[] gos = GameObject.FindGameObjectsWithTag("test");
            for (int i = 0; i < gos.Length; ++i)
            {
                GameObject goFromArray = gos[i];
                gos[i] = goFromArray;
            }

            CesiumGeoreference[] georeferences = UnityEngine.Object.FindObjectsOfType<CesiumGeoreference>();

            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[meshDataArray.Length - 1];

            VertexAttributeDescriptor[] descriptorsArray = new VertexAttributeDescriptor[1];
            VertexAttributeDescriptor descriptor0 = descriptorsArray[0];

            meshData.SetVertexBufferParams(1, descriptorsArray);
            meshData.SetIndexBufferParams(1, IndexFormat.UInt16);
            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0, new SubMeshDescriptor(0, 1, MeshTopology.Triangles));

            NativeArray<Vector3> positionNormal = meshData.GetVertexData<Vector3>(0);
            NativeArray<Vector2> texCoord = meshData.GetVertexData<Vector2>(0);
            NativeArray<ushort> indices = meshData.GetIndexData<ushort>();
            NativeArray<uint> indices32 = meshData.GetIndexData<uint>();

            int positionNormalLength = positionNormal.Length;
            int texCoordLength = texCoord.Length;
            int indicesLength = indices.Length;
            int indices32Length = indices32.Length;

            unsafe
            {
                NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(positionNormal);
                NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(texCoord);
                NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(indices);
                NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(indices32);
            }

            meshDataArray.Dispose();

            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, meshes, MeshUpdateFlags.Default);

            Physics.BakeMesh(mesh.GetInstanceID(), false);

#if UNITY_EDITOR
            SceneView sv = SceneView.lastActiveSceneView;
            Camera svc = sv.camera;

            bool isPlaying = EditorApplication.isPlaying;
            EditorApplication.update += () => {};
#endif
        }
    }
}
//