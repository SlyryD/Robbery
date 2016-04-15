using UnityEngine;
using System.Collections;

public class CardStock : MonoBehaviour
{
	// Number of vertices in arc to control round-ness of corners (0 = square cards)
	public int Smooth = 2;
	// Shape name in atlas for back of card
	public string Back;
	// Shape name for color of border and transparent background
	public string Paper;
	// This scale is multiplied by all sizes for easy tweaking size.
	public float Scale = 1.0f;
	// Size of card
	public Vector2 Size = new Vector2(1,1.5f);
	// Corner = part of Size reserved for corners
	public float CornerSize = 0.15f;
	// Border = part of Size reserved for symbols etc.
	public Vector2 Border = new Vector2(0.1f,0.1f);
	public Vector2 BackBorder = new Vector2(0.1f,0.1f);
	// Size of text in corner numbers
	public Vector2 TextSize = new Vector2(0.125f,0.125f);
	public Vector2 TextOffset = new Vector2(0.1f,0.1f);
	public Vector2 SymbolOffset = new Vector2(0.1f,0.2f);
	// Size of symbols
	public float SymbolSize = 0.125f; // normal pips
	public float BigSymbolSize = 0.4f; // large Ace center pip
	public float CornerSymbolSize = 0.06f; // corner
	// Basic material used for all generated meshes.
	public Material DefaultMaterial;
	
	// Optional flag to make corner text/symbols upright.
	public bool AlwaysUpright = false;
	
	// Is the back of the card a half or full image?
	public bool HalfBack = true;
	
	public bool TwoSided = true;
	
	const float MinSize = 0.01f;
	
	public void Validate()
	{
		if (Smooth < 0)
		{
			Smooth = 0;
		}
		if (Size.x < MinSize)
		{
			Size.x = MinSize;
		}
		if (Size.y < MinSize)
		{
			Size.y = MinSize;
		}
		float maxCorner = Mathf.Min(Size.x,Size.y)*0.25f; 
		if (CornerSize > maxCorner)
		{
			CornerSize = maxCorner;
		}
		if (CornerSize < 0.01f*Mathf.Max(Size.x,Size.y))
		{
			Smooth = 0;
		}
		if (string.IsNullOrEmpty(Paper))
		{
			Debug.LogError("Paper must be set to a valid shape from your atlas.");
		}
		if (TwoSided && string.IsNullOrEmpty(Back))
		{
			Debug.LogError("Back must be set to a valid shape from your atlas.");
		}
	}
}
