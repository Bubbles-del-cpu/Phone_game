import uuid
from collections import defaultdict

def ng(): return str(uuid.uuid4())

# ── Character GUIDs ────────────────────────────────────────────────────────────
VIC = "4633184ded7444943962e7d570a2109f"   # Victoria (conversation partner)

# ── Sprite GUIDs (Gallery/ep26) ────────────────────────────────────────────────
LOVE_3 = "4b1548174c3f06f40b777e603b7934c3"
LOVE_4 = "0e310ab723f081044a40924db751946b"
LOVE_5 = "09a1db688b9f96049a4cc69e92b8558f"
LOVE_6 = "bcf3d20bf6a084f4ab500f6382f26c72"
LOVE_7 = "9bec8146189c5714bbaa1c91fbc25a54"
EXTORT_1 = "afadeaadca6da894689dd351c764fcf5"
EXTORT_2 = "23256bfd163caaf44ba28541e95275dd"
EXTORT_3 = "e2dc9d3a82647534399497686105f7d3"
EXTORT_4 = "a05b94698306b2741a2335746c4494c2"
EXTORT_6 = "567fdea51fa576440959f2d1659305d3"
EXTORT_7_MOUTH = "534a343e92b33ab4398ad8439d03fa99"
EXTORT_7_PUSSY = "dd44cb6dcdaf9b144ac156bc49183ff3"
EXTORT_8 = "47ab100787a99c745b49c726bdddc275"
EXTORT_9 = "ed0c2ab82f8b7494eb3527a34d4e9df6"

# ── Video GUIDs ─────────────────────────────────────────────────────────────────
LOVE_VID = "daf51a30ec880bd4db6990f3ed48ac66"
EXTORT_VID = "1ea2e9cb062edd444983ab835098ba7b"
EXTORT_VID_THUMB = "a8dc8e1e0d9ba8e4b9235c008475f035"

# ── Social Post GUIDs ────────────────────────────────────────────────────────────
POST_NECKLACE = "310432cf4b2991f4a839935b01ce541c"
POST_NONECKLACE = "2820a8da6b45a9749acc3be8d1a81f40"

# ── Event SO GUIDs (variable setters) ─────────────────────────────────────────────
EVT_ENDPATH_TRUE  = "0492f0363e51ad44dba9d912b1cb4319"   # victoria_ending_path = 1
EVT_ENDPATH_FALSE = "2b628a064193447184d8cabfd2ec8abe"   # victoria_ending_path = 0
EVT_CLIMAX_MOUTH  = "8a655c852f4d3d646a3d5b9fb589bb4f"   # victoria_alley_climax_mouth = 1
EVT_CLIMAX_PUSSY  = "179aec7adb6f4f1a83f2c5fdf6bcabcf"   # victoria_alley_climax_mouth = 0
EVT_BFEAT_TRUE    = "8eb1b695330a6674997a8e3eca3b5019"   # vic_bf_eat = 1
EVT_BFEAT_FALSE   = "8983786d9fbc47029072ac7dfedfe9d7"   # vic_bf_eat = 0

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
    """ports = list of english strings (choice options / player messages)"""
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

# ── Shorthand helpers ─────────────────────────────────────────────────────────
def V(en="", spr=None, vid=None, thumb=None, post=None, gal=0):
    """Victoria (NPC) message / media / social post -> dialogue node (left side)."""
    return npc(VIC, en, "", spr=spr, vid=vid, thumb=thumb, post=post, gal=gal)

def TL(text):
    """Timelapse divider node."""
    return npc(VIC, "", text)

def MC(en=""):
    """MC (player) text message -> single-option choice node (right side)."""
    return mc2(VIC, [en])

def MM(spr=None, vid=None, thumb=None, gal=0):
    """MC-sent media. The engine only supports media on dialogue nodes (rendered
    as a chat media bubble), so MC-sent pictures are stored as dialogue nodes."""
    return npc(VIC, "", "", spr=spr, vid=vid, thumb=thumb, gal=gal)

def CH(opts):
    """Branching choice (2+ options)."""
    return mc2(VIC, opts)

def EVT(guid): return evt(guid)
def END():     return end()
def START():   return start()

# =============================================================================
# NODE DEFINITIONS
# =============================================================================

g_start = START()
g_if_homecare = ng()   # AT_HOME_CARE gate
g_if_extort   = ng()   # victoria_extort branch
g_end_fallback = END() # home_care == false safety end

# =============================================================================
# LOVE PATH  (victoria_extort == false)
# =============================================================================
love_tl   = TL("Early Afternoon...")
v_hey     = V("Hey...")
v_car     = V("I just got to my car.")
v_walked  = V("I know I literally just walked out your door five minutes ago...")
v_miss    = V("But I miss you already.")

