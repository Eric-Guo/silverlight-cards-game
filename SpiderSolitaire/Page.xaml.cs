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

namespace SpiderSolitaire
{
    public partial class Page : UserControl
    {
        Deck dealer;
        List<Deck> rowDecks;
        List<Deck> stackDecks;
        int level = 1;

        public Page()
        {
            // Required to initialize variables
            InitializeComponent();
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            gameShape.Game = new Game();
            //Dealer
            dealer = new Deck(2, 13, gameShape.Game);
            
            dealer.FlipAllCards();
            dealer.EnableAllCards(false);
            dealer.MakeAllCardsDragable(false);
            dealer.Enabled = false;
            Dealer.Deck = dealer;
            gameShape.DeckShapes.Add(Dealer);

            //NewGame();

            gameShape.CardMouseLeftButtonDown += new MouseButtonEventHandler(gameShape_CardMouseLeftButtonDown);
            gameShape.CardDrag += new CardDragEventHandler(gameShape_CardDrag);
        }

        public void NewGame()
        {
            dealer.Shuffle(5);

            rowDecks = new List<Deck>();
            stackDecks = new List<Deck>();

            //Row Decks
            for (int i = 0; i < 10; i++)
            {
                Deck deck = new Deck(gameShape.Game);
                rowDecks.Add(deck);

                DeckShape deckShape = new DeckShape();
                gameShape.DeckShapes.Add(deckShape);
                deckShape.CardSpacerY = 15;
                deckShape.MaxCardsSpace = 15;
                deckShape.Deck = deck;

                this.LayoutRoot.Children.Add(deckShape);
                Canvas.SetLeft(deckShape, 10 + (i * 75));
                Canvas.SetTop(deckShape, 10);

                if(i<4)
                    dealer.Draw(deck, 6);
                else
                    dealer.Draw(deck, 5);

                deck.TopCard.Visible = true;
                deck.TopCard.Enabled = true;
                deck.TopCard.IsDragable = true;
            }

            //Stack Decks
            for (int i = 0; i < 8; i++)
            {
                Deck deck = new Deck(gameShape.Game);
                stackDecks.Add(deck);

                DeckShape deckShape = new DeckShape();
                gameShape.DeckShapes.Add(deckShape);
                deckShape.CardSpacerY = 1;
                deckShape.MaxCardsSpace = 10;
                deckShape.Deck = deck;

                deck.Enabled = false;

                this.LayoutRoot.Children.Add(deckShape);
                Canvas.SetLeft(deckShape, 10 + (i * 75));
                Canvas.SetTop(deckShape, 350);
            }

            dealer.TopCard.Enabled = true;
            dealer.TopCard.IsDragable = false;
        }

        protected void gameShape_CardMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Card card = ((CardShape)sender).Card;

            if (card.Enabled)
            {
                if (card.Deck == dealer)
                {
                    //Deal
                    for (int i = 0; i < 10; i++)
                    {
                        if (rowDecks[i].TopCard != null)
                            rowDecks[i].TopCard.Enabled = false;

                        dealer.Draw(rowDecks[i], 1);

                        rowDecks[i].TopCard.Visible = true;
                        rowDecks[i].TopCard.Enabled = true;
                        rowDecks[i].TopCard.IsDragable = true;
                    }

                    if (dealer.TopCard != null)
                    {
                        dealer.TopCard.Enabled = true;
                    }

                    CalculateDragableCards();
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
                    if (((newDeckShape.Deck.TopCard != null) && (cardShape.Card.Number + 1 == newDeckShape.Deck.TopCard.Number)) ||
                        (newDeckShape.Deck.TopCard == null))
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

                        CalculateDragableCards();
                    }
                }
            }
        }

        /// <summary>
        /// Check for correct running cards and adjust dragable cards and checks for completed full stacks
        /// </summary>
        protected void CalculateDragableCards()
        {
            //Loop on each stack
            for (int i = 0; i < 10; i++)
            {
                //Loop on each card from bottom up, and make it enabled until a wrong card placement or an invisible card occur
                bool correctOrder = true;
                int correctCount = 0;
                for (int j = rowDecks[i].Cards.Count - 1; j >= 0; j--)
                {
                    rowDecks[i].Cards[j].Enabled = correctOrder;
                    rowDecks[i].Cards[j].IsDragable = correctOrder;
                    
                    if (correctOrder) //If we still in a correct order state check for the next card
                    {
                        if ((j != 0) && ((rowDecks[i].Cards[j - 1].Visible == false) || (IsWrongPlacement(rowDecks[i].Cards[j], rowDecks[i].Cards[j - 1]))))
                        {
                            correctOrder = false;
                        }
                        correctCount++;
                    }
                }

                if (correctCount == 13)
                {
                    //A full stack is complete, move it to the first empty stack
                    for (int k = 0; k < 8; k++)
                    {
                        if (stackDecks[k].Cards.Count == 0)
                        {
                            rowDecks[i].Draw(stackDecks[k], 13);
                            stackDecks[k].EnableAllCards(false);
                            stackDecks[k].MakeAllCardsDragable(false);

                            //Flip the remaining card
                            if (rowDecks[i].TopCard != null)
                            {
                                rowDecks[i].TopCard.Visible = true;
                            }

                            //All Stacks are full, do win animation
                            if (k == 7)
                                DoWinAnimation();

                            //Doing this to repeat the calculation on the current row deck as it has been changed
                            i--;

                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Return false if the two cards are not in the same suit and in ordered numbers
        /// </summary>
        /// <param name="card1"></param>
        /// <param name="card2"></param>
        /// <returns></returns>
        public bool IsWrongPlacement(Card card1, Card card2)
        {
            if ((card1.Suit == card2.Suit) && (card1.Number + 1 == card2.Number))
                return false;
            else
                return true;
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

        private void L1_Click(object sender, RoutedEventArgs e)
        {
            level = 1;

            for (int i = 0; i < dealer.Cards.Count; i++)
            {
                dealer.Cards[i].Suit = CardSuit.Spades;
            }

            Options.Visibility = Visibility.Collapsed;
            NewGame();
        }

        private void L2_Click(object sender, RoutedEventArgs e)
        {
            level = 2;

            for (int i = 0; i < dealer.Cards.Count; i++)
            {
                if (dealer.Cards[i].Color == CardColor.Black)
                    dealer.Cards[i].Suit = CardSuit.Spades;
                else
                    dealer.Cards[i].Suit = CardSuit.Hearts;
            }

            Options.Visibility = Visibility.Collapsed;
            NewGame();
        }

        private void L3_Click(object sender, RoutedEventArgs e)
        {
            level = 3;

            int diamonds = 0;
            for (int i = 0; i < dealer.Cards.Count; i++)
            {
                if (dealer.Cards[i].Suit == CardSuit.Diamonds)
                {
                    diamonds++;
                    if (diamonds <= 13)
                        dealer.Cards[i].Suit = CardSuit.Spades;
                    else
                        dealer.Cards[i].Suit = CardSuit.Hearts;
                }                
            }

            Options.Visibility = Visibility.Collapsed;
            NewGame();
        }

        private void L4_Click(object sender, RoutedEventArgs e)
        {
            level = 4;

            Options.Visibility = Visibility.Collapsed;
            NewGame();
        }
    }
}
