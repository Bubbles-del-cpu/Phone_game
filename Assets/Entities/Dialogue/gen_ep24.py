import uuid, os
from collections import defaultdict

def ng(): return str(uuid.uuid4())

# ── Characters ────────────────────────────────────────────────────────────────
LILY      = "44da74b3d308fdb4e9f9fdc127092247"
IZZY      = "8f6af30f1ed271b4aac60c69e0824cfb"
DAVE      = "14173166ace8e7b418d7ae1d1a0526b0"
LISA      = "c9e8e94be1dbf9b4da1b2fd941c31ea2"
MATH      = "3d9d5c52104213243936a370d1fdfff5"
MATHBABES = "df054aa4384061f44999656197df76d1"
INSURANCE = "0043999410142aa4e9b8a3b0bebb746f"

# ── Events ────────────────────────────────────────────────────────────────────
EVT_TAG_TRUE  = "c48a9ef07aa927545a5cf0142ce79561"
EVT_TAG_FALSE = "9ad0a94b32a698540b5cd03fe51bca91"

# ── Variable names ────────────────────────────────────────────────────────────
VAR_MATH_CONT = "Continue_math"
VAR_DOC_DENY  = "Docter_Medicine_denied"
VAR_LISA_KNOW = "Lisa_know_cuckhold"
VAR_DAVE_LILY = "Dave_Lily_path"
VAR_TAG_ALONG = "mc_dave_lisa_tag_along_ep24"

# ── Images ────────────────────────────────────────────────────────────────────
I_LILY_TAXI      = "114fd06ce740ea946984ab5980822030"
I_LILY_SHOWER    = "9ec8425a28625944095337a4b36051c5"
I_LILY_BED_CROP  = "2d3f8902e29e2e54587a7503b97c4077"
I_LILY_KISS      = "7f1722e3c2ecd4c44bc2240df72e6109"
I_MATH_ASS       = "8d137fa8b1ff5da43a0d2c30f2040cbf"
I_LILY_DRESS_TRD = "60b18b8d341a6e645b3600eba5a698c8"
I_LILY_PRONE     = "bdbe75b3adf339b408a2d8685fa4646a"
I_LILY_IZZY_SLP  = "c893b4662c95904429884ef8727bc7aa"
I_LILY_WINK      = "6361b414731d0d8448bd60b6f9c70373"
I_LISA_SWIMWEAR  = "cb283141cc625ef409bb759d4fed8277"
I_LISA_MIA       = "bb8e19bac7a49cc4ca00683d0c2c3e58"
I_LISA_BACK      = "b280f6c52bea02f45b397924f405bc66"
I_IZZY_BIK1      = "24c3cda0a833ce34e92823f612469c38"
I_IZZY_BLACK_F   = "230f41053378b114b925f5beebd6264b"
I_IZZY_BLACK_B   = "2e9bae6831dae364eac9b5ce02e1afe1"
I_LILY_BIK1      = "b2e1f7c7c3c7af9438ff600d93ba671b"
I_LILY_BIK2      = "369d5589db76c9748a55f5590ddbe27e"
I_LILY_BIK3_IZZ  = "5b24de3d58b9fcf46bc443b6506dd1ed"
I_IZZY_THUMBS    = "8bf56d735f19913488e7c37ccdb74058"
I_LISA_ASS_MORN  = "c939d9fdeb46eec4d9906ebaf72a7268"
I_IZZY_OUT       = "80c5c5a32fe3ac8438d49043bfe09a88"
I_LILY_MIRROR    = "e651e17f687b1f04d8622619cb0c171a"
I_LOBBY          = "35f3b583292d6ea408824480c18e5ef6"
I_LILY_BED_BIK   = "152137e27be84f64b84dd5af44b97cf6"
I_LILY_TOP_SIDE  = "794d862ad488e8742b5fe947401d03cc"
I_LILY_BOT_SIDE  = "8f59369f25472654cb4e08ba15e5b230"
I_GWAGON_DAVE    = "6816935ce66aa4a4faa80203fb5661e1"
I_GWAGON_TRIO    = "ea920624077ebbd4cbd8619a5a92c757"
I_GWAGON_HAND    = "f3aa9ad1b81b0394b8a81850b4f8c05b"

# ── Videos ────────────────────────────────────────────────────────────────────
V_BREAST     = "99f55b7e652d09742b4f16bc312d68ef"
V_BREAST_BNR = "156f613e0f7c8244fb529d302c002bdc"
V_CUM        = "40339b2cb0927644bbc945180bb1bb54"
V_CUM_BNR    = "d9f3856c80c368d418dd6c555e3a30df"

# ── Social Posts ──────────────────────────────────────────────────────────────
P_PRONE_BONE   = "87a7dbfe97bf02f458b2eda830734db3"
P_LISA_GLASSES = "4b3860e2e2e004248acb17c7c6aa51c2"
P_IZZY_DRUNK   = "421f5f856781e5d41bb0da18e66c7dcf"

# ── Collections ───────────────────────────────────────────────────────────────
links    = []
cnodes   = []
dnodes   = []
enodes   = []
ifnodes  = []
stnodes  = []
endnodes = []
port_pgs = {}

_xc = [0]
def _px():
    _xc[0] += 300
    return _xc[0]

def safe(t):
    return (t or "").replace("\n", " ").replace("\r", " ")

# ── Node builders ─────────────────────────────────────────────────────────────

def npc(char, text="", tl="", img=None, vid=None, thumb=None, post=None, gal=0, y=0):
    g = ng()
    x = _px()
    mt = 1 if vid else 0
    img_f  = f"{{fileID: 21300000, guid: {img}, type: 3}}"   if img   else "{fileID: 0}"
    vid_f  = f"{{fileID: 32900000, guid: {vid}, type: 3}}"   if vid   else "{fileID: 0}"
    thm_f  = f"{{fileID: 21300000, guid: {thumb}, type: 3}}" if thumb else "{fileID: 0}"
    pst_f  = f"{{fileID: 11400000, guid: {post}, type: 2}}"  if post  else "{fileID: 0}"
    dnodes.append(
        f"  - NodeGuid: {g}\n"
        f"    Position: {{x: {x}, y: {y}}}\n"
        f"    DialogueNodePorts: []\n"
        f"    AudioClips:\n"
        f"    - languageEnum: 0\n"
        f"      LanguageGenericType: {{fileID: 0}}\n"
        f"    - languageEnum: 1\n"
        f"      LanguageGenericType: {{fileID: 0}}\n"
        f"    Character: {{fileID: 11400000, guid: {char}, type: 2}}\n"
        f"    AvatarPos: 0\n"
        f"    AvatarType: 0\n"
        f"    Texts:\n"
        f"    - languageEnum: 0\n"
        f"      LanguageGenericType: {safe(text)}\n"
        f"    - languageEnum: 1\n"
        f"      LanguageGenericType: \n"
        f"    Timelapses:\n"
        f"    - languageEnum: 0\n"
        f"      LanguageGenericType: {safe(tl)}\n"
        f"    - languageEnum: 1\n"
        f"      LanguageGenericType: \n"
        f"    Timelapse: \n"
        f"    Duration: 2\n"
        f"    Delay: 0\n"
        f"    MediaType: {mt}\n"
        f"    Image: {img_f}\n"
        f"    Video: {vid_f}\n"
        f"    VideoThumbnail: {thm_f}\n"
        f"    NotBackgroundCapable: 0\n"
        f"    GalleryVisibility: {gal}\n"
        f"    Post: {pst_f}\n"
        f"    DelayTimer: 0\n"
    )
    return g

def mc2(char, port_texts, npc_txt="", tl="", y=0):
    g = ng()
    x = _px()
    req = 1 if safe(npc_txt) else 0
    pgs = []
    ports_yaml = ""
    for i, (txt, hint) in enumerate(port_texts):
        pg = ng()
        pgs.append(pg)
        ports_yaml += (
            f"    - PortGuid: {pg}\n"
            f"      InputGuid: __IGUID_{pg}__\n"
            f"      OutputGuid: {g}\n"
            f"      TextLanguage:\n"
            f"      - languageEnum: 0\n"
            f"        LanguageGenericType: {safe(txt)}\n"
            f"      - languageEnum: 1\n"
            f"        LanguageGenericType: Choice {i+1:02d}\n"
            f"      HintLanguage:\n"
            f"      - languageEnum: 0\n"
            f"        LanguageGenericType: {safe(hint)}\n"
            f"      - languageEnum: 1\n"
            f"        LanguageGenericType: \n"
        )
    port_pgs[g] = pgs
    cnodes.append(
        f"  - NodeGuid: {g}\n"
        f"    Position: {{x: {x}, y: {y}}}\n"
        f"    DialogueNodePorts:\n"
        + ports_yaml +
        f"    AudioClips:\n"
        f"    - languageEnum: 0\n"
        f"      LanguageGenericType: {{fileID: 0}}\n"
        f"    - languageEnum: 1\n"
        f"      LanguageGenericType: {{fileID: 0}}\n"
        f"    Character: {{fileID: 11400000, guid: {char}, type: 2}}\n"
        f"    AvatarPos: 0\n"
        f"    AvatarType: 0\n"
        f"    TextType:\n"
        f"    - languageEnum: 0\n"
        f"      LanguageGenericType: {safe(npc_txt)}\n"
        f"    - languageEnum: 1\n"
        f"      LanguageGenericType: \n"
        f"    Duration: 2\n"
        f"    Delay: 0\n"
        f"    Timelapse: {safe(tl)}\n"
        f"    RequireCharacterInput: {req}\n"
        f"    SelectedChoice: []\n"
    )
    return g

