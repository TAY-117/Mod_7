using System;

namespace Mod_7.Moven
{
	// Location - месторасположение
	struct Location
	{
		Location(int x, int y) { X = x; Y = y; }
		internal int X { set; get; }

		internal int Y { set; get; }
	}

	//-----------------------------------------------------------------------------

	// Entitys - перечисление возможных сущностей, размещаемых на поле
	enum Entitys : byte
	{
		none = 0,   // ничего, никто
		player,		// игрок
		rock,			// камень
		stump,		// пень
		apple,		// яблоня
		pear,			// груша
		wolf,			// волк
		bear			// медведь
	}

	//-----------------------------------------------------------------------------

	// Direction - возможные направления перемещения по полю
	enum Direction : byte
	{
		west = 0,   // на Запад,	X+
		north,      // на Север,	Y+
		east,       // на Восток,	X-
		south       // на Юг,		Y-
	}

	//-----------------------------------------------------------------------------

	// Field - игровое поле с размерами Width (0...x_max) и Height (0...y_max)
	class Field
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

		internal Field(int width, int height)
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

		// Field.Memory(x, y, ent) - запоминает что за нечто в ячейке поля (локации)
		internal static void Memory(int x, int y, Entitys ent)
		{
			array_entitys[x, y] = ent;
		}

		// Field.Memory(xy, ent) - запоминает что за нечто в ячейке поля (локации)
		internal static void Memory(Location xy, Entitys ent)
		{
			array_entitys[xy.X, xy.Y] = ent;
		}

		// Field.Forget(x, y) - забывает что за нечто было в ячейке поля (локации)
		internal static void Forget(int x, int y)
		{
			array_entitys[x, y] = Entitys.none;
		}

		// Field.Forget(xy) - забывает что за нечто было в ячейке поля (локации)
		internal static void Forget(Location xy)
		{
			array_entitys[xy.X, xy.Y] = Entitys.none;
		}

		// Field.IsFreeLocation(x, y) - возвращает true если ячейка поля (локация) свободна
		internal static bool IsFreeLocation(int x, int y)
		{
			return array_entitys[x, y] == Entitys.none;
		}

		// Field.IsFreeLocation(xy) - возвращает true если ячейка поля (локация) свободна
		internal static bool IsFreeLocation(Location xy)
		{
			return array_entitys[xy.X, xy.Y] == Entitys.none;
		}

		// Field.WhoInLocation(x, y) - возвращает Entitys из заданной ячейки поля (локации)
		internal static Entitys WhoInLocation(int x, int y)
		{
			return array_entitys[x, y];
		}

		// Field.WhoInLocation(xy) - возвращает Entitys из заданной ячейки поля (локации)
		internal static Entitys WhoInLocation(Location xy)
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
		internal static int SetMinMax(int min, int num, int max)
		{
			if (num < min)
			{ num = min; }
			else
			{
				if (num > max)
				{ num = max; }
			}
			return num;
		}

		// Entity.Iam() - сообщает, что это оно такое
		internal virtual Entitys Iam() { return Entitys.none; }

		// Entity.Show() - отображает нечто на поле
		internal virtual void Show() { }

		// Entity.Hide() - скрывает нечто на поле (оно становится невидимым, но продолжает ещё существовать)
		internal virtual void Hide() { }

		// Entity.Done() - "удаляет" нечто с поля (оно становится невидимым, стирается из памяти поля)
		internal virtual void Done()
		{
			Hide();
			Field.Forget(loc);
		}
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

		// Creature.DecreaseTime() - обратный счётчик времени, как станет < 0, так будет присвоено time_max в сеттере
		internal virtual void DecreaseTime()	
		{
			Time--;
		}

		// Creature.GetDirLeft() - возвращает направление влево от текущего
		internal virtual Direction GetDirLeft()
		{
			int i_dir = (int)Dir;
			i_dir++;
			if (i_dir > 3) { i_dir = 0; }
			return (Direction)i_dir;
		}

		// Creature.GetDirRight() - возвращает направление вправо от текущего
		internal virtual Direction GetDirRight()
		{
			int i_dir = (int)Dir;
			i_dir--;
			if (i_dir < 0) { i_dir = 3; }
			return (Direction)i_dir;
		}

