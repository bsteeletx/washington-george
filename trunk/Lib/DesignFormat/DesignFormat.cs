using System;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using CombineDesign;

namespace CombineDesign
{
	public class DesignFormat
	{
		protected System.IO.BinaryReader fileIn;
		protected String _filename;
		protected byte readyStatus = 0; //NotOpen, IOError, ParseError, Ready
		protected String lastError = "";
		//List<Byte> EntireFile = new List<byte>();
		protected byte fileType = 0;
		protected Int16 OriginalFirstColorLeft = 0;
		protected Int16 OriginalFirstColorTop = 0;
		protected float currentAngle = 0.0f;

		int _designID = 0;
		bool _primeDesign = false;

		/* Machine codes for stitch flags */
		public const byte NORMAL = 0; /* stitch to (xx, yy) */
		public const byte JUMP = 1; /* move to(xx, yy) */
		public const byte TRIM = 2; /* trim + move to(xx, yy) */
		public const byte STOP = 4; /* pause machine for thread change */
		public const byte SEQUIN = 8; /* sequin */
		public const byte END = 16; /* end of program */

		/*const Int16 LINETO = 0;
		const Int16 MOVETO = 1;
		const Int16 ARCTOMID = 2;
		const Int16 ARCTOEND = 4;
		const Int16 ELLIPSETORAD = 8;
		const Int16 ELLIPSETOEND = 16;
		const Int16 CUBICTOCONTROL1 = 32;
		const Int16 CUBICTOCONTROL2 = 64;
		const Int16 CUBICTOEND = 128;
		const Int16 QUADTOCONTROL = 256;
		const Int16 QUADTOEND = 512;*/

		protected const byte PECFILESTRUCT = 1;
		protected const byte PESFILESTRUCT = 2;

		protected Single PixelToMMRatio = 1.0f;

		//Int16 dstJumpsPerTrim = 6;

		/*List<EmbArc> arcList = new List<EmbArc>();
		List<EmbCircle> circleList = new List<EmbCircle>();
		List<EmbEllipse> ellipseList = new List<EmbEllipse>();
		List<EmbLine> lineList = new List<EmbLine>();
		List<EmbPoint> pointList = new List<EmbPoint>();
		List<EmbPolygon> polygonList = new List<EmbPolygon>();
		List<EmbPolyline> polylineList = new List<EmbPolyline>();
		List<EmbRectangle> rectangleList = new List<EmbRectangle>();
		List<EmbSpline> splineList = new List<EmbSpline>();*/

		public List<ColorBlock> BlocksInDesignByColor = new List<ColorBlock>();
		protected ColorBlock CurColorBlock = new ColorBlock();
		//protected List<StitchBlock> BlocksInDesign = new List<StitchBlock>();
		protected StitchBlock CurStitchBlock = new StitchBlock();
		protected List<Thread> ThreadsInDesign = new List<Thread>();
		Point SaveOffset = new Point();
				
		Int32 minX;
		Int32 minY;
		Int32 maxX;
		Int32 maxY;
		//Int32 width = 0;
		//Int32 height = 0;
		//Point StartStitch = new Point(0, 0);
		//protected bool StartStitchSet = false;
		Int32 blockMinX = 0;
		Int32 blockMaxX = 0;
		Int32 blockMinY = 0;
		Int32 blockMaxY = 0;

		public int SidewaysOffset = 0;

		protected Int32 currentColorIndex;
		Int16 lastX;
		Int16 lastY;
		protected bool lastStitchSet = false;
		bool ToggleValue = false;

		protected ushort uHoopWidth = 0;
		protected bool IsSideways = false;
		public int NumOfDesignsInPattern = 1;

		//protected float[] AffTransAsFloats = { 0f, 0f, 0f, 0f };

		public DesignFormat()
		{	
		}

		public DesignFormat(String Filename, int ID)
		{
			fileIn = new System.IO.BinaryReader(System.IO.File.Open(Filename, 
					System.IO.FileMode.Open, System.IO.FileAccess.Read));

			_designID = ID;
			_filename = Filename;
		}

		///////////////////OVERRIDES!!!//////////////////////////
		public virtual String GetEncodedPECSection(Point Offset, int 
			designCount, bool lastStopCode, Stitch LastStitch, Point LastOffset, Point TopLeftOfDesign, MyRect BoundsOfPattern)
		{
			String Ret = "";
			return Ret;
		}
		
		public virtual String GetSewSegSection(Point Offset, Stitch LastStitch = 			null)
		{
			String Ret = "";
			return Ret;
		}

		public virtual float GetVersionNumber()
		{
			return 0;
		}
		
		/// <summary>
		/// Base Method to be overriden
		/// Should read the fileformat selected
		/// </summary>
		/// <returns>
		/// Returns the DesignFormat object of the file read
		/// </returns>
		public virtual DesignFormat read() 
		{
			return this; 
		}

		/// <summary>
		/// Base Method to be overriden
		/// Saves out the debug information--not written yet
		/// </summary>
		public virtual void saveDebugInfo() { }
		//public virtual void write(String Filename = "") { }
		///////////////////END OF OVERRIDES!!///////////////////

		////////////////START OF MEMBER ONLY METHODS//////////////////
		/*private void addCircleObject(Double cx, Double cy, Double r)
		{
			EmbCircle embCircle = new EmbCircle(cx, cy, r);

			circleList.Add(embCircle);
		}

		private void addEllipseObject(Double cx, Double cy, Double rx, Double ry)
		{
			EmbEllipse embEllipse = new EmbEllipse(cx, cy, rx, ry);

			ellipseList.Add(embEllipse);
		}

		private void addLineObject(Int32 x1, Int32 y1, Int32 x2, Int32 y2)
		{
			EmbLine embLine = new EmbLine(x1, y1, x2, y2);

			lineList.Add(embLine);
		}

		private void addPathObject()
		{
			//???
		}

		private void addPointObject(Int32 x, Int32 y)
		{
			EmbPoint embPoint = new EmbPoint(x, y);

			pointList.Add(embPoint);
		}

		private void addRectangleObject(Int32 x, Int32 y, Int32 w, Int32 h)
		{
			EmbRectangle embRectangle = new EmbRectangle(x, y, w, h);

			rectangleList.Add(embRectangle);
		}*/

		//The original writing of this method didn't look like it would work
		//before using it, figure out what it's upposed to do
		//wer'e not thinking this is going to be useful, deprecating it...
		/*private void combineJumpStitches()
		{
			//created a new list of stitches that I think were supposed to 
			//duplicate the main list of stitches (stitchList)
			Int32 jumpCount = 0;
			List<Stitch> jumpListStart = new List<Stitch>();
			
			foreach (Stitch S in CurStitchBlock) //S = pointer
			{
				if ((S.Flags & JUMP) == 1)
				{
					if (jumpCount == 0)
						jumpListStart.Add(S);

					jumpCount++;
				}
				else
				{
					if (jumpCount > 0)
					{
						jumpListStart[jumpCount].XX = S.XX;
					}
				}
			}
		}*/

		//not sure this does anything
		/*private void correctForMaxStitchLength(Int16 maxStitchLength, Int32 maxJumpLength)
		{
			Int32 j = 0, splits; //no idea what splits means
			Int32 maxXy = 0;
			Int32 maxLen = 0;
			Int16 addX = 0;
			Int16 addY = 0;

			if (stitchesInDesign.Count > 1)
			{
				//created a stitchlist pointer, then assigned it to the next 
				//stitch in the original stitchlist
				Stitch Prev = stitchesInDesign[0];

				foreach (Stitch S in stitchesInDesign) //S = pointer
				{
					if (S == stitchesInDesign[0]) //don't want to do the first one
						continue;

					Int16 xx = Prev.XX;
					Int16 yy = Prev.YY;
					Int16 dx = (Int16)(S.XX - xx);
					Int16 dy = (Int16)(S.YY - yy);

					if ((Math.Abs(dx) > maxStitchLength) || (Math.Abs(dy) >
						maxStitchLength))
					{
						//declares a pointer to a stitchlist and assigns S
						//to the stitchlist, I don't think that's needed
						maxXy = Math.Max(Math.Abs(dx), Math.Abs(dy));

						if ((S.Flags & (JUMP | TRIM)) != 0)
							maxLen = maxJumpLength;
						else
							maxLen = maxStitchLength;

						splits = (Int16)(maxXy / maxLen);

						if (splits > 1)
						{
							Int32 flagsToUse = S.Flags;
							Int32 colorToUse = S.ThreadSelection.ThreadColorInIndex;
							addX = (Int16)(dx / splits);
							addY = (Int16)(dy / splits);

							for (j = 1; j < splits; j++)
							{
								Stitch St = new Stitch();
								St.XX = (Int16)(xx + addX * j);
								St.YY = (Int16)(yy + addY * j);
								St.Flags = (Int16)flagsToUse;
								St.ThreadSelection.ThreadColorInIndex = colorToUse;
								Prev = St;
							}
						}
					}

					Prev = S;
				}
			}

			if (stitchesInDesign[stitchesInDesign.Count - 1].Flags != END)
				stitchesInDesign.Add(new Stitch(END, stitchesInDesign
					[stitchesInDesign.Count - 1].XX, stitchesInDesign
					[stitchesInDesign.Count - 1].YY, new Thread()));
		}*/

		/*private void fixColorCount()
		{
			Int32 maxColorIndex = 0;

			for (int i = 0; i < CurStitchBlock.Count; i++)
				maxColorIndex = System.Math.Max(maxColorIndex, CurStitchBlock
					[i].ThreadSelection.ThreadColorInIndex);

			while (ThreadsInDesign.Count <= maxColorIndex)
				ThreadsInDesign.Add(new Thread());

			//this wasn't in original code, but they had a TODO to put it in, is it needed?
			//threadList.RemoveAt(threadList.Count - 1);
		}*/

		private void FormatToPes()
		{
			//TODO
		}

		/*private void hideStitchesOverLength(Int32 length)
		{
			Double prevX = 0;
			Double prevY = 0;

			for (int i = 0; i < stitchesInDesign.Count; i++)
			{
				if ((System.Math.Abs(stitchesInDesign[i].XX - prevX) > length) ||
					(System.Math.Abs(stitchesInDesign[i].YY - prevY) > length))
				{
					stitchesInDesign[i].Flags |= TRIM;
					stitchesInDesign[i].Flags &= ~NORMAL;
				}
				prevX = stitchesInDesign[i].XX;
				prevY = stitchesInDesign[i].YY;
			}
		}*/