def evt(event_guid, y=0):
    g = ng()
    x = _px()
    enodes.append(
        f"  - NodeGuid: {g}\n"
        f"    Position: {{x: {x}, y: {y}}}\n"
        f"    EventScriptableObjects:\n"
        f"    - DialogueEventSO: {{fileID: 11400000, guid: {event_guid}, type: 2}}\n"
    )
    return g

def ifn(var, true_g, false_g, preset_guid=None, y=0):
    g = preset_guid if preset_guid else ng()
    links.append((g, true_g))
    links.append((g, false_g))
    x = _px()
    ifnodes.append(
        f"  - NodeGuid: {g}\n"
        f"    Position: {{x: {x}, y: {y}}}\n"
        f"    ValueName: {var}\n"
        f"    Operations: 0\n"
        f"    OperationValue: \n"
        f"    TrueGUID: {true_g}\n"
        f"    FalseGUID: {false_g}\n"
    )
    return g

def start():
    g = ng()
    x = _px()
    stnodes.append(
        f"  - NodeGuid: {g}\n"
        f"    Position: {{x: {x}, y: 0}}\n"
        f"    startID: \n"
    )
    return g

def end(y=0):
    g = ng()
    x = _px()
    endnodes.append(
        f"  - NodeGuid: {g}\n"
        f"    Position: {{x: {x}, y: {y}}}\n"
        f"    EndNodeType: 0\n"
        f"    Dialogue: {{fileID: 0}}\n"
    )
    return g

def chain(*nodes):
    for a, b in zip(nodes, nodes[1:]):
        links.append((a, b))

# Shorthand helpers
def L(text="", **kw):    return npc(LILY,      text, **kw)
def I_(text="", **kw):   return npc(IZZY,      text, **kw)
def D(text="", **kw):    return npc(DAVE,      text, **kw)
def LS(text="", **kw):   return npc(LISA,      text, **kw)
def MA(text="", **kw):   return npc(MATH,      text, **kw)
def MBB(text="", **kw):  return npc(MATHBABES, text, **kw)
def INS(text="", **kw):  return npc(INSURANCE, text, **kw)

def ML(txt, hint="", tl="", y=0):   return mc2(LILY,      [(txt, hint)], tl=tl, y=y)
def MI_(txt, hint="", tl="", y=0):  return mc2(IZZY,      [(txt, hint)], tl=tl, y=y)
def MD(txt, hint="", tl="", y=0):   return mc2(DAVE,      [(txt, hint)], tl=tl, y=y)
def MLS(txt, hint="", tl="", y=0):  return mc2(LISA,      [(txt, hint)], tl=tl, y=y)
def MMA(txt, hint="", tl="", y=0):  return mc2(MATH,      [(txt, hint)], tl=tl, y=y)

# =============================================================================
# NODE DEFINITIONS
# =============================================================================

g_start = start()

# ── OPENING — Insurance spam (all paths) ─────────────────────────────────────
g_ins_payment = INS("You have a pending payment of 420,89$ due in 2 days")
g_ins_plans   = INS("If you can't pay right now you can look up our payment plans!")
g_ins_day     = INS("Have a pleasant day")

# pre-alloc IF math_continue
g_if_math = ng()

# =============================================================================
# NTS PATH  (math_continue == false)
# =============================================================================
g_l_gm_nts     = L("Good morning baby")
g_l_taxi_back  = L("I'm in the taxi heading back to the hotel")
g_l_slept_well = L("I slept so well")
g_mc_gm        = ML("Good morning")
g_mc_thats_gd  = ML("That's good to hear!")
g_mc_drive_sf  = ML("Drive safe")
g_l_taxi_pic   = L("", img=I_LILY_TAXI, gal=1)
g_l_will_try   = L("I will... uhm... try")
g_l_not_driv   = L("Although I'm not the one driving hihi")
g_l_see_soon   = L("See you soon... well not soon but you know hihi")
g_l_love_nts1  = L("Love you")
g_mc_love_nts1 = ML("Love you too")
g_l_im_back    = L("I'm back!", tl="30 minutes later...")
g_l_izzy_slp   = L("Izzy is still sleeping")
g_l_busy_nite  = L("She had a busy night with Samuel")
g_l_shower_txt = L("I'm going to take a shower and then maybe we can catch up properly")
g_mc_sounds_gd = ML("Sounds good")
g_mc_want_hear = ML("I want to hear everything")
g_l_hihi_nts   = L("hihi")
g_l_heard_all  = L("You already heard everything though")
g_mc_again1    = ML("I want to hear it again!")
g_mc_again2    = ML("And then again!")
g_l_cry_laugh  = L("\U0001f602")
g_l_20_min     = L("Give me 20 minutes")
g_l_shower_pic = L("", img=I_LILY_SHOWER, gal=1, tl="10 minutes later...")
g_mc_so        = ML("So?", tl="20 minutes later...")
g_l_so         = L("So...")
g_l_last_nite  = L("Last night was really something")
g_mc_tell_all  = ML("Tell me everything")
g_l_know_most  = L("Well you already know most of it")
g_l_but_person = L("But being there in person...")
g_l_studio     = L("His studio, the light, the painting... the people")
g_l_special    = L("It felt really special")
g_mc_and_leo   = ML("And Leo?")
g_l_leo_was    = L("Leo was...")
g_l_good_man   = L("He's a good man")
g_l_checking   = L("He kept checking in with me the whole time")
g_l_ok_while   = L("Making sure I was okay")
g_l_dicking    = L("While dicking me down so good of course.. hihi")
g_mc_hmm_love  = ML("Hmm I love the sounds of that")
g_mc_glad_care = ML("I'm glad though... that he is actually taking proper care of you")
g_l_me_too_nts = L("Me too")
g_l_found_one  = L("I think we found a good one")
g_mc_we_did    = ML("We did")
g_l_bed_pic    = L("", img=I_LILY_BED_CROP, gal=1)
g_l_enough_nts = L("Okay enough about last night")
g_l_3_classes  = L("He has like 3 more classes today so he is busy busy hihi")
g_mc_busy_man  = ML("Busy man")
g_l_what_do    = L("What are you doing today?")
g_mc_same_yd   = ML("The same as yesterday")
g_mc_laying    = ML("Laying in bed...")
g_l_no_plans   = L("I have no plans either")
g_l_kinda_nice = L("But that's kinda nice for once")
g_l_relaxing   = L("Just relaxing")
g_l_good_day   = L("I think it's going to be a good day")
g_mc_happy_hr  = ML("Happy to hear that")
g_l_hihi_nts2  = L("hihi")
g_l_text_latr  = L("text later okay")
g_mc_sure_luv  = ML("Sure, love you")
g_l_luv_too    = L("Love you too")
g_l_kiss_pic   = L("", img=I_LILY_KISS, gal=1)

# =============================================================================
# CUCK PATH  (math_continue == true, docter_medicine_denied == false)
# =============================================================================
# pre-alloc IF docter_medicine_denied
g_if_doc = ng()

g_ma_morn_ck   = MA("Morning cuck")
g_ma_dropping  = MA("Dropping your limp wife off now")
g_ma_long_nite = MA("She had a long night")
g_mc_imagine   = MMA("I can imagine")
g_ma_details   = MA("I will leave it up to her to tell the details")
g_ma_not_walk  = MA("But she isn't walking so straight...")
g_ma_ass_pic   = MA("", img=I_MATH_ASS, gal=1)
g_ma_text_bk   = MA("She'll text you when she's back")
g_mc_thnx_math = MMA("Thanks Math")

g_l_hey_ck     = L("Hey baby", tl="Text from Lily...")
g_l_back_htl   = L("I'm back at the hotel")
g_l_dress_pic  = L("", img=I_LILY_DRESS_TRD, gal=1)
g_mc_tired_ck  = ML("You look a little tired")
g_l_math_early = L("Yea, Math woke me up a little earlier than expected")
g_l_no_compln  = L("But, I wasn't complaining and it was his birthday yesterday\u2026 so\u2026.")
g_l_prone_pic  = L("", img=I_LILY_PRONE, gal=1)
g_mc_glad_hpy  = ML("I see, I'm glad you made him happy")
g_l_freshen_ck = L("Going to freshen up")
g_l_talk_soon  = L("Talk soon okay")
g_l_love_ck    = L("Love you")
g_mc_love_ck   = ML("Love you too")

# =============================================================================
# NTR PATH  (math_continue == true, docter_medicine_denied == true)
# =============================================================================
g_mbb_prone_post = MBB("", post=P_PRONE_BONE)

g_l_gm_ntr     = L("Good morning baby")
g_l_sorry_quiet= L("Sorry I went quiet last night")
g_l_too_much   = L("I think I had a bit too much to drink hihi")
g_l_back_safe  = L("We got back safely though")
g_l_casual_pic = L("", img=I_LILY_IZZY_SLP, gal=1)
g_mc_morn_ntr  = ML("Morning")
g_mc_glad_safe = ML("Glad you're back safe")
g_l_sleep_ok   = L("Did you sleep okay?")
g_mc_yea_ntr   = ML("Yea...")
g_mc_worried   = ML("Well, I was a little worried")
g_l_sorry_ntr  = L("hihi, sorry about that")
g_l_freshen_ntr= L("I'm going to freshen up")

