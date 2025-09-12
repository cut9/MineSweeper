MineSweeper game = new();
game.Play();

public class MineSweeper
{
    private Cell[,] minefield = null;
    private int maxMines = 0;
    private int mineCounter = 0;
    private int maxFlags = 0;
    private int currentFlags = 0;
    private int currentTurn = 0;
    private bool isGameContinue = true;
    private Random rand = new Random();
    private List<LevelSettings> difficultyPresets = new()
    {
        new LevelSettings(9, 9, 10, "Новичок"),
        new LevelSettings(16, 16, 40, "Любитель"),
        new LevelSettings(16, 30, 99, "Профессионал"),
        new LevelSettings(9, 9, 10, "Особый\nНастраиваемая сложность")
    };
    private int currentDifficultyIndex = 0;
    private class Cell
    {
        public int Number { get; private set; } = 0;
        public bool IsMine { get; private set; } = false;
        public bool IsOpened { get; private set; } = false;
        public bool IsFlagged { get; private set; } = false;
        public bool CanBeMine { get; set; } = true;
        public override string ToString()
        {
            if (IsFlagged) return "[&]";
            if (!IsOpened) return "[#]";
            if (IsMine) return "[*]";
            if (Number > 0) return $"[{Number}]";
            return "[ ]";
        }
        public void Open() => IsOpened = true;
        public bool ToggleFlagMode()
        {
            IsFlagged = !IsFlagged;
            return IsFlagged;
        }
        public void IncreaseNumber()
        {
            if (!IsMine)
                Number++;
        }
        public bool MakeNewMine()
        {
            if (!CanBeMine)
                return false;
            CanBeMine = false;
            IsMine = true;
            Number = 0;
            return true;
        }
    }

    private class LevelSettings
    {
        public int Rows;
        public int Columns;
        public int Mines;
        public string Name;
        public LevelSettings(int rows, int columns, int mines, string name)
        {
            Rows = rows;
            Columns = columns;
            Mines = mines;
            Name = name;
        }

        public void ChangeRows(int rows) => Rows = rows;
        public void ChangeColumns(int columns) => Columns = columns;
        public void ChangeMines(int mines) => Mines = mines;

        public override string ToString()
        {
            return $"{Name}\n{Rows}x{Columns}\n{Mines} мин";
        }
    }

    public MineSweeper()
    {
        currentDifficultyIndex = 0;
    }

    public void Play()
    {
        while (isGameContinue)
        {
            PreGameSettings();
            CreateGame();
            while (isGameContinue)
            {
                Console.Clear();
                ShowMinefield();
                Console.WriteLine("Введите команду (open row columns, flag row columns):");
                Interact(Console.ReadLine());
            }
            isGameContinue = ExitQuestion();
        }
    }

    private bool ExitQuestion()
    {
        while (true)
        {
            Console.WriteLine("Сыграть ещё раз?");
            var input = Console.ReadLine()?.ToLower();
            switch (input)
            {
                case "д":
                case "да":
                case "y":
                case "yes":
                    return true;
                case "нет":
                case "н":
                case "no":
                case "n":
                    return false;
                default:
                    Console.WriteLine("Некорректная команда");
                    break;
            }
        }
    }

    private void PreGameSettings()
    {
        while (true)
        {
            Console.WriteLine("Введите команду [Начать], [Выбрать сложность]:");
            var userInput = Console.ReadLine()?.Trim().ToLower();
            if (string.IsNullOrEmpty(userInput)) continue;

            if (userInput is "начать" or "начать игру" or "играть" or "start" or "start game" or "н" or "s")
                return;

            if (userInput is "выбрать сложность" or "сложность" or "выбрать" or "изменить" or "change" or "difficulty" or "c" or "d" or "в" or "и")
            {
                Console.Clear();
                for (int i = 0; i < difficultyPresets.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {difficultyPresets[i]}");
                    if (i == currentDifficultyIndex) Console.WriteLine("Текущая сложность!");
                    Console.WriteLine("-------------------------");
                }
                ChangeDifficulty();
                continue;
            }

            Console.WriteLine("Некорректная команда");
        }
    }

