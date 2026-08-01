import uuid
from collections import defaultdict

def ng(): return str(uuid.uuid4())

# ── Character GUIDs ────────────────────────────────────────────────────────────
LILY = "7bd5d83a9443f8449b2f8346c4479558"
DAVE = "14173166ace8e7b418d7ae1d1a0526b0"
LISA = "c9e8e94be1dbf9b4da1b2fd941c31ea2"
IZZY = "8f6af30f1ed271b4aac60c69e0824cfb"
MB   = "df054aa4384061f44999656197df76d1"   # Math & Babes
ML   = "187cddd07c1283b4a98ac2bd7a2d40d5"   # Math & Lily

# ── Sprite GUIDs (Gallery/ep19/ep25) ───────────────────────────────────────────
B_1 = "4d204012bbd058841a308f3a40ab4d93"
B_2 = "cbffcd1b36ec2044ea2cc6ae7413f627"
B_3 = "5a7746f3c0306d547815b86e27deffe4"   # Izzy sleeping/drooling
B_4 = "23c1e591b5a3bbb4294951e5b84bf60d"
B_5 = "7d0df1b618ce68a4797cb0a17cbf4488"
B_6 = "3ea4668ae535eff4a80842cc864091c1"
B_7 = "57d6947719548014ebf606b4bde183fa"
OPEN_DR_SELFIE = "be377cbfedde3dd4897a455e9536311b"   # Lily + Math hug (open NTR)
DAVELISA_CABIN = "4f7b4e9537efb9d4ea3f713340cf7fe5"
DAVELISAMIA_CABIN = "41ece0c8307222445a40d5acaeb88777"
NTR_1 = "1f78f72e89b37a4408ca03529bcb36fd"
NTR_2 = "a7f937908647aa94b8c430ca4062499d"
NTR_3 = "7939db96fbcd717469c288634219832d"
NTR_35 = "e2f0cc82ed4f7184ba6efef67d863fd3"
NTR_4 = "87972747ae5eed248bfd31afd794486a"
NTR_5 = "f2811c164aaa9c7429662243a6e4bcf4"
NTS_2 = "ef070e447af4449428feafc4659fbaac"
NTS_3 = "22b0023fa3deded47817f8c2f03de17c"
MARINA_1 = "8f8a6bc21f0b8e04a8667bda14cb3818"
MARINA_2 = "6ac4f4e8be8986f42acabf2869c4a7d3"
LILY_SLEEP = "9c3b00b9c5d33e548aeb80bfe123365c"

# ── Video GUIDs (+ banners as thumbnails) ──────────────────────────────────────
CUCK_VID = "9b7088f4c1082804d8e1a2884bf98d2f"
CUCK_VID_THUMB = "6069c7ef85cb54844aa0f79dfbf31d7d"
NTS_VID = "8919e797ac6cf3d4daf592072be1ee24"
NTS_VID_THUMB = "3357b7d94c490cb438ca733369a15c3b"

# ── Social Post GUIDs (already configured SOs) ──────────────────────────────────
POST_MB_OPENING = "a2e8a607ff42df44cacc203009df01c4"   # "While the main girl is away"
POST_IZZY       = "8f592fe317d789143973a2e69d875d23"   # wine
POST_MB_MID     = "50502bc03bad5c040846af98d5622c61"   # secret NTR mid (bj eyes covered)
POST_ML_END     = "8a5ecefc1d86fcc4a8e4d01612f7dcd4"   # open NTR end (Math&Lily bj)
POST_MB_END     = "16ba2379b99db5746b979492ff2fcb9a"   # secret NTR end (ass eyes covered)

# ── Event SO GUID ───────────────────────────────────────────────────────────────
EVT_DAVELILY_PENDING = "b06e33eb9b33404b986ccabd09a55212"   # dave_lily_pending = 1

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

# ── YAML helpers (current MeetAndTalk schema: flat fields, 2 languages) ─────────
def _safe(txt):
    return (txt or "").replace("\n", " ").replace("\r", " ")

def _l2(en=""):
    return (f"    - languageEnum: 0\n      LanguageGenericType: {_safe(en)}\n"
            f"    - languageEnum: 1\n      LanguageGenericType: \n")

def _l2p(en=""):
    return (f"      - languageEnum: 0\n        LanguageGenericType: {_safe(en)}\n"
            f"      - languageEnum: 1\n        LanguageGenericType: \n")

def _audio2():
    return ("    - languageEnum: 0\n      LanguageGenericType: {fileID: 0}\n"
            "    - languageEnum: 1\n      LanguageGenericType: {fileID: 0}\n")

def _media_fields(spr=None, vid=None, thumb=None, gal=0):
    if vid:
        mt = 1
        img = "{fileID: 0}"
        video = f"{{fileID: 32900000, guid: {vid}, type: 3}}"
        vthumb = f"{{fileID: 21300000, guid: {thumb}, type: 3}}" if thumb else "{fileID: 0}"
    elif spr:
        mt = 0
        img = f"{{fileID: 21300000, guid: {spr}, type: 3}}"
        video = "{fileID: 0}"
        vthumb = "{fileID: 0}"
    else:
        mt = 0
        img = "{fileID: 0}"
        video = "{fileID: 0}"
        vthumb = "{fileID: 0}"
    return (f"    MediaType: {mt}\n    Image: {img}\n    Video: {video}\n"
            f"    VideoThumbnail: {vthumb}\n    NotBackgroundCapable: 0\n"
            f"    GalleryVisibility: {gal}\n")

