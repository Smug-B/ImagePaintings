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

		public UITextbox LegacyPaintingButton { get; private set; }

		public UITextbox OutputTextbox { get; private set; }

		public UITextbox GenerateButton { get; private set; }

		public UITextbox ResetButton { get; private set; }

		public string Information => "What's New?"
			+ "\n---------------------"
			+ "\nThe newest version of Image Paintings brings this new UI, as well as a new type of painting that can be placed anywhere. "
			+ "These paintings are only breakable with a 'Painting Hammer' which can be crafted from 5 pieces of wood. "
			+ "To open this UI, type '/paintingUI'. To close, click the X button, press escape, or type '/paintingUI' again."
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
			+ "\n \nLegacy Painting: If true, the resulting painting will be a 'legacy' variant that places actual blocks. "
			+ "By default, this is disabled.";

		public override void PreLoad(ref string name)
		{
			AutoSetState = true;
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
			closeButton.OnClick += (evt, listeningElement) =>
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

			WidthTextbox = new UITextbox(defaultText: "Width", includeScrollbar: false);
			WidthTextbox.Left.Pixels = 10;
			WidthTextbox.Top.Pixels = 530;
			WidthTextbox.Width.Pixels = 175;
			WidthTextbox.Height.Pixels = 38;
			WidthTextbox.OnMouseOver += HandleHoverMouseOver;
			WidthTextbox.OnMouseOut += HandleHoverMouseOut;
			handleForcedNumericalValue(WidthTextbox, "Width", 1, 50);
			createTextboxTitle(WidthTextbox, "Dimensions in Blocks ( Required )");
			MasterBackground.Append(WidthTextbox);

			HeightTextbox = new UITextbox(defaultText: "Height", includeScrollbar: false);
			HeightTextbox.Left.Pixels = 195;
			HeightTextbox.Top.Pixels = 530;
			HeightTextbox.Width.Pixels = 175;
			HeightTextbox.Height.Pixels = 38;
			HeightTextbox.OnMouseOver += HandleHoverMouseOver;
			HeightTextbox.OnMouseOut += HandleHoverMouseOut;
			handleForcedNumericalValue(HeightTextbox, "Height", 1, 50);
			MasterBackground.Append(HeightTextbox);

			ResolutionWidthTextbox = new UITextbox(defaultText: "Width", includeScrollbar: false);
			ResolutionWidthTextbox.Left.Pixels = 10;
			ResolutionWidthTextbox.Top.Pixels = 602;
			ResolutionWidthTextbox.Width.Pixels = 175;
			ResolutionWidthTextbox.Height.Pixels = 38;
			ResolutionWidthTextbox.OnMouseOver += HandleHoverMouseOver;
			ResolutionWidthTextbox.OnMouseOut += HandleHoverMouseOut;
			handleForcedNumericalValue(ResolutionWidthTextbox, "Resolution Width", 1, 2048);
			createTextboxTitle(ResolutionWidthTextbox, "Resolution in Pixels ( Recommended )");
			MasterBackground.Append(ResolutionWidthTextbox);

			ResolutionHeightTextbox = new UITextbox(defaultText: "Height", includeScrollbar: false);
			ResolutionHeightTextbox.Left.Pixels = 195;
			ResolutionHeightTextbox.Top.Pixels = 602;
			ResolutionHeightTextbox.Width.Pixels = 175;
			ResolutionHeightTextbox.Height.Pixels = 38;
			ResolutionHeightTextbox.OnMouseOver += HandleHoverMouseOver;
			ResolutionHeightTextbox.OnMouseOut += HandleHoverMouseOut;
			handleForcedNumericalValue(ResolutionHeightTextbox, "Resolution Height", 1, 2048);
			MasterBackground.Append(ResolutionHeightTextbox);

			FramerateTextbox = new UITextbox(defaultText: "Default", includeScrollbar: false);
			FramerateTextbox.Left.Pixels = 380;
			FramerateTextbox.Top.Pixels = 96;
			FramerateTextbox.Width.Pixels = 240f;
			FramerateTextbox.Height.Pixels = 38f;
			FramerateTextbox.OnMouseOver += HandleHoverMouseOver;
			FramerateTextbox.OnMouseOut += HandleHoverMouseOut;
			handleForcedNumericalValue(FramerateTextbox, "Frame Duration", 1, 100);
			createTextboxTitle(FramerateTextbox, "Frame Duration ( GIFs Only )");
			MasterBackground.Append(FramerateTextbox);

			BrightnessTextbox = new UITextbox(defaultText: "Default", includeScrollbar: false);
			BrightnessTextbox.Left.Pixels = 380;
			BrightnessTextbox.Top.Pixels = 168;
			BrightnessTextbox.Width.Pixels = 240f;
			BrightnessTextbox.Height.Pixels = 38f;
			BrightnessTextbox.OnMouseOver += HandleHoverMouseOver;
			BrightnessTextbox.OnMouseOut += HandleHoverMouseOut;
			handleForcedNumericalValue(BrightnessTextbox, "Brightness", 0, 100);
			createTextboxTitle(BrightnessTextbox, "Brightness");
			MasterBackground.Append(BrightnessTextbox);

			LegacyPaintingButton = new UITextbox(includeScrollbar: false, editable: false);
			LegacyPaintingButton.Left.Pixels = 380;
			LegacyPaintingButton.Top.Pixels = 240;
			LegacyPaintingButton.Width.Pixels = 240f;
			LegacyPaintingButton.Height.Pixels = 38;
			LegacyPaintingButton.ForceUpdateText("False");
			LegacyPaintingButton.OnMouseOver += HandleHoverMouseOver;
			LegacyPaintingButton.OnMouseOut += HandleHoverMouseOut;
			LegacyPaintingButton.OnClick += ToggleBoolean;
			createTextboxTitle(LegacyPaintingButton, "Legacy Painting?");
			MasterBackground.Append(LegacyPaintingButton);

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

			OutputTextbox = new UITextbox(editable: false);
			OutputTextbox.Left.Pixels = 380;
			OutputTextbox.Top.Pixels = 340;
			OutputTextbox.Width.Pixels = 410f;
			OutputTextbox.Height.Pixels = 300f;
			OutputTextbox.ForceUpdateText(Information);
			createTextboxTitle(OutputTextbox, "READ ME");
			MasterBackground.Append(OutputTextbox);

			GenerateButton = new UITextbox(includeScrollbar: false, editable: false);
			GenerateButton.Left.Pixels = 640;
			GenerateButton.Top.Pixels = 94;
			GenerateButton.Width.Pixels = 150;
			GenerateButton.Height.Pixels = 38f;
			GenerateButton.ForceUpdateText("Generate");
			GenerateButton.OnMouseOver += HandleHoverMouseOver;
			GenerateButton.OnMouseOut += HandleHoverMouseOut;
            GenerateButton.OnClick += GeneratePainting;
			createTextboxTitle(GenerateButton, "Options");
			MasterBackground.Append(GenerateButton);

			ResetButton = new UITextbox(includeScrollbar: false, editable: false);
			ResetButton.Left.Pixels = 640;
			ResetButton.Top.Pixels = 142;
			ResetButton.Width.Pixels = 150;
			ResetButton.Height.Pixels = 38f;
			ResetButton.ForceUpdateText("Reset");
			ResetButton.OnMouseOver += HandleHoverMouseOver;
			ResetButton.OnMouseOut += HandleHoverMouseOut;
            ResetButton.OnClick += ResetToZero;
			MasterBackground.Append(ResetButton);

			Append(MasterBackground);
		}

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

			if (Main.keyState.IsKeyDown(Keys.Escape))
			{
				SoundEngine.PlaySound(SoundID.MenuClose);
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
			int paintingType = LegacyPaintingButton.CurrentText == "True" ? ModContent.ItemType<ImagePainting>() : ModContent.ItemType<NewImagePainting>();
			int imageIndex = Item.NewItem(Entity.GetSource_None(), Main.LocalPlayer.getRect(), paintingType);
			PaintingBase generatedPainting = Main.item[imageIndex].ModItem as PaintingBase;
			generatedPainting.PaintingData = new PaintingData(new ImageIndex(url, resX, resY), width, height, frameDuration, brightness);
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				NetMessage.SendData(MessageID.SyncItem, -1, -1, null, imageIndex, 1f);
			}
		}
	}
}