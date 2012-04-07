using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using Cards.Data;
using System.Windows.Browser;

namespace Cards.Silverlight
{
    public delegate void CardDragEventHandler(CardShape cardShape, DeckShape oldDeckShape, DeckShape newDeckShape);

	public partial class CardShape : UserControl
    {
        #region Constants

        public const double CardOrigX = 76;
        public const double CardOrigY = 61; 
        public const double CardWidth = 72*2;
        public const double CardHeight = 97*2;
        public const double CardWidthRect = 73*2;
        public const double CardHeightRect = 98*2;

        #endregion

        #region Properties

        private Card card = null;
        public Card Card
        {
            get
            {
                return card;
            }
            set
            {
                if (card != null)
                {
                    card.VisibleChanged -= new EventHandler(card_VisibleChanged);
                    card.DeckChanged -= new EventHandler(card_DeckChanged);
                }

                card = value;

                //Handle Card Events
                card.VisibleChanged += new EventHandler(card_VisibleChanged);
                card.DeckChanged += new EventHandler(card_DeckChanged);

                //Adjust the clipping of the cards image to reflect the current card
                double x = 0;
                double y = 0;

                if (Card.Visible)
                {
                    //Define the card position in the cards image
                    if (Card.Number <= 10)
                    {
                        x = (Card.Number - 1) % 2;
                        y = (Card.Number - 1) / 2;

                        switch (Card.Suit)
                        {
                            case CardSuit.Spades:
                                x += 6;
                                break;
                            case CardSuit.Hearts:
                                x += 0;
                                break;
                            case CardSuit.Diamonds:
                                x += 2;
                                break;
                            case CardSuit.Clubs:
                                x += 4;
                                break;
                        }
                    }
                    else
                    {
                        int number = (Card.Number-11);
                        switch (Card.Suit)
                        {
                            case CardSuit.Spades:
                                number += 6;
                                break;
                            case CardSuit.Hearts:
                                number += 9;
                                break;
                            case CardSuit.Diamonds:
                                number += 3;
                                break;
                            case CardSuit.Clubs:
                                number += 0;
                                break;
                        }

                        x = (number % 2) + 8;
                        y = number / 2;
                    }
                }
                else
                {
                    //Show back of the card
                    x = 8;
                    y = 6;
                }

                ((RectangleGeometry)imgCard.Clip).Rect = new Rect(x * CardWidthRect + CardOrigX, y * CardHeightRect + CardOrigY, CardWidth, CardHeight);
                foreach (Transform tran in ((TransformGroup)imgCard.RenderTransform).Children)
                {
                    if (tran.GetType() == typeof(TranslateTransform))
                    {
                        tran.SetValue(TranslateTransform.XProperty, -x * CardWidthRect - CardOrigX);
                        tran.SetValue(TranslateTransform.YProperty, -y * CardHeightRect - CardOrigY);
                    }
                }
                imgCard.RenderTransformOrigin = new Point(0.05 + (x * 0.1), 0.08 + (y * 0.166666));
            }
        }

        #endregion

        #region Events

        public event MouseEventHandler CardMouseEnter;
        public event MouseEventHandler CardMouseLeave;
        public event MouseEventHandler CardMouseMove;
        public event MouseButtonEventHandler CardMouseLeftButtonDown;
        public event MouseButtonEventHandler CardMouseLeftButtonUp;
        public event CardDragEventHandler CardDrag;

        #endregion

        #region Private Variables

        private Point oldMousePos;
        private bool isDrag = false;

        #endregion

        #region Constructors

        public CardShape()
        {
            // Required to initialize variables
            InitializeComponent();
            
            rectBorder.Visibility = Visibility.Collapsed;
            aniFlipStart.Completed += new EventHandler(aniFlipStart_Completed);
        }

        #endregion

        #region Card Event Handlers

        private void card_VisibleChanged(object sender, EventArgs e)
        {            
            aniFlipStart.Begin();
        }
        
