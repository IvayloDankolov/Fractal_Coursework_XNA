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
    // Класът на програмата, който прави всичко. 
    // НЕ спазваме много великата OOP идея, но на кой му пука.
    public class Main : Microsoft.Xna.Framework.Game
    {

        // Елементи на графичната среда, не са важни за нашия фрактал.
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
        // Програмата за оцветяване (shader), която всъщност извършва пресмятането.
        Effect marcher;
        
        // Фиктивният "екран", върху който ще се проектира сцената
        Screen scr;

        // Това не е типична проекционна матрица (защото и подходът ни не е типичен). 
        // Тя съдържа трансформацията, която ще позиционира гореспоменатият екран в пространството
        Matrix View;


        // Позиция и посока на камерата, по Look-at системата
        Vector3 CameraPos;
        Vector3 CameraDir;
        Vector3 CameraUp;

        // Две ротации наляво/надясно и нагоре/надолу
        float leftRot =0;
        float upRot = 0;

        // Скорост на движение из сцената. Влияе се от мащаба (scale).
        float MoveSpeed = 3;

        // Скорост на въртене на камерата. Не се влияе от мащаба.
        float rotationSpeed = 0.1f;

        // Мащаб на екрана. Във 2D случая това ще промени приближението, а в 3D ъгълът на виждане (field of view)
        float scale = 1;

        // Предишни състояния на мишката и скролера, които ще ни помогнат за да сметнем какво е направил потребителя между 
        // 2 периода на опресняване
        private MouseState originalMouseState;
        private int originalScroll = 0;

        // Степента на полинома, който итерираме. 1 всъщност е фиктивна степен и казва на програмата да превключи от 3D във 2D
        int pow = 1;

        
        // По-лесен начин да се обръщаме към размерите на екрана.
        public int Width { get { return GraphicsDevice.Viewport.Width; } }
        public int Height { get { return GraphicsDevice.Viewport.Height; } }


        // Конструктор на програмата. Грижи се за създаване на някои важни неща като графичният мениджър.
        public Main()
        {
            graphics = new GraphicsDeviceManager(this);

            Window.AllowUserResizing = true;

            Content.RootDirectory = "Content";
            scr = new Screen();
        }

        // Вика се преди показването на прозореца.
        // Ние го използваме, за да оправим първоначално камерата.
        protected override void Initialize()
        {
            CameraPos = new Vector3(0, 0, -5);
            CameraDir = new Vector3(0, 0, 1);
            CameraUp = new Vector3(0, 1, 0);

            UpdateView();

            base.Initialize();
        }


        // Вика се след като имаме готово графично устройство, за да заредим модели, текстури, шейдъри и т.н.
        // При нас е важно само да центрираме мишката и да заредим шейдъра "raytracer", който ще прави сметките
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Първоначалното центриране е необходимо, за да не подскочи камерата.
            Mouse.SetPosition(Width / 2, Height / 2);
            originalMouseState = Mouse.GetState();

            marcher = Content.Load<Effect>("raytracer");

        }

        // Преизчислява матрицата, която позиционира екрана и вектора за посока на камерата
        void UpdateView()
        {
            // В тази матрица има само две ротации след мащабиране. Транслацията
            // ще я правим отделно, и без това имаме в шейдъра позицията на камерата
            Matrix rot = Matrix.CreateRotationX(upRot) * Matrix.CreateRotationY(leftRot);
            View = rot * Matrix.CreateScale(scale);
        
            // Каноничната посока на камерата е да гледа към положителната част на остта Z
            // Само трябва да я завъртим правилно, за да получим текущата посока
            CameraDir = Vector3.Transform(new Vector3(0, 0, 1f), rot);

        }

        // Добавяме вектор към позицията на камерата
        // Метода е необходим, защото първо трябва да разграничим между 2D и 3D случаите
        // и второ, защото ляво/дясно, напред/назад и т.н. са относителни спрямо ориентацията
        // на екрана и трябва да вземем това в предвид.
        private void AddToCameraPosition(Vector3 vectorToAdd)
        {
            if (pow > 1)
            {
                // Умножавайки вектора по View не само ще го докараме до правилна ориентация,
                // ами и го мащабираме подходящо спрямо scale. С един куршум два заека.
                Vector3 rotatedVector = Vector3.Transform(vectorToAdd, View);
                CameraPos += MoveSpeed * rotatedVector;
            }
            else
            {
                // 2D случаят не е интересен.
                Vector3 actual = new Vector3(vectorToAdd.X, vectorToAdd.Z, 0);
                CameraPos += MoveSpeed * scale * actual;
            }
            
        }

        // Занимава се с обработване на съобщенията от мишката, като върти
        // камерата и мащабира при необходимост.
        private void HandleMouse(float amount)
        {
            MouseState currentMouseState = Mouse.GetState();
            // Вършим работа само, ако състоянието се е променило
            if (currentMouseState != originalMouseState)
            {
                // Формулата за ротациите е пределно проста.
                // Разликата в координатите на мишката, по скоростта на въртене, по времето от последното
                // опресняване до сега. Престо!
                float xDifference = currentMouseState.X - originalMouseState.X;
                float yDifference = currentMouseState.Y - originalMouseState.Y;
                leftRot += rotationSpeed * xDifference * amount;
                upRot += rotationSpeed * yDifference * amount;
                Mouse.SetPosition(Width / 2, Height / 2);
                

                int scroll = currentMouseState.ScrollWheelValue - originalScroll;

                originalScroll = currentMouseState.ScrollWheelValue;

                // За мащаба пак е подобна идеята, обаче ...
                // Бихме желали да се приближаваме с геометрична прогресия, а не линейно, иначе е много
                // лесно да подминем детайлите, които искаме да видим.
                // Все пак, фракталите имат детайли на всяко ниво на приближение.
                scale *= (float)Math.Pow(1.001, -(double)scroll);


                UpdateView();
            }
        }


        // Занимава се с обработка на съобщенията от клавиатурата.
        // Това го ползваме за движение из сцената и смяна на фракталите.
        private void HandleKeyboard(float amount)
        {
            KeyboardState keyState = Keyboard.GetState();

            // Тези редове по-долу са абсолютно глупави, просто ме мързеше да го правя красиво
            // и за това копирах кода за клавиша "1" 10 пъти.
            // Виновен ! (това всъщност е повече код, отколкото за рисуване на фрактала, ха!)

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

            //Тук пресмятаме накъде ще се движим.
            
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

            // И най-накрая прибавяме това към позицията, стига да има смисъл
            if(moveVector != Vector3.Zero)
                AddToCameraPosition(moveVector * amount);
        }


        // Този метод отговаря за обновяването на всичко, което не е свързано с рисуване по екрана.
        // Физика, таймери, събития, и т.н.
        // Нашата програма е проста и трябва само да погледнем какво се е случило с мишката и клавиатурата
        protected override void Update(GameTime gameTime)
        {
            // Позволява ни да спрем програмата с Escape клавиша
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            // Смятаме разликата от последното викане на Update. Това е важно, защото няма гаранция, че тези викания,
            // стават равномерно, 60 пъти в секунда. Ако не отчетем това, движенията ни ще са много накъсани при по-тежките фрактали
            float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;

            //Викаме функциите които отговарят за мишката и клавиатурата.
            HandleMouse(timeDifference);
            HandleKeyboard(timeDifference);

            base.Update(gameTime);
        }


        // И ключовото нещо в нашата програма, поне от C# страната, метода който отговаря за рисуването по екрана.
        protected override void Draw(GameTime gameTime)
        {
            // Изчиства последния кадър. Цветът отдолу не е от значение
            GraphicsDevice.Clear(Color.Red);

            // Подаваме някои важни за шейдъра неща, като матриците, позиция на камерата,
            // брой итерации, степен на полинома и мащаб.
            marcher.Parameters["View"].SetValue(View);
            marcher.Parameters["camPos"].SetValue(CameraPos);
            marcher.Parameters["camDir"].SetValue(CameraDir);
            marcher.Parameters["Iterations"].SetValue( (pow == 1) ? 255 : 1024);
            marcher.Parameters["MarchSteps"].SetValue(255);
            marcher.Parameters["Power"].SetValue(pow);
            marcher.Parameters["Bailout"].SetValue(200);
            marcher.Parameters["Scale"].SetValue(scale);

            // Избираме техника за изобразяване, 2D или 3D в нашия случай
            if(pow > 1)
                marcher.CurrentTechnique = marcher.Techniques["Raymarch"];
            else
                marcher.CurrentTechnique = marcher.Techniques["Iterate"];

            // Прилагаме всички етапи на съответната техника и рисуваме екрана с тях
            // В случая и двете имат само по един етап, но това не е от особено значение.
            foreach( var pass in marcher.CurrentTechnique.Passes)
            {
                pass.Apply();
               
                scr.Draw(GraphicsDevice);
            }

            base.Draw(gameTime);
        }
    }
}
