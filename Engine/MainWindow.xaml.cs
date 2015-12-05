using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace CombineDesign
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		UILogic UserLogic = new UILogic();
		List<Image> Designs = new List<Image>();
		List<float> RotateValues = new List<float>();
		float currentRotate = 0.0f;
		List<Point> _scaleValues = new List<Point>(); //1, 1 = normal, -1, 1 = vert. flip, 1, -1 = hor. flip, -1, -1 = both flip
		Point MousePoint;
		List<Image> Selected = new List<Image>();
		Double CanvasLeft, CanvasTop;
		Boolean FirstTime = true;
		Boolean SnapToGrid = false;
		//Options OptionsWindow;
		Boolean ZoomToggle = false;
		About AboutWindow;
		float StickyValue = 10.0f;
		float NudgeValue = 1.0f;
		public Boolean IsWindowOpen = false;
		Single ZoomLevel = 0.25f;
		float GridSize = 1.0f;	 //in tenth of measurement
		List<string> _filenames = new List<string>();
		Confirm _box;
		bool _ctrlDown = false;
		float _negGridMod = 1.0f;
		double _widthBeforeRotate = 0.0;
		double _heightBeforeRotate = 0.0;
		Border _ImageBorder = new Border();
		bool _borderAdded = false;
		const float INCH = MM * 2.54f;
		const float MM = 10.37f;
		bool rotating = false;

		public MainWindow()
		{
			SplashScreen splash = new SplashScreen("Combine Design.png");
			splash.Show(true);
			System.Threading.Thread.Sleep(2500);
			InitializeComponent();

#if BASIC
			radioButton5x7Hoop.IsEnabled = false;
			radioButton7x5Hoop.IsEnabled = false;
			expanderOptions.IsEnabled = false;
			expanderOptions.Visibility = System.Windows.Visibility.Hidden;
			buttonRotateLeft.IsEnabled = false;
			buttonRotateRight.IsEnabled = false;
			radioButtonTopLeft.IsEnabled = false;
			radioButtonTopRight.IsEnabled = false;
			radioButtonBottomLeft.IsEnabled = false;
			radioButtonBottomRight.IsEnabled = false;
			textBoxCellSize.IsEnabled = false;
			textBoxStickinessAmount.IsEnabled = false;
			buttonMirrorHorizontal.IsEnabled = false;
			buttonMirrorVertical.IsEnabled = false;

			buttonRotateLeft.ToolTip += " Not available in Basic version";
			buttonRotateRight.ToolTip += " Not available in Basic version";
			buttonMirrorHorizontal.ToolTip += " Not available in Basic version";
			buttonMirrorVertical.ToolTip += " Not available in Basic version";
#elif INT
			buttonRotateLeft.IsEnabled = false;
			buttonRotateRight.IsEnabled = false;
			radioButtonTopLeft.IsEnabled = true;
			radioButtonTopRight.IsEnabled = true;
			radioButtonBottomLeft.IsEnabled = true;
			radioButtonBottomRight.IsEnabled = true;
			textBoxCellSize.IsEnabled = true;
			textBoxStickinessAmount.IsEnabled = true;
			buttonMirrorHorizontal.IsEnabled = false;
			buttonMirrorVertical.IsEnabled = false;
#endif
		}

		void CreateGrid()
		{
			var Lines = canvasDrawingArea.Children.OfType<Line>().ToList();

			foreach (var L in Lines)
				canvasDrawingArea.Children.Remove(L);

			Single ModCellSize = GridSize * 10 * INCH * ZoomLevel; //Grid Size is actually in tenth of inches, have to multiply by 10
			ModCellSize -= _negGridMod; //take into account Margin? Not sure

			for (Single i = ModCellSize; i < canvasDrawingArea.ActualWidth; i += ModCellSize)
			{
				Line Temp = new Line();
				Temp.Stroke = Brushes.LightGray;
				Temp.StrokeThickness = 1;
				Temp.X1 = i;
				Temp.Y1 = 0;
				Temp.X2 = i;
				Temp.Y2 = canvasDrawingArea.ActualHeight;
				canvasDrawingArea.Children.Add(Temp);
			}

			for (Single i = ModCellSize; i < canvasDrawingArea.ActualHeight; i += ModCellSize)
			{
				Line Temp = new Line();
				Temp.Stroke = Brushes.LightGray;
				Temp.StrokeThickness = 1;
				Temp.X1 = 0;
				Temp.Y1 = i;
				Temp.X2 = canvasDrawingArea.ActualWidth;
				Temp.Y2 = i;
				canvasDrawingArea.Children.Add(Temp);
			}

			Line VerticalCenter = new Line();
			Line HorizontalCenter = new Line();

			VerticalCenter.Stroke = HorizontalCenter.Stroke = Brushes.Black;
			VerticalCenter.StrokeThickness = HorizontalCenter.StrokeThickness = 1;
																				  
			VerticalCenter.X1 = VerticalCenter.X2 = canvasDrawingArea.Width / 2.0;
			VerticalCenter.Y1 = 0;
			//VerticalCenter.X2 = VerticalCenter.X1;
			VerticalCenter.Y2 = canvasDrawingArea.Height;
			HorizontalCenter.X1 = 0;
			HorizontalCenter.Y1 = HorizontalCenter.Y2 = canvasDrawingArea.Height / 2.0;
			HorizontalCenter.X2 = canvasDrawingArea.Width;
			//HorizontalCenter.Y2 = HorizontalCenter.Y1;

			canvasDrawingArea.Children.Add(VerticalCenter);
			canvasDrawingArea.Children.Add(HorizontalCenter);
		}

		private void MenuItem_File_Open_Click(Object Sender, EventArgs e)
		{
			OpenFileDialog OFD = new OpenFileDialog();
			OFD.DefaultExt = ".pes";
			OFD.Filter = "PES Embroidery Files|*.pes;";//*.dst;*.pec;";
			OFD.Multiselect = true;

			Nullable<Boolean> Result = OFD.ShowDialog();

			if (Result == true)
			{
				foreach (String Filename in OFD.FileNames)
				{
					if (!System.IO.File.Exists(Filename))
						return;

					_filenames.Add(Filename);
					//RotateValues.Add(0.0f);
					//_scaleValues.Add(new Point(1, 1));
				}

				UserLogic.UserSettings.LastOpenDir = System.IO.Path.GetDirectoryName(OFD.FileNames[0]);
				UserLogic.openFiles(OFD.FileNames, new Point(0,0));

				DisplayImages(OFD.FileNames.Length);
			}

			if (Designs.Count != 0)
			{
				MenuItem_File_Save_As.IsEnabled = true;
				buttonNew.IsEnabled = true;
				buttonDelete.IsEnabled = true;
				buttonSave.IsEnabled = true;
			}
		}

		void _SetImagePosition(Image I, Point PostRotate, int ID)
		{
			SetImageX(I, PostRotate.X, ID);
			SetImageY(I, PostRotate.Y, ID);
			//I.SetValue(Canvas.LeftProperty, Position.X);
			//I.SetValue(Canvas.TopProperty, Position.Y);
		}

		void _SetBorderPosition(Border B, Point Position)
		{
			B.SetValue(Canvas.LeftProperty, Position.X);
			B.SetValue(Canvas.TopProperty, Position.Y);
		}

		void DisplayImages(int ExpectedCount)
		{
			Int32 Index = 0;
			List<Image> AllImages = UserLogic.GetImagesFromDesigns(ZoomLevel, ExpectedCount);

			foreach (System.Windows.Controls.Image I in AllImages)
			{
				//if it's already added, skip it
				if (AllImages.Count - (ExpectedCount + Index++) > 0)
					continue;

				I.LostMouseCapture += new MouseEventHandler(I_LostMouseCapture);
				I.MouseDown += new MouseButtonEventHandler(I_MouseDown);
				I.MouseMove += new MouseEventHandler(I_MouseMove);
				I.MouseUp += new MouseButtonEventHandler(I_MouseUp);
				//I.KeyDown += new KeyEventHandler(I_KeyDown);
				I.SnapsToDevicePixels = false;
				I.Focusable = true;
				I.GotFocus += new RoutedEventHandler(I_GotFocus);
												
				Designs.Add(I);

				_SetImagePosition(I, new Point(0.0, 0.0), Designs.Count - 1);
				_scaleValues.Add(new Point(1.0, 1.0));
				RotateValues.Add(0.0f);
								
				if (AdjustHoopSize(I))
					canvasDrawingArea.Children.Add(I);
				else
				{
#if INT
					ShowError("Size of at least one of your designs is larger than 5x7 or 7x5, please choose a 4x4, 5x7, or 7x5 design");
#elif BASIC
					ShowError("Combine Design Basic is limited to a 4x4 hoop size. The paid version allows 5x7 or 7x5 hoop sizes");
#endif
					List<Image> LI = new List<Image>();
					LI.Add(I);
					RemoveOneDesign(LI);
					RotateValues.RemoveAt(RotateValues.Count - 1);
					_scaleValues.RemoveAt(_scaleValues.Count - 1);
					break;
				}
			}
		}
		
		private void I_GotFocus(object sender, RoutedEventArgs e)
		{
 			//SetNewImageBorder();
		}

		Boolean Fit4x4(Image I)
		{
			if (I.Width > (100 * ZoomLevel * 10))
				return false;
			else
			{
				if (I.Height > (100 * ZoomLevel * 10))
					return false;
				else
					return true;
			}
		}

		Boolean Fit5x7(Image I)
		{
			if (I.Width > (130 * ZoomLevel * 10))
				return false;
			else
			{
				if (I.Height > (180 * ZoomLevel * 10))
					return false;
				else
					return true;
			}
		}

		Boolean Fit7x5(Image I)
		{
			if (I.Width > (180 * ZoomLevel * 10))
			{
				if (Fit5x7(I))
					return true;

				return false;
			}
			else
			{
				if (I.Height > (130 * ZoomLevel * 10))
				{
					if (Fit5x7(I))
						return true;

					return false;
				}
				
				return true;
			}
		}

		Boolean AdjustHoopSize(Image I)
		{
			Byte HoopWidthCheck = 0; //can be 4, 5, (6), 7, (and 10)
			
			if (radioButton4x4Hoop.IsChecked == true)
				HoopWidthCheck = 4;
			else if (radioButton5x7Hoop.IsChecked == true)
				HoopWidthCheck = 5;
			else if (radioButton6x10Hoop.IsChecked == true)
				HoopWidthCheck = 6;
			else if (radioButton7x5Hoop.IsChecked == true)
				HoopWidthCheck = 7;
			else if (radioButton10x6Hoop.IsChecked == true)
				HoopWidthCheck = 10;

			if (!(I.Width > canvasDrawingArea.ActualWidth || I.Height >
				canvasDrawingArea.ActualHeight))
				return true;

#if !BASIC
			//only dealing with hoop changes now
			switch (HoopWidthCheck)
			{
				case 4:
					//check 5x7
					if (Fit5x7(I))
					{
						radioButton5x7Hoop.IsChecked = true;
						return true;
					}
					else if (Fit7x5(I))
					{
						radioButton7x5Hoop.IsChecked = true;
						return true;
					}
					else
						return false;
				case 5:
					if (Fit7x5(I))
					{
						radioButton7x5Hoop.IsChecked = true;
						return true;
					}
					else
						return false;
				case 7:
					if (Fit5x7(I))
					{
						radioButton5x7Hoop.IsChecked = true;
						return true;
					}
					return false;
			}

			return true;
#elif BASIC
			return false;
#endif
		}

		void I_MouseMove(Object Sender, MouseEventArgs e)
		{
			bool mouseCaptured = false;

			if (e.LeftButton != MouseButtonState.Pressed)
				return;

			foreach (Image I in Selected)
			{
				if (I.IsMouseCaptured)
				{
					mouseCaptured = true;
					break;
				}
			}

			if (mouseCaptured)
			{
				Point MouseCurrent = e.GetPosition(null);
				Double Left = MouseCurrent.X - MousePoint.X;
				Double Top = MouseCurrent.Y - MousePoint.Y;
				int count = 0;

				foreach (Image I in Selected)
				{
					Rect Bounds = GetBounds(I);

					if (Bounds.Left < 0.0)
					{
						CanvasLeft = Math.Abs(Bounds.Left);

						if (Left < 0.0)
							Left = 0.0;
					}
					else if (Bounds.Right > canvasDrawingArea.ActualWidth)
						CanvasLeft = canvasDrawingArea.ActualWidth - Bounds.Width;
				
					if (Bounds.Top < 0.0)
					{
						CanvasTop = Math.Abs(Bounds.Top);

						if (Top < 0.0)
							Top = 0.0;
					}
					else if (Bounds.Bottom > canvasDrawingArea.ActualHeight)
						CanvasTop = canvasDrawingArea.ActualHeight - Bounds.Height;
				
					//actual setting of values
					if (testMovement(I, count, new Point(Left, Top)))
					{
						//_SetImagePosition(I, new Point(GetImageX(I, false, count) + Left, GetImageY(I, false, count) + Top), count++);
						MoveImage(I, new Point(Left, Top), false);
						MoveBorder(_ImageBorder, new Point(Left, Top));
						//SetNewImageBorder(GetImagePosition(I));
						//_SetBorderPosition(GetBorder(), new Point(CanvasLeft + Left, CanvasTop + Top));
					}
				}
				//keeps from jittering
				MousePoint = MouseCurrent;
				CanvasLeft = GetImagePosition((Image)Sender).X;
				CanvasTop = GetImagePosition((Image)Sender).Y;
				//CanvasLeft = GetBounds((Image)Sender).Left;
				//CanvasTop = GetBounds((Image)Sender).Top;

				//SetNewImageBorder();
			}
		}

		Point GetImagePosition(Image I)
		{
			int imageID = Designs.IndexOf(I);
			double x = GetImageX(I, false, imageID);
			double y = GetImageY(I, false, imageID);

			return new Point(x,y);
		}

		void I_LostMouseCapture(Object Sender, MouseEventArgs e)
		{
			((Image)Sender).ReleaseMouseCapture();
		}

		byte GetSnapLocation()
		{
			if (radioButtonTopLeft.IsChecked == true)
				return 0;
			else if (radioButtonTopRight.IsChecked == true)
				return 1;
			else if (radioButtonBottomLeft.IsChecked == true)
				return 2;

			return 3;
		}

		void I_MouseUp(Object Sender, MouseButtonEventArgs e)
		{
			if (!ZoomToggle)
			{
				if (Selected == null)
					return;

				/*Queue<int> ID = new Queue<int>();

				for (int i = 0; i < Designs.Count; i++)
				{
					foreach (Image I in Selected)
					{
						if (I == Designs[i])
						{
							ID.Enqueue(i);
							break;
						}
					}
				} */

				foreach (Image I in Selected)
				{
					I.ReleaseMouseCapture();
					//SetNewImageBorder(GetImagePosition(I));

					if (SnapToGrid)
						ApplySnap();
					/*Rect R = GetBounds(I);

					if (!SnapToGrid)
					{
						SetNewImageBorder();
						continue;
					}

					//checkEdges(I, false, ID.Dequeue());

					if (R.Left < 0.0)
					{
						float temp = StickyValue;
						StickyValue = (float)-R.Left;
						ApplySnap(0);
						StickyValue = temp;
					}
					else if (R.Right > canvasDrawingArea.Width)
					{
						float temp = StickyValue;			   
						StickyValue = (float)-R.Right;		   //not right
						ApplySnap(1);
						StickyValue = temp;
					}
					else if (R.Top < 0.0)
					{
						float temp = StickyValue;
						StickyValue = (float)-R.Top;
						ApplySnap(0);
						StickyValue = temp;
					}
					else if (R.Bottom > canvasDrawingArea.Height)
					{
						float temp = StickyValue;
						StickyValue = (float)-R.Bottom; //not right
						ApplySnap(2);
						StickyValue = temp;
					}

					ApplySnap();
					SetNewImageBorder();*/
				}
			}
			else
			{
				MousePoint = e.GetPosition(null);
				/*if (e.LeftButton == MouseButtonState.Pressed)
					ZoomIn(MousePoint);
				else if (e.RightButton == MouseButtonState.Pressed)
					ZoomOut(MousePoint);*/
			}
		}

		private void ApplySnap(byte SnapLocation = 4)
		{

			Double XComparison = 0.0;
			Double YComparison = 0.0;
			Double XChangeAmount = 0.0;
			Double YChangeAmount = 0.0;
			double xMin = 9999;
			double yMin = 9999;
			double xMax = 0;
			double yMax = 0;
			double fullWidth = 0;
			double fullHeight = 0;
			Image FurthestLeft = new Image();
			Image FurthestTop = new Image();
			Image FurthestRight = new Image();
			Image FurthestBottom = new Image();
			Image XImage = new Image();
			Image YImage = new Image();

			if (SnapLocation == 4)
				SnapLocation = GetSnapLocation();

			switch (SnapLocation)
			{
				case 0:
					//top left
					foreach (Image I in Selected)
					{
						double x = xMin;
						double y = yMin;
						Rect R = GetBounds(I);

						xMin = Math.Min(xMin, R.Left);
						yMin = Math.Min(yMin, R.Top);
						//xMax = Math.Max(xMax, Canvas.GetLeft(I) + I.ActualWidth);
						//yMax = Math.Max(yMax, Canvas.GetTop(I) + I.ActualHeight);

						if (x != xMin)
							FurthestLeft = I;
						if (y != yMin)
							FurthestTop = I;
					}
					XComparison = xMin;
					YComparison = yMin;
					break;
				case 1:
					//top right
					foreach (Image I in Selected)
					{
						double x = xMax;
						double y = yMin;
						Rect R = GetBounds(I);

						xMax = Math.Max(xMax, R.Right);
						yMin = Math.Min(yMin, R.Top);
						fullWidth = Math.Max(fullWidth, I.ActualWidth);

						if (x != xMax)
							FurthestRight = I;
						if (y != yMin)
							FurthestTop = I;
					}
					XComparison = xMax;
					YComparison = yMin;
					XChangeAmount = -fullWidth;
					break;
				case 2:
					//bottom left
					foreach (Image I in Selected)
					{
						double x = xMin;
						double y = yMax;
						Rect R = GetBounds(I);

						xMin = Math.Min(xMin, R.Left);
						yMax = Math.Max(yMax, R.Bottom);
						fullHeight = Math.Max(fullHeight, I.ActualHeight);

						if (x != xMin)
							FurthestLeft = I;
						if (y != yMax)
							FurthestBottom = I;
					}
					XComparison = xMin;
					YComparison = yMax;
					YChangeAmount = -fullHeight;
					break;
				case 3:
					//bottom right
					foreach (Image I in Selected)
					{
						double x = xMax;
						double y = yMax;
						Rect R = GetBounds(I);

						xMax = Math.Max(xMax, R.Right);
						yMax = Math.Max(yMax, R.Bottom);
						fullHeight = Math.Max(fullHeight, I.ActualHeight);
						fullWidth = Math.Max(fullWidth, I.ActualWidth);

						if (x != xMax)
							FurthestRight = I;
						if (y != yMax)
							FurthestBottom = I;
					}
					XComparison = xMax;
					YComparison = yMax;
					XChangeAmount = -fullWidth;
					YChangeAmount = -fullHeight;
					break;
			}

			switch (SnapLocation)
			{
				case 0:
					XImage = FurthestLeft;
					YImage = FurthestTop;
					break;
				case 1:
					XImage = FurthestRight;
					YImage = FurthestTop;
					break;
				case 2:
					XImage = FurthestLeft;
					YImage = FurthestBottom;
					break;
				case 3:
					XImage = FurthestRight;
					YImage = FurthestBottom;
					break;
			}

			TestAgainstLine(XComparison, YComparison, XChangeAmount, YChangeAmount, XImage, YImage, SnapLocation);
		}

		private void TestAgainstLine(double XComparison, double YComparison, double XChangeAmount, double YChangeAmount, Image XImage, Image YImage, byte SnapLocation)
		{
			bool moved = false;

			foreach (Object Child in canvasDrawingArea.Children)
			{
				if (!(Child is Line))
					continue;

				Line L = (Line)Child;

				//figure out if we're using a horizontal or vertical line to test to
				//First test for Vertical
				if (L.X1 == L.X2)
				{
					if (Math.Abs(XComparison - L.X1) < StickyValue)
					{
						MoveImage(XImage, new Point(L.X1 - XComparison, 0.0), false);
						MoveBorder(_ImageBorder, new Point(L.X1 - XComparison, 0.0));
						moved = true;
					}
						//SetImageX(XImage, L.X1, 0);
						//_SetImagePosition(XImage, new Point(L.X1 + XChangeAmount, GetImagePosition(XImage).Y));
				}
				//if it's not vertical, it must be horizontal
				else
				{
					if (Math.Abs(YComparison - L.Y1) < StickyValue)
					{
						//_SetImagePosition(YImage, new Point(GetImagePosition(YImage).X, L.Y1 + YChangeAmount));
						//SetImageY(YImage, L.Y1, 0);

						MoveImage(YImage, new Point(0.0, L.Y1 - YComparison), false);
						MoveBorder(_ImageBorder, new Point(0.0, L.Y1 - YComparison));
						moved = true;
					}
				}
			}

			if (!moved)
			{
				Rect XBounds = GetImageBounds(XImage, false, Designs.IndexOf(XImage));
				Rect YBounds = GetImageBounds(YImage, false, Designs.IndexOf(YImage));
				Point Offset = new Point();

				//checking borders
				switch (SnapLocation)
				{
					case 0: //top left
						if (XComparison < StickyValue) //checking 0 value, nothing to subtract
							Offset.X = -XComparison;
						if (YComparison < StickyValue) //again, checking 0 value
							Offset.Y = -YComparison;
						break;
					case 1: //top right
						if (canvasDrawingArea.ActualWidth - XComparison < StickyValue) //checking 0 value, nothing to subtract
							Offset.X = canvasDrawingArea.ActualWidth - XBounds.Right;//NewPosX = canvasDrawingArea.ActualWidth - XImage.ActualWidth; //was 0.0
						if (YComparison < StickyValue) //again, checking 0 value
							Offset.Y = -YComparison;
						break;
					case 2: //bottom left
						if (XComparison < StickyValue) //checking 0 value, nothing to subtract
							Offset.X = -XComparison;
						if (canvasDrawingArea.ActualHeight - YComparison < StickyValue) //again, checking 0 value
							Offset.Y = canvasDrawingArea.ActualHeight - YBounds.Bottom;//NewPosY = canvasDrawingArea.ActualHeight - XImage.ActualHeight;
						break;
					case 3: //bottom right
						if (canvasDrawingArea.ActualWidth - XComparison < StickyValue) //checking 0 value, nothing to subtract
							Offset.X = canvasDrawingArea.ActualWidth - XBounds.Right;
						if (canvasDrawingArea.ActualHeight - YComparison < StickyValue) //again, checking 0 value
							Offset.Y = canvasDrawingArea.ActualHeight - YBounds.Bottom;
						break;
				}

				MoveImage(XImage, new Point(Offset.X, 0), false);
				MoveBorder(_ImageBorder, new Point(Offset.X, 0.0));
				MoveImage(YImage, new Point(0.0, Offset.Y), false);
				MoveBorder(_ImageBorder, new Point(0.0, Offset.Y));
			}
			//MoveImage(XImage, new Point(NewPosX, 0.0));
			//MoveImage(YImage, new Point(0.0, NewPosY));
			//_SetImagePosition(XImage, new Point(NewPosX, GetImagePosition(XImage).Y));
			//_SetImagePosition(YImage, new Point(GetImagePosition(YImage).X, NewPosY));

			///////End Snap To Grid
		}

		/*public Rect GetNewBounds(FrameworkElement FE, Rect R)
		{
			//get the current bounds
			Rect CurBounds = GetBounds(FE);

			GeneralTransform transform = FE.TransformToVisual(canvasDrawingArea);


		} */

		public Rect GetBounds(FrameworkElement FE)
		{

			GeneralTransform transform = FE.TransformToVisual(canvasDrawingArea);

			return transform.TransformBounds(new Rect(0, 0, FE.ActualWidth, FE.ActualHeight));

		}

		void SetNewImageBorder(Point Location, Size NewSize)
		{
			if (Selected.Count == 0)
				return;

			double minX = 9999;
			double minY = 9999;
			double maxX = 0;
			double maxY = 0;
			Rect Bounds;
			int counter = 0;

			//ransformGroup TG = new TransformGroup();
			//Rect R = new Rect();

			if (_ImageBorder == null)
			{
				_ImageBorder = new Border();
				_borderAdded = false;
			}

			//RemoveBorder();
			if (!_borderAdded)
			{
				_ImageBorder.BorderThickness = new System.Windows.Thickness(2.0);
				_ImageBorder.BorderBrush = Brushes.Black;
				_ImageBorder.VerticalAlignment = System.Windows.VerticalAlignment.Center;
				//}

				foreach (Image I in Selected)
				{
					/*minX = Math.Min(minX, Canvas.GetLeft(I));
					minY = Math.Min(minY, Canvas.GetTop(I));
					maxX = Math.Max(maxX, Canvas.GetLeft(I) + I.ActualWidth);
					maxY = Math.Max(maxY, Canvas.GetTop(I) + I.ActualHeight);*/

					Bounds = GetImageBounds(I, rotating, counter);

					minX = Math.Min(minX, Bounds.Left);
					minY = Math.Min(minY, Bounds.Top);
					maxX = Math.Max(maxX, Bounds.Right);
					maxY = Math.Max(maxY, Bounds.Bottom);

					counter++;
				}

				_SetBorderPosition(_ImageBorder, new Point(minX, minY));
				_ImageBorder.Width = maxX - minX;
				_ImageBorder.Height = maxY - minY;
				//}

				//if ()
				//windowCombineDesign.LayoutUpdated += windowCombineDesign_LayoutUpdated;

				_ImageBorder.Visibility = System.Windows.Visibility.Visible;

				//if (!_borderAdded)
				//{
				canvasDrawingArea.Children.Add(_ImageBorder);
				_borderAdded = true;
				//}
			}
			else
			{
				_SetBorderPosition(_ImageBorder, Location);
				_ImageBorder.Width = NewSize.Width;
				_ImageBorder.Height = NewSize.Height;
			}
		}

		private void windowCombineDesign_LayoutUpdated(object sender, EventArgs e)
		{
			//SetNewImageBorder();
			//windowCombineDesign.LayoutUpdated -= windowCombineDesign_LayoutUpdated;
		}

		void I_MouseDown(Object Sender, MouseButtonEventArgs e)
		{
			//((Image)Sender).Focus();
			MousePoint = e.GetPosition(null);
			Image TempImage = ((Image)Sender);

			if (_ImageBorder.Visibility == System.Windows.Visibility.Hidden)
				_ImageBorder.Visibility = System.Windows.Visibility.Visible;

			if (!ZoomToggle)
			{
				CanvasLeft = GetImagePosition(TempImage).X;
				CanvasTop = GetImagePosition(TempImage).Y;
				((Image)Sender).CaptureMouse();

				int designID = Designs.IndexOf((Image)Sender);

				Rect Bounds = GetImageBounds(TempImage, true, designID);
				
				if (_ImageBorder == null)
					_ImageBorder = new Border();

				if (_ctrlDown)
					Selected.Add((Image)Sender);
				else if (Selected.Count == 1)
				{
					if (Selected[0] == ((Image)Sender))
					{
						if ((_ImageBorder.Width == Bounds.Width) && (_ImageBorder.Height == Bounds.Height))
						{
							if ((GetBorderX(_ImageBorder) == Bounds.Left) && (GetBorderY(_ImageBorder) == Bounds.Y))
								return;
						}
					}

					//switched from one design to another
					Selected.Clear();
					Selected.Add((Image)Sender);

					_SetBorderPosition(_ImageBorder, Bounds.TopLeft);
					_ImageBorder.Width = Bounds.Width;
					_ImageBorder.Height = Bounds.Height;

					//if (GetBorderX(_ImageBorder) != GetImageX(Selected[0], false, designID))
					/*if ((Canvas.GetLeft(Selected[0]) != Canvas.GetLeft(_ImageBorder)) || (Canvas.GetTop(Selected[0]) != Canvas.GetTop(_ImageBorder)))
					{
						_ImageBorder.RenderTransformOrigin = new Point(0.5, 0.5);
						//_ImageBorder.RenderTransform = new RotateTransform(RotateValues[designID]);

						TransformGroup TG = new TransformGroup();
						RotateTransform RT = new RotateTransform();
						ScaleTransform ST = new ScaleTransform();

						RT.Angle = RotateValues[designID];
						ST.ScaleX = _scaleValues[designID].X;
						ST.ScaleY = _scaleValues[designID].Y;

						TG.Children.Add(RT);
						TG.Children.Add(ST);

						_ImageBorder.RenderTransform = TG;
					}*/
					
					return;
				}
				else
				{
					Selected.Clear();
					Selected.Add((Image)Sender);
				}

				SetNewImageBorder(GetImagePosition(TempImage), TempImage.RenderSize);
				
				//Selected.Focus();
			}
		}

		private void radioButton4x4Hoop_Checked(object sender, RoutedEventArgs e)
		{
			if (FirstTime)
			{
				FirstTime = false;
				return;
			}

			SetHeightWidth(100, 100);
		}

		private void SetHeightWidth(Int16 SizeX, Int16 SizeY)
		{
			Double UndoSizeX = canvasDrawingArea.ActualWidth / ZoomLevel / 10;
			Double UndoSizeY = canvasDrawingArea.ActualHeight / ZoomLevel / 10;
			double xDiff = 0.0;
			double yDiff = 0.0;

			if (SizeX > UndoSizeX) //expanding X
				xDiff = (SizeX * ZoomLevel * 10) - canvasDrawingArea.ActualWidth;
			else //shrinkingX
				xDiff = -(canvasDrawingArea.ActualWidth - (SizeX * ZoomLevel * 10));

			if (SizeY > UndoSizeY) //expanding Y
				yDiff = (SizeY * ZoomLevel * 10) - canvasDrawingArea.ActualHeight;
			else //shrinkingY
				yDiff = -(canvasDrawingArea.ActualHeight - (SizeY * ZoomLevel * 10));

			canvasDrawingArea.Width += xDiff;
			canvasDrawingArea.Height += yDiff;
			borderEdit.Width += xDiff;
			borderEdit.Height += yDiff;
			windowCombineDesign.Width += xDiff;
			windowCombineDesign.Height = canvasDrawingArea.Height + 100;

			if (moveImages() == false)
			{
				ShowError("Design is wider or taller than this size");
				SetHeightWidth((Int16)UndoSizeX, (Int16)UndoSizeY);

				if (UndoSizeX == 100)
					radioButton4x4Hoop.IsChecked = true;
				else if (UndoSizeX == 130)
					radioButton5x7Hoop.IsChecked = true;
				else
					radioButton7x5Hoop.IsChecked = true;
			}	
		}

		void ShowError(String Message)
		{
			ErrorWindow EW2 = new ErrorWindow();

			EW2.SetMessage(Message);
			EW2.ShowDialog();
		}

		private void radioButton5x7Hoop_Checked(object sender, RoutedEventArgs e)
		{
			if (FirstTime)
			{
				FirstTime = false;
				return;
			}

			SetHeightWidth(130, 180);
		}

		private void radioButton7x5Hoop_Checked(object sender, RoutedEventArgs e)
		{
			if (FirstTime)
			{
				FirstTime = false;
				return;
			}

			SetHeightWidth(180, 130);
		}

		private void radioButton6x10Hoop_Checked(object sender, RoutedEventArgs e)
		{
			if (FirstTime)
			{
				FirstTime = false;
				return;
			}

			SetHeightWidth(150, 250);
		}

		private void radioButton10x6Hoop_Checked(object sender, RoutedEventArgs e)
		{
			if (FirstTime)
			{
				FirstTime = false;
				return;
			}

			SetHeightWidth(250, 150);
		}

		Boolean moveImages()
		{
			int count = 0;

			foreach (UIElement UIE in canvasDrawingArea.Children)
			{
				if (UIE is Line)
					continue;
				if (UIE is Border)
					continue;

				//only images from here on

				if (((Image)UIE).ActualWidth > canvasDrawingArea.ActualWidth)
					return false;
				if (((Image)UIE).ActualHeight > canvasDrawingArea.ActualHeight)
					return false;

				if (GetImagePosition((Image)UIE).X + ((Image)UIE).ActualWidth > canvasDrawingArea.ActualWidth)
					SetImageX((Image)UIE, canvasDrawingArea.ActualWidth - ((Image)UIE).ActualWidth, count);
				if (GetImagePosition((Image)UIE).Y + ((Image)UIE).ActualHeight > canvasDrawingArea.ActualHeight)
					SetImageY((Image)UIE, canvasDrawingArea.ActualHeight - ((Image)UIE).ActualHeight, count);

				count++;
			}

			HideBorder();

			return true;
		}

		Point RotatePointByImageRotation(Point PreRotate, int imageID)
		{
			RotateTransform RT = new RotateTransform(-RotateValues[imageID]);

			Point Test = RT.Transform(PreRotate);

			return Test;
		} 

		void SetImageX(Image I, double x, int ID)
		{
			//Point setX = new Point(x, GetImageY(I, false, ID));



			I.SetValue(LeftProperty, x);
			//SetNewImageBorder();
		}

		void SetImageY(Image I, double y, int ID)
		{
			//Point setY = new Point(GetImageX(I, false, ID), y); 
				
			I.SetValue(TopProperty, y);
			//SetNewImageBorder();
		}

		private void windowCombineDesign_Loaded(object sender, RoutedEventArgs e)
		{
			CreateGrid();
		}

		private Border GetBorder()
		{
			if (canvasDrawingArea.Children.OfType<Border>().ToList().Count == 0)
				return null;

			var Borders = canvasDrawingArea.Children.OfType<Border>().ToList();

			return Borders[Borders.Count - 1];
		}

		private void HideBorder()
		{
			if (_ImageBorder != null)
				_ImageBorder.Visibility = System.Windows.Visibility.Hidden;
		}

		private void RemoveBorder()
		{
			canvasDrawingArea.Children.Remove(_ImageBorder);

			_ImageBorder = null;
		}

		private void canvasDrawingArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			foreach (Image I in Selected)
			{
				if (I.IsMouseCaptured)
					return;
			}

			Selected.Clear();
			buttonDelete.IsEnabled = false;
			buttonMirrorHorizontal.IsEnabled = false;
			buttonMirrorVertical.IsEnabled = false;
			buttonRotateLeft.IsEnabled = false;
			buttonRotateRight.IsEnabled = false;
			HideBorder();
		}

		double GetImageWidth(Image I, bool updateTransform = false, int ID = -1)
		{
			//Get Position then rotate by rotation values
			Rect bounds = GetBounds(I);

			if (!updateTransform)
				return bounds.Width;

			Point Center = new Point((bounds.Left + bounds.Right) / 2, (bounds.Top + bounds.Bottom) / 2);
			RotateTransform RT = new RotateTransform(currentRotate);
			
			Point topLeft = RT.Transform(new Point(bounds.TopLeft.X - Center.X, bounds.TopLeft.Y - Center.Y));
			Point topRight = RT.Transform(new Point(bounds.TopRight.X - Center.X, bounds.TopRight.Y - Center.Y));
			Point bottomLeft = RT.Transform(new Point(bounds.BottomLeft.X - Center.X, bounds.BottomLeft.Y - Center.Y));
			Point bottomRight = RT.Transform(new Point(bounds.BottomRight.X - Center.X, bounds.BottomRight.Y - Center.Y));

			topLeft = new Point(topLeft.X + Center.X, topLeft.Y + Center.Y);
			topRight = new Point(topRight.X + Center.X, topRight.Y + Center.Y);
			bottomLeft = new Point(bottomLeft.X + Center.X, bottomLeft.Y + Center.Y);
			bottomRight = new Point(bottomRight.X + Center.X, bottomRight.Y + Center.Y);

			double maxMin = Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X));
			double maxMax = Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X));

			return maxMax - maxMin;
		}

		double GetImageHeight(Image I, bool updateTransform, int ID)
		{
			//Get Position then rotate by rotation values
			Rect bounds = GetBounds(I);

			if (!updateTransform)
				return bounds.Height;

			Point Center = new Point((bounds.Left + bounds.Right) / 2, (bounds.Top + bounds.Bottom) / 2);
			RotateTransform RT = new RotateTransform(currentRotate);

			Point topLeft = RT.Transform(new Point(bounds.TopLeft.X - Center.X, bounds.TopLeft.Y - Center.Y));
			Point topRight = RT.Transform(new Point(bounds.TopRight.X - Center.X, bounds.TopRight.Y - Center.Y));
			Point bottomLeft = RT.Transform(new Point(bounds.BottomLeft.X - Center.X, bounds.BottomLeft.Y - Center.Y));
			Point bottomRight = RT.Transform(new Point(bounds.BottomRight.X - Center.X, bounds.BottomRight.Y - Center.Y));

			topLeft = new Point(topLeft.X + Center.X, topLeft.Y + Center.Y);
			topRight = new Point(topRight.X + Center.X, topRight.Y + Center.Y);
			bottomLeft = new Point(bottomLeft.X + Center.X, bottomLeft.Y + Center.Y);
			bottomRight = new Point(bottomRight.X + Center.X, bottomRight.Y + Center.Y);

			double maxMin = Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y));
			double maxMax = Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y));

			return maxMax - maxMin;
		}

		double GetImageX(Image I, bool updateTransform, int ID)
		{
			//Get Position then rotate by rotation values
			Rect bounds = GetImageBounds(I, updateTransform, ID);

			if (!updateTransform)
				return bounds.Left;

			RotateTransform RT = new RotateTransform(currentRotate);
			Point Center = new Point(bounds.Left + (bounds.Width / 2), bounds.Top + (bounds.Height / 2));
			
			bounds.Offset(-Center.X, -Center.Y);
			bounds = RT.TransformBounds(bounds);

			bounds.Offset(Center.X, Center.Y);

			return bounds.Left;

			/*Point topLeft = RT.Transform(new Point(bounds.TopLeft.X - Center.X, bounds.TopLeft.Y - Center.Y));
			Point topRight = RT.Transform(new Point(bounds.TopRight.X - Center.X, bounds.TopRight.Y - Center.Y));
			Point bottomLeft = RT.Transform(new Point(bounds.BottomLeft.X - Center.X, bounds.BottomLeft.Y - Center.Y));
			Point bottomRight = RT.Transform(new Point(bounds.BottomRight.X - Center.X, bounds.BottomRight.Y - Center.Y));

			topLeft = new Point(topLeft.X + Center.X, topLeft.Y + Center.Y);
			topRight = new Point(topRight.X + Center.X, topRight.Y + Center.Y);
			bottomLeft = new Point(bottomLeft.X + Center.X, bottomLeft.Y + Center.Y);
			bottomRight = new Point(bottomRight.X + Center.X, bottomRight.Y + Center.Y);

			double maxMin = Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X));

			return maxMin;*/

		}

		double GetImageY(Image I, bool updateTransform, int ID)
		{
			//Get Position then rotate by rotation values
			Rect bounds = GetImageBounds(I, updateTransform, ID);

			if (!updateTransform)
				return bounds.Top;

			RotateTransform RT = new RotateTransform(currentRotate);
			Point Center = new Point(bounds.Left + (bounds.Width / 2), bounds.Top + (bounds.Height / 2));

			bounds.Offset(-Center.X, -Center.Y);
			bounds = RT.TransformBounds(bounds);

			bounds.Offset(Center.X, Center.Y);

			return bounds.Top;
		}

		Rect GetImageBounds(Image I, bool updateTransform, int ID)
		{
			//Get Position then rotate by rotation values
			Rect bounds = GetBounds(I);

			if (!updateTransform)
				return bounds;

			RotateTransform RT = new RotateTransform(currentRotate);
			Point Center = new Point(bounds.Left + (bounds.Width / 2), bounds.Top + (bounds.Height / 2));

			bounds.Offset(-Center.X, -Center.Y);
			bounds = RT.TransformBounds(bounds);

			_ImageBorder.RenderTransform = RT;

			bounds.Offset(Center.X, Center.Y);

			return bounds;
		}

		void SetBorderX(Border B, double x)
		{
			B.SetValue(LeftProperty, x);
		}

		void SetBorderY(Border B, double y)
		{
			B.SetValue(TopProperty, y);
		}

		double GetBorderX(Border B)
		{
			return Canvas.GetLeft(B);
		}

		double GetBorderY(Border B)
		{
			return Canvas.GetTop(B);
		}

		void MoveBorder(Border B, Point P)
		{
			SetBorderX(B, GetBorderX(B) + P.X);
			SetBorderY(B, GetBorderY(B) + P.Y);
		}

		void MoveImage(Image I, Point P, bool fromRotation)
		{
			int imageID = Designs.IndexOf(I);
			//Point PostRotate = RotatePointByImageRotation(P, imageID);
			//Point newPosition = new Point(PostRotate.X + GetImageX(I, fromRotation, 0), PostRotate.Y + GetImageY(I, fromRotation, 0));
			Point newPosition = new Point(Canvas.GetLeft(I) + P.X, Canvas.GetTop(I) + P.Y);
			
			_SetImagePosition(I, newPosition, imageID);
			//if (RotateValues[0] != 0.0f)
			//	SetImageX(I, GetImageX(I, fromRotation, 0) + P.X, 0);
				//SetImageX(I, Canvas.GetLeft(I) + P.X, 0);
			/*else
				SetImageX(I, Canvas.GetLeft(I) + P.X, 0);*/

			//if (RotateValues[0] != 0.0f)
//				SetImageY(I, GetImageY(I, fromRotation, 0) + P.Y, 0);
				//SetImageY(I, Canvas.GetTop(I) + P.Y, 0);
			/*else
				SetImageY(I, Canvas.GetTop(I) + P.Y, 0);*/
		}

		private void windowCombineDesign_KeyDown(object sender, KeyEventArgs e)
		{
			if (textBoxNudgeAmount.IsFocused)
			{
				Selected.Clear();
				HideBorder();

				if (e.Key == Key.Enter)
					textBoxNudgeAmount.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
			}
			
			if (Selected.Count == 0)
				return;
			
			if (e.Key == Key.Delete)
			{
				_box = new Confirm();
				if (_box.ShowDialog() == true)
					RemoveOneDesign(Selected);

				if (Designs.Count == 0)
				{
					buttonDelete.IsEnabled = false;
					buttonNew.IsEnabled = false;
					MenuItem_File_Save_As.IsEnabled = false;
					buttonSave.IsEnabled = false;
				}
				return;
			}
			/*else if ((e.Key == Key.LeftCtrl) || (e.Key == Key.RightCtrl))
			{
				_ctrlDown = true;
				return;
			} */

			var Borders = canvasDrawingArea.Children.OfType<Border>().ToList();

			if (Borders.Count == 0)
				return;

			float AppliedNudge = (float)(NudgeValue * ZoomLevel);

			if (e.Key == Key.Left)
			{
				int counter = 0;

				foreach (Image I in Selected)
				{
					/*if (GetImageX(I, false, counter) - AppliedNudge < 0)
					{
						counter++;
						continue;
					} */

					if (testMovement(I, counter, new Point(-AppliedNudge, 0)))
					{
						MoveImage(I, new Point(-AppliedNudge, 0.0), false);
						MoveBorder(_ImageBorder, new Point(-AppliedNudge, 0.0));
						//SetNewImageBorder(GetImagePosition(I));
					}
				}
			}
			else if (e.Key == Key.Right)
			{
				int counter = 0;

				foreach (Image I in Selected)
				{
					//Rect Bounds = GetBounds(I);

					/*if (Bounds.Width + GetImageX(I, false, counter) + AppliedNudge > canvasDrawingArea.ActualWidth)
					{
						counter++;
						continue;
					} */

					if (testMovement(I, counter, new Point(AppliedNudge, 0)))
					{
						MoveImage(I, new Point(AppliedNudge, 0.0), false);
						MoveBorder(_ImageBorder, new Point(AppliedNudge, 0.0));
						//SetNewImageBorder(GetImagePosition(I));
					}
				}
			}
			else if (e.Key == Key.Up)
			{
				int counter = 0;

				foreach (Image I in Selected)
				{
					/*if (GetImageY(I, false, counter) - AppliedNudge < 0)
					{
						counter++;
						continue;
					} */
					if (testMovement(I, counter, new Point(0, -AppliedNudge)))
					{
						MoveImage(I, new Point(0.0, -AppliedNudge), false);
						MoveBorder(_ImageBorder, new Point(0.0, -AppliedNudge));
						//SetNewImageBorder(GetImagePosition(I));
					}
				}
			}
			else if (e.Key == Key.Down)
			{
				int counter = 0;

				foreach (Image I in Selected)
				{
					//Rect bounds = GetBounds(I);
					/*if (bounds.Height + GetImageY(I, false, counter) + AppliedNudge > canvasDrawingArea.ActualHeight)
					{
						counter++;
						continue;
					} */

					if (testMovement(I, counter, new Point(0, AppliedNudge)))
					{
						MoveImage(I, new Point(0.0, AppliedNudge), false);
						MoveBorder(_ImageBorder, new Point(0.0, AppliedNudge));
						//SetNewImageBorder(GetImagePosition(I));
					}
				}
			}

		}

		bool testMovement(Image I, int ID, Point MoveAmount)
		{
			if (MoveAmount.X < 0)
			{
				if (GetImageX(I, false, ID) + MoveAmount.X < 0)
					return false;
			}
			else if (MoveAmount.X > 0)
			{
				Rect Bounds = GetImageBounds(I, false, ID);

				if (Bounds.Right + MoveAmount.X > canvasDrawingArea.ActualWidth)
					return false;
			}
			
			if (MoveAmount.Y < 0)
			{
				if (GetImageY(I, false, ID) + MoveAmount.Y < 0)
					return false;
			}
			else if (MoveAmount.Y > 0)
			{
				Rect bounds = GetImageBounds(I, false, ID);
				
				if (bounds.Bottom + MoveAmount.Y > canvasDrawingArea.ActualHeight)
					return false;
			}

			return true;
		}

		private bool testTransform(Image I, int ID)
		{
			TransformGroup TG = new TransformGroup();
			RotateTransform RT = new RotateTransform();
			ScaleTransform ST = new ScaleTransform();
			Image testImage = I;

			testImage.RenderTransformOrigin = new Point(0.5, 0.5);

			RT.Angle = currentRotate;
			ST.ScaleX = _scaleValues[ID].X;
			ST.ScaleY = _scaleValues[ID].Y;

			//GetBorder().RenderTransform = new RotateTransform(RotateValues[imageID]);

			TG.Children.Add(RT);
			TG.Children.Add(ST);

			testImage.RenderTransform = TG;

			Rect NewBounds = GetImageBounds(testImage, true, ID);

			if (NewBounds.Left < 0.0)
				return false;
			if (NewBounds.Top < 0.0)
				return false;
			if (NewBounds.Right > canvasDrawingArea.ActualWidth)
				return false;
			if (NewBounds.Bottom > canvasDrawingArea.ActualHeight)
				return false;

			return true;
			
		}

		private void windowCombineDesign_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			//maybe do something with saving options here?
		}

		//Take this out when you have the options screen
		private void textBoxNudgeAmount_TextChanged(object sender, TextChangedEventArgs e)
		{
			float temp = NudgeValue;
			Selected.Clear();
			HideBorder();

			if (((TextBox)sender).Text == "")
				return;

			try
			{
				if (textBoxNudgeAmount.Text[textBoxNudgeAmount.Text.Length - 1] == '.')
					return;

				NudgeValue = Convert.ToSingle(textBoxNudgeAmount.Text);
			}
			catch
			{
				return;
			}

			if (NudgeValue > 200.0f)
				NudgeValue = temp;

			//textBoxNudgeAmount.Text = Convert.ToString(NudgeValue);
		}

		private void MenuItem_File_Save_As_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog SFD = new SaveFileDialog();
			Int16 HoopWidth = 0;
			Int16 HoopHeight = 0;
			List<Rect> ImageBounds = new List<Rect>();
			
			SFD.DefaultExt = ".pes";
			SFD.Filter = "Pes Embroidery Files (*.pes)|*.pes";
			
			Nullable<Boolean> Result = SFD.ShowDialog();

			if (radioButton4x4Hoop.IsChecked == true)
				HoopWidth = HoopHeight = 4;
			else if (radioButton5x7Hoop.IsChecked == true)
			{
				HoopWidth = 5;
				HoopHeight = 7;
			}
			else if (radioButton6x10Hoop.IsChecked == true)
			{
				HoopWidth = 6;
				HoopHeight = 10;
			}
			else if (radioButton7x5Hoop.IsChecked == true)
			{
				HoopWidth = 7;
				HoopHeight = 5;
			}
			else if (radioButton10x6Hoop.IsChecked == true)
			{
				HoopWidth = 10;
				HoopHeight = 6;
			}

			if (Result == true)
			{
				int counter = 0;

				List<Matrix> Matrices = new List<Matrix>();

				foreach (Image I in Designs)
				{
                    Rect Temp = new Rect(GetImageX(I, false, counter) / ZoomLevel, GetImageY(I, false, counter) / ZoomLevel, I.ActualWidth / ZoomLevel, I.ActualHeight / ZoomLevel);
                    				
					ImageBounds.Add(Temp);

					while (RotateValues[counter] < 0.0f)
						RotateValues[counter] += 360.0f;

					if (RotateValues[counter] > 360.0f)
						RotateValues[counter++] %= 360.0f;

					Matrices.Add(I.RenderTransform.Value);
					
				}

				UserLogic.SavePattern(SFD.FileName, HoopWidth, HoopHeight, canvasDrawingArea, Matrices, ImageBounds);//_scaleValues, RotateValues);
				UserLogic.UserSettings.LastSavedDir = System.IO.Path.GetDirectoryName(SFD.FileName);
			}
		}

		private void buttonClear_Click(object sender, RoutedEventArgs e)
		{
			if (Designs.Count != 0)
			{
				_box = new Confirm();
				if (_box.ShowDialog() == true)
					ClearAllDesigns();

				if (Designs.Count == 0)
				{
					buttonDelete.IsEnabled = false;
					buttonNew.IsEnabled = false;
					buttonSave.IsEnabled = false;
					MenuItem_File_Save_As.IsEnabled = false;
				}
			}
		}

		private void ClearAllDesigns(bool AndFilenames = true)
		{
			Designs.Clear();
			UserLogic.Clear();
			canvasDrawingArea.Children.Clear();
			CreateGrid();
			HideBorder();
			RotateValues.Clear();
			_scaleValues.Clear();

			if (AndFilenames)
				_filenames.Clear();

			MenuItem_File_Save_As.IsEnabled = false;
			_borderAdded = false;
		}

		private void RemoveOneDesign(List<Image> LI)
		{
			if (LI.Count == 0)
				return;

			List<int> IDs = new List<int>();

			foreach (Image I in LI)
			{
				UserLogic.ClearDesign(I);
				canvasDrawingArea.Children.Remove(I);
			}
			
			HideBorder();
			//_borderAdded = false;

			for (int i = 0; i < Designs.Count; i++)
			{
				foreach (Image I in LI)
				{
					if (Designs[i] == I)
					{
						IDs.Add(i);
						break;
					}
				}
			}

			foreach (Image I in LI)
				Designs.Remove(I);

			foreach (int i in IDs)
			{
				_filenames.RemoveAt(i);
				RotateValues.RemoveAt(i);
				_scaleValues.RemoveAt(i);
			}
			
			Int32 Count = 0;

			foreach (UIElement UIE in canvasDrawingArea.Children)
			{
				if (UIE is Line)
					continue;
				if (UIE is Border)
					continue;

				Count = 1;
				break;
			}

			if (Count == 0)
			{
				MenuItem_File_Save_As.IsEnabled = false;
				//RemoveBorder();
				HideBorder();
			}
		}

		private void MenuItemAbout_Click(object sender, RoutedEventArgs e)
		{
			AboutWindow = new About();
			AboutWindow.Closed += new EventHandler(AboutWindow_Closed);
			AboutWindow.ShowDialog();
			/*AboutWindow.SetValue(LeftProperty, Canvas.GetLeft
				(windowCombineDesign) + windowCombineDesign.ActualWidth);*/
		}

		private void AboutWindow_Closed(object sender, EventArgs e)
		{
			AboutWindow.Closed -= AboutWindow_Closed;
			AboutWindow.Close();
		}
