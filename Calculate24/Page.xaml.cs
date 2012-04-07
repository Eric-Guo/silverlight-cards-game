using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Cards.Data;
using Cards.Silverlight;

namespace Calculate24
{
    public partial class MainPage : UserControl
    {
        Deck dealer;

        List<Deck> stackDecks;
        
        public MainPage()
        {
            InitializeComponent();
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            NewGame();
            gameShape.CardMouseLeftButtonDown += new MouseButtonEventHandler(gameShape_CardMouseLeftButtonDown);
        }

        protected void gameShape_CardMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Card card = ((CardShape)sender).Card;

            if (card.Enabled)
            {
                if (card.Deck == dealer)
                {
                    //Deal one card
                    card.Visible = true;
                    card.Deck = ReturnLeastCardDeck();
                }
            }
        }

        private Deck ReturnLeastCardDeck()
        {
            Deck least = stackDecks[0];
            for (int i = 0; i < 10; i++)
            {
                foreach (Deck dk in stackDecks)
                {
                    if (dk.Cards.Count == i)
                        return dk;
                }
            }

            return least;
        }

        private void NewGame()
        {
            stackDecks = new List<Deck>();

            //Dealer
            dealer = new Deck(1, 10, gameShape.Game);
            dealer.Shuffle(5);
            dealer.FlipAllCards();
            dealer.EnableAllCards(true);
            dealer.MakeAllCardsDragable(true);
            dealer.Enabled = true;
            Dealer.Deck = dealer;
            gameShape.DeckShapes.Add(Dealer);

            //Stack Decks
            for (int i = 0; i < 4; i++)
            {
                Deck deck = new Deck(gameShape.Game);
                stackDecks.Add(deck);

                DeckShape deckShape = new DeckShape();
                gameShape.DeckShapes.Add(deckShape);
                deckShape.CardSpacerY = 1;
                deckShape.MaxCardsSpace = 10;
                deckShape.Deck = deck;

                this.LayoutRoot.Children.Add(deckShape);
                if (i % 2 == 0)
                {
                    Canvas.SetLeft(deckShape, 200 + (i * 100));
                    Canvas.SetTop(deckShape, 24);
                }
                else
                {
                    Canvas.SetLeft(deckShape, 200 + (i - 1) * 100);
                    Canvas.SetTop(deckShape, 24+220);
                }
            }

        }
    }
}
