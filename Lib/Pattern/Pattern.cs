using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using CombineDesign;

namespace CombineDesign
{
	public class Pattern
	{
		public List<DesignFormat> Designs = new List<DesignFormat>();
		List<Point> OffsetAmount = new List<Point>();
		System.IO.BinaryWriter fileOut;
		List<Image> AllBitmaps = new List<Image>();
		int _IDCounter = 0;
		//List<float> Rotations = new List<float>();
		//List<Point> Scales = new List<Point>();
		
		public Pattern()
		{
	
		}

		public void OpenDesigns(String[] Filenames)
		{
			foreach (String S in Filenames)
			{
				Int32 FileType = -1;

				try
				{
					if (S.EndsWith(".pes") || S.EndsWith(".PES"))
						FileType = 1;
					else if (S.EndsWith(".pec") || S.EndsWith(".PEC"))
						FileType = 0;
					else if (S.EndsWith(".dst") || S.EndsWith(".DST"))
						FileType = 0;

					if (!System.IO.File.Exists(S))
						return;
				}
				catch
				{
					throw new System.ArgumentException("File Must be a PES");
				}

				switch (FileType)
				{
					case 0: //PEC
						Designs.Add(new Pec(S, _IDCounter++));
						break;
					case 1: //PES
						Designs.Add(new Pes(S, _IDCounter++));
						break;
				}

				Int32 DesignID = Designs.Count - 1;
				DesignFormat CurDesign = Designs[DesignID];

				foreach (DesignFormat DF in Designs)
				{
					DF.NumOfDesignsInPattern = Designs.Count;
				}

				if (Designs.Count == 1)
					Designs[0].SetAsPrimeDesign();

				CurDesign.read();
			}
		}

		public List<Image> GetImages(Single ZoomLevel, Int32 Count)
		{
			Int32 Index = 0;

			foreach (DesignFormat DF in Designs)
			{
				bool hideInitJump = false;

				if (Index == 0)
					hideInitJump = true;

				if (Designs.Count - (Count + Index++) > 0)
					continue;

				DF.SetPixelToMMRatio(ZoomLevel);
				AllBitmaps.Add((Image)DF.ToBitmap(2.0f, hideInitJump));
			}

			return AllBitmaps;
		}

		/*public Bitmap BitmapsToOne(Single PixelToMMRatio)
		{
			Bitmap OneBitmap;
			Int32 tempOffset = 0;
			List<Bitmap> AllBitmaps = new List<Bitmap>();
			Graphics xGraph;
			Int32 Width = 0, Height = 0;

			foreach (DesignFormat DF in Designs)
			{
				DF.SetPixelToMMRatio(PixelToMMRatio);
				Width = Math.Max(Width, DF.GetRelWidth());
				Height = Math.Max(Height, DF.GetRelHeight());
			}

			OneBitmap = new Bitmap(Width, Height);
			xGraph = Graphics.FromImage(OneBitmap);
			xGraph.Clear(Color.White);
			//xGraph.DrawRectangle(new Pen(Color.White), 0, 0, Width, Height);

			foreach (DesignFormat DF in Designs)
			{
				AllBitmaps.Add(DF.ToBitmap(2.0f));
			}

			foreach (Bitmap B in AllBitmaps)
			{
				B.MakeTransparent();
				xGraph.DrawImage(B, new Rectangle(tempOffset, 0, B.Width,
					B.Height));
				tempOffset += 25;
			}

			return OneBitmap;
		}*/
		private void ApplyAffineTransform(List<float[]> AffTrans, List<MyRect> ImageBounds)
		{
			foreach (float[] fa in AffTrans)
			{
				foreach (MyRect MR in ImageBounds)
				{
					MR.Rotate(fa);
				}
			}
		}

