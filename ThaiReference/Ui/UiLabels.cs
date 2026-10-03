namespace ThaiReference.Ui;

public static class UiLabels
{
	public static class Shell
	{
		public const string Orphography = "shell.orphography";
		public const string Grammar = "shell.grammar";
		public const string BasicTerms = "shell.basic_terms";
		public const string Space = "shell.space";
		public const string Time = "shell.time";
		public const string Homophones = "shell.homophones";
		public const string MinimalSets = "shell.minimal_sets";
		public const string Settings = "shell.settings";

		public const string Consonants = "shell.consonants";
		public const string Vowels = "shell.vowels";
		public const string Marks = "shell.marks";
		public const string ToneRules = "shell.tone_rules";
		public const string Clusters = "shell.clusters";
		public const string Numbers = "shell.numbers";
		public const string Pronouns = "shell.pronouns";
		public const string Colors = "shell.colors";
		public const string Directions = "shell.directions";
		public const string Position = "shell.position";
		public const string Grid = "shell.grid";
		public const string Hours = "shell.hours";
		public const string Days = "shell.days";
		public const string Months = "shell.months";
	}

	public static class Account
	{
		public const string Devices = "account.devices";
		public const string ThisDevice = "account.this_device";
		public const string Members = "account.members";
		public const string Unlink = "account.unlink";
		public const string ShowQr = "account.show_qr";
		public const string HideQr = "account.hide_qr";
		public const string ScanQr = "account.scan_qr";
		public const string Email = "account.email";
		public const string EmailHint = "account.email_hint";
		public const string AttachEmail = "account.attach_email";
		public const string ConfirmEmail = "account.confirm_email";
		public const string Recover = "account.recover";
		public const string ConfirmRecover = "account.confirm_recover";
		public const string CodeHint = "account.code_hint";
		public const string StatusOffline = "account.status_offline";
		public const string StatusPaired = "account.status_paired";
		public const string StatusUnlinked = "account.status_unlinked";
		public const string StatusEmailSent = "account.status_email_sent";
		public const string StatusRecoverSent = "account.status_recover_sent";
		public const string StatusEmailConfirmed = "account.status_email_confirmed";
		public const string StatusRecovered = "account.status_recovered";
	}

	public static class Chrome
	{
		public const string ModernFont = "chrome.modern_font";
		public const string Romanisation = "chrome.romanisation";
		public const string ViewOptions = "chrome.view_options";
		public const string PreferRecordings = "chrome.prefer_recordings";
		public const string UiLanguage = "chrome.ui_language";
		public const string AppName = "chrome.app_name";
	}

	public static class Options
	{
		public const string List = "opt.list";
		public const string ListBySound = "opt.list_by_sound";
		public const string ListBySoundType = "opt.list_by_sound_type";

		public const string ClassLow = "opt.class.low";
		public const string ClassMid = "opt.class.mid";
		public const string ClassHigh = "opt.class.high";

		public const string PosInitial = "opt.pos.initial";
		public const string PosFinal = "opt.pos.final";
		public const string PosInitialAndFinal = "opt.pos.initial_and_final";

		public const string PluralitySingular = "opt.plurality.singular";
		public const string PluralityAll = "opt.plurality.all";
		public const string PluralityPlural = "opt.plurality.plural";

		public const string PersonFirst = "opt.person.first";
		public const string PersonSecond = "opt.person.second";
		public const string PersonThird = "opt.person.third";

		public const string SoundAll = "opt.sound.all";
		public const string SoundShort = "opt.sound.short";
		public const string SoundLong = "opt.sound.long";

		public const string VizSound = "opt.viz.sound";
		public const string VizOpen = "opt.viz.open";
		public const string VizClosed = "opt.viz.closed";
	}

	public static class Marks
	{
		public const string ToneSection = "marks.tone_section";
		public const string MiscSection = "marks.misc_section";
	}

	public static class Hints
	{
		public const string ColorsTap = "hint.colors_tap";
		public const string DaysTap = "hint.days_tap";
		public const string DaysWan = "hint.days_wan";
		public const string DirectionsTap = "hint.directions_tap";
		public const string GridTap = "hint.grid_tap";
		public const string MonthsTap = "hint.months_tap";
		public const string ClustersExamplesFor = "hint.clusters_examples_for";
	}

