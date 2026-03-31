import uuid, sys

def ng(): return str(uuid.uuid4())

# ── External asset GUIDs ──────────────────────────────────────────────────────
FREYA = "5543b17ec0634114989a7ab5c88868d3"
LENNY = "4130a1b79fdcb784d81a7e8ef8976ce5"

EVT_CONF_ADD1  = "68b2a1d7ee9a7d0429b1538bcd03cbdb"  # freya_mc_confidence + 1
EVT_LENNY_ADD1 = "96bae97425445c746914c97fdb0f3788"  # lenny_aliance + 1
EVT_WIZ_TRUE   = "ed5c4a01d17fdd5478a2930f8c919962"  # wizard post goes live

# Social post assets
P_BALCONY   = "f2b768372a58b4346846a971e2af70fc"  # cha 2 - balcony picture
P_GOING_OUT = "0b4e0b8275ca31f42bdd4a7d125fb030"  # cha 2 - going out
P_BEER      = "31ccff9131f024940a9007d9b561004a"  # cha 2 - just a small beer...
# P_WIZARD: no social post asset created yet in Unity - add GUID when ready

# Sprites (Chapter 2 gallery)
S_BEDROOM  = "d0f4a26566dea7c4ba3fc7b612c2d311"  # cha 2 infront of bed (bedroom pose latex)
S_BALCONY_S= "d707a653765144e498979b6a8576e62b"  # cha 2 balcony
S_NAUGHTY  = "2e6d28b4960be2246aefcd354d3ec28d"  # cha 2 showing ass blue latex
S_TAME     = "52546218056357143b89d537a04cb451"  # cha 2 tame laying on stomach
S_ENERGY1  = "bc9576be530734f45941a43e347830bd"  # energy drink sponsor (smiling, pink)
S_ENERGY2  = "fa94f9b326fa107499304203c1bd64fc"  # energy drink tired (dead inside, green)
S_GOODNITE = "f6943e050d9f89946a86b5d8e8efc49d"  # laying in bed pyjama
S_LINGERIE = "9bf878b18ca4a2144870824fb6679601"  # dark blue lingerie on bed (reveal)
S_LNG_FRONT= "c536a6a0c0b5c9a44924e9d3bf55016e"  # blue lingerie front sitting on bed
S_LNG_BACK = "f10bccba444b63a4f8f4a14784b7b8e0"  # blue lingerie showing backside
S_LNG_SED  = "18ebec96a12bab94fadf10c40b740797"  # seductive close up blue lingerie
S_BATH_SEL = "0c841ab60963f1046b5abbf6e3f643a0"  # bathroom selfie pulling lingerie down
S_IMPLIED  = "878ad0fc13ddcee4d9f874c612765437"  # implied nude (arms covering chest)
S_NUDE     = "22f3d5d3fcd1f074d8a4b43d3d8d7c7d"  # topless
S_LNY_IMP  = "d15e5b0e0b7bb2746bd32c7acebf2e29"  # lenny model reference - implied
S_LNY_NUDE = "9a54227165df6cd48905ffffdda8180a"  # lenny model reference - topless
S_GOINGOUT = "8812c3caa06bd2f4eb862ac9251d2883"  # selfie going out dress
S_BAR      = "27245a7ba8895514fa2a608a2ff066bd"  # bar selfie with beer

# Video
V_DANCE      = "49568489424d95544b85c146612119bb"  # slow dance blue latex (video)
V_DANCE_THUMB= "02f7f3a95c919a0459b7ad37e0569a15"  # dance video thumbnail

# ── Node collections ──────────────────────────────────────────────────────────
links    = []
cnodes   = []
dnodes   = []
enodes   = []
ifnodes  = []
stnodes  = []
endnodes = []
port_pgs = {}
node_pos = {}

_x = [0]
def px():
    _x[0] += 350
    return _x[0]

def px_store(g):
    p = px()
    node_pos[g] = p
    return p

# ── YAML helpers ──────────────────────────────────────────────────────────────
def _safe(txt):
    return (txt or "").replace("\n", " ").replace("\r", " ")

def _l5(txt=""):
    txt = _safe(txt)
    rows = ""
    for i in range(5):
        v = txt if i == 0 else ""
        rows += f"    - languageEnum: {i}\n      LanguageGenericType: {v}\n"
    return rows

def _l5p(txt=""):
    txt = _safe(txt)
    rows = ""
    for i in range(5):
        if i == 0:   v = txt
        elif txt:    v = "Choice 01"
        else:        v = ""
        rows += f"      - languageEnum: {i}\n        LanguageGenericType: {v}\n"
    return rows

def _audio():
    rows = ""
    for i in range(5):
        rows += f"    - languageEnum: {i}\n      LanguageGenericType: {{fileID: 0}}\n"
    return rows

def _mdata(spr=None, vid=None, thumb=None, gal=0, indent="      "):
    if vid:
        mt   = 1
        mobj = f"{{fileID: 32900000, guid: {vid}, type: 3}}"
        ctmb = f"{{fileID: 21300000, guid: {thumb}, type: 3}}" if thumb else "{fileID: 0}"
    elif spr:
        mt   = 0
        mobj = f"{{fileID: 21300000, guid: {spr}, type: 3}}"
        ctmb = "{fileID: 0}"
    else:
        mt   = 0
        mobj = "{fileID: 0}"
        ctmb = "{fileID: 0}"
    i = indent
    return (f"{i}MediaType: {mt}\n{i}MediaObject: {mobj}\n"
            f"{i}CustomThumbnail: {ctmb}\n{i}NotBackgroundCapable: 0\n"
            f"{i}GalleryVisibility: {gal}")

# ── Node builders ─────────────────────────────────────────────────────────────
def npc(char, text="", timelapse="", spr=None, vid=None, thumb=None, post=None, gal=0, y=0):
    g = ng()
    post_ref = f"{{fileID: 11400000, guid: {post}, type: 2}}" if post else "{fileID: 0}"
    md = _mdata(spr, vid, thumb, gal, indent="      ")
    dnodes.append(
        f"  - NodeGuid: {g}\n"
        f"    Position: {{x: {px_store(g)}, y: {y}}}\n"
        f"    DialogueNodePorts: []\n"
        f"    AudioClips:\n{_audio()}"
        f"    Character: {{fileID: 11400000, guid: {char}, type: 2}}\n"
        f"    AvatarPos: 0\n    AvatarType: 0\n"
        f"    Texts:\n{_l5(text)}"
        f"    Timelapses:\n{_l5(timelapse)}"
        f"    Timelapse: \n    Duration: 2\n    Delay: 0\n"
        f"    MediaData:\n{md}\n"
        f"    Post: {post_ref}\n    DelayTimer: 0\n"
    )
    return g

