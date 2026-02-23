using NuklearDotNet;
using System;
using System.Text;

namespace ExampleShared
{
	public static class Shared
	{
		static NuklearCalculator CalcA;
		static NuklearCalculator CalcB;

		static StringBuilder ConsoleBuffer = new StringBuilder();
		static StringBuilder InputBuffer = new StringBuilder();

		static float sliderFloat = 0.5f;
		static int sliderInt = 50;
		static nuint progressValue = 40;
		static bool checkboxActive = false;
		static int radioOption = 0;
		static nk_colorf pickerColor = new nk_colorf { r = 1.0f, g = 0.5f, b = 0.2f, a = 1.0f };
		static float propertyFloat = 1.0f;
		static int propertyInt = 10;
		static float knobValue = 0.5f;
		static StringBuilder textEditBuffer = new StringBuilder("Edit me!", 256);

		static readonly string[] ButtonLabels = ["Some Button 0", "Some Button 1", "Some Button 2", "Some Button 3", "Some Button 4"];
		static readonly string[] OptionLabels = ["Option 1", "Option 2", "Option 3"];

		public static void Init(NuklearDevice Dev)
		{
			NuklearAPI.Init(Dev);

			CalcA = new NuklearCalculator("Calc A", 50, 50);
			CalcB = new NuklearCalculator("Calc B", 300, 50);

			for (int i = 0; i < 30; i++)
				ConsoleBuffer.AppendLine($"LINE NUMBER {i}");
		}

		public static void DrawLoop(float DeltaTime = 0)
		{
			NuklearAPI.SetDeltaTime(DeltaTime);

			NuklearAPI.Frame(() =>
			{
				TestWindow(50, 50);
				ConsoleThing(280, 350, ConsoleBuffer, InputBuffer);
				WidgetShowcase(600, 50);
			});
		}

		static void TestWindow(float X, float Y)
		{
			const NkPanelFlags Flags = NkPanelFlags.BorderTitle | NkPanelFlags.MovableScalable | NkPanelFlags.Minimizable | NkPanelFlags.ScrollAutoHide;

			NuklearAPI.Window("Test Window", new NkRect(X, Y, 200, 200), Flags, () =>
			{
				NuklearAPI.LayoutRowDynamic(35);

				for (int i = 0; i < 5; i++)
					if (NuklearAPI.ButtonLabel(ButtonLabels[i]))
						Console.WriteLine($"You pressed button {i}");

				if (NuklearAPI.ButtonLabel("Exit"))
					Environment.Exit(0);
			});
		}

		static void ConsoleThing(int X, int Y, StringBuilder OutBuffer, StringBuilder InBuffer)
		{
			const NkPanelFlags Flags = NkPanelFlags.BorderTitle | NkPanelFlags.MovableScalable | NkPanelFlags.Minimizable;

			NuklearAPI.Window("Console", new NkRect(X, Y, 300, 300), Flags, () =>
			{
				NkRect Bounds = NuklearAPI.WindowGetBounds();
				NuklearAPI.LayoutRowDynamic(Bounds.H - 85);
				NuklearAPI.EditString(NkEditTypes.Editor | (NkEditTypes)(NkEditFlags.GotoEndOnActivate), OutBuffer);

				NuklearAPI.LayoutRowDynamic();
				if (NuklearAPI.EditString(NkEditTypes.Field, InBuffer).HasFlag(NkEditEvents.Active) && NuklearAPI.IsKeyPressed(NkKeys.Enter))
				{
					string Txt = InBuffer.ToString().Trim();
					InBuffer.Clear();

					if (Txt.Length > 0)
						OutBuffer.AppendLine(Txt);
				}
			});
		}

