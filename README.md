# MineSweeper (Console)

Консольная версия игры «Сапёр» с логикой расстановки мин, флагов, открытия клеток и разными уровнями сложности.

[![.NET CI](https://github.com/cut9/MineSweeper/actions/workflows/dotnet-ci-windows.yml/badge.svg)](https://github.com/cut9/MineSweeper/actions)
[![Release](https://img.shields.io/github/v/release/cut9/MineSweeper)](https://github.com/cut9/MineSweeper/releases)

---

## Информация
- Платформа: кроссплатформенная консоль (.NET)
- Язык: C# .NET 8
- Тип: Консольное приложение (CLI)

---

## Быстрый старт — запустить релиз (рекомендуется для обычных пользователей)

1. Перейдите в раздел **Releases**: https://github.com/cut9/MineSweeper/releases  
2. Скачайте архив `win-x64.rar` для вашей платформы.  
3. Распакуйте архив и дважды кликните `MineSweeper.exe`.

---

## Быстрый старт — запустить из исходников (для разработчиков)

### Команды (PowerShell / CMD)
```powershell
git clone https://github.com/cut9/MineSweeper.git
cd MineSweeper
dotnet restore
dotnet run --project ./MineSweeper/MineSweeper.csproj
```
