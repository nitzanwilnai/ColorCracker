using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGuess
{
    [Serializable]
    public struct UserData
    {
        public byte[] PickedColors;
        public byte[][] Guesses;
        public byte[][] Results;
        public int GuessRow;
        public int ColorIndex;
        public byte[] GuessIndices;
        public int GuessCount;
    }

    public sealed class GameManager : MonoBehaviourSingleton<GameManager>
    {
        public enum MENU_STATE { MAIN_MENU, IN_GAME, PRIVACY_POLICY, TUTORIAL };
        public MENU_STATE MenuState;

        public GameObject UIMainMenu;
        public GameObject UIInGame;
        public GameObject UIPrivacyPolicy;
        public GameObject UITutorial;
        public Text BestTimeText;
        public Text Title;
        public Color[] Colors;

        public UserData UserData;

        public ColorGuessVisual ColorGuessVisual;

        // Start is called before the first frame update
        void Start()
        {
            SetMenuState(MENU_STATE.MAIN_MENU);
            ColorGuessLogic.Allocate(ref UserData);

            float bestTime = PlayerPrefs.GetFloat("BestTime");
            TimeSpan timeSpan = TimeSpan.FromSeconds(bestTime);
            BestTimeText.text = "BEST TIME: " + timeSpan.ToString(@"mm\:ss");
            BestTimeText.gameObject.SetActive(false);

            string titleString = "";
            int colorIndex = 0;
            AddColorWordToTitle("COLOR", ref titleString, ref colorIndex);
            titleString += " ";
            AddColorWordToTitle("CODE", ref titleString, ref colorIndex);
            titleString += "\n<size=135>";
            AddColorWordToTitle("CRACKER", ref titleString, ref colorIndex);
            titleString += "</size>";
            Title.text = titleString;
        }

        public void AddColorWordToTitle(string word, ref string title, ref int colorIndex)
        {
            for (int i = 0; i < word.Length; i++)
            {
                title += "<color=#" + ColorUtility.ToHtmlStringRGBA(Colors[colorIndex]) + ">" + word[i] + "</color>";
                if(word[i] != ' ')
                    colorIndex = (colorIndex + 1) % 6;
            }
        }

        public void SetMenuState(MENU_STATE newMenuState)
        {
            MenuState = newMenuState;
            UIMainMenu.SetActive(MenuState == MENU_STATE.MAIN_MENU);
            UIInGame.SetActive(MenuState == MENU_STATE.IN_GAME);
            UIPrivacyPolicy.SetActive(MenuState == MENU_STATE.PRIVACY_POLICY);
            UITutorial.SetActive(MenuState == MENU_STATE.TUTORIAL);
        }

        public void StartGame()
        {
            int seconds = (int)(DateTimeOffset.Now.ToUnixTimeSeconds() % int.MaxValue);
            ColorGuessLogic.StartGame(ref UserData, seconds);
            ColorGuessVisual.StartGame(UserData);
            SetMenuState(MENU_STATE.IN_GAME);

            // For testing:
            //ColorGuessVisual.ShowPickedColors(UserData);
        }

        public void SetColorIndex(int index)
        {
            if(UserData.Guesses[UserData.GuessRow][index] == 0)
            {
                UserData.ColorIndex = index;
                ColorGuessVisual.UpdateGuessColors(UserData, UserData.GuessRow);
            }
            else
            {
                ColorGuessVisual.UndoSpecificColorIndex(ref UserData, index);
            }
        }

        public void SelectColor(int color)
        {
            ColorGuessVisual.SelectColor(ref UserData, color);
        }

        public void Undo()
        {
            ColorGuessVisual.Undo(ref UserData);
        }

        public void GoToPrivacyPolicy()
        {
            SetMenuState(MENU_STATE.PRIVACY_POLICY);
        }

        public void GoToTutorial()
        {
            SetMenuState(MENU_STATE.TUTORIAL);
        }

        public void GoToMainMenu()
        {
            SetMenuState(MENU_STATE.MAIN_MENU);
        }

        // Update is called once per frame
        void Update()
        {
            if (MenuState == MENU_STATE.IN_GAME)
            {
                ColorGuessVisual.Tick(Time.deltaTime);
                //ColorGuessVisual.HandleInput();
            }

            if (Input.GetKeyUp("s"))
            {
                DateTimeOffset now = DateTime.UtcNow;
                string name = Screen.width + "x" + Screen.height + "_" + now.ToString("yyyy-MM-dd HH.mm.ss") + ".png";
                ScreenCapture.CaptureScreenshot(name);
            }
        }
    }

}