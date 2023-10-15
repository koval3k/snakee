using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Threading;

namespace SnakeGame
{
    class Program
    {
        static readonly int x = 60;
        static readonly int y = 26;

        static Walls walls;
        static Snake snake;
        static FoodFactory foodFactory;
        static System.Timers.Timer time;

        static int score = 3;

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.SetWindowSize(x + 1, y + 1);
            Console.SetBufferSize(x + 1, y + 1);
            Console.CursorVisible = false;

            bool exit = false;

            while (!exit)
            {
                Console.Clear();

                Console.WriteLine("Главное меню:");
                Console.WriteLine("1. Как играть?");
                Console.WriteLine("2. Информация");
                Console.WriteLine("3. Начать игру");
                Console.WriteLine("4. Выход");

                ConsoleKeyInfo keyInfo = Console.ReadKey();

                switch (keyInfo.Key)
                {
                    case ConsoleKey.D1:
                        Console.Clear();
                        Console.WriteLine("Управление: ←, →, ↑, ↓");
                        Console.WriteLine("Ваша цель - съесть как можно больше еды, не задевая стены или саму себя.");
                        Console.WriteLine("Нажмите любую клавишу, чтобы продолжить...");
                        Console.ReadKey();
                        break;

                    case ConsoleKey.D2:
                        Console.Clear();
                        Console.WriteLine("Змейка (Snake Game) - это жанр видеоигр,");
                        Console.WriteLine("где игрок управляет точкой, квадратом или объектом на плоскости с четкими границами.");
                        Console.WriteLine("Нажмите любую клавишу, чтобы продолжить...");
                        Console.ReadKey();
                        break;

                    case ConsoleKey.D3:
                        Console.Clear();
                        StartGame();
                        break;

                    case ConsoleKey.D4:
                        exit = true;
                        break;

                    default:
                        Console.Clear();
                        Console.WriteLine("Неизвестный выбор. Нажмите любую клавишу, чтобы продолжить...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        static void StartGame()
        {
            walls = new Walls(x, y, '■');
            snake = new Snake(x / 2, y / 2, 3);
            foodFactory = new FoodFactory(x, y, '☺');
            foodFactory.CreateFood();

            time = new System.Timers.Timer(200); // 200 миллисекунд
            time.Elapsed += Loop; // Подписываемся на событие Elapsed
            time.AutoReset = true;
            time.Enabled = true;


            MainGameLoop();
        }

        static void MainGameLoop()
        {
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    snake.Rotation(key.Key);
                }

                snake.Move();

                if (snake.Eat(foodFactory.food))
                {
                    foodFactory.CreateFood();
                    score++;
                }
                else if (walls.IsHit(snake.GetHead()) || snake.IsHit(snake.GetHead()))
                {
                    time.Dispose(); // Остановка таймера
                    Console.Clear();
                }
                {
                    time.Change(Timeout.Infinite, Timeout.Infinite);
                    Console.Clear();

                    Console.SetCursorPosition(x / 2 - 5, y / 2);
                    Console.Write("Игра окончена!");
                    Console.SetCursorPosition(x / 2 - 10, y / 2 + 1);
                    Console.Write("Ваш счет: " + (snake.GetLength() - 3));

                    Console.SetCursorPosition(x / 2 - 30, y / 2 + 2);
                    Console.WriteLine("1. Вернуться в главное меню");
                    Console.WriteLine("2. Начать игру заново");

                    Console.Write("Выберите опцию (Для выбора нажмите ENTER и 1 или 2): ");

                    bool exit = false;
                    int choice;

                    while (!exit)
                    {
                        string input = Console.ReadLine();

                        if (int.TryParse(input, out choice))
                        {
                            if (choice == 1)
                            {
                                Console.Clear();
                                Main(); // Возврат в главное меню
                            }
                            else if (choice == 2)
                            {
                                Console.Clear();
                                StartGame();
                            }
                        }
                    }
                }
            }
        }



        // структура, що представляє точку на екрані
        struct Point //використовується для представлення позицій стін, змійки і їжі на грі
        {
            public int x { get; set; }
            public int y { get; set; }
            public char ch { get; set; }

            //перетворення n на точку
            public static implicit operator Point((int, int, char) value) =>
                  new Point { x = value.Item1, y = value.Item2, ch = value.Item3 };

            // Пeрeвіркa на рівність точок
            public static bool operator ==(Point a, Point b) =>
                    (a.x == b.x && a.y == b.y) ? true : false;
            public static bool operator !=(Point a, Point b) =>
                    (a.x != b.x || a.y != b.y) ? true : false;

            // Мaлює точку на екрaні
            public void Draw()
            {
                DrawPoint(ch);
            }

            // oчищує точку на екрані
            public void Clear()
            {
                DrawPoint(' ');
            }

            // Мaлює символ на задaній позиції
            private void DrawPoint(char _ch)
            {
                Console.SetCursorPosition(x, y);
                Console.Write(_ch);
            }
        }

        class Walls //стіни
        {
            private char ch;
            private List<Point> wall = new List<Point>();

            //створює стіни
            public Walls(int x, int y, char ch)
            {
                this.ch = ch;

                DrawHorizontal(x, 0);
                DrawHorizontal(x, y);
                DrawVertical(0, y);
                DrawVertical(x, y);
            }

            // горизонтальнa стінa
            private void DrawHorizontal(int x, int y)
            {
                for (int i = 0; i < x; i++)
                {
                    Point p = (i, y, ch);
                    p.Draw();
                    wall.Add(p);
                }
            }

            //вертикальнa стінa
            private void DrawVertical(int x, int y)
            {
                for (int i = 0; i < y; i++)
                {
                    Point p = (x, i, ch);
                    p.Draw();
                    wall.Add(p);
                }
            }

            // Перевіряє, чи точка зіткнулася із стіною
            public bool IsHit(Point p)
            {
                foreach (var w in wall)
                {
                    if (p == w)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        enum Direction //можливі напрямки руху для змійки 
        {
            LEFT,
            RIGHT,
            UP,
            DOWN
        }

        //сама змійка і її логіка
        class Snake //осн логікa змійки.
        {
            //змійка представлена списком точок, де кожна точка це одна частина тіла змійки
            //змійка може рухатися вліво, вправо, вгору або вниз, і вона може з'їдати їжу, збільшуючи свою довжину
            //відстежується можливість зіткнення змійки зі стіною або самою собою
            private List<Point> snake;

            private Direction direction;
            private int step = 1;
            private Point tail;
            private Point head;

            bool rotate = true;

            public Snake(int x, int y, int length)//створення змійкки
            {
                direction = Direction.RIGHT;

                snake = new List<Point>();
                for (int i = x - length; i < x; i++)
                {
                    Point p = (i, y, '0');
                    snake.Add(p);

                    p.Draw();
                }
            }

            public Point GetHead() => snake.Last(); //повертання голови

            public void Move() //рух
            {
                head = GetNextPoint();
                snake.Add(head);

                tail = snake.First();
                snake.Remove(tail);

                tail.Clear();
                head.Draw();

                rotate = true;
            }

            public bool Eat(Point p) //чи з'їла їжу
            {
                head = GetNextPoint();
                if (head == p)
                {
                    snake.Add(head);
                    head.Draw();
                    return true;
                }
                return false;
            }

            public Point GetNextPoint() // oбчислює наступну позицію голови змійки залежно від напрямку
            {
                Point p = GetHead();

                switch (direction)
                {
                    case Direction.LEFT:
                        p.x -= step;
                        break;
                    case Direction.RIGHT:
                        p.x += step;
                        break;
                    case Direction.UP:
                        p.y -= step;
                        break;
                    case Direction.DOWN:
                        p.y += step;
                        break;
                }
                return p;
            }

            public void Rotation(ConsoleKey key)   // Обробляє ввод користувача для зміни напрямку руху
            {
                if (rotate)
                {
                    switch (direction)
                    {
                        case Direction.LEFT:
                        case Direction.RIGHT:
                            if (key == ConsoleKey.DownArrow)
                                direction = Direction.DOWN;
                            else if (key == ConsoleKey.UpArrow)
                                direction = Direction.UP;
                            break;
                        case Direction.UP:
                        case Direction.DOWN:
                            if (key == ConsoleKey.LeftArrow)
                                direction = Direction.LEFT;
                            else if (key == ConsoleKey.RightArrow)
                                direction = Direction.RIGHT;
                            break;
                    }
                    rotate = false;
                }
            }

            public bool IsHit(Point p) //чи змійка зіткнулася з точкою
            {
                for (int i = snake.Count - 2; i > 0; i--)
                {
                    if (snake[i] == p)
                    {
                        return true;
                    }
                }
                return false;
            }

            public int GetLength()// Повертає довжину змійки
            {
                return snake.Count;
            }
        }

        class FoodFactory //food
        {
            int x;
            int y;
            char ch;
            public Point food { get; private set; }

            Random random = new Random();

            public FoodFactory(int x, int y, char ch) //ініціалізує об'єкт їжі
            {
                this.x = x;
                this.y = y;
                this.ch = ch;
            }

            public void CreateFood()// Створює новий об'єкт їжі на випадковій позиції
            {
                food = (random.Next(2, x - 2), random.Next(2, y - 2), ch);
                food.Draw();
            }
        }
    }
}
