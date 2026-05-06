import uuid, os
from collections import defaultdict

def ng(): return str(uuid.uuid4())

# ── Characters ────────────────────────────────────────────────────────────────
DAVE = "14173166ace8e7b418d7ae1d1a0526b0"
LISA = "c9e8e94be1dbf9b4da1b2fd941c31ea2"
LILY = "44da74b3d308fdb4e9f9fdc127092247"

# ── Variable names (IF checks) ────────────────────────────────────────────────
VAR_TAG_ALONG = "mc_dave_lisa_tag_along_ep24"
VAR_LISA_KNOW = "Lisa_know_cuckhold"
VAR_DAVE_LILY = "Dave_Lily_path"
VAR_LILY_JOIN = "ep24.5_lily_join_lisa"
VAR_LILY_BLOW = "ep24.5_lily_dave_blowjob"

# ── Event GUIDs (variable setters) ───────────────────────────────────────────
EVT_LILY_JOIN_TRUE  = "6b1cf09a317d143e682fb09b740dc063"
EVT_LILY_JOIN_FALSE = "3aa2426503bfb4b76ae5cefc5f3d8f04"
EVT_LILY_BLOW_TRUE  = "64f2186053f754f54934d4bf7d7ab866"
EVT_LILY_BLOW_FALSE = "0221180345dcf4000b0ecdcfc225b516"

# ── Images ────────────────────────────────────────────────────────────────────
I_YACHT_MARINA      = "2c37cb464fb664575b63eb38904ac8ae"
I_LISA_SUNSET_CHAMP = "46daaba5d50004b9e82534d7862ff1a0"
I_DAVE_TUXEDO_LOST  = "e20e732c6c32b46fda561c022057086a"
I_FINE_DISH         = "ec98034637a3340c3ada5f6c720be062"
I_FINE_DESSERT      = "e7d9861668c30403aa2b9f17eaad12ac"
I_LILY_WHITE_WINE   = "8d40c59ac7be14958a75100dc0fd448d"
I_LILY_DESSERT      = "d62f791f25f3048bb9f9b87a26fc4f7e"
I_STRIPPER_POLE     = "bcac4ac4ed9d04f3ebc3b2b9326a8b4a"
I_MIA_POLE_LINGERIE = "2f4b244f35d80364b9438becf3d8d225"
I_MIA_ASS_POLE      = "d308102674634d443a7540ef465b160c"
I_MIA_LISA_DANCING  = "bc02263eb38064c5db61c4fa3f239a1a"
I_MIA_OVER_DAVE     = "a68c5dc579e314c8f8a4434326f6d169"
I_MIA_HEAD_DAVE     = "4769a08c9c40f5049a059b96732ede86"
I_LILY_NEXT_DAVE    = "5feb503472ee54a2284185b6355b40dd"
I_HUSBAND_CHAIR     = "ad4af748dccd80f40b5e11ff2f227459"
I_LISA_PURPLE_LING  = "81e018864ae4d644290cdf5fbf4ad162"
I_LISA_HAND_WAIST   = "d57c2ea53da507e469c3921ce35f2c20"
I_LISA_ASS_GRAB     = "0f8326458b6153e4b9720aaeabd024f3"
I_LISA_SELFIE_BLUSH = "87ffd6bc2fe7c934a86250b6a97d1072"
I_LISA_BLACK_LING   = "c0374a80cd8cc074286f63d878f9598c"
I_LISA_KNEES        = "851c652d810cb5143925b4e49b4eface"
I_MIA_DEEP_LEAN     = "ae3c4352bd98261419812621336cf28d"
I_LISA_BJ_SIDE      = "8d8e083ef0e83554fbe8e51e7b3a3d38"
I_LILY_BEHIND_KNEES = "e5c70f01f9e4baa46b48ed514b19dc01"
I_LILY_KISS_TIP     = "1d1ef73af69789146addda9c01f2197b"
I_LILY_DOORWAY      = "44ef01303f58ea842895528c1ab23f88"
I_LILY_HOLD_DAVE    = "be134802ad2bd2346a7192805efb222a"
I_DAVE_MARINA_MORN  = "20a8b43713d4641db9829f24d1a0eb32"

# ── Videos ────────────────────────────────────────────────────────────────────
V_MIA_POLE       = "d20832b0bf9dc3c4383239b5c9922507"
V_MIA_POLE_BNR   = "a4f96bda03ecbfa4d95ceee618fa1938"
V_LISA_DANCE     = "dec52c87e2bbc441a8957edc3063dd5b"
V_LISA_DANCE_BNR = "5e5d19beb7b9f419880ed9ceb4735170"
V_MIA_DEEP       = "850572726aaed5f4695046aa66ddaa65"
V_MIA_DEEP_BNR   = "6865d45a0c17dc545bf08d7ce1f62463"
V_MIA_LILY_KISS  = "43fe8899105cfd944afb8c44214b39cc"
V_MIA_LILY_BNR   = "444b7f2cdd5519642bd3888c244074ec"
V_LILY_BLOW_DAVE = "cc2b4bad37fe6f94c87a89987c798b9d"
V_LILY_BLOW_BNR  = "44d3176930fe7f04e96a91a9dacff2c7"

# ── Social Posts ──────────────────────────────────────────────────────────────
P_DAVE_MORNING = "34add98871d2744358b206cc2b3ffc07"

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
    img_f = f"{{fileID: 21300000, guid: {img}, type: 3}}"   if img   else "{fileID: 0}"
    vid_f = f"{{fileID: 32900000, guid: {vid}, type: 3}}"   if vid   else "{fileID: 0}"
    thm_f = f"{{fileID: 21300000, guid: {thumb}, type: 3}}" if thumb else "{fileID: 0}"
    pst_f = f"{{fileID: 11400000, guid: {post}, type: 2}}"  if post  else "{fileID: 0}"
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

# Shorthands
def D(text="",  **kw): return npc(DAVE, text, **kw)
def LS(text="", **kw): return npc(LISA, text, **kw)
def L(text="",  **kw): return npc(LILY, text, **kw)
def MD(txt, hint="", tl="", y=0):  return mc2(DAVE, [(txt, hint)], tl=tl, y=y)
def MLS(txt, hint="", tl="", y=0): return mc2(LISA, [(txt, hint)], tl=tl, y=y)
def ML(txt, hint="", tl="", y=0):  return mc2(LILY, [(txt, hint)], tl=tl, y=y)

# =============================================================================
# PRE-ALLOCATE IF GUIDs
# =============================================================================
g_if_tag        = ng()  # VAR_TAG_ALONG — main split
g_if_lk1        = ng()  # VAR_LISA_KNOW — lily intro
g_if_lk2        = ng()  # VAR_LISA_KNOW — lily dessert
g_if_lk_dave    = ng()  # VAR_LISA_KNOW — lily near Dave
g_if_dl_dave    = ng()  # VAR_DAVE_LILY — selfie near Dave
g_if_lk_mia_vid = ng()  # VAR_LISA_KNOW — Lily watching Mia video
g_if_lk_dl      = ng()  # VAR_LISA_KNOW — nested care choice outer
g_if_dl_inner   = ng()  # VAR_DAVE_LILY — nested care choice inner
g_if_lk_lily3   = ng()  # VAR_LISA_KNOW — lily section vs pushing head
g_if_dl_lily    = ng()  # VAR_DAVE_LILY — inside lily section
g_if_join       = ng()  # VAR_LILY_JOIN — join or doorway
g_if_lk_conv    = ng()  # VAR_LISA_KNOW — convergence ending

# =============================================================================
# NODE DEFINITIONS
# =============================================================================

g_start = start()

# ── NO YACHT PATH (y=3000) ───────────────────────────────────────────────────
g_dave_morning_post = D("", post=P_DAVE_MORNING, tl="The next morning...", y=3000)
g_end_no_yacht      = end(y=3000)