# ── Node builders ─────────────────────────────────────────────────────────────
def npc(char, en="", tl="", spr=None, vid=None, thumb=None, post=None, gal=0):
    g = ng()
    post_ref = f"{{fileID: 11400000, guid: {post}, type: 2}}" if post else "{fileID: 0}"
    dnodes.append(
        f"  - NodeGuid: {g}\n"
        f"    Position: {{x: {px_store(g)}, y: 0}}\n"
        f"    DialogueNodePorts: []\n"
        f"    AudioClips:\n{_audio2()}"
        f"    Character: {{fileID: 11400000, guid: {char}, type: 2}}\n"
        f"    AvatarPos: 0\n    AvatarType: 0\n"
        f"    Texts:\n{_l2(en)}"
        f"    Timelapses:\n{_l2(tl)}"
        f"    Timelapse: \n    Duration: 2\n    Delay: 0\n"
        f"{_media_fields(spr, vid, thumb, gal)}"
        f"    Post: {post_ref}\n    DelayTimer: 0\n"
    )
    return g

def mc2(char, ports, npc_en=""):
    g = ng()
    ports_yaml = ""
    pgs = []
    for opt in ports:
        pg = ng()
        pgs.append(pg)
        ports_yaml += (
            f"    - PortGuid: {pg}\n"
            f"      InputGuid: __IGUID_{pg}__\n"
            f"      OutputGuid: {g}\n"
            f"      TextLanguage:\n{_l2p(opt)}"
            f"      HintLanguage:\n{_l2p()}"
        )
    port_pgs[g] = pgs
    req = 1 if _safe(npc_en) else 0
    cnodes.append(
        f"  - NodeGuid: {g}\n"
        f"    Position: {{x: {px_store(g)}, y: 0}}\n"
        f"    DialogueNodePorts:\n{ports_yaml}"
        f"    AudioClips:\n{_audio2()}"
        f"    Character: {{fileID: 11400000, guid: {char}, type: 2}}\n"
        f"    AvatarPos: 0\n    AvatarType: 0\n"
        f"    TextType:\n{_l2(npc_en)}"
        f"    Duration: 2\n    Delay: 0\n    Timelapse: \n"
        f"    RequireCharacterInput: {req}\n"
        f"    SelectedChoice: []\n"
    )
    return g

def evt(event_guid):
    g = ng()
    enodes.append(
        f"  - NodeGuid: {g}\n"
        f"    Position: {{x: {px_store(g)}, y: 0}}\n"
        f"    EventScriptableObjects:\n"
        f"    - DialogueEventSO: {{fileID: 11400000, guid: {event_guid}, type: 2}}\n"
    )
    return g

def ifn(var, ops, val, true_guid, false_guid, preset_guid=None, after_guid=None):
    g = preset_guid if preset_guid else ng()
    links.append((g, true_guid))
    links.append((g, false_guid))
    _pos = (node_pos[after_guid] + 350) if (after_guid and after_guid in node_pos) else px()
    node_pos[g] = _pos
    ifnodes.append(
        f"  - NodeGuid: {g}\n"
        f"    Position: {{x: {_pos}, y: 0}}\n"
        f"    ValueName: {var}\n"
        f"    Operations: {ops}\n"
        f"    OperationValue: {val}\n"
        f"    TrueGUID: {true_guid}\n"
        f"    FalseGUID: {false_guid}\n"
    )
    return g

def start():
    g = ng()
    stnodes.append(f"  - NodeGuid: {g}\n    Position: {{x: {px_store(g)}, y: 0}}\n    startID: \n")
    return g

def end():
    g = ng()
    endnodes.append(f"  - NodeGuid: {g}\n    Position: {{x: {px_store(g)}, y: 0}}\n    EndNodeType: 0\n    Dialogue: {{fileID: 0}}\n")
    return g

def chain(node_guids):
    for a, b in zip(node_guids, node_guids[1:]):
        links.append((a, b))

# ── Shorthand helpers ─────────────────────────────────────────────────────────
def L(t="", spr=None, vid=None, thumb=None, post=None):  return npc(LILY, t, "", spr=spr, vid=vid, thumb=thumb, post=post)
def LM(t=""):  return mc2(LILY, [t])
def D(t="", spr=None, vid=None, thumb=None, post=None):   return npc(DAVE, t, "", spr=spr, vid=vid, thumb=thumb, post=post)
def DM(t=""):  return mc2(DAVE, [t])
def SA(t="", spr=None, vid=None, thumb=None, post=None):  return npc(LISA, t, "", spr=spr, vid=vid, thumb=thumb, post=post)
def SAM(t=""): return mc2(LISA, [t])
def TL(t, char=LILY): return npc(char, "", t)
def POST(char, p): return npc(char, "", post=p)
def EVT(g): return evt(g)

# =============================================================================
# NODE DEFINITIONS
# =============================================================================
g_start = start()
g_end   = end()

# ── [A] Opening Math&Babes post (Continue_math && Docter_Medicine_denied) ───────
if_a_cm  = ng()
if_a_dmd = ng()
a_post = POST(MB, POST_MB_OPENING)

# ── [B] Top split (tag_along / lisa_know) ──────────────────────────────────────
if_b_tag = ng()
if_b_lk  = ng()