g_c_ntr_choice = mc2(LILY, [
    ("Okay",        ""),
    ("Freshen up?", "")
])
# Port 0 — okay, nothing extra
g_l_wink_pic_0 = L("", img=I_LILY_WINK, gal=1, y=0)
# Port 1 — freshen up question
g_l_uh_shower  = L("Uh yea well I mean just morning shower you know", y=600)
g_l_just_woke  = L("I just woke up after all", y=600)
g_mc_yea_ok    = ML("Yea okay of course", y=600)
g_l_wink_pic_1 = L("", img=I_LILY_WINK, gal=1, y=600)

# MERGE NTR
g_l_talk_ntr   = L("Talk later okay")
g_l_love_ntr   = L("Love you")
g_mc_love_ntr  = ML("Love you too")

# =============================================================================
# ALL PATHS — LISA SECTION
# =============================================================================
# pre-alloc IF lisa_know
g_if_lisa_know = ng()

# TRUE — lisa_know
g_ls_gm_cuck   = LS("Good morning cuck", y=0)
# FALSE — not lisa_know
g_ls_gm_norm   = LS("Good morning", y=600)

# MERGE lisa greeting
g_mc_hi_lisa   = MLS("Hi Lisa")
g_ls_tell_you  = LS("I have to tell you about something very exciting...")
g_ls_yesterday = LS("It happened yesterday")
g_mc_okay_ls1  = MLS("Okay...")
g_ls_dave_nap  = LS("While Dave was napping... I went out to do some shopping... alone..")
g_mc_okay_ls2  = MLS("Okay...")
g_ls_browsing  = LS("I was just browsing")
g_ls_minding   = LS("Minding my own business")
g_ls_swimwear  = LS("Maybe finding some sexy swimwear")
g_ls_swim_pic  = LS("", img=I_LISA_SWIMWEAR, gal=1)

g_c_swimwear   = mc2(LISA, [
    ("Sexy indeed", ""),
    ("Wow",         "")
])
# Port 0
g_ls_anyway_0  = LS("Anyway\u2026 (;", y=0)
# Port 1
g_ls_anyway_1  = LS("Anyway\u2026 (;", y=600)

# MERGE swimwear choice — use one set of nodes
g_ls_girl_wlks = LS("Then this girl walks up to me")
g_ls_nowhere   = LS("Out of nowhere, super confident and beautiful")
g_ls_she_says  = LS("And she just says")
g_ls_stunning  = LS("\u201cMy husband thinks you\u2019re stunning\u201d")
g_mc_thats_all = MLS("That's all she said?")
g_ls_pointing  = LS("Well she was also pointing...")
g_ls_look_over = LS("So I look over and there's this man")
g_ls_bentley   = LS("Sitting in a Bentley")
g_ls_looking   = LS("Just... looking me up and down")
g_mc_said_noth = MLS("And he said nothing?")
g_ls_no_car    = LS("Didn't even get out of the car")
g_ls_at_first  = LS("At first I didn't understand\u2026")
g_ls_but_talk  = LS("But then she started talking more to me...")
g_mc_what_want = MLS("What did they want?")
g_ls_open_mind = LS("She said that they were an open minded couple")
g_ls_best_nite = LS("And that they could give me one of the best nights of my life")
g_mc_oh_god    = MLS("Oh god")
g_mc_tables    = MLS("Interesting how the tables turned on you")
g_ls_right     = LS("RIGHT!")
g_ls_normally  = LS("Normally I'm the one scouting people out")
g_ls_scouted   = LS("But I got scouted yesterday")
g_ls_mill_cpl  = LS("By a millionaires couple")
g_mc_how_mill  = MLS("How do you know they are millionaires")
g_ls_talked_bt = LS("Well... we talked for a bit")
g_ls_mia_pic   = LS("", img=I_LISA_MIA, gal=1)
g_mc_gorgeous  = MLS("She's gorgeous")
g_ls_i_know    = LS("I know")
g_ls_gd_taste  = LS("The husband has good taste that's for sure")
g_ls_yacht     = LS("But... they invited me on a yacht\u2026 for tonight")
g_mc_millionrs = MLS("Aha... millionaires indeed")
g_ls_told_ya   = LS("Told you")
g_ls_married   = LS("I told her I was married and that I had a husband")
g_ls_no_mind   = LS("They didn't mind. They told me to bring him")
g_mc_how_crazy = MLS("How crazy that they picked you")
g_ls_guess_why = LS("Can you guess why they picked me specifically")

g_c_guess_why  = mc2(LISA, [
    ("I think I have a pretty good idea", ""),
    ("Tell me",                           "")
])
# Port 0 — pretty good idea
g_ls_do_you    = LS("Do you now", y=0)
g_ls_clearer   = LS("Let me make it clearer for you", y=0)
# Port 1 — tell me
g_ls_show_inst = LS("I'll show you instead", y=600)

# MERGE — Lisa ass pic
g_ls_ass_pic   = LS("", img=I_LISA_BACK, gal=1)
g_ls_questions = LS("Any questions")

g_c_questions  = mc2(LISA, [
    ("None whatsoever",            ""),
    ("The husband has very good taste", "")
])
# Port 0
g_ls_didnt_thk = LS("Didn't think so", y=0)
# Port 1
g_ls_he_really = LS("He really does", y=600)
g_ls_wife_too  = LS("So does his wife", y=600)

# MERGE
g_ls_dave_fell = LS("And Dave almost fell off the bed when I told him")
g_ls_cry_laugh = LS("\U0001f602")
g_mc_can_imag  = MLS("I can imagine")
g_ls_tonight   = LS("So tonight we are going")
g_ls_feel_it   = LS("It's going to be something else I can already feel it")
g_ls_tag_along = LS("So I think I already know the answer but do you want to tag along? Digitally of course?")

g_c_tag_along  = mc2(LISA, [
    ("Duuuhhhh of course",                   ""),
    ("It all sounds amazing but... no...", "")
])

# ── CHOICE 1: tag along TRUE ──────────────────────────────────────────────────
g_evt_tag_true  = evt(EVT_TAG_TRUE, y=0)
g_mc_duuh       = ML("Duuuhhhh of course", y=0)

# pre-alloc IF lisa_know inside tag-along=TRUE
g_if_lisa_in_tag = ng()

# lisa_know TRUE inside tag=TRUE
g_ls_obviously  = LS("Obviously", y=0)
g_ls_one_more   = LS("Oh and one more thing", y=0)
g_mc_q_mark     = MLS("?", y=0)
g_ls_lily_infmd = LS("Lily is already informed", y=0)
g_mc_informed   = MLS("? Informed?", y=0)
g_mc_lisa_q     = MLS("Lisa", y=0)
g_mc_what_tell  = MLS("What did you tell Lily?", y=0)
g_mc_lisa_q2    = MLS("Lisa?", tl="2 minutes later...", y=0)

# lisa_know FALSE inside tag=TRUE
g_ls_well_then  = LS("Well then", y=600)
g_ls_talk_latr  = LS("Talk later", y=600)
g_mc_later_tag  = ML("Later!", y=600)

# ── CHOICE 2: tag along FALSE ─────────────────────────────────────────────────
g_evt_tag_false = evt(EVT_TAG_FALSE, y=1200)
g_mc_no_tag     = ML("It all sounds amazing but... no...", y=1200)
g_ls_dots       = LS("...", y=1200)
g_ls_didnt_exp  = LS("Didn't expect that but... okay I guess", y=1200)
g_ls_well_f     = LS("Well", y=1200)
g_ls_talk_latr2 = LS("Talk later then", y=1200)
g_mc_sorry_latr = ML("Sorry, Later", y=1200)

# =============================================================================
# EXTENDED LILY SECTION  (tag_along==true && lisa_know==true)
# =============================================================================
g_mc_hey_ext   = ML("Hey", tl="1 hour later...")
g_mc_so_ext    = ML("So...")
g_mc_lisa_txd  = ML("Lisa texted you?")
g_l_hey_ext    = L("Hey", tl="5 minutes later...")
g_l_def_might  = L("She definitely might have texted me a little")
g_l_why_ask    = L("Why are you asking... hihi")
g_mc_what_say2 = ML("What did she say?")
g_l_know_say   = L("I think you already know what she said")
g_l_tell_any   = L("But I'll tell you anyway")
g_l_started    = L("She started with...")
g_l_adv_dave   = L("Telling me about her adventures with Dave")
g_mc_just_dave = ML("Just Dave...")
g_l_hihi_adv   = L("hihi")
g_l_other_cpl  = L("And other couples or people")
g_mc_and_ext   = ML("And?")
g_l_not_surp   = L("I mean... it's not that surprising honestly")
g_l_after_all  = L("Not after everything we've been doing hihi")
g_l_still_funn = L("But it's still funny to think about")
g_l_ur_bro     = L("Your brother of all people")
g_mc_i_know_br = ML("I know")
g_l_where_get  = L("Where do you guys get it from?")
g_mc_no_clue   = ML("... no clue")
g_l_hihi_bk    = L("hihi, Anyway")
g_l_spk_adv    = L("Speaking of adventures")
g_l_izzy_date  = L("Izzy also has ANOTHER date tonight")
g_mc_another   = ML("Another one?")
g_l_another    = L("Another one")
g_l_dont_know  = L("I don't even know when this girl sleeps")
g_l_milking    = L("She is absolutely milking this free vacation for everything it's worth")
g_mc_move_isl  = ML("At this point she should just move to the island")
g_l_right_ext  = L("RIGHT \U0001f602")
g_l_back_lisa  = L("Okay but back to Lisa")
g_l_after_dave = L("So after she told me about her and Dave")
g_l_mentioned  = L("She mentioned tonight")
g_l_the_yacht  = L("The yacht")
g_l_rich_cpl   = L("This super rich couple")
g_mc_what_thk  = ML("What did you think?")
g_l_she_invt   = L("Well she invited me")
g_mc_invited   = ML("She invited you!?")
g_l_just_come  = L("Just to come along")
g_l_not_per_se = L("Not to do anything per se")
g_l_just_be    = L("Just be there")
g_l_see_nite   = L("See what the night brings")
g_l_honestly   = L("And honestly...")
g_mc_honestly2 = ML("Honestly?")
g_l_excited    = L("I'm kind of excited about it")
g_l_weird_q    = L("Is that weird?")
g_mc_not_at_al = ML("Not at all")
g_l_u_excitd   = L("You're excited too aren't you")
g_mc_obviously = ML("Obviously!")
g_l_thought_so = L("I thought so")
g_l_just_see   = L("So we just... see what happens on the boat?")
g_mc_perfect   = ML("I think that's perfect")
g_l_yeah_perf  = L("Yeah")
g_l_me_too_ext = L("Me too")

