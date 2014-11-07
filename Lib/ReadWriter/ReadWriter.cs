using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using FileFormats

namespace FileFormats
{
	public class ReadWriter
	{
		private System.IO.BinaryReader fileIn; 
		private System.IO.BinaryReader fileOut;
		
		public ReadWriter(String filename)
		{
			String ending = "";

			if (filename.Length != 0)
			{

				ending = filename.TrimEnd('.');
				ending = ending.ToLower();

				if (ending == ".10o")
				{

				}
				else if (ending == ".pes")
				{
				}
			}

		}
	}
}