# ── [C] Dave late-night storm chat (tag_along && !lisa_know) ────────────────────
c_tl    = TL("Late that night... The storm begins to pass. Cell service is finally restored.", DAVE)
c1 = D("Bro!")
c2 = D("Finally got a signal again")
c3 = DM("Dave! Thank god. Are you guys okay?!")
c4 = D("Yeah, we're safe.")
c5 = D("But holy fuck dude, that was terrifying")
c6 = D("I was genuinely shitting bricks. The waves were tossing this massive yacht around like it was a plastic toy.")
c7 = D("The crew rushed us all up to the captain's cabin to bunker down.")
c8 = D("", spr=DAVELISA_CABIN)
c9 = DM("Glad to see you guys in one piece.")
c10 = D("Captain says the worst of the storm broke. We are heading back to the marina now.")
c11 = D("Kinda killed the mood for the night though 😂")
c12 = D("The husband didn't even come up here. Captain said he has his own secure room below deck. Crazy rich people shit.")
c13 = DM("I can imagine man. Just get back safe.")
c14 = D("Will do bro. I'm exhausted. I'm going to sleep the entire day tomorrow.")

# ── [D] Shopping path ──────────────────────────────────────────────────────────
d_izzy = POST(IZZY, POST_IZZY)
d_tl1  = TL("The next morning...", LILY)
d1 = L("Good morning baby!")
d2 = L("Did you sleep well?")
d3 = L("I ended up just having a super quiet night in the hotel.")
d4 = LM("Good morning, baby.")
d5 = LM("I did… got some good rest.")
d6 = L("Exactly! Although it wasn't quiet for long... Izzy just burst in like a hurricane hihi.")
d7 = L("She just got back from her date with Samuel.")
d8 = LM("Just got back? It's morning there right?")
d9 = L("I know! She completely skipped sleep...")
d10 = L("She looks exhausted but satisfied. She's been telling me all about it while trying to take her makeup off.")
d11 = LM("I can only imagine the details...")
d12 = L("Oh, she spares no details hihi.")
d13 = L("She sent me some pictures last night while I was already asleep. Look.")
d14 = L("", spr=B_1)
d15 = LM("She definitely went all out.")
d16 = L("She did! And Samuel matched her energy.")
d17 = L("", spr=B_2)
d18 = LM("Damn")
d19 = L("Yea, now you understand why she is like this now")
d20 = L("", spr=B_3)
d21 = LM("Let her sleep. What are your plans for today?")
d22 = L("Well, since Izzy is going to be out cold for hours, I thought I'd treat myself.")
d23 = L("I'm going to go do a little shopping! The stores here are amazing and I want to find something special.")
d24 = LM("Sounds like a great idea. Send me pictures of what you try on.")
d25 = L("You know I will!")

# 3-way (who Lily sends pics to)
if_send_cm  = ng()
if_send_dmd = ng()
# secret NTR
s1 = L("Oh and I'll send them to Ma—")
s2 = L("To Izzy I mean")
s3 = LM("To Izzy? But she's out cold")
s4 = L("She'll see them when she wakes up hihi")
s5 = LM("haha I guess so")
# open NTR
o1 = L("And to Math of course")
o2 = LM("Of course")
o3 = LM("Maybe he could post something")
o4 = L("Maybe hihi")
# NTS
n1 = L("And to Leo of course")
n2 = LM("Of course, how could I forget")

d26 = L("Love you babe")
d27 = LM("Love you too")
d_tl2 = TL("2 hours later...", LILY)
d28 = L("Okay baby, I'm at the boutique.")
d29 = L("I picked out a few things. Tell me what you think!")
d30 = L("First option... very summery and innocent.")
d31 = L("", spr=B_4)
d32 = LM("That looks beautiful on you.")
d33 = L("It's nice, right? But maybe a little too safe.")

# 3-way (dressing room comment)
if_dress_cm  = ng()
if_dress_dmd = ng()
ds_secret = L("I feel like a good girl in this... hihi. Let's see if the next one is better.")
ds_open   = L("Math just said it makes me look like a good girl... hihi. Let's see what he thinks of the next one.")
ds_nts    = L("Leo said the colors are perfect for a daytime portrait, but I want to show off a bit more.")

d34 = L("Let's turn up the heat a bit.")
d35 = L("", spr=B_5)
d36 = LM("Wow. That's definitely not safe.")
d37 = L("hihi I feel so sexy in this one. I might have to save it for a special date night.")
d38 = L("But I also found something... strictly for underneath.")
d39 = L("", spr=B_6)
d40 = LM("Oh my god...")
d41 = L("It gets better")
d42 = L("", spr=B_7)
d43 = LM("Thats crazy...")

# 3-way (lingerie reaction)
if_ling_cm  = ng()
if_ling_dmd = ng()
# secret NTR
ls1 = L("I wonder what kind of trouble this will get me into...")
ls2 = L("Should I buy this one? I have a feeling I'm going to need it before this trip is over...")
ls3 = POST(MB, POST_MB_MID)
# open NTR
lo1 = L("Math is litterly drooling…")
lo2 = LM("Did he send a drooling emoji?")
lo3 = L("No... He's like acctualy here")
lo4 = L("", spr=OPEN_DR_SELFIE)
lo5 = LM("Oh shit")
lo6 = L("Were gonna make a couple pics for the page real quick")
lo7 = L("hihi")
lo8 = LM("Well don't let me stop you guys")
lo9 = L("But should I buy it?")
# NTS
ln1 = L("Leo is practically begging me to let him paint me in this...")
ln2 = L("Should I buy it? I have a feeling I'm going to need it before this trip is over...")

