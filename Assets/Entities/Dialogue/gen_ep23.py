import uuid, os
from collections import defaultdict

def ng(): return str(uuid.uuid4())

# ── Characters ────────────────────────────────────────────────────────────────
LILY      = "7bd5d83a9443f8449b2f8346c4479558"
IZZY      = "8f6af30f1ed271b4aac60c69e0824cfb"
LEO       = "6323b46c77e31894982815cddd266d24"
MATH      = "3d9d5c52104213243936a370d1fdfff5"
MATHBABES = "df054aa4384061f44999656197df76d1"

# ── Events ────────────────────────────────────────────────────────────────────
EVT_STUDENT_T = "e8fe6a56d41b3d448bd1d63e2d893e17"
EVT_STUDENT_F = "1b2d1db2551b10a409118dca5e5b723d"

# ── Variable names for IF nodes ───────────────────────────────────────────────
VAR_IZZY_HEAD = "Izzy_give_Lily_Head"
VAR_MATH_CONT = "Continue_math"
VAR_LEO_HUM   = "ep14_leo_images"
VAR_DOC_DENY  = "Docter_Medicine_denied"
VAR_STU_NUM   = "student_number"

# ── Images ────────────────────────────────────────────────────────────────────
I_LEO_COCK       = "10ba12c5a04c0b741b330089488999df"
I_LILY_ROBE_FL   = "d59951b0a6ac1ae469ef862efd95c48f"
I_LILY_CAR       = "05cc4f91e25c7f8489aa4b120e6f5682"
I_LILY_ROBE_HALL = "9d25847d1bf0e614eb29402e270f3e55"
I_STUDIO_WIDE    = "d6f36e7a1b2644f4483f4ed50b5395ba"
I_LILY_BACK_NAK  = "dc6fec991365e1e4c84b699d6eac863b"
I_LILY_UNDW_BACK = "7fd98b66abeb1324f9373e277972112b"
I_LILY_BARE_STD  = "6d8dbbe015e898441b1b10013ac01589"
I_LILY_UNDW_STD  = "3559d3b6339f24444b562c157592f0a8"
I_NUM_PAPER      = "d9f0cb535bf4e24478f94cbe86f38c8c"
I_PAPER_TRASH    = "21a9febf06bba6f43b6e381803974fe7"
I_WINE_GLASSES   = "db9477543a3fb034aaccaf9ff72ed70e"
I_LILY_RIDING    = "a1dedb0210e743f429482a76bfd1d4ea"
I_LILY_BED       = "00f93f38ac2d99b478a7963985098db0"
I_CUM_MOUTH      = "729360fd36a4a44418af9fd61236ced4"
I_BLACK_DRESS    = "0e828e5ec90bbf64fb4011383826c73c"
I_IZZY_RED       = "c2490e2caeec17c4daf8f7348e07fcb5"
I_CLUB_ENT       = "e3a0adfd18066e440a84e61229ffae7b"
I_MATH_BOOTH     = "1f904d2492256994aa373bbd224387d0"
I_LILY_MTH_DNC   = "04b003467f7118c40a6f6f80b4bf5e80"
I_LILY_DRK_WAVE  = "46edce3f04846884cb38e642ac4138ea"
I_MATH_FUCK      = "418ff30c0de1a11479df74694a82ba95"
I_BOOTH_SELFIE   = "a931e8541a9cc4546ae67b99deeaaf66"
I_LILY_ASS       = "624093d3dc1507147949640e5b6aad32"
I_LILY_CUM_COCK  = "aa4441a4c63edb04ea10c84f59946a44"
I_IZZY_BAR       = "8911289d55a28494ebd9f3ecc54b1c40"
I_LILY_ALONE_DNC = "70955db8cb4776f4ca2c44274e137a3f"
I_LILY_DRK_HND   = "c2c21bd9bff1cdd4a8b1b4095113a9c5"
I_IZZY_LILY_CUM  = "d1e6b67e84d7fe141b8e59e5b5fcf3b5"
I_MARTINI        = "e28264dcfa63ebd45865770eb89c1bce"
I_LILY_TAXI      = "59fe3fa3a8f9fe34e8bda5ce9206e5f3"
I_LILY_FUCK_CAR  = "fc5ac33ab4763b641a9f3636457e7112"
I_LEO_STUDIO     = "a946ddb7d488f4441b7456b699ba2539"

# ── Videos ────────────────────────────────────────────────────────────────────
V_LEO_MISS  = "ddbe368c34c6b3e44b919f00b86b0723"
V_LEO_BNR   = "f5325c6cdc6069d4b88a74124b3a71c6"
V_BJ_MATH   = "f77990d463dfd774581d04c972082c6b"
V_BJ_BNR    = "26085ecd70c1b764ba251a68adf9442e"
V_DANCE     = "e10edf26dba4d124d94f5ebc2f9f79ef"
V_DANCE_BNR = "1ef1a4f76cf8ba345aa02a0bd0ed819c"

# ── Social Posts ──────────────────────────────────────────────────────────────
P_IZZY_RUN  = "4b3b693d38eac9f4ab1b216fbdaf0964"
P_TAXI      = "5df0d64658d2e444593a1ce380eb9d69"
P_DNC_VID   = "bad9f2678df464e418f7a71ba7be599a"
P_DNC_STILL = "230949a5a1f6dca4a92484995a2ed9ae"
P_DEEPTHRT  = "39bf3b0d938699a46be1f9f7d7fe0a89"
P_BJ_NTR    = "44238d9a387d15948bf00b026a9eef58"
P_BALLS     = "d0fabd70281e23943abc6db223da4ae0"

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
    """port_texts: list of (choice_text, hint_text) tuples"""
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

def ifn(var, true_g, false_g, preset_guid=None, after_guid=None, y=0):
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
def L(text="", **kw):   return npc(LILY,  text, **kw)
def I_(text="", **kw):  return npc(IZZY,  text, **kw)
def Leo(text="", **kw): return npc(LEO,   text, **kw)
def MA(text="", **kw):  return npc(MATH,  text, **kw)
def MBB(text="", **kw): return npc(MATHBABES, text, **kw)

def ML(txt, hint="", tl="", y=0):   return mc2(LILY,  [(txt, hint)], tl=tl, y=y)
def MLeo(txt, hint="", tl="", y=0): return mc2(LEO,   [(txt, hint)], tl=tl, y=y)
def MMA(txt, hint="", tl="", y=0):  return mc2(MATH,  [(txt, hint)], tl=tl, y=y)
def MI_(txt, hint="", tl="", y=0):  return mc2(IZZY,  [(txt, hint)], tl=tl, y=y)

# =============================================================================
# NODE DEFINITIONS
# =============================================================================

g_start = start()

# ── OPENING (all paths) ───────────────────────────────────────────────────────
g_l_gm          = L("Good morning baby")
g_l_sleep       = L("Did you sleep well")
g_mc_fine       = ML("Hey, yea slept fine.. you?")
g_l_well        = L("I slept very well after all the excitement from yesterday")
g_mc_imagine    = ML("I can imagine")

# Pre-alloc for IF izzy_eat_out
g_if_izzy       = ng()

# TRUE branch – izzy did eat out Lily (y=0)
g_l_unexpected  = L("It was so unexpected.", y=0)
g_l_didnt       = L("I really didn't think Izzy would go that far", y=0)
g_mc_wild       = ML("It was wild", y=0)
g_mc_izzy_wild  = ML("But... Izzy is kinda wild", y=0)
g_l_true        = L("True", y=0)
g_mc_where      = ML("Where is she now", y=0)
g_l_run         = L("She went out to go for a run actually", y=0)
g_mc_run_q      = ML("A run? Izzy?", y=0)
g_l_liked_it    = L("Yea, seems like she kinda liked it hihi", y=0)

# FALSE branch – shorter (y=600)
g_mc_busy       = ML("You definitely had a busy day", y=600)
g_l_i_did       = L("I did hihi", y=600)

# MERGE → social post
g_izzy_run_post = I_("", post=P_IZZY_RUN)

# Pre-alloc for IF math_continue
g_if_math       = ng()

# =============================================================================
# NTS PATH  (math_continue == false → FalseGUID of g_if_math)
# =============================================================================
g_l_but_more    = L("But way more important")
g_l_call_early  = L("I got a call very early this morning")
g_mc_leo_guess  = ML("Let me guess... Leo")
g_l_who_else    = L("Who else hihi")
g_mc_what_say   = ML("What did he say?")
g_l_class_thing = L("He talked about the class thing...")
g_l_model_multi = L("The one where I model... for multiple people...")
g_mc_remember   = ML("I remember")
g_l_spot        = L("He has a spot")
g_l_today_today = L("Like... today today")
g_mc_soon       = ML("That's soon")
g_l_i_know      = L("I know")
g_l_cancell     = L("He said he had a cancellation and the timing is perfect")
g_l_had_think   = L("I told him I had to think about it")
g_mc_and        = ML("And?")
g_l_texting_u   = L("Well I'm texting you aren't I hihi")
g_l_what_think  = L("What do you think?")
g_l_want_do_it  = L("Do you want me to do it?")

