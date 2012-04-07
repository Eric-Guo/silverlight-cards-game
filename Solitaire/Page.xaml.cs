using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Cards.Data;
using Cards.Silverlight;
using System.Windows.Browser;

namespace Solitaire
{
    public partial class Page : UserControl
    {
        Deck dealer;
        Deck ground;
        List<Deck> rowDecks;
        List<Deck> stackDecks;

        public Page()
        {
            // Required to initialize variables
            InitializeComponent();
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            NewGame();

            gameShape.CardMouseLeftButtonDown += new MouseButtonEventHandler(gameShape_CardMouseLeftButtonDown);
            gameShape.CardDrag += new CardDragEventHandler(gameShape_CardDrag);
        }

        public void NewGame()
        {
            rowDecks = new List<Deck>();
            stackDecks = new List<Deck>();

            gameShape.Game = new Game();

            //Dealer
            dealer = new Deck(1, 13, gameShape.Game);
            dealer.Shuffle(5);
            dealer.FlipAllCards();
            dealer.EnableAllCards(false);
            dealer.MakeAllCardsDragable(false);
            dealer.Enabled = false;
            Dealer.Deck = dealer;
            gameShape.DeckShapes.Add(Dealer);
            Dealer.DeckMouseLeftButtonDown += new MouseButtonEventHandler(Dealer_DeckMouseLeftButtonDown);

            //Ground
            ground = new Deck(gameShape.Game);
            Ground.Deck = ground;
            Ground.UpdateCardShapes();
            gameShape.DeckShapes.Add(Ground);
            
            //Row Decks
            for (int i = 0; i < 7; i++)
            {
                Deck deck = new Deck(gameShape.Game);
                rowDecks.Add(deck);

                DeckShape deckShape = new DeckShape();
                gameShape.DeckShapes.Add(deckShape);
                deckShape.CardSpacerY = 20;
                deckShape.MaxCardsSpace = 10;
                deckShape.Deck = deck;

                this.LayoutRoot.Children.Add(deckShape);
                Canvas.SetLeft(deckShape, 25 + (i * 85));
                Canvas.SetTop(deckShape, 150);

                dealer.Draw(deck, i + 1);

                deck.TopCard.Visible = true;
                deck.TopCard.Enabled = true;
                deck.TopCard.IsDragable = true;
            }

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
                Canvas.SetLeft(deckShape, 280 + (i * 85));
                Canvas.SetTop(deckShape, 24);
            }

            dealer.TopCard.Enabled = true;
            dealer.TopCard.IsDragable = false;
        }