		public void saveAsPes(String Filename, Int16 HoopWidth, Int16 HoopHeight, List<MyRect> ImagePos, float ZoomLevel, List<float[]> MatrixFloats)//List<Point> ScaleInfo, List<float> RotationInfo)
		{
			Int64 pecLocation = 0;
			String PesOutFile = "";
			String PesFrame = "#PES0001";
			String PesHoopSizeFormat = "";
			String EmbOneHeader = "CEmbOne";
			String CSewSegHeader = "CSewSeg";
			String CSewSegFooter = "";
			String EncodedPECHeader = "";
			String PaletteSection = GetPaletteSection(Filename);
			//String EncodedPECFooter = GetPECStitchFooter();
			String EncodedBitmap = "";
			Queue<String> CSewSegSection = new Queue<String>();
			List<String> EncodedPECSection = new List<String>();
			int LastDesignID = 0;
			int counter = 0;

			foreach (DesignFormat DF in Designs)
			{
				DF.SetHoopWidth((ushort)HoopWidth);

				DF.NumOfDesignsInPattern = ImagePos.Count;
				if (DF == Designs[Designs.Count - 1])
					LastDesignID = DF.GetID();

				if (DF.GetIsSideways())
					DF.SetSideways270Offset();
#if !BASIC
										
				//Apply Transform to everything
				ApplyAffineTransform(MatrixFloats, ImagePos);	//Apply Matrix to each colorblock (and stitchblock?) to change width/height

				foreach (ColorBlock CB in DF.BlocksInDesignByColor)
					CB.BoundsUpdated();
#endif
			}

			GetOffsetAmount(ImagePos, HoopWidth, HoopHeight, ZoomLevel, MatrixFloats);   
			PesHoopSizeFormat = GetPesHoopSizeFormat(HoopWidth, HoopHeight);

			//for (int i = 0; i < ImagePos.Count; i++)
				//ImagePos[i].Rotate(RotationInfo[i]);
			
			Int32 Count = 0;
			Stitch LastStitch = null;
			bool stopCodeForPEC = true;
			Point LastOffset = new Point();
			Point CenterPoint = new Point();
			MyRect AllBounds = GetAllBounds(ImagePos, ZoomLevel, MatrixFloats);
			CenterPoint.X = AllBounds.Left;
			CenterPoint.Y = AllBounds.Top;

			foreach (DesignFormat DF in Designs)
			{
				Point PECOffset = new Point((int)(ImagePos[Count].Left/ZoomLevel), (int)(ImagePos[Count].Top/ZoomLevel));
				Point PESOffset = OffsetAmount[Count];

				ColorBlock Next = Designs[Count].GetFirstColorBlock();
				DF.SetSaveOffset(PESOffset);

				/*if (LastStitch != null && (LastStitch.Flags & 0x03) != 0)
				{
					if ((DF.BlocksInDesignByColor[0].GetStitchBlocks()[0].GetStitchList()[0].Flags & 0x03) != 0)
						EncodedPECSection[EncodedPECSection.Count - 1] = EncodedPECSection[EncodedPECSection.Count - 1].Substring(0, EncodedPECSection[EncodedPECSection.Count - 1].Length - 4);
				}*/

				CSewSegSection.Enqueue(DF.WriteNewDesignBreak(LastStitch, Next));
				CSewSegSection.Enqueue(DF.GetSewSegSection(PESOffset, LastStitch));

				EncodedPECSection.Add(DF.GetEncodedPECSection(PECOffset, LastDesignID, stopCodeForPEC, LastStitch, LastOffset, CenterPoint, AllBounds));

				if (Count == Designs.Count - 1)
					EncodedPECSection.Add(DF.GetASCII8String(1, 0xFF));

				if (Count == 0)
					EmbOneHeader += GetEmbOneHeader(PESOffset, HoopWidth,HoopHeight, MatrixFloats);

				bool lastColorFirstColorSame = false;

				int LastStitchColor = Designs[Count].GetLastColorIndex();
				LastStitch = Designs[Count++].GetLastColorBlock().GetLastStitchInColorBlock();// GetStitchBlocks()[lastRealStitchBlock - 1].GetLastStitch();

				int FirstStitchColor = 0;

				try
				{
					FirstStitchColor = Designs[Count].GetFirstStitchBlock().GetColorIndex();
				}
				catch
				{
					FirstStitchColor = -1;
				}
				
				lastColorFirstColorSame = (LastStitchColor == FirstStitchColor);

				/*byte modifier = 1;

				if (Count == 1)
					modifier = 0;*/
				//odd change, even stay the same
				byte change = 0;

				if (!lastColorFirstColorSame)
				{
					change = (byte)((DF.GetColorTotal()) % 2);

					if (change == 1)
						stopCodeForPEC = !stopCodeForPEC;
				}
	
				LastOffset = PECOffset;
			}

			OffsetAmount.Clear();

			EncodedBitmap += GetBitmapSection(ImagePos, HoopWidth, HoopHeight, AllBitmaps, ZoomLevel);

			CSewSegFooter += GetCSewSegFooter(Designs[Designs.Count - 1].GetLastColorIndex());
			
/////////////////////Time to put it together!!//////////////////

			Int32 CSewSegLength = 0;
			Int32 PECStitchLength = 0;
			//Int32 ImageByteLength = 0;

			foreach (String S in CSewSegSection)
			{
				CSewSegLength += S.Length;
			}

			foreach (string S in EncodedPECSection)
			{
				PECStitchLength += S.Length;
			}

			// + 4 for the newly added 4 Byes the string once inserted, not sure				where the other 2 come from
			pecLocation = PesFrame.Length + PesHoopSizeFormat.Length + 
				EmbOneHeader.Length + CSewSegHeader.Length + CSewSegLength +
				CSewSegFooter.Length + 6;
			
			PesFrame += Designs[0].GetASCII8String(4, pecLocation);
			PesFrame += PesHoopSizeFormat;
			//Part of EmbOneHeader, but easierif we put it here
			PesFrame += Designs[0].GetASCII8String(2, 7);
			
			//now we have the value that we need to finish the stitch section
			Int32 GraphicsOffsetLocation = PECStitchLength + 16; //16 for the				following 16 Bytes

			//finishing stitch section
			EncodedPECHeader += GetPECStitchHeader(GraphicsOffsetLocation, HoopWidth, HoopHeight, ZoomLevel, ImagePos, MatrixFloats);
			
			PesOutFile = PesFrame + EmbOneHeader + CSewSegHeader;

			foreach (String S in CSewSegSection)
			{
				PesOutFile += S;
			}

			PesOutFile += CSewSegFooter + PaletteSection + EncodedPECHeader;

			foreach (String S in EncodedPECSection)
			{
				PesOutFile += S;
			}

			//PesOutFile += EncodedPECFooter;

			/*foreach (Byte[] B in ImageAsBytes)
			{
				foreach (Byte By in B)
				{
					PesOutFile += Designs[0].GetASCII8String(1, By);
				}
			}*/

			PesOutFile += EncodedBitmap;

			StartWriter(Filename);

			Encoding enc = Encoding.GetEncoding("IBM437");
			Byte[] tempBytes = enc.GetBytes(PesOutFile);

			fileOut.Write(tempBytes);
			fileOut.Close();
		}

		string GetCSewSegFooter(int lastColorIndex)
		{
			string footer = "";

			footer += Designs[0].GetASCII8String(4, 1);
			footer += Designs[0].GetASCII8String(2, lastColorIndex);
			footer += Designs[0].GetASCII8String(4, 0);

			return footer;
		}

		string GetPECStitchHeader(int graphicsOffset, int HoopWidth, int HoopHeight, float ZoomLevel, List<MyRect> ImagePos, List<float[]> MatrixFloats)
		{
			string header = "";

			header += Designs[0].GetASCII8String(2, 0x00);
			header += Designs[0].GetASCII8String(3,	graphicsOffset);
			header += Designs[0].GetASCII8String(1, 0x31);
			header += Designs[0].GetASCII8String(1, 0xFF);
			header += Designs[0].GetASCII8String(1, 0xF0);

			MyRect AllBounds = GetAllBounds(ImagePos, ZoomLevel, MatrixFloats);

			if (HoopWidth <= HoopHeight)
			{
				header += Designs[0].GetASCII8String(2, AllBounds.Width);
				header += Designs[0].GetASCII8String(2, AllBounds.Height);
			}
			else
			{
				header += Designs[0].GetASCII8String(2, AllBounds.Height);
				header += Designs[0].GetASCII8String(2, AllBounds.Width);
			}

			header += Designs[0].GetASCII8String(2, 0x1E0);
			header += Designs[0].GetASCII8String(2, 0x1B0);

			return header;
		}

