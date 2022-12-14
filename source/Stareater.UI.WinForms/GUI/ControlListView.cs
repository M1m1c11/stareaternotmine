using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Stareater.GUI
{
	public class ControlListView : FlowLayoutPanel
	{
		public const int NoneSelected = -1;

		private int selectedIndex = NoneSelected;
		private Color lastBackColor;
		private Color lastForeColor;
		private Control lastSelected = null;
		private readonly HashSet<Control> unselectables = new HashSet<Control>();

		protected override void OnControlAdded(ControlEventArgs e)
		{
			base.OnControlAdded(e);
			e.Control.Click += onItemClick;
			
			checkSelectionIndex();
		}

		protected override void OnControlRemoved(ControlEventArgs e)
		{
			base.OnControlRemoved(e);
			e.Control.Click -= onItemClick;
			unselectables.Remove(e.Control);

			if (e.Control.Equals(lastSelected))
			{
				selectedIndex = Math.Min(selectedIndex, Controls.Count - 1);

				if (Controls.Count > 0 && !unselectables.Contains(Controls[selectedIndex]))
				{	
					select(selectedIndex);
					if (SelectedIndexChanged != null)
						SelectedIndexChanged(this, new EventArgs());
				}
				else
					selectedIndex = NoneSelected;
			}
			   
			checkSelectionIndex();
		}

		protected virtual void onItemClick(object sender, EventArgs e)
		{
			if (unselectables.Contains(sender as Control))
				return;
			
			if (selectedIndex != NoneSelected)
				deselect();

			var clickedControl = sender as Control;
			select(Controls.IndexOf(clickedControl));
			if (SelectedIndexChanged != null)
				SelectedIndexChanged(this, new EventArgs());
		}

		private void select(int controlIndex)
		{
			lastBackColor = Controls[controlIndex].BackColor;
			lastForeColor = Controls[controlIndex].ForeColor;
			lastSelected = Controls[controlIndex];
			Controls[controlIndex].BackColor = SystemColors.Highlight;
			Controls[controlIndex].ForeColor = SystemColors.HighlightText;
			selectedIndex = controlIndex;
		}

		private void deselect()
		{
			Controls[selectedIndex].BackColor = lastBackColor;
			Controls[selectedIndex].ForeColor = lastForeColor;
			selectedIndex = NoneSelected;
		}

		private void checkSelectionIndex()
		{
			if (selectedIndex == NoneSelected || lastSelected == null || Controls[selectedIndex].Equals(lastSelected))
				return;
			
			if (Controls.Count > 0)
			{
				selectedIndex = 0;
				while(!Controls[selectedIndex].Equals(lastSelected))
					selectedIndex++;
			}
			else
			{
				selectedIndex = NoneSelected;
				return;
			}
		}
		
		public event EventHandler SelectedIndexChanged;

		public bool HasSelection
		{
			get { return (selectedIndex != NoneSelected); }
		}

		public int SelectedIndex
		{
			get
			{
				return selectedIndex;
			}
			set
			{
				if (selectedIndex != NoneSelected)
					deselect();

				if (value != NoneSelected)
					select(value);
				else
					selectedIndex = NoneSelected;

				if (SelectedIndexChanged != null)
					SelectedIndexChanged(this, new EventArgs());
			}
		}

		public Control SelectedItem
		{
			get
			{
				return (selectedIndex != NoneSelected) ? Controls[selectedIndex] : null;
			}
		}
		
		public void Unselectable(Control control)
		{
			unselectables.Add(control);
		}
	}
}
