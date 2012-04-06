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

namespace Cards.Silverlight
{
    public partial class DeckShape : UserControl
    {
        #region Static Properties

        private static List<DeckShape> playingDecks = new List<DeckShape>();

        #endregion

        #region Properties

        private double cardSpacerX = 0;
        public double CardSpacerX
        {
            get { return cardSpacerX; }
            set { cardSpacerX = value; }
        }

        private double cardSpacerY = 0;
        public double CardSpacerY
        {
            get { return cardSpacerY; }
            set { cardSpacerY = value; }
        }

        private int maxCardsSpace = 0;
        public int MaxCardsSpace
        {
            get
            {
                return maxCardsSpace;
            }
            set
            {
                maxCardsSpace = value;
            }
        }

        private double nextCardX = 0;
        public double NextCardX
        {
            get { return nextCardX; }
            set { nextCardX = value; }
        }

        private double nextCardY = 0;
        public double NextCardY
        {
            get { return nextCardY; }
            set { nextCardY = value; }
        }
        
        private Deck deck = null;
        public Deck Deck
        {
            get
            {
                return deck;
            }
            set
            {
                deck = value;
                UpdateCardShapes();
            }
        }

        #endregion

        #region Events

        public event MouseEventHandler DeckMouseEnter;
        public event MouseEventHandler DeckMouseLeave;
        public event MouseEventHandler DeckMouseMove;
        public event MouseButtonEventHandler DeckMouseLeftButtonDown;
        public event MouseButtonEventHandler DeckMouseLeftButtonUp;

        #endregion

        #region Constructors

        public DeckShape()
        {
            InitializeComponent();

            playingDecks.Add(this);
            rectBorder.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Methods

        public Point GetNextCardPosition()
        {
            Point p = new Point(NextCardX, NextCardY);

            NextCardX += CardSpacerX;
            NextCardY += CardSpacerY;

            return p;
        }

        /// <summary>
        /// Recalculate all the card positions and animate them to the new positions
        /// Should be called when the deck change its cards order or count
        /// </summary>
        public void UpdateCardShapes()
        {            
            GameShape game = GameShape.GetGameShape(Deck.Game);
            NextCardX = 0;
            NextCardY = 0;

            double localCardSpacerX = CardSpacerX;
            double localCardSpacerY = CardSpacerY;

            if ((MaxCardsSpace > 0) && (Deck.Cards.Count > MaxCardsSpace))
            {
                //override the spacers values to squeez cards
                localCardSpacerX = (CardSpacerX * MaxCardsSpace) / Deck.Cards.Count;
                localCardSpacerY = (CardSpacerY * MaxCardsSpace) / Deck.Cards.Count;
            }

            //Create the animation to move the card from one deck to the other
            Duration duration = new Duration(TimeSpan.FromSeconds(0.2));

            Storyboard sb = new Storyboard();
            sb.Duration = duration;

            //Loop on the Deck Cards (not playing cards)
            for (int i = 0; i < Deck.Cards.Count; i++)
            {
                //Get the card shape
                CardShape cardShape = game.GetCardShape(Deck.Cards[i]);
                if (cardShape.Parent != this.LayoutRoot)
                {
                    LayoutRoot.Children.Add(cardShape);
                }

                //Animate card to the its correct position
                DoubleAnimation xAnim = new DoubleAnimation();
                xAnim.Duration = duration;
                sb.Children.Add(xAnim);
                Storyboard.SetTarget(xAnim, cardShape);
                Storyboard.SetTargetProperty(xAnim, new PropertyPath("(Canvas.Left)"));
                xAnim.To = NextCardX;

                DoubleAnimation yAnim = new DoubleAnimation();
                yAnim.Duration = duration;
                sb.Children.Add(yAnim);
                Storyboard.SetTarget(yAnim, cardShape);
                Storyboard.SetTargetProperty(yAnim, new PropertyPath("(Canvas.Top)"));
                yAnim.To = NextCardY;

                Canvas.SetZIndex(cardShape, i);

                //Increment the next card position
                NextCardX += localCardSpacerX;
                NextCardY += localCardSpacerY;
            }

            if(LayoutRoot.Resources.Contains("sb"))
                LayoutRoot.Resources.Remove("sb");

            LayoutRoot.Resources.Add("sb",sb);
            sb.Begin();
        }

        #endregion

        #region DeckShape Event Handlers

        private void rectBorderBack_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Deck.Enabled)
                rectBorder.Visibility = Visibility.Visible;

            if (DeckMouseEnter != null)
                DeckMouseEnter(this, e);
        }

        private void rectBorderBack_MouseLeave(object sender, MouseEventArgs e)
        {
            if (Deck.Enabled)
                rectBorder.Visibility = Visibility.Collapsed;

            if (DeckMouseLeave != null)
                DeckMouseLeave(this, e);
        }

        private void rectBorderBack_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DeckMouseLeftButtonDown != null)
                DeckMouseLeftButtonDown(this, e);
        }

        private void rectBorderBack_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DeckMouseLeftButtonUp != null)
                DeckMouseLeftButtonUp(this, e);
        }

        private void rectBorderBack_MouseMove(object sender, MouseEventArgs e)
        {
            if (DeckMouseMove != null)
                DeckMouseMove(this, e);
        }

        #endregion
    }
}