		private void loadExternalColorFile(String filename)
		{
			//OA said there was a memory leak somewhere
			//again, doesn't seem like anything happens here

			Int32 DotPos = filename.IndexOf('.');
			String BaseFilename = filename.Substring(0, DotPos);
			System.IO.BinaryReader colorIn = new System.IO.BinaryReader(System.IO.File.Open(BaseFilename + ".edr", System.IO.FileMode.Open, System.IO.FileAccess.Read));

			//what now?
		}
		////////////////END OF MEMBER ONLY METHODS///////////////////

		////////////////START OF INHERITED METHODS////////////////////
		protected void closeReader()
		{
			fileIn.Close();
		}

		/*protected void closeWriter()
		{
			fileOut.Close();
		}*/

		/*protected void loadFile()
		{
			fileIn.BaseStream.Position = 0;
			Encoding enc = Encoding.GetEncoding("IBM437");

			while (fileIn.BaseStream.Position != fileIn.BaseStream.Length)
				EntireFile.Add(fileIn.ReadByte());

			fileIn.Close();
		}*/

		protected void setLastError(String Error)
		{
			lastError = Error;
		}

		protected void setReadyStatus(byte status)
		{
			readyStatus = status;
		}

		/*protected void writeFile(String NewFilename = "")
		{
			StartWriter(NewFilename);
			Encoding enc = Encoding.GetEncoding("IBM437");
			String OutFile = "";

			foreach (Byte B in EntireFile)
			{
				OutFile += this.GetASCII8String(1, B);
			}

			Byte[] tempBytes = enc.GetBytes(OutFile);
			fileOut.Write(tempBytes);

			//fileOut.Write(EntireFile.ToArray());

			//getWriter().Write(tempBytes);
			fileOut.Close();
		}*/
		///////////////END OF INHERITED METHODS//////////////////////

		//////////////START OF PUBLIC MEHTODS///////////////////////
		public bool GetIsSideways()
		{
			return IsSideways;
		}

		public void AddStitchToBlock(Stitch S)
		{
			byte flags = (byte)S.Flags;
			short x = S.XX;
			short y = S.YY;
			bool isAutoColorIndex = true;
			bool FirstStitch = false;
			int lastColorBlock = BlocksInDesignByColor.Count - 1;
			int lastStitchBlock = -1;
			
			if (lastStitchBlock >= 0)
				lastStitchBlock = (int)BlocksInDesignByColor[lastColorBlock].Count - 1;

			if (BlocksInDesignByColor.Count == 0 && 
				CurStitchBlock.GetStitchTotal() == 0)
			{
				CurStitchBlock = new StitchBlock(ThreadsInDesign, 0);
				FirstStitch = true;
			}

			if ((CurStitchBlock.GetStitchTotal() == 0) && (flags == NORMAL))
			{
				blockMinX = x;
				blockMinY = y;
				blockMaxX = x;
				blockMaxY = y;
			}
			else if (flags == NORMAL)
			{
				blockMinX = Math.Min(x, blockMinX);
				blockMinY = Math.Min(y, blockMinY);
				blockMaxX = Math.Max(x, blockMaxX);
				blockMaxY = Math.Max(y, blockMaxY);
			}

			if ((flags & END) != 0)//|| (((flags & STOP) != 0) && 
				//currentColorIndex == ThreadsInDesign.Count - 1))
			{
				/*if (lastStitchBlock != -1 && lastStitchBlock != -1)
					CurStitchBlock.AddStitch(S, BlocksInDesignByColor[lastColorBlock].GetStitchBlocks()[(int)lastStitchBlock - 1].GetLastStitch());
				else*/
					CurStitchBlock.AddStitch(S);

				CurColorBlock.Add(CurStitchBlock);
				BlocksInDesignByColor.Add(CurColorBlock);
				return;
			}

			if (((flags & STOP) != 0) && (CurStitchBlock.GetStitchTotal() == 0))
			{
				/*if (lastStitchBlock != -1 && lastStitchBlock != -1)
					CurStitchBlock.AddStitch(S, BlocksInDesignByColor[lastColorBlock].GetStitchBlocks()[(int)lastStitchBlock - 1].GetLastStitch());
				else*/
					CurStitchBlock.AddStitch(S);

				CurColorBlock.Add(CurStitchBlock);				
				BlocksInDesignByColor.Add(CurColorBlock);

				CurStitchBlock = new StitchBlock(ThreadsInDesign,
					++currentColorIndex);
				CurColorBlock = new ColorBlock();
				return;
			}

			if (((flags & STOP) != 0) && (isAutoColorIndex))
			{
				/*if (lastStitchBlock != -1 && lastStitchBlock != -1)
					CurStitchBlock.AddStitch(S, BlocksInDesignByColor[lastColorBlock].GetStitchBlocks()[(int)lastStitchBlock - 1].GetLastStitch());
				else*/
					CurStitchBlock.AddStitch(S);

				CurColorBlock.Add(CurStitchBlock);
				BlocksInDesignByColor.Add(CurColorBlock);
				
				CurStitchBlock = CurStitchBlock = new StitchBlock
					(ThreadsInDesign, ++currentColorIndex);
				CurColorBlock = new ColorBlock();
				
				/*blockMinY = S.YY;
				blockMinX = S.XX;
				blockMaxX = S.XX;
				blockMaxY = S.YY;*/
			}
			else if (flags != NORMAL && flags != SEQUIN && !FirstStitch)
			{
				/*if (lastStitchBlock != -1 && lastStitchBlock != -1)
					CurStitchBlock.AddStitch(S, BlocksInDesignByColor[lastColorBlock].GetStitchBlocks()[(int)lastStitchBlock - 1].GetLastStitch());
				else*/
					CurStitchBlock.AddStitch(S);

				CurColorBlock.Add(CurStitchBlock);
				
				CurStitchBlock = new StitchBlock(ThreadsInDesign,
					currentColorIndex);
				blockMinY = S.YY;
				blockMinX = S.XX;
				blockMaxX = S.XX;
				blockMaxY = S.YY;
			}

			/*if (lastStitchBlock != -1 && lastStitchBlock != -1)
				CurStitchBlock.AddStitch(S, BlocksInDesignByColor[lastColorBlock].GetStitchBlocks()[(int)lastStitchBlock - 1].GetLastStitch());
			else*/
				CurStitchBlock.AddStitch(S);

			lastX = S.XX;
			lastY = S.YY;
		}

		/* Old PECtoPES for reference..
		public Point GetPESLoc(Stitch PECStitch)
		{
			short dx = PECStitch.XX;
			short dy = PECStitch.YY;
			byte flags = PECStitch.Flags;
			Int16 x = 0;
			Int16 y = 0;

			//was just get stitchtotal, I think this is more correct
			if (CurStitchBlock.GetStitchTotal() != 0)
			{
				x = (Int16)(lastX + dx);
				y = (Int16)(lastY + dy);
			}
			else
			{
				Stitch PesStitch = new Stitch(StartStitch, DesignFormat.JUMP, 
					ThreadsInDesign[currentColorIndex], Stitch.PESSTITCH);

				AddPESStitch(PesStitch);

				lastX = (Int16)StartStitch.X;
				lastY = (Int16)StartStitch.Y;
				x = (Int16)(dx + lastX);
				y = (Int16)(dy + lastY);
			}

			minX = Math.Min((Int32)x, minX);
			minY = Math.Min((Int32)y, minY);
			maxX = Math.Max((Int32)x, maxX);
			maxY = Math.Max((Int32)y, maxY);

			Stitch PESS = new Stitch(new Point(x, y), flags, 
				ThreadsInDesign[currentColorIndex], Stitch.PESSTITCH);

			return PESS;
		}*/

		public Point GetPESLoc(Point Delta)
		{
			short dx = (short)Delta.X;
			short dy = (short)Delta.Y;
			Int16 x = 0;
			Int16 y = 0;

			//if (CurStitchBlock.GetStitchTotal() != 0)
			//{
				x = (Int16)(lastX + dx);
				y = (Int16)(lastY + dy);
			//}
			/*else
			{
				Stitch PesStitch = new Stitch(StartStitch, StartStitch, DesignFormat.JUMP,
					ThreadsInDesign[currentColorIndex], Stitch.PESSTITCH);

				AddStitchToBlock(PesStitch);

				lastX = (Int16)StartStitch.X;
				lastY = (Int16)StartStitch.Y;
				x = (Int16)(dx + lastX);
				y = (Int16)(dy + lastY);
			}*/

			minX = Math.Min((Int32)x, minX);
			minY = Math.Min((Int32)y, minY);
			maxX = Math.Max((Int32)x, maxX);
			maxY = Math.Max((Int32)y, maxY);

			return new Point(x, y);
		}

		public void AddThread(Thread thread)
		{
			ThreadsInDesign.Add(thread);
		}

		//NOT WORKING YET, puting in a temp Min/Max X/Y values as part of Pattern
		//Will be updated on every added stitch
		public EmbRectangle CalcRelBoundBox()
		{
			EmbRectangle BoundingRectangle = new EmbRectangle(minX, minY, maxX,
				maxY);

			/*foreach (Stitch S in stitchList)
			{
				if ((S.Flags & TRIM) != 0)
				{
					BoundingRectangle.Left = Math.Min(BoundingRectangle.Left, S.XX);
					BoundingRectangle.Top = Math.Min(BoundingRectangle.Top, S.YY);
					BoundingRectangle.Right = Math.Min(BoundingRectangle.Right, S.XX);
					BoundingRectangle.Bottom = Math.Min(BoundingRectangle.Bottom, S.YY);
				}
			}

			foreach (EmbArc A in arcList)
			{
				//NOT DONE YET...NEEDED?
			}

			foreach (EmbCircle C in circleList)
			{
				BoundingRectangle.Left = Math.Min(BoundingRectangle.Left, C.getCenterX() - C.getRadius());
				BoundingRectangle.Top = Math.Min(BoundingRectangle.Top, C.getCenterY() - C.getRadius());
				BoundingRectangle.Right = Math.Min(BoundingRectangle.Right, C.getCenterX() + C.getRadius());
				BoundingRectangle.Bottom = Math.Min(BoundingRectangle.Bottom, C.getCenterY() + C.getRadius());
			}

			foreach (EmbEllipse E in ellipseList)
			{
				//NOT DONE YET
			}

			foreach (EmbLine L in lineList)
			{
				//NOT DONE YET
			}

			foreach (EmbPoint P in pointList)
			{
				//NOT DONE YET
			}

			foreach (EmbPolygon P in polygonList)
			{
				//NOT DONE YET
			}

			foreach (EmbPolyline P in polylineList)
			{
				//NOT DONE YET
			}

			foreach (EmbRectangle R in rectangleList)
			{
				//NOT DONE YET
			}

			foreach (EmbSpline S in splineList)
			{
				//NOT DONE YET
			}*/

			return BoundingRectangle;
		}

