using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorGuess
{
    public class ColorGuessLogic
    {
        public static int CustomRandInt(ref int seed)
        {
            seed = (214013 * seed + 2531011);
            return (seed >> 16) & 0x7FFF;
        }

        public static void Allocate(ref UserData userData)
        {
            userData.Guesses = new byte[7][];
            userData.Results = new byte[7][];
            for (int i = 0; i < 7; i++)
            {
                userData.Guesses[i] = new byte[4];
                userData.Results[i] = new byte[4];
            }

            userData.PickedColors = new byte[4];
        }

        public static unsafe void StartGame(ref UserData userData, int seed)
        {
            int* colorIndices = stackalloc int[6];
            for (int i = 0; i < 6; i++)
                colorIndices[i] = (i + 1);
            for (int i = 0; i < 6; i++)
            {
                int newIndex = CustomRandInt(ref seed) % 6;
                int oldColor = colorIndices[i];
                colorIndices[i] = colorIndices[newIndex];
                colorIndices[newIndex] = oldColor;
            }

            for (int i = 0; i < 4; i++)
                userData.PickedColors[i] = (byte)colorIndices[i];

            for (int i = 0; i < 7; i++)
                for (int j = 0; j < 4; j++)
                {
                    userData.Guesses[i][j] = 0;
                    userData.Results[i][j] = 0;
                }

            userData.GuessRow = 0;
        }

        public static void AddGuessColor(ref UserData userData, byte color)
        {
            for (int i = 0; i < 4; i++)
                if (userData.Guesses[userData.GuessRow][i] == 0)
                {
                    userData.Guesses[userData.GuessRow][i] = color;
                    if (i == 3)
                        SubmitGuess(ref userData);
                    return;
                }
        }

        public static void SubmitGuess(ref UserData userData)
        {
            for (int i = 0; i < 4; i++)
                userData.Results[userData.GuessRow][0] = 0;

            int guessCount = 0;
            for (int i = 0; i < 4; i++)
            {
                if (userData.Guesses[userData.GuessRow][i] == userData.PickedColors[i])
                    userData.Results[userData.GuessRow][guessCount++] = 2;
                else
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (i != j && userData.Guesses[userData.GuessRow][i] == userData.PickedColors[j])
                            userData.Results[userData.GuessRow][guessCount++] = 1;
                    }
                }
            }

            userData.GuessRow++;
        }

        public static int Undo(UserData userData)
        {
            for (int i = 3; i >= 0; i--)
                if (userData.Guesses[userData.GuessRow][i] > 0)
                {
                    int color = userData.Guesses[userData.GuessRow][i];
                    userData.Guesses[userData.GuessRow][i] = 0;
                    return color;
                }
            return 0;
        }

        public static bool CheckWin(UserData userData, int row)
        {
            for (int i = 0; i < 4; i++)
                if (userData.Results[row][i] != 2)
                    return false;
            return true;
        }

        public static bool CheckGameOver(UserData userData)
        {
            return (userData.GuessRow >= 7);
        }
    }
}