public interface IPlayerLadderLogic {
    public bool IsClimbing { get; }
    
    public LadderTop LadderTop { get; set; }
    public LadderMiddle LadderMiddle { get; set; }
    public LadderBottom LadderBottom { get; set; }

    bool TryGetLadderTop(out LadderTop ladderTop);
    bool LadderLogic();
    void EnterLadder();
    void ExitLadder();
}