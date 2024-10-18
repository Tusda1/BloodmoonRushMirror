using FMOD.Studio;

public static class FmodManagerExtensions {
    public static PLAYBACK_STATE GetPlaybackState(this EventInstance eventInstance) {
        eventInstance.getPlaybackState(out var state);
        return state;
    }
}