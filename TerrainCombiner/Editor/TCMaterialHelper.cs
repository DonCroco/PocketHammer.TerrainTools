#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PocketHammer
{

	public class TCMaterialHelper {

		public static void UpdateCombinerCache(TerrainCombiner combiner)
		{
			TerrainData terrainData = combiner.GetComponent<Terrain>().terrainData;

			// Update target materials from sources
			List<SplatPrototype> targetSplatList = new List<SplatPrototype>();
			for(int i=0;i<combiner.Instances.Length; i++) {
				TerrainCombinerInstance sourceData = combiner.Instances[i];

				if(sourceData.source == null) {
					continue;
				}

				Terrain sourceTerrain = sourceData.source.GetComponent<Terrain>();
				foreach(SplatPrototype splat in sourceTerrain.terrainData.splatPrototypes) {

                    if (splat.texture == null)
                        continue;

                    // If splay use alpha material it should not be added to target
					if(sourceData.source.alphaMaterial != null && splat.texture.ToString() == sourceData.source.alphaMaterial.ToString()) {
						continue;
					}

					int index = FindMaterialIndex(targetSplatList, splat.texture);
					if(index == -1) {
						targetSplatList.Add(splat);
					}
				}
			}
			terrainData.splatPrototypes = targetSplatList.ToArray();


			// TODO: release ??
			combiner.MaterialCache.RenderTextures.Clear();

			int size = terrainData.alphamapResolution;
			int targetLayerCount = terrainData.alphamapLayers;
			for(int i=0;i<targetLayerCount;i++) {
				RenderTexture renderTexture = TCGraphicsHelper.CreateRenderTarget(size);
				combiner.MaterialCache.RenderTextures.Add(renderTexture);
			}

			combiner.MaterialCache.Texture = TCGraphicsHelper.CreateTexture(size);
			combiner.MaterialCache.ResultData = new float[size,size,terrainData.alphamapLayers];
		}

		public static void UpdateSourceCache(TerrainCombinerSource source)
		{
			TerrainData sourceTerrainData = source.GetComponent<Terrain>().terrainData;

			// TODO: reuse/scale textures ?
			// TODO: release?

			source.CachedMaterialTextures.Clear();

			int size = sourceTerrainData.alphamapResolution;
			float[,,] alphaMaps = sourceTerrainData.GetAlphamaps(0,0,size,size);
			for(int layer=0;layer<sourceTerrainData.alphamapLayers;layer++) {
				Texture2D texture = TCGraphicsHelper.CreateTexture(size);
				TCGraphicsHelper.LoadTextureData(alphaMaps, layer, ref texture);
				source.CachedMaterialTextures.Add(texture);
			}
		}

		// TODO: find better place
		private static Texture2D CreateTexture(Color color) {
			var texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);

			// set the pixel values
			texture.SetPixel(0, 0, color);
			texture.SetPixel(1, 0, color);
			texture.SetPixel(0, 1, color);
			texture.SetPixel(1, 1, color);

			// Apply all SetPixel calls
			texture.Apply();

			return texture;
		}


		public static void StartCombine(TerrainCombiner terrainCombiner)
		{
			Terrain targetTerrain = terrainCombiner.GetComponent<Terrain>();
			TerrainData targetTerrainData = targetTerrain.terrainData;

			RenderTexture prevRenderTarget = RenderTexture.active;

			Texture2D blackTexture = CreateTexture(Color.black); // TODO: cache?


			// Iterate through alphamaps 
			for(int nLayer=0;nLayer<targetTerrainData.alphamapLayers;nLayer++) {
				Graphics.SetRenderTarget(terrainCombiner.MaterialCache.RenderTextures[nLayer]);
				GL.Clear(false,true,Color.black);
			
				SplatPrototype targetSplat = targetTerrainData.splatPrototypes[nLayer];

				// Apply all sources
				for(int i=0;i<terrainCombiner.Instances.Length; i++) {
					TerrainCombinerInstance sourceData = terrainCombiner.Instances[i];

					if(sourceData.source == null) {
						continue;
					}

					Terrain sourceTerrain = sourceData.source.GetComponent<Terrain>();
					TerrainData sourceTerrainData = sourceTerrain.terrainData;

						//					// TODO: map splat prototypes to find correct id
						//					int nTexture = nLayer/4;
						//					int nColor = nLayer % 4;
						//
						//					Texture2D[] textures = sourceTerrainData.alphamapTextures;
						//
						//					if(textures != null && textures.Length > nTexture) {
						//
						//						Vector2 position = sourceData.position;
						//						Texture2D sourceTexture = textures[nTexture];
						//
						//						Material material = new Material(Shader.Find("PockerHammer/TCMaterialShader"));
						//						Matrix4x4 m = Matrix4x4.zero;
						//
						//						switch(nColor) {
						//						case 0:
						//							m.m00 = 1.0f;
						//							break;
						//						case 1:
						//							m.m11 = 1.0f;
						//							break;
						//						case 2:
						//							m.m22 = 1.0f;
						//							break;
						//						case 3:
						//							m.m33 = 1.0f;
						//							break;
						//						}
						//
						//						material.SetMatrix ("_ColorMapping", m);
						//
						//						TCGraphicsHelper.DrawTexture(sourceTexture, material, position, sourceData.rotation, sourceData.scale);
						//
						//					}

						//				if(nSourceLayer < sourceData.source.CachedMaterialTextures.Count) {


					int nSourceLayer = FindMaterialIndex(sourceTerrainData, targetSplat.texture);
					Texture2D sourceTexture = nSourceLayer != -1 ?sourceData.source.CachedMaterialTextures[nSourceLayer] : blackTexture;

					Texture2D alphaTexture = blackTexture;
					if(sourceData.source.alphaMaterial != null) {
						int nSourceAlphaLayer = FindMaterialIndex(sourceTerrainData, sourceData.source.alphaMaterial );
						alphaTexture = sourceData.source.CachedMaterialTextures[nSourceAlphaLayer];
					}

					Material material = new Material(Shader.Find("PockerHammer/TCMaterialShader"));
					material.SetTexture("_Texture2",alphaTexture);

					Vector2 scale = TerrainCombiner.CalcChildTerrainPlaneScale(targetTerrain.terrainData.size, sourceTerrain.terrainData.size, sourceData.size);

					TCGraphicsHelper.DrawTexture(sourceTexture, material, sourceData.position, sourceData.rotation, scale);
				}
			}
			Graphics.SetRenderTarget(prevRenderTarget);
		}


		public static void SampleTexture(TerrainCombiner terrainCombiner) {
			Terrain targetTerrain = terrainCombiner.GetComponent<Terrain>();
			TerrainData targetTerrainData = targetTerrain.terrainData;

			RenderTexture prevRenderTarget = RenderTexture.active;
		
			for(int nLayer=0;nLayer<targetTerrainData.alphamapLayers;nLayer++) {

				Graphics.SetRenderTarget(terrainCombiner.MaterialCache.RenderTextures[nLayer]);

				// Sample render target
				int targetSize = targetTerrainData.alphamapResolution;
				TCGraphicsHelper.ReadRenderTarget(targetSize, ref terrainCombiner.MaterialCache.Texture);
				TCGraphicsHelper.ReadDataFromTexture(terrainCombiner.MaterialCache.Texture, ref terrainCombiner.MaterialCache.ResultData, nLayer);
			}

			Graphics.SetRenderTarget(prevRenderTarget);	
		}


		public static void SampleTexture(TerrainCombiner terrainCombiner, int nLayer) {
			Terrain targetTerrain = terrainCombiner.GetComponent<Terrain>();
			TerrainData targetTerrainData = targetTerrain.terrainData;

			RenderTexture prevRenderTarget = RenderTexture.active;

			Graphics.SetRenderTarget(terrainCombiner.MaterialCache.RenderTextures[nLayer]);

			// Sample render target
			int targetSize = targetTerrainData.alphamapResolution;
			TCGraphicsHelper.ReadRenderTarget(targetSize, ref terrainCombiner.MaterialCache.Texture);
			TCGraphicsHelper.ReadDataFromTexture(terrainCombiner.MaterialCache.Texture, ref terrainCombiner.MaterialCache.ResultData, nLayer);

			Graphics.SetRenderTarget(prevRenderTarget);	
		}
	

		public static void ApplyData(TerrainCombiner terrainCombiner) {
			Terrain targetTerrain = terrainCombiner.GetComponent<Terrain>();
			TerrainData targetTerrainData = targetTerrain.terrainData;
			targetTerrainData.SetAlphamaps(0,0,terrainCombiner.MaterialCache.ResultData);
		}
			
		private static int FindMaterialIndex(TerrainData terrainData, Texture2D tex)
		{
			List<SplatPrototype> list = new List<SplatPrototype>(terrainData.splatPrototypes);
			return FindMaterialIndex(list, tex);
		}

		private static int FindMaterialIndex(List<SplatPrototype> list, Texture2D tex)
		{
			if (tex == null)
			{
				return -1;
			}

			for (int i = 0; i < list.Count; i++)
			{
				SplatPrototype m = list[i];
								if (Compare(m, tex))
				{
					return i;
				}
			}
			return -1;
		}

		public static bool Compare(SplatPrototype matA, Texture tex)
		{
			if (matA.texture == null || tex == null)
			{
				return false;
			}

			string sA = matA.texture.ToString();
			string sB = tex.ToString();
			if (sA.Equals(sB))
			{
				return true;
			}

			return false;
		}
	}

}

#endif