		/*public void Center()
		{
			//OA didn't think this actually worked
			Int32 moveLeft = 0;
			Int32 moveTop = 0;
			EmbRectangle BoundingRectangle = CalcRelBoundBox();

			moveLeft = (Int32)(BoundingRectangle.Left - (BoundingRectangle.getWidth() / 2.0));
			moveTop = (Int32)(BoundingRectangle.Top - (BoundingRectangle.getHeight() / 2.0));

			foreach (Stitch S in stitchesInDesign)
			{
				S.XX -= (Int16)moveLeft;
				S.YY -= (Int16)moveTop;
			}
		}
		
		public void ChangeColor(Int32 index)
		{
			Rectangle BlockBounds = new Rectangle(blockMinX, blockMinY,
					blockMaxX - blockMinX, blockMaxY - blockMinY);
			CurStitchBlock = new StitchBlock(ThreadsInDesign, stitchesInDesign,
					currentColorIndex, BlockBounds);
			BlocksInDesign.Add(CurStitchBlock);
			stitchesInDesign = new List<Stitch>();
			currentColorIndex = index;
		}

		public void CleanUp()
		{
			for (int i = 0; i < BlocksInDesign.Count; i++)
			{
				if (BlocksInDesign[i].GetStitchesTotal() == 0)
				{
					BlocksInDesign.Remove(BlocksInDesign[i]);
					i = 0;
				}
			}
		}

		public void FlipVertical()
		{
			foreach (StitchBlock S in BlocksInDesign)
			{
				for (int i = 0; i < S.GetStitchPointArray().Length; i++)
					S.GetStitchPointArray()[i].Y = -S.GetStitchPointArray()
						[i].Y;
			}

		}*/

		/*public Int32 GetAbsHeight()
		{
			MyRect Box = GetBoundingBox();

			return Box.Height;
		}*/

		public MyRect GetBoundingBox(Int32 ForColorBlock = -1)
		{
			if (ForColorBlock == -1)
				return GetBoundsOfDesign();
			else
				return BlocksInDesignByColor[ForColorBlock].GetColorBounds();
		}

		public Point GetSaveOffset()
		{
			return SaveOffset;
		}

		public void SetSaveOffset(Point NewOffset)
		{
			//Seeing if it's a perfect 500 when sideways...below it is the original working code
			//SaveOffset.X = NewOffset.X;
			//SaveOffset.Y = NewOffset.Y - 500;
			SaveOffset = NewOffset;
		}

		/*public Int32 GetAbsWidth()
		{
			MyRect Box = GetBoundingBox();

			return Box.Width;
		}*/

		public String GetAffineTransform()
		{
			String AffineTransform = "";

			//I've seen this be different only occaisionally
			//UPDATE: Different if saved by Embird vs. Brother, going with Brother
			if (uHoopWidth != 7)
			{
				AffineTransform += GetASCII8String(2, 0);
				AffineTransform += GetASCII8String(2, 16256); //0x3F80
				AffineTransform += GetASCII8String(2, 0);
				AffineTransform += GetASCII8String(2, 0);
				AffineTransform += GetASCII8String(2, 0);
				AffineTransform += GetASCII8String(2, 0);
				AffineTransform += GetASCII8String(2, 0);
				AffineTransform += GetASCII8String(2, 16256); //0x3F80
				AffineTransform += GetASCII8String(2, 0);
				AffineTransform += GetASCII8String(2, 0);
				AffineTransform += GetASCII8String(2, 0);
				AffineTransform += GetASCII8String(2, 0);
			}
			else //might need more here, this should work for 7x5 though
			{
				AffineTransform += GetASCII8String(2, 2886); //0x0B46
				AffineTransform += GetASCII8String(2, 12881); //0x3251
				AffineTransform += GetASCII8String(2, 0);
				AffineTransform += GetASCII8String(2, 16256); //0x3F80
				AffineTransform += GetASCII8String(2, 0);
				AffineTransform += GetASCII8String(2, -16512); //0xBF80
				/////////////////Split//////////////////////
				AffineTransform += GetASCII8String(2, 2886); //0x0B46
				AffineTransform += GetASCII8String(2,
					12881); //0x3251
				AffineTransform += GetASCII8String(2, 0);
				AffineTransform += GetASCII8String(2,
					17658); //0x44FA
				AffineTransform += GetASCII8String(2, 0);
				AffineTransform += GetASCII8String(2, 0);
			}

			return AffineTransform;
		}

		//will now accept any value and turn it into the correct number of Bytes
		public String GetASCII8String(Int16 numOfBytes, Int64 value)
		{
			Byte[] tempBytes = { 0x00 };
			Encoding enc = Encoding.GetEncoding("IBM437");

			if (numOfBytes == 1)
				tempBytes = BitConverter.GetBytes((Byte)value);
			else if (numOfBytes == 2)
				tempBytes = BitConverter.GetBytes((Int16)value);
			else if (numOfBytes <= 4)
				tempBytes = BitConverter.GetBytes((Int32)value);
			else if (numOfBytes <= 8)
				tempBytes = BitConverter.GetBytes((Int64)value);

			Char[] CharsAsHex = enc.GetChars(tempBytes);
			String outText = new String(CharsAsHex);

			while (outText.Length > numOfBytes)
				//remove last character
				outText = outText.Remove(outText.Length - 1);

			return outText;
		}

		public String GetASCII8String(short numOfBytes, float value)
		{
			Byte[] tempBytes = { 0x00 };
			Encoding enc = Encoding.GetEncoding("IBM437");

			tempBytes = BitConverter.GetBytes(value);

			Char[] CharsAsHex = enc.GetChars(tempBytes);
			String outText = new String(CharsAsHex);

			while (outText.Length > numOfBytes)
				outText = outText.Remove(outText.Length - 1);

			return outText;
		}

		public Int32 GetColorTotal(bool NoDupes = false)
		{
			if (!NoDupes)
				return ThreadsInDesign.Count;
			else
			{
				Thread LastThread = new Thread();
				int count = 0;

				foreach (Thread T in ThreadsInDesign)
				{
					if (T != LastThread)
						count++;

					LastThread = T;
				}

				return count;
			}
		}

		public String GetDebugInfo()
		{
			//System.IO.StringWriter outfile = new System.IO.StringWriter();
			//string name = "";
			//outfile.WriteLine("PES header");
			//outfile.WriteLine("PES number:\t" + pesNum);
			/*for (int i = 0; i < pesHeader.Count; i++)
			{
				name = (i + 1).ToString();
				outfile.WriteLine(name + "\t" + pesHeader[i].ToString());
			}
			if (embOneHeader.Count > 0)
			{
				outfile.WriteLine("CEmbOne header");
				for (int i = 0; i < embOneHeader.Count; i++)
				{
					switch (i + 1)
					{
						case 22:
							name = "translate x";
							break;
						case 23:
							name = "translate y";
							break;
						case 24:
							name = "width";
							break;
						case 25:
							name = "height";
							break;
						default:
							name = (i + 1).ToString();
							break;
					}

					outfile.WriteLine(name + "\t" + embOneHeader[i].ToString());
				}
			}
			if (embPunchHeader.Count > 0)
			{
				outfile.WriteLine("CEmbPunch header");
				for (int i = 0; i < embPunchHeader.Count; i++)
				{
					switch (i + 1)
					{
						default:
							name = (i + 1).ToString();
							break;
					}

					outfile.WriteLine(name + "\t" + embPunchHeader[i].ToString());
				}
			}

			outfile.WriteLine("stitches start: " + startStitches.ToString());
			outfile.WriteLine("block info");
			outfile.WriteLine("number\tcolor\tstitches");
			for (int i = 0; i < this.blocks.Count; i++)
			{
				outfile.WriteLine((i + 1).ToString() + "\t" + blocks[i].colorIndex.ToString() + "\t" + blocks[i].stitchesTotal.ToString());
			}
			outfile.WriteLine("color table");
			outfile.WriteLine("number\ta\tb");
			for (int i = 0; i < colorTable.Count; i++)
			{
				outfile.WriteLine((i + 1).ToString() + "\t" + colorTable[i].a.ToString() + ", " + colorTable[i].b.ToString());
			}
			if (blocks.Count > 0)
			{
				outfile.WriteLine("Extended stitch debug info");
				for (int blocky = 0; blocky < blocks.Count; blocky++)
				{
					outfile.WriteLine("block " + (blocky + 1).ToString() + " start");
					for (int stitchy = 0; stitchy < blocks[blocky].stitches.Length; stitchy++)
					{
						outfile.WriteLine(blocks[blocky].stitches[stitchy].X.ToString() + ", " + blocks[blocky].stitches[stitchy].Y.ToString());
					}
				}
			}
			outfile.Close();
			return outfile.ToString();*/

			return "";
		}

		/*public MyRect GetBoundsOfDesign()
		{
			MyRect OrigBounds = PESBlocksInDesignByColor[0].GetColorBounds();
			MyRect Bounds = new MyRect(OrigBounds.Left, OrigBounds.Top,
				OrigBounds.Right, OrigBounds.Bottom);

			foreach (ColorBlock CB in PESBlocksInDesignByColor)
			{
				MyRect Test = CB.GetColorBounds();

				Bounds.Left = Math.Min(Bounds.Left, Test.Left);
				Bounds.Top = Math.Min(Bounds.Top, Test.Top);
				Bounds.Right = Math.Max(Bounds.Right, Test.Right);
				Bounds.Bottom = Math.Max(Bounds.Bottom, Test.Bottom);
			}

			return Bounds;
		}*/

		public String GetFilename()
		{
			return _filename;
		}

		public MyRect GetBoundsOfDesign()
		{
			MyRect Bounds = BlocksInDesignByColor[0].GetColorBounds();
			
			foreach (ColorBlock CB in BlocksInDesignByColor)
			{
				MyRect Test = CB.GetColorBounds();

				if (Test == null)
					continue;

				Bounds.Left = Math.Min(Bounds.Left, Test.Left);
				Bounds.Top = Math.Min(Bounds.Top, Test.Top);
				Bounds.Right = Math.Max(Bounds.Right, Test.Right);
				Bounds.Bottom = Math.Max(Bounds.Bottom, Test.Bottom);			
			}

			//Bounds.Offset(SaveOffset);

			return Bounds;
		}

		public StitchBlock GetFirstStitchBlock()
		{
			return BlocksInDesignByColor[0].GetStitchBlocks()[0];
		}

