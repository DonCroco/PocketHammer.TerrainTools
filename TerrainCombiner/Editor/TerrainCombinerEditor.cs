﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace PocketHammer
{
	[CustomEditor(typeof(TerrainCombiner))]
	public class TerrainCombinerEditor : Editor {


		TerrainCombiner terrainCombiner;

		bool bTriggerUpdate = false;
//		bool bApply = false;
		int nApplyFrameCount;

		SerializedProperty groundLevelFraction;

		int selectedInstanceIndex = -1;     // TODO (mogensh) move to TerrainCombiner as non serialized property ? Or serialized`?

		GUISkin GUISkin;

		void OnEnable () {

			groundLevelFraction = serializedObject.FindProperty("GroundLevelFraction");

			terrainCombiner = (TerrainCombiner)target;

			terrainCombiner.CacheDirty = true;
			bTriggerUpdate = true;

			EditorApplication.update += Update;


			GUISkin = TCResourceHelper.LoadAsset<GUISkin>("GUI/TerrainCombinerGUISkin.guiskin");

		}


		public static string FindAssetFolder(string folderToStart, string desiredFolderName)
		{
			string[] folderEntries = Directory.GetDirectories(folderToStart);

			for (int n = 0, len = folderEntries.Length; n < len; ++n)
			{
				string folderName = GetLastFolder(folderEntries[n]);
				//Debug.Log("folderName: " + folderName);

				if (folderName == desiredFolderName)
				{
					return folderEntries[n];
				}
				else
				{
					string recursed = FindAssetFolder(folderEntries[n], desiredFolderName);
					string recursedFolderName = GetLastFolder(recursed);
					if (recursedFolderName == desiredFolderName)
					{
						return recursed;
					}
				}
			}
			return "";
		}

		static string GetLastFolder(string inFolder)
		{
			inFolder = inFolder.Replace('\\', '/');

			//Debug.Log("folder: " + inFolder);
			//string folderName = Path.GetDirectoryName(folderEntries[n]);

			int lastSlashIdx = inFolder.LastIndexOf('/');
			if (lastSlashIdx == -1)
			{
				return "";
			}
			return inFolder.Substring(lastSlashIdx+1, inFolder.Length-lastSlashIdx-1);
		}

		void OnDisable()
		{
			EditorApplication.update -= Update;
		}


		public override void OnInspectorGUI()
		{
			// DOnt bother with skin anyway as we then need to think about light/dark skin
//			GUI.skin = GUISkin;

			GUIContent content;

			serializedObject.Update();
			EditorGUI.BeginChangeCheck();

			EditorGUILayout.PropertyField(groundLevelFraction);

			GUILayout.Label("Sources:");

            SerializedProperty instances = serializedObject.FindProperty("Instances");
            if (instances != null)
            {

                for (int i = 0; i < instances.arraySize; i++)
                {

                    SerializedProperty instance = instances.GetArrayElementAtIndex(i);

                    SerializedProperty source = instance.FindPropertyRelative("source");
                    SerializedProperty displayName = instance.FindPropertyRelative("displayName");

                    // Find button name
                    string buttonName = source.objectReferenceValue != null ? source.objectReferenceValue.name : "";
                    buttonName = displayName.stringValue != "" ? displayName.stringValue + " (" + buttonName + ")" : buttonName;
                    buttonName = buttonName != "" ? buttonName : "New Layer";

                    SerializedProperty openInInspector = instance.FindPropertyRelative("openInInspector");

                    // Header
                    Color prevColor = GUI.backgroundColor;
                    GUI.backgroundColor = i == selectedInstanceIndex ? Color.green : GUI.backgroundColor;
                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button(buttonName))
                    {
                        selectedInstanceIndex = i;
                    }

                    string expandButton = "V";
                    if (GUILayout.Button(expandButton, GUILayout.Width(30)))
                    {
                        openInInspector.boolValue = !openInInspector.boolValue;
                    }

                    GUILayout.EndHorizontal();
                    GUI.backgroundColor = prevColor;


                    if (openInInspector.boolValue)
                    {

                        GUILayout.BeginHorizontal();

                        GUIStyle myStyle = new GUIStyle(GUI.skin.box);
                        RectOffset margin = myStyle.margin;
                        margin.left += 10;
                        myStyle.margin = margin;

                        GUILayout.Space(5);

                        GUILayout.BeginVertical(myStyle);
                        SerializedProperty position = instance.FindPropertyRelative("position");
                        SerializedProperty rotation = instance.FindPropertyRelative("rotation");
                        SerializedProperty size = instance.FindPropertyRelative("size");
                        SerializedProperty heightSize = instance.FindPropertyRelative("heightSize");

                        EditorGUILayout.PropertyField(displayName);
                        source.objectReferenceValue = EditorGUILayout.ObjectField("Source", source.objectReferenceValue, typeof(TerrainCombinerSource), true, null);


                        Vector2Field("Position", position);

                        FloatField("Rotation", rotation);

                        // TODO: combine scale in vector3 with same coords as parent ????

                        Vector2Field("Size", size);
                        FloatField("Height size", heightSize);


                        GUILayout.EndVertical();
                        GUILayout.EndHorizontal();
                    }
                }
            }

				
		
			if (EditorGUI.EndChangeCheck())
			{
				bTriggerUpdate = true;
			}


			serializedObject.ApplyModifiedProperties();


			GUILayout.Label("--------------------------");

			TerrainCombiner terrainCombiner = (TerrainCombiner)target;
//			Terrain terrain = terrainCombiner.GetComponent<Terrain>();
			// TODO: test for terrain

			float newGroundLevelFraction = EditorGUILayout.FloatField("HeightDataGround",terrainCombiner.GroundLevelFraction);
			if(newGroundLevelFraction != terrainCombiner.GroundLevelFraction) {
				terrainCombiner.GroundLevelFraction = Mathf.Clamp(newGroundLevelFraction,0,1);
				bTriggerUpdate = true;
			}

			GUILayout.Label("Sources:");

			for(int i=0;i<terrainCombiner.Instances.Length; i++) {
                TerrainCombinerInstance sourceData = terrainCombiner.Instances[i];

				GUILayout.Label("Source:");

				// TODO show error if gameObject not set OR no terrain on gameobject

				// TODO: only select object with terrain component
				GameObject currentGameObject = sourceData.source != null ? sourceData.source.gameObject : null;
				GameObject newGameObject = EditorGUILayout.ObjectField(currentGameObject,typeof(Object),true) as GameObject;
				TerrainCombinerSource newSource = newGameObject != null ? newGameObject.GetComponent<TerrainCombinerSource>() : null;
				if(newSource != sourceData.source) {
					sourceData.source = newSource;
					terrainCombiner.CacheDirty = true;
					bTriggerUpdate = true;
				}



				Vector2 newPosition = EditorGUILayout.Vector2Field("Position",sourceData.position);
				if(newPosition != sourceData.position) {
					sourceData.position = newPosition;
					bTriggerUpdate = true;
				}

				float newRotation = EditorGUILayout.FloatField("Rotation",sourceData.rotation);
				if(newRotation != sourceData.rotation) {
					sourceData.rotation = newRotation;
					bTriggerUpdate = true;
				}

				sourceData.size = EditorGUILayout.Vector2Field("Scale",sourceData.size);

				if(GUILayout.Button("World space scale")) {
					
				}

				terrainCombiner.Instances[i] = sourceData;
			}


			if (GUILayout.Button("Add source")) {

                GameObject go = new GameObject("NewInstance", typeof(TerrainCombinerInstance));
                go.transform.SetParent(terrainCombiner.transform, false);
			}

			if (GUILayout.Button("COMBINE")) {
				bTriggerUpdate = true;
			}

			if(bTriggerUpdate) {
				TCWorker.RequestUpdate(terrainCombiner);
				bTriggerUpdate = false;
			}
		}

        private void FloatField(string name, SerializedProperty property ) {
			GUILayout.BeginHorizontal();
			GUILayout.Label(name, GUILayout.Width( 75 ));
			GUILayout.FlexibleSpace();
			EditorGUILayout.PropertyField(property, GUIContent.none, GUILayout.MinWidth( 50 ) );
			GUILayout.EndHorizontal();
		}

		private void Vector2Field(string name, SerializedProperty property ) {
			GUILayout.BeginHorizontal();
			GUILayout.Label(name, GUILayout.Width( 75 ));
			GUILayout.FlexibleSpace();
			EditorGUILayout.PropertyField(property, GUIContent.none, GUILayout.MinWidth( 100 ) );
			GUILayout.EndHorizontal();
		}



		private void Update() {

//			// Apply first so it is a frame after combine start
//			if(bApply) {
//
//				nApplyFrameCount ++;
//				if(nApplyFrameCount > 5) {
//					TCHeightmapHelper.ApplyCombine(terrainCombiner); 
//					TCMaterialHelper.Apply(terrainCombiner);
//					bApply = false;
//				}
//			}
//
//
//			UpdateCaches(terrainCombiner);
//
//			if(bTriggerUpdate && !bApply) {
//				TCHeightmapHelper.StartCombine(terrainCombiner, terrainCombiner.GroundLevelFraction);
//				TCMaterialHelper.Combine(terrainCombiner);
//				bTriggerUpdate = false;
//				bApply = true;
//				nApplyFrameCount = 0;
//			}
		} 

	}
}