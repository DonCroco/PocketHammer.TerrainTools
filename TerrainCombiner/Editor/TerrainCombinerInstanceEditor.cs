using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PocketHammer
{
    [CustomEditor(typeof(TerrainCombinerInstance))]
    public class TerrainCombinerInstanceEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            TerrainCombinerInstance instance = (TerrainCombinerInstance)target;

            TerrainCombiner combiner = GetCombiner();
            if (combiner == null)
            {
                GUILayout.Label("Gameobject needs to be located as child of TerrainCombiner");
                return;
            }

            DrawDefaultInspector();
        }

        void OnSceneGUI()
        {
            if (GetCombiner() == null)
                return;

            TerrainCombinerInstance instance = (TerrainCombinerInstance)target;

            if(instance.transform.hasChanged)
            {
                Vector3 pos = instance.transform.localPosition;

                instance.position.x = pos.z * 0.01f;
                instance.position.y = pos.x * 0.01f;

                TCWorker.RequestUpdate(GetCombiner());
            }
        }

        TerrainCombiner GetCombiner()
        {
            TerrainCombinerInstance instance = (TerrainCombinerInstance)target;
            return instance.transform.parent != null ? instance.transform.parent.gameObject.GetComponent<TerrainCombiner>() : null;
        }

    }
}
