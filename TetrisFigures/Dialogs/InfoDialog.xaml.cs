﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TetrisFigures.Interfaces;

namespace TetrisFigures.Dialogs
{
    /// <summary>
    /// Interaction logic for InfoDialog.xaml
    /// </summary>
    public partial class InfoDialog : Window
    {
        private int rotationAngle;
        private TetrisUserControl ctrl;
        private readonly DispatcherTimer tmr;
        private double left;
        private double bottom;
        private readonly TextBlock tblk;

        public InfoDialog()
        {
            InitializeComponent();
            rotationAngle = 0;
            tmr = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(2) };
            tblk = new TextBlock
            {
                Text = "Drops the figure ontop of the pile",
                Foreground = new SolidColorBrush(Colors.White),
                Visibility = Visibility.Hidden
            };
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            ctrl = new TetrisCapControl();
            ctrl.ChangeSize(30);
            ctrl.color = new SolidColorBrush(Colors.Red);
            Canvas.SetLeft(ctrl, ShowCase.ActualWidth / 2 - ctrl.Width / 2);
            Canvas.SetBottom(ctrl, ShowCase.ActualHeight / 2 - ctrl.Height / 2);
            left = Canvas.GetLeft(ctrl);
            bottom = Canvas.GetBottom(ctrl);
            _ = ShowCase.Children.Add(ctrl);
            Canvas.SetBottom(tblk, 10);
            Canvas.SetLeft(tblk, ActualWidth / 2 - tblk.Width / 2);
            _ = ShowCase.Children.Add(tblk);

            tmr.Tick += delegate
            {
                Canvas.SetLeft(ctrl, left);
                Canvas.SetBottom(ctrl, bottom);
                tblk.Visibility = Visibility.Hidden;
            };
            tmr.Start();
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            rotationAngle = (rotationAngle + 90) % 360;
            ctrl.LayoutTransform = new RotateTransform(rotationAngle);
        }

        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            Canvas.SetLeft(ctrl, left - 60);
        }

        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            Canvas.SetLeft(ctrl, left + 60);
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            Canvas.SetBottom(ctrl, bottom - 30);
        }

        private void SpaceButton_Click(object sender, RoutedEventArgs e)
        {
            tblk.Visibility = Visibility.Visible;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    UpButton_Click(sender, new RoutedEventArgs(e.RoutedEvent));
                    break;
                case Key.Down:
                    DownButton_Click(sender, new RoutedEventArgs(e.RoutedEvent));
                    break;
                case Key.Left:
                    LeftButton_Click(sender, new RoutedEventArgs(e.RoutedEvent));
                    break;
                case Key.Right:
                    RightButton_Click(sender, new RoutedEventArgs(e.RoutedEvent));
                    break;
                case Key.Space:
                    SpaceButton_Click(sender, new RoutedEventArgs(e.RoutedEvent));
                    break;
                default:
                    break;
            }
        }
    }
}