		protected ushort GetUHoopWidth()
		{
			return this.uHoopWidth;
		}

		public String GetLastError()
		{
			return lastError;
		}

		public StitchBlock GetLastStitchBlock()
		{
			int lastColorBlock = BlocksInDesignByColor.Count - 1;
			int lastStitchBlock = (int)BlocksInDesignByColor
				[lastColorBlock].Count - 1;

			return BlocksInDesignByColor[lastColorBlock].GetStitchBlocks()
				[lastStitchBlock];
		}

		public Int16 GetReadyStatus()
		{
			return readyStatus;
		}

		public Int32 GetRelHeight()
		{
			EmbRectangle BoundingBox = CalcRelBoundBox();

			return BoundingBox.getHeight();
		}


		public Int32 GetRelWidth()
		{
			EmbRectangle BoundingBox = CalcRelBoundBox();

			return BoundingBox.getWidth();
		}

		public List<Thread> GetThreads()
		{
			return ThreadsInDesign;
		}

		public int GetNumberOfBlocks()
		{
			int BlockCount = 0;

			foreach (ColorBlock CB in BlocksInDesignByColor)
			{
				BlockCount += CB.GetStitchBlocks().Count;
			}

			return BlockCount;
		}

		public short GetLastColorIndex()
		{
			return BlocksInDesignByColor[BlocksInDesignByColor.Count - 
				1].GetColorIndex();
		}

		public ColorBlock GetLastColorBlock()
		{
			return BlocksInDesignByColor[BlocksInDesignByColor.Count - 1];
		}

		public uint GetJumpCount(Int32 ThreadNumber)
		{
			return BlocksInDesignByColor[ThreadNumber].ModBlockCount - 1;
		}

		//when getting closer to working, replace this with getting the designs left bounding box property
		public Int16 GetOriginalLeft()
		{
			return (short)GetBoundsOfDesign().Left;
		}

		//Same as above
		public Int16 GetOriginalTop()
		{
			return (short)GetBoundsOfDesign().Top;
		}

		/*public MyRect GetColorBlockBounds(int Block)
		{
			return BlocksInDesignByColor[Block].GetColorBounds();
		}*/

		public ColorBlock GetFirstColorBlock()
		{
			return BlocksInDesignByColor[0];
		}

		public String WriteNewDesignBreak(Stitch Last, ColorBlock Next)
		{
			String DesignBreak = "";
			int FirstBlock = Next.GetFirstRealBlockNumber();
			//What to do if FirstBlock returns -1? FREAK OUT:?!?
			int StitchTotal = Next.GetStitchBlocks()[FirstBlock].GetStitchTotal();
			
			if (Last != null)
			{
				DesignBreak += WriteColorBreak(Last, Next, FirstBlock, true);
				DesignBreak += GetASCII8String(2, 0x8003);
				StitchTotal--;
			}

			DesignBreak += GetASCII8String(2, 0);
			DesignBreak += GetASCII8String(2, Next.ColorIndex);

			if (Last == null)
				StitchTotal++;

			DesignBreak += GetASCII8String(2, StitchTotal);

			if (Last == null)
			{
				DesignBreak += GetASCII8String(2, BlocksInDesignByColor[0].GetStitchBlocks()[0].GetStitchList()[0].XX + GetSaveOffset().X);
				DesignBreak += GetASCII8String(2, BlocksInDesignByColor[0].GetStitchBlocks()[0].GetStitchList()[0].YY + GetSaveOffset().Y);
			}
			
			return DesignBreak;
		}

		public String WriteColorBreak(Stitch Prev, ColorBlock Next, int FirstRealBlock, bool FirstBlockInDesign = false)
		{
			String ColorBreak = "";
			ToggleValue = false;
			MyRect NextBoundingBox = Next.GetColorBounds();
			int NumberOfBreaksInNext = (int)Next.ModBlockCount - 1;
			Stitch NextStitch = Next.GetFirstFlaglessStitch();
			short NumberOfStitchesInNext = -1;

			if (FirstRealBlock != -1)
				NumberOfStitchesInNext = (short)(Next.GetStitchBlocks()[FirstRealBlock].GetStitchTotal() - 1);
			else
			{
				NumberOfBreaksInNext = 1;
				NextStitch = Next.GetStitchBlocks()[0].GetStitchList()[0];
				Stitch LastStitch = Next.GetLastStitchInColorBlock();
				NextBoundingBox = new MyRect(Math.Min(NextStitch.XX, LastStitch.XX), Math.Min(NextStitch.YY, LastStitch.YY), Math.Max(NextStitch.XX, LastStitch.XX), Math.Max(NextStitch.YY, LastStitch.YY));
				NumberOfStitchesInNext = 1;
			}

			ColorBreak += GetASCII8String(4, 1);
			ColorBreak += GetASCII8String(2, Prev.ThreadSelection.ColorInIndex);
			ColorBreak += GetASCII8String(2, 0x8001);

			if (IsSideways)
				SaveOffset.Y += SidewaysOffset;

			for (int i = 0; i < 2; i++)
			{
				ColorBreak += GetASCII8String(2, NextBoundingBox.Left + SaveOffset.X);
				ColorBreak += GetASCII8String(2, NextBoundingBox.Top + SaveOffset.Y);
				ColorBreak += GetASCII8String(2, NextBoundingBox.Right + SaveOffset.X);
				ColorBreak += GetASCII8String(2, NextBoundingBox.Bottom + SaveOffset.Y);
			}

			ColorBreak += GetAffineTransform();
			ColorBreak += GetASCII8String(2, 1);

			ColorBreak += GetASCII8String(2, NextBoundingBox.Left + SaveOffset.X);
			ColorBreak += GetASCII8String(2, NextBoundingBox.Bottom + SaveOffset.Y);
			ColorBreak += GetASCII8String(2, NextBoundingBox.Width);
			ColorBreak += GetASCII8String(2, NextBoundingBox.Height);
			
			ColorBreak += GetASCII8String(8, 0);

			if (FirstBlockInDesign)
			{
				NumberOfBreaksInNext *= 2;
				NumberOfBreaksInNext++;
			}
			else
			{
				NumberOfBreaksInNext *= 2;
				NumberOfBreaksInNext += 3;
			}

			if (FirstRealBlock != -1)
				ColorBreak += GetASCII8String(2, NumberOfBreaksInNext);
			else 
				//total hack here. Not sure what you're supposed to do with a color block with no real stitches
				ColorBreak += GetASCII8String(2, 1);

			if (!FirstBlockInDesign)
			{
				ColorBreak += WriteSectionBreak(Prev, NextStitch, NumberOfStitchesInNext, true, false);
			}
			
			return ColorBreak;
		}

		public int GetID()
		{
			return _designID;
		}

		public void SetAsPrimeDesign()
		{
			_primeDesign = true;
		}

		public bool IsPrimeDesign()
		{
			return _primeDesign;
		}

		public String WriteSectionBreak(Stitch Last, Stitch Next, int ActualStitchTotal, bool Orig, bool toggle = true, bool firstBlock = false)
		{
			String SectionBreak = "";
			short ColorValue = (short)Next.ThreadSelection.ColorInIndex;

			if (ActualStitchTotal == 1)
				ColorValue = 0x20;

			if (!firstBlock)
			{
				if (Orig && ActualStitchTotal > 1)
				{
					if (!toggle)
					{
						SectionBreak += WriteSectionBreak(Last, Next, 2, false, toggle);
						toggle = !toggle;
					}

					SectionBreak += WriteSectionBreak(Last, Next, 2, false);
				}

				SectionBreak += GetASCII8String(2, 0x8003);

				if (toggle)
					ToggleValue = !ToggleValue;
			}

			if (ToggleValue)
				SectionBreak += GetASCII8String(2, 1);
			else
				SectionBreak += GetASCII8String(2, 0);

			SectionBreak += GetASCII8String(2, ColorValue);
			SectionBreak += GetASCII8String(2, ActualStitchTotal);
			
			if (!toggle && !firstBlock && (ActualStitchTotal == 2 && !Orig))
				Next = Last;

			if (ActualStitchTotal == 2 && !Orig)
			{
				SectionBreak += GetASCII8String(2, Last.XX + SaveOffset.X);
				SectionBreak += GetASCII8String(2, Last.YY + SaveOffset.Y);
				SectionBreak += GetASCII8String(2, Next.XX + SaveOffset.X);
				SectionBreak += GetASCII8String(2, Next.YY + SaveOffset.Y);
			}
			else if (ActualStitchTotal == 1)
			{

			}

			return SectionBreak;
		}

		public void SetHoopWidth(ushort HoopWidth)
		{
			this.uHoopWidth = HoopWidth;
		}

		public void SetPixelToMMRatio(Single Ratio)
		{
			PixelToMMRatio = Ratio;
		}

		/*public void SetStartingPoint(Point Start)
		{
			StartStitch = Start;
			StartStitchSet = true;
			minX = StartStitch.X;
			maxX = StartStitch.X;
			minY = StartStitch.Y;
			maxY = StartStitch.Y;
		}*/

		/*public Point GetStartingPoint()
		{
			return StartStitch;
		}*/

		/*public Int32 GetNumberOfColorBlocks()
		{
			return PESBlocksInDesignByColor.Count;
		}*/

		//does not insert or delte stitches to preserve stitch density
		public void Scale(Point ScaleValues)
		{

			short MinX = 32767;
			short MinY = 32767;

			bool firstStitch = true;

			foreach (ColorBlock CB in BlocksInDesignByColor)
			{
				List<StitchBlock> LSB = CB.GetStitchBlocks();

				foreach (StitchBlock SB in LSB)
				{
					List<Stitch> LS = SB.GetStitchList();

					foreach (Stitch S in LS)
					{
						Point Temp = new Point(ScaleValues.X * S.XX, ScaleValues.Y * S.YY);
						Point DeltaTemp = new Point(S.Delta.X * ScaleValues.X, S.Delta.Y * ScaleValues.Y);

						S.XX = (short)Temp.X;
						S.YY = (short)Temp.Y;
						S.Delta = DeltaTemp;

						MinX = (short)Math.Min(Temp.X, MinX);
						MinY = (short)Math.Min(Temp.Y, MinY);

						/*if (ScaleValues.X == -1.0)
							S.YY += (short)GetBoundingBox().Height;
						if (ScaleValues.Y == -1.0)
							S.XX += (short)GetBoundingBox().Width;*/
					}		   
				}
			}

			foreach (ColorBlock CB in BlocksInDesignByColor)
			{
				foreach (StitchBlock SB in CB.GetStitchBlocks())
				{
					foreach (Stitch S in SB.GetStitchList())
					{
						S.XX += Math.Abs(MinX);
						S.YY += Math.Abs(MinY);

						if (firstStitch)
						{
							S.Delta = new Point(S.Delta.X + Math.Abs(MinX), S.Delta.Y + Math.Abs(MinY));
							firstStitch = false;
						}
					}
				}

				CB.BoundsUpdated();
				//CB.UpdateBounds();
			}
		}

