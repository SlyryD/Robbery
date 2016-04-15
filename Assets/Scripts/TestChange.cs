using UnityEngine;
using System.Collections;

public class TestChange : MonoBehaviour {
	
	public GameObject Target;
	
	float m_startTime;
	
	// Use this for initialization
	void Start ()
	{
		m_startTime = Time.time;
	}

    int Sequence;

	// Update is called once per frame
	void Update ()
	{
		float dt = Time.time  - m_startTime;
		if (dt > 1.0f)
		{
			Card newCard = Target.GetComponent<Card>();

            switch (Sequence % 4)
            {
                case 0:
                    newCard.Definition.Text = "2";
                    newCard.Definition.Symbol = "Heart";
                    newCard.Definition.Pattern = 2;
                    break;
                case 1:
                    newCard.Definition.Text = "3";
                    newCard.Definition.Symbol = "Club";
                    newCard.Definition.Pattern = 3;
                    break;
                case 2:
                    newCard.Definition.Text = "4";
                    newCard.Definition.Symbol = "Diamond";
                    newCard.Definition.Pattern = 4;
                    break;
                case 3:
                    newCard.Definition.Text = "A";
                    newCard.Definition.Symbol = "Spade";
                    newCard.Definition.Pattern = 1;
                    break;
            }

            newCard.Rebuild();

            m_startTime = Time.time;
            ++Sequence;
        }
    }
}
