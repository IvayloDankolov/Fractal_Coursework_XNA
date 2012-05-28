using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace FractalView
{
    // ������ �� ����������, ����� ����� ������. 
    // �� �������� ����� �������� OOP ����, �� �� ��� �� ����.
    public class Main : Microsoft.Xna.Framework.Game
    {

        // �������� �� ���������� �����, �� �� ����� �� ����� �������.
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
        // ���������� �� ���������� (shader), ����� �������� �������� ������������.
        Effect marcher;
        
        // ���������� "�����", ����� ����� �� �� ��������� �������
        Screen scr;

        // ���� �� � ������� ����������� ������� (������ � �������� �� �� � �������). 
        // �� ������� ���������������, ����� �� ����������� ��������������� ����� � ��������������
        Matrix View;


        // ������� � ������ �� ��������, �� Look-at ���������
        Vector3 CameraPos;
        Vector3 CameraDir;
        Vector3 CameraUp;

        // ��� ������� ������/������� � ������/������
        float leftRot =0;
        float upRot = 0;

        // ������� �� �������� �� �������. ����� �� �� ������ (scale).
        float MoveSpeed = 3;

        // ������� �� ������� �� ��������. �� �� ����� �� ������.
        float rotationSpeed = 0.1f;

        // ����� �� ������. ��� 2D ������ ���� �� ������� �������������, � � 3D ������ �� ������� (field of view)
        float scale = 1;

        // �������� ��������� �� ������� � ��������, ����� �� �� �������� �� �� ������� ����� � �������� ����������� ����� 
        // 2 ������� �� �����������
        private MouseState originalMouseState;
        private int originalScroll = 0;

        // �������� �� ��������, ����� ���������. 1 �������� � �������� ������ � ����� �� ���������� �� ��������� �� 3D ��� 2D
        int pow = 1;

        
        // ��-����� ����� �� �� �������� ��� ��������� �� ������.
        public int Width { get { return GraphicsDevice.Viewport.Width; } }
        public int Height { get { return GraphicsDevice.Viewport.Height; } }


        // ����������� �� ����������. ����� �� �� ��������� �� ����� ����� ���� ���� ���������� ��������.
        public Main()
        {
            graphics = new GraphicsDeviceManager(this);

            Window.AllowUserResizing = true;

            Content.RootDirectory = "Content";
            scr = new Screen();
        }

        // ���� �� ����� ����������� �� ���������.
        // ��� �� ����������, �� �� ������� ������������ ��������.
        protected override void Initialize()
        {
            CameraPos = new Vector3(0, 0, -5);
            CameraDir = new Vector3(0, 0, 1);
            CameraUp = new Vector3(0, 1, 0);

            UpdateView();

            base.Initialize();
        }


        // ���� �� ���� ���� ����� ������ �������� ����������, �� �� ������� ������, ��������, ������� � �.�.
        // ��� ��� � ����� ���� �� ���������� ������� � �� ������� ������� "raytracer", ����� �� ����� ��������
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //�������������� ���������� � ����������, �� �� �� �������� ��������.
            Mouse.SetPosition(Width / 2, Height / 2);
            originalMouseState = Mouse.GetState();

            marcher = Content.Load<Effect>("raytracer");

        }

        // ������������ ���������, ����� ����������� ������ � ������� �� ������ �� ��������
        void UpdateView()
        {
            // � ���� ������� ��� ���� ��� ������� ���� ����������. ������������
            // �� � ������ �������, � ��� ���� ����� � ������� ��������� �� ��������
            Matrix rot = Matrix.CreateRotationX(upRot) * Matrix.CreateRotationY(leftRot);
            View = rot * Matrix.CreateScale(scale);
        
            // ����������� ������ �� �������� � �� ����� ��� ������������� ���� �� ����� Z
            // ���� ������ �� � �������� ��������, �� �� ������� �������� ������
            CameraDir = Vector3.Transform(new Vector3(0, 0, 1f), rot);

        }

        // �������� ������ ��� ��������� �� ��������
        // ������ � ���������, ������ ����� ������ �� ����������� ����� 2D � 3D ��������
        // � �����, ������ ����/�����, ������/����� � �.�. �� ����������� ������ ������������
        // �� ������ � ������ �� ������ ���� � �������.
        private void AddToCameraPosition(Vector3 vectorToAdd)
        {
            if (pow > 1)
            {
                // ����������� ������� �� View �� ���� �� �� �������� �� �������� ����������,
                // ��� � �� ���������� ��������� ������ scale. � ���� ������ ��� �����.
                Vector3 rotatedVector = Vector3.Transform(vectorToAdd, View);
                CameraPos += MoveSpeed * rotatedVector;
            }
            else
            {
                // 2D ������� �� � ���������.
                Vector3 actual = new Vector3(vectorToAdd.X, vectorToAdd.Z, 0);
                CameraPos += MoveSpeed * scale * actual;
            }
            
        }

        // �������� �� � ����������� �� ����������� �� �������, ���� �����
        // �������� � �������� ��� ������������.
        private void HandleMouse(float amount)
        {
            MouseState currentMouseState = Mouse.GetState();
            // ������ ������ ����, ��� ����������� �� � ���������
            if (currentMouseState != originalMouseState)
            {
                // ��������� �� ��������� � �������� ������.
                // ��������� � ������������ �� �������, �� ��������� �� �������, �� ������� �� ����������
                // ����������� �� ����. ������!
                float xDifference = currentMouseState.X - originalMouseState.X;
                float yDifference = currentMouseState.Y - originalMouseState.Y;
                leftRot += rotationSpeed * xDifference * amount;
                upRot += rotationSpeed * yDifference * amount;
                Mouse.SetPosition(Width / 2, Height / 2);
                

                int scroll = currentMouseState.ScrollWheelValue - originalScroll;

                originalScroll = currentMouseState.ScrollWheelValue;

                // �� ������ ��� � ������� ������, ����� ...
                // ����� ������ �� �� ������������ � ����������� ���������, � �� �������, ����� � �����
                // ����� �� �������� ���������, ����� ������ �� �����.
                // ��� ���, ���������� ���� ������� �� ����� ���� �� �����������.
                scale *= (float)Math.Pow(1.001, -(double)scroll);


                UpdateView();
            }
        }


        // �������� �� � ��������� �� ����������� �� ������������.
        // ���� �� �������� �� �������� �� ������� � ����� �� ����������.
        private void HandleKeyboard(float amount)
        {
            KeyboardState keyState = Keyboard.GetState();

            // ���� ������ ��-���� �� ��������� �������, ������ �� ������� �� �� ����� �������
            // � �� ���� ������� ���� �� ������� "1" 10 ����.
            // ������� ! (���� �������� � ������ ���, ��������� �� �������� �� ��������, ��!)

            if (keyState.IsKeyDown(Keys.NumPad1))
            {
                pow = 1;
                CameraPos = new Vector3(0, 0, -5);
                scale = 3;
                upRot = 0;
                leftRot = 0;
                UpdateView();
            }
            else if (keyState.IsKeyDown(Keys.NumPad2))
            {
                pow = 2;
                CameraPos = new Vector3(0, 0, -5);
                scale = 1;
                upRot = 0;
                leftRot = 0;
                UpdateView();
            }
            else if (keyState.IsKeyDown(Keys.NumPad3))
            {
                pow = 3;
                CameraPos = new Vector3(0, 0, -5);
                scale = 1;
                upRot = 0;
                leftRot = 0;
                UpdateView();
            }
            else if (keyState.IsKeyDown(Keys.NumPad4))
            {
                pow = 4;
                CameraPos = new Vector3(0, 0, -5);
                scale = 1;
                upRot = 0;
                leftRot = 0;
                UpdateView();
            }
            else if (keyState.IsKeyDown(Keys.NumPad5))
            {
                pow = 5;
                CameraPos = new Vector3(0, 0, -5);
                scale = 1;
                upRot = 0;
                leftRot = 0;
                UpdateView();
            }
            else if (keyState.IsKeyDown(Keys.NumPad6))
            {
                pow = 6;
                CameraPos = new Vector3(0, 0, -5);
                scale = 1;
                upRot = 0;
                leftRot = 0;
                UpdateView();
            }
            else if (keyState.IsKeyDown(Keys.NumPad7))
            {
                pow = 7;
                CameraPos = new Vector3(0, 0, -5);
                scale = 1;
                upRot = 0;
                leftRot = 0;
                UpdateView();
            }
            else if (keyState.IsKeyDown(Keys.NumPad8))
            {
                pow = 8;
                CameraPos = new Vector3(0, 0, -5);
                scale = 1;
                upRot = 0;
                leftRot = 0;
                UpdateView();
            }
            else if (keyState.IsKeyDown(Keys.NumPad9))
            {
                pow = 9;
                CameraPos = new Vector3(0, 0, -5);
                scale = 1;
                upRot = 0;
                leftRot = 0;
                UpdateView();
            }

            Vector3 moveVector = new Vector3(0, 0, 0);

            //��� ���������� ������ �� �� ������.
            
            if (keyState.IsKeyDown(Keys.Up) || keyState.IsKeyDown(Keys.W))
                moveVector += new Vector3(0, 0, 1);
            if (keyState.IsKeyDown(Keys.Down) || keyState.IsKeyDown(Keys.S))
                moveVector += new Vector3(0, 0, -1);
            if (keyState.IsKeyDown(Keys.Right) || keyState.IsKeyDown(Keys.D))
                moveVector += new Vector3(1, 0, 0);
            if (keyState.IsKeyDown(Keys.Left) || keyState.IsKeyDown(Keys.A))
                moveVector += new Vector3(-1, 0, 0);
            if (keyState.IsKeyDown(Keys.Q))
                moveVector += new Vector3(0, 1, 0);
            if (keyState.IsKeyDown(Keys.Z))
                moveVector += new Vector3(0, -1, 0);

            // � ���-������ ��������� ���� ��� ���������, ����� �� ��� ������
            if(moveVector != Vector3.Zero)
                AddToCameraPosition(moveVector * amount);
        }


        // ���� ����� �������� �� ������������ �� ������, ����� �� � �������� � �������� �� ������.
        // ������, �������, �������, � �.�.
        // ������ �������� � ������ � ������ ���� �� ��������� ����� �� � ������� � ������� � ������������
        protected override void Update(GameTime gameTime)
        {
            // ��������� �� �� ����� ���������� � Escape �������
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            // ������� ��������� �� ���������� ������ �� Update. ���� � �����, ������ ���� ��������, �� ���� �������,
            // ������ ����������, 60 ���� � �������. ��� �� ������� ����, ���������� �� �� �� ����� �������� ��� ��-������� ��������
            float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;

            //������ ��������� ����� ��������� �� ������� � ������������.
            HandleMouse(timeDifference);
            HandleKeyboard(timeDifference);

            base.Update(gameTime);
        }


        // � ��������� ���� � ������ ��������, ���� �� C# ��������, ������ ����� �������� �� ���������� �� ������.
        protected override void Draw(GameTime gameTime)
        {
            // �������� ��������� �����. ������ ������ �� � �� ��������
            GraphicsDevice.Clear(Color.Red);

            // �������� ����� ����� �� ������� ����, ���� ���������, ������� �� ��������,
            // ���� ��������, ������ �� �������� � �����.
            marcher.Parameters["View"].SetValue(View);
            marcher.Parameters["camPos"].SetValue(CameraPos);
            marcher.Parameters["camDir"].SetValue(CameraDir);
            marcher.Parameters["Iterations"].SetValue( (pow == 1) ? 255 : 1024);
            marcher.Parameters["MarchSteps"].SetValue(255);
            marcher.Parameters["Power"].SetValue(pow);
            marcher.Parameters["Bailout"].SetValue(200);
            marcher.Parameters["Scale"].SetValue(scale);

            // �������� ������� �� ������������, 2D ��� 3D � ����� ������
            if(pow > 1)
                marcher.CurrentTechnique = marcher.Techniques["Raymarch"];
            else
                marcher.CurrentTechnique = marcher.Techniques["Iterate"];

            // ��������� ������ ����� �� ����������� ������� � �������� ������ � ���
            // � ������ � ����� ���� ���� �� ���� ����, �� ���� �� � �� ������� ��������.
            foreach( var pass in marcher.CurrentTechnique.Passes)
            {
                pass.Apply();
               
                scr.Draw(GraphicsDevice);
            }

            base.Draw(gameTime);
        }
    }
}
