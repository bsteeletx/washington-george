using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Media;


namespace PrEmbroiderMe
{
	public partial class MainForm : Form
	{
		UILogic UserLogic = new UILogic();
		List<MoveableImage> Designs = new List<MoveableImage>();
		
		public MainForm()
		{
			InitializeComponent();
		}

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			List<Bitmap> AllBitmaps = new List<Bitmap>();

			if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				foreach (String Filename in openFileDialog1.FileNames)
				{
					if (!System.IO.File.Exists(Filename))
						return;
				}
			
				UserLogic.UserSettings.LastOpenDir = System.IO.Path.GetDirectoryName(openFileDialog1.FileNames[0]);
				UserLogic.openFiles(openFileDialog1.FileNames);

				AllBitmaps = UserLogic.GetImagesFromDesigns();
				Int32 XOffset = 25;

				foreach (Bitmap B in AllBitmaps)
				{
					MoveableImage Temp = new MoveableImage();

					
					Temp.Image = B;
					Temp.Size = new Size(B.Width, B.Height);
					Temp.Visible = true;
					Temp.Show();
					Temp.Location = new Point(XOffset, 0);
					Temp.Enabled = true;
					Temp.BackColor = Color.Transparent;
					
					Temp.BringToFront();
					panelEditingArea.
						.Controls.Add(Temp);
					Designs.Add(Temp);
					XOffset += 25;
				}
			}
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MessageBox.Show("PrEmbroiderMe! version " + UserLogic.currentVersion() + ". This program opens multiple design files and saves them as one");
		}

		private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				if (UserLogic.UserSettings.LastSavedDir != null)
					saveFileDialog1.InitialDirectory = UserLogic.UserSettings.LastSavedDir;
				else
					saveFileDialog1.InitialDirectory = UserLogic.UserSettings.LastOpenDir;

				UserLogic.SavePattern(saveFileDialog1.FileName);
			}
		}

		private void panelEditingArea_Paint(object sender, PaintEventArgs e)
		{
			
		}
	}

	class MoveableImage : System.Drawing.Image
	{
		Point MouseDownLocation;

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			MouseDownLocation = e.Location;
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (e.Button == MouseButtons.Left)
			{
				this.Left += e.X - MouseDownLocation.X;
				this.Top += e.Y - MouseDownLocation.Y;
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			if (MouseDownLocation.X != 0 && MouseDownLocation.Y != 0)
				MouseDownLocation = new Point();
		}
	}
}