ch_miss = CH(["I miss you too, Victoria. You did a really great job today.",
              "Already? You barely just pulled out of the driveway."])
v_thankyou = V("Thank you...")
v_happy    = V("That makes me so happy to hear.")
v_sorry1   = V("I know... I'm sorry.")
v_enjoyed  = V("I just really enjoyed our session today.")

v_progress = V("I'm just so happy with your progress.")
v_weight   = V("You're putting so much more weight on your leg now.")
v_pushing  = V("I... I hope I wasn't pushing you too hard with the stretches.")
mc_handled = MC("Not at all. I handled them perfectly fine.")
mc_reward  = MC("Knowing what my reward was going to be definitely gave me some extra motivation.")
v_oh1      = V("Oh...")
v_haha1    = V("Haha... yes...")
v_cramped  = V("I think my hand is actually still a little cramped.")
mc_complain= MC("You didn't seem to be complaining while you were stroking me.")
v_wasnt    = V("I wasn't! I... I really enjoyed it.")
v_uniform  = V("I didn't plan on doing that while still in my uniform, though...")
v_looked   = V("But you just looked at me...")
v_melted   = V("And my professional boundaries kind of just... melted.")
v_sudden   = V("I'm sorry if it was a bit sudden...")
mc_never   = MC("Never apologize for that. I loved it.")
mc_pics1   = MC("I actually took some pictures while you were taking care of me.")
v_youdid   = V("You did...?")
v_gosh     = V("Oh gosh...")
v_letsee   = V("Let me see? If that's okay...")
# (Picture LOVE_1 - MC-sent - IMAGE MISSING, node omitted)
v_omg1     = V("Oh my god...")
v_eager    = V("I... I look so eager for you.")
v_nametag  = V("I can't believe my name tag is visible in that...")
mc_perfect1= MC("You look perfect. And you took care of it so incredibly well.")
# (Picture LOVE_2 - MC-sent - IMAGE MISSING, node omitted)
v_ohwow1   = V("Oh... wow...")
v_mess1    = V("I look like such a mess...")
v_happymess= V("But a happy mess.")
v_lovedsec = V("I loved every second of it.")
v_didgood  = V("Did I do good for you today...?")
mc_amazing1= MC("You did amazing. Best physical therapy session I've ever had.")
v_glad1    = V("I'm so glad...")
mc_mentioned= MC("Now, about what I mentioned before you left...")
mc_takeout = MC("Since my leg is doing so well, I think it's time I take my favorite nurse out properly.")
mc_clear   = MC("Clear your schedule for tonight.")
v_wait1    = V("Wait... really?")
v_realdate = V("Like... a real date?")
mc_fancy   = MC("A real, fancy date. I want to treat you.")
v_omg2     = V("Oh my god...")
v_excited  = V("I am so excited...")
v_racing   = V("My heart is literally racing right now.")
v_longdate = V("I... I haven't been on a real date in so long.")
v_dresscode= V("What is the dress code...? Where are we going?")
mc_surprise= MC("It's a surprise. Just dress to impress.")
v_ohokay   = V("Oh! Okay...")
v_tearcloset= V("I am going to tear my closet apart right now.")
v_givebit  = V("Give me a little bit to put something together, please?")
mc_taketime= MC("Take your time. Send me a picture when you find something.")
v_iwill1   = V("I will!")

love_tl2   = TL("1 hour later...")
v_love3    = V("", spr=LOVE_3)
v_warzone  = V("I might have torn my closet apart a little too much...")
mc_warzone = MC("Looks like a warzone.")
v_itis     = V("It is! But I want everything to be perfect.")
v_drcall   = V("Actually... Dr. Weber just called me while I was trying things on.")
v_paperwork= V("He asked me to come into the office tonight to file some late paperwork.")
v_toldno   = V("But I told him no! Because I'm going out with you.")

ch_react = CH(["How did he react to that?",
               "Good girl. He doesn't own your free time."])
v_annoyed  = V("He was pretty annoyed... grumbling about \"employee loyalty\" and dedication.")
v_veins    = V("I just imagined the veins popping on his shiny bald head and almost laughed on the phone.")
v_doesntmtr= V("It doesn't matter anyway. The only thing that matters to me tonight is you.")
v_definitely= V("He definitely doesn't. Tonight is just for us.")

v_okay_d   = V("Okay...")
v_foundone = V("I think I finally found the one...")
v_nervous1 = V("I'm a little nervous, to be honest...")
v_love4    = V("", spr=LOVE_4)
v_whatthink= V("What do you think...?")
v_nice4hero= V("Is this nice enough for my hero?")
v_toomuch  = V("Or is it too much...? I can change if it's too much...")

