using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Kinect;

namespace Final.models
{
  class UserSkeleton
  {
    KinectSensor kinect;
    Canvas canvas;
    Grid grid;
    Joint leftHand;
    Joint rightHand;
    List<SkeletonPoint> prevLeftHandPositions = new List<SkeletonPoint>();
    List<SkeletonPoint> prevRightHandPositions = new List<SkeletonPoint>();

    public UserSkeleton(Skeleton skeleton, 
                        KinectSensor kinect,
                        Canvas canvas)
    {
      this.kinect = kinect;
      this.canvas = canvas;
      this.grid = new Grid();

      canvas.Children.Add(grid);

      UpdateBy(skeleton);
    }

    public void Destroy()
    {
      canvas.Children.Remove(grid);
      grid.Children.Clear();
      this.kinect = null;
      this.canvas = null;
      this.grid = null;
      this.prevLeftHandPositions = null;
      this.prevRightHandPositions = null;
    }

    public void UpdateBy(Skeleton skeleton)
    {
      leftHand = skeleton.Joints[JointType.HandLeft];
      rightHand = skeleton.Joints[JointType.HandRight];
      // draw(skeleton);
    }

    public ColorImagePoint LeftHandPoint()
    {
      SkeletonPoint point = leftHand.Position;
      return kinect.CoordinateMapper.MapSkeletonPointToColorPoint(
        skeletonPoint: point,
        colorImageFormat: kinect.ColorStream.Format
      );
    }

    public ColorImagePoint RightHandPoint()
    {
      SkeletonPoint point = rightHand.Position;
      return kinect.CoordinateMapper.MapSkeletonPointToColorPoint(
        skeletonPoint: point,
        colorImageFormat: kinect.ColorStream.Format
      );
    }

    void Draw(Skeleton skeleton)
    {
      grid.Children.Clear();
      skeleton.Joints.ToList().ForEach(joint => Draw(joint));
    }

    void Draw(Joint joint)
    {
      if (joint.TrackingState != JointTrackingState.Tracked) return;

      const int R = 5;
      var point = kinect.CoordinateMapper
        .MapSkeletonPointToColorPoint(
          skeletonPoint: joint.Position,
          colorImageFormat: kinect.ColorStream.Format
        );
      var left = (int)Scale(
        value: point.X, 
        source: kinect.ColorStream.FrameWidth, 
        dest: canvas.ActualWidth
      );
      var top = (int)Scale(
        value: point.Y,
        source: kinect.ColorStream.FrameHeight,
        dest: canvas.ActualHeight
      );
      grid.Children.Add(new Ellipse
      {
        Fill = new SolidColorBrush(Colors.Yellow),
        Margin = new Thickness(
          left: left - R,
          top: top - R,
          right: 0,
          bottom: 0
        ),
        Width = R * 2,
        Height = R * 2,
      });
    }

    double Scale(double value, double source, double dest)
    {
      return (value * dest) / source;
    }
  }
}
