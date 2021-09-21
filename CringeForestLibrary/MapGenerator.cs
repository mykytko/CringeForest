using System.Diagnostics;

namespace CringeForestLibrary
{
    class PerlinNoise2D
    {
        private int _repeat;

        public PerlinNoise2D(int repeat = -1)
        {
            _repeat = repeat;
        }

        private static readonly int[] Permutation = { 151,160,137,91,90,15,                 // Hash lookup table as defined by Ken Perlin.  This is a randomly
            131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,    // arranged array of all numbers from 0-255 inclusive.
            190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
            88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
            77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
            102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
            135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
            5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
            223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
            129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
            251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
            49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
            138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
        };

        private static readonly int[] P;                                                    // Doubled permutation to avoid overflow

        static PerlinNoise2D()
        {
            P = new int[512];
            for (int x = 0; x < 512; x++)
            {
                P[x] = Permutation[x % 256];
            }
        }

        public double Noise(double x, double y)
        {
            if (_repeat > 0)
            {                                    // If we have any repeat on, change the coordinates to their "local" repetitions
                x = x % _repeat;
                y = y % _repeat;
            }

            int xi = (int)x & 255;                              // Calculate the "unit square" that the point asked will be located in
            int yi = (int)y & 255;                              // The left bound is ( |_x_|,|_y_|,|_z_| ) and the right bound is that
            double xf = x - (int)x;                             // plus 1.  Next we calculate the location (from 0.0 to 1.0) in that square.
            double yf = y - (int)y;

            double u = Fade(xf);
            double v = Fade(yf);

            int g1, g2, g3, g4;
            g1 = P[P[xi] + yi];
            g2 = P[P[Inc(xi)] + yi];
            g3 = P[P[xi] + Inc(yi)];
            g4 = P[P[Inc(xi)] + Inc(yi)];

            double d1 = Grad(g1, xf, yf);
            double d2 = Grad(g2, xf - 1, yf);
            double d3 = Grad(g3, xf, yf - 1);
            double d4 = Grad(g4, xf - 1, yf - 1);

            double x1Inter = Lerp(u, d1, d2);                    // The gradient function calculates the dot product between a pseudorandom
            double x2Inter = Lerp(u, d3, d4);                    // gradient vector and the vector from the input coordinate to the 8
            double yInter = Lerp(v, x1Inter, x2Inter);           // surrounding points in its unit cube.
                                                                 // This is all then lerped together as a sort of weighted average based on the faded (u,v,w)
                                                                 // values we made earlier.
            return (yInter + 1) / 2;                                       // For convenience we bind the result to 0 - 1 (theoretical min/max before is [-1, 1])
        }
        private int Inc(int num)
        {
            num++;
            if (_repeat > 0) num %= _repeat;

            return num;
        }
        private static double Grad(int hash, double x, double y)
        {
            switch (hash & 3)
            {
                case 0: return x + y;
                case 1: return -x + y;
                case 2: return x - y;
                case 3: return -x - y;
                default: return 0; //never happens
            }
        }
        private static double Fade(double t)
        {
            // Fade function as defined by Ken Perlin.  This eases coordinate values
            // so that they will "ease" towards integral values.  This ends up smoothing
            // the final output.
            return t * t * t * (t * (t * 6 - 15) + 10);         // 6t^5 - 15t^4 + 10t^3
        }
        private double Lerp(double amount, double left, double right)
        {
            return ((1 - amount) * left + amount * right);
        }
    }
    public class MapGenerator
    {
        private double[,] _terrain; //height map, 0.3 - sea ​​level
        private int _width;
        private int _height;
        private const double Scale = 0.08; //the higher is scale, the rougher is the gradient

        public MapGenerator(int width = 100, int height = 100)
        {
            Trace.WriteLine("Generating the map...");
            _width = width;
            _height = height;
            _terrain = new double[width, height];
        }
        public double[,] GenerateTerrain()
        {
            PerlinNoise2D pn = new PerlinNoise2D();
            for (int i = 0; i < _width; i++)
            {
                for (int j = 0; j < _height; j++)
                {
                    _terrain[i, j] = pn.Noise(i * Scale, j * Scale);
                }
            }
            return _terrain;
        }
    }
}