		int GetHoopWidthInMM(int HoopWidth)
		{
			switch (HoopWidth)
			{
				case 4: return 1000;
				case 5: return 1300;
				case 7: return 1800;
			}

			return 0;
		}

		int GetHoopHeightInMM(int HoopHeight)
		{
			switch (HoopHeight)
			{
				case 4: return 1000;
				case 5: return 1300;
				case 7: return 1800;
			}

			return 0;
		}

		void GetBitmapFrame(byte[,] BlankBytes)
		{
			for (int i = 0; i < 38; i++)
			{
				for (int j = 0; j < 6; j++)
					BlankBytes[i, j] = 0;
			}

			for (int i = 0; i < 38; i++)
			{
				if (i == 0 || i == 37)
					continue;
				if (i == 1 || i == 36)
				{
					BlankBytes[i, 5] = 0x0f;
					BlankBytes[i, 4] = 0xff;
					BlankBytes[i, 3] = 0xff;
					BlankBytes[i, 2] = 0xff;
					BlankBytes[i, 1] = 0xff;
					BlankBytes[i, 0] = 0xf0;
				}
				else if (i == 2 || i == 35)
				{
					BlankBytes[i, 5] = 0x10;
					BlankBytes[i, 4] = 0x00;
					BlankBytes[i, 3] = 0x00;
					BlankBytes[i, 2] = 0x00;
					BlankBytes[i, 1] = 0x00;
					BlankBytes[i, 0] = 0x08;
				}
				else if (i == 3 || i == 34)
				{
					BlankBytes[i, 5] = 0x20;
					BlankBytes[i, 4] = 0x00;
					BlankBytes[i, 3] = 0x00;
					BlankBytes[i, 2] = 0x00;
					BlankBytes[i, 1] = 0x00;
					BlankBytes[i, 0] = 0x04;
				}
				else
				{
					BlankBytes[i, 5] = 0x40;
					BlankBytes[i, 4] = 0x00;
					BlankBytes[i, 3] = 0x00;
					BlankBytes[i, 2] = 0x00;
					BlankBytes[i, 1] = 0x00;
					BlankBytes[i, 0] = 0x02;
				}
			}
		}

		List<MyRect> GetImageBounds(List<MyRect> DesignPos, float Zoom)
		{
			List<MyRect> Values = new List<MyRect>();

			foreach (MyRect MR in DesignPos)
			{
				MyRect Temp = new MyRect((int)(MR.Left / Zoom), (int)(MR.Top / Zoom), (int)(MR.Right / Zoom), (int)(MR.Bottom / Zoom));
				Values.Add(Temp);
			}

			return Values;
		}

		List<List<List<Point>>> GetScaledPoints(int scaledWidth, int scaledHeight, int fullWidth, int fullHeight, float zoom, List<MyRect> ImageBounds)
		{
			int Counter = 0;
			List<List<List<Point>>> Values = new List<List<List<Point>>>();

			if (ImageBounds.Count > 1)
			{
				foreach (DesignFormat DF in Designs)
				{
					Values.Add(DF.ScaleDesignByColor(scaledWidth, scaledHeight,	GetHoopWidthInMM(fullWidth), GetHoopHeightInMM(fullHeight), -1.0f, new Point(ImageBounds[Counter].Left,
						ImageBounds[Counter].Top)));

					Counter++;
				}
			}
			else //use zoom
				Values.Add(Designs[0].ScaleDesignByColor(scaledWidth, scaledHeight, GetHoopWidthInMM(fullWidth), GetHoopHeightInMM(fullHeight), zoom, new Point(0, 0)));

			return Values;
		}

		short GetRatioedOffset(int xOrYValue, int HoopWidth)
		{
			short ratioedOffset = 0;

			if (Designs.Count > 1)
			{
				if (HoopWidth == 4)
					ratioedOffset = (short)(0.032 * xOrYValue);
				else
					ratioedOffset = (short)(0.020 * xOrYValue);
			}

			return ratioedOffset;
		}

		void SetThisBitmapByte(Byte[,] ToSet, Point ToChange, int HoopWidth)
		{
			byte add = 0;
			short col = 0;

			try
			{
							
				if (HoopWidth == 4)
				{
					//if only one design, center it
					if (Designs.Count == 1)
					{
						add = GetStitchValue(ToChange.X + 5);
						col = GetColumn(ToChange.X + 5);
						ToSet[ToChange.Y + 3, col] |= add;
					}
					else
					{
						add = GetStitchValue(ToChange.X + 5);
						col = GetColumn(ToChange.X + 5);
						ToSet[ToChange.Y + 3, col] |= add;
					}
								
				}
				else// if (HoopWidth == 5)
				{
					//if only one design, center it
					if (Designs.Count == 1)
					{
						add = GetStitchValue(ToChange.X + 13);
						col = GetColumn(ToChange.X + 13);
						ToSet[ToChange.Y + 3, col] |= add;
					}
					else
					{
						add = GetStitchValue(ToChange.X + 13);
						col = GetColumn(ToChange.X + 13);
						ToSet[ToChange.Y + 3, col] |= add;
					}
								
				}
			}
			catch
			{
			}

			if (col == 0 && add == 1)
				throw new Exception("Bitmap in Frame, Column 0");
			else if (col == 5 && add == 128)
				throw new Exception("Bitmap in Frame, Column 5");
			else if (col == 0 && ToChange.Y + 3 == 3 && add < 4)
				throw new Exception("Bitmap in Frame, Column 0, Row 3");
			else if (col == 5 && ToChange.Y + 3 == 3 && add > 32)
				throw new Exception("Bitmap in Frame, Column 5, Row 3");
			else if (col == 0 && ToChange.Y + 3 == 0x022 && add < 4)
				throw new Exception("Bitmap in Frame, Column 0, Row 0x22");
			else if (col == 5 && ToChange.Y + 3 == 0x022 && add > 32)
				throw new Exception("Bitmap in Frame, Column 5, Row 0x22");
			else if (ToChange.Y + 3 < 3)
				throw new Exception("Bitmap in Frame, Rows 0-2");
			else if (ToChange.Y + 3 >= 0x023)
				throw new Exception("Bitmap in Frame, Rows 0x023-0x025");
		}