ch_dress = CH(["You look absolutely breathtaking, Victoria.",
               "It looks amazing, but I'm mostly thinking about how I'm going to take it off you later."])
v_reallythink= V("You really think so...?")
v_thankbetter= V("Thank you so much... that makes me feel so much better.")
v_oh2      = V("Oh...")
v_hahawell = V("Haha... well...")
v_ifwant   = V("If you want to... I think I'd really like that.")

v_makeup   = V("I'm going to finish getting my makeup ready...")
v_see7     = V("I'll see you at 7:00! ❤️")

date_tl    = TL("Later that evening, at dinner...")
ch_neck = CH(["Give her the diamond necklace.",
              "Just enjoy the dinner."])
evt_neck_true  = EVT(EVT_ENDPATH_TRUE)
evt_neck_false = EVT(EVT_ENDPATH_FALSE)

if_post = ng()
v_post_neck   = V("", post=POST_NECKLACE)
v_post_noneck = V("", post=POST_NONECKLACE)

after_tl   = TL("3 hours later...")
v_hey2     = V("Hey...")
mc_bathroom= MC("You're texting me from my own bathroom?")
v_sorry2   = V("I know... I'm sorry.")
v_waitbed  = V("Are you waiting for me in bed...?")
mc_iam1    = MC("I am. You've been in there for ten minutes, Victoria. What's taking so long?")
v_surprise1= V("I'm sorry! I'm just getting your surprise ready...")
mc_another = MC("Another surprise?")

if_ad = ng()
v_ad_spoil1   = V("You just... you spoiled me so much tonight.")
v_ad_dinnerneck= V("The dinner... the beautiful necklace...")
v_ad_under1   = V("I felt so underdressed next to how amazing you were.")
v_ad_spoil2   = V("You just... you spoiled me so much tonight.")
v_ad_dinner2  = V("The dinner was so amazing...")
v_ad_under2   = V("I felt so underdressed next to how amazing you were.")

v_return   = V("So I wanted to make sure I gave you something special in return...")
v_bought   = V("I bought this specifically for tonight.")
v_hopelike = V("I hope you like it...")
v_love5    = V("", spr=LOVE_5)
v_isitokay = V("Is it okay...?")
mc_wow1    = MC("Wow... Victoria.")
mc_morethan= MC("It's more than okay.")
mc_getout  = MC("Stop texting and get out here right now.")
v_okay_l   = V("Okay...")
v_coming1  = V("I'm coming... ❤️")

night_tl   = TL("2:30 AM...")
v_hey3     = V("Hey...")
v_frontdoor= V("I just walked through my front door.")
mc_safe    = MC("You made it back safe. I miss you in my bed already.")
v_misstoo  = V("I miss you too... so much.")
v_cologne  = V("I can still smell your cologne on my skin...")
v_tonightwas= V("Tonight was...")
v_nowords  = V("I don't even have the words for it.")
mc_dontneed= MC("You don't need them. I had an amazing time with you.")
v_special  = V("You made me feel so special...")
v_neveranyone= V("I've never had anyone look at me the way you did across that table.")
v_dreaming = V("I felt like I was dreaming.")

if_night = ng()
v_n_neck1     = V("And I haven't taken the necklace off...")
v_n_sleepin   = V("I'm going to sleep in it tonight.")
mc_n_beautiful= MC("It looks beautiful on you.")
mc_n_highlight1= MC("But coming back to my place was definitely the highlight of the night for me.")
mc_n_glad     = MC("I'm glad you enjoyed it as much as I did.")
mc_n_highlight2= MC("But coming back to my place was definitely the highlight of the night for me.")

v_reallywas= V("It really was...")
v_feltright= V("Everything just felt so right with you.")
v_feltsafe = V("I felt so safe.")
v_deep     = V("I wanted to feel you as deep as possible...")
v_okvideo  = V("Is it okay if I send you a video we took...?")
mc_ofcourse_v= MC("Of course. Send it.")
v_lovevid  = V("", vid=LOVE_VID)
mc_god1    = MC("God... watching that brings it all right back. You were so tight.")
v_couldnt  = V("I... I couldn't stop looking into your eyes.")
v_justus   = V("It felt like it was just the two of us in the whole world.")
mc_love6   = MM(spr=LOVE_6)
v_rightbefore= V("Oh thats right before we started")
v_lookshy  = V("God I'm looking shy")
mc_cute    = MC("It was cute")
v_connection= V("I've never felt a connection like that before...")
v_didntend = V("I didn't want it to end.")
v_finishinside= V("And I really loved feeling you finish inside me...")
v_warmperfect= V("It felt so warm... and perfect.")
v_love7    = V("", spr=LOVE_7)
mc_driveover= MC("You're making me want to drive over there and bring you right back.")
v_tempting = V("Haha... that's very tempting...")
v_legrest  = V("But I think your leg really needs to rest after all the \"exercises\" we did tonight!")
v_overwork = V("I don't want to overwork my favorite patient...")
v_thankagain= V("Thank you again for the most perfect evening.")
v_myhero   = V("You really are my hero.")
v_loveyou  = V("I love you... so much.")
mc_loveyoutoo= MC("I love you too, Victoria. Sleep well.")
v_goodnight= V("I will... goodnight. ❤️")
end_love   = END()

