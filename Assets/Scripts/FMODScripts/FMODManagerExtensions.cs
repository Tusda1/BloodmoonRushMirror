using FMOD.Studio;

namespace FMODScripts {
    public static class FMODManagerExtensions {
        public static PLAYBACK_STATE GetPlaybackState(this EventInstance eventInstance) {
            eventInstance.getPlaybackState(out var state);
            return state;
        }
    }
}