using System;
using SMF;
using Inst;

namespace Synth {
	public class Channel {
		const double THRESHOLD_STANDBY = 1e-8; //-80db
		const double THRESHOLD_ACTIVE = 1e-6; //-60db
		const double RMS_SPEED = 132.0;
		const double PEAK_ATT_DELTA = 2.0;
		const double PI2_SQRT3 = 3.6275987;

		public enum STATE : byte {
			STANDBY,
			STARTUP,
			ACTIVE
		}

		public readonly int Num;
		public STATE State { get; private set; }
		public string Name;

		public bool Mute { get; private set; }

		public bool IsDrum { get; private set; }
		public byte BankMsb { get; private set; }
		public byte BankLsb { get; private set; }
		public byte ProgNum { get; private set; }

		public bool Hold { get; private set; }

		public double Pitch { get; private set; }

		public double Vol { get; private set; }
		public double Exp { get; private set; }
		public double Pan { get; private set; }

		public double VibRange { get; private set; }
		public double VibDepth { get; private set; }
		public double VibRate { get; private set; }
		public double VibDelay { get; private set; }

		public double ChoSend { get; private set; }
		public double ChoRate { get; private set; }
		public double ChoDepth { get; private set; }

		public double DelSend { get; private set; }
		public double DelTime { get; private set; }
		public double DelFeedback { get; private set; }
		public double DelCross { get; private set; }

		public double Cutoff { get; private set; }
		public double Resonance { get; private set; }

		public double RmsL { get; private set; }
		public double RmsR { get; private set; }
		public double PeakL { get; private set; }
		public double PeakR { get; private set; }
		public bool EnableSpectrum {
			get { return mSpectrumGain > 0; }
			set { mSpectrumGain = value ? 1 : 0; }
		}

		internal double DeltaTime { get; private set; }
		internal double PanRe { get; private set; }
		internal double PanIm { get; private set; }
		internal int InputSamples { get; private set; }
		internal double[] InputBufferL;
		internal double[] InputBufferR;

		internal int RegionCount { get; private set; }
		internal REGION[] Regions { get; private set; }
		internal WAVE_INFO[] WaveInfoTable { get; private set; }

		RPN_TYPE mRpn;
		NRPN_TYPE mNrpn;
		byte mDataMSB;
		byte mDataLSB;
		int mBendRange;

		int mChoIdx;
		int mChoSamples;
		double mChoLfoU;
		double mChoLfoV;
		double mChoLfoW;
		double[] mChoBufferL;
		double[] mChoBufferR;

		int mDelIdx;
		int mDelSamples;
		double[] mDelBufferL;
		double[] mDelBufferR;

		double mGain;
		double mSpectrumGain;

		public Channel(int num) {
			Num = num;
			Name = "Channel " + num;
			Init();
		}

		public void Setup(int sampleRate, int bufferSamples) {
			State = STATE.STANDBY;

			mChoSamples = sampleRate / 10;
			mChoIdx = 0;
			mChoLfoU = 1.0;
			mChoLfoV = -0.5;
			mChoLfoW = -0.5;
			mChoBufferL = new double[mChoSamples];
			mChoBufferR = new double[mChoSamples];

			mDelSamples = sampleRate;
			mDelIdx = 0;
			mDelBufferL = new double[mDelSamples];
			mDelBufferR = new double[mDelSamples];

			DeltaTime = 1.0 / sampleRate;
			InputSamples = bufferSamples;
			InputBufferL = new double[InputSamples];
			InputBufferR = new double[InputSamples];
		}

		void Init() {
			Mute = false;

			IsDrum = false;
			BankMsb = 0;
			BankLsb = 0;
			ProgChange(0);

			Vol = 100 / 127.0;
			Exp = 1;
			mGain = Vol * Exp;
			mSpectrumGain = 1;
			SetPan(64);

			Hold = false;

			Cutoff = 1;
			Resonance = 0;

			Pitch = 0;
			mBendRange = 1;

			VibRange = 0.5;
			VibDepth = 0;
			VibRate = 4;
			VibDelay = 1.0;

			ChoSend = 0;
			ChoRate = 0.1;
			ChoDepth = 0.01;

			DelSend = 0;
			DelTime = 0.2;
			DelCross = 0;

			RmsL = 0;
			RmsR = 0;
			PeakL = 0;
			PeakR = 0;
			mRpn = RPN_TYPE.NULL;
			mNrpn = NRPN_TYPE.NULL;
		}

		void ResetCTRL() {
			Exp = 1;
			Hold = false;
			VibDepth = 0;
			Pitch = 0;
			mRpn = RPN_TYPE.NULL;
			mNrpn = NRPN_TYPE.NULL;
		}

		void SetPan(byte value) {
			var th = 0.25 * Math.PI * (value - 64) / 64.0;
			PanRe = Math.Cos(th);
			PanIm = Math.Sin(th);
			Pan = value;
		}

