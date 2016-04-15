using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(AtlasDesc))]
public class AtlasInspector : Editor
{
	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector();
		
		//EditorGUIUtility.LookLikeControls(15f);

		AtlasDesc c = target as AtlasDesc;
		
		string msg = c.Validate();
		
		if (msg != null)
		{
			EditorGUILayout.HelpBox(msg,MessageType.Error);
		}
		
		EditorGUILayout.BeginHorizontal();
		{
			bool valid = (msg == null);
			if (DrawButton("Generate", "Generate Atlas", valid))
			{
				AtlasUtils.Generate(c);
				EditorUtility.SetDirty(c.Atlas);
			}
		}
		EditorGUILayout.EndHorizontal();
	}
	
	/// <summary>
	/// Helper function that draws a button in an enabled or disabled state.
	/// </summary>

	static bool DrawButton (string title, string tooltip, bool enabled)
	{
		if (enabled)
		{
			// Draw a regular button
			return GUILayout.Button(new GUIContent(title, tooltip)); //, GUILayout.Width(width));
		}
		else
		{
			// Button should be disabled -- draw it darkened and ignore its return value
			Color color = GUI.color;
			GUI.color = new Color(1f, 1f, 1f, 0.25f);
			GUILayout.Button(new GUIContent(title, tooltip)); //, GUILayout.Width(width));
			GUI.color = color;
			return false;
		}
	}
}