d44 = LM("Buy it. Immediately.")
d45 = L("Consider it done.")
d46 = L("By the way, crazy switch but... did you hear from Dave or Lisa this morning?")

# 2-way (Dave/Lisa news)
if_news_tag = ng()
# oblivious (tag_along == false)
ob1 = LM("No why?")
ob2 = L("I heard there was like a big storm...")
ob3 = LM("Oh shit... Il tekst him right away")
ob4 = D("We're fine, but holy shit dude. It was terrifying.")
ob5 = D("The waves were tossing that massive yacht around like it was a plastic toy. We had to bunker down in the captain's cabin for hours.")
ob6 = DM("That sounds insane. I'm glad you guys are safe.")
ob7 = D("I genuinely thought we were going down at one point.")
ob8 = D("Totally killed the mood for the night. We barely made it back to the marina in one piece.")
ob9 = D("We got back to the hotel a few hours ago. Lisa is completely out cold.")
ob10 = D("We are dead tired bro. Gonna sleep the entire day. Talk later.")
ob11 = DM("Get some rest, man.")
ob_dave_msg = DM("Yo bro. Lily just told me about a storm? Are you guys okay?!")
# already knew (tag_along == true && lisa_know == false)
ak1 = LM("Yeah, Dave actually texted me about it late last night while it was happening.")
ak2 = L("Oh my god! Are they okay?!")
ak3 = LM("They're fine. They had to bunker down in the captain's cabin for a few hours while the boat got tossed around, but they made it back to the marina safe.")
ak4 = L("That sounds absolutely terrifying...")
ak5 = LM("Dave said it totally killed the mood for their night. He told me he's basically going to sleep the entire day.")
ak6 = L("I don't blame him! I'm just glad everyone is okay.")
ak7 = LM("Me too. Just enjoy your shopping trip today, babe.")
ak8 = LM("Love you")
ak9 = L("Love you too baby")

# post block (CUCK NTR / secret NTR posts)
if_postblk_cm  = ng()
if_postblk_dmd = ng()
# open NTR / cuck (CM && !DMD)
cuck_post = POST(ML, POST_ML_END)
cuck_tl = TL("30 minutes later...", LILY)
cu1 = L("Did you see the post")
cu2 = LM("I did")
cu3 = L("A lot more happened but... Math says you cant see")
cu4 = LM("oh")
cu5 = L("But you're still my husband so…")
cu6 = L("", vid=CUCK_VID, thumb=CUCK_VID_THUMB)
cu7 = LM("Fuck...")
cu8 = L("Now I gotte go..")
cu9 = L("Love you")
cu10 = LM("Love you too")
# secret NTR post (CM && DMD)
secret_post = POST(MB, POST_MB_END)

# ── [E] Boat path (tag_along && lisa_know) ─────────────────────────────────────
e_tl  = TL("4 hours later...", DAVE)
e1 = D("Bro!")
e2 = D("Finally got a signal again")
e3 = DM("Dave! Thank god. Are you guys okay?!")
e4 = D("Yeah, we're safe.")
e5 = D("But holy fuck dude, that was terrifying")
e6 = D("I was genuinely shitting bricks. The waves were tossing this massive yacht around like it was a plastic toy.")
e7 = D("The crew rushed us all up to the captain's cabin to bunker down.")
e8 = D("", spr=DAVELISAMIA_CABIN)
e9 = DM("Glad to see you guys in one piece.")
e10 = D("Captain says the worst of it is over. We're slowly making our way back to the marina.")
e11 = DM("That's a huge relief.")
e12 = DM("How is Lily holding up? She told me she was getting really scared before the signal cut out.")
e13 = D("...")
e14 = D("Bro.")
e15 = D("That's the thing.")
e16 = DM("What?")
e17 = DM("Dave!, what's wrong!?")
e18 = D("Lily isn't up here with us.")
e19 = DM("What do you mean she isn't up there?!")

# lily_join_lisa branch (Dave's account)
if_boat_ljl = ng()
bjl_t1 = D("Lisa burst into the cabin right after me and Mia. But she was alone.")
bjl_t2 = D("I asked her where Lily was, since they were both down there together doing... you know.")
bjl_t3 = D("Lisa said when the boat violently lurched, the crew yelled to get upstairs. Lisa ran for the stairs, but she thought Lily was right behind her.")
bjl_t4 = D("Bro, I think she's still trapped below deck with the husband.")
bjl_f1 = D("When the crew started yelling for everyone to get upstairs, it was complete chaos. The deck was slipping everywhere.")
bjl_f2 = D("I grabbed Mia, and I swear I thought Lily was right behind me.")
bjl_f3 = D("But when they locked the cabin door... she wasn't here.")
bjl_f4 = D("Bro, I have no idea where she went. She must still be below deck.")

e20 = DM("Fuck...")
e21 = DM("Is she safe?! Can you go look for her?")
e22 = D("The captain told us not to leave the cabin until we are fully docked...")
e23 = D("The hallways are probably still tilting like crazy.")

# NTR vs NTS
if_boat_cm = ng()

