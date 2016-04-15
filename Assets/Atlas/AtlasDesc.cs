//using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AtlasDesc : MonoBehaviour
{
	public CardAtlas Atlas;
	public Texture2D Target;
	public int Padding;
	
	public string Validate()
	{
		if (Atlas == null)
		{
			return "Atlas property is not set.";
		}
		if (List.Length == 0)
		{
			return "List of shapes is empty.";
		}
		return null;
	}
	
	[System.Serializable]
	public class Item
	{
		// Name for this image to be listed in atlas
		public string AtlasShape;
		// The source-bitmap to be embedded in the atlas texture
		public Texture2D Image;
		// Border is the number of extra pixels around the edge of the image
		// the UV coordinates will be adjusted inward to avoid the border zone
		// real color data is useful to prevent mip-map bleeding
		public float BorderPixels;
	}
	
	public Item[] List;
}

