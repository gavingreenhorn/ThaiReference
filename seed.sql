-- ============================================================
-- Thai Minimal Sets
-- ============================================================

-- DDL

CREATE TABLE IF NOT EXISTS "thai"."MinimalSet" (
  id    INTEGER PRIMARY KEY,
  label TEXT NULL
);

CREATE TABLE IF NOT EXISTS "thai"."MinimalSetWord" (
  id      INTEGER PRIMARY KEY,
  set_id  INTEGER NOT NULL REFERENCES "thai"."MinimalSet"(id),
  word_id INTEGER NOT NULL REFERENCES "thai"."Word"(id)
);

-- ============================================================
-- ALREADY IN DATABASE (do not re-insert; seed reuses IDs)
--   Spellings (by value): ขาว=34, เขา=296, มา=182, ม้า=192,
--     หมา=253, นะ=115, เที่ยง=316, ข้า=35, ฝา=150, ล่าง=202,
--     คน=38, นี่=120, ส้อม=402
--   Lexeme: white=144, grandmother/noun=568, fork=556; lexemes 580-636
--   MinimalSet: id=1 (empty set, no MinimalSetWord rows yet)
--   Transcriptions: none of the seed tone variants exist yet
--   Words: all 45 seed words are new (new spelling+transcription pairs)
-- ============================================================
-- Continuing from existing max IDs:
--   Transcription 513  Spelling 418  Word 434
--   Lexeme 578  WordLexeme 639
--   MinimalSet 1  MinimalSetWord 0
-- ============================================================
-- Lexeme.value = English meaning
-- Lexeme.tags  = dialect/register metadata only, empty string if none
-- ============================================================

-- ============================================================
-- SET 1 · /kʰaːw/ + /kʰaw/ — long-vowel tones + short-vowel tones
-- ============================================================
INSERT INTO "thai"."MinimalSet" (id, label) VALUES (1, NULL)
ON CONFLICT (id) DO NOTHING;

INSERT INTO "thai"."Transcription" (id, phonemic, paiboon, "IPA") VALUES
  (514, 'khaao', 'khǎao', 'kʰǎːw'),
  (515, 'khaao', 'khàao', 'kʰàːw'),
  (516, 'khaao', 'khâao', 'kʰâːw'),
  (517, 'khao', 'khāo', 'kʰāw'),
  (518, 'khao', 'khâo', 'kʰâw');

INSERT INTO "thai"."Spelling" (id, value) VALUES
  (420, 'ข่าว'),
  (421, 'ข้าว'),
  (423, 'เข้า')
ON CONFLICT (id) DO NOTHING;

INSERT INTO "thai"."Word" (id, transcription, spelling_id, is_primary) VALUES
  (435, 514, 34, TRUE),
  (436, 515, 420, TRUE),
  (437, 516, 421, TRUE),
  (438, 517, 296, TRUE),
  (439, 518, 423, TRUE);

INSERT INTO "thai"."WordLexeme" (id, word, lexeme) VALUES
  (640, 435, 144),
  (641, 436, 580),
  (642, 437, 581),
  (643, 437, 582),
  (644, 438, 583),
  (645, 438, 584),
  (646, 439, 585);

INSERT INTO "thai"."MinimalSetWord" (id, set_id, word_id) VALUES
  (1, 1, 435),
  (2, 1, 436),
  (3, 1, 437),
  (4, 1, 438),
  (5, 1, 439);

-- ============================================================
-- SET 2 · /miː/ — tone only: mid (มี) vs rising (หมี)
--   Reuses existing words 61 (มี, have) and 62 (หมี, bear)
-- ============================================================
INSERT INTO "thai"."MinimalSet" (id, label) VALUES (2, NULL)
ON CONFLICT (id) DO NOTHING;

INSERT INTO "thai"."MinimalSetWord" (id, set_id, word_id) VALUES
  (46, 2, 61),
  (47, 2, 62);

-- ============================================================
-- SET 3 · /maː/ — full tone ladder: mid, falling, high, rising
-- ============================================================
INSERT INTO "thai"."MinimalSet" (id, label) VALUES (3, NULL)
ON CONFLICT (id) DO NOTHING;

INSERT INTO "thai"."Transcription" (id, phonemic, paiboon, "IPA") VALUES
  (519, 'maa', 'māa', 'māː'),
  (520, 'maa', 'màa', 'mâː'),
  (521, 'maa', 'máa', 'máː'),
  (522, 'maa', 'mǎa', 'mǎː');

INSERT INTO "thai"."Spelling" (id, value) VALUES
  (425, 'ม่า')