		/*Point ScaleLine(Point Prev, Point Cur, Point Offset, float ratio)
		{
			//Translate to origin
			Point Next = new Point(Cur.X - Prev.X, Cur.Y - Prev.Y);

			//scale
			Next.X = (int)(ratio * Next.X);
			Next.Y = (int)(ratio * Next.Y);

			//translate back
			Next.X += Prev.X;
			Next.Y += Prev.Y;

		}*/

		public List<List<Point>> ScaleDesignByColor(int width, int height, int canvasWidth, int canvasHeight, float ZoomLevel, Point ImageLoc)
		{
			List<Point> ScaledList = new List<Point>();
			List<List<Point>> ListOfScaledLists = new List<List<Point>>();
			float widthToCanvas = ((float)(width) / (float)canvasWidth);
			float heightToCanvas = ((float)(height) / (float)canvasWidth);
			float ratio = Math.Min(widthToCanvas, heightToCanvas);
						
			foreach (ColorBlock CB in BlocksInDesignByColor)
			{
				List<StitchBlock> LSB = CB.GetStitchBlocks();
												
				foreach (StitchBlock SB in LSB)
				{
					List<Stitch> LS = SB.GetStitchList();

					foreach (Stitch S in LS)
					{
						if (S.Flags == NORMAL)
						{
							int x = 0;
							int y = 0;

							//always use imageLoc
							/*if (ZoomLevel != -1.0f)
							{
								x = (int)Math.Round((S.XX / ZoomLevel) * 
									ratio, 0, MidpointRounding.ToEven);
								y = (int)Math.Round((S.YY / ZoomLevel) * 
									ratio, 0, MidpointRounding.ToEven);
							}*/
							//else //use imageloc
							//{
								x = (int)Math.Round(((S.XX + ImageLoc.X) * ratio), 0, MidpointRounding.ToEven);
								y = (int)Math.Round(((S.YY + ImageLoc.Y) * ratio), 0, MidpointRounding.ToEven);
							//}

							ScaledList.Add(new Point(x, y));
						}
					}
				}

				ListOfScaledLists.Add(ScaledList);
				ScaledList = new List<Point>();
			}

			if (width > height)
				ListOfScaledLists = Rotate90(ListOfScaledLists);
			if (IsSideways)
				ListOfScaledLists = Rotate270(ListOfScaledLists);

			return ListOfScaledLists;
		}

		List<List<Point>> Rotate270(List<List<Point>> PointList)
		{
			int MinY = 0;

			foreach (List<Point> LP in PointList)
			{
				for (int i = 0; i < LP.Count; i++)
				{
					//Point Replacement = LP[i];

					//Replacement.X = LP[i].Y;
					//Replacement.Y = -LP[i].X;

					//LP[i] = Replacement;
					MinY = Math.Min(LP[i].Y, MinY);
				}
			}

			foreach (List<Point> LP in PointList)
			{
				for (int i = 0; i < LP.Count; i++)
				{
					Point Replacement = LP[i];
					Replacement.Y += Math.Abs(MinY);
					LP[i] = Replacement;
				}
			}

			return PointList;
		}

		List<List<Point>> Rotate90(List<List<Point>> PointList)
		{
			int MinX = 0;

			foreach (List<Point> LP in PointList)
			{
				for (int i = 0; i < LP.Count; i++)
				{
					Point Replacement = LP[i];

					Replacement.X = -LP[i].Y;
					Replacement.Y = LP[i].X;

					LP[i] = Replacement;
					MinX = Math.Min(LP[i].X, MinX);
				}
			}

			foreach (List<Point> LP in PointList)
			{
				for (int i = 0; i < LP.Count; i++)
				{
					Point Replacement = LP[i];
					Replacement.X += Math.Abs(MinX);
					LP[i] = Replacement;
				}
			}

			return PointList;
		}

		public double ToRadians(float degrees)
		{
			return (degrees * (Math.PI / 180.0f));
		}

		public void RotateStitches(float degrees, Point TopLeft, int hoopWidth)
		{
			short MinX = 32767;
			short MinY = 32767;
			short MaxX = -32767;
			short MaxY = -32767;
			short hoopMaxX = 0;
			short hoopMaxY = 0;
			
			bool firstStitch = true;
			short tempX = 0;
			short tempY = 0;

			Stitch LastStitch = null;
			Point Center = GetBoundsOfDesign().Center;
			
			int debugCounter = 0;

			switch (hoopWidth)
			{
				case 4:
					hoopMaxX = 1000;
					hoopMaxY = 1000;
					break;
				case 5:
					hoopMaxX = 1300;
					hoopMaxY = 1800;
					break;
				case 7:
					hoopMaxX = 1800;
					hoopMaxY = 1300;
					break;
			}
						
			foreach (ColorBlock CB in BlocksInDesignByColor)
			{
				foreach (StitchBlock SB in CB.GetStitchBlocks())
				{
					foreach (Stitch S in SB.GetStitchList())
					{
						//Point Temp = new Point(-S.YY, S.XX);
						Point DeltaTemp = new Point();
						short deltaTempX = 0;
						short deltaTempY = 0;

						if (LastStitch == null)
						{

						}
						else if (LastStitch.Flags != NORMAL && LastStitch.Flags == S.Flags)
						{
							if (LastStitch.XX == S.XX)
							{
								if (LastStitch.YY == S.YY)
									S.Delta = new Point();
							}
						} 
												
						//short tempX = (short)(((S.XX - Center.X) * (short)Math.Cos(ToRadians(degrees))) - ((S.YY - Center.Y) * (short)Math.Sin(ToRadians(degrees))));
						//short tempY = (short)(((S.XX - Center.X) * (short)Math.Sin(ToRadians(degrees))) + ((S.YY - Center.Y) * (short)Math.Cos(ToRadians(degrees))));

						if (firstStitch)
						{
							deltaTempX = (short)(((S.Delta.X - Center.X) * (short)Math.Cos(ToRadians(degrees))) - ((S.Delta.Y - Center.Y) * (short)Math.Sin(ToRadians(degrees))));
							deltaTempY = (short)(((S.Delta.X - Center.X) * (short)Math.Sin(ToRadians(degrees))) + ((S.Delta.Y - Center.Y) * (short)Math.Cos(ToRadians(degrees))));
							DeltaTemp = new Point(deltaTempX + Center.X, deltaTempY + Center.Y);
							tempX = (short)DeltaTemp.X;
							tempY = (short)DeltaTemp.Y;
							firstStitch = false;
						}
						else //if (S.Delta != new Point())
						{
							deltaTempX = (short)(((S.Delta.X) * (short)Math.Cos(ToRadians(degrees))) - ((S.Delta.Y) * (short)Math.Sin(ToRadians(degrees))));
							deltaTempY = (short)(((S.Delta.X) * (short)Math.Sin(ToRadians(degrees))) + ((S.Delta.Y) * (short)Math.Cos(ToRadians(degrees))));
							DeltaTemp = new Point(deltaTempX, deltaTempY);
							tempX += deltaTempX;
							tempY += deltaTempY;
						}
						
						//Point Temp = new Point(tempX + Center.X, tempY + Center.Y);
						
						S.XX = (short)tempX;
						S.YY = (short)tempY;
						S.Delta = DeltaTemp;
						
						MinX = (short)Math.Min(tempX, MinX);
						MinY = (short)Math.Min(tempY, MinY);
						MaxX = (short)Math.Max(tempX, MaxX);
						MaxY = (short)Math.Max(tempY, MaxY);

						LastStitch = S;

						debugCounter++;
					}
				}
			}

			firstStitch = true;
			debugCounter = 0;

			foreach (ColorBlock CB in BlocksInDesignByColor)
			{
				foreach (StitchBlock SB in CB.GetStitchBlocks())
				{
					foreach (Stitch S in SB.GetStitchList())
					{
						if (MinX < 0)
							S.XX += Math.Abs(MinX);
						if (MinY < 0)
							S.YY += Math.Abs(MinY);
						if (MaxX > hoopMaxX)
							S.XX -= (short)(MaxX - hoopMaxX);
						if (MaxY > hoopMaxY)
							S.YY -= (short)(MaxY - hoopMaxY);

						if (firstStitch)
						{
							Point Offset = new Point();
							if (MinX < 0)
								Offset.X = Math.Abs(MinX);
							if (MinY < 0)
								Offset.Y = Math.Abs(MinY);
							if (MaxX > hoopMaxX)
								Offset.X = hoopMaxX - MaxX;
							if (MaxY > hoopMaxY)
								Offset.Y = hoopMaxY - MaxY;

							S.Delta = new Point(S.Delta.X + Offset.X, S.Delta.Y + Offset.Y);
							firstStitch = false;
						}

						debugCounter++;
					}
				}

				CB.BoundsUpdated();
				//CB.UpdateBounds();
			}

			if (MinX < 0)
				SidewaysOffset = Math.Abs(MinX);
			if (MaxX > hoopMaxX)
				SidewaysOffset = MaxX - hoopMaxX;

			currentAngle = degrees;
		}

		//currently only works in 90 degree increments
		public void Rotate(float degrees, MyRect DesignPos, float zoom, short hoopWidth)
		{
			MyRect TempRect = DesignPos;
						
			TempRect *= (1 / zoom);

			RotateStitches(degrees, new Point(TempRect.Left, TempRect.Top), hoopWidth);

			//while (currentAngle % 360.0f != degrees)
				//RotateStitches90(); //RotateStitches Funciton updates currentAngle

			//MyRect CurrentBounds = GetBoundsOfDesign();

			/*if (DesignPos.Left != CurrentBounds.Left)
			{
				int xDiff = CurrentBounds.Left - DesignPos.Left;
				
				Translate(new Point(xDiff, 0));
			} 

			if (DesignPos.Top != CurrentBounds.Top)
			{
				int yDiff = CurrentBounds.Top - DesignPos.Top;

				Translate(new Point(0, yDiff));
			} */
		}

		public void Translate(Point Amount)
		{
			bool firstStitch = true;

			foreach (ColorBlock CB in BlocksInDesignByColor)
			{
				foreach (StitchBlock SB in CB.GetStitchBlocks())
				{
					foreach (Stitch S in SB.GetStitchList())
					{
						S.XX += (short)Amount.X;
						S.YY += (short)Amount.Y;

						if (firstStitch)
						{
							S.Delta = new Point(S.Delta.X + Amount.X, S.Delta.Y + Amount.Y);
							firstStitch = false;
						}
					}
				}
			}
		}