# ── YACHT PATH: Lisa intro (y=0) ─────────────────────────────────────────────
g_ls_we_at_marina   = LS("We are at the marina", tl="A little later...", y=0)
g_ls_oh_my_god      = LS("Oh my god", y=0)
g_ls_knew_money     = LS("I knew they had money but...", y=0)
g_mc_what           = MLS("What?", y=0)
g_ls_just_wait      = LS("Just wait", y=0)
g_ls_yacht_pic      = LS("", img=I_YACHT_MARINA, gal=1, y=0)
g_ls_ours           = LS("That's ours for the night", y=0)
g_mc_wow            = MLS("Wow", y=0)
g_ls_i_know         = LS("I know", y=0)
g_ls_hub_onboard    = LS("The husband is already on board", y=0)
g_ls_havent_seen    = LS("We haven't seen him yet", y=0)
g_ls_just_mia       = LS("Just Mia for now", y=0)
g_ls_incredible     = LS("She looks incredible by the way", y=0)
g_ls_white_dress    = LS("She's wearing this white red low cut summer dress that leaves nothing to the imagination", y=0)
g_ls_captain        = LS("The captain just welcomed us aboard", y=0)
g_ls_update1        = LS("I'll update you when we're out on the water", y=0)
g_mc_waiting        = MLS("I'll be waiting", y=0)
g_ls_moving         = LS("Okay we are moving", tl="30 minutes later...", y=0)
g_ls_20_min         = LS("The captain took us out about 20 minutes ago", y=0)
g_ls_sun_down       = LS("The sun is just going down", y=0)
g_ls_sunset_pic     = LS("", img=I_LISA_SUNSET_CHAMP, gal=1, y=0)
g_ls_used_to        = LS("I could get used to this", y=0)
g_mc_husband        = MLS("And the husband?", y=0)
g_ls_appeared       = LS("He appeared when we got on board", y=0)
g_ls_quiet          = LS("Quiet man... Very quiet", y=0)
g_ls_not_weird      = LS("But not in a weird way", y=0)
g_ls_commanding     = LS("In a... commanding way", y=0)
g_ls_shook_hand     = LS("He shook Dave's hand and that was basically it", y=0)
g_ls_mia_talks      = LS("Mia does all the talking", y=0)
g_mc_look_like      = MLS("What does he look like?", y=0)
g_mc_picture        = MLS("Do you have a picture?", y=0)
g_ls_let_me_see     = LS("Let me see what I can do...", y=0)
g_ls_tall           = LS("He is tall... very calm looking", tl="5 minutes later...", y=0)
g_ls_black_mid40    = LS("Black guy, probably mid 40s", y=0)
g_ls_used_to_get    = LS("He just looks like someone who is used to getting what he wants", y=0)
g_ls_if_makes       = LS("If that makes sense", y=0)
g_mc_i_see          = MLS("Oh... I see", y=0)
g_mc_setup          = MLS("What's the setup like on the boat?", y=0)
g_ls_main_deck      = LS("There's a main deck with a long dining table", y=0)
g_ls_back_room      = LS("And at the back there is a room with a stripper pole", y=0)
g_mc_of_course      = MLS("Of course there is", y=0)
g_ls_first_thing    = LS("It's the first thing Dave noticed", y=0)
g_ls_emoji1         = LS("\U0001f602", y=0)
g_ls_keeps_walking  = LS("He keeps walking past it", y=0)
g_ls_very_casual    = LS("Very casual", y=0)
g_mc_dave_overall   = MLS("How is Dave doing overall?", y=0)
g_ls_hes_good       = LS("He's good", y=0)
g_ls_trying_cool    = LS("Trying very hard to be cool", y=0)
g_ls_mandarin       = LS("The husband just said something in Mandarin to the butler", y=0)
g_ls_lost_puppy     = LS("Dave looked at me like a lost puppy", y=0)
g_ls_dave_pic       = LS("", img=I_DAVE_TUXEDO_LOST, gal=1, y=0)
g_ls_emoji2         = LS("\U0001f602", y=0)
g_ls_mia_transl     = LS("Mia translated", y=0)
g_ls_3_courses      = LS("3 courses but keep it light", y=0)
g_mc_wonder_light   = MLS("I wonder why he wants to keep it light", y=0)
g_ls_we_all_know    = LS("I think we all know why", y=0)
g_ls_wink           = LS("(;", y=0)
g_ls_butler         = LS("Butler hasn't let our glasses get below half full once by the way", y=0)
g_ls_on_it          = LS("He's on it", y=0)
g_mc_pro            = MLS("A pro", y=0)

# ── lisa_know FALSE (has lily) intro branch (y=600) ──────────────────────────
g_ls_lily_taking    = LS("Lily is taking it all in by the way", y=600)
g_ls_lily_wine_pic  = LS("", img=I_LILY_WHITE_WINE, gal=1, y=600)
g_ls_excited_ns     = LS("You can see she's excited but trying not to show it", y=600)
g_ls_hub_looked     = LS("The husband looked at her for a long time when we got on board", y=600)
g_mc_wonder_think   = MLS("I wonder what he was thinking...", y=600)
g_ls_same_ideas     = LS("Same... but I have some ideas", y=600)

# ── MERGE: dinner table (y=0) ─────────────────────────────────────────────────
g_ls_dinner_move    = LS("We are moving to the dinner table", tl="5 minutes later...", y=0)
g_ls_keep_updated   = LS("I'll keep you updated", y=0)
g_ls_dish_pic       = LS("", img=I_FINE_DISH, gal=1, tl="45 minutes later...", y=0)

# ── Dish choice ───────────────────────────────────────────────────────────────
g_c_dish = mc2(LISA, [("Looks tasty", ""), ("Looks tasty and well light indeed", "")], y=0)

# Port 0 (y=0)
g_ls_dish_0a = LS("It was so good", y=0)
g_ls_dish_0b = LS("To think they can just make this on their boat out in the ocean is crazy", y=0)
# Port 1 (y=600)
g_ls_dish_1a = LS("It does haha", y=600)
g_ls_dish_1b = LS("It was small but it tasted amazing", y=600)

# ── MERGE: dessert ────────────────────────────────────────────────────────────
g_ls_dessert_next   = LS("Small dessert is up next", y=0)
g_ls_dessert_pic    = LS("", img=I_FINE_DESSERT, gal=1, tl="30 minutes later...", y=0)
g_mc_amazing        = MLS("Amazing... and light again", y=0)
g_ls_so_good        = LS("Oh my god this was so good", y=0)

# ── lisa_know FALSE (has lily) dessert branch (y=600) ────────────────────────
g_ls_lily_dessert   = LS("", img=I_LILY_DESSERT, gal=1, y=600)
g_ls_lily_agrees    = LS("I think Lily agrees", y=600)

# ── MERGE: whats next ─────────────────────────────────────────────────────────
g_mc_whats_next     = MLS("What's next on the menu?", y=0)
g_ls_butler_plates  = LS("Well the butler just took all the plates and cleared the table", y=0)
g_ls_mia_left       = LS("Mia just left", y=0)
g_mc_left_what      = MLS("Left to do what?", y=0)
g_ls_dont_know      = LS("I don't know", y=0)
g_ls_i_know_now     = LS("I know now", tl="10 minutes later...", y=0)
g_mc_question1      = MLS("?", y=0)
g_ls_got_back       = LS("She just got back", y=0)
g_ls_different      = LS("Just a little different...", y=0)
g_ls_changed_ling   = LS("She changed into lingerie", y=0)
g_ls_stand_behind   = LS("Standing behind her husband now like she owns the place", y=0)
g_mc_oh             = MLS("Oh...", y=0)
g_ls_move_pole      = LS("She is telling us to move to the room with the pole...", y=0)
g_ls_i_think_know   = LS("I think I know what's going to happen", y=0)
g_ls_all_for_it     = LS("And I'm all for it", y=0)

