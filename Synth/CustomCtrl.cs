using SMF;

/// <summary>
/// コントロール・イベント
/// </summary>
public class CustomCtrl : Ctrl {
	/// <summary>
	/// コントロールの値
	/// </summary>
	public new double Value { get; set; }

	/// <summary>
	/// コントロール・イベントを作成
	/// </summary>
	/// <param name="message">SMFメッセージ</param>
	public CustomCtrl(byte[] message) : base(message) {
		switch (CtrlType) {
		case CTRL.MOD:
		case CTRL.VOL:
		case CTRL.EXP:
		case CTRL.RESONANCE:
		case CTRL.REV_SEND:
		case CTRL.CHO_SEND:
		case CTRL.DEL_SEND:
			Value = base.Value / 127.0;
			break;
		case CTRL.PAN:
			Value = (base.Value - 64) / 64.0;
			break;
		case CTRL.HOLD1:
			Value = base.Value < 64 ? 0 : 1;
			break;
		case CTRL.CUTOFF:
			Value = (base.Value - 64) / 64.0 + 1;
			break;
		default:
			Value = base.Value;
			break;
		}
	}

	/// <summary>
	/// コントロール・イベントを作成
	/// </summary>
	/// <param name="type">コントロールの種類</param>
	public CustomCtrl(CTRL type) : base(type) { }
}