# [E1] NTR
ntr1 = DM("Dave, please. You have to go find her. I need to know she's okay.")
ntr2 = D("Ugh, alright. Fuck.")
ntr3 = D("Give me a minute. I'm going to wait for the captain to look the other way and slip out.")
ntr4 = DM("Be careful man.")
ntr_tl1 = TL("10 minutes later...", DAVE)
ntr5 = D("Okay, I slipped out of the captain's cabin.")
ntr6 = D("Bro, the interior stairs to the lower deck are locked down by the crew.")
ntr7 = D("I couldn't get down there from the inside.")
ntr8 = DM("So what are you doing?")
ntr9 = D("I had to use the side hatch to get out onto the lower exterior walkway.")
ntr10 = D("It is slippery as fuck out here, dude. The wind is still going crazy.")
ntr11 = D("I'm holding onto the railing for dear life.")
ntr12 = DM("Be careful bro!")
ntr13 = D("I'm trying. I'm inching my way along the side of the lower VIP suites.")
ntr14 = D("", spr=NTR_1)
ntr15 = D("One of them has the lights on. I'm gonna look inside.")
ntr16 = DM("Is she in there?!")
ntr17 = D("Wait.")
ntr18 = D("I think I found them.")
ntr19 = DM("Is she okay?!")
ntr20 = D("", spr=NTR_2)
ntr21 = D("She's... safe from the storm, I guess.")
ntr22 = D("But bro.")
ntr23 = D("", spr=NTR_3)
ntr24 = D("They're making out.")
ntr25 = D("Like, really going at it. He's practically swallowing her face.")
ntr26 = DM("Fuck...")
ntr27 = D("What the fuck do I do, bro?!")
ntr28 = D("Do I bang on the glass? Do I try to find a door and break in?")
ntr29 = D("Tell me what to do!")
ntr30 = DM("No, don't intervene.")
ntr31 = D("Are you serious?!")
ntr32 = D("Your wife is swapping spit with this guy while we're in the middle of a storm!")
ntr33 = DM("Think about it, Dave. If you bang on the glass or cause a scene right now....")
ntr34 = DM("He owns this boat. He has his own crew. We're in the middle of the ocean.")
ntr35 = DM("Just let it be… please")
ntr36 = D("You're insane.")
ntr37 = D("But fine. I'm freezing my ass off and gripping this rail too hard to fight a billionaire anyway.")
ntr38 = D("I'll just go back to the cabin")
ntr39 = DM("WAIT!")
ntr40 = DM("Can you still see whats going on?")
ntr41 = D("Bro I'm risking my live out here")
ntr42 = D("Il see what I can do...")
ntr_tl2 = TL("5 minutes later...", DAVE)
ntr43 = D("Bro… Things are escalating fast.")
ntr44 = D("He didn't even bother taking his pants fully off. Just unzipped.")
ntr45 = D("He picked her up and set her on the edge of the mahogany desk.")
ntr46 = D("", spr=NTR_4)
ntr47 = DM("She didn't stop him...")
ntr48 = D("She didn't even try, bro. She's pulling him closer.")
ntr49 = D("Fuck, he's just shoving it in.")
ntr50 = D("", spr=NTR_35)
ntr51 = D("I can't hear a single thing over the wind and the waves.")
ntr52 = D("But the way her eyes are rolling back... she looks like she's losing her mind.")
ntr53 = DM("She's taking all of it.")
ntr54 = D("Yeah....")
ntr_tl3 = TL("2 minutes later...", DAVE)
ntr55 = D("FUCK!")
ntr56 = DM("What?! What happened?!")
ntr57 = D("I slipped. The boat tilted hard and I lost my footing.")
ntr58 = D("My boot slammed right into the metal siding under the window. It was loud as fuck bro.")
ntr59 = DM("Did they hear you?!")
ntr60 = D("I ducked down instantly. I'm pressing myself against the deck.")
ntr61 = D("Give me a second... I'm going to peek back up.")
ntr_tl4 = TL("2 minutes later...", DAVE)
ntr62 = D("Bro...")
ntr63 = D("", spr=NTR_5)
ntr64 = D("They covered the window.")
ntr65 = D("He definitely heard it. He shut the curtains.")
ntr66 = DM("Fuck... Now we can't see anything.")
ntr67 = D("I'm completely locked out. And I'm freezing.")
ntr68 = D("I'm heading back to the side hatch before one of his crew members comes out here to check the noise.")
ntr69 = D("I can't stay here.")
ntr70 = DM("Go. Get back to the cabin. Be safe.")
# MC tries Lily directly
ntrL1 = LM("Lily?")
ntrL2 = LM("Dave said you're stuck down there. Are you okay? It must be so scary being all alone in the dark.")
ntr_tlL1 = TL("5 minutes later...", LILY)
ntrL3 = LM("Lily, please answer me. I'm worried about you being by yourself.")
ntr_tlL2 = TL("10 minutes later...", LILY)
ntrL4 = LM("Just tell me you're safe!")
ntr_tlL3 = TL("No response... You smirk at the screen, knowing exactly how \"alone\" she really is behind those curtains.", LILY)
ntr_tlD = TL("15 minutes later...", DAVE)
ntr71 = D("Made it back to the cabin. I'm soaking wet and shivering.")
ntr72 = D("The engines just shifted pitch. The captain said we're entering the marina.")
ntr73 = D("Lisa and Mia are getting ready to head out to the main deck.")
ntr74 = D("We're docking now.")
ntr75 = D("I'll text you when we're off this floating nightmare.")

