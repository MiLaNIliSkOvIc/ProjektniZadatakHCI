using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ProjektniZadatakHCI
{
    public partial class MainWindow : Window
    {
        private GameLogic _gameLogic;
        private DispatcherTimer _timer;
        private int _timeElapsed;
        private int _moves;
        private int _score;

        public MainWindow()
        {
            InitializeComponent();

            _gameLogic = new GameLogic();
            _gameLogic.CardsUpdated += UpdateUI;
            _gameLogic.GameWon += EndGame;
            ElapsedTimeText.Text = "0 s";
            ScoreTextRight.Text = "0";
            MovesText.Text = "0";
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += (s, e) =>
            {
                _timeElapsed++;
                ElapsedTimeText.Text = $"{_timeElapsed} s";
            };

            this.SizeChanged += MainWindow_SizeChanged;
        }

        private void StartGame(int numberOfPairs)
        {
            CardPanel.Children.Clear();
            _moves = 0;
            _score = 0;
            UpdateScoreAndMoves();

            int totalCards = numberOfPairs * 2;
            int rows = (int)Math.Ceiling(Math.Sqrt(totalCards));
            int columns = (int)Math.Ceiling((double)totalCards / rows);
            CardPanel.Rows = rows;
            CardPanel.Columns = columns;

            var cards = _gameLogic.InitializeGame(numberOfPairs);

            foreach (var card in cards)
            {
                var button = new Button
                {
                    DataContext = card,
                    Margin = new Thickness(2),
                    Width = 50,
                    Height = 50,
                    Padding = new Thickness(0),
                };

                var grid = new Grid();

                var placeholderText = new TextBlock
                {
                    Text = "?",
                    FontSize = 20,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brushes.FloralWhite
                };

                grid.Children.Add(placeholderText);

                button.Content = grid;

                
                button.Click += (s, e) =>
                {
                    var btn = s as Button;
                    var model = btn.DataContext as CardModel;

                    if (!model.IsFlipped && !model.IsMatched)
                    {
                        
                        var rotateTransform = new RotateTransform
                        {
                            CenterX = btn.Width / 2,
                            CenterY = btn.Height / 2,
                            Angle = 0
                        };

                        btn.RenderTransform = rotateTransform;

                        
                        var flipAnimation = new DoubleAnimation
                        {
                            From = 0,
                            To = 360,
                            Duration = TimeSpan.FromMilliseconds(400),
                            AutoReverse = false
                        };

                        flipAnimation.Completed += (sender, args) =>
                        {
                            model.IsFlipped = true;
                            _gameLogic.FlipCard(model);
                            _moves++;
                            UpdateScoreAndMoves();
                            UpdateUI();
                        };

                        rotateTransform.BeginAnimation(RotateTransform.AngleProperty, flipAnimation);
                    }
                };

                CardPanel.Children.Add(button);
            }

            RevealAllCardsThenHide();
            _timer.Start();
        }

        private async void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedLevel = (LevelSelector.SelectedItem as ComboBoxItem)?.Content.ToString();
            _timer.Stop();
            _timeElapsed = 0;
            ElapsedTimeText.Text = "0 s";

            int pairs = selectedLevel switch
            {
                "Easy" => 6,
                "Medium" => 8,
                "Hard" => 15,
                _ => 6
            };
            CountdownText.Visibility = Visibility.Visible;

            CardPanel.Visibility = Visibility.Collapsed;
            for (int i = 3; i > 0; i--)
            {
                CountdownText.Text = "Game starts in " + i.ToString();
                await Task.Delay(1000);
            }

            CountdownText.Visibility = Visibility.Collapsed;
            CardPanel.Visibility = Visibility.Visible;
            StartGame(pairs);
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int totalCards = CardPanel.Children.Count;

            if (totalCards == 0) return;

            int rows = (int)Math.Ceiling(Math.Sqrt(totalCards));
            int columns = (int)Math.Ceiling((double)totalCards / rows);

            CardPanel.Rows = rows;
            CardPanel.Columns = columns;
        }

        private async void RevealAllCardsThenHide()
        {
            foreach (Button button in CardPanel.Children)
            {
                var card = button.DataContext as CardModel;
                if (card != null && !card.IsMatched)
                {
                    card.IsFlipped = true;
                }
            }

            UpdateUI();
            await Task.Delay(3000);

            foreach (Button button in CardPanel.Children)
            {
                var card = button.DataContext as CardModel;
                if (card != null && !card.IsMatched)
                {
                    card.IsFlipped = false;
                }
            }

            UpdateUI();
        }

        private void UpdateUI()
        {
            foreach (Button button in CardPanel.Children)
            {
                var card = button.DataContext as CardModel;
                var grid = button.Content as Grid;

                if (grid != null)
                {
                    var placeholderText = grid.Children[0] as TextBlock;

                    if (card != null && placeholderText != null)
                    {
                        placeholderText.Text = card.IsFlipped ? card.Id.ToString() : "?";
                        button.IsEnabled = !card.IsMatched;

                        if (card.IsFlipped)
                        {
                            button.BorderBrush = Brushes.DarkGray;
                            button.BorderThickness = new Thickness(2.5);
                        }
                        else
                        {
                            button.BorderBrush = Brushes.DarkSlateBlue;
                            button.BorderThickness = new Thickness(2);
                        }

                        if (card.IsMatched && !card.ScoreCounted)
                        {
                            _score += 10;
                            card.ScoreCounted = true;
                            UpdateScoreAndMoves();
                        }
                    }
                }
            }
        }

        private void UpdateScoreAndMoves()
        {
            ScoreTextRight.Text = _score.ToString();
            MovesText.Text = _moves.ToString();
        }

        private void EndGame()
        {
            CardPanel.Visibility = Visibility.Collapsed;
            CountdownText.Visibility = Visibility.Visible;

            CountdownText.Text = $"Congratulations, you've scored {_score+20} points.";
            _timer.Stop();
        }

        private void RestartGame_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            _timeElapsed = 0;
            ElapsedTimeText.Text = "0 s";
            CardPanel.Visibility = Visibility.Visible;
            CountdownText.Visibility = Visibility.Collapsed;
            StartGame(15);
            _timer.Start();
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            _score = _score - 20;
            UpdateScoreAndMoves();
            RevealAllCardsThenHide();
        }

        private void ExitGame_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
