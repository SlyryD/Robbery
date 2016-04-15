using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(Card))]
public class CardInspector : Editor
{
	static string Verify(CardAtlas atlas, string text, string property, string msg)
	{
			if (string.IsNullOrEmpty(text))
			{
				return string.Format("Set the {0} property {1}.",property,msg);
			}
			if (atlas == null || atlas.FindById(text) == null)
			{
				return string.Format("{0} shape name '{1}' is not defined in the Atlas.",property,text);
			}
			return null;
	}
	
	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector();
		
		//EditorGUIUtility.LookLikeControls(15f);

		Card c = target as Card;
		
		Color old = GUI.color;

		bool valid = c.IsValid();

		if (valid)
		{
			string msg = null;
			if (msg == null)
			{
				msg = Verify(c.Definition.Atlas,c.Definition.Text,"Text","to display a number in the corner");
			}
			if (msg == null)
			{
				if (c.Definition.Pattern == 0)
				{
					if (c.Definition.Image == null)
					{
						msg = "Set the Pattern property to display symbols on the card.";
					}
				}
				else if (c.Definition.Pattern < 0 || c.Definition.Pattern > 10)
				{
					msg = "The Pattern property should be 0,1,2,3,4,5,6,7,8,9,10.";
				}
				else if (!string.IsNullOrEmpty(c.Definition.Image))
				{
					msg = "The Image property is ignored when Pattern is set.";
				}
			}
			if (msg != null)
			{
				EditorGUILayout.HelpBox(msg,MessageType.Info);
			}
		}
		else
		{
			string msg = "The card description is invalid.";
			if (c.Definition != null)
			{
				if (c.Definition.Atlas == null)
				{
					msg = "You must assign the Atlas property to a valid object reference.";
				}
				else if (c.Definition.Stock == null)
				{
					msg = "You must assign the Stock property to a valid object reference.";
				}
				else if (c.Definition.Stock.DefaultMaterial == null)
				{
					msg = "The Card Stock property DefaultMaterial is not a valid object reference.";
				}
			}
			else
			{
				msg = "Null reference?";
			}
			GUI.color = new Color(1.0f,0.5f,0.5f,1.0f);
			EditorGUILayout.HelpBox(msg,MessageType.Error);
		}
		
		EditorGUILayout.BeginHorizontal();
		{
			GUI.color = valid ? Color.white : Color.red;
			string tip = valid ? "Generate Mesh" : "Fix errors to enable button.";
			if (DrawButton("Generate", tip, 160f) && valid)
			{
				c.Rebuild();
			}
			GUI.color = old;
		}
		EditorGUILayout.EndHorizontal();
	}
	
	/// <summary>
	/// Helper function that draws a button in an enabled or disabled state.
	/// </summary>

	static bool DrawButton (string title, string tooltip, float width)
	{
		// Draw a regular button
		return GUILayout.Button(new GUIContent(title, tooltip)); //, GUILayout.Width(width));
	}
}
