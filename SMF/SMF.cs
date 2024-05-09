using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SMF {
	public partial class SMF {
		int mTrackCount;
		int mUnitTick;

		/// <summary>
		/// イベントリスト
		/// </summary>
		public List<Event> EventList = new List<Event>();

		/// <summary>
		/// ファイルを開く
		/// </summary>
		/// <param name="filePath">ファイルパス</param>
		public SMF(string filePath) {
			if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) {
				return;
			}
			using (var fs = new FileStream(filePath, FileMode.Open)) {
				var br = new BinaryReader(fs);
				LoadMThd(br);
				for (int i = 0; i < mTrackCount; i++) {
					LoadMTrk(br, i);
				}
			}
			PairingNotes();
			EventList.RemoveAll(x => x.MetaType == META.EOT);
			EventList.Sort((a, b) => {
				var diffTick = a.Tick - b.Tick;
				return (0 == diffTick) ? (a.Track - b.Track) : diffTick;
			});
		}

		/// <summary>
		/// ファイルに保存
		/// </summary>
		/// <param name="filePath">ファイルパス</param>
		public void Save(string filePath) {
			var trackList = new List<int>();
			foreach (var ev in EventList) {
				if (!trackList.Contains(ev.Track)) {
					trackList.Add(ev.Track);
				}
			}
			trackList.Sort();
			using (var fs = new FileStream(filePath, FileMode.Create)) {
				SaveMThd(fs, trackList.Count);
				foreach (var trackNum in trackList) {
					SaveMTrk(fs, trackNum);
				}
			}
		}

		/// <summary>
		/// SMFメッセージからイベントを作成
		/// </summary>
		/// <param name="message">SMFメッセージ</param>
		/// <returns></returns>
		public static Event AssignmentEvent(byte[] message) {
			var type = (MSG)(message[0] < (int)MSG.SYS_EX ? (message[0] & 0xF0) :  message[0]);
			switch (type) {
			case MSG.NOTE_OFF:
			case MSG.NOTE_ON:
				return new Note(message);
			case MSG.KEY_PRESS:
				return new KeyPress(message);
			case MSG.CTRL_CHG:
				return new Ctrl(message);
			case MSG.PROG_CHG:
				return new Prog(message);
			case MSG.CH_PRESS:
				return new ChPress(message);
			case MSG.PITCH:
				return new Pitch(message);
			case MSG.SYS_EX:
				return new SysEx(message);
			case MSG.META:
				switch ((META)message[1]) {
				case META.TEMPO:
					return new Tempo(message);
				case META.MEASURE:
					return new Measure(message);
				case META.KEY:
					return new Key(message);
				case META.DATA:
					return new Binary(message);
				default:
					if (Enum.IsDefined(typeof(TEXT), message[1])) {
						return new Text(message);
					}
					else {
						return new Meta(message);
					}
				}
			default:
				throw new Exception();
			}
		}

		internal static byte[] GetDelta(int delta) {
			if (delta < (1 << 7)) {
				return new byte[] { (byte)delta };
			}
			else if (delta < (1 << 14)) {
				return new byte[] {
				(byte)(0x80 | (delta >> 7) & 0x7F),
				(byte)(delta & 0x7F)
			};
			}
			else if (delta < (1 << 21)) {
				return new byte[] {
				(byte)(0x80 | (delta >> 14) & 0x7F),
				(byte)(0x80 | (delta >> 7) & 0x7F),
				(byte)(delta & 0x7F)
			};
			}
			else {
				return new byte[] {
				(byte)(0x80 | (delta >> 21) & 0x7F),
				(byte)(0x80 | (delta >> 14) & 0x7F),
				(byte)(0x80 | (delta >> 7) & 0x7F),
				(byte)(delta & 0x7F)
			};
			}
		}

		internal static int GetDeltaSize(int start, params byte[] data) {
			var index = start;
			var count = 0;
			while (count++ < 4 && data[index] >= 0x80 && ++index < data.Length)
				;
			return count;
		}

		void LoadMThd(BinaryReader br) {
			var signature = Encoding.ASCII.GetString(br.ReadBytes(4));
			var size = ReadUInt32(br);
			if ("MThd" != signature || 6 != size) {
				throw new Exception();
			}
			var format = ReadUInt16(br);
			if (format >= 3) {
				throw new Exception();
			}
			mTrackCount = ReadUInt16(br);
			mUnitTick = ReadUInt16(br);
		}

		void LoadMTrk(BinaryReader br, int number) {
			var signature = Encoding.ASCII.GetString(br.ReadBytes(4));
			if ("MTrk" != signature) {
				EventList.Clear();
				throw new Exception();
			}
			var size = ReadUInt32(br);
			var end = br.BaseStream.Position + size;
			var tick = 0;
			byte lastStatus = 0;
			while (br.BaseStream.Position < end) {
				tick += ReadDelta(br);
				var message = ReadMessage(br, ref lastStatus);
				var ev = AssignmentEvent(message);
				ev.Tick = tick * UnitTick / mUnitTick;
				if (!(ev is Measure || ev is Key || ev is Tempo)) {
					ev.Track = number;
				}
				EventList.Add(ev);
			}
		}

		void SaveMThd(FileStream fs, int trackCount) {
			Write(fs, (uint)0x4D546864);
			Write(fs, (uint)6);
			Write(fs, (ushort)1);
			Write(fs, (ushort)trackCount);
			Write(fs, (ushort)UnitTick);
		}

		void SaveMTrk(FileStream fs, int trackNum) {
			var trackEvents = EventList.FindAll(x => x.Track == trackNum);
			trackEvents.Sort((a, b) => a.Tick - b.Tick);
			// EOTをトラックの終端に追加
			{
				trackEvents.RemoveAll(x => x.MetaType == META.EOT);
				var lastEvent = trackEvents[trackEvents.Count - 1];
				var eot = new EOT() {
					Tick = lastEvent.Tick
				};
				trackEvents.Add(eot);
			}
			// トラックのバイト数をカウント
			int trackSize = 0;
			{
				int lastTick = 0;
				foreach (var ev in trackEvents) {
					trackSize += GetDelta(ev.Tick - lastTick).Length + ev.Data.Length;
					lastTick = ev.Tick;
				}
			}
			// MTrkを書き込み
			{
				Write(fs, (uint)0x4D54726B);
				Write(fs, (uint)trackSize);
				var bw = new BinaryWriter(fs);
				var lastTick = 0;
				foreach (var ev in trackEvents) {
					bw.Write(GetDelta(ev.Tick - lastTick));
					bw.Write(ev.Data);
					lastTick = ev.Tick;
				}
			}
		}

		void PairingNotes() {
			var pairedList = new List<Event>();
			var noteOnList = new List<Note>();
			foreach (var ev in EventList) {
				if (ev is Note note) {
					if (note.MsgType == MSG.NOTE_ON) {
						if (0 == note.Velocity) {
							AddNote(note);
						}
						else {
							noteOnList.Add(note);
						}
					}
					else {
						AddNote(note);
					}
				}
				else {
					pairedList.Add(ev);
				}
			}
			EventList.Clear();
			EventList.AddRange(pairedList);
			return;
			void AddNote(Note noteOff) {
				var noteOn = noteOnList.Find(x => x.Track == noteOff.Track && x.Channel == noteOff.Channel && x.Tone == noteOff.Tone);
				if (null == noteOn) {
					return;
				}
				var note = new Note(noteOn.Tick, noteOff.Tick, noteOn.Tone, noteOn.Velocity) {
					Track = noteOn.Track,
					Channel = noteOn.Channel
				};
				pairedList.Add(note);
				pairedList.Add(note.Pair);
				noteOnList.Remove(noteOn);
			}
		}

		static byte[] ReadMessage(BinaryReader br, ref byte lastStatus) {
			var status = br.ReadByte();
			byte data1;
			if (status < 0x80) {
				data1 = status;
				status = lastStatus;
			}
			else {
				data1 = br.ReadByte();
				lastStatus = status;
			}

			var msgType = (MSG)(status < (int)MSG.SYS_EX ? (status & 0xF0) : status);
			switch (msgType) {
			case MSG.PROG_CHG:
			case MSG.CH_PRESS:
				return new byte[] { status, data1 };
			case MSG.NOTE_OFF:
			case MSG.NOTE_ON:
			case MSG.KEY_PRESS:
			case MSG.CTRL_CHG:
			case MSG.PITCH:
				return new byte[] { status, data1, br.ReadByte() };
			case MSG.SYS_EX: {
				var arr = new List<byte>() { status, data1 };
				byte readByte;
				do {
					readByte = br.ReadByte();
					arr.Add(readByte);
				} while (readByte != 0xF7);
				return arr.ToArray();
			}
			case MSG.META: {
				var len = ReadDelta(br);
				var data = br.ReadBytes(len);
				var delta = GetDelta(len);
				var start = 2 + delta.Length;
				var ret = new byte[start + data.Length];
				ret[0] = status;
				ret[1] = data1;
				Array.Copy(delta, 0, ret, 2, delta.Length);
				Array.Copy(data, 0, ret, start, data.Length);
				return ret;
			}
			default:
				throw new Exception();
			}
		}

		static uint ReadUInt32(BinaryReader br) {
			var value = (uint)br.ReadByte() << 24;
			value |= (uint)br.ReadByte() << 16;
			value |= (uint)br.ReadByte() << 8;
			value |= br.ReadByte();
			return value;
		}

		static ushort ReadUInt16(BinaryReader br) {
			var value = (ushort)(br.ReadByte() << 8);
			value |= br.ReadByte();
			return value;
		}

		static int ReadDelta(BinaryReader br) {
			int value = 0;
			int readByte;
			do {
				value <<= 7;
				readByte = br.ReadByte();
				value |= readByte & 0x7F;
			} while (readByte >= 0x80);
			return value;
		}

		static void Write(FileStream fs, uint value) {
			fs.WriteByte((byte)(value >> 24));
			fs.WriteByte((byte)((value >> 16) & 0xFF));
			fs.WriteByte((byte)((value >> 8) & 0xFF));
			fs.WriteByte((byte)(value & 0xFF));
		}

		static void Write(FileStream fs, ushort value) {
			fs.WriteByte((byte)(value >> 8));
			fs.WriteByte((byte)(value & 0xFF));
		}
	}
}
