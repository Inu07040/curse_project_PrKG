using System;
using System.Drawing;
using System.Windows.Forms;
using Tao.DevIl;
using Tao.FreeGlut;
using Tao.OpenGl;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Drawing.Imaging;

namespace CourseProject_Ipatova_PRI118
{
    public partial class Form1 : Form
    {
        int imageId, imageId2;
        double a = 150;
        double deltaA;
        uint mGlTextureObject, mGlTextureObject2;
        float global_time = 0;
        private Explosion explosion = new Explosion(0,0,0, 200, 500);
        double angle, angleX, angleY, angleZ; 
        double translateX, translateY, translateZ;
        bool isExplosion = false;
        bool flagExplosion = false;
        double sizeX = 1;
        double sizeY = 1;
        double sizeZ = 1;
        double cameraSpeed = 10;
        String traffic_light = "green";
        int carX = 150;
        double distance;

        private double[] _lightFromPos = { 200, 5, 200 }; // позиция из которой испускается свет
        private double[] _lightToPos = { 100, 0, 0 }; // в какую позиция светит источник света
        private uint _shadowMap; // карта теней
        int _shadowMapSize = 512;

        float[] mv = new float[16]; // матрица вида при создании карты теней
        float[] pr = new float[16]; // матрица проекции при создании карты теней

        int mouseCoordX, mouseCoordY;
        uint fractalFern;
        
        private int _width;
        private int _height;
        // Bitmap для папоротника
        private Bitmap _fern;
        // используем для отрисовки на PictureBox
        private Graphics _graph;

        private void button1_Click(object sender, EventArgs e)
        {
            traffic_light = "green";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            traffic_light = "red";
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    angle = 10; angleX = -72; angleY = 0; angleZ = -50;
                    translateX = -292; translateY = 273; translateZ = -135;

                    break;
                case 1:
                    angle = 10; angleX = -82; angleY = 0; angleZ = -50;
                    translateX = -112; translateY = 123; translateZ = -45;

                    break;
                case 2:
                    angle = 10; angleX = -92; angleY = 0; angleZ = -90;
                    translateX = -252; translateY = 3; translateZ = -45;

                    break;
            }
            AnT.Focus();
        }

