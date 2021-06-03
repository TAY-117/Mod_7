using System;

namespace Mod_7.Moven
{
	struct Location	// месторасположение
	{
		Location(int x, int y)
		{
			X = x;
			Y = y;
		}

		internal int X // координата x не может быть за границами поля
		{ set; get; }

		internal int Y // координата y не может быть за границами поля
		{ set; get; }

	}

	//-----------------------------------------------------------------------------
	enum Entitys : byte   // перечисление возможных сущностей, размещаемых на поле
	{
		none = 0,   // ничего, никто
		play,			// игрок
		rock,			// камень
		stump,		// пень
		apple,		// яблоня
		pear,			// груша
		wolf,			// волк
		bear			// медведь
	}

	//-----------------------------------------------------------------------------

	class Field // игровое поле с размерами Width (0...x_max) и Height (0...y_max)
	{
		private static int width;
		private static int height;
		private static int x_max;
		private static int y_max;

		private static Entitys[, ] array_entitys = new Entitys[width, height];

		internal static int Width 
		{
			set { width = value; x_max = width - 1; }
			get { return width; }
		}
		internal static int Height
		{
			set { height = value; y_max = height - 1; }
			get { return height; }
		}
		internal static int X_max
		{
			get { return x_max; }
		}
		internal static int Y_max
		{
			get { return y_max; }
		}

		internal Field(int width, int height )
		{
			Width = width;
			Height = height;
			for (int x = 0; x <= X_max; x++)
			{
				for (int y = 0; y <= Y_max; y++)
				{
					Memory(x, y, Entitys.none);
				}
			}
		}

		internal static void Memory(int x, int y, Entitys ent)  // запоминает что за нечто в ячейке поля
		{
			array_entitys[x, y] = ent;
		}
		internal static void Memory(Location xy, Entitys ent)  // запоминает что за нечто в ячейке поля
		{
			array_entitys[xy.X, xy.Y] = ent;
		}

		internal static void Forget(int x, int y)  // забывает что за нечто было в ячейке поля
		{
			array_entitys[x, y] = Entitys.none;
		}

		internal static void Forget(Location xy)  // забывает что за нечто было в ячейке поля
		{
			array_entitys[xy.X, xy.Y] = Entitys.none;
		}

		internal static bool IsFreeLocation(int x, int y)  // возвращает true если локация xy свободна
		{
			return array_entitys[x, y] == Entitys.none;
		}

		internal static bool IsFreeLocation(Location xy)   // возвращает true если локация xy свободна
		{
			return array_entitys[xy.X, xy.Y] == Entitys.none;
		}

		internal static Entitys WhoInLocation(int x, int y)  // возвращает Entitys из заданной локации xy
		{
			return array_entitys[x, y];
		}

		internal static Entitys WhoInLocation(Location xy)   // возвращает Entitys из заданной локации xy
		{
			return array_entitys[xy.X, xy.Y];
		}

	}

	//-----------------------------------------------------------------------------
	abstract class Entity // нечто, находящееся на поле в локации loc
	{
		private protected Location loc;

		internal Entity()
		{
			Field.Memory(loc, Iam());
		}
		internal Entity(int x, int y)
		{
			loc.X = x;
			loc.Y = y;
			Field.Memory(loc, Iam());
		}

		internal Entity(Location xy)
		{
			loc = xy;
			Field.Memory(loc, Iam());
		}

		internal virtual Entitys Iam() { return Entitys.none; } // сообщает, что это такое

		internal virtual void Show() { }	// отобразить нечто на поле

		internal virtual void Hide() { } // скрыть нечто на поле (оно становится невидимым, но продолжает ещё существовать)

		internal virtual void Done()  // "удалить" нечто с поля (оно становится невидимым, стирается из памяти поля)
		{
			Hide();
			Field.Forget(loc);
		}
	}

	//-----------------------------------------------------------------------------
	enum Direction : byte   // возможные направления перемещения по полю
	{
		west = 0,   // на Запад,	X+
		north,      // на Север,	Y+
		east,       // на Восток,	X-
		south       // на Юг,		Y-
	}

	//-----------------------------------------------------------------------------
	abstract class Creature : Entity // некая тварь, которая может поворачиваться и перемещаться по полю
	{
		private protected int time_max;	// время, необходимое для совершения перемещения на 1 клетку поля
		private protected int time;      // счётчик времени, которое осталось до появления возможности переместиться
		private protected Direction dir; // направление ориентации твари, в этом направлении и может переместиться

		internal virtual int Time			// контролируем диапазон времени
		{
			set
			{
				if ((value < 0) || (value > time_max))
				{ time = time_max; }
				else
				{ time = value; }
			}
			get { return time; }
		}

		internal virtual Direction Dir { set; get; } // тварь может быть ориентирована в любую сторону

		internal Creature() : base()
		{
			time_max = 1;
			time = 0;
			dir = Direction.west;
		}

		internal Creature(int x, int y, int time_max, Direction dir ) : base(x, y)
		{
			this.time_max = time_max;
			Time = 0;
			this.dir = dir;
		}
		internal Creature(Location xy, int time_max, Direction dir) : base(xy)
		{
			this.time_max = time_max;
			Time = 0;
			this.dir = dir;
		}

		internal virtual void DecreaseTime()	// обратный счётчик времени, как станет < 0, так будет присвоено time_max в сеттере
		{
			Time--;
		}

