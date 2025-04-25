public class AUBFight : ConversationTrigger {
    public void TriggerFightBreakConversation() {
        cutsceneGraph.TriggerUnityEvent("FightBreak");
    }

    public void TriggerFightEndConversation() {
        cutsceneGraph.TriggerUnityEvent("FightEnd");
    }
}