# ── Dave chat: stripper pole ──────────────────────────────────────────────────
g_d_pole_pic        = D("", img=I_STRIPPER_POLE, gal=1, y=0)
g_mc_yep_pole       = MD("Yep... that's a stripper pole", y=0)
g_d_yea_yacht       = D("Yea! On a fucking yacht", y=0)
g_d_crazy           = D("Crazy right?", y=0)
g_mc_yea_d          = MD("Yea", y=0)
g_d_mia_chick       = D("And this Mia chick holy fuck", y=0)
g_d_gonna_show      = D("I think she's gonna give a show", y=0)
g_d_standing_pole   = D("She's standing next to the pole", y=0)

# ── lisa_know FALSE (has lily near Dave) branch (y=600) ──────────────────────
g_mc_lily_q         = MD("How is Lily doing", y=600)
g_d_lily_good1      = D("She's good", y=600)
# dave_lily FALSE (no selfie, y=600) — leads straight to merge
# dave_lily TRUE (selfie, y=1200)
g_d_sitting_next    = D("She's sitting next to me", y=1200)
g_d_lily_selfie_pic = D("", img=I_LILY_NEXT_DAVE, gal=1, y=1200)

# ── MERGE: getting started ────────────────────────────────────────────────────
g_d_getting_started = D("She's getting started", y=0)
g_d_mia_pole_pic    = D("", img=I_MIA_POLE_LINGERIE, gal=1, y=0)
g_mc_stunning       = MD("She looks stunning", y=0)
g_d_keep_updated2   = D("I'll keep you updated", y=0)

# ── Lisa: Mia dancing ─────────────────────────────────────────────────────────
g_ls_mia_vid        = LS("", vid=V_MIA_POLE, thumb=V_MIA_POLE_BNR, gal=1, tl="20 minutes later...", y=0)
g_mc_wow_mia        = MLS("Wow", y=0)
g_ls_no_words       = LS("I have no words", y=0)
g_ls_something_else = LS("She is something else", y=0)
g_ls_way_moves      = LS("The way she moves...", y=0)
g_ls_even_i         = LS("Even I can't stop watching", y=0)
g_ls_dave_forgot    = LS("And Dave... well he looks like he forgot his own name", y=0)
g_ls_danced_while   = LS("She's been dancing for a while now", tl="10 minutes later...", y=0)
g_ls_lost_top       = LS("She just lost her top", y=0)
g_ls_mia_ass_pic    = LS("", img=I_MIA_ASS_POLE, gal=1, y=0)
g_ls_walking_to     = LS("She's walking towards me", y=0)
g_mc_doing_what     = MLS("What is she doing?", y=0)
g_mc_lisa_q1        = MLS("Lisa?", y=0)

# ── Dave interlude ────────────────────────────────────────────────────────────
g_d_walked_up       = D("She just walked right up to Lisa...", tl="10 minutes later...", y=0)
g_d_fuck_dude       = D("Fuck dude...", y=0)
g_d_pulled_lisa     = D("She just pulled Lisa up", y=0)
g_d_dancing_pic     = D("", img=I_MIA_LISA_DANCING, gal=1, y=0)
g_mc_damn           = MD("Damn", y=0)
g_d_dots            = D("...", y=0)
g_mc_what_d1        = MD("What?", y=0)
g_d_walked_hub      = D("She just walked Lisa to her husband...", y=0)
g_d_dude            = D("Dude", y=0)
g_mc_lisa_doing     = MD("What's Lisa doing", y=0)
g_d_looked_back     = D("She just looked back to me and smiled", y=0)
g_d_not_touching    = D("He's not doing anything... not touching her...", y=0)
g_d_turned_finger   = D("He just turned his finger ordering her to do a spin...", y=0)
g_mc_is_she_spin    = MD("Is she doing it", y=0)
g_d_she_is          = D("She is...", y=0)
g_mc_wow_d          = MD("Wow", y=0)
g_d_havent_seen     = D("I haven't seen this before", y=0)
g_d_feels_weird     = D("It feels weird but...", y=0)
g_d_wait            = D("Wait...!", y=0)
g_d_hub_took_lisa   = D("The husband just stood up and took Lisa by her hand... they are going somewhere else", y=0)
g_d_mia_coming      = D("Mia is coming up to me", y=0)
g_d_mia_close_pic   = D("", img=I_MIA_OVER_DAVE, gal=1, y=0)
g_mc_god_hot        = MD("God she's hot", y=0)
g_d_no_doubt        = D("There's no doubt about that", y=0)
g_d_but_lisa        = D("But Lisa... I should text her...", y=0)
g_d_fuck1           = D("Fuck.....", y=0)
g_mc_what_d2        = MD("What?", y=0)
g_mc_what_dude      = MD("What dude?", y=0)
g_d_head_pic        = D("", img=I_MIA_HEAD_DAVE, gal=1, tl="10 minutes later...", y=0)
g_d_shes_crazy      = D("She's crazy", y=0)
g_d_actually_crazy  = D("Like actually crazy", y=0)
g_mc_where_lisa     = MD("Where did the husband take Lisa?", y=0)
g_d_below           = D("Below deck I think", y=0)
g_d_cant_really     = D("I can't really...", y=0)
g_d_focus           = D("Focus right now", y=0)

# ── lisa_know FALSE (Lily watching Mia, y=600) ────────────────────────────────
g_mc_lily_watch     = MD("What about Lily?", y=600)
g_d_lily_still      = D("She's...", y=600)
g_d_still_here      = D("Still here", y=600)
g_d_watching        = D("Watching", y=600)
g_d_i_think         = D("I think", y=600)
g_d_fuck_video      = D("FUCK", vid=V_MIA_DEEP, thumb=V_MIA_DEEP_BNR, gal=1, y=600)

# ── MERGE: Lisa below deck ────────────────────────────────────────────────────
g_ls_hey_bd         = LS("Hey", tl="20 minutes later...", y=0)
g_mc_where_bd       = MLS("Where did he take you?", y=0)
g_ls_room           = LS("He took me to a room...", y=0)
g_mc_a_room         = MLS("A room", y=0)
g_ls_below_deck     = LS("Yea, below deck", y=0)
g_ls_closet         = LS("It's more like a walk in closet", y=0)
g_ls_interesting    = LS("With... some interesting outfits...", y=0)
g_ls_also_chair     = LS("But there is also a chair", y=0)
g_mc_i_know_sit     = MLS("I think I know who is sitting there", y=0)
g_ls_chair_pic      = LS("", img=I_HUSBAND_CHAIR, gal=1, y=0)
g_ls_pick_outfits   = LS("He told me to pick out some outfits and wear them for him", y=0)
g_mc_oh_god         = MLS("Oh god", y=0)
g_ls_picked_two     = LS("I already picked two...", y=0)
g_ls_show_you       = LS("But before I show them to him I'll show them to you", y=0)
g_mc_not_complain   = MLS("I'm not going to complain", y=0)
g_ls_but_dave       = LS("But!", y=0)
g_ls_how_dave       = LS("How's Dave doing...?", y=0)
g_ls_texted_right   = LS("He probably texted you right?", y=0)
g_mc_taken_care     = MLS("He's being taken care of", y=0)
g_ls_hmm            = LS("Hmm...", y=0)
g_ls_cryptic        = LS("Cryptic... I like it", y=0)

# ── Nested IF: lisa_know && dave_lily → care choice ───────────────────────────
# lisa_know TRUE but dave_lily TRUE: care choice nodes (y=1200)
g_ls_lily_care      = LS("Is Lisa the one taking care of him?", y=1200)
g_ls_or_just_mia    = LS("Or is it just Mia", y=1200)
g_c_care = mc2(LISA, [("Just Mia...", ""), ("Just Mia for now", "")], y=1200)
# Port 0 (y=1200)
g_ls_sounds_boring  = LS("Hmmm... sounds boring", y=1200)
g_mc_not_boring     = MLS("She is definitely not boring", y=1200)
g_ls_im_sure        = LS("I'm sure she isn't", y=1200)
# Port 1 (y=1800)
g_ls_oh_for_now     = LS("Oh... for now... good boy", y=1800)

