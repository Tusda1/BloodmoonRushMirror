using UnityEngine;

public class AUBFight : ConversationTrigger {
    [Header("Dependencies")]
    [SerializeField] private BossBase bossBase;

    private void Awake() {
        bossBase.OnBattleEnd += TriggerFightEndConversation;
    }

    private void TriggerFightEndConversation() {
        cutsceneGraph.TriggerUnityEvent("FightEnd");
    }
}