	public static class Numbers
	{
		public const string Units = "numbers.units";
	}

	public static class Pronouns
	{
		public const string Details = "pronouns.details";
	}

	public static class Clusters
	{
		public const string True = "clusters.true";
		public const string False = "clusters.false";
		public const string Leading = "clusters.leading";
		public const string Enepenthetic = "clusters.enepenthetic";
		public const string NonSonorant = "clusters.non_sonorant";
		public const string SubSyllable = "clusters.sub_syllable";

		public const string SubtypeEnepenthetic = "clusters.subtype.enepenthetic";
		public const string SubtypeEnepentheticPreposed = "clusters.subtype.enepenthetic_preposed";
		public const string SubtypeLowClassSecond = "clusters.subtype.low_class_second";
		public const string SubtypeMidClassSecond = "clusters.subtype.mid_class_second";
		public const string SubtypeHighClassSecond = "clusters.subtype.high_class_second";
	}

	public static class Tone
	{
		public const string Mid = "tone.mid";
		public const string Low = "tone.low";
		public const string Falling = "tone.falling";
		public const string High = "tone.high";
		public const string Rising = "tone.rising";
	}

	public static class ToneRules
	{
		public const string EmptyBody = "tonerules.empty_body";
		public const string ChartLong = "tonerules.chart.long";
		public const string ChartShort = "tonerules.chart.short";

		public static string TitleId(string cellId) => $"tonerules.{cellId}.title";
		public static string DetailId(string cellId) => $"tonerules.{cellId}.detail";
	}

	/// <summary>Shell <see cref="Microsoft.Maui.Controls.Shell"/> Route / ClassId → label id.</summary>
	public static readonly IReadOnlyDictionary<string, string> RouteToId =
		new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			["orphography"] = Shell.Orphography,
			["grammar"] = Shell.Grammar,
			["terms"] = Shell.BasicTerms,
			["space"] = Shell.Space,
			["time"] = Shell.Time,
			["homophones"] = Shell.Homophones,
			["minimalsets"] = Shell.MinimalSets,
			["settings"] = Shell.Settings,

