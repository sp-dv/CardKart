using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace SGL
{
    public static class TextureLoader
    {
        private static List<LoadMe> LoadingQueue = new List<LoadMe>();

        public static void LoadTextures()
        {
            lock (LoadingQueue)
            {
                foreach (var loadme in LoadingQueue)
                {
                    var id = CreateTextureID(loadme.Image);
                    loadme.GLTexture.ID = id;
                    loadme.GLTexture.Loaded = true;
                }

                LoadingQueue.Clear();
            }
        }

        public static Texture CreateTexture(Image image)
        {
            var texture = new Texture(-1);
            lock (LoadingQueue)
            {
                var loadme = new LoadMe();
                loadme.GLTexture = texture;
                loadme.Image = image;
                LoadingQueue.Add(loadme);
            }
            return texture;
        }

        private static int CreateTextureID(Image image)
        {
            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);

            Bitmap bmp = new Bitmap(image);

            BitmapData data = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                data.Width,
                data.Height,
                0,
                PixelFormat.Bgra,
                PixelType.UnsignedByte,
                data.Scan0);

            bmp.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

            return id;
        }

        class LoadMe
        {
            public Image Image;
            public Texture GLTexture;
        }
    }



    public class Texture
    {
        public int ID;
        public bool Loaded;

        public Texture(int iD)
        {
            ID = iD;
        }
    }
}
