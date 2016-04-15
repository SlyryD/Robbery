using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AtlasUtils
{
	static public TextureImporter GetTextureImporter (string path)
	{
		if (!string.IsNullOrEmpty(path))
		{
			TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
			if (ti != null)
			{
				return ti;
			}
		}
		return null;
	}

	/// <summary>
	/// Makes the texture readable.
	/// </summary>
	static public bool SetTextureReadable (string path, bool force)
	{
		TextureImporter ti = GetTextureImporter(path);
		if (ti != null)
		{
			TextureImporterSettings settings = new TextureImporterSettings();
			ti.ReadTextureSettings(settings);

			if (force ||
				settings.mipmapEnabled ||
				!settings.readable ||
				settings.maxTextureSize < 4096 ||
				settings.filterMode != FilterMode.Point ||
				settings.wrapMode != TextureWrapMode.Clamp ||
				settings.npotScale != TextureImporterNPOTScale.None)
			{
				settings.mipmapEnabled = false;
				settings.readable = true;
				settings.maxTextureSize = 4096;
				settings.textureFormat = TextureImporterFormat.ARGB32;
				settings.filterMode = FilterMode.Point;
				settings.wrapMode = TextureWrapMode.Clamp;
				settings.npotScale = TextureImporterNPOTScale.None;
				
				ti.SetTextureSettings(settings);
				AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
			}
			return true;
		}
		return false;
	}
	
	static public Texture2D GetReadableTexture (string path, bool force)
	{
		Debug.Log("Import Shape = "+path);
		if (SetTextureReadable(path, force))
		{
			return AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
		}
		return null;
	}
	
	static public Texture2D GetReadableTexture (Texture tex, bool force)
	{
		if (tex != null)
		{
			string path = AssetDatabase.GetAssetPath(tex.GetInstanceID());
			return GetReadableTexture(path, force);
		}
		return null;
	}
	
	static bool PrepareAtlas (string path, bool force)
	{
		if (!string.IsNullOrEmpty(path))
		{
			TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
			if (ti != null)
			{
				TextureImporterSettings settings = new TextureImporterSettings();
				ti.ReadTextureSettings(settings);
		
				if (force ||
					settings.readable ||
					settings.maxTextureSize < 4096 ||
					settings.wrapMode != TextureWrapMode.Clamp ||
					settings.npotScale != TextureImporterNPOTScale.ToNearest)
				{
					settings.mipmapEnabled = true;
					settings.readable = false;
					settings.maxTextureSize = 4096;
					// high-precision color is required to prevent bleeding
					settings.textureFormat = TextureImporterFormat.RGBA32;
//					settings.textureFormat = TextureImporterFormat.DXT5;
					settings.filterMode = FilterMode.Trilinear;
					settings.aniso = 4;
					settings.wrapMode = TextureWrapMode.Clamp;
					settings.npotScale = TextureImporterNPOTScale.ToNearest;
					
					ti.SetTextureSettings(settings);
					AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
				}
				return true;
			}
		}
		return false;
	}
	
	static public Texture2D GetAtlasTexture (string path, bool force)
	{
		Debug.Log("Import Atlas = "+path);
		if (PrepareAtlas(path,force))
		{
			return AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
		}
		return null;
	}
	
	static public void Generate(AtlasDesc desc)
	{
		Debug.Log("Generate");
		
		string path = null;
		Texture2D txm = desc.Target;
		
		if (txm == null)
		{
			txm = new Texture2D(1, 1, TextureFormat.ARGB32, true);			
			path = AssetDatabase.GetAssetPath(desc.GetInstanceID());
			Debug.Log("New Texture = "+path);
		}
		else
		{
			path = AssetDatabase.GetAssetPath(txm.GetInstanceID());
			txm = GetReadableTexture(path,false);
			Debug.Log("Recycle Texture = "+path);
		}
		
		path = string.IsNullOrEmpty(path) ? "Assets/" + desc.name + ".png" : path.Replace(".prefab", ".png");
		Debug.Log("Path = "+path);
		
		if (!System.IO.File.Exists(path))
		{
			Debug.LogError("File does not exist = "+path);
		}
		
		if (txm != null)
		{
			List<Texture2D> src = new List<Texture2D>();
			foreach (AtlasDesc.Item it in desc.List)
			{
				if (it.Image != null)
				{
					//Debug.Log("Shape = "+it.AtlasShape);
					Texture2D t = GetReadableTexture(it.Image,false);
					if (t != null)
					{
						//Debug.Log("Succes!");
						src.Add(t);
					}
					else
					{
						Debug.LogError(string.Format("Atlas Shape '{0}' is not readable?",it.AtlasShape));
					}
				}
				else
				{
					Debug.LogError(string.Format("Atlas Shape '{0}' has no image?",it.AtlasShape));
				}
			}
			
			Debug.Log("Packing...");
			Rect [] result = txm.PackTextures(src.ToArray(),desc.Padding);
			
			Debug.Log("Writing file = "+path);
			byte [] data = txm.EncodeToPNG();
			System.IO.File.WriteAllBytes(path,data);
			data = null;
			
			AssetDatabase.Refresh();
			txm = GetAtlasTexture(path,false);
			if (txm == null)
			{
				Debug.Log ("Unable to import texture?");
			}
			desc.Target = txm;
			AssetDatabase.SaveAssets();
			
			if (desc.Atlas != null)
			{
				List<CardShape> shapes = new List<CardShape>();
				for (int i=0; i<result.Length; ++i)
				{
					CardShape s = new CardShape();
					string id = desc.List[i].AtlasShape;
					s.Id = string.IsNullOrEmpty(id) ? string.Format("Element{0}",i) : id;
					s.Image = txm; //desc.List[i].Image;
					Rect r = result[i];
					float bx = desc.List[i].BorderPixels / txm.width;
					float by = desc.List[i].BorderPixels / txm.height;
					s.Min = new Vector2(r.xMin+bx,r.yMin+by);
					s.Max = new Vector2(r.xMax-bx,r.yMax-by);
					shapes.Add(s);
				}
				desc.Atlas.ShapeList = shapes.ToArray();
			}
		}
	}
}