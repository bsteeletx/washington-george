using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace EmbroideryDesign.DesignObjects
{
	public class Stitch
	{
		private Int16 flags;
		private Int16 xx;
		private Int16 yy;
		private Thread thread;

		public Int16 Flags
		{
			get
			{
				return flags;
			}
			set
			{
				flags = value;
			}
		}
		public Int16 XX
		{
			get
			{
				return xx;
			}
			set
			{
				xx = value;

			}
		}
		public Int16 YY
		{
			get
			{
				return yy;
			}
			set
			{
				yy = value;
			}
		}
		public Thread ThreadSelection
		{
			get
			{
				return thread;
			}
		}

		public Point getPoint()
		{
			return new Point(xx, yy);
		}

		public Stitch(Int16 flags, Int16 x, Int16 y, Thread thread)
		{
			this.flags = flags;
			xx = x;
			yy = y;
			this.thread = thread;
		}

		public Stitch()
		{
			flags = 0;
			xx = 0;
			yy = 0;
			thread = new Thread();
		}
	}

	public class Thread
	{
		public Color getColorFromHexStr(String HexDigts)
		{
			/* expect string beginning with 6 hex digits */
			Int32 rgbColor = 0;
			Color _Color = new Color();
    
			for(int i = 0; i < 6; i++)
			{
				if(HexDigts[i] >= '0' && HexDigts[i] <= '9')
				{
					rgbColor *= 16;
					rgbColor += HexDigts[i] - '0';
				}
        		else if(HexDigts[i] >= 'A' && HexDigts[i] <= 'F')
				{
					rgbColor *= 16;
					rgbColor += HexDigts[i] - 'A';
				}
				else if(HexDigts[i] >= 'a' && HexDigts[i] <= 'f')
				{
					rgbColor *= 16;
					rgbColor += HexDigts[i] - 'a';
				}
			}
			
			_Color = System.Drawing.Color.FromArgb(((rgbColor >> 16) & 0xFF), ((rgbColor >> 8) & 0xFF), (rgbColor & 0xFF));
			
			return _Color;
		}

		private Color threadColor;	
		private String description;
		private String catalogNumber;
		private Int32 threadColorIndex;

		public Color ThreadColor
		{
			get
			{
				return threadColor;
			}
		}

		public Int32 ThreadColorInIndex
		{
			get
			{
				return threadColorIndex;
			}
			set
			{
				threadColorIndex = value;
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
				
				Color c = currentThreadItem[i].threadColor;
				deltaRed = red - c.R;
				deltaBlue = green - c.G;
				deltaGreen = blue - c.B;
				dist = System.Math.Sqrt((Double)(deltaRed * deltaRed) + (deltaBlue * deltaBlue) + (deltaGreen * deltaGreen));
				
				if(dist <= currentClosestValue)
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
						
			threadColor = System.Drawing.Color.FromArgb(rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255));
			description = "random";
			catalogNumber = "";
			threadColorIndex = -1;
		}

		public Thread (Color threadColor, String description, String catalogNumber, Int32 colorIndex)
		{
			this.threadColor = threadColor;
			this.description = description;
			this.catalogNumber = catalogNumber;
			threadColorIndex = colorIndex;
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

	public class EmbArc	: EmbLine
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

	public class stitchBlock
	{
		public Color color;
		public Int32 colorIndex;
		public Int32 stitchesTotal;
		public Point[] stitchPoints;
		public Int16 flags;
		public Rectangle AbsoluteBounds = new Rectangle();
		public List<Stitch> stitches = new List<Stitch>();
		
		public stitchBlock()
		{
			color = System.Drawing.Color.Black;
		}

		public stitchBlock(List<Thread> ThreadLibrary, List<Stitch> Stitches, Int32 			ThreadIndex, Rectangle BoundingBox)
		{
			stitchesTotal = Stitches.Count;
			stitches = Stitches;
			stitchPoints = new Point[stitchesTotal];
			AbsoluteBounds = BoundingBox;

			for (int i = 0; i < stitchesTotal; i++)
			{
				stitchPoints[i] = Stitches[i].getPoint();
				flags = Stitches[i].Flags;
			}

			colorIndex = ThreadLibrary[ThreadIndex].ThreadColorInIndex;
			color = ThreadLibrary[ThreadIndex].ThreadColor;
		}
	}

	public class Pattern
	{
		/* Machine codes for stitch flags */
		public const Int16 NORMAL = 0; /* stitch to (xx, yy) */
		public const Int16 JUMP = 1; /* move to(xx, yy) */
		public const Int16 TRIM = 2; /* trim + move to(xx, yy) */
		public const Int16 STOP  = 4; /* pause machine for thread change */
		public const Int16 SEQUIN = 8; /* sequin */
		public const Int16 END = 16; /* end of program */

		private const Int16 LINETO = 0;
		private const Int16 MOVETO = 1;
		private const Int16 ARCTOMID = 2;
		private const Int16 ARCTOEND = 4;
		private const Int16 ELLIPSETORAD = 8;
		private const Int16 ELLIPSETOEND = 16;
		private const Int16 CUBICTOCONTROL1 = 32;
		private const Int16 CUBICTOCONTROL2 = 64;
		private const Int16 CUBICTOEND = 128;
		private const Int16 QUADTOCONTROL = 256;
		private const Int16 QUADTOEND = 512;

		private Int16 dstJumpsPerTrim = 6;

		public List<Stitch> stitchList = new List<Stitch>();
		public List<Thread> threadList = new List<Thread>();
		public List<EmbArc> arcList = new List<EmbArc>();
		public List<EmbCircle> circleList = new List<EmbCircle>();
		public List<EmbEllipse> ellipseList = new List<EmbEllipse>();
		public List<EmbLine> lineList = new List<EmbLine>();
		public List<EmbPoint> pointList = new List<EmbPoint>();
		public List<EmbPolygon> polygonList = new List<EmbPolygon>();
		public List<EmbPolyline> polylineList = new List<EmbPolyline>();
		public List<EmbRectangle> rectangleList = new List<EmbRectangle>();
		public List<EmbSpline> splineList = new List<EmbSpline>();
		public List<stitchBlock> blocks = new List<stitchBlock>();
		
		//might be too much info here, above can find last of everything
		//leaving it in for now in case it serves another purpose
		/*List<Stitch> lastStitch = new List<Stitch>();
		List<Thread> lastThread = new List<Thread>();
		List<Arc> lastArc = new List<Arc>();
		List<Circle> lastCircle = new List<Circle>();
		List<Ellipse> lastEllipse = new List<Ellipse>();
		List<Line> lastLine = new List<Line>();
		List<Point> lastPoint = new List<Point>();
		List<Polygon> lastPolygon = new List<Polygon>();
		List<Polyline> lastPolylines = new List<Polyline>();
		List<Rectangle> lastRectangle = new List<Rectangle>();
		List<Spline> lastSpline = new List<Spline>();*/

		//not sure why the above was a list, it makes more sense to be individual
		//values as below
		/*Stitch lastStitch = new Stitch();
		Thread lastThread = new Thread();
		EmbArc lastArc = new EmbArc();
		EmbCircle lastCircle = new EmbCircle();
		EmbEllipse lastEllipse = new EmbEllipse();
		EmbLine lastLine = new EmbLine();
		EmbPoint lastPoint = new EmbPoint();
		EmbPolygon lastPolygon = new EmbPolygon();
		EmbPolyline lastPolylines = new EmbPolyline();
		EmbRectangle lastRectangle = new EmbRectangle();
		EmbSpline lastSpline = new EmbSpline();*/

		stitchBlock curBlock = new stitchBlock();
		Int32 minX = 0;
		Int32 minY = 0;
		Int32 maxX = 0;
		Int32 maxY = 0;
		Int32 width = 0;
		Int32 height = 0;
		private Point StartStitch = new Point(0, 0);
		Int32 blockMinX = 0;
		Int32 blockMaxX = 0;
		Int32 blockMinY = 0;
		Int32 blockMaxY = 0;
		
		//Seems meaninglist...it's a list of no data?
		//EmbPathObjectList* pathObjList;

		public Int32 currentColorIndex;
		public Int16 lastX;
		public Int16 lastY;

		public Pattern()
		{
			currentColorIndex = 0;
		}

		public Rectangle getAbsBoundingBox()
		{
			Rectangle Box = new Rectangle();
			Boolean FirstTimeThrough = true;

			foreach (stitchBlock SB in blocks)
			{
				if (FirstTimeThrough)
				{
					Box = SB.AbsoluteBounds;
					FirstTimeThrough = false;
					continue;
				}
				
				if (SB.AbsoluteBounds.Left < Box.Left)
					Box.X = SB.AbsoluteBounds.Left;

				if (SB.AbsoluteBounds.Width > Box.Width)
					Box.Width = SB.AbsoluteBounds.Width;

				if (SB.AbsoluteBounds.Top < Box.Top)
					Box.Y = SB.AbsoluteBounds.Top;

				if (SB.AbsoluteBounds.Height > Box.Height)
					Box.Height = SB.AbsoluteBounds.Height;
			}

			return Box;
		}

		private void hideStitchesOverLength(Int32 length)
		{
			Double prevX = 0;
			Double prevY = 0;

			for (int i = 0; i < stitchList.Count; i++)
			{
				if((System.Math.Abs(stitchList[i].XX - prevX) > length) || (System.Math.Abs(stitchList[i].YY - prevY) > length))
				{
					stitchList[i].Flags |= TRIM;
					stitchList[i].Flags &= ~NORMAL;
				}
				prevX = stitchList[i].XX;
				prevY = stitchList[i].YY;
			}
		}

		public void fixColorCount()
		{
			Int32 maxColorIndex = 0;

			for (int i = 0; i < stitchList.Count; i++)
				maxColorIndex = System.Math.Max(maxColorIndex, stitchList[i].ThreadSelection.ThreadColorInIndex);

			while (threadList.Count <= maxColorIndex)
				threadList.Add(new Thread());

			//this wasn't in original code, but they had a TODO to put it in, is it needed?
			//threadList.RemoveAt(threadList.Count - 1);
		}

		//not sure if this is needed
		public void addThread(Thread thread)
		{
			threadList.Add(thread);
		}

		private void addStitchAbs(Int16 x, Int16 y, Int16 flags, Boolean 
			isAutoColorIndex)
		{
			Stitch S = new Stitch(flags, x, y, threadList[currentColorIndex]);
			
			if ((stitchList.Count == 0) && (flags == NORMAL))
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

			if ((flags & END) != 0) //do a search for 1 &, make sure it is != 0
			{
				Rectangle BlockBounds = new Rectangle(blockMinX, blockMinY, 
					blockMaxX - blockMinX, blockMaxY - blockMinY);
				curBlock = new stitchBlock(threadList, stitchList, 
					currentColorIndex, BlockBounds);
				blocks.Add(curBlock);
				stitchList = new List<Stitch>();
				return;
				//commented out in original
				//hideStitchesOverLength(127);
			}

			if (((flags & STOP) != 0) && (stitchList.Count == 0))
			{
				stitchList.Add(S);
				Rectangle BlockBounds = new Rectangle(blockMinX, blockMinY,
					blockMaxX - blockMinX, blockMaxY - blockMinY);
				curBlock = new stitchBlock(threadList, stitchList,
					currentColorIndex, BlockBounds);
				blocks.Add(curBlock);
				stitchList = new List<Stitch>();
				return;
			}

			if (((flags & STOP) != 0) && (isAutoColorIndex))
			{
				Rectangle BlockBounds = new Rectangle(blockMinX, blockMinY,
					blockMaxX - blockMinX, blockMaxY - blockMinY);
				curBlock = new stitchBlock(threadList, stitchList,
					currentColorIndex, BlockBounds);
				blocks.Add(curBlock);
				stitchList = new List<Stitch>();
				currentColorIndex++;
				blockMinY = S.YY;
				blockMinX = S.XX;
				blockMaxX = S.XX;
				blockMaxY = S.YY;
			}
			else if (flags != NORMAL)
			{
				Rectangle BlockBounds = new Rectangle(blockMinX, blockMinY,
					blockMaxX - blockMinX, blockMaxY - blockMinY);
				curBlock = new stitchBlock(threadList, stitchList,
					currentColorIndex, BlockBounds);
				blocks.Add(curBlock);
				stitchList = new List<Stitch>();
				blockMinY = S.YY;
				blockMinX = S.XX;
				blockMaxX = S.XX;
				blockMaxY = S.YY;
			}

			stitchList.Add(S);
			lastX = S.XX;
			lastY = S.YY;
		}

		public void addStitchRel(Int16 dx, Int16 dy, Int16 flags, Boolean 
			isAutoColorIncrement)
		{
			Int16 x = 0;
			Int16 y = 0;

			if (stitchList.Count != 0)
			{
				x = (Int16) (lastX + dx);
				y = (Int16) (lastY + dy);
			}
			else
			{
				lastX = (Int16)StartStitch.X;
				lastY = (Int16)StartStitch.Y;
				x = (Int16)(dx + lastX);
				y = (Int16)(dy + lastY);
			}

			minX = Math.Min((Int32)x, minX);
			minY = Math.Min((Int32)y, minY);
			maxX = Math.Max((Int32)x, maxX);
			maxY = Math.Max((Int32)y, maxY);

			addStitchAbs(x, y, flags, isAutoColorIncrement);
		}

		private void changeColor(Int32 index)
		{
			Rectangle BlockBounds = new Rectangle(blockMinX, blockMinY,
					blockMaxX - blockMinX, blockMaxY - blockMinY);
			curBlock = new stitchBlock(threadList, stitchList,
					currentColorIndex, BlockBounds);
			blocks.Add(curBlock);
			stitchList = new List<Stitch>();
			currentColorIndex = index;
		}

		//don't think I need this...memory management which I HOPE C# can handle
		//public void free

		//seems like there should be more to this
		private Pattern read(String filename)
		{
			System.IO.BinaryReader fileIn = new System.IO.BinaryReader(System.IO.File.Open(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read));
			Pattern P = new Pattern();

			return P;
		}

		//also seem sike there more here
		private void write(String filename)
		{
			//System.IO.BinaryWriter fileOut = new System.IO.BinaryWriter(System.IO.File.Open(filename, System.IO.FileMode.Open, System.IO.FileAccess.Write));

			//fileOut.Write(P.ToString());
		}
		
		//does not insert or delte stitches to preserve stitch density
		public void scale(Single scale)
		{
			for (int i = 0; i < stitchList.Count; i++)
			{
				stitchList[i].XX *= (Int16) scale;
				stitchList[i].YY *= (Int16) scale;
			}
		}

		//NOT WORKING YET, puting in a temp Min/Max X/Y values as part of Pattern
		//Will be updated on every added stitch
		public EmbRectangle calcRelBoundBox()
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

		public void setStartingPoint(Point Start)
		{
			StartStitch = Start;
			minX = StartStitch.X;
			maxX = StartStitch.X;
			minY = StartStitch.Y;
			maxY = StartStitch.Y;
		}

		public void flipVertical()
		{
			foreach (stitchBlock S in blocks)
			{
				for (int i = 0; i < S.stitchPoints.Length; i++)
					S.stitchPoints[i].Y = -S.stitchPoints[i].Y;
			}

		}

		//The original writing of this method didn't look like it would work
		//before using it, figure out what it's upposed to do
		//wer'e not thinking this is going to be useful, deprecating it...
		/*public void combineJumpStitches()
		{
			//created a new list of stitches that I think were supposed to 
			//duplicate the main list of stitches (stitchList)
			Int32 jumpCount = 0;
			List<Stitch> jumpListStart = new List<Stitch>();

			foreach (Stitch S in stitchList) //S = pointer
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
		public void correctForMaxStitchLength(Int16 maxStitchLength, Int32 maxJumpLength)
		{
			Int32 j = 0, splits; //no idea what splits means
			Int32 maxXy = 0;
			Int32 maxLen = 0;
			Int16 addX = 0;
			Int16 addY = 0;

			if (stitchList.Count > 1)
			{
				//created a stitchlist pointer, then assigned it to the next 
				//stitch in the original stitchlist
				Stitch Prev = stitchList[0];

				foreach (Stitch S in stitchList) //S = pointer
				{
					if (S == stitchList[0]) //don't want to do the first one
						continue;

					Int16 xx = Prev.XX;
					Int16 yy = Prev.YY;
					Int16 dx = (Int16) (S.XX - xx);
					Int16 dy = (Int16) (S.YY - yy);

					if ((Math.Abs(dx) > maxStitchLength) || (Math.Abs(dy) > maxStitchLength))
					{
						//declares a pointer to a stitchlist and assigns S
						//to the stitchlist, I don't think that's needed
						maxXy = Math.Max(Math.Abs(dx), Math.Abs(dy));

						if ((S.Flags & (JUMP | TRIM)) != 0)
							maxLen = maxJumpLength;
						else
							maxLen = maxStitchLength;

						splits = (Int16) (maxXy/maxLen);

						if (splits > 1)
						{
							Int32 flagsToUse = S.Flags;
							Int32 colorToUse = S.ThreadSelection.ThreadColorInIndex;
							addX = (Int16)(dx/splits);
							addY = (Int16)(dy/splits);

							for (j = 1; j < splits; j++)
							{
								Stitch St = new Stitch();
								St.XX = (Int16) (xx + addX * j);
								St.YY = (Int16) (yy + addY * j);
								St.Flags = (Int16) flagsToUse;
								St.ThreadSelection.ThreadColorInIndex = colorToUse;
								Prev = St;
							}
						}
					}

					Prev = S;
				}
			}

			if (stitchList[stitchList.Count - 1].Flags != END)
				stitchList.Add(new Stitch(END, stitchList[stitchList.Count - 1].XX, stitchList[stitchList.Count - 1].YY, new Thread()));
		}

		private void center()
		{
			//OA didn't think this actually worked
			Int32 moveLeft = 0;
			Int32 moveTop = 0;
			EmbRectangle BoundingRectangle = calcRelBoundBox();

			moveLeft = (Int32)(BoundingRectangle.Left - (BoundingRectangle.getWidth()/2.0));
			moveTop = (Int32)(BoundingRectangle.Top - (BoundingRectangle.getHeight()/2.0));

			foreach (Stitch S in stitchList)
			{
				S.XX -= (Int16) moveLeft;
				S.YY -= (Int16) moveTop;
			}
		}

		private void loadExternalColorFile(String filename)
		{
			//OA said there was a memory leak somewhere
			//again, doesn't seem like anything happens here

			Int32 DotPos = filename.IndexOf('.');
			String BaseFilename = filename.Substring(0, DotPos);
			System.IO.BinaryReader colorIn= new System.IO.BinaryReader(System.IO.File.Open(BaseFilename + ".edr", System.IO.FileMode.Open, System.IO.FileAccess.Read));
			
			//what now?
		}

		private void addCircleObject(Double cx, Double cy, Double r)
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
		}

		public Int32 GetWidth()
		{
			EmbRectangle BoundingBox = calcRelBoundBox();

			return BoundingBox.getWidth();
		}

		public Int32 GetHeight()
		{
			EmbRectangle BoundingBox = calcRelBoundBox();

			return BoundingBox.getHeight();
		}

		public String getDebugInfo()
		{
			/*System.IO.StringWriter outfile = new System.IO.StringWriter();
			string name = "";
			outfile.WriteLine("PES header");
			outfile.WriteLine("PES number:\t" + pesNum);
			for (int i = 0; i < pesHeader.Count; i++)
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

		public Bitmap patternToBitmap(Single threadThickness)
        {
            Bitmap DrawArea;
            Graphics xGraph;
			Int32 bitmapWidth = GetWidth() + (Int32)(threadThickness * 2);
			Int32 bitmapHeight = GetHeight() + (Int32)(threadThickness * 2);
			
			DrawArea = new Bitmap(bitmapWidth, bitmapHeight);
            xGraph = Graphics.FromImage(DrawArea);
            xGraph.TranslateTransform(threadThickness-minX, threadThickness-minY);
        
            //List<stitchBlock> tmpblocks;
            //tmpblocks = blocks;
			//Need to figure out how to do blocks/Color changes
			for (int i = 0; i < blocks.Count; i++)
			{
				if ((i != 0) && (blocks[i].stitchesTotal > 1))
				{
					Int32 EndPoint = blocks[i - 1].stitchesTotal - 1;
					Point lastPoint = new Point(blocks[i - 1].stitchPoints
						[EndPoint].X, blocks[i - 1].stitchPoints[EndPoint].Y);
					
					Point thisPoint = blocks[i].stitchPoints[0]; //was i - 1

					Point[] line = new Point[2]{lastPoint, thisPoint};
					Single[] dashValues = { 5, 5, 5, 5 };
					Pen tempPen = new Pen(Color.Black, 1.0f);
					tempPen.DashPattern = dashValues;
					tempPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
					tempPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
					tempPen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
					xGraph.SmoothingMode = 
						System.Drawing.Drawing2D.SmoothingMode.HighQuality;
					xGraph.DrawLines(tempPen, line);
				}

				if (blocks[i].stitchesTotal > 1)
				{
					Pen tempPen = new Pen(blocks[i].color, threadThickness); 
						//tmpblocks[i].color, threadThickness);
                    
					tempPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    tempPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    tempPen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
                    xGraph.SmoothingMode = 
						System.Drawing.Drawing2D.SmoothingMode.HighQuality;
					xGraph.DrawLines(tempPen, blocks[i].stitchPoints);
                }
            }
            xGraph.Dispose();
            return DrawArea;
        }
    }
}