		String GetBitmapSection(List<MyRect> DesignPos, Int32 HoopWidth, Int32 HoopHeight, List<Image> Images, float Zoom)
		{
			byte[,] ThumbnailBytes = new byte[38, 6];
			String EncodedSection = "";
			int Counter = 0;
			List<MyRect> ImageBounds = new List<MyRect>();
			Queue<byte[,]> BytesByColor = new Queue<byte[,]>();
			int BITMAP_WIDTH = 31; //max is 40
			int BITMAP_HEIGHT = 31; //max is 32
			List<List<List<Point>>> ScaledPoints = new List<List<List<Point>>>
				();
			int XRatioedOffset = 0;
			int YRatioedOffset = 0;

						
			if (HoopWidth != 4)
			{
				if (HoopWidth == 5)
				{
					BITMAP_WIDTH = 22;
					BITMAP_HEIGHT = 31;
				}
				else
				{
					BITMAP_WIDTH = 31;
					BITMAP_HEIGHT = 22;
				}
			}

			ImageBounds = GetImageBounds(DesignPos, Zoom);
			ScaledPoints = GetScaledPoints(BITMAP_WIDTH, BITMAP_HEIGHT, HoopWidth, HoopHeight, Zoom, ImageBounds);

			byte[,] BitmapBytes = new byte[38, 6];
			bool FrameSet = false;

			foreach (DesignFormat DF in Designs)
			{
				List<List<Point>> DesignPoints = ScaledPoints[Counter];
				int ColorCounter = 0;

				XRatioedOffset = GetRatioedOffset(DesignPos[Counter].Left, 
					HoopWidth);
				YRatioedOffset = GetRatioedOffset(DesignPos[Counter].Top, 
					HoopWidth);

				foreach (ColorBlock CB in DF.BlocksInDesignByColor)
				{
					List<Point> ColorPoints = DesignPoints[ColorCounter++];

					if (!FrameSet)
					{
						GetBitmapFrame(BitmapBytes);
						FrameSet = true;
					}

					foreach (Point P in ColorPoints)
					{
						Point OffP = P;
						//OffP.X += XRatioedOffset;
						//OffP.Y += YRatioedOffset;

						SetThisBitmapByte(BitmapBytes, OffP, HoopWidth);
					}

					if (AreBitmapBytesQueable(CB, Counter))
					{
						BytesByColor.Enqueue(BitmapBytes);
						BitmapBytes = new Byte[38, 6];
						FrameSet = false;
					}
				}
				
				Counter++;
			}

			String StepSection = GetBitmapStepSection(BytesByColor, 
				ThumbnailBytes);

			EncodedSection = GetThumbnailSection(ThumbnailBytes);
			EncodedSection += StepSection;

			return EncodedSection;
		}

		bool AreBitmapBytesQueable(ColorBlock CB, int DesignCount)
		{
			bool value = true;
			
			if (CB == Designs[DesignCount].GetLastColorBlock())
				value = false;

			if (!value)
			{
				try
				{
					value = CB.ColorIndex != Designs[DesignCount +
						1].BlocksInDesignByColor[0].ColorIndex;
				}
				catch
				{
					value = true;
				}
			}

			return value;
		}

		String GetThumbnailSection(byte[,] Thumbnails)
		{
			String ThumbnailSection = "";

			for (int i = 0; i < 38; i++)
			{
				for (int j = 0; j < 6; j++)
					ThumbnailSection += Designs[0].GetASCII8String(1,
						Thumbnails[i, j]);
			}

			return ThumbnailSection;
		}

		String GetBitmapStepSection(Queue<byte[,]> BSection, byte[,] Thumbnails)
		{
			String StepSection = "";

			foreach (Byte[,] TDBA in BSection)
			{
				for (int i = 0; i < 38; i++)
				{
					for (int j = 0; j < 6; j++)
					{
						StepSection += Designs[0].GetASCII8String(1, TDBA[i,
							j]);
						Thumbnails[i, j] |= TDBA[i, j];
					}
				}
			}

			return StepSection;
		}

		/*Queue<byte> GetBitmapBytes(Bitmap ToByte)
		{
			System.IO.MemoryStream stream = new System.IO.MemoryStream();

			ToByte.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);

			stream.Position = 0;
			byte[] data = new byte[stream.Length];
			stream.Read(data, 0, (int)stream.Length);

			Queue<byte> Value = new Queue<byte>();

			foreach (byte B in data)
			{
				Value.Enqueue(B);
			}

			return Value;
		}*/

		Bitmap ScaleImage(Bitmap B, int width, int height, int canvasWidth, int 
			canvasHeight, float ZoomLevel)
		{
			float widthToCanvas = (B.Width * (1/ZoomLevel)) / canvasWidth;
			float heightToCanvas = (B.Height * (1/ZoomLevel)) / canvasHeight;
			float ratio = Math.Min(widthToCanvas, heightToCanvas);

			int newWidth = (int)(width * ratio);
			int newHeight = (int)(height * ratio);

			Bitmap Value = new Bitmap(newWidth, newHeight);

			//is this entirely needed? it seems like it gets displayed
			Graphics.FromImage(Value).DrawImage(B, 0, 0, newWidth, newHeight);
			
			return Value;
		}

		short GetColumn(int x)
		{	
			float value = (float)(x + 0.0f) / 8.0f;
			return (short)Math.Floor(value);
		}