		static void WidgetShowcase(float X, float Y)
		{
			const NkPanelFlags Flags = NkPanelFlags.BorderTitle | NkPanelFlags.MovableScalable | NkPanelFlags.Minimizable | NkPanelFlags.ScrollAutoHide;

			NuklearAPI.Window("Widget Showcase", new NkRect(X, Y, 350, 700), Flags, () =>
			{
				NuklearAPI.LayoutRowDynamic(25);
				NuklearAPI.Label("=== Sliders ===", (NkTextAlign)NkTextAlignment.NK_TEXT_CENTERED);

				NuklearAPI.LayoutRowDynamic(25);
				NuklearAPI.Label($"Float Slider: {sliderFloat:F2}");
				NuklearAPI.LayoutRowDynamic(25);
				sliderFloat = NuklearAPI.SlideFloat(0.0f, sliderFloat, 1.0f, 0.01f);

				NuklearAPI.LayoutRowDynamic(25);
				NuklearAPI.Label($"Int Slider: {sliderInt}");
				NuklearAPI.LayoutRowDynamic(25);
				sliderInt = NuklearAPI.SlideInt(0, sliderInt, 100, 1);

				NuklearAPI.LayoutRowDynamic(25);
				NuklearAPI.Label("=== Progress Bar ===", (NkTextAlign)NkTextAlignment.NK_TEXT_CENTERED);

				NuklearAPI.LayoutRowDynamic(25);
				NuklearAPI.Label($"Progress: {progressValue}%");
				NuklearAPI.LayoutRowDynamic(30);
				progressValue = NuklearAPI.Progress(progressValue, 100);

				NuklearAPI.LayoutRowDynamic(25);
				NuklearAPI.Label("=== Checkbox & Radio ===", (NkTextAlign)NkTextAlignment.NK_TEXT_CENTERED);

				NuklearAPI.LayoutRowDynamic(25);
				checkboxActive = NuklearAPI.CheckLabel("Enable Feature", checkboxActive);

				NuklearAPI.LayoutRowDynamic(25);
				NuklearAPI.Label("Select Option:");
				NuklearAPI.LayoutRowDynamic(20);
				for (int i = 0; i < 3; i++)
				{
					if (NuklearAPI.OptionLabel(OptionLabels[i], radioOption == i))
						radioOption = i;
				}

				NuklearAPI.LayoutRowDynamic(25);
				NuklearAPI.Label("=== Color Picker ===", (NkTextAlign)NkTextAlignment.NK_TEXT_CENTERED);

				NuklearAPI.LayoutRowDynamic(120);
				pickerColor = NuklearAPI.ColorPicker(pickerColor);

				NuklearAPI.LayoutRowDynamic(25);
				NuklearAPI.LabelColored(
					$"R:{pickerColor.r:F2} G:{pickerColor.g:F2} B:{pickerColor.b:F2}",
					NkColor.FromColorf(pickerColor));

				NuklearAPI.LayoutRowDynamic(25);
				NuklearAPI.Label("=== Property Fields ===", (NkTextAlign)NkTextAlignment.NK_TEXT_CENTERED);

				NuklearAPI.LayoutRowDynamic(25);
				propertyFloat = NuklearAPI.PropertyFloat("Float:", propertyFloat, new(0.0f, 10.0f, 0.1f, 0.1f));

				NuklearAPI.LayoutRowDynamic(25);
				propertyInt = NuklearAPI.PropertyInt("Int:", propertyInt, new(0, 100, 1, 1.0f));

				NuklearAPI.LayoutRowDynamic(25);
				NuklearAPI.Label("=== Knob ===", (NkTextAlign)NkTextAlignment.NK_TEXT_CENTERED);

				NuklearAPI.LayoutRowDynamic(80);
				knobValue = NuklearAPI.KnobFloat(0.0f, knobValue, 1.0f, 0.01f);
				NuklearAPI.LayoutRowDynamic(25);
				NuklearAPI.Label($"Knob Value: {knobValue:F2}", (NkTextAlign)NkTextAlignment.NK_TEXT_CENTERED);

				NuklearAPI.LayoutRowDynamic(25);
				NuklearAPI.Label("=== Text Edit ===", (NkTextAlign)NkTextAlignment.NK_TEXT_CENTERED);

				NuklearAPI.LayoutRowDynamic(25);
				NuklearAPI.EditString(NkEditTypes.Field, textEditBuffer);

				NuklearAPI.LayoutRowDynamic(25);
				NuklearAPI.Label("=== Spacing & Rules ===", (NkTextAlign)NkTextAlignment.NK_TEXT_CENTERED);

				NuklearAPI.LayoutRowDynamic(10);
				NuklearAPI.Spacer();

				NuklearAPI.LayoutRowDynamic(5);
				NuklearAPI.RuleHorizontal(new NkColor(255, 100, 100, 255), 1);

				NuklearAPI.LayoutRowDynamic(25);
				NuklearAPI.Label("Above: spacer + rule", (NkTextAlign)NkTextAlignment.NK_TEXT_CENTERED);

				NuklearAPI.LayoutRowDynamic(25);
				NuklearAPI.Label("=== Line Chart ===", (NkTextAlign)NkTextAlignment.NK_TEXT_CENTERED);

				NuklearAPI.LayoutRowDynamic(80);
				NuklearAPI.ChartLines(10, 0.0f, 1.0f, _ =>
				{
					for (int i = 0; i < 10; i++)
					{
						float val = (float)Math.Sin(i * 0.5 + Environment.TickCount64 * 0.001) * 0.5f + 0.5f;
						NuklearAPI.ChartPush(val);
					}
				});

				NuklearAPI.LayoutRowDynamic(25);
				NuklearAPI.Label("=== Collapsible Group ===", (NkTextAlign)NkTextAlignment.NK_TEXT_CENTERED);

				NuklearAPI.LayoutRowDynamic(100);
				NuklearAPI.Group("inner_group", "Details", NkPanelFlags.Border | NkPanelFlags.Title, () =>
				{
					NuklearAPI.LayoutRowDynamic(20);
					NuklearAPI.Label("This is a nested group!");
					NuklearAPI.Label("Groups can have scrollbars.");
					NuklearAPI.Label("And more content...");
					for (int i = 0; i < 5; i++)
					{
						NuklearAPI.Label($"Item {i + 1}");
					}
				});
			});
		}