    private void ChangeDifficulty()
    {
        string userTextInput;
        int userNumberInput;
        while (true)
        {
            Console.WriteLine("Выберите сложность [1], [2], [3], [4]:");
            userTextInput = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(userTextInput)) continue;
            if (userTextInput.ToLower() == "назад")
            {
                Console.Clear();
                return;
            }
            if (int.TryParse(userTextInput, out userNumberInput) && userNumberInput - 1 > -1 && userNumberInput - 1 < difficultyPresets.Count)
            {
                userNumberInput--;
                if (userNumberInput == 3)
                    SpecialDifficultyChanger();
                currentDifficultyIndex = userNumberInput;
                Console.Clear();
                Console.WriteLine("Настройки успешно изменены!");
                return;
            }
            else
            {
                Console.WriteLine("Некорректное значение!");
            }
        }
    }

    private void SpecialDifficultyChanger()
    {
        Console.Clear();
        while (true)
        {
            Console.WriteLine("Введите количество строк (9-30) или 'назад':");
            var rowsText = Console.ReadLine();
            if (rowsText?.ToLower() == "назад") { Console.Clear(); return; }
            Console.WriteLine("Введите количество столбцов (9-30) или 'назад':");
            var colsText = Console.ReadLine();
            if (colsText?.ToLower() == "назад") { Console.Clear(); return; }
            Console.WriteLine("Введите количество мин (10 - rows*columns-18) или 'назад':");
            var minesText = Console.ReadLine();
            if (minesText?.ToLower() == "назад") { Console.Clear(); return; }

            if (!int.TryParse(rowsText, out int rows) || !int.TryParse(colsText, out int columns) || !int.TryParse(minesText, out int mines))
            {
                Console.WriteLine("Ввод должен быть числом. Попробуйте ещё раз.");
                continue;
            }

            if (rows >= 9 && rows <= 30 && columns >= 9 && columns <= 30 && mines >= 10 && mines <= rows * columns - 18)
            {
                difficultyPresets[3].ChangeRows(rows);
                difficultyPresets[3].ChangeColumns(columns);
                difficultyPresets[3].ChangeMines(mines);
                return;
            }
            else
            {
                Console.WriteLine("Ваши настройки не подходят под условия!");
            }
        }
    }

    private void ShowMinefield()
    {
        if (minefield == null) return;
        Console.Write("[0]");
        for (int i = 1; i <= minefield.GetLength(1); i++)
        {
            if (i < 10)
                Console.Write($" {i} ");
            else
                Console.Write($" {i}");
        }
        Console.WriteLine();

        for (int i = 0; i < minefield.GetLength(0); i++)
        {
            for (int j = 0; j < minefield.GetLength(1); j++)
            {
                if (j == 0 && i < 9)
                    Console.Write($"{i + 1}  ");
                else if (j == 0)
                    Console.Write($"{i + 1} ");

                Console.Write(minefield[i, j]);
            }
            Console.WriteLine();
        }
        Console.WriteLine($"Поставлено {currentFlags} флагов из {maxFlags}");
    }

    private void Interact(string command)
    {
        if (string.IsNullOrWhiteSpace(command)) return;
        string[] input = command.Split();
        if (!(input.Length == 3 && int.TryParse(input[1], out int x) && int.TryParse(input[2], out int y)))
            return;
        input[0] = input[0].ToLower();
        x--;
        y--;
        if (!IsValid(x, y) || minefield[x, y].IsOpened)
            return;

        bool actionPerformed = false;
        switch (input[0])
        {
            case "открыть":
            case "о":
            case "open":
            case "o":
                Open(x, y);
                actionPerformed = true;
                break;
            case "флаг":
            case "ф":
            case "flag":
            case "f":
                Flag(x, y);
                actionPerformed = true;
                break;
        }
        if (actionPerformed) currentTurn++;
    }

    private void Open(int x, int y)
    {
        if (mineCounter == 0)
        {
            int[,] directions =
            {
                {-1, -1}, {-1, 0}, {-1, 1},
                {0, -1}, {0, 0 }, {0, 1},
                {1, -1}, {1, 0}, {1, 1}
            };
            for (int k = 0; k < directions.GetLength(0); k++)
            {
                int newRow = x + directions[k, 0];
                int newCol = y + directions[k, 1];
                if (IsValid(newRow, newCol))
                    minefield[newRow, newCol].CanBeMine = false;
            }
            CreateMinefield();
        }
        RevealEmptyCells(x, y);
        if (minefield[x, y].IsMine)
        {
            GameOver("Lose");
        }
        CheckWin();
    }

    private void RevealEmptyCells(int x, int y)
    {
        if (!IsValid(x, y) || minefield[x, y].IsOpened || minefield[x, y].IsFlagged)
            return;
        minefield[x, y].Open();
        if (minefield[x, y].Number > 0)
            return;
        RevealEmptyCells(x - 1, y - 1);
        RevealEmptyCells(x - 1, y);
        RevealEmptyCells(x - 1, y + 1);
        RevealEmptyCells(x, y - 1);
        RevealEmptyCells(x, y + 1);
        RevealEmptyCells(x + 1, y - 1);
        RevealEmptyCells(x + 1, y);
        RevealEmptyCells(x + 1, y + 1);
    }

    private void Flag(int x, int y)
    {
        if (minefield[x, y].IsFlagged)
        {
            minefield[x, y].ToggleFlagMode();
            currentFlags--;
            return;
        }
        if (!minefield[x, y].IsFlagged && currentFlags < maxFlags)
        {
            minefield[x, y].ToggleFlagMode();
            currentFlags++;
        }
        if (currentFlags == maxFlags)
        {
            CheckWin();
        }
    }

    private void CheckWin()
    {
        bool allNonMinesOpened = true;
        bool allMinesFlaggedCorrectly = true;

        foreach (var cell in minefield)
        {
            if (!cell.IsMine && !cell.IsOpened)
                allNonMinesOpened = false;
            if (cell.IsMine && !cell.IsFlagged)
                allMinesFlaggedCorrectly = false;
            if (!cell.IsMine && cell.IsFlagged)
                allMinesFlaggedCorrectly = false;
        }

        if (allNonMinesOpened || allMinesFlaggedCorrectly)
        {
            GameOver("Win");
        }
    }

    private void GameOver(string state)
    {
        isGameContinue = false;
        Console.Clear();
        foreach (var cell in minefield)
        {
            cell.Open();
        }
        ShowMinefield();
        switch (state)
        {
            case "Win":
                Console.WriteLine("Поздравляю! Вы победили!");
                break;
            case "Lose":
                Console.WriteLine("В следующий раз точно получится. Вы проиграли.");
                break;
        }
        Console.WriteLine("Потрачено " + currentTurn + " ходов.");
    }

    private void CreateGame()
    {
        var cur = difficultyPresets[currentDifficultyIndex];
        maxMines = cur.Mines;
        maxFlags = maxMines;
        currentFlags = 0;
        currentTurn = 0;
        mineCounter = 0;
        minefield = null;
        minefield = new Cell[cur.Rows, cur.Columns];
        for (int i = 0; i < minefield.GetLength(0); i++)
        {
            for (int j = 0; j < minefield.GetLength(1); j++)
            {
                minefield[i, j] = new Cell();
            }
        }
    }

    private void CreateMinefield()
    {
        while (mineCounter < maxMines)
        {
            int i = rand.Next(0, minefield.GetLength(0));
            int j = rand.Next(0, minefield.GetLength(1));
            if (minefield[i, j].MakeNewMine())
            {
                mineCounter++;
                AddMinefieldNumber(i, j);
            }
        }
    }

    private void AddMinefieldNumber(int i, int j)
    {
        int[,] directions =
        {
            {-1, -1}, {-1, 0}, {-1, 1},
            {0, -1},          {0, 1},
            {1, -1}, {1, 0}, {1, 1}
        };
        for (int k = 0; k < directions.GetLength(0); k++)
        {
            int newRow = i + directions[k, 0];
            int newCol = j + directions[k, 1];
            if (IsValid(newRow, newCol))
                minefield[newRow, newCol].IncreaseNumber();
        }
    }
    private bool IsValid(int x, int y) => minefield != null && x >= 0 && x < minefield.GetLength(0) && y >= 0 && y < minefield.GetLength(1);
}