using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenTK_winforms_z02
{
    public partial class Form1 : Form
    {
        // Sursa 1 de iluminare (extinsă, POO) – Light1
        private LightSource light1;

        //Stări de control cameră.
        private int eyePosX, eyePosY, eyePosZ;

        private Point mousePos;
        private float camDepth;

        //Stări de control mouse.
        private bool statusControlMouse2D, statusControlMouse3D, statusMouseDown;

        //Stări de control axe de coordonate.
        private bool statusControlAxe;

        //Stări de control iluminare.
        private bool lightON;
        private bool lightON_0;

        //Stări de control obiecte 3D.
        private string statusCube;

        //Structuri de stocare a vertexurilor și a listelor de vertexuri.
        private int[,] arrVertex = new int[50, 3];
        private int nVertex;

        private int[] arrQuadsList = new int[100];
        private int nQuadsList;

        private int[] arrTrianglesList = new int[100];
        private int nTrianglesList;

        //Fișiere de in/out pentru manipularea vertexurilor.
        private string fileVertex = "vertexList.txt";
        private string fileQList = "quadsVertexList.txt";
        private string fileTList = "trianglesVertexList.txt";
        private bool statusFiles;

        //# SET 1
        private float[] valuesAmbientTemplate0 = new float[] { 0.1f, 0.1f, 0.1f, 1.0f };
        private float[] valuesDiffuseTemplate0 = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
        private float[] valuesSpecularTemplate0 = new float[] { 0.1f, 0.1f, 0.1f, 1.0f };
        private float[] valuesPositionTemplate0 = new float[] { 0.0f, 0.0f, 5.0f, 1.0f };

        private float[] valuesAmbient0 = new float[4];
        private float[] valuesDiffuse0 = new float[4];
        private float[] valuesSpecular0 = new float[4];
        private float[] valuesPosition0 = new float[4];

        //-----------------------------------------------------------------------------------------
        //   ON_LOAD
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetupValues();
            SetupWindowGUI();

            // Inițializare sursă suplimentară Light1 – versiunea extinsă
            light1 = new LightSource(LightName.Light1, 30f, 30f, 30f);
            light1.Enabled = true;
        }

        //-----------------------------------------------------------------------------------------
        //   SETARI INIȚIALE
        private void SetupValues()
        {
            eyePosX = 100;
            eyePosY = 100;
            eyePosZ = 50;

            camDepth = 1.04f;

            setLight0Values();

            numericXeye.Value = eyePosX;
            numericYeye.Value = eyePosY;
            numericZeye.Value = eyePosZ;
        }

        private void SetupWindowGUI()
        {
            setControlMouse2D(false);
            setControlMouse3D(false);

            numericCameraDepth.Value = (int)camDepth;

            setControlAxe(true);

            setCubeStatus("OFF");
            setIlluminationStatus(false);
            setSource0Status(false);

            setTrackLigh0Default();
            setColorAmbientLigh0Default();
            setColorDifuseLigh0Default();
            setColorSpecularLigh0Default();
        }

        //-----------------------------------------------------------------------------------------
        //   MANIPULARE VERTEXURI ȘI LISTE DE COORDONATE.
        private void loadVertex()
        {
            try
            {
                StreamReader fileReader = new StreamReader(fileVertex);
                nVertex = Convert.ToInt32(fileReader.ReadLine().Trim());
                Console.WriteLine("Vertexuri citite: " + nVertex.ToString());

                string tmpStr = "";
                string[] str = new string[3];
                for (int i = 0; i < nVertex; i++)
                {
                    tmpStr = fileReader.ReadLine();
                    str = tmpStr.Trim().Split(' ');
                    arrVertex[i, 0] = Convert.ToInt32(str[0].Trim());
                    arrVertex[i, 1] = Convert.ToInt32(str[1].Trim());
                    arrVertex[i, 2] = Convert.ToInt32(str[2].Trim());
                }
                fileReader.Close();
            }
            catch (Exception)
            {
                statusFiles = false;
                Console.WriteLine("Fisierul cu informații vertex <" + fileVertex + "> nu exista!");
                MessageBox.Show("Fisierul cu informații vertex <" + fileVertex + "> nu exista!");
            }
        }

        private void loadQList()
        {
            try
            {
                StreamReader fileReader = new StreamReader(fileQList);

                int tmp;
                string line;
                nQuadsList = 0;

                while ((line = fileReader.ReadLine()) != null)
                {
                    tmp = Convert.ToInt32(line.Trim());
                    arrQuadsList[nQuadsList] = tmp;
                    nQuadsList++;
                }

                fileReader.Close();
            }
            catch (Exception)
            {
                statusFiles = false;
                MessageBox.Show("Fisierul cu informații vertex <" + fileQList + "> nu exista!");
            }
        }

        private void loadTList()
        {
            try
            {
                StreamReader fileReader = new StreamReader(fileTList);

                int tmp;
                string line;
                nTrianglesList = 0;

                while ((line = fileReader.ReadLine()) != null)
                {
                    tmp = Convert.ToInt32(line.Trim());
                    arrTrianglesList[nTrianglesList] = tmp;
                    nTrianglesList++;
                }

                fileReader.Close();
            }
            catch (Exception)
            {
                statusFiles = false;
                MessageBox.Show("Fisierul cu informații vertex <" + fileTList + "> nu exista!");
            }
        }

        //-----------------------------------------------------------------------------------------
        //   CONTROL CAMERĂ
        private void numericXeye_ValueChanged(object sender, EventArgs e)
        {
            eyePosX = (int)numericXeye.Value;
            GlControl1.Invalidate();
        }

        private void numericYeye_ValueChanged(object sender, EventArgs e)
        {
            eyePosY = (int)numericYeye.Value;
            GlControl1.Invalidate();
        }

        private void numericZeye_ValueChanged(object sender, EventArgs e)
        {
            eyePosZ = (int)numericZeye.Value;
            GlControl1.Invalidate();
        }

        private void numericCameraDepth_ValueChanged(object sender, EventArgs e)
        {
            camDepth = 1 + ((float)numericCameraDepth.Value) * 0.1f;
            GlControl1.Invalidate();
        }

        //-----------------------------------------------------------------------------------------
        //   CONTROL MOUSE
        private void setControlMouse2D(bool status)
        {
            if (!status)
            {
                statusControlMouse2D = false;
                btnMouseControl2D.Text = "2D mouse control OFF";
            }
            else
            {
                statusControlMouse2D = true;
                btnMouseControl2D.Text = "2D mouse control ON";
            }
        }

        private void setControlMouse3D(bool status)
        {
            if (!status)
            {
                statusControlMouse3D = false;
                btnMouseControl3D.Text = "3D mouse control OFF";
            }
            else
            {
                statusControlMouse3D = true;
                btnMouseControl3D.Text = "3D mouse control ON";
            }
        }

        private void btnMouseControl2D_Click(object sender, EventArgs e)
        {
            if (statusControlMouse2D)
            {
                setControlMouse2D(false);
            }
            else
            {
                setControlMouse3D(false);
                setControlMouse2D(true);
            }
        }

        private void btnMouseControl3D_Click(object sender, EventArgs e)
        {
            if (statusControlMouse3D)
            {
                setControlMouse3D(false);
            }
            else
            {
                setControlMouse2D(false);
                setControlMouse3D(true);
            }
        }

        private void GlControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (statusMouseDown)
            {
                mousePos = new Point(e.X, e.Y);
                GlControl1.Invalidate();
            }
        }

        private void GlControl1_MouseDown(object sender, MouseEventArgs e)
        {
            statusMouseDown = true;
        }

        private void GlControl1_MouseUp(object sender, MouseEventArgs e)
        {
            statusMouseDown = false;
        }

        //-----------------------------------------------------------------------------------------
        //   CONTROL ILUMINARE
        private void setIlluminationStatus(bool status)
        {
            if (!status)
            {
                lightON = false;
                btnLights.Text = "Iluminare OFF";
            }
            else
            {
                lightON = true;
                btnLights.Text = "Iluminare ON";
            }
        }

        private void btnLights_Click(object sender, EventArgs e)
        {
            if (!lightON)
                setIlluminationStatus(true);
            else
                setIlluminationStatus(false);

            GlControl1.Invalidate();
        }

        private void btnLightsNo_Click(object sender, EventArgs e)
        {
            int nr = GL.GetInteger(GetPName.MaxLights);
            MessageBox.Show("Nr. maxim de luminii pentru aceasta implementare este <" + nr.ToString() + ">.");
        }

        private void setSource0Status(bool status)
        {
            if (!status)
            {
                lightON_0 = false;
                btnLight0.Text = "Sursa 0 OFF";
            }
            else
            {
                lightON_0 = true;
                btnLight0.Text = "Sursa 0 ON";
            }
        }

        private void btnLight0_Click(object sender, EventArgs e)
        {
            if (lightON)
            {
                if (!lightON_0)
                    setSource0Status(true);
                else
                    setSource0Status(false);

                GlControl1.Invalidate();
            }
        }

        private void setTrackLigh0Default()
        {
            trackLight0PositionX.Value = (int)valuesPosition0[0];
            trackLight0PositionY.Value = (int)valuesPosition0[1];
            trackLight0PositionZ.Value = (int)valuesPosition0[2];
        }

        private void trackLight0PositionX_Scroll(object sender, EventArgs e)
        {
            valuesPosition0[0] = trackLight0PositionX.Value;
            GlControl1.Invalidate();
        }

        private void trackLight0PositionY_Scroll(object sender, EventArgs e)
        {
            valuesPosition0[1] = trackLight0PositionY.Value;
            GlControl1.Invalidate();
        }

        private void trackLight0PositionZ_Scroll(object sender, EventArgs e)
        {
            valuesPosition0[2] = trackLight0PositionZ.Value;
            GlControl1.Invalidate();
        }

        private void setColorAmbientLigh0Default()
        {
            numericLight0Ambient_Red.Value = (decimal)(valuesAmbient0[0] * 100);
            numericLight0Ambient_Green.Value = (decimal)(valuesAmbient0[1] * 100);
            numericLight0Ambient_Blue.Value = (decimal)(valuesAmbient0[2] * 100);
        }

        private void numericLight0Ambient_Red_ValueChanged(object sender, EventArgs e)
        {
            valuesAmbient0[0] = (float)numericLight0Ambient_Red.Value / 100;
            GlControl1.Invalidate();
        }

        private void numericLight0Ambient_Green_ValueChanged(object sender, EventArgs e)
        {
            valuesAmbient0[1] = (float)numericLight0Ambient_Green.Value / 100;
            GlControl1.Invalidate();
        }

        private void numericLight0Ambient_Blue_ValueChanged(object sender, EventArgs e)
        {
            valuesAmbient0[2] = (float)numericLight0Ambient_Blue.Value / 100;
            GlControl1.Invalidate();
        }

        private void setColorDifuseLigh0Default()
        {
            numericLight0Difuse_Red.Value = (decimal)(valuesDiffuse0[0] * 100);
            numericLight0Difuse_Green.Value = (decimal)(valuesDiffuse0[1] * 100);
            numericLight0Difuse_Blue.Value = (decimal)(valuesDiffuse0[2] * 100);
        }

        private void numericLight0Difuse_Red_ValueChanged(object sender, EventArgs e)
        {
            valuesDiffuse0[0] = (float)numericLight0Difuse_Red.Value / 100;
            GlControl1.Invalidate();
        }

        private void numericLight0Difuse_Green_ValueChanged(object sender, EventArgs e)
        {
            valuesDiffuse0[1] = (float)numericLight0Difuse_Green.Value / 100;
            GlControl1.Invalidate();
        }

        private void numericLight0Difuse_Blue_ValueChanged(object sender, EventArgs e)
        {
            valuesDiffuse0[2] = (float)numericLight0Difuse_Blue.Value / 100;
            GlControl1.Invalidate();
        }

        private void setColorSpecularLigh0Default()
        {
            numericLight0Specular_Red.Value = (decimal)(valuesSpecular0[0] * 100);
            numericLight0Specular_Green.Value = (decimal)(valuesSpecular0[1] * 100);
            numericLight0Specular_Blue.Value = (decimal)(valuesSpecular0[2] * 100);
        }

        private void numericLight0Specular_Red_ValueChanged(object sender, EventArgs e)
        {
            valuesSpecular0[0] = (float)numericLight0Specular_Red.Value / 100;
            GlControl1.Invalidate();
        }

        private void numericLight0Specular_Green_ValueChanged(object sender, EventArgs e)
        {
            valuesSpecular0[1] = (float)numericLight0Specular_Green.Value / 100;
            GlControl1.Invalidate();
        }

        private void numericLight0Specular_Blue_ValueChanged(object sender, EventArgs e)
        {
            valuesSpecular0[2] = (float)numericLight0Specular_Blue.Value / 100;
            GlControl1.Invalidate();
        }

        private void setLight0Values()
        {
            for (int i = 0; i < valuesAmbientTemplate0.Length; i++)
                valuesAmbient0[i] = valuesAmbientTemplate0[i];

            for (int i = 0; i < valuesDiffuseTemplate0.Length; i++)
                valuesDiffuse0[i] = valuesDiffuseTemplate0[i];

            for (int i = 0; i < valuesSpecularTemplate0.Length; i++)
                valuesSpecular0[i] = valuesSpecularTemplate0[i];

            for (int i = 0; i < valuesPositionTemplate0.Length; i++)
                valuesPosition0[i] = valuesPositionTemplate0[i];
        }

        private void btnLight0Reset_Click(object sender, EventArgs e)
        {
            setLight0Values();
            setTrackLigh0Default();
            setColorAmbientLigh0Default();
            setColorDifuseLigh0Default();
            setColorSpecularLigh0Default();
            GlControl1.Invalidate();
        }

        //-----------------------------------------------------------------------------------------
        //   CONTROL OBIECTE 3D
        private void setControlAxe(bool status)
        {
            if (!status)
            {
                statusControlAxe = false;
                btnShowAxes.Text = "Axe Oxyz OFF";
            }
            else
            {
                statusControlAxe = true;
                btnShowAxes.Text = "Axe Oxyz ON";
            }
        }

        private void btnShowAxes_Click(object sender, EventArgs e)
        {
            if (statusControlAxe)
                setControlAxe(false);
            else
                setControlAxe(true);

            GlControl1.Invalidate();
        }

        private void setCubeStatus(string status)
        {
            if (status.Trim().ToUpper().Equals("TRIANGLES"))
                statusCube = "TRIANGLES";
            else if (status.Trim().ToUpper().Equals("QUADS"))
                statusCube = "QUADS";
            else
                statusCube = "OFF";
        }

        private void btnCubeQ_Click(object sender, EventArgs e)
        {
            statusFiles = true;
            loadVertex();
            loadQList();
            setCubeStatus("QUADS");
            GlControl1.Invalidate();
        }

        private void btnCubeT_Click(object sender, EventArgs e)
        {
            statusFiles = true;
            loadVertex();
            loadTList();
            setCubeStatus("TRIANGLES");
            GlControl1.Invalidate();
        }

        private void btnResetObjects_Click(object sender, EventArgs e)
        {
            setCubeStatus("OFF");
            GlControl1.Invalidate();
        }

        //-----------------------------------------------------------------------------------------
        //   ADMINISTRARE MOD 3D (PAINT)
        private void GlControl1_Paint(object sender, PaintEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            GL.ClearColor(Color.Black);

            Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView(camDepth, 4f / 3f, 1, 10000);
            Matrix4 lookat = Matrix4.LookAt(eyePosX, eyePosY, eyePosZ, 0, 0, 0, 0, 1, 0);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.LoadMatrix(ref perspective);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.LoadMatrix(ref lookat);

            GL.Viewport(0, 0, GlControl1.Width, GlControl1.Height);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            // Iluminare globală ON/OFF
            if (lightON)
                GL.Enable(EnableCap.Lighting);
            else
                GL.Disable(EnableCap.Lighting);

            // Aplicăm Light1 (clasa POO) – doar dacă există
            if (light1 != null)
                light1.Apply();

            // Configurăm Light0 (standard, cu controale de pe form)
            GL.Light(LightName.Light0, LightParameter.Ambient, valuesAmbient0);
            GL.Light(LightName.Light0, LightParameter.Diffuse, valuesDiffuse0);
            GL.Light(LightName.Light0, LightParameter.Specular, valuesSpecular0);
            GL.Light(LightName.Light0, LightParameter.Position, valuesPosition0);

            if (lightON && lightON_0)
                GL.Enable(EnableCap.Light0);
            else
                GL.Disable(EnableCap.Light0);

            // Rotații cu mouse-ul
            if (statusControlMouse2D)
                GL.Rotate(mousePos.X, 0, 1, 0);

            if (statusControlMouse3D)
                GL.Rotate(mousePos.X, 0, 1, 1);

            // Axe
            if (statusControlAxe)
                DeseneazaAxe();

            // Cub
            if (statusCube.ToUpper().Equals("QUADS"))
                DeseneazaCubQ();
            else if (statusCube.ToUpper().Equals("TRIANGLES"))
                DeseneazaCubT();

            GlControl1.SwapBuffers();
        }

        //-----------------------------------------------------------------------------------------
        //   DESENARE OBIECTE 3D
        private void DeseneazaAxe()
        {
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.Red);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(75, 0, 0);
            GL.End();

            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.Yellow);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 75, 0);
            GL.End();

            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.Green);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 0, 75);
            GL.End();
        }

        private void DeseneazaCubQ()
        {
            GL.Begin(PrimitiveType.Quads);
            for (int i = 0; i < nQuadsList; i++)
            {
                switch (i % 4)
                {
                    case 0: GL.Color3(Color.Blue); break;
                    case 1: GL.Color3(Color.Red); break;
                    case 2: GL.Color3(Color.Green); break;
                    case 3: GL.Color3(Color.Yellow); break;
                }
                int x = arrQuadsList[i];
                GL.Vertex3(arrVertex[x, 0], arrVertex[x, 1], arrVertex[x, 2]);
            }
            GL.End();
        }

        private void DeseneazaCubT()
        {
            GL.Begin(PrimitiveType.Triangles);
            for (int i = 0; i < nTrianglesList; i++)
            {
                switch (i % 3)
                {
                    case 0: GL.Color3(Color.Blue); break;
                    case 1: GL.Color3(Color.Red); break;
                    case 2: GL.Color3(Color.Green); break;
                }
                int x = arrTrianglesList[i];
                GL.Vertex3(arrVertex[x, 0], arrVertex[x, 1], arrVertex[x, 2]);
            }
            GL.End();
        }

        //-----------------------------------------------------------------------------------------
        //   CONTROL LIGHT1 CU TASTATURA – W A S D Q E
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (light1 != null)
            {
                switch (keyData)
                {
                    case Keys.W: light1.Position[1] += 5; break;
                    case Keys.S: light1.Position[1] -= 5; break;
                    case Keys.A: light1.Position[0] -= 5; break;
                    case Keys.D: light1.Position[0] += 5; break;
                    case Keys.Q: light1.Position[2] += 5; break;
                    case Keys.E: light1.Position[2] -= 5; break;
                }

                GlControl1.Invalidate();
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