		void SetRpn() {
			switch (mRpn) {
			case RPN_TYPE.BEND_RANGE:
				mBendRange = mDataMSB;
				break;
			case RPN_TYPE.MOD_RANGE:
				VibRange = mDataMSB + mDataLSB / 128.0;
				break;
			default:
				break;
			}
		}

		void SetNrpn() {

		}

		public void SetWaveTable(IntPtr pWaveTable) {
			//WaveTable = pWaveTable;
		}

		public void NoteOn() {
			if (STATE.STANDBY == State) {
				State = STATE.STARTUP;
			}
		}

		public void CtrlChange(CTRL_TYPE type, byte value) {
			switch (type) {
			case CTRL_TYPE.BANK_MSB:
				BankMsb = value;
				break;
			case CTRL_TYPE.BANK_LSB:
				BankLsb = value;
				break;

			case CTRL_TYPE.MOD:
				VibDepth = VibRange * value / 127.0;
				break;

			case CTRL_TYPE.POLTA_TIME:
				break;
			case CTRL_TYPE.POLTA_SWITCH:
				break;

			case CTRL_TYPE.VOL:
				Vol = value / 127.0;
				break;
			case CTRL_TYPE.EXP:
				Exp = value / 127.0;
				break;
			case CTRL_TYPE.PAN:
				SetPan(value);
				break;

			case CTRL_TYPE.HOLD:
				Hold = value >= 64;
				break;

			case CTRL_TYPE.RESONANCE:
				Resonance = value / 127.0;
				break;
			case CTRL_TYPE.CUTOFF:
				Cutoff = value / 127.0;
				break;

			case CTRL_TYPE.VIB_RATE:
				VibRate = 20 * value / 127.0;
				break;
			case CTRL_TYPE.VIB_DEPTH:
				break;
			case CTRL_TYPE.VIB_DELAY:
				VibDelay = value / 127.0;
				break;

			case CTRL_TYPE.REV_SEND:
				break;
			case CTRL_TYPE.CHO_SEND:
				ChoSend = value / 127.0;
				break;
			case CTRL_TYPE.DEL_SEND:
				DelSend = value / 127.0;
				break;

			case CTRL_TYPE.DATA_MSB:
				mDataMSB = value;
				SetRpn();
				SetNrpn();
				break;
			case CTRL_TYPE.DATA_LSB:
				mDataLSB = value;
				SetRpn();
				SetNrpn();
				break;

			case CTRL_TYPE.NRPN_MSB:
				mNrpn = (NRPN_TYPE)((value << 8) | ((int)mNrpn & 0xFF));
				mRpn = RPN_TYPE.NULL;
				break;
			case CTRL_TYPE.NRPN_LSB:
				mNrpn = (NRPN_TYPE)((int)mNrpn & 0xFF00 | value);
				mRpn = RPN_TYPE.NULL;
				break;
			case CTRL_TYPE.RPN_MSB:
				mRpn = (RPN_TYPE)((value << 8) | ((int)mRpn & 0xFF));
				mNrpn = NRPN_TYPE.NULL;
				break;
			case CTRL_TYPE.RPN_LSB:
				mRpn = (RPN_TYPE)((int)mRpn & 0xFF00 | value);
				mNrpn = NRPN_TYPE.NULL;
				break;

			case CTRL_TYPE.RESET_ALL_CTRL:
				ResetCTRL();
				break;

			case CTRL_TYPE.DISABLE:
				Mute = true;
				break;
			case CTRL_TYPE.ENABLE:
				Mute = false;
				break;

			default:
				break;
			}
		}

		public void ProgChange(byte value) {
			ProgNum = value;
		}

		public void PitchBend(byte msb, byte lsb) {
			var value = ((msb << 7) | lsb) - 8192;
			Pitch = mBendRange * value / 8192.0;
		}