# CHOICE: encourage / conditional
g_c_encourage   = mc2(LILY, [
    ("I think you should go for it", ""),
    ("Only if you feel ready",       "")
])
# Port 0 – go for it
g_l_yeah_ok     = L("Yeah?", y=0)
g_l_okay_too    = L("Okay... okay I think so too", y=0)
g_l_heart_fast  = L("My heart is already going a little fast just thinking about it", y=0)
# Port 1 – only if ready
g_l_think_am    = L("I think I am", y=600)
g_l_gondola     = L("I mean... after yesterday on that gondola", y=600)
g_l_room_full   = L("I think I can handle a room full of people looking at me hihi", y=600)
g_mc_fair       = ML("Fair point", y=600)

# MERGE after encourage choice
g_l_okay_text   = L("Okay")
g_l_going_text  = L("I'm going to text Leo back")
g_l_keep_upd    = L("I'll keep you updated")
g_l_pic_also    = L("I'll probably send him a picture as well...")

# Pre-alloc IF leo_humiliation (first check)
g_if_leo_h1     = ng()

# TRUE – humiliation (y=0)
g_l_leo_decides = L("And Leo can decide if you see it too", y=0)
g_mc_ok_h1      = ML("Okay", y=0)
# FALSE – normal (y=600)
g_l_ok_with     = L("Are you okay with that?", y=600)
g_mc_of_course  = ML("Of course... As long as I get to see it too", y=600)
g_l_make_sure   = L("I'll make sure of it", y=600)

# MERGE
g_l_love_baby   = L("Love you baby")
g_mc_love_too   = ML("Love you too")

# 10 minutes later
g_l_its_on      = L("It's on", tl="10 minutes later...")
g_l_picking_up  = L("He's picking me up in 2 hours")
g_l_freaking    = L("I'm freaking out a little")
g_mc_you_got    = ML("You got this")
g_l_thanks1     = L("Thanks")

# Pre-alloc IF leo_humiliation (dickpic section)
g_if_leo_h2     = ng()

# TRUE – dickpic sequence (y=0)
g_l_sent_pic    = L("I sent him the picture by the way", y=0)
g_mc_allowed    = ML("And? Am I allowed to see?", y=0)
g_l_no_hihi     = L("No... hihi", y=0)
g_mc_oh         = ML("oh...", y=0)
g_l_but_excl    = L("But...!", y=0)
g_l_leo_sent    = L("Leo did send me a picture back...", y=0)
g_l_see_if_want = L("And he said you could see it if you really wanted to", y=0)
g_mc_what_pic   = ML("What kind of picture", y=0)
g_l_secret      = L("Secret :3", y=0)
g_mc_secret_q   = ML("Secret?", y=0)
g_l_yes_no      = L("Just yes or no ;P", y=0)
g_c_dickpic     = mc2(LILY, [
    ("I'm okay I think", "You will not get a Leo dickpic"),
    ("Sure... let me see", "You will get a Leo dickpic")
], y=0)
# Port 0 – no dickpic
g_l_jail        = L("Jail ):", y=0)
g_mc_oh_no      = ML("oh no...", y=0)
g_l_not_like    = L("I don't think you would like the picture anyways", y=0)
g_l_right_ch    = L("So I think you made the right choice hihi", y=0)
# Port 1 – yes dickpic
g_l_pic1        = L("", img=I_LEO_COCK, gal=1, y=600)
g_mc_wow        = ML("wow", y=600)
g_l_yea_cock    = L("yea....", y=600)

# FALSE – no humiliation (y=600)
g_l_this_pic    = L("This is the picture I sent Leo by the way", y=600)
g_l_pic2        = L("", img=I_LILY_ROBE_FL, gal=1, y=600)
g_mc_amazing    = ML("Amazing", y=600)
g_mc_did_like   = ML("Did he like it?", y=600)
g_l_loved       = L("Loved it", y=600)

# MERGE both leo_hum branches → getting ready
g_l_get_ready   = L("Okay I need to actually get ready now")
g_l_text_pick   = L("I'll text you when he picks me up")
g_mc_waiting    = ML("Will be waiting")
g_l_heart_3     = L("<3 hihi")

# 2 hours later – Lily car selfie
g_l_pic3        = L("", img=I_LILY_CAR, gal=1, tl="2 hours later...")

# Leo texts
g_leo_hey_dude  = Leo("Hey dude")
g_leo_getting   = Leo("Lily is getting dressed for the first session")
g_mc_multiple   = MLeo("There are multiple sessions")
g_leo_breaks    = Leo("Yea well... just some breaks in between")
g_leo_keep_clo  = Leo("In the breaks Lily could also decide if she wants to keep all the clothes on")
g_leo_take_off  = Leo("Or take some off of course (;")
g_mc_i_see      = MLeo("I see")
g_mc_nervous    = MLeo("Is she nervous")
g_leo_little    = Leo("A little")
g_leo_seen      = Leo("But she hasn't seen the class yet")
g_mc_people     = MLeo("A lot of people?")
g_leo_5         = Leo("Just 5... not too bad... normally it's more")
g_mc_interest   = MLeo("Any interesting people")
g_leo_4long     = Leo("4 who have been coming here a long time")
g_leo_older     = Leo("Older folks")
g_leo_new_guy   = Leo("And a new guy... he's younger")
g_mc_lily_age   = MLeo("Lily's age")
g_leo_could     = Leo("Could be")

# Lily texts – outfit check
g_l_before_in   = L("Okay so before I go in")
g_l_outfit_ck   = L("Outfit check")
g_l_leo_said    = L("Leo said I can wear whatever I'm comfortable with")
g_l_first_sess  = L("For the first session at least")
g_l_pic4        = L("", img=I_LILY_ROBE_HALL, gal=1)
g_l_glamorous   = L("Very glamorous I know hihi")

# CHOICE: beautiful / perfect simple
g_c_robe        = mc2(LILY, [
    ("You look beautiful",  ""),
    ("Perfect. Simple.",    "")
])
# Port 0 – beautiful
g_l_thank_baby  = L("Thank you baby", y=0)
g_l_helps       = L("That helps actually", y=0)
# Port 1 – simple
g_l_going_for   = L("That's what I was going for", y=600)
g_l_easy_rem    = L("Simple and... easy to remove I guess hihi", y=600)
g_mc_thinking   = ML("Are you thinking about that?", y=600)
g_mc_removing   = ML("Removing some of the clothes", y=600)
g_l_see_first   = L("I'll have to see how I feel first", y=600)
g_mc_wouldnt    = ML("Of course... Just know I wouldn't mind (;", y=600)
g_l_of_course2  = L("Of course you wouldn't", y=600)

# MERGE robe choice → peek scene
g_l_peek        = L("Okay so I took a little peek through the door")
g_l_oh_my_god   = L("Oh my god")
g_mc_what       = ML("What?")
g_l_babe        = L("Babe")
g_l_3_women     = L("It's like... 3 older women")
g_l_older_man   = L("And an older man")
g_l_serious     = L("They all look like they have been doing this for years... Very serious")
g_l_easels      = L("Easels set up, charcoal in hand, the whole thing")
g_mc_and2       = ML("And?")
g_l_and3        = L("And...")
g_l_one_more    = L("There is one more")
g_mc_young_g    = ML("The young guy")
g_l_you_knew    = L("You knew?")
g_mc_leo_ment   = ML("Leo mentioned someone")
g_l_of_course3  = L("Of course he did hihi")
g_l_our_age     = L("Babe he is... our age")
g_l_not_bad     = L("And he is not bad looking... at all")
g_l_setting_up  = L("He was setting up his easel and he looked up when I peeked")
g_l_pulled_back = L("I pulled back so fast")
g_l_dont_think  = L("I don't think he saw me")
g_mc_he_def     = ML("He definitely saw you")
g_l_he_didnt    = L("He didn't...")
g_l_doubt       = L("Now you are making me doubt")
g_l_think_look  = L("He's going to think I was looking at him")
g_mc_emoji_hah  = ML("\U0001f602")
g_mc_i_mean     = ML("I mean you were right?")
g_l_no          = L("NO")
g_l_just_wanted = L("I just wanted to see who was going to be in the class...")
g_mc_sureeee    = ML("Sureeeee (;")
g_l_omgstop     = L("o m g s t o p")
g_l_leo_waving  = L("Leo is waving me over....")
g_l_start       = L("I think we are about to start...")
g_mc_you_got2   = ML("You got this!")

