using UnityEngine;
using System.Collections;

public class GameButton : MonoBehaviour
{
	public string Message;

	void OnMouseDown()
	{
		transform.parent.gameObject.GetComponent<CardGame>().OnButton(Message);
	}
}