# =============================================================================
# EXTORT PATH  (victoria_extort == true)
# =============================================================================
ex_tl      = TL("Early Afternoon...")
ev_car     = V("I... I just got to my car.")
ev_leaving = V("I'm leaving your driveway now...")
emc_swallow= MC("Did you swallow all of your practice material today?")
ev_yes1    = V("I... yes.")
ev_exactly = V("I did exactly what you told me to do.")
emc_good1  = MC("Good. I expect my nurse to follow all of my medical instructions.")
emc_document= MC("I took some pictures to document your progress.")
ev_pics1   = V("You took pictures again...?")
ev_dontshow= V("Please don't show those to anyone...")
emc_depends= MC("That depends entirely on your behavior, Victoria.")
emc_extort1= MM(spr=EXTORT_1)
emc_enjoy  = MC("You look like you're actually starting to enjoy your mandatory training.")
ev_satisfied= V("I... I just want to make sure you're satisfied.")
ev_dontsend= V("So you don't send anything to him...")
emc_extort2= MM(spr=EXTORT_2)
emc_goodgirl1= MC("You took it like a good girl today.")
ev_thank1  = V("Thank you...")
ev_um1     = V("Um...")
ev_favor   = V("Since I did so well today... I was wondering if I could ask you for a favor...?")
emc_noask  = MC("You don't ask me for favors. But go ahead.")
ev_sorryknow= V("I'm sorry... I know.")
ev_bday    = V("It's just... tonight is my boyfriend's birthday.")
ev_reserv  = V("He made reservations at a really nice restaurant for us weeks ago.")
ev_nightoff= V("I was hoping... if it's okay with you... that I could just have the night off?")
ev_justtonight= V("Just for tonight...?")
emc_romantic= MC("A birthday dinner. How romantic.")
emc_cango  = MC("You can go.")
ev_really1 = V("Oh... really?!")
ev_makeup1 = V("Thank you so much! I promise I'll make it up to you tomorrow—")
emc_notfinished= MC("I wasn't finished, Victoria.")
ev_ohsorry = V("...Oh. I'm sorry.")
emc_pickoutfit= MC("You can go to dinner with him. But I am picking your outfit.")
emc_blackdress= MC("That black dress you wore last week. The one that barely covers your thighs.")
emc_nopanties= MC("And no panties.")
emc_marker = MC("But before you put it on, take a black marker. Write \"Slut\" on your ass.")
ev_what1   = V("What?!")
ev_bleed   = V("Please, no... what if it bleeds through the fabric? What if he sees it when I'm changing later?!")
emc_hope   = MC("You better hope he doesn't.")
emc_45min  = MC("You have 45 minutes to send me the picture of the writing, Victoria.")
ev_okay_e1 = V("Okay...")
ev_dosorry = V("I'll do it. I'm sorry.")

ex_tl2     = TL("45 minutes later...")
ev_extort3 = V("", spr=EXTORT_3)
ev_didit1  = V("I did it.")
emc_perfect_e= MC("Perfect. Now put the dress on.")
ev_extort4 = V("", spr=EXTORT_4)
ev_waiting1= V("He's waiting for me downstairs.")
ev_exposed = V("I feel so exposed... knowing what's written under here. The fabric is riding up every time I take a step.")
emc_phonelap= MC("Keep your phone on your lap under the table.")
ev_okay_e2 = V("Okay...")

