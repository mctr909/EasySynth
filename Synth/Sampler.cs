using System;
using Synth.Instruments;

namespace Synth {
	public partial class Sampler {
		const double BASE_FREQ = 8.175798916;
		const double THRESHOLD_STANDBY = 0.0001; // -80db
		const double THRESHOLD_ATTACK = 0.98168; // 1-exp(-4)
		const double LPF_LOWER = 0.99; //-1%
		const double LPF_UPPER = 1.01; //+1%

		const double ADJUST = 0.975;
		const double PI = 3.14159265;
		const double INV_FACT2 = 5.00000000e-01;
		const double INV_FACT3 = 1.66666667e-01;
		const double INV_FACT4 = 4.16666667e-02;
		const double INV_FACT5 = 8.33333333e-03;
		const double INV_FACT6 = 1.38888889e-03;
		const double INV_FACT7 = 1.98412698e-04;
		const double INV_FACT8 = 2.48015873e-05;
		const double INV_FACT9 = 2.75573192e-06;

		static readonly sbyte[] SIN_TABLE = new sbyte[] { 0 };

		public enum STATE : byte {
			STANDBY,
			PURGE,
			PRESS,
			RELEASE,
			HOLD
		}

		public struct ID {
			public int Channel;
			public byte Note;
			public STATE State;
		}

		struct Filter {
			public double d1a1;
			public double d1a2;
			public double d1b1;
			public double d1b2;
			public double d2a1;
			public double d2a2;
			public double d2b1;
			public double d2b2;
			public void Clear() {
				d1a1 = 0;
				d1a2 = 0;
				d1b1 = 0;
				d1b2 = 0;
				d2a1 = 0;
				d2a2 = 0;
				d2b1 = 0;
				d2b2 = 0;
			}
		}

		internal delegate void DWriteBuffer();
		internal DWriteBuffer WriteBuffer;

		internal ID Id;

		#region private variables
		Channel mChannel;

		double mDeltaTime4;
		double[] mOutputL;
		double[] mOutputR;

		double mWaveGain;
		double mWavePos;
		double mWavePosDelta;
		WAVE_INFO mWaveInfo;
		short[] mWaveData;

		double mRegionPanL;
		double mRegionPanR;
		double mPanL;
		double mPanR;

		double mEgAmpValue;
		bool mEgAmpAttack;
		ENV_AMP mEnvAmp;

		double mEgLpfValue;
		bool mEgLpfAttack;
		ENV_LPF mEnvLpf;

		double mEgPitchValue;
		bool mEgPitchAttack;
		double mEgPitchAttackDelta;
		double mEgPitchAttackMin;
		double mEgPitchAttackMax;
		double mEgPitchDecayDelta;
		double mEgPitchDecayMin;
		double mEgPitchDecayMax;
		double mEgPitchReleaseDelta;
		double mEgPitchReleaseMin;
		double mEgPitchReleaseMax;

		double mVibDepth;
		double mVibRe;
		double mVibIm;

		Filter mLpfL;
		Filter mLpfR;
		#endregion

		internal void PurgeChannelNotes(Channel channel) {
			if (STATE.PRESS <= Id.State && mChannel == channel) {
				Id.State = STATE.PURGE;
			}
		}

		internal void HoldOff(Channel channel) {
			if (STATE.HOLD == Id.State && mChannel == channel) {
				Id.State = STATE.RELEASE;
			}
		}

		internal void NoteOff(Channel channel, int note) {
			if (STATE.PRESS == Id.State && mChannel == channel && Id.Note == note) {
				Id.State = mChannel.Hold ? STATE.HOLD : STATE.RELEASE;
			}
		}

