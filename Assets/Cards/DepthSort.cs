using UnityEngine;
using System.Collections;

//ExecuteInEditMode,
[RequireComponent(typeof(Camera))]
public class DepthSort : MonoBehaviour
{
	void Awake()
	{
		// This mode is required for overlapping cards to sort properly.
		// Otherwise objects are sorted by center which does not work for flat stacked objects.
		this.GetComponent<Camera>().transparencySortMode = TransparencySortMode.Orthographic;
	}
}
