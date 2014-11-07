using System;
using System.Collections.Generic;
using System.Drawing;
using CombineDesign;

namespace CombineDesign
{
	public class Pec 
		: DesignFormat
	{
		public const int threadCount = 65;
		public static List<Thread> pecThreadLibrary = new List<Thread>();
		public Int64 graphicsOffsetLocation, graphicsOffsetValue = 0;
		public float[] AffineTransform = { 0.0f, 0.0f, 0.0f, 0.0f };
		short _pecSidewaysOffset = 0;

		public Thread getThreadInList(Int16 threadID)
		{
			if (threadID > threadCount)
				threadID %= threadCount;

			return pecThreadLibrary[threadID];
		}

		private void initThreads()
		{
			Int16 i = 0;
			pecThreadLibrary.Add(new Thread(Color.FromArgb(0, 0, 0), "Unknown", "",
				i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(14, 31, 124), "Persian Blue", "", 				++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(10, 85, 163), "Blue", 
				"", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(0, 135, 119), 
				"Teal Green", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(75, 107, 175), 
				"Cornflower Blue", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(237, 23, 31), "Red", "", 				++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(209, 92, 0), 
				"Reddish Brown", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(145, 54, 151), 
				"Magenta", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(228, 154, 203), 
				"Light Lilac", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(145, 95, 172), 
				"Lilac", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(158, 214, 125), 
				"Mint Green", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(232, 169, 0), 
				"Deep Gold", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(254, 186, 53), 
				"Orange", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(255, 255, 0), 
				"Yellow", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(112, 188, 31), 
				"Lime Green", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(186, 152, 0), "Brass", 
				"", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(168, 168, 168), 
				"Silver", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(125, 111, 0), 
				"Russet Brown", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(255, 255, 179), 
				"Cream Brown", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(79, 85, 86), "Pewter", 
				"", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(0, 0, 0), "Black", "", 
				++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(11, 61, 145), 
				"Ultramarine", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(119, 1, 118), 
				"Royal Purple", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(41, 49, 51), 
				"Dark Gray", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(42, 19, 1), 
				"Dark Brown", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(246, 74, 138), 
				"Deep Rose", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(178, 118, 36), 
				"Light Brown", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(252, 186, 197), 
				"Salmon Pink", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(254, 55, 15), 
				"Vermillion", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(240, 240, 240), "White", 				"", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(106, 28, 138), "Violet", 				"", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(168, 221, 196), 
				"Seacrest", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(37, 132, 187), 
				"Sky Blue", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(254, 179, 67), 
				"Pumpkin", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(255, 243, 107), 
				"Cream Yellow", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(208, 166, 96), "Khaki", 
				"", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(209, 84, 0), 
				"Clay Brown", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(102, 186, 73), 
				"Leaf Green", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(19, 74, 70), 
				"Peacock Blue", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(135, 135, 135), "Gray", 
				"", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(216, 204, 198), 
				"Warm Gray", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(67, 86, 7), 
				"Dark Olive", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(253, 217, 222), 
				"Flesh Pink", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(249, 147, 188), "Pink", 
				"", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(0, 56, 34), 
				"Deep Green", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(178, 175, 212), 
				"Lavender", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(104, 106, 176), 
				"Wisteria Violet", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(239, 227, 185), "Beige", 				"", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(247, 56, 102), 
				"Carmine", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(181, 75, 100), 
				"Amber Red", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(19, 43, 26), 
				"Olive Green", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(199, 1, 86), 
				"Dark Fuschia", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(254, 158, 50), 
				"Tangerine", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(168, 222, 235), 
				"Light Blue", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(0, 103, 62), 
				"Emerald Green", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(78, 41, 144), "Purple", 
				"", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(47, 126, 32), 
				"Moss Green", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(255, 204, 204), 
				"Flesh Pink", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(255, 217, 17), 
				"Harvest Gold", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(9, 91, 166), 
				"Electric Blue", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(240, 249, 112), 
				"Lemon Yellow", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(227, 243, 91), 
				"Fresh Green", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(255, 153, 0), "Orange", 
				"", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(255, 240, 141), 
				"Cream Yellow", "", ++i));
			pecThreadLibrary.Add(new Thread(Color.FromArgb(255, 200, 200), 
				"Applique", "", ++i));
		}

		public Pec(String filename, int ID)
			: base(filename, ID)
		{
			initThreads();
			fileType = PECFILESTRUCT;
		}

		public Pec()
		{
			initThreads();
		}

		protected void SetPECStitchPos(System.IO.BinaryReader Reader, int 
			pecStart)
		{
			while (Reader.BaseStream.CanRead)
			{
				try
				{
					if (Reader.ReadByte() == 0xE0)
					{
						if (Reader.ReadByte() == 0x01)
						{
							if (Reader.ReadByte() == 0xB0)
							{
								if (Reader.ReadByte() == 0x01)
									return;
								else
									Reader.BaseStream.Position -= 3;
							}
							else
								Reader.BaseStream.Position -= 2;
						}
						else
							Reader.BaseStream.Position -= 1;
					}
				}
				catch
				{
					throw new Exception("Could not find beginning of PEC Stitches!");
				}
			}

			throw new Exception("Invalid PES File, no 0xE001B001 Seperator");
		}

		protected void readStitches(System.IO.BinaryReader Reader)
		{
			while (Reader.BaseStream.CanRead)
			{
				Int16 val1 = Reader.ReadByte();
				Int16 val2 = Reader.ReadByte();
				byte stitchType = DesignFormat.NORMAL;
				Stitch S = new Stitch();
				Point Delta = new Point();

				if (val1 == 0xFF && val2 == 0x00)
				{
					stitchType = DesignFormat.END;
					
					S = new Stitch(GetPESLoc(Delta), Delta, stitchType, ThreadsInDesign
						[currentColorIndex], Stitch.PESSTITCH);

					AddStitchToBlock(S);
					break;
				}

				if (val1 == 0xFE && val2 == 0xB0)
				{
					Reader.ReadByte();
					stitchType = DesignFormat.STOP;
					
					S = new Stitch(GetPESLoc(Delta), Delta, stitchType, ThreadsInDesign
						[currentColorIndex], Stitch.PESSTITCH);

					AddStitchToBlock(S);
					continue;
				}

				//High bit set means 12-bit offset, otherwise 7-bit signed delta
				if ((val1 & 0x80) != 0)
				{
					stitchType = DesignFormat.SEQUIN;

					if ((val1 & 0x20) != 0)
						stitchType = DesignFormat.TRIM;
					if ((val1 & 0x10) != 0)
						stitchType = DesignFormat.JUMP;
					

					val1 = (Int16)(((val1 & 0x0F) << 8) + val2);

					if ((val1 & 0x800) != 0)
						val1 -= 0x1000;

					val2 = Reader.ReadByte();
				}
				else if (val1 >= 0x40)
					val1 -= 0x80;

				if ((val2 & 0x80) != 0)
				{
					stitchType = DesignFormat.SEQUIN;

					if ((val2 & 0x20) != 0)
						stitchType = DesignFormat.TRIM;
					if ((val2 & 0x10) != 0)
						stitchType = DesignFormat.JUMP;

					val2 = (Int16)(((val2 & 0x0F) << 8) + Reader.ReadByte());

					if ((val2 & 0x800) != 0)
						val2 -= 0x1000;
				}
				else if (val2 >= 0x40)
					val2 -= 0x80;

				if (!IsSideways)
					Delta = new Point(val1, val2);
				else
					Delta = new Point(val2, -val1);
		
				S = new Stitch(GetPESLoc(Delta), Delta, stitchType, ThreadsInDesign
					[currentColorIndex], Stitch.PESSTITCH);

				AddStitchToBlock(S);
			}
		}

		/*public void readStitches()
		{
			Int32 stitchNumber = 0;

			while (fileIn.BaseStream.CanRead)
			{
				Int16 val1 = fileIn.ReadByte();
				Int16 val2 = fileIn.ReadByte();
				Int16 stitchType = DesignFormat.NORMAL;

				if (val1 == 0xFF && val2 == 0x00)
				{
					PECtoPES(0, 0, DesignFormat.END, true);
					break;
				}

				if (val1 == 0xFE && val2 == 0xB0)
				{
					fileIn.ReadByte();
					PECtoPES(0, 0, DesignFormat.STOP, true);
					stitchNumber++;
					continue;
				}

				//High bit set means 12-bit offset, otherwise 7-bit signed delta
				if ((val1 & 0x80) != 0)
				{
					if ((val1 & 0x20) != 0)
						stitchType = DesignFormat.TRIM;
					if ((val1 & 0x10) != 0)
						stitchType = DesignFormat.JUMP;

					val1 = (Int16)(((val1 & 0x0F) << 8) + val2);

					if ((val1 & 0x800) != 0)
						val1 -= 0x1000;

					val2 = fileIn.ReadByte();
				}
				else if (val1 >= 0x40)
					val1 -= 0x80;

				if ((val2 & 0x80) != 0)
				{
					if ((val2 & 0x20) != 0)
						stitchType = DesignFormat.TRIM;
					if ((val2 & 0x10) != 0)
						stitchType = DesignFormat.JUMP;

					val2 = (Int16)(((val2 & 0x0F) << 8) + fileIn.ReadByte());

					if ((val2 & 0x800) != 0)
						val2 -= 0x1000;
				}
				else if (val2 >= 0x40)
					val2 -= 0x80;

				PECtoPES(val1, val2, stitchType, true);
				stitchNumber++;
			}
		}*/

		private String encodeJump(Int16 x, Int16 types)
		{
			Int32 outputVal = Math.Abs(x) & 0x7FF;
			UInt16 orPart = 0x80;
			String jumpCode = "";

			if ((types & DesignFormat.TRIM) != 0)
				orPart |= 0x20;
			else if ((types & DesignFormat.JUMP) != 0)
				orPart |= 0x10;

			if (x < 0)
			{
				outputVal = x + 0x1000 & 0x7FF;
				outputVal |= 0x800;
			}

			//fileOut.Write((Byte)(((outputVal >> 8 & 0x0F) | orPart)));
			//fileOut.Write((Byte)(outputVal & 0xFF));
			jumpCode = GetASCII8String(1, (((outputVal >> 8) & 0x0F) | orPart));
			jumpCode += GetASCII8String(1, (outputVal & 0xFF));

			return jumpCode;
		}

		private String encodeStop(Byte val)
		{
			//fileOut.Write(0xFE);
			//fileOut.Write(0xB0);
			//fileOut.Write(val);
			String stopCode = GetASCII8String(1, 0xFE);
			stopCode += GetASCII8String(1, 0xB0);
			stopCode += GetASCII8String(1, val);

			return stopCode;
		}

		public override void saveDebugInfo()
		{
			System.IO.StreamWriter outfile = new System.IO.StreamWriter(System.IO.Path.ChangeExtension(_filename, ".txt"));
			outfile.Write(GetDebugInfo());
			outfile.Close();
		}

		public override DesignFormat read()
		{
			fileIn.BaseStream.Position = 0x38;
			int numColors = fileIn.ReadByte() + 1;

			for (int i = 0; i < numColors; i++)
			{
				Byte B = fileIn.ReadByte();
				Thread T = getThreadInList(B);
				ThreadsInDesign.Add(T);
			}

			SetPECStitchPos(fileIn, (int)fileIn.BaseStream.Position);
			//DecodeFirstJump(fileIn.ReadByte(), fileIn.ReadByte(), fileIn.ReadByte(), fileIn.ReadByte());

			readStitches(fileIn);
			//original read
			/*Byte graphicsOffset = 0;
			Byte colorChanges = 0;

			//read Transform
			fileIn.BaseStream.Position = 0x2F;
			
			for (int i = 0; i < 6; i++)
				AffineTransform[i] = fileIn.ReadInt32();

			fileIn.BaseStream.Position = 0x38;
			colorChanges = fileIn.ReadByte();

			for (int i = 0; i < colorChanges; i++)
				AddThread(pecThreadLibrary[fileIn.ReadByte() % threadCount]);

			//Get Graphics Offset
			fileIn.BaseStream.Position = 0x20A;
			graphicsOffset = fileIn.ReadByte();
			graphicsOffset |= (Byte) (fileIn.ReadByte() << 8);
			graphicsOffset |= (Byte)(fileIn.ReadByte() << 16);

			fileIn.ReadByte(); //0x31
			fileIn.ReadByte(); //0xFF
			fileIn.ReadByte(); //0xF0

			//get X and Y size in .1 mm
			// 0x210
			fileIn.ReadInt16(); //x size
			fileIn.ReadInt16(); //y size
			fileIn.ReadInt16(); //unknown
			fileIn.ReadInt16(); //unknown
			fileIn.ReadInt16(); //Hoop Size?
			fileIn.ReadInt16(); //Hoop size?

			//Begin Stitch Data
			//0x21C
			//had this commented out: unsigned int end = graphicsOffset + 0x208;
			readStitches(fileIn);

			//reading images
			fileIn.BaseStream.Position = graphicsOffset + 0x208;

			//had this commented out too:
			/*
			 * unsigned char* imageData = new unsigned char[(pattern->threadList.size() + 1) * 228];
				binaryReadBytes(file, imageData, (pattern->threadList.size() + 1) * 228);
				var colors = new List<Embroidery.Shared.Color> {Embroidery.Shared.Color.Black};
				colors.AddRange(pattern.ColorList.Select(t => t.Color));
				ReadImages(imageData, colors);
			 * */
			//FlipVertical();*/
			////////
			fileIn.Close();
			readyStatus = 3;//Ready

			return this;
		}

		public override String GetEncodedPECSection(Point ImageLoc, int lastDesignID, bool lastStopCode, Stitch LastStitch, Point LastImageLoc, Point TopLeft, MyRect PatternBounds)
		{
			short deltaX = 0;
			short deltaY = 0;
			Byte stopCode = 0;
			String Encoded = "";
			MyRect Bounds = GetBoundingBox();
			Boolean NewColor = false;
			Boolean IsFirstStitch = true;
			Boolean IsFirstSection = true;
			Stitch Last = new Stitch();
			int colorBlockCount = 0;
			bool forceTrim = false;
			
			if (LastStitch != null)
			{
				if (LastStitch.ThreadSelection.ColorInIndex != BlocksInDesignByColor[0].GetColorIndex())
				{
					NewColor = true;
					lastStopCode = !lastStopCode;
				}

				IsFirstStitch = false;
			}

			if (lastStopCode)
				stopCode = 2;
			else
				stopCode = 1;
			
			foreach (ColorBlock CB in BlocksInDesignByColor)
			{
				List<StitchBlock> LSB = CB.GetStitchBlocks();
				short zeroCount = 0;
				
				if (NewColor)
				{

					Encoded += encodeStop(stopCode);

					if (stopCode == 2)
						stopCode = 1;
					else
						stopCode = 2;
				}

				foreach (StitchBlock SB in LSB)
				{
					List<Stitch> LS = SB.GetStitchList();
					
					for (int i = 0; i < LS.Count; i++)
					{
						Byte curFlags = LS[i].Flags;

						if ((curFlags & 0x14) != 0) //STOP and END are not real stitches
							continue;

						if (i == 0 && !IsFirstStitch && !IsFirstSection)
						{
							deltaX = (short)((LS[0].XX - Last.XX));
							deltaY = (short)((LS[0].YY - Last.YY));
						}
						else if (i == 0 && IsFirstSection && !IsFirstStitch && LastStitch != null)
						{
							deltaX = (short)((LS[0].XX + ImageLoc.X) - (LastStitch.XX + LastImageLoc.X));
							deltaY = (short)((LS[0].YY + ImageLoc.Y) - (LastStitch.YY + LastImageLoc.Y));

							//for Sideways K
							if (IsSideways)
								deltaY += (short)SidewaysOffset;
						}
						else
						{
							deltaX = (short)LS[i].Delta.X;
							deltaY = (short)LS[i].Delta.Y;
						}

						if (GetUHoopWidth() == 7)
						{
							short temp = deltaX;
							deltaX = (short)-deltaY;
							deltaY = temp;
						}

						if (IsFirstStitch)
						{
							if (GetUHoopWidth() == 7)
							{
								deltaX += (short)((PatternBounds.Height) + (TopLeft.Y - ImageLoc.Y));
								deltaY += (short)(ImageLoc.X - TopLeft.X);
							}
							else
							{
								deltaX += (short)(ImageLoc.X - TopLeft.X);
								deltaY += (short)(ImageLoc.Y - TopLeft.Y);
							}
						}

						if (NewColor) 
						{
							if (LS[i].Delta == new Point(0, 0))
							{
								if (zeroCount < 3)
									Encoded += GetASCII8String(2, 0);
								else
									NewColor = false;

								zeroCount++;

								continue;
							}
							else if (curFlags == NORMAL)
								curFlags = TRIM;
						}

						//WRITE STITCH OUT
						if ((deltaX < 64 && deltaX >= -64) && !IsFirstStitch && !NewColor)
						{
							if (deltaX < 0)
								Encoded += GetASCII8String(1, deltaX + 0x80);
							else
								Encoded += GetASCII8String(1, deltaX);

							//this will fix Jamie Dean's issue, but I think it breaks something else...
							curFlags = NORMAL;
						}
						else
						{
							if (curFlags == NORMAL)
								Encoded += encodeJump(deltaX, SEQUIN);
							else if (!NewColor && !forceTrim)
								Encoded += encodeJump(deltaX, LS[i].Flags);
							else
							{
								if (zeroCount == 0)
									Encoded += GetASCII8String(2, 0);

								Encoded += encodeJump(deltaX, TRIM);
							}
						}

						if ((deltaY < 64 && deltaY >= -64) && !IsFirstStitch && !NewColor && curFlags == NORMAL)
						{
							if (deltaY < 0)
								Encoded += GetASCII8String(1, deltaY + 0x80);
							else
								Encoded += GetASCII8String(1, deltaY);
						}
						else
						{
							if (curFlags == NORMAL)
								Encoded += encodeJump(deltaY, SEQUIN);
							else if (!NewColor && !forceTrim)
								Encoded += encodeJump(deltaY, LS[i].Flags);
							else
								Encoded += encodeJump(deltaY, TRIM);
						}
						
						IsFirstSection = false;
						IsFirstStitch = false;
						NewColor = false;
						Last = LS[i];
						zeroCount = 0;
						forceTrim = false;
					}
				}

				if (colorBlockCount + 1 != BlocksInDesignByColor.Count)
				{
					if (BlocksInDesignByColor[colorBlockCount].Color != BlocksInDesignByColor[colorBlockCount + 1].Color)
						NewColor = true;
					else
					{
						NewColor = false;
						forceTrim = true;
					}

					colorBlockCount++;
				}

			}

			return Encoded;
		}

		void SetSidewaysOffset()
		{
			short MaxY = 0;

			foreach (ColorBlock CB in BlocksInDesignByColor)
			{
				foreach (StitchBlock SB in CB.GetStitchBlocks())
				{
					foreach (Stitch S in SB.GetStitchList())
					{
						MaxY = Math.Max(MaxY, S.YY);
					}
				}
			}

			_pecSidewaysOffset = MaxY;
		}

		/*public String GetPECStitches()
		{
			//EmbRectangle Bounds = new EmbRectangle();
			String Stitches = "";
			//Int32 flen, dotPos, lastSlash, currentThreadCount = 0; 
				//colorsToStitch = 0;
			//String start = "";
			/*Char[] extension = {'.', 'p', 'e', 'c'};
			
			if (fileType == PESFILESTRUCT)
				extension[3] = 's';

			String filenameBase = _filename.Substring(0, _filename.Length - 4);
			lastSlash = filenameBase.LastIndexOf('\\');
			filenameBase = filenameBase.Substring(lastSlash + 1);

			dotPos = filenameBase.Length;
			Stitches = "LA:" + filenameBase;
			flen = filenameBase.Length; 

			for (int i = 0; i < (int)16 - flen; i++)
				Stitches += GetASCII8String(1, 0x20);
			
			Stitches += GetASCII8String(1, 0x0D);

			for (int i = 0; i < 12; i++)
				Stitches += GetASCII8String(1, 0x20);
			
			Stitches += GetASCII8String(1, 0xFF);
			Stitches += GetASCII8String(1, 0x00);
			Stitches += GetASCII8String(1, 0x06);
			Stitches += GetASCII8String(1, 0x26);

			for (int i = 0; i < 12; i++)
				Stitches += GetASCII8String(1, 0x20);

			currentThreadCount = threadsInDesign.Count - 1;
			Stitches += GetASCII8String(1, currentThreadCount);

			foreach (Thread T in threadsInDesign)
				Stitches += GetASCII8String(1, T.ThreadColorInIndex);
			
			for (int i = 0; i < (int)(0x1CF - currentThreadCount - 1); i++)
				Stitches += GetASCII8String(1, 0x20);
			
			return Stitches;
		}*/

		/*public override void write(String Filename = "")
		{
			flipVertical();
			fixColorCount();
			correctForMaxStitchLength(127, 127); //was 12.7 for some reason?
			//P.scale(10.0f); //taking out scale for now
			fileOut.Write("#PEC0001");
			writeStitches();
			fileOut.Close();
		}*/
	}
}
