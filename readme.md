# WPF Bingo - ビンゴ抽選機

A fun and interactive bingo number drawing application built with WPF (Windows Presentation Foundation).

## Features

- **Random Number Drawing**: Draw numbers from 1 to 75 in random order
- **Fun Animations**: Newly drawn numbers are displayed with engaging scale and fade animations
- **Clear History Display**: View all previously drawn numbers in an easy-to-read format with animated number balls
- **Reset Functionality**: Start a new game at any time
- **Bilingual UI**: Japanese and English labels for better accessibility

## Requirements

- .NET 10.0 or later
- Windows operating system

## How to Build

```bash
cd WpfBingo
dotnet restore
dotnet build
```

## How to Run

```bash
cd WpfBingo
dotnet run
```

Or simply open the solution in Visual Studio and press F5.

## How to Use

1. Click the "抽選 / Draw" button to draw a new random number
2. The number will be displayed with an animation in the center
3. All drawn numbers are shown in the history section at the bottom
4. Click "リセット / Reset" to start a new game

## Technical Details

- Built with .NET 10.0 and WPF
- Follows MVVM (Model-View-ViewModel) architecture
- Uses data binding for reactive UI updates
- Implements custom animations using WPF Storyboards
- Material Design inspired color scheme

## License

This project is open source and available under the MIT License.