rest_tl    = TL("8:00 PM, at the restaurant...")
ev_satdown = V("We just sat down...")
ev_beautiful= V("He keeps telling me how beautiful I look, but he noticed how short the dress is.")
ev_pulldown= V("He asked me to pull it down. I told him I couldn't.")
emc_goodgirl2= MC("Good girl. Send me a picture of the birthday boy.")
ev_cantpic = V("I... I can't take a picture right now, the waiter is here.")
emc_now1   = MC("I said send me a picture, Victoria. Now.")
ev_okaymad = V("Okay... please don't be mad.")
# (Picture EXTORT_5 - V-sent - IMAGE MISSING, node omitted)
emc_noidea = MC("He has no idea what a slut his girlfriend really is.")
ev_dontcall= V("Please don't call me that...")
emc_freshair= MC("Tell him you need some fresh air.")
emc_backexit= MC("Go out the back exit into the alley behind the kitchen.")
ev_alleywhy= V("The alley? Why...?")
emc_parked = MC("Because I'm parked in it.")
emc_twomin = MC("You have two minutes before I walk into the main dining room and show him my phone.")
ev_here1   = V("What?! You're here?!")
ev_pleaseno1= V("Oh my god... please, no...")
ev_comingstay= V("Okay! I'm coming! Please just stay outside!")

alley_tl   = TL("In the alley behind the restaurant...")
emc_extort6= MM(spr=EXTORT_6)
emc_extortvid= MM(vid=EXTORT_VID, thumb=EXTORT_VID_THUMB)

ch_climax = CH(["Cum in her mouth", "Cum inside her pussy"])
evt_climax_mouth = EVT(EVT_CLIMAX_MOUTH)   # true
evt_climax_pussy = EVT(EVT_CLIMAX_PUSSY)    # false

if_climaxpic = ng()
emc_extort7_mouth= MM(spr=EXTORT_7_MOUTH)
emc_extort7_pussy= MM(spr=EXTORT_7_PUSSY)

bath_tl    = TL("15 minutes later...")
ev_locked  = V("I'm locked in the restaurant bathroom...")
ev_shaking = V("My hands are shaking so badly I can barely type.")
ev_breathing= V("I can't get my breathing under control. I feel so sick...")
ev_anyone  = V("Anyone could have walked out that door and seen us.")
emc_didnt  = MC("But they didn't. You took it perfectly. My cock is still throbbing.")

if_bath = ng()
ev_b_smell = V("I'm terrified he's going to smell it on my breath... I've washed my mouth three times but I can still taste you.")
emc_b_stopwash= MC("Stop washing. I want you to sit back down at that table tasting like me.")
ev_b_clean = V("I'm trying to clean myself up with toilet paper, but it keeps dripping out of me. I'm terrified it's going to run down my leg at the table.")
emc_b_stopwipe= MC("Stop wiping. I want it inside you when you sit back across from him.")

ev_textedwhy= V("He texted asking why I was gone so long and why my makeup was smudged.")
ev_toldfood= V("I told him the food wasn't sitting right.")
emc_good2  = MC("Good.")
emc_celebrate= MC("Now, about how he's going to celebrate his birthday when you go back to his place tonight.")
ev_intimate= V("He said he wanted to be intimate tonight.")
ev_tootired= V("Please... can I just tell him I'm too tired? I'm so exhausted and sore from what you just did to me.")

ch_push = CH(["Force him to eat it.", "Sloppy seconds."])
evt_bf_true  = EVT(EVT_BFEAT_TRUE)
evt_bf_false = EVT(EVT_BFEAT_FALSE)

# vic_bf_eat == true
emc_eat_no = MC("No. When you get back to his place, you are going to spread your legs and make him eat you out.")
ev_eat_what= V("WHAT?!")
ev_eat_beg = V("No! Please... I'm begging you!")
if_eat_climax = ng()
ev_eat_m_know= V("He'll know something is wrong! My mouth still tastes like you, I can't kiss him to lead him down there!")
emc_eat_m_figure= MC("Figure it out. Tell him you want him to focus on your pussy for his birthday.")
ev_eat_p_know= V("He'll know! You finished inside of me! He'll taste your cum!")
emc_eat_p_wet= MC("Tell him you're extra wet for his birthday.")
ev_eat_ruin= V("Please, don't make me do this... it's going to ruin everything.")
emc_eat_send= MC("If you don't do exactly what I say, I am sending the video from the alley to his phone right now.")
ev_eat_dots= V("...")
ev_eat_monster= V("You are a monster.")
ev_eat_dolife= V("I'll do it. I'll make him do it. Please just don't ruin his life.")
emc_eat_goodgirl= MC("Good girl. Text me when he's down there.")

