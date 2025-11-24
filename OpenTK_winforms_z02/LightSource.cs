using OpenTK.Graphics.OpenGL;

namespace OpenTK_winforms_z02
{
    public class LightSource
    {
        public LightName Id;
        public float[] Ambient = new float[4];
        public float[] Diffuse = new float[4];
        public float[] Specular = new float[4];
        public float[] Position = new float[4];
        public bool Enabled = false;

        public LightSource(LightName id, float x, float y, float z)
        {
            Id = id;

            Ambient[0] = 0.1f; Ambient[1] = 0.1f; Ambient[2] = 0.1f; Ambient[3] = 1f;
            Diffuse[0] = 1f; Diffuse[1] = 1f; Diffuse[2] = 1f; Diffuse[3] = 1f;
            Specular[0] = 1f; Specular[1] = 1f; Specular[2] = 1f; Specular[3] = 1f;

            Position[0] = x;
            Position[1] = y;
            Position[2] = z;
            Position[3] = 1f;
        }

        public void Apply()
        {
            GL.Light(Id, LightParameter.Ambient, Ambient);
            GL.Light(Id, LightParameter.Diffuse, Diffuse);
            GL.Light(Id, LightParameter.Specular, Specular);
            GL.Light(Id, LightParameter.Position, Position);

            if (Enabled)
                GL.Enable((EnableCap)Id);
            else
                GL.Disable((EnableCap)Id);
        }
    }
}
