using Global;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum EGameState
{
    None,
    Intro,
    PlayGame,
    CheckAnswer,
    GameOver,
    EndGame,
    Max
}
public class CFDGameData
{
    public string m_ID = string.Empty;
    public string m_IntroImagePath = string.Empty;
    public string m_AnswerImagePath = string.Empty;
    public FDStepInfoRecord m_IntroRecord = null;
    public FDStepInfoRecord m_AnswerIntroRecord = null;

    public CFDGameData(string _ID, string _introImagePath, string _AnswerImagePath, FDStepInfoRecord _introRecord, FDStepInfoRecord _answerIntroRecord)
    {
        m_ID = _ID;
        m_IntroImagePath = _introImagePath;
        m_AnswerImagePath = _AnswerImagePath;
        m_IntroRecord = _introRecord;
        m_AnswerIntroRecord = _answerIntroRecord;
    }    
}
public partial class FDGamesManager
{
    private List<CFDGameData> m_GameDataList = new List<CFDGameData>();
    private int m_CurrentIDX = 0;
    private int m_MaxRound = 0;
    private EGameState m_GameState = EGameState.None;
    
    private float m_AccIntroTime = 0f;
    private float m_AccCheckAnswer = 0f;

    public int CurrentGameRound { get { return m_CurrentIDX; } }

    public void InitData()
    {
        InitGameData();
    }
    public void InitGameData()
    {
        m_CurrentIDX = 0;
        m_MaxRound = 0;
        m_AccIntroTime = 0f;
        m_AccCheckAnswer = 0f;
        m_GameState = EGameState.None;

        InitSaveData();
    }
    public void InitMode(string _GameID)
    {
        UpdateState(EGameState.None);

        m_GameDataList.Clear();
        m_CurrentIDX = 0;

        string _introImagePath = ImageTable.Instance.GetRandomAnswerImagePath();
        string _answerIntroImagePath = ImageTable.Instance.GetRandomAnswerImagePath();
        FDStepInfoRecord _introRecord = FDStepInfoTable.Instance.GetStepInfoRecord(EFDStepStateType.Intro, 1);
        FDStepInfoRecord _answerIntroRecord = FDStepInfoTable.Instance.GetStepInfoRecord(EFDStepStateType.AnswerIntro, 1);

        m_GameDataList.Add(new CFDGameData(_GameID, _introImagePath, _answerIntroImagePath, _introRecord, _answerIntroRecord));
        m_MaxRound = 1;

        UpdateState(EGameState.Intro);
    }
    public void UpdateState(EGameState state)
    {
        m_GameState = state;

        switch (m_GameState)
        {
            case EGameState.None:
                {
                    InitGameData();
                }
                break;
            case EGameState.Intro:
                {
                    m_AccIntroTime = 0f;
                    GameIntro();
                    PlayGameSound();
                }
                break;
            case EGameState.PlayGame:
                {
                    PlayGame();
                }
                break;
            case EGameState.CheckAnswer:
                {
                    m_AccCheckAnswer = 0f;
                    SoundManager.Instance.PlayClip("10003");
                    CheckAnswer();
                }
                break;
            case EGameState.GameOver:
                {
                    GameOver();
                }
                break;
            case EGameState.EndGame:
                {
                    InitGameData();
                    CloseGame();
                    SoundManager.Instance.PlayClip("10002");
                }
                break;           
        }
    }       
    public CFDGameData GetGameData()
    {
        if (CommonFunction.IsVaildIndex(m_CurrentIDX, m_GameDataList.Count) == false)
            return null;

        return m_GameDataList[m_CurrentIDX];
    }    
    private void GameIntro()
    {
        UIManager.Instance.OpenUI<UIPanel_GameIntro>(EUIID.UIGameIntro, _auction =>
        {
            CFDGameData _data = GetGameData();
            if (_data == null)
                return;

            _auction.SetData(_data.m_ID, _data.m_IntroRecord);
        });
    }
    private void PlayGame()
    {
        CFDGameData _data = GetGameData();
        if (_data == null)
            return;
       
        UIManager.Instance.CloseUI(EUIID.UIGameIntro);

        OpenGame<FDPanel_Games>(_data.m_ID, _Action =>
        {
            _Action.SetData(_data.m_ID);
        });
    }
    private void CheckAnswer()
    {
        CFDGameData _data = GetGameData();
        if (_data == null)
            return;

        if (GetOpenGame<FDPanel_Games>(_data.m_ID) is FDPanel_Games _fdGame)
        {
            _fdGame.PlayCheckAllAnswer();
        }
    }
    private void GameOver()
    {
        GameResult();
    }    
    private void PlayGameSound()
    {
        CFDGameData _gameData = GetGameData();
        if (_gameData == null)
            return;

        FDGamesRecord _record = FDGamesTable.Instance.Find(_gameData.m_ID);
        if (_record == null)
            return;

        SoundManager.Instance.PlayClip(_record.m_SoundID);
    }
    private void Update()
    {
        float _deltaTime = Time.deltaTime * Time.timeScale;

        UpdateTimer(_deltaTime);
    }
    private void UpdateTimer(float _deltaTime)
    {
        switch (m_GameState)
        {
            case EGameState.Intro:
                {
                    m_AccIntroTime += _deltaTime;
                    if (m_AccIntroTime >= GoogleSheetManager.Instance.m_IntroTime)
                    {
                        UpdateState(EGameState.PlayGame);
                        m_AccIntroTime = 0f;
                    }
                }
                break;
            case EGameState.CheckAnswer:
                {
                    m_AccCheckAnswer += _deltaTime;
                    if (m_AccCheckAnswer >= 5f)
                    {
                        UpdateState(EGameState.GameOver);
                        m_AccCheckAnswer = 0f;
                    }

                    if (m_OpenFDGame != null)
                    {
                        m_OpenFDGame.UpdateTime(_deltaTime);
                    }
                }
                break;
            case EGameState.GameOver:
            case EGameState.PlayGame:
            case EGameState.EndGame:
                {
                    if (m_OpenFDGame != null)
                    {
                        m_OpenFDGame.UpdateTime(_deltaTime);
                    }
                }
                break;
            default:
                break;
        }
    }
    private void GameResult()
    {
        if (m_OpenFDGame == null)
            return;

        SaveData_ClearStage(m_GameDataList[0].m_ID);

        m_OpenFDGame.ShowGameResult();
    }    
    public void RePlay()
    {
        CloseGame();
        InitMode(m_GameDataList[0].m_ID);
    }
    public void NextGame()
    {
        CloseGame();

        FDGamesRecord _record = FDGamesTable.Instance.Find(m_GameDataList[0].m_ID);
        if(_record != null)
        {
            UIManager.Instance.CloseUI(EUIID.UIGameResult);
            InitMode(_record.NextID);
        }        
    }
    public void SetPause(bool isPause)
    {
        if (m_OpenFDGame == null)
            return;

        m_OpenFDGame.SetPause(isPause);
    }    
}