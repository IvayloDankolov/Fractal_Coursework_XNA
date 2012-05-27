using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FractalView
{
    class Screen
    {
        ScreenVertex[] verts;

        static int[] indices = new int[] { 3, 2, 0, 2, 1, 0 };

        public Screen()
        {
            verts = new ScreenVertex[] {
                new ScreenVertex { position = new Vector4(-1, -1, 0f, 1f) },
                new ScreenVertex { position = new Vector4(1, -1, 0f, 1f) },
                new ScreenVertex { position = new Vector4(1, 1, 0f, 1f) },
                new ScreenVertex { position = new Vector4(-1, 1, 0f, 1f) }
            };
        }  

        public void Draw(GraphicsDevice device)
        {
            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, verts, 0, 4, indices, 0, 2);
        }
    }
}
