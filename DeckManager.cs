using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{

    public class Card
    {
        public string type;
        public string subType;
        public int index;
        public string cardName;
        public string ability;
        public int abilityType; //0 is play on cast..    1 is Interupt.. 2 is hold meaning the ability can be played
        //sometime after the card is played.. 3 is constant which can be activated once every turn ... 4 is combo Interupt first then constant
        public int abilityLength; // 1 and 2 with 3 being infinite
        public int attack, defence;

        //zero mean nothing does not exist
        public bool precast; //before cast the player must select something
        public bool etb;//the card has an ability when entering the battlefield
        public int[] etbList;

    }
    public static DeckManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    public Dictionary<int, Card> cardLibrary;

    public void InitTestDeck()
    {
        int[] deck = new int[48];
        deck[0] = 8;
        for(int i = 0; i < 47; i++)
        {
            deck[i] = i;
        }
        GameManager.instance.customDeck.Add(0, deck);
    }

    public Color ReturnCardColor(int value,int sub)
    {
        string subType = cardLibrary[value].subType;
        string type = cardLibrary[value].type;
        Color32 first = new Color32(0, 0, 0, 0);
        Color32 second = new Color32(0, 0, 0, 0);
        if (subType.Contains("Explorer"))
        {
            first = new Color32(69, 108, 162, 255);
            second = new Color32(146, 250, 255, 255);
        }
        if (subType=="Politician")
        {
            first = new Color32(252, 41, 48, 255);
            second = new Color32(185, 199, 250, 255);
        }
        if (subType=="Athlete")
        {
            first = new Color32(108, 98, 120, 255);
            second = new Color32(0, 163, 204, 255);
        }
        if (subType=="Conqueror")
        {
            first = new Color32(245, 116, 12, 255);
            second = new Color32(255, 168, 81, 255);
        }
        if (type == "Land")
        {
            first = new Color32(52, 187, 4, 255);
            second = new Color32(157, 253, 86, 255);
        }
        if (subType == "Spy")
        {
            first = new Color32(106, 0, 0, 255);
            second = new Color32(196, 177, 101, 255);
        }
        if (subType=="Documents")
        {
            first = new Color32(214, 208, 208, 255);
            second = new Color32(172, 171, 171, 255);
        }
        if (subType=="Businessman")
        {
            first = new Color32(65, 82, 48, 255);
            second = new Color32(63, 225, 135, 255);
        }
        if (subType=="Organization")
        {
            first = new Color32(147, 147, 84, 255);
            second = new Color32(222, 222, 165, 255);
        }
        if (subType=="Scientist")
        {
            first = new Color32(196, 118, 46, 255);
            second = new Color32(225, 199, 92, 255);
        }
        if (subType=="Army")
        {
            first = new Color32(132, 87, 22, 255);
            second = new Color32(216, 184, 82, 255);
        }
        if (subType=="Artist")
        {
            first = new Color32(178, 178, 178, 255);
            second = new Color32(69, 69, 69, 255);
        }
        if (subType=="Economist")
        {
            first = new Color32(65, 82, 48, 255);
            second = new Color32(63, 225, 135, 255);
        }
        if (subType=="Knowledge")
        {
            first = new Color32(83, 137, 121, 255);
            second = new Color32(198, 203, 192, 255);
        }
        if (subType=="Activist")
        {
            first = new Color32(58, 56, 96, 255);
            second = new Color32(121, 172, 167, 255);
        }
        if (subType=="Technology")
        {
            first = new Color32(84, 225, 34, 255);
            second = new Color32(126, 205, 199, 255);
        }
        if (subType=="Leader")
        {
            first = new Color32(196, 118, 46, 255);
            second = new Color32(224, 207, 138, 255);
        }
        if (subType.Contains("Outlaw"))
        {
            first = new Color32(196, 118, 46, 255);
            second = new Color32(194, 175, 98, 255);
        }
        if (subType=="Location")
        {
            first = new Color32(69, 108, 102, 255);
            second = new Color32(146, 250, 255, 255);
        }
        if (subType.Contains("Spiritual"))
        {
            first = new Color32(125, 126, 120, 255);
            second = new Color32(202, 203, 195, 255);
        }
        if (subType=="Warrior")
        {
            first = new Color32(132, 87, 22, 255);
            second = new Color32(216, 184, 82, 255);
        }
        if (subType=="Event")
        {
            first = new Color32(69, 108, 162, 255);
            second = new Color32(146, 250, 255, 255);
        }
        if (subType=="Author")
        {
            first = new Color32(0, 0, 0, 255);
            second = new Color32(167, 161, 161, 255);
        }
        if (subType=="Philosophers")
        {
            first = new Color32(136, 136, 136, 255);
            second = new Color32(253, 248, 86, 255);
        }
        if (subType == "Inventor")
        {
            first = new Color32(196, 118, 46, 255);
            second = new Color32(225, 199, 92, 255);
        }
        if (subType == "Entertainer")
        {
            first = new Color32(196, 118, 46, 255);
            second = new Color32(218, 197, 111, 255);
        }

        if (sub == 0)
        {
            return first;
        }
        else
        {
            return second;
        }
    }
    public void InitCards()
    {

        cardLibrary = new Dictionary<int, Card>();
        Card card = new Card();
        int i = 0;
        //0
        card.cardName = "Aaron Burr";
        card.index = i;
        card.type = "Character";
        card.subType = "Politician";
        card.ability = "Choose a character an opponent controls. If the chosen character has a lower or equal Strength value than Aaron Burr, discard that card, otherwise discard Aaron Burr.";
        card.abilityType = 0;
        card.abilityLength = 1;
        card.attack = 100;
        card.defence = 120;
        card.precast = true;
        card.etb = true;
        cardLibrary.Add(i, card);
        i++;
        //1
        card = new Card();
        card.cardName = "Akashi Shiganosuke";
        card.index = i;
        card.type = "Character";
        card.subType = "Athlete";
        card.ability = "Increase your Morale by 200.";
        card.abilityType = 0;
        card.abilityLength = 1;
        card.attack = 40;
        card.defence = 30;
        card.etb = true;
        card.etbList = new int[2] { 1, 200 }; 
        cardLibrary.Add(i, card);
        i++;
        //2
        card = new Card();
        card.cardName = "Alexander the Great";
        card.index = i;
        card.type = "Character";
        card.subType = "Conqueror";
        card.ability = "During the first two turns of play, increase this lands Strength by 500.";
        card.abilityType = 3;
        card.abilityLength = 2;
        card.attack = 1000;
        card.defence = 900;
        cardLibrary.Add(i, card);
        i++;
        //3
        card = new Card();
        card.cardName = "Amelia Earhart";
        card.index = i;
        card.type = "Character";
        card.subType = "Explorer";
        card.ability = "When attacked by another explorer, increase this lands Defense by 400.";
        card.abilityType = 3;
        card.abilityLength = 3;
        card.attack = 100;
        card.defence = 200;
        cardLibrary.Add(i, card);
        i++;
        //4
        card = new Card();
        card.cardName = "Argentina";
        card.index = i;
        card.type = "Land";
        card.subType = "South America";
        card.ability = "Discard a character: Increase moral by 300 and Strength by 300";
        card.abilityType = 2;
        card.abilityLength = 1;
        card.attack = 0;
        card.defence = 0;
        cardLibrary.Add(i, card);
        i++;
        //5
        card = new Card();
        card.cardName = "James Armistead Lafayette";
        card.index = i;
        card.type = "Character";
        card.subType = "Spy";
        card.ability = "INTERRUPT:???";
        card.abilityType = 1;
        card.abilityLength = 1;
        card.attack = 80;
        card.defence = 80;
        cardLibrary.Add(i, card);
        i++;
        //6
        card = new Card();
        card.cardName = "Bill of Rights";
        card.index = i;
        card.type = "Active";
        card.subType = "Documents";
        card.ability = "INTERRUPT: Increase your Moral by 200. CONSTANT: Increase all your Lands Defense by 300.";
        card.abilityType = 4;
        card.abilityLength = 3;
        card.attack = 0;
        card.defence = 0;
        card.etb = true;
        card.etbList = new int[2] { 1, 200 };
        cardLibrary.Add(i, card);
        i++;
        //7
        card = new Card();
        card.cardName = "Andrew Carnegie";
        card.index = i;
        card.type = "Character";
        card.subType = "Businessman";
        card.ability = "Double the ability of any card in your active area.";
        card.abilityType = 2;
        card.abilityLength = 1;
        card.attack = 160;
        card.defence = 120;
        cardLibrary.Add(i, card);
        i++;
        //8
        card = new Card();
        card.cardName = "Central Intelligence Agency";
        card.index = i;
        card.type = "Active";
        card.subType = "Organization";
        card.ability = "INTERRUPT: You have 10 seconds to discard any non-Land card an opponents controls, or discard this card.";
        card.abilityType = 1;
        card.abilityLength = 1;
        card.attack = 0;
        card.defence = 0;
        cardLibrary.Add(i, card);
        i++;
        // 9
        card = new Card();
        card.cardName = "Charles Robert Darwin";
        card.index = i;
        card.type = "Character";
        card.subType = "Scientist";
        card.ability = "Discover a new Land from an opponents' Land deck";
        card.abilityType = 0;
        card.abilityLength = 1;
        card.attack = 20;
        card.defence = 60;
        cardLibrary.Add(i, card);
        i++;
        //10
        card = new Card();
        card.cardName = "Chinese Warriors";
        card.index = i;
        card.type = "Character";
        card.subType = "Army";
        card.ability = "If you win an attack, you may choose any card in your discard pile and return it to your hand.";
        card.abilityType = 1;
        card.abilityLength = 1;
        card.attack = 3000;
        card.defence = 3000;
        cardLibrary.Add(i, card);
        i++;
        //11
        card = new Card();
        card.cardName = "Claude Monet";
        card.index = i;
        card.type = "Character";
        card.subType = "Artist";
        card.ability = "Whenever an opponents Moral is increased by an Artists, increase your Moral by the same amount.";
        card.abilityType = 3;
        card.abilityLength = 3;
        card.attack = 20;
        card.defence = 40;
        cardLibrary.Add(i, card);
        i++;
        //12
        card = new Card();
        card.cardName = "David Ricardo";
        card.index = i;
        card.type = "Character";
        card.subType = "Economist";
        card.ability = "Increase you Morale by 200. If an opponents controls Karl Marx, that opponent loses 300 Morale.";
        card.abilityType = 0;
        card.abilityLength = 1;
        card.attack = 100;
        card.defence = 60;
        card.etb = true;
        card.etbList = new int[2] { 1, 200 };
        cardLibrary.Add(i, card);
        i++;
        //13
        card = new Card();
        card.cardName = "Egypt";
        card.index = i;
        card.type = "Land";
        card.subType = "Africa/Asia";
        card.ability = "If this Land is occupied by a Pharaoh, increase the lands Strength by 500.";
        card.abilityType = 3;
        card.abilityLength = 3;
        card.attack = 20;
        card.defence = 40;
        cardLibrary.Add(i, card);
        i++;
        //14
        card = new Card();
        card.cardName = "English Language";
        card.index = i;
        card.type = "Active";
        card.subType = "Knowledge";
        card.ability = "Choose any player: You may look at that players hand";
        card.abilityType = 2;
        card.abilityLength = 2;
        card.attack = 0;
        card.defence = 0;
        cardLibrary.Add(i, card);
        i++;
        //15
        card = new Card();
        card.cardName = "Florence Nightingale";
        card.index = i;
        card.type = "Character";
        card.subType = "Activist";
        card.ability = "You may return one Character from your discard pile to your hand.";
        card.abilityType = 2;
        card.abilityLength = 1;
        card.attack = 40;
        card.defence = 60;
        cardLibrary.Add(i, card);
        i++;
        //16
        card = new Card();
        card.cardName = "George Bass";
        card.index = i;
        card.type = "Character";
        card.subType = "Explorer-Sea";
        card.ability = "If you loose land, you may discover a new Land. If you do, move any Character you control to the new land; discard George Bass.";
        card.abilityType = 2;
        card.abilityLength = 1;
        card.attack = 300;
        card.defence = 200;
        cardLibrary.Add(i, card);
        i++;
        //17
        card = new Card();
        card.cardName = "Johannes Gutenberg";
        card.index = i;
        card.type = "Character";
        card.subType = "Inventor";
        card.ability = "Increase you Morale by 200. If the Printing Press is in play increase your Morale an additional 200";
        card.abilityType = 0;
        card.abilityLength = 1;
        card.attack = 40;
        card.defence = 60;
        card.etb = true;
        card.etbList = new int[2] { 1, 200 };
        cardLibrary.Add(i, card);
        i++;
        //18
        card = new Card();
        card.cardName = "Invention of the Automobile";
        card.index = i;
        card.type = "Active";
        card.subType = "Technology";
        card.ability = "ONCE: You may attack twice with any Land. CONSTANT: Increase all your Lands Strength by 200";
        card.abilityType = 3;
        card.abilityLength = 3;
        card.attack = 0;
        card.defence = 0;
        cardLibrary.Add(i, card);
        i++;
        //19
        card = new Card();
        card.cardName = "Invention of the Printing Press";
        card.index = i;
        card.type = "Active";
        card.subType = "Technology";
        card.ability = "Increase you Morale by 200. If the Johannes Gutenberg is in play increase your Morale an additional 200";
        card.abilityType = 0;
        card.abilityLength = 1;
        card.attack = 0;
        card.defence = 0;
        card.etb = true;
        card.etbList = new int[2] { 1, 200 };
        cardLibrary.Add(i, card);
        i++;
        //20
        card = new Card();
        card.cardName = "Isabella Baumfree";
        card.index = i;
        card.type = "Character";
        card.subType = "Activist";
        card.ability = "If your Moral is above 500. You moral cannot go lower than 500.";
        card.abilityType = 3;
        card.abilityLength = 3;
        card.attack = 20;
        card.defence = 20;
        cardLibrary.Add(i, card);
        i++;
        //21
        card = new Card();
        card.cardName = "James Madison";
        card.index = i;
        card.type = "Character";
        card.subType = "Leader";
        card.ability = "When played inscrease your Morale by 300. HOLD: If you successfully defend against a 'British Emprie' card, increase your Morale by 200.";
        card.abilityType = 2;
        card.abilityLength = 1;
        card.attack = 400;
        card.defence = 500;
        card.etb = true;
        card.etbList = new int[2] { 1, 300 };
        cardLibrary.Add(i, card);
        i++;
        //22
        card = new Card();
        card.cardName = "John Cabot";
        card.index = i;
        card.type = "Character";
        card.subType = "Explorer-Sea";
        card.ability = "If the Land discovered by John Cabot is Canadian inscrease your Moral by 100. If the Land is an Asian Land increase your Moral by 200.";
        card.abilityType = 0;
        card.abilityLength = 1;
        card.attack = 200;
        card.defence = 400;
        cardLibrary.Add(i, card);
        i++;
        //23
        card = new Card();
        card.cardName = "King Ferdinand and Queen Isabella";
        card.index = i;
        card.type = "Character";
        card.subType = "Leader";
        card.ability = "If this Land looses an attack do not discard any characters. Discard this character card if this Land looses an attack twice in one turn.";
        card.abilityType = 2;
        card.abilityLength = 1;
        card.attack = 400;
        card.defence = 500;
        cardLibrary.Add(i, card);
        i++;
        //24
        card = new Card();
        card.cardName = "King Louis XIV";
        card.index = i;
        card.type = "Character";
        card.subType = "Conqueror";
        card.ability = "Before his first battle, a Leader from the Land he is attacking is discarded.";
        card.abilityType = 2;
        card.abilityLength = 1;
        card.attack = 800;
        card.defence = 700;
        cardLibrary.Add(i, card);
        i++;
        //25
        card = new Card();
        card.cardName = "Ramses the Great";
        card.index = i;
        card.type = "Character";
        card.subType = "Conqueror";
        card.ability = "If your opponent igves you a Character, then you will not attack for hte rest of that turn.";
        card.abilityType = 2;
        card.abilityLength = 2;
        card.attack = 1000;
        card.defence = 900;
        cardLibrary.Add(i, card);
        i++;
        //26
        card = new Card();
        card.cardName = "Lester Gillis";
        card.index = i;
        card.type = "Character";
        card.subType = "Outlaw/Mobster";
        card.ability = "Decrease your opponent's Morale by 400, Discard this card after two rounds";
        card.abilityType = 0;
        card.abilityLength = 1;
        card.attack = 700;
        card.defence = 200;
        card.etb = true;
        card.etbList = new int[2] { 2, -400 };
        cardLibrary.Add(i, card);
        i++;
        //27
        card = new Card();
        card.cardName = "Lewis Latimer";
        card.index = i;
        card.type = "Character";
        card.subType = "Inventor";
        card.ability = "Latimer drafts a copy of one of your opponents' technologies. You gain the same effects as that card.";
        card.abilityType = 2;
        card.abilityLength = 1;
        card.attack = 30;
        card.defence = 20;
        cardLibrary.Add(i, card);
        i++;
        //28
        card = new Card();
        card.cardName = "Lost Among the Pharaohs";
        card.index = i;
        card.type = "Active";
        card.subType = "Location";
        card.ability = "You can play any one card from you discard pile.";
        card.abilityType = 0;
        card.abilityLength = 1;
        card.attack = 0;
        card.defence = 0;
        cardLibrary.Add(i, card);
        i++;
        //29
        card = new Card();
        card.cardName = "Margaret Hughes";
        card.index = i;
        card.type = "Character";
        card.subType = "Entertainer";
        card.ability = "Hughes give strength to women, For every woman in your Civilization, increase you Morale by 100.";
        card.abilityType = 0;
        card.abilityLength = 1;
        card.attack = 20;
        card.defence = 20;
        card.etb = true;
        cardLibrary.Add(i, card);
        i++;
        //30
        card = new Card();
        card.cardName = "Martin Luther";
        card.index = i;
        card.type = "Character";
        card.subType = "Spiritual Leader";
        card.ability = "Increase you Morale by 400. If any Pope is in play , he must be discarded immediately.";
        card.abilityType = 0;
        card.abilityLength = 1;
        card.attack = 20;
        card.defence = 20;
        card.etb = true;
        card.etbList = new int[2] { 1, 400 };
        cardLibrary.Add(i, card);
        i++;
        //31
        card = new Card();
        card.cardName = "Peter Salem";
        card.index = i;
        card.type = "Character";
        card.subType = "Warrior";
        card.ability = "Your opponent must discard one Character, when he attacks a Land Salem is defending.";
        card.abilityType = 2;
        card.abilityLength = 2;
        card.attack = 300;
        card.defence = 400;
        cardLibrary.Add(i, card);
        i++;
        //32
        card = new Card();
        card.cardName = "Philippines";
        card.index = i;
        card.type = "Land";
        card.subType = "South Pacific";
        card.ability = "Any one US territory may combine Strength with this Land, when defending against an opponent.";
        card.abilityType = 2;
        card.abilityLength = 2;
        card.attack = 0;
        card.defence = 0;
        cardLibrary.Add(i, card);
        i++;
        //33
        card = new Card();
        card.cardName = "Queen Ahhotep";
        card.index = i;
        card.type = "Character";
        card.subType = "Leader";
        card.ability = "Due to steady wise leadership, increase your Morale by 200.";
        card.abilityType = 0;
        card.abilityLength = 1;
        card.attack = 200;
        card.defence = 400;
        card.etb = true;
        card.etbList = new int[2] { 1, 200 };
        cardLibrary.Add(i, card);
        i++;
        //34
        card = new Card();
        card.cardName = "Renaissance";
        card.index = i;
        card.type = "Active";
        card.subType = "Event";
        card.ability = "Through enlightenment, increase your Morale by 400 points.";
        card.abilityType = 0;
        card.abilityLength = 1;
        card.attack = 0;
        card.defence = 0;
        card.etb = true;
        card.etbList = new int[2] { 1, 400 };
        cardLibrary.Add(i, card);
        i++;
        //35
        card = new Card();
        card.cardName = "Salem Witch Trials";
        card.index = i;
        card.type = "Active";
        card.subType = "Event";
        card.ability = "Decrease one of your opponent's Morale by 400 points.";
        card.abilityType = 0;
        card.abilityLength = 1;
        card.attack = 0;
        card.defence = 0;
        card.etb = true;
        card.etbList = new int[2] { 2, -400 };
        cardLibrary.Add(i, card);
        i++;
        //36
        card = new Card();
        card.cardName = "Samurai Warriors";
        card.index = i;
        card.type = "Character";
        card.subType = "Army";
        card.ability = "Every time you win a battle using the Samurai Warriors, you increase your Morale by 100 points.";
        card.abilityType = 3;
        card.abilityLength = 3;
        card.attack = 3000;
        card.defence = 2500;
        cardLibrary.Add(i, card);
        i++;
        //37
        card = new Card();
        card.cardName = "ScotLand";
        card.index = i;
        card.type = "Land";
        card.subType = "Europe";
        card.ability = "Double any Viking Character or Army's Attack and Defense, when defending Scotland.";
        card.abilityType = 3;
        card.abilityLength = 3;
        card.attack = 0;
        card.defence = 0;
        cardLibrary.Add(i, card);
        i++;
        //38
        card = new Card();
        card.cardName = "Sinking of the Titanic";
        card.index = i;
        card.type = "Active";
        card.subType = "Event";
        card.ability = "Interupt: Expluding Land, one of your opponent's cards in play(of your choice) must be discarded.";
        card.abilityType = 0;
        card.abilityLength = 1;
        card.attack = 0;
        card.defence = 0;
        cardLibrary.Add(i, card);
        i++;
        //39
        card = new Card();
        card.cardName = "Sir Walter Scott";
        card.index = i;
        card.type = "Character";
        card.subType = "Author";
        card.ability = "He may enter your deck and pull out any non-Character card and play it immediately.";
        card.abilityType = 0;
        card.abilityLength = 1;
        card.attack = 40;
        card.defence = 40;
        cardLibrary.Add(i, card);
        i++;
        //40
        card = new Card();
        card.cardName = "Socrates";
        card.index = i;
        card.type = "Character";
        card.subType = "Philosophers";
        card.ability = "Increase your Morale by 300.";
        card.abilityType = 0;
        card.abilityLength = 1;
        card.attack = 20;
        card.defence = 20;
        card.etb = true;
        card.etbList = new int[2] { 1, 300 };
        cardLibrary.Add(i, card);
        i++;
        //41
        card = new Card();
        card.cardName = "Kingdom of Thailand";
        card.index = i;
        card.type = "Land";
        card.subType = "Asia";
        card.ability = "No European Character cards may occupy this Land.";
        card.abilityType = 3;
        card.abilityLength = 3;
        card.attack = 0;
        card.defence = 0;
        cardLibrary.Add(i, card);
        i++;
        //42
        card = new Card();
        card.cardName = "The Persian Immortals";
        card.index = i;
        card.type = "Character";
        card.subType = "Army";
        card.ability = "If in an Asian Land, the Persian Immortals increase your Strength by 500.";
        card.abilityType = 3;
        card.abilityLength = 3;
        card.attack = 4000;
        card.defence = 3000;
        cardLibrary.Add(i, card);
        i++;
        //43
        card = new Card();
        card.cardName = "Theodore Roosevelt";
        card.index = i;
        card.type = "Character";
        card.subType = "Leader";
        card.ability = "All of your Businessmen, Economists, and Inventors in play must be sent to the discard pile, when Teddy is played.";
        card.abilityType = 0;
        card.abilityLength = 1;
        card.attack = 400;
        card.defence = 500;
        cardLibrary.Add(i, card);
        i++;
        //44
        card = new Card();
        card.cardName = "Tuskegee Airmen";
        card.index = i;
        card.type = "Character";
        card.subType = "Army";
        card.ability = "The Tuskegee Airmen give you and additional 500 Strength, during your first attack of any turn.";
        card.abilityType = 3;
        card.abilityLength = 3;
        card.attack = 4000;
        card.defence = 2000;
        cardLibrary.Add(i, card);
        i++;
        //45
        card = new Card();
        card.cardName = "USA - West Coast";
        card.index = i;
        card.type = "Land";
        card.subType = "North America";
        card.ability = "If you have at least two U.S. territories, increase Morale by 300 points.";
        card.abilityType = 0;
        card.abilityLength = 1;
        card.attack = 0;
        card.defence = 0;
        cardLibrary.Add(i, card);
        i++;
        //46
        card = new Card();
        card.cardName = "Vasco Nunez de Balboa";
        card.index = i;
        card.type = "Character";
        card.subType = "Explorer-Sea";
        card.ability = "AFter two tuns, Balboa may discover a second Land, and must be discarded immediately.";
        card.abilityType = 2;
        card.abilityLength = 1;
        card.attack = 600;
        card.defence = 700;
        cardLibrary.Add(i, card);
        i++;
        //47
        card = new Card();
        card.cardName = "Vietnam War";
        card.index = i;
        card.type = "Active";
        card.subType = "Event";
        card.ability = "Interrupt: Send any Army in play to defend any Land under attack. Transportation rules do not apply.";
        card.abilityType = 2;
        card.abilityLength = 2;
        card.attack = 0;
        card.defence = 0;
        cardLibrary.Add(i, card);
        i++;
    }

}