# vic_bf_eat == false (sloppy seconds)
emc_sec_no = MC("No. You are going to let him fuck you.")
if_sec_climax = ng()
emc_sec_m_taste= MC("I want you to let him fuck you while you still have my taste in your mouth.")
ev_sec_m_sore= V("That's disgusting... I'm so sore from the alley, I can't pretend this is romantic for him.")
emc_sec_p_full= MC("I want you to feel him inside you and know that you are taking him while you're still completely full of my cum.")
ev_sec_p_squish= V("That's disgusting... He'll feel the squish... he'll know I'm completely blown out.")
ev_sec_sleep= V("Please, just let me go to sleep. I've done enough...")
emc_sec_do = MC("Do it, or the alley video goes to his phone.")
ev_sec_dots= V("...")
ev_sec_pretend= V("I'll do it. I'll just close my eyes and pretend it's over.")
emc_sec_goodgirl= MC("Good girl. Text me when he's done.")

late_tl    = TL("1:00 AM...")
if_1am = ng()
# eat == true
ev_doing   = V("He's doing it...")
ev_extort8 = V("", spr=EXTORT_8)
ev_overwhelmed= V("He thinks I'm just looking at my phone because I'm overwhelmed.")
ev_tasted  = V("He said I tasted different... I told him it was just my perfume.")
emc_prove  = MC("Prove it.")
emc_seeeat = MC("I want to see him eating my cum.")
ev_cant1   = V("I can't!")
ev_pointcam= V("If I point the camera down, he might look up and see the screen!")
emc_doit1  = MC("Do it, Victoria. Or I send him the alley video right now while he's between your legs.")
ev_dots_1am= V("...")
ev_please1 = V("Please...")
ev_extort9 = V("", spr=EXTORT_9)
emc_obedient= MC("Good girl. I knew you were an obedient nurse.")
ev_crying1 = V("I was crying the entire time.")
ev_stopped = V("He finally stopped... he kissed me, swallowed it, and smiled at me.")
ev_broken1 = V("I feel so entirely broken.")
ev_belong1 = V("I belong to you now. I know that.")
# eat == false
ev_asleep  = V("He's asleep.")
emc_followed= MC("Did you follow my instructions?")
ev_yeslet  = V("Yes. I let him do it.")
if_1am_climax = ng()
ev_1am_m   = V("Every time he kissed me while he was thrusting, I could still taste you.")
ev_1am_p   = V("Every time he thrust into me, I could feel your mess squishing inside of me.")
ev_humiliating= V("It was the most humiliating thing I've ever experienced.")
ev_broken2 = V("I feel so entirely broken.")
ev_belong2 = V("I belong to you now. I know that.")

emc_nextshift= MC("I'll see you for your next shift, Victoria. Don't be late.")
ev_seethen = V("Yes... I'll see you then.")
end_extort = END()

# =============================================================================
# WIRE LINKS
# =============================================================================

# ── Entry gates ──────────────────────────────────────────────────────────────
links.append((g_start, g_if_homecare))
ifn("AT_HOME_CARE", 0, "", g_if_extort, g_end_fallback, preset_guid=g_if_homecare, after_guid=g_start)
ifn("victoria_extort", 0, "", ex_tl, love_tl, preset_guid=g_if_extort, after_guid=g_if_homecare)

# ── LOVE PATH ────────────────────────────────────────────────────────────────
chain([love_tl, v_hey, v_car, v_walked, v_miss])
links.append((v_miss, ch_miss))
links.append((ch_miss, v_thankyou))   # port 0
links.append((ch_miss, v_sorry1))     # port 1
chain([v_thankyou, v_happy]);  links.append((v_happy, v_progress))
chain([v_sorry1, v_enjoyed]);  links.append((v_enjoyed, v_progress))

chain([v_progress, v_weight, v_pushing, mc_handled, mc_reward, v_oh1, v_haha1,
       v_cramped, mc_complain, v_wasnt, v_uniform, v_looked, v_melted, v_sudden,
       mc_never, mc_pics1, v_youdid, v_gosh, v_letsee, v_omg1, v_eager, v_nametag,
       mc_perfect1, v_ohwow1, v_mess1, v_happymess, v_lovedsec, v_didgood,
       mc_amazing1, v_glad1, mc_mentioned, mc_takeout, mc_clear, v_wait1, v_realdate,
       mc_fancy, v_omg2, v_excited, v_racing, v_longdate, v_dresscode, mc_surprise,
       v_ohokay, v_tearcloset, v_givebit, mc_taketime, v_iwill1])

chain([v_iwill1, love_tl2, v_love3, v_warzone, mc_warzone, v_itis, v_drcall,
       v_paperwork, v_toldno])
links.append((v_toldno, ch_react))
links.append((ch_react, v_annoyed))     # port 0
links.append((ch_react, v_definitely))  # port 1
chain([v_annoyed, v_veins, v_doesntmtr]); links.append((v_doesntmtr, v_okay_d))
links.append((v_definitely, v_okay_d))