def mc2(char, port_texts, npc_text="", port_sprs=None, port_vids=None,
        port_thumbs=None, port_gals=None, y=0):
    g = ng()
    if port_sprs is None:   port_sprs   = [None]*len(port_texts)
    if port_vids is None:   port_vids   = [None]*len(port_texts)
    if port_thumbs is None: port_thumbs = [None]*len(port_texts)
    if port_gals is None:   port_gals   = [0]*len(port_texts)
    ports_yaml = ""
    pgs = []
    for i, txt in enumerate(port_texts):
        pg = ng()
        pgs.append(pg)
        is_media = 1 if (port_sprs[i] or port_vids[i]) else 0
        md = _mdata(port_sprs[i], port_vids[i], port_thumbs[i], port_gals[i], "        ")
        ports_yaml += (
            f"    - PortGuid: {pg}\n"
            f"      InputGuid: __IGUID_{pg}__\n"
            f"      OutputGuid: {g}\n"
            f"      TextLanguage:\n{_l5p(txt)}"
            f"      HintLanguage:\n"
        )
        for j in range(5):
            ports_yaml += f"      - languageEnum: {j}\n        LanguageGenericType: \n"
        ports_yaml += f"      MediaData:\n{md}\n      IsMediaPort: {is_media}\n"
    port_pgs[g] = pgs
    req = 1 if _safe(npc_text) else 0
    cnodes.append(
        f"  - NodeGuid: {g}\n"
        f"    Position: {{x: {px_store(g)}, y: {y}}}\n"
        f"    DialogueNodePorts:\n{ports_yaml}"
        f"    AudioClips:\n{_audio()}"
        f"    Character: {{fileID: 11400000, guid: {char}, type: 2}}\n"
        f"    AvatarPos: 0\n    AvatarType: 0\n"
        f"    Texts:\n{_l5(npc_text)}"
        f"    Duration: 2\n"
        f"    RequireCharacterInput: {req}\n"
        f"    SelectedChoice: []\n    ChoiceIndex: 0\n"
    )
    return g

def evt(event_guid, y=0):
    g = ng()
    enodes.append(
        f"  - NodeGuid: {g}\n"
        f"    Position: {{x: {px_store(g)}, y: {y}}}\n"
        f"    EventScriptableObjects:\n"
        f"    - DialogueEventSO: {{fileID: 11400000, guid: {event_guid}, type: 2}}\n"
    )
    return g

def ifn(var, ops, val, true_guid, false_guid, preset_guid=None, after_guid=None, y=0):
    g = preset_guid if preset_guid else ng()
    links.append((g, true_guid))
    links.append((g, false_guid))
    _pos = (node_pos[after_guid] + 350) if (after_guid and after_guid in node_pos) else px()
    node_pos[g] = _pos
    ifnodes.append(
        f"  - NodeGuid: {g}\n"
        f"    Position: {{x: {_pos}, y: {y}}}\n"
        f"    ValueName: {var}\n"
        f"    Operations: {ops}\n"
        f"    OperationValue: {val}\n"
        f"    TrueGUID: {true_guid}\n"
        f"    FalseGUID: {false_guid}\n"
    )
    return g

def start():
    g = ng()
    stnodes.append(
        f"  - NodeGuid: {g}\n"
        f"    Position: {{x: {px_store(g)}, y: 0}}\n"
        f"    startID: \n"
    )
    return g

def end():
    g = ng()
    endnodes.append(
        f"  - NodeGuid: {g}\n"
        f"    Position: {{x: {px_store(g)}, y: 0}}\n"
        f"    EndNodeType: 0\n"
        f"    Dialogue: {{fileID: 0}}\n"
    )
    return g

def chain(node_guids):
    for a, b in zip(node_guids, node_guids[1:]):
        links.append((a, b))

# Shorthands
def F(txt, spr=None, timelapse="", post=None, vid=None, thumb=None, gal=0, y=0):
    return npc(FREYA, txt, timelapse=timelapse, spr=spr, post=post,
               vid=vid, thumb=thumb, gal=gal, y=y)
def L(txt, spr=None, timelapse="", y=0):
    return npc(LENNY, txt, timelapse=timelapse, spr=spr, y=y)
def MC(txt, npc_txt="", spr=None, vid=None, thumb=None, gal=0, y=0):
    return mc2(FREYA, [txt], npc_txt,
               port_sprs=[spr], port_vids=[vid], port_thumbs=[thumb], port_gals=[gal], y=y)
def MCL(txt, npc_txt="", spr=None, y=0):
    return mc2(LENNY, [txt], npc_txt, port_sprs=[spr], port_gals=[0], y=y)
def EVT(guid, y=0): return evt(guid, y=y)
def END():   return end()
def START(): return start()

# =============================================================================
# NODE DEFINITIONS
# =============================================================================

g_start = START()

# ── Shoot section (MC sends Freya pics during the blue latex shoot) ───────────
g_mc_onway    = MC("On my way")
g_f_bedroom   = F("", spr=S_BEDROOM, timelapse="30 minutes later...")
g_f_balcony   = F("", spr=S_BALCONY_S, timelapse="30 minutes later...")
g_v_dance     = F("", vid=V_DANCE, thumb=V_DANCE_THUMB, timelapse="30 minutes later...", gal=1)

# Pre-alloc IF: confidence > 1 → naughty pic, else tame pic
g_if_shoot    = ng()
g_mc_naughty  = MC("", spr=S_NAUGHTY, gal=1, y=0)
g_mc_tame     = MC("", spr=S_TAME, gal=1, y=600)

g_tl_60m      = F("", timelapse="60 minutes later... You are back home")

# Social post: balcony goes live
g_social_balcony = F("", post=P_BALCONY)

# ── After shoot chat ──────────────────────────────────────────────────────────
g_mc_fast     = MC("That was fast")
g_f_haha      = F("haha")
g_f_sorry     = F("Sorry, I just couldn't wait")
g_mc_pics_gd  = MC("Pictures were that good?")
g_f_honestly  = F("Honestly? Yes")
g_f_notif     = F("My notifications are going crazy right now")
g_f_eye       = F("You actually have an eye for this")
g_f_nodirect  = F("I didn't have to direct you at all")

# CHOICE: latex compliment (confidence+1) vs happy they liked it
g_choice_comp = mc2(FREYA, [
    "Well, a gorgeous girl in latex definitely makes my job easier",
    "I'm happy you liked them"
])
# Branch 1: compliment → confidence + 1
g_evt_conf1   = EVT(EVT_CONF_ADD1, y=0)
g_f_calmdown  = F("Calm down Mr. Smooth Talker", y=0)
g_f_guess     = F("But I guess that's true so I shouldn't praise you too much", y=0)
# Branch 2: happy
g_f_vhappy    = F("Very happy indeed", y=600)

# Merge
g_f_howwas    = F("How was it for you?")
g_f_lil_much  = F("Did you like working with me? I know I can be a bit much...")
g_mc_great_tm = MC("I had a great time")
g_mc_not_much = MC("You weren't too much at all")
g_f_good_ok   = F("Good")

