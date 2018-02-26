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

            HandleTransformChange();

            // Draw bounds
            Handles.color = Color.yellow;
            Vector3 instanceSize = Vector3.Scale(sourceTerrain.terrainData.size, instance.transform.localScale);
            
            Handles.matrix = Matrix4x4.TRS(instance.transform.position, instance.transform.rotation, instanceSize);
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
            if (instance.transform.position != lastPosition)
            {
                Vector3 instanceSize = Vector3.Scale(sourceTerrain.terrainData.size, instance.transform.localScale);

                instance.position.x = instance.transform.localPosition.z / combinerTerrain.terrainData.size.z;
//                instance.position.y = (combinerTerrain.terrainData.size.x - instance.transform.localPosition.x - instanceSize.x) / combinerTerrain.terrainData.size.x;
                instance.position.y = instance.transform.localPosition.x  / combinerTerrain.terrainData.size.x;

                triggerRebuild = true;

                lastPosition = instance.transform.position;
            }

            if(instance.transform.rotation.eulerAngles.y != lastRotation)
            {
                instance.rotation = instance.transform.rotation.eulerAngles.y;
                triggerRebuild = true;

                lastRotation = instance.transform.rotation.eulerAngles.y;
            }

            if(triggerRebuild)
            {
                combiner.CacheDirty = true;
                TCWorker.RequestUpdate(combiner);
            }
        }



    }
}
