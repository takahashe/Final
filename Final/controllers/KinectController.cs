using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using Final.models;

namespace Final.controllers
{
  class KinectController
  {
    public static void EnsureKinectConnection()
    {
      if (KinectSensor.KinectSensors.Count == 0)
      {
        throw new Exception("Kinectを接続してください");
      }
    }

    KinectSensor kinect;
    GameController gameController;
    Image rgbImage;
    Button stopButton;
    Canvas skeletonCanvas;
    Dictionary<int, UserSkeleton> userSkeletonDic
      = new Dictionary<int, UserSkeleton>();

    public void Setup(Image rgbImage,
                      Button stopButton,
                      Canvas skeletonCanvas,
                      GameController gameController)
    {
      this.rgbImage = rgbImage;
      this.stopButton = stopButton;
      this.skeletonCanvas = skeletonCanvas;
      this.gameController = gameController;
      this.kinect = KinectSensor.KinectSensors.First();

      kinect.ColorStream.Enable();
      kinect.SkeletonStream.Enable();
      kinect.ColorFrameReady += HandleColorFrameReady;
      kinect.SkeletonFrameReady += HandleSkeletonFrameReady;
      kinect.Start();
    }

    public void Stop()
    {
      if (!kinect.IsRunning) return;

      kinect.ColorFrameReady -= HandleColorFrameReady;
      kinect.SkeletonFrameReady -= HandleSkeletonFrameReady;
      kinect.Stop();
      kinect.Dispose();
      kinect = null;
      gameController = null;
      rgbImage = null;
      skeletonCanvas = null;
      userSkeletonDic = null;
    }

    void HandleColorFrameReady(object sender,
                               ColorImageFrameReadyEventArgs e)
    {
      try
      {
        using (ColorImageFrame frame = e.OpenColorImageFrame())
        {
          if (frame == null) return;

          DrawRGBImageWith(frame);
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }

    void HandleSkeletonFrameReady(object sender,
                                  SkeletonFrameReadyEventArgs e)
    {
      try
      {
        using (SkeletonFrame frame = e.OpenSkeletonFrame())
        {
          if (frame == null) return;

          Skeleton[] skeletons = new Skeleton[frame.SkeletonArrayLength];
          frame.CopySkeletonDataTo(skeletons);
          var skeletonsList = skeletons.ToList();
          skeletonsList.ForEach(skeleton => BuildOrUpdateUserSkeletonBy(skeleton));
          userSkeletonDic.Keys.ToList()
            .FindAll(key => !skeletonsList.Exists(s => s.TrackingId == key))
            .ForEach(key => RemoveUserSkeletonAt(key));
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }

    void DrawRGBImageWith(ColorImageFrame frame)
    {
      rgbImage.Source = BitmapSource.Create(
        pixelWidth: frame.Width,
        pixelHeight: frame.Height,
        dpiX: 96,
        dpiY: 96,
        pixelFormat: PixelFormats.Bgr32,
        palette: null,
        pixels: frame.GetRawPixelData(),
        stride: frame.Width * frame.BytesPerPixel
      );
    }

    void BuildOrUpdateUserSkeletonBy(Skeleton skeleton)
    {
      if (skeleton.TrackingState != SkeletonTrackingState.Tracked) return;

      UserSkeleton userSkeleton;

      if (userSkeletonDic.ContainsKey(skeleton.TrackingId)) {
        userSkeleton = userSkeletonDic[skeleton.TrackingId];
        userSkeleton.UpdateBy(skeleton);
      } else {
        userSkeleton = new UserSkeleton(
          skeleton: skeleton,
          kinect: kinect,
          canvas: skeletonCanvas
        );
        userSkeletonDic[skeleton.TrackingId] = userSkeleton;
      }

      if (!gameController.IsPlayable()) return;

      var leftHandPoint = userSkeleton.LeftHandPoint();
      if (stopButton.Margin.Left <= leftHandPoint.X
          && leftHandPoint.X <= stopButton.Margin.Left + stopButton.ActualWidth
          && stopButton.Margin.Top <= leftHandPoint.Y
          && leftHandPoint.Y <= stopButton.Margin.Top + stopButton.ActualHeight)
      {
        gameController.Refresh();
        return;
      }

      var rightHandPoint = userSkeleton.RightHandPoint();
      if (stopButton.Margin.Left <= rightHandPoint.X
          && rightHandPoint.X <= stopButton.Margin.Left + stopButton.ActualWidth
          && stopButton.Margin.Top <= rightHandPoint.Y
          && rightHandPoint.Y <= stopButton.Margin.Top + stopButton.ActualHeight)
      {
        gameController.Refresh();
      }
    }

    void RemoveUserSkeletonAt(int key)
    {
      var userSkeleton = userSkeletonDic[key];
      userSkeleton.Destroy();
      userSkeletonDic.Remove(key);
    }
  }
}
