# Pong
First graphical game in C#. Using MonoGame for now.

This iteration of pong implements a simple prediction-algorithm for the opponent's AI paddle, a user-controlled paddle, and a bouncing ball. 

At this point in time, the visuals are just the collision masks. Future iterations will include graphics/animations, sound effects, and actual win/lose win conditions.

![image](https://user-images.githubusercontent.com/7895936/169744696-52a71b34-10ed-4743-a1dc-c19639ff4969.png)

## Important notes

Need to make sure the PATHEXT includes .exe so that dotnet CLI can be executed without adding the .exe extension. The monogame tools depend on dotnet CLI as a dependency.

![image](https://user-images.githubusercontent.com/7895936/169742816-29e28393-4370-4d8e-8d67-197a0015539b.png)

- 1). Install the templates with `dotnet new --install MonoGame.Templates.CSharp`.
- 2). Create a new Visual Studios MonoGame project with `dotnet new mgdesktopgl`, within the folder that will contain the game.
- 3). Add the MonoGame framework with `dotnet new mgdesktopgl`.
- 4). Open up project with Visual Studio 2022.