chain([v_okay_d, v_foundone, v_nervous1, v_love4, v_whatthink, v_nice4hero, v_toomuch])
links.append((v_toomuch, ch_dress))
links.append((ch_dress, v_reallythink))  # port 0
links.append((ch_dress, v_oh2))          # port 1
chain([v_reallythink, v_thankbetter]); links.append((v_thankbetter, v_makeup))
chain([v_oh2, v_hahawell, v_ifwant]);  links.append((v_ifwant, v_makeup))

chain([v_makeup, v_see7, date_tl])
links.append((date_tl, ch_neck))
links.append((ch_neck, evt_neck_true))   # port 0
links.append((ch_neck, evt_neck_false))  # port 1
links.append((evt_neck_true, if_post))
links.append((evt_neck_false, if_post))
ifn("victoria_ending_path", 0, "", v_post_neck, v_post_noneck, preset_guid=if_post, after_guid=evt_neck_true)
links.append((v_post_neck, after_tl))
links.append((v_post_noneck, after_tl))

chain([after_tl, v_hey2, mc_bathroom, v_sorry2, v_waitbed, mc_iam1, v_surprise1, mc_another])
links.append((mc_another, if_ad))
ifn("victoria_ending_path", 0, "", v_ad_spoil1, v_ad_spoil2, preset_guid=if_ad, after_guid=mc_another)
chain([v_ad_spoil1, v_ad_dinnerneck, v_ad_under1]); links.append((v_ad_under1, v_return))
chain([v_ad_spoil2, v_ad_dinner2, v_ad_under2]);    links.append((v_ad_under2, v_return))

chain([v_return, v_bought, v_hopelike, v_love5, v_isitokay, mc_wow1, mc_morethan,
       mc_getout, v_okay_l, v_coming1])

chain([v_coming1, night_tl, v_hey3, v_frontdoor, mc_safe, v_misstoo, v_cologne,
       v_tonightwas, v_nowords, mc_dontneed, v_special, v_neveranyone, v_dreaming])
links.append((v_dreaming, if_night))
ifn("victoria_ending_path", 0, "", v_n_neck1, mc_n_glad, preset_guid=if_night, after_guid=v_dreaming)
chain([v_n_neck1, v_n_sleepin, mc_n_beautiful, mc_n_highlight1]); links.append((mc_n_highlight1, v_reallywas))
chain([mc_n_glad, mc_n_highlight2]); links.append((mc_n_highlight2, v_reallywas))

chain([v_reallywas, v_feltright, v_feltsafe, v_deep, v_okvideo, mc_ofcourse_v,
       v_lovevid, mc_god1, v_couldnt, v_justus, mc_love6, v_rightbefore, v_lookshy,
       mc_cute, v_connection, v_didntend, v_finishinside, v_warmperfect, v_love7,
       mc_driveover, v_tempting, v_legrest, v_overwork, v_thankagain, v_myhero,
       v_loveyou, mc_loveyoutoo, v_goodnight, end_love])

# ── EXTORT PATH ──────────────────────────────────────────────────────────────
chain([ex_tl, ev_car, ev_leaving, emc_swallow, ev_yes1, ev_exactly, emc_good1,
       emc_document, ev_pics1, ev_dontshow, emc_depends, emc_extort1, emc_enjoy,
       ev_satisfied, ev_dontsend, emc_extort2, emc_goodgirl1, ev_thank1, ev_um1,
       ev_favor, emc_noask, ev_sorryknow, ev_bday, ev_reserv, ev_nightoff,
       ev_justtonight, emc_romantic, emc_cango, ev_really1, ev_makeup1,
       emc_notfinished, ev_ohsorry, emc_pickoutfit, emc_blackdress, emc_nopanties,
       emc_marker, ev_what1, ev_bleed, emc_hope, emc_45min, ev_okay_e1, ev_dosorry])

chain([ev_dosorry, ex_tl2, ev_extort3, ev_didit1, emc_perfect_e, ev_extort4,
       ev_waiting1, ev_exposed, emc_phonelap, ev_okay_e2])

chain([ev_okay_e2, rest_tl, ev_satdown, ev_beautiful, ev_pulldown, emc_goodgirl2,
       ev_cantpic, emc_now1, ev_okaymad, emc_noidea, ev_dontcall, emc_freshair,
       emc_backexit, ev_alleywhy, emc_parked, emc_twomin, ev_here1, ev_pleaseno1,
       ev_comingstay])