# ── IF confidence > 2: hired fast (high) or hired slow (low) ─────────────────
g_if_hire     = ng()

# HIGH path (confidence >= 3) y=0
g_f_hired     = F("So I guess you're hired", y=0)
g_f_official  = F("As my official cameraman for now anyway", y=0)
g_f_treat     = F("If you keep treating me right maybe you'll get a promotion later", y=0)
g_mc_agame    = MC("I'll make sure to bring my A-game", y=0)
g_f_better    = F("You better", y=0)
g_f_energy    = F("Anyway I've got to go burn off some energy", y=0)
g_f_gym       = F("Hitting the gym", y=0)
g_f_gym_pic   = F("", spr=S_BEDROOM, y=0)  # gym selfie dark blue yoga pants (infront of bed)
g_mc_damn_gym = MC("Damn", y=0)
g_mc_goodwk   = MC("Have a good workout", y=0)
g_f_thanks_gym= F("Thanks", y=0)
g_f_tmrw1     = F("Talk to you tomorrow", y=0)

# LOW path (confidence < 3) y=600
g_f_glad      = F("I'm glad you liked it", y=600)
g_f_process   = F("But I think I need to process everything first", y=600)
g_f_bigstep   = F("Bringing someone on is a big step for my brand", y=600)
g_f_letme     = F("Let me think about it?", y=600)
g_mc_sure_time= MC("Sure take your time", y=600)
g_f_tl_2h_a  = F("", timelapse="2 hours later...", y=600)
g_f_thought   = F("Okay I thought about it", y=600)
g_f_hired2    = F("You're hired as my cameraman", y=600)
g_f_seehow    = F("Let's see how it goes and take it from there", y=600)
g_mc_plan     = MC("Sounds like a plan", y=600)
g_f_cool_ok   = F("Cool", y=600)
g_f_gym2      = F("I'm heading to the gym now to clear my head", y=600)
g_f_tmrw2     = F("Talk to you tomorrow", y=600)
g_mc_goodwk2  = MC("Have a good workout", y=600)
g_mc_seeyatmrw= MC("See ya tomorrow", y=600)

# ── MORNING: confession about management ─────────────────────────────────────
g_tl_morning  = F("", timelapse="Next morning...")
g_f_morning   = F("Morning")
g_mc_morning  = MC("Morning")
g_f_confession= F("So I have a small confession to make")

# CHOICE: reaction to confession
g_choice_conf = mc2(FREYA, [
    "You're actually a millionaire CEO who's gonna retire me cause you're in love with me?",
    "Please don't tell me you are secretly married"
])
# Branch 1
g_f_emoji_conf= F("\U0001f602", y=0)
g_f_close     = F("Close, but no", y=0)
# Branch 2
g_f_godno     = F("God no \U0001f602", y=600)
g_f_nothing   = F("Nothing like that", y=600)

# Merge: management talk
g_f_mgmt      = F("I actually have management")
g_mc_ohlike   = MC("oh... like a group that.... Manages you?")
g_f_signed    = F("Yea, so I signed with them when I was a lot smaller")
g_f_network   = F("They are like a creator network")
g_mc_whatdo   = MC("So what do they do for you")
g_f_sponsors  = F("Sponsors, events, collabs with other creators in the network... the usual")
g_mc_okay     = MC("Okay")
g_f_shifting  = F("But things are shifting now")
g_mc_spicy    = MC("Because of the spicy stuff?")
g_f_yeah_spcy = F("Yeah")
g_f_lenny_num = F("Honestly... Lenny was the one who first showed me the numbers")
g_f_position  = F("Like what other girls in my position were actually making")
g_mc_changed  = MC("And that changed things for you?")
g_f_unsee     = F("I mean... once you see it you can't unsee it right?")
g_f_direction = F("I was already kind of going in that direction anyway")
g_f_front     = F("He just... put it in front of me in a way that made it click")
g_f_realpath  = F("Like okay this is actually a real path")
g_mc_hisidea  = MC("So it was his idea?")
g_f_nononono  = F("No no no")
g_f_vision    = F("It was always my vision")
g_f_viable    = F("He just helped me see it was actually viable")
g_f_bigdiff   = F("Big difference")
g_mc_right    = MC("Right...")
g_f_anyway_net= F("Anyway since they are a mainstream network and not an adult company...")
g_f_castcall  = F("They couldn't exactly put out a casting call for a guy to shoot spicy stuff with me")
g_mc_dating   = MC("Ah... so that's why the dating app")
g_f_exactly   = F("Exactly")
g_f_told      = F("I told Lenny about the shoot yesterday by the way")
g_f_aboutyou  = F("And about you")
g_mc_ohboy    = MC("Oh boy")
g_f_wants     = F("He wants to talk to you")
g_f_logistics = F("Just logistics, making sure everyone is on the same page")
g_f_hisnum    = F("I already gave him your number... his name is Lenny")
g_mc_movfast  = MC("Moving fast")
g_f_youknow   = F("You know it")
g_f_letmeknow = F("Let me know what he says")

# Wizard video social post goes live + event
# NOTE: P_WIZARD social post asset not yet created. Add GUID to P_WIZARD above when ready.
# g_social_wizard = F("", post=P_WIZARD)
g_evt_wiz_true  = EVT(EVT_WIZ_TRUE)

# ── Lenny afternoon conversation ──────────────────────────────────────────────
g_l_yo        = L("Yo, This the new camera guy?", timelapse="In the afternoon...")
g_mcl_depends = MCL("Depends who's asking")
g_l_lenny     = L("Lenny")
g_l_support   = L("Freya's number one supporter AKA her manager (:")
g_l_shegave   = L("She gave me your number")
g_mcl_mention = MCL("Yeah, she mentioned you would reach out")
g_l_great     = L("Great!")
g_l_brief     = L("Look I'll keep it brief")
g_l_balcony   = L("I saw the Balcony post...")
g_l_good_work = L("You did good work")
g_l_camera_q  = L("Do you have experience with a camera?")
g_mcl_no      = MCL("No actually...")
g_l_impress   = L("Ha, all the more impressive then!")
g_l_positive  = L("Look, Freya was very positive about you")

# IF confidence > 1: Lenny says "pictures and maybe more" vs "pictures show potential"
g_if_lenny_conf= ng()
# TRUE path (confidence >= 2) y=0
g_l_pics_more = L("About the pictures and maybe more...", y=0)
g_l_ambs      = L("She already told you about her ambitions right?", y=0)
g_mcl_shedid  = MCL("She did", y=0)
g_l_great2    = L("Great", y=0)
# FALSE path (confidence < 2) y=600
g_l_potential = L("And the pictures clearly show your potential", y=600)