# pre-alloc IF dave_lily inside extended section
g_if_dave_lily_ext = ng()

# dave_lily FALSE — simple ending
g_l_drag_izzy_f = L("Okay so I'm going to drag Izzy bikini shopping", y=0)
g_l_before_date = L("Before she disappears for her date", y=0)
g_l_new_tonite  = L("I need something new for tonight", y=0)
g_mc_good_idea  = ML("Good idea", y=0)
g_l_send_pics   = L("I'll send you pics", y=0)
g_mc_waiting    = ML("I'll be waiting", y=0)
g_l_love_ext_f  = L("Love you", y=0)
g_mc_love_ext_f = ML("Love you too", y=0)

# dave_lily TRUE — extended conversation then same ending
g_l_ok_so_dl   = L("Okay so...", y=600)
g_l_one_more_dl= L("There was one more thing Lisa mentioned", y=600)
g_mc_oh_dl     = ML("Oh", y=600)
g_l_yeah_dl    = L("Yeah...", y=600)
g_l_specific   = L("She mentioned something specific about Dave", y=600)
g_mc_lily_i    = ML("Lily... I", y=600)
g_l_thought_it = L("Is that something you actually thought about\u2026 me and Dave?", y=600)
g_mc_honestly3 = ML("Honestly?", y=600)
g_l_thats_all  = L("That's all I want...", y=600)
g_mc_crossed   = ML("The thought crossed my mind", y=600)
g_mc_didnt_pln = ML("I didn't plan it", y=600)
g_mc_lisa_askd = ML("Lisa just asked me if it was something I thought about...", y=600)
g_mc_but_yes   = ML("But yes", y=600)
g_mc_crossed2  = ML("It crossed my mind", y=600)
g_l_hes_bro    = L("He's your brother", y=600)
g_mc_i_know_dl = ML("I know", y=600)
g_l_different  = L("That's so different from everything else...", y=600)
g_mc_know_it   = ML("I know it is", y=600)
g_l_dave_know  = L("Does Dave even know?", y=600)
g_mc_no_dl     = ML("No", y=600)
g_mc_lisa_own  = ML("Lisa brought it up on her own\u2026 I don't know if she told Dave actually", y=600)
g_l_okay_dl    = L("Okay", y=600)
g_l_dont_feel  = L("I don't know how I feel about it", y=600)
g_l_not_noth   = L("It's not nothing", y=600)
g_mc_know_dl2  = ML("I know", y=600)
g_mc_not_push  = ML("And I'm not pushing it", y=600)
g_mc_not_close = ML("Not even close", y=600)
g_l_wouldnt_st = L("But you wouldn't stop it either would you?", y=600)
g_mc_dots_dl   = ML("...", y=600)
g_mc_dont_know = ML("I don't know", y=600)
g_l_honest_dl  = L("Okay... that's honest at least", y=600)
g_l_can_we     = L("Can we just", y=600)
g_l_see_tonite = L("See what happens tonight", y=600)
g_l_no_plan    = L("Without that being a planned thing...", y=600)
g_mc_of_course = ML("Of course", y=600)
g_mc_no_press  = ML("Absolutely no pressure", y=600)
g_l_need_dist  = L("Okay I think I need a distraction now", y=600)
g_l_drag_izzy_t= L("I'm going to drag Izzy bikini shopping", y=600)
g_l_bef_date_t = L("Before she disappears for her date", y=600)
g_l_new_tonite2= L("I need something new for tonight", y=600)
g_mc_good_idea2= ML("Good idea", y=600)
g_l_send_pics2 = L("I'll send you pics", y=600)
g_mc_waiting2  = ML("I'll be waiting", y=600)
g_l_love_ext_t = L("Love you", y=600)
g_mc_love_ext_t= ML("Love you too", y=600)

# =============================================================================
# SIMPLE LILY BIKINI SECTION  (tag_along==false OR lisa_know==false)
# =============================================================================
g_l_hey_simple = L("Hey baby")
g_l_bikini_shp = L("Me and Izzy are going bikini shopping")
g_mc_hey_smpl  = ML("Hey")
g_mc_occasion  = ML("Okay... is there a special occasion?")
g_l_not_really = L("Not really")
g_l_izzy_date2 = L("Izzy is going on a date again tonight")
g_mc_again_s   = ML("Again?")
g_l_again_s    = L("Again")
g_l_dont_know2 = L("I don't even know when this girl sleeps")
g_l_milking2   = L("She is absolutely milking this free vacation for everything it's worth")
g_mc_move_isl2 = ML("At this point she should just move to the island")
g_l_right_s    = L("RIGHT \U0001f602")
g_l_anyway_s   = L("Anyway. Izzy wants a new bikini for her date")
g_l_tag_alng   = L("I'll just tag along")
g_mc_have_fun  = ML("Okay, have fun")
g_l_thanks_luv = L("Thanks baby love you")
g_mc_luv_too_s = ML("Love you too")

# =============================================================================
# DAVE SECTION  (tag_along==true && lisa_know==true)
# =============================================================================
# pre-alloc IF dave_lily for Dave texts
g_if_dave_lily_dave = ng()

# dave_lily FALSE — short Dave text
g_d_yo_f       = D("Yo dude", y=0)
g_d_filled_in_f= D("I heard Lisa already filled you in on the new adventure...", y=0)
g_mc_she_did_f = MD("She did", y=0)
g_mc_sounds_cz = MD("Sounds crazy dude", y=0)
g_d_it_does    = D("It does", y=0)
g_d_chick_hot  = D("I almost fell out of the bed when she told me", y=0)
g_d_crazy_hot  = D("That chick is crazy hot... like crazy", y=0)
g_d_first_time = D("And this is like the first time someone approached us... or Lisa in this case", y=0)
g_mc_yea_norm  = MD("Yea I said to Lisa as well, normally it's the other way around", y=0)
g_d_exactly    = D("Exactly", y=0)
g_d_lily_too   = D("And I heard Lily is coming along", y=0)
g_mc_yea_f     = MD("yea", y=0)
g_d_super_sick = D("Super sick dude", y=0)
g_d_even_watch = D("Even if she just watches it super cool she is coming along", y=0)
g_mc_agreed    = MD("Agreed... can't wait to see what happens", y=0)
g_d_check_in   = D("Well just wanted to check in...", y=0)
g_d_talk_latr  = D("Talk later", y=0)
g_mc_cant_wait = MD("Can't wait for later dude", y=0)
g_d_me_too_f   = D("Me too dude", y=0)

# dave_lily TRUE — long Dave text
g_d_yo_t       = D("Yo dude", y=600)
g_d_filled_in_t= D("I heard Lisa already filled you in on the new adventure...", y=600)
g_mc_she_did_t = MD("She did", y=600)
g_d_good_good  = D("Good good", y=600)
g_d_but_uh     = D("But uh...", y=600)
g_d_lisa_told  = D("Lisa also told me something else", y=600)
g_mc_yeah_t    = MD("Yeah, I thought so", y=600)
g_d_its_true   = D("So it's true?", y=600)
g_mc_crossed_d = MD("It crossed my mind yeah", y=600)
g_d_bro        = D("Bro...", y=600)
g_d_are_sure   = D("Are you sure about that?", y=600)
g_mc_what_mean = MD("What do you mean", y=600)
g_d_its_me     = D("I mean...", y=600)
g_d_im_bro     = D("It's me dude", y=600)
g_d_ur_bro     = D("I'm your brother", y=600)
g_d_really_sure= D("Are you really sure you want to go there", y=600)
g_mc_lily_talk = MD("I talked to Lily about it", y=600)
g_d_u_talked   = D("You talked to Lily about it?", y=600)
g_mc_yeah_d    = MD("Yeah", y=600)
g_d_she_ok     = D("And she was okay with it?", y=600)
g_mc_no_no     = MD("She didn't say no", y=600)
g_mc_no_yes    = MD("But she didn't say yes either", y=600)
g_mc_see_nite  = MD("She wants to see how tonight goes", y=600)
g_d_bro2       = D("Bro I just...", y=600)
g_d_no_weird   = D("I don't want you to look back on this and feel weird about it", y=600)
g_d_u_know     = D("You know?", y=600)
g_d_cant_undo  = D("Like we can never undo that", y=600)
g_mc_i_know_d  = MD("I know", y=600)
g_mc_lily_leads= MD("That's why Lily leads", y=600)
g_mc_whatever  = MD("Whatever she is okay with in the moment is what happens", y=600)
g_mc_contact   = MD("She keeps contact with me", y=600)
g_mc_no_push   = MD("Nothing gets pushed", y=600)
g_d_u_sure_t   = D("You sure?", y=600)
g_mc_im_sure_d = MD("I'm sure", y=600)
g_d_okay_d     = D("Okay", y=600)
g_d_aight      = D("Aight", y=600)
g_d_trust_u    = D("I trust you bro", y=600)
g_d_if_wrong   = D("But just know that if at any point tonight it feels wrong", y=600)
g_d_dont_do    = D("We don't do anything", y=600)
g_d_mean_it    = D("I mean that", y=600)
g_mc_i_know_d2 = MD("I know", y=600)
g_mc_apprec    = MD("I appreciate that", y=600)
g_d_aight2     = D("Aight", y=600)
g_d_wild       = D("Wild honeymoon man", y=600)
g_mc_tell_me_d = MD("Tell me about it", y=600)
g_d_haha       = D("Haha", y=600)
g_d_talk_latr2 = D("Talk later bro", y=600)
g_mc_later_d   = MD("Later", y=600)
g_d_owe_you    = D("Kinda feel like I owe you or something...", tl="5 minutes later...", y=600)