		byte GetStitchValue(int value)
		{
			int column = (value + 0) % 8;

			switch (column)
			{
				/*case 0: Value += 128; break;
				case 1: Value += 64; break;
				case 2: Value += 32; break;
				case 3: Value += 16; break;
				case 4: Value += 8; break;
				case 5: Value += 4; break;
				case 6: Value += 2; break;
				case 7: Value += 1; break;*/
				case 0: return 1;
				case 1: return 2;
				case 2: return 4;
				case 3: return 8;
				case 4: return 16; 
				case 5: return 32;
				case 6: return 64;
				case 7: return 128;
				default: return 0xFF;
			}
		}

		byte GetPixel(int row, int column, Bitmap ToCheck, Color ForColor, 
			MyRect Bounds)
		{
			byte Value = 0;
			int col = column * 8;
			int colMod = 0;
			
			if (row > Bounds.Bottom - 1)
				return 0;
			if (row < Bounds.Top)
				return 0;

			for (int i = 0; i < 8; i++)
			{
				if (column == 0)
				{
					if (i < 3)
						continue;
				}
				else if (column == 5)
				{
					if (i > 4)
						continue;
				}
				
				colMod = i - 3;


				if (col + colMod < Bounds.Left)
					continue;
				if (col + colMod > Bounds.Right - 1)
					return Value;

				Color Test = ToCheck.GetPixel(col + colMod, row);

				if (Test == ForColor)
				{
					switch (i)
					{
						case 0: Value += 128; break;
						case 1: Value += 64; break;
						case 2: Value += 32; break;
						case 3: Value += 16; break;
						case 4: Value += 8; break;
						case 5: Value += 4; break;
						case 6: Value += 2; break;
						case 7: Value += 1; break;
						/*case 0: Value += 1; break;
						case 1: Value += 2; break;
						case 2: Value += 4; break;
						case 3: Value += 8; break;
						case 4: Value += 16; break;
						case 5: Value += 32; break;
						case 6: Value += 64; break;
						case 7: Value += 128; break;*/
					}
				}
			}

			return Value;
		}

		byte GetFrame(int row, int column)
		{
			if (row == 1 || row == 36)
			{
				if (column == 0)
					return 0xF0;
				if (column == 5)
					return 0x0F;

				return 0xFF;
			}
			if (row == 2 || row == 35)
			{
				if (column == 0)
					return 0x08;
				if (column == 5)
					return 0x10;
			}
			if (row == 3 || row == 34)
			{
				if (column == 0)
					return 0x04;
				if (column == 5)
					return 0x20;
			}
			
			return 0;
		}

		MyRect GetAllBounds(List<MyRect> ImagePos, float ZoomLevel, List<float[]> MatrixFloats)
		{
			List<MyRect> ImageValues = new List<MyRect>();

			foreach (MyRect MR in ImagePos)
			{
				MyRect Value = new MyRect(MR);
				Value *= (1 / ZoomLevel);
				ImageValues.Add(Value);
			}
			
			MyRect FirstBounds = new MyRect(Designs[0].GetBoundsOfDesign());
			FirstBounds.Rotate(MatrixFloats[0]);
			int counter = 1;
			Int32 LeftValue = ImageValues[0].Left + FirstBounds.Left;
			Int32 RightValue = ImageValues[0].Left + FirstBounds.Right;
			Int32 TopValue = ImageValues[0].Top + FirstBounds.Top;
			Int32 BottomValue = ImageValues[0].Top + FirstBounds.Bottom;


			foreach (DesignFormat DF in Designs)
			{
				if (DF == Designs[0])
					continue;

				MyRect Test = new MyRect(DF.GetBoundsOfDesign());
				MyRect ImageLoc = new MyRect(ImageValues[counter++]);

				if (DF.GetIsSideways())
				{
					Test.Top += Math.Abs(DF.SidewaysOffset);
					Test.Bottom += Math.Abs(DF.SidewaysOffset);
				}

				LeftValue = Math.Min(LeftValue, Test.Left + ImageLoc.Left);
				RightValue = Math.Max(RightValue, Test.Right + ImageLoc.Left);
				TopValue = Math.Min(TopValue, Test.Top + ImageLoc.Top);
				BottomValue = Math.Max(BottomValue, Test.Bottom + ImageLoc.Top);

			}

			return new MyRect(LeftValue, TopValue, RightValue, BottomValue);
		}

		String GetPECStitchFooter()
		{
			String Footer = "";

			Footer += Designs[0].GetASCII8String(2, 0);
			Footer += Designs[0].GetASCII8String(2, 0);
			Footer += Designs[0].GetASCII8String(2, 0);
			Footer += Designs[0].GetASCII8String(1, 0xF0);
			Footer += Designs[0].GetASCII8String(4, -1);
			Footer += Designs[0].GetASCII8String(1, 0x0F);

			return Footer;
		}

