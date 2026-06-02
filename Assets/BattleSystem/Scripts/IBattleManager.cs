using System;

public interface IBattleManager
{
    ALifeSystem CurrentEntity { get; }
    ALifeSystem OtherEntity { get; }
    int CurrentEntityTurn { get; set; }

    void HandleEndRound();
    void OnAnswer();
}
