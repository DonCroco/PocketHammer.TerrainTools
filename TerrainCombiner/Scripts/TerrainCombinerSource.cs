#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace PocketHammer
{

	[RequireComponent (typeof (Terrain))]
	public class TerrainCombinerSource : MonoBehaviour {

		public float GroundLevelFraction = 0.0f;
		public Texture2D alphaMaterial;

		// Cache
		public Texture2D CachedHeightmapTexture;
		public List<Texture2D> CachedMaterialTextures = new List<Texture2D>();
		public bool CacheDirty = true;

		public Terrain Terrain
		{
			get { return GetComponent<Terrain>(); }
		}

		public Vector3 WorldSize
		{
			get { return Terrain.terrainData.size; }
		}
	}

}

#endif