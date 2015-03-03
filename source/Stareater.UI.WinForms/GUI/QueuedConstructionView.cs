﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Stareater.AppData;
using Stareater.Controllers.Views;
using Stareater.Utils.NumberFormatters;

namespace Stareater.GUI
{
	public partial class QueuedConstructionView : UserControl
	{
		private ConstructableItem data;
		
		public QueuedConstructionView()
		{
			InitializeComponent();
		}
		
		public ConstructableItem Data 
		{
			get
			{
				return data;
			}
			set
			{
				this.data = value;
				
				thumbnailImage.Image = ImageCache.Get[data.ImagePath];
				nameLabel.Text = data.Name;
				
				ThousandsFormatter costFormatter = new ThousandsFormatter(data.Cost);
				costLabel.Text = costFormatter.Format(data.Stockpile) + " / " + costFormatter.Format(data.Cost);
				
				ThousandsFormatter formatter = new ThousandsFormatter();
				investmentLabel.Text = "+" + formatter.Format(data.Investment);
			}
		}
		
		private void thumbnailImage_Click(object sender, EventArgs e)
		{
			this.InvokeOnClick(this, e);
		}
		
		private void nameLabel_Click(object sender, EventArgs e)
		{
			this.InvokeOnClick(this, e);
		}
		
		private void costLabel_Click(object sender, EventArgs e)
		{
			this.InvokeOnClick(this, e);
		}
		
		private void investmentLabel_Click(object sender, EventArgs e)
		{
			this.InvokeOnClick(this, e);
		}
	}
}
