using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace SequenceTester
{
    internal static class SequencedDrop
    {
        internal static readonly string SequencedDropSequencesPath = "SequencedDropSequences\\";
        internal static readonly int MaxHeight = 26;

        internal enum DropPosition
        {
            Pos0 = 1,
            Pos1 = 11,
            Pos2 = 0,
            Pos3 = 10,

            Pos4 = 18,
            Pos5 = 7,
            Pos6 = 19,
            Pos7 = 8,

            Pos8 = 16,
            Pos9 = 5,
            Pos10 = 15,
            Pos11 = 4,

            Pos12 = 14,
            Pos13 = 3,
            Pos14 = 13,
            Pos15 = 2,
        }
        internal struct InstructionDataDrop(DropPosition dropPosition, float dropTime) : IInstructionData
        {
            public DropPosition dropPosition = dropPosition;
            public float dropTime = dropTime;
        }
        internal struct InstructionDataMultiDrop(DropPosition[] dropPositions, float dropTime, int dropCount, float waitTime) : IInstructionData
        {
            public DropPosition[] dropPositions = dropPositions;
            public float dropTime = dropTime;
            public int dropCount = dropCount;
            public float waitTime = waitTime;
        }
        internal struct InstructionDataWait(float waitTime) : IInstructionData
        {
            public float waitTime = waitTime;
        }
        internal interface IInstructionData;
        internal enum InstructionType
        {
            Drop,
            MultiDrop,
            Wait
        }
        internal struct Instruction(InstructionType instructionType, IInstructionData instructionData)
        {
            public InstructionType instructionType = instructionType;
            public IInstructionData instructionData = instructionData;
        }
        internal struct Sequence(string name, int height, int minPlayers, int maxPlayers, Instruction[] instructions)
        {
            public string name = name;
            public int height = height;
            public int minPlayers = minPlayers;
            public int maxPlayers = maxPlayers;
            public Instruction[] instructions = instructions;
        }

        internal static List<Sequence> sequences = [];
        internal static bool playing = false;

        internal static DropPosition IndexToDropPosition(int index)
        {
            switch (index)
            {
                case 0:
                    return DropPosition.Pos0;
                case 1:
                    return DropPosition.Pos1;
                case 2:
                    return DropPosition.Pos2;
                case 3:
                    return DropPosition.Pos3;
                case 4:
                    return DropPosition.Pos4;
                case 5:
                    return DropPosition.Pos5;
                case 6:
                    return DropPosition.Pos6;
                case 7:
                    return DropPosition.Pos7;
                case 8:
                    return DropPosition.Pos8;
                case 9:
                    return DropPosition.Pos9;
                case 10:
                    return DropPosition.Pos10;
                case 11:
                    return DropPosition.Pos11;
                case 12:
                    return DropPosition.Pos12;
                case 13:
                    return DropPosition.Pos13;
                case 14:
                    return DropPosition.Pos14;
                case 15:
                    return DropPosition.Pos15;
            }
            return DropPosition.Pos0;
        }
        internal static void Drop(DropPosition dropPosition, float dropTime)
        {
            foreach (ulong clientId in LobbyManager.steamIdToUID.Keys)
                ServerSend.SendBlockCrush(1 / dropTime, (int)dropPosition, clientId);
        }

        internal static void Load()
        {
            string sequencedDropSequencesPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase)[6..], SequencedDropSequencesPath);
            if (!Directory.Exists(sequencedDropSequencesPath))
                Directory.CreateDirectory(sequencedDropSequencesPath);

            sequences = [];
            foreach (string file in Directory.GetFiles(sequencedDropSequencesPath))
            {
                string[] lines = File.ReadAllLines(file);
                int sequencesIndex = -1;
                for (int index = 0; index < lines.Length; index++)
                    if (lines[index].Trim().ToLower() == "sequence:")
                    {
                        sequencesIndex = index;
                        break;
                    }
                if (sequencesIndex == -1) continue;

                string name = "Unnamed";
                int height = 1;
                int minPlayers = -1;
                int maxPlayers = -1;
                for (int index = 0; index < sequencesIndex; index++)
                {
                    string line = lines[index].Trim();
                    if (line == "" || line.StartsWith('#')) continue;
                    int splitIndex = line.IndexOf('=');
                    if (splitIndex == -1) continue;

                    string key = line[..splitIndex].ToLower();
                    string value = line[(splitIndex + 1)..];

                    switch (key)
                    {
                        case "name":
                            {
                                name = value;
                                break;
                            }
                        case "height":
                            {
                                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue))
                                    height = Math.Max(1, intValue);
                                break;
                            }
                        case "minplayers":
                            {
                                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue))
                                    minPlayers = Math.Max(-1, intValue);
                                break;
                            }
                        case "maxplayers":
                            {
                                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue))
                                    maxPlayers = Math.Max(-1, intValue);
                                break;
                            }
                    }
                }

                List<Instruction> instructions = [];
                for (int index = sequencesIndex + 1; index < lines.Length; index++)
                {
                    string line = lines[index].Trim().ToLower();
                    if (line == "" || line.StartsWith('#')) continue;
                    int splitIndex = line.IndexOf('=');
                    if (splitIndex == -1) continue;

                    string key = line[..splitIndex];
                    string value = line[(splitIndex + 1)..].Replace(" ", "");

                    switch (key)
                    {
                        case "drop":
                            {
                                string[] properties = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                                if (properties.Length <= 1) continue;

                                if (!int.TryParse(properties[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue)) break;
                                if (float.TryParse(properties[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue))
                                    instructions.Add(new Instruction(InstructionType.Drop, new InstructionDataDrop(IndexToDropPosition(Math.Clamp(intValue, 0, 15)), Math.Max(float.Epsilon, floatValue))));
                                break;
                            }
                        case "multidrop":
                            {
                                if (!value.StartsWith('[')) break;
                                int multiDropEndIndex = value.IndexOf(']');
                                if (multiDropEndIndex == -1) break;

                                string[] multiDropIndexes = value[1..multiDropEndIndex].Split(',', StringSplitOptions.RemoveEmptyEntries);
                                List<DropPosition> dropPositions = [];
                                bool encounteredError = false;
                                foreach (string dropIndexValue in multiDropIndexes)
                                {
                                    if (!int.TryParse(dropIndexValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue))
                                    {
                                        encounteredError = true;
                                        break;
                                    }
                                    dropPositions.Add(IndexToDropPosition(Math.Clamp(intValue, 0, 15)));
                                }
                                if (encounteredError) break;

                                string[] properties = value[(multiDropEndIndex + 1)..].Split(',', StringSplitOptions.RemoveEmptyEntries);
                                if (properties.Length == 0) continue;
                                if (!float.TryParse(properties[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue)) break;
                                int dropCount = 1;
                                if (properties.Length >= 2 && !int.TryParse(properties[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out dropCount)) break;
                                float waitTime = 1f;
                                if (properties.Length >= 3 && !float.TryParse(properties[2], NumberStyles.Float, CultureInfo.InvariantCulture, out waitTime)) break;

                                instructions.Add(new Instruction(InstructionType.MultiDrop, new InstructionDataMultiDrop([.. dropPositions], Math.Max(float.Epsilon, floatValue), Math.Max(1, dropCount), Math.Max(float.Epsilon, waitTime))));
                                break;
                            }
                        case "wait":
                            {
                                if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue))
                                    instructions.Add(new Instruction(InstructionType.Wait, new InstructionDataWait(Math.Max(float.Epsilon, floatValue))));
                                break;
                            }
                    }
                }

                sequences.Add(new Sequence(name, height, minPlayers, maxPlayers, [.. instructions]));
            }
        }
        internal static IEnumerator ProcessSequence(Sequence sequence)
        {
            playing = true;
            foreach (ulong clientId in GameManager.Instance.activePlayers.Keys)
                if (!GameManager.Instance.activePlayers[clientId].dead)
                    ServerSend.DropItem(clientId, 2, SharedObjectManager.Instance.GetNextId(), int.MaxValue);

            ServerSend.SendChatMessage(1, $"Current Sequence: {sequence.name}");
            foreach (Instruction instruction in sequence.instructions)
                switch (instruction.instructionType)
                {
                    case InstructionType.Drop:
                        {
                            InstructionDataDrop drop = (InstructionDataDrop)instruction.instructionData;
                            Drop(drop.dropPosition, drop.dropTime);
                            break;
                        }
                    case InstructionType.MultiDrop:
                        {
                            InstructionDataMultiDrop multiDrop = (InstructionDataMultiDrop)instruction.instructionData;
                            for (int dropped = 0; dropped < multiDrop.dropCount; dropped++)
                            {
                                if (dropped != 0 && multiDrop.waitTime > 0f)
                                    yield return new WaitForSeconds(multiDrop.waitTime);
                                foreach (DropPosition dropPosition in multiDrop.dropPositions)
                                    Drop(dropPosition, multiDrop.dropTime);
                            }
                            break;
                        }
                    case InstructionType.Wait:
                        {
                            InstructionDataWait wait = (InstructionDataWait)instruction.instructionData;
                            if (wait.waitTime > 0f)
                                yield return new WaitForSeconds(wait.waitTime);
                            break;
                        }
                }
            playing = false;
        }
    }
}