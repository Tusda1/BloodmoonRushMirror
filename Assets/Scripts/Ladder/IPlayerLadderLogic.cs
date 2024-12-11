public interface IPlayerLadderLogic {
    bool TryGetLadderTop(out LadderTop ladderTop);
    bool LadderLogic();
    void EnterLadder();
    void ExitLadder();
}