        protected void Dealer_DeckMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //If dealer is empty
            if (dealer.TopCard == null)
            {
                //Reload
                while (ground.TopCard != null)
                {
                    ground.TopCard.Visible = false;
                    ground.TopCard.IsDragable = false;
                    ground.TopCard.Enabled = true;
                    ground.TopCard.Deck = dealer;
                }
            }
        }

        protected void gameShape_CardMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Card card = ((CardShape)sender).Card;

            if (card.Enabled)
            {
                if (card.Deck == dealer)
                {
                    //Deal one card
                    if (ground.TopCard != null)
                    {
                        ground.TopCard.IsDragable = false;
                        ground.TopCard.Enabled = false;
                    }

                    card.Visible = true;
                    card.IsDragable = true;
                    card.Enabled = true;
                    card.Deck = ground;

                    if (dealer.TopCard != null)
                    {
                        dealer.TopCard.Enabled = true;
                    }
                    else
                    {
                        dealer.Enabled = true;
                    }
                }
            }
        }

        protected void gameShape_CardDrag(CardShape cardShape, DeckShape oldDeckShape, DeckShape newDeckShape)
        {
            if (oldDeckShape.Deck != newDeckShape.Deck)
            {
                //To Row Decks
                if (rowDecks.Contains(newDeckShape.Deck))
                {
                    //Color/Rank Rules
                    if (((newDeckShape.Deck.TopCard != null) && (cardShape.Card.Color != newDeckShape.Deck.TopCard.Color) && (cardShape.Card.Number + 1 == newDeckShape.Deck.TopCard.Number)) ||
                        ((newDeckShape.Deck.TopCard == null) && (cardShape.Card.Rank == CardRank.King)))
                    {
                        //Move the current card with all cards after it to the new deck
                        for (int i = oldDeckShape.Deck.Cards.IndexOf(cardShape.Card); i < oldDeckShape.Deck.Cards.Count; i++)
                        {
                            oldDeckShape.Deck.Cards[i].Deck = newDeckShape.Deck;
                            i--;
                        }

                        //Flip the first remaining card in the old deck
                        if (oldDeckShape.Deck.TopCard != null)
                        {
                            oldDeckShape.Deck.TopCard.Visible = true;
                            oldDeckShape.Deck.TopCard.Enabled = true;
                            oldDeckShape.Deck.TopCard.IsDragable = true;
                        }
                    }
                }

                //To Stack Decks
                if (stackDecks.Contains(newDeckShape.Deck))
                {
                    //Must be dragging one card only
                    if (oldDeckShape.Deck.Cards.IndexOf(cardShape.Card) == oldDeckShape.Deck.Cards.Count - 1)
                    {
                        if (((newDeckShape.Deck.TopCard == null) && (cardShape.Card.Number == 1)) ||
                            ((newDeckShape.Deck.TopCard != null) && (cardShape.Card.Suit == newDeckShape.Deck.TopCard.Suit) && (cardShape.Card.Number - 1 == newDeckShape.Deck.TopCard.Number)))
                        {
                            //Move card to stack
                            cardShape.Card.Deck = newDeckShape.Deck;

                            //Flip the first remaining card in the old deck
                            if (oldDeckShape.Deck.TopCard != null)
                            {
                                oldDeckShape.Deck.TopCard.Visible = true;
                                oldDeckShape.Deck.TopCard.Enabled = true;
                                oldDeckShape.Deck.TopCard.IsDragable = true;
                            }

                            //Check for winning condition
                            bool win = true;
                            for (int i = 0; i < 4; i++)
                            {
                                if ((stackDecks[i].TopCard == null) || (stackDecks[i].TopCard.Rank != CardRank.King))
                                    win = false;
                            }

                            if (win)
                            {
                                DoWinAnimation();
                            }
                        }
                    }
                }
            }
        }

        protected void DoWinAnimation()
        {
            //Do final animation
            Random random = new Random();
            Duration duration = new Duration(TimeSpan.FromSeconds(4));

            Storyboard sb = new Storyboard();
            sb.Duration = duration;

            for (int i = 0; i < gameShape.CardShapes.Count; i++)
            {
                //Get the animation positions in relation to the world (background)
                int startX = (int)(Canvas.GetLeft(gameShape.GetDeckShape(gameShape.CardShapes[i].Card.Deck)) + Canvas.GetLeft(gameShape.CardShapes[i]));
                int startY = (int)(Canvas.GetTop(gameShape.GetDeckShape(gameShape.CardShapes[i].Card.Deck)) + Canvas.GetTop(gameShape.CardShapes[i]));

                //Animate card to a random position
                DoubleAnimation xAnim = new DoubleAnimation();
                xAnim.Duration = duration;
                sb.Children.Add(xAnim);
                Storyboard.SetTarget(xAnim, gameShape.CardShapes[i]);
                Storyboard.SetTargetProperty(xAnim, new PropertyPath("(Canvas.Left)"));
                xAnim.To = random.Next(50-startX, 500 - startX);
                
                DoubleAnimation yAnim = new DoubleAnimation();
                yAnim.Duration = duration;
                sb.Children.Add(yAnim);
                Storyboard.SetTarget(yAnim, gameShape.CardShapes[i]);
                Storyboard.SetTargetProperty(yAnim, new PropertyPath("(Canvas.Top)"));
                yAnim.To = random.Next(50-startY, 380 - startY);
                
                //Rotate card
                gameShape.CardShapes[i].Card.Enabled = false;
                gameShape.CardShapes[i].Card.IsDragable = false;
                gameShape.CardShapes[i].Card.Visible = true;
                gameShape.CardShapes[i].Rotate(random.NextDouble()+0.2);
            }

            if (LayoutRoot.Resources.Contains("sb"))
                LayoutRoot.Resources.Remove("sb");
                        
            LayoutRoot.Resources.Add("sb",sb);
            sb.Begin();
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ClearGame();
            NewGame();
        }

        private void ClearGame()
        {
            this.LayoutRoot.Children.Remove(gameShape);
            foreach (DeckShape deck in gameShape.DeckShapes)
            {
                if((deck!=Dealer)&&(deck!=Ground)&&(this.LayoutRoot.Children.Contains(deck)))
                    this.LayoutRoot.Children.Remove(deck);
            }
            
            gameShape = new GameShape();
            this.LayoutRoot.Children.Add(gameShape);
            Canvas.SetZIndex(gameShape, -1);
        }
    }
}