# ── MERGE: back below ─────────────────────────────────────────────────────────
g_ls_back_below     = LS("Now... back to the below deck adventures", y=0)
g_ls_impatient      = LS("I think he is getting impatient", y=0)
g_ls_throat         = LS("He just cleared his throat", y=0)
g_ls_purple_ling    = LS("", img=I_LISA_PURPLE_LING, gal=1, tl="5 minutes later...", y=0)
g_ls_look_good      = LS("Does it look good?", y=0)
g_mc_more_than_gd   = MLS("More than good", y=0)
g_ls_see_thinks     = LS("Let's see what he thinks", y=0)

# ── Lingerie choice ───────────────────────────────────────────────────────────
g_c_think = mc2(LISA, [("What does he think?", ""), ("Wait patiently", "")], y=0)
# Port 0 (y=0)
g_ls_hand_waist_pic = LS("", img=I_LISA_HAND_WAIST, gal=1, tl="5 minutes later...", y=0)
# Port 1 (y=600)
g_ls_likes_it       = LS("I think he likes it", tl="10 minutes later...", y=600)

# ── MERGE: ass grab ───────────────────────────────────────────────────────────
g_ls_ass_grab_pic   = LS("", img=I_LISA_ASS_GRAB, gal=1, y=0)
g_mc_i_think_does   = MLS("I think he does yes...", y=0)
g_ls_dance_him      = LS("He wants me to dance for him…", y=0)
g_ls_cant_dance     = LS("But I can't dance that well...", y=0)
g_mc_you_can_try    = MLS("You can try…", y=0)
g_ls_switch_outfit  = LS("First I'm going to switch to another set", y=0)
g_ls_cold_water     = LS("And get some cold water", y=0)
g_ls_selfie_blush   = LS("", img=I_LISA_SELFIE_BLUSH, gal=1, y=0)
g_mc_feel_hot       = MLS("Did he make you feel hot?", y=0)
g_ls_a_little       = LS("A little…", y=0)
g_ls_redress        = LS("Now let me redress", y=0)
g_ls_second_set     = LS("Okay second set", tl="5 minutes later...", y=0)
g_ls_before_show    = LS("Before I show him...", y=0)
g_ls_black_ling_pic = LS("", img=I_LISA_BLACK_LING, gal=1, y=0)
g_ls_what_think     = LS("What do you think?", y=0)
g_mc_even_better    = MLS("Even better", y=0)
g_ls_i_thought      = LS("I thought so", y=0)
g_ls_his_turn       = LS("Okay his turn now...", y=0)
g_ls_wish_luck      = LS("Wish me luck", y=0)
g_mc_no_need        = MLS("You don't need it", y=0)
g_ls_dance_vid      = LS("", vid=V_LISA_DANCE, thumb=V_LISA_DANCE_BNR, gal=1, tl="10 minutes later...", y=0)
g_ls_not_moved      = LS("He hasn't moved once", y=0)
g_ls_not_expr       = LS("Not even his expression", y=0)
g_ls_but_eyes       = LS("But his eyes...", y=0)
g_ls_eyes_talking   = LS("His eyes are doing a lot of talking", y=0)
g_mc_what_saying    = MLS("What are they saying?", y=0)
g_ls_everything     = LS("Everything", y=0)
g_ls_give_me        = LS("Give me a", y=0)
g_mc_q_mark         = MLS("?", y=0)
g_mc_lisa_q2        = MLS("Lisa?", tl="2 minutes later...", y=0)
g_mc_what_happened  = MLS("What happened?", y=0)

# ── Dave: 15 min check-in ────────────────────────────────────────────────────
g_d_bro_there       = D("Bro you there?", tl="15 minutes later...", y=0)
g_mc_im_here        = MD("I'm here", y=0)
g_d_heard_lisa      = D("Have you heard from Lisa?", y=0)
g_d_somewhere       = D("She went somewhere with him and I just...", y=0)
g_d_dont_know1      = D("I don't know", y=0)
g_mc_relax          = MD("Relax", y=0)
g_mc_lingerie       = MD("She was trying on some lingerie for him", y=0)
g_mc_last_heard     = MD("Last I heard", y=0)
g_d_oh              = D("Oh", y=0)
g_d_thats           = D("That's...", y=0)
g_d_okay_yea        = D("Okay yea that's fine", y=0)
g_d_can_handle      = D("That I can handle", y=0)
g_d_haha_d          = D("\U0001f602", y=0)
g_mc_how_doing_d    = MD("How are you doing?", y=0)
g_d_deep_pic        = D("", img=I_MIA_DEEP_LEAN, gal=1, y=0)
g_d_managing        = D("Managing", y=0)
g_mc_i_can_see      = MD("I can see that", y=0)
g_d_not_stopped     = D("She hasn't stopped once", y=0)
g_d_not_once        = D("Not once", y=0)
g_mc_crazy_skill    = MD("Crazy skill", y=0)
g_d_yea_skill       = D("Yea…", y=0)

# ── Lisa: 15 min reveal ───────────────────────────────────────────────────────
g_ls_hey2           = LS("Hey", tl="15 minutes later...", y=0)
g_ls_still_there    = LS("Still there?", y=0)
g_mc_im_here2       = MLS("I'm here!", y=0)
g_mc_what_happened2 = MLS("What happened?", y=0)
g_ls_so             = LS("So...", y=0)
g_ls_while_dancing  = LS("While I was dancing for him", y=0)
g_ls_he_just        = LS("He just...", y=0)
g_ls_reached        = LS("Reached into his pants", y=0)
g_ls_caught_off     = LS("It caught me a little off guard to be honest", y=0)
g_mc_can_imagine    = MLS("I can imagine", y=0)
g_mc_pull_it_out    = MLS("Did he.... pull it out?", y=0)
g_ls_he_did         = LS("He did...", y=0)
g_ls_like_nothing   = LS("Like it was nothing", y=0)
g_ls_pointed_floor  = LS("And then he pointed at the floor", y=0)
g_ls_just_looked    = LS("And just looked at me", y=0)
g_mc_and_ls         = MLS("And?", y=0)
g_ls_i_mean         = LS("I mean...", y=0)
g_ls_couldnt_say    = LS("I couldn't say no", y=0)
g_ls_knees_pic      = LS("", img=I_LISA_KNEES, gal=1, y=0)
g_mc_wow_ls         = MLS("Wow", y=0)
g_ls_impressive     = LS("He is very… Impressive", y=0)
g_ls_leave_it       = LS("I'll leave it at that", y=0)
g_ls_not_one_word   = LS("He still hasn't said a single word by the way", y=0)
g_ls_not_one        = LS("Not one", y=0)
g_mc_how_doing_ls   = MLS("How are you doing?", y=0)
g_ls_good_ls        = LS("I'm good", y=0)
g_ls_more_than_gd2  = LS("More than good", y=0)
g_ls_but2           = LS("But...", y=0)

# ── lisa_know FALSE (no lily): pushing head path (y=0) ───────────────────────
g_ls_pushing        = LS("… He's pushing my head", y=0)
g_ls_need_go        = LS("I need to go...", y=0)
g_ls_update_dave    = LS("Update Dave for me... thanks", y=0)
g_mc_okay_ls        = MLS("Okay", y=0)
g_mc_dude           = MD("Dude", tl="10 minutes later...", y=0)
g_d_what_d          = D("What?", y=0)
g_mc_lisa_bj        = MD("Lisa... she's giving him a blowjob below deck right now", y=0)
g_d_wait_what1      = D("...", y=0)
g_d_wait_what2      = D("Wait what", y=0)
g_mc_yep            = MD("Yep", y=0)
g_d_while_mia       = D("While I'm up here with Mia", y=0)
g_d_shes_down       = D("And she's down there with him?", y=0)
g_mc_thats_it       = MD("That's the situation yea", y=0)
g_d_i_d             = D("I...", y=0)
g_d_dont_know_feel  = D("I don't know how I feel about that", y=0)
g_d_like_genuinely  = D("Like genuinely", y=0)
g_d_diff_sep        = D("It's different when it's separate", y=0)
g_mc_i_get_that     = MD("I get that", y=0)
g_d_but_also        = D("But also...", y=0)
g_d_mia_here        = D("Mia is right here", y=0)
g_d_not_easy        = D("And she's not making it easy to think straight", y=0)
g_d_cry_laugh3      = D("\U0001f602", y=0)
g_d_process_later   = D("I'll process it later", y=0)
g_d_right_now       = D("Right now I just...", y=0)
g_d_cant            = D("Can't", y=0)
g_mc_go             = MD("Go", y=0)
g_d_yea_go          = D("Yea", y=0)
g_d_later_nk        = D("Later", y=0)

