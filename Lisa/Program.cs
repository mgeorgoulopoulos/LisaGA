

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Lisa {
	class MyApplication {
		[STAThread]
		public static void Main() {

			initialize();


			using (var game = new GameWindow(512, 512)) {
				game.Load += (sender, e) => {
					// setup settings, load textures, sounds
					game.VSync = VSyncMode.Off;
				};

				game.Resize += (sender, e) => {
					
				};

				game.KeyDown += (sender, e) => {
					if (e.Key == Key.F1) {
						presentBest = !presentBest;
						displayState = 0;
					}
				};

				game.UpdateFrame += (sender, e) => {
					// add game logic, input handling
					if (game.Keyboard[Key.Escape]) {
						game.Exit();
					}
				};

				game.RenderFrame += (sender, e) => {
					render(game);
				};

				// Run the game at 60 updates per second
				//game.Run(60.0);
				game.Run();
			}
		}


		static bool presentBest = false;

		private static void render(GameWindow game) {
			if (presentBest && bestSpecimen != null) {
				renderBestSpecimen(game);
			} else {
				renderExperiment(game);
			}
		}
		
		private static void renderExperiment(GameWindow game) {
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			GL.Ortho(-1.0, 1.0, -1.0, 1.0, 0.0, 4.0);

			Specimen s = evolver.tick();

			// render specimen on bottom left
			GL.Enable(EnableCap.Blend);
			GL.BlendEquation(BlendEquationMode.FuncAdd);
			GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);
			GL.Viewport(0, 0, 128, 128);
			renderSpecimenLeft(s);
			GL.Viewport(128, 0, 128, 128);
			renderSpecimenRight(s);

			GL.Disable(EnableCap.Blend);

			// draw the compound bitmap
			GL.Viewport(0, 0, 256, 128);
			GL.WindowPos2(0, 256);
			GL.PixelZoom(1.0f, -1.0f);
			GL.DrawPixels(256, 128, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

			// create a copy of the compound bitmap
			GL.WindowPos2(256, 128);
			GL.PixelZoom(1.0f, 1.0f);
			GL.CopyPixels(0, 128, 256, 128, PixelCopyType.Color);

			// subtract 1
			GL.Enable(EnableCap.Blend);
			GL.BlendEquation(BlendEquationMode.FuncSubtract);
			GL.WindowPos2(0, 128);
			GL.CopyPixels(0, 0, 256, 128, PixelCopyType.Color);

			// subtract 2
			GL.Enable(EnableCap.Blend);
			GL.BlendEquation(BlendEquationMode.FuncReverseSubtract);
			GL.WindowPos2(256, 128);
			GL.CopyPixels(0, 0, 256, 128, PixelCopyType.Color);

			// Copy the first diff to the bottom right
			GL.Disable(EnableCap.Blend);
			GL.WindowPos2(256, 0);
			GL.CopyPixels(0, 128, 256, 128, PixelCopyType.Color);

			// Add the second diff upon it
			GL.Enable(EnableCap.Blend);
			GL.BlendEquation(BlendEquationMode.FuncAdd);
			GL.WindowPos2(256, 0);
			GL.CopyPixels(256, 128, 256, 128, PixelCopyType.Color);

			// read back the final diff
			GL.ReadPixels(256, 0, 256, 128, OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, unmanagedBuffer);

			// count all pixel channels
			Marshal.Copy(unmanagedBuffer, managedBuffer, 0, managedBuffer.Length);
			int totalDiff = 0;
			foreach (var b in managedBuffer) {
				totalDiff += (int)b;
			}

			s.fitness = (double)(Specimen.maxDiff - totalDiff) / (double)Specimen.maxDiff;
			if (bestSpecimen == null || s.fitness > bestSpecimen.fitness) {
				bestSpecimen = s;
			}

			// render the best one
			GL.Enable(EnableCap.Blend);
			GL.BlendEquation(BlendEquationMode.FuncAdd);
			GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);
			GL.Viewport(0, 256, 256, 256);
			renderSpecimenLeft(bestSpecimen);
			GL.Viewport(256, 256, 256, 256);
			renderSpecimenRight(bestSpecimen);

			game.SwapBuffers();
		}

		static int displayState = 0;

		static void renderBestSpecimen(GameWindow game) {
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			GL.Ortho(-1.0, 1.0, -1.0, 1.0, -4.0, 4.0);

			GL.MatrixMode(MatrixMode.Modelview);
			GL.PushMatrix();

			displayState++;

			const int waitFrames = 4000;
			float rotation = 0.0f;

			if (displayState < waitFrames) {
				// wait
				rotation = 0.0f;
			} else if (displayState < 900 + waitFrames) { 
				// advance rotation angle
				rotation = (displayState - waitFrames) / 10.0f;
			} else if (displayState < 900 + 2 * waitFrames) {
				rotation = 90.0f;
			} else if (displayState < 1800 + 2 * waitFrames) {
				// reduce rotation angle
				rotation = (900 - (displayState - (900 + 2 * waitFrames))) / 10.0f;
			} else {
				displayState = 0;
			}

			GL.Rotate(rotation, 0.0f, 1.0f, 0.0f);

			// render the best
			GL.Enable(EnableCap.Blend);
			GL.BlendEquation(BlendEquationMode.FuncAdd);
			GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);
			GL.Viewport(0, 0, Math.Min(game.Width, game.Height), Math.Min(game.Width, game.Height));

			// render 3D mesh
			GL.Begin(PrimitiveType.Triangles);
			for (int i = 0; i < Specimen.triangleCount * 3; i++) {
				GL.Color4(bestSpecimen.colors[i]);
				GL.Vertex3(bestSpecimen.verts[i].X, bestSpecimen.verts[i].Y, bestSpecimen.verts[i].Z);
			}
			GL.End();

			GL.PopMatrix();

			game.SwapBuffers();
		}

		private static void renderSpecimenRight(Specimen s) {
			GL.Begin(PrimitiveType.Triangles);
			for (int i = 0; i < Specimen.triangleCount * 3; i++) {
				GL.Color4(s.colors[i]);
				GL.Vertex2(s.verts[i].Z, s.verts[i].Y);
			}
			GL.End();
		}

		private static void renderSpecimenLeft(Specimen s) {
			// render first angle
			GL.Begin(PrimitiveType.Triangles);
			for (int i = 0; i < Specimen.triangleCount * 3; i++) {
				GL.Color4(s.colors[i]);
				GL.Vertex2(s.verts[i].X, s.verts[i].Y);
			}
			GL.End();
		}

		static Specimen bestSpecimen = null;

		static Bitmap bmp = null;
		static BitmapData bmp_data;
		static IntPtr unmanagedBuffer;
		static Byte[] managedBuffer = new Byte[256 * 128 * 3];
		static Evolver evolver = null;

		static void initialize() {
			bmp = new Bitmap("compound.png");
			bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, 
				System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			// allocate unmanaged buffer to count the diffs
			unmanagedBuffer = Marshal.AllocHGlobal(managedBuffer.Length);

			// initialize evolver
			evolver = new Evolver();
			evolver.initialize();
		}
	}
}