# [E2] NTS
nts1 = DM("Don't risk it Dave. It's too dangerous to walk around if the boat is still rocking.")
nts2 = D("You sure? I can try to sneak out.")
nts3 = DM("No, stay put. The husband is down there somewhere, he wouldn't just let her get hurt.")
nts4 = DM("I'm going to try texting her directly again.")
nts5 = D("Aight bro. Let me know if she answers. I'm stressing out here.")
nts6 = LM("Lily!")
nts7 = LM("Dave said you aren't in the captain's cabin!")
nts8 = LM("Where are you?! Are you safe?")
nts_tl1 = TL("2 minutes later...", LILY)
nts9 = L("Baby!")
nts10 = L("Yes, I'm safe... I finally have a signal again.")
nts11 = LM("Thank god. Where are you?")
nts12 = L("I'm... below deck.")
# lily_join_lisa branch (Lily's account)
if_nts_ljl = ng()
njl_t1 = L("When the alarm went off and the boat tilted, Lisa ran for the door.")
njl_t2 = L("I tried to follow her, but...")
njl_t3 = L("He grabbed my wrist.")
njl_t4 = L("We were already doing so much before the storm hit... I was already so worked up.")
njl_t5 = L("And then suddenly Lisa was gone, the heavy door was locked, and the boat started tossing everywhere.")
njl_t6 = L("The fear of the storm is just mixing with how turned on I already was.")
njl_t7 = L("It's so intense baby.")
njl_f1 = L("When everyone was rushing upstairs, I lost my balance.")
njl_f2 = L("I slipped near the stairs and fell... it was so scary.")
njl_f3 = L("But then he was just there.")
njl_f4 = L("He caught me, picked me up, and carried me down into this secure room. He locked the heavy door so the water wouldn't get in.")
njl_f5 = LM("Are you hurt?")
njl_f6 = L("No... he was actually really gentle when he caught me.")
njl_f7 = L("But now I'm just... trapped down here. With him.")

nts13 = LM("It's okay Lily, the storm is passing. Dave said the captain is taking the boat back to the marina.")
nts14 = L("I know... but the boat is still rocking so much.")
nts15 = L("And it's so quiet in here.")
nts16 = L("He hasn't spoken a single word to me.")
nts17 = L("He's just standing there. Watching me text you.")
nts18 = LM("What is he doing?")
nts19 = L("He just took his shirt off.")
nts20 = L("Baby... my heart is pounding so fast. I'm terrified of the storm, but...")
nts21 = L("Being locked in here with him...")
nts22 = L("It's making me feel something else.")
nts23 = LM("Use that feeling, Lily. Let him take control.")
nts24 = LM("Focus on him, not the rocking of the boat. Give yourself to him.")
nts25 = L("You want me to? Baby, my hands are trembling.")
nts26 = L("He just pointed at me. He wants me to undress.")
nts27 = LM("Do it for him. Show me.")
# NTS_1 picture MISSING -> omitted
nts28 = L("He stepped closer. God, he is so dominant. He just pulled me against him.")
nts29 = L("", spr=NTS_2)
nts30 = LM("You're doing so good, baby. Let him use your mouth to wait out the storm.")
nts31 = L("", spr=NTS_3)
nts32 = L("He just pulled me up. He lifted me right onto the mahogany desk.")
# NTS_4 picture MISSING -> omitted
nts33 = LM("Take him, Lily. Take all of him for me.")
nts34 = L("He's sliding it in. Fuck!")
nts35 = L("", vid=NTS_VID, thumb=NTS_VID_THUMB)
nts36 = LM("Fuck, you look so beautiful taking him like that.")
# NTS_5 (biting lip) picture MISSING -> omitted
nts37 = L("He's getting close baby... I can feel him tensing up.")
nts38 = L("He pulled out.")
# NTS_6 (cum face) picture MISSING -> omitted
nts39 = LM("Perfect. You were perfect.")
nts40 = L("The engine sound just changed. We must be getting into the marina.")
nts41 = L("He just zipped up his pants, handed me a warm towel, and walked out of the room. Still hasn't said a single word.")
nts42 = L("Thank you for guiding me baby. I don't know what I would have done without you talking me through that panic.")
nts43 = L("I'm going to get dressed and head up to the main deck to find Dave and Lisa.")

# ── [F] Marina convergence ─────────────────────────────────────────────────────
f_tl = TL("The storm breaks. The yacht finally docks at the marina.", DAVE)
# (Lisa marina social post — no asset, omitted)
f1 = D("We're off the boat. Standing on the dock now.")
f2 = D("", spr=MARINA_1)
f3 = D("The adrenaline is completely crashing. I feel like I could sleep for a week.")
f4 = DM("Is everyone together?")
# NTR vs NTS marina
if_marina_cm = ng()
fn1 = D("Yeah. Lily just came up from below deck.")
fn2 = D("", spr=MARINA_2)
fn3 = D("Bro... I can't even look her in the eye.")
fn4 = D("She's standing there adjusting her hair, acting like she was just hiding from the storm.")
fn5 = D("I feel sick knowing what we just watched her do.")
fn6 = DM("Just keep it together Dave. Don't say anything to her.")
fn7 = D("I won't. I just want to get back to the hotel.")
fs1 = D("Yeah, Lily just came up.")
fs2 = D("", spr=MARINA_2)
fs3 = D("She looks a little flushed, but she says she's okay. Thank god.")
fs4 = D("Lisa has this crazy smug look on her face though. Mia is just sipping water like nothing happened.")
fs5 = D("Rich people are weird, bro.")

