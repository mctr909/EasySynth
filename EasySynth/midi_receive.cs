using WINMM;

namespace EasySynth {
	class MidiReceive : MidiIn {
		protected override void Receive(byte[] message) {
			Playback.SendMessage(message);
		}
	}
}