		sealed class NuklearCalculator
		{
			public enum CurrentThing
			{
				A = 0,
				B
			}

			public bool Open = true;
			public bool Set;
			public float A, B;
			public char Prev, Op;

			public CurrentThing CurrentThingy;
			public float Current
			{
				get
				{
					if (CurrentThingy == CurrentThing.A)
						return A;
					return B;
				}
				set
				{
					if (CurrentThingy == CurrentThing.A)
						A = value;
					else
						B = value;
				}
			}

			StringBuilder Buffer;
			string Name;
			float X, Y;

			public NuklearCalculator(string Name, float X, float Y)
			{
				Buffer = new StringBuilder(255);

				this.Name = Name;
				this.X = X;
				this.Y = Y;
			}

			public void Calculator()
			{
				const string Numbers = "789456123";
				const string Ops = "+-*/";
				const NkPanelFlags F = NkPanelFlags.Border | NkPanelFlags.Movable | NkPanelFlags.NoScrollbar | NkPanelFlags.Title
					| NkPanelFlags.Closable | NkPanelFlags.Minimizable;

				bool Solve = false;
				string BufferStr;

				NuklearAPI.Window(Name, new NkRect(X, Y, 180, 250), F, () =>
				{
					NuklearAPI.LayoutRowDynamic(35, 1);

					Buffer.Clear();
					Buffer.AppendFormat("{0:0.00}", Current);

					NuklearAPI.EditString(NkEditTypes.Simple, Buffer, (ref nk_text_edit TextBox, uint Rune) =>
					{
						char C = (char)Rune;

						if (char.IsNumber(C))
							return 1;

						return 0;
					});

					BufferStr = Buffer.ToString().Trim();
					if (BufferStr.Length > 0)
						if (float.TryParse(BufferStr, out float CurFloat))
							Current = CurFloat;

					NuklearAPI.LayoutRowDynamic(35, 4);
					for (int i = 0; i < 16; i++)
					{
						if (i == 12)
						{
							if (NuklearAPI.ButtonLabel("C"))
							{
								A = B = 0;
								Op = ' ';
								Set = false;
								CurrentThingy = CurrentThing.A;
							}

							if (NuklearAPI.ButtonLabel("0"))
							{
								Current = Current * 10;
								Op = ' ';
							}

							if (NuklearAPI.ButtonLabel("="))
							{
								Solve = true;
								Prev = Op;
								Op = ' ';
							}
						}
						else if (((i + 1) % 4) != 0)
						{
							int NumIdx = (i / 4) * 3 + i % 4;

							if (NumIdx < Numbers.Length && NuklearAPI.ButtonText(Numbers[NumIdx]))
							{
								Current = Current * 10 + int.Parse(Numbers[NumIdx].ToString());
								Set = false;
							}
						}
						else if (NuklearAPI.ButtonText(Ops[i / 4]))
						{
							if (!Set)
							{
								if (CurrentThingy != CurrentThing.B)
									CurrentThingy = CurrentThing.B;
								else
								{
									Prev = Op;
									Solve = true;
								}
							}

							Op = Ops[i / 4];
							Set = true;
						}
					}

					if (Solve)
					{
						if (Prev == '+')
							A = A + B;
						else if (Prev == '-')
							A = A - B;
						else if (Prev == '*')
							A = A * B;
						else if (Prev == '/')
							A = A / B;

						CurrentThingy = CurrentThing.A;
						if (Set)
							CurrentThingy = CurrentThing.B;

						B = 0;
						Set = false;
					}
				});

				if (NuklearAPI.WindowIsClosed(Name) || NuklearAPI.WindowIsHidden(Name))
					Open = false;
			}
		}
	}
}
