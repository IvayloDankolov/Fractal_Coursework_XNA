using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Runtime.InteropServices;

namespace FractalView
{
    
    public struct ScreenVertex : IVertexType
    {
        public Vector4 position;

        
        public static VertexDeclaration Format = new VertexDeclaration(
            new VertexElement[] {
                new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 0)
            });

        #region IVertexType Members

        public VertexDeclaration VertexDeclaration
        {
            get { return Format; }
        }

        #endregion
    }
}