		String GetPaletteSection(String Filename)
		{
			String FilenameBase = Filename.Substring(0, Filename.Length - 4);
			Int32 LastSlash = FilenameBase.LastIndexOf('\\');
			
			FilenameBase = FilenameBase.Substring(LastSlash + 1);
			
			Int32 DotPos = FilenameBase.Length;
			String Stitches = "LA:" + FilenameBase;

			if (Stitches.Length > 11)
			{
				Stitches = Stitches.Substring(0, 11);
				FilenameBase = FilenameBase.Substring(0, 8);
			}
						
			for (int i = 0; i < 16 - FilenameBase.Length; i++)
				Stitches += Designs[0].GetASCII8String(1, 0x20);

			Stitches += Designs[0].GetASCII8String(1, 0x0D);

			for (int i = 0; i < 12; i++)
				Stitches += Designs[0].GetASCII8String(1, 0x20);

			Stitches += Designs[0].GetASCII8String(1, 0xFF);
			Stitches += Designs[0].GetASCII8String(1, 0x00);
			Stitches += Designs[0].GetASCII8String(1, 0x06);
			Stitches += Designs[0].GetASCII8String(1, 0x26);

			for (int i = 0; i < 4; i++)
				Stitches += Designs[0].GetASCII8String(1, 0x20);

			Stitches += Designs[0].GetASCII8String(1, 0x64);
			Stitches += Designs[0].GetASCII8String(1, 0x20);
			Stitches += Designs[0].GetASCII8String(1, 0x00);
			Stitches += Designs[0].GetASCII8String(1, 0x20);
			Stitches += Designs[0].GetASCII8String(1, 0x00);
			Stitches += Designs[0].GetASCII8String(1, 0x20);
			Stitches += Designs[0].GetASCII8String(1, 0x20);
			Stitches += Designs[0].GetASCII8String(1, 0x20);

			Int32 currentThreadCount = 0;
			short currentColorIndex = -1;

			foreach (DesignFormat DF in Designs)
			{
				if (currentColorIndex == DF.GetFirstColorBlock().ColorIndex)
					currentThreadCount--;

				currentThreadCount += DF.GetColorTotal(true);
				currentColorIndex = DF.GetLastColorIndex();
			}

			Stitches += Designs[0].GetASCII8String(1, currentThreadCount - 1);
			currentColorIndex = -1;

			foreach (DesignFormat DF in Designs)
			{
				foreach (Thread T in DF.GetThreads())
				{
					if (currentColorIndex == T.ColorInIndex)
						continue;

					Stitches += DF.GetASCII8String(1, T.ColorInIndex);
					currentColorIndex = T.ColorInIndex;
				}
			}

			for (int i = 0; i < (int)(0x1CF - currentThreadCount); i++)
				Stitches += Designs[0].GetASCII8String(1, 0x20);

			return Stitches;
		}

		public MyRect RotateBounds(float degrees, MyRect CurrentBounds, int hoopWidth)
		{
			int left, top, right, bottom, maxHeight = 0;
			int maxWidth = 0;
			
			switch (hoopWidth)
			{
				case 4:
					maxWidth = maxHeight = 1000;
					break;
				case 5:
					maxWidth = 1300;
					maxHeight = 1800;
					break;
				case 7:
					maxWidth = 1800;
					maxHeight = 1300;
					break;
			}
						
			left = (int)((CurrentBounds.Left - CurrentBounds.Center.X) * Math.Cos(Designs[0].ToRadians(degrees)) - ((CurrentBounds.Top - CurrentBounds.Center.Y) * Math.Sin(Designs[0].ToRadians(degrees))));
			top = (int)((CurrentBounds.Left - CurrentBounds.Center.X) * Math.Sin(Designs[0].ToRadians(degrees)) + ((CurrentBounds.Top - CurrentBounds.Center.Y) * Math.Cos(Designs[0].ToRadians(degrees))));
			right = (int)((CurrentBounds.Right - CurrentBounds.Center.X) * Math.Cos(Designs[0].ToRadians(degrees)) - ((CurrentBounds.Bottom - CurrentBounds.Center.Y) * Math.Sin(Designs[0].ToRadians(degrees))));
			bottom = (int)((CurrentBounds.Right - CurrentBounds.Center.X) * Math.Sin(Designs[0].ToRadians(degrees)) + ((CurrentBounds.Bottom - CurrentBounds.Center.Y) * Math.Cos(Designs[0].ToRadians(degrees))));
				
			left += CurrentBounds.Center.X;
			top += CurrentBounds.Center.Y;
			right += CurrentBounds.Center.X;
			bottom += CurrentBounds.Center.Y;

			if (right < left)
			{
				int temp = left;
				left = right;
				right = temp;
			}
			
			if (bottom < top)
			{
				int temp = top;
				top = bottom;
				bottom = temp;
			}
			
			if (left < 0)
			{
				right += Math.Abs(left);
				left = 0;
			}
			else if (right > maxWidth)
			{
				left += maxWidth - right;
				right = maxWidth;
			}

			if (top < 0)
			{
				bottom += Math.Abs(top);
				top = 0;
			}
			else if (bottom > maxHeight)
			{
				top += maxHeight - bottom;
				bottom = maxHeight;
			}

			return new MyRect(left, top, right, bottom);
		}

		Point MultiplyPointByMatrix(Point P, float[] matrix)
		{
			Point ReturnPoint = new Point();

			ReturnPoint.X = (int)((P.X * matrix[0]) + (P.Y * matrix[2]));
			ReturnPoint.Y = (int)((P.X * matrix[1]) + (P.Y * matrix[3]));

			return ReturnPoint;
		}

		void GetOffsetAmount(List<MyRect> ImageBoxes, Int32 HoopWidth, Int32 HoopHeight, float zoomValue, List<float[]> MatrixValues)//, List<float>RotateValues)
		{
			Int32 YOffset = 0;
			Int32 XOffset = 0;
			Queue<Int32> ImageXOffsets = new Queue<int>();
			Queue<Int32> ImageYOffsets = new Queue<int>();
			Queue<int> ImageWidths = new Queue<int>();
			Queue<int> ImageHeights = new Queue<int>();
			Queue<Point> TransformedPoint = new Queue<Point>();
			int counter = 0;
			
			foreach (MyRect R in ImageBoxes)
			{
				//MyRect UpdatedR = RotateBounds(RotateValues[count++], R, HoopWidth);
				ImageXOffsets.Enqueue((int)(R.Left / zoomValue));
				ImageYOffsets.Enqueue((int)(R.Top / zoomValue));
				ImageWidths.Enqueue((int)((R.Right / zoomValue) - (R.Left / zoomValue)));
				ImageHeights.Enqueue((int)((R.Bottom / zoomValue) - (R.Top / zoomValue)));
			
				///General Offset applied to all
				switch (HoopWidth)
				{
					case 4:
						TransformedPoint.Enqueue(MultiplyPointByMatrix(new Point(0x1F4, 0x1F4), MatrixValues[counter++]));
						break;
					case 5:
						TransformedPoint.Enqueue(MultiplyPointByMatrix(new Point(0x15E, 0x64), MatrixValues[counter++]));
						break;
					case 7:
						TransformedPoint.Enqueue(MultiplyPointByMatrix(new Point(0x64, 0x15E), MatrixValues[counter++]));
						break;
				}
			}

			foreach (DesignFormat DF in Designs)
			{
				int ImageXOffset = ImageXOffsets.Dequeue();
				int ImageYOffset = ImageYOffsets.Dequeue();
				int NewOffsetAmountX = 0;//ImageXOffset + XOffset;
				int NewOffsetAmountY = 0;// ImageYOffset + YOffset;
				Point TPoint = TransformedPoint.Dequeue();
				int Width = ImageWidths.Dequeue();
				int Height = ImageHeights.Dequeue();

				if (TPoint.X < 0 && TPoint.Y > 0)
				{
					NewOffsetAmountY = TPoint.X - Height - ImageXOffset;
					NewOffsetAmountX = ImageYOffset + TPoint.Y;
				}
				else if (TPoint.Y < 0 && TPoint.X > 0)
				{
					NewOffsetAmountX = TPoint.Y - Width - ImageYOffset;
					NewOffsetAmountY = ImageXOffset + TPoint.X;
				}
				else if (TPoint.X < 0 || TPoint.Y < 0)
				{
					NewOffsetAmountY = TPoint.X - Height - ImageYOffset;
					NewOffsetAmountX = TPoint.Y - Width - ImageXOffset;
				}
				else
				{
					NewOffsetAmountX = ImageXOffset + TPoint.X;
					NewOffsetAmountY = ImageYOffset + TPoint.Y;
				}
				
				Point NewOffset = new Point(NewOffsetAmountX, NewOffsetAmountY);

				OffsetAmount.Add(NewOffset);
			}
		}

