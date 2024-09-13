# Boo! Greedy Kid

**Version**: v11

**Visual Studio requirements**: version 2022 with the .NET 8.0 workload.

This is the main repository for [Boo! Greedy Kid](https://store.steampowered.com/app/770630/Boo_Greedy_Kid/) on all platforms (Windows, macOS, and Linux) and its level editor (Windows-only).

Boo! Greedy Kid is coded in C# with [MonoGame](https://monogame.net/) 3.8.1, and uses native compilation on all platforms when published (using .NET ```PublishAot```).

A post-build script will create a macOS bundle automatically when using ```dotnet publish -r osx-x64```.

## All rights reserved

Boo! Greedy Kid © 2017-2024 Flying Oak Games. All rights reserved.

This is **not** a free software. You can't use code or assets for your own projects.

Redistributed third party software in this repository (that do not apply to Flying Oak Games' copyright):
- DotNetZip, under MS-PL license
- Steamworks.NET, under MIT license

## Educational fair use

Flying Oak Games is allowing derivative works for **educational purposes** only (e.g. learning MonoGame or learning to make games) under these conditions:
- No commercial use can be made
- Obligation to credit Flying Oak Games
- Obligation to detail which part is being used or modified

## Repository content

This repository contains both the game source code and its level editor.

Both comes with an optional Steam integration (achievements and workshop support). Please mind to use your own AppID in both ```SteamworksHelper.cs``` and ```steam_appid.txt```.

## This is not a community project

This repository sole purpose is for educational use.

We don't accept pull requests.

We don't accept comments, questions, or bug reports.

## Please consider thanking us

The game remains a commercial project. If you are using repository, please consider buying the game on Steam to support us: [Boo! Greedy Kid on Steam](https://store.steampowered.com/app/770630/Boo_Greedy_Kid/).