ON CONFLICT (id) DO NOTHING;

INSERT INTO "thai"."Word" (id, transcription, spelling_id, is_primary) VALUES
  (440, 519, 182, TRUE),
  (441, 520, 425, TRUE),
  (442, 521, 192, TRUE),
  (443, 522, 253, TRUE);


INSERT INTO "thai"."Lexeme" (id, value, part_of_speech, tags) VALUES
  (586, 'to come', 'verb', ''),
  (588, 'horse', 'noun', ''),
  (589, 'dog', 'noun', '')
ON CONFLICT (id) DO NOTHING;


INSERT INTO "thai"."WordLexeme" (id, word, lexeme) VALUES
  (647, 440, 586),
  (648, 441, 568),
  (649, 442, 588),
  (650, 443, 589);

INSERT INTO "thai"."MinimalSetWord" (id, set_id, word_id) VALUES
  (6, 3, 440),
  (7, 3, 441),
  (8, 3, 442),
  (9, 3, 443);

-- ============================================================
-- SET 4 · /naː/ + /na/ — tone + vowel length contrast
-- ============================================================
INSERT INTO "thai"."MinimalSet" (id, label) VALUES (4, NULL)
ON CONFLICT (id) DO NOTHING;

INSERT INTO "thai"."Transcription" (id, phonemic, paiboon, "IPA") VALUES
  (523, 'naa', 'nāa', 'nāː'),
  (524, 'naa', 'nàa', 'nàː'),
  (525, 'naa', 'nâa', 'nâː'),
  (526, 'na', 'nā', 'nā');

INSERT INTO "thai"."Spelling" (id, value) VALUES
  (428, 'นา'),
  (429, 'น่า'),
  (430, 'หน้า')
ON CONFLICT (id) DO NOTHING;

INSERT INTO "thai"."Word" (id, transcription, spelling_id, is_primary) VALUES
  (444, 523, 428, TRUE),
  (445, 524, 429, TRUE),
  (446, 525, 430, TRUE),
  (447, 526, 115, TRUE);


INSERT INTO "thai"."WordLexeme" (id, word, lexeme) VALUES
  (651, 444, 590),
  (652, 445, 591),
  (653, 446, 592),
  (654, 446, 593),
  (655, 447, 594);

INSERT INTO "thai"."MinimalSetWord" (id, set_id, word_id) VALUES
  (10, 4, 444),
  (11, 4, 445),
  (12, 4, 446),
  (13, 4, 447);

-- ============================================================
-- SET 5 · /tʰɔːŋ/ — tone contrast: mid, low, high
-- ============================================================
INSERT INTO "thai"."MinimalSet" (id, label) VALUES (5, NULL)
ON CONFLICT (id) DO NOTHING;

INSERT INTO "thai"."Transcription" (id, phonemic, paiboon, "IPA") VALUES
  (527, 'thoong', 'thōong', 'tʰɔ̄ːŋ'),
  (528, 'thoong', 'thòong', 'tʰɔ̀ːŋ'),
  (529, 'thoong', 'thóong', 'tʰɔ́ːŋ');

INSERT INTO "thai"."Spelling" (id, value) VALUES
  (432, 'ทอง'),
  (433, 'ท่อง'),
  (434, 'ท้อง')
ON CONFLICT (id) DO NOTHING;

INSERT INTO "thai"."Word" (id, transcription, spelling_id, is_primary) VALUES
  (448, 527, 432, TRUE),
  (449, 528, 433, TRUE),
  (450, 529, 434, TRUE);


INSERT INTO "thai"."WordLexeme" (id, word, lexeme) VALUES
  (656, 448, 595),
  (657, 449, 596),
  (658, 449, 597),
  (659, 450, 598),
  (660, 450, 599);

INSERT INTO "thai"."MinimalSetWord" (id, set_id, word_id) VALUES
  (14, 5, 448),
  (15, 5, 449),
  (16, 5, 450);

-- ============================================================
-- SET 6 · /tɔːŋ/ — tone contrast: mid, falling
-- ============================================================
INSERT INTO "thai"."MinimalSet" (id, label) VALUES (6, NULL)
ON CONFLICT (id) DO NOTHING;

INSERT INTO "thai"."Transcription" (id, phonemic, paiboon, "IPA") VALUES
  (530, 'dtoong', 'dtōong', 'tɔ̄ːŋ'),
  (531, 'dtoong', 'dtông', 'tɔ̂ːŋ');

INSERT INTO "thai"."Spelling" (id, value) VALUES
  (435, 'ตอง'),
  (436, 'ต้อง')
