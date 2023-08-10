namespace InteligentnyDomWebViewer.Model
{
	public class DeviceVersion
	{
		public enum ProgramStateType { Bootloader, Program, Unknown };

		public enum HardwareType1Enum
		{
			None = 0,
			Common = 1,
			DIN = 2,
			BOX = 3,
			RadioBOX = 4,
		};

		public enum HardwareType2Enum
		{
			None = 0,
			CU = 1,
			CU_WR = 2,
			Expander = 3,
			Radio = 4,
			Amplifier = 5,
			Acin = 41,
			Anin = 42,
			Anout = 43,
			Digin = 44,
			Dim = 45,
			Led = 46,
			Mul = 47,
			Rel = 48,
			Rol = 49,
			Temp = 50,
			Tablet = 81,
			TouchPanel = 82,
		};

		public enum HardwareType
		{
			None = (HardwareType1Enum.None << 8) | HardwareType2Enum.None,

			ISR_DIN_CU = (HardwareType1Enum.DIN << 8) | HardwareType2Enum.CU,
			ISR_DIN_CU_WR = (HardwareType1Enum.DIN << 8) | HardwareType2Enum.CU_WR,
			ISR_DIN_EKSP = (HardwareType1Enum.DIN << 8) | HardwareType2Enum.Expander,
			ISR_DIN_RADIO = (HardwareType1Enum.DIN << 8) | HardwareType2Enum.Radio,
			ISR_BOX_EKSP = (HardwareType1Enum.BOX << 8) | HardwareType2Enum.Expander,
			ISR_BOX_RADIO = (HardwareType1Enum.BOX << 8) | HardwareType2Enum.Radio,
			ISR_RADIO_AMP = (HardwareType1Enum.RadioBOX << 8) | HardwareType2Enum.Amplifier,
			// tablet, ISR_BOX_TP4, ISR_RADIO_TP4

			ISR_DIN_ACIN = (HardwareType1Enum.DIN << 8) | HardwareType2Enum.Acin,
			ISR_DIN_ANIN = (HardwareType1Enum.DIN << 8) | HardwareType2Enum.Anin,
			ISR_DIN_ANOUT = (HardwareType1Enum.DIN << 8) | HardwareType2Enum.Anout,
			ISR_DIN_DIGIN = (HardwareType1Enum.DIN << 8) | HardwareType2Enum.Digin,
			ISR_DIN_DIM = (HardwareType1Enum.DIN << 8) | HardwareType2Enum.Dim,
			ISR_DIN_LED = (HardwareType1Enum.DIN << 8) | HardwareType2Enum.Led,
			ISR_DIN_MUL = (HardwareType1Enum.DIN << 8) | HardwareType2Enum.Mul,
			ISR_DIN_REL = (HardwareType1Enum.DIN << 8) | HardwareType2Enum.Rel,
			ISR_DIN_ROL = (HardwareType1Enum.DIN << 8) | HardwareType2Enum.Rol,
			ISR_DIN_TEMP = (HardwareType1Enum.DIN << 8) | HardwareType2Enum.Temp,

			ISR_BOX_ACIN = (HardwareType1Enum.BOX << 8) | HardwareType2Enum.Acin,
			ISR_BOX_ANIN = (HardwareType1Enum.BOX << 8) | HardwareType2Enum.Anin,
			ISR_BOX_ANOUT = (HardwareType1Enum.BOX << 8) | HardwareType2Enum.Anout,
			ISR_BOX_DIGIN = (HardwareType1Enum.BOX << 8) | HardwareType2Enum.Digin,
			ISR_BOX_DIM = (HardwareType1Enum.BOX << 8) | HardwareType2Enum.Dim,
			ISR_BOX_LED = (HardwareType1Enum.BOX << 8) | HardwareType2Enum.Led,
			ISR_BOX_MUL = (HardwareType1Enum.BOX << 8) | HardwareType2Enum.Mul,
			ISR_BOX_REL = (HardwareType1Enum.BOX << 8) | HardwareType2Enum.Rel,
			ISR_BOX_ROL = (HardwareType1Enum.BOX << 8) | HardwareType2Enum.Rol,
			ISR_BOX_TEMP = (HardwareType1Enum.BOX << 8) | HardwareType2Enum.Temp,

			ISR_RADIO_ACIN = (HardwareType1Enum.RadioBOX << 8) | HardwareType2Enum.Acin,
			ISR_RADIO_DIGIN = (HardwareType1Enum.RadioBOX << 8) | HardwareType2Enum.Digin,
			ISR_RADIO_DIM = (HardwareType1Enum.RadioBOX << 8) | HardwareType2Enum.Dim,
			ISR_RADIO_LED = (HardwareType1Enum.RadioBOX << 8) | HardwareType2Enum.Led,
			ISR_RADIO_MUL = (HardwareType1Enum.RadioBOX << 8) | HardwareType2Enum.Mul,
			ISR_RADIO_REL = (HardwareType1Enum.RadioBOX << 8) | HardwareType2Enum.Rel,
			ISR_RADIO_ROL = (HardwareType1Enum.RadioBOX << 8) | HardwareType2Enum.Rol,
			ISR_RADIO_TEMP = (HardwareType1Enum.RadioBOX << 8) | HardwareType2Enum.Temp,
		}

		public enum RealHardwareType
		{
			None = (HardwareType1Enum.None << 8) | HardwareType2Enum.None,

			ISR_DIN_CU = (HardwareType1Enum.DIN << 8) | HardwareType2Enum.CU,
			ISR_DIN_CU_WR = (HardwareType1Enum.DIN << 8) | HardwareType2Enum.CU_WR,
			ISR_DIN_EKSP = (HardwareType1Enum.DIN << 8) | HardwareType2Enum.Expander,
			ISR_DIN_RADIO = (HardwareType1Enum.DIN << 8) | HardwareType2Enum.Radio,
			ISR_BOX_EKSP = (HardwareType1Enum.BOX << 8) | HardwareType2Enum.Expander,
			ISR_BOX_RADIO = (HardwareType1Enum.BOX << 8) | HardwareType2Enum.Radio,
			ISR_RADIO_AMP = (HardwareType1Enum.RadioBOX << 8) | HardwareType2Enum.Amplifier,
			// tablet, ISR_BOX_TP4, ISR_RADIO_TP4

			ISR_DIN_ACIN_4 = (HardwareType1Enum.DIN << 8) | HardwareType2Enum.Acin | (4 << 16),
			ISR_DIN_ANIN_4 = (HardwareType1Enum.DIN << 8) | HardwareType2Enum.Anin | (4 << 16),
			ISR_DIN_ANOUT_2 = (HardwareType1Enum.DIN << 8) | HardwareType2Enum.Anout | (2 << 16),
			ISR_DIN_DIGIN_4 = (HardwareType1Enum.DIN << 8) | HardwareType2Enum.Digin | (4 << 16),
			ISR_DIN_DIM_1 = (HardwareType1Enum.DIN << 8) | HardwareType2Enum.Dim | (1 << 16),
			ISR_DIN_LED_4 = (HardwareType1Enum.DIN << 8) | HardwareType2Enum.Led | (4 << 16),
			ISR_DIN_MUL = (HardwareType1Enum.DIN << 8) | HardwareType2Enum.Mul,
			ISR_DIN_REL_2 = (HardwareType1Enum.DIN << 8) | HardwareType2Enum.Rel | (2 << 16),
			ISR_DIN_ROL_1 = (HardwareType1Enum.DIN << 8) | HardwareType2Enum.Rol | (1 << 16),
			ISR_DIN_TEMP_4 = (HardwareType1Enum.DIN << 8) | HardwareType2Enum.Temp | (4 << 16),

			ISR_BOX_ACIN_3 = (HardwareType1Enum.BOX << 8) | HardwareType2Enum.Acin | (3 << 16),
			ISR_BOX_ANIN_3 = (HardwareType1Enum.BOX << 8) | HardwareType2Enum.Anin | (3 << 16),
			ISR_BOX_ANOUT_2 = (HardwareType1Enum.BOX << 8) | HardwareType2Enum.Anout | (2 << 16),
			ISR_BOX_DIGIN_3 = (HardwareType1Enum.BOX << 8) | HardwareType2Enum.Digin | (3 << 16),
			ISR_BOX_DIM_1 = (HardwareType1Enum.BOX << 8) | HardwareType2Enum.Dim | (1 << 16),
			ISR_BOX_LED_3 = (HardwareType1Enum.BOX << 8) | HardwareType2Enum.Led | (3 << 16),
			ISR_BOX_MUL = (HardwareType1Enum.BOX << 8) | HardwareType2Enum.Mul,
			ISR_BOX_REL_2 = (HardwareType1Enum.BOX << 8) | HardwareType2Enum.Rel | (2 << 16),
			ISR_BOX_ROL_1 = (HardwareType1Enum.BOX << 8) | HardwareType2Enum.Rol | (1 << 16),
			ISR_BOX_TEMP_2 = (HardwareType1Enum.BOX << 8) | HardwareType2Enum.Temp | (2 << 16),

			ISR_RADIO_ACIN_4 = (HardwareType1Enum.RadioBOX << 8) | HardwareType2Enum.Acin | (4 << 16),
			ISR_RADIO_DIGIN_4 = (HardwareType1Enum.RadioBOX << 8) | HardwareType2Enum.Digin | (4 << 16),
			ISR_RADIO_DIM_1 = (HardwareType1Enum.RadioBOX << 8) | HardwareType2Enum.Dim | (1 << 16),
			ISR_RADIO_LED_3 = (HardwareType1Enum.RadioBOX << 8) | HardwareType2Enum.Led | (3 << 16),
			ISR_RADIO_MUL = (HardwareType1Enum.RadioBOX << 8) | HardwareType2Enum.Mul,
			ISR_RADIO_REL_2 = (HardwareType1Enum.RadioBOX << 8) | HardwareType2Enum.Rel | (2 << 16),
			ISR_RADIO_ROL_1 = (HardwareType1Enum.RadioBOX << 8) | HardwareType2Enum.Rol | (1 << 16),
			ISR_RADIO_TEMP_3 = (HardwareType1Enum.RadioBOX << 8) | HardwareType2Enum.Temp | (3 << 16),
		}

		public enum CpuType { None, Stm32, Esp32 };

		public DateTime ProgrammedProgram = new();
		public DateTime ProgramDateTime = new();
		public byte ProgramVersionMajor;
		public byte ProgramVersionMinor;
		public DateTime BootloaderDateTime = new();
		public byte BootloaderVersionMajor;
		public byte BootloaderVersionMinor;
		public HardwareType1Enum HardwareType1;
		public HardwareType2Enum HardwareType2;
		public byte HardwareSegmentsCount;
		public byte HardwareVersion;
		public uint Uptime; /// seconds
		public float Vin;
		public ProgramStateType ProgramState = ProgramStateType.Unknown;
		public int TimeResponse;

		//public static DeviceVersion CreateReadDevice(RealHardwareType realHardwareType)
		//{
		//	int hardwareType = (int)realHardwareType;
		//	return new DeviceVersion()
		//	{
		//		HardwareType1 = (HardwareType1Enum)((hardwareType >> 8) & 0xff),
		//		HardwareType2 = (HardwareType2Enum)(hardwareType & 0xff),
		//		HardwareTypeCount = (byte)((hardwareType >> 16) & 0xff),
		//	};
		//}

		public CpuType GetCpuType()
		{
			if (HardwareType1 == HardwareType1Enum.DIN && HardwareType2 == HardwareType2Enum.CU ||
					HardwareType1 == HardwareType1Enum.DIN && HardwareType2 == HardwareType2Enum.CU_WR ||
					HardwareType1 == HardwareType1Enum.BOX)
				return CpuType.Esp32;

			if (HardwareType1 == HardwareType1Enum.DIN)
				return CpuType.Stm32;

			return CpuType.None;
		}

		public string ToString_(bool showProgram = true, bool showBootloader = false,
				bool showProgramDate = false, bool showBootloaderDate = false,
				bool showProgramState = false, bool showDescription = false)
		{
			string s = "";
			if (showProgram)
			{
				if (showDescription)
					s += "program: ";
				s += "v" + ProgramVersionMajor + "." + ProgramVersionMinor + " ";
				if (showProgramDate)
					s += ProgrammedProgram.ToString("yyyy-MM-dd HH:mm") + " ";
			}
			if (showBootloader)
			{
				if (showDescription)
					s += "bootloader: ";
				s += "v" + BootloaderVersionMajor + "." + BootloaderVersionMinor + " ";
				if (showBootloaderDate)
					s += BootloaderDateTime.ToShortDateString() + " ";
			}
			if (showProgramState)
			{
				if (showDescription)
					s += "state: ";
				switch (ProgramState)
				{
					case ProgramStateType.Program: s += "program"; break;
					case ProgramStateType.Bootloader: s += "bootloader"; break;
					case ProgramStateType.Unknown: s += "unknown"; break;
				}
			}
			return s.TrimEnd();
		}

		public string ToTableString()
		{
			string s = "";
			s += ("v" + ProgramVersionMajor + "." + ProgramVersionMinor).PadLeft(6) + "  ";
			s += ProgramDateTime.ToShortDateString() + "  ";
			s += ProgrammedProgram.ToString("yyyy-MM-dd HH:mm") + " ";
			s += ("v" + BootloaderVersionMajor + "." + BootloaderVersionMinor).PadLeft(6) + " ";
			s += BootloaderDateTime.ToShortDateString() + " ";
			s += TimeSpan.FromSeconds(Uptime).ToString().PadLeft(13) + " ";
			s += Vin.ToString("0.00").PadLeft(5) + "V ";

			string t = "";
			switch (ProgramState)
			{
				case ProgramStateType.Program: t += "program"; break;
				case ProgramStateType.Bootloader: t += "{$red}bootloader{$default}"; break;
				case ProgramStateType.Unknown: t += "unknown"; break;
			}
			t = t.PadLeft(13);

			//if (IsInDirectMode.HasValue)
			//  t += IsInDirectMode.Value ? "  DirectMode" : "    Normal  ";

			return s + t;
		}
	}
}
