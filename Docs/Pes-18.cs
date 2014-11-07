using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace PesFile
{
    public enum statusEnum { NotOpen, IOError, ParseError, Ready };
    public class stitchBlock
    {
        public Color color;
        public Int32 colorIndex;
        public Int32 stitchesTotal;
        public Point[] stitches;
        public stitchBlock()
        {
            color = System.Drawing.Color.Black;
        }
    }

    public struct intPair
    {
        public int a;
        public int b;
    }

    public class PesFile
    {
        System.IO.BinaryReader fileIn;
        int imageWidth;
        int imageHeight;
        string _filename;
        public List<Int64> pesHeader = new List<Int64>();
        public List<Int16> embOneHeader = new List<short>();
        public List<Int16> sewSegHeader = new List<short>();
        public List<Int16> embPunchHeader = new List<short>();
        public List<Int16> sewFigSegHeader = new List<short>();
        public List<stitchBlock> blocks = new List<stitchBlock>();
        public List<intPair> colorTable = new List<intPair>();
        private statusEnum readyStatus = statusEnum.NotOpen;
		List<Point> tempStitches = new List<Point>();
        Int64 startStitches = 0;
        string lastError = "";
        string pesNum = "";
        Point translateStart;
		int colorNum = -1;
		int prevX = 0;
		int prevY = 0;
		int maxX = 0;
		int maxY = 0;
		int minX = 0;
		int minY = 0;
		public List<short> colorList = new List<short>();
		
        //means we couldn't figure out some or all
        //of the colors, best guess will be used
        private bool colorWarning = false;
		private bool formatWarning = false;
		private bool classWarning = false;

        public PesFile(string filename)
        {
            OpenFile(filename);
        }

		private bool isSectionBreak(short testValue)
		{
			if (testValue == 0)
				return true;

			return false;
		}

		private bool isNewColor(short testValue)
		{
			if (testValue == -336) //FEB0
				return true; //does this ever come true?
			else if (testValue == -20226) //B0FE
				return true;

			return false;
		}

		private bool isEndData(short testValue)
		{
			if (testValue == -256) //FF00
				return true; //does this ever come true?
			else if (testValue == 255)
				return true;

			return false;
		}

		private bool isJumpStitch(short testValue)
		{
			if ((testValue & 0x8000) == 0x8000)
				return true;
			else if ((testValue & 0x0080) == 0x0080) //I don't think this is possible
				return true;

			return false;
		}

		private short checkBytes(byte[] testBytes)
		{
			short temp = testBytes[0];
			
			temp <<= 8;
			temp += testBytes[1];

			if (isEndData(temp))
				return 0;

			if (isNewColor(temp))
				return 1;

			//if (isSectionBreak(temp))
				//return 2;

			if (isJumpStitch(temp))
				return 3;

			return -1;
		}

		private void addBlockData(List<Point> stitchData, bool changeColor)
		{
			stitchBlock curBlock = new stitchBlock();
						
			curBlock.stitches = new Point[stitchData.Count];
			stitchData.CopyTo(curBlock.stitches);
			curBlock.stitchesTotal = stitchData.Count;

			if (colorList.Count > colorNum + 1)
				curBlock.colorIndex = colorList[colorNum + 1];
			
			curBlock.color = getColorFromIndex(curBlock.colorIndex);
			blocks.Add(curBlock);
			tempStitches = new List<Point>();

			if (changeColor)
				colorNum++;
		}

		private void addJumpStitch(List<Point> stitchData, byte[] values)
		{
			short[] delta = { 0, 0 };
			bool[] jump = {false, false};
			byte[] mulSign = { values[0], 0 };
			byte[] mag = { values[1], 0 };

			jump[0] = ((values[0] & 0x80) == 0x80);

			if (jump[0])
				mulSign[1] = fileIn.ReadByte();
			else
				mulSign[1] = values[1];

			jump[1] = ((mulSign[1] & 0x80) == 0x80);

			if (jump[1])
				mag[1] = fileIn.ReadByte();

			for (int i = 0; i < 2; i++)
			{
				if (jump[i])
				{
					if ((mulSign[i] & 0xf) <= 7)
					{//not sure this works yet
						delta[i] = (short)(mulSign[i] & 0xf);
						delta[i] *= (short)(0x100);
						delta[i] += mag[i];
					}
					else
					{//not sure this works yet either
						delta[i] = (short)((mulSign[i] & 0xf) - 0xf);
						delta[i] *= (short)(0x100);
						delta[i] += (short)(mag[i] - 0x100);
					}
				}
				else
					delta[i] = modStitchData(mulSign[i]);
			}

			stitchData.Add(new Point(prevX + delta[0], prevY + delta[1]));

			prevX += delta[0];
			prevY += delta[1];

			if (prevX > maxX)
				maxX = prevX;
			else if (prevX < minX)
				minX = prevX;

			if (prevY > maxY)
				maxY = prevY;
			else if (prevY < minY)
				minY = prevY;
		}

		private void addStitchData(List<Point> stitchData, byte[] values)
		{
			short[] delta = {0, 0};
            			                        
            for (int i = 0; i < values.Length; i++)
				delta[i] = modStitchData(values[i]);
			
			stitchData.Add(new Point(prevX + delta[0], prevY + delta[1]));
			
			prevX += delta[0];
            prevY += delta[1];
                    
			if (prevX > maxX)
				maxX = prevX;
			else if (prevX < minX)
                minX = prevX;
                    
			if (prevY > maxY)
				maxY = prevY;
            else if (prevY < minY)
                minY = prevY;
		}

		private short modStitchData(byte value)
		{
			short modValue = value; 

			if (value > 63)
				return (short) (modValue - 128);

			return modValue;
		}

        private void OpenFile(string filename)
        {
#if !DEBUG
            try
            {
#endif
            _filename = filename;
			fileIn = new System.IO.BinaryReader(System.IO.File.Open(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read));
			string startFileSig = "";
			
			for (int i = 0; i < 8; i++)//8 bytes
				startFileSig += fileIn.ReadChar();
                
            if (!startFileSig.StartsWith("#PES"))//this is not a file that we can read
            {
                readyStatus = statusEnum.ParseError;
                lastError = "Missing #PES at beginning of file";
                fileIn.Close();
                return;
            }
            
			int pecstart = fileIn.ReadInt32();
			fileIn.BaseStream.Position = pecstart + 48;
            int numColors = fileIn.ReadByte() +1;
                        
			for (int x = 0; x < numColors; x++)
				colorList.Add(fileIn.ReadByte());

            fileIn.BaseStream.Position = pecstart + 532; //was 532, but it doesn't read correctly
			
			while (true)
            {
                byte[] bytes = {0, 0};
                Int16 val = new Int16();
                val = fileIn.ReadInt16();
				bytes = BitConverter.GetBytes(val);
				short byteCheck = checkBytes(bytes); ///need to change everything to make it 16 bits instead of 32

				switch (byteCheck)
				{
					case -1: //normal stitch
						addStitchData(tempStitches, bytes);
						break;
					case 1: //new Color
						addBlockData(tempStitches, true);
						fileIn.ReadByte(); //misc byte
						break;
					case 2: //new section, deprecated
						addStitchData(tempStitches, bytes);
						addBlockData(tempStitches, false);
						break;
					case 3: //jump stitch
						
						if (tempStitches.Count >= 2)
							addBlockData(tempStitches, false);

						addJumpStitch(tempStitches, bytes);
						break;
					default: //end of data
						addBlockData(tempStitches, true);
						break;
				}

				//end of data
				if (byteCheck == 0)
					break;
            }
            
			imageWidth = maxX - minX;
            imageHeight = maxY - minY;
            translateStart.X = -minX;
            translateStart.Y = -minY;
            readyStatus = statusEnum.Ready;

#if !DEBUG
            }
            catch (System.IO.IOException ioex)
            {
                readyStatus = statusEnum.IOError;
                lastError = ioex.Message;
                if (fileIn != null)
                {
                    fileIn.Close();
                }
            }
            catch (Exception ex)
            {
                readyStatus = statusEnum.ParseError;
                lastError = ex.Message;
                if (fileIn != null)
                {
                    fileIn.Close();
                }
            }
#endif
        }

        void readCSewFigSeg(System.IO.BinaryReader file)
        {
            startStitches = fileIn.BaseStream.Position;

            bool doneWithStitches = false;
            int xValue = -100;
            int yValue = -100;
            stitchBlock currentBlock;
            int blockType; //if this is equal to newColorMarker, it's time to change color
            int colorIndex = 0;
            int remainingStitches;
            List<Point> stitchData;
            stitchData = new List<Point>();
            currentBlock = new stitchBlock();

            while (!doneWithStitches)
            {
                //reset variables
                xValue = 0;
                yValue = 0;

                blockType = file.ReadInt16();
                if (blockType == 16716)
                    break;
                colorIndex = file.ReadInt16();
                if (colorIndex == 16716)
                    break;
                remainingStitches = file.ReadInt16();
                if (remainingStitches == 16716)
                    break;
                while (remainingStitches >= 0)
                {
                    xValue = file.ReadInt16();
                    if (xValue == -32765)
                    {
                        break;//drop out before we start eating into the next section 
                    }
                    if (remainingStitches == 0)
                    {
                        int junk2 = 0;
                        junk2 = blocks.Count;

                        file.ReadBytes(24);
                        if (file.ReadInt16() == -1)
                            doneWithStitches = true;

                        currentBlock.stitches = new Point[stitchData.Count];
                        stitchData.CopyTo(currentBlock.stitches);
                        currentBlock.colorIndex = colorIndex;
                        currentBlock.color = getColorFromIndex(colorIndex);
                        currentBlock.stitchesTotal = stitchData.Count;
                        blocks.Add(currentBlock);
                        stitchData = new List<Point>();
                        currentBlock = new stitchBlock();

                        file.ReadBytes(48);

                        break;
                    }
                    else if (xValue == 16716 || xValue == 8224)
                    {
                        doneWithStitches = true;
                        break;
                    }
                    yValue = fileIn.ReadInt16();
                    if (yValue == 16716 || yValue == 8224)
                    {
                        doneWithStitches = true;
                        break;
                    }
                    stitchData.Add(new Point(xValue - translateStart.X, yValue + imageHeight - translateStart.Y));
                    remainingStitches--;
                }
            }
            if (stitchData.Count > 1)
            {
                currentBlock.stitches = new Point[stitchData.Count];
                stitchData.CopyTo(currentBlock.stitches);
                currentBlock.colorIndex = colorIndex;
                currentBlock.color = getColorFromIndex(colorIndex);
                currentBlock.stitchesTotal = stitchData.Count;
                blocks.Add(currentBlock);
            }
        }

        List<stitchBlock> filterStitches(List<stitchBlock> input, int threshold)
        {
            List<stitchBlock> retval = new List<stitchBlock>();
            List<Point> tempStitchData = new List<Point>();
            for (int x = 0; x < input.Count; x++)
            {

                for (int i = 0; i < input[x].stitches.Length; i++)
                {
                    if (i > 0)//need a previous point to check against, can't check the first
                    {
                        double diffx = Math.Abs(input[x].stitches[i].X - input[x].stitches[i - 1].X);
                        double diffy = Math.Abs(input[x].stitches[i].Y - input[x].stitches[i - 1].Y);
                        if (Math.Sqrt(Math.Pow(diffx, 2.0) + Math.Pow(diffy, 2.0)) < threshold) //check distance between this point and the last one
                        {
                            if (tempStitchData.Count == 0 && i > 1)//first stitch of block gets left out without this, except for very first stitch
                            {
                                tempStitchData.Add(input[x].stitches[i - 1]);
                            }
                            tempStitchData.Add(input[x].stitches[i]);
                        }
                        else//stitch is too far from the previous one
                        {
                            if (tempStitchData.Count > 2)//add the block and start a new one
                            {
                                stitchBlock tempBlock = new stitchBlock();
                                tempBlock.color = input[x].color;
                                tempBlock.colorIndex = input[x].colorIndex;
                                tempBlock.stitches = new Point[tempStitchData.Count];
                                tempStitchData.CopyTo(tempBlock.stitches);
                                retval.Add(tempBlock);
                                tempStitchData = new List<Point>();
                            }
                            else//reset variables
                            {
                                tempStitchData = new List<Point>();
                            }
                        }
                    }
                    else //just add the first one, don't have anything to compare against
                    {
                        tempStitchData.Add(input[x].stitches[i]);
                    }
                }
                if (tempStitchData.Count > 2)
                {
                    stitchBlock tempBlock = new stitchBlock();
                    tempBlock.color = input[x].color;
                    tempBlock.colorIndex = input[x].colorIndex;
                    tempBlock.stitches = new Point[tempStitchData.Count];
                    tempStitchData.CopyTo(tempBlock.stitches);
                    retval.Add(tempBlock);
                    tempStitchData = new List<Point>();
                }
            }
            return retval;
        }

        public int GetWidth()
        {
            return imageWidth;
        }

        public int GetHeight()
        {
            return imageHeight;
        }

        public string GetFileName()
        {
            if (_filename == null)
            {
                return "";
            }
            else
            {
                return _filename;
            }
        }

        public void saveDebugInfo()
        {
            System.IO.StreamWriter outfile = new System.IO.StreamWriter(System.IO.Path.ChangeExtension(_filename, ".txt"));
            outfile.Write(getDebugInfo());
            outfile.Close();
        }

        public string getDebugInfo()
        {
            System.IO.StringWriter outfile = new System.IO.StringWriter();
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
            return outfile.ToString();
        }

        public statusEnum getStatus()
        {
            return readyStatus;
        }

        public string getLastError()
        {
            return lastError;
        }

        public bool getColorWarning()
        {
            return colorWarning;
        }

        public bool getFormatWarning()
        {
            return formatWarning;
        }

        public bool getClassWarning()
        {
            return classWarning;
        }

        private Color getColorFromIndex(int index)
        {
            Color retval;// = Color.White;
            switch (index)
            {
                case 1:
                    retval = Color.FromArgb(14, 31, 124);
                    break;
                case 2:
                    retval = Color.FromArgb(10, 85, 163);
                    break;
                case 3:
                    retval = Color.FromArgb(48, 135, 119);
                    break;
                case 4:
                    retval = Color.FromArgb(75, 107, 175);
                    break;
                case 5:
                    retval = Color.FromArgb(237, 23, 31);
                    break;
                case 6:
                    retval = Color.FromArgb(209, 92, 0);
                    break;
                case 7:
                    retval = Color.FromArgb(145, 54, 151);
                    break;
                case 8:
                    retval = Color.FromArgb(228, 154, 203);
                    break;
                case 9:
                    retval = Color.FromArgb(145, 95, 172);
                    break;
                case 10:
                    retval = Color.FromArgb(157, 214, 125);
                    break;
                case 11:
                    retval = Color.FromArgb(232, 169, 0);
                    break;
                case 12:
                    retval = Color.FromArgb(254, 186, 53);
                    break;
                case 13:
                    retval = Color.FromArgb(255, 255, 0);
                    break;
                case 14:
                    retval = Color.FromArgb(112, 188, 31);
                    break;
                case 15:
                    retval = Color.FromArgb(186, 152, 0);
                    break;
                case 16:
                    retval = Color.FromArgb(168, 168, 168);
                    break;
                case 17:
                    retval = Color.FromArgb(123, 111, 0);
                    break;
                case 18:
                    retval = Color.FromArgb(255, 255, 179);
                    break;
                case 19:
                    retval = Color.FromArgb(79, 85, 86);
                    break;
                case 20:
                    retval = Color.FromArgb(0, 0, 0);
                    break;
                case 21:
                    retval = Color.FromArgb(11, 61, 145);
                    break;
                case 22:
                    retval = Color.FromArgb(119, 1, 118);
                    break;
                case 23:
                    retval = Color.FromArgb(41, 49, 51);
                    break;
                case 24:
                    retval = Color.FromArgb(42, 19, 1);
                    break;
                case 25:
                    retval = Color.FromArgb(246, 74, 138);
                    break;
                case 26:
                    retval = Color.FromArgb(178, 118, 36);
                    break;
                case 27:
                    retval = Color.FromArgb(252, 187, 196);
                    break;
                case 28:
                    retval = Color.FromArgb(254, 55, 15);
                    break;
                case 29:
                    retval = Color.FromArgb(240, 240, 240);
                    break;
                case 30:
                    retval = Color.FromArgb(106, 28, 138);
                    break;
                case 31:
                    retval = Color.FromArgb(168, 221, 196);
                    break;
                case 32:
                    retval = Color.FromArgb(37, 132, 187);
                    break;
                case 33:
                    retval = Color.FromArgb(254, 179, 67);
                    break;
                case 34:
                    retval = Color.FromArgb(255, 240, 141);
                    break;
                case 35:
                    retval = Color.FromArgb(208, 166, 96);
                    break;
                case 36:
                    retval = Color.FromArgb(209, 84, 0);
                    break;
                case 37:
                    retval = Color.FromArgb(102, 186, 73);
                    break;
                case 38:
                    retval = Color.FromArgb(19, 74, 70);
                    break;
                case 39:
                    retval = Color.FromArgb(135, 135, 135);
                    break;
                case 40:
                    retval = Color.FromArgb(216, 202, 198);
                    break;
                case 41:
                    retval = Color.FromArgb(67, 86, 7);
                    break;
                case 42:
                    retval = Color.FromArgb(254, 227, 197);
                    break;
                case 43:
                    retval = Color.FromArgb(249, 147, 188);
                    break;
                case 44:
                    retval = Color.FromArgb(0, 56, 34);
                    break;
                case 45:
                    retval = Color.FromArgb(178, 175, 212);
                    break;
                case 46:
                    retval = Color.FromArgb(104, 106, 176);
                    break;
                case 47:
                    retval = Color.FromArgb(239, 227, 185);
                    break;
                case 48:
                    retval = Color.FromArgb(247, 56, 102);
                    break;
                case 49:
                    retval = Color.FromArgb(181, 76, 100);
                    break;
                case 50:
                    retval = Color.FromArgb(19, 43, 26);
                    break;
                case 51:
                    retval = Color.FromArgb(199, 1, 85);
                    break;
                case 52:
                    retval = Color.FromArgb(254, 158, 50);
                    break;
                case 53:
                    retval = Color.FromArgb(168, 222, 235);
                    break;
                case 54:
                    retval = Color.FromArgb(0, 103, 26);
                    break;
                case 55:
                    retval = Color.FromArgb(78, 41, 144);
                    break;
                case 56:
                    retval = Color.FromArgb(47, 126, 32);
                    break;
                case 57:
                    retval = Color.FromArgb(253, 217, 222);
                    break;
                case 58:
                    retval = Color.FromArgb(255, 217, 17);
                    break;
                case 59:
                    retval = Color.FromArgb(9, 91, 166);
                    break;
                case 60:
                    retval = Color.FromArgb(240, 249, 112);
                    break;
                case 61:
                    retval = Color.FromArgb(227, 243, 91);
                    break;
                case 62:
                    retval = Color.FromArgb(255, 200, 100);
                    break;
                case 63:
                    retval = Color.FromArgb(255, 200, 150);
                    break;
                case 64:
                    retval = Color.FromArgb(255, 200, 200);
                    break;
                default:
                    retval = Color.White;
                    colorWarning = true;
                    break;
            }
            return retval;
        }

        public Bitmap designToBitmap(Single threadThickness, bool filterUglyStitches, int filterUglyStitchesThreshold)
        {
            Bitmap DrawArea;
            Graphics xGraph;

            DrawArea = new Bitmap(GetWidth() + (int)(threadThickness * 2), GetHeight() + (int)(threadThickness * 2));
            //panel1.Width = design.GetWidth() + (int)(threadThickness * 2);
            //panel1.Height = design.GetHeight() + (int)(threadThickness * 2);
            xGraph = Graphics.FromImage(DrawArea);
            xGraph.TranslateTransform(threadThickness+translateStart.X, threadThickness+translateStart.Y);
            //xGraph.FillRectangle(Brushes.White, 0, 0, DrawArea.Width, DrawArea.Height);
            List<stitchBlock> tmpblocks;
#if DEBUG
            tmpblocks = blocks;
#else
            if (filterUglyStitches && !formatWarning) //only filter stitches if we think we understand the format
            {
                tmpblocks = filterStitches(blocks, filterUglyStitchesThreshold);
            }
            else
            {
                tmpblocks = blocks;
            }
#endif
            for (int i = 0; i < tmpblocks.Count; i++)
            {
                if (tmpblocks[i].stitches.Length > 1)//must have 2 points to make a line
                {
                    Pen tempPen = new Pen(tmpblocks[i].color, threadThickness);
                    tempPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    tempPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    tempPen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
                    xGraph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    xGraph.DrawLines(tempPen, tmpblocks[i].stitches);
                }
            }
            xGraph.Dispose();
            return DrawArea;
        }
    }
}