# 45 min later – Leo texts
g_leo_done      = Leo("First session done", tl="45 minutes later...")
g_leo_something = Leo("She was something else in there")
g_leo_silent    = Leo("The room was completely silent the whole time")
g_leo_never     = Leo("That never happens with a new model")
g_mc_handle     = MLeo("How did she handle it")
g_leo_owned     = Leo("Like she owned the place")
g_leo_break_15  = Leo("There's a break now. 15 minutes.")
g_leo_pic5      = Leo("", img=I_STUDIO_WIDE)
g_leo_text_you  = Leo("She's going to text you")
g_leo_wanting   = Leo("She's been wanting to since it started hihi")

# Lily back room chat
g_l_back_room   = L("Okay I'm in the back room")
g_l_oh_god2     = L("Oh my god babe")
g_mc_tell_me    = ML("Tell me everything")
g_l_so_quiet    = L("It was so quiet")
g_l_charcoal    = L("Just the sound of charcoal on paper")
g_l_everyone    = L("And everyone looking at me")
g_l_older_focus = L("The older ones were so focused and serious")
g_l_but_guy     = L("But that guy")
g_mc_about_him  = ML("What about him")
g_l_looked_up   = L("He kept looking up")
g_l_more_others = L("More than the others")
g_l_every_time  = L("Every time I felt his eyes I had to concentrate really hard on not reacting hihi")
g_mc_react      = ML("Did you react")
g_l_maybe_tiny  = L("Maybe a tiny bit")
g_l_leo_noticed = L("Leo definitely noticed")
g_l_smile_bk    = L("He gave me this little smile from behind the class")
g_l_die         = L("I wanted to die hihi")
g_mc_good_way   = ML("In a good way")
g_l_best_way    = L("The best way")
g_l_ok_so       = L("Okay so")
g_l_leo_choice  = L("Leo is giving me the choice for the second session")
g_l_want_know   = L("And I want to know what you think before I decide")
g_mc_options    = ML("What are the options")
g_l_option1     = L("Option one")
g_l_keep_bra    = L("I keep the robe off but stay in my bra and panties")
g_l_already_lot = L("Which is already a lot I think")
g_l_or_opt2     = L("Or option two")
g_l_go_bare     = L("I go completely bare")
g_l_everything  = L("Everything off")
g_l_let_them    = L("And just... let them draw me like that")
g_mc_your_call  = ML("That's your call. What do you want.")
g_l_want_u_1st  = L("I want to know what YOU want first")
g_l_tell_u_hihi = L("Then I'll tell you what I want hihi")

# UNDRESS CHOICE — direct branching, no variable needed
g_c_undress     = mc2(LILY, [
    ("Option two. All of it.",     "Lily will go fully nude"),
    ("Option one. Bra and panties.", "Lily will stay in underwear")
])

# ── UNDRESS TRUE PATH (all off, y=0) ─────────────────────────────────────────
g_l_all_q       = L("All of it?", y=0)
g_mc_all_2      = ML("All of it.", y=0)
g_l_dots_ok     = L("...", y=0)
g_l_ok_heard    = L("Okay", y=0)
g_l_wanted_hear = L("That's what I wanted to hear", y=0)
g_l_i_think2    = L("I think", y=0)
g_l_30_sec      = L("I need like 30 seconds to mentally prepare", y=0)
g_l_pic6        = L("", img=I_LILY_BACK_NAK, gal=1, y=0)
g_l_ok_ready    = L("Okay. I'm ready.", y=0)
g_mc_amazing_t  = ML("You're amazing", y=0)
g_l_tell_again  = L("Tell me that again in 10 minutes hihi", y=0)
g_l_love_t      = L("Love you", y=0)
g_mc_love_t     = ML("Love you", y=0)
g_leo_walked    = Leo("She walked back in and dropped the robe", y=0)
g_leo_breathing = Leo("I don't think anyone in the room was breathing", y=0)
g_leo_5_sec     = Leo("For about 5 seconds", y=0)
g_leo_incl_me   = Leo("Including me", y=0)
g_leo_pic8      = Leo("", img=I_LILY_BARE_STD, gal=1, y=0)
g_leo_commit    = Leo("That's commitment", y=0)

# ── UNDRESS FALSE PATH (bra/panties, y=600) ───────────────────────────────────
g_l_yeah_q      = L("Yeah?", y=600)
g_l_lean_that   = L("Honestly that's what I was leaning toward too", y=600)
g_l_baby_steps  = L("Baby steps hihi", y=600)
g_mc_already_ic = ML("You're already doing something incredible", y=600)
g_l_youre_sweet = L("You're sweet", y=600)
g_l_pic7        = L("", img=I_LILY_UNDW_BACK, gal=1, y=600)
g_l_terrifying  = L("This feels like enough to be terrifying", y=600)
g_l_exciting    = L("And enough to be exciting at the same time", y=600)
g_mc_stunning   = ML("You look stunning", y=600)
g_l_going_back  = L("Okay. Going back in.", y=600)
g_l_love_f      = L("Love you", y=600)
g_mc_love_f     = ML("Love you", y=600)
g_leo_shifted   = Leo("She came back in and the room just shifted", y=600)
g_leo_pic9      = Leo("", img=I_LILY_UNDW_STD, gal=1, y=600)
g_leo_students  = Leo("The students didn't know what to do with themselves", y=600)

# MERGE undress paths
g_leo_good_sess = Leo("Good session. She'll fill you in on the rest.")

# Pre-alloc IF leo_humiliation (after undress session)
g_if_leo_h3     = ng()

# TRUE – extra leo line (y=0)
g_leo_fill_her  = Leo("And then after I'll fill her a little more (;", y=0)
# FALSE – empty node to merge through (y=600) — use a "dummy" npc with no text
g_leo_h3_false  = Leo("", y=600)

# MERGE → 40 min later
g_l_its_done    = L("Okay it's done", tl="40 minutes later...")
g_l_second_over = L("Second session is over")
g_l_packing     = L("The students are packing up")
g_l_in_back2    = L("I'm in the back room again")
g_mc_feeling    = ML("How are you feeling")
g_l_honestly    = L("Honestly?")
g_l_amazing     = L("Amazing")
g_l_weird_amaz  = L("Weird and amazing")
g_l_eye_contact = L("There was one moment where I made eye contact with the young guy")
g_l_by_accident = L("By accident I think")
g_l_not_entire  = L("Or maybe not entirely by accident")
g_l_didnt_look  = L("And he didn't look away")
g_l_held_it     = L("I held it for like two seconds and then I looked back at Leo")
g_l_eyebrow     = L("Leo raised an eyebrow at me")
g_l_bite_cheek  = L("I had to bite the inside of my cheek to not laugh")
g_mc_say_after  = ML("Did he say anything to you after")
g_l_leo_q       = L("Leo?")
g_mc_the_guy    = ML("The guy")
g_l_oh          = L("Oh")
g_l_yes_act     = L("...Yes actually")
g_l_came_up     = L("He came up when the others were leaving")
g_l_polite      = L("Very polite")
g_l_first_class = L("He said it was his first life drawing class")
g_l_glad_came   = L("And that he was glad he came")
g_l_gave_num    = L("And then he gave me his number")
g_l_sketch_pap  = L("Written on a corner of his sketch paper")
g_l_tore_off    = L("He just tore it off and handed it to me")
g_l_most_normal = L("Like it was the most normal thing in the world")
g_mc_bold       = ML("Bold")
g_l_right_q     = L("Right?")
g_l_just_took   = L("I didn't know what to say so I just took it")
g_l_standing    = L("And now I'm standing here holding a little piece of paper")
g_l_what_do     = L("What do I do with it")

# CHOICE: keep / throw number
g_c_number      = mc2(LILY, [
    ("Keep it",     ""),
    ("Throw it away", "")
])
# Port 0 – keep
g_evt_stu_t     = evt(EVT_STUDENT_T, y=0)
g_l_yeah_keep   = L("Yeah?", y=0)
g_mc_dont_have  = ML("You don't have to do anything with it", y=0)
g_mc_but_keep   = ML("But keep it", y=0)
g_l_okay_keep   = L("Okay", y=0)
g_l_pic10       = L("", img=I_NUM_PAPER, gal=1, y=0)
g_l_ill_keep    = L("I'll keep it then", y=0)
# Port 1 – throw
g_evt_stu_f     = evt(EVT_STUDENT_F, y=600)
g_l_really_q    = L("Really?", y=600)
g_l_you_sure    = L("You sure?", y=600)
g_mc_im_sure    = ML("I'm sure", y=600)
g_l_pic11       = L("", img=I_PAPER_TRASH, gal=1, y=600)
g_l_done_burn   = L("Done... or should I burn it?", y=600)
g_mc_crying     = ML("\U0001f62d I think this is good enough", y=600)
g_l_ok_wont     = L("Okay I won't hihi", y=600)