ON CONFLICT (id) DO NOTHING;

INSERT INTO "thai"."Word" (id, transcription, spelling_id, is_primary) VALUES
  (451, 530, 435, TRUE),
  (452, 531, 436, TRUE);


INSERT INTO "thai"."WordLexeme" (id, word, lexeme) VALUES
  (661, 451, 600),
  (662, 452, 601);

INSERT INTO "thai"."MinimalSetWord" (id, set_id, word_id) VALUES
  (17, 6, 451),
  (18, 6, 452);

-- ============================================================
-- SET 7 · /iaŋ/ — initial class + tone contrast: mid, low
-- ============================================================
INSERT INTO "thai"."MinimalSet" (id, label) VALUES (7, NULL)
ON CONFLICT (id) DO NOTHING;

INSERT INTO "thai"."Transcription" (id, phonemic, paiboon, "IPA") VALUES
  (532, 'dtiang', 'dtiāng', 'tiāŋ'),
  (533, 'dtiang', 'thîang', 'tʰiàŋ');

INSERT INTO "thai"."Spelling" (id, value) VALUES
  (437, 'เตียง')
ON CONFLICT (id) DO NOTHING;

INSERT INTO "thai"."Word" (id, transcription, spelling_id, is_primary) VALUES
  (453, 532, 437, TRUE),
  (454, 533, 316, TRUE);


INSERT INTO "thai"."WordLexeme" (id, word, lexeme) VALUES
  (663, 453, 602),
  (664, 454, 603),
  (665, 454, 604);

INSERT INTO "thai"."MinimalSetWord" (id, set_id, word_id) VALUES
  (19, 7, 453),
  (20, 7, 454);

-- ============================================================
-- SET 8 · /uaŋ/ — tone contrast across initials
-- ============================================================
INSERT INTO "thai"."MinimalSet" (id, label) VALUES (8, NULL)
ON CONFLICT (id) DO NOTHING;

INSERT INTO "thai"."Transcription" (id, phonemic, paiboon, "IPA") VALUES
  (534, 'chuang', 'chûang', 'tɕʰuàŋ'),
  (535, 'huang', 'hùang', 'huàŋ'),
  (536, 'huang', 'hǔang', 'huǎŋ');

INSERT INTO "thai"."Spelling" (id, value) VALUES
  (439, 'ช่วง'),
  (440, 'ห่วง'),
  (441, 'หวง')
ON CONFLICT (id) DO NOTHING;

INSERT INTO "thai"."Word" (id, transcription, spelling_id, is_primary) VALUES
  (455, 534, 439, TRUE),
  (456, 535, 440, TRUE),
  (457, 536, 441, TRUE);


INSERT INTO "thai"."WordLexeme" (id, word, lexeme) VALUES
  (666, 455, 605),
  (667, 456, 606),
  (668, 457, 607);

INSERT INTO "thai"."MinimalSetWord" (id, set_id, word_id) VALUES
  (21, 8, 455),
  (22, 8, 456),
  (23, 8, 457);

-- ============================================================
-- SET 9 · /leːk/ vs /lek/ — vowel length + tone contrast
-- ============================================================
INSERT INTO "thai"."MinimalSet" (id, label) VALUES (9, NULL)
ON CONFLICT (id) DO NOTHING;

INSERT INTO "thai"."Transcription" (id, phonemic, paiboon, "IPA") VALUES
  (537, 'leek', 'lêek', 'lêːk'),
  (538, 'lek', 'lék', 'lék'),
  (539, 'lek', 'lèk', 'lèk');

INSERT INTO "thai"."Spelling" (id, value) VALUES
  (442, 'เลข'),
  (443, 'เล็ก'),
  (444, 'เหล็ก')
ON CONFLICT (id) DO NOTHING;

INSERT INTO "thai"."Word" (id, transcription, spelling_id, is_primary) VALUES
  (458, 537, 442, TRUE),
  (459, 538, 443, TRUE),
  (460, 539, 444, TRUE);


INSERT INTO "thai"."WordLexeme" (id, word, lexeme) VALUES
  (669, 458, 608),
  (670, 459, 609),
  (671, 460, 610);

INSERT INTO "thai"."MinimalSetWord" (id, set_id, word_id) VALUES
  (24, 9, 458),
  (25, 9, 459),
  (26, 9, 460);

-- ============================================================
-- SET 10 · /paː/ — tone contrast: mid, low, falling
-- ============================================================
INSERT INTO "thai"."MinimalSet" (id, label) VALUES (10, NULL)
ON CONFLICT (id) DO NOTHING;

