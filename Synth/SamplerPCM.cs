namespace Synth {
	public partial class Sampler {
		void WritePCM() {
			for (int i = 0; i < mChannel.BufferSamples; i++) {
				/* ピッチ */
				double pitch;
				var tone = (int)((mVibIm * mVibDepth + mEgPitchValue) * 128);
				if (tone < 0) {
					tone *= -1;
					pitch = PITCH.HALFTONE[tone >> 7] * PITCH.FINETUNE[tone & 0x7F];
					pitch = 1.0 / pitch;
				} else {
					pitch = PITCH.HALFTONE[tone >> 7] * PITCH.FINETUNE[tone & 0x7F];
				}
				pitch *= mChannel.Pitch;
				/* 波形データの値を線形補間して取得 */
				var idxA = (int)mWavePos;
				var idxB = idxA + 1;
				var a2b = mWavePos - idxA;
				var wave = (mWaveData[idxA] * (1 - a2b) + mWaveData[idxB] * a2b) * mEgAmpValue * mWaveGain;
				/* 波形データ参照位置更新 */
				mWavePos += pitch * mWavePosDelta;
				if (mWavePos <= mWaveInfo.LoopBegin + mWaveInfo.LoopLength) {
					if (mWaveInfo.LoopEnable) {
						mWavePos -= mWaveInfo.LoopLength;
					} else {
						Id.State = STATE.STANDBY;
					}
				}
				/* LPF */
				{
					double c, s;
					{
						var rad = mEgLpfValue * (PI * ADJUST);
						var rad_2 = rad * rad;
						c = INV_FACT8;
						c *= rad_2;
						c -= INV_FACT6;
						c *= rad_2;
						c += INV_FACT4;
						c *= rad_2;
						c -= INV_FACT2;
						c *= rad_2;
						c++;
						s = INV_FACT9;
						s *= rad_2;
						s -= INV_FACT7;
						s *= rad_2;
						s += INV_FACT5;
						s *= rad_2;
						s -= INV_FACT3;
						s *= rad_2;
						s++;
						s *= rad;
					}
					var alpha = s / (mEnvLpf.Resonance * 4.0 + 1.0);
					var alpha1 = alpha + 1.0;
					var ka1 = -2.0 * c / alpha1;
					var kb1 = (1.0 - c) / alpha1;
					var ka2 = (1.0 - alpha) / alpha1;
					var kb0 = kb1 * 0.5;
					var temp
						= kb0 * wave
						+ kb1 * mLpfL.d1b1
						+ kb0 * mLpfL.d1b2
						- ka1 * mLpfL.d1a1
						- ka2 * mLpfL.d1a2;
					mLpfL.d1b2 = mLpfL.d1b1;
					mLpfL.d1b1 = wave;
					mLpfL.d1a2 = mLpfL.d1a1;
					mLpfL.d1a1 = temp;
					wave
						= kb0 * mLpfL.d1a1
						+ kb1 * mLpfL.d2b1
						+ kb0 * mLpfL.d2b2
						- ka1 * mLpfL.d2a1
						- ka2 * mLpfL.d2a2;
					mLpfL.d2b2 = mLpfL.d2b1;
					mLpfL.d2b1 = mLpfL.d1a1;
					mLpfL.d2a2 = mLpfL.d2a1;
					mLpfL.d2a1 = wave;
				}
				/* 波形に定位を掛けて出力 */
				mOutputL[i] = wave * mPanL;
				mOutputR[i] = wave * mPanR;
				/* 定位更新 */
				mPanL += (mRegionPanL * mChannel.PanRe - mRegionPanR * mChannel.PanIm - mPanL) * 0.1;
				mPanR += (mRegionPanR * mChannel.PanRe + mRegionPanL * mChannel.PanIm - mPanR) * 0.1;
				mPanL = (mPanL < 0) ? 0 : (mPanL > 1) ? 1 : mPanL;
				mPanR = (mPanR < 0) ? 0 : (mPanR > 1) ? 1 : mPanR;
				/* ビブラート更新 */
				mVibDepth += (mChannel.VibDepth - mVibDepth) * mDeltaTime4 / mChannel.VibDelay;
				var vibK = 1.570796 * mChannel.VibRate * mDeltaTime4;
				mVibRe -= mVibIm * vibK;
				mVibIm += mVibRe * vibK;
				/* エンベロープ更新 */
				switch (Id.State) {
				case STATE.PURGE:
					mEgAmpValue -= mEgAmpValue * 0.05;
					break;
				case STATE.PRESS:
					if (mEgAmpAttack) {
						mEgAmpValue += (1 - mEgAmpValue) * mDeltaTime4 / mEnvAmp.Attack;
						mEgAmpAttack = mEgAmpValue < THRESHOLD_ATTACK;
					} else {
						mEgAmpValue += (mEnvAmp.Sustain - mEgAmpValue) * mDeltaTime4 / mEnvAmp.Decay;
					}
					if (mEgLpfAttack) {
						mEgLpfValue += (mEnvLpf.Level - mEgLpfValue) * mDeltaTime4 / mEnvLpf.Attack;
						mEgLpfAttack = mEgLpfValue < (mEnvLpf.Level * LPF_LOWER) || mEgLpfValue > (mEnvLpf.Level * LPF_UPPER);
					} else {
						mEgLpfValue += (mEnvLpf.Sustain - mEgLpfValue) * mDeltaTime4 / mEnvLpf.Decay;
					}
					if (mEgPitchAttack) {
						mEgPitchValue += mEgPitchAttackDelta;
						mEgPitchAttack = mEgPitchValue < mEgPitchAttackMin || mEgPitchValue > mEgPitchAttackMax;
					} else {
						if (mEgPitchValue < mEgPitchDecayMin || mEgPitchValue > mEgPitchDecayMax) {
							mEgPitchValue += mEgPitchDecayDelta;
						}
					}
					break;
				case STATE.RELEASE:
					mEgAmpValue -= mEgAmpValue * mDeltaTime4 / mEnvAmp.Release;
					mEgLpfValue += (mEnvLpf.Fall - mEgLpfValue) * mDeltaTime4 / mEnvAmp.Release;
					if (mEgPitchValue < mEgPitchReleaseMin || mEgPitchValue > mEgPitchReleaseMax) {
						mEgPitchValue += mEgPitchReleaseDelta;
					}
					break;
				case STATE.HOLD:
					mEgAmpValue -= mEgAmpValue * mDeltaTime4 / mEnvAmp.Hold;
					mEgLpfValue += (mEnvLpf.Fall - mEgLpfValue) * mDeltaTime4 / mEnvAmp.Hold;
					break;
				}
				if (!mEgAmpAttack && mEgAmpValue < THRESHOLD_STANDBY) {
					Id.State = STATE.STANDBY;
				}
			}
		}
	}
}
