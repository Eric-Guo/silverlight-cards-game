using System;
using System.Collections.Generic;
using System.Text;

namespace Cards.Data
{
    public class Deck
    {
        #region Properties

        private List<Card> cards = new List<Card>();
        public List<Card> Cards
        {
            get
            {
                return cards;
            }
        }

        private bool enabled = true;
        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;
            }
        }

        private Game game;
        public Game Game
        {
            get
            {
                return game;
            }
        }

        public Card TopCard
        {
            get
            {
                if (Cards.Count > 0)
                    return Cards[Cards.Count - 1];
                else
                    return null;
            }
        }

        public Card BottomCard
        {
            get
            {
                if (Cards.Count > 0)
                    return Cards[0];
                else
                    return null;
            }
        }

        public bool HasCards
        {
            get
            {
                if (Cards.Count > 0)
                    return true;
                else
                    return false;
            }
        }

        #endregion

        #region Events

        public event EventHandler SortChanged;

        #endregion

        #region Constructors

        public Deck(Game game)
        {
            this.game = game;
            this.game.Decks.Add(this);
        }

        public Deck(int numberOfDecks, int uptoNumber, Game game)
            : this(game)
        {
            for (int deck = 0; deck < numberOfDecks; deck++)
            {
                for (int suit = 1; suit <= 4; suit++)
                {
                    for (int number = 1; number <= uptoNumber; number++)
                    {
                        Cards.Add(new Card(number, (CardSuit)suit, this));
                    }
                }
            }

            Shuffle();
        }

        #endregion

        #region Get Methods

        public bool Has(int number, CardSuit suit)
        {
            return Has((CardRank)number, suit);
        }

        public bool Has(CardRank rank, CardSuit suit)
        {
            if (GetCard(rank, suit) != null)
                return true;
            else
                return false;
        }

        public Card GetCard(int number, CardSuit suit)
        {
            return GetCard((CardRank)number, suit);
        }

        public Card GetCard(CardRank rank, CardSuit suit)
        {
            foreach (Card card in Cards)
            {
                if ((card.Rank == rank) && (card.Suit == suit))
                    return card;
            }

            return null;
        }

        #endregion

        #region Sort Methods

        public void Shuffle()
        {
            Shuffle(1);
        }

        public void Shuffle(int times)
        {
            for (int time = 0; time < times; time++)
            {
                for (int i = 0; i < Cards.Count; i++)
                {
                    Cards[i].Shuffle();
                }
            }

            if (SortChanged != null)
                SortChanged(this, null);
        }

        public void Sort()
        {
            Cards.Sort(Game.cardSuitComparer);

            if (SortChanged != null)
                SortChanged(this, null);
        }

        #endregion

        #region Draw Cards Methods

        public void Draw(Deck toDeck, int count)
        {
            for (int i = 0; i < count; i++)
            {
                TopCard.Deck = toDeck;
            }
        }

        #endregion

        #region Generic Methods

        public void FlipAllCards()
        {
            for (int i = 0; i < Cards.Count; i++)
            {
                Cards[i].Visible = !Cards[i].Visible;
            }
        }

        public void EnableAllCards(bool enable)
        {
            for (int i = 0; i < Cards.Count; i++)
            {
                Cards[i].Enabled = enable;
            }
        }

        public void MakeAllCardsDragable(bool isDragable)
        {
            for (int i = 0; i < Cards.Count; i++)
            {
                Cards[i].IsDragable = isDragable;
            }
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();

            output.Append("[" + Environment.NewLine);

            for (int i = 0; i < Cards.Count; i++)
            {
                output.Append(Cards[i].ToString() + Environment.NewLine);
            }

            output.Append("]" + Environment.NewLine);

            return output.ToString();
        }

        #endregion
    }
}
