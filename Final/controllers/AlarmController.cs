using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Microsoft.Kinect;

namespace Final.controllers
{
  class AlarmController
  {
    static string hourFormat = @"^([0-1][0-9]|2[0-3]|\d)$";
    static string minuteFormat = @"^([0-5][0-9]|\d)$";

    Random random = new System.Random();
    Grid initialWindow;
    Grid inputGrid;
    TextBox hourInput;
    TextBox minuteInput;
    Button setButton;
    Button unsetButton;
    Button stopButton;
    KinectSensor kinect;
    KinectController kinectController;
    Timer timer;
    bool playable = false;
    bool hourValidity = false;
    bool minuteValidity = false;
    string validHour = "";
    string validMinute = "";
    int touchCount = 0;

    public bool IsPlayable()
    {
      return playable;
    }

    public void Setup(Grid initialWindow,
                      Grid inputGrid,
                      TextBox hourInput,
                      TextBox minuteInput,
                      Button setButton,
                      Button unsetButton,
                      Button stopButton,
                      KinectController kinectController)
    {
      this.initialWindow = initialWindow;
      this.inputGrid = inputGrid;
      this.hourInput = hourInput;
      this.minuteInput = minuteInput;
      this.setButton = setButton;
      this.unsetButton = unsetButton;
      this.stopButton = stopButton;
      this.kinect = kinectController.kinect;
      this.kinectController = kinectController;
      hourInput.TextChanged += HandleHourChange;
      minuteInput.TextChanged += HandleMinuteChange;
      setButton.Click += HandleSetButtonClick;
      unsetButton.Click += HandleUnsetButtonClick;
    }

    public void Dispose()
    {
      hourInput.TextChanged -= HandleHourChange;
      minuteInput.TextChanged -= HandleMinuteChange;
      setButton.Click -= HandleSetButtonClick;
      hourInput.Text = null;
      minuteInput.Text = null;
      hourInput = null;
      minuteInput = null;
      validHour = null;
      validMinute = null;
      initialWindow = null;
      hourInput = null;
      minuteInput = null;
      setButton = null;
      unsetButton = null;
      stopButton = null;
      kinect = null;
      kinectController = null;
      timer = null;
    }

    void Start()
    {
      touchCount = 0;
      DoubleAnimation animation = new DoubleAnimation();
      animation.From = 1;
      animation.To = 0;
      animation.Duration = new Duration(TimeSpan.FromSeconds(.5));
      animation.Completed += (a, b) => SetStopButtonRandomly();
      initialWindow.BeginAnimation(Grid.OpacityProperty, animation);
    }

    public void Refresh()
    {
      touchCount += 1;
      playable = false;
      DoubleAnimation animation = new DoubleAnimation();
      animation.From = 1;
      animation.To = 0;
      animation.Duration = new Duration(TimeSpan.FromSeconds(.5));
      if (touchCount < 10)
      {
        animation.Completed += (a, b) => SetStopButtonRandomly();
      }
      else
      {
        animation.Completed += (a, b) => Stop();
      }
      stopButton.BeginAnimation(Grid.OpacityProperty, animation);
    }

    public void Stop()
    {
      playable = false;
      hourValidity = false;
      minuteValidity = false;
      validHour = "";
      validMinute = "";
      hourInput.Text = "";
      minuteInput.Text = "";
      setButton.Visibility = Visibility.Visible;
      unsetButton.Visibility = Visibility.Collapsed;
      DoubleAnimation animation = new DoubleAnimation();
      animation.From = 0;
      animation.To = 1;
      animation.Duration = new Duration(TimeSpan.FromSeconds(.5));
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
      stopButton.BeginAnimation(Grid.OpacityProperty, animation);
    }

    double CalcMarginTopOfStopButton()
    {
      var availableHeight
        = kinect.ColorStream.FrameHeight - stopButton.ActualHeight;
      return random.NextDouble() * availableHeight;
    }

    double CalcMarginLeftOfStopButton()
    {
      var availableWidth
        = kinect.ColorStream.FrameWidth - stopButton.ActualWidth;
      return random.NextDouble() * availableWidth;
    }

    void WaitUntilSetTimeCome()
    {
      var now = DateTime.Now;
      if (now.Hour == int.Parse(validHour)
          && now.Minute == int.Parse(validMinute))
      {
        if (timer != null)
        {
          timer.Stop();
          timer = null;
        }
        Application.Current.Dispatcher.Invoke(new Action(() => Start()));
      }
      else if (timer == null)
      {
        timer = new Timer();
        timer.Interval = 1000;
        timer.Elapsed += (a, b) => WaitUntilSetTimeCome();
        timer.Start();
      }
    }

    void HandleSetButtonClick(object sender, RoutedEventArgs e)
    {
      if (hourValidity && minuteValidity)
      {
        setButton.Visibility = Visibility.Collapsed;
        unsetButton.Visibility = Visibility.Visible;
        WaitUntilSetTimeCome();
      }
    }

    void HandleUnsetButtonClick(object sender, RoutedEventArgs e)
    {
      if (hourValidity && minuteValidity)
      {
        setButton.Visibility = Visibility.Visible;
        unsetButton.Visibility = Visibility.Collapsed;
        if (timer != null)
        {
          timer.Stop();
          timer = null;
        }
      }
    }

    void HandleHourChange(object sender, TextChangedEventArgs e)
    {
      if (timer != null)
      {
        hourInput.Text = validHour;
        return;
      }
      var text = hourInput.Text;
      hourValidity = Regex.Match(text, hourFormat).Success;
      if (hourValidity)
      {
        validHour = text;
      }
      else if (text != "")
      {
        hourInput.Text = validHour;
      }
    }

    void HandleMinuteChange(object sender, TextChangedEventArgs e)
    {
      if (timer != null)
      {
        minuteInput.Text = validMinute;
        return;
      }
      var text = minuteInput.Text;
      minuteValidity = Regex.Match(text, minuteFormat).Success;
      if (minuteValidity)
      {
        validMinute = text;
      }
      else if (text != "")
      {
        minuteInput.Text = validMinute;
      }
    }
  }
}
