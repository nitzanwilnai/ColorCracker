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
        public Color ColorExistsRightPosition;
        public Color ColorExistsWrongPosition;
        public Color ColorDoesNotExist;
        public Color ColorNotYetGuessed;
        public GameObject[] ColorButtons;

        public GameObject ButtonSelection;
        public GameObject NewGameButton;
        public GameObject WinText;
        public GameObject GameOverText;
        public GameObject UndoButton;
        public Text TimerText;
        public Text BestTimeText;

        float m_time;

        private void Awake()
        {
            for (int i = 0; i < 7; i++)
                for (int j = 0; j < 4; j++)
                    ColorGuessRows[i].ColorButtons[j].onClick.AddListener(GameManager.Undo);
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        public void StartGame()
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
                    ColorGuessRows[i].Results[j].color = ColorNotYetGuessed;
                }
            }

            for (int i = 0; i < 6; i++)
                ColorButtons[i].SetActive(true);

            ButtonSelection.SetActive(true);
            NewGameButton.SetActive(false);
            WinText.SetActive(false);
            GameOverText.SetActive(false);
            UndoButton.SetActive(false);

            m_time = 0.0f;

            GameState = GAME_STATE.IN_GAME;

            float bestTime = PlayerPrefs.GetFloat("BestTime");
            TimeSpan timeSpan = TimeSpan.FromSeconds(bestTime);
            BestTimeText.text = "BEST TIME\n" + "<SIZE=60>"+ timeSpan.ToString(@"mm\:ss") + "</SIZE>";
            BestTimeText.gameObject.SetActive(true);
        }

        public void ShowWin(UserData userData)
        {
            ButtonSelection.SetActive(false);
            NewGameButton.SetActive(true);
            WinText.SetActive(true);
            GameOverText.SetActive(false);
            UndoButton.SetActive(false);

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
            WinText.SetActive(false);
            GameOverText.SetActive(true);
            UndoButton.SetActive(false);

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
                UndoButton.SetActive(false);
            }
            else
                UndoButton.SetActive(true);

            if (ColorGuessLogic.CheckWin(userData, currentRow))
                ShowWin(userData);

            if (ColorGuessLogic.CheckGameOver(userData))
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
                int colorIndex = userData.Guesses[row][i];
                ColorGuessRows[row].Colors[i].sprite = (colorIndex > 0) ? Colors[colorIndex - 1] : NoColor;
            }
        }

        public void ShowPrevGuessResults(UserData userData)
        {
            int offset = (int)(UnityEngine.Random.value * 4);
            for (int i = 0; i < 4; i++)
            {
                int index = (i + offset) % 4;
                switch(userData.Results[userData.GuessRow - 1][index])
                {
                    case 0:
                        ColorGuessRows[userData.GuessRow - 1].Results[index].color = ColorNotYetGuessed;
                        break;
                    case 1:
                        ColorGuessRows[userData.GuessRow - 1].Results[index].color = ColorExistsWrongPosition;
                        break;
                    case 2:
                        ColorGuessRows[userData.GuessRow - 1].Results[index].color = ColorExistsRightPosition;
                        break;
                }
            }
        }

        public void Undo(UserData userData)
        {
            int undoColorIndex = ColorGuessLogic.Undo(userData);
            if(undoColorIndex > 0)
            {
                UpdateGuessColors(userData, userData.GuessRow);
                ColorButtons[undoColorIndex-1].SetActive(true);
            }

            UndoButton.SetActive(userData.Guesses[userData.GuessRow][0] > 0);
        }

        public void HandleInput()
        {
#if UNITY_EDITOR
            bool mouseDown = Input.GetMouseButtonDown(0);
            bool mouseMove = Input.GetMouseButton(0);
            bool mouseUp = Input.GetMouseButtonUp(0);
            Vector3 mousePosition = Input.mousePosition;
#else
            bool mouseDown = (Input.touchCount > 0) && Input.GetTouch(0).phase == TouchPhase.Began;
            bool mouseMove = (Input.touchCount > 0) && Input.GetTouch(0).phase == TouchPhase.Moved;
            bool mouseUp = (Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled);
            Vector3 mousePosition = Vector3.zero;
            if (Input.touchCount > 0)
                mousePosition = Input.GetTouch(0).position;
#endif

        }

        public void Tick(float dt)
        {
            if(GameState == GAME_STATE.IN_GAME)
            {
                m_time += dt;
                TimeSpan timeSpan = TimeSpan.FromSeconds(m_time);
                TimerText.text = timeSpan.ToString(@"mm\:ss");
            }
        }
    }
}