# Merge: straight talk
g_l_straight  = L("Look I'll be straight with you")
g_l_family    = L("Freya is like family to me at this point")
g_mcl_trusts  = MCL("I can tell she trusts you")
g_l_guys      = L("I've seen a lot of guys come into her life and not get it")
g_l_ex        = L("Her ex was a perfect example of that")
g_l_longtalk  = L("Let's just say I had a long talk with her about that situation")
g_l_helpedher = L("Helped her see it clearly")
g_mcl_see     = MCL("I see")
g_l_understand= L("So you understand why I need to know who I'm dealing with")
g_l_drawing   = L("What's drawing you to this? The money? Her? The bigger picture?")
g_l_nowrong   = L("No wrong answer by the way")

# CHOICE: money answer (lenny+1) vs genuine answer
g_choice_lenny = mc2(LENNY, [
    "Look, if there is actually money to be made here I'm all for it",
    "I just met her on a dating app... I like her vision and I want to help her"
])
# Branch 1: money (lenny+1) y=0
g_evt_lenny1  = EVT(EVT_LENNY_ADD1, y=0)
g_l_honest    = L("Honest. I like that.", y=0)
g_l_connected = L("You're connected to her now... if she goes big, you go big", y=0)
g_mcl_works   = MCL("That works for me", y=0)
# Branch 2: genuine y=600
g_l_good_ans  = L("Good answer", y=600)
g_l_stay      = L("Just make sure it stays that way", y=600)
g_l_connect2  = L("You're connected to her now... if she goes big, you go big", y=600)
g_mcl_undstd  = MCL("Understood", y=600)

# Merge: wrapping up Lenny convo
g_l_details   = L("We'll talk more about the details another time... busy day")
g_mcl_sure    = MCL("Sure")
g_l_onething  = L("One more thing... Freya has a sponsor that's interested")
g_l_fillin    = L("She'll fill you in")
g_l_intouch   = L("We'll be in touch")

# ── Freya evening conversation ─────────────────────────────────────────────────
g_f_hey       = F("Hey!", timelapse="Later in the day...")
g_f_howlenny  = F("So how did it go with Lenny?")
g_mc_finelenny= MC("It went fine")
g_mc_basic    = MC("He just asked some basic questions")
g_f_good2     = F("Good")
g_f_hetxt     = F("He also texted me")
g_mc_sponsor  = MC("About a sponsor right?")
g_f_yea_spons = F("YEA! he found a new sponsor for me!")
g_f_real_one  = F("A real one this time")
g_mc_awesome  = MC("That's awesome")
g_mc_whatkind = MC("What kind of sponsor?")
g_f_clothing  = F("It's a clothing brand")
g_f_tomorrow  = F("I'm getting a package tomorrow with a sample product to promote")
g_mc_nice     = MC("Nice")
g_mc_whatcloth= MC("Do you know what kind of clothes?")
g_f_notexact  = F("Not exactly")
g_f_vibe      = F("He just said it fits my new \"vibe\"")
g_f_darkblue  = F("I just hope it's dark blue... it's my favorite color")
g_f_notdrink  = F("But honestly I'm just happy it's not another gaming energy drink")
g_mc_haha_drk = MC("haha, that's the brand deals you had before?")
g_f_yea_drk   = F("yea...")
g_f_herelook  = F("Here look")
g_f_energy1_s = F("", spr=S_ENERGY1)
g_f_thisone   = F("And this one")
g_f_energy2_s = F("", spr=S_ENERGY2)
g_mc_profess  = MC("Very professional")
g_f_emoji2    = F("\U0001f602")
g_f_girlpay   = F("Hey a girl's gotta pay the bills")
g_f_ready     = F("But yeah I'm ready for some real brand deals now")
g_mc_hope     = MC("I hope its everything you expect from it")
g_f_thanks2   = F("Thanks! (:")
g_f_needtomrw = F("I will need you tomorrow to take some good pictures.")
g_f_theywill  = F("They will send those to the company and then they will decide if they actually want to sponsor some posts")
g_mc_okaygood = MC("Okay sounds good")
g_mc_tonight  = MC("So are you doing anything tonight")
g_f_nope      = F("Nope")
g_f_chilling  = F("Just chilling at home")

# CHOICE: go out or stay home
g_choice_tonight = mc2(FREYA, [
    "Want to go out? have some fun?",
    "Okay, see you tomorrow"
])

# Port 0: Want to go out → IF confidence > 1
g_if_tonight  = ng()

# TRUE: bar night (confidence >= 2) → confidence + 3 via 3x events
g_evt_conf3a  = EVT(EVT_CONF_ADD1, y=0)
g_evt_conf3b  = EVT(EVT_CONF_ADD1, y=0)
g_evt_conf3c  = EVT(EVT_CONF_ADD1, y=0)
g_f_oh        = F("Oh?", y=0)
g_f_wantgo    = F("You want to go out with me?", y=0)
g_mc_maybe    = MC("Maybe", y=0)
g_f_fine_emoji= F("\U0001f602", y=0)
g_f_fine      = F("Fine, what do you have in mind?", y=0)
g_mc_bar      = MC("Just go to a bar... get some drinks...", y=0)
g_f_okaybar   = F("Okay, you know a place", y=0)
g_mc_addr     = MC("Yeah, I'll text you an address", y=0)
g_f_okay_addr = F("Okay", y=0)
g_social_goout= F("", post=P_GOING_OUT, y=0)
g_social_beer = F("", post=P_BEER, y=0)

# FALSE: confidence too low, Freya declines
g_f_oh_no     = F("oh", y=600)
g_f_nosorry   = F("no sorry... I want to be full of energy for the thing tomorrow", y=600)
g_f_maybeday  = F("Maybe another day okay?", y=600)
g_mc_sure_np  = MC("Sure, no problem", y=600)
g_mc_goodnite = MC("Have a good night", y=600)
g_f_youtoo    = F("You too... See you tomorrow", y=600)

# Port 1: Stay home → goodnight
g_mc_seeyatmrw2 = MC("Okay, see you tomorrow", y=1200)
g_f_yep_gn    = F("Yep", y=1200)
g_f_gn_selfie = F("Have a good night", spr=S_GOODNITE, y=1200)
g_mc_youtoo2  = MC("You too", y=1200)

# ── NEXT MORNING ──────────────────────────────────────────────────────────────
g_tl_nextmorn = F("", timelapse="The next morning...")

# IF confidence > 4: hangover morning vs normal morning
g_if_morn_conf = ng()

