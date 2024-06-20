# CrabGameSequenceTester
A small BepInEx mod for Crab Game that allows you to test sequences for the CrabGame+ Sequenced Drop custom game mode.

# Usage
In order to use this mod, you will first need to properly add BepInEx Il2Cpp to Crab Game. Once you have done that, you can put the SequenceTester.dll from the latest release in your plugins folder and start your game. This will create a "SequencedDropSequences" folder in the same folder you have the .dll file in.

Once the "SequencedDropSequences" folder has been created, you can put any sequences you want into it (you can get some examples from [here](https://github.com/lammas321/CrabGameSequenceTester/tree/main/SequenceExamples)) and start a practice lobby. Once in, you may need to type "!rel" (reload) to make the mod recognize the added sequences, and then you can type "!seq <sequence name>" (play sequence) to play out a sequence by name! This will also drop revolvers with infinite ammo at every player's feet, and prevent any other sequences from being played until that sequence finishes.

If the blocks start getting too high, you can use "!res" (restart) to restart the game, clearing all the blocks and stopping the active sequence if there is one.


# Making Sequences
Sequences use a semi-custom format to decide how they will function. The first few lines (before the "Sequence:" line) act as a header, describing some information about the sequence, such as its name, how many layers up it goes, its min supported players, and its max supported players.

The name can be anything you like (as long as it only uses ASCII characters, as those are the only characters that can properly appear in chat).

The sequence's height should be the exact about of layers it ends up using, it being too high or low can cause the game to end too soon or late.

The min supported players allows you to prevent the sequence from being played with a low number of players, useful if the sequence is generally easy and has lots of room. Set to -1 to have no minimum.

The max supported players allows you to prevent the sequence from being player with a high number of players, useful if the sequence is more difficult or doesn't have lots of room. Set to -1 to have no maximum.

After the "Sequence:" line is where sequence is played out, there are currently 3 instructions:

## Drop
Drop=<dropPosition: int>, <dropTime: float>

The Drop instruction takes in a position and a time to drop.

The position ranges from 0-15 (inclusive) with 0 starting in the top left corner and going right then down.

The drop time is roughly how long the drop will take to reach the floor. Anything you enter multiplied by about 4 is how long it'll take to reach the bottom in seconds (roughly 1/4 being growing in size, and the other 3/4 being frozen for a short time and falling).

## MultiDrop
MultiDrop=<dropPositions: int[]>, <dropTime: float>, [dropCount: int = 1], [waitTime: float = 1f]

The MultiDrop instruction takes in multiple positions and a drop time, as well as an option drop count (default 1) and an option wait time between drops (default 1).

The positions work the same as the Drop instruction's postion, except everything in the array will drop at the same time.

The drop time works the same as the Drop instruction's drop time.

The drop count will drop blocks at all drop positions that many times with a delay of wait time between them.

The wait time is how long to wait in seconds between repeated drops when drop count is larger than 1.

## Wait
Wait=<waitTime: float>

The Wait instruction takes in a wait time, which is how long in seconds to wait before executing the next instruction.


# Notes
It can be difficult to understand how to make your own sequences, so I highly recommend testing the examples like Chess and looking into it's file to figure out how it works first!

If you have any questions or think something should be better clarified here, feel free to let me know on [Discord](https://discord.gg/jBGMZqndT3)!

The actual Sequenced Drop game mode will randomly rotate and flip sequences before they are played, making the experience not as repetitive, but this SequenceTester mod will not do that, making testing and debugging sequences easier!

If you make a cool sequence and would like to see it when I host, show me! I'd be happy to add your sequences!