# ── lisa_know TRUE (has lily): lily section ───────────────────────────────────
g_ls_lily_upstairs  = LS("Lily is still upstairs", y=600)
g_ls_all_alone      = LS("All by herself", y=600)
g_ls_waste          = LS("That feels like a waste", y=600)
g_ls_maybe_text     = LS("Maybe you should text her... invite her down?", y=600)
g_ls_your_call      = LS("Your call", y=600)
g_mc_ill_text       = MLS("I'll text her", y=600)

# Lily chat
g_mc_hey_lily       = ML("Hey", tl="2 hours later...", y=600)
g_mc_how_doing_up   = ML("How are you doing up there?", y=600)
g_l_hey_l           = L("Hey", tl="2 minutes later...", y=600)
g_l_honestly        = L("Honestly?", y=600)
g_l_overwhelmed     = L("A little overwhelmed", y=600)
g_mc_good_way       = ML("In a good way?", y=600)
g_l_think_so        = L("I think so", y=600)
g_l_watching_mia    = L("Watching Mia and Dave is... hot", y=600)
g_l_demon           = L("She is a demon...", y=600)
g_l_soul_ex1        = L("And Dave...", y=600)
g_l_soul_ex2        = L("Dave is just getting his soul extracted", y=600)
g_l_laugh           = L("\U0001f602", y=600)
g_mc_and_you        = ML("And you?", y=600)
g_l_just_sitting    = L("I'm just sitting here", y=600)
g_l_taking_in       = L("Taking it all in", y=600)
g_l_what_lisa       = L("What is Lisa doing?", y=600)
g_mc_special_att    = ML("She's downstairs... Giving the husband some special attention?", y=600)
g_l_same_dave       = L("The same as Dave is getting!?", y=600)
g_mc_dave_knows     = ML("Dave doesn't know yet but.... yea", y=600)
g_l_oh_my           = L("Oh my... dirty Lisa hihi", y=600)

# ── dave_lily FALSE (TrueGUID): choice join or look (y=600) ──────────────────
g_c_lily_nodl = mc2(LILY, [
    ("You could join her if you want?", ""),
    ("You could go below deck and take a look...", "")
], y=600)
# Port 0: join=true (y=600)
g_evt_join_true_1   = evt(EVT_LILY_JOIN_TRUE, y=600)
g_l_join_join1      = L("Like join her join her?", y=600)
g_mc_yea_join1      = ML("Yea... helping her out", y=600)
g_l_okay_go1        = L("Okay...", y=600)
g_l_im_going1       = L("I'm going", y=600)
# Port 1: join=false (y=1200)
g_evt_join_false_1  = evt(EVT_LILY_JOIN_FALSE, y=1200)
g_l_can_do          = L("I can do that...", y=1200)
g_l_curious         = L("I'm kinda curious now... hihi", y=1200)
g_l_okay_look       = L("Okay", y=1200)
g_l_going_look      = L("I'm going", y=1200)

# ── LAND 1 / MERGE both lily choices (y=600) ─────────────────────────────────
g_l_almost          = L("I'm... here...", tl="5 minutes later...", y=600)
g_l_almost2         = L("Well almost", y=600)
g_l_can_hear        = L("I can hear them", y=600)
g_mc_what_hear      = ML("What can you hear", y=600)
g_l_sounds_wet      = L("It sounds... wet", y=600)
g_l_hear_suck       = L("I can hear her suck his dick", y=600)
g_mc_fuck_hot       = ML("Fuck that's hot", y=600)
g_mc_that_loud      = ML("Is she that loud", y=600)
g_l_not_far         = L("Well I'm not far away but yea", y=600)
g_l_hear_sloppy     = L("I can hear she's making it sloppy", y=600)
g_l_going_in        = L("I'm going into the room", y=600)
g_mc_keep_posted    = ML("Keep me up to date!", y=600)
g_l_i_will          = L("I will (;", y=600)

# ── lily_join FALSE (doorway, TrueGUID, y=1200) ───────────────────────────────
g_l_doorway         = L("I'm in the doorway", tl="5 minutes later...", y=1200)
g_l_oh_god_dw       = L("Oh my god", y=1200)
g_mc_what_dw        = ML("What?", y=1200)
g_l_lisa_going      = L("Lisa is really going for it", y=1200)
g_l_still_not_moved = L("He still hasn't moved once", y=1200)
g_l_his_hand        = L("Just his hand on her head", y=1200)
g_l_very_firm       = L("Very firm", y=1200)
g_l_just_taking     = L("And she's just taking it", y=1200)
g_mc_feel_watching  = ML("How does it feel watching?", y=1200)
g_l_strange         = L("Strange", y=1200)
g_l_cant_look_away  = L("But I can't look away", y=1200)
g_l_selfie_dw       = L("", img=I_LILY_DOORWAY, gal=1, y=1200)
g_l_oh_god_dw2      = L("Oh god", y=1200)
g_mc_what_dw2       = ML("What?", y=1200)
g_l_he_looked       = L("He just looked at me", y=1200)
g_mc_did_what       = ML("What did he do?", y=1200)
g_l_nothing_dw      = L("Nothing", y=1200)
g_l_just_looked2    = L("Just looked", y=1200)
g_l_then_back       = L("Then back to Lisa", y=1200)
g_l_not_consider    = L("Like I wasn't even a consideration", y=1200)
g_mc_show_me        = ML("Show me what you are seeing", y=1200)
g_l_suck_pic_dw     = L("", img=I_LISA_BJ_SIDE, gal=1, y=1200)
g_mc_fuck_dw        = ML("Fuck", y=1200)

# ── lily_join TRUE (entering, FalseGUID, y=600) ───────────────────────────────
g_l_in_room         = L("I'm in the room", tl="5 minutes later...", y=600)
g_l_oh_wow          = L("Oh wow", y=600)
g_l_he_is           = L("He is...", y=600)
g_mc_big            = ML("Big?", y=600)
g_l_very_big        = L("Very", y=600)
g_mc_what_lisa_do   = ML("What is Lisa doing?", y=600)
g_l_really_going1   = L("She's...", y=600)
g_l_really_going2   = L("Really going for it", y=600)
g_l_looked_up       = L("She looked up at me when I walked in", y=600)
g_l_just_smiled     = L("And just smiled", y=600)
g_l_taking_deep     = L("She's… taking him deep", y=600)
g_mc_could_same     = ML("Do you think you could do the same?", y=600)
g_l_not_sure        = L("I'm not sure", y=600)
g_l_def_try         = L("But I can definitely try", y=600)
g_l_give_bit        = L("Give me a bit...", y=600)
g_mc_keep_updated3  = ML("Keep me updated", y=600)
g_l_will_baby       = L("I will baby... don't worry", y=600)
g_l_this_view       = L("This is my view", y=600)
g_l_suck_pic_in     = L("", img=I_LISA_BJ_SIDE, gal=1, y=600)
g_mc_fuck_in        = ML("Fuck", y=600)

# Lisa response (after lily enters, y=600)
g_mc_happening_ls   = MLS("What's happening Lisa", tl="10 minutes later...", y=600)
g_ls_hmmm           = LS("Hmmm", y=600)
g_ls_interesting    = LS("Interesting stuff", y=600)
g_mc_come_on        = MLS("Come on... I need more than that", y=600)
g_ls_i_know_do      = LS("I know you do", y=600)
g_ls_ill_show       = LS("I'll show you…", y=600)
g_ls_lily_behind    = LS("", img=I_LILY_BEHIND_KNEES, gal=1, y=600)
g_mc_is_she_suck    = MLS("Is.. she", y=600)
g_mc_sucking        = MLS("Sucking him?", y=600)
g_ls_she_might      = LS("She might…", y=600)
g_ls_lily_tip_pic   = LS("", img=I_LILY_KISS_TIP, gal=1, y=600)
g_mc_fuck_ls2       = MLS("Fuck", y=600)

