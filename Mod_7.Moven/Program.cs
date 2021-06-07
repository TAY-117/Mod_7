using System;

namespace Mod_7.Moven
/*
Программа имитирует "жизнь", где существуют и взаимодействуют следующие сущности:
	- на поле размещены препятствия (камни и пни, просто лежат и всем мешают ходить);
	- на поле размещены деревья (яблони и вишни), которые плодоносят;
	- по полю ходит игрок (тратит при этом жизненные силы) и собирает яблоки и вишню (пополняя жизненные силы);
	- по полю хаотично бродят волки и ведмеди, которые если набредут на игрока, то съедят его (на этом конец игрового процесса);
	- мир в игре замкнут, т.е. при попытке какой-нибудь твари выйти за пределы поля она окажется с его противоположной стороны.

Программа пока не функциональна по нескольким причинам:
	- не написан код методов отрисовывающих графику (я пока не знаю, как задействовать функционал графического экрана (а не текстового экрана консоли));
	- не написан код управления игроком (я не знаю, как считывать нажатия клавиш стрелочек не останавливая игру в ожидании нажатия какой-нибудь клавиши).

Взаимодействие сущностей производится через посредника, которым является игровое поле, которое не только отображает себя в виде ячеек, 
но и содержит в себе в качестве статических элементов все определённые в игре сущности. Для упрощения кода размеры массивов из сущностей заданы статично,
но при желании можно будет задавать динамически перед началом игры, написав дополнительно код случайного размещения сущностей в пределах поля.

Примечание: в коде я создал индексацию массива деревьев в классе Поле, но по факту её в дальнейшем не использовал, обошёлся без неё.	
*/

