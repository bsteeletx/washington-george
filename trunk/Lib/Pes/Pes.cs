using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using CombineDesign;

using System.Diagnostics;

namespace CombineDesign
{
	public class Pes
		: Pec
	{
		float _pesVersion;
		short _hoopWidth;
		short _hoopHeight;

		public float Version
		{
			get
			{
				float version = _pesVersion;
				return version;
			}
		}

		public short HoopWidth
		{
			get
			{
				short hoopWidth = _hoopWidth;
				return hoopWidth;
			}
			private set
			{
				_hoopWidth = value;
			}
		}

		public short HoopHeight
		{
			get
			{
				short hoopHeight = _hoopHeight;
				return hoopHeight;
			}
			private set
			{
				_hoopHeight = value;
			}
		}
		
		public Pes(String filename, int ID)
			: base(filename, ID)
		{
			char[] TestChar = fileIn.ReadChars(4);

			String TestString = new String(TestChar);

			if (TestString != ("#PES"))//this is not a file that we can read
			{
				setReadyStatus(2);
				setLastError("Missing #PES at beginning of file");
				closeReader();
				return;
			}

			//wasn't working right
			byte[] TestBytes = fileIn.ReadBytes(4);

			_pesVersion = (TestBytes[0] - 0x30) * 100;
			_pesVersion += (TestBytes[1] - 0x30) * 10;
			_pesVersion += (TestBytes[2] - 0x30);
			_pesVersion += (TestBytes[3] - 0x30) * 0.1f;

			if (_pesVersion == 0.1f)
				_pesVersion = 1.0f;

			_hoopHeight = 0;
			_hoopWidth = 0;
		}

		public Pes()
			: base()
		{
			fileType = PESFILESTRUCT;
		}

		private Int32 pecDecodeNormal(Byte input)
		{
			if (input < 0x40)
				return input;

			return (input - 0x80);
		}

		private Int32 pecJumpDecode(Byte byte1, Byte byte2)
		{
			Int32 n1 = (Byte)(byte1 & 0x0F);

			if (n1 <= 7)
			{
				Int32 returnValue = (n1 << 8) + byte2;
				return returnValue;
			}
			else
			{
				Int32 returnValue = -((256 - byte2) + ((15 - n1) << 8));
				return returnValue;
			}
		}

		public override float GetVersionNumber()
		{
			return Version;
		}

		public override DesignFormat read()
		{
			Int32 pecStart, numColors = 0;

			switch ((int)Version)
			{
				case 1: fileIn.BaseStream.Position = 0x2F; break;
				case 2: fileIn.BaseStream.Position = 0x41; break;
				case 3: fileIn.BaseStream.Position = 0x45; break;
				case 4: fileIn.BaseStream.Position = 0x3A; break;
				case 5: fileIn.BaseStream.Position = 0x2E; break;
				case 6: fileIn.BaseStream.Position = 0x3A; break;
				case 9: fileIn.BaseStream.Position = 0x43; break;
			}

			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 2; j++ )
					AffineTransform[i,j] = fileIn.ReadSingle();
			}

			if (Version >= 6.0f)
			{
				if (Version == 6.0f)
				fileIn.BaseStream.Position = 0x19;
				else if (Version == 9.0f)
					fileIn.BaseStream.Position = 0x1B;
				
				HoopWidth = fileIn.ReadInt16();
				HoopHeight = fileIn.ReadInt16();
			}

			fileIn.BaseStream.Position = 8;
			pecStart = fileIn.ReadInt32();
			fileIn.BaseStream.Position = pecStart + 48;
			numColors = fileIn.ReadByte() + 1;

			for (int i = 0; i < numColors; i++)
			{
				Byte B = fileIn.ReadByte();
				Thread T = getThreadInList(B);
				ThreadsInDesign.Add(T);
			}

			SetPECStitchPos(fileIn, pecStart); 
			//DecodeFirstJump(fileIn.ReadByte(), fileIn.ReadByte(), fileIn.ReadByte(), fileIn.ReadByte());

			readStitches(fileIn);

			/*if (IsSideways)
			{
				RotateStitches270();
			}*/

			setReadyStatus(3); //ready

