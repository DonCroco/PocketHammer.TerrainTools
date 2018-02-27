using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PocketHammer
{
    [CustomEditor(typeof(TerrainCombinerInstance))]
    public class TerrainCombinerInstanceEditor : Editor
    {

        Vector3 lastPosition;
        float lastRotation;
        Vector3 lastScale;

        TerrainCombiner combiner;
        TerrainCombinerInstance instance;
        Terrain combinerTerrain;
        Terrain sourceTerrain;

        private void OnEnable()
        {
            instance = (TerrainCombinerInstance)target;

            combiner = instance.transform.parent != null ? instance.transform.parent.gameObject.GetComponent<TerrainCombiner>() : null;
            if(combiner != null)
                combinerTerrain = combiner.GetComponent<Terrain>();

            TerrainCombinerSource source = instance.source;
            if (source != null)
                sourceTerrain = source.GetComponent<Terrain>();

            lastPosition = instance.transform.position;
            lastRotation = instance.transform.rotation.eulerAngles.y;
            lastScale = instance.transform.localScale;
        }

        public override void OnInspectorGUI()
        {
            TerrainCombinerInstance instance = (TerrainCombinerInstance)target;

            if (combiner == null)
            {
                GUILayout.Label("Gameobject needs to be located as child of TerrainCombiner");
                return;
            }

            DrawDefaultInspector();

            HandleTransformChange();
        }

        void OnSceneGUI()
        {
            if (sourceTerrain == null)
                return;

            TerrainCombinerInstance instance = (TerrainCombinerInstance)target;

            HandleTransformChange();

            // Draw bounds
            Handles.color = Color.yellow;
            Vector3 worldSize = instance.GetWorldSize();
            Handles.matrix = Matrix4x4.TRS(instance.transform.position, instance.transform.rotation, worldSize);
            Handles.DrawWireCube(Vector3.zero, Vector3.one);
        }


        void HandleTransformChange()
        {
            // Contraint position combiner terrain height
            float y = combinerTerrain.transform.position.y + combinerTerrain.terrainData.size.y * combiner.groundLevelFraction;
            Vector3 instancePos = instance.transform.localPosition;
            instancePos.y = y;
            instance.transform.localPosition = instancePos;

            // Contraint rotation to y axis
            Quaternion rot = instance.transform.localRotation;
            rot = Quaternion.Euler(0, rot.eulerAngles.y, 0);
            instance.transform.localRotation = rot;

            bool triggerRebuild = false;
            triggerRebuild |= instance.transform.position != lastPosition;
            triggerRebuild |= instance.transform.rotation.eulerAngles.y != lastRotation;
            triggerRebuild |= instance.transform.localScale != lastScale;
            if (triggerRebuild)
            {
                combiner.CacheDirty = true;
                TCWorker.RequestUpdate(combiner);
            }

            lastPosition = instance.transform.position;
            lastRotation = instance.transform.rotation.eulerAngles.y;
            lastScale = instance.transform.localScale;

            instance.position.x = instance.transform.localPosition.z / combinerTerrain.terrainData.size.z;
            instance.position.y = instance.transform.localPosition.x  / combinerTerrain.terrainData.size.x;
            instance.rotation = instance.transform.rotation.eulerAngles.y;
            instance.size.x = instance.transform.localScale.z;
            instance.size.y = instance.transform.localScale.x;
        }
    }
}