# ── dave_lily TRUE (FalseGUID): choice join Lisa or help Mia (y=1200) ─────────
g_c_lily_dl = mc2(LILY, [
    ("You could go downstairs and join Lisa?", ""),
    ("You could help Mia out a bit....", "")
], y=1200)
# Port 0: join Lisa (y=1200) → jump to LAND 1
g_evt_join_true_2   = evt(EVT_LILY_JOIN_TRUE, y=1200)
g_evt_blow_false_2  = evt(EVT_LILY_BLOW_FALSE, y=1200)
g_l_join_join2      = L("Like join her join her?", y=1200)
g_mc_yea_join2      = ML("Yea... helping her out", y=1200)
g_l_wow_sure        = L("Wow... are you sure", y=1200)
g_mc_fuck_yea       = ML("Fuck yea…", y=1200)
g_l_okay_dl2        = L("Okay...", y=1200)
g_l_im_going2       = L("I'm going…", y=1200)

# Port 1: help Mia (y=1800)
g_evt_blow_true     = evt(EVT_LILY_BLOW_TRUE, y=1800)
g_evt_join_false_2  = evt(EVT_LILY_JOIN_FALSE, y=1800)
g_l_like            = L("Like...", y=1800)
g_mc_yea_bl         = ML("Yea", y=1800)
g_l_oh_bl           = L("Oh...", y=1800)
g_l_but_bl          = L("But...", y=1800)
g_l_okay_bl1        = L("Okay", y=1800)
g_l_are_sure        = L("Are you sure… that this is what you want?", y=1800)
g_mc_yes_bl         = ML("Yes... please do it", y=1800)
g_l_okay_bl2        = L("Okay", y=1800)
# Dave chat
g_d_dude_bl         = D("Dude…", y=1800)
g_d_sure_q          = D("You are like actually fully sure about this?", tl="5 minutes later...", y=1800)
g_mc_yes_im         = MD("Yes... I am", y=1800)
g_d_okay_bl         = D("Okay...", y=1800)
g_d_saying_lily     = D("I'm saying that to Lily", y=1800)
g_d_kiss_vid        = D("", vid=V_MIA_LILY_KISS, thumb=V_MIA_LILY_BNR, gal=1, tl="5 minutes later...", y=1800)
g_mc_wow_kiss       = MD("Wow", y=1800)
g_d_yea_kiss        = D("Yea…", y=1800)
g_d_making_hard     = D("They're making me so fucking hard right now dude", y=1800)
g_mc_making_me      = MD("It's making me hard", y=1800)
g_d_cry_laugh2      = D("\U0001f602", y=1800)
g_d_fuck_bl         = D("Fuck", y=1800)
g_d_mia_moved       = D("Mia just moved to the side", y=1800)
g_d_lily_cock       = D("", img=I_LILY_HOLD_DAVE, gal=1, y=1800)
g_mc_fuck_bl        = MD("Fuck", y=1800)
g_mc_make_her       = MD("Make her take it", y=1800)
g_d_blow_vid        = D("", vid=V_LILY_BLOW_DAVE, thumb=V_LILY_BLOW_BNR, gal=1, tl="2 minutes later...", y=1800)
g_d_shes_doing      = D("She's… doing it dude", y=1800)
g_mc_fuck_bl2       = MD("Fuck", y=1800)

# ── CONVERGENCE: storm ────────────────────────────────────────────────────────
g_d_storm           = D("Bro", tl="30 minutes later...", y=0)
g_d_captain         = D("Captain just came down", y=0)
g_d_bad_weather     = D("Says we need to turn around", y=0)
g_d_bad_weather2    = D("Bad weather coming in fast", y=0)
g_mc_ok_d           = MD("Is everyone okay?", y=0)
g_d_yea_good        = D("Yea all good", y=0)
g_d_chaotic         = D("Just chaotic up here", y=0)
g_d_talk_back       = D("Talk when we're back", y=0)
g_ls_storm          = LS("Storm coming", y=0)
g_ls_turn_around    = LS("Captain is turning us around", y=0)
g_ls_talk_marina    = LS("Talk when we're at the marina", y=0)

# lisa_know FALSE (no lily): lisa ending (y=0)
g_mc_still_lisa     = MLS("Still there Lisa?", tl="2 minutes later...", y=0)
g_mc_hello_ls       = MLS("Hello?", y=0)

# lisa_know TRUE (has lily): lily scared (y=600)
g_l_hey_storm       = L("Hey", y=600)
g_l_rocking         = L("The boat is rocking really hard", y=600)
g_l_scared          = L("I'm a little scared", y=600)
g_mc_youre_ok       = ML("You're okay", y=600)
g_mc_turning_back   = ML("They're turning back now", y=600)
g_l_i_know_but      = L("I know but still…", y=600)
g_mc_ok_baby        = ML("It will be okay baby", y=600)
g_mc_still_there2   = ML("Are you still there?", tl="2 minutes later...", y=600)
g_mc_hello2         = ML("Hello?", y=600)

g_end = end(y=0)

# =============================================================================
# CONNECTIONS
# =============================================================================

# Start → IF tag_along
links.append((g_start, g_if_tag))
g_if_tag_node = ifn(VAR_TAG_ALONG, g_ls_we_at_marina, g_dave_morning_post,
                    preset_guid=g_if_tag, y=0)

# NO YACHT PATH
chain(g_dave_morning_post, g_end_no_yacht)

# YACHT: Lisa intro
chain(g_ls_we_at_marina, g_ls_oh_my_god, g_ls_knew_money, g_mc_what,
      g_ls_just_wait, g_ls_yacht_pic, g_ls_ours, g_mc_wow, g_ls_i_know,
      g_ls_hub_onboard, g_ls_havent_seen, g_ls_just_mia, g_ls_incredible,
      g_ls_white_dress, g_ls_captain, g_ls_update1, g_mc_waiting,
      g_ls_moving, g_ls_20_min, g_ls_sun_down, g_ls_sunset_pic,
      g_ls_used_to, g_mc_husband, g_ls_appeared, g_ls_quiet,
      g_ls_not_weird, g_ls_commanding, g_ls_shook_hand, g_ls_mia_talks,
      g_mc_look_like, g_mc_picture, g_ls_let_me_see,
      g_ls_tall, g_ls_black_mid40, g_ls_used_to_get, g_ls_if_makes,
      g_mc_i_see, g_mc_setup, g_ls_main_deck, g_ls_back_room,
      g_mc_of_course, g_ls_first_thing, g_ls_emoji1, g_ls_keeps_walking,
      g_ls_very_casual, g_mc_dave_overall, g_ls_hes_good, g_ls_trying_cool,
      g_ls_mandarin, g_ls_lost_puppy, g_ls_dave_pic, g_ls_emoji2,
      g_ls_mia_transl, g_ls_3_courses, g_mc_wonder_light,
      g_ls_we_all_know, g_ls_wink, g_ls_butler, g_ls_on_it, g_mc_pro)

# IF lisa_know #1 — lily intro
g_if_lk1_node = ifn(VAR_LISA_KNOW, g_ls_lily_taking, g_ls_dinner_move,
                    preset_guid=g_if_lk1, y=0)
links.append((g_mc_pro, g_if_lk1_node))
chain(g_ls_lily_taking, g_ls_lily_wine_pic, g_ls_excited_ns,
      g_ls_hub_looked, g_mc_wonder_think, g_ls_same_ideas, g_ls_dinner_move)

# Dinner
chain(g_ls_dinner_move, g_ls_keep_updated, g_ls_dish_pic)
links.append((g_ls_dish_pic, g_c_dish))
links.append((g_c_dish, g_ls_dish_0a))
links.append((g_c_dish, g_ls_dish_1a))
chain(g_ls_dish_0a, g_ls_dish_0b, g_ls_dessert_next)
chain(g_ls_dish_1a, g_ls_dish_1b, g_ls_dessert_next)
chain(g_ls_dessert_next, g_ls_dessert_pic, g_mc_amazing, g_ls_so_good)