		public void SetSideways270Offset()
		{
			short MinY = 32767;

			foreach (ColorBlock CB in BlocksInDesignByColor)
			{
				foreach (StitchBlock SB in CB.GetStitchBlocks())
				{
					foreach (Stitch S in SB.GetStitchList())
					{
						MinY = Math.Min(S.YY, MinY);
					}
				}
			}

			SidewaysOffset = Math.Abs(MinY);
		}

		void AddBitmapJump(Point Current, Point Last, Graphics G, Color col)
		{
			Point ThisPoint = new Point(Current.X, Current.Y);

			Last = new Point((int)(Last.X *	PixelToMMRatio), (int)(Last.Y *
				PixelToMMRatio));

			ThisPoint = new Point((int)(ThisPoint.X *
				PixelToMMRatio), (int)(ThisPoint.Y *
				PixelToMMRatio));

			Point[] line = new Point[2] { Last, ThisPoint };
			Single[] dashValues = { 5, 5, 5, 5 };
			Pen tempPen = new Pen(col, 0.0001f);
			tempPen.DashPattern = dashValues;
			tempPen.StartCap =
				System.Drawing.Drawing2D.LineCap.Round;
			tempPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
			tempPen.LineJoin =
				System.Drawing.Drawing2D.LineJoin.Round;
			G.SmoothingMode =
				System.Drawing.Drawing2D.SmoothingMode.HighQuality;
			G.DrawLines(tempPen, line);
		}

		public Bitmap ToBitmap(Single threadThickness, Boolean FirstDesignInPattern = false, Boolean ShowJumpStitches = true)
		{
			Bitmap DrawArea;
			Graphics xGraph;
			MyRect Bounds = GetBoundsOfDesign();
			Int32 bitmapWidth = (Int32)((Bounds.Width + (Int32)(threadThickness	* 2)) * PixelToMMRatio);
			Int32 bitmapHeight = (Int32)((Bounds.Height + (Int32)
				(threadThickness * 2)) * PixelToMMRatio);
			int sidewaysYOffset = 0;
			bool firstStitchBlock = true;

			if (IsSideways)
				sidewaysYOffset = SidewaysOffset;
				
			DrawArea = new Bitmap(bitmapWidth, bitmapHeight);

			

			xGraph = Graphics.FromImage(DrawArea);
			xGraph.TranslateTransform(threadThickness - (minX * PixelToMMRatio), threadThickness - (minY * PixelToMMRatio));

			DrawArea = new Bitmap(bitmapWidth, bitmapHeight);
			xGraph = Graphics.FromImage(DrawArea);
			xGraph.TranslateTransform(threadThickness - (minX * PixelToMMRatio), threadThickness - (minY * PixelToMMRatio));

			Point LastPoint = new Point();
			//bool FirstStitch = true;
			
			foreach (ColorBlock CB in BlocksInDesignByColor)
			{
				List<StitchBlock> LSB = CB.GetStitchBlocks();

				foreach (StitchBlock SB in LSB)
				{
					if (SB.GetStitchTotal() > 1)
					{
						Pen tempPen = new Pen(SB.GetColor(), threadThickness);
						List<Point> ModPoints = new List<Point>();

						foreach (Stitch S in SB.GetStitchList())
						{
							Point StitchPoint = new Point(S.XX, S.YY);

							StitchPoint.Y -= sidewaysYOffset;

							if (S.Flags == NORMAL || S.Flags == SEQUIN)
							{
								LastPoint = new Point((Int32)(StitchPoint.X * PixelToMMRatio), (Int32)(StitchPoint.Y * PixelToMMRatio));
								ModPoints.Add(LastPoint);
							}
							else if (S.Flags == JUMP)
							{
								if (!(firstStitchBlock && FirstDesignInPattern))
									AddBitmapJump(StitchPoint, LastPoint, xGraph, tempPen.Color);
							}
							else if (S.Flags == TRIM)
							{
								if (!(firstStitchBlock && FirstDesignInPattern))
									AddBitmapJump(StitchPoint, LastPoint, xGraph, Color.LightGray);
							}
							/*else if (S.Flags == SEQUIN && S != SB.GetLastStitch
								())
							{
								AddBitmapJump(StitchPoint, LastPoint, xGraph,
									tempPen.Color);
							}*/

							firstStitchBlock = false;
						}

						if (ModPoints.Count > 1)
						{
							tempPen.StartCap =
								System.Drawing.Drawing2D.LineCap.Round;
							tempPen.EndCap = 
								System.Drawing.Drawing2D.LineCap.Round;
							tempPen.LineJoin =
								System.Drawing.Drawing2D.LineJoin.Round;
							xGraph.SmoothingMode =
							System.Drawing.Drawing2D.SmoothingMode.HighQuality;
							xGraph.DrawLines(tempPen, ModPoints.ToArray());
						}
					}
				}
			}

			xGraph.Dispose();
			return DrawArea;
		}
	}

	public class Stitch
	{
		byte _flags;
		protected Thread thread;
		byte _stitchType;
		Point _location;
		Point _delta;

		public const byte UNKNOWN = 0xFF;
		public const byte PECSTITCH = 0;
		public const byte PESSTITCH = 1;
		

		public byte Flags
		{
			get
			{
				byte value = _flags;
				return value;
			}
		}
		public Int16 XX
		{
			get
			{
				int value = _location.X;
				return (short)value;
			}
			set
			{
				_location.X = value;
			}
		}
		public Int16 YY
		{
			get
			{
				short value = (short)_location.Y;
				return value;
			}
			set
			{
				_location.Y = value;
			}
		}
		public Thread ThreadSelection
		{
			get
			{
				return thread;
			}
		}
		public byte Type
		{
			get
			{
				byte value = _stitchType;
				return value;
			}
		}

		public Point Delta
		{
			get
			{
				Point Value = _delta;
				return Value;
			}

			set
			{
				_delta = value;
			}
		}

		public Point GetPoint()
		{
			Point Value = new Point(_location.X, _location.Y);
			return Value;
		}

		public Stitch(Point Location, Point Delta, byte flag, Thread Thread, byte Type)
		{
			_flags = flag;
			_location = Location;
			thread = Thread;
			_stitchType = Type;
			_delta = Delta;
		}

		public Stitch()
		{
			_flags = 0;
			_location = new Point(0, 0);
			_delta = new Point(0, 0);
			thread = new Thread();
			_stitchType = Stitch.UNKNOWN;
		}
	}

	public class Thread
	{
		public Color getColorFromHexStr(String HexDigts)
		{
			/* expect string beginning with 6 hex digits */
			Int32 rgbColor = 0;
			Color _Color = new Color();

			for (int i = 0; i < 6; i++)
			{
				if (HexDigts[i] >= '0' && HexDigts[i] <= '9')
				{
					rgbColor *= 16;
					rgbColor += HexDigts[i] - '0';
				}
				else if (HexDigts[i] >= 'A' && HexDigts[i] <= 'F')
				{
					rgbColor *= 16;
					rgbColor += HexDigts[i] - 'A';
				}
				else if (HexDigts[i] >= 'a' && HexDigts[i] <= 'f')
				{
					rgbColor *= 16;
					rgbColor += HexDigts[i] - 'a';
				}
			}

			_Color = System.Drawing.Color.FromArgb(((rgbColor >> 16) & 0xFF), ((rgbColor >> 8) & 0xFF), (rgbColor & 0xFF));

			return _Color;
		}

		Color _threadColor;
		String _description;
		String _catalogNumber;
		short _threadColorIndex;

		public Color ThreadColor
		{
			get
			{
				Color Value = _threadColor;
				return Value;
			}
		}

		public short ColorInIndex
		{
			get
			{
				short value = _threadColorIndex;
				return value;
			}
		}

		public String Description
		{
			get
			{
				String description = _description;
				return description;
			}
		}

		public Int32 findNearestColor(System.Drawing.Color color, List<Thread> colors)
		{
			Double currentClosestValue = 9999999;
			Int32 closestIndex = -1;
			Int32 red = color.R;
			Int32 green = color.G;
			Int32 blue = color.B;
			List<Thread> currentThreadItem = colors;

			for (int i = 0; i < currentThreadItem.Count; i++)
			{
				int deltaRed;
				int deltaBlue;
				int deltaGreen;
				Double dist;

				Color c = currentThreadItem[i].ThreadColor;
				deltaRed = red - c.R;
				deltaBlue = green - c.G;
				deltaGreen = blue - c.B;
				dist = System.Math.Sqrt((Double)(deltaRed * deltaRed) + (deltaBlue * deltaBlue) + (deltaGreen * deltaGreen));

				if (dist <= currentClosestValue)
				{
					currentClosestValue = dist;
					closestIndex = i;
				}
			}
			return closestIndex;
		}

		public Thread()
		{
			Random rnd = new Random();

			_threadColor = System.Drawing.Color.FromArgb(rnd.Next(0, 255), 
				rnd.Next(0, 255), rnd.Next(0, 255));
			_description = "random";
			_catalogNumber = "";
			_threadColorIndex = -1;
		}

		public Thread(Color ThreadColor, String Description, String 
			CatalogNumber, short ColorIndex)
		{
			_threadColor = ThreadColor;
			_description = Description;
			_catalogNumber = CatalogNumber;
			_threadColorIndex = ColorIndex;
		}
	}

	public class EmbStruct
	{
		protected Int32 linetype;
		protected List<Stitch> stitchList = new List<Stitch>();

		public EmbStruct()
		{
		}
	}

	public class EmbShape : EmbStruct
	{
		protected Double rotation;

		public EmbShape()
		{
		}
	}

	public class EmbArc : EmbLine
	{
		private Double midX;    // absolute position (not relative) 
		private Double midY;

		public EmbArc(Double sx, Double sy, Double mx, Double my, Double ex, Double ey)
			: base(sx, sy, ex, ey)
		{
			midX = mx;
			midY = my;
		}

		public EmbArc()
			: base()
		{
			midX = 0;
			midY = 0;
		}
	}

	public class EmbCircle : EmbEllipse
	{
		public Double getRadius()
		{
			return radiusX;
		}

		public EmbCircle(Double cx, Double cy, Double r)
			: base(cx, cy, r, r)
		{
		}

		public EmbCircle()
			: base()
		{
		}
	}

	public class EmbEllipse : EmbShape
	{
		protected Double centerX;
		protected Double centerY;
		protected Double radiusX;
		private Double radiusY;