g_c_owe        = mc2(DAVE, [
    ("Maybe a little", ""),
    ("Uh why?",        "")
], y=600)
# Port 0 — Maybe a little
g_d_right_d    = D("Right...", y=600)
g_d_here_pic   = D("Here", y=600)
g_d_lisa_ass   = D("", img=I_LISA_ASS_MORN, gal=1, y=600)
g_mc_damn_d    = MD("Damn", y=600)
g_d_more_even  = D("Now I feel like we are a little more even", y=600)
g_d_later_d    = D("Later", y=600)
g_mc_uh_later  = MD("uh Later", y=600)
# Port 1 — Uh why
g_d_idk        = D("I don't know", y=1200)
g_d_nvm        = D("Never mind", y=1200)
g_d_later_d2   = D("Later", y=1200)

# =============================================================================
# ALL PATHS — LISA SOCIAL POST + BIKINI SHOPPING
# =============================================================================
g_ls_glasses_post = LS("", post=P_LISA_GLASSES)

# Izzy bikini shopping
g_i_bik1_pic   = I_("", img=I_IZZY_BIK1, gal=1)
g_mc_fun       = MI_("Fun")
g_mc_summery   = MI_("Very summery")
g_i_summery    = I_("Summery")
g_i_that_word  = I_("That's the word you're going with")
g_mc_suits_u   = MI_("It suits you")
g_i_hmm        = I_("Hmm")
g_i_polite     = I_("That's a very polite way of saying it's not the one")
g_mc_didnt_say = MI_("I didn't say that")
g_i_didnt_have = I_("You didn't have to")
g_i_minute     = I_("Give me a minute")
g_i_black_pic  = I_("", img=I_IZZY_BLACK_F, gal=1, tl="A few minutes later...")
g_mc_wow_bk    = MI_("wow...")
g_mc_buy_it    = MI_("Are you going to buy that one?")
g_i_not_yet    = I_("Haven't decided yet")
g_i_do_u_like  = I_("Do you like it?")
g_mc_its       = MI_("Its\u2026")
g_i_u_can_say  = I_("You can say it")

g_l_be_honest  = L("You can be honest", tl="1 hour later...")
g_l_izzy_back  = L("", img=I_IZZY_BLACK_B, gal=1)
g_l_its_hot    = L("Its hot right")
g_mc_sexy_af   = ML("Its sexy as fuck")
g_l_tell_her   = L("Tell her!")
g_i_so_q       = I_("So?")
g_mc_sexy_af2  = MI_("Its sexy as fuck")
g_i_ill_buy    = I_("Then I'll buy it")

# Lily's bikini turn
g_l_my_turn    = L("Okay my turn")
g_l_3_options  = L("I found three options")
g_l_here_one   = L("Here is number one")
g_l_bik1_pic   = L("", img=I_LILY_BIK1, gal=1)
g_mc_nice_1    = ML("Really nice")
g_l_num_two    = L("Number two")
g_l_bik2_pic   = L("", img=I_LILY_BIK2, gal=1)
g_mc_even_btr  = ML("Even better")
g_l_and_three  = L("And three...")
g_l_bik3_pic   = L("", img=I_LILY_BIK3_IZZ, gal=1)
g_mc_dots_bik  = ML("...")
g_l_i_know_bik = L("I know")
g_l_so_q       = L("So?")
g_l_which_one  = L("Which one?")
g_mc_get_vote  = ML("Do I even get a vote?")
g_mc_izzy_thr  = ML("With Izzy right there?")
g_l_hihi_vote  = L("hihi, good question")
g_l_strong_opin= L("She already has a very strong opinion")
g_l_see_face   = L("I can see it on her face")

g_c_bikini     = mc2(LILY, [
    ("**Text Izzy**",          ""),
    ("Let me guess. Number three.", "")
])
# Port 0 — text Izzy
g_mc_which_iz  = MI_("So which one Izzy?", y=0)
g_i_u_know     = I_("You know which one but I'll make it more clear for you", y=0)
g_i_lily_bik3  = I_("", img=I_LILY_BIK3_IZZ, gal=1, y=0)
g_mc_fuck_bik  = MI_("Fuck...", y=0)
g_mc_i_get_it  = MI_("Okay, I get it", y=0)
# Port 1 — let me guess
g_l_izzy_thumb = L("Izzy is giving thumbs up", y=600)
g_l_thumbs_pic = L("", img=I_IZZY_THUMBS, gal=1, y=600)

# MERGE bikini choice
g_l_decision   = L("Well... I guess the decision is made")
g_l_bit_much   = L("It's a little much.... But I like it")

# pre-alloc IF tag_along for bikini ending text
g_if_tag_bik   = ng()

# tag_along TRUE + lisa_know TRUE — for tonight
g_l_perfect_nt = L("And honestly... for tonight it's perfect", y=0)
g_mc_instincts = ML("Izzy has good instincts", y=0)
g_l_she_always = L("She always does", y=0)
g_l_heading_bk = L("Okay we are heading back", y=0)
g_l_get_ready  = L("I need to get ready for tonight", y=0)

# tag_along FALSE / lisa_know FALSE — quiet evening
g_mc_instincts2= ML("She really does", y=600)
g_l_heading_bk2= L("Okay we are heading back", y=600)
g_l_quiet_eve  = L("Quiet evening ahead", y=600)

# MERGE bikini ending
g_l_love_bik   = L("Love you")
g_mc_love_bik  = ML("Love you too")

# =============================================================================
# EVENING — YACHT PATH  (tag_along==true && lisa_know==true)
# =============================================================================
# pre-alloc IF tag+know for evening path
g_if_tag_eve   = ng()

g_l_izzy_left  = L("Izzy just left for her date", tl="1 hour later...", y=0)
g_l_izzy_out_pic= L("", img=I_IZZY_OUT, gal=1, y=0)
g_l_lobby_20   = L("Dave and Lisa are meeting me in the lobby in 20 minutes", y=0)
g_l_mirror_pic = L("", img=I_LILY_MIRROR, gal=1, tl="A few minutes later...", y=0)
g_l_ready      = L("Ready", y=0)
g_mc_wow_yacht = ML("Wow", y=0)
g_l_that_good  = L("That good?", y=0)
g_mc_that_good = ML("That good", y=0)
g_l_good_ok    = L("Good", y=0)
g_l_text_boat  = L("I'll text you when we're on the boat", y=0)
g_l_love_yacht = L("Love you", y=0)
g_mc_love_yacht= ML("Love you too", y=0)
g_l_im_lobby   = L("Okay I'm in the lobby", tl="15 minutes later...", y=0)
g_l_see_them   = L("I can see them walking in", y=0)
g_l_oh_wow     = L("Oh wow", y=0)
g_l_dave_great = L("Dave looks great but Lisa...", y=0)
g_l_lisa_absol = L("Lisa looks absolutely\u2014", y=0)
g_mc_lily_q    = ML("Lily?", y=0)
g_mc_hello_q   = ML("Hello?", y=0)
g_l_lobby_pic  = L("", img=I_LOBBY, gal=1, y=0)