INSERT INTO "thai"."Transcription" (id, phonemic, paiboon, "IPA") VALUES
  (540, 'bpaa', 'bpāa', 'pāː'),
  (541, 'bpaa', 'bpàa', 'pàː'),
  (542, 'bpaa', 'bpâa', 'pâː');

INSERT INTO "thai"."Spelling" (id, value) VALUES
  (445, 'ปา'),
  (446, 'ป่า'),
  (447, 'ป้า')
ON CONFLICT (id) DO NOTHING;

INSERT INTO "thai"."Word" (id, transcription, spelling_id, is_primary) VALUES
  (461, 540, 445, TRUE),
  (462, 541, 446, TRUE),
  (463, 542, 447, TRUE);


INSERT INTO "thai"."WordLexeme" (id, word, lexeme) VALUES
  (672, 461, 611),
  (673, 462, 612),
  (674, 463, 613),
  (675, 463, 614);

INSERT INTO "thai"."MinimalSetWord" (id, set_id, word_id) VALUES
  (27, 10, 461),
  (28, 10, 462),
  (29, 10, 463);

-- ============================================================
-- SET 11 · /kʰaː/ — tone contrast: rising, low, falling
-- ============================================================
INSERT INTO "thai"."MinimalSet" (id, label) VALUES (11, NULL)
ON CONFLICT (id) DO NOTHING;

INSERT INTO "thai"."Transcription" (id, phonemic, paiboon, "IPA") VALUES
  (543, 'khaa', 'khǎa', 'kʰǎː'),
  (544, 'khaa', 'khàa', 'kʰàː'),
  (545, 'khaa', 'khâa', 'kʰâː');

INSERT INTO "thai"."Spelling" (id, value) VALUES
  (448, 'ขา'),
  (449, 'ข่า')
ON CONFLICT (id) DO NOTHING;

INSERT INTO "thai"."Word" (id, transcription, spelling_id, is_primary) VALUES
  (464, 543, 448, TRUE),
  (465, 544, 449, TRUE),
  (466, 545, 35, TRUE);


INSERT INTO "thai"."WordLexeme" (id, word, lexeme) VALUES
  (676, 464, 615),
  (677, 464, 616),
  (678, 465, 617),
  (679, 466, 618);

INSERT INTO "thai"."MinimalSetWord" (id, set_id, word_id) VALUES
  (30, 11, 464),
  (31, 11, 465),
  (32, 11, 466);

-- ============================================================
-- SET 12 · /faː/ — tone contrast: high, rising, low
-- ============================================================
INSERT INTO "thai"."MinimalSet" (id, label) VALUES (12, NULL)
ON CONFLICT (id) DO NOTHING;

INSERT INTO "thai"."Transcription" (id, phonemic, paiboon, "IPA") VALUES
  (546, 'faa', 'fáa', 'fáː'),
  (547, 'faa', 'fǎa', 'fǎː'),
  (548, 'faa', 'fàa', 'fàː');

INSERT INTO "thai"."Spelling" (id, value) VALUES
  (451, 'ฟ้า'),
  (453, 'ฝ่า')
ON CONFLICT (id) DO NOTHING;

INSERT INTO "thai"."Word" (id, transcription, spelling_id, is_primary) VALUES
  (467, 546, 451, TRUE),
  (468, 547, 150, TRUE),
  (469, 548, 453, TRUE);


INSERT INTO "thai"."WordLexeme" (id, word, lexeme) VALUES
  (680, 467, 619),
  (681, 467, 620),
  (682, 468, 621),
  (683, 468, 622),
  (684, 469, 623),
  (685, 469, 624);

INSERT INTO "thai"."MinimalSetWord" (id, set_id, word_id) VALUES
  (33, 12, 467),
  (34, 12, 468),
  (35, 12, 469);

-- ============================================================
-- SET 13 · /laːŋ/ — tone contrast: mid, low, high, rising
-- ============================================================
INSERT INTO "thai"."MinimalSet" (id, label) VALUES (13, NULL)
ON CONFLICT (id) DO NOTHING;

INSERT INTO "thai"."Transcription" (id, phonemic, paiboon, "IPA") VALUES
  (549, 'laang', 'lāang', 'lāːŋ'),
  (550, 'laang', 'làang', 'làːŋ'),
  (551, 'laang', 'láang', 'láːŋ'),
  (559, 'laang', 'lǎang', 'lǎːŋ');

INSERT INTO "thai"."Spelling" (id, value) VALUES
  (454, 'ลาง'),
  (456, 'ล้าง')
