using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class MainMenu : MonoBehaviour
{
    // Audio clips
    public AudioClip ThoughtLoop;
    public AudioClip ThoughtEnd;

    // Deck of cards
    public CardDeck Deck;

    // Deck dealing time
    const float DealTime = 0.35f;
    const float FlyTime = 0.5f;

    // Get deck and discard planes
    GameObject DeckPlane;

    // Audio source for music
    AudioSource music;

    // Renderer for flashing start sign
    TextMesh mesh;
    bool fade;

    // Use this for initialization
    void Start()
    {
        Deck.Initialize();

        // Setup discard plane
        DeckPlane = this.transform.Find("Deck-Plane").gameObject;
        DeckPlane.SetActive(true);

        // Start music
        music = this.GetComponent<AudioSource>();
        music.loop = true;
        music.clip = ThoughtLoop;
        music.Play();

        // Start by fading start sign
        GameObject startSign = this.transform.Find("StartGame").gameObject;
        mesh = startSign.GetComponent<TextMesh>();
        fade = true;

        // TODO: Start animation
        List<Card> cards = new List<Card>(DrawDeck());
        Debug.Log("Num cards = " + cards.Count);
        StartCoroutine(Animation(cards));
    }

    // Listen for key presses
    public void OnGUI()
    {
        if (Event.current.type == EventType.KeyUp)
        {
            switch (Event.current.keyCode)
            {
                case KeyCode.Return:
                    UnityEngine.SceneManagement.SceneManager.LoadScene("Robbery");
                    break;
                default:
                    break;
            }
        }
    }

    void LateUpdate()
    {
        Color color = mesh.color;
        float delta = Time.deltaTime;
        color.a += fade ? -delta : delta;
        if (color.a <= 0.2f)
        {
            color.a = 0.2f;
            fade = false;
        }
        else if (color.a >= 1f)
        {
            color.a = 1f;
            fade = true;
        }
        mesh.color = color;
    }

    void Clear()
    {
        Deck.Reset();
    }

    Vector3 GetDeckPosition()
    {
        return DeckPlane.transform.position;
    }

    IEnumerable<Card> DrawDeck()
    {
        CardDef c1 = Deck.Pop();
        Debug.Log("Popped card: " + c1);
        while (c1 != null)
        {
            Debug.Log("Deck-Popped");
            GameObject newObj = new GameObject();
            newObj.name = "Card";
            Card newCard = newObj.AddComponent(typeof(Card)) as Card;
            newCard.Definition = c1;
            newObj.transform.parent = Deck.transform;
            newCard.TryBuild();
            yield return newCard;
            c1 = Deck.Pop();
        }
    }

    IEnumerator Animation(List<Card> cards)
    {
        Vector3 source = GetDeckPosition();
        Vector3 dest = new Vector3(-source.x, source.y, source.z);
        Vector3 temp = Vector3.zero;
        foreach (Card card in cards)
        {
            Vector3 deckPos = GetDeckPosition();
            card.transform.position = deckPos;
            card.SetFlyTarget(source, dest, FlyTime);
            yield return new WaitForSeconds(FlyTime);
        }
        DeckPlane.transform.position = dest;
        temp = dest;
        dest = source;
        source = temp;
    }
}
