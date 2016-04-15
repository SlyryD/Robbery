using UnityEngine;
using System.Collections;

public class CardAtlas : MonoBehaviour
{
	public CardShape [] ShapeList;
	
	// Search for a shape reference by its name.
	public CardShape FindById(string id)
	{
		if (!string.IsNullOrEmpty(id))
		{
			foreach (CardShape shape in ShapeList)
			{
				if (string.Compare(shape.Id,id,true) == 0) // ignore upper/lower case differences
				{
					return shape;
				}
			}
		}
		return null; // not found
	}
}

[System.Serializable]
public class CardShape
{
	public string Id;
	public Texture2D Image;
	// Top-left UV coordinate in texture atlas
	public Vector2 Min = new Vector2(0,0);
	// Bottom-right UV coordinate in texture atlas
	public Vector2 Max = new Vector2(1,1);
	
	// Unity uses OpenGL layout where 0,0 is the bottom left corner of the texture.
	public Vector2 UV0 // top-left
	{
		get { return new Vector2(Min.x,Max.y); }
	}
	public Vector2 UV1 // top-right
	{
		get { return Max; }
	}
	public Vector2 UV2 // bottom-right
	{
		get { return new Vector2(Max.x,Min.y); }
	}
	public Vector2 UV3 // bottom-left
	{
		get { return Min; }
	}
}