        private void AnT_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.D)
            {
                translateY -= cameraSpeed;

            }
            if (e.KeyCode == Keys.A)
            {
                translateY += cameraSpeed;
            }
            if (e.KeyCode == Keys.W)
            {
                translateX += cameraSpeed;
            }
            if (e.KeyCode == Keys.S)
            {
                translateX -= cameraSpeed;

            }
            if (e.KeyCode == Keys.ControlKey)
            {
                translateZ += cameraSpeed;

            }
            if (e.KeyCode == Keys.Space)
            {
                translateZ -= cameraSpeed;
            }


            if (e.KeyCode == Keys.Q)
            {
                angleZ -= angle;
            }
            if (e.KeyCode == Keys.E)
            {
                angleZ += angle;
            }

            if (e.KeyCode == Keys.R)
            {
                angleX -= angle;
            }
            if (e.KeyCode == Keys.F)
            {
                angleX += angle;
            }

            if (e.KeyCode == Keys.I && carX >= -150)
            {
                carX -= 2;
            }
            if (e.KeyCode == Keys.K && carX <= 150)
            {
                carX += 2;
            }

        }

        public Form1()
        {
            InitializeComponent();
            AnT.InitializeContexts();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_RGB | Glut.GLUT_DOUBLE);

            Il.ilInit();
            Il.ilEnable(Il.IL_ORIGIN_SET);

            Gl.glClearColor(255, 255, 255, 1);
            Gl.glClearStencil(0);

            Gl.glViewport(0, 0, AnT.Width, AnT.Height);

            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();

            Glu.gluPerspective(60, (float)AnT.Width / (float)AnT.Height, 0.1, 800);

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();

            Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glDepthFunc(Gl.GL_LESS);

            Gl.glHint(Gl.GL_POLYGON_SMOOTH_HINT, Gl.GL_NICEST);
            Gl.glHint(Gl.GL_PERSPECTIVE_CORRECTION_HINT, Gl.GL_NICEST);

            Gl.glGenTextures(1, out _shadowMap);
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, _shadowMap);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_CLAMP_TO_EDGE);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_CLAMP_TO_EDGE);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);

            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_COMPARE_MODE_ARB, Gl.GL_COMPARE_R_TO_TEXTURE_ARB);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_COMPARE_FUNC_ARB, Gl.GL_LEQUAL);

            Gl.glTexGeni(Gl.GL_S, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_EYE_LINEAR);
            Gl.glTexGeni(Gl.GL_T, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_EYE_LINEAR);
            Gl.glTexGeni(Gl.GL_R, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_EYE_LINEAR);
            Gl.glTexGeni(Gl.GL_Q, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_EYE_LINEAR);

            Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_DEPTH_COMPONENT, _shadowMapSize, _shadowMapSize, 0,
                Gl.GL_DEPTH_COMPONENT, Gl.GL_UNSIGNED_BYTE, null);


            

            ////// вычисляем коэффициент
            _width = 600;
            _height = 600;
            // создаем Bitmap для папоротника
            _fern = new Bitmap(_width, _height);
            // cоздаем новый объект Graphics из указанного Bitmap
            _graph = Graphics.FromImage(_fern);
            // устанавливаем фон
            _graph.Clear(Color.Transparent);


            DrawFern();


            Gl.glGenTextures(1, out fractalFern);

            Gl.glPixelStorei(Gl.GL_UNPACK_ALIGNMENT, 1);

            Gl.glBindTexture(Gl.GL_TEXTURE_2D, fractalFern);

            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_REPEAT);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_REPEAT);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
            Gl.glTexEnvf(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_MODULATE);


            BitmapData data = _fern.LockBits(new System.Drawing.Rectangle(0, 0, _width, _height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGBA, _width, _height, 0, Gl.GL_BGRA, Gl.GL_UNSIGNED_BYTE, data.Scan0);



            Il.ilGenImages(1, out imageId);
            Il.ilBindImage(imageId);
            if (Il.ilLoadImage("../../textures/background.png"))
            {
                int width = Il.ilGetInteger(Il.IL_IMAGE_WIDTH);
                int height = Il.ilGetInteger(Il.IL_IMAGE_HEIGHT);
                int bitspp = Il.ilGetInteger(Il.IL_IMAGE_BITS_PER_PIXEL);
                switch (bitspp)
                {
                    case 24:
                        mGlTextureObject = MakeGlTexture(Gl.GL_RGB, Il.ilGetData(), width, height);
                        break;
                    case 32:
                        mGlTextureObject = MakeGlTexture(Gl.GL_RGBA, Il.ilGetData(), width, height);
                        break;
                }
            }
            Il.ilDeleteImages(1, ref imageId);

            Il.ilGenImages(1, out imageId2);
            Il.ilBindImage(imageId2);
            if (Il.ilLoadImage("../../textures/pol.png"))
            {
                int width = Il.ilGetInteger(Il.IL_IMAGE_WIDTH);
                int height = Il.ilGetInteger(Il.IL_IMAGE_HEIGHT);
                int bitspp = Il.ilGetInteger(Il.IL_IMAGE_BITS_PER_PIXEL);
                switch (bitspp)
                {
                    case 24:
                        mGlTextureObject2 = MakeGlTexture(Gl.GL_RGB, Il.ilGetData(), width, height);
                        break;
                    case 32:
                        mGlTextureObject2 = MakeGlTexture(Gl.GL_RGBA, Il.ilGetData(), width, height);
                        break;
                }
            }
            Il.ilDeleteImages(1, ref imageId2);

            RenderTimer.Start();
        }

        private static uint MakeGlTexture(int Format, IntPtr pixels, int w, int h)
        {
            uint texObject;

            Gl.glGenTextures(1, out texObject);

            Gl.glPixelStorei(Gl.GL_UNPACK_ALIGNMENT, 1);

            Gl.glBindTexture(Gl.GL_TEXTURE_2D, texObject);

            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_REPEAT);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_REPEAT);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
            Gl.glTexEnvf(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_MODULATE);

            switch (Format)
            {
                case Gl.GL_RGB:
                    Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGB, w, h, 0, Gl.GL_RGB, Gl.GL_UNSIGNED_BYTE, pixels);
                    break;

                case Gl.GL_RGBA:
                    Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGBA, w, h, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, pixels);
                    break;
            }

            return texObject;
        }

        private void RenderTimer_Tick(object sender, EventArgs e)
        {
            global_time += (float)RenderTimer.Interval / 1000;
            Display();
        }

        private void Display()
        {
            RenderToShadowMap(); // отрисовываем карту теней

            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);

            // устанавливаем положение главной камеры
            Gl.glViewport(0, 0, AnT.Width, AnT.Height);
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();

            Glu.gluPerspective(45, AnT.Width / (float)AnT.Height, 0.1, 800);

            Gl.glMatrixMode(Gl.GL_MODELVIEW);

            Gl.glClearColor(255, 255, 255, 1);
            //Gl.glLoadIdentity();
            //Gl.glScaled(sizeX, sizeY, sizeZ);
            
            Gl.glPushMatrix();

            int camera = comboBox1.SelectedIndex;

            Gl.glRotated(angleX, 1, 0, 0); Gl.glRotated(angleY, 0, 1, 0); Gl.glRotated(angleZ, 0, 0, 1);
            Gl.glTranslated(translateX, translateY, translateZ);

            // устанавлвиаем карту теней
            Gl.glActiveTextureARB(Gl.GL_TEXTURE1_ARB);
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, _shadowMap);

            Gl.glEnable(Gl.GL_TEXTURE_2D);
            Gl.glEnable(Gl.GL_TEXTURE_GEN_S);
            Gl.glEnable(Gl.GL_TEXTURE_GEN_T);
            Gl.glEnable(Gl.GL_TEXTURE_GEN_R);
            Gl.glEnable(Gl.GL_TEXTURE_GEN_Q);

            Gl.glTexGenfv(Gl.GL_S, Gl.GL_EYE_PLANE, new[] { 1f, 0f, 0f, 0f });
            Gl.glTexGenfv(Gl.GL_T, Gl.GL_EYE_PLANE, new[] { 0f, 1f, 0f, 0f });
            Gl.glTexGenfv(Gl.GL_R, Gl.GL_EYE_PLANE, new[] { 0f, 0f, 1f, 0f });
            Gl.glTexGenfv(Gl.GL_Q, Gl.GL_EYE_PLANE, new[] { 0f, 0f, 0f, 1f });

            Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_MODULATE);

            // корректируем текстурные координаты
            Gl.glMatrixMode(Gl.GL_TEXTURE);
            Gl.glPushMatrix();

            Gl.glLoadIdentity();
            Gl.glTranslatef(0.5f, 0.5f, 0.5f);     // remap from [-1,1]^2 to [0,1]^2
            Gl.glScalef(0.5f, 0.5f, 0.5f);
            Gl.glMultMatrixf(pr);
            Gl.glMultMatrixf(mv);

            Gl.glActiveTextureARB(Gl.GL_TEXTURE0_ARB);

            Draw(); // отрисовываем сцену

            Gl.glActiveTextureARB(Gl.GL_TEXTURE1_ARB);
            Gl.glDisable(Gl.GL_TEXTURE_2D);
            Gl.glActiveTextureARB(Gl.GL_TEXTURE0_ARB);

            // рисуем источники света
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glPopMatrix();
            Gl.glPushMatrix();

            Gl.glTranslatef((float)_lightFromPos[0], (float)_lightFromPos[1], (float)_lightFromPos[2]);
            Gl.glActiveTextureARB(Gl.GL_TEXTURE0_ARB);
            Gl.glDisable(Gl.GL_TEXTURE_2D);
            Glut.glutSolidSphere(0.1f, 15, 15);
            Gl.glPopMatrix();

            Gl.glMatrixMode(Gl.GL_TEXTURE);
            Gl.glPopMatrix();

            Gl.glMatrixMode(Gl.GL_MODELVIEW);

            Gl.glFlush();

            AnT.Invalidate();
        }

        private void Draw()
        {

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            explosion.Calculate(global_time);
            Gl.glPushMatrix();
            if (isExplosion && !flagExplosion)
            {
                flagExplosion = true;
                explosion.SetNewPosition(carX, 0, 15);
                explosion.SetNewPower(900);
                explosion.Boooom(global_time, 500, 300);
            }

            /////////////////////
            // ДОРОГА   
            /////////////////////

            Gl.glPushMatrix();

            Gl.glColor3d(0, 0.391, 0);
            Gl.glBegin(Gl.GL_TRIANGLE_FAN);
            Gl.glVertex3d(250, 250, 0);
            Gl.glVertex3d(-250, 250, 0);
            Gl.glVertex3d(-250, -250, 0);
            Gl.glVertex3d(250, -250, 0);
            Gl.glEnd();

            Gl.glColor3f(0.502f, 0.502f, 0.502f);
            Gl.glBegin(Gl.GL_TRIANGLE_FAN);
            Gl.glVertex3d(25, 200, 1);
            Gl.glVertex3d(-25, 200, 1);
            Gl.glVertex3d(-25, -200, 1);
            Gl.glVertex3d(25, -200, 1);
            Gl.glEnd();

            Gl.glBegin(Gl.GL_TRIANGLE_FAN);
            Gl.glVertex3d(200, 25, 1);
            Gl.glVertex3d(-200, 25, 1);
            Gl.glVertex3d(-200, -25, 1);
            Gl.glVertex3d(200, -25, 1);
            Gl.glEnd();

            Gl.glPopMatrix();

            /////////////////////
            // ДОМА   
            /////////////////////

            //розовый
            Gl.glPushMatrix();
            Gl.glColor3f(1f, 0.412f, 0.706f);
            Gl.glTranslated(75, 75, 25);
            Glut.glutSolidCube(50);
            Gl.glColor3f(1f, 0.388f, 0.278f);
            Gl.glTranslated(0, 0, 21);
            Gl.glRotated(45, 0, 0, 1);
            Gl.glScaled(40, 40, 40);
            Glut.glutSolidOctahedron();
            Gl.glPopMatrix();

            //оранжевый
            Gl.glPushMatrix();
            Gl.glColor3f(1f, 0.412f, 0f);
            Gl.glTranslated(75, 150, 25);
            Glut.glutSolidCube(50);
            Gl.glColor3f(0.741f, 0.718f, 0.420f);
            Gl.glTranslated(0, 0, 21);
            Gl.glRotated(45, 0, 0, 1);
            Gl.glScaled(40, 40, 40);
            Glut.glutSolidOctahedron();
            Gl.glPopMatrix();

            //синий
            Gl.glPushMatrix();
            Gl.glColor3f(0.275f, 0.510f, 0.706f);
            Gl.glTranslated(-75, -150, 25);
            Glut.glutSolidCube(50);
            Gl.glColor3f(0.561f, 0.737f, 0.561f);
            Gl.glTranslated(0, 0, 21);
            Gl.glRotated(45, 0, 0, 1);
            Gl.glScaled(40, 40, 40);
            Glut.glutSolidOctahedron();
            Gl.glPopMatrix();

            //аквамарин
            Gl.glPushMatrix();
            Gl.glColor3f(0.498f, 1f, 0.831f);
            Gl.glTranslated(-150, -75, 25);
            Glut.glutSolidCube(50);
            Gl.glColor3f(0.282f, 0.239f, 0.545f);
            Gl.glTranslated(0, 0, 21);
            Gl.glRotated(45, 0, 0, 1);
            Gl.glScaled(40, 40, 40);
            Glut.glutSolidOctahedron();
            Gl.glPopMatrix();

            //////////////////////////
            /////МАШИНА управляемая
            //////////////////////////

            //                           x                     y          z
            distance = Math.Sqrt((carX-0)*(carX - 0) + (0 - a)* (0 - a) + 0);

            if (distance > 41 && isExplosion == false)
            {
                Gl.glPushMatrix();
                Gl.glTranslated(carX, 0, 15);

                Gl.glPushMatrix();
                Gl.glColor3f(0f, 0f, 0.502f);
                Glut.glutSolidCube(20);
                Gl.glTranslated(-20, 0, 0);
                Glut.glutSolidCube(20);
                Gl.glColor3f(0f, 0f, 0.570f);
                Gl.glTranslated(15, 0, 15);
                Glut.glutSolidCube(15);
                Gl.glColor3f(0.184f, 0.184f, 0.184f);
                Gl.glRotated(90, 1, 0, 0);
                Gl.glTranslated(7, -23, -13);
                Glut.glutSolidCylinder(5, 25, 16, 16);
                Gl.glTranslated(-23, 0, 0);
                Glut.glutSolidCylinder(5, 25, 16, 16);
                Gl.glPopMatrix();

                Gl.glPopMatrix();
            }
            else
            {
                isExplosion = true;
            }


            //////////////////////////
            /////МАШИНА НЕуправляемая
            //////////////////////////
            if (distance > 41 && isExplosion == false)
            {
                if (traffic_light == "green")
                {
                    Gl.glPushMatrix();
                    Gl.glTranslated(0, a, 15);

                    Gl.glPushMatrix();
                    Gl.glRotated(90, 0, 0, 1);
                    Gl.glColor3f(0f, 0f, 0.502f);
                    Gl.glTranslated(0, 0, 0);
                    Glut.glutSolidCube(20);
                    Gl.glTranslated(-20, 0, 0);
                    Glut.glutSolidCube(20);
                    Gl.glColor3f(0f, 0f, 0.570f);
                    Gl.glTranslated(15, 0, 15);
                    Glut.glutSolidCube(15);
                    Gl.glColor3f(0.184f, 0.184f, 0.184f);
                    Gl.glRotated(90, 1, 0, 0);
                    Gl.glTranslated(7, -23, -13);
                    Glut.glutSolidCylinder(5, 25, 16, 16);
                    Gl.glTranslated(-23, 0, 0);
                    Glut.glutSolidCylinder(5, 25, 16, 16);
                    Gl.glPopMatrix();

                    a += deltaA;
                    if (a == -150) deltaA = 1;
                    if (a == 150) deltaA = -1;
                    Gl.glPopMatrix();
                }
                else if (traffic_light == "red")
                {
                    Gl.glPushMatrix();
                    Gl.glTranslated(0, a, 15);

                    Gl.glPushMatrix();
                    Gl.glRotated(90, 0, 0, 1);
                    Gl.glColor3f(0f, 0f, 0.502f);
                    Gl.glTranslated(0, 0, 0);
                    Glut.glutSolidCube(20);
                    Gl.glTranslated(-20, 0, 0);
                    Glut.glutSolidCube(20);
                    Gl.glColor3f(0f, 0f, 0.570f);
                    Gl.glTranslated(15, 0, 15);
                    Glut.glutSolidCube(15);
                    Gl.glColor3f(0.184f, 0.184f, 0.184f);
                    Gl.glRotated(90, 1, 0, 0);
                    Gl.glTranslated(7, -23, -13);
                    Glut.glutSolidCylinder(5, 25, 16, 16);
                    Gl.glTranslated(-23, 0, 0);
                    Glut.glutSolidCylinder(5, 25, 16, 16);
                    Gl.glPopMatrix();

                    Gl.glPopMatrix();
                }
            }
            else
            {
                isExplosion = true;
            }

            /////////////////////////
            ///СВЕТОФОР
            ////////////////////////
            Gl.glPushMatrix();
            Gl.glTranslated(30, 30, 0);
            Gl.glColor3f(0.184f, 0.184f, 0.184f);
            Glut.glutSolidCylinder(2, 60, 16, 16);
            Gl.glTranslated(0, 0, 60);
            Glut.glutSolidCube(10);
            if (traffic_light == "green")
            {
                Gl.glColor3f(0f, 1f, 0f);
                Glut.glutSolidSphere(6, 16, 16);
            } 
            else if (traffic_light == "red")
            {
                Gl.glColor3f(1f, 0f, 0f);
                Glut.glutSolidSphere(6, 16, 16);
            }
            Gl.glPopMatrix();


            /////////////////////
            // ФОН
            /////////////////////

            Gl.glPushMatrix();
            Gl.glEnable(Gl.GL_TEXTURE_2D);
            Gl.glEnable(Gl.GL_BLEND);
            Gl.glEnable(Gl.GL_ALPHA_TEST);
            Gl.glAlphaFunc(Gl.GL_GEQUAL, 0.5f);
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, fractalFern);
            Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);

            Gl.glPushMatrix();
            Gl.glTranslated(-200, -200, 0);

            Gl.glBegin(Gl.GL_QUADS);
            Gl.glTexCoord2f(1, 0);
            Gl.glVertex3d(0, 0, 0);
            Gl.glTexCoord2f(0, 0);
            Gl.glVertex3d(0, 200, 0);
            Gl.glTexCoord2f(0, 1);
            Gl.glVertex3d(0, 200, 200);
            Gl.glTexCoord2f(1, 1);
            Gl.glVertex3d(0, 0, 200);
            Gl.glEnd();

            Gl.glBegin(Gl.GL_QUADS);
            Gl.glTexCoord2f(1, 0);
            Gl.glVertex3d(0, 200, 0);
            Gl.glTexCoord2f(0, 0);
            Gl.glVertex3d(0, 400, 0);
            Gl.glTexCoord2f(0, 1);
            Gl.glVertex3d(0, 400, 200);
            Gl.glTexCoord2f(1, 1);
            Gl.glVertex3d(0, 200, 200);
            Gl.glEnd();

            Gl.glBegin(Gl.GL_QUADS);
            Gl.glTexCoord2f(1, 0);
            Gl.glVertex3d(0, 400, 0);
            Gl.glTexCoord2f(0, 0);
            Gl.glVertex3d(200, 400, 0);
            Gl.glTexCoord2f(0, 1);
            Gl.glVertex3d(200, 400, 200);
            Gl.glTexCoord2f(1, 1);
            Gl.glVertex3d(0, 400, 200);
            Gl.glEnd();

            Gl.glBegin(Gl.GL_QUADS);
            Gl.glTexCoord2f(1, 0);
            Gl.glVertex3d(200, 400, 0);
            Gl.glTexCoord2f(0, 0);
            Gl.glVertex3d(400, 400, 0);
            Gl.glTexCoord2f(0, 1);
            Gl.glVertex3d(400, 400, 200);
            Gl.glTexCoord2f(1, 1);
            Gl.glVertex3d(200, 400, 200);
            Gl.glEnd();

            Gl.glBegin(Gl.GL_QUADS);
            Gl.glTexCoord2f(1, 0);
            Gl.glVertex3d(0, 0, 0);
            Gl.glTexCoord2f(0, 0);
            Gl.glVertex3d(400, 0, 0);
            Gl.glTexCoord2f(0, 1);
            Gl.glVertex3d(400, 0, 200);
            Gl.glTexCoord2f(1, 1);
            Gl.glVertex3d(0, 0, 200);
            Gl.glEnd();
            Gl.glPopMatrix();

            Gl.glDisable(Gl.GL_ALPHA_TEST);
            Gl.glDisable(Gl.GL_BLEND);
            Gl.glDisable(Gl.GL_TEXTURE_2D);

            Gl.glPopMatrix();

            Gl.glPopMatrix();


        }

        private void DrawFern()
        {

            var r = new Random();
            double x = 0;
            double y = 0;
            for (int count = 0; count < 100000; count++)
            {
                int roll = r.Next(100);
                double xp = x;
                if (roll < 1)
                {
                    x = 0;
                    y = 0.16 * y;
                }
                else if (roll < 86)
                {
                    x = 0.85 * x + 0.04 * y;
                    y = -0.04 * xp + 0.85 * y + 1.6;
                }
                else if (roll < 93)
                {
                    x = 0.2 * x - 0.26 * y;
                    y = 0.23 * xp + 0.22 * y + 1.6;
                }
                else
                {
                    x = -0.15 * x + 0.28 * y;
                    y = 0.26 * xp + 0.24 * y + 0.44;
                }
                _fern.SetPixel((int)(300 + 58 * x), (int)(58 * y), Color.ForestGreen);
            }
        }

        private void RenderToShadowMap() // создаем карту теней
        {
            Gl.glColorMask(Gl.GL_FALSE, Gl.GL_FALSE, Gl.GL_FALSE, Gl.GL_FALSE);
            Gl.glDisable(Gl.GL_TEXTURE_2D);

            Gl.glEnable(Gl.GL_POLYGON_OFFSET_FILL);
            Gl.glPolygonOffset(4, 4);

            // устанавливаем проекцию истоника света
            Gl.glViewport(0, 0, _shadowMapSize, _shadowMapSize);
            Gl.glClear(Gl.GL_DEPTH_BUFFER_BIT);

            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();

            Glu.gluPerspective(120, 1, 0.1, 500);
            Glu.gluLookAt(_lightFromPos[0], _lightFromPos[1], _lightFromPos[2],        // eye
            _lightToPos[0], _lightToPos[1], _lightToPos[2], // center
                0, 1, 0);                     // up

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();

            // получаем матрицы вида и проекции
            Gl.glGetFloatv(Gl.GL_MODELVIEW_MATRIX, mv);
            Gl.glGetFloatv(Gl.GL_PROJECTION_MATRIX, pr);

            // отрисовываем сцену с точки зрения источника света
            Draw();

            // копируем текстуру глубины в карту теней
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, _shadowMap);
            Gl.glCopyTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_DEPTH_COMPONENT, 0, 0, _shadowMapSize, _shadowMapSize, 0);

            // возвращаем состояние для обычного отображения
            Gl.glDisable(Gl.GL_POLYGON_OFFSET_FILL);
            Gl.glColorMask(Gl.GL_TRUE, Gl.GL_TRUE, Gl.GL_TRUE, Gl.GL_TRUE);
            Gl.glEnable(Gl.GL_TEXTURE_2D);
        }
    }
}
