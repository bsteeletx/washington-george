using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Reflection;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows;
using CombineDesign;
using System.Windows.Controls;

namespace CombineDesign
{
	class UILogic
	{
		Pattern CurPattern = new Pattern();
		public Settings UserSettings = new Settings();
		List<System.Windows.Controls.Image> ControlImages = new
				List<System.Windows.Controls.Image>();

		public void openFiles(String[] Filenames, System.Windows.Point Center)
		{
			CurPattern.OpenDesigns(Filenames);
		}
					
		public String currentVersion()
		{
			return Assembly.GetExecutingAssembly().GetName().Version.ToString();
		}

		public void SavePattern(String Filename, Int16 HoopSizeWidth, Int16 HoopSizeHeight, Canvas DrawingArea, List<System.Windows.Controls.Image> Drawings, float ZoomLevel, 
			List<System.Windows.Media.Matrix> Matrices)//List<System.Windows.Point> ScaleInfo, List<float> RotationInfo)
		{
			List<MyRect> ImageInfo = new List<MyRect>();
			//List<System.Drawing.Point> SDP = new List<System.Drawing.Point>();
			List<float[]> MatrixToFloats = new List<float[]>();
			
			foreach (System.Windows.Controls.Image SWCI in Drawings)
			{
				int Left, Right, Bottom, Top;
				
				Left = (int)Canvas.GetLeft(SWCI);
				Right = (int)(SWCI.ActualWidth + Canvas.GetLeft(SWCI));
				Bottom = (int)(SWCI.ActualHeight + Canvas.GetTop(SWCI));
				Top = (int)Canvas.GetTop(SWCI);

				MyRect RectInfo = new MyRect(Left, Top, Right, Bottom);
				ImageInfo.Add(RectInfo);
			}

			/*foreach (System.Windows.Point SWP in ScaleInfo)
			{
				SDP.Add(new System.Drawing.Point((int)SWP.X, (int)SWP.Y));
			} */

			foreach (System.Windows.Media.Matrix M in Matrices)
			{
				float[] f4 = new float[4];

				f4[0] = (float)M.M11;
				f4[1] = (float)M.M12;
				f4[2] = (float)M.M21;
				f4[3] = (float)M.M22;

				MatrixToFloats.Add(f4);
			}

			CurPattern.saveAsPes(Filename, HoopSizeWidth, HoopSizeHeight, ImageInfo, ZoomLevel, MatrixToFloats);//SDP, RotationInfo);
		}

		public List<System.Windows.Controls.Image> GetImagesFromDesigns(Single ZoomLevel, Int32 NewImageCount)
		{
			//This gets all images--including old ones
			List<System.Drawing.Image> DrawingImages = CurPattern.GetImages(ZoomLevel, NewImageCount);
			Int32 Index = 0;

			foreach (System.Drawing.Image SDI in DrawingImages)
			{
				//if it's already added, skip it
				if (DrawingImages.Count - (NewImageCount + Index++) > 0)
					continue;

				System.Windows.Controls.Image SWCI = new 
					System.Windows.Controls.Image();
				Bitmap B = new Bitmap(SDI);
				IntPtr HBitmap = B.GetHbitmap();
				System.Windows.Media.ImageSource WPFBitmap = 
					System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap
					(HBitmap, IntPtr.Zero, 
					Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

				SWCI.Source = WPFBitmap;
				SWCI.Width = B.Width;
				SWCI.Height = B.Height;
				SWCI.Stretch = System.Windows.Media.Stretch.Fill;

				//Global Value
				ControlImages.Add(SWCI);
			}

			return ControlImages;
		}

		public void Clear()
		{
			CurPattern.Clear();
			ControlImages.Clear();
		}

		public void ClearDesign(System.Windows.Controls.Image I)
		{
			Int32 Count = 0;

			foreach (System.Windows.Controls.Image SWCI in ControlImages)
			{
				if (SWCI == I)
				{
					ControlImages.Remove(SWCI);
					break;
				}
				Count++;
			}

			CurPattern.ClearDesign(Count);
		}

		public List<System.Windows.Controls.Image> ChangeZoom(float NewZoomValue, System.Windows.Point Center, int imageCount)
		{

			List<System.Drawing.Image> DrawingImages = CurPattern.GetImages(NewZoomValue, imageCount);
			Int32 Index = 0;

			foreach (System.Drawing.Image SDI in DrawingImages)
			{
				//if it's already added, skip it
				if (DrawingImages.Count - (imageCount + Index++) > 0)
					continue;

				System.Windows.Controls.Image SWCI = new
					System.Windows.Controls.Image();
				Bitmap B = new Bitmap(SDI);
				IntPtr HBitmap = B.GetHbitmap();
				System.Windows.Media.ImageSource WPFBitmap =
					System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap
					(HBitmap, IntPtr.Zero,
					Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

				SWCI.Source = WPFBitmap;
				SWCI.Width = B.Width;
				SWCI.Height = B.Height;
				SWCI.Stretch = System.Windows.Media.Stretch.Fill;

				//Global Value
				ControlImages.Add(SWCI);
			}

			return ControlImages;
		}
	}

	class Settings
	{
		String _LastSavedDir = "";
		String _LastOpenDir = "";

		public String LastSavedDir
		{
			get	{return _LastSavedDir;}
			set	{_LastSavedDir = value;	}
		}

		public String LastOpenDir
		{
			get	{return _LastOpenDir;}
			set	{_LastOpenDir = value;	}
		}
	}
}