# dave_lily block
if_dl = ng()
dl1 = SA("Hey. We made it to land.")
dl2 = SAM("Dave told me. I'm so glad everyone is safe.")
dl3 = SA("Yeah, we're safe.")
dl4 = SA("But I just wanted to tell you...")
dl5 = SA("Don't think the storm washed away what was happening before all hell broke loose.")
dl6 = SA("Me, Dave, and Lily... we have unfinished business.")
dl7 = SA("It's just postponed. (;")
dl8 = SAM("I understand. Get some sleep, Lisa.")
dl_evt = EVT(EVT_DAVELILY_PENDING)

f5 = D("We just got a taxi.")
f6 = D("It is dead silent in here. Everyone is completely drained.")
f7 = DM("Get some rest, man. We'll talk tomorrow.")

# ── [G] Lily taxi chat (convergence of shopping + boat) ────────────────────────
g1 = L("We're in the taxi baby.")
g2 = L("I am so physically and emotionally exhausted.")
g3 = L("I can barely keep my eyes open.")
g4 = L("I'm just going to collapse into bed as soon as we get to the room.")
g5 = LM("You earned it. Sleep well, Lily. I love you.")
g6 = L("I love you too. Tomorrow is a new day.")

# ── [H] End of day ─────────────────────────────────────────────────────────────
h_tl = TL("At the end of the day…", LILY)
h1 = L("I'm in bed baby...")
h2 = L("I'm so tired")
h3 = L("", spr=LILY_SLEEP)
h4 = L("I love you")
h5 = LM("Hey!")
h6 = LM("You look sleepy baby")
h7 = LM("I love you too")

# =============================================================================
# WIRE LINKS
# =============================================================================

# ── [A] opening post (CM && DMD) ───────────────────────────────────────────────
links.append((g_start, if_a_cm))
ifn("Continue_math", 0, "", if_a_dmd, if_b_tag, preset_guid=if_a_cm, after_guid=g_start)
ifn("Docter_Medicine_denied", 0, "", a_post, if_b_tag, preset_guid=if_a_dmd, after_guid=if_a_cm)
links.append((a_post, if_b_tag))

# ── [B] top split ──────────────────────────────────────────────────────────────
ifn("mc_dave_lisa_tag_along_ep24", 0, "", if_b_lk, d_izzy, preset_guid=if_b_tag, after_guid=if_a_dmd)
ifn("Lisa_know_cuckhold", 0, "", e_tl, c_tl, preset_guid=if_b_lk, after_guid=if_b_tag)
#   TAG true -> if_b_lk; LK true -> boat (e_tl); LK false -> dave late chat (c_tl) -> shopping
#   TAG false -> shopping (d_izzy)

# ── [C] Dave late-night chat -> shopping ───────────────────────────────────────
chain([c_tl, c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12, c13, c14])
links.append((c14, d_izzy))

# ── [D] shopping ───────────────────────────────────────────────────────────────
chain([d_izzy, d_tl1, d1, d2, d3, d4, d5, d6, d7, d8, d9, d10, d11, d12, d13, d14,
       d15, d16, d17, d18, d19, d20, d21, d22, d23, d24, d25])
# 3-way send
links.append((d25, if_send_cm))
ifn("Continue_math", 0, "", if_send_dmd, n1, preset_guid=if_send_cm, after_guid=d25)
ifn("Docter_Medicine_denied", 0, "", s1, o1, preset_guid=if_send_dmd, after_guid=if_send_cm)
chain([s1, s2, s3, s4, s5]); links.append((s5, d26))
chain([o1, o2, o3, o4]);     links.append((o4, d26))
chain([n1, n2]);             links.append((n2, d26))
# continue
chain([d26, d27, d_tl2, d28, d29, d30, d31, d32, d33])
# 3-way dressing
links.append((d33, if_dress_cm))
ifn("Continue_math", 0, "", if_dress_dmd, ds_nts, preset_guid=if_dress_cm, after_guid=d33)
ifn("Docter_Medicine_denied", 0, "", ds_secret, ds_open, preset_guid=if_dress_dmd, after_guid=if_dress_cm)
links.append((ds_secret, d34)); links.append((ds_open, d34)); links.append((ds_nts, d34))
# continue
chain([d34, d35, d36, d37, d38, d39, d40, d41, d42, d43])
# 3-way lingerie
links.append((d43, if_ling_cm))
ifn("Continue_math", 0, "", if_ling_dmd, ln1, preset_guid=if_ling_cm, after_guid=d43)
ifn("Docter_Medicine_denied", 0, "", ls1, lo1, preset_guid=if_ling_dmd, after_guid=if_ling_cm)
chain([ls1, ls2, ls3]); links.append((ls3, d44))
chain([lo1, lo2, lo3, lo4, lo5, lo6, lo7, lo8, lo9]); links.append((lo9, d44))
chain([ln1, ln2]); links.append((ln2, d44))
# continue
chain([d44, d45, d46])
# 2-way Dave/Lisa news
links.append((d46, if_news_tag))
ifn("mc_dave_lisa_tag_along_ep24", 0, "", ak1, ob1, preset_guid=if_news_tag, after_guid=d46)
# oblivious branch (tag false)
chain([ob1, ob2, ob3, ob_dave_msg, ob4, ob5, ob6, ob7, ob8, ob9, ob10, ob11])
links.append((ob11, if_postblk_cm))
# already knew branch (tag true && lk false)
chain([ak1, ak2, ak3, ak4, ak5, ak6, ak7, ak8, ak9])
links.append((ak9, if_postblk_cm))
# post block
ifn("Continue_math", 0, "", if_postblk_dmd, g1, preset_guid=if_postblk_cm, after_guid=ob11)
ifn("Docter_Medicine_denied", 0, "", secret_post, cuck_post, preset_guid=if_postblk_dmd, after_guid=if_postblk_cm)
# CM&&!DMD -> cuck block
chain([cuck_post, cuck_tl, cu1, cu2, cu3, cu4, cu5, cu6, cu7, cu8, cu9, cu10])
links.append((cu10, g1))
# CM&&DMD -> secret post
links.append((secret_post, g1))
# (CM false -> g1 directly, handled by if_postblk_cm false target)

