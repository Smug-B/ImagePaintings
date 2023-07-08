using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.GameContent.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ImagePaintings.Core.UI;
using ImagePaintings.Core.UI.Elements;
using Terraria.UI;
using Terraria.ModLoader;
using ImagePaintings.Content.Items;
using Microsoft.Xna.Framework.Input;
using Terraria.GameContent;

namespace ImagePaintings.Content.UI
{
    public partial class GeneratePaintingState : AutoUIState
	{
		public static bool Visible { get; set; }

		public UIPanel MasterBackground { get; private set; }

		public UITextbox URLTextbox { get; private set; }

		public UITextbox WidthTextbox { get; private set; }

		public UITextbox HeightTextbox { get; private set; }

		public UITextbox ResolutionWidthTextbox { get; private set; }

		public UITextbox ResolutionHeightTextbox { get; private set; }

		public UITextbox FramerateTextbox { get; private set; }

		public UITextbox BrightnessTextbox { get; private set; }

		public UITextbox LayeringButton { get; private set; }

		public UITextbox OutputTextbox { get; private set; }

		public UITextbox GenerateButton { get; private set; }

		public UITextbox ResetButton { get; private set; }

		public string Information => "What's New?"
			+ "\n---------------------"
			+ "\nThe newest version of Image Paintings adds in options for configuring draw layer when utilizing Alternative Draw. "
			+ "To make room for this new configuration, the option to generate Legacy Paintings has been removed."
			+ "NOTICE: Above Tile paintings WILL look weird unless you configure a value for brightness. "
			+ "\n \nNew paintings are no longer consumable! As a result, placed paintings will not be dropping their items upon destruction. "
			+ "\n \nThe maximum size of paintings has also been increased from 50x50 blocks to 256x256 blocks! "
			+ "(Why you would need that is beyond me)"
			+ "\n \nAn hotkey to automatically bring up this UI is added, check your keybinds!"
			+ "\n---------------------"
			+ "\nPainting Placement Origin: "
			+ "\nYou can stop placing paintings from the top left corner! "
			+ "Simply head to your keybinds and bind your 'Configure Place Origin' key to something. "
			+ "While holding that button, as well as a painting, you can press your arrow keys to change the place origin! "
			+ "For users without a 75+% keyboard, head to your Image Painting Configs and look for a new toggle named 'Right Click Origin Configuration'..."
			+ "\n---------------------"
			+ "\nURL: The URL of the painting. Verify that your link ends with a compatible extension ( .png, .jpeg, .jpg, or .gif )."
			+ "\n \nWidth: The width of the placed painting in blocks."
			+ "\n \nHeight: The height of the placed painting in blocks."
			+ "\n \nResolution: By default, the resolution of the produced image is set according to the width and height of the painting. "
			+ "As a result, in most circumstances, there would be no need to adjust the resolution width and height settings. "
			+ "However, as the image produced by the paintings are saved in memory, if one wishes to reduce their memory consumption"
			+ "they should consider reducing the resolution width and height respectively."
			+ "\n \nFrame Duration: This setting is ignored on non GIF images but defaults to a value of 5. "
			+ "Determines the duration of each frame in ticks, usually 1 / 60th of a second."
			+ "\n \nBrightness: This value, if above 0, allows paintings to be drawn with a uniform brightness thereby disregarding environment lighting. "
			+ "\n \nLayering Options: Configures how the painting should be placed, whether behind walls, above walls, or above tiles. This only works with Alternative Draw enabled!";

		public override void PreLoad(ref string name)
		{
			AutoSetState = false;
			AutoAddHandler = true;
		}

		public override UIHandler Load() => new UIHandler(UserInterface, "Vanilla: Inventory", LayerName);

