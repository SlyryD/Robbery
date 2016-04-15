using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DeckStandard : CardDeck
{
	public CardAtlas Atlas;
	public CardStock Stock;
	
	public override void Initialize()
	{
		if (Atlas == null) { Debug.LogError("CardAtlas is not initialized."); }
		if (Stock == null) { Debug.LogError("CardStock is not initialized."); }

		Debug.Log("Atlas = "+Atlas.name);
		string [] suits = new string[]{"Heart","Spade","Diamond","Club"};
		string [] prefixes = new string[]{"H-","S-","D-","C-"};
		List<CardDef> defs = new List<CardDef>();
		for (int i=0; i<4; ++i)
		{
			//int ii = i*13;
			string symbol = suits[i];
			defs.Add( new CardDef(Atlas,Stock,"A",symbol,1) );
			defs.Add( new CardDef(Atlas,Stock,"2",symbol,2) );
			defs.Add( new CardDef(Atlas,Stock,"3",symbol,3) );
			defs.Add( new CardDef(Atlas,Stock,"4",symbol,4) );
			defs.Add( new CardDef(Atlas,Stock,"5",symbol,5) );
			defs.Add( new CardDef(Atlas,Stock,"6",symbol,6) );
			defs.Add( new CardDef(Atlas,Stock,"7",symbol,7) );
			defs.Add( new CardDef(Atlas,Stock,"8",symbol,8) );
			defs.Add( new CardDef(Atlas,Stock,"9",symbol,9) );
			defs.Add( new CardDef(Atlas,Stock,"10",symbol,10) );
			string prefix = prefixes[i];
			CardDef jj = new CardDef(Atlas,Stock,"J",symbol,0);
			jj.Image = prefix+"Jack";
			defs.Add(jj);
			CardDef qq = new CardDef(Atlas,Stock,"Q",symbol,0);
			qq.Image = prefix+"Queen";
			defs.Add( qq );
			CardDef kk = new CardDef(Atlas,Stock,"K",symbol,0);
			kk.Image = prefix+"King";
			defs.Add( kk );
		}
		
		m_itemList = new DeckItem[52];
		for (int i=0; i<defs.Count; ++i)
		{
			DeckItem item = new DeckItem();
			item.Count = 1;
			item.Card = defs[i];
			m_itemList[i] = item;
		}
	}
}