		// Creature.Rotate(new_dir) - тварь ориентируется в новое направление (поворачивается)
		internal virtual void Rotate(Direction new_dir)
		{
			Hide();
			Dir = new_dir;
			Show();
		}

		// Creature.RotateRandom() - тварь поворачивает на 90 градусов в случайную сторону
		internal virtual void RotateRandom()
		{
			Random rnd = new Random();
			if (rnd.Next(2) == 0)				// случайное число 0 или 1
			{ Rotate(GetDirLeft()); }
			else { Rotate(GetDirRight()); }
		}

		// Creature.GetLocInDirection(in_dir) - возвращает соседнюю локацию в заданном направлении
		internal virtual Location GetLocInDirection(Direction in_dir)
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
			// нормализует локацию - если выскочили за край поля, то появляемся с противоположной стороны
			if (xy.X > Field.X_max)
			{ xy.X -= Field.Width; }
			else
			{ if (xy.X < 0) { xy.X += Field.Width; } }

			if (xy.Y > Field.Y_max)
			{ xy.Y -= Field.Height; }
			else
			{ if (xy.Y < 0) { xy.Y += Field.Height; } }
			return xy;
		}

		// Creature.JumpTo(xy) - перескок в заданную локацию, если вылетаем за край поля, то появляемся с противоположной стороны
		internal virtual void JumpTo(Location xy)
		{
			Field.Forget(loc);
			Hide();
			loc = xy;
			Show();
			Field.Memory(loc, Iam());
		}

		// Creature.JumpTo( x, y) - вариант перескока в заданную локацию по координатам
		internal virtual void JumpTo(int x, int y)
		{
			Location xy = new Location { X = x, Y = y };
			JumpTo(xy);
		}

		// Creature.Move() - если время подошло, то совершаем перемещение в соседнюю локацию в текущем направлении
		//							при наличии там свободного места, иначе делаем поворот в произвольную сторону на 90 градусов
		internal virtual void Move()
		{
			if (Time == 0)
			{
				Location xy = GetLocInDirection(Dir);
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
		private protected int path_max;	// максимальное расстояние, которое проходит хищник до смены направления движения
		private protected int path;		// путь, который осталось пройти до смены направления движения

		internal virtual int Path { set; get; }	// пока простейший вариант для хищника, потом будет сложнее
		
		internal Predator() : base()
		{
			path_max = 8;
			path = path_max;
		}

		internal Predator(int x, int y, int time_max, Direction dir, int path_max) : base(x, y, time_max, dir)
		{
			this.path_max = path_max;
			path = path_max;
		}

		internal Predator(Location xy, int time_max, Direction dir, int path_max) : base(xy, time_max, dir)
		{
			this.path_max = path_max;
			path = path_max;
		}

		// Predator.DecreasePath() - обратный счётчик пройденного пути
		internal virtual void DecreasePath()
		{
			Path--;
		}

		// Predator.NewPath() - назначаем новое значение пути в выбранном направлении
		internal virtual void NewPath()
		{
			Random rnd = new Random();
			Path = rnd.Next(1, path_max);
		}

		// Predator.Move() - если время подошло, то совершаем перемещение в соседнюю локацию в текущем направлении
		//							при наличии там свободного места, иначе делаем поворот в произвольную сторону на 90 градусов
		internal override void Move()
		{
			if (Time == 0)
			{
				if (Path <= 0)
				{ RotateRandom(); NewPath(); }
				else
				{
					Location xy = GetLocInDirection(Dir);
					if (Field.IsFreeLocation(xy))
					{ JumpTo(xy); DecreasePath(); }	// перемещаемся и уменьшаем оставшийся путь
					else
					{ RotateRandom(); NewPath(); }	// поворачиваем и генерируем новый путь
				}
			}
			DecreaseTime();
		}

		// Predator.Action() - реализация логики действий хищника по поиску жертвы
		internal virtual void Action()
		{
			bool is_find;
			Location xy = GetLocInDirection(Dir);
			if (is_find = (Field.WhoInLocation(xy) == Entitys.player))
			{ }   // съедает игрока, ссылку на которого надо как-то найти
			else
			{
				Direction inspect_dir = GetDirLeft();
				xy = GetLocInDirection(inspect_dir);
				if (is_find = (Field.WhoInLocation(xy) == Entitys.player))
				{
					Rotate(inspect_dir);
				}   // съедает игрока
				else
				{
					inspect_dir = GetDirRight();
					xy = GetLocInDirection(inspect_dir);
					if (is_find = (Field.WhoInLocation(xy) == Entitys.player))
					{
						Rotate(inspect_dir);
					}   // съедает игрока
				}
			}
			if (is_find == false) { Move(); }
		}

	}

	//=============================================================================
	class Player : Creature // игрок
	{
		private protected int vital_max;	// максимальное уровень жизненности
		private protected int vital;		// текщий уровень жизненности

		internal virtual int Vital
		{
			set
			{
				vital = SetMinMax(0, value, vital_max);
			}
			get { return vital; }
		}

		internal Player() : base()
		{
			vital_max = 2;
			vital = vital_max;
		}

		internal Player(int x, int y, int time_max, Direction dir, int vital_max) : base(x, y, time_max, dir)
		{
			this.vital_max = vital_max;
			vital = vital_max;
		}

		internal Player(Location xy, int time_max, Direction dir, int vital_max) : base(xy, time_max, dir)
		{
			this.vital_max = vital_max;
			vital = vital_max;
		}

		// Player.Move() - перемещение игрока
		internal override void Move()
		{
			if (Time == 0)
			{
				Location xy = GetLocInDirection(Dir);
				if (Field.IsFreeLocation(xy))
					{ JumpTo(xy); }  // перемещаемся
			}
			DecreaseTime();
		}

		// Player.Control() - управление направлением перемещения игрока
		internal virtual void Control()
		{
			// по управляющим клавишам назначается Direction new_dir и выполнятеся Rotate(new_dir);
			// и где-то должен вызываться метод Move(), но вот где... ?

		}

		// Player.Search() - если нашли что-нибудь сьедобное, то съедаем и увеличиваем жизненность
		internal virtual void Search()
		{
			Location xy = GetLocInDirection(Dir);
			Entitys ent = Field.WhoInLocation(xy);
			if (ent == Entitys.apple || ent == Entitys.pear)
			{
				Vital = Vital + 1; // заменить 1 на возвращаемое деревом количество жизненности
				// и как-то должна у дерева сняться эта жизненность, как определить у какого, ссылки ведь на него нет...
			}
		}
	}

	//=============================================================================

	class Wolf : Predator // волк
	{
		internal Wolf(int x, int y, int time_max, Direction dir, int path_max) : base(x, y, time_max, dir, path_max)
		{ }

		internal Wolf(Location xy, int time_max, Direction dir, int path_max) : base(xy, time_max, dir, path_max)
		{ }

		// Wolf.Iam() - сообщает, что это волк
		internal override Entitys Iam() { return Entitys.wolf; }

		// Wolf.Show() - отображает волка на поле
		internal override void Show() { }

		// Wolf.Hide() - скрывает волка на поле (он становится невидимым, но продолжает ещё существовать)
		internal override void Hide() { }
	}

	//=============================================================================
	class Bear : Predator // ведмедь
	{
		internal Bear(int x, int y, int time_max, Direction dir, int path_max) : base(x, y, time_max, dir, path_max)
		{ }

		internal Bear(Location xy, int time_max, Direction dir, int path_max) : base(xy, time_max, dir, path_max)
		{ }

		// Bear.Iam() - сообщает, что это волк
		internal override Entitys Iam() { return Entitys.bear; }

		// Bear.Show() - отображает волка на поле
		internal override void Show() { }

		// Bear.Hide() - скрывает волка на поле (он становится невидимым, но продолжает ещё существовать)
		internal override void Hide() { }
	}

	//=============================================================================

	class Rock : Entity	// камень
	{
		internal Rock(int x, int y) : base(x, y)
		{ }

		internal Rock(Location xy) : base(xy)
		{ }

		// Rock.Iam() - сообщает, что это камень
		internal override Entitys Iam() { return Entitys.rock; }

		// Rock.Show() - отображает камень на поле
		internal override void Show() { }
	}

	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
		}
	}
}