		public override void OnInitialize()
		{
			MasterBackground = new UIPanel();
			MasterBackground.Width.Pixels = 800;
			MasterBackground.Height.Pixels = 650;
			MasterBackground.HAlign = 0.5f;
			MasterBackground.VAlign = 0.5f;
			MasterBackground.BackgroundColor = new Color(33, 43, 79) * 0.8f;
			MasterBackground.SetPadding(0f);

			UIImageButton closeButton = new UIImageButton(Main.Assets.Request<Texture2D>("Images/UI/SearchCancel"));
			closeButton.Left.Pixels = -10;
			closeButton.Top.Pixels = 10;
			closeButton.HAlign = 1f;
			closeButton.OnLeftClick += (evt, listeningElement) =>
			{
				SoundEngine.PlaySound(SoundID.MenuClose);
				ModContent.GetInstance<GeneratePaintingState>().UserInterface.SetState(null);
			};
			MasterBackground.Append(closeButton);

			UIText title = new UIText("Image Painting Creator", 0.8f, true);
			title.HAlign = 0.5f;
			title.Top.Pixels = 16;
			MasterBackground.Append(title);

			UISeparator titleSeparator = new UISeparator(Color.Black);
			titleSeparator.Left.Pixels = 10;
			titleSeparator.Top.Pixels = 60;
			titleSeparator.Width.Pixels = MasterBackground.Width.Pixels - 20;
			titleSeparator.Height.Pixels = 2;
			MasterBackground.Append(titleSeparator);

			void createTextboxTitle(UITextbox parentTextbox, string text)
			{
				UIText title = new UIText(text);
				title.Left.Pixels = 8f;
				title.Top.Pixels = -parentTextbox.Height.Pixels - 10;
				title.VAlign = 1f;
				parentTextbox.Append(title);
			}

			URLTextbox = new UITextbox(defaultText: "Type ( enjoy misery? ) or paste an URL here!");
			URLTextbox.Left.Pixels = 10;
			URLTextbox.Top.Pixels = 96;
			URLTextbox.Width.Pixels = 360f;
			URLTextbox.Height.Pixels = 400f;
			URLTextbox.OnMouseOver += HandleHoverMouseOver;
			URLTextbox.OnMouseOut += HandleHoverMouseOut;
			MasterBackground.Append(URLTextbox);
			createTextboxTitle(URLTextbox, "URL ( Required )");

			void handleForcedNumericalValue(UITextbox uiTextbox, string uiTextboxName, int min, int max)
            {
				uiTextbox.OnUnfocus += uiElement =>
				{
					if (uiElement is UITextbox uiTextbox)
					{
						if (int.TryParse(uiTextbox.CurrentText, out int result))
						{
							if (result < min || result > max)
							{
								uiTextbox.ForceUpdateText(Utils.Clamp(result, min, max).ToString());
								OutputTextbox.UIScrollbar.ViewPosition = 0;
								OutputTextbox.ForceUpdateText(uiTextboxName + ", if required, must be an integer value between " + min + " and " + max + ". "
								+ "Otherwise, " + uiTextboxName + " can also be no value for its default implementation.\n \n" + Information);
							}
						}
						else
						{
							uiTextbox.ForceUpdateText(string.Empty);
							OutputTextbox.UIScrollbar.ViewPosition = 0;
							OutputTextbox.ForceUpdateText(uiTextboxName + ", if required, must be an integer value between " + min + " and " + max + ". "
								+ "Otherwise, " + uiTextboxName + " can also be no value for its default implementation.\n \n" + Information);
						}
					}
				};
			}

			float singleLineTextboxHeight = FontAssets.MouseText.Value.MeasureString("C").Y + 10;
			WidthTextbox = new UITextbox(defaultText: "Width", includeScrollbar: false);
			WidthTextbox.Left.Pixels = 10;
			WidthTextbox.Top.Pixels = 530;
			WidthTextbox.Width.Pixels = 175;
			WidthTextbox.Height.Pixels = singleLineTextboxHeight;
			WidthTextbox.OnMouseOver += HandleHoverMouseOver;
			WidthTextbox.OnMouseOut += HandleHoverMouseOut;
			handleForcedNumericalValue(WidthTextbox, "Width", 1, 256);
			createTextboxTitle(WidthTextbox, "Dimensions in Blocks ( Required )");
			MasterBackground.Append(WidthTextbox);

			HeightTextbox = new UITextbox(defaultText: "Height", includeScrollbar: false);
			HeightTextbox.Left.Pixels = 195;
			HeightTextbox.Top.Pixels = 530;
			HeightTextbox.Width.Pixels = 175;
			HeightTextbox.Height.Pixels = singleLineTextboxHeight;
			HeightTextbox.OnMouseOver += HandleHoverMouseOver;
			HeightTextbox.OnMouseOut += HandleHoverMouseOut;
			handleForcedNumericalValue(HeightTextbox, "Height", 1, 256);
			MasterBackground.Append(HeightTextbox);

			ResolutionWidthTextbox = new UITextbox(defaultText: "Width", includeScrollbar: false);
			ResolutionWidthTextbox.Left.Pixels = 10;
			ResolutionWidthTextbox.Top.Pixels = 564 + singleLineTextboxHeight;
			ResolutionWidthTextbox.Width.Pixels = 175;
			ResolutionWidthTextbox.Height.Pixels = singleLineTextboxHeight;
			ResolutionWidthTextbox.OnMouseOver += HandleHoverMouseOver;
			ResolutionWidthTextbox.OnMouseOut += HandleHoverMouseOut;
			handleForcedNumericalValue(ResolutionWidthTextbox, "Resolution Width", 1, 2048);
			createTextboxTitle(ResolutionWidthTextbox, "Resolution in Pixels ( Recommended )");
			MasterBackground.Append(ResolutionWidthTextbox);

			ResolutionHeightTextbox = new UITextbox(defaultText: "Height", includeScrollbar: false);
			ResolutionHeightTextbox.Left.Pixels = 195;
			ResolutionHeightTextbox.Top.Pixels = 564 + singleLineTextboxHeight;
			ResolutionHeightTextbox.Width.Pixels = 175;
			ResolutionHeightTextbox.Height.Pixels = singleLineTextboxHeight;
			ResolutionHeightTextbox.OnMouseOver += HandleHoverMouseOver;
			ResolutionHeightTextbox.OnMouseOut += HandleHoverMouseOut;
			handleForcedNumericalValue(ResolutionHeightTextbox, "Resolution Height", 1, 2048);
			MasterBackground.Append(ResolutionHeightTextbox);

			FramerateTextbox = new UITextbox(defaultText: "Default", includeScrollbar: false);
			FramerateTextbox.Left.Pixels = 380;
			FramerateTextbox.Top.Pixels = 96;
			FramerateTextbox.Width.Pixels = 240f;
			FramerateTextbox.Height.Pixels = singleLineTextboxHeight;
			FramerateTextbox.OnMouseOver += HandleHoverMouseOver;
			FramerateTextbox.OnMouseOut += HandleHoverMouseOut;
			handleForcedNumericalValue(FramerateTextbox, "Frame Duration", 1, 100);
			createTextboxTitle(FramerateTextbox, "Frame Duration ( GIFs )");
			MasterBackground.Append(FramerateTextbox);

			BrightnessTextbox = new UITextbox(defaultText: "Default", includeScrollbar: false);
			BrightnessTextbox.Left.Pixels = 380;
			BrightnessTextbox.Top.Pixels = 130 + singleLineTextboxHeight;
			BrightnessTextbox.Width.Pixels = 240f;
			BrightnessTextbox.Height.Pixels = singleLineTextboxHeight;
			BrightnessTextbox.OnMouseOver += HandleHoverMouseOver;
			BrightnessTextbox.OnMouseOut += HandleHoverMouseOut;
			handleForcedNumericalValue(BrightnessTextbox, "Brightness", 0, 100);
			createTextboxTitle(BrightnessTextbox, "Brightness");
			MasterBackground.Append(BrightnessTextbox);

			LayeringButton = new UITextbox(includeScrollbar: false, editable: false);
			LayeringButton.Left.Pixels = 380;
			LayeringButton.Top.Pixels = 202 + singleLineTextboxHeight;
			LayeringButton.Width.Pixels = 240f;
			LayeringButton.Height.Pixels = singleLineTextboxHeight;
			LayeringButton.ForceUpdateText("Above Walls");
			LayeringButton.OnMouseOver += HandleHoverMouseOver;
			LayeringButton.OnMouseOut += HandleHoverMouseOut;
			LayeringButton.OnLeftClick += ToggleLayering;
			createTextboxTitle(LayeringButton, "Layering Options");
			MasterBackground.Append(LayeringButton);

			UISeparator buttonSeparator = new UISeparator(Color.Black);
			buttonSeparator.Left.Pixels = 630;
			buttonSeparator.Top.Pixels = 60;
			buttonSeparator.Width.Pixels = 2f;
			buttonSeparator.Height.Pixels = 246;
			MasterBackground.Append(buttonSeparator);

			UISeparator outPutBoxSeparator = new UISeparator(Color.Black);
			outPutBoxSeparator.Left.Pixels = 380;
			outPutBoxSeparator.Top.Pixels = 306;
			outPutBoxSeparator.Width.Pixels = 410f;
			outPutBoxSeparator.Height.Pixels = 2;
			MasterBackground.Append(outPutBoxSeparator);

			MasterBackground.Height.Pixels = ResolutionHeightTextbox.Top.Pixels + singleLineTextboxHeight + 10;

			OutputTextbox = new UITextbox(editable: false);
			OutputTextbox.Left.Pixels = 380;
			OutputTextbox.Top.Pixels = 340;
			OutputTextbox.Width.Pixels = 410f;
			OutputTextbox.Height.Pixels = MasterBackground.Height.Pixels - 350;
			OutputTextbox.ForceUpdateText(Information);
			createTextboxTitle(OutputTextbox, "READ ME");
			MasterBackground.Append(OutputTextbox);

			GenerateButton = new UITextbox(includeScrollbar: false, editable: false);
			GenerateButton.Left.Pixels = 640;
			GenerateButton.Top.Pixels = 94;
			GenerateButton.Width.Pixels = 150;
			GenerateButton.Height.Pixels = singleLineTextboxHeight;
			GenerateButton.ForceUpdateText("Generate");
			GenerateButton.OnMouseOver += HandleHoverMouseOver;
			GenerateButton.OnMouseOut += HandleHoverMouseOut;
            GenerateButton.OnLeftClick += GeneratePainting;
			createTextboxTitle(GenerateButton, "Options");
			MasterBackground.Append(GenerateButton);

			ResetButton = new UITextbox(includeScrollbar: false, editable: false);
			ResetButton.Left.Pixels = 640;
			ResetButton.Top.Pixels = 104 + singleLineTextboxHeight;
			ResetButton.Width.Pixels = 150;
			ResetButton.Height.Pixels = singleLineTextboxHeight;
			ResetButton.ForceUpdateText("Reset");
			ResetButton.OnMouseOver += HandleHoverMouseOver;
			ResetButton.OnMouseOut += HandleHoverMouseOut;
            ResetButton.OnLeftClick += ResetToZero;
			MasterBackground.Append(ResetButton);

			Append(MasterBackground);
		}

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

