public class DrucillaFight : ConversationTrigger {
    public void TriggerFightEndConversation() {
        cutsceneGraph.TriggerUnityEvent("End");
    }
}