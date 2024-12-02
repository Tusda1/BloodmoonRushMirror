namespace FMODScripts {
    public struct StopSoundEvent : IFMODEvent {
        public readonly StopSoundType SoundType;

        public StopSoundEvent(StopSoundType soundType) {
            SoundType = soundType;
        }
    }

    public enum StopSoundType { Music, All }
}