# IF lisa_know #2 — lily dessert
g_if_lk2_node = ifn(VAR_LISA_KNOW, g_ls_lily_dessert, g_mc_whats_next,
                    preset_guid=g_if_lk2, y=0)
links.append((g_ls_so_good, g_if_lk2_node))
chain(g_ls_lily_dessert, g_ls_lily_agrees, g_mc_whats_next)

# Post-dinner → mia change
chain(g_mc_whats_next, g_ls_butler_plates, g_ls_mia_left,
      g_mc_left_what, g_ls_dont_know,
      g_ls_i_know_now, g_mc_question1, g_ls_got_back, g_ls_different,
      g_ls_changed_ling, g_ls_stand_behind, g_mc_oh, g_ls_move_pole,
      g_ls_i_think_know, g_ls_all_for_it)

# Dave: stripper pole
chain(g_ls_all_for_it, g_d_pole_pic, g_mc_yep_pole, g_d_yea_yacht,
      g_d_crazy, g_mc_yea_d, g_d_mia_chick, g_d_gonna_show, g_d_standing_pole)

# IF lisa_know #3 — lily near Dave
g_if_lk_dave_node = ifn(VAR_LISA_KNOW, g_mc_lily_q, g_d_getting_started,
                         preset_guid=g_if_lk_dave, y=0)
links.append((g_d_standing_pole, g_if_lk_dave_node))
chain(g_mc_lily_q, g_d_lily_good1)
g_if_dl_dave_node = ifn(VAR_DAVE_LILY, g_d_sitting_next, g_d_getting_started,
                         preset_guid=g_if_dl_dave, y=600)
links.append((g_d_lily_good1, g_if_dl_dave_node))
chain(g_d_sitting_next, g_d_lily_selfie_pic, g_d_getting_started)

chain(g_d_getting_started, g_d_mia_pole_pic, g_mc_stunning, g_d_keep_updated2)

# Lisa dancing
chain(g_d_keep_updated2, g_ls_mia_vid, g_mc_wow_mia, g_ls_no_words,
      g_ls_something_else, g_ls_way_moves, g_ls_even_i, g_ls_dave_forgot,
      g_ls_danced_while, g_ls_lost_top, g_ls_mia_ass_pic, g_ls_walking_to,
      g_mc_doing_what, g_mc_lisa_q1)

# Dave interlude
chain(g_mc_lisa_q1, g_d_walked_up, g_d_fuck_dude, g_d_pulled_lisa,
      g_d_dancing_pic, g_mc_damn, g_d_dots, g_mc_what_d1, g_d_walked_hub,
      g_d_dude, g_mc_lisa_doing, g_d_looked_back, g_d_not_touching,
      g_d_turned_finger, g_mc_is_she_spin, g_d_she_is, g_mc_wow_d,
      g_d_havent_seen, g_d_feels_weird, g_d_wait, g_d_hub_took_lisa,
      g_d_mia_coming, g_d_mia_close_pic, g_mc_god_hot, g_d_no_doubt,
      g_d_but_lisa, g_d_fuck1, g_mc_what_d2, g_mc_what_dude,
      g_d_head_pic, g_d_shes_crazy, g_d_actually_crazy,
      g_mc_where_lisa, g_d_below, g_d_cant_really, g_d_focus)

# IF lisa_know #4 — Lily watching Mia video
g_if_lk_mia_vid_node = ifn(VAR_LISA_KNOW, g_mc_lily_watch, g_ls_hey_bd,
                             preset_guid=g_if_lk_mia_vid, y=0)
links.append((g_d_focus, g_if_lk_mia_vid_node))
chain(g_mc_lily_watch, g_d_lily_still, g_d_still_here, g_d_watching,
      g_d_i_think, g_d_fuck_video, g_ls_hey_bd)

# Lisa below deck
chain(g_ls_hey_bd, g_mc_where_bd, g_ls_room, g_mc_a_room,
      g_ls_below_deck, g_ls_closet, g_ls_interesting, g_ls_also_chair,
      g_mc_i_know_sit, g_ls_chair_pic, g_ls_pick_outfits, g_mc_oh_god,
      g_ls_picked_two, g_ls_show_you, g_mc_not_complain,
      g_ls_but_dave, g_ls_how_dave, g_ls_texted_right, g_mc_taken_care,
      g_ls_hmm, g_ls_cryptic)

# Nested IF: lisa_know && dave_lily
g_if_lk_dl_node = ifn(VAR_LISA_KNOW, g_if_dl_inner, g_ls_back_below,
                       preset_guid=g_if_lk_dl, y=0)
links.append((g_ls_cryptic, g_if_lk_dl_node))
g_if_dl_inner_node = ifn(VAR_DAVE_LILY, g_ls_lily_care, g_ls_back_below,
                          preset_guid=g_if_dl_inner, y=600)
chain(g_ls_lily_care, g_ls_or_just_mia)
links.append((g_ls_or_just_mia, g_c_care))
links.append((g_c_care, g_ls_sounds_boring))
links.append((g_c_care, g_ls_oh_for_now))
chain(g_ls_sounds_boring, g_mc_not_boring, g_ls_im_sure, g_ls_back_below)
chain(g_ls_oh_for_now, g_ls_back_below)

# Back below → purple lingerie
chain(g_ls_back_below, g_ls_impatient, g_ls_throat,
      g_ls_purple_ling, g_ls_look_good, g_mc_more_than_gd, g_ls_see_thinks)
links.append((g_ls_see_thinks, g_c_think))
links.append((g_c_think, g_ls_hand_waist_pic))
links.append((g_c_think, g_ls_likes_it))
chain(g_ls_hand_waist_pic, g_ls_ass_grab_pic)
chain(g_ls_likes_it, g_ls_ass_grab_pic)

chain(g_ls_ass_grab_pic, g_mc_i_think_does,
      g_ls_dance_him, g_ls_cant_dance, g_mc_you_can_try,
      g_ls_switch_outfit, g_ls_cold_water, g_ls_selfie_blush,
      g_mc_feel_hot, g_ls_a_little, g_ls_redress,
      g_ls_second_set, g_ls_before_show, g_ls_black_ling_pic,
      g_ls_what_think, g_mc_even_better, g_ls_i_thought, g_ls_his_turn,
      g_ls_wish_luck, g_mc_no_need, g_ls_dance_vid,
      g_ls_not_moved, g_ls_not_expr, g_ls_but_eyes, g_ls_eyes_talking,
      g_mc_what_saying, g_ls_everything, g_ls_give_me, g_mc_q_mark,
      g_mc_lisa_q2, g_mc_what_happened)

# Dave 15 min
chain(g_mc_what_happened,
      g_d_bro_there, g_mc_im_here, g_d_heard_lisa,
      g_d_somewhere, g_d_dont_know1, g_mc_relax, g_mc_lingerie,
      g_mc_last_heard, g_d_oh, g_d_thats, g_d_okay_yea, g_d_can_handle,
      g_d_haha_d, g_mc_how_doing_d, g_d_deep_pic, g_d_managing,
      g_mc_i_can_see, g_d_not_stopped, g_d_not_once,
      g_mc_crazy_skill, g_d_yea_skill)

# Lisa 15 min reveal
chain(g_d_yea_skill,
      g_ls_hey2, g_ls_still_there, g_mc_im_here2, g_mc_what_happened2,
      g_ls_so, g_ls_while_dancing, g_ls_he_just, g_ls_reached,
      g_ls_caught_off, g_mc_can_imagine, g_mc_pull_it_out,
      g_ls_he_did, g_ls_like_nothing, g_ls_pointed_floor, g_ls_just_looked,
      g_mc_and_ls, g_ls_i_mean, g_ls_couldnt_say, g_ls_knees_pic,
      g_mc_wow_ls, g_ls_impressive, g_ls_leave_it, g_ls_not_one_word,
      g_ls_not_one, g_mc_how_doing_ls, g_ls_good_ls, g_ls_more_than_gd2,
      g_ls_but2)

