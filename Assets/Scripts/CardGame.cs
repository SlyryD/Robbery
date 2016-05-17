using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CardGame : MonoBehaviour
{
    // Deck of cards
    public CardDeck Deck;

    // Lists of cards
    List<Card> m_boardCards = new List<Card>();
    List<Card> m_dealerHand = new List<Card>();
    List<Card> m_playerHand = new List<Card>();
    List<Card> m_dealerVault = new List<Card>();
    List<Card> m_playerVault = new List<Card>();

    // Get discard plane
    GameObject DiscardPlane;

    // Win messages
    GameObject PlayerWins;
    GameObject DealerWins;
    GameObject NobodyWins;

    // Num cards
    GameObject PlayerNumCards;
    GameObject DealerNumCards;

    // Turn text
    GameObject DealerTurnText;
    GameObject PlayerTurnText;

    enum GameState
    {
        Invalid,
        //Started,
        //PlayerBusted,
        Resolving,
        DealerWins,
        PlayerWins,
        NobodyWins,
        DealerTurn,
        PlayerTurn,
    };

    GameState m_state;

    //GameObject[] Buttons;

    // Use this for initialization
    void Start()
    {
        m_state = GameState.Invalid;
        Deck.Initialize();

        // Setup discard plane
        DiscardPlane = this.transform.Find("Discard-Plane").gameObject;
        DiscardPlane.SetActive(false);

        // Set up win messages
        PlayerWins = this.transform.Find("MessagePlayerWins").gameObject;
        DealerWins = this.transform.Find("MessageDealerWins").gameObject;
        NobodyWins = this.transform.Find("MessageTie").gameObject;
        PlayerWins.SetActive(false);
        DealerWins.SetActive(false);
        NobodyWins.SetActive(false);

        //Buttons = new GameObject[3];
        //Buttons[0] = this.transform.Find("Button1").gameObject;
        //Buttons[1] = this.transform.Find("Button2").gameObject;
        //Buttons[2] = this.transform.Find("Button3").gameObject;
        //UpdateButtons();

        // Get number of cards
        DealerNumCards = this.transform.Find("DealerNumCards").gameObject;
        PlayerNumCards = this.transform.Find("PlayerNumCards").gameObject;
        DealerNumCards.SetActive(true);
        PlayerNumCards.SetActive(true);

        // Turn text
        DealerTurnText = this.transform.Find("DealerTurnText").gameObject;
        PlayerTurnText = this.transform.Find("PlayerTurnText").gameObject;
        DealerTurnText.SetActive(false);
        PlayerTurnText.SetActive(false);

        // Deal cards to start game
        StartCoroutine(OnReset());
    }

    //void UpdateButtons()
    //{
    //    Buttons[0].GetComponent<Renderer>().material.color = Color.blue;
    //    Buttons[1].GetComponent<Renderer>().material.color = (m_state == GameState.Started) ? Color.blue : Color.red;
    //    Buttons[2].GetComponent<Renderer>().material.color = (m_state == GameState.Started || m_state == GameState.PlayerBusted) ? Color.blue : Color.red;
    //}

    void ShowMessage(string msg)
    {
        if (msg == "Dealer")
        {
            PlayerWins.SetActive(false);
            DealerWins.SetActive(true);
            NobodyWins.SetActive(false);
        }
        else if (msg == "Player")
        {
            PlayerWins.SetActive(true);
            DealerWins.SetActive(false);
            NobodyWins.SetActive(false);
        }
        else if (msg == "Nobody")
        {
            PlayerWins.SetActive(false);
            DealerWins.SetActive(false);
            NobodyWins.SetActive(true);
        }
        else
        {
            PlayerWins.SetActive(false);
            DealerWins.SetActive(false);
            NobodyWins.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            StartCoroutine(OnReset());
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            StartCoroutine(OnDrawCards());
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            OnStop();
        }
        //UpdateButtons();
    }

    void Shuffle()
    {
        if (m_state != GameState.Invalid)
        {
        }
    }

    void Clear()
    {
        List<List<Card>> cardsList = new List<List<Card>> { m_boardCards, m_dealerHand, m_playerHand, m_dealerVault, m_playerVault };
        foreach (List<Card> cards in cardsList)
        {
            foreach (Card c in cards)
            {
                GameObject.Destroy(c.gameObject);
            }
            cards.Clear();
        }
        Deck.Reset();
    }

    Vector3 GetDeckPosition()
    {
        return new UnityEngine.Vector3(-6, 0, 0);
    }

    const float FlyTime = 0.5f;

    void AdjustCount(GameObject CountText, int val)
    {
        TextMesh mesh = CountText.GetComponent<TextMesh>();
        mesh.text = (int.Parse(mesh.text) + val).ToString();
    }

    Vector3 GetNextBoardPosition()
    {
        float x = -3 + (m_boardCards.Count) * 2.5f;
        float y = 0;
        float z = (m_boardCards.Count) * -0.1f;
        return new Vector3(x, y, z);
    }

    Vector3 GetNextDealerPosition()
    {
        float x = -3 + (m_dealerHand.Count) * 1.5f;
        float y = 10;
        float z = 5f;
        return new Vector3(x, y, z);
    }

    Vector3 GetNextPlayerPosition()
    {
        float x = -3 + (m_playerHand.Count) * 2.5f;
        float y = -10;
        float z = (m_playerHand.Count) * -0.1f;
        return new Vector3(x, y, z);
    }

    Vector3 GetNextDealerVaultPosition()
    {
        float x = 0;
        float y = 5;
        float z = (m_playerVault.Count) * -0.1f;
        return new Vector3(x, y, z);
    }

    Vector3 GetNextPlayerVaultPosition()
    {
        float x = 0;
        float y = -5;
        float z = (m_playerVault.Count) * -0.1f;
        return new Vector3(x, y, z);
    }

    bool DrawHelper(List<Card> player, Vector3 position)
    {
        CardDef c1 = Deck.Pop();
        if (c1 != null)
        {
            Debug.Log("Deck-Popped");
            GameObject newObj = new GameObject();
            newObj.name = "Card";
            Card newCard = newObj.AddComponent(typeof(Card)) as Card;
            newCard.Definition = c1;
            newObj.transform.parent = Deck.transform;
            newCard.TryBuild();
            player.Add(newCard);
            Vector3 deckPos = GetDeckPosition();
            newObj.transform.position = deckPos;
            newCard.SetFlyTarget(deckPos, position, FlyTime);
            return true;
        }
        return false;
    }

    void DealBoard()
    {
        Debug.Log("DealBoard");
        DrawHelper(m_boardCards, GetNextBoardPosition());
    }

    bool DrawDealer()
    {
        Debug.Log("DrawDealer");
        if (DrawHelper(m_dealerHand, GetNextDealerPosition()))
        {
            AdjustCount(DealerNumCards, 1);
            return true;
        }
        return false;
    }

    bool DrawPlayer()
    {
        Debug.Log("DrawPlayer");
        if (DrawHelper(m_playerHand, GetNextPlayerPosition()))
        {
            AdjustCount(PlayerNumCards, 1);
            return true;
        }
        return false;
    }

    /* TURN ACTIONS */

    IEnumerator Draw()
    {
        if (m_state == GameState.PlayerTurn)
        {
            while (m_playerHand.Count < 5 && DrawPlayer())
            {
                yield return new WaitForSeconds(DealTime);
            }
        }
        else if (m_state == GameState.DealerTurn)
        {
            while (m_playerHand.Count < 5 && DrawDealer())
            {
                yield return new WaitForSeconds(DealTime);
            }
        }
    }

    void TransferCard(Card card, List<Card> cards1, List<Card> cards2, Vector3 dest)
    {
        if (cards1.Remove(card))
        {
            cards2.Add(card);
            StartCoroutine(MoveCard(card, card.transform.position, dest));
        }
        else
        {
            throw new System.Exception(string.Format("Card {0} not found in hand1", card));
        }
    }

    void DiscardHelper(List<Card> cards, Card card)
    {
        TransferCard(card, cards, m_boardCards, GetNextBoardPosition());
    }

    IEnumerator Discard()
    {
        if (m_state == GameState.PlayerTurn)
        {
            List<Card> cards = GetClickedCards(m_playerHand);
            DiscardHelper(m_playerHand, cards[0]);
            DiscardPlane.SetActive(false);
            AdjustCount(PlayerNumCards, -1);
            yield return new WaitForSeconds(DealTime);
            Tidy();
            yield return new WaitForSeconds(DealTime);
            //m_state = GameState.DealerTurn;
        }
        else if (m_state == GameState.DealerTurn)
        {
            // TODO: Implement
            //DiscardHelper(m_dealerHand, card);
            yield return new WaitForSeconds(DealTime);
        }
    }

    bool SameValue(Card c1, Card c2)
    {
        return string.Compare(c1.Definition.Text, c2.Definition.Text) == 0;
    }

    IEnumerator Loot(List<Card> boardCards)
    {
        if (m_state == GameState.PlayerTurn)
        {
            List<Card> handCards = GetClickedCards(m_playerHand);
            if (handCards.Count > 0 && SameValue(handCards[0], boardCards[0]))
            {
                foreach (Card c in boardCards)
                {
                    TransferCard(c, m_boardCards, m_playerVault, GetNextPlayerVaultPosition());
                }
                foreach (Card c in handCards)
                {
                    TransferCard(c, m_playerHand, m_playerVault, GetNextPlayerVaultPosition());
                }
                DiscardPlane.SetActive(false);
                AdjustCount(PlayerNumCards, -handCards.Count);
                yield return new WaitForSeconds(DealTime);
                Tidy();
                yield return Draw();
            }
            //m_state = GameState.DealerTurn;
        }
        else if (m_state == GameState.DealerTurn)
        {
            // TODO: Implement
            //DiscardHelper(m_dealerHand, card);
            yield return new WaitForSeconds(DealTime);
        }
    }

    /* END TURN ACTIONS */

    void Tidy()
    {
        // Board cards
        Card[] temp = new Card[m_boardCards.Count];
        m_boardCards.CopyTo(temp);
        m_boardCards.Clear();
        foreach (Card c in temp)
        {
            c.transform.position = GetNextBoardPosition();
            m_boardCards.Add(c);
        }

        // Player hand cards
        temp = new Card[m_playerHand.Count];
        m_playerHand.CopyTo(temp);
        m_playerHand.Clear();
        foreach (Card c in temp)
        {
            c.transform.position = GetNextPlayerPosition();
            m_playerHand.Add(c);
        }
    }

    IEnumerator ShowAndFade(GameObject obj)
    {
        // Ensure text shows
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        Color c = renderer.material.color;
        c.a = 1f;
        renderer.material.color = c;
        obj.SetActive(true);
        yield return new WaitForSeconds(DealTime);

        // Fade text
        for (float f = 1f; f >= 0; f -= 0.1f)
        {
            c = renderer.material.color;
            c.a = f;
            renderer.material.color = c;
            yield return new WaitForSeconds(0.1f);
        }

        // Hide text
        obj.SetActive(false);
    }

    IEnumerator BeginTurn()
    {

        if (m_state == GameState.PlayerTurn)
        {
            yield return StartCoroutine(ShowAndFade(PlayerTurnText));
            yield return Draw();
        }
        else if (m_state == GameState.DealerTurn)
        {
            yield return StartCoroutine(ShowAndFade(PlayerTurnText));
            yield return Draw();
            // TODO: Begin AI
        }
    }

    IEnumerator MoveCard(Card card, Vector3 source, Vector3 dest)
    {
        card.SetFlyTarget(source, dest, FlyTime, true);
        yield return new WaitForSeconds(FlyTime);
    }

    static int GetScore(List<Card> cards)
    {
        int score = 0;
        foreach (Card c in cards)
        {
            switch (c.Definition.Text)
            {
                case "A":
                    score += 20;
                    break;
                case "K":
                case "Q":
                case "J":
                case "10":
                case "9":
                case "8":
                    score += 10;
                    break;
                case "7":
                case "6":
                case "5":
                case "4":
                case "3":
                case "2":
                    score += 5;
                    break;
                default:
                    score += 0;
                    break;
            }
        }
        return score;
    }

    int GetDealerScore()
    {
        return GetScore(m_dealerVault);
    }

    int GetPlayerScore()
    {
        return GetScore(m_playerVault);
    }

    const float DealTime = 0.35f;

    IEnumerator OnReset()
    {
        if (m_state != GameState.Resolving)
        {
            // Resolve game
            m_state = GameState.Resolving;
            ShowMessage("");

            // Clear cards and shuffle deck
            Clear();
            Deck.Shuffle();

            // DrawHelper cards and place on board
            for (int i = 0; i < 4; i++)
            {
                DealBoard();
                yield return new WaitForSeconds(DealTime);
            }

            // DrawHelper cards for dealer and player
            for (int i = 0; i < 4; i++)
            {
                DrawDealer();
                yield return new WaitForSeconds(DealTime);
                DrawPlayer();
                yield return new WaitForSeconds(DealTime);
            }

            // Randomly choose player's turn
            m_state = GameState.PlayerTurn;
            //if (Random.value >= 0.5f)
            //{
            //    m_state = GameState.PlayerTurn;
            //} else
            //{
            //    m_state = GameState.DealerTurn;
            //}

            // Begin turn
            StartCoroutine(BeginTurn());
        }
    }

    IEnumerator OnDrawCards()
    {
        Debug.Log("OnDrawCards");
        if (m_state == GameState.PlayerTurn)
        {
            DrawPlayer();
            yield return new WaitForSeconds(DealTime);
        }
        else if (m_state == GameState.DealerTurn)
        {
            DrawDealer();
            yield return new WaitForSeconds(DealTime);
        }
    }

    bool TryFinalize(int playerScore, int dealerScore)
    {
        if (dealerScore > playerScore)
        {
            ShowMessage("Dealer");
            m_state = GameState.DealerWins;
            return true;
        }
        else if (dealerScore < playerScore)
        {
            ShowMessage("Player");
            m_state = GameState.PlayerWins;
            return true;
        }
        else
        {
            // Nobody Wins!
            ShowMessage("Nobody");
            m_state = GameState.NobodyWins;
            return true;
        }
    }

    void OnStop()
    {
        if (m_state == GameState.DealerTurn || m_state == GameState.PlayerTurn)
        {
            m_state = GameState.Resolving;
            //UpdateButtons();
            int playerScore = GetPlayerScore();
            int dealerScore = GetDealerScore();
            Debug.Log(string.Format("Player={0}  Dealer={1}", playerScore, dealerScore));
            TryFinalize(playerScore, dealerScore);
        }
    }

    public void OnButton(string msg)
    {
        Debug.Log("OnButton = " + msg);
        switch (msg)
        {
            case "Discard":
                StartCoroutine(Discard());
                break;
            case "Reset":
                StartCoroutine(OnReset());
                break;
            case "Draw":
                StartCoroutine(OnDrawCards());
                break;
            case "Stop":
                OnStop();
                break;
        }
    }

    private void MoveY(Transform trans, int dy)
    {
        Vector3 pos = trans.position;
        trans.position = new Vector3(pos.x, pos.y + dy, pos.z);
    }

    private void MoveUp(Transform trans)
    {
        MoveY(trans, 1);
    }

    private void MoveDown(Transform trans)
    {
        MoveY(trans, -1);
    }

    private List<Card> GetClickedCards(List<Card> cards)
    {
        List<Card> clickedCards = new List<Card>();
        foreach (Card c in cards)
        {
            if (c.Clicked)
            {
                clickedCards.Add(c);
            }
        }
        return clickedCards;
    }

    // Callback for card clicks
    public void CardClicked(Card card)
    {
        // Player clicks only matter when it's their turn
        if (m_state == GameState.PlayerTurn)
        {
            // When player clicks on a card in their hand
            if (m_playerHand.Contains(card))
            {
                // Unclick card
                if (card.Clicked)
                {
                    card.Clicked = false;
                    MoveDown(card.transform);
                    if (GetClickedCards(m_playerHand).Count <= 0)
                    {
                        DiscardPlane.SetActive(false);
                    }
                }
                else
                {
                    // Unclick other cards in hand
                    foreach (Card c in GetClickedCards(m_playerHand))
                    {
                        if (c != card && !SameValue(c, card))
                        {
                            c.Clicked = false;
                            MoveDown(c.transform);
                        }
                    }

                    // Click card
                    card.Clicked = true;
                    MoveUp(card.transform);
                }

                // Show discard action if only one card selected
                int numClicked = GetClickedCards(m_playerHand).Count;
                if (numClicked == 1)
                {
                    DiscardPlane.transform.position = GetNextBoardPosition();
                    DiscardPlane.SetActive(true);
                }
                else
                {
                    DiscardPlane.SetActive(false);
                }
            }
            // When player clicks on a card on the board
            else if (m_boardCards.Contains(card))
            {
                // TODO: Handle case of multiple cards in middle
                List<Card> handCards = GetClickedCards(m_playerHand);
                if (handCards.Count > 0)
                {
                    List<Card> boardCards = new List<Card> { card };
                    StartCoroutine(Loot(boardCards));
                }
            }
        }
    }

}