#if !BASIC
		/*private void windowCombineDesign_MouseEnter(object sender, MouseEventArgs e)
		{
			if (OptionsWindow == null)
				return;

			if (OptionsWindow.CellSize != GridSize)
			{
				GridSize = OptionsWindow.CellSize;
				CreateGrid(GridSize);
			}
		} */
#endif
		private void Expander_Expanded(object sender, RoutedEventArgs e)
		{
			windowCombineDesign.Width += 240;
			
			//make things visible
			labelHoopSize.Visibility = System.Windows.Visibility.Visible;
			radioButton4x4Hoop.Visibility = System.Windows.Visibility.Visible;
			radioButton5x7Hoop.Visibility = System.Windows.Visibility.Visible;
			radioButton7x5Hoop.Visibility = System.Windows.Visibility.Visible;
			radioButton6x10Hoop.Visibility = System.Windows.Visibility.Visible;
			radioButton10x6Hoop.Visibility = System.Windows.Visibility.Visible;
			borderHoopSize.Visibility = System.Windows.Visibility.Visible;
			labelNudgeAmount.Visibility = System.Windows.Visibility.Visible;
			textBoxNudgeAmount.Visibility = System.Windows.Visibility.Visible;
			//separatorSnapToGrid.Visibility = System.Windows.Visibility.Visible;
			labelSnapToGridOptions.Visibility = System.Windows.Visibility.Visible;
			labelSnapLocation.Visibility = System.Windows.Visibility.Visible;
			radioButtonTopLeft.Visibility = System.Windows.Visibility.Visible;
			radioButtonTopRight.Visibility = System.Windows.Visibility.Visible;
			radioButtonBottomLeft.Visibility = System.Windows.Visibility.Visible;
			radioButtonBottomRight.Visibility = System.Windows.Visibility.Visible;
			labelStickiness.Visibility = System.Windows.Visibility.Visible;
			textBoxStickinessAmount.Visibility = System.Windows.Visibility.Visible;
			labelGridSize.Visibility = System.Windows.Visibility.Visible;
			textBoxCellSize.Visibility = System.Windows.Visibility.Visible;
			borderSnapToGrid.Visibility = System.Windows.Visibility.Visible;
			buttonShrinkApp.Visibility = System.Windows.Visibility.Visible;
			buttonExpandApp.Visibility = System.Windows.Visibility.Visible;
			labelAppSize.Visibility = System.Windows.Visibility.Visible;
			listBoxColor.Visibility = System.Windows.Visibility.Visible;
			labelColorList.Visibility = System.Windows.Visibility.Visible;
			//change name, location
			((Expander)sender).Header = "Less";
			((Expander)sender).IsTabStop = false;
		}

		private void Expander_Collapsed(object sender, RoutedEventArgs e)
		{
			labelHoopSize.Visibility = System.Windows.Visibility.Collapsed;
			radioButton4x4Hoop.Visibility = System.Windows.Visibility.Collapsed;
			radioButton5x7Hoop.Visibility = System.Windows.Visibility.Collapsed;
			radioButton7x5Hoop.Visibility = System.Windows.Visibility.Collapsed;
			radioButton6x10Hoop.Visibility = System.Windows.Visibility.Collapsed;
			radioButton10x6Hoop.Visibility = System.Windows.Visibility.Collapsed;
			borderHoopSize.Visibility = System.Windows.Visibility.Collapsed;
			labelNudgeAmount.Visibility = System.Windows.Visibility.Collapsed;
			textBoxNudgeAmount.Visibility = System.Windows.Visibility.Collapsed;
			//separatorSnapToGrid.Visibility = System.Windows.Visibility.Collapsed;
			labelSnapToGridOptions.Visibility = System.Windows.Visibility.Collapsed;
			labelSnapLocation.Visibility = System.Windows.Visibility.Collapsed;
			radioButtonTopLeft.Visibility = System.Windows.Visibility.Collapsed;
			radioButtonTopRight.Visibility = System.Windows.Visibility.Collapsed;
			radioButtonBottomLeft.Visibility = System.Windows.Visibility.Collapsed;
			radioButtonBottomRight.Visibility = System.Windows.Visibility.Collapsed;
			labelStickiness.Visibility = System.Windows.Visibility.Collapsed;
			textBoxStickinessAmount.Visibility = System.Windows.Visibility.Collapsed;
			labelGridSize.Visibility = System.Windows.Visibility.Collapsed;
			textBoxCellSize.Visibility = System.Windows.Visibility.Collapsed;
			borderSnapToGrid.Visibility = System.Windows.Visibility.Collapsed;
			buttonShrinkApp.Visibility = System.Windows.Visibility.Collapsed;
			buttonExpandApp.Visibility = System.Windows.Visibility.Collapsed;
			labelAppSize.Visibility = System.Windows.Visibility.Collapsed;
			listBoxColor.Visibility = System.Windows.Visibility.Collapsed;
			labelColorList.Visibility = System.Windows.Visibility.Collapsed;
			windowCombineDesign.Width -= 240;
			((Expander)sender).Header = "More";
			((Expander)sender).IsTabStop = false;
		}

		private void buttonDelete_Click(object sender, RoutedEventArgs e)
		{
			if (Selected != null)
			{
				_box = new Confirm();
				if (_box.ShowDialog() == true)
					RemoveOneDesign(Selected);

				if (Designs.Count == 0)
				{
					buttonDelete.IsEnabled = false;
					buttonNew.IsEnabled = false;
					MenuItem_File_Save_As.IsEnabled = false;
					buttonSave.IsEnabled = false;
					ClearAllDesigns();
				}
			}
		}

		private void toggleButtonSnapToGrid_Click(object sender, RoutedEventArgs e)
		{
			SnapToGrid = !SnapToGrid;

			radioButtonTopLeft.IsEnabled = SnapToGrid;
			radioButtonTopRight.IsEnabled = SnapToGrid;
			radioButtonBottomLeft.IsEnabled = SnapToGrid;
			radioButtonBottomRight.IsEnabled = SnapToGrid;
			textBoxCellSize.IsEnabled = SnapToGrid;
			textBoxStickinessAmount.IsEnabled = SnapToGrid;
		}

		private void toggleButtonZoom_Click(object sender, RoutedEventArgs e)
		{
			ZoomToggle = !ZoomToggle;
		}

		private void canvasDrawingArea_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			MousePoint = e.GetPosition(null);

			/*if (ZoomToggle)
				ZoomOut(MousePoint);*/
		}

		private void canvasDrawingArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			MousePoint = e.GetPosition(null);

			/*if (ZoomToggle)
				ZoomIn(MousePoint);*/
		}
