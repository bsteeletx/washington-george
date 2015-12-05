using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
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

		public void SavePattern(String Filename, Int16 HoopSizeWidth, Int16 HoopSizeHeight, Canvas DrawingArea,  
			List<System.Windows.Media.Matrix> Matrices, List<Rect> Bounds)//List<System.Windows.Point> ScaleInfo, List<float> RotationInfo)
		{
			List<MyRect> ImageInfo = new List<MyRect>();
            //List<float[]> MatrixToFloats = new List<float[]>();
            List<Matrix> DrawingMatrices = new List<Matrix>();
            
            foreach (Rect R in Bounds)
            {
                ImageInfo.Add(new MyRect((int)R.Left, (int)R.Top, (int)R.Right, (int)R.Bottom));
            }

			foreach (System.Windows.Media.Matrix M in Matrices)
            {
                DrawingMatrices.Add(new Matrix((float)M.M11, (float)M.M12, (float)M.M21, (float)M.M22, (float)M.OffsetX, (float)M.OffsetY));
            }

			CurPattern.saveAsPes(Filename, HoopSizeWidth, HoopSizeHeight, ImageInfo, DrawingMatrices);//SDP, RotationInfo);
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