# MERGE → Leo knocks
g_l_leo_knocked = L("Leo just knocked on the door")
g_l_all_gone    = L("The students are all gone")
g_l_just_us     = L("It's just us in the studio now")
g_mc_how_feel   = ML("How does that feel")
g_l_quiet       = L("Quiet")
g_l_good_quiet  = L("Good quiet")
g_l_pouring     = L("He's pouring two glasses of wine from somewhere")
g_mc_of_course4 = ML("Of course he is....")
g_mc_rude       = ML("Would be rude not to drink it right")
g_l_right       = L("RIGHT!")
g_l_text_later  = L("I'll text you later okay")
g_l_love_so     = L("I love you so much")

# CHOICE: have fun / tell me everything
g_c_goodbye     = mc2(LILY, [
    ("I love you too. Have fun.", ""),
    ("I love you. Tell me everything later.", "")
])
# Port 0 – have fun
g_l_i_will      = L("I will", y=0)
g_l_heart_have  = L("<3", y=0)
# Port 1 – tell everything
g_l_every_det   = L("Every single detail", y=600)
g_l_promise     = L("Promise", y=600)
g_l_heart_prom  = L("<3", y=600)

# MERGE → Leo social post (image-only node, no post asset found)
g_leo_post_img  = Leo("", img=I_LEO_STUDIO)

# 2 hours later – Lily in Leo's bed
g_l_hey_baby    = L("Hey baby", tl="2 hours later...")
g_l_in_bed      = L("I'm in Leo's bed")
g_l_pass_out    = L("About to pass out hihi")
g_mc_feeling2   = ML("How are you feeling")
g_l_so_good     = L("So good")
g_l_tired_best  = L("Tired in the best way")
g_l_we_had_sex  = L("So... we had sex")
g_mc_tell_me2   = ML("Tell me")
g_l_really_nice = L("It was really nice")
g_l_so_gentle   = L("He was so gentle at first")
g_l_took_time   = L("Like he just took his time with me")
g_l_condom      = L("We used a condom of course")
g_l_pic13       = L("", img=I_LILY_RIDING, gal=1)
g_l_i_took      = L("I took that one hihi")
g_l_he_took     = L("And he took one of us")
g_l_vid1        = L("", vid=V_LEO_MISS, thumb=V_LEO_BNR, gal=1)
g_l_came_twice  = L("I came twice")
g_l_not_expect  = L("Which I was not expecting hihi")
g_mc_incredible = ML("That's incredible")
g_mc_so_happy   = ML("You sound so happy")
g_l_i_am        = L("I am")
g_l_anyway      = L("Anyway")
g_l_really_tired= L("I'm really tired now")
g_l_love_baby2  = L("I love you so much baby")
g_l_thank_being = L("Thank you for being you")
g_l_i_mean_it   = L("I mean it")
g_l_goodnight   = L("Goodnight")
g_mc_gn_love    = ML("Goodnight. I love you.")
g_l_heart_gn    = L("<3")
g_l_pic14       = L("", img=I_LILY_BED, gal=1)

# Leo thanks the MC
g_leo_hey_ty    = Leo("Hey", tl="")
g_leo_thank_you = Leo("Just wanted to say thank you")
g_leo_genuinely = Leo("Genuinely")
g_mc_no_need    = MLeo("No need to thank me")
g_leo_special   = Leo("What you and Lily have is something special")
g_leo_not_many  = Leo("Not a lot of people could handle this with the grace you do")
g_leo_talks_u   = Leo("She talks about you a lot you know")
g_mc_talks_too  = MLeo("She talks about you a lot too")
g_mc_good_way2  = MLeo("In a good way")
g_leo_love      = Leo("Always with so much love")
g_leo_gn        = Leo("Goodnight man.")
g_mc_gn_leo     = MLeo("Goodnight Leo")

# Pre-alloc IF leo_humiliation (final)
g_if_leo_h4     = ng()

# TRUE – cum mouth finale (y=0)
g_leo_one_thing = Leo("Oh and one more thing", y=0)
g_leo_pic14b    = Leo("", img=I_CUM_MOUTH, gal=1, y=0)
g_leo_insisted  = Leo("She insisted", y=0)
g_leo_condom_mn = Leo("Said the condom was for the main event", y=0)
g_leo_fair_game = Leo("Everything else was fair game", y=0)
g_mc_dots       = MLeo("...", y=0)
g_mc_i_see_cum  = MLeo("I can see that", y=0)
g_leo_sleep_ck  = Leo("Sleep well cuck", y=0)
g_end_nts_hum   = end(y=0)

# FALSE – clean ending (y=600)
g_end_nts       = end(y=600)

# =============================================================================
# NTR / CUCK PATH  (math_continue == true → TrueGUID of g_if_math)
# =============================================================================
g_l_up_today    = L("So what are you up to today")
g_mc_not_much   = ML("Not much....")
g_mc_resting    = ML("Just resting...")
g_l_okay_ntr    = L("Okay")

# Pre-alloc IF docter_medicine_denied
g_if_doctor     = ng()

# =============================================================================
# CUCK PATH  (docter_medicine_denied == false → FalseGUID of g_if_doctor)
# =============================================================================
g_l_tell_you    = L("I wanted to tell you something")
g_l_about_tnt   = L("About tonight")
g_mc_oh_q       = ML("Oh?")
g_l_bday        = L("It's Math's birthday")
g_mc_is_it      = ML("Is it?")
g_l_yeah_bday   = L("Yeah")
g_l_renting     = L("He's renting out a section at a club")
g_l_izzy_going  = L("Izzy and I are going")
g_l_sounds_fun  = L("Sounds fun right?")

g_c_cuck_bday   = mc2(LILY, [
    ("Of course. Sounds awesome!",                        ""),
    ("As long as you enjoy yourself... That's all I really care about", "")
])
# Port 0
g_l_best_baby   = L("You're the best baby", y=0)
g_l_knew_like   = L("I knew you would like the sound of it (:", y=0)
# Port 1
g_l_i_will2     = L("I will", y=600)
g_l_always_do   = L("I always do with Math hihi", y=600)

# MERGE cuck bday choice
g_l_getting_rdy = L("I'm going to start getting ready later")
g_l_send_pic    = L("I'll send you a picture before we leave")
g_mc_waiting2   = ML("I'll be waiting")
g_l_good_boy    = L("Good boy hihi (;")

# Evening – black dress pic
g_l_almost_rdy  = L("Okay almost ready", tl="That evening...")
g_l_outfit_ck2  = L("Outfit check")
g_l_pic15_ck    = L("", img=I_BLACK_DRESS, gal=1)
g_mc_incredible2= ML("You look incredible")
g_l_bday_look   = L("For Math's birthday I have to look good right")
g_l_hihi_ck     = L("hihi")
g_mc_lucky      = ML("He's a lucky man")
g_l_lucky_you   = L("You're a lucky man... but yeah. For tonight he is definitely going to get more lucky than you hihi")

# Math texts MC
g_ma_hey_ck     = MA("Hey cuck")
g_mc_happy_bd   = MMA("Oh hey... Happy birthday Math")
g_ma_thanks_ck  = MA("Thanks cuck")
g_ma_pic17      = MA("", img=I_MATH_BOOTH)
g_ma_great_nite = MA("She's going to have a great night")
g_ma_ill_make   = MA("I'll make sure of it")
g_mc_look_after = MMA("Look after her")
g_ma_know_i_wll = MA("You know I will")
g_ma_every_way  = MA("In every way (;")

# 1 hour later at club
g_l_were_here   = L("We're here", tl="1 hour later...")
g_l_pic18_ck    = L("", img=I_CLUB_ENT, gal=1)
g_l_amazing_pl  = L("This place is amazing")
g_l_so_many     = L("So many people")
g_l_math_frnd   = L("Math's friends are all so nice")
g_l_whole_booth = L("He has a whole booth for us")
g_mc_drink_q    = ML("Are you drinking already")
g_l_maybe_lit   = L("Maybe a little hihi")
g_mc_have_fun   = ML("haha, have fun")

