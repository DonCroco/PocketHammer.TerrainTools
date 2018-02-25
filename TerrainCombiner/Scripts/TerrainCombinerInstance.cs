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
        public float heightSize = 1.0f;
    }

}