		public Double getCenterX()
		{
			return centerX;
		}

		public Double getCenterY()
		{
			return centerY;
		}

		public Double getRadiusX()
		{
			return radiusX;
		}

		public Double getRadiusY()
		{
			return radiusY;
		}

		public Double getDiameterX()
		{
			return radiusX * 2.0;
		}

		public Double getDiameterY()
		{
			return radiusY * 2.0;
		}

		public Double getWidth()
		{
			return radiusX * 2.0;
		}

		public Double getHeight()
		{
			return radiusY * 2.0;
		}

		public EmbEllipse(double cx, double cy, double rx, double ry)
			: base()
		{
			centerX = cx;
			centerY = cy;
			radiusX = rx;
			radiusY = ry;
		}

		public EmbEllipse()
			: base()
		{
			centerX = 0;
			centerY = 0;
			radiusX = 0;
			radiusY = 0;
		}
	}

	public class EmbLine : EmbStruct
	{
		protected Double x1;
		protected Double y1;
		protected Double x2;
		protected Double y2;

		public Double getX1()
		{
			return x1;
		}

		public Double getY1()
		{
			return y1;
		}

		public Double getX2()
		{
			return x2;
		}

		public Double getY2()
		{
			return y2;
		}

		public EmbLine(Double x1, Double y1, Double x2, Double y2)
			: base()
		{
			this.x1 = x1;
			this.y1 = y1;
			this.x2 = x2;
			this.y2 = y2;
		}

		public EmbLine()
			: base()
		{
			x1 = 0;
			y1 = 0;
			x2 = 0;
			y2 = 0;
		}
	}

	public class EmbPoint
	{
		public Double xx;
		public Double yy;

		public EmbPoint(Double x, Double y)
		{
			xx = x;
			yy = y;
		}

		public EmbPoint()
		{
			xx = 0;
			yy = 0;
		}
	}

	public class EmbPolygon : EmbShape
	{
		private List<EmbPoint> polygonPoints = new List<EmbPoint>();

		public EmbPolygon(List<EmbPoint> polyPoints)
			: base()
		{
			polygonPoints = polyPoints;
		}

		public EmbPolygon()
			: base()
		{

		}
	}

	public class EmbRectangle : EmbShape
	{
		private Int32 top;
		private Int32 left;
		private Int32 bottom;
		private Int32 right;

		public Int32 Top
		{
			get
			{
				return top;
			}
			set
			{
				top = value;
			}
		}

		public Int32 Left
		{
			get
			{
				return left;
			}
			set
			{
				left = value;
			}
		}

		public Int32 Bottom
		{
			get
			{
				return bottom;
			}
			set
			{
				bottom = value;
			}
		}

		public Int32 Right
		{
			get
			{
				return right;
			}
			set
			{
				right = value;
			}
		}

		public Int32 getWidth()
		{
			Int32 width = right - left;
			return width;
		}

		public Int32 getHeight()
		{
			Int32 height = bottom - Top;
			return height;
		}

		public void setWidth(Int32 w)
		{
			right = left + w;
		}

		public void setHeight(Int32 h)
		{
			bottom = top + h;
		}

		public void resetCoordsTo(Int32 x1, Int32 y1, Int32 x2, Int32 y2)
		{
			left = x1;
			right = x2;
			top = y1;
			bottom = y1;
		}

		public void resetRectangle(Int32 x, Int32 y, Int32 w, Int32 h)
		{
			left = x;
			right = w;
			top = y;
			bottom = h;
		}

		public EmbRectangle(Int32 x, Int32 y, Int32 w, Int32 h)
		{
			left = x;
			right = w;
			top = y;
			bottom = h;
		}

		public EmbRectangle()
		{
			left = 0;
			right = 0;
			top = 0;
			bottom = 0;
		}
	}

	public class EmbSpline : EmbLine
	{
		private Double control1X;
		private Double control1Y;
		private Double control2X;
		private Double control2Y;

		public EmbSpline(Double startX, Double startY, Double control1X, Double control1Y, Double control2X, Double control2Y, Double endX, Double endY)
			: base(startX, startY, endX, endY)
		{
			this.control1X = control1X;
			this.control1Y = control1Y;
			this.control2X = control2X;
			this.control2Y = control2Y;
		}

		public EmbSpline()
			: base()
		{
			control1X = 0;
			control1Y = 0;
			control2X = 0;
			control2Y = 0;
		}
	}

	public class EmbPolyline : EmbStruct
	{
		private List<EmbPoint> polylinePoints = new List<EmbPoint>();

		public EmbPolyline(List<EmbPoint> polyPoints)
		{
			polylinePoints = polyPoints;
		}

		public EmbPolyline()
		{
		}
	}

	public class StitchBlock
	{
		Color _color;
		byte _colorIndex;
		//Point[] stitchPoints;
		//List<short> _allFlags = new List<short>();
		MyRect _bounds;
		List<Stitch> _stitches = new List<Stitch>();
		bool _firstStitchSet = false;
		short _realStitchCount = 0;
		
		public StitchBlock()
		{
			_color = System.Drawing.Color.Black;
		}

		public StitchBlock(List<Thread> ThreadLibrary, Int32 ThreadIndex)
		{
			//stitchPoints = new Point[stitchesTotal];
			_colorIndex = (byte)ThreadLibrary[ThreadIndex].ColorInIndex;
			_color = ThreadLibrary[ThreadIndex].ThreadColor;
		}

		public MyRect GetBlockBoundingBox()
		{
			if (_firstStitchSet == true)
				return new MyRect(_bounds);
			return new MyRect();
		}

		public Color GetColor()
		{
			Color Value = new Color();
			Value = _color;
			return Value;
		}

		public byte GetColorIndex()
		{
			byte value = new byte();
			value = _colorIndex;
			return value;
		}

		public List<Stitch> GetStitchList()
		{
			List<Stitch> Value = new List<Stitch>();
			Value = _stitches;
			return Value;
		}

		public short GetStitchTotal()
		{
			short value = new int();
			value = (short) _stitches.Count;
			return value;
		}

		public short GetStitchTotalMinusFlags()
		{
			return _realStitchCount;
		}

		public Point GetLastStitchPoint()
		{
			Point LastPoint = new Point(_stitches[_stitches.Count - 1].XX,
				_stitches[_stitches.Count - 1].YY);

			return LastPoint;
		}

		public Stitch GetLastStitch()
		{
			Stitch Value = new Stitch();

			Value = _stitches[_stitches.Count - 1];

			return Value;
		}

		public Stitch GetLastRealStitch()
		{
			int lastStitchID = _stitches.Count - 1;
			Stitch LastStitch = new Stitch();

			for (int i = lastStitchID; i >= 0; i--)
			{
				if (_stitches[i].Flags == DesignFormat.NORMAL)
				{
					lastStitchID = i;
					break;
				}
			}

			LastStitch = _stitches[lastStitchID];

			return LastStitch;
		}

		public void AddStitch(Stitch S)//, Stitch LastStitchOfPrevSB = null)
		{
			/*if (_stitches.Count != 0)
				S.SetPrev(GetLastStitch());
			else if (LastStitchOfPrevSB != null)
				S.SetPrev(LastStitchOfPrevSB);*/

			_stitches.Add(S);

			int MaxX, MaxY, MinX, MinY;
			
			if (!_firstStitchSet)
			{
				MaxX = S.XX;
				MaxY = S.YY;
				MinX = S.XX;
				MinY = S.YY;
				_firstStitchSet = true;
			}
			else
			{
				MaxX = _bounds.Right;
				MaxY = _bounds.Bottom;
				MinX = _bounds.Left;
				MinY = _bounds.Top;
			}

			for (int i = 0; i < _stitches.Count; i++)
			{
				MinX = Math.Min(_stitches[i].XX, MinX);
				MaxX = Math.Max(_stitches[i].XX, MaxX);
				MinY = Math.Min(_stitches[i].YY, MinY);
				MaxY = Math.Max(_stitches[i].YY, MaxY);
			}

			_bounds = new MyRect(MinX, MinY, MaxX, MaxY);

			if (S.Flags == DesignFormat.NORMAL)
				_realStitchCount++;
		}

		public void UpdateBounds()
		{
			int MinX = 32767;//_bounds.Left;
			int MinY = 32767;// _bounds.Top;
			int MaxX = -32767;// _bounds.Right;
			int MaxY = -32767;// _bounds.Bottom;

			for (int i = 0; i < _stitches.Count; i++)
			{
				MinX = Math.Min(_stitches[i].XX, MinX);
				MaxX = Math.Max(_stitches[i].XX, MaxX);
				MinY = Math.Min(_stitches[i].YY, MinY);
				MaxY = Math.Max(_stitches[i].YY, MaxY);
			}

			_bounds = new MyRect(MinX, MinY, MaxX, MaxY);
		}
	}

	public class MyRect
	{
		Int32 _left, _right, _top, _bottom, _width, _height;

		public MyRect()
		{
			_left = _right = _top = _bottom = _width = _height = 0;
		}

		public MyRect(MyRect CopyFrom)
		{
			_left = CopyFrom.Left;
			_right = CopyFrom.Right;
			_bottom = CopyFrom.Bottom;
			_top = CopyFrom.Top;

			SetWidth();
			SetHeight();
		}

		public MyRect(Int32 left, Int32 top, Int32 right, Int32 bottom)
		{
			_left = left;
			_top = top;
			_bottom = bottom;
			_right = right;

			SetWidth();
			SetHeight();
		}

		public static MyRect operator *(MyRect MR1, float value)
		{
			MyRect Value = new MyRect(MR1);

			Value.Left = (int)(value * Value.Left);
			Value.Top = (int)(value * Value.Top);
			Value.Right = (int)(value * Value.Right);
			Value.Bottom = (int)(value * Value.Bottom);

			return Value;
		}

		public Int32 Left
		{
			get
			{
				return _left;
			}

			set
			{
				_left = value;
				SetWidth();
			}
		}

		public Int32 Top
		{
			get
			{
				return _top;
			}

			set
			{
				_top = value;
				SetHeight();
			}
		}

		public Int32 Right
		{
			get
			{
				return _right;
			}

			set
			{
				_right = value;
				SetWidth();
			}
		}

		public Int32 Bottom
		{
			get
			{
				return _bottom;
			}

			set
			{
				_bottom = value;
				SetHeight();
			}
		}

		public Int32 Height
		{
			get
			{
				return _height;
			}

			private set
			{
				_height = value;
			}
		}

		public Int32 Width
		{
			get
			{
				return _width;
			}

			private set
			{
				_width = value;
			}
		}