# =============================================================================
# EVENING — NON-YACHT PATH  (tag_along==false OR lisa_know==false)
# =============================================================================
g_l_izzy_left2 = L("Izzy just left for her date", tl="1 hour later...", y=600)
g_l_izzy_out_p2= L("", img=I_IZZY_OUT, gal=1, y=600)
g_l_early_nite = L("I'm just going to chill and get an early night...", y=600)
g_l_fun_tonite = L("Or are you maybe in the mood to give me some more fun tonight?", y=600)
g_l_bed_bik_pic= L("", img=I_LILY_BED_BIK, gal=1, y=600)
g_mc_take_off  = ML("Take it off", y=600)
g_l_bossy      = L("Hmmm... bossy", y=600)
g_l_move_lit   = L("Maybe I'll just move it a little", y=600)
g_l_top_pic    = L("", img=I_LILY_TOP_SIDE, gal=1, y=600)
g_l_like_this  = L("Like this?", y=600)
g_mc_exactly   = ML("Exactly like that", y=600)
g_l_bottoms_q  = L("And the bottoms?", y=600)
g_mc_move_too  = ML("Move those too", y=600)
g_l_bot_pic    = L("", img=I_LILY_BOT_SIDE, gal=1, y=600)
g_l_better_q   = L("Better?", y=600)
g_mc_much_btr  = ML("Much better", y=600)
g_l_good_nys   = L("Good", y=600)
g_l_tell_want  = L("Now tell me what you want me to do", y=600)
g_mc_touch_yrself= ML("Touch yourself", y=600)
g_l_already_am = L("Already am...", y=600)
g_l_tell_wish  = L("Tell me what you wish you could do to me right now", y=600)
g_l_breast_vid = L("", vid=V_BREAST, thumb=V_BREAST_BNR, gal=1, y=600)
g_mc_god_nys   = ML("God", y=600)
g_mc_neck      = ML("I'd start with your neck", y=600)
g_mc_work_down = ML("Work my way down slowly", y=600)
g_mc_not_rush  = ML("Not rushing anything", y=600)
g_l_keep_going = L("...", y=600)
g_l_keep_go2   = L("Keep going", y=600)
g_mc_take_time = ML("I'd take my time with every part of you", y=600)
g_mc_focus     = ML("But I'll focus mostly on that sweet pussy of yours", y=600)
g_l_wish_here  = L("I wish you were here", y=600)
g_l_i_really   = L("I really do", y=600)
g_mc_me_too_nys= ML("Me too", y=600)
g_l_going_cum  = L("I... I'm going to cum already babe... just thinking of you", y=600)
g_l_hand_pic   = L("", img=I_LILY_BOT_SIDE, gal=1, y=600)
g_l_cum_vid    = L("", vid=V_CUM, thumb=V_CUM_BNR, gal=1, y=600)
g_l_fuck_good  = L("FUCK THAT WAS GOOD", y=600)
g_l_really_ndd = L("That was really needed", y=600)
g_mc_could_tell= ML("I could tell", y=600)
g_l_shut_up    = L("Shut up", y=600)
g_l_cry_laugh2 = L("\U0001f602", y=600)
g_l_love_nys   = L("I love you", y=600)
g_mc_love_nys  = ML("I love you too", y=600)
g_l_early_nite2= L("Okay NOW I'm getting an early night", y=600)
g_l_goodnight  = L("Goodnight baby", y=600)
g_mc_goodnight = ML("Goodnight", y=600)

# =============================================================================
# G-WAGON SECTION  (tag_along==true only — branched via nested IFs)
# =============================================================================
# pre-alloc IF tag_along for g-wagon
g_if_tag_gw    = ng()
# pre-alloc IF lisa_know inside tag=TRUE for g-wagon
g_if_lk_gw    = ng()
# pre-alloc IF dave_lily inside lisa_know=TRUE for g-wagon
g_if_dl_gw    = ng()

# tag_along TRUE, lisa_know FALSE — Dave text
g_d_gwagon_pic = D("", img=I_GWAGON_DAVE, gal=1, y=0)
g_d_ready_gw   = D("Ready bro", y=0)
g_d_something  = D("Tonight is going to be something else", y=0)
g_mc_have_fun2 = MD("Have fun out there", y=0)
g_d_oh_we_will = D("Oh we will", y=0)

# tag_along TRUE, lisa_know TRUE, dave_lily FALSE — Lisa text trio
g_ls_trio_pic  = LS("", img=I_GWAGON_TRIO, gal=1, y=600)
g_mc_seems_rdy = MLS("Seems like you guys are ready", y=600)
g_ls_ready_can = LS("Ready as can be", y=600)
g_ls_hot_prob  = LS("Tonight is going to be... hot probably (;", y=600)
g_mc_fun_out   = MLS("Have fun out there", y=600)

# tag_along TRUE, lisa_know TRUE, dave_lily TRUE — Lisa text with hand
g_ls_hand_pic  = LS("", img=I_GWAGON_HAND, gal=1, y=1200)
g_mc_seems_rdy2= MLS("Seems like you guys are ready", y=1200)
g_ls_ready_can2= LS("Ready as can be", y=1200)
g_ls_notice    = LS("Aren't you noticing anything else?", y=1200)
g_mc_i_do      = MLS("I do", y=1200)
g_mc_dave_close= MLS("Dave and Lily are close...", y=1200)
g_ls_good_eye  = LS("Good eye", y=1200)
g_ls_hot_prob2 = LS("Tonight is going to be... hot probably", y=1200)
g_ls_keep_upd  = LS("We'll keep you up to date (;", y=1200)

# =============================================================================
# END — Izzy social post (all paths)
# =============================================================================
g_i_drunk_post = I_("", post=P_IZZY_DRUNK)
g_end          = end()

# =============================================================================
# WIRE LINKS
# =============================================================================

# Opening chain
chain(g_start, g_ins_payment, g_ins_plans, g_ins_day)

# IF math_continue: TRUE → check medicine, FALSE → NTS path
g_if_math_node = ifn(VAR_MATH_CONT, g_if_doc, g_l_gm_nts,
                     preset_guid=g_if_math, y=0)
links.append((g_ins_day, g_if_math_node))

# ── NTS PATH ─────────────────────────────────────────────────────────────────
chain(g_l_gm_nts, g_l_taxi_back, g_l_slept_well,
      g_mc_gm, g_mc_thats_gd, g_mc_drive_sf,
      g_l_taxi_pic, g_l_will_try, g_l_not_driv, g_l_see_soon,
      g_l_love_nts1, g_mc_love_nts1,
      g_l_im_back, g_l_izzy_slp, g_l_busy_nite, g_l_shower_txt,
      g_mc_sounds_gd, g_mc_want_hear,
      g_l_hihi_nts, g_l_heard_all, g_mc_again1, g_mc_again2,
      g_l_cry_laugh, g_l_20_min,
      g_l_shower_pic,
      g_mc_so, g_l_so, g_l_last_nite, g_mc_tell_all,
      g_l_know_most, g_l_but_person, g_l_studio, g_l_special,
      g_mc_and_leo, g_l_leo_was, g_l_good_man, g_l_checking,
      g_l_ok_while, g_l_dicking,
      g_mc_hmm_love, g_mc_glad_care,
      g_l_me_too_nts, g_l_found_one, g_mc_we_did,
      g_l_bed_pic, g_l_enough_nts, g_l_3_classes, g_mc_busy_man,
      g_l_what_do, g_mc_same_yd, g_mc_laying,
      g_l_no_plans, g_l_kinda_nice, g_l_relaxing, g_l_good_day,
      g_mc_happy_hr, g_l_hihi_nts2, g_l_text_latr,
      g_mc_sure_luv, g_l_luv_too, g_l_kiss_pic)
links.append((g_l_kiss_pic, g_if_lisa_know))

# ── CUCK PATH ─────────────────────────────────────────────────────────────────
g_if_doc_node = ifn(VAR_DOC_DENY, g_mbb_prone_post, g_ma_morn_ck,
                    preset_guid=g_if_doc, y=0)

chain(g_ma_morn_ck, g_ma_dropping, g_ma_long_nite,
      g_mc_imagine, g_ma_details, g_ma_not_walk,
      g_ma_ass_pic, g_ma_text_bk, g_mc_thnx_math,
      g_l_hey_ck, g_l_back_htl, g_l_dress_pic,
      g_mc_tired_ck, g_l_math_early, g_l_no_compln,
      g_l_prone_pic, g_mc_glad_hpy,
      g_l_freshen_ck, g_l_talk_soon, g_l_love_ck, g_mc_love_ck)
links.append((g_mc_love_ck, g_if_lisa_know))

# ── NTR PATH ─────────────────────────────────────────────────────────────────
chain(g_mbb_prone_post,
      g_l_gm_ntr, g_l_sorry_quiet, g_l_too_much, g_l_back_safe,
      g_l_casual_pic,
      g_mc_morn_ntr, g_mc_glad_safe, g_l_sleep_ok,
      g_mc_yea_ntr, g_mc_worried,
      g_l_sorry_ntr, g_l_freshen_ntr)
links.append((g_l_freshen_ntr, g_c_ntr_choice))

# NTR Choice Port 0 — okay
links.append((g_c_ntr_choice, g_l_wink_pic_0))
links.append((g_l_wink_pic_0, g_l_talk_ntr))
# NTR Choice Port 1 — freshen up?
links.append((g_c_ntr_choice, g_l_uh_shower))
chain(g_l_uh_shower, g_l_just_woke, g_mc_yea_ok, g_l_wink_pic_1)
links.append((g_l_wink_pic_1, g_l_talk_ntr))

chain(g_l_talk_ntr, g_l_love_ntr, g_mc_love_ntr)
links.append((g_mc_love_ntr, g_if_lisa_know))

# ── LISA SECTION (all paths merge here) ──────────────────────────────────────
g_if_lisa_know_node = ifn(VAR_LISA_KNOW, g_ls_gm_cuck, g_ls_gm_norm,
                          preset_guid=g_if_lisa_know, y=0)
links.append((g_ls_gm_cuck, g_mc_hi_lisa))
links.append((g_ls_gm_norm, g_mc_hi_lisa))

chain(g_mc_hi_lisa, g_ls_tell_you, g_ls_yesterday, g_mc_okay_ls1,
      g_ls_dave_nap, g_mc_okay_ls2,
      g_ls_browsing, g_ls_minding, g_ls_swimwear,
      g_ls_swim_pic)
links.append((g_ls_swim_pic, g_c_swimwear))

# swimwear choice
links.append((g_c_swimwear, g_ls_anyway_0))
links.append((g_c_swimwear, g_ls_anyway_1))
links.append((g_ls_anyway_0, g_ls_girl_wlks))
links.append((g_ls_anyway_1, g_ls_girl_wlks))