# 30 min later – Math texts dancing
g_ma_dance_flo  = MA("She's something else on a dancefloor", tl="30 minutes later...")
g_ma_just_know  = MA("Just so you know")
g_mc_dancing_q  = MMA("Is she dancing")
g_ma_with_me    = MA("With me")
g_ma_right_now  = MA("Right now... Izzy will send a pic")
g_i_pic19       = I_("", img=I_LILY_MTH_DNC, gal=1)
g_ma_great_bday = MA("Birthday is off to a great start")

g_c_math_dance  = mc2(MATH, [
    ("Don't hold back", ""),
    ("I can see that",  "")
])
# Port 0
g_ma_never_do   = MA("Never do", y=0)
# Port 1
g_ma_not_yet    = MA("You haven't seen anything yet", y=600)

# MERGE → spicy post dance video
g_mb_dance_post = MBB("", post=P_DNC_VID, vid=V_DANCE, thumb=V_DANCE_BNR, gal=1)

# 30 min later – blowjob
g_ma_interesting= MA("The night is getting interesting", tl="30 minutes later...")
g_ma_few_drinks = MA("Lily had a few drinks")
g_ma_good_mood  = MA("She and I are in a very good mood")
g_ma_standing   = MA("She's standing in front of me right now")
g_ma_waving     = MA("She's waving hi")
g_ma_pic23      = MA("", img=I_LILY_DRK_WAVE)
g_mc_hi_back    = MMA("Hi back")
g_ma_she_said   = MA("She also said to tell you")
g_ma_bday_prst  = MA("That she wants to give me a birthday present... A proper one")
g_ma_asked      = MA("I asked what kind")
g_ma_whispered  = MA("She whispered it in my ear")
g_mc_what_is    = MMA("What is it?")
g_ma_see_bit    = MA("You'll see in a bit...")
g_ma_keep_close = MA("Keep that phone close cuck")

# 5 min later – blowjob video
g_ma_vid2       = MA("", vid=V_BJ_MATH, thumb=V_BJ_BNR, gal=1, tl="5 minutes later...")
g_ma_bday_rec   = MA("Birthday present received")
g_ma_incredible3= MA("She's incredible")
g_mc_i_see2     = MMA("I can see that")
g_ma_wanted_snd = MA("She wanted me to send this one to you specifically")
g_ma_her_idea   = MA("Her idea")

# Spicy post deepthroat
g_mb_deepthrt   = MBB("", post=P_DEEPTHRT)

# 1 hour later
g_ma_booth_back = MA("Back at the booth now", tl="1 hour later...")
g_ma_she_looks  = MA("She looks amazing")
g_ma_pic30      = MA("", img=I_MATH_FUCK, gal=1)
g_mc_uh         = MMA("uh")
g_ma_haha_just  = MA("oh haha... that one was from just 10 minutes ago")
g_ma_pic31      = MA("", img=I_BOOTH_SELFIE, gal=1)
g_ma_head_back  = MA("I think we'll head back to mine soon")
g_mc_sleep_q    = MMA("Okay... Is she gonna sleep at your place?")
g_ma_of_course5 = MA("Of course")
g_ma_lot_more   = MA("We've got a lot more to do (;")

# Lily drunk text
g_l_hey_drunk   = L("Hey bwaby")
g_l_going_mat   = L("Going back to Mat")
g_l_text_tom    = L("I'll text you tommowom probably okys")
g_l_love_drunk  = L("I love yuo babe")
g_mc_love_drunk = ML("I love you too baby")

# 2 hours later – Math sends explicit pics
g_ma_tucked_in  = MA("She's all tucked in now", tl="2 hours later...")
g_ma_good_girl  = MA("She was a good girl")
g_ma_pic32      = MA("", img=I_LILY_ASS, gal=1)
g_ma_pic33      = MA("", img=I_LILY_CUM_COCK, gal=1)
g_ma_gn_cuck    = MA("Goodnight cuck")
g_mc_thanks_ck  = MMA("Thanks for taking such good care of her")
g_ma_wink       = MA("(;")
g_end_cuck      = end(y=0)

# =============================================================================
# SECRET NTR PATH  (docter_medicine_denied == true → TrueGUID of g_if_doctor)
# =============================================================================
g_l_izzy_going2 = L("Izzy and I are going out tonight by the way", y=600)
g_l_girls_night = L("Just a girls' night", y=600)
g_l_nothing_cz  = L("Nothing crazy", y=600)
g_mc_sounds_gd  = ML("Sounds good", y=600)
g_mc_anywhere   = ML("Anywhere specific", y=600)
g_l_club_found  = L("Just a club she found", y=600)
g_l_you_know_iz = L("You know Izzy", y=600)
g_l_always_find = L("She always finds somewhere", y=600)
g_l_outfits     = L("I'll send you the outfits we'll be wearing later okay...", y=600)
g_mc_have_fun2  = ML("Sure! Have fun...", y=600)
g_l_thanks_ntr  = L("Thanks baby", y=600)
g_l_text_later2 = L("I'll text you later", y=600)

# Evening – black dress
g_l_almost_rdy2 = L("Okay almost ready", tl="That evening...", y=600)
g_l_outfit_ck3  = L("Outfit check", y=600)
g_l_pic15_ntr   = L("", img=I_BLACK_DRESS, gal=1, y=600)
g_mc_wow_ntr    = ML("Wow.... you really dressed up", y=600)
g_l_thank_ntr   = L("Thank you baby... I did...", y=600)
g_l_izzy_made   = L("Izzy made me dress up like this", y=600)
g_l_her_dress   = L("This is her dress actually", y=600)
g_l_you_know    = L("You know how she is", y=600)
g_mc_i_do       = ML("I do... The dress looks amazing on you", y=600)
g_l_thankss     = L("Thanksss... Okay she's ready too", y=600)
g_l_look        = L("Look", y=600)
g_l_pic16_ntr   = L("", img=I_IZZY_RED, gal=1, y=600)

# Social post taxi
g_i_taxi_post   = I_("", post=P_TAXI, y=600)

# 1 hour later at club
g_l_were_here2  = L("We're here", tl="1 hour later...", y=600)
g_l_pic18_ntr   = L("", img=I_CLUB_ENT, gal=1, y=600)
g_l_amazing_pl2 = L("This place is amazing", y=600)
g_l_so_many2    = L("So many people", y=600)
g_l_izzy_bar2   = L("Izzy already found the bar hihi", y=600)
g_l_text_bit    = L("I'll text you in a bit okay", y=600)
g_mc_ok_fun     = ML("Okay have fun", y=600)
g_l_heart_ntr   = L("<3", y=600)

# Spicy still post
g_mb_still_post = MBB("", post=P_DNC_STILL, y=600)

# 30 min later – Lily drunk pic with hand
g_l_having_fun  = L("Having so much fun babe", tl="30 minutes later...", y=600)
g_l_this_place  = L("This place is incredible", y=600)
g_mc_glad       = ML("Glad to hear it", y=600)
g_mc_izzy_q     = ML("How's Izzy", y=600)
g_l_izzy_being  = L("Izzy is... being Izzy hihi", y=600)
g_l_talking_guy = L("She's already talking to some guy at the bar", y=600)
g_l_im_dancing  = L("I'm just dancing", y=600)
g_mc_good_nite  = ML("Sounds like a good night", y=600)
g_l_it_really   = L("It really is", y=600)
g_l_text_later3 = L("I'll text you later okay", y=600)
g_l_dont_stay   = L("Don't stay up too late", y=600)
g_i_pic21       = I_("", img=I_IZZY_BAR, gal=1, y=600)
g_i_pic22       = I_("", img=I_LILY_ALONE_DNC, gal=1, y=600)

# 1 hour later – drunk Lily with hand visible
g_l_hey_ntr     = L("Hey baby", tl="1 hour later...", y=600)
g_l_still_up    = L("Are you still up", y=600)
g_mc_im_up      = ML("I'm up yeah", y=600)
g_mc_hows_nite  = ML("How's the night going", y=600)
g_l_so_good2    = L("So good", y=600)
g_l_best_time   = L("We're having the best time", y=600)
g_l_bit_drunk   = L("I might be a little drunk hihi", y=600)
g_l_pic25       = L("", img=I_LILY_DRK_HND, gal=1, y=600)

g_c_ntr_hand    = mc2(LILY, [
    ("A little?",          ""),
    ("Who's hand is that?", "")
], y=600)
# Port 0 – a little
g_l_ok_more     = L("Okay maybe more than a little", y=600)
# Port 1 – whose hand
g_l_didnt_see   = L("Lol... I didn't even see that hand", y=600)
g_l_prob_walk   = L("Probably just someone who was walking by and needed support hihi", y=600)
g_l_getting_lat = L("It's getting later so people are getting pretty drunk", y=600)
g_mc_i_see3     = ML("I see", y=600)

