using UnityEngine;
using System.Collections;

public class CardDefinition : MonoBehaviour
{
    public CardDef Data;
}

[System.Serializable]
public class CardDef
{
    public CardAtlas Atlas;
    public CardStock Stock;
    public string Text;
    public string Symbol; // Atlas shape name
    public int Pattern;
    public string Image;
    public bool FullImage;

    public CardDef(CardAtlas atlas, CardStock stock, string text, string symbol, int pattern)
    {
        Atlas = atlas;
        Stock = stock;
        Text = text;
        Symbol = symbol;
        Pattern = pattern;
    }

    public override string ToString()
    {
        if (string.IsNullOrEmpty(Symbol) || string.IsNullOrEmpty(Text)) {
            return "MISSINGNO";
        }
        return string.Format("{0}-{1}", Symbol[0], Text);
    }
}