# TRUE: confidence >= 5, went out and got drunk
g_f_morn_hi   = F("Morning", y=0)
g_f_lastnight = F("Last night was really fun", y=0)
g_mc_morn_hi  = MC("Morning", y=0)
g_mc_greattime= MC("Yeah I had a great time", y=0)
g_f_same_haha = F("Same \U0001f602", y=0)
g_f_mailman   = F("Even though the mailman definitely judged me this morning", y=0)
g_mc_why      = MC("Why what did you do?", y=0)
g_f_hungover  = F("I answered the door looking like a complete hungover mess...", y=0)
g_mc_barman   = MC("HaHa... yea the barman should have probably cut us off a bit sooner...", y=0)
g_f_shots     = F("I can't remember how many shots I took.... and did we kiss?", y=0)
g_mc_didwe    = MC("Did we?", y=0)
g_f_dreamt    = F("Maybe I dreamt it...", y=0)
g_f_regret    = F("And if it did happen... I don't regret it", y=0)
g_mc_sad      = MC("Now I feel sad...", y=0)
g_f_why2      = F("Why?", y=0)
g_mc_noremem  = MC("Cause I don't remember any of it", y=0)
g_f_dots      = F("...", y=0)
g_f_good3     = F("Good", y=0)
g_f_emoji3    = F("\U0001f602", y=0)
g_f_mailman2  = F("But the mailman gave me such a look.... \U0001f602", y=0)
g_mc_package  = MC("Hey as long as you got the package", y=0)
g_f_exactlyp  = F("Exactly", y=0)
g_f_woke      = F("And it woke me right up", y=0)
g_f_package   = F("THE PACKAGE IS HERE", y=0)

# FALSE: normal morning
g_f_morn_lo   = F("Morning", y=600)
g_mc_morn_lo  = MC("Morning", y=600)
g_f_package2  = F("The package is here!", y=600)

# Merge: package arrived for both paths
g_mc_awesome2 = MC("Awesome")
g_mc_opened   = MC("Did you open it?")
g_f_couldnt   = F("I couldn't wait \U0001f602")
g_f_look      = F("Look at this")
g_f_ling_rev  = F("", spr=S_LINGERIE)
g_mc_simple   = MC("Oh its simple but nice")
g_f_excited   = F("I know right...I'm so excited")
g_f_another   = F("There is another set in there but il keep that one a secret until you get here")
g_f_color     = F("It's exactly my color too")
g_mc_sounds   = MC("haha sounds good")
g_f_come      = F("Get over here so we can shoot this")
g_mc_onway2   = MC("On my way")
g_f_seeyousoon= F("See you soon")

# ── Lenny's request (lingerie shoot) ─────────────────────────────────────────
g_l_yo2       = L("Yo")
g_l_timing    = L("When is that lingerie shoot happening?")
g_mcl_heading = MCL("Heading to her place right now actually")
g_l_perfect   = L("Perfect timing then")
g_l_favor     = L("Listen, I need a favor")

# IF lenny_aliance == 1: money path vs plain path
g_if_lenny_req = ng()

# TRUE: Lenny remembers you're in for the money
g_l_money_q   = L("You said you were in this for the money, right?", y=0)
g_mcl_well    = MCL("Well, if there is money to be made then I'm interested, that's what I said....", y=0)
g_l_spons_hit = L("The sponsor just hit me up", y=0)
g_l_bonus     = L("They are willing to add a nice bonus payment!", y=0)
g_mcl_great_q = MCL("Okay... great. You tell Freya about that yet?", y=0)
g_l_nojust1   = L("no no Just listen first....", y=0)
g_l_lesres    = L("They want the shots to be less restrictive", y=0)
g_mcl_less_q  = MCL("Less restrictive?", y=0)
g_l_topless   = L("Topless. Or at least implied nudity....", y=0)
g_l_model1    = L("", spr=S_LNY_NUDE, y=0)
g_l_model2    = L("", spr=S_LNY_IMP, y=0)
g_l_viber     = L("That's the vibe they want", y=0)
g_mcl_see     = MCL("I see", y=0)
g_l_camera    = L("Since you're behind the camera today...", y=0)
g_l_push      = L("I need you to push her to do it", y=0)

# FALSE: Lenny goes straight to the ask
g_l_spons2    = L("The sponsor just reached out to me", y=600)
g_l_bonus2    = L("They are willing to add a nice bonus", y=600)
g_mcl_tellfr  = MCL("Okay, Did you tell this to Freya already", y=600)
g_l_nojust2   = L("no no... Just hold on a second", y=600)
g_mcl_catch   = MCL("Which is?", y=600)
g_l_lesres2   = L("They want the photos to be less restrictive", y=600)
g_l_implnud   = L("Implied nudity. Or just dropping the top.", y=600)
g_l_douyund   = L("Do you understand what I mean?", y=600)
g_mcl_see2    = MCL("Okay... I see", y=600)
g_l_great_img = L("This would be GREAT for her new online image...", y=600)
g_l_camera2   = L("Since you're behind the camera, I need you to push her", y=600)
g_l_takoff    = L("Get her to take the top off", y=600)
g_l_favor2    = L("You would be doing her a favor trust me", y=600)

# Merge: can you handle it?
g_l_handle    = L("Can you handle that?")

# CHOICE: agree to push (lenny+1) vs not gonna pressure
g_choice_push = mc2(LENNY, [
    "I can try to push her a little in the right direction...",
    "I'm not gonna pressure her... if she wants to then it happens..."
])
# Branch 1: push (lenny+1) y=0
g_evt_lenny2  = EVT(EVT_LENNY_ADD1, y=0)
g_l_you_get   = L("You get it....", y=0)
g_l_make_hpn  = L("Make it happen today", y=0)
g_l_comp      = L("I'll make sure you get properly compensated (;", y=0)
# Branch 2: won't pressure y=600
g_l_see2      = L("I see...", y=600)
g_l_undstd2   = L("You have to understand that this would really help her out", y=600)
g_l_favor3    = L("You would really be doing her a favor...", y=600)
g_l_feelout   = L("Just feel it out", y=600)

# Merge
g_l_goodluck  = L("Good luck")
g_mcl_thanks  = MCL("Thanks")

# ── 2 hours later: back home, Freya sends pics ───────────────────────────────
g_tl_2h_back  = F("", timelapse="2 hours later... You are back home")

# IF confidence > 1: full warm chat vs straight to pics
g_if_after_conf = ng()

# TRUE: full warm version
g_f_safe      = F("Hey! Did you make it home safe?", y=0)
g_mc_walked   = MC("Yeah, just walked through the door", y=0)
g_f_good4     = F("Good!", y=0)
g_f_thinking  = F("I was just thinking about how fun today was", y=0)
g_mc_team     = MC("Me too. We make a good team", y=0)
g_f_wereally  = F("We really do...", y=0)
g_f_anyway    = F("Anyway! I just finished sorting through the pictures you took", y=0)
g_mc_fast2    = MC("haha, that was fast", y=0)

# FALSE: straight to pics
g_f_sort      = F("Hey! I just finished sorting through the pictures you took", y=600)
g_mc_fast3    = MC("That was fast", y=600)

