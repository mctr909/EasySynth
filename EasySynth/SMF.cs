using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EasySynth {
	public class SMF {
		public enum MESSAGE : byte {
			NOTE_OFF = 0x80,
			NOTE_ON = 0x90,
			KEY_PRESS = 0xA0,
			CTRL_CHG = 0xB0,
			PROG_CHG = 0xC0,
			CH_PRESS = 0xD0,
			PITCH = 0xE0,
			SYS_EX = 0xF0,
			META = 0xFF
		}

		public enum CTRL : byte {
			BANK_MSB = 0,
			BANK_LSB = 32,
			MOD = 1,
			POLTA_TIME = 5,
			DATA_MSB = 6,
			DATA_LSB = 38,
			VOL = 7,
			PAN = 10,
			EXP = 11,
			HOLD = 64,
			POLTA_SWITCH = 65,
			RESONANCE = 71,
			RELEASE = 72,
			ATTACK = 73,
			CUTOFF = 74,
			VIB_RATE = 76,
			VIB_DEPTH = 77,
			VIB_DELAY = 78,
			REV_SEND = 91,
			CHO_SEND = 93,
			DEL_SEND = 94,
			NRPN_LSB = 98,
			NRPN_MSB = 99,
			RPN_LSB = 100,
			RPN_MSB = 101,
			ALL_SOUND_OFF = 120,
			RESET_ALL_CTRL = 121,
			ALL_NOTE_OFF = 123,
			INVALID = 255
		}

		public enum RPN : ushort {
			BEND_RANGE = 0x0000,
			FINETUNE = 0x0001,
			COURSETUNE = 0x0002,
			MOD_RANGE = 0x0005,
			NULL = 0x7F7F
		}

		public enum META : byte {
			/* text data */
			TEXT = 1,
			COPYRIGHT = 2,
			TRACK_NAME = 3,
			INST_NAME = 4,
			LYRIC = 5,
			MARKER = 6,
			QUEUE = 7,
			PROG_NAME = 8,
			DEVICE_NAME = 9,

			/* numeric data */
			SEQ_NO = 0x00,
			CH_PREFIX = 0x20,
			PORT = 0x21,

			/* system data */
			EOT = 0x2F,
			TEMPO = 0x51,
			SMPTE = 0x54,
			MEASURE = 0x58,
			KEY = 0x59,

			/* byte data */
			DATA = 0x7F,

			INVALID = 0xFF
		}

		public enum TEXT : byte {
			TEXT = META.TEXT,
			COPYRIGHT = META.COPYRIGHT,
			TRACK_NAME = META.TRACK_NAME,
			INST_NAME = META.INST_NAME,
			LYRIC = META.LYRIC,
			MARKER = META.MARKER,
			QUEUE = META.QUEUE,
			PROG_NAME = META.PROG_NAME,
			DEVICE_NAME = META.DEVICE_NAME,
		}

		public enum KEY_SIG : ushort {
			b7 = 0xF900,
			b6 = 0xFA00,
			b5 = 0xFB00,
			b4 = 0xFC00,
			b3 = 0xFD00,
			b2 = 0xFE00,
			b1 = 0xFF00,
			n  = 0x0000,
			s1 = 0x0100,
			s2 = 0x0200,
			s3 = 0x0300,
			s4 = 0x0400,
			s5 = 0x0500,
			s6 = 0x0600,
			s7 = 0x0700,
			MAJOR = 0x0000,
			MINOR = 0x0001
		}

		public enum KEY : ushort {
			Cb_MAJOR = KEY_SIG.b7 | KEY_SIG.MAJOR,
			Ab_MINOR = KEY_SIG.b7 | KEY_SIG.MINOR,
			Gb_MAJOR = KEY_SIG.b6 | KEY_SIG.MAJOR,
			Eb_MINOR = KEY_SIG.b6 | KEY_SIG.MINOR,
			Db_MAJOR = KEY_SIG.b5 | KEY_SIG.MAJOR,
			Bb_MINOR = KEY_SIG.b5 | KEY_SIG.MINOR,
			Ab_MAJOR = KEY_SIG.b4 | KEY_SIG.MAJOR,
			F_MINOR  = KEY_SIG.b4 | KEY_SIG.MINOR,
			Eb_MAJOR = KEY_SIG.b3 | KEY_SIG.MAJOR,
			C_MINOR  = KEY_SIG.b3 | KEY_SIG.MINOR,
			Bb_MAJOR = KEY_SIG.b2 | KEY_SIG.MAJOR,
			G_MINOR  = KEY_SIG.b2 | KEY_SIG.MINOR,
			F_MAJOR  = KEY_SIG.b1 | KEY_SIG.MAJOR,
			D_MINOR  = KEY_SIG.b1 | KEY_SIG.MINOR,
			C_MAJOR  = KEY_SIG.n  | KEY_SIG.MAJOR,
			A_MINOR  = KEY_SIG.n  | KEY_SIG.MINOR,
			G_MAJOR  = KEY_SIG.s1 | KEY_SIG.MAJOR,
			E_MINOR  = KEY_SIG.s1 | KEY_SIG.MINOR,
			D_MAJOR  = KEY_SIG.s2 | KEY_SIG.MAJOR,
			B_MINOR  = KEY_SIG.s2 | KEY_SIG.MINOR,
			A_MAJOR  = KEY_SIG.s3 | KEY_SIG.MAJOR,
			Fs_MINOR = KEY_SIG.s3 | KEY_SIG.MINOR,
			E_MAJOR  = KEY_SIG.s4 | KEY_SIG.MAJOR,
			Cs_MINOR = KEY_SIG.s4 | KEY_SIG.MINOR,
			B_MAJOR  = KEY_SIG.s5 | KEY_SIG.MAJOR,
			Gs_MINOR = KEY_SIG.s5 | KEY_SIG.MINOR,
			Fs_MAJOR = KEY_SIG.s6 | KEY_SIG.MAJOR,
			Ds_MINOR = KEY_SIG.s6 | KEY_SIG.MINOR,
			Cs_MAJOR = KEY_SIG.s7 | KEY_SIG.MAJOR,
			As_MINOR = KEY_SIG.s7 | KEY_SIG.MINOR,
			INVALID = 0xFFFF
		}

		public class Measure {
			public int Numerator {
				get { return mData[3]; }
				set { mData[3] = (byte)value; }
			}
			public int Denominator {
				get { return (int)Math.Pow(2, mData[4]); }
				set { mData[4] = (byte)Math.Log(value, 2); }
			}
			byte[] mData;
			public Measure() {
				mData = new byte[5];
			}
			internal Measure(byte[] data) {
				mData = data;
			}
		}

		public class Message {
			public int Tick { get; set; }
			public int Track { get; set; }
			public byte[] Data { get; private set; }

			public MESSAGE Type { get { return (MESSAGE)(Data[0] < (int)MESSAGE.SYS_EX ? (Data[0] & 0xF0) : Data[0]); } }
			public CTRL CtrlType { get { return Type == MESSAGE.CTRL_CHG ? (CTRL)Data[1] : CTRL.INVALID; } }
			public META MetaType { get { return Type == MESSAGE.META ? (META)Data[1] : META.INVALID; } }
			public int Channel {
				get { return Type < MESSAGE.SYS_EX ? (Data[0] & 0xF) : -1; }
				set { if (Type < MESSAGE.SYS_EX) Data[0] = (byte)((Data[0] & 0xF0) | (value & 0xF)); }
			}
			public double Tempo {
				get { return MetaType == META.TEMPO ? (60000000.0 / ((Data[3] << 16) | (Data[4] << 8) | Data[5])) : 0; }
				set {
					if (MetaType == META.TEMPO) {
						var x = (int)(60000000.0 / value);
						Data[3] = (byte)((x >> 16) & 0xFF);
						Data[4] = (byte)((x >> 8) & 0xFF);
						Data[5] = (byte)(x & 0xFF);
					}
				}
			}
			public Measure Measure { get { return MetaType == META.MEASURE ? new Measure(Data) : null; } }
			public KEY Key {
				get { return MetaType == META.KEY ? (KEY)((Data[3] << 8) | Data[4]) : KEY.INVALID; }
				set {
					if (MetaType == META.KEY) {
						Data[3] = (byte)((int)value >> 8);
						Data[4] = (byte)((int)value & 0xFF);
					}
				}
			}
			public string Text {
				get {
					if (Enum.IsDefined(typeof(TEXT), (int)MetaType)) {
						var textStart = 2 + GetDeltaSize(2, Data);
						return Encoding.Default.GetString(Data, textStart, Data.Length - textStart);
					} else {
						return null;
					}
				}
				set {
					if (Enum.IsDefined(typeof(TEXT), (int)MetaType)) {
						var arrText = Encoding.Default.GetBytes(value);
						var delta = GetDelta(arrText.Length);
						var list = new List<byte> { Data[0], Data[1] };
						list.AddRange(delta);
						list.AddRange(arrText);
						Data = list.ToArray();
					}
				}
			}

			internal Message(BinaryReader br, int track, int tick, ref byte lastStatus) {
				Track = track;
				Tick = tick;

				var status = br.ReadByte();
				byte value1;
				if (status < 0x80) {
					value1 = status;
					status = lastStatus;
				} else {
					value1 = br.ReadByte();
					lastStatus = status;
				}

				var msgType = (MESSAGE)(status < (int)MESSAGE.SYS_EX ? (status & 0xF0) : status);
				switch (msgType) {
				case MESSAGE.PROG_CHG:
				case MESSAGE.CH_PRESS:
					Data = new byte[] { status, value1 };
					break;
				case MESSAGE.NOTE_OFF:
				case MESSAGE.NOTE_ON:
				case MESSAGE.KEY_PRESS:
				case MESSAGE.CTRL_CHG:
				case MESSAGE.PITCH:
					Data = new byte[] { status, value1, br.ReadByte() };
					break;
				case MESSAGE.SYS_EX: {
					var arr = new List<byte>() { status, value1 };
					byte readByte;
					do {
						readByte = br.ReadByte();
						arr.Add(readByte);
					} while (readByte != 0xF7);
					Data = arr.ToArray();
					break;
				}
				case MESSAGE.META: {
					var arr = new List<byte>() { status, value1 };
					var len = ReadDelta(br);
					arr.AddRange(GetDelta(len));
					for (int i = 0; i < len; i++) {
						arr.Add(br.ReadByte());
					}
					Data = arr.ToArray();
					break;
				}
				default:
					throw new Exception();
				}
			}

			public Message(params byte[] data) {
				Data = data;
			}

			public Message(int channel, int tone, int velocity) {
				Data = new byte[] {
					(byte)((int)MESSAGE.NOTE_ON | (channel & 0xF)),
					(byte)tone,
					(byte)velocity
				};
			}

			public Message(int channel, CTRL type, int value) {
				Data = new byte[] {
					(byte)((int)MESSAGE.CTRL_CHG | (channel & 0xF)),
					(byte)type,
					(byte)value
				};
			}

			public Message(int channel, int progNum, bool isDrum) {
				Data = new byte[] {
					(byte)((int)MESSAGE.PROG_CHG | (channel & 0xF)),
					(byte)progNum
				};
			}

			public Message(int channel, int pitch) {
				pitch += 8192;
				Data = new byte[] {
					(byte)((int)MESSAGE.PITCH | (channel & 0xF)),
					(byte)(pitch & 0x7F),
					(byte)(pitch >> 7)
				};
			}

			public Message(TEXT type, string text) {
				var arr = Encoding.Default.GetBytes(text);
				var data = new List<byte>() { (byte)MESSAGE.META, (byte)type };
				data.AddRange(GetDelta(arr.Length));
				data.AddRange(arr);
				Data = data.ToArray();
			}

			public Message(Measure measure) {
				Data = new byte[] {
					(byte)MESSAGE.META,
					(byte)META.MEASURE,
					4,
					(byte)measure.Numerator,
					(byte)Math.Log(measure.Denominator, 2),
					24,
					8
				};
			}

			public Message(KEY key) {
				Data = new byte[] {
					(byte)MESSAGE.META,
					(byte)META.KEY,
					2,
					(byte)((int)key >> 7),
					(byte)((int)key & 0x7F)
				};
			}

			public Message(double tempo) {
				var x = (int)(60000000.0 / tempo);
				Data = new byte[] {
					(byte)MESSAGE.META,
					(byte)META.TEMPO,
					3,
					(byte)((x >> 16) & 0xFF),
					(byte)((x >> 8) & 0xFF),
					(byte)(x & 0xFF)
				};
			}
		}

		public List<Message> EventList = new List<Message>();

		const int BaseTick = 960;

		int mTrackCount;
		int mUnitTick;

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
			EventList.RemoveAll(x => x.MetaType == META.EOT);
			EventList.Sort((a, b) => {
				var diffTick = a.Tick - b.Tick;
				return (0 == diffTick) ? (a.Track - b.Track) : diffTick;
			});
		}

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
				EventList.Add(new Message(br, number, tick * BaseTick / mUnitTick, ref lastStatus));
			}
		}

		void SaveMThd(FileStream fs, int trackCount) {
			Write(fs, (uint)0x4D546864);
			Write(fs, (uint)6);
			Write(fs, (ushort)1);
			Write(fs, (ushort)trackCount);
			Write(fs, (ushort)BaseTick);
		}

		void SaveMTrk(FileStream fs, int trackNum) {
			var eventList = EventList.FindAll(x => x.Track == trackNum);
			eventList.Sort((a, b) => a.Tick - b.Tick);
			// EOTをトラックの終端に追加
			{
				var lastEvent = eventList[eventList.Count - 1];
				var eot = new Message((byte)MESSAGE.META, (byte)META.EOT, (byte)0) {
					Tick = lastEvent.Tick
				};
				eventList.Add(eot);
			}
			// トラックのバイト数をカウント
			int trackSize = 0;
			{
				int lastTick = 0;
				foreach (var ev in eventList) {
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
				foreach (var ev in eventList) {
					bw.Write(GetDelta(ev.Tick - lastTick));
					bw.Write(ev.Data);
					lastTick = ev.Tick;
				}
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

		static byte[] GetDelta(int value) {
			if (value < (1 << 7)) {
				return new byte[] { (byte)value };
			} else if (value < (1 << 14)) {
				return new byte[] {
					(byte)(0x80 | (value >> 7) & 0x7F),
					(byte)(value & 0x7F)
				};
			} else if (value < (1 << 21)) {
				return new byte[] {
					(byte)(0x80 | (value >> 14) & 0x7F),
					(byte)(0x80 | (value >> 7) & 0x7F),
					(byte)(value & 0x7F)
				};
			} else {
				return new byte[] {
					(byte)(0x80 | (value >> 21) & 0x7F),
					(byte)(0x80 | (value >> 14) & 0x7F),
					(byte)(0x80 | (value >> 7) & 0x7F),
					(byte)(value & 0x7F)
				};
			}
		}

		static int GetDeltaSize(int start, params byte[] arr) {
			var index = start;
			while (arr[index] >= 0x80 && (++index) < arr.Length) ;
			return index - start;
		}
	}
}
