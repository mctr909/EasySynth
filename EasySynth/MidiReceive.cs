using WinMM;

namespace EasySynth {
	class MidiReceive : MidiIn {
		protected override void Receive(byte[] message) {
			//Playback.SendMessage(message);
		}
	}
}
