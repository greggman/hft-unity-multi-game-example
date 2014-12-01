HFT-Unity-Multi-Game-Example
============================

The point of this sample is to show running multiple games that pass players between each other.
This allows you to make a game that spans multiple computers

This sample **MUST BE RUN FROM THE COMMAND LINE**. The reason is each instance needs an ID and so
running by clicking or from an editor would require editing that id. To run from the command
line you must EXPORT an executable.  With Unity closed, open a Terminal/Command Prompt and type
`hft export` to automatically export

The on OSX these command lines would launch 3 windows in the local machine

    bin/unitymultigame-osx.app/Contents/MacOS/unitymultigame-osx --hft-id=game0 --num-games=3
    bin/unitymultigame-osx.app/Contents/MacOS/unitymultigame-osx --hft-id=game1 --num-games=3
    bin/unitymultigame-osx.app/Contents/MacOS/unitymultigame-osx --hft-id=game2 --num-games=3

If you run those commands and then open a browser window to http://localhost:18679 a player should
appear. Jumping off the left / right edges of the screen should make you go to the next window.

If you were running on multiple machines the 3 machines might use lines like this

*   machine 1

        bin/unitymultigame-osx.app/Contents/MacOS/unitymultigame-osx --hft-id=game0 --num-games=3 --hft-url=ws://192.168.1.9:18679 --hft-master

*   machine 2

        bin/unitymultigame-osx.app/Contents/MacOS/unitymultigame-osx --hft-id=game1 --num-games=3 --hft-url=ws://192.168.1.9:18679

*   machine 3

        bin/unitymultigame-osx.app/Contents/MacOS/unitymultigame-osx --hft-id=game2 --num-games=3 --hft-url=ws://192.168.1.9:18679

where `192.68.1.9` is the IP address of the machine running HappyFunTimes.

All machines must have the game the same path AND whatever machine is running HappyFunTimes aos needs a copy of this repo at the exact
same PATH as these machines.

This particular game just runs a simple level and when you jump off the right side of the screen it transfers that player to the next game.
In other words, if you are on `game1` and you go off the right side of the screen you'll be sent to `game2`. Conversely if you jump off the
left side you'd be send to `game0`. `--num-games` determines when to wrap around so in the above example if you're on `game2` and go off the
right you'll be sent to `game0`.


Of course in your own game you could put up a dialog box or something and ask for an ID but
most people running multiple machine games are probably doing so in a setting they want to
automate and so using the command line seems like the best way.




