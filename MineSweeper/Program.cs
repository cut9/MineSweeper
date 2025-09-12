using System.Text.Json;
using System.Text.Json.Serialization;

MineSweeper game = new();
game.Play();

public class MineSweeper
{
    private Cell[,] _minefield = null;
    private int _maxMines = 0;
    private int _mineCounter = 0;
    private int _maxFlags = 0;
    private int _currentFlags = 0;
    private int _currentTurn = 0;
    private bool _isGameContinue = true;
    private Random _rand = new Random();
    private List<DifficultySettings> _difficultyPresets = new()
    {
        new DifficultySettings(9, 9, 10, "Новичок"),
        new DifficultySettings(16, 16, 40, "Любитель"),
        new DifficultySettings(16, 30, 99, "Профессионал"),
        new DifficultySettings(9, 9, 10, "Особый\nНастраиваемая сложность")
    };
    private int _currentDifficultyIndex = 0;
    private readonly string? _folderPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "cut9\\MineSweeper"
    );
    private readonly string? _settings;
    private readonly string? _statistic;
    public MineSweeper()
    {
        _settings = Path.Combine(_folderPath, "Settings.json");
        _statistic = Path.Combine(_folderPath, "Statistic.json");
    }
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
    private class DifficultySettings
    {
        [JsonIgnore]
        public string Name { get; private set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public int Mines { get; set; }
        public DifficultySettings() { }
        public DifficultySettings(int rows, int columns, int mines, string name)
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

    public void Play()
    {
        LoadStatistic();
        while (_isGameContinue)
        {
            PreGameSettings();
            CreateGame();
            while (_isGameContinue)
            {
                Console.Clear();
                ShowMinefield();
                Console.WriteLine("Введите команду (open row columns, flag row columns):");
                Interact(Console.ReadLine());
            }
            _isGameContinue = ExitQuestion();
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
        LoadSettings();
        while (true)
        {
            Console.WriteLine("Введите команду [Начать], [Выбрать сложность], [Статистика]:");
            var userInput = Console.ReadLine()?.Trim().ToLower();
            if (string.IsNullOrEmpty(userInput)) continue;

            if (userInput is "начать" or "начать игру" or "играть" or "start" or "start game" or "н" or "s")
                return;

            if (userInput is "выбрать сложность" or "сложность" or "выбрать" or "изменить" or "change" or "difficulty" or "c" or "d" or "в" or "и")
            {
                Console.Clear();
                for (int i = 0; i < _difficultyPresets.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {_difficultyPresets[i]}");
                    if (i == _currentDifficultyIndex) Console.WriteLine("Текущая сложность!");
                    Console.WriteLine("-------------------------");
                }
                ChangeDifficulty();
                continue;
            }

            if (userInput is "статистика" or "стат" or "stats")
            {
                Console.Clear();
                ShowStats();
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
            if (int.TryParse(userTextInput, out userNumberInput) && userNumberInput - 1 > -1 && userNumberInput - 1 < _difficultyPresets.Count)
            {
                userNumberInput--;
                if (userNumberInput == 3)
                    SpecialDifficultyChanger();
                _currentDifficultyIndex = userNumberInput;
                Console.Clear();
                Console.WriteLine("Настройки успешно изменены!");
                SaveSettings();
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
                _difficultyPresets[3].ChangeRows(rows);
                _difficultyPresets[3].ChangeColumns(columns);
                _difficultyPresets[3].ChangeMines(mines);
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
        if (_minefield == null) return;
        Console.Write("[0]");
        for (int i = 1; i <= _minefield.GetLength(1); i++)
        {
            if (i < 10)
                Console.Write($" {i} ");
            else
                Console.Write($" {i}");
        }
        Console.WriteLine();

        for (int i = 0; i < _minefield.GetLength(0); i++)
        {
            for (int j = 0; j < _minefield.GetLength(1); j++)
            {
                if (j == 0 && i < 9)
                    Console.Write($"{i + 1}  ");
                else if (j == 0)
                    Console.Write($"{i + 1} ");

                Console.Write(_minefield[i, j]);
            }
            Console.WriteLine();
        }
        Console.WriteLine($"Поставлено {_currentFlags} флагов из {_maxFlags}");
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
        if (!IsValid(x, y) || _minefield[x, y].IsOpened)
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
        if (actionPerformed) _currentTurn++;
    }

    private void Open(int x, int y)
    {
        if (_mineCounter == 0)
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
                    _minefield[newRow, newCol].CanBeMine = false;
            }
            CreateMinefield();
        }
        RevealEmptyCells(x, y);
        if (_minefield[x, y].IsMine)
        {
            GameOver("Lose");
        }
        CheckWin();
    }

    private void RevealEmptyCells(int x, int y)
    {
        if (!IsValid(x, y) || _minefield[x, y].IsOpened || _minefield[x, y].IsFlagged)
            return;
        _minefield[x, y].Open();
        if (_minefield[x, y].Number > 0)
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
        if (_minefield[x, y].IsFlagged)
        {
            _minefield[x, y].ToggleFlagMode();
            _currentFlags--;
            return;
        }
        if (!_minefield[x, y].IsFlagged && _currentFlags < _maxFlags)
        {
            _minefield[x, y].ToggleFlagMode();
            _currentFlags++;
        }
        if (_currentFlags == _maxFlags)
        {
            CheckWin();
        }
    }

    private void CheckWin()
    {
        bool allNonMinesOpened = true;
        bool allMinesFlaggedCorrectly = true;

        foreach (var cell in _minefield)
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
        _isGameContinue = false;
        Console.Clear();
        foreach (var cell in _minefield)
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
        Console.WriteLine("Потрачено " + _currentTurn + " ходов.");
        StatModifier(state);
    }

    public void StatModifier(string lastGame)
    {
        DifficultyStats[_currentDifficultyIndex].GamesCounter++;
        switch (lastGame)
        {
            case "Win":
                DifficultyStats[_currentDifficultyIndex].WinsCounter++;
                if (DifficultyStats[_currentDifficultyIndex].LastGame == lastGame)
                    DifficultyStats[_currentDifficultyIndex].WinsStreak++;
                if (DifficultyStats[_currentDifficultyIndex].WinsStreak > DifficultyStats[_currentDifficultyIndex].WinsInRow++)
                    DifficultyStats[_currentDifficultyIndex].WinsInRow = DifficultyStats[_currentDifficultyIndex].WinsStreak;
                DifficultyStats[_currentDifficultyIndex].LastGame = lastGame;
                if (_currentTurn > DifficultyStats[_currentDifficultyIndex].BestTurns)
                    DifficultyStats[_currentDifficultyIndex].BestTurns = _currentTurn;
                break;
            case "Lose":
                DifficultyStats[_currentDifficultyIndex].LosesCounter++;
                if (DifficultyStats[_currentDifficultyIndex].LastGame == lastGame)
                    DifficultyStats[_currentDifficultyIndex].LosesStreak++;
                if (DifficultyStats[_currentDifficultyIndex].LosesStreak > DifficultyStats[_currentDifficultyIndex].LosesInRow++)
                    DifficultyStats[_currentDifficultyIndex].LosesInRow = DifficultyStats[_currentDifficultyIndex].LosesStreak;
                DifficultyStats[_currentDifficultyIndex].LastGame = lastGame;
                break;
        }
        SaveStatistic();
    }

    private void CreateGame()
    {
        var cur = _difficultyPresets[_currentDifficultyIndex];
        _maxMines = cur.Mines;
        _maxFlags = _maxMines;
        _currentFlags = 0;
        _currentTurn = 0;
        _mineCounter = 0;
        _minefield = null;
        _minefield = new Cell[cur.Rows, cur.Columns];
        for (int i = 0; i < _minefield.GetLength(0); i++)
        {
            for (int j = 0; j < _minefield.GetLength(1); j++)
            {
                _minefield[i, j] = new Cell();
            }
        }
    }

    private void CreateMinefield()
    {
        while (_mineCounter < _maxMines)
        {
            int i = _rand.Next(0, _minefield.GetLength(0));
            int j = _rand.Next(0, _minefield.GetLength(1));
            if (_minefield[i, j].MakeNewMine())
            {
                _mineCounter++;
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
                _minefield[newRow, newCol].IncreaseNumber();
        }
    }
    private bool IsValid(int x, int y) => _minefield != null && x >= 0 && x < _minefield.GetLength(0) && y >= 0 && y < _minefield.GetLength(1);
    public class GameSettings
    {
        public int CurrentDifficultyIndex { get; set; }
        public DifficultyDto? SpecialPreset { get; set; }
    }
    public class DifficultyDto
    {
        public int Rows { get; set; }
        public int Columns { get; set; }
        public int Mines { get; set; }
    }
    public void FolderExistingCheck()
    {
        if (!Directory.Exists(_folderPath))
            Directory.CreateDirectory(_folderPath);
    }
    private void SaveSettings()
    {
        FolderExistingCheck();
        var settingsObj = new
        {
            CurrentDifficultyIndex = _currentDifficultyIndex,
            SpecialPreset = new
            {
                _difficultyPresets[3].Rows,
                _difficultyPresets[3].Columns,
                _difficultyPresets[3].Mines
            }
        };
        string json = JsonSerializer.Serialize(settingsObj);
        File.WriteAllText(_settings, json);
    }
    private void LoadSettings()
    {
        if (!File.Exists(_settings)) return;
        string json = File.ReadAllText(_settings);
        var settingsObj = JsonSerializer.Deserialize<GameSettings>(json);
        _currentDifficultyIndex = settingsObj.CurrentDifficultyIndex;
        _difficultyPresets[3].Rows = settingsObj.SpecialPreset.Rows;
        _difficultyPresets[3].Columns = settingsObj.SpecialPreset.Columns;
        _difficultyPresets[3].Mines = settingsObj.SpecialPreset.Mines;
    }
    private class Statistic
    {
        public int GamesCounter = 0;
        public int WinsCounter = 0;
        public int LosesCounter = 0;
        public float WinsPercent => GamesCounter == 0 ? 0 : (float)WinsCounter / GamesCounter * 100f;
        public int WinsStreak = 0;
        public int WinsInRow = 0;
        public int LosesStreak = 0;
        public int LosesInRow = 0;
        public string LastGame = "Нет данных о последней игре.";
        public int BestTurns = 0;
        public override string ToString()
        {
            return
                $"Всего игр: {GamesCounter}\n" +
                $"Всего побед: {WinsCounter}\n" +
                $"Всего проигрышей: {LosesCounter}\n" +
                $"Процент побед: {WinsPercent:F2}%\n" +
                $"Побед подряд: {WinsInRow}\n" +
                $"Проигрышей подряд: {LosesInRow}\n" +
                $"Минимальное количество ходов для победы: {BestTurns}\n" +
                $"Последняя игра: {LastGame}";
        }
    }
    Statistic[] DifficultyStats =
    {
        new Statistic(),
        new Statistic(),
        new Statistic(),
        new Statistic()
    };
    private void ShowStats()
    {
        for (int i = 0; i < 4; i++)
        {
            Console.WriteLine($"{i + 1}. {_difficultyPresets[i].Name}\n{DifficultyStats[i]}");
            Console.WriteLine("----------------------------------------------");
        }
    }
    private void SaveStatistic()
    {
        FolderExistingCheck();
        var settingsObj = DifficultyStats;
        string json = JsonSerializer.Serialize(settingsObj);
        File.WriteAllText(_statistic, json);
    }
    private void LoadStatistic()
    {
        if (!File.Exists(_statistic)) return;
        string json = File.ReadAllText(_statistic);
        var settingsObj = JsonSerializer.Deserialize<Statistic[]>(json);
        if (settingsObj != null)
            DifficultyStats = settingsObj;
    }
}