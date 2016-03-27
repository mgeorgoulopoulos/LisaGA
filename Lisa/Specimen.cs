using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;

namespace Lisa {
	class Specimen {

		public const int triangleCount = 200;

		public Vector3[] verts = new Vector3[triangleCount * 3];
		public Color4[] colors = new Color4[triangleCount * 3];
		public double fitness = 0;
		public const int maxDiff = 256 * 128 * 3 * 256;

		public static Specimen random {
			get {
				Specimen ret = new Specimen();

				for (int i = 0; i < triangleCount * 3; i++) {
					ret.verts[i] = randomVertex();
					//ret.colors[i] = randomColor();
					ret.colors[i] = Color4.Black;
				}

				return ret;
			}
		}


		public static Vector3 randomVertex() {
			Vector3 ret = new Vector3();

			ret.X = nextFloat * 2.0f - 1.0f;
			ret.Y = nextFloat * 2.0f - 1.0f;
			ret.Z = nextFloat * 2.0f - 1.0f;

			return ret;
		}

		public static Color4 randomColor() {
			Color4 ret = new Color4();

			ret.R = nextFloat;
			ret.G = nextFloat;
			ret.B = nextFloat;
			ret.A = 1.0f;

			return ret;
		}

		static Random rnd = new Random();

		public static float nextFloat {
			get {
				return (float)rnd.NextDouble();
			}
		}

		public Specimen clone {
			get {
				Specimen ret = new Specimen();
				ret.verts = (Vector3[]) verts.Clone();
				ret.colors = (Color4[]) colors.Clone();
				ret.fitness = fitness;

				return ret;
			}
		}
	}
}