/*
		private void ZoomIn(Point Center)
		{
			ZoomLevel *= 2.0f;

			PerformZoom(Center);
		}

		private void ZoomOut(Point Center)
		{
			ZoomLevel /= 2.0f;

			//PerformZoom(Center);
		}
*/
		void PerformZoom(Queue<Point> LocToCanvasRatio)
		{
			ClearAllDesigns(false);
			UserLogic.openFiles(_filenames.ToArray(), new Point());
			DisplayImages(_filenames.Count);
			int count = 0;

			foreach (Image I in Designs)
			{
				Point Current = LocToCanvasRatio.Dequeue();
				Current.X *= canvasDrawingArea.Width;
				Current.Y *= canvasDrawingArea.Height;
				_SetImagePosition(I, Current, count);
				ApplyTransform(I, count++);
			}

			//CreateGrid();
		}

		private void buttonShrinkApp_Click(object sender, RoutedEventArgs e)
		{
			double xDiff = canvasDrawingArea.Width - (canvasDrawingArea.Width * 0.8);
			double yDiff = canvasDrawingArea.Height - (canvasDrawingArea.Height * 0.8);
			Queue<Point> DesignLocations = new Queue<Point>();

			foreach (Image I in Designs)
			{
				Point CurrentPoint = GetImagePosition(I);
				CurrentPoint.X /= canvasDrawingArea.Width;
				CurrentPoint.Y /= canvasDrawingArea.Height;
				DesignLocations.Enqueue(CurrentPoint);
			}

			canvasDrawingArea.Width -= xDiff;
			canvasDrawingArea.Height -= yDiff;
			borderEdit.Width -= xDiff;
			borderEdit.Height -= yDiff;
			windowCombineDesign.Width -= xDiff;
			windowCombineDesign.Height = canvasDrawingArea.Height + 100;

			ZoomLevel *= 0.8f;
			_negGridMod *= 0.8f;

			if (ZoomLevel == 0.25f)
				buttonShrinkApp.IsEnabled = false;

			buttonExpandApp.IsEnabled = true;

			PerformZoom(DesignLocations);
		}

		private void buttonExpandApp_Click(object sender, RoutedEventArgs e)
		{
			double xDiff = (canvasDrawingArea.Width * 1.25) - canvasDrawingArea.Width;
			double yDiff = (canvasDrawingArea.Height * 1.25) - canvasDrawingArea.Height;
			Queue<Point> DesignLocations = new Queue<Point>();

			foreach (Image I in Designs)
			{
				Point CurrentPoint = GetImagePosition(I);
				CurrentPoint.X /= canvasDrawingArea.Width;
				CurrentPoint.Y /= canvasDrawingArea.Height;
				DesignLocations.Enqueue(CurrentPoint);
			}
			 
			canvasDrawingArea.Width += xDiff;
			canvasDrawingArea.Height += yDiff;
			borderEdit.Width += xDiff;
			borderEdit.Height += yDiff;
			windowCombineDesign.Width += xDiff;
			windowCombineDesign.Height = canvasDrawingArea.Height + 100;

			ZoomLevel *= 1.25f;
			_negGridMod *= 1.25f;
			
			PerformZoom(DesignLocations);

			buttonShrinkApp.IsEnabled = true;

			//checkWindowMax();

		}

		private void textBoxCellSize_TextChanged(object sender, TextChangedEventArgs e)
		{
			//float temp = GridSize;
			float temp = GridSize;
			double testSize = 0.0;

			if ((textBoxCellSize.Text == "") | textBoxCellSize.Text == ".")
				return;

			if (canvasDrawingArea.ActualHeight > canvasDrawingArea.ActualWidth)
				testSize = canvasDrawingArea.ActualHeight;
			else
				testSize = canvasDrawingArea.ActualWidth;

			//GridSize = Convert.ToSingle(textBoxCellSize.Text);
			GridSize = Convert.ToSingle(textBoxCellSize.Text);

			if (GridSize <= 0 || GridSize > testSize / 2.0)
			{
				GridSize = temp;
			}
			else
				CreateGrid();
		}

		private void canvasDrawingArea_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			CreateGrid();
		}

		void Rotate(float amount)
		{
			Queue<int> ID = new Queue<int>();

			currentRotate = amount;

			for (int i = 0; i < Designs.Count; i++)
			{
				foreach (Image I in Selected)
				{
					if (I == Designs[i])
					{
						ID.Enqueue(i);
						break;
					}
				}
			}

			if (ID.Count == 0)
				return;

			rotating = true;

			foreach (Image I in Selected)
			{
				//won't work with multiple designs
				//Debug
				/*Rect R = GetBounds(I);
				_widthBeforeRotate = R.Width;
				_heightBeforeRotate = R.Height;*/
				//End Debug
				int thisID = ID.Dequeue();
				RotateValues[thisID] += (float)(amount * _scaleValues[thisID].X * _scaleValues[thisID].Y);
				/*I.RenderTransformOrigin = new Point(0.5, 0.5);
				I.RenderTransform = new RotateTransform(RotateValues[thisID]);
				GetBorder().RenderTransformOrigin = new Point(0.5, 0.5);
				GetBorder().RenderTransform = new RotateTransform(RotateValues[thisID]);*/
				//Selected.LayoutTransform = new RotateTransform(RotateValues[ID], 0.5, 0.5);
				RotateValues[thisID] %= 360.0f;

				if (RotateValues[thisID] < 0.0f)
					RotateValues[thisID] += 360.0f;

				ApplyTransform(I, thisID);
				//checkEdges(I, true, thisID);
			}

			currentRotate = 0.0f;
			rotating = false;
		}

		private void buttonRotateLeft_Click(object sender, RoutedEventArgs e)
		{
			if (Selected.Count == 0)
				return;

			Rotate(-90.0f);
		}

		private void buttonRotateRight_Click(object sender, RoutedEventArgs e)
		{
			if (Selected.Count == 0)
				return;

			Rotate(90.0f);

		}

		/*void checkEdges(Image I, bool updateTransform, int ID)
		{
			double currX = GetImageX(I, updateTransform, ID);
			double currY = GetImageY(I, updateTransform, ID);
			double width = GetImageWidth(I, updateTransform, ID);
			double height = GetImageHeight(I, updateTransform, ID);
			//Rect PrevBounds = GetBounds(I);
			Rect Bounds = new Rect(new Point(currX, currY), new Size(width, height));

			if (currX < 0.0)
				SetImageX(I, -currX, ID);
			if (currY < 0.0)
				SetImageY(I, -currY, ID);
			if (currX + Bounds.Width > canvasDrawingArea.ActualWidth)
				SetImageX(I, canvasDrawingArea.ActualWidth - Bounds.Width, ID);
			if (currY + Bounds.Height > canvasDrawingArea.ActualHeight)
				SetImageY(I, canvasDrawingArea.ActualHeight - Bounds.Height, ID);

			SetNewImageBorder();
		} */

		private Point getTransformOffset(Image I, int ID)
		{
			TransformGroup TG = new TransformGroup();
			RotateTransform RT = new RotateTransform();
			ScaleTransform ST = new ScaleTransform();
			Image testImage = I;

			testImage.RenderTransformOrigin = new Point(0.5, 0.5);

			RT.Angle = currentRotate;
			ST.ScaleX = _scaleValues[ID].X;
			ST.ScaleY = _scaleValues[ID].Y;

			//GetBorder().RenderTransform = new RotateTransform(RotateValues[imageID]);

			TG.Children.Add(RT);
			TG.Children.Add(ST);

			testImage.RenderTransform = TG;

			Rect NewBounds = GetImageBounds(testImage, true, ID);
			double x = 0.0;
			double y = 0.0;
			
			if (NewBounds.Left < 0.0)
				x = -NewBounds.Left;
			else if (NewBounds.Right > canvasDrawingArea.ActualWidth)
				x = canvasDrawingArea.ActualWidth - NewBounds.Right;

			if (NewBounds.Top < 0.0)
				y = -NewBounds.Top;
			else if (NewBounds.Bottom > canvasDrawingArea.ActualHeight)
				y = canvasDrawingArea.ActualHeight - NewBounds.Bottom;

			return new Point(x, y);
		}

		private double getTransformOffsetX(Image I, int ID)
		{
			TransformGroup TG = new TransformGroup();
			RotateTransform RT = new RotateTransform();
			ScaleTransform ST = new ScaleTransform();
			Image testImage = I;

			testImage.RenderTransformOrigin = new Point(0.5, 0.5);

			RT.Angle = currentRotate;
			ST.ScaleX = _scaleValues[ID].X;
			ST.ScaleY = _scaleValues[ID].Y;

			//GetBorder().RenderTransform = new RotateTransform(RotateValues[imageID]);

			TG.Children.Add(RT);
			TG.Children.Add(ST);

			testImage.RenderTransform = TG;

			double x = GetImageX(testImage, true, ID);
			double width = GetImageWidth(testImage, true, ID);

			if (x < 0.0)
				return x;
			if (x + width > canvasDrawingArea.ActualWidth)
				return canvasDrawingArea.ActualWidth - (x + width);

			return 0.0;
		}

		private double getTransformOffsetY(Image I, int ID)
		{
			TransformGroup TG = new TransformGroup();
			RotateTransform RT = new RotateTransform();
			ScaleTransform ST = new ScaleTransform();
			Image testImage = I;

			testImage.RenderTransformOrigin = new Point(0.5, 0.5);

			RT.Angle = currentRotate;
			ST.ScaleX = _scaleValues[ID].X;
			ST.ScaleY = _scaleValues[ID].Y;

			//GetBorder().RenderTransform = new RotateTransform(RotateValues[imageID]);

			TG.Children.Add(RT);
			TG.Children.Add(ST);

			testImage.RenderTransform = TG;

			double y = GetImageY(testImage, true, ID);
			double height = GetImageHeight(testImage, true, ID);

			if (y < 0.0)
				return -y;
			if (y + height > canvasDrawingArea.ActualHeight)
				return canvasDrawingArea.ActualHeight - (y + height);

			return 0.0;
		}

		void ApplyTransform(Image I, int imageID)
		{
			Point Offset = new Point();

			if(!testTransform(I, imageID))
			{
				//double x = getTransformOffsetX(I, imageID);
				//double y = getTransformOffsetY(I, imageID);

				Offset = getTransformOffset(I, imageID);

				MoveImage(I, Offset, false);
				MoveBorder(_ImageBorder, Offset);
				//get x offset
				//get y offset
				//apply x offset
				//apply y offset
			}

			TransformGroup TG = new TransformGroup();
			RotateTransform RT = new RotateTransform();
			ScaleTransform ST = new ScaleTransform();
			
			I.RenderTransformOrigin = new Point(0.5, 0.5);

			if (GetBorder() != null)
				GetBorder().RenderTransformOrigin = new Point(0.5, 0.5);

			RT.Angle = RotateValues[imageID];
			ST.ScaleX = _scaleValues[imageID].X;
			ST.ScaleY = _scaleValues[imageID].Y;
			
			//GetBorder().RenderTransform = new RotateTransform(RotateValues[imageID]);
			
			TG.Children.Add(RT);
			TG.Children.Add(ST);

			I.RenderTransform = TG;

			//checkEdges(I, true, imageID);

			//if (_ImageBorder.RenderTransform != TG)
			//{
			Rect Bounds = GetImageBounds(I, false, imageID);
			Point BoundsPlusOffset = new Point(Bounds.TopLeft.X + Offset.X, Bounds.TopLeft.Y + Offset.Y);

			_SetBorderPosition(_ImageBorder, BoundsPlusOffset);
			_ImageBorder.Width = Bounds.Width;
			_ImageBorder.Height = Bounds.Height;
			//}
		}

		void Mirror(bool mirrorVertical)
		{
			Queue<int> ID = new Queue<int>();

			for (int i = 0; i < Designs.Count; i++)
			{
				foreach (Image I in Selected)
				{
					if (I == Designs[i])
					{
						ID.Enqueue(i);
						break;
					}
				}
			}

			if (ID.Count == 0)
				return;

			foreach (Image I in Selected)
			{
				int thisID = ID.Dequeue();

				if (mirrorVertical)
					_scaleValues[thisID] = new Point(_scaleValues[thisID].X, _scaleValues[thisID].Y * -1);
				else
					_scaleValues[thisID] = new Point(_scaleValues[thisID].X * -1, _scaleValues[thisID].Y);

				ApplyTransform(I, thisID);
			}

			//SetNewImageBorder();
		}

		private void buttonMirrorVertical_Click(object sender, RoutedEventArgs e)
		{
			if (Selected.Count == 0)
				return;

			Mirror(true);
		}

		private void buttonMirrorHorizontal_Click(object sender, RoutedEventArgs e)
		{
			if (Selected.Count == 0)
				return;

			Mirror(false);
		}

		private void windowCombineDesign_KeyUp(object sender, KeyEventArgs e)
		{
			if ((e.Key == Key.LeftCtrl) || (e.Key == Key.RightCtrl))
				_ctrlDown = false;
		}

		private void textBoxNudgeAmount_GotFocus(object sender, RoutedEventArgs e)
		{
			Selected.Clear();
			HideBorder();
		}

		private void textBoxStickinessAmount_TextChanged(object sender, TextChangedEventArgs e)
		{
			float temp = StickyValue;
			Selected.Clear();
			HideBorder();

			if (((TextBox)sender).Text == "")
				return;

			try
			{
				if (textBoxStickinessAmount.Text[textBoxStickinessAmount.Text.Length - 1] == '.')
					return;

				StickyValue = Convert.ToSingle(textBoxStickinessAmount.Text);
			}
			catch
			{
				return;
			}

			if (StickyValue > 100.0f)
				StickyValue = temp;
		}

		private void textBoxCellSize_GotFocus(object sender, RoutedEventArgs e)
		{
			Selected.Clear();
			HideBorder();
		}

		private void textBoxStickinessAmount_GotFocus(object sender, RoutedEventArgs e)
		{
			Selected.Clear();
			HideBorder();
		}

	}
}