			if (Main.keyState.IsKeyDown(Keys.Escape))
			{
				SoundEngine.PlaySound(SoundID.MenuClose);
				Main.blockInput = false;
				ModContent.GetInstance<GeneratePaintingState>().UserInterface.SetState(null);
				return;
			}

			if (MasterBackground.ContainsPoint(Main.MouseScreen))
			{
				Main.LocalPlayer.mouseInterface = true;
			}
		}

		private void HandleHoverMouseOver(UIMouseEvent evt, UIElement listeningElement)
		{
			if (evt.Target is UIPanel uiPanel)
			{
				uiPanel.BackgroundColor = new Color(73, 94, 171);
				uiPanel.BorderColor = Colors.FancyUIFatButtonMouseOver;
			}
		}

		private void HandleHoverMouseOut(UIMouseEvent evt, UIElement listeningElement)
		{
			if (evt.Target is UIPanel uiPanel)
			{
				uiPanel.BackgroundColor = new Color(63, 82, 151) * 0.7f;
				uiPanel.BorderColor = Color.Black;
			}
		}

		private void ToggleBoolean(UIMouseEvent evt, UIElement listeningElement)
		{
			if (evt.Target is UITextbox uiTextbox)
			{
				if (uiTextbox.CurrentText == "True")
                {
					uiTextbox.ForceUpdateText("False");
                }
				else if (uiTextbox.CurrentText == "False")
				{
					uiTextbox.ForceUpdateText("True");
				}
			}
		}