# IF lisa_know #5 — lily section vs pushing head
g_if_lk_lily3_node = ifn(VAR_LISA_KNOW, g_ls_lily_upstairs, g_ls_pushing,
                           preset_guid=g_if_lk_lily3, y=0)
links.append((g_ls_but2, g_if_lk_lily3_node))

# lisa_know FALSE (no lily): push head → inform dave → convergence
chain(g_ls_pushing, g_ls_need_go, g_ls_update_dave, g_mc_okay_ls,
      g_mc_dude, g_d_what_d, g_mc_lisa_bj,
      g_d_wait_what1, g_d_wait_what2, g_mc_yep,
      g_d_while_mia, g_d_shes_down, g_mc_thats_it,
      g_d_i_d, g_d_dont_know_feel, g_d_like_genuinely, g_d_diff_sep,
      g_mc_i_get_that, g_d_but_also, g_d_mia_here, g_d_not_easy,
      g_d_cry_laugh3, g_d_process_later, g_d_right_now, g_d_cant,
      g_mc_go, g_d_yea_go, g_d_later_nk)
links.append((g_d_later_nk, g_d_storm))

# lisa_know TRUE (has lily): lily section
chain(g_ls_lily_upstairs, g_ls_all_alone, g_ls_waste, g_ls_maybe_text,
      g_ls_your_call, g_mc_ill_text,
      g_mc_hey_lily, g_mc_how_doing_up, g_l_hey_l, g_l_honestly,
      g_l_overwhelmed, g_mc_good_way, g_l_think_so, g_l_watching_mia,
      g_l_demon, g_l_soul_ex1, g_l_soul_ex2, g_l_laugh,
      g_mc_and_you, g_l_just_sitting, g_l_taking_in, g_l_what_lisa,
      g_mc_special_att, g_l_same_dave, g_mc_dave_knows, g_l_oh_my)

# IF dave_lily inside lily section
g_if_dl_lily_node = ifn(VAR_DAVE_LILY, g_c_lily_dl, g_c_lily_nodl,
                         preset_guid=g_if_dl_lily, y=600)
links.append((g_l_oh_my, g_if_dl_lily_node))

# dave_lily FALSE: choice join or look
links.append((g_c_lily_nodl, g_evt_join_true_1))
links.append((g_c_lily_nodl, g_evt_join_false_1))
chain(g_evt_join_true_1, g_l_join_join1, g_mc_yea_join1, g_l_okay_go1, g_l_im_going1)
chain(g_evt_join_false_1, g_l_can_do, g_l_curious, g_l_okay_look, g_l_going_look)
links.append((g_l_im_going1, g_l_almost))
links.append((g_l_going_look, g_l_almost))

# LAND 1 common section
chain(g_l_almost, g_l_almost2, g_l_can_hear, g_mc_what_hear,
      g_l_sounds_wet, g_l_hear_suck, g_mc_fuck_hot, g_mc_that_loud,
      g_l_not_far, g_l_hear_sloppy, g_l_going_in, g_mc_keep_posted, g_l_i_will)

# IF lily_join
g_if_join_node = ifn(VAR_LILY_JOIN, g_l_in_room, g_l_doorway,
                     preset_guid=g_if_join, y=600)
links.append((g_l_i_will, g_if_join_node))

# doorway path (join=false)
chain(g_l_doorway, g_l_oh_god_dw, g_mc_what_dw, g_l_lisa_going,
      g_l_still_not_moved, g_l_his_hand, g_l_very_firm, g_l_just_taking,
      g_mc_feel_watching, g_l_strange, g_l_cant_look_away,
      g_l_selfie_dw, g_l_oh_god_dw2, g_mc_what_dw2, g_l_he_looked,
      g_mc_did_what, g_l_nothing_dw, g_l_just_looked2, g_l_then_back,
      g_l_not_consider, g_mc_show_me, g_l_suck_pic_dw, g_mc_fuck_dw)
links.append((g_mc_fuck_dw, g_d_storm))

# entering path (join=true)
chain(g_l_in_room, g_l_oh_wow, g_l_he_is, g_mc_big, g_l_very_big,
      g_mc_what_lisa_do, g_l_really_going1, g_l_really_going2, g_l_looked_up,
      g_l_just_smiled, g_l_taking_deep, g_mc_could_same, g_l_not_sure,
      g_l_def_try, g_l_give_bit, g_mc_keep_updated3, g_l_will_baby,
      g_l_this_view, g_l_suck_pic_in, g_mc_fuck_in)
chain(g_mc_fuck_in, g_mc_happening_ls, g_ls_hmmm, g_ls_interesting,
      g_mc_come_on, g_ls_i_know_do, g_ls_ill_show, g_ls_lily_behind,
      g_mc_is_she_suck, g_mc_sucking, g_ls_she_might, g_ls_lily_tip_pic,
      g_mc_fuck_ls2)
links.append((g_mc_fuck_ls2, g_d_storm))

# dave_lily TRUE: join or help Mia
links.append((g_c_lily_dl, g_evt_join_true_2))
links.append((g_c_lily_dl, g_evt_blow_true))

# Port 0: join Lisa → jump to LAND 1
chain(g_evt_join_true_2, g_evt_blow_false_2,
      g_l_join_join2, g_mc_yea_join2, g_l_wow_sure,
      g_mc_fuck_yea, g_l_okay_dl2, g_l_im_going2)
links.append((g_l_im_going2, g_l_almost))

# Port 1: help Mia
chain(g_evt_blow_true, g_evt_join_false_2,
      g_l_like, g_mc_yea_bl, g_l_oh_bl, g_l_but_bl, g_l_okay_bl1,
      g_l_are_sure, g_mc_yes_bl, g_l_okay_bl2,
      g_d_dude_bl, g_d_sure_q, g_mc_yes_im, g_d_okay_bl, g_d_saying_lily,
      g_d_kiss_vid, g_mc_wow_kiss, g_d_yea_kiss, g_d_making_hard,
      g_mc_making_me, g_d_cry_laugh2, g_d_fuck_bl, g_d_mia_moved,
      g_d_lily_cock, g_mc_fuck_bl, g_mc_make_her,
      g_d_blow_vid, g_d_shes_doing, g_mc_fuck_bl2)
links.append((g_mc_fuck_bl2, g_d_storm))

# CONVERGENCE: storm
chain(g_d_storm, g_d_captain, g_d_bad_weather, g_d_bad_weather2,
      g_mc_ok_d, g_d_yea_good, g_d_chaotic, g_d_talk_back,
      g_ls_storm, g_ls_turn_around, g_ls_talk_marina)

g_if_lk_conv_node = ifn(VAR_LISA_KNOW, g_l_hey_storm, g_mc_still_lisa,
                         preset_guid=g_if_lk_conv, y=0)
links.append((g_ls_talk_marina, g_if_lk_conv_node))

# no lily ending
chain(g_mc_still_lisa, g_mc_hello_ls)
links.append((g_mc_hello_ls, g_end))

# has lily ending
chain(g_l_hey_storm, g_l_rocking, g_l_scared, g_mc_youre_ok,
      g_mc_turning_back, g_l_i_know_but, g_mc_ok_baby,
      g_mc_still_there2, g_mc_hello2)
links.append((g_mc_hello2, g_end))

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
    "  m_Name: Episode 24.5\n"
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

out_path = os.path.join(os.path.dirname(os.path.abspath(__file__)), "Episode 24.5.asset")
with open(out_path, "w", encoding="utf-8") as f:
    f.write(out)

print(f"Written {len(out):,} bytes -> {out_path}")
print(f"Nodes: {len(cnodes)} choice | {len(dnodes)} dialogue | "
      f"{len(enodes)} event | {len(ifnodes)} if | "
      f"{len(stnodes)} start | {len(endnodes)} end")
print(f"Links: {len(links)}")