		public void Clear()
		{
			Designs.Clear();
			OffsetAmount.Clear();
			AllBitmaps.Clear();
		}

		public void ClearDesign(Int32 Count)
		{
			if (Designs[Count].IsPrimeDesign())
			{
				int minID = 9999;
				
				foreach (DesignFormat DF in Designs)
				{
					if (Designs[Count] == DF)
						continue;

					minID = Math.Min(DF.GetID(), minID);
				}

				foreach (DesignFormat DF in Designs)
				{
					if (DF.GetID() == minID)
						DF.SetAsPrimeDesign();
				}
			}
			if (Designs.Count != 0)
				Designs.RemoveAt(Count);
			if (OffsetAmount.Count != 0)
				OffsetAmount.RemoveAt(Count);
			if (AllBitmaps.Count != 0)
				AllBitmaps.RemoveAt(Count);
		}

		String GetEmbOneHeader(Point Offset, Int32 HoopWidth, Int32 HoopHeight, List<float[]> MatrixValues)
		{
			String EmbOneHeader = "";
			MyRect FirstColorBlockBounds = Designs[0].GetFirstColorBlock().GetColorBounds();
			FirstColorBlockBounds.Rotate(MatrixValues[0]);

			//if (Designs[0].GetIsSideways())
			//{
				//Offset.Y += Designs[0].SidewaysOffset;
			//}

			int LeftValue = FirstColorBlockBounds.Left + Offset.X;
			Int32 TopValue = FirstColorBlockBounds.Top + Offset.Y;
			Int32 RightValue = FirstColorBlockBounds.Right + Offset.X;
			Int32 BottomValue = FirstColorBlockBounds.Bottom + Offset.Y;
			int WidthValue = FirstColorBlockBounds.Width;
			int HeightValue = FirstColorBlockBounds.Height;
			Int32 OffsetValue = 0;

			//First Section not entire Design
			/*foreach (Rectangle R in ImageBoxes)
			{
				LeftValue = Math.Min(R.Left, LeftValue);
				TopValue = Math.Min(R.Top, TopValue);
				RightValue = Math.Max(R.Right, RightValue);
				BottomValue = Math.Max(R.Bottom, BottomValue);
			}*/

			//might only work on 7x5 :(
			if (HoopWidth > HoopHeight)
				OffsetValue = 0x78;

			EmbOneHeader = Designs[0].GetASCII8String(2, LeftValue);
			EmbOneHeader += Designs[0].GetASCII8String(2, TopValue);
			EmbOneHeader += Designs[0].GetASCII8String(2, RightValue + 
				OffsetValue);
			EmbOneHeader += Designs[0].GetASCII8String(2, BottomValue + 
				OffsetValue);
			//Duplicates for some reason
			EmbOneHeader += Designs[0].GetASCII8String(2, LeftValue);
			EmbOneHeader += Designs[0].GetASCII8String(2, TopValue);
			EmbOneHeader += Designs[0].GetASCII8String(2, RightValue);
			EmbOneHeader += Designs[0].GetASCII8String(2, BottomValue);
			EmbOneHeader += GetAffineTransform(HoopWidth, HoopHeight, MatrixValues);
			EmbOneHeader += Designs[0].GetASCII8String(2, 1);
			EmbOneHeader += Designs[0].GetASCII8String(2, LeftValue);
			EmbOneHeader += Designs[0].GetASCII8String(2, BottomValue);
			EmbOneHeader += Designs[0].GetASCII8String(2, WidthValue);
			EmbOneHeader += Designs[0].GetASCII8String(2, HeightValue);
			EmbOneHeader += Designs[0].GetASCII8String(8, 0);
			EmbOneHeader += Designs[0].GetASCII8String(2, (Designs
				[0].GetJumpCount(0) * 2) + 1);
			EmbOneHeader += Designs[0].GetASCII8String(2, -1);
			EmbOneHeader += Designs[0].GetASCII8String(2, 0);

			//part of CSewSeg, but easier to place here
			EmbOneHeader += Designs[0].GetASCII8String(2, 7);

			return EmbOneHeader;
		}