# MERGE ntr_hand → Lily goes silent
g_l_izzy_eye    = L("Izzy is keeping an eye on me don't worry", y=600)
g_mc_good       = ML("Good", y=600)
g_mc_heading_bk = ML("Are you planning on heading back to the hotel soon?", y=600)

# 5 min later
g_mc_lily_there = ML("Are you there Lily?", tl="5 minutes later...", y=600)

# Spicy post blowjob NTR
g_mb_bj_ntr     = MBB("", post=P_BJ_NTR, y=600)

# MC texts Izzy
g_mc_hey_izzy   = MI_("Hey", y=600)
g_mc_still_lily = MI_("Is Lily still with you", y=600)
g_i_ofcourse    = I_("Ofcourse!", y=600)
g_i_pic27       = I_("", img=I_IZZY_BAR, y=600)
g_mc_not_see    = MI_("I don't see her in the pic", y=600)
g_i_oh_yea      = I_("Oh yea... she's pwobaby in the bathrom wit", y=600)
g_i_i_mean      = I_("I mean she is probably just in the bathroom", y=600)
g_mc_with_who   = MI_("With who?", y=600)
g_i_mistyped    = I_("I just miss typed", y=600)
g_mc_are_sure   = MI_("Are you sure?", y=600)
g_mc_more_ppl   = MI_("Are you guys with more people?", y=600)

# 5 min later – Izzy shows lily with cum on face
g_i_here_she    = I_("Here she is", tl="5 minutes later...", y=600)
g_i_pic28       = I_("", img=I_IZZY_LILY_CUM, gal=1, y=600)

g_c_cum_face    = mc2(IZZY, [
    ("What's that on her face?", ""),
    ("Okay... great",            "")
], y=600)
# Port 0 – ask about cum
g_i_q_mark      = I_("?", y=600)
g_i_what_mean   = I_("what do you mwean", y=600)
g_mc_white_stuff= MI_("That white stuff on Lily's face", y=600)
g_i_oh_haha     = I_("Oh haha", y=600)
# wait for response
g_mc_2_min_what = MI_("So what is it... Hello?", tl="2 minutes later...", y=600)
g_i_its_from    = I_("Its from this", y=600)
g_i_pic29       = I_("", img=I_MARTINI, y=600)
g_i_she_just    = I_("She just took a sip before we took the pic hahahaha", y=600)
g_mc_i_see4     = MI_("I see", y=600)
# Port 1 – okay great (merges below)

# MERGE → thanks Izzy
g_mc_thanks_iz  = MI_("Thanks Izzy", y=600)

# 1 hour later – Lily taxi, Izzy video
g_l_taxi_home   = L("Hey I'm in the taxi heading back now", tl="1 hour later...", y=600)
g_l_izzy_stay   = L("Izzy is staying out a bit longer", y=600)
g_l_met_someone = L("She met someone hihi", y=600)
g_mc_of_course6 = ML("Of course she did", y=600)
g_l_hihi2       = L("hihi", y=600)
g_l_pic34       = L("", img=I_LILY_TAXI, gal=1, y=600)
g_l_almost_htl  = L("I'm almost at the hotel", y=600)
g_l_good_night2 = L("It was such a good night", y=600)

g_c_ntr_taxi    = mc2(LILY, [
    ("Why are you sitting in the front of the taxi?", ""),
    ("I'm glad",                                       "")
], y=600)
# Port 0 – front seat question
g_l_oh_uh       = L("Oh... uh", y=600)
g_l_door_handle = L("The door handle was broken", y=600)
g_mc_both_sides = ML("On both sides of the taxi?", y=600)
g_l_yeah_strange= L("Yeah it's really strange", y=600)
g_l_gotta_go    = L("Gotta go now... I'll text you", y=600)
# Port 1 – I'm glad
g_l_love_baby3  = L("Love you baby", y=600)

# MERGE → spicy post fucked outside
g_mb_fucked_car = MBB("", img=I_LILY_FUCK_CAR, gal=1, y=600)

# 30 min later – Izzy check
g_mc_are_back   = MI_("Are you back Lily", tl="30 minutes later...", y=600)
g_mc_at_hotel   = MI_("Are you at the hotel already?", y=600)
g_mc_izzy_reach = MI_("Izzy... Can you reach Lily?", y=600)
g_mc_she_said_tx= MI_("She said she took a taxi and went to the hotel", y=600)
g_mc_been_30    = MI_("But it's been 30 minutes", y=600)
g_i_busy        = I_("busy", y=600)

# Spicy post balls licking
g_mb_balls      = MBB("", post=P_BALLS, y=600)

g_end_ntr       = end(y=600)

# =============================================================================
# WIRE LINKS
# =============================================================================

# Opening chain
chain(g_start,
      g_l_gm, g_l_sleep, g_mc_fine, g_l_well, g_mc_imagine)

# IF izzy_eat_out
g_if_izzy_node = ifn(VAR_IZZY_HEAD, g_l_unexpected, g_mc_busy,
                     preset_guid=g_if_izzy, y=0)
links.append((g_mc_imagine, g_if_izzy_node))

# TRUE branch
chain(g_l_unexpected, g_l_didnt, g_mc_wild, g_mc_izzy_wild,
      g_l_true, g_mc_where, g_l_run, g_mc_run_q, g_l_liked_it)
links.append((g_l_liked_it, g_izzy_run_post))

# FALSE branch
chain(g_mc_busy, g_l_i_did)
links.append((g_l_i_did, g_izzy_run_post))

# MERGE → IF math_continue
g_if_math_node = ifn(VAR_MATH_CONT, g_l_up_today, g_l_but_more,
                     preset_guid=g_if_math, y=0)
links.append((g_izzy_run_post, g_if_math_node))

# ── NTS PATH ─────────────────────────────────────────────────────────────────
chain(g_l_but_more, g_l_call_early, g_mc_leo_guess, g_l_who_else,
      g_mc_what_say, g_l_class_thing, g_l_model_multi, g_mc_remember,
      g_l_spot, g_l_today_today, g_mc_soon, g_l_i_know,
      g_l_cancell, g_l_had_think, g_mc_and, g_l_texting_u,
      g_l_what_think, g_l_want_do_it)
links.append((g_l_want_do_it, g_c_encourage))

# Choice: go for it
links.append((g_c_encourage, g_l_yeah_ok))
chain(g_l_yeah_ok, g_l_okay_too, g_l_heart_fast)
links.append((g_l_heart_fast, g_l_okay_text))

# Choice: only if ready
links.append((g_c_encourage, g_l_think_am))
chain(g_l_think_am, g_l_gondola, g_l_room_full, g_mc_fair)
links.append((g_mc_fair, g_l_okay_text))

# MERGE → leo picture section
chain(g_l_okay_text, g_l_going_text, g_l_keep_upd, g_l_pic_also)

# IF leo_humiliation #1
g_if_leo_h1_node = ifn(VAR_LEO_HUM, g_l_leo_decides, g_l_ok_with,
                       preset_guid=g_if_leo_h1, y=0)
links.append((g_l_pic_also, g_if_leo_h1_node))

# TRUE h1
chain(g_l_leo_decides, g_mc_ok_h1)
links.append((g_mc_ok_h1, g_l_love_baby))
# FALSE h1
chain(g_l_ok_with, g_mc_of_course, g_l_make_sure)
links.append((g_l_make_sure, g_l_love_baby))

chain(g_l_love_baby, g_mc_love_too,
      g_l_its_on, g_l_picking_up, g_l_freaking, g_mc_you_got, g_l_thanks1)

# IF leo_humiliation #2
g_if_leo_h2_node = ifn(VAR_LEO_HUM, g_l_sent_pic, g_l_this_pic,
                       preset_guid=g_if_leo_h2, y=0)
links.append((g_l_thanks1, g_if_leo_h2_node))

# TRUE h2 – dickpic sequence
chain(g_l_sent_pic, g_mc_allowed, g_l_no_hihi, g_mc_oh, g_l_but_excl,
      g_l_leo_sent, g_l_see_if_want, g_mc_what_pic, g_l_secret,
      g_mc_secret_q, g_l_yes_no)
links.append((g_l_yes_no, g_c_dickpic))
# No dickpic port
links.append((g_c_dickpic, g_l_jail))
chain(g_l_jail, g_mc_oh_no, g_l_not_like, g_l_right_ch)
links.append((g_l_right_ch, g_l_get_ready))
# Yes dickpic port
links.append((g_c_dickpic, g_l_pic1))
chain(g_l_pic1, g_mc_wow, g_l_yea_cock)
links.append((g_l_yea_cock, g_l_get_ready))