		public void WriteBuffer(double[] pOut, double[] pSpec) {
			for (int i = 0, j=0; i < InputSamples; i++, j+=2) {
				var outL = InputBufferL[i] * mGain;
				InputBufferL[i] = 0;
				var outR = InputBufferR[i] * mGain;
				InputBufferR[i] = 0;
				mGain += (Vol * Exp - mGain) * 0.1;
				/* コンプレッサー */
				{
				}
				/* イコライザー */
				{
				}
				/* コーラス */
				{
					/* U相V相W相の位置を取得 */
					var posU = mChoIdx - (mChoLfoU * 0.495 + 0.5) * ChoDepth;
					var posV = mChoIdx - (mChoLfoV * 0.495 + 0.5) * ChoDepth;
					var posW = mChoIdx - (mChoLfoW * 0.495 + 0.5) * ChoDepth;
					/* LFOを更新 */
					var choLfoD = ChoRate * PI2_SQRT3 * DeltaTime;
					mChoLfoU += (mChoLfoV - mChoLfoW) * choLfoD;
					mChoLfoV += (mChoLfoW - mChoLfoU) * choLfoD;
					mChoLfoW += (mChoLfoU - mChoLfoV) * choLfoD;
					/* バッファを更新 */
					mChoBufferL[mChoIdx] = outL;
					mChoBufferR[mChoIdx] = outR;
					mChoIdx++;
					mChoIdx -= mChoIdx / mChoSamples * mChoSamples;
					/* U相V相W相の値を線形補間 */
					var idxUa = (int)posU;
					var idxUb = idxUa + 1;
					var idxVa = (int)posV;
					var idxVb = idxVa + 1;
					var idxWa = (int)posW;
					var idxWb = idxWa + 1;
					var a2bU = posU - idxUa;
					var a2bV = posV - idxVa;
					var a2bW = posW - idxWa;
					idxUa -= idxUa / mChoSamples * mChoSamples;
					idxUb -= idxUb / mChoSamples * mChoSamples;
					idxVa -= idxVa / mChoSamples * mChoSamples;
					idxVb -= idxVb / mChoSamples * mChoSamples;
					idxWa -= idxWa / mChoSamples * mChoSamples;
					idxWb -= idxWb / mChoSamples * mChoSamples;
					var choL
						= mChoBufferL[idxUa] * (1 - a2bU) + mChoBufferL[idxUb] * a2bU
						+ mChoBufferL[idxVa] * (1 - a2bV) + mChoBufferL[idxVb] * a2bV
						+ mChoBufferL[idxWa] * (1 - a2bW) + mChoBufferL[idxWb] * a2bW;
					var choR
						= mChoBufferR[idxUa] * (1 - a2bU) + mChoBufferR[idxUb] * a2bU
						+ mChoBufferR[idxVa] * (1 - a2bV) + mChoBufferR[idxVb] * a2bV
						+ mChoBufferR[idxWa] * (1 - a2bW) + mChoBufferR[idxWb] * a2bW;
					/* 出力 */
					outL += choL * ChoSend / 3.0;
					outR += choR * ChoSend / 3.0;
				}
				/* ディレイ */
				{
					/* 遅延した値を取得 */
					var idx = (int)(mDelIdx - DelTime / DeltaTime);
					idx -= idx / mDelSamples * mDelSamples;
					var delL = mDelBufferL[idx];
					var delR = mDelBufferR[idx];
					/* 左右をミックス */
					var mixL = delL * (1 - DelCross) + delR * DelCross;
					var mixR = delR * (1 - DelCross) + delL * DelCross;
					/* バッファを更新 */
					mDelBufferL[mDelIdx] = outL + mixL * DelFeedback;
					mDelBufferR[mDelIdx] = outR + mixR * DelFeedback;
					mDelIdx++;
					mDelIdx -= mDelIdx / mDelSamples * mDelSamples;
					/* 出力 */
					outL += mixL * DelSend;
					outR += mixR * DelSend;
				}
				/* 出力 */
				pOut[j] += outL;
				pOut[j + 1] += outR;
				pSpec[j] += outL * mSpectrumGain;
				pSpec[j + 1] += outR * mSpectrumGain;
				/* メーター */
				{
					RmsL += ((outL * outL) - RmsL) * RMS_SPEED * DeltaTime;
					RmsR += ((outR * outR) - RmsR) * RMS_SPEED * DeltaTime;
					PeakL -= PeakL * PEAK_ATT_DELTA * DeltaTime;
					PeakR -= PeakR * PEAK_ATT_DELTA * DeltaTime;
					if (outL < 0) outL *= -1;
					if (outR < 0) outR *= -1;
					if (PeakL < outL) PeakL = outL;
					if (PeakR < outR) PeakR = outR;
				}
				/* 状態遷移 */
				if (STATE.ACTIVE == State && RmsL < THRESHOLD_STANDBY && RmsR < THRESHOLD_STANDBY) {
					State = STATE.STANDBY;
					break;
				} else if (STATE.STARTUP == State && (RmsL >= THRESHOLD_ACTIVE || RmsR >= THRESHOLD_ACTIVE)) {
					State = STATE.ACTIVE;
				}
			}
			/* 待機状態になったらクリア */
			if (STATE.STANDBY == State) {
				RmsL = 0;
				RmsR = 0;
				PeakL = 0;
				PeakR = 0;
				mChoIdx = 0;
				mChoLfoU = 1.0;
				mChoLfoV = -0.5;
				mChoLfoW = -0.5;
				for (int i = 0; i < mChoSamples; i++) {
					mChoBufferL[i] = 0;
					mChoBufferR[i] = 0;
				}
				mDelIdx = 0;
				for (int i = 0; i < mDelSamples; i++) {
					mDelBufferL[i] = 0;
					mDelBufferR[i] = 0;
				}
				for (int i = 0; i < InputSamples; i++) {
					InputBufferL[i] = 0;
					InputBufferR[i] = 0;
				}
			}
		}
	}
}