        private void card_DeckChanged(object sender, EventArgs e)
        {
            //Get Decks Shapes
            GameShape gameShape = GameShape.GetGameShape(this.Card.Deck.Game);
            DeckShape oldDeck = (DeckShape)((Canvas)this.Parent).Parent;
            DeckShape newDeck = gameShape.GetDeckShape(this.Card.Deck);

            //Get the animation positions in relation to the world (background)
            double startX = Canvas.GetLeft(oldDeck) + Canvas.GetLeft(this);
            double startY = Canvas.GetTop(oldDeck) + Canvas.GetTop(this);

            double endX = Canvas.GetLeft(newDeck);
            double endY = Canvas.GetTop(newDeck);

            //Change the card parent
            ((Canvas)this.Parent).Children.Remove(this);
            newDeck.LayoutRoot.Children.Add(this);

            //Maintain the same card position relative to the new parent
            Canvas.SetLeft(this, startX-endX);
            Canvas.SetTop(this, startY-endY);

            //Reorder decks
            oldDeck.UpdateCardShapes();
            newDeck.UpdateCardShapes();
        }

        #endregion

        #region CardShape Event Handlers

        protected void aniFlipStart_Completed(object sender, EventArgs e)
        {
            this.Card = this.Card;
            aniFlipEnd.Begin();
        }

        private void imgCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Card.IsDragable)
            {
                imgCard.CaptureMouse();
                isDrag = true;
                oldMousePos = e.GetPosition(LayoutRoot);
            }

            if (CardMouseLeftButtonDown != null)
                CardMouseLeftButtonDown(this, e);            
        }

        private void imgCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDrag)
            {
                imgCard.ReleaseMouseCapture();
                isDrag = false;
                
                //Get which deck this card was dropped into
                GameShape gameShape = GameShape.GetGameShape(this.Card.Deck.Game);
                DeckShape oldDeckShape=gameShape.GetDeckShape(this.Card.Deck);
                DeckShape nearestDeckShape = null;
                double nearestDistance = double.MaxValue;

                foreach (DeckShape deck in gameShape.DeckShapes)
                {
                    if (deck.Deck.Enabled)
                    {
                        double dx = Canvas.GetLeft(deck) - (Canvas.GetLeft(this) + Canvas.GetLeft((UIElement)((Canvas)this.Parent).Parent));
                        double dy = Canvas.GetTop(deck) - (Canvas.GetTop(this) + Canvas.GetTop((UIElement)((Canvas)this.Parent).Parent));
                        double distance = Math.Sqrt(dx * dx + dy * dy);

                        if (distance < nearestDistance)
                        {
                            nearestDistance = distance;
                            nearestDeckShape = deck;
                        }
                    }
                }

                if ((nearestDeckShape != null) && (CardDrag != null))
                {
                    CardDrag(this, gameShape.GetDeckShape(this.Card.Deck), nearestDeckShape);
                }
                
                gameShape.GetDeckShape(this.Card.Deck).UpdateCardShapes();
                Canvas.SetZIndex(oldDeckShape, 0);
            }

            if (CardMouseLeftButtonUp != null)
                CardMouseLeftButtonUp(this, e);
        }

        private void imgCard_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrag)
            {
                Point newMousePos = e.GetPosition(LayoutRoot);

                double dx = newMousePos.X - oldMousePos.X;
                double dy = newMousePos.Y - oldMousePos.Y;

                GameShape gameShape = GameShape.GetGameShape(this.Card.Deck.Game);

                for (int i = this.Card.Deck.Cards.IndexOf(this.Card); i < this.Card.Deck.Cards.Count; i++)
                {
                    CardShape cardShape = gameShape.GetCardShape(this.Card.Deck.Cards[i]);
                    Canvas.SetLeft(cardShape, Canvas.GetLeft(cardShape) + dx);
                    Canvas.SetTop(cardShape, Canvas.GetTop(cardShape) + dy);
                    Canvas.SetZIndex(gameShape.GetDeckShape(this.Card.Deck), 100);
                }
            }

            if (CardMouseMove != null)
                CardMouseMove(this, e);
        }

        private void imgCard_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Card.Enabled)
                rectBorder.Visibility = Visibility.Visible;

            if (CardMouseEnter != null)
                CardMouseEnter(this, e);
        }

        private void imgCard_MouseLeave(object sender, MouseEventArgs e)
        {
            if (Card.Enabled)
                rectBorder.Visibility = Visibility.Collapsed;

            if (CardMouseLeave != null)
                CardMouseLeave(this, e);
        }

        #endregion

        #region Methods

        public void Rotate(double speedRatio)
        {
            animRotate.SpeedRatio = speedRatio;
            animRotate.Begin();
        }

        #endregion
    }
}