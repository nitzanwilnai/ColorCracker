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
    }

    public sealed class GameManager : MonoBehaviour
    {
        public enum MENU_STATE { MAIN_MENU, IN_GAME, QUIT_CONFIRMATION };
        public MENU_STATE MenuState;

        public GameObject UIMainMenu;
        public GameObject UIInGame;
        public Text BestTimeText;

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
        }

        public void SetMenuState(MENU_STATE newMenuState)
        {
            MenuState = newMenuState;
            UIMainMenu.SetActive(MenuState == MENU_STATE.MAIN_MENU);
            UIInGame.SetActive(MenuState == MENU_STATE.IN_GAME);
        }

        public void StartGame()
        {
            int seconds = (int)(DateTimeOffset.Now.ToUnixTimeSeconds() % int.MaxValue);
            ColorGuessLogic.StartGame(ref UserData, seconds);
            ColorGuessVisual.StartGame();
            SetMenuState(MENU_STATE.IN_GAME);

            //ColorGuessVisual.ShowPickedColors(UserData);
        }

        public void SelectColor(int color)
        {
            ColorGuessVisual.SelectColor(ref UserData, color);
        }

        public void Undo()
        {
            ColorGuessVisual.Undo(UserData);
        }

        // Update is called once per frame
        void Update()
        {
            if (MenuState == MENU_STATE.IN_GAME)
                ColorGuessVisual.Tick(Time.deltaTime);
        }
    }

}