		internal virtual Direction GetDirLeft() // направление влево от текущего
		{
			int i_dir = (int)Dir;
			i_dir++;
			if (i_dir > 3) { i_dir = 0; }
			return (Direction)i_dir;
		}
		internal virtual Direction GetDirRight() // направление вправо от текущего
		{
			int i_dir = (int)Dir;
			i_dir--;
			if (i_dir < 0) { i_dir = 3; }
			return (Direction)i_dir;
		}

		internal virtual void Rotate(Direction new_dir)	// тварь ориентируется в новое направление (поворачивается)
		{
			Hide();
			Dir = new_dir;
			Show();
		}

		internal virtual void RotateRandom() // тварь поворачивает на 90 градусов в случайную сторону
		{
			Random rnd = new Random();
			if (rnd.Next(2) == 0)				// случайное число 0 или 1
			{ Rotate(GetDirLeft()); }
			else { Rotate(GetDirRight()); }
		}

		internal virtual Location GetLocInDirection(Direction in_dir)	// возвращает соседнюю локацию в заданном направлении
		{
			Location xy = new Location();
			switch (in_dir)
			{
				case Direction.west:
					xy.X = loc.X + 1;
					xy.Y = loc.Y;
					break;
				case Direction.north:
					xy.X = loc.X;
					xy.Y = loc.Y + 1;
					break;
				case Direction.east:
					xy.X = loc.X - 1;
					xy.Y = loc.Y;
					break;
				default: // Direction.south:
					xy.X = loc.X;
					xy.Y = loc.Y - 1;
					break;
			}
			return xy;
		}

		internal virtual void NormLocation(ref Location xy)  // нормализует локацию - если выскочили за край поля, то появляемся с противоположной стороны
		{
			if (xy.X > Field.X_max)
			{ xy.X -= Field.Width; }
			else
			{ if (xy.X < 0) { xy.X += Field.Width; } }

			if (xy.Y > Field.Y_max)
			{ xy.Y -= Field.Height; }
			else
			{ if (xy.Y < 0) { xy.Y += Field.Height; } }
		}

		internal virtual void JumpTo(Location xy)   // перескок в заданную локацию, если вылетаем за край поля, то появляемся с противоположной стороны
		{
			Field.Forget(loc);
			Hide();
			loc = xy;
			Show();
			Field.Memory(loc, Iam());

		}
		internal virtual void JumpTo(int x, int y)  // вариант перескока в заданную локацию по координатам
		{
			Location xy = new Location { X = x, Y = y };
			JumpTo(xy);
		}

		internal virtual void Move()	// если время подошло, то совершаем перемещение в соседнюю локацию в текущем направлении
		{										// при наличии там свободного места, иначе делаем поворот в произвольную сторону на 90 градусов
			if (Time == 0)
			{
				Location xy = GetLocInDirection(Dir);
				NormLocation(ref xy);
				if (Field.IsFreeLocation(xy))
				{ JumpTo(xy); }
				else
				{ RotateRandom(); }
			}
			DecreaseTime();
		}
	}

	//-----------------------------------------------------------------------------

	abstract class Predator : Creature // хищник
	{
		private protected int path_max;  // максимальное расстояние, которое проходит хищник до смены направления движения
		private protected int path;      // путь, который осталось пройти до смены направления движения

		internal virtual int Path { set; get; } // пока простейший вариант для хищника, потом будет сложнее
		
		internal Predator() : base()
		{

		}

		internal Predator(int x, int y, int time_max, Direction dir) : base(x, y, time_max, dir)
		{

		}
		internal Predator(Location xy, int time_max, Direction dir) : base(xy, time_max, dir)
		{

		}

		internal virtual void DecreasePath()	// обратный счётчик пройденного пути
		{
			Path--;
		}

		internal virtual void NewPath()	// новый путь
		{
			Random rnd = new Random();
			Path = rnd.Next(1, path_max);	// назначаем новое значение пути в выбранном направлении
		}

		internal override void Move()		// если время подошло, то совершаем перемещение в соседнюю локацию в текущем направлении
		{
			if (Time == 0)
			{
				if (Path <= 0)
				{ RotateRandom(); NewPath(); }
				else
				{
					Location xy = GetLocInDirection(Dir);
					NormLocation(ref xy);
					if (Field.IsFreeLocation(xy))
					{ JumpTo(xy); DecreasePath(); }	// перемещаемся и уменьшаем оставшийся путь
					else
					{ RotateRandom(); NewPath(); }	// поворачиваем и генерируем новый путь
				}
			}
			DecreaseTime();
		}

		internal virtual void Action()   // реализация логики действий хищника по поиску жертвы
		{
			bool is_find;
			Location xy = GetLocInDirection(Dir);
			NormLocation(ref xy);
			if (is_find = (Field.WhoInLocation(xy) == Entitys.play))
			{ }   // съедает игрока
			else
			{
				Direction inspect_dir = GetDirLeft();
				xy = GetLocInDirection(inspect_dir);
				NormLocation(ref xy);
				if (is_find = (Field.WhoInLocation(xy) == Entitys.play))
				{
					Rotate(inspect_dir);
				}   // съедает игрока
				else
				{
					inspect_dir = GetDirRight();
					xy = GetLocInDirection(inspect_dir);
					NormLocation(ref xy);
					if (is_find = (Field.WhoInLocation(xy) == Entitys.play))
					{
						Rotate(inspect_dir);
					}   // съедает игрока
				}
			}
			if (is_find == false) { Move(); }
		}

	}


	//-----------------------------------------------------------------------------

	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
		}
	}
}
