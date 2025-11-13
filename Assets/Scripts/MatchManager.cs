using UnityEngine;
using System;
using System.Collections;

public enum MatchState
{
    PreMatch,
    MatchStarting,
    CastingPhase,
    MatchEnding,
    MatchEnded
}

public class MatchManager : MonoBehaviour
{
    [Header("Match Settings")]
    [SerializeField] private float matchStartDelay = 3f;
    [SerializeField] private float matchTimeLimit = 300f;
    [SerializeField] private bool useTimeLimit = false;
    
    [Header("Player References")]
    [SerializeField] private PlayerStats player1Stats;
    [SerializeField] private PlayerStats player2Stats;
    
    [Header("UI References")]
    [SerializeField] private PlayerUIController player1UI;
    [SerializeField] private PlayerUIController player2UI;
    
    public static MatchManager Instance { get; private set; }
    
    public event Action<MatchState> OnMatchStateChanged;
    public event Action<PlayerStats> OnPlayerWon;
    public event Action OnMatchTimeLimitReached;
    
    private MatchState currentState = MatchState.PreMatch;
    private float matchTimer = 0f;
    private PlayerStats winner = null;
    
    public MatchState CurrentState => currentState;
    public float MatchTimeRemaining => useTimeLimit ? Mathf.Max(0f, matchTimeLimit - matchTimer) : -1f;
    public float MatchTimeElapsed => matchTimer;
    public PlayerStats Winner => winner;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    private void Start()
    {
        InitializePlayers();
        StartCoroutine(StartMatchSequence());
    }
    
    private void Update()
    {
        if (currentState == MatchState.CastingPhase)
        {
            UpdateMatchTimer();
        }
    }
    
    private void InitializePlayers()
    {
        if (player1Stats != null)
        {
            player1Stats.OnPlayerDied += () => OnPlayerDefeated(player1Stats, player2Stats);
            player1Stats.ResetHealth();
        }
        
        if (player2Stats != null)
        {
            player2Stats.OnPlayerDied += () => OnPlayerDefeated(player2Stats, player1Stats);
            player2Stats.ResetHealth();
        }
        
        Debug.Log("Match Manager: Players initialized");
    }
    
    private IEnumerator StartMatchSequence()
    {
        ChangeState(MatchState.MatchStarting);
        
        Debug.Log($"<color=cyan>⚔️ Match starting in {matchStartDelay} seconds...</color>");
        
        yield return new WaitForSeconds(matchStartDelay);
        
        StartMatch();
    }
    
    private void StartMatch()
    {
        ChangeState(MatchState.CastingPhase);
        matchTimer = 0f;
        
        Debug.Log("<color=green>⚔️ MATCH STARTED! Begin casting!</color>");
    }
    
    private void UpdateMatchTimer()
    {
        matchTimer += Time.deltaTime;
        
        if (useTimeLimit && matchTimer >= matchTimeLimit)
        {
            OnTimeLimitReached();
        }
    }
    
    private void OnTimeLimitReached()
    {
        Debug.Log("<color=yellow>⏱️ Time limit reached!</color>");
        OnMatchTimeLimitReached?.Invoke();
        
        DetermineWinnerByHealth();
    }
    
    private void DetermineWinnerByHealth()
    {
        if (player1Stats == null || player2Stats == null)
        {
            EndMatch(null);
            return;
        }
        
        if (player1Stats.CurrentHealth > player2Stats.CurrentHealth)
        {
            EndMatch(player1Stats);
        }
        else if (player2Stats.CurrentHealth > player1Stats.CurrentHealth)
        {
            EndMatch(player2Stats);
        }
        else
        {
            Debug.Log("<color=yellow>Match ended in a draw!</color>");
            EndMatch(null);
        }
    }
    
    private void OnPlayerDefeated(PlayerStats defeated, PlayerStats victor)
    {
        if (currentState != MatchState.CastingPhase) return;
        
        Debug.Log($"<color=red>{defeated.gameObject.name} was defeated!</color>");
        Debug.Log($"<color=green>🏆 {victor.gameObject.name} wins the match!</color>");
        
        EndMatch(victor);
    }
    
    private void EndMatch(PlayerStats matchWinner)
    {
        ChangeState(MatchState.MatchEnding);
        
        winner = matchWinner;
        
        if (winner != null)
        {
            OnPlayerWon?.Invoke(winner);
            Debug.Log($"<color=green>🎉 Victory for {winner.gameObject.name}!</color>");
        }
        else
        {
            Debug.Log("<color=yellow>Match ended with no winner (draw)</color>");
        }
        
        StartCoroutine(FinalizeMatchEnd());
    }
    
    private IEnumerator FinalizeMatchEnd()
    {
        yield return new WaitForSeconds(2f);
        
        ChangeState(MatchState.MatchEnded);
        
        Debug.Log("<color=cyan>Match ended. Press R to restart (if implemented)</color>");
    }
    
    private void ChangeState(MatchState newState)
    {
        if (currentState == newState) return;
        
        MatchState previousState = currentState;
        currentState = newState;
        
        Debug.Log($"<color=cyan>Match State: {previousState} → {newState}</color>");
        
        OnMatchStateChanged?.Invoke(newState);
    }
    
    public void RestartMatch()
    {
        winner = null;
        matchTimer = 0f;
        
        if (player1Stats != null) player1Stats.ResetHealth();
        if (player2Stats != null) player2Stats.ResetHealth();
        
        ChangeState(MatchState.PreMatch);
        StartCoroutine(StartMatchSequence());
        
        Debug.Log("<color=green>Match restarted!</color>");
    }
    
    public void PauseMatch()
    {
        Time.timeScale = 0f;
        Debug.Log("Match paused");
    }
    
    public void ResumeMatch()
    {
        Time.timeScale = 1f;
        Debug.Log("Match resumed");
    }
    
    public string GetMatchTimeString()
    {
        int minutes = Mathf.FloorToInt(matchTimer / 60f);
        int seconds = Mathf.FloorToInt(matchTimer % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
    
    public PlayerStats GetPlayer1() => player1Stats;
    public PlayerStats GetPlayer2() => player2Stats;
    
    public PlayerStats GetOpponent(PlayerStats player)
    {
        if (player == player1Stats) return player2Stats;
        if (player == player2Stats) return player1Stats;
        return null;
    }
}
