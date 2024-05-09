using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SMF {
	public enum MSG_TYPE : byte {
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

	public enum META_TYPE : byte {
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
		TEMPO = 0x51,
		SMPTE = 0x54,
		MESURE = 0x58,
		KEY = 0x59,

		EOT = 0x2F,

		/* byte data */
		DATA = 0x7F,

		INVALID = 0xFF
	}

	public enum CTRL_TYPE : byte {
		BANK_MSB = 0,
		BANK_LSB = 32,
		MOD = 1,
		POLTA_TIME = 5,
		POLTA_SWITCH = 65,
		DATA_MSB = 6,
		DATA_LSB = 38,
		VOL = 7,
		PAN = 10,
		EXP = 11,
		HOLD = 64,
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

		DISABLE = 254,
		ENABLE = 255
	}

	public enum RPN_TYPE : ushort {
		BEND_RANGE = 0x0000,
		FINETUNE = 0x0001,
		COURSETUNE = 0x0002,
		MOD_RANGE = 0x0005,
		NULL = 0x7F7F
	}

	public class Event {
		public static Comparison<Event> SortCondition = new Comparison<Event>((a, b) => {
			var tick = a.Tick - b.Tick;
			if (0 == tick) {
				var track = a.Track - b.Track;
				if (0 == track) {
					return a.mIndex - b.mIndex;
				} else {
					return track;
				}
			} else {
				return tick;
			}
		});

		int mPort;
		int mIndex;

		public int Track;
		public int Tick;
		public byte[] Data { get; private set; }

		public MSG_TYPE Type {
			get {
				return (MSG_TYPE)(Data[0] >= (int)MSG_TYPE.SYS_EX ? Data[0] : (Data[0] & 0xF0));
			}
		}
		public CTRL_TYPE CtrlType {
			get {
				return (CTRL_TYPE)((Data[0] & 0xF0) == (int)MSG_TYPE.CTRL_CHG ? Data[1] : 0xFF);
			}
		}
		public META_TYPE MetaType {
			get {
				return (META_TYPE)(Data[0] == (int)MSG_TYPE.META ? Data[1] : 0xFF);
			}
		}

		public int Channel {
			get {
				return (Data[0] >= (int)MSG_TYPE.SYS_EX) ? -1 : ((mPort << 4) | (Data[0] & 0xF));
			}
			set {
				if (Data[0] < (int)MSG_TYPE.SYS_EX) {
					Data[0] = (byte)((Data[0] & 0xF0) | (value & 0xF));
					mPort = value >> 4;
				}
			}
		}

		Event() { }
		Event(params byte[] data) {
			Data = data;
		}

		public Event(FileStream fs, int track, ref int tick, ref int index, ref byte lastStatus) {
			Track = track;

			var delta = ReadDelta(fs);
			tick += delta;
			Tick = tick;

			mIndex = index;
			if (delta == 0) {
				index++;
			} else {
				index = 0;
			}

			var status = (byte)fs.ReadByte();
			byte data0;
			if (status < 0x80) {
				data0 = status;
				status = lastStatus;
			} else {
				data0 = (byte)fs.ReadByte();
				lastStatus = status;
			}

			var msgType = (MSG_TYPE)(status >= (int)MSG_TYPE.SYS_EX ? status : (status & 0xF0));
			switch (msgType) {
			case MSG_TYPE.PROG_CHG:
			case MSG_TYPE.CH_PRESS:
				Data = new byte[] { status, data0 };
				break;
			case MSG_TYPE.NOTE_ON: {
				var velo = (byte)fs.ReadByte();
				if (0 == velo) {
					Data = new byte[] { (byte)((byte)MSG_TYPE.NOTE_OFF | (status & 0xF)), data0, 0 };
				} else {
					Data = new byte[] { status, data0, velo };
				}
				break;
			}
			case MSG_TYPE.NOTE_OFF:
			case MSG_TYPE.KEY_PRESS:
			case MSG_TYPE.CTRL_CHG:
			case MSG_TYPE.PITCH:
				Data = new byte[] { status, data0, (byte)fs.ReadByte() };
				break;
			case MSG_TYPE.SYS_EX: {
				var arr = new List<byte>();
				arr.Add(status);
				arr.Add(data0);
				byte readByte;
				do {
					readByte = (byte)fs.ReadByte();
					arr.Add(readByte);
				} while (readByte != 0xF7);
				Data = arr.ToArray();
				break;
			}
			case MSG_TYPE.META: {
				var arr = new List<byte>();
				arr.Add(status);
				arr.Add(data0);
				var len = ReadDelta(fs);
				arr.AddRange(GetDelta(len));
				for (int i = 0; i < len; i++) {
					arr.Add((byte)fs.ReadByte());
				}
				Data = arr.ToArray();
				break;
			}
			default:
				throw new Exception();
			}
		}

		static int ReadDelta(FileStream fs) {
			int value = 0;
			int readByte;
			do {
				value <<= 7;
				readByte = fs.ReadByte();
				value |= readByte & 0x7F;
			} while (readByte >= 0x80);
			return value;
		}

		static byte[] GetDelta(int value) {
			if (value < (1 << 8)) {
				return new byte[] { (byte)value };
			} else if (value < (1 << 15)) {
				return new byte[] {
					(byte)(0x80 | (value >> 7) & 0x7F),
					(byte)(value & 0x7F)
				};
			} else if (value < (1 << 22)) {
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

		public static Event NoteOff(int channel, int note) {
			return new Event(
				(byte)((byte)MSG_TYPE.NOTE_OFF | (channel & 0xF)),
				(byte)note,
				0
			) {
				mPort = channel >> 4
			};
		}

		public static Event NoteOn(int channel, int note, int velo) {
			return new Event(
				(byte)((byte)MSG_TYPE.NOTE_ON | (channel & 0xF)),
				(byte)note,
				(byte)velo
			) {
				mPort = channel >> 4
			};
		}

		public static Event CtrlChange(CTRL_TYPE type, int channel, int value) {
			return new Event(
				(byte)((byte)MSG_TYPE.CTRL_CHG | (channel & 0xF)),
				(byte)type,
				(byte)value
			) {
				mPort = channel >> 4
			};
		}

		public static Event ProgChange(int channel, int value) {
			return new Event(
				(byte)((byte)MSG_TYPE.PROG_CHG | (channel & 0xF)),
				(byte)value
			) {
				mPort = channel >> 4
			};
		}

		public static Event Pitch(int channel, int value) {
			value += 8192;
			return new Event(
				(byte)((byte)MSG_TYPE.PITCH | (channel & 0xF)),
				(byte)(value & 0x7F),
				(byte)(value >> 7)
			) {
				mPort = channel >> 4
			};
		}

		public static Event SysEx(params byte[] data) {
			return new Event(data);
		}

		public static Event Tempo(double bpm) {
			return new Event(
				(byte)MSG_TYPE.META,
				(byte)META_TYPE.TEMPO,
				3, 0, 0, 0
			);
		}

		public static Event Measure(int num, int denomi) {
			return new Event(
				(byte)MSG_TYPE.META,
				(byte)META_TYPE.MESURE,
				4, 0, 0, 0, 0
			);
		}

		public static Event Key(int key) {
			return new Event(
				(byte)MSG_TYPE.META,
				(byte)META_TYPE.KEY,
				2,
				(byte)(key >> 7),
				(byte)(key & 0x7F)
			);
		}
	}

	public class File {
		public List<Event> EventList = new List<Event>();

		int mFormat;
		int mTrackCount;
		int mUnitTick;

		public File(string filePath) {
			if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath)) {
				return;
			}
			using (var fs = new FileStream(filePath, FileMode.Open)) {
				LoadMThd(fs);
				for (int i = 0; i < mTrackCount; i++) {
					LoadTrack(fs, i);
				}
			}
			EventList.Sort(Event.SortCondition);
		}

		void LoadMThd(FileStream fs) {
			var id = Encoding.ASCII.GetString(ReadBytes(fs, 4));
			var len = ReadUInt32(fs);
			if ("MThd" != id || 6 != len) {
				throw new Exception();
			}
			mFormat = ReadUInt16(fs);
			if (mFormat >= 3) {
				throw new Exception();
			}
			mTrackCount = ReadUInt16(fs);
			mUnitTick = ReadUInt16(fs);
		}

		void LoadTrack(FileStream fs, int number) {
			var id = Encoding.ASCII.GetString(ReadBytes(fs, 4));
			if ("MTrk" != id) {
				EventList.Clear();
				throw new Exception();
			}
			var len = ReadUInt32(fs);
			var end = fs.Position + len;
			var tick = 0;
			var index = 0;
			byte lastStatus = 0;
			while (fs.Position < end) {
				var ev = new Event(fs, number, ref tick, ref index, ref lastStatus);
				ev.Tick *= 960 / mUnitTick;
				EventList.Add(ev);
			}
		}

		static byte[] ReadBytes(FileStream fs, int size) {
			var arr = new byte[size];
			fs.Read(arr, 0, size);
			return arr;
		}

		static uint ReadUInt32(FileStream fs) {
			var value = (uint)fs.ReadByte() << 24;
			value |= (uint)fs.ReadByte() << 16;
			value |= (uint)fs.ReadByte() << 8;
			value |= (uint)fs.ReadByte();
			return value;
		}

		static ushort ReadUInt16(FileStream fs) {
			var value = (ushort)(fs.ReadByte() << 8);
			value |= (ushort)fs.ReadByte();
			return value;
		}
	}
}
