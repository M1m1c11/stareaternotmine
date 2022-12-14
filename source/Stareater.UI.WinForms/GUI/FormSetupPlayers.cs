using System;
using System.Drawing;
using System.Windows.Forms;
using Stareater.Controllers;
using Stareater.AppData;
using Stareater.GuiUtils;
using Stareater.Localization;
using Stareater.Players;
using Stareater.Controllers.Views;

namespace Stareater.GUI
{
	public partial class FormSetupPlayers : Form
	{
		private NewGameController controller;
		private bool eventClutch = false;

		public FormSetupPlayers()
		{
			InitializeComponent();
			
			setLanguage();
		}

		public FormSetupPlayers(NewGameController controller) : this()
		{
			this.controller = controller;

			updatePlayerViews();

			foreach(var playerType in controller.PlayerTypes)
				controllerPicker.Items.Add(new Tag<PlayerType>(playerType, playerType.Name));

			organizationPicker.Items.Add(new Tag<OrganizationInfo>(null, LocalizationManifest.Get.CurrentLanguage["General"]["RandomOrganization"].Text()));
			foreach (var org in PlayerAssets.Organizations)
				organizationPicker.Items.Add(new Tag<OrganizationInfo>(org, org.Name));

			foreach (var color in PlayerAssets.Colors) {
				var colorItem = new ColorItem();
				colorItem.Color = color;
				colorsLayout.Controls.Add(colorItem);
			}

			playerViewsLayout.SelectedIndex = 0;
		}

		private void setLanguage()
		{
			Context context = LocalizationManifest.Get.CurrentLanguage["FormSetupPlayers"];

			this.Font = SettingsWinforms.Get.FormFont;
			this.Text = context["FormTitle"].Text();

			controllerLabel.Text = context["controllerLabel"].Text() + ":";
			nameLabel.Text = context["nameLabel"].Text() + ":";
			organizationLabel.Text = context["organizationLabel"].Text() + ":";

			addButton.Text = context["addButton"].Text();
			removeButton.Text = context["removeButton"].Text();
			acceptButton.Text = context["acceptButton"].Text();
		}

		private void selectColor(Color color)
		{
			for(int i=0; i < colorsLayout.Controls.Count;i++)
				if ((colorsLayout.Controls[i] as ColorItem).Color == color) {
					colorsLayout.SelectedIndex = i;
					break;
				}
		}

		private void updatePlayerViews()
		{
			var players = controller.PlayerList;

			while (playerViewsLayout.Controls.Count < players.Count)
				playerViewsLayout.Controls.Add(new NewGamePlayerView());
			while (playerViewsLayout.Controls.Count > players.Count)
				playerViewsLayout.Controls.RemoveAt(playerViewsLayout.Controls.Count - 1);

			for (int i = 0; i < players.Count; i++)
				(playerViewsLayout.Controls[i] as NewGamePlayerView).SetData(players[i]);
		}

		private NewGamePlayerView selectedPlayerView
		{
			get
			{
				return playerViewsLayout.SelectedItem as NewGamePlayerView;
			}
		}

		private void playerViewsLayout_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (!playerViewsLayout.HasSelection) 
				return;

			NewGamePlayerInfo playerInfo = controller.PlayerList[playerViewsLayout.SelectedIndex];
			eventClutch = true;

			nameInput.Text = playerInfo.Name;
			controllerPicker.SelectedItem = new Tag<PlayerType>(playerInfo.ControlType, null);

			if (playerInfo.Organization != null)
				organizationPicker.SelectedItem = new Tag<OrganizationInfo>(playerInfo.Organization, null);
			else
				organizationPicker.SelectedIndex = 0;

			selectColor(playerInfo.Color);

			eventClutch = false;
		}

		private void controllerPicker_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (eventClutch || controllerPicker.SelectedItem == null || !playerViewsLayout.HasSelection)
				return;

			controller.UpdatePlayer(playerViewsLayout.SelectedIndex, (controllerPicker.SelectedItem as Tag<PlayerType>).Value);
		}

		private void nameInput_TextChanged(object sender, EventArgs e)
		{
			if (eventClutch || !playerViewsLayout.HasSelection)
				return;

			controller.UpdatePlayer(playerViewsLayout.SelectedIndex, nameInput.Text);
			selectedPlayerView.SetData(controller.PlayerList[playerViewsLayout.SelectedIndex]);
		}

		private void organizationPicker_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (organizationPicker.SelectedItem == null || !playerViewsLayout.HasSelection)
				return;

			if (!eventClutch) {
				controller.UpdatePlayer(playerViewsLayout.SelectedIndex, (organizationPicker.SelectedItem as Tag<OrganizationInfo>).Value);
				selectedPlayerView.SetData(controller.PlayerList[playerViewsLayout.SelectedIndex]);
			}
			
			var org = controller.PlayerList[playerViewsLayout.SelectedIndex].Organization;
			organizationDescription.Text = (org != null) ? org.Description : "";
		}

		private void colorsLayout_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (eventClutch || !playerViewsLayout.HasSelection)
				return;

			int playerIndex = playerViewsLayout.SelectedIndex;
			Color newColor = (colorsLayout.SelectedItem as ColorItem).Color;
			controller.UpdatePlayer(playerIndex, newColor);

			if (controller.PlayerList[playerIndex].Color == newColor)
				selectedPlayerView.SetData(controller.PlayerList[playerIndex]);
			else {
				eventClutch = true;
				selectColor(controller.PlayerList[playerIndex].Color);
				eventClutch = false;
			}
		}

		private void addButton_Click(object sender, EventArgs e)
		{
			controller.AddPlayer();
			updatePlayerViews();
		}

		private void removeButton_Click(object sender, EventArgs e)
		{
			if (!playerViewsLayout.HasSelection)
				return;

			controller.RemovePlayer(playerViewsLayout.SelectedIndex);
			updatePlayerViews();
		}

		private void acceptButton_Click(object sender, EventArgs e)
		{
			this.DialogResult = System.Windows.Forms.DialogResult.OK;
		}
	}
}