chain(g_ls_girl_wlks, g_ls_nowhere, g_ls_she_says, g_ls_stunning,
      g_mc_thats_all, g_ls_pointing, g_ls_look_over,
      g_ls_bentley, g_ls_looking,
      g_mc_said_noth, g_ls_no_car, g_ls_at_first, g_ls_but_talk,
      g_mc_what_want, g_ls_open_mind, g_ls_best_nite,
      g_mc_oh_god, g_mc_tables,
      g_ls_right, g_ls_normally, g_ls_scouted, g_ls_mill_cpl,
      g_mc_how_mill, g_ls_talked_bt, g_ls_mia_pic,
      g_mc_gorgeous, g_ls_i_know, g_ls_gd_taste,
      g_ls_yacht, g_mc_millionrs, g_ls_told_ya,
      g_ls_married, g_ls_no_mind,
      g_mc_how_crazy, g_ls_guess_why)
links.append((g_ls_guess_why, g_c_guess_why))

# guess why choice
links.append((g_c_guess_why, g_ls_do_you))
chain(g_ls_do_you, g_ls_clearer)
links.append((g_ls_clearer, g_ls_ass_pic))
links.append((g_c_guess_why, g_ls_show_inst))
links.append((g_ls_show_inst, g_ls_ass_pic))

chain(g_ls_ass_pic, g_ls_questions)
links.append((g_ls_questions, g_c_questions))

# questions choice
links.append((g_c_questions, g_ls_didnt_thk))
links.append((g_ls_didnt_thk, g_ls_dave_fell))
links.append((g_c_questions, g_ls_he_really))
chain(g_ls_he_really, g_ls_wife_too)
links.append((g_ls_wife_too, g_ls_dave_fell))

chain(g_ls_dave_fell, g_ls_cry_laugh, g_mc_can_imag,
      g_ls_tonight, g_ls_feel_it, g_ls_tag_along)
links.append((g_ls_tag_along, g_c_tag_along))

# ── TAG ALONG CHOICE ─────────────────────────────────────────────────────────
# Port 0 — TRUE
links.append((g_c_tag_along, g_evt_tag_true))
chain(g_evt_tag_true, g_mc_duuh)

g_if_lisa_in_tag_node = ifn(VAR_LISA_KNOW, g_ls_obviously, g_ls_well_then,
                            preset_guid=g_if_lisa_in_tag, y=0)
links.append((g_mc_duuh, g_if_lisa_in_tag_node))

# lisa_know TRUE inside tag=TRUE
chain(g_ls_obviously, g_ls_one_more, g_mc_q_mark,
      g_ls_lily_infmd, g_mc_informed, g_mc_lisa_q, g_mc_what_tell,
      g_mc_lisa_q2)
# from mc_lisa_q2 → extended lily section
links.append((g_mc_lisa_q2, g_mc_hey_ext))

# lisa_know FALSE inside tag=TRUE
chain(g_ls_well_then, g_ls_talk_latr, g_mc_later_tag)
# from mc_later_tag → simple lily section (tag_along=TRUE, lisa_know=FALSE)
links.append((g_mc_later_tag, g_l_hey_simple))

# Port 1 — FALSE
links.append((g_c_tag_along, g_evt_tag_false))
chain(g_evt_tag_false, g_mc_no_tag,
      g_ls_dots, g_ls_didnt_exp, g_ls_well_f,
      g_ls_talk_latr2, g_mc_sorry_latr)
# from mc_sorry_latr → simple lily section
links.append((g_mc_sorry_latr, g_l_hey_simple))

# ── EXTENDED LILY SECTION ────────────────────────────────────────────────────
chain(g_mc_hey_ext, g_mc_so_ext, g_mc_lisa_txd,
      g_l_hey_ext, g_l_def_might, g_l_why_ask,
      g_mc_what_say2, g_l_know_say, g_l_tell_any,
      g_l_started, g_l_adv_dave, g_mc_just_dave,
      g_l_hihi_adv, g_l_other_cpl, g_mc_and_ext,
      g_l_not_surp, g_l_after_all, g_l_still_funn, g_l_ur_bro,
      g_mc_i_know_br, g_l_where_get, g_mc_no_clue, g_l_hihi_bk,
      g_l_spk_adv, g_l_izzy_date, g_mc_another, g_l_another,
      g_l_dont_know, g_l_milking, g_mc_move_isl, g_l_right_ext,
      g_l_back_lisa, g_l_after_dave, g_l_mentioned, g_l_the_yacht,
      g_l_rich_cpl, g_mc_what_thk, g_l_she_invt, g_mc_invited,
      g_l_just_come, g_l_not_per_se, g_l_just_be, g_l_see_nite,
      g_l_honestly, g_mc_honestly2, g_l_excited, g_l_weird_q,
      g_mc_not_at_al, g_l_u_excitd, g_mc_obviously, g_l_thought_so,
      g_l_just_see, g_mc_perfect, g_l_yeah_perf, g_l_me_too_ext)

g_if_dave_lily_ext_node = ifn(VAR_DAVE_LILY, g_l_ok_so_dl, g_l_drag_izzy_f,
                              preset_guid=g_if_dave_lily_ext, y=0)
links.append((g_l_me_too_ext, g_if_dave_lily_ext_node))

# dave_lily FALSE — simple ending
chain(g_l_drag_izzy_f, g_l_before_date, g_l_new_tonite,
      g_mc_good_idea, g_l_send_pics, g_mc_waiting,
      g_l_love_ext_f, g_mc_love_ext_f)
links.append((g_mc_love_ext_f, g_if_dave_lily_dave))

# dave_lily TRUE — extended conversation
chain(g_l_ok_so_dl, g_l_one_more_dl, g_mc_oh_dl, g_l_yeah_dl,
      g_l_specific, g_mc_lily_i, g_l_thought_it,
      g_mc_honestly3, g_l_thats_all, g_mc_crossed, g_mc_didnt_pln,
      g_mc_lisa_askd, g_mc_but_yes, g_mc_crossed2,
      g_l_hes_bro, g_mc_i_know_dl, g_l_different, g_mc_know_it,
      g_l_dave_know, g_mc_no_dl, g_mc_lisa_own,
      g_l_okay_dl, g_l_dont_feel, g_l_not_noth,
      g_mc_know_dl2, g_mc_not_push, g_mc_not_close,
      g_l_wouldnt_st, g_mc_dots_dl, g_mc_dont_know,
      g_l_honest_dl, g_l_can_we, g_l_see_tonite, g_l_no_plan,
      g_mc_of_course, g_mc_no_press, g_l_need_dist,
      g_l_drag_izzy_t, g_l_bef_date_t, g_l_new_tonite2,
      g_mc_good_idea2, g_l_send_pics2, g_mc_waiting2,
      g_l_love_ext_t, g_mc_love_ext_t)
links.append((g_mc_love_ext_t, g_if_dave_lily_dave))

# ── DAVE DAYTIME TEXTS (tag_along=TRUE, lisa_know=TRUE) ──────────────────────
g_if_dave_lily_dave_node = ifn(VAR_DAVE_LILY, g_d_yo_t, g_d_yo_f,
                               preset_guid=g_if_dave_lily_dave, y=0)

# dave_lily FALSE — short text
chain(g_d_yo_f, g_d_filled_in_f, g_mc_she_did_f, g_mc_sounds_cz,
      g_d_it_does, g_d_chick_hot, g_d_crazy_hot, g_d_first_time,
      g_mc_yea_norm, g_d_exactly, g_d_lily_too, g_mc_yea_f,
      g_d_super_sick, g_d_even_watch, g_mc_agreed,
      g_d_check_in, g_d_talk_latr, g_mc_cant_wait, g_d_me_too_f)
links.append((g_d_me_too_f, g_ls_glasses_post))

# dave_lily TRUE — long text
chain(g_d_yo_t, g_d_filled_in_t, g_mc_she_did_t, g_d_good_good,
      g_d_but_uh, g_d_lisa_told, g_mc_yeah_t, g_d_its_true,
      g_mc_crossed_d, g_d_bro, g_d_are_sure, g_mc_what_mean,
      g_d_its_me, g_d_im_bro, g_d_ur_bro, g_d_really_sure,
      g_mc_lily_talk, g_d_u_talked, g_mc_yeah_d, g_d_she_ok,
      g_mc_no_no, g_mc_no_yes, g_mc_see_nite,
      g_d_bro2, g_d_no_weird, g_d_u_know, g_d_cant_undo,
      g_mc_i_know_d, g_mc_lily_leads, g_mc_whatever,
      g_mc_contact, g_mc_no_push, g_d_u_sure_t,
      g_mc_im_sure_d, g_d_okay_d, g_d_aight, g_d_trust_u,
      g_d_if_wrong, g_d_dont_do, g_d_mean_it,
      g_mc_i_know_d2, g_mc_apprec, g_d_aight2,
      g_d_wild, g_mc_tell_me_d, g_d_haha,
      g_d_talk_latr2, g_mc_later_d, g_d_owe_you)
links.append((g_d_owe_you, g_c_owe))

# owe choice
links.append((g_c_owe, g_d_right_d))
chain(g_d_right_d, g_d_here_pic, g_d_lisa_ass, g_mc_damn_d,
      g_d_more_even, g_d_later_d, g_mc_uh_later)
links.append((g_mc_uh_later, g_ls_glasses_post))

links.append((g_c_owe, g_d_idk))
chain(g_d_idk, g_d_nvm, g_d_later_d2)
links.append((g_d_later_d2, g_ls_glasses_post))

