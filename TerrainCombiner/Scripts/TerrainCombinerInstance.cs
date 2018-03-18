using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PocketHammer
{
    public class TerrainCombinerInstance : MonoBehaviour
    {
        public TerrainCombinerSource source = null;
        public Vector2 position = Vector2.zero;
        public float rotation = 0;
        public Vector2 size = Vector2.one;

        public Vector3 WorldSize
        {
            get { return source == null ? Vector3.zero : Vector3.Scale(source.Terrain.terrainData.size, transform.localScale); }
        }

        public float WorldGroundHeight
        {
            get { return source == null ? 0 : source.GroundLevelFraction * WorldSize.y; }
        }
    }
}