{
	// Location - месторасположение на поле (в его системе координат)
	struct Location
	{
		Location(int x, int y) { X = x; Y = y; }
		internal int X { set; get; }

		internal int Y { set; get; }

		public static bool operator == (Location a, Location b)
		{
			return (a.X == b.X) && (a.Y == b.Y);
		}
		public static bool operator != (Location a, Location b)
		{
			return !(a.X == b.X && a.Y == b.Y);
		}
	}

	//-----------------------------------------------------------------------------

	// ID_Entity - перечисление возможных сущностей, размещаемых на поле
	enum ID_Entity : byte
	{
		none = 0,   // ничего, никто
		player,		// игрок
		rock,			// камень
		stump,		// пень
		apple,      // яблоня
		cherry,		// вишня
		wolf,			// волк
		bear			// медведь
	}

	//-----------------------------------------------------------------------------

	// ID_Direct - возможные направления перемещения по полю
	enum ID_Direct : byte
	{
		west = 0,   // на Запад,	X+
		north,      // на Север,	Y+
		east,       // на Восток,	X-
		south       // на Юг,		Y-
	}

	//-----------------------------------------------------------------------------

	class Field // Field - игровое поле с размерами Width (0...x_max) и Height (0...y_max)
	{
		private static int width;
		private static int height;
		private static int x_max;
		private static int y_max;

		private static ID_Entity[ , ] arr_ent_loc = new ID_Entity[width, height];

		internal static Entity[] entities = new Entity[]
		{
			new Rock (18,  3),
			new Stump( 3, 18)
		};

		internal static Player player = new Player(10, 10, 16, ID_Direct.west, 256);

		internal static Predator[] predators = new Predator[]
		{
			new Wolf( 0,  0,  8, ID_Direct.north, 8),
			new Bear(16, 16, 24, ID_Direct.east,  8)
	   };

		private static Tree[] trees = new Tree[]
		{
			new Apple ( 5, 12, 255),
		   new Cherry(12,  5, 127)
		};
		public Tree this[Location xy]
		{
			get
			{
				// Проходим по всему массиву в поисках дерева с такими координатами
				for (int i = 0; i < trees.Length; i++)
				{
					if (trees[i].Loc == xy)
					{
						return trees[i];	// Возвращаем такое дерево
					}
				}

				return null;				// Нет в массиве дерева с такими координатами
			}
		}

		internal static int GetVital(Location xy)
		{
			for (int i = 0; i < trees.Length; i++)
			{
				if (trees[i].Loc == xy) { return trees[i].GetNyam(); }
			}
			return 0;
		}

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
					Memory(x, y, ID_Entity.none);
				}
			}
		}

		// Field.Memory(x, y, ent) - запоминает что за нечто в ячейке поля (локации)
		internal static void Memory(int x, int y, ID_Entity ent)
		{
			arr_ent_loc[x, y] = ent;
		}

		// Field.Memory(xy, ent) - запоминает что за нечто в ячейке поля (локации)
		internal static void Memory(Location xy, ID_Entity ent)
		{
			arr_ent_loc[xy.X, xy.Y] = ent;
		}

		// Field.Forget(x, y) - забывает что за нечто было в ячейке поля (локации)
		internal static void Forget(int x, int y)
		{
			arr_ent_loc[x, y] = ID_Entity.none;
		}

		// Field.Forget(xy) - забывает что за нечто было в ячейке поля (локации)
		internal static void Forget(Location xy)
		{
			arr_ent_loc[xy.X, xy.Y] = ID_Entity.none;
		}

		// Field.IsFreeLocation(x, y) - возвращает true если ячейка поля (локация) свободна
		internal static bool IsFreeLocation(int x, int y)
		{
			return arr_ent_loc[x, y] == ID_Entity.none;
		}

		// Field.IsFreeLocation(xy) - возвращает true если ячейка поля (локация) свободна
		internal static bool IsFreeLocation(Location xy)
		{
			return arr_ent_loc[xy.X, xy.Y] == ID_Entity.none;
		}

		// Field.WhoInLocation(x, y) - возвращает ID_Entity из заданной ячейки поля (локации)
		internal static ID_Entity WhoInLocation(int x, int y)
		{
			return arr_ent_loc[x, y];
		}

		// Field.WhoInLocation(xy) - возвращает ID_Entity из заданной ячейки поля (локации)
		internal static ID_Entity WhoInLocation(Location xy)
		{
			return arr_ent_loc[xy.X, xy.Y];
		}

		// Field.Show - отображает (отрисовывает) поле (точечки по углам ячеек) и всех сущностей
		internal static void Show()
		{
			//тут должен быть код отрисовки поля (точечками по углам ячеек)

			for (int i = 0; i < entities.Length; i++) { entities[i].Show();}

			for (int i = 0; i < trees.Length; i++) { trees[i].Show(); }

			player.Show();

			for (int i = 0; i < predators.Length; i++) { predators[i].Show();}
		}

		// Field.Action - действия всех участников на поле (кроме камней и пней, которые просто лежат)
		internal static void Action()
		{
			for (int i = 0; i < trees.Length; i++)
			{
				trees[i].IncreaseNutrient();
			}
			player.Action();
			for (int i = 0; i < predators.Length; i++)
			{
				predators[i].Action();
			}
		}

	}

	//-----------------------------------------------------------------------------
	abstract class Entity   // нечто, находящееся на поле в локации Loc
	{
		internal Location Loc { set; get; }

		internal Entity()
		{
			Field.Memory(Loc, Iam());
		}
		internal Entity(int x, int y)
		{
			Loc = new Location { X = x, Y = y }; ;
			Field.Memory(Loc, Iam());
		}

		internal Entity(Location xy)
		{
			Loc = xy;
			Field.Memory(xy, Iam());
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
		internal virtual ID_Entity Iam() { return ID_Entity.none; }

		// Entity.Show() - отображает нечто на поле
		internal virtual void Show() { }

		// Entity.Hide() - скрывает нечто на поле (оно становится невидимым, но продолжает ещё существовать в памяти)
		internal virtual void Hide() { }

		// Entity.Done() - "удаляет" нечто с поля (оно становится невидимым, стирается из памяти поля)
		internal virtual void Done()
		{
			Hide();
			Field.Forget(Loc);
		}
	}

	//-----------------------------------------------------------------------------
	abstract class Tree : Entity  // дерево, оно генерирует периодически плоды, сожержащие жизненность
	{
		private protected int nutrient_max;  // максимальный уровень питательности плода, при его достижении рост прекращается
		private protected int nutrient;      // текущий уровень питательности плода
		internal virtual int Nutrient
		{
			set
			{
				nutrient = SetMinMax(0, value, nutrient_max);
			}
			get { return nutrient; }
		}
		internal Tree() : base()
		{
			nutrient_max = 255;
			Nutrient = 0;
		}

		internal Tree(int x, int y, int nutrient_max) : base(x, y)
		{
			this.nutrient_max = nutrient_max;
			Nutrient = 0;
		}
		internal Tree(Location xy, int nutrient_max) : base(xy)
		{
			this.nutrient_max = nutrient_max;
			Nutrient = 0;
		}

		// Tree.IncreaseNutrient() - счётчик уровеня питательности, как станет > nutrient_max, так перестанет расти (действие сеттера)
		internal virtual void IncreaseNutrient()
		{
			Nutrient++;
		}

		// Tree.Nyam() - кто-то съел плод со всей его питательностью
		internal virtual int GetNyam()
		{
			Hide();
			int plod = Nutrient;
			Nutrient = 0;
			Show();
			return plod; 
		}
	}

	//-----------------------------------------------------------------------------
	abstract class Creature : Entity // некая тварь, которая может поворачиваться и перемещаться по полю
	{
		private protected int time_max;	// время, необходимое для совершения перемещения на 1 клетку поля
		private protected int time;      // счётчик времени, которое осталось до появления возможности переместиться
		private protected ID_Direct dir; // направление ориентации твари, в этом направлении и может переместиться

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

		internal virtual ID_Direct Dir { set; get; } // тварь может быть ориентирована в любую сторону

		internal Creature() : base()
		{
			time_max = 1;
			Time = 0;
			dir = ID_Direct.west;
		}

		internal Creature(int x, int y, int time_max, ID_Direct dir ) : base(x, y)
		{
			this.time_max = time_max;
			Time = 0;
			this.dir = dir;
		}
		internal Creature(Location xy, int time_max, ID_Direct dir) : base(xy)
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
		internal virtual ID_Direct GetDirLeft()
		{
			short i_dir = (short)Dir;
			i_dir++;
			if (i_dir > 3) { i_dir = 0; }
			return (ID_Direct)i_dir;
		}

		// Creature.GetDirRight() - возвращает направление вправо от текущего
		internal virtual ID_Direct GetDirRight()
		{
			short i_dir = (short)Dir;
			i_dir--;
			if (i_dir < 0) { i_dir = 3; }
			return (ID_Direct)i_dir;
		}

		// Creature.Rotate(new_dir) - тварь ориентируется в новое направление (поворачивается)
		internal virtual void Rotate(ID_Direct new_dir)
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
		internal virtual Location GetLocInDirection(ID_Direct in_dir)
		{
			Location xy = new Location();
			switch (in_dir)
			{
				case ID_Direct.west:
					xy.X = Loc.X + 1;
					xy.Y = Loc.Y;
					break;
				case ID_Direct.north:
					xy.X = Loc.X;
					xy.Y = Loc.Y + 1;
					break;
				case ID_Direct.east:
					xy.X = Loc.X - 1;
					xy.Y = Loc.Y;
					break;
				default: // ID_Direct.south:
					xy.X = Loc.X;
					xy.Y = Loc.Y - 1;
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
			Field.Forget(Loc);
			Hide();
			Loc = xy;
			Show();
			Field.Memory(Loc, Iam());
		}

		// Creature.JumpTo( x, y) - вариант перескока в заданную локацию по координатам
		internal virtual void JumpTo(int x, int y)
		{
			JumpTo(new Location { X = x, Y = y });
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

		// Creature.Action() - реализация логики действий твари
		internal virtual void Action()
		{ }
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

		internal Predator(int x, int y, int time_max, ID_Direct dir, int path_max) : base(x, y, time_max, dir)
		{
			this.path_max = path_max;
			path = path_max;
		}

		internal Predator(Location xy, int time_max, ID_Direct dir, int path_max) : base(xy, time_max, dir)
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

		// Predator.Action() - реализация логики действий хищника
		internal override void Action()
		{
			bool is_find;
			Location xy = GetLocInDirection(Dir);
			if (is_find = (Field.WhoInLocation(xy) == ID_Entity.player))
			{ Field.player.Done(); }	// съедает игрока
			else
			{
				ID_Direct inspect_dir = GetDirLeft();
				xy = GetLocInDirection(inspect_dir);
				if (is_find = (Field.WhoInLocation(xy) == ID_Entity.player))
				{
					Rotate(inspect_dir);
					Field.player.Done();	// съедает игрока
				}
				else
				{
					inspect_dir = GetDirRight();
					xy = GetLocInDirection(inspect_dir);
					if (is_find = (Field.WhoInLocation(xy) == ID_Entity.player))
					{
						Rotate(inspect_dir);
						Field.player.Done();	// съедает игрока
					}
				}
			}
			if (is_find == false) { Move(); }
		}
	}

	//=============================================================================
	class Apple : Tree  // яблоня, она генерирует периодически яблоки, сожержащие жизненность
	{

		internal Apple() : base()
		{ }

		internal Apple(int x, int y, int nutrient_max) : base(x, y, nutrient_max)
		{ }
		internal Apple(Location xy, int nutrient_max) : base(xy, nutrient_max)
		{ }

		// Apple.Iam() - сообщает, что это яблоня
		internal override ID_Entity Iam() { return ID_Entity.apple; }

		// Apple.Show() - отображает яблоню на поле
		internal override void Show() { }

		// Apple.Hide() - скрывает яблоню на поле (она становится невидимым, но продолжает ещё существовать в памяти)
		internal override void Hide() { }
	}

	//=============================================================================
	class Cherry : Tree  // вишня, она генерирует периодически вишенки, сожержащие жизненность
	{

		internal Cherry() : base()
		{ }

		internal Cherry(int x, int y, int nutrient_max) : base(x, y, nutrient_max)
		{ }
		internal Cherry(Location xy, int nutrient_max) : base(xy, nutrient_max)
		{ }

		// Cherry.Iam() - сообщает, что это вишня
		internal override ID_Entity Iam() { return ID_Entity.cherry; }

		// Cherry.Show() - отображает вишню на поле
		internal override void Show() { }

		// Cherry.Hide() - скрывает вишню на поле (она становится невидимым, но продолжает ещё существовать в памяти)
		internal override void Hide() { }
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
			vital_max = 255;
			vital = vital_max;
		}

		internal Player(int x, int y, int time_max, ID_Direct dir, int vital_max) : base(x, y, time_max, dir)
		{
			this.vital_max = vital_max;
			vital = vital_max;
		}

		internal Player(Location xy, int time_max, ID_Direct dir, int vital_max) : base(xy, time_max, dir)
		{
			this.vital_max = vital_max;
			vital = vital_max;
		}

		// Player.Move() - перемещение игрока
		internal override void Move()
		{
			if ((Time == 0) && (Vital > 0))
			{
				Location xy = GetLocInDirection(Dir);
				if (Field.IsFreeLocation(xy))
					{
					JumpTo(xy);				// перемещаемся
					Vital -= time_max;	// тратим жизненные силы
				}  // перемещаемся
			}
			DecreaseTime();
		}

		// Player.Control() - управление направлением перемещения игрока
		internal virtual void Control()
		{
			// по управляющим клавишам назначается ID_Direct new_dir и выполнятеся Rotate(new_dir);
			// и где-то должен вызываться метод Move(), но вот где... ? в Action() ???

		}

		// Player.Search() - если нашли что-нибудь сьедобное, то съедаем и увеличиваем жизненность
		internal virtual void Search()
		{
			Location xy = GetLocInDirection(Dir);
			ID_Entity ent = Field.WhoInLocation(xy);
			if (ent == ID_Entity.apple || ent == ID_Entity.cherry)
			{
				Vital += Field.GetVital(xy);
			}
		}

		// Player.Action() - реализация действий игрока
		internal override void Action()
		{
			Search();
			Control();
			Move();
		}
		internal override void Done()
		{
			Vital = 0;
			Hide();
			Field.Forget(Loc);
		}
	}

	//=============================================================================
	class Wolf : Predator // волк
	{
		internal Wolf(int x, int y, int time_max, ID_Direct dir, int path_max) : base(x, y, time_max, dir, path_max)
		{ }

		internal Wolf(Location xy, int time_max, ID_Direct dir, int path_max) : base(xy, time_max, dir, path_max)
		{ }

		// Wolf.Iam() - сообщает, что это волк
		internal override ID_Entity Iam() { return ID_Entity.wolf; }

		// Wolf.Show() - отображает волка на поле
		internal override void Show() { }

		// Wolf.Hide() - скрывает волка на поле (он становится невидимым, но продолжает ещё существовать в памяти)
		internal override void Hide() { }
	}

	//=============================================================================
	class Bear : Predator // ведмедь
	{
		internal Bear(int x, int y, int time_max, ID_Direct dir, int path_max) : base(x, y, time_max, dir, path_max)
		{ }

		internal Bear(Location xy, int time_max, ID_Direct dir, int path_max) : base(xy, time_max, dir, path_max)
		{ }

		// Bear.Iam() - сообщает, что это волк
		internal override ID_Entity Iam() { return ID_Entity.bear; }

		// Bear.Show() - отображает волка на поле
		internal override void Show() { }

		// Bear.Hide() - скрывает волка на поле (он становится невидимым, но продолжает ещё существовать в памяти)
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
		internal override ID_Entity Iam() { return ID_Entity.rock; }

		// Rock.Show() - отображает камень на поле
		internal override void Show() { }

		// Rock.Hide() - скрывает камень на поле (он становится невидимым, но продолжает ещё существовать в памяти)
		internal override void Hide() { }
	}

	//=============================================================================
	class Stump : Entity  // пень
	{
		internal Stump(int x, int y) : base(x, y)
		{ }

		internal Stump(Location xy) : base(xy)
		{ }

		// Stump.Iam() - сообщает, что это пень
		internal override ID_Entity Iam() { return ID_Entity.stump; }

		// Stump.Show() - отображает пень на поле
		internal override void Show() { }

		// Stump.Hide() - скрывает пень на поле (он становится невидимым, но продолжает ещё существовать в памяти)
		internal override void Hide() { }
	}

	//=============================================================================
	class Program
	{
		static void Main(string[] args)
		{
			Field.Show();
			do { Field.Action(); } while (Field.player.Vital > 0);
			Console.WriteLine("Game over!");
		}
	}
}