# FALSE h2 – show breast pic
chain(g_l_this_pic, g_l_pic2, g_mc_amazing, g_mc_did_like, g_l_loved)
links.append((g_l_loved, g_l_get_ready))

# MERGE h2 → getting ready
chain(g_l_get_ready, g_l_text_pick, g_mc_waiting, g_l_heart_3,
      g_l_pic3)

# Leo texts section
chain(g_l_pic3, g_leo_hey_dude, g_leo_getting, g_mc_multiple,
      g_leo_breaks, g_leo_keep_clo, g_leo_take_off, g_mc_i_see,
      g_mc_nervous, g_leo_little, g_leo_seen, g_mc_people,
      g_leo_5, g_mc_interest, g_leo_4long, g_leo_older,
      g_leo_new_guy, g_mc_lily_age, g_leo_could)

# Lily outfit check
chain(g_leo_could, g_l_before_in, g_l_outfit_ck, g_l_leo_said,
      g_l_first_sess, g_l_pic4, g_l_glamorous)
links.append((g_l_glamorous, g_c_robe))

# Robe choice
links.append((g_c_robe, g_l_thank_baby))
chain(g_l_thank_baby, g_l_helps)
links.append((g_l_helps, g_l_peek))

links.append((g_c_robe, g_l_going_for))
chain(g_l_going_for, g_l_easy_rem, g_mc_thinking, g_mc_removing,
      g_l_see_first, g_mc_wouldnt, g_l_of_course2)
links.append((g_l_of_course2, g_l_peek))

# Peek scene
chain(g_l_peek, g_l_oh_my_god, g_mc_what, g_l_babe,
      g_l_3_women, g_l_older_man, g_l_serious, g_l_easels,
      g_mc_and2, g_l_and3, g_l_one_more, g_mc_young_g,
      g_l_you_knew, g_mc_leo_ment, g_l_of_course3,
      g_l_our_age, g_l_not_bad, g_l_setting_up, g_l_pulled_back,
      g_l_dont_think, g_mc_he_def, g_l_he_didnt, g_l_doubt,
      g_l_think_look, g_mc_emoji_hah, g_mc_i_mean, g_l_no,
      g_l_just_wanted, g_mc_sureeee, g_l_omgstop,
      g_l_leo_waving, g_l_start, g_mc_you_got2)

# 45 min later – Leo
chain(g_mc_you_got2,
      g_leo_done, g_leo_something, g_leo_silent, g_leo_never,
      g_mc_handle, g_leo_owned, g_leo_break_15, g_leo_pic5,
      g_leo_text_you, g_leo_wanting)

# Lily back room chat
chain(g_leo_wanting,
      g_l_back_room, g_l_oh_god2, g_mc_tell_me, g_l_so_quiet,
      g_l_charcoal, g_l_everyone, g_l_older_focus, g_l_but_guy,
      g_mc_about_him, g_l_looked_up, g_l_more_others, g_l_every_time,
      g_mc_react, g_l_maybe_tiny, g_l_leo_noticed, g_l_smile_bk,
      g_l_die, g_mc_good_way, g_l_best_way, g_l_ok_so,
      g_l_leo_choice, g_l_want_know, g_mc_options,
      g_l_option1, g_l_keep_bra, g_l_already_lot,
      g_l_or_opt2, g_l_go_bare, g_l_everything, g_l_let_them,
      g_mc_your_call, g_l_want_u_1st, g_l_tell_u_hihi)
links.append((g_l_tell_u_hihi, g_c_undress))

# Undress TRUE path
links.append((g_c_undress, g_l_all_q))
chain(g_l_all_q, g_mc_all_2, g_l_dots_ok, g_l_ok_heard,
      g_l_wanted_hear, g_l_i_think2, g_l_30_sec, g_l_pic6,
      g_l_ok_ready, g_mc_amazing_t, g_l_tell_again,
      g_l_love_t, g_mc_love_t,
      g_leo_walked, g_leo_breathing, g_leo_5_sec, g_leo_incl_me,
      g_leo_pic8, g_leo_commit)
links.append((g_leo_commit, g_leo_good_sess))

# Undress FALSE path
links.append((g_c_undress, g_l_yeah_q))
chain(g_l_yeah_q, g_l_lean_that, g_l_baby_steps, g_mc_already_ic,
      g_l_youre_sweet, g_l_pic7, g_l_terrifying, g_l_exciting,
      g_mc_stunning, g_l_going_back, g_l_love_f, g_mc_love_f,
      g_leo_shifted, g_leo_pic9, g_leo_students)
links.append((g_leo_students, g_leo_good_sess))

# MERGE after undress
links.append((g_leo_good_sess, g_if_leo_h3))

# IF leo_humiliation #3
g_if_leo_h3_node = ifn(VAR_LEO_HUM, g_leo_fill_her, g_leo_h3_false,
                       preset_guid=g_if_leo_h3, y=0)
links.append((g_leo_fill_her, g_l_its_done))
links.append((g_leo_h3_false, g_l_its_done))

# 40 min later – session done
chain(g_l_its_done, g_l_second_over, g_l_packing, g_l_in_back2,
      g_mc_feeling, g_l_honestly, g_l_amazing, g_l_weird_amaz,
      g_l_eye_contact, g_l_by_accident, g_l_not_entire,
      g_l_didnt_look, g_l_held_it, g_l_eyebrow, g_l_bite_cheek,
      g_mc_say_after, g_l_leo_q, g_mc_the_guy, g_l_oh,
      g_l_yes_act, g_l_came_up, g_l_polite, g_l_first_class,
      g_l_glad_came, g_l_gave_num, g_l_sketch_pap, g_l_tore_off,
      g_l_most_normal, g_mc_bold, g_l_right_q, g_l_just_took,
      g_l_standing, g_l_what_do)
links.append((g_l_what_do, g_c_number))

# Number choice
links.append((g_c_number, g_evt_stu_t))
chain(g_evt_stu_t, g_l_yeah_keep, g_mc_dont_have, g_mc_but_keep,
      g_l_okay_keep, g_l_pic10, g_l_ill_keep)
links.append((g_l_ill_keep, g_l_leo_knocked))

links.append((g_c_number, g_evt_stu_f))
chain(g_evt_stu_f, g_l_really_q, g_l_you_sure, g_mc_im_sure,
      g_l_pic11, g_l_done_burn, g_mc_crying, g_l_ok_wont)
links.append((g_l_ok_wont, g_l_leo_knocked))

# Leo knocks
chain(g_l_leo_knocked, g_l_all_gone, g_l_just_us, g_mc_how_feel,
      g_l_quiet, g_l_good_quiet, g_l_pouring, g_mc_of_course4,
      g_mc_rude, g_l_right, g_l_text_later, g_l_love_so)
links.append((g_l_love_so, g_c_goodbye))

# Goodbye choice
links.append((g_c_goodbye, g_l_i_will))
chain(g_l_i_will, g_l_heart_have)
links.append((g_l_heart_have, g_leo_post_img))

links.append((g_c_goodbye, g_l_every_det))
chain(g_l_every_det, g_l_promise, g_l_heart_prom)
links.append((g_l_heart_prom, g_leo_post_img))

# 2 hours later – bed scene
chain(g_leo_post_img,
      g_l_hey_baby, g_l_in_bed, g_l_pass_out, g_mc_feeling2,
      g_l_so_good, g_l_tired_best, g_l_we_had_sex, g_mc_tell_me2,
      g_l_really_nice, g_l_so_gentle, g_l_took_time, g_l_condom,
      g_l_pic13, g_l_i_took, g_l_he_took, g_l_vid1,
      g_l_came_twice, g_l_not_expect, g_mc_incredible,
      g_mc_so_happy, g_l_i_am, g_l_anyway, g_l_really_tired,
      g_l_love_baby2, g_l_thank_being, g_l_i_mean_it,
      g_l_goodnight, g_mc_gn_love, g_l_heart_gn, g_l_pic14)

# Leo thank you
chain(g_l_pic14,
      g_leo_hey_ty, g_leo_thank_you, g_leo_genuinely,
      g_mc_no_need, g_leo_special, g_leo_not_many,
      g_leo_talks_u, g_mc_talks_too, g_mc_good_way2,
      g_leo_love, g_leo_gn, g_mc_gn_leo)

# IF leo_humiliation #4
g_if_leo_h4_node = ifn(VAR_LEO_HUM, g_leo_one_thing, g_end_nts,
                       preset_guid=g_if_leo_h4, y=0)