			["Consonants"] = Shell.Consonants,
			["Vowels"] = Shell.Vowels,
			["Marks"] = Shell.Marks,
			["ToneRules"] = Shell.ToneRules,
			["Clusters"] = Shell.Clusters,
			["Numbers"] = Shell.Numbers,
			["Pronouns"] = Shell.Pronouns,
			["Colors"] = Shell.Colors,
			["Directions"] = Shell.Directions,
			["Positions"] = Shell.Position,
			["Grid"] = Shell.Grid,
			["Hours"] = Shell.Hours,
			["Days"] = Shell.Days,
			["Months"] = Shell.Months,
			["Homophones"] = Shell.Homophones,
			["MinimalSets"] = Shell.MinimalSets,
			["Settings"] = Shell.Settings,
		};

	public static readonly IReadOnlyDictionary<string, string> ClusterTypeToId =
		new Dictionary<string, string>(StringComparer.Ordinal)
		{
			["true"] = Clusters.True,
			["false"] = Clusters.False,
			["leading"] = Clusters.Leading,
			["enepenthetic"] = Clusters.Enepenthetic,
			["non-sonorant"] = Clusters.NonSonorant,
			["sub-syllable"] = Clusters.SubSyllable,
		};

	public static readonly IReadOnlyDictionary<string, string> ClusterSubtypeToId =
		new Dictionary<string, string>(StringComparer.Ordinal)
		{
			["enepenthetic"] = Clusters.SubtypeEnepenthetic,
			["enepenthetic_preposed"] = Clusters.SubtypeEnepentheticPreposed,
			["low_class_second"] = Clusters.SubtypeLowClassSecond,
			["mid_class_second"] = Clusters.SubtypeMidClassSecond,
			["high_class_second"] = Clusters.SubtypeHighClassSecond,
		};

	static readonly Dictionary<string, (string Th, string En)> Map = new(StringComparer.Ordinal)
	{
		[Shell.Orphography] = ("อักขรวิธี", "Orphography"),
		[Shell.Grammar] = ("ไวยากรณ์", "Grammar"),
		[Shell.BasicTerms] = ("คำพื้นฐาน", "Basic Terms"),
		[Shell.Space] = ("ตำแหน่ง", "Space"),
		[Shell.Time] = ("เวลา", "Time"),
		[Shell.Homophones] = ("คำพ้องเสียง", "Homophones"),
		[Shell.MinimalSets] = ("ชุดเสียงใกล้เคียง", "Minimal sets"),
		[Shell.Settings] = ("ตั้งค่า", "Settings"),

		[Shell.Consonants] = ("พยัญชนะ", "Consonants"),
		[Shell.Vowels] = ("สระ", "Vowels"),
		[Shell.Marks] = ("เครื่องหมาย", "Marks"),
		[Shell.ToneRules] = ("กฎวรรณยุกต์", "Tone rules"),
		[Shell.Clusters] = ("อักษรควบ", "Clusters"),
		[Shell.Numbers] = ("ตัวเลข", "Numbers"),
		[Shell.Pronouns] = ("สรรพนาม", "Pronouns"),
		[Shell.Colors] = ("สี", "Colors"),
		[Shell.Directions] = ("ทิศทาง", "Directions"),
		[Shell.Position] = ("ตำแหน่ง", "Position"),
		[Shell.Grid] = ("ตาราง", "Grid"),
		[Shell.Hours] = ("ชั่วโมง", "Hours"),
		[Shell.Days] = ("วัน", "Days"),
		[Shell.Months] = ("เดือน", "Months"),

		[Chrome.ModernFont] = ("ฟอนต์สมัยใหม่", "Modern font"),
		[Chrome.Romanisation] = ("การถอดเสียง", "Romanisation type"),
		[Chrome.ViewOptions] = ("ตัวเลือกมุมมอง", "View options"),
		[Account.Devices] = ("อุปกรณ์", "Devices"),
		[Account.ThisDevice] = ("อุปกรณ์นี้", "This device"),
		[Account.Members] = ("สมาชิกในกลุ่ม", "Group members"),
		[Account.Unlink] = ("ยกเลิกการเชื่อม", "Unlink"),
		[Account.ShowQr] = ("แสดง QR", "Show QR"),
		[Account.HideQr] = ("ปิด QR", "Hide QR"),
		[Account.ScanQr] = ("สแกน QR", "Scan QR"),
		[Account.Email] = ("อีเมล", "Email"),
		[Account.EmailHint] = ("อีเมลสำหรับกู้คืน", "Recovery email"),
		[Account.AttachEmail] = ("ผูกอีเมล", "Attach email"),
		[Account.ConfirmEmail] = ("ยืนยันรหัส", "Confirm code"),
		[Account.Recover] = ("กู้คืน", "Recover"),
		[Account.ConfirmRecover] = ("ยืนยันรหัสกู้คืน", "Confirm recovery code"),
		[Account.CodeHint] = ("รหัสจากอีเมล", "Code from email"),
		[Account.StatusOffline] = ("เซิร์ฟเวอร์ไม่พร้อม", "Server unavailable"),
		[Account.StatusPaired] = ("เชื่อมอุปกรณ์แล้ว", "Device linked"),
		[Account.StatusUnlinked] = ("ยกเลิกการเชื่อมแล้ว", "Device unlinked"),
		[Account.StatusEmailSent] = ("ส่งรหัสแล้ว ตรวจกล่องจดหมาย", "Code sent. Check your email"),
		[Account.StatusRecoverSent] = ("ถ้ามีบัญชี จะส่งรหัสกู้คืน", "If that account exists, a recovery code was sent"),
		[Account.StatusEmailConfirmed] = ("ผูกอีเมลแล้ว", "Email attached"),
		[Account.StatusRecovered] = ("กู้คืนแล้ว", "Recovered"),

		[Chrome.PreferRecordings] = ("ใช้ไฟล์เสียงถ้ามี", "Prefer recordings if present"),
		[Chrome.UiLanguage] = ("ภาษา UI", "UI language"),
		[Chrome.AppName] = ("Thai Reference", "Thai Reference"),

		[Options.List] = ("รายการ", "List"),
		[Options.ListBySound] = ("รายการตามเสียง", "List by sound"),
		[Options.ListBySoundType] = ("รายการตามชนิดเสียง", "List by sound type"),

		[Options.ClassLow] = ("อักษรต่ำ", "Low"),
		[Options.ClassMid] = ("อักษรกลาง", "Mid"),
		[Options.ClassHigh] = ("อักษรสูง", "High"),

		[Options.PosInitial] = ("พยัญชนะต้น", "Initial"),
		[Options.PosFinal] = ("พยัญชนะท้าย", "Final"),
		[Options.PosInitialAndFinal] = ("ต้นและท้าย", "Initial & final"),

		[Options.PluralitySingular] = ("เอกพจน์", "Singular"),
		[Options.PluralityAll] = ("ทั้งหมด", "All"),
		[Options.PluralityPlural] = ("พหูพจน์", "Plural"),

		[Options.PersonFirst] = ("บุรุษที่หนึ่ง", "First"),
		[Options.PersonSecond] = ("บุรุษที่สอง", "Second"),
		[Options.PersonThird] = ("บุรุษที่สาม", "Third"),

		[Options.SoundAll] = ("ทั้งหมด", "All"),
		[Options.SoundShort] = ("สั้น", "Short"),
		[Options.SoundLong] = ("ยาว", "Long"),

		[Options.VizSound] = ("เสียง", "Sound"),
		[Options.VizOpen] = ("คำเป็น", "Open"),
		[Options.VizClosed] = ("คำตาย", "Closed"),

		[Marks.ToneSection] = ("วรรณยุกต์", "Tone marks"),
		[Marks.MiscSection] = ("เครื่องหมายอื่น", "Miscellaneous"),

		[Hints.ColorsTap] = ("แตะสีบนวงล้อ", "Tap a color on the wheel"),
		[Hints.DaysTap] = ("แตะวันเพื่อดูชื่อวันภาษาไทย สี และดาว", "Tap a day for the Thai weekday, color, and planet"),
		[Hints.DaysWan] = ("วัน", "wan"),
		[Hints.DirectionsTap] = ("แตะทิศบนเข็มทิศ", "Tap a direction on the rose"),
		[Hints.GridTap] = ("แตะช่องบนตาราง", "Tap a cell on the grid"),
		[Hints.MonthsTap] = ("แตะวงนอกสำหรับราศี · วงในสำหรับชื่อเดือน", "Tap the outer ring for zodiac sign · inner ring for month name"),
		[Hints.ClustersExamplesFor] = ("ตัวอย่างของ", "Examples for"),

		[Numbers.Units] = ("หน่วย", "Units"),
		[Pronouns.Details] = ("รายละเอียด", "Details"),

		[Clusters.True] = ("อักษรควบแท้", "True clusters"),
		[Clusters.False] = ("อักษรควบไม่แท้", "False clusters"),
		[Clusters.Leading] = ("ห/อ + อักษรต่ำ", "Tone shifting · ห/อ + low-class"),
		[Clusters.Enepenthetic] = ("สวรภักดิ์", "Enepenthetic"),
		[Clusters.NonSonorant] = ("พยัญชนะที่สองไม่ใช่เสียงก้อง", "Non-sonorant"),
		[Clusters.SubSyllable] = ("[บร-] → [เบาะ-ร-]", "Sub-syllable · [บร-] → [เบาะ-ร-]"),

		[Clusters.SubtypeEnepenthetic] = ("สวรภักดิ์", "Enepenthetic"),
		[Clusters.SubtypeEnepentheticPreposed] = ("สวรภักดิ์ (สระหน้า)", "Enepenthetic preposed"),
		[Clusters.SubtypeLowClassSecond] = ("ตัวหลังอักษรต่ำ", "Low class second"),
		[Clusters.SubtypeMidClassSecond] = ("ตัวหลังอักษรกลาง", "Mid class second"),
		[Clusters.SubtypeHighClassSecond] = ("ตัวหลังอักษรสูง", "High class second"),

		[Tone.Mid] = ("สามัญ", "Mid"),
		[Tone.Low] = ("เอก", "Low"),
		[Tone.Falling] = ("โท", "Falling"),
		[Tone.High] = ("ตรี", "High"),
		[Tone.Rising] = ("จัตวา", "Rising"),

		[ToneRules.EmptyBody] = (
			"แตะช่องเพื่อดูรายละเอียด",
			"Tap a cell for details"),
		[ToneRules.ChartLong] = ("ยาว", "Long"),
		[ToneRules.ChartShort] = ("สั้น", "Short"),

		[ToneRules.TitleId("hdr.class")] = ("พ.ต.", "IC"),
		[ToneRules.DetailId("hdr.class")] = (
			"พยัญชนะไทยแบ่งเป็นอักษรต่ำ กลาง สูง ชั้นอักษร ชนิดพยางค์ และวรรณยุกต์กำหนดเสียงวรรณยุกต์",
			"Thai consonants fall into low, mid, or high class. Class, syllable type and tone mark determine the spoken tone"),
		[ToneRules.TitleId("hdr.syllable")] = ("ชนิดพยางค์", "Syllable type"),
		[ToneRules.DetailId("hdr.syllable")] = (
			"พยางค์ไทยเป็นคำเป็นหรือคำตาย ซึ่งกำหนดวรรณยุกต์ที่ไม่มีรูป",
			"Thai syllables can be live or dead, which determines the derived (unmarked) tone"),
		[ToneRules.TitleId("hdr.tonemark")] = ("วรรณยุกต์", "Tone mark"),
		[ToneRules.DetailId("hdr.tonemark")] = (
			"รูปวรรณยุกต์แทนเสียงที่ไม่มีรูป ◌๊ และ ◌๋ ใช้กับพ.ต. อักษรกลางเท่านั้น",
			"Tone marks override the derived tone. ◌๊ and ◌๋ occur only with mid-class IC"),
		[ToneRules.TitleId("hdr.live")] = ("คำเป็น", "Live"),
		[ToneRules.DetailId("hdr.live")] = (
			"พยางค์คำเป็นลงท้ายด้วยสระยาว สระประสม หรือพ.ท. เสียงก้อง (ง น ม ย ว)",
			"A live syllable ends in a long vowel, diphthong or a sonorant FC (ง น ม ย ว)"),
		[ToneRules.TitleId("hdr.dead")] = ("คำตาย", "Dead"),
		[ToneRules.DetailId("hdr.dead")] = (
			"พยางค์คำตายลงท้ายด้วยเสียงกัก (ก ด บ) หรือสระสั้น",
			"A dead syllable ends in a stop sound (k t p) or a short vowel"),
		[ToneRules.TitleId("hdr.long")] = ("คำตาย · ยาว", "Dead · long"),
		[ToneRules.DetailId("hdr.long")] = (
			"พยางค์คำตายที่มีสระยาวหรือสระประสม",
			"Dead syllable with a long vowel or a diphthong"),
		[ToneRules.TitleId("hdr.short")] = ("คำตาย · สั้น", "Dead · short"),
		[ToneRules.DetailId("hdr.short")] = (
			"พยางค์คำตายที่มีสระสั้น",
			"Dead syllable with a short vowel"),
		[ToneRules.TitleId("hdr.mai_ek")] = ("◌่ ไม้เอก", "◌่"),
		[ToneRules.DetailId("hdr.mai_ek")] = (
			"ไม้เอก อักษรกลาง/สูง → เสียงเอก อักษรต่ำ → เสียงโท",
			"First tone mark. With mid/high initials → low tone; with low initials → falling tone."),
		[ToneRules.TitleId("hdr.mai_tho")] = ("◌้ ไม้โท", "◌้"),
		[ToneRules.DetailId("hdr.mai_tho")] = (
			"ไม้โท อักษรกลาง/สูง → เสียงโท อักษรต่ำ → เสียงตรี",
			"Second tone mark. With mid/high initials → falling tone; with low initials → high tone."),
		[ToneRules.TitleId("hdr.mai_tri")] = ("◌๊ ไม้ตรี", "◌๊"),
		[ToneRules.DetailId("hdr.mai_tri")] = (
			"ไม้ตรี ใช้กับอักษรกลาง → เสียงตรี",
			"Third tone mark. Used with mid-class initials → high tone."),
		[ToneRules.TitleId("hdr.mai_chattawa")] = ("◌๋ ไม้จัตวา", "◌๋"),
		[ToneRules.DetailId("hdr.mai_chattawa")] = (
			"ไม้จัตวา ใช้กับอักษรกลาง → เสียงจัตวา",
			"Fourth tone mark. Used with mid-class initials → rising tone."),
		[ToneRules.TitleId("hdr.low")] = ("อักษรต่ำ", "Low class"),
		[ToneRules.DetailId("hdr.low")] = (
			"พ.ต. อักษรต่ำ (เช่น ค ช ท พ ฟ ฮ …) ใช้ได้แค่ ◌่ และ ◌้",
			"Low-class initials (e.g. ค ช ท พ ฟ ฮ …). Only ◌่ and ◌้ can be used"),
		[ToneRules.TitleId("hdr.mid")] = ("อักษรกลาง", "Mid class"),
		[ToneRules.DetailId("hdr.mid")] = (
			"พ.ต. อักษรกลาง (ก จ ด ต บ ป อ) ใช้รูปวรรณยุกต์ได้ครบสี่ตัว",
			"Mid-class initials (ก จ ด ต บ ป อ). All four tone marks are available."),
		[ToneRules.TitleId("hdr.high")] = ("อักษรสูง", "High class"),
		[ToneRules.DetailId("hdr.high")] = (
			"พ.ต. อักษรสูง (ข ฉ ถ ผ ฝ ศ ษ ส ห) ใช้ได้แค่ ◌่ และ ◌้",
			"High-class initials (ข ฉ ถ ผ ฝ ศ ษ ส ห). Only ◌่ and ◌้ can be used."),

		[ToneRules.TitleId("low.live")] = ("ต่ำ · คำเป็น · ไม่มีรูป", "Low · live · unmarked"),
		[ToneRules.DetailId("low.live")] = ("พยางค์คำเป็น อักษรต่ำ ไม่มีรูป → เสียงสามัญ", "Live syllable, low-class initial, no tone mark → mid tone."),
		[ToneRules.TitleId("low.dead.long")] = ("ต่ำ · คำตาย · ยาว · ไม่มีรูป", "Low · dead · long · unmarked"),
		[ToneRules.DetailId("low.dead.long")] = ("พยางค์คำตายสระยาว อักษรต่ำ ไม่มีรูป → เสียงโท", "Dead syllable with long vowel, low-class initial, no tone mark → falling tone."),
		[ToneRules.TitleId("low.dead.short")] = ("ต่ำ · คำตาย · สั้น · ไม่มีรูป", "Low · dead · short · unmarked"),
		[ToneRules.DetailId("low.dead.short")] = ("พยางค์คำตายสระสั้น อักษรต่ำ ไม่มีรูป → เสียงตรี", "Dead syllable with short vowel, low-class initial, no tone mark → high tone."),
		[ToneRules.TitleId("low.mai_ek")] = ("ต่ำ · ◌่", "Low · ◌่"),
		[ToneRules.DetailId("low.mai_ek")] = ("อักษรต่ำกับไม้เอก → เสียงโท", "Low-class initial with ◌่ → falling tone."),
		[ToneRules.TitleId("low.mai_tho")] = ("ต่ำ · ◌้", "Low · ◌้"),
		[ToneRules.DetailId("low.mai_tho")] = ("อักษรต่ำกับไม้โท → เสียงตรี", "Low-class initial with ◌้ → high tone."),

		[ToneRules.TitleId("mid.live")] = ("กลาง · คำเป็น · ไม่มีรูป", "Mid · live · unmarked"),
		[ToneRules.DetailId("mid.live")] = ("พยางค์คำเป็น อักษรกลาง ไม่มีรูป → เสียงสามัญ", "Live syllable, mid-class initial, no tone mark → mid tone."),
		[ToneRules.TitleId("mid.dead")] = ("กลาง · คำตาย · ไม่มีรูป", "Mid · dead · unmarked"),
		[ToneRules.DetailId("mid.dead")] = ("พยางค์คำตาย (ยาวหรือสั้น) อักษรกลาง ไม่มีรูป → เสียงเอก", "Dead syllable (long or short), mid-class initial, no tone mark → low tone."),
		[ToneRules.TitleId("mid.mai_ek")] = ("กลาง · ◌่", "Mid · ◌่"),
		[ToneRules.DetailId("mid.mai_ek")] = ("อักษรกลางกับไม้เอก → เสียงเอก", "Mid-class initial with ◌่ → low tone."),
		[ToneRules.TitleId("mid.mai_tho")] = ("กลาง · ◌้", "Mid · ◌้"),
		[ToneRules.DetailId("mid.mai_tho")] = ("อักษรกลางกับไม้โท → เสียงโท", "Mid-class initial with ◌้ → falling tone."),
		[ToneRules.TitleId("mid.mai_tri")] = ("กลาง · ◌๊", "Mid · ◌๊"),
		[ToneRules.DetailId("mid.mai_tri")] = ("อักษรกลางกับไม้ตรี → เสียงตรี", "Mid-class initial with ◌๊ → high tone."),
		[ToneRules.TitleId("mid.mai_chattawa")] = ("กลาง · ◌๋", "Mid · ◌๋"),
		[ToneRules.DetailId("mid.mai_chattawa")] = ("อักษรกลางกับไม้จัตวา → เสียงจัตวา", "Mid-class initial with ◌๋ → rising tone."),

		[ToneRules.TitleId("high.live")] = ("สูง · คำเป็น · ไม่มีรูป", "High · live · unmarked"),
		[ToneRules.DetailId("high.live")] = ("พยางค์คำเป็น อักษรสูง ไม่มีรูป → เสียงจัตวา", "Live syllable, high-class initial, no tone mark → rising tone."),
		[ToneRules.TitleId("high.dead")] = ("สูง · คำตาย · ไม่มีรูป", "High · dead · unmarked"),
		[ToneRules.DetailId("high.dead")] = ("พยางค์คำตาย (ยาวหรือสั้น) อักษรสูง ไม่มีรูป → เสียงเอก", "Dead syllable (long or short), high-class initial, no tone mark → low tone."),
		[ToneRules.TitleId("high.mai_ek")] = ("สูง · ◌่", "High · ◌่"),
		[ToneRules.DetailId("high.mai_ek")] = ("อักษรสูงกับไม้เอก → เสียงเอก", "High-class initial with ◌่ → low tone."),
		[ToneRules.TitleId("high.mai_tho")] = ("สูง · ◌้", "High · ◌้"),
		[ToneRules.DetailId("high.mai_tho")] = ("อักษรสูงกับไม้โท → เสียงโท", "High-class initial with ◌้ → falling tone."),
	};

	public static string Get(string id, UiLanguage language)
	{
		if (!Map.TryGetValue(id, out var pair))
			return id;
		return language == UiLanguage.Thai ? pair.Th : pair.En;
	}

	public static string ForRoute(string? route, UiLanguage language)
	{
		if (string.IsNullOrWhiteSpace(route))
			return string.Empty;
		if (!RouteToId.TryGetValue(route.Trim(), out var id))
			return route;
		return Get(id, language);
	}

	/// <summary>
	/// Strip caption: Thai from data when available and language is Thai; otherwise catalog <paramref name="labelId"/>.
	/// </summary>
	public static string ForStrip(string labelId, UiLanguage language, string? thaiFromData = null)
	{
		if (language == UiLanguage.Thai && !string.IsNullOrWhiteSpace(thaiFromData))
			return thaiFromData!;
		return Get(labelId, language);
	}
}
