using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

using Final.controllers;

namespace Final
{
  public partial class MainWindow : Window
  {
    KinectController kinectController;
    AlarmController alarmController;

    public MainWindow()
    {
      InitializeComponent();

      KinectController.EnsureKinectConnection();

      kinectController = new KinectController();
      alarmController = new AlarmController();

      kinectController.Setup(
        rgbImage: rgbImage,
        stopButton: stopButton,
        skeletonCanvas: skeletonCanvas,
        alarmController: alarmController
      );

      alarmController.Setup(
        initialWindow: initialWindow,
        inputGrid: inputGrid,
        hourInput: hourInput,
        minuteInput: minuteInput,
        setButton: setButton,
        unsetButton: unsetButton,
        stopButton: stopButton,
        kinectController: kinectController
      );
    }

    public void Close(object sender, CancelEventArgs e)
    {
      kinectController.Dispose();
      alarmController.Dispose();
      kinectController = null;
      alarmController = null;
      rgbImage.Source = null;
      skeletonCanvas.Children.Clear();
    }
  }
}
