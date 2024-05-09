using WinMM;
using SMF;
using static SMF.SMF;

namespace EasySynth {
	class MidiReceive : MidiIn {
		protected override void Receive(byte[] message) {
			if ((int)MSG.CTRL_CHG == (message[0] & 0xF0)) {
				Playback.SendMessage(new CustomCtrl(message));
			} else {
				Playback.SendMessage(AssignmentEvent(message));
			}
		}
	}
}