# ── SIMPLE LILY SECTION (tag_along=FALSE or lisa_know=FALSE) ─────────────────
chain(g_l_hey_simple, g_l_bikini_shp, g_mc_hey_smpl, g_mc_occasion,
      g_l_not_really, g_l_izzy_date2, g_mc_again_s, g_l_again_s,
      g_l_dont_know2, g_l_milking2, g_mc_move_isl2, g_l_right_s,
      g_l_anyway_s, g_l_tag_alng, g_mc_have_fun,
      g_l_thanks_luv, g_mc_luv_too_s)
links.append((g_mc_luv_too_s, g_ls_glasses_post))

# ── BIKINI SHOPPING (all paths) ───────────────────────────────────────────────
chain(g_ls_glasses_post,
      g_i_bik1_pic, g_mc_fun, g_mc_summery,
      g_i_summery, g_i_that_word, g_mc_suits_u,
      g_i_hmm, g_i_polite, g_mc_didnt_say, g_i_didnt_have,
      g_i_minute, g_i_black_pic,
      g_mc_wow_bk, g_mc_buy_it, g_i_not_yet,
      g_i_do_u_like, g_mc_its, g_i_u_can_say,
      g_l_be_honest, g_l_izzy_back, g_l_its_hot,
      g_mc_sexy_af, g_l_tell_her,
      g_i_so_q, g_mc_sexy_af2, g_i_ill_buy,
      g_l_my_turn, g_l_3_options, g_l_here_one,
      g_l_bik1_pic, g_mc_nice_1, g_l_num_two,
      g_l_bik2_pic, g_mc_even_btr, g_l_and_three,
      g_l_bik3_pic, g_mc_dots_bik, g_l_i_know_bik,
      g_l_so_q, g_l_which_one,
      g_mc_get_vote, g_mc_izzy_thr, g_l_hihi_vote,
      g_l_strong_opin, g_l_see_face)
links.append((g_l_see_face, g_c_bikini))

# bikini choice
links.append((g_c_bikini, g_mc_which_iz))
chain(g_mc_which_iz, g_i_u_know, g_i_lily_bik3, g_mc_fuck_bik, g_mc_i_get_it)
links.append((g_mc_i_get_it, g_l_decision))

links.append((g_c_bikini, g_l_izzy_thumb))
chain(g_l_izzy_thumb, g_l_thumbs_pic)
links.append((g_l_thumbs_pic, g_l_decision))

chain(g_l_decision, g_l_bit_much)

# IF tag_along for bikini ending
g_if_tag_bik_node = ifn(VAR_TAG_ALONG, g_l_perfect_nt, g_mc_instincts2,
                        preset_guid=g_if_tag_bik, y=0)
links.append((g_l_bit_much, g_if_tag_bik_node))

# tag_along TRUE
chain(g_l_perfect_nt, g_mc_instincts, g_l_she_always,
      g_l_heading_bk, g_l_get_ready)
links.append((g_l_get_ready, g_l_love_bik))

# tag_along FALSE
chain(g_mc_instincts2, g_l_heading_bk2, g_l_quiet_eve)
links.append((g_l_quiet_eve, g_l_love_bik))

chain(g_l_love_bik, g_mc_love_bik)

# IF tag_along for evening path
g_if_tag_eve_node = ifn(VAR_TAG_ALONG, g_l_izzy_left, g_l_izzy_left2,
                        preset_guid=g_if_tag_eve, y=0)
links.append((g_mc_love_bik, g_if_tag_eve_node))

# ── YACHT PATH ────────────────────────────────────────────────────────────────
chain(g_l_izzy_left, g_l_izzy_out_pic, g_l_lobby_20,
      g_l_mirror_pic, g_l_ready,
      g_mc_wow_yacht, g_l_that_good, g_mc_that_good, g_l_good_ok,
      g_l_text_boat, g_l_love_yacht, g_mc_love_yacht,
      g_l_im_lobby, g_l_see_them, g_l_oh_wow,
      g_l_dave_great, g_l_lisa_absol,
      g_mc_lily_q, g_mc_hello_q, g_l_lobby_pic)
links.append((g_l_lobby_pic, g_if_tag_gw))

# ── NON-YACHT PATH ────────────────────────────────────────────────────────────
chain(g_l_izzy_left2, g_l_izzy_out_p2, g_l_early_nite, g_l_fun_tonite,
      g_l_bed_bik_pic, g_mc_take_off, g_l_bossy, g_l_move_lit,
      g_l_top_pic, g_l_like_this, g_mc_exactly, g_l_bottoms_q,
      g_mc_move_too, g_l_bot_pic, g_l_better_q, g_mc_much_btr,
      g_l_good_nys, g_l_tell_want, g_mc_touch_yrself,
      g_l_already_am, g_l_tell_wish, g_l_breast_vid,
      g_mc_god_nys, g_mc_neck, g_mc_work_down, g_mc_not_rush,
      g_l_keep_going, g_l_keep_go2, g_mc_take_time, g_mc_focus,
      g_l_wish_here, g_l_i_really, g_mc_me_too_nys,
      g_l_going_cum, g_l_hand_pic, g_l_cum_vid,
      g_l_fuck_good, g_l_really_ndd, g_mc_could_tell,
      g_l_shut_up, g_l_cry_laugh2, g_l_love_nys, g_mc_love_nys,
      g_l_early_nite2, g_l_goodnight, g_mc_goodnight)
links.append((g_mc_goodnight, g_if_tag_gw))

# ── G-WAGON SECTION ───────────────────────────────────────────────────────────
# IF tag_along TRUE → IF lisa_know
g_if_tag_gw_node = ifn(VAR_TAG_ALONG, g_if_lk_gw, g_i_drunk_post,
                       preset_guid=g_if_tag_gw, y=0)

g_if_lk_gw_node = ifn(VAR_LISA_KNOW, g_if_dl_gw, g_d_gwagon_pic,
                      preset_guid=g_if_lk_gw, y=0)

g_if_dl_gw_node = ifn(VAR_DAVE_LILY, g_ls_hand_pic, g_ls_trio_pic,
                      preset_guid=g_if_dl_gw, y=0)

# lisa_know FALSE → Dave g-wagon
chain(g_d_gwagon_pic, g_d_ready_gw, g_d_something,
      g_mc_have_fun2, g_d_oh_we_will)
links.append((g_d_oh_we_will, g_i_drunk_post))

# lisa_know TRUE, dave_lily FALSE → Lisa trio
chain(g_ls_trio_pic, g_mc_seems_rdy, g_ls_ready_can,
      g_ls_hot_prob, g_mc_fun_out)
links.append((g_mc_fun_out, g_i_drunk_post))

# lisa_know TRUE, dave_lily TRUE → Lisa with hand
chain(g_ls_hand_pic, g_mc_seems_rdy2, g_ls_ready_can2,
      g_ls_notice, g_mc_i_do, g_mc_dave_close,
      g_ls_good_eye, g_ls_hot_prob2, g_ls_keep_upd)
links.append((g_ls_keep_upd, g_i_drunk_post))

# END
links.append((g_i_drunk_post, g_end))

# =============================================================================
# SERIALIZE
# =============================================================================

def link_yaml():
    out = ""
    seen = set()
    for a, b in links:
        if (a, b) not in seen:
            seen.add((a, b))
            out += f"  - BaseNodeGuid: {a}\n    TargetNodeGuid: {b}\n"
    return out

_outlinks = defaultdict(list)
for _base, _tgt in links:
    if _base in port_pgs:
        _outlinks[_base].append(_tgt)

_cnodes_str = "".join(cnodes)
for _ng_id, _pgs in port_pgs.items():
    _targets = _outlinks.get(_ng_id, [])
    for _i, _pg in enumerate(_pgs):
        _t = _targets[_i] if _i < len(_targets) else ""
        _cnodes_str = _cnodes_str.replace(f"__IGUID_{_pg}__", _t)

out = (
    "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n"
    "--- !u!114 &11400000\nMonoBehaviour:\n"
    "  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n"
    "  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n"
    "  m_GameObject: {fileID: 0}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n"
    "  m_Script: {fileID: 11500000, guid: 1946feaec865d344dabf0419b1c94973, type: 3}\n"
    "  m_Name: Episode 24\n"
    "  m_EditorClassIdentifier: Assembly-CSharp::MeetAndTalk.DialogueContainerSO\n"
    "  AllowDialogueSave: 0\n  BlockingReopeningDialogue: 0\n"
    "  NodeLinkDatas:\n" + link_yaml() +
    "  DialogueChoiceNodeDatas:\n" + _cnodes_str +
    "  DialogueNodeDatas:\n"      + "".join(dnodes) +
    "  TimerChoiceNodeDatas: []\n"
    "  EndNodeDatas:\n"           + "".join(endnodes) +
    "  EventNodeDatas:\n"         + "".join(enodes) +
    "  StartNodeDatas:\n"         + "".join(stnodes) +
    "  RandomNodeDatas: []\n"
    "  CommandNodeDatas: []\n"
    "  IfNodeDatas:\n"            + "".join(ifnodes) +
    "  SpyNodeDatas: []"
)

out_path = os.path.join(os.path.dirname(os.path.abspath(__file__)), "Episode 24.asset")
with open(out_path, "w", encoding="utf-8") as f:
    f.write(out)

print(f"Written {len(out):,} bytes -> {out_path}")
print(f"Nodes: {len(cnodes)} choice | {len(dnodes)} dialogue | "
      f"{len(enodes)} event | {len(ifnodes)} if | "
      f"{len(stnodes)} start | {len(endnodes)} end")
print(f"Links: {len(links)}")