chain([ev_comingstay, alley_tl, emc_extort6, emc_extortvid])
links.append((emc_extortvid, ch_climax))
links.append((ch_climax, evt_climax_mouth))  # port 0 mouth
links.append((ch_climax, evt_climax_pussy))  # port 1 pussy
links.append((evt_climax_mouth, if_climaxpic))
links.append((evt_climax_pussy, if_climaxpic))
ifn("victoria_alley_climax_mouth", 0, "", emc_extort7_mouth, emc_extort7_pussy, preset_guid=if_climaxpic, after_guid=evt_climax_mouth)
links.append((emc_extort7_mouth, bath_tl))
links.append((emc_extort7_pussy, bath_tl))

chain([bath_tl, ev_locked, ev_shaking, ev_breathing, ev_anyone, emc_didnt])
links.append((emc_didnt, if_bath))
ifn("victoria_alley_climax_mouth", 0, "", ev_b_smell, ev_b_clean, preset_guid=if_bath, after_guid=emc_didnt)
chain([ev_b_smell, emc_b_stopwash]); links.append((emc_b_stopwash, ev_textedwhy))
chain([ev_b_clean, emc_b_stopwipe]); links.append((emc_b_stopwipe, ev_textedwhy))

chain([ev_textedwhy, ev_toldfood, emc_good2, emc_celebrate, ev_intimate, ev_tootired])
links.append((ev_tootired, ch_push))
links.append((ch_push, evt_bf_true))   # port 0 eat
links.append((ch_push, evt_bf_false))  # port 1 seconds

# eat == true
links.append((evt_bf_true, emc_eat_no))
chain([emc_eat_no, ev_eat_what, ev_eat_beg])
links.append((ev_eat_beg, if_eat_climax))
ifn("victoria_alley_climax_mouth", 0, "", ev_eat_m_know, ev_eat_p_know, preset_guid=if_eat_climax, after_guid=ev_eat_beg)
chain([ev_eat_m_know, emc_eat_m_figure]); links.append((emc_eat_m_figure, ev_eat_ruin))
chain([ev_eat_p_know, emc_eat_p_wet]);    links.append((emc_eat_p_wet, ev_eat_ruin))
chain([ev_eat_ruin, emc_eat_send, ev_eat_dots, ev_eat_monster, ev_eat_dolife, emc_eat_goodgirl])
links.append((emc_eat_goodgirl, late_tl))

# eat == false
links.append((evt_bf_false, emc_sec_no))
links.append((emc_sec_no, if_sec_climax))
ifn("victoria_alley_climax_mouth", 0, "", emc_sec_m_taste, emc_sec_p_full, preset_guid=if_sec_climax, after_guid=emc_sec_no)
chain([emc_sec_m_taste, ev_sec_m_sore]);  links.append((ev_sec_m_sore, ev_sec_sleep))
chain([emc_sec_p_full, ev_sec_p_squish]); links.append((ev_sec_p_squish, ev_sec_sleep))
chain([ev_sec_sleep, emc_sec_do, ev_sec_dots, ev_sec_pretend, emc_sec_goodgirl])
links.append((emc_sec_goodgirl, late_tl))

# 1:00 AM
links.append((late_tl, if_1am))
ifn("vic_bf_eat", 0, "", ev_doing, ev_asleep, preset_guid=if_1am, after_guid=late_tl)
# eat == true
chain([ev_doing, ev_extort8, ev_overwhelmed, ev_tasted, emc_prove, emc_seeeat,
       ev_cant1, ev_pointcam, emc_doit1, ev_dots_1am, ev_please1, ev_extort9,
       emc_obedient, ev_crying1, ev_stopped, ev_broken1, ev_belong1])
links.append((ev_belong1, emc_nextshift))
# eat == false
chain([ev_asleep, emc_followed, ev_yeslet])
links.append((ev_yeslet, if_1am_climax))
ifn("victoria_alley_climax_mouth", 0, "", ev_1am_m, ev_1am_p, preset_guid=if_1am_climax, after_guid=ev_yeslet)
links.append((ev_1am_m, ev_humiliating))
links.append((ev_1am_p, ev_humiliating))
chain([ev_humiliating, ev_broken2, ev_belong2])
links.append((ev_belong2, emc_nextshift))

chain([emc_nextshift, ev_seethen, end_extort])

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
    "  m_Name: Episode 26\n"
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
out_path = os.path.join(os.path.dirname(__file__), "Episode 26.asset")
with open(out_path, "w", encoding="utf-8") as f:
    f.write(out)
print(f"Written {len(out)} bytes  ->  {out_path}")
print(f"Nodes: {len(cnodes)} choice | {len(dnodes)} dialogue | "
      f"{len(enodes)} event | {len(ifnodes)} if | {len(stnodes)} start | {len(endnodes)} end")
print(f"Links: {len(links)}")