# Merge: lingerie pics share
g_f_couldnt2  = F("I couldn't wait!")
g_f_wearing   = F("Wearing that dark blue set... I just felt so confident.")
g_f_bestones  = F("Here are the best ones.")
g_f_lng_f_s   = F("", spr=S_LNG_FRONT)
g_mc_sexy     = MC("Sexy")
g_f_lng_b_s   = F("", spr=S_LNG_BACK)
g_f_lng_sd_s  = F("", spr=S_LNG_SED)
g_mc_incredbl = MC("You look incredible in all of them")

# ── Triple nude branch ────────────────────────────────────────────────────────
# IF lenny > 0 (lenny pushed for nudity)
g_if_lenny_nude = ng()

# TRUE: Lenny pushed → IF confidence > 1
g_if_conf_nude  = ng()

# TRUE-TRUE: topless (lenny pushed + confidence high) y=0
g_f_thenthis  = F("And then there's this one...", y=0)
g_f_nude_s    = F("", spr=S_NUDE, gal=1, y=0)
g_f_cantbelv  = F("I can't believe I actually took the top off.", y=0)
g_f_hotaf     = F("But it does look hot as fuck", y=0)
g_mc_powerful = MC("It does... Didn't it make you feel powerful?", y=0)
g_f_powerful  = F("Yeah... I really did feel powerful.", y=0)
g_f_sending1  = F("I'm sending all these pictures to Lenny now so he can communicate with the sponsor.", y=0)
g_f_hope1     = F("I really hope it's good enough for them.", y=0)

# TRUE-FALSE: implied nude (lenny pushed + confidence low) y=600
g_f_riskier   = F("And here is the... riskier one you suggested.", y=600)
g_f_impl_s    = F("", spr=S_IMPLIED, gal=1, y=600)
g_f_cover     = F("This is as far as I want to go for now.", y=600)
g_mc_impl_hot = MC("It's perfect. Implied is sometimes even hotter than full nude.", y=600)
g_f_pushed    = F("Thanks... It definitely pushed myself a bit....", y=600)
g_f_sending2  = F("I'm sending all these pictures to Lenny now.", y=600)
g_f_hope2     = F("He will communicate with the sponsor, I hope they appreciate the effort I put in.", y=600)

# FALSE (lenny == 0): covered lingerie path y=1200
g_f_lastset   = F("And the last one for the set.", y=1200)
g_f_cute_s    = F("", spr=S_LNG_FRONT, y=1200)
g_f_happy_pic = F("I'm really happy with all the pics...", y=1200)
g_mc_happy_too= MC("I'm happy you are happy", y=1200)
g_f_sending3  = F("I'm sending all these pictures to Lenny now so he can communicate with the sponsor.", y=1200)
g_f_hope3     = F("I just hope it's good enough for them!", y=1200)

# Bonus: IF confidence > 1 in lenny==0 path → bathroom selfie
g_if_conf_bonus = ng()
g_f_ohyea     = F("oh yea", y=0)
g_f_kept_out  = F("I might have kept one picture out of the batch I'm sending to Lenny.", y=0)
g_f_fav_cam   = F("Just for my favorite cameraman.", y=0)
g_mc_oh_rly   = MC("Oh really?", y=0)
g_f_bath_s    = F("", spr=S_BATH_SEL, gal=1, y=0)
g_f_bonus_wink= F("Consider it a bonus (;", y=0)
g_mc_motivat  = MC("These types of bonuses will definitely keep me motivated...", y=0)
g_f_increase  = F("Good... and just so you know the bonuses increase over time (;", y=0)
g_mc_cantw8   = MC("Can't wait to see my next one", y=0)

# Merge all 3 paths + bonus into shower section
g_f_shower    = F("Anyway, I'm going to take a shower and wash this makeup off.")
g_f_lennyback = F("Hopefully Lenny gets back to us soon with what the sponsor thinks.")
g_f_future    = F("If they like these sample pics, we'll get to do the real official shoot in the future! probably with more sets maybe even a studio!!!")
g_mc_fingers  = MC("Fingers crossed. Go relax, you earned it.")
g_f_willdoa   = F("Will do! Talk later")

# ── IF lenny > 0: Lenny payment ───────────────────────────────────────────────
g_if_lenny_pay = ng()

# TRUE: Lenny sends payment
g_l_yo3       = L("Yo.", timelapse="1 hour later...")
g_l_saw       = L("Just saw the pictures Freya sent over.")
g_mcl_and     = MCL("And?")
g_l_great3    = L("She did great.")
g_l_better    = L("Honestly better than I expected")
g_l_good_at   = L("You're good at this")
g_mcl_worked  = MCL("Glad it worked out")
g_l_always    = L("It always works out when the right people are involved")
g_l_talksoon  = L("Talk soon")
g_l_payment   = L("Chase bank: You have received $250.00 from Lenny Management Inc.")

g_end = END()

# =============================================================================
# WIRE LINKS
# =============================================================================

# Opening shoot
chain([g_start, g_mc_onway, g_f_bedroom, g_f_balcony, g_v_dance])

# IF shoot: naughty vs tame
g_if_shoot_node = ifn("freya_mc_confidence", 2, "1",
                      g_mc_naughty, g_mc_tame,
                      preset_guid=g_if_shoot, after_guid=g_v_dance)
links.append((g_v_dance, g_if_shoot_node))
links.append((g_mc_naughty, g_tl_60m))
links.append((g_mc_tame,    g_tl_60m))

chain([g_tl_60m, g_social_balcony,
       g_mc_fast, g_f_haha, g_f_sorry, g_mc_pics_gd, g_f_honestly,
       g_f_notif, g_f_eye, g_f_nodirect])

# CHOICE: compliment vs happy
links.append((g_f_nodirect, g_choice_comp))
links.append((g_choice_comp, g_evt_conf1))
chain([g_evt_conf1, g_f_calmdown, g_f_guess])
links.append((g_f_guess, g_f_howwas))
links.append((g_choice_comp, g_f_vhappy))
links.append((g_f_vhappy, g_f_howwas))

chain([g_f_howwas, g_f_lil_much, g_mc_great_tm, g_mc_not_much, g_f_good_ok])

# IF hire: high vs low confidence path
g_if_hire_node = ifn("freya_mc_confidence", 2, "2",
                     g_f_hired, g_f_glad,
                     preset_guid=g_if_hire, after_guid=g_f_good_ok)
links.append((g_f_good_ok, g_if_hire_node))

# HIGH path
chain([g_f_hired, g_f_official, g_f_treat, g_mc_agame, g_f_better,
       g_f_energy, g_f_gym, g_f_gym_pic, g_mc_damn_gym, g_mc_goodwk,
       g_f_thanks_gym, g_f_tmrw1])
links.append((g_f_tmrw1, g_tl_morning))