			fileIn.Close();
			return this;
			//base.read();
		}

		/*void DecodeFirstJump(byte val1, byte val2, byte val3, byte val4)
		{
			short x, y;
			x = (short)(((val1 & 0x0F) << 8) + val2);

			if ((x & 0x800) != 0)
				x -= 0x1000;

			y = (Int16)(((val3 & 0x0F) << 8) + val4);

			if ((val2 & 0x800) != 0)
				y -= 0x1000;

			if (!IsSideways)
				SetStartingPoint(new Point(x, y));
			else
				SetStartingPoint(new Point(-y, x));
		}*/

		String GetSewSegColorBlock(ColorBlock CB, short LastColor, Stitch LastStitch)
		{
			Int16 ThisColor = CB.GetColorIndex();
			List<StitchBlock> LSB = CB.GetStitchBlocks();
			String ColorBlockSection = "";
			int jumps = (int)CB.ModBlockCount - 1;
			int FirstRealBlockNumber = CB.GetFirstRealBlockNumber();
			int LastRealBlockNumber = 0;
			Stitch NextStitch = CB.GetFirstFlaglessStitch();

			//if everything is broken, write away!
			if (NextStitch == null)
			{
				jumps = 0;
				FirstRealBlockNumber = 0;
				LastRealBlockNumber = 0;
				NextStitch = CB.GetStitchBlocks()[0].GetStitchList()[0];
			}

			//if it's a new design, we already wrote the break
			if (CB != BlocksInDesignByColor[0])
			{
				if (CB.GetFirstFlaglessStitch() != null)
					ColorBlockSection += WriteColorBreak(LastStitch, CB, FirstRealBlockNumber);
				else
					ColorBlockSection += WriteColorBreak(LastStitch, CB, -1);
			}

			for (int i = LSB.Count - 1; i > -1; i--)
			{
				if (LSB[i].GetStitchTotalMinusFlags() > 0)
				{
					LastRealBlockNumber = i;
					break;
				}
			}

			for (int i = FirstRealBlockNumber; i < LSB.Count; i++)
			{
				if (CB.GetFirstFlaglessStitch() != null)
				{
					if (LSB[i].GetStitchTotalMinusFlags() < 1)
						continue;

					foreach (Stitch S in LSB[i].GetStitchList())
					{
						if (S.Flags == DesignFormat.NORMAL)
						{
							NextStitch = S;
							break;
						}
					}
				}
				else //for weird design that has a new color block but no "real" stitches
				{
					foreach (Stitch S in LSB[i].GetStitchList())
					{
						NextStitch = S;
						break;
					}
				}
				
				if (i != FirstRealBlockNumber)
					ColorBlockSection += WriteSectionBreak(LastStitch, NextStitch, LSB[i].GetStitchTotal() - 1, true);

				ColorBlockSection += GetSewSegStitchBlock(LSB[i], LastStitch, FirstRealBlockNumber - i);
				
				LastStitch = LSB[LastRealBlockNumber].GetLastRealStitch();

				if (LastStitch == null)
					LastStitch = LSB[LastRealBlockNumber].GetLastStitch();
			}

			return ColorBlockSection;
		}

		String GetSewSegStitchBlock(StitchBlock SB, Stitch LastStitch, int BlockNumber)
		{
			List<Stitch> StitchesInBlock = SB.GetStitchList();
			//Stitch FirstStitch = StitchesInBlock[0];
			String StitchBlockSection = "";
			byte thisColor = SB.GetColorIndex();
			int stitchAmount = StitchesInBlock.Count - 1;
			
			if (BlockNumber > 0)
				stitchAmount--;

			if (LastStitch == null)
				stitchAmount++;

			for (int i = 0; i < stitchAmount; i++)
			{
				Stitch S = StitchesInBlock[i];

				StitchBlockSection += GetSewSegStitchSection(S);
			}

			return StitchBlockSection;
		}

		String GetSewSegStitchSection(Stitch S)
		{
			String StitchSection = "";
			Point SavedOffset = GetSaveOffset();

			//commented out because sideways K was off
			/*if (IsSideways)
				SavedOffset.Y += SidewaysOffset;*/

			VerifyStitchValue(S, SavedOffset);

			StitchSection += GetASCII8String(2, S.XX + SavedOffset.X);
			StitchSection += GetASCII8String(2, S.YY + SavedOffset.Y);
			
			return StitchSection;
		}

		void VerifyStitchValue(Stitch S, Point Offset)
		{
			const String X_TOO_SMALL = "X VALUE IS TOO LOW";
			const string X_TOO_BIG = "X VALUE IS TOO HIGH";
			const string Y_TOO_SMALL = "Y VALUE IS TOO LOW";
			const string Y_TOO_BIG = "Y VALUE IS TOO HIGH";
			bool exceptionThrown = false;
			string error = "";
			int minX = 0;
			int minY = 0;
			int maxX = 0;
			int maxY = 0;

			switch (uHoopWidth)
			{
				case 4:
					minX = 0x01F4;
					maxX = 1000 + minX;
					minY = 0x01F4;
					maxY = 1000 + minY;
					break;
				case 5:
					minX = 0x015E;
					maxX = 1300 + minX;
					minY = 0x0064;
					maxY = 1800 + minY;
					break;
				case 7:
					minX = 0x0064;
					maxX = 1800 + minX;
					minY = 0x015E;
					maxY = 1300 + minY;
					break;
			}

			Point MoveOffset = new Point(0, 0);

			if (S.XX + Offset.X < minX)
			{
				//exceptionThrown = true;
				MoveOffset.X += minX - (S.XX + Offset.X);
				error = X_TOO_SMALL;
			}
			else if (S.XX + Offset.X > maxX)
			{
				//exceptionThrown = true;
				MoveOffset.X += maxX - (S.XX + Offset.X);
				error = X_TOO_BIG;
			}
			
			if (S.YY + Offset.Y < minY)
			{
				//exceptionThrown = true;
				MoveOffset.Y += minY - (S.YY + Offset.Y);
				error = Y_TOO_SMALL;
			}
			else if (S.YY + Offset.Y > maxY)
			{
				MoveOffset.Y += maxY - (S.YY + Offset.Y);
				//exceptionThrown = true;
				error = Y_TOO_BIG;
			}

			if (exceptionThrown)
				throw new Exception(error);

			MoveStitchesBy(MoveOffset);
		}

		private void MoveStitchesBy(Point Offset)
		{
			foreach (ColorBlock CB in BlocksInDesignByColor)
			{
				foreach (StitchBlock SB in CB.GetStitchBlocks())
				{
					foreach (Stitch S in SB.GetStitchList())
					{
						S.XX += (short)Offset.X;
						S.YY += (short)Offset.Y;
					}
				}
			}
		}

		public override String GetSewSegSection(Point Offset, Stitch LastStitch	= null)
		{
			String SewSegSection = "";
			
			//SaveOffset = Offset;
											
			foreach (ColorBlock CB in BlocksInDesignByColor)
			{
				if (CB.GetFirstRealBlockNumber() == -1)
				{
					Stitch TestStitch = CB.GetLastStitchInColorBlock();

					if (TestStitch.Flags != DesignFormat.END)
						continue;
				}

				if (CB != BlocksInDesignByColor[0] || LastStitch != null)
					SewSegSection += GetSewSegColorBlock(CB, (short)LastStitch.ThreadSelection.ColorInIndex, LastStitch);
				else
					SewSegSection += GetSewSegColorBlock(CB, -1, LastStitch);
				
				LastStitch = CB.GetLastRealStitchInColorBlock();

				if (LastStitch == null)
				{
					if (CB.GetLastStitchInColorBlock().Flags == DesignFormat.END)
						LastStitch = CB.GetLastStitchInColorBlock();
				}
			}

			return SewSegSection;
		}

		/*public String GetEmbOneSection()
		{
			//This could be re-written to include sections, not sure I need it
			//Rectangle Section1Bounds = blocksInDesign[0].AbsoluteBounds;
			MyRect Bounds = GetBoundingBox(0);
			String EmbOneSection = "";
			
			/* Added this to main section
			for (int i = 0; i < 2; i++)
			{
				EmbOneSection += GetASCII8String(2, Bounds.Left);
				EmbOneSection += GetASCII8String(2, Bounds.Top);
				EmbOneSection += GetASCII8String(2, Bounds.Right);
				EmbOneSection += GetASCII8String(2, Bounds.Bottom);
			}

			//AffineTransform
			foreach (Int32 T in AffineTransform)
				EmbOneSection += GetASCII8String(4, T);*/

			/*EmbOneSection += GetASCII8String(2, 1);

			EmbOneSection += GetASCII8String(2, Bounds.Left);
			EmbOneSection += GetASCII8String(2, Bounds.Bottom);
			EmbOneSection += GetASCII8String(2, Bounds.Width);
			EmbOneSection += GetASCII8String(2, Bounds.Height);

			for (int i = 0; i < 8; i++)
				EmbOneSection += GetASCII8String(1, 0);
			
			EmbOneSection += GetASCII8String(2, 1);
			EmbOneSection += GetASCII8String(2, -1);
			EmbOneSection += GetASCII8String(2, 0);

			//had this commented out:
			//WriteSubObjects(br, pes, SubBlocks);

			return EmbOneSection;
		}*/

		/*private String writePaletteSection()
		{
			//getWriter().Write(getPattern().threadList.Count);
			String PalletteSection = "";
			PalletteSection = GetASCII8String(2, 1);
			PalletteSection += GetASCII8String(1, ThreadsInDesign.Count - 
				1);

			foreach (StitchBlock B in BlocksInDesign)
				PalletteSection += GetASCII8String(2, B.GetColorIndex());

			PalletteSection += GetASCII8String(4, 0);

			return PalletteSection;
		}*/

		/*public void write(String writeToFilename = "")
		{
			//COMMENTING EVERYTHING OUT!!!! JUST NEED TO SAVE OUT WHAT'S IN MEMORY 
				//AS WE ARE NOT MAKING ANY MODIFICATIONS TO THE FILE YET!!
			/*Int64 pecLocation = 0;
			String PesOutFile = "";
			String PesFrame = "";
			String EmbOneSection = "";
			String CSewSegSection = "";
			String PaletteSection = "";
			String StitchSection = "";
			String EncodedSection = "";
			String BitmapSection = "";*/
			
			//getPattern().flipVertical();
			//getPattern().scale(10.0f); //originally scaled, removing for now
			/*PesFrame = "#PES0001";
												
			EmbOneSection = writeEmbOneSection();
			CSewSegSection = writeSewSegSection();
			//PaletteSection = writePaletteSection();

			// + 14 for the newly added 4 Byes the string once inserted
			// and th following misc. data...this did have the pallete section
			pecLocation = PesFrame.Length + EmbOneSection.Length +		
				CSewSegSection.Length + 14;

			PesFrame += getASCII8String(4, pecLocation);
			PesFrame += getASCII8String(2, (Int16)0);//this might be a continuation
				//of the pecLocation, need another file to fine out
			//write object count--is that the same as sections?? Might have to redo
			PesFrame += getASCII8String(2, (Int16)1);
			PesFrame += getASCII8String(2, (Int16)2); //needed this
			PesFrame += getASCII8String(2, (Int16)(-1));
			PesFrame += getASCII8String(2, (Int16)0);
			//PesFrame Done

			//put most of it together
			PesOutFile = PesFrame;
			PesOutFile += EmbOneSection;
			PesOutFile += CSewSegSection;
			//PesOutFile += PaletteSection;
			
			StitchSection = writeStitches(); //more to this later
			//need to do encoding
			EncodedSection = encode();

			//now we have the value that we need to finish the stitch section
			graphicsOffsetLocation = EncodedSection.Length + 8; //8 for the 
				//following 8 Bytes

			//finishing stitch section
			StitchSection += getASCII8String(2, 0x00);
			StitchSection += getASCII8String(2, graphicsOffsetLocation);
			StitchSection += getASCII8String(1, 0x00);
			StitchSection += getASCII8String(1, 0x31);
			StitchSection += getASCII8String(1, 0xFF);
			StitchSection += getASCII8String(1, 0xF0);

			graphicsOffsetValue = (PesFrame.Length + EmbOneSection.Length +
				CSewSegSection.Length -	graphicsOffsetLocation + 2);
			BitmapSection = writeBitmap();

			PesOutFile += StitchSection;
			PesOutFile += EncodedSection;
			PesOutFile += BitmapSection;

			writeFile(writeToFilename);
		}*/

		/*private String writeBitmap()
		{
			String SBitmap = "";
			
			//fileOut.Write(graphicsOffsetValue & 0xFF);
			//fileOut.Write((graphicsOffsetValue >> 8) & 0xFF);
			//fileOut.Write((graphicsOffsetValue >> 16) & 0xFF);
			SBitmap = GetASCII8String(1, graphicsOffsetValue & 0xFF); //or is it 2?
			SBitmap = GetASCII8String(1, (graphicsOffsetValue >> 8) & 
				0xFF); //2?
			SBitmap = GetASCII8String(1, (graphicsOffsetValue >> 16) & 
				0xFF); //2?

			//TODO: Write each colors image, not just 0's
			for (int i = 0; i < ThreadsInDesign.Count + 1; i++)
			{
				for (int j = 0; j < 228; j++)
				{
					//fileOut.Write(0x00);
					SBitmap += GetASCII8String(1, 0x00);
				}
			}

			return SBitmap;
		}*/
	}
}