		internal bool NoteOn(Channel channel, REGION region, int note, int velo) {
			if (STATE.STANDBY != Id.State) {
				if (mChannel == channel && Id.Note == note) {
					/* 既に鳴っている同音を破棄する */
					Id.State = STATE.PURGE;
				}
				/* ノート・オン失敗 */
				return false;
			}
			mChannel = channel;
			mDeltaTime4 = channel.DeltaTime * 4;
			mOutputL = channel.InputBufferL;
			mOutputR = channel.InputBufferR;
			/* バッファ書き込み関数を選択 */
			switch (region.WaveIndex) {
			case WAVE_TYPE.FM:
				WriteBuffer = WriteFM;
				break;
			case WAVE_TYPE.SAW:
				WriteBuffer = WriteSAW;
				break;
			case WAVE_TYPE.PWM:
				WriteBuffer = WritePWM;
				break;
			default:
				WriteBuffer = WritePCM;
				break;
			}
			/* 波形情報設定 */
			mWaveGain = region.Gain * velo / 127.0;
			mWavePos = 0;
			if (WritePCM == WriteBuffer) {
				mWaveInfo = channel.WaveInfoTable[region.WaveIndex];
				//mWaveData = mWaveInfo.Offset
				mWavePosDelta = region.Pitch * mWaveInfo.Pitch * mWaveInfo.SampleRate * channel.DeltaTime;
				var diffNote = note - mWaveInfo.UnityNote;
				if (diffNote < 0) {
					mWavePosDelta *= 1.0 / PITCH.HALFTONE[-diffNote];
				} else {
					mWavePosDelta *= PITCH.HALFTONE[diffNote];
				}
				mWaveGain *= mWaveInfo.Gain / 32768.0;
			} else {
				mWavePosDelta = BASE_FREQ * PITCH.HALFTONE[note] * region.Pitch * channel.DeltaTime;
			}
			/* 定位初期化 */
			mRegionPanL = region.PanL;
			mRegionPanR = region.PanR;
			mPanL = region.PanL * channel.PanRe - region.PanR * channel.PanIm;
			mPanR = region.PanR * channel.PanRe + region.PanL * channel.PanIm;
			/* EG初期化 */
			mEnvAmp = region.EnvAmp;
			mEgAmpValue = 0;
			mEgAmpAttack = true;
			mEnvLpf = region.EnvLpf;
			mEgLpfValue = region.EnvLpf.Rise;
			mEgLpfAttack = true;
			mEgPitchValue = region.EnvPitch.Rise;
			mEgPitchAttack = true;
			mEgPitchAttackDelta = (region.EnvPitch.Level - region.EnvPitch.Rise) * channel.DeltaTime / region.EnvPitch.Attack;
			mEgPitchAttackMin = region.EnvPitch.Level - Math.Abs(mEgPitchAttackDelta) / 2;
			mEgPitchAttackMax = region.EnvPitch.Level + Math.Abs(mEgPitchAttackDelta) / 2;
			mEgPitchDecayDelta = -region.EnvPitch.Level * channel.DeltaTime / region.EnvPitch.Decay;
			mEgPitchDecayMin = -Math.Abs(mEgPitchDecayDelta) / 2;
			mEgPitchDecayMax = Math.Abs(mEgPitchDecayDelta) / 2;
			mEgPitchReleaseDelta = region.EnvPitch.Fall * channel.DeltaTime / region.EnvAmp.Release;
			mEgPitchReleaseMin = region.EnvPitch.Fall - Math.Abs(mEgPitchReleaseDelta) / 2;
			mEgPitchReleaseMax = region.EnvPitch.Fall + Math.Abs(mEgPitchReleaseDelta) / 2;
			/* フィルター初期化 */
			mLpfL.Clear();
			mLpfR.Clear();
			/* ビブラート初期化 */
			mVibDepth = 0;
			mVibRe = 1;
			mVibIm = 0;
			/* ID設定 */
			Id.Channel = channel.Num;
			Id.Note = (byte)note;
			Id.State = STATE.PRESS;
			/* ノート・オン成功 */
			return true;
		}
	}
}