# LOW path
chain([g_f_glad, g_f_process, g_f_bigstep, g_f_letme, g_mc_sure_time,
       g_f_tl_2h_a, g_f_thought, g_f_hired2, g_f_seehow, g_mc_plan,
       g_f_cool_ok, g_f_gym2, g_f_tmrw2, g_mc_goodwk2, g_mc_seeyatmrw])
links.append((g_mc_seeyatmrw, g_tl_morning))

# Morning confession
chain([g_tl_morning, g_f_morning, g_mc_morning, g_f_confession])
links.append((g_f_confession, g_choice_conf))
links.append((g_choice_conf, g_f_emoji_conf))
chain([g_f_emoji_conf, g_f_close])
links.append((g_f_close, g_f_mgmt))
links.append((g_choice_conf, g_f_godno))
chain([g_f_godno, g_f_nothing])
links.append((g_f_nothing, g_f_mgmt))

chain([g_f_mgmt, g_mc_ohlike, g_f_signed, g_f_network, g_mc_whatdo,
       g_f_sponsors, g_mc_okay, g_f_shifting, g_mc_spicy, g_f_yeah_spcy,
       g_f_lenny_num, g_f_position, g_mc_changed, g_f_unsee, g_f_direction,
       g_f_front, g_f_realpath, g_mc_hisidea, g_f_nononono, g_f_vision,
       g_f_viable, g_f_bigdiff, g_mc_right, g_f_anyway_net, g_f_castcall,
       g_mc_dating, g_f_exactly, g_f_told, g_f_aboutyou, g_mc_ohboy,
       g_f_wants, g_f_logistics, g_f_hisnum, g_mc_movfast, g_f_youknow,
       g_f_letmeknow, g_evt_wiz_true])

# Lenny conversation
chain([g_evt_wiz_true, g_l_yo, g_mcl_depends, g_l_lenny, g_l_support,
       g_l_shegave, g_mcl_mention, g_l_great, g_l_brief, g_l_balcony,
       g_l_good_work, g_l_camera_q, g_mcl_no, g_l_impress, g_l_positive])

g_if_lenny_conf_node = ifn("freya_mc_confidence", 2, "1",
                           g_l_pics_more, g_l_potential,
                           preset_guid=g_if_lenny_conf, after_guid=g_l_positive)
links.append((g_l_positive, g_if_lenny_conf_node))

chain([g_l_pics_more, g_l_ambs, g_mcl_shedid, g_l_great2])
links.append((g_l_great2, g_l_straight))
links.append((g_l_potential, g_l_straight))

chain([g_l_straight, g_l_family, g_mcl_trusts, g_l_guys, g_l_ex,
       g_l_longtalk, g_l_helpedher, g_mcl_see, g_l_understand,
       g_l_drawing, g_l_nowrong])
links.append((g_l_nowrong, g_choice_lenny))
links.append((g_choice_lenny, g_evt_lenny1))
chain([g_evt_lenny1, g_l_honest, g_l_connected, g_mcl_works])
links.append((g_mcl_works, g_l_details))
links.append((g_choice_lenny, g_l_good_ans))
chain([g_l_good_ans, g_l_stay, g_l_connect2, g_mcl_undstd])
links.append((g_mcl_undstd, g_l_details))

chain([g_l_details, g_mcl_sure, g_l_onething, g_l_fillin, g_l_intouch])

# Freya evening
chain([g_l_intouch, g_f_hey, g_f_howlenny, g_mc_finelenny, g_mc_basic,
       g_f_good2, g_f_hetxt, g_mc_sponsor, g_f_yea_spons, g_f_real_one,
       g_mc_awesome, g_mc_whatkind, g_f_clothing, g_f_tomorrow, g_mc_nice,
       g_mc_whatcloth, g_f_notexact, g_f_vibe, g_f_darkblue, g_f_notdrink,
       g_mc_haha_drk, g_f_yea_drk, g_f_herelook, g_f_energy1_s, g_f_thisone,
       g_f_energy2_s, g_mc_profess, g_f_emoji2, g_f_girlpay, g_f_ready,
       g_mc_hope, g_f_thanks2, g_f_needtomrw, g_f_theywill, g_mc_okaygood,
       g_mc_tonight, g_f_nope, g_f_chilling])

# CHOICE: tonight - go out vs stay home
links.append((g_f_chilling, g_choice_tonight))

# Port 0 → IF tonight (confidence > 1)
g_if_tonight_node = ifn("freya_mc_confidence", 2, "1",
                        g_evt_conf3a, g_f_oh_no,
                        preset_guid=g_if_tonight, after_guid=g_f_chilling)
links.append((g_choice_tonight, g_if_tonight_node))

# TRUE: bar night
chain([g_evt_conf3a, g_evt_conf3b, g_evt_conf3c, g_f_oh, g_f_wantgo,
       g_mc_maybe, g_f_fine_emoji, g_f_fine, g_mc_bar, g_f_okaybar,
       g_mc_addr, g_f_okay_addr, g_social_goout, g_social_beer])
links.append((g_social_beer, g_tl_nextmorn))

# FALSE: decline
chain([g_f_oh_no, g_f_nosorry, g_f_maybeday, g_mc_sure_np,
       g_mc_goodnite, g_f_youtoo])
links.append((g_f_youtoo, g_tl_nextmorn))

# Port 1 → stay home
links.append((g_choice_tonight, g_mc_seeyatmrw2))
chain([g_mc_seeyatmrw2, g_f_yep_gn, g_f_gn_selfie, g_mc_youtoo2])
links.append((g_mc_youtoo2, g_tl_nextmorn))

# Next morning IF
g_if_morn_node = ifn("freya_mc_confidence", 2, "4",
                     g_f_morn_hi, g_f_morn_lo,
                     preset_guid=g_if_morn_conf, after_guid=g_tl_nextmorn)
links.append((g_tl_nextmorn, g_if_morn_node))

# HIGH: hangover path
chain([g_f_morn_hi, g_f_lastnight, g_mc_morn_hi, g_mc_greattime, g_f_same_haha,
       g_f_mailman, g_mc_why, g_f_hungover, g_mc_barman, g_f_shots,
       g_mc_didwe, g_f_dreamt, g_f_regret, g_mc_sad, g_f_why2,
       g_mc_noremem, g_f_dots, g_f_good3, g_f_emoji3, g_f_mailman2,
       g_mc_package, g_f_exactlyp, g_f_woke, g_f_package])
links.append((g_f_package, g_mc_awesome2))

# LOW: normal morning
chain([g_f_morn_lo, g_mc_morn_lo, g_f_package2])
links.append((g_f_package2, g_mc_awesome2))

chain([g_mc_awesome2, g_mc_opened, g_f_couldnt, g_f_look, g_f_ling_rev,
       g_mc_simple, g_f_excited, g_f_another, g_f_color, g_mc_sounds,
       g_f_come, g_mc_onway2, g_f_seeyousoon])

