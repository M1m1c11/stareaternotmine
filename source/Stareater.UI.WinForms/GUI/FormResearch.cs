﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NGenerics.Extensions;
using Stareater.Controllers;
using Stareater.Controllers.Views;
using Stareater.Localization;
using Stareater.AppData;

namespace Stareater.GUI
{
	public partial class FormResearch : Form
	{
		private GameController controller;
		private IList<TechnologyTopic> topics;
		
		private Control lastTopic = null;
		
		public FormResearch()
		{
			InitializeComponent();
		}
		
		public FormResearch(GameController controller) : this()
		{
			this.controller = controller;
			this.topics = controller.ResearchTopics().ToList();
			
			updateList();
			
			if (topics.Count > 0)
				topicList.SelectedIndex = controller.ResearchFocus;
			
			updateDescription(topicList.SelectedItem);
				
			Context context = SettingsWinforms.Get.Language["FormTech"];
			this.Text = context["ResearchTitle"].Text();
		}
		
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == Keys.Escape) 
				this.Close();
			return base.ProcessCmdKey(ref msg, keyData);
		}
		
		private void updateList()
		{
			topicList.SuspendLayout();
			
			while (topicList.Controls.Count < topics.Count) {
				var topicControl = new TechnologyItem();
				topicControl.MouseEnter += topic_OnMouseEnter;
				topicList.Controls.Add(topicControl);
			}

			for (int i = 0; i < topics.Count; i++)
				(topicList.Controls[i] as TechnologyItem).SetData(topics[i]);
			
			topicList.ResumeLayout();
		}
		
		private void updateDescription(Control topic)
		{
			if (topic == null) {
				techImage.Image = null;
				techName.Text = "";
				techDescription.Text = "";
				techLevel.Text = "";
			} else if (lastTopic != topic) {
				var selection = topic as TechnologyItem;
				System.Diagnostics.Trace.WriteLine(selection.Data.Name);
				
				techImage.Image = ImageCache.Get[selection.Data.ImagePath];
				techName.Text = selection.Data.Name;
				techDescription.Text = selection.Data.Description;
				techLevel.Text = selection.TopicLevelText;
				lastTopic = topic;
			}
		}
		
		private void topic_OnMouseEnter(object sender, EventArgs e)
		{
			updateDescription(sender as Control);
		}
		
		private void topicList_MouseLeave(object sender, EventArgs e)
		{
			updateDescription(topicList.SelectedItem);
		}
		
		private void topicList_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.controller.ResearchFocus = topicList.SelectedIndex;
			this.topics = controller.ResearchTopics().ToList();
			
			updateList();
		}
	}
}