		public Point Center
		{
			get 
			{
				return new Point((Left + Right) / 2, (Top + Bottom) / 2);
			}
		}

		void SetWidth()
		{
			Width = Right - Left;
		}

		void SetHeight()
		{
			Height = Bottom - Top;
		}

		public void Offset(Point Value)
		{
			Left += Value.X;
			Right += Value.X;
			Top += Value.Y;
			Bottom += Value.Y;
		}

		public void Scale(int width, int height, int canvasWidth, int 
			canvasHeight, float ZoomLevel)
		{
			float widthToCanvas = (Width * (1/ZoomLevel)) / canvasWidth;
			float heightToCanvas = (Height * (1/ZoomLevel)) / canvasHeight;
			float ratio = Math.Min(widthToCanvas, heightToCanvas);

			int newWidth = (int)(width * ratio);
			int newHeight = (int)(height * ratio);

			Right = newWidth;
			Bottom = newHeight;
		}

		public void Rotate(float[] matrix)
		{
			Point[] RecPoints = { new Point(_left, _top), new Point(_right, _top), new Point(_left, _bottom), new Point(_right, _bottom) };
			Point[] RecPointsPrime = { new Point(), new Point(), new Point(), new Point() };
			Point TopLeftCorner = new Point(_left, _top);

			for(int i = 0; i < 4; i++)
			{
				RecPoints[i].X -= Center.X;
				RecPoints[i].Y -= Center.Y;

				RecPointsPrime[i].X = (RecPoints[i].X * (int)matrix[0] + (RecPoints[i].Y * (int)matrix[2]));
				RecPointsPrime[i].Y = (RecPoints[i].X * (int)matrix[1] + (RecPoints[i].Y * (int)matrix[3]));

				RecPointsPrime[i].X += Center.X;
				RecPointsPrime[i].Y += Center.Y;
			}

			_left = Math.Min(RecPointsPrime[0].X, Math.Min(RecPointsPrime[1].X, Math.Min(RecPointsPrime[2].X, RecPointsPrime[3].X)));
			_top = Math.Min(RecPointsPrime[0].Y, Math.Min(RecPointsPrime[1].Y, Math.Min(RecPointsPrime[2].Y, RecPointsPrime[3].Y)));
			_right = Math.Max(RecPointsPrime[0].X, Math.Max(RecPointsPrime[1].X, Math.Max(RecPointsPrime[2].X, RecPointsPrime[3].X)));
			_bottom = Math.Max(RecPointsPrime[0].Y, Math.Max(RecPointsPrime[1].Y, Math.Max(RecPointsPrime[2].Y, RecPointsPrime[3].Y)));

			if (_left < 0)
			{
				int temp = _left;
				_left += -temp;
				_right += -temp;
			}
			if (_top < 0)
			{
				int temp = _top;
				_top += -temp;
				_bottom += temp;
			}

			//I try to keep the top left corner in the same location after rotation
			/*if (TopLeftCorner.X < _left)
			{
				_right -= TopLeftCorner.X;
				_left = TopLeftCorner.X;
			}
			else if (TopLeftCorner.X > _left)
			{
				_right += TopLeftCorner.X;
				_left = TopLeftCorner.X;
			}
			if (TopLeftCorner.Y < _top)
			{ 
				_bottom -= TopLeftCorner.Y;
				_top = TopLeftCorner.Y;
			}
			else if (TopLeftCorner.Y > _top)
			{
				_bottom += TopLeftCorner.Y;
				_top = TopLeftCorner.Y;
			} *///not needed

			SetWidth();
			SetHeight();

			/*int left = _left - Center.X;
			int top = _top - Center.Y;
			int bottom = _bottom - Center.Y;
			int right = _right - Center.X;

			left = (left * (int)matrix[0]) + (left * (int)matrix[2]) + Center.X;
			top = (top * (int)matrix[1]) + (top * (int)matrix[3]) + Center.Y;
			bottom = (bottom * (int)matrix[1]) + (bottom * (int)matrix[3]) + Center.Y;
			right = (right * (int)matrix[0]) + (right * (int)matrix[2]) + Center.X;

			_left = Math.Min(left, right);
			_top = Math.Min(top, bottom);
			_bottom = Math.Max(top, bottom);
			_right = Math.Max(left, right);										   */
		}

		private double ToRadians(float degrees)
		{
			return (degrees * (Math.PI / 180.0f));
		}
	}

	public class ColorBlock
	{
		List<StitchBlock> _blocks = new List<StitchBlock>();
		MyRect _bounds;
		uint _count;
		short _colorIndex;
		Color _color;

		public ColorBlock()
		{

		}

		public ColorBlock(List<StitchBlock> BlocksOfSameColor)
		{
			_blocks = BlocksOfSameColor;
			UpdateBounds();
		}

		public short ColorIndex
		{
			get
			{
				short value = _colorIndex;
				return value;
			}
		}

		public Color Color
		{
			get
			{
				Color Value = _color;
				return Value;
			}
		}

		public uint Count
		{
			get
			{
				return _count;
			}
		}

		public uint ModBlockCount
		{
			get
			{
				uint value = 0;

				foreach (StitchBlock SB in _blocks)
				{
					if (SB.GetStitchTotalMinusFlags() > 0)
						value++;
				}

				return value;
			}
		}

		public void Add(StitchBlock BlockOfSameColor)
		{
			if (_blocks.Count == 0)
				_blocks.Add(BlockOfSameColor);
			else if (BlockOfSameColor.GetColorIndex() == _blocks
				[0].GetColorIndex())
				_blocks.Add(BlockOfSameColor);

			UpdateBounds();
			_count++;
			_colorIndex = BlockOfSameColor.GetColorIndex();
			_color = BlockOfSameColor.GetColor();
		}

		public void UpdateBounds()
		{
			foreach (StitchBlock SB in _blocks)
			{
				if (SB.GetStitchTotalMinusFlags() != 0)
				{
					_bounds = SB.GetBlockBoundingBox();
					break;
				}
			}
			
			foreach (StitchBlock SB in _blocks)
			{
				if (SB.GetStitchTotalMinusFlags() == 0)
					continue;

				MyRect TestBox = SB.GetBlockBoundingBox();

				_bounds.Left = Math.Min(_bounds.Left, TestBox.Left);
				_bounds.Top = Math.Min(_bounds.Top, TestBox.Top);
				_bounds.Right = Math.Max(_bounds.Right, TestBox.Right);
				_bounds.Bottom = Math.Max(_bounds.Bottom, TestBox.Bottom);

				if (_bounds.Width > 1800)
					_bounds.Left = _bounds.Left;
			}
		}

		public void BoundsUpdated()
		{ 
			foreach (StitchBlock SB in _blocks)
			{
				SB.UpdateBounds();
			}

			UpdateBounds();
		}

		public MyRect GetColorBounds()
		{
			if (_bounds != null)
				return new MyRect(_bounds);
			else
				return null;
		}

		public int[] GetMaxXStitchNumber()
		{
			int count = 0;
			int blockCount = 0;
			int MaxX = 0;
			int stitchNumber = -1;
			int blockNumber = -1;
			int[] debugInfo = new int[2];

			foreach (StitchBlock SB in _blocks)
			{
				foreach (Stitch S in SB.GetStitchList())
				{
					if (S.Flags != DesignFormat.NORMAL)
						continue;
					if (stitchNumber == -1)
					{
						MaxX = S.XX;
						stitchNumber = count;
						blockNumber = blockCount;
					}
					else
					{
						if (S.XX > MaxX)
						{
							MaxX = S.XX;
							stitchNumber = count;
							blockNumber = blockCount;
						}
					}
					count++;
				}

				blockCount++;
			}
			debugInfo[0] = blockNumber;
			debugInfo[1] = stitchNumber;

			return debugInfo;
		}

		public int[] GetMinXStitchNumber()
		{
			int count = 0;
			int blockCount = 0;
			int MinX = 0;
			int stitchNumber = -1;
			int blockNumber = -1;
			int[] debugInfo = new int[2];

			foreach (StitchBlock SB in _blocks)
			{
				foreach (Stitch S in SB.GetStitchList())
				{
					if (S.Flags != DesignFormat.NORMAL)
						continue;
					if (stitchNumber == -1)
					{
						MinX = S.XX;
						stitchNumber = count;
						blockNumber = blockCount;
					}
					else
					{
						if (S.XX < MinX)
						{
							MinX = S.XX;
							stitchNumber = count;
							blockNumber = blockCount;
						}
					}
					count++;
				}

				blockCount++;
			}
			debugInfo[0] = blockNumber;
			debugInfo[1] = stitchNumber;

			return debugInfo;
		}

		public short GetColorIndex()
		{
			return (short)_blocks[0].GetColorIndex();
		}

		public List<StitchBlock> GetStitchBlocks()
		{
			return _blocks;
		}

		public Stitch GetLastStitchInColorBlock()
		{
			int lastBlockID = _blocks.Count - 1;
			Stitch Value = new Stitch();
			Value = _blocks[lastBlockID].GetLastStitch();

			return Value;
		}

		public Stitch GetLastRealStitchInColorBlock()
		{
			int lastBlockID = _blocks.Count - 1;

			for (int i = lastBlockID; i >= 0; i--)
			{
				if (_blocks[i].GetStitchTotalMinusFlags() > 1)
				{
					lastBlockID = i;
					break;
				}
			}

			return _blocks[lastBlockID].GetLastRealStitch();
		}

		public Stitch GetFirstFlaglessStitch()
		{
			Stitch StitchValue = null;

			foreach (StitchBlock SB in _blocks)
			{

				foreach (Stitch S in SB.GetStitchList())
				{
					if (S.Flags == DesignFormat.NORMAL)
					{
						StitchValue = S;
						break;
					}
				}

				if (StitchValue != null)
					break;
			}

			return StitchValue;
		}

		public int GetFirstRealBlockNumber()
		{
			int BlockNumber = 0;
			int BlockCount = GetStitchBlocks().Count;

			foreach (StitchBlock SB in GetStitchBlocks())
			{
				if (SB.GetStitchTotalMinusFlags() == 0)
				{
					BlockNumber++;
					continue;
				}

				break;
			}

			if (BlockCount == BlockNumber)
				return -1;

			return BlockNumber;
		}

		public int GetLastRealBlockNumber()
		{
			int BlockNumber = _blocks.Count - 1;

			for (int i = BlockNumber; i > 0; i--)	
			{
				if (_blocks[i].GetStitchTotalMinusFlags() != 0)
				{
					BlockNumber = i;
					break;
				}
			}

			return BlockNumber;
		}
	}
}