# Lenny lingerie request
chain([g_f_seeyousoon, g_l_yo2, g_l_timing, g_mcl_heading, g_l_perfect, g_l_favor])

g_if_lenny_req_node = ifn("lenny_aliance", 0, "1",
                          g_l_money_q, g_l_spons2,
                          preset_guid=g_if_lenny_req, after_guid=g_l_favor)
links.append((g_l_favor, g_if_lenny_req_node))

# TRUE: money path
chain([g_l_money_q, g_mcl_well, g_l_spons_hit, g_l_bonus, g_mcl_great_q,
       g_l_nojust1, g_l_lesres, g_mcl_less_q, g_l_topless,
       g_l_model1, g_l_model2, g_l_viber, g_mcl_see, g_l_camera, g_l_push])
links.append((g_l_push, g_l_handle))

# FALSE: direct path
chain([g_l_spons2, g_l_bonus2, g_mcl_tellfr, g_l_nojust2, g_mcl_catch,
       g_l_lesres2, g_l_implnud, g_l_douyund, g_mcl_see2, g_l_great_img,
       g_l_camera2, g_l_takoff, g_l_favor2])
links.append((g_l_favor2, g_l_handle))

links.append((g_l_handle, g_choice_push))
links.append((g_choice_push, g_evt_lenny2))
chain([g_evt_lenny2, g_l_you_get, g_l_make_hpn, g_l_comp])
links.append((g_l_comp, g_l_goodluck))
links.append((g_choice_push, g_l_see2))
chain([g_l_see2, g_l_undstd2, g_l_favor3, g_l_feelout])
links.append((g_l_feelout, g_l_goodluck))

chain([g_l_goodluck, g_mcl_thanks, g_tl_2h_back])

# After shoot: IF confidence > 1
g_if_after_node = ifn("freya_mc_confidence", 2, "1",
                      g_f_safe, g_f_sort,
                      preset_guid=g_if_after_conf, after_guid=g_tl_2h_back)
links.append((g_tl_2h_back, g_if_after_node))

chain([g_f_safe, g_mc_walked, g_f_good4, g_f_thinking, g_mc_team,
       g_f_wereally, g_f_anyway, g_mc_fast2])
links.append((g_mc_fast2, g_f_couldnt2))
chain([g_f_sort, g_mc_fast3])
links.append((g_mc_fast3, g_f_couldnt2))

chain([g_f_couldnt2, g_f_wearing, g_f_bestones,
       g_f_lng_f_s, g_mc_sexy, g_f_lng_b_s, g_f_lng_sd_s, g_mc_incredbl])

# IF lenny > 0
g_if_lenny_nude_node = ifn("lenny_aliance", 2, "0",
                           g_if_conf_nude, g_f_lastset,
                           preset_guid=g_if_lenny_nude, after_guid=g_mc_incredbl)
links.append((g_mc_incredbl, g_if_lenny_nude_node))

# TRUE: IF confidence > 1
g_if_conf_nude_node = ifn("freya_mc_confidence", 2, "1",
                          g_f_thenthis, g_f_riskier,
                          preset_guid=g_if_conf_nude, after_guid=g_if_lenny_nude_node)

# TRUE-TRUE: topless
chain([g_f_thenthis, g_f_nude_s, g_f_cantbelv, g_f_hotaf,
       g_mc_powerful, g_f_powerful, g_f_sending1, g_f_hope1])
links.append((g_f_hope1, g_f_shower))

# TRUE-FALSE: implied
chain([g_f_riskier, g_f_impl_s, g_f_cover, g_mc_impl_hot,
       g_f_pushed, g_f_sending2, g_f_hope2])
links.append((g_f_hope2, g_f_shower))

# FALSE (lenny==0): covered path
chain([g_f_lastset, g_f_cute_s, g_f_happy_pic, g_mc_happy_too,
       g_f_sending3, g_f_hope3])

# Bonus IF confidence > 1
g_if_conf_bonus_node = ifn("freya_mc_confidence", 2, "1",
                           g_f_ohyea, g_f_shower,
                           preset_guid=g_if_conf_bonus, after_guid=g_f_hope3)
links.append((g_f_hope3, g_if_conf_bonus_node))

chain([g_f_ohyea, g_f_kept_out, g_f_fav_cam, g_mc_oh_rly,
       g_f_bath_s, g_f_bonus_wink, g_mc_motivat, g_f_increase, g_mc_cantw8])
links.append((g_mc_cantw8, g_f_shower))

chain([g_f_shower, g_f_lennyback, g_f_future, g_mc_fingers, g_f_willdoa])

# IF lenny > 0: payment
g_if_lenny_pay_node = ifn("lenny_aliance", 2, "0",
                          g_l_yo3, g_end,
                          preset_guid=g_if_lenny_pay, after_guid=g_f_willdoa)
links.append((g_f_willdoa, g_if_lenny_pay_node))

chain([g_l_yo3, g_l_saw, g_mcl_and, g_l_great3, g_l_better,
       g_l_good_at, g_mcl_worked, g_l_always, g_l_talksoon, g_l_payment, g_end])

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

# Post-fill InputGuid placeholders
from collections import defaultdict
_outlinks = defaultdict(list)
for _base, _tgt in links:
    if _base in port_pgs:
        _outlinks[_base].append(_tgt)

_cnodes_str = "".join(cnodes)
for _ng, _pgs in port_pgs.items():
    _targets = _outlinks.get(_ng, [])
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
    "  m_Name: Chapter 2 - Good intentions\n"
    "  m_EditorClassIdentifier: Assembly-CSharp::MeetAndTalk.DialogueContainerSO\n"
    "  AllowDialogueSave: 0\n  BlockingReopeningDialogue: 0\n"
    "  NodeLinkDatas:\n" + link_yaml() +
    "  DialogueChoiceNodeDatas:\n" + _cnodes_str +
    "  DialogueNodeDatas:\n"       + "".join(dnodes) +
    "  TimerChoiceNodeDatas: []\n"
    "  EndNodeDatas:\n"            + "".join(endnodes) +
    "  EventNodeDatas:\n"          + "".join(enodes) +
    "  StartNodeDatas:\n"          + "".join(stnodes) +
    "  RandomNodeDatas: []\n  CommandNodeDatas: []\n"
    "  IfNodeDatas:\n"             + "".join(ifnodes) +
    "  SpyNodeDatas: []"
)

import os
out_path = os.path.join(os.path.dirname(__file__), "Chapter 2 - Good intentions.asset")
with open(out_path, "w", encoding="utf-8") as f:
    f.write(out)
print(f"Written {len(out)} bytes  ->  {out_path}")
print(f"Nodes: {len(cnodes)} choice | {len(dnodes)} dialogue | "
      f"{len(enodes)} event | {len(ifnodes)} if | "
      f"{len(stnodes)} start | {len(endnodes)} end")
print(f"Links: {len(links)}")