# ── [E] boat path ──────────────────────────────────────────────────────────────
chain([e_tl, e1, e2, e3, e4, e5, e6, e7, e8, e9, e10, e11, e12, e13, e14, e15, e16,
       e17, e18, e19])
links.append((e19, if_boat_ljl))
ifn("ep24.5_lily_join_lisa", 0, "", bjl_t1, bjl_f1, preset_guid=if_boat_ljl, after_guid=e19)
chain([bjl_t1, bjl_t2, bjl_t3, bjl_t4]); links.append((bjl_t4, e20))
chain([bjl_f1, bjl_f2, bjl_f3, bjl_f4]); links.append((bjl_f4, e20))
chain([e20, e21, e22, e23])
links.append((e23, if_boat_cm))
ifn("Continue_math", 0, "", ntr1, nts1, preset_guid=if_boat_cm, after_guid=e23)
# [E1] NTR
chain([ntr1, ntr2, ntr3, ntr4, ntr_tl1, ntr5, ntr6, ntr7, ntr8, ntr9, ntr10, ntr11,
       ntr12, ntr13, ntr14, ntr15, ntr16, ntr17, ntr18, ntr19, ntr20, ntr21, ntr22,
       ntr23, ntr24, ntr25, ntr26, ntr27, ntr28, ntr29, ntr30, ntr31, ntr32, ntr33,
       ntr34, ntr35, ntr36, ntr37, ntr38, ntr39, ntr40, ntr41, ntr42, ntr_tl2, ntr43,
       ntr44, ntr45, ntr46, ntr47, ntr48, ntr49, ntr50, ntr51, ntr52, ntr53, ntr54,
       ntr_tl3, ntr55, ntr56, ntr57, ntr58, ntr59, ntr60, ntr61, ntr_tl4, ntr62, ntr63,
       ntr64, ntr65, ntr66, ntr67, ntr68, ntr69, ntr70,
       ntrL1, ntrL2, ntr_tlL1, ntrL3, ntr_tlL2, ntrL4, ntr_tlL3,
       ntr_tlD, ntr71, ntr72, ntr73, ntr74, ntr75])
links.append((ntr75, f_tl))
# [E2] NTS
chain([nts1, nts2, nts3, nts4, nts5, nts6, nts7, nts8, nts_tl1, nts9, nts10, nts11, nts12])
links.append((nts12, if_nts_ljl))
ifn("ep24.5_lily_join_lisa", 0, "", njl_t1, njl_f1, preset_guid=if_nts_ljl, after_guid=nts12)
chain([njl_t1, njl_t2, njl_t3, njl_t4, njl_t5, njl_t6, njl_t7]); links.append((njl_t7, nts13))
chain([njl_f1, njl_f2, njl_f3, njl_f4, njl_f5, njl_f6, njl_f7]); links.append((njl_f7, nts13))
chain([nts13, nts14, nts15, nts16, nts17, nts18, nts19, nts20, nts21, nts22, nts23,
       nts24, nts25, nts26, nts27, nts28, nts29, nts30, nts31, nts32, nts33, nts34,
       nts35, nts36, nts37, nts38, nts39, nts40, nts41, nts42, nts43])
links.append((nts43, f_tl))

# ── [F] marina convergence ─────────────────────────────────────────────────────
chain([f_tl, f1, f2, f3, f4])
links.append((f4, if_marina_cm))
ifn("Continue_math", 0, "", fn1, fs1, preset_guid=if_marina_cm, after_guid=f4)
chain([fn1, fn2, fn3, fn4, fn5, fn6, fn7]); links.append((fn7, if_dl))
chain([fs1, fs2, fs3, fs4, fs5]);           links.append((fs5, if_dl))
# dave_lily block
ifn("Dave_Lily_path", 0, "", dl1, f5, preset_guid=if_dl, after_guid=fn7)
chain([dl1, dl2, dl3, dl4, dl5, dl6, dl7, dl8, dl_evt])
links.append((dl_evt, f5))
chain([f5, f6, f7])
links.append((f7, g1))

# ── [G] Lily taxi chat ─────────────────────────────────────────────────────────
chain([g1, g2, g3, g4, g5, g6])

# ── [H] end of day ─────────────────────────────────────────────────────────────
chain([g6, h_tl, h1, h2, h3, h4, h5, h6, h7, g_end])

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
    "  m_Name: Episode 25\n"
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
    "  SpyNodeDatas: []\n"
)

import os
out_path = os.path.join(os.path.dirname(__file__), "Episode 25.asset")
with open(out_path, "w", encoding="utf-8") as f:
    f.write(out)
print(f"Written {len(out)} bytes  ->  {out_path}")
print(f"Nodes: {len(cnodes)} choice | {len(dnodes)} dialogue | "
      f"{len(enodes)} event | {len(ifnodes)} if | {len(stnodes)} start | {len(endnodes)} end")
print(f"Links: {len(links)}")
