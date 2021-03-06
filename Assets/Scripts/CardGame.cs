using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class CardGame : MonoBehaviour
{
    // Audio clips
    public AudioClip ThoughtLoop;
    public AudioClip ThoughtEnd;

    // Deck of cards
    public CardDeck Deck;

    // Deck dealing time
    const float DealTime = 0.35f;
    const float FlyTime = 0.5f;

    // Lists of cards
    List<Card> m_boardCards = new List<Card>();
    List<Card> m_dealerHand = new List<Card>();
    List<Card> m_playerHand = new List<Card>();
    List<Card> m_dealerVault = new List<Card>();
    List<Card> m_playerVault = new List<Card>();

    // Get deck and discard planes
    GameObject DeckPlane;
    GameObject DiscardPlane;

    // Win messages
    GameObject PlayerWins;
    GameObject DealerWins;
    GameObject NobodyWins;
    GameObject ScoreText;

    // Num cards
    GameObject PlayerNumCards;
    GameObject DealerNumCards;

    // Turn text
    GameObject DealerTurnText;
    GameObject PlayerTurnText;

    // Audio source for music
    AudioSource music;
    bool load;
    int numUpdates;

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
        ShowingText,
    };

    GameState m_state;

    // Use this for initialization
    void Start()
    {
        m_state = GameState.Invalid;
        Deck.Initialize();

        // Setup discard plane
        DeckPlane = this.transform.Find("Deck-Plane").gameObject;
        DeckPlane.SetActive(true);
        DiscardPlane = this.transform.Find("Discard-Plane").gameObject;
        DiscardPlane.SetActive(false);

        // Set up win messages
        PlayerWins = this.transform.Find("MessagePlayerWins").gameObject;
        DealerWins = this.transform.Find("MessageDealerWins").gameObject;
        NobodyWins = this.transform.Find("MessageTie").gameObject;
        ScoreText = this.transform.Find("Score").gameObject;
        PlayerWins.SetActive(false);
        DealerWins.SetActive(false);
        NobodyWins.SetActive(false);
        ScoreText.SetActive(false);

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

        // Start music
        // Audio source for music
        music = this.GetComponent<AudioSource>();
        music.loop = true;
        music.clip = ThoughtLoop;
        music.Play();
        load = false;
        numUpdates = 0;

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
            ScoreText.SetActive(true);
        }
        else if (msg == "Player")
        {
            PlayerWins.SetActive(true);
            DealerWins.SetActive(false);
            NobodyWins.SetActive(false);
            ScoreText.SetActive(true);
        }
        else if (msg == "Nobody")
        {
            PlayerWins.SetActive(false);
            DealerWins.SetActive(false);
            NobodyWins.SetActive(true);
            ScoreText.SetActive(true);
        }
        else
        {
            PlayerWins.SetActive(false);
            DealerWins.SetActive(false);
            NobodyWins.SetActive(false);
            ScoreText.SetActive(false);
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

    void LateUpdate()
    {
        // Eventually load credits
        if (load)
        {
            if (numUpdates < 750)
            {
                numUpdates++;
            }
            else
            {
                load = false;
                UnityEngine.SceneManagement.SceneManager.LoadScene("Credits");
            }
        }
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

    void AdjustCount(GameObject CountText, int val)
    {
        TextMesh mesh = CountText.GetComponent<TextMesh>();
        mesh.text = (int.Parse(mesh.text) + val).ToString();
    }

    Vector3 GetNextBoardPosition(int numCards)
    {
        float x = -3f;
        float y = 0f;
        float z = (m_boardCards.Count) * -0.1f;
        if (numCards > 5)
        {
            x += (m_boardCards.Count) * 12.5f / numCards;
        }
        else
        {
            x += (m_boardCards.Count) * 2.5f;
        }
        return new Vector3(x, y, z);
    }

    Vector3 GetNextDealerPosition()
    {
        float x = -3f;
        float y = 10f;
        float z = 5f;
        return new Vector3(x, y, z);
    }

    Vector3 GetNextPlayerPosition()
    {
        float x = -3 + (m_playerHand.Count) * 2.5f;
        float y = -10f;
        float z = (m_playerHand.Count) * -0.1f;
        return new Vector3(x, y, z);
    }

    Vector3 GetNextDealerVaultPosition()
    {
        float x = 0f;
        float y = 4f;
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

    Vector3 GetNextPlayerShowPosition(int numCards)
    {
        float x = -6f + (m_playerVault.Count * 15f / numCards);
        float y = -7.5f;
        float z = (m_playerVault.Count) * -0.1f;
        return new Vector3(x, y, z);
    }

    Vector3 GetNextDealerShowPosition(int numCards)
    {
        float x = -6f + (m_dealerVault.Count * 15f / numCards);
        float y = 4f;
        float z = (m_dealerVault.Count) * -0.1f;
        return new Vector3(x, y, z);
    }

    bool DrawHelper(List<Card> player, Vector3 position, bool noFlip = false)
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
        DeckPlane.SetActive(false);
        return false;
    }

    void DealBoard()
    {
        Debug.Log("DealBoard");
        DrawHelper(m_boardCards, GetNextBoardPosition(m_boardCards.Count + 1));
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
            while (m_dealerHand.Count < 5 && DrawDealer())
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
        TransferCard(card, cards, m_boardCards, GetNextBoardPosition(m_boardCards.Count + 1));
    }

    IEnumerator Discard()
    {
        // Check if game is over
        bool playerDone = (m_playerHand.Count <= 0);
        bool dealerDone = (m_dealerHand.Count <= 0);
        if (playerDone && dealerDone)
        {
            OnStop();
        }

        // Otherwise discard and switch turns
        if (m_state == GameState.PlayerTurn)
        {
            if (!playerDone)
            {
                List<Card> cards = GetClickedCards(m_playerHand);
                DiscardHelper(m_playerHand, cards[0]);
                DiscardPlane.SetActive(false);
                AdjustCount(PlayerNumCards, -1);
                yield return new WaitForSeconds(DealTime);
                Tidy();
                yield return new WaitForSeconds(DealTime);
            }
            Debug.Log("Switching from player to dealer");
            m_state = GameState.DealerTurn;
        }
        else if (m_state == GameState.DealerTurn)
        {
            if (!dealerDone)
            {
                int cardIdx = UnityEngine.Random.Range(0, m_dealerHand.Count);
                Card card = m_dealerHand[cardIdx];
                DiscardHelper(m_dealerHand, card);
                AdjustCount(DealerNumCards, -1);
                yield return new WaitForSeconds(DealTime);
                Tidy();
                yield return new WaitForSeconds(DealTime);
            }
            Debug.Log("Switching from dealer to player");
            m_state = GameState.PlayerTurn;
        }
        yield return StartCoroutine(BeginTurn());
    }

    bool SameValue(Card c1, Card c2)
    {
        if (c1 == null || c2 == null)
        {
            return false;
        }
        return string.Compare(c1.Definition.Text, c2.Definition.Text) == 0;
    }

    List<Card> GetTopCards(List<Card> cards)
    {
        List<Card> topCards = new List<Card>();
        Card topCard = GetLastCard(cards);
        if (topCard != null)
        {
            for (int i = cards.Count - 1; i >= 0; i--)
            {
                Card nextCard = cards[i];
                if (SameValue(nextCard, topCard))
                {
                    topCards.Add(nextCard);
                }
                else
                {
                    break;
                }
            }
        }
        return topCards;
    }

    Card GetLastCard(List<Card> cards)
    {
        if (cards.Count > 0)
        {
            return cards[cards.Count - 1];
        }
        return null;
    }

    IEnumerator Loot(List<Card> lootCards, List<Card> source)
    {
        if (m_state == GameState.PlayerTurn)
        {
            List<Card> handCards = GetClickedCards(m_playerHand);
            Debug.Log(String.Format("{0} cards to loot", lootCards.Count));
            Debug.Log(String.Format("{0} hand cards to loot with", handCards.Count));
            Debug.Log(String.Format("They are the same value: {0}", SameValue(handCards[0], lootCards[0])));
            if (handCards.Count > 0 && SameValue(handCards[0], lootCards[0]))
            {
                foreach (Card c in lootCards)
                {
                    TransferCard(c, source, m_playerVault, GetNextPlayerVaultPosition());
                }
                foreach (Card c in handCards)
                {
                    TransferCard(c, m_playerHand, m_playerVault, GetNextPlayerVaultPosition());
                }
                DiscardPlane.SetActive(false);
                AdjustCount(PlayerNumCards, -handCards.Count);
                yield return new WaitForSeconds(DealTime);
                Tidy();
                yield return StartCoroutine(Draw());
            }
            if (m_playerHand.Count <= 0)
            {
                yield return StartCoroutine(Discard());
            }
        }
        else if (m_state == GameState.DealerTurn)
        {
            Debug.Log("Begin dealer loot phase...");
            // Cards in hand?
            if (m_dealerHand.Count > 0)
            {
                Debug.Log(string.Format("Cards in dealer\'s hand = {0}", m_dealerHand.Count));
                bool failedLoot = false;
                if (m_playerVault.Count > 0)
                {
                    Debug.Log(string.Format("Cards in player\'s vault = {0}", m_playerVault.Count));
                    // Try looting player vault
                    List<Card> handCards = new List<Card>();
                    Card cardToLoot = GetLastCard(m_playerVault);
                    foreach (Card handCard in m_dealerHand)
                    {
                        if (SameValue(cardToLoot, handCard))
                        {
                            handCards.Add(handCard);
                        }
                    }

                    // Looting...
                    Debug.Log(string.Format("Plan to loot {0} cards", handCards.Count));
                    if (handCards.Count > 0)
                    {
                        Debug.Log(string.Format("Looting with {0} hand cards", handCards.Count));
                        // TODO: Insert looting animation here

                        // Transfer first card
                        TransferCard(cardToLoot, m_playerVault, m_dealerVault, GetNextDealerVaultPosition());

                        // Look for more
                        Card nextCard = GetLastCard(m_playerVault);
                        while (nextCard != null && SameValue(cardToLoot, nextCard))
                        {
                            TransferCard(nextCard, m_playerVault, m_dealerVault, GetNextDealerVaultPosition());
                            nextCard = GetLastCard(m_playerVault);
                        }

                        // Transfer hand cards to dealer vault
                        foreach (Card handCard in handCards)
                        {
                            TransferCard(handCard, m_dealerHand, m_dealerVault, GetNextDealerVaultPosition());
                        }

                        // Decrement hand count
                        AdjustCount(DealerNumCards, -handCards.Count);

                        // Tidy up board afterwards
                        Tidy();
                        yield return new WaitForSeconds(DealTime);
                        yield return StartCoroutine(Draw());

                        // Loot again
                        yield return new WaitForSeconds(DealTime);
                        yield return StartCoroutine(Loot(null, null));
                    }
                    else
                    {
                        failedLoot = true;
                    }
                }

                // Try looting board
                if (m_playerVault.Count <= 0 || failedLoot)
                {
                    Debug.Log("Attempting to loot board...");
                    List<Card> handCards = new List<Card>();
                    List<Card> cardsToLoot = new List<Card>();
                    Card foundCard = null;

                    // Look through hand cards
                    foreach (Card handCard in m_dealerHand)
                    {
                        if (foundCard == null)
                        {
                            // Find matching board cards and add to list
                            foreach (Card cardToLoot in m_boardCards)
                            {
                                if (SameValue(handCard, cardToLoot))
                                {
                                    foundCard = handCard;
                                    cardsToLoot.Add(cardToLoot);
                                }
                            }
                        }

                        // Add matching hand cards to list
                        if (foundCard != null)
                        {
                            if (SameValue(handCard, foundCard))
                            {
                                handCards.Add(handCard);
                            }
                        }
                    }

                    // Loot board
                    if (handCards.Count > 0 && cardsToLoot.Count > 0)
                    {
                        Debug.Log(string.Format("Looting board with {0} hand cards", handCards.Count));
                        // Put board cards in vault
                        foreach (Card cardToLoot in cardsToLoot)
                        {
                            TransferCard(cardToLoot, m_boardCards, m_dealerVault, GetNextDealerVaultPosition());
                        }

                        // Put hand cards in vault
                        foreach (Card handCard in handCards)
                        {
                            TransferCard(handCard, m_dealerHand, m_dealerVault, GetNextDealerVaultPosition());
                        }

                        // Decrement hand count
                        AdjustCount(DealerNumCards, -handCards.Count);

                        // Tidy up board afterwards
                        Tidy();
                        yield return new WaitForSeconds(DealTime);
                        yield return StartCoroutine(Draw());

                        // Loot again
                        yield return new WaitForSeconds(DealTime);
                        yield return StartCoroutine(Loot(null, null));
                    }
                    else
                    {
                        Debug.Log("Dealer discarding");
                        yield return new WaitForSeconds(DealTime);
                        yield return StartCoroutine(Discard());
                    }
                }
            }
            else
            {
                Debug.Log("Dealer discarding");
                yield return new WaitForSeconds(DealTime);
                yield return StartCoroutine(Discard());
            }
        }
    }

    /* END TURN ACTIONS */

    void ShowVaultCards()
    {
        // Player vault cards
        Tidy(m_playerVault, new Func<int, Vector3>(GetNextPlayerShowPosition), m_playerVault.Count);

        // Dealer vault cards
        Tidy(m_dealerVault, new Func<int, Vector3>(GetNextDealerShowPosition), m_dealerVault.Count);
    }

    void Tidy()
    {
        // Board cards
        Tidy(m_boardCards, new Func<int, Vector3>(GetNextBoardPosition), m_boardCards.Count);

        // Player hand cards
        Tidy(m_playerHand, new Func<Vector3>(GetNextPlayerPosition));

        // Player vault
        Tidy(m_playerVault, new Func<Vector3>(GetNextPlayerVaultPosition));

        // Dealer vault
        Tidy(m_dealerVault, new Func<Vector3>(GetNextDealerVaultPosition));
    }

    void Tidy(List<Card> cards, Delegate method)
    {
        // Board cards
        Card[] temp = new Card[cards.Count];
        cards.CopyTo(temp);
        cards.Clear();
        foreach (Card c in temp)
        {
            Vector3 source = c.transform.position;
            Vector3 target = (Vector3)method.DynamicInvoke();
            Vector3 distance = source - target;
            if (distance.magnitude > 0.1f)
            {
                c.SetFlyTarget(source, target, FlyTime, true);
            }
            cards.Add(c);
        }
    }

    void Tidy(List<Card> cards, Delegate method, int numCards)
    {
        // Board cards
        Card[] temp = new Card[cards.Count];
        cards.CopyTo(temp);
        cards.Clear();
        foreach (Card c in temp)
        {
            Vector3 source = c.transform.position;
            Vector3 target = (Vector3)method.DynamicInvoke(numCards);
            Vector3 distance = source - target;
            if (distance.magnitude > 0.1f)
            {
                c.SetFlyTarget(source, target, FlyTime, true);
            }
            cards.Add(c);
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
            if (m_playerHand.Count <= 0)
            {
                yield return StartCoroutine(Discard());
            }
            else
            {
                m_state = GameState.ShowingText;
                yield return StartCoroutine(ShowAndFade(PlayerTurnText));
                m_state = GameState.PlayerTurn;
                yield return StartCoroutine(Draw());
            }
        }
        else if (m_state == GameState.DealerTurn)
        {
            if (m_dealerHand.Count <= 0)
            {
                yield return StartCoroutine(Discard());
            }
            else
            {
                m_state = GameState.ShowingText;
                yield return StartCoroutine(ShowAndFade(DealerTurnText));
                m_state = GameState.DealerTurn;
                yield return StartCoroutine(Draw());
                yield return new WaitForSeconds(DealTime);
                yield return StartCoroutine(Loot(null, null));
            }
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
            //m_state = GameState.PlayerTurn;
            if (UnityEngine.Random.value >= 0.5f)
            {
                m_state = GameState.PlayerTurn;
            }
            else
            {
                m_state = GameState.DealerTurn;
            }

            // Begin turn
            yield return StartCoroutine(BeginTurn());
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

    void TryFinalize(int playerScore, int dealerScore)
    {
        // Play end music
        music.Stop();
        music.clip = ThoughtEnd;
        music.loop = false;
        music.Play();
        load = true;

        // Show vault cards
        ShowVaultCards();

        // Show score
        TextMesh mesh = ScoreText.GetComponent<TextMesh>();
        if (dealerScore > playerScore)
        {
            mesh.text = String.Format(mesh.text, dealerScore, playerScore);
            ShowMessage("Dealer");
            m_state = GameState.DealerWins;
        }
        else if (dealerScore < playerScore)
        {
            mesh.text = String.Format(mesh.text, playerScore, dealerScore);
            ShowMessage("Player");
            m_state = GameState.PlayerWins;
        }
        else
        {
            // Nobody Wins!
            mesh.text = String.Format(mesh.text, playerScore, dealerScore);
            ShowMessage("Nobody");
            m_state = GameState.NobodyWins;
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
                    Vector3 position = GetNextBoardPosition(m_boardCards.Count);
                    position.z = -0.1f;
                    DiscardPlane.transform.position = position;
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
                    StartCoroutine(Loot(boardCards, m_boardCards));
                }
            }
            // When player clicks on a card on the board
            else if (m_dealerVault.Contains(card))
            {
                Debug.Log("Attempting to loot dealer\'s vault...");
                List<Card> handCards = GetClickedCards(m_playerHand);
                Debug.Log(string.Format("{0} relevant cards in hand", handCards.Count));
                if (handCards.Count > 0)
                {
                    List<Card> vaultCards = GetTopCards(m_dealerVault);
                    Debug.Log(string.Format("{0} relevant cards in vault", vaultCards.Count));
                    StartCoroutine(Loot(vaultCards, m_dealerVault));
                }
            }
        }
    }

}