		String GetAffineTransform(Int32 HoopWidth, Int32 HoopHeight, List<float[]> MatrixValues)
		{
			String AffineTransform = "";

			//I've seen this be different only occaisionally
			//UPDATE: Different if saved by Embird vs. Brother, going with Brother
			if (HoopWidth <= HoopHeight)
			{
				/*AffineTransform += Designs[0].GetASCII8String(2, 0);
				AffineTransform += Designs[0].GetASCII8String(2, 16256); //0x3F80
				AffineTransform += Designs[0].GetASCII8String(2, 0);
				AffineTransform += Designs[0].GetASCII8String(2, 0);
				AffineTransform += Designs[0].GetASCII8String(2, 0);
				AffineTransform += Designs[0].GetASCII8String(2, 0);
				AffineTransform += Designs[0].GetASCII8String(2, 0);
				AffineTransform += Designs[0].GetASCII8String(2, 16256); //0x3F80
				AffineTransform += Designs[0].GetASCII8String(2, 0);
				AffineTransform += Designs[0].GetASCII8String(2, 0);
				AffineTransform += Designs[0].GetASCII8String(2, 0);
				AffineTransform += Designs[0].GetASCII8String(2, 0);*/

				for (int i = 0; i < 4; i++)
					AffineTransform += Designs[0].GetASCII8String(4, MatrixValues[0][i]);

				AffineTransform += Designs[0].GetASCII8String(2, 0);
				AffineTransform += Designs[0].GetASCII8String(2, 0);
				AffineTransform += Designs[0].GetASCII8String(2, 0);
				AffineTransform += Designs[0].GetASCII8String(2, 0);
			}
			else //might need more here, this should work for 7x5 though
			{
				AffineTransform += Designs[0].GetASCII8String(2, 2886); //0x0B46
				AffineTransform += Designs[0].GetASCII8String(2, 12881); //0x3251
				AffineTransform += Designs[0].GetASCII8String(2, 0);
				AffineTransform += Designs[0].GetASCII8String(2, 16256); //0x3F80
				AffineTransform += Designs[0].GetASCII8String(2, 0);
				AffineTransform += Designs[0].GetASCII8String(2, -16512); //0xBF80
				/////////////////Split//////////////////////
				AffineTransform += Designs[0].GetASCII8String(2, 2886); //0x0B46
				AffineTransform += Designs[0].GetASCII8String(2, 12881); //0x3251
				AffineTransform += Designs[0].GetASCII8String(2, 0);
				AffineTransform += Designs[0].GetASCII8String(2, 17658); //0x44FA
				AffineTransform += Designs[0].GetASCII8String(2, 0);
				AffineTransform += Designs[0].GetASCII8String(2, 0);
			}

			return AffineTransform;
		}

		Int32 GetColorCount()
		{
			Int32 SectionCount = 0;

			foreach (DesignFormat DF in Designs)
			{
				SectionCount += DF.GetColorTotal();
			}

			return SectionCount;
		}

		String GetPesHoopSizeFormat(Int16 HoopWidth, Int16 HoopHeight)
		{
			String HoopSizeFormat = "";
			
			switch (HoopWidth)
			{
				case 4:
					HoopSizeFormat += Designs[0].GetASCII8String(2, 0);
					HoopSizeFormat += Designs[0].GetASCII8String(2, 1);

					HoopSizeFormat += Designs[0].GetASCII8String(2, GetColorCount());
					HoopSizeFormat += Designs[0].GetASCII8String(2, -1);
					HoopSizeFormat += Designs[0].GetASCII8String(2, 0);
					break;
				case 5:
					HoopSizeFormat += Designs[0].GetASCII8String(2, 1);
					HoopSizeFormat += Designs[0].GetASCII8String(2, 1);
					
					HoopSizeFormat += Designs[0].GetASCII8String(2,	GetColorCount());
					HoopSizeFormat += Designs[0].GetASCII8String(2, -1);
					HoopSizeFormat += Designs[0].GetASCII8String(2, 0);
					break;
				case 6:
					HoopSizeFormat += Designs[0].GetASCII8String(2, 1);
					HoopSizeFormat += Designs[0].GetASCII8String(2, 12592); //this actually says 01, it might be the number of Designs
					HoopSizeFormat += Designs[0].GetASCII8String(2, 0);
					HoopSizeFormat += Designs[0].GetASCII8String(2, 0);
					HoopSizeFormat += Designs[0].GetASCII8String(2, 0);
					HoopSizeFormat += Designs[0].GetASCII8String(2, -24576);
					HoopSizeFormat += Designs[0].GetASCII8String(2, 64);
					HoopSizeFormat += Designs[0].GetASCII8String(2, 1);
					HoopSizeFormat += Designs[0].GetASCII8String(2, 112);
					HoopSizeFormat += Designs[0].GetASCII8String(2, 4864);
					HoopSizeFormat += Designs[0].GetASCII8String(2, 0);
					HoopSizeFormat += Designs[0].GetASCII8String(2, 16);
					HoopSizeFormat += Designs[0].GetASCII8String(2, 16);
					HoopSizeFormat += Designs[0].GetASCII8String(2, 6400);
					HoopSizeFormat += Designs[0].GetASCII8String(2, 16);
					HoopSizeFormat += Designs[0].GetASCII8String(2, 0);
					HoopSizeFormat += Designs[0].GetASCII8String(2, 0);
					HoopSizeFormat += Designs[0].GetASCII8String(2, 0);
					HoopSizeFormat += Designs[0].GetASCII8String(2, 16);
					HoopSizeFormat += Designs[0].GetASCII8String(1, 0);
					HoopSizeFormat += Designs[0].GetASCII8String(2, -1);
					HoopSizeFormat += Designs[0].GetASCII8String(2, 0);
					break;
				case 7: //same as case 5 it looks like
					HoopSizeFormat += Designs[0].GetASCII8String(2, 1);
					HoopSizeFormat += Designs[0].GetASCII8String(2, 1);
					
					HoopSizeFormat += Designs[0].GetASCII8String(2,	GetColorCount());
					HoopSizeFormat += Designs[0].GetASCII8String(2, -1);
					HoopSizeFormat += Designs[0].GetASCII8String(2, 0);
					break;
			}

			return HoopSizeFormat;
		}

		/// <summary>
		/// Starts a fileout proecess and sets up fileOut to be valid
		/// </summary>
		/// <param name="Filename">The Filename to write out to</param>
		void StartWriter(String Filename = "")
		{
			fileOut = new System.IO.BinaryWriter(System.IO.File.Open(Filename,
				System.IO.FileMode.Create));
		}
    }
}