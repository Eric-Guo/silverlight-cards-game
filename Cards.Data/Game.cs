using System;
using System.Collections.Generic;
using System.Text;

namespace Cards.Data
{
    public class Game
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

        private List<Deck> decks = new List<Deck>();
        public List<Deck> Decks
        {
            get
            {
                return decks;
            }
        }

        internal Random random = new Random();
        internal CardSuitComparer cardSuitComparer = new CardSuitComparer();

        #endregion
    }
}