links.append((g_mc_gn_leo, g_if_leo_h4_node))

# TRUE – humiliation finale
chain(g_leo_one_thing, g_leo_pic14b, g_leo_insisted,
      g_leo_condom_mn, g_leo_fair_game, g_mc_dots,
      g_mc_i_see_cum, g_leo_sleep_ck)
links.append((g_leo_sleep_ck, g_end_nts_hum))

# ── NTR / CUCK PATH ───────────────────────────────────────────────────────────
chain(g_l_up_today, g_mc_not_much, g_mc_resting, g_l_okay_ntr)

# IF docter_medicine_denied
g_if_doctor_node = ifn(VAR_DOC_DENY, g_l_izzy_going2, g_l_tell_you,
                       preset_guid=g_if_doctor, y=0)
links.append((g_l_okay_ntr, g_if_doctor_node))

# ── CUCK PATH ─────────────────────────────────────────────────────────────────
chain(g_l_tell_you, g_l_about_tnt, g_mc_oh_q, g_l_bday,
      g_mc_is_it, g_l_yeah_bday, g_l_renting, g_l_izzy_going,
      g_l_sounds_fun)
links.append((g_l_sounds_fun, g_c_cuck_bday))

links.append((g_c_cuck_bday, g_l_best_baby))
chain(g_l_best_baby, g_l_knew_like)
links.append((g_l_knew_like, g_l_getting_rdy))

links.append((g_c_cuck_bday, g_l_i_will2))
chain(g_l_i_will2, g_l_always_do)
links.append((g_l_always_do, g_l_getting_rdy))

chain(g_l_getting_rdy, g_l_send_pic, g_mc_waiting2, g_l_good_boy,
      g_l_almost_rdy, g_l_outfit_ck2, g_l_pic15_ck,
      g_mc_incredible2, g_l_bday_look, g_l_hihi_ck,
      g_mc_lucky, g_l_lucky_you,
      g_ma_hey_ck, g_mc_happy_bd, g_ma_thanks_ck, g_ma_pic17,
      g_ma_great_nite, g_ma_ill_make, g_mc_look_after,
      g_ma_know_i_wll, g_ma_every_way,
      g_l_were_here, g_l_pic18_ck, g_l_amazing_pl, g_l_so_many,
      g_l_math_frnd, g_l_whole_booth, g_mc_drink_q,
      g_l_maybe_lit, g_mc_have_fun,
      g_ma_dance_flo, g_ma_just_know, g_mc_dancing_q,
      g_ma_with_me, g_ma_right_now, g_i_pic19, g_ma_great_bday)
links.append((g_ma_great_bday, g_c_math_dance))

links.append((g_c_math_dance, g_ma_never_do))
links.append((g_ma_never_do, g_mb_dance_post))
links.append((g_c_math_dance, g_ma_not_yet))
links.append((g_ma_not_yet, g_mb_dance_post))

chain(g_mb_dance_post,
      g_ma_interesting, g_ma_few_drinks, g_ma_good_mood,
      g_ma_standing, g_ma_waving, g_ma_pic23, g_mc_hi_back,
      g_ma_she_said, g_ma_bday_prst, g_ma_asked,
      g_ma_whispered, g_mc_what_is, g_ma_see_bit, g_ma_keep_close,
      g_ma_vid2, g_ma_bday_rec, g_ma_incredible3,
      g_mc_i_see2, g_ma_wanted_snd, g_ma_her_idea,
      g_mb_deepthrt,
      g_ma_booth_back, g_ma_she_looks, g_ma_pic30,
      g_mc_uh, g_ma_haha_just, g_ma_pic31,
      g_ma_head_back, g_mc_sleep_q, g_ma_of_course5, g_ma_lot_more,
      g_l_hey_drunk, g_l_going_mat, g_l_text_tom,
      g_l_love_drunk, g_mc_love_drunk,
      g_ma_tucked_in, g_ma_good_girl, g_ma_pic32, g_ma_pic33,
      g_ma_gn_cuck, g_mc_thanks_ck, g_ma_wink)
links.append((g_ma_wink, g_end_cuck))

# ── SECRET NTR PATH ───────────────────────────────────────────────────────────
chain(g_l_izzy_going2, g_l_girls_night, g_l_nothing_cz,
      g_mc_sounds_gd, g_mc_anywhere, g_l_club_found,
      g_l_you_know_iz, g_l_always_find, g_l_outfits,
      g_mc_have_fun2, g_l_thanks_ntr, g_l_text_later2,
      g_l_almost_rdy2, g_l_outfit_ck3, g_l_pic15_ntr,
      g_mc_wow_ntr, g_l_thank_ntr, g_l_izzy_made,
      g_l_her_dress, g_l_you_know, g_mc_i_do,
      g_l_thankss, g_l_look, g_l_pic16_ntr,
      g_i_taxi_post,
      g_l_were_here2, g_l_pic18_ntr, g_l_amazing_pl2, g_l_so_many2,
      g_l_izzy_bar2, g_l_text_bit, g_mc_ok_fun, g_l_heart_ntr,
      g_mb_still_post,
      g_l_having_fun, g_l_this_place, g_mc_glad, g_mc_izzy_q,
      g_l_izzy_being, g_l_talking_guy, g_l_im_dancing,
      g_mc_good_nite, g_l_it_really, g_l_text_later3,
      g_l_dont_stay, g_i_pic21, g_i_pic22,
      g_l_hey_ntr, g_l_still_up, g_mc_im_up, g_mc_hows_nite,
      g_l_so_good2, g_l_best_time, g_l_bit_drunk, g_l_pic25)
links.append((g_l_pic25, g_c_ntr_hand))

links.append((g_c_ntr_hand, g_l_ok_more))
links.append((g_l_ok_more, g_l_izzy_eye))

links.append((g_c_ntr_hand, g_l_didnt_see))
chain(g_l_didnt_see, g_l_prob_walk, g_l_getting_lat, g_mc_i_see3)
links.append((g_mc_i_see3, g_l_izzy_eye))

chain(g_l_izzy_eye, g_mc_good, g_mc_heading_bk, g_mc_lily_there,
      g_mb_bj_ntr,
      g_mc_hey_izzy, g_mc_still_lily, g_i_ofcourse,
      g_i_pic27, g_mc_not_see, g_i_oh_yea, g_i_i_mean,
      g_mc_with_who, g_i_mistyped, g_mc_are_sure, g_mc_more_ppl,
      g_i_here_she, g_i_pic28)
links.append((g_i_pic28, g_c_cum_face))

links.append((g_c_cum_face, g_i_q_mark))
chain(g_i_q_mark, g_i_what_mean, g_mc_white_stuff, g_i_oh_haha,
      g_mc_2_min_what, g_i_its_from, g_i_pic29, g_i_she_just,
      g_mc_i_see4)
links.append((g_mc_i_see4, g_mc_thanks_iz))

links.append((g_c_cum_face, g_mc_thanks_iz))

chain(g_mc_thanks_iz,
      g_l_taxi_home, g_l_izzy_stay, g_l_met_someone,
      g_mc_of_course6, g_l_hihi2, g_l_pic34,
      g_l_almost_htl, g_l_good_night2)
links.append((g_l_good_night2, g_c_ntr_taxi))

links.append((g_c_ntr_taxi, g_l_oh_uh))
chain(g_l_oh_uh, g_l_door_handle, g_mc_both_sides,
      g_l_yeah_strange, g_l_gotta_go)
links.append((g_l_gotta_go, g_mb_fucked_car))

links.append((g_c_ntr_taxi, g_l_love_baby3))
links.append((g_l_love_baby3, g_mb_fucked_car))

chain(g_mb_fucked_car,
      g_mc_are_back, g_mc_at_hotel,
      g_mc_izzy_reach, g_mc_she_said_tx, g_mc_been_30,
      g_i_busy, g_mb_balls)
links.append((g_mb_balls, g_end_ntr))

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

# Resolve InputGuid placeholders
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
    "  m_Name: Episode 23\n"
    "  m_EditorClassIdentifier: \n"
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

out_path = os.path.join(os.path.dirname(os.path.abspath(__file__)), "Episode 23.asset")
with open(out_path, "w", encoding="utf-8") as f:
    f.write(out)

print(f"Written {len(out):,} bytes -> {out_path}")
print(f"Nodes: {len(cnodes)} choice | {len(dnodes)} dialogue | "
      f"{len(enodes)} event | {len(ifnodes)} if | "
      f"{len(stnodes)} start | {len(endnodes)} end")
print(f"Links: {len(links)}")
