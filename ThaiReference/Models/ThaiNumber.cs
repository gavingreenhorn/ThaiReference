using ThaiReference.Contracts;
using ThaiReference.Models.BaseModels;
using ThaiReference.Models.Interfaces;

namespace ThaiReference.Models
{
    public class ThaiNumber(int arabic, INumeral[] numerals) : IHaveDescription, IAudible
    {
        readonly int _arabic = arabic;
        readonly INumeral[] _numerals = numerals;

        static class Units
        {
            internal static Numeral Sip
            {
                get
                {
                    return new(
                    "",
                    new Word(
                            "สิบ",
                            new Transcription("sip", "sìp"),
                            ["ten"]
                        )
                    );
                }
            }

            internal static Numeral Roi
            {
                get
                {
                    return new(
                        "",
                        new Word(
                            "ร้อย",
                            new Transcription("raawy", "rɔ́ɔi"),
                            ["hundred"]
                        )
                    );
                }
            }

            internal static Numeral Phan
            {
                get
                {
                    return new(
                        "",
                        new Word(
                            "พัน",
                            new Transcription("phan", "pan"),
                            ["thousand"]
                        ) 
                    );
                }
            }

            internal static Numeral Meuun
            {
                get
                {
                    return new(
                        "",
                        new Word(
                            "หมื่น",
                            new Transcription("meuun","mʉ̀ʉn"),
                            ["ten-thousand"]
                        )
                    );
                }
            }

            internal static Numeral Saaen
            {
                get
                {
                    return new(
                        "",
                        new Word(
                            "แสน",
                            new Transcription("saaen", "sɛ̌ɛn"),
                            ["hundred-thousand"]
                        )
                    );
                }
            }

            internal static Numeral Laan
            {
                get
                {
                    return new(
                        "",
                        new Word(
                            "ล้าน",
                            new Transcription("laan", "láan"),
                            ["million"]
                        )
                    );
                }
            }

            internal static Dictionary<int, Numeral> RadixToUnit
            {
                get {
                    return new Dictionary<int, Numeral>()
                    {
                        {1, Sip},
                        {2, Roi},
                        {3, Phan},
                        {4, Meuun},
                        {5, Saaen},
                        {6, Laan}
                    };
                }
            }
        }

        static class Irregulars
        {
            internal static Numeral UnvoicedSuun = new(
                "๐",
                new Word(
                    "",
                    new Transcription("",""),
                    new List<string> { "" }
                )
            );

            internal static Numeral NuengSip = new(
                "๑",
                new Word(
                    "สิบ",
                    new Transcription("sip", "sìp"),
                    new List<string> { "ten" }
                )            
            );

            internal static Numeral Et = new(
                "๑",
                new Word(
                    "เอ็ด",
                    new Transcription("et", "èt"),
                    new List<string> { "one" }
                )
            );

            internal static Numeral Yii = new(
                "๒",
                new Word(
                    "ยี่",
                    new Transcription("yee","yîi"),
                    new List<string> { "two" }
                )            
            );
        }

        public List<INumeral> Numerals        
        {
            get
            {
                int n_of_digits = Utils.NumberOfDigits(_arabic);
                List<INumeral> matched_numerals = new();
                if (n_of_digits > 1)
                {
                    char[] digits = _arabic.ToString().ToCharArray();
                    for (int i = 0; i < digits.Length; i++)
                    {
                        if (digits[i] == '0')
                        {
                            matched_numerals.Add(Irregulars.UnvoicedSuun);
                        }
                        else if (digits[i] == '1' && i == digits.Length - 1)
                        {
                            matched_numerals.Add(Irregulars.Et);
                        }
                        else if (digits[i] == '1' && i == digits.Length - 2)
                        {
                            var nueng_unit = Units.Sip;
                            nueng_unit.Grapheme = "๑";
                            matched_numerals.Add(nueng_unit);
                        }
                        else if (digits[i] == '2' && i == digits.Length-2)
                        {
                            matched_numerals.Add(Irregulars.Yii);
                        }
                        else
                        {
                            int int_digit = int.Parse(digits[i].ToString());
                            matched_numerals.Add(_numerals[int_digit]);
                        }
                        if (i < digits.Length - 1)
                        {
                            int offset = digits.Length - i - 1;
                            if (!(digits[i] == '1' && offset == 1) && !(digits[i] == '0'))
                            {
                                matched_numerals.Add(Units.RadixToUnit.GetValueOrDefault(offset));
                            }
                        }
                    }
                }
                else { matched_numerals.Add(_numerals[_arabic]); };
                return matched_numerals;
            }
        }

        public string Name => string.Join(' ', Numerals.Select(x => x.Name.Value));

        public string Paiboon => string.Join(' ', Numerals.Select(x => x.Name.Transcription.Paiboon));

        public string IPA => string.Join(' ', Numerals.Select(x => x.Name.Transcription.IPA));

        public string Content => Numerals.Count == 1 ? Numerals[0].Grapheme : string.Join(string.Empty, Numerals.Select(x => x.Grapheme));
        public string Tooltip => Numerals.Count == 1 ? Numerals[0].Name.Value : string.Join(' ', Numerals.Select(x => x.Name.Value));

        public IEnumerable<string> TrackURLs {
            get {
                int count = Numerals.Count;
                if (count == 0)
                {
                    yield break;
                }
                if (count == 1)
                {
                    if (!string.IsNullOrEmpty(Numerals[0].Name.Audible))
                    {
                        yield return Numerals[0].Name.Audible;
                    }
                }
                else
                {
                    List<string> playable_entries = new();
                    var num_reversed = Numerals.AsEnumerable().Reverse().ToList();
                    for (int i = 0;  i < count; i++)
                    {
                        var current_numeral = num_reversed[i];
                        string path = current_numeral.Name.Audible;
                        if (!string.IsNullOrEmpty(path))
                        {
                            playable_entries.Add(path);
                        }
                    }
                    foreach (var entry in playable_entries.AsEnumerable().Reverse())
                    {
                        yield return entry;
                    }
                }
            }
        }
        public IEnumerable<string> AudibleChain
        {
            get
            {
                int count = Numerals.Count;
                if (count == 0)
                {
                    yield break;
                }
                if (count == 1)
                {
                    if (!string.IsNullOrEmpty(Numerals[0].Name.Value))
                    {
                        yield return Numerals[0].Name.Value;
                    }
                }
                else
                {
                    List<string> playable_entries = new();
                    var num_reversed = Numerals.AsEnumerable().Reverse().ToList();
                    for (int i = 0; i < count; i++)
                    {
                        var current_numeral = num_reversed[i];
                        string path = current_numeral.Name.Value;
                        if (!string.IsNullOrEmpty(path))
                        {
                            playable_entries.Add(path);
                        }
                    }
                    foreach (var entry in playable_entries.AsEnumerable().Reverse())
                    {
                        yield return entry;
                    }
                }
            }
        }

        public string Audible => string.Join(" ", AudibleChain);
    }
}