ON CONFLICT (id) DO NOTHING;

INSERT INTO "thai"."Word" (id, transcription, spelling_id, is_primary) VALUES
  (470, 549, 454, TRUE),
  (471, 550, 202, TRUE),
  (472, 551, 456, TRUE),
  (480, 559, 264, TRUE);


INSERT INTO "thai"."WordLexeme" (id, word, lexeme) VALUES
  (686, 470, 625),
  (687, 471, 626),
  (688, 472, 627),
  (696, 480, 389);

INSERT INTO "thai"."MinimalSetWord" (id, set_id, word_id) VALUES
  (36, 13, 470),
  (37, 13, 471),
  (38, 13, 472),
  (48, 13, 480);

-- ============================================================
-- SET 14 · /kʰon/ vs /kʰoːn/ — vowel length contrast, same mid tone
-- ============================================================
INSERT INTO "thai"."MinimalSet" (id, label) VALUES (14, NULL)
ON CONFLICT (id) DO NOTHING;

INSERT INTO "thai"."Transcription" (id, phonemic, paiboon, "IPA") VALUES
  (552, 'khon', 'khōn', 'kʰōn'),
  (553, 'khoon', 'khōon', 'kʰōːn');

INSERT INTO "thai"."Spelling" (id, value) VALUES
  (458, 'โคน')
ON CONFLICT (id) DO NOTHING;

INSERT INTO "thai"."Word" (id, transcription, spelling_id, is_primary) VALUES
  (473, 552, 38, TRUE),
  (474, 553, 458, TRUE);


INSERT INTO "thai"."WordLexeme" (id, word, lexeme) VALUES
  (689, 473, 628),
  (690, 474, 629);

INSERT INTO "thai"."MinimalSetWord" (id, set_id, word_id) VALUES
  (39, 14, 473),
  (40, 14, 474);

-- ============================================================
-- SET 15 · /daː/ — tone contrast: mid, low
-- ============================================================
INSERT INTO "thai"."MinimalSet" (id, label) VALUES (15, NULL)
ON CONFLICT (id) DO NOTHING;

INSERT INTO "thai"."Transcription" (id, phonemic, paiboon, "IPA") VALUES
  (554, 'daa', 'dāa', 'dāː'),
  (555, 'daa', 'dàa', 'dàː');

INSERT INTO "thai"."Spelling" (id, value) VALUES
  (459, 'ดา'),
  (460, 'ด่า')
ON CONFLICT (id) DO NOTHING;

INSERT INTO "thai"."Word" (id, transcription, spelling_id, is_primary) VALUES
  (475, 554, 459, TRUE),
  (476, 555, 460, TRUE);


INSERT INTO "thai"."WordLexeme" (id, word, lexeme) VALUES
  (691, 475, 630),
  (692, 476, 631);

INSERT INTO "thai"."MinimalSetWord" (id, set_id, word_id) VALUES
  (41, 15, 475),
  (42, 15, 476);

-- ============================================================
-- SET 16 · /niː/ vs /ni/ — tone + vowel length contrast
-- ============================================================
INSERT INTO "thai"."MinimalSet" (id, label) VALUES (16, NULL)
ON CONFLICT (id) DO NOTHING;

INSERT INTO "thai"."Transcription" (id, phonemic, paiboon, "IPA") VALUES
  (556, 'nii', 'nîi', 'nîː'),
  (557, 'nii', 'nìi', 'nìː'),
  (558, 'ni', 'nī', 'nī');

INSERT INTO "thai"."Spelling" (id, value) VALUES
  (461, 'นี้'),
  (463, 'นิ')
ON CONFLICT (id) DO NOTHING;

INSERT INTO "thai"."Word" (id, transcription, spelling_id, is_primary) VALUES
  (477, 556, 461, TRUE),
  (478, 557, 120, TRUE),
  (479, 558, 463, TRUE);


INSERT INTO "thai"."WordLexeme" (id, word, lexeme) VALUES
  (693, 477, 632),
  (694, 478, 633),
  (695, 479, 634);

INSERT INTO "thai"."MinimalSetWord" (id, set_id, word_id) VALUES
  (43, 16, 477),
  (44, 16, 478),
  (45, 16, 479);

-- ============================================================
-- FINAL IDs AFTER INSERTS
--   Transcription  : 559
--   Spelling        : 463
--   Word            : 480
--   Lexeme          : 634 (579 white skipped, reuses 144)
--   WordLexeme      : 696
--   MinimalSet      : 16
--   MinimalSetWord  : 48
-- ============================================================