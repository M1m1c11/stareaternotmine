﻿/*
 * Created by SharpDevelop.
 * User: ekraiva
 * Date: 13.6.2014.
 * Time: 13:29
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace Stareater.GUI
{
	partial class SavedGame
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the control.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.gameName = new System.Windows.Forms.Label();
			this.turnText = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.Location = new System.Drawing.Point(3, 3);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(69, 69);
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// gameName
			// 
			this.gameName.AutoSize = true;
			this.gameName.Location = new System.Drawing.Point(78, 3);
			this.gameName.Name = "gameName";
			this.gameName.Size = new System.Drawing.Size(88, 13);
			this.gameName.TabIndex = 8;
			this.gameName.Text = "Game name here";
			// 
			// turnText
			// 
			this.turnText.AutoSize = true;
			this.turnText.Location = new System.Drawing.Point(78, 16);
			this.turnText.Name = "turnText";
			this.turnText.Size = new System.Drawing.Size(50, 13);
			this.turnText.TabIndex = 7;
			this.turnText.Text = "Turn 251";
			// 
			// SavedGame
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.gameName);
			this.Controls.Add(this.turnText);
			this.Controls.Add(this.pictureBox1);
			this.Name = "SavedGame";
			this.Size = new System.Drawing.Size(300, 75);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private System.Windows.Forms.Label turnText;
		private System.Windows.Forms.Label gameName;
		private System.Windows.Forms.PictureBox pictureBox1;
	}
}
