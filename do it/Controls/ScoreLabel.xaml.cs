using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace DoIt.Controls
{
    /// <summary>
    /// Ein einfaches Label das den einen Punktestand anzeigt und Animationen für diesen bereitstellt.
    /// </summary>
    public partial class ScoreLabel : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// Der vom Label darzustellende Punktesstand.
        /// Score DepencyProperty Implementation.
        /// </summary>
        public static readonly DependencyProperty ScoreProperty =
             DependencyProperty.Register("Score", typeof(int),
             typeof(ScoreLabel), new PropertyMetadata(0, new PropertyChangedCallback(ScoreChanged)));

        /// <summary>
        /// Der animierte Score, der grade angezeigt wird.
        /// Score DepencyProperty Implementation.
        /// </summary>
        public static readonly DependencyProperty ActualScoreProperty =
             DependencyProperty.Register("ActualScore", typeof(double),
             typeof(ScoreLabel), new PropertyMetadata(0.0d, new PropertyChangedCallback(ActualScoreChanged)));

        public static readonly DependencyProperty ForegroundColorProperty =
             DependencyProperty.Register("ForegroundColor", typeof(Color),
             typeof(ScoreLabel), new PropertyMetadata((Color)App.Current.Resources["PhoneForegroundColor"]));

        public Color ForegroundColor
        {
            get { return (Color)GetValue(ForegroundColorProperty); }
            set { SetValue(ForegroundColorProperty, value); }
        }

        /// <summary>
        /// Der vom Label darzustellende Punktesstand.
        /// </summary>
        public int Score
        {
            get { return (int)GetValue(ScoreProperty); }
            set { SetValue(ScoreProperty, value); }
        }

        /// <summary>
        /// Der animierte Score, der grade angezeigt wird.
        /// </summary>
        public double ActualScore
        {
            get
            {
                return (double)GetValue(ActualScoreProperty);
            }
            set { SetValue(ActualScoreProperty, value); }
        }

        /// <summary>
        /// Formatierte Textform von ActualScore.
        /// </summary>
        public string ActualScoreText { get { return ((int)this.ActualScore).ToString(); } }

        /// <summary>
        /// Konstruktor.
        /// </summary>
        public ScoreLabel()
        {
            InitializeComponent();
        }

        internal void ScorePayedAnimation(int score, string bonus)
        {
            ScorePopupBox.Text = score.ToString();
            BonusTextBox.Text = bonus;

            var storyboard = (Storyboard)this.Resources["ScorePopupStoryboard"];
            storyboard.Begin();
        }

        #region ScoreChanged Animationen
        private static void ScoreChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var scoreLabel = (ScoreLabel)sender;

            if ((int)e.OldValue <= 0) // Beim ersten Wechsel Keine Animation abspielen.
            {
                scoreLabel.ActualScore = (int)e.NewValue;
            }
            else
            {
                Storyboard storyboard;
                if ((int)e.NewValue < (int)e.OldValue)
                    storyboard = (Storyboard)scoreLabel.Resources["ScoreDecreaseStoryBoard"];
                else if ((int)e.NewValue > (int)e.OldValue)
                    storyboard = (Storyboard)scoreLabel.Resources["ScoreIncreaseStoryBoard"];
                else
                    return;

                foreach (var animation in storyboard.Children)
                {
                    DoubleAnimation doubleAnimation = animation as DoubleAnimation;
                    ColorAnimationUsingKeyFrames colorAnimation = animation as ColorAnimationUsingKeyFrames;
                    if (doubleAnimation != null)
                    {
                        doubleAnimation.From = (int)e.OldValue;
                        doubleAnimation.To = (int)e.NewValue;
                    }
                    if (colorAnimation != null)
                    {
                        foreach (ColorKeyFrame frame in colorAnimation.KeyFrames)
                        {
                            if (frame.Value == Colors.Black)
                                frame.Value = scoreLabel.ForegroundColor;
                        }
                    }
                }

                //storyboard.SpeedRatio = 0.5;
                storyboard.Begin();
            }
        }

        private static void ActualScoreChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var scoreLabel = (ScoreLabel)sender;

            if (scoreLabel.PropertyChanged != null)
                scoreLabel.PropertyChanged(scoreLabel, new PropertyChangedEventArgs("ActualScoreText"));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
