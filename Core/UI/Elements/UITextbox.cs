using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using System;
using Terraria.GameInput;
using ReLogic.Graphics;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace ImagePaintings.Core.UI.Elements
{
	public class UITextbox : UIPanel
	{
		public DynamicSpriteFont TextFont { get; private set; }

		public string DefaultText { get; private set; }

		public float TextScale { get; set; } = 1f;

		public event Action<UIElement> OnTextChanged;

		public string CurrentText { get; private set; }

		public UIScrollbar UIScrollbar { get; private set; }

		public float MaxScrollbarViewHeight { get; private set; }

		public bool Editable { get; private set; }

		public bool Focused;

		public event Action<UIElement> OnFocus;

		public event Action<UIElement> OnUnfocus;

		public bool DisplayTextBlinker;

		public int DisplayTextBlinkerTimer;

		public Vector2 TextPadding;

		public UITextbox(DynamicSpriteFont spriteFont = null, string defaultText = "", float textScale = 1f, bool includeScrollbar = true, bool editable = true)
		{
			SetPadding(0f);

			TextFont = spriteFont ?? FontAssets.MouseText.Value;
			DefaultText = defaultText;
			CurrentText = string.Empty;
			TextScale = textScale;
			Editable = editable;
			TextPadding = new Vector2(8, 4);

			if (includeScrollbar)
			{
				UIScrollbar = new UIScrollbar();
				UIScrollbar.Left.Pixels = -4;
				UIScrollbar.Top.Pixels = 10;
				UIScrollbar.HAlign = 1f;
				Append(UIScrollbar);
			}
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			Vector2 mousePosition = new Vector2(Main.mouseX, Main.mouseY);
			bool clicked = Main.mouseLeft || Main.mouseRight;
			if (clicked && Editable)
			{
				if (ContainsPoint(mousePosition))
				{
					Focus();
				}
				else
				{
					Unfocus();
				}
			}
		}

		public override void Recalculate()
		{
			base.Recalculate();

			if (UIScrollbar != null)
			{
				UIScrollbar.Height = Height;
				UIScrollbar.Height.Pixels -= 20;
				UIScrollbar.SetView(GetInnerDimensions().Height, MaxScrollbarViewHeight);
			}
		}

		public override void ScrollWheel(UIScrollWheelEvent evt)
		{
			base.ScrollWheel(evt);

			if (UIScrollbar != null)
			{
				UIScrollbar.ViewPosition -= evt.ScrollWheelValue / 3;
			}
		}

		public void Focus()
		{
			if (!Focused)
			{
				Main.clrInput();
				Focused = true;
				Main.blockInput = true;
				OnFocus?.Invoke(this);
				DisplayTextBlinker = true;
				DisplayTextBlinkerTimer = 0;
			}
		}

		public void Unfocus()
		{
			if (Focused)
			{
				Focused = false;
				Main.blockInput = false;
				OnUnfocus?.Invoke(this);
			}
		}

		public void ForceUpdateText(string newText)
        {
			CurrentText = newText;
			OnTextChanged?.Invoke(this);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);

			CalculatedStyle dimensions = GetDimensions();
			float scaleAdjustment = 1f / TextScale;
			float widthAccountedForExtremities = (dimensions.Width - TextPadding.X * 2) * scaleAdjustment;
			float heightAccountedForExtremities = (dimensions.Height - TextPadding.Y * 2) * scaleAdjustment;

			float offset = 0f;
			if (UIScrollbar != null)
			{
				offset -= UIScrollbar.GetValue();
				widthAccountedForExtremities -= UIScrollbar.GetDimensions().Width + 8;
			}

			if (Focused)
			{
				PlayerInput.WritingText = true;
				Main.instance.HandleIME();
				Main.editSign = true;
				string playerInput = Main.GetInputText(CurrentText);

				void handleUpdatingText()
				{
					if (playerInput != CurrentText)
					{
						CurrentText = playerInput;
						OnTextChanged?.Invoke(this);
					}
				}

				if (UIScrollbar == null)
				{
					string wrappedInput = TextFont.CreateWrappedText(playerInput, widthAccountedForExtremities);
					Vector2 wrappedInputDimensions = TextFont.MeasureString(wrappedInput) * TextScale;
					if (wrappedInputDimensions.Y < heightAccountedForExtremities)
					{
						handleUpdatingText();
					}
					else if (CurrentText.Length > 0)
					{
						CurrentText = CurrentText.Remove(CurrentText.Length - 1, 1);
					}
				}
				else
				{
					handleUpdatingText();
				}

				if (++DisplayTextBlinkerTimer >= 30)
				{
					DisplayTextBlinker = !DisplayTextBlinker;
					DisplayTextBlinkerTimer = 0;
				}

				Main.instance.DrawWindowsIMEPanel(new Vector2(98f, (Main.screenHeight - 36)));
			}

			bool usingDefaultText = !Focused && string.IsNullOrEmpty(CurrentText);
			string currentTextAdjustedForBlinker = usingDefaultText ? DefaultText : CurrentText;
			if (DisplayTextBlinker && Focused)
			{
				currentTextAdjustedForBlinker += "|";
			}

			string wrappedText = TextFont.CreateWrappedText(currentTextAdjustedForBlinker, widthAccountedForExtremities);
			MaxScrollbarViewHeight = TextFont.MeasureString(wrappedText).Y * TextScale;
			string[] textsByLine = wrappedText.Split('\n');
			Vector2 drawPos = dimensions.Position() + TextPadding;
			foreach (string text in textsByLine)
			{
				float textHeight = TextFont.MeasureString(text).Y * TextScale;
				if (offset + textHeight > heightAccountedForExtremities + 10)
				{
					break;
				}

				if (offset >= 0f)
				{
					Vector2 adjustedDrawPos = drawPos + new Vector2(0, offset);
					if (usingDefaultText)
					{
						spriteBatch.DrawString(TextFont, text, adjustedDrawPos, Color.LightGray, 0f, Vector2.Zero, TextScale, SpriteEffects.None, 0f);
					}
					else
					{
						ChatManager.DrawColorCodedStringWithShadow(spriteBatch, TextFont, text, adjustedDrawPos, Color.White, 0f, Vector2.Zero, new Vector2(TextScale), -1, 2);
					}
				}
				offset += textHeight;
			}
			Recalculate();
		}
	}
}