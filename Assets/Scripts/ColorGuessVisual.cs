using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGuess
{
    public class ColorGuessVisual : MonoBehaviour
    {
        public GameManager GameManager;

        public enum GAME_STATE { IN_GAME, VICTORY, GAME_OVER };
        public GAME_STATE GameState;

        public Sprite[] Colors;
        public Sprite NoColor;
        public Image[] PickedColors;
        public GameObject[] QuestionMarks;
        public ColorGuessRow[] ColorGuessRows;
        public Sprite ColorExistsRightPosition;
        public Sprite ColorExistsWrongPosition;
        public Sprite ColorDoesNotExist;
        public Sprite ColorNotYetGuessed;
        public GameObject[] ColorButtons;

        public GameObject ButtonSelection;
        public GameObject NewGameButton;
        public Text WinText;
        public Text GuessText;
        public GameObject GameOverText;
        public GameObject UndoButton;
        public GameObject QuitButton;
        public Text TimerText;
        public Text BestTimeText;

        float m_time;

        private void Awake()
        {
            for (int i = 0; i < 7; i++)
                for (int j = 0; j < 4; j++)
                {
                    int localJ = j;
                    ColorGuessRows[i].ColorButtons[j].onClick.AddListener(() => { GameManager.Instance.SetColorIndex(localJ); });
                }

            TimerText.gameObject.SetActive(false);
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        public void StartGame(UserData userData)
        {
            for (int i = 0; i < 4; i++)
            {
                PickedColors[i].sprite = NoColor;
                QuestionMarks[i].SetActive(true);
            }

            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    ColorGuessRows[i].Colors[j].sprite = NoColor;
                    ColorGuessRows[i].Results[j].sprite = ColorNotYetGuessed;
                    ColorGuessRows[i].Selection[j].SetActive(false);
                }
            }

            for (int i = 0; i < 6; i++)
                ColorButtons[i].SetActive(true);

            ButtonSelection.SetActive(true);
            NewGameButton.SetActive(false);
            GameOverText.SetActive(false);
            UndoButton.SetActive(false);
            QuitButton.SetActive(false);
            WinText.gameObject.SetActive(false);

            GuessText.gameObject.SetActive(true);
            string title = "";
            int colorIndex = 0;
            GameManager.Instance.AddColorWordToTitle("GUESS THE COLORS", ref title, ref colorIndex);
            GuessText.text = title;

            m_time = 0.0f;

            GameState = GAME_STATE.IN_GAME;

            float bestTime = PlayerPrefs.GetFloat("BestTime");
            TimeSpan timeSpan = TimeSpan.FromSeconds(bestTime);
            BestTimeText.text = "BEST TIME\n" + "<SIZE=60>" + timeSpan.ToString(@"mm\:ss") + "</SIZE>";
            BestTimeText.gameObject.SetActive(false);

            UpdateGuessColors(userData, userData.GuessRow);

            // TESTING
            //ShowPickedColors(userData);
        }

        public void ShowWin(UserData userData)
        {
            ButtonSelection.SetActive(false);
            NewGameButton.SetActive(true);
            GameOverText.SetActive(false);
            UndoButton.SetActive(false);
            QuitButton.SetActive(true);
            GuessText.gameObject.SetActive(false);
            WinText.gameObject.SetActive(true);
            string title = "";
            int colorIndex = 0;
            GameManager.Instance.AddColorWordToTitle("YOU WIN!", ref title, ref colorIndex);
            WinText.text = title;

            for (int i = 0; i < 4; i++)
                QuestionMarks[i].SetActive(false);

            ShowPickedColors(userData);

            GameState = GAME_STATE.VICTORY;

            float bestTime = PlayerPrefs.HasKey("BestTime") ? PlayerPrefs.GetFloat("BestTime") : float.MaxValue;
            if (bestTime > m_time)
                PlayerPrefs.SetFloat("BestTime", m_time);
        }

        public void ShowGameOver(UserData userData)
        {
            ButtonSelection.SetActive(false);
            NewGameButton.SetActive(true);
            GuessText.gameObject.SetActive(false);
            WinText.gameObject.SetActive(false);
            GameOverText.SetActive(true);
            UndoButton.SetActive(false);
            QuitButton.SetActive(true);

            for (int i = 0; i < 4; i++)
                QuestionMarks[i].SetActive(false);

            ShowPickedColors(userData);

            GameState = GAME_STATE.GAME_OVER;
        }

        public void SelectColor(ref UserData userData, int color)
        {
            ColorButtons[color - 1].SetActive(false);

            int currentRow = userData.GuessRow;
            ColorGuessLogic.AddGuessColor(ref userData, (byte)color);
            UpdateGuessColors(userData, currentRow);
            if (userData.GuessRow > currentRow)
            {
                ShowPrevGuessResults(userData);
                for (int i = 0; i < 6; i++)
                    ColorButtons[i].SetActive(true);
                for (int i = 0; i < 4; i++)
                    ColorGuessRows[currentRow].Selection[i].SetActive(false);
                UndoButton.SetActive(false);
                if (!ColorGuessLogic.CheckGameOver(userData))
                    UpdateGuessColors(userData, userData.GuessRow);
            }
            else
                UndoButton.SetActive(true);

            if (ColorGuessLogic.CheckWin(userData, currentRow))
                ShowWin(userData);
            else if (ColorGuessLogic.CheckGameOver(userData))
                ShowGameOver(userData);
        }

        public void ShowPickedColors(UserData userData)
        {
            // for testing
            for (int i = 0; i < 4; i++)
            {
                int colorIndex = userData.PickedColors[i];
                PickedColors[i].sprite = (colorIndex > 0) ? Colors[colorIndex - 1] : NoColor;
            }

        }

        public void UpdateGuessColors(UserData userData, int row)
        {
            for (int i = 0; i < 4; i++)
            {
                ColorGuessRows[row].Colors[i].gameObject.SetActive(true);

                int colorIndex = userData.Guesses[row][i];
                ColorGuessRows[row].Colors[i].sprite = (colorIndex > 0) ? Colors[colorIndex - 1] : NoColor;
                ColorGuessRows[row].Selection[i].SetActive(userData.ColorIndex == i);
            }
        }

        public void ShowPrevGuessResults(UserData userData)
        {
            int offset = (int)(UnityEngine.Random.value * 4);
            for (int i = 0; i < 4; i++)
            {
                int index = (i + offset) % 4;
                switch (userData.Results[userData.GuessRow - 1][index])
                {
                    case 0:
                        ColorGuessRows[userData.GuessRow - 1].Results[index].sprite = ColorNotYetGuessed;
                        break;
                    case 1:
                        ColorGuessRows[userData.GuessRow - 1].Results[index].sprite = ColorExistsWrongPosition;
                        break;
                    case 2:
                        ColorGuessRows[userData.GuessRow - 1].Results[index].sprite = ColorExistsRightPosition;
                        break;
                }
            }
        }

        public void Undo(ref UserData userData)
        {
            int undoColorIndex = ColorGuessLogic.Undo(ref userData);
            if (undoColorIndex > 0)
            {
                UpdateGuessColors(userData, userData.GuessRow);
                ColorButtons[undoColorIndex - 1].SetActive(true);
            }

            UndoButton.SetActive(userData.GuessCount > 0);
        }

        public void UndoSpecificColorIndex(ref UserData userData, int colorIndex)
        {
            int undoColorIndex = ColorGuessLogic.UndoColorIndex(ref userData, colorIndex);
            if (undoColorIndex > 0)
            {
                UpdateGuessColors(userData, userData.GuessRow);
                ColorButtons[undoColorIndex - 1].SetActive(true);
            }

            UndoButton.SetActive(userData.GuessCount > 0);
        }

        public void Tick(float dt)
        {
            if (GameState == GAME_STATE.IN_GAME)
            {
                m_time += dt;
                TimeSpan timeSpan = TimeSpan.FromSeconds(m_time);
                TimerText.text = timeSpan.ToString(@"mm\:ss");
            }
        }
    }
}