		private void ToggleLayering(UIMouseEvent evt, UIElement listeningElement)
		{
			if (evt.Target is UITextbox uiTextbox)
			{
				if (uiTextbox.CurrentText == "Above Walls")
				{
					uiTextbox.ForceUpdateText("Above Tiles");
				}
				else if (uiTextbox.CurrentText == "Above Tiles")
				{
					uiTextbox.ForceUpdateText("Behind Walls");
				}
				else
				{
					uiTextbox.ForceUpdateText("Above Walls");
				}
			}
		}

		private void ResetToZero(UIMouseEvent evt, UIElement listeningElement)
		{
			URLTextbox.ForceUpdateText(string.Empty);
			WidthTextbox.ForceUpdateText(string.Empty);
			HeightTextbox.ForceUpdateText(string.Empty);
			ResolutionWidthTextbox.ForceUpdateText(string.Empty);
			ResolutionHeightTextbox.ForceUpdateText(string.Empty);
			FramerateTextbox.ForceUpdateText(string.Empty);
			BrightnessTextbox.ForceUpdateText(string.Empty);
		}

		private void GeneratePainting(UIMouseEvent evt, UIElement listeningElement)
		{
			string url = URLTextbox.CurrentText;
			if (string.IsNullOrEmpty(url) || !int.TryParse(WidthTextbox.CurrentText, out int width) || !int.TryParse(HeightTextbox.CurrentText, out int height))
			{
				return;
			}
			int resX = int.TryParse(ResolutionWidthTextbox.CurrentText, out int possibleResX) ? possibleResX : width * 16;
			int resY = int.TryParse(ResolutionHeightTextbox.CurrentText, out int possibleResY) ? possibleResY : height * 16;
			int frameDuration = int.TryParse(FramerateTextbox.CurrentText, out int possibleFrameDuration) ? possibleFrameDuration : 5;
			int brightness = int.TryParse(BrightnessTextbox.CurrentText, out int possibleBrightness) ? possibleBrightness : 0;
			PaintingRenderLayer layering = LayeringButton.CurrentText == "Above Walls" ? PaintingRenderLayer.BehindTiles : (LayeringButton.CurrentText == "Above Tiles" ? PaintingRenderLayer.AboveEverything : PaintingRenderLayer.BehindWall);
			int imageIndex = Item.NewItem(Entity.GetSource_None(), Main.LocalPlayer.getRect(), ModContent.ItemType<NewImagePainting>());
			PaintingBase generatedPainting = Main.item[imageIndex].ModItem as PaintingBase;
			generatedPainting.PaintingData = new PaintingData(new ImageIndex(url, resX, resY), width, height, frameDuration, brightness, layering);
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				NetMessage.SendData(MessageID.SyncItem, -1, -1, null, imageIndex, 1f);
			}
		}
	}
}