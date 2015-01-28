using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Final.controllers
{
  class GameController
  {
    Random random = new System.Random();
    Grid initialWindow;
    Button startButton;
    Button stopButton;
    KinectController kinectController;
    bool playable = false;

    public bool IsPlayable()
    {
      return playable;
    }

    public void Setup(Grid initialWindow,
                      Button startButton,
                      Button stopButton,
                      KinectController kinectController)
    {
      this.initialWindow = initialWindow;
      this.startButton = startButton;
      this.stopButton = stopButton;
      this.kinectController = kinectController;
      startButton.Click += HandleStartButtonClick;
    }

    public void Stop()
    {
      startButton.Click -= HandleStartButtonClick;
      this.initialWindow = null;
      this.startButton = null;
      this.stopButton = null;
      this.kinectController = null;
    }
    
    public void Refresh()
    {
      playable = false;
      DoubleAnimation animation = new DoubleAnimation();
      animation.From = 1;
      animation.To = 0;
      animation.Duration = new Duration(TimeSpan.FromSeconds(.5));
      animation.Completed += (a, b) => SetStopButtonRandomly();
      stopButton.BeginAnimation(Button.OpacityProperty, animation);
    }

    void Start()
    {
      SetStopButtonRandomly();
    }

    void HandleStartButtonClick(object sender, RoutedEventArgs e)
    {
      DoubleAnimation animation = new DoubleAnimation();
      animation.From = 1;
      animation.To = 0;
      animation.Duration = new Duration(TimeSpan.FromSeconds(.5));
      animation.Completed += (a, b) => Start();
      initialWindow.BeginAnimation(Grid.OpacityProperty, animation);
    }

    void SetStopButtonRandomly()
    {
      stopButton.Margin = new Thickness
      {
        Top = CalcMarginTopOfStopButton(),
        Left = CalcMarginLeftOfStopButton()
      };
      DoubleAnimation animation = new DoubleAnimation();
      animation.From = 0;
      animation.To = 1;
      animation.Duration = new Duration(TimeSpan.FromSeconds(.5));
      animation.Completed += (a, b) => playable = true;
      stopButton.BeginAnimation(Button.OpacityProperty, animation);
    }

    double CalcMarginTopOfStopButton()
    {
      return random.NextDouble() * 300;
    }

    double CalcMarginLeftOfStopButton()
    {
      return random.NextDouble() * 500;
    }
  }
}
