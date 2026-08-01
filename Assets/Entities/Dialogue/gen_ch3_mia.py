import uuid
from collections import defaultdict

def ng(): return str(uuid.uuid4())

# ── Character GUIDs ───────────────────────────────────────────────────────────
MIA  = "c6231bd7adaacaa4192f0c0a864957e5"
KIRA = "783ebf452ba92764ba0ef9c0e6d89cb1"

# ── Variable/Event GUIDs ──────────────────────────────────────────────────────
EVT_KIRA_TRUE  = "03e558d48e89818418f228e1e8f7f56b"
EVT_KIRA_FALSE = "d443e4179c90b9a4cab4c9133e856789"

# ── Social Post GUIDs ─────────────────────────────────────────────────────────
P_MIA_NOTES = "5da879b8574beb44293804bc5b3e1997"
P_KIRA_BUS  = "54f91dcff9ca4e34f8da5d737855c6c3"

# ── Sprite GUIDs ──────────────────────────────────────────────────────────────
S_KIRA_DOOR     = "a5a83fc8adc8b334c8d3b8fcda851f1e"
S_KIRA_DOGGY    = "f2a563f9cc68c1f47a7fb63d2bcd60f8"
S_KIRA_FACIAL   = "da6c46d6047f32d4a9a325995b0b816e"
S_KIRA_NIGHT    = "669220c331944cb48a0c0c3a52e632e3"
S_KIRA_FACEDOWN = "a147d76a4feddc649890d64446c838a9"
S_KIRA_SLEEPY   = "c2c569e55c6e54241bf4af2405bd754b"
S_KIRA_PEELING  = "5bdb2ec0550b3094db54ced5eef3e933"
S_KIRA_LECTURE  = "5a50787d2e9fe4a469859b5d22a4196b"
S_KIRA_BUS_SPR  = "525ea226bca5f74408baa3207d107d34"
S_KIRA_MIA_BCH  = "32d9a073b5c47624cb5de9017db4925d"
S_MIA_DORM_OUT  = "e2d67a000ebd89049b36893a9abffde3"
S_MIA_NOTE      = "797cb13abe957ee4fad580792003f999"
S_MIA_LECTURE   = "efd7f67c5b5b31342bde42a780fe2c44"
S_MIA_SCREENSHOT= "c65d1c60cd579ab459f62412a3d8290b"
S_JACK_SELFIE   = "2d98657296e7d3c42a5dc93556cf1f68"

V_KIRA_FIT    = "76201bb640f06c247b88529080f38e98"
V_KIRA_FIT_TH = "e814a140d168a3b46acda75a357fd09a"

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

def _l5(en="", ja="", nl="", fr="", es=""):
    vals = [_safe(en), _safe(ja), _safe(nl), _safe(fr), _safe(es)]
    rows = ""
    for i, v in enumerate(vals):
        rows += f"    - languageEnum: {i}\n      LanguageGenericType: {v}\n"
    return rows

def _l5p(en="", ja="", nl="", fr="", es=""):
    vals = [_safe(en), _safe(ja), _safe(nl), _safe(fr), _safe(es)]
    rows = ""
    for i, v in enumerate(vals):
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
def npc(char, en="", ja="", nl="", fr="", es="",
        tl_en="", tl_ja="", tl_nl="", tl_fr="", tl_es="",
        spr=None, vid=None, thumb=None, post=None, gal=0):
    g = ng()
    post_ref = f"{{fileID: 11400000, guid: {post}, type: 2}}" if post else "{fileID: 0}"
    md = _mdata(spr, vid, thumb, gal, "      ")
    dnodes.append(
        f"  - NodeGuid: {g}\n"
        f"    Position: {{x: {px_store(g)}, y: 0}}\n"
        f"    DialogueNodePorts: []\n"
        f"    AudioClips:\n{_audio()}"
        f"    Character: {{fileID: 11400000, guid: {char}, type: 2}}\n"
        f"    AvatarPos: 0\n    AvatarType: 0\n"
        f"    Texts:\n{_l5(en, ja, nl, fr, es)}"
        f"    Timelapses:\n{_l5(tl_en, tl_ja, tl_nl, tl_fr, tl_es)}"
        f"    Timelapse: \n    Duration: 2\n    Delay: 0\n"
        f"    MediaData:\n{md}\n"
        f"    Post: {post_ref}\n    DelayTimer: 0\n"
    )
    return g

def mc2(char, ports, npc_en="", npc_ja="", npc_nl="", npc_fr="", npc_es="",
        port_sprs=None, port_vids=None, port_thumbs=None, port_gals=None):
    """ports = list of (en, ja, nl, fr, es) tuples"""
    g = ng()
    n = len(ports)
    if port_sprs   is None: port_sprs   = [None]*n
    if port_vids   is None: port_vids   = [None]*n
    if port_thumbs is None: port_thumbs = [None]*n
    if port_gals   is None: port_gals   = [0]*n
    ports_yaml = ""
    pgs = []
    for i, pd in enumerate(ports):
        pg = ng()
        pgs.append(pg)
        p_en, p_ja, p_nl, p_fr, p_es = (list(pd) + [""]*5)[:5]
        is_media = 1 if (port_sprs[i] or port_vids[i]) else 0
        md = _mdata(port_sprs[i], port_vids[i], port_thumbs[i], port_gals[i], "        ")
        ports_yaml += (
            f"    - PortGuid: {pg}\n"
            f"      InputGuid: __IGUID_{pg}__\n"
            f"      OutputGuid: {g}\n"
            f"      TextLanguage:\n{_l5p(p_en, p_ja, p_nl, p_fr, p_es)}"
            f"      HintLanguage:\n"
        )
        for j in range(5):
            ports_yaml += f"      - languageEnum: {j}\n        LanguageGenericType: \n"
        ports_yaml += f"      MediaData:\n{md}\n      IsMediaPort: {is_media}\n"
    port_pgs[g] = pgs
    req = 1 if _safe(npc_en) else 0
    cnodes.append(
        f"  - NodeGuid: {g}\n"
        f"    Position: {{x: {px_store(g)}, y: 0}}\n"
        f"    DialogueNodePorts:\n{ports_yaml}"
        f"    AudioClips:\n{_audio()}"
        f"    Character: {{fileID: 11400000, guid: {char}, type: 2}}\n"
        f"    AvatarPos: 0\n    AvatarType: 0\n"
        f"    Texts:\n{_l5(npc_en, npc_ja, npc_nl, npc_fr, npc_es)}"
        f"    Duration: 2\n"
        f"    RequireCharacterInput: {req}\n"
        f"    SelectedChoice: []\n    ChoiceIndex: 0\n"
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
def M(en, ja="", nl="", fr="", es="", spr=None, tl=None, post=None, gal=0):
    t = tl if tl else ("","","","","")
    return npc(MIA, en,ja,nl,fr,es, t[0],t[1],t[2],t[3],t[4], spr=spr, post=post, gal=gal)

def K(en, ja="", nl="", fr="", es="", spr=None, tl=None, post=None, gal=0, vid=None, thumb=None):
    t = tl if tl else ("","","","","")
    return npc(KIRA, en,ja,nl,fr,es, t[0],t[1],t[2],t[3],t[4], spr=spr, vid=vid, thumb=thumb, post=post, gal=gal)

def MC1(en, ja="", nl="", fr="", es="", char=MIA, spr=None, vid=None, thumb=None, gal=0,
        npc_en="", npc_ja="", npc_nl="", npc_fr="", npc_es=""):
    return mc2(char, [(en,ja,nl,fr,es)], npc_en,npc_ja,npc_nl,npc_fr,npc_es,
               port_sprs=[spr], port_vids=[vid], port_thumbs=[thumb], port_gals=[gal])

def EVT(guid): return evt(guid)
def END():     return end()
def START():   return start()

# =============================================================================
# NODE DEFINITIONS
# =============================================================================

g_start = START()

# ── kira_path IF: pre-assign so we can reference it ──────────────────────────
g_if_kira_start = ng()

# ── Night scene (kira_path == true only) ─────────────────────────────────────
g_tl_night   = K("", tl=("That night...", "その夜…", "Die avond…", "Ce soir-là…", "Esa noche…"))
g_kira_door  = K("Open up!", "開けて！", "Doe open!", "Ouvre !", "¡Abre!", spr=S_KIRA_DOOR)
g_tl_2h_a    = K("", tl=("2 hours later...", "2時間後…", "2 uur later…", "2 heures plus tard…", "2 horas después…"))
g_mc_doggy   = MC1("", char=KIRA, spr=S_KIRA_DOGGY)
g_mc_facial  = MC1("", char=KIRA, spr=S_KIRA_FACIAL)
g_kira_night = K("night", "おやすみ", "nacht", "nuit", "buenas noches", spr=S_KIRA_NIGHT)

# ── MIA morning — both paths ──────────────────────────────────────────────────
g_tl_morn      = M("", tl=("The next morning...", "翌朝…", "De volgende ochtend…", "Le lendemain matin…", "A la mañana siguiente…"))
g_mc_penpal1   = MC1("Hey penpal", "やあ、ペンパル", "Hey penpal", "Hey correspondante", "Hola amiga")
g_mc_quiet     = MC1("You okay? You went quiet on me last night",
                     "大丈夫？昨夜急に黙っちゃったけど",
                     "Alles goed? Je werd gisteravond stil",
                     "Ça va ? Tu t'es tue hier soir",
                     "¿Estás bien? Anoche te quedaste callada")
g_tl_10m       = M("", tl=("10 minutes later...", "10分後…", "10 minuten later…", "10 minutes plus tard…", "10 minutos después…"))
g_mia_hi       = M("...hi", "…こんにちは", "…hoi", "…salut", "…hola")
g_mia_sorry1   = M("Sorry", "ごめん", "Sorry", "Désolée", "Lo siento")
g_mia_minute   = M("I just needed a minute", "少し時間が必要だっただけ", "Ik had even een moment nodig", "J'avais juste besoin d'un moment", "Solo necesitaba un momento")
g_mc_hours     = MC1("More like a few hours haha", "数時間じゃないの笑", "Meer zoals een paar uur haha", "Plutôt quelques heures haha", "Más bien unas cuantas horas jaja")
g_mia_sorry2   = M("I know... sorry", "わかってる…ごめん", "Ik weet het… sorry", "Je sais… désolée", "Lo sé… lo siento")
g_mc_alright   = MC1("Don't apologize. You alright though?", "謝らなくていい。大丈夫？", "Maak je geen zorgen. Alles goed?", "Ne t'excuse pas. Ça va quand même ?", "No te disculpes. ¿Estás bien?")
g_mia_sleep    = M("I didn't really sleep", "あまり眠れなかった", "Ik heb niet echt geslapen", "Je n'ai pas vraiment dormi", "No dormí mucho")
g_mia_woke     = M("Also I woke up to this", "それにこれで目が覚めた", "En ik werd hiermee wakker", "Et je me suis réveillée avec ça", "Y me desperté con esto")
g_mia_kira_fd  = M("", spr=S_KIRA_FACEDOWN)
g_mc_facedown  = MC1("haha did she just fall face down and die", "笑 うつぶせに倒れてそのまま？", "haha viel ze gewoon voorover en stierf", "haha elle est juste tombée face contre terre et elle est morte", "jaja ¿se cayó de bruces y ya?")
g_mia_basically= M("Basically", "まあね", "Min of meer", "En gros", "Básicamente")

# ── Kira path IF for Mia morning extra content ───────────────────────────────
g_if_kira_mia_morn = ng()

# kira_path == true extra
g_mia_late1    = M("She came back really late...", "彼女、本当に遅く帰ってきたんだ…", "Ze kwam heel laat terug…", "Elle est rentrée vraiment tard…", "Volvió muy tarde…")
g_mia_late2    = M("Like I woke up at 3 and she still wasn't home", "3時に起きたらまだ帰ってなかった", "Ik werd om 3 uur wakker en ze was nog niet thuis", "Je me suis réveillée à 3h et elle n'était toujours pas là", "Me desperté a las 3 y todavía no había llegado")
g_mc_wow       = MC1("Wow", "へえ", "Wauw", "Wow", "Vaya")
g_mc_bignight  = MC1("Big night", "大きな夜だったね", "Grote nacht", "Grande soirée", "Gran noche")
g_mia_guess1   = M("I guess", "そうかな", "Denk het wel", "J'imagine", "Supongo")
g_mia_weird    = M("Weird thing though", "でもおかしいんだけど", "Maar iets is raar", "Mais il y a un truc bizarre", "Pero hay algo raro")
g_mc_what      = MC1("What", "何？", "Wat", "Quoi", "¿Qué?")
g_mia_noalc    = M("She doesn't even smell like alcohol", "アルコールの匂いも全然しない", "Ze ruikt niet eens naar alcohol", "Elle ne sent même pas l'alcool", "Ni siquiera huele a alcohol")
g_mia_atall    = M("Like at all", "全然まったく", "Helemaal niet", "Pas du tout", "Para nada")
g_mia_allnight = M("For someone who was at a party all night", "一晩中パーティーにいた人なのに", "Voor iemand die de hele nacht op een feestje was", "Pour quelqu'un qui était à une soirée toute la nuit", "Para alguien que estuvo en una fiesta toda la noche")
g_mia_soap     = M("She smells like... soap?", "彼女の匂いは…石鹸？", "Ze ruikt naar… zeep?", "Elle sent… le savon ?", "Huele a… ¿jabón?")
g_mc_hm        = MC1("Hm", "ふむ", "Hm", "Hm", "Hm")
g_mc_sobered   = MC1("Maybe she sobered up", "もしかして酔いが覚めたのかも", "Misschien was ze nuchter geworden", "Peut-être qu'elle avait décuvé", "Quizás se le pasó la borrachera")
g_mia_guess2   = M("I guess", "そうかな", "Denk het wel", "J'imagine", "Supongo")
g_mia_stillwd  = M("Still. Weird.", "それでも。変だよ。", "Toch. Raar.", "Quand même. Bizarre.", "Aun así. Raro.")
g_mc_overthink = MC1("Don't overthink it", "考えすぎないで", "Denk er niet te veel over na", "Ne te prends pas trop la tête", "No le des demasiadas vueltas")
g_mia_right1   = M("You're right", "そうだね", "Je hebt gelijk", "T'as raison", "Tienes razón")

# kira_path == false extra
g_mia_bar      = M("She smells like a bar", "バーみたいな匂いがする", "Ze ruikt naar een kroeg", "Elle sent le bar", "Huele a bar")

# ── Continue for both (after kira morning IF merge) ──────────────────────────
g_mc_breath    = MC1("Is she breathing", "息してる？", "Ademt ze nog?", "Elle respire ?", "¿Está respirando?")
g_mia_unfort   = M("Unfortunately yes", "残念ながらね", "Helaas wel", "Malheureusement oui", "Desafortunadamente sí")
g_mia_noise    = M("She just made a noise actually", "今ちょっと声出した", "Ze maakte net een geluid eigenlijk", "Elle vient de faire un bruit d'ailleurs", "Acaba de hacer un ruido de hecho")
g_mc_human     = MC1("A human noise?", "人間っぽい音？", "Een menselijk geluid?", "Un bruit humain ?", "¿Un ruido humano?")
g_mia_barely   = M("Barely", "かろうじてね", "Nauwelijks", "À peine", "Apenas")
g_mc_letsleep  = MC1("Let her sleep", "寝かせておいて", "Laat haar slapen", "Laisse-la dormir", "Déjala dormir")
g_mc_keptup    = MC1("What kept you up though", "でも何で眠れなかったの？", "Maar wat hield jou wakker", "Mais qu'est-ce qui t'a tenue éveillée", "Pero ¿qué te mantuvo despierta?")
g_mia_dots1    = M("...", "…", "…", "…", "…")
g_mia_noteimg  = M("", spr=S_MIA_NOTE)
g_mia_sitting  = M("Its just sitting there", "ただそこに置いてある", "Het ligt daar gewoon", "Il est juste là posé", "Está ahí sentado nada más")
g_mia_looking  = M("Looking at me", "私を見てる", "En kijkt me aan", "Et me regarde", "Mirándome")
g_mc_haha1     = MC1("haha", "笑", "haha", "haha", "jaja")
g_mc_oneway    = MC1("I think there's only one way to make it stop",
                     "止める方法は一つしかないと思う",
                     "Ik denk dat er maar één manier is om het te stoppen",
                     "Je pense qu'il n'y a qu'une façon de l'arrêter",
                     "Creo que solo hay una manera de que pare")
g_mia_dontthink= M("I don't want to think about it", "考えたくない", "Ik wil er niet aan denken", "Je veux pas y penser", "No quiero pensar en eso")
g_mc_staring   = MC1("It will just keep staring at you though",
                     "でもずっとこっちを見てくるよ",
                     "Het blijft je gewoon aanstaren",
                     "Il va continuer à te fixer",
                     "Seguirá mirándote fijamente")
g_mia_dots2    = M("...", "…", "…", "…", "…")
g_mia_iknow1   = M("I know", "わかってる", "Ik weet het", "Je sais", "Lo sé")
g_mia_3times   = M("I already picked it up like 3 times this morning", "今朝もう3回拾い上げた", "Ik heb het deze ochtend al 3 keer opgepakt", "Je l'ai déjà ramassé 3 fois ce matin", "Ya lo recogí como 3 veces esta mañana")
g_mc_and1      = MC1("And?", "で？", "En?", "Et ?", "¿Y?")
g_mia_putdown  = M("And then I put it back down", "そしてまた置いた", "En dan legde ik het weer neer", "Et puis je l'ai reposé", "Y luego lo volví a poner")
g_mia_stupid1  = M("Its so stupid", "すごく馬鹿げてる", "Het is zo stom", "C'est tellement stupide", "Es tan estúpido")
g_mc_notstupid = MC1("Its not stupid", "馬鹿げてないよ", "Het is niet stom", "C'est pas stupide", "No es estúpido")
g_mia_cant     = M("I can't sleep over some note from a guy on a bench", "ベンチの男からのメモで眠れないなんて", "Ik kan niet slapen vanwege een briefje van een kerel op een bankje", "Je peux pas dormir à cause d'un mot d'un gars sur un banc", "No puedo dormir por una nota de un chico en un banco")
g_mia_IS       = M("That IS stupid", "これは確かに馬鹿げてる", "DAT is stom", "Ça c'est stupide", "Eso SÍ es estúpido")
g_mc_notused   = MC1("You're not used to it", "慣れてないだけだよ", "Je bent het niet gewend", "T'as pas l'habitude", "No estás acostumbrada")
g_mc_notsame   = MC1("That's not the same as stupid",
                     "それは馬鹿げてることとは違う",
                     "Dat is niet hetzelfde als stom",
                     "C'est pas pareil que stupide",
                     "Eso no es lo mismo que estúpido")
g_mia_dots3    = M("...", "…", "…", "…", "…")
g_mia_whatif1  = M("What if I text him and he doesn't reply", "テキストして返事がなかったら", "Wat als ik hem berich en hij antwoordt niet", "Et s'il ne répond pas si je lui écris", "Y si le escribo y no responde")
g_mc_whatif2   = MC1("What if he does", "返事が来たら？", "Wat als hij wel antwoordt", "Et s'il répond", "Y si responde")
g_mia_whatif3  = M("What if he thinks I'm weird", "変な子だと思われたら", "Wat als hij denkt dat ik raar ben", "Et s'il me trouve bizarre", "Y si piensa que soy rara")
g_mc_note_bag  = MC1("Mia. He put a note in your bag calling you cute",
                     "ミア。彼はバッグにメモを入れて可愛いって言った",
                     "Mia. Hij stopte een briefje in je tas en noemde je schattig",
                     "Mia. Il a mis un mot dans ton sac en te disant que t'es mignonne",
                     "Mia. Dejó una nota en tu bolsa diciéndote que eres linda")
g_mc_clearly   = MC1("He clearly wants you to text him",
                     "明らかにテキストしてほしいんだよ",
                     "Hij wil duidelijk dat je hem berichten stuurt",
                     "Il veut clairement que tu lui écrives",
                     "Claramente quiere que le escribas")
g_mia_guess3   = M("I guess", "そうかな", "Denk het wel", "J'imagine", "Supongo")
g_mia_alive    = M("Kira is alive", "キラが生きてる", "Kira leeft", "Kira est vivante", "Kira está viva", spr=S_KIRA_SLEEPY)
g_mia_pointing = M("She is pointing at the note", "メモを指差してる", "Ze wijst naar het briefje", "Elle pointe la note", "Está señalando la nota")
g_mc_agree     = MC1("haha tell her I agree with her",
                     "笑 同意してるって伝えて",
                     "haha zeg haar dat ik het met haar eens ben",
                     "haha dis-lui que je suis d'accord",
                     "jaja dile que estoy de acuerdo con ella")
g_mia_quote    = M("She says and I quote", "彼女曰く、そのまま言うと", "Ze zegt en ik citeer", "Elle dit et je cite", "Dice, y cito textualmente")
g_mia_threat   = M("\"if you don't text him I will\"",
                   "「テキストしないなら私がする」",
                   "\"als je hem niet beright doe ik het\"",
                   "\"si tu lui écris pas je le fais\"",
                   "\"si no le escribes yo lo haré\"")
g_mc_listen    = MC1("She is making a great point... I think you should listen to her",
                     "いいこと言ってる…聞いた方がいいと思うよ",
                     "Ze heeft een goed punt… ik denk dat je naar haar moet luisteren",
                     "Elle a tout à fait raison… je pense que tu devrais l'écouter",
                     "Tiene toda la razón… creo que deberías escucharla")
g_mia_asleep   = M("She fell back asleep", "また寝ちゃった", "Ze is weer in slaap gevallen", "Elle s'est rendormie", "Se volvió a dormir")
g_mia_insane   = M("She is insane", "頭おかしい", "Ze is gek", "Elle est folle", "Está loca")
g_mc_shesright = MC1("She's right though", "でも彼女は正しいよ", "Ze heeft wel gelijk", "Elle a raison quand même", "Pero tiene razón")
g_mc_twovotes  = MC1("So now you have two votes", "これで2票だよ", "Dus nu heb je twee stemmen", "Donc maintenant t'as deux votes", "Así que ahora tienes dos votos")
g_mc_outnumb   = MC1("You are outnumbered penpal", "多数決で負けてるよ、ペンパル", "Je bent in de minderheid penpal", "T'es en minorité correspondante", "Estás en minoría amiga")
g_mia_dots4    = M("...", "…", "…", "…", "…")
g_mia_fine     = M("Okay.... fine", "わかった…わかったよ", "Oké… goed dan", "Ok… bon d'accord", "Bien… está bien")
g_mia_scared   = M("What do I even say.... I'm scared",
                   "何て言えばいいの…怖い",
                   "Wat moet ik eigenlijk zeggen… ik ben bang",
                   "Qu'est-ce que je lui dis… j'ai peur",
                   "Qué le digo... tengo miedo")

# ── CHOICE: just say hi / send ass pic ───────────────────────────────────────
g_choice_text = mc2(MIA,
    [("Just say hi",
      "ただHiって言えばいい",
      "Zeg gewoon hoi",
      "Dis juste salut",
      "Solo di hola"),
     ("Send him a picture of your ass",
      "お尻の写真を送れば",
      "Stuur hem een foto van je kont",
      "Envoie-lui une photo de tes fesses",
      "Mándale una foto de tu culo")])

# Choice 1 branch
g_mia_justhi   = M("Just... hi?", "ただ…こんにちは？", "Gewoon… hoi?", "Juste… salut ?", "Solo… ¿hola?")
g_mc_simple    = MC1("Simple. You found his note, you're saying hi. That's all it needs to be",
                     "シンプルに。メモを見つけて挨拶する。それだけでいい",
                     "Simpel. Je vond zijn briefje, je zegt hoi. Meer hoeft het niet te zijn",
                     "Simple. T'as trouvé son mot, tu dis salut. C'est tout ce qu'il faut",
                     "Simple. Encontraste su nota, le dices hola. Eso es todo lo que necesitas")
g_mia_feelwrd  = M("That feels weird", "なんか変な感じ", "Dat voelt raar", "Ça fait bizarre", "Se siente raro")
g_mc_mia_ch1   = MC1("Mia", "ミア", "Mia", "Mia", "Mia")
g_mia_okok1    = M("Okay okay", "わかったわかった", "Oké oké", "Ok ok", "Bien bien")

# Choice 2 branch
g_mia_WHAT     = M("WHAT", "何てこと！", "WAT", "QUOI", "¿QUÉ?")

# Sub-choice 2.1 vs 2.2
g_choice_ass = mc2(MIA,
    [("haha I'm joking I'm joking",
      "笑 冗談冗談",
      "haha ik maak een grapje",
      "haha je plaisante je plaisante",
      "jaja estoy bromeando"),
     ("Just kidding, only send those to me of course",
      "冗談だよ、もちろん私にだけ送ればいい",
      "Grapje, stuur die alleen naar mij natuurlijk",
      "Je plaisante, envoie-les seulement à moi bien sûr",
      "Era broma, claro que solo mándamelas a mí")])

# Choice 2.1 path
g_mc_sayhi_21  = MC1("Just say hi", "ただHiって言えばいい", "Zeg gewoon hoi", "Dis juste salut", "Solo di hola")
g_mia_omg      = M("Oh my god", "おいおい", "Oh mijn god", "Oh mon dieu", "Dios mío")
g_mia_hateu    = M("I hate you", "最悪", "Ik haat je", "Je te déteste", "Te odio")
g_mia_heart    = M("My heart nearly jumped out of my chest",
                   "心臓が飛び出るかと思った",
                   "Mijn hart sprong bijna uit mijn borst",
                   "Mon cœur a failli sortir de ma poitrine",
                   "Mi corazón casi se me sale del pecho")
g_mc_sorry_haha= MC1("haha sorry sorry", "笑 ごめんごめん", "haha sorry sorry", "haha désolé désolé", "jaja perdón perdón")
g_mc_seriously = MC1("Seriously though, just say hi",
                     "でも本当に、ただHiって言えばいい",
                     "Serieus, zeg gewoon hoi",
                     "Sérieusement, dis juste salut",
                     "En serio, solo di hola")
g_mia_dots_21  = M("...", "…", "…", "…", "…")
g_mia_never    = M("Never do that again", "二度とやらないで", "Doe dat nooit meer", "Ne refais plus jamais ça", "No vuelvas a hacer eso")
g_mc_nopromise = MC1("No promises", "約束はできない", "Geen beloftes", "Pas de promesses", "No prometo nada")

# Choice 2.2 path
g_mc_sayhi_22  = MC1("Just say hi", "ただHiって言えばいい", "Zeg gewoon hoi", "Dis juste salut", "Solo di hola")
g_mia_I        = M("I", "私…", "Ik", "Je", "Yo")
g_mia_what2    = M("What...", "なに…", "Wat…", "Quoi…", "Qué…")
g_mc_hahajust  = MC1("haha just saying", "笑 ただ言っただけ", "haha gewoon gezegd", "haha je disais ça", "jaja solo lo digo")
g_mia_cantbel  = M("I can't believe you said that...",
                   "そんなこと言うなんて信じられない…",
                   "Ik kan niet geloven dat je dat zei…",
                   "Je peux pas croire que t'aies dit ça…",
                   "No puedo creer que hayas dicho eso…")
g_mia_uwant    = M("You would want that?", "それがほしいの？", "Zou je dat willen?", "Tu voudrais ça ?", "¿Querrías eso?")
g_mc_uwont     = MC1("You think I wouldn't", "そう思う？", "Denk je van niet", "Tu crois que non", "¿Crees que no?")
g_mia_I2       = M("I...", "私…", "Ik…", "Je…", "Yo…")
g_mia_question = M("Why are you asking a question to my question!",
                   "私の質問に質問で返さないで！",
                   "Waarom beantwoord je mijn vraag met een vraag!",
                   "Pourquoi tu réponds à ma question par une question !",
                   "¡Por qué respondes mi pregunta con una pregunta!")
g_mc_cry_emoji = MC1("\U0001F602", "\U0001F602", "\U0001F602", "\U0001F602", "\U0001F602")
g_mc_notmind   = MC1("I would definitely not mind",
                     "絶対嫌じゃないよ",
                     "Ik zou het zeker niet erg vinden",
                     "Je m'en plaindrais certainement pas",
                     "Definitivamente no me molestaría")
g_mia_uhm      = M("uhm", "えーと", "uhm", "euh", "mm")
g_mia_youre    = M("you're", "あなたは…", "jij bent…", "t'es…", "eres…")
g_mc_amazing   = MC1("amazing?", "素敵？", "geweldig?", "génial ?", "¿increíble?")
g_mia_weird_mc = M("weird", "変な人", "raar", "bizarre", "raro")
g_mc_okok22    = MC1("Okay okay", "わかったわかった", "Oké oké", "Ok ok", "Bien bien")
g_mc_gotext    = MC1("Go text the bench guy", "ベンチの人にテキストしなよ", "Ga de bankjeskerel berigen", "Vas envoyer un message au gars du banc", "Ve a escribirle al chico del banco")
g_mia_iwill    = M("I will", "するよ", "Doe ik", "Je vais le faire", "Lo haré")

# ── Continue for all (merge after choice branches) ───────────────────────────
g_mia_givesec  = M("Give me a second", "ちょっと待って", "Geef me even", "Donne-moi une seconde", "Dame un segundo")
g_tl_5m        = M("", tl=("5 minutes later", "5分後", "5 minuten later", "5 minutes plus tard", "5 minutos después"))
g_mia_did_it   = M("I did it", "やった", "Ik heb het gedaan", "Je l'ai fait", "Lo hice")
g_mia_ocean    = M("I want to throw my phone in the ocean",
                   "携帯を海に投げたい",
                   "Ik wil mijn telefoon in de oceaan gooien",
                   "J'ai envie de jeter mon téléphone dans l'océan",
                   "Quiero tirar mi teléfono al océano")
g_mc_whatusay  = MC1("haha what did you say", "笑 何て言ったの？", "haha wat heb je gezegd", "haha t'as dit quoi", "jaja ¿qué dijiste?")
g_mia_justsaid = M("Just hi and that I found his note",
                   "ただHiって彼のメモを見つけたって",
                   "Gewoon hoi en dat ik zijn briefje had gevonden",
                   "Juste salut et que j'avais trouvé son mot",
                   "Solo hola y que encontré su nota")
g_mc_perfect   = MC1("Perfect", "完璧", "Perfect", "Parfait", "Perfecto")
g_mia_tonight  = M("What if he doesn't reply until like tonight",
                   "今夜まで返事が来なかったら",
                   "Wat als hij pas vanavond antwoordt",
                   "Et s'il répond pas avant ce soir",
                   "Y si no responde hasta esta noche")
g_mc_survive   = MC1("Then you survive until tonight",
                     "じゃあ今夜まで生きろ",
                     "Dan overleef je het tot vanavond",
                     "Alors tu survives jusqu'à ce soir",
                     "Entonces sobrevives hasta esta noche")
g_mia_easy     = M("Easy for you to say", "言うのは簡単", "Makkelijk gezegd", "Facile à dire", "Fácil decirlo")
g_mc_getready  = MC1("Go get ready for class", "授業の準備して", "Ga je klaar maken voor de les", "Vas te préparer pour les cours", "Ve a prepararte para clase")
g_mc_stare     = MC1("Stop staring at your phone",
                     "携帯をじっと見るのはやめて",
                     "Stop met naar je telefoon staren",
                     "Arrête de fixer ton téléphone",
                     "Deja de mirar fijamente tu teléfono")
g_mia_notstare = M("I'm not staring at my phone",
                   "じっと見てない",
                   "Ik staar niet naar mijn telefoon",
                   "Je fixe pas mon téléphone",
                   "No estoy mirando fijamente mi teléfono")
g_mc_mia2      = MC1("Mia", "ミア", "Mia", "Mia", "Mia")
g_mia_okiam    = M("...okay I am", "…わかった、見てる", "…ok ik staar", "…ok je le fixe", "…ok sí lo estoy haciendo")
g_mc_go        = MC1("Go", "行って", "Ga", "Vas-y", "Ve")
g_mia_fine2    = M("Fine fine", "わかったわかった", "Oké oké", "Bon bon", "Bien bien")
g_mia_talater  = M("Talk later penpal", "またね、ペンパル", "Praten we later penpal", "À plus correspondante", "Hablamos luego amiga")
g_mc_later     = MC1("Later (;", "またね (;", "Later (;", "À plus (;", "Luego (;")

# ── KIRA section — Later that morning ────────────────────────────────────────
g_if_kira_morn = ng()  # pre-assign

# kira_path == true morning
g_tl_later_a   = K("", tl=("Later that morning...", "その朝、後ほど…", "Later die ochtend…", "Plus tard ce matin-là…", "Más tarde esa mañana…"))
g_kira_morn    = K("morning", "おはよう", "ochtend", "matin", "buenos días")
g_kira_head    = K("my head is killing me", "頭が割れそう", "mijn hoofd doet zo'n pijn", "ma tête me tue", "me está matando la cabeza")
g_mc_gm_kira   = MC1("Good morning to you too!", "君もおはよう！", "Goedemorgen voor jou ook!", "Bonjour à toi aussi !", "¡Buenos días para ti también!", char=KIRA)
g_kira_dont    = K("don't", "やめて", "doe dat niet", "non", "no")
g_mc_dontwhat  = MC1("Don't what", "何がやめて？", "Niet wat", "Pas quoi", "¿No qué?", char=KIRA)
g_kira_cheerful= K("be all cheerful", "そんな元気にしないで", "zo vrolijk zijn", "être aussi joyeux", "estar tan animado")
g_kira_early   = K("its too early", "まだ早すぎる", "het is te vroeg", "c'est trop tôt", "es demasiado temprano")
g_mc_9am       = MC1("Its 9am Kira", "もう9時だよキラ", "Het is 9 uur Kira", "Il est 9h Kira", "Son las 9 Kira", char=KIRA)
g_kira_exactly = K("exactly", "だから", "precies", "exactement", "exactamente")
g_kira_waytoo  = K("way too early", "全然早すぎる", "veel te vroeg", "beaucoup trop tôt", "demasiado temprano")
g_kira_clubbed = K("", spr=S_KIRA_PEELING,
    tl=("", "", "", "", ""))
g_kira_woke    = K("I literally just woke up like this",
                   "本当にこのまま目が覚めた",
                   "Ik werd letterlijk zo wakker",
                   "Je me suis littéralement réveillée comme ça",
                   "Literalmente me desperté así")
g_kira_notmove = K("did not move a single muscle since I got home",
                   "家に帰ってから全く動いてない",
                   "heb geen spier bewogen sinds ik thuis ben",
                   "n'ai pas bougé un seul muscle depuis que je suis rentrée",
                   "no moví ni un músculo desde que llegué a casa")
g_mc_alive_k   = MC1("How are you even alive", "どうやって生きてるの", "Hoe leef je überhaupt", "Comment t'es encore en vie", "Cómo sigues viva", char=KIRA)
g_kira_exp     = K("years of experience", "長年の経験", "jaren ervaring", "des années d'expérience", "años de experiencia")
g_kira_heard   = K("anyway I heard Mia before she left", "とにかくミアが出かける前に聞いた", "hoe dan ook hoorde ik Mia voor ze wegging", "de toute façon j'ai entendu Mia avant qu'elle parte", "de todos modos escuché a Mia antes de que se fuera")
g_kira_somethn = K("something about that note...", "あのメモのこと…", "iets over dat briefje…", "un truc à propos de ce mot…", "algo sobre esa nota…")
g_kira_getText = K("did you actually get her to text him",
                   "本当に彼にテキストさせたの",
                   "heb je haar echt laten berigen",
                   "t'as vraiment réussi à lui faire envoyer un message",
                   "¿de verdad conseguiste que le escribiera?")
g_mc_haha_k    = MC1("haha yea", "笑 うん", "haha ja", "haha ouais", "jaja sí", char=KIRA)
g_kira_good    = K("good... someone had to push her",
                   "よかった…誰かが背中を押す必要があった",
                   "goed… iemand moest haar duwtje geven",
                   "bien… il fallait que quelqu'un la pousse",
                   "bien… alguien tenía que empujarla")
g_kira_wkwould = K("she would have stared at that thing for a week",
                   "一週間あのメモを見つめてたはず",
                   "ze had een week naar dat ding gestaard",
                   "elle aurait fixé ce truc pendant une semaine",
                   "habría mirado esa cosa durante una semana")
g_mc_threwout  = MC1("I think she would have thrown it out",
                     "捨ててたと思う",
                     "Ik denk dat ze het had weggegooid",
                     "Je pense qu'elle l'aurait jeté",
                     "Creo que lo habría tirado a la basura", char=KIRA)
g_kira_orthat  = K("Or that...", "あるいはね…", "Of dat…", "Ou ça…", "O eso…")
g_kira_getout  = K("anyway I need to get out of this dress",
                   "とにかくこのドレスを脱がないと",
                   "hoe dan ook ik moet uit deze jurk",
                   "de toute façon je dois enlever cette robe",
                   "de todos modos tengo que quitarme este vestido")
g_kira_finally = K("finally", "やっと", "eindelijk", "enfin", "por fin")
g_kira_talater = K("talk later okay?", "また後でね？", "praten we later oké?", "on parle plus tard ok ?", "hablamos luego ¿vale?")
g_mc_sure_k    = MC1("Sure", "もちろん", "Tuurlijk", "Bien sûr", "Claro", char=KIRA)
g_tl_30m_k     = K("", tl=("30 minutes later...", "30分後…", "30 minuten later…", "30 minutes plus tard…", "30 minutos después…"))
g_kira_solate  = K("okay I am SO late", "もう本当に遅刻",  "oké ik ben ZO laat", "ok je suis tellement en retard", "ok llego tardísimo")
g_kira_vid     = K("", vid=V_KIRA_FIT, thumb=V_KIRA_FIT_TH)
g_kira_doeswork= K("does this work", "これでいい？", "werkt dit", "ça marche", "¿funciona esto?")
g_kira_noanswer= K("don't answer that I already left",
                   "答えなくていい、もう出た",
                   "antwoord niet ik ben al weg",
                   "réponds pas j'ai déjà quitté",
                   "no respondas ya me fui")
g_mc_works     = MC1("It works", "いいよ", "Het werkt", "Ça marche", "Funciona", char=KIRA)
g_kira_yourloss= K("Your loss (;", "あなたの損 (;", "Jouw verlies (;", "Tant pis pour toi (;", "Tu pierdes (;")
g_kira_bye     = K("bye", "バイ", "doei", "ciao", "adiós")

# kira_path == false morning
g_tl_later_b   = K("", tl=("Later that morning...", "その朝、後ほど…", "Later die ochtend…", "Plus tard ce matin-là…", "Más tarde esa mañana…"))
g_kira_hey     = K("hey", "ねえ", "hey", "hey", "hey")
g_mc_mornkira2 = MC1("Morning Kira", "おはようキラ", "Ochtend Kira", "Matin Kira", "Buenos días Kira", char=KIRA)
g_kira_somiaxd = K("so Mia texted the guy right?", "ミア、あの人にテキストしたんだよね？", "dus Mia heeft die kerel beright toch?", "donc Mia a envoyé un message au gars ?", "entonces ¿Mia le escribió al chico?")
g_kira_casual  = K("she was trying to be casual about it", "何でもないふりしてた", "ze probeerde er nonchalant over te doen", "elle essayait de faire ça l'air de rien", "estaba tratando de parecer casual")
g_kira_notcasul= K("she was not casual about it", "全然そうじゃなかったけど", "ze was niet nonchalant", "elle était pas du tout casual", "no era casual para nada")
g_mc_convinced = MC1("haha yea I convinced her to text the guy from the note",
                     "笑 うん、メモの人にテキストするよう説得した",
                     "haha ja ik heb haar overtuigd de briefjeskerel te berigen",
                     "haha ouais je l'ai convaincue d'écrire au gars du mot",
                     "jaja sí la convencí de escribirle al chico de la nota", char=KIRA)
g_kira_stranger= K("You got Mia to text a stranger!",
                   "ミアに知らない人にテキストさせたの！",
                   "Jij kreeg Mia zover om een vreemde te berigen!",
                   "T'as réussi à faire écrire Mia à un inconnu !",
                   "¡Conseguiste que Mia le escribiera a un desconocido!")
g_kira_genuinely= K("genuinely how...", "本当にどうやって…", "echt hoe…", "vraiment comment…", "en serio ¿cómo?")
g_mc_2vs1      = MC1("It was 2 against 1... she had to",
                     "2対1だったから…しょうがない",
                     "Het was 2 tegen 1… ze moest wel",
                     "C'était 2 contre 1… elle avait pas le choix",
                     "Éramos 2 contra 1… no le quedó otra", char=KIRA)
g_kira_isee    = K("I see", "なるほど", "Ik zie", "Je vois", "Ya veo")
g_kira_goodfor = K("you are good for her", "あなたは彼女にいい存在", "jij bent goed voor haar", "t'es bon pour elle", "eres bueno para ella")
g_kira_bubble  = K("She needs to get out of her bubble",
                   "彼女はバブルから出る必要がある",
                   "Ze moet uit haar bubbel komen",
                   "Elle a besoin de sortir de sa bulle",
                   "Necesita salir de su burbuja")
g_kira_dormout = K("", spr=S_MIA_DORM_OUT)
g_kira_lookgo  = K("look at her go", "見て、出かけてる", "kijk haar eens gaan", "regarde-la partir", "mírala ir")
g_kira_nervous = K("don't make her nervous about it okay",
                   "彼女を緊張させないでね",
                   "maak haar er niet nerveus over oké",
                   "la rends pas nerveuse à ce sujet ok",
                   "no la pongas nerviosa ¿vale?")
g_kira_spooks  = K("she spooks easy", "すぐ怖がる", "ze schrikt snel", "elle flippe facilement", "se asusta fácilmente")
g_mc_iknow_k   = MC1("I know that by now haha",
                     "もうわかってるよ笑",
                     "Dat weet ik nu wel haha",
                     "Je le sais maintenant haha",
                     "Ya lo sé a estas alturas jaja", char=KIRA)
g_kira_good2   = K("good", "よかった", "goed", "bien", "bien")
g_kira_byenerd = K("bye nerd", "バイ、オタク", "doei nerd", "ciao nerd", "adiós nerd")

# ── MIA social post + later morning chat ─────────────────────────────────────
g_social_mia   = M("", post=P_MIA_NOTES)
g_mc_penpal2   = MC1("Hey penpal", "やあ、ペンパル", "Hey penpal", "Hey correspondante", "Hola amiga")
g_mc_socialq   = MC1("Is that a social post I see...?",
                     "ソーシャルポストが見える…？",
                     "Is dat een social post die ik zie...?",
                     "C'est un post que je vois... ?",
                     "¿Eso es una publicación que veo...?")
g_mia_godsaw   = M("oh god you saw that", "うわ、見たんだ", "oh god je zag dat", "oh non t'as vu ça", "dios mío lo viste")
g_mc_ofcourse  = MC1("Of course I saw it", "もちろん見たよ", "Natuurlijk zag ik het", "Bien sûr que je l'ai vu", "Claro que lo vi")
g_mc_ilike     = MC1("I like it", "いいね", "Ik vind het mooi", "J'aime bien", "Me gusta")
g_mia_4times   = M("I almost deleted it like 4 times",
                   "4回くらい消しそうになった",
                   "Ik had het bijna 4 keer verwijderd",
                   "J'ai failli le supprimer genre 4 fois",
                   "Casi lo borré unas 4 veces")
g_mc_why2      = MC1("Why", "なんで？", "Waarom", "Pourquoi", "¿Por qué?")
g_mia_feltwd   = M("I don't know it felt weird",
                   "なんか変な気がして",
                   "Ik weet het niet het voelde raar",
                   "Je sais pas ça faisait bizarre",
                   "No sé se sentía raro")
g_mia_whowants = M("Like who wants to see my notes",
                   "誰が私のノートを見たいの",
                   "Alsof iemand mijn aantekeningen wil zien",
                   "Genre qui veut voir mes notes",
                   "O sea ¿quién quiere ver mis apuntes?")
g_mc_ido       = MC1("I do", "私が見たい", "Ik wil", "Moi", "Yo")
g_mia_dots5    = M("...", "…", "…", "…", "…")
g_mia_okay     = M("okay", "わかった", "oké", "ok", "ok")
g_mc_dontdel   = MC1("Don't delete it", "消さないで", "Verwijder het niet", "Supprime-le pas", "No lo borres")
g_mia_wont     = M("I won't now", "もう消さない", "Ik doe het niet meer", "Je le ferai plus maintenant", "Ya no lo haré")
g_mc_good2     = MC1("Good", "よかった", "Goed", "Bien", "Bien")
g_mc_so        = MC1("So", "それで", "Dus", "Alors", "Entonces")
g_mc_textback  = MC1("Did he text back", "彼から返事来た？", "Heeft hij teruggebericht", "Il a répondu ?", "¿Te respondió?")
g_mia_no       = M("No", "まだ", "Nee", "Non", "No")
g_mia_notyet   = M("Not yet", "まだ来てない", "Nog niet", "Pas encore", "Todavía no")
g_mc_oksthat   = MC1("That's okay", "大丈夫だよ", "Dat is oké", "C'est ok", "Está bien")
g_mia_iknow2   = M("I know", "わかってる", "Ik weet het", "Je sais", "Lo sé")
g_mia_nostare2 = M("I'm not staring at my phone",
                   "携帯をじっと見てない",
                   "Ik staar niet naar mijn telefoon",
                   "Je fixe pas mon téléphone",
                   "No estoy mirando fijamente mi teléfono")
g_mc_mia3      = MC1("Mia", "ミア", "Mia", "Mia", "Mia")
g_mia_iam      = M("I'm not!", "見てないよ！", "Ik ben het niet!", "Si je le fixe pas !", "¡No lo estoy haciendo!")
g_mc_ok3       = MC1("Okay", "わかった", "Oké", "Ok", "Ok")
g_mia_dots6    = M("...", "…", "…", "…", "…")
g_mia_letknow  = M("I'll let you know when he does",
                   "来たら教える",
                   "Ik laat het weten als hij dat doet",
                   "Je te dirai quand il le fait",
                   "Te avisaré cuando lo haga")
g_mc_iknowyou  = MC1("I know you will", "わかってるよ", "Ik weet het", "Je sais", "Lo sé")
g_mia_shutup   = M("Shut up", "黙って", "Hou je mond", "Ferme-la", "Cállate")
g_mia_lecture2 = M("I'm in my lecture now", "今講義中", "Ik zit nu in mijn college", "Je suis en cours là", "Estoy en mi clase ahora")
g_mia_lecture_pic = M("", spr=S_MIA_LECTURE)
g_mc_backrow   = MC1("Back row again", "また後ろの列", "Achterste rij weer", "Dernier rang encore", "Fila de atrás otra vez")
g_mia_obviously= M("Obviously", "もちろん", "Uiteraard", "Évidemment", "Por supuesto")
g_mc_learn     = MC1("Go learn something", "何か学んで", "Ga iets leren", "Vas apprendre quelque chose", "Ve a aprender algo")
g_mia_bye2     = M("Bye penpal", "またね、ペンパル", "Doei penpal", "Ciao correspondante", "Adiós amiga")
g_mc_bye2      = MC1("Bye", "またね", "Doei", "Ciao", "Adiós")

# ── KIRA social post ──────────────────────────────────────────────────────────
g_social_kira  = K("", post=P_KIRA_BUS, spr=S_KIRA_BUS_SPR)

# ── MIA: About an hour later — HE REPLIED ────────────────────────────────────
g_tl_1h        = M("", tl=("About an hour later...", "約1時間後…", "Ongeveer een uur later…", "Environ une heure plus tard…", "Alrededor de una hora después…"))
g_mia_replied1 = M("HE REPLIED", "返事来た", "HIJ HEEFT GEANTWOORD", "IL A RÉPONDU", "RESPONDIÓ")
g_mia_replied2 = M("HE REPLIED HE REPLIED HE REPLIED",
                   "返事来た返事来た返事来た",
                   "HIJ HEEFT GEANTWOORD HIJ HEEFT GEANTWOORD",
                   "IL A RÉPONDU IL A RÉPONDU IL A RÉPONDU",
                   "RESPONDIÓ RESPONDIÓ RESPONDIÓ")
g_mc_breathe   = MC1("Breathe Mia", "息して、ミア", "Adem Mia", "Respire Mia", "Respira Mia")
g_mia_cant2    = M("I can't I'm dying", "無理、死にそう", "Ik kan het niet ik ga dood", "Je peux pas je vais mourir", "No puedo me estoy muriendo")
g_mia_hallway  = M("I'm in the hallway between lectures",
                   "講義の合間に廊下にいる",
                   "Ik sta in de gang tussen de lessen",
                   "Je suis dans le couloir entre les cours",
                   "Estoy en el pasillo entre clases")
g_mia_cantread = M("I can't read it now....", "今は読めない…", "Ik kan het nu niet lezen…", "Je peux pas le lire maintenant…", "No puedo leerlo ahora…")
g_mc_screenshot= MC1("Send me a screenshot", "スクリーンショット送って", "Stuur me een screenshot", "Envoie-moi une capture d'écran", "Mándame una captura de pantalla")
g_mia_okok2    = M("Okay okay okay", "わかったわかったわかった", "Oké oké oké", "Ok ok ok", "Ok ok ok")
g_mia_holdon   = M("Hold on", "ちょっと待って", "Wacht even", "Attends", "Espera")
g_mia_screensht= M("", spr=S_MIA_SCREENSHOT)
g_mia_omg2     = M("Oh my god", "おいおい", "Oh mijn god", "Oh mon dieu", "Dios mío")
g_mia_cutie    = M("He called me cutie", "かわいいって呼んだ", "Hij noemde me schatje", "Il m'a appelée mignonne", "Me llamó linda")
g_mia_cutie2   = M("He CALLED ME CUTIE", "彼、私をかわいいって呼んだ！", "Hij NOEMDE ME SCHATJE", "Il M'A APPELÉE MIGNONNE", "Me LLAMÓ LINDA")
g_mc_dots_r    = MC1("...", "…", "…", "…", "…")

# ── CHOICE: Mia slow down / That's great Mia ─────────────────────────────────
g_choice_reply = mc2(MIA,
    [("Mia slow down",
      "ミア、落ち着いて",
      "Mia rustig aan",
      "Mia calme-toi",
      "Mia tranquila"),
     ("That's great Mia",
      "よかったね、ミア",
      "Dat is geweldig Mia",
      "C'est super Mia",
      "Qué bien Mia")])

# Choice 1: Mia slow down
g_mia_what_r   = M("What", "なに？", "Wat", "Quoi", "¿Qué?")
g_mc_idontknow = MC1("I don't know", "わからない", "Ik weet het niet", "Je sais pas", "No sé")
g_mc_notsound  = MC1("That doesn't really sound like the guy on the bench",
                     "ベンチの人っぽくないな",
                     "Dat klinkt niet echt als de kerel op het bankje",
                     "Ça ressemble pas vraiment au gars du banc",
                     "No suena muy propio del chico del banco")
g_mia_whatmean = M("What do you mean", "どういう意味？", "Wat bedoel je", "Qu'est-ce que tu veux dire", "¿Qué quieres decir?")
g_mc_forward   = MC1("I mean he's a little forward", "ちょっと積極的すぎる気が", "Ik bedoel hij is een beetje direct", "Je veux dire il est un peu direct", "Quiero decir es un poco directo")
g_mc_reallyfw  = MC1("Like really forward", "本当に積極的", "Zoals echt direct", "Genre vraiment direct", "O sea muy directo")
g_mc_hitorask  = MC1("He went from hi to asking you out in one message",
                     "Hiから一メッセージでデートに誘ってる",
                     "Hij ging van hoi naar je uitnodigen in één bericht",
                     "Il est passé de salut à te demander de sortir en un message",
                     "Fue de hola a pedirte una cita en un mensaje")
g_mia_confident= M("Maybe he's just confident",
                   "ただ自信があるだけかも",
                   "Misschien is hij gewoon zelfverzekerd",
                   "Peut-être qu'il est juste confiant",
                   "Quizás solo es seguro de sí mismo")
g_mia_left_note= M("He did leave me a note after all",
                   "結局メモを残してくれたんだし",
                   "Hij liet wel een briefje achter voor me",
                   "Il m'a quand même laissé un mot",
                   "Después de todo me dejó una nota")
g_mc_iguess    = MC1("I guess", "そうかな", "Denk het wel", "J'imagine", "Supongo")
g_mia_beingwrd = M("You're being weird", "変だよ", "Je doet raar", "T'es bizarre", "Estás siendo raro")
g_mia_thought  = M("I thought you'd be excited for me",
                   "喜んでくれると思ってた",
                   "Ik dacht dat je blij voor me zou zijn",
                   "Je pensais que tu serais content pour moi",
                   "Pensé que estarías emocionado por mí")
g_mc_iam       = MC1("I am", "喜んでるよ", "Dat ben ik", "Je le suis", "Lo estoy")
g_mc_careful   = MC1("Just be careful okay",
                     "ただ気をつけて",
                     "Wees gewoon voorzichtig oké",
                     "Sois juste prudente ok",
                     "Solo ten cuidado ¿vale?")
g_mia_canbe    = M("I can be careful", "気をつけられるよ", "Ik kan voorzichtig zijn", "Je peux être prudente", "Puedo ser cuidadosa")
g_mia_justtxting= M("Its just texting", "ただのテキストだし", "Het is gewoon berigen", "C'est juste des messages", "Solo son mensajes")

# Choice 2: That's great Mia
g_mc_great_r   = MC1("That's great Mia", "よかったね、ミア", "Dat is geweldig Mia", "C'est super Mia", "Qué bien Mia")
g_mia_right_r  = M("Right?!", "でしょ！", "Toch?!", "Hein ?!", "¿Verdad?!")
g_mc_gowitit   = MC1("Just go with it", "流れに乗って", "Gewoon meegaan", "Vas-y", "Solo sigue la corriente")
g_mc_clearlyinto= MC1("He's clearly into you", "明らかに好きみたいだよ", "Hij is duidelijk in jou geïnteresseerd", "Il t'aime clairement", "Claramente le gustas")
g_mia_whatback  = M("I don't know what to say back though",
                    "でも何て返したらいいか…",
                    "Ik weet alleen niet wat ik terug moet zeggen",
                    "Mais je sais pas quoi répondre",
                    "Pero no sé qué responderle")
g_mc_sayanything= MC1("Say anything", "何でも言えばいい", "Zeg maar iets", "Dis n'importe quoi", "Di cualquier cosa")
g_mc_hardpart   = MC1("You already did the hard part",
                      "難しい部分はもう終わった",
                      "Je hebt het moeilijkste al gedaan",
                      "T'as déjà fait le plus dur",
                      "Ya hiciste la parte difícil")
g_mia_okay_r    = M("Okay", "わかった", "Oké", "Ok", "Ok")
g_mia_okayican  = M("Okay I can do this", "わかった、できる", "Oké ik kan dit", "Ok je peux le faire", "Ok puedo hacer esto")
g_mia_whatdo    = M("What do I do about tonight though",
                    "でも今夜はどうすれば",
                    "Maar wat doe ik vanavond",
                    "Mais qu'est-ce que je fais pour ce soir",
                    "Pero ¿qué hago con lo de esta noche?")
g_mc_uptoyou    = MC1("Up to you", "あなた次第", "Aan jou", "À toi de voir", "Depende de ti")
g_mc_textcasual = MC1("Just text casually for now",
                      "とりあえずカジュアルにテキストして",
                      "Bericht voor nu gewoon casual",
                      "Envoie juste des messages casual pour l'instant",
                      "Solo escribe de manera casual por ahora")
g_mia_rightcas  = M("Right. Casual.", "そうだね。カジュアルに。", "Juist. Casual.", "Ouais. Casual.", "Claro. Casual.")
g_mia_sonotcas  = M("I am so not casual right now",
                    "今全然カジュアルじゃない",
                    "Ik ben nu zo niet casual",
                    "Je suis tellement pas casual là",
                    "No soy nada casual ahora mismo")

# ── Continue for both reply choices ──────────────────────────────────────────
g_mia_nextlect  = M("My next lecture is starting",
                    "次の講義が始まる",
                    "Mijn volgende college begint",
                    "Mon prochain cours commence",
                    "Mi siguiente clase va a empezar")
g_mia_cantthink = M("I can't think", "考えられない", "Ik kan niet nadenken", "Je peux pas réfléchir", "No puedo pensar")
g_mc_try        = MC1("Try", "頑張って", "Probeer het", "Essaie", "Intenta")
g_mia_talater2  = M("Talk later penpal", "またね、ペンパル", "Praten we later penpal", "À plus correspondante", "Hablamos luego amiga")
g_mc_bye3       = MC1("Bye", "またね", "Doei", "Ciao", "Adiós")

# ── KIRA: 2 hours later — talking about Jack ─────────────────────────────────
g_tl_2h_k      = K("", tl=("2 hours later...", "2時間後…", "2 uur later…", "2 heures plus tard…", "2 horas después…"))
g_mc_kira2     = MC1("Kira", "キラ", "Kira", "Kira", "Kira", char=KIRA)
g_mc_replied_k = MC1("He replied to Mia", "ミアに返事が来た", "Hij heeft Mia geantwoord", "Il a répondu à Mia", "Le respondió a Mia", char=KIRA)
g_kira_wait    = K("wait what", "え、何？", "wacht wat", "attends quoi", "espera qué")
g_kira_noteguy = K("the note guy", "メモの人？", "de briefjeskerel", "le gars du mot", "¿el de la nota?")
g_mc_yeah_k    = MC1("Yeah", "うん", "Ja", "Ouais", "Sí", char=KIRA)
g_kira_and     = K("and", "それで？", "en", "et", "¿y?")
g_mc_losing    = MC1("She's losing her mind", "頭おかしくなってる", "Ze is haar verstand aan het verliezen", "Elle perd la tête", "Está perdiendo la cabeza", char=KIRA)
g_kira_haha_k  = K("haha of course she is", "笑 当然だよ", "haha natuurlijk is ze dat", "haha bien sûr qu'elle l'est", "jaja claro que sí")
g_kira_lecture2= K("", spr=S_KIRA_LECTURE)

# IF kira_path for lecture scene
g_if_kira_lect = ng()
g_kira_vibrate = K("I can see her vibrating all the way in the back",
                   "後ろからでも彼女が震えてるのが見える",
                   "Ik kan haar helemaal achterin zien trillen",
                   "Je la vois vibrer tout au fond",
                   "La veo vibrar desde el fondo")

# Continue for both (lecture)
g_kira_whatname= K("what's his name again", "名前なんだっけ", "wat is zijn naam ook alweer", "c'est quoi son nom déjà", "¿cómo se llama?")
g_mc_jack      = MC1("Jack", "ジャック", "Jack", "Jack", "Jack", char=KIRA)
g_kira_dots_k  = K("...", "…", "…", "…", "…")
g_kira_jackwhat= K("Jack what", "ジャック・何？", "Jack wat", "Jack quoi", "¿Jack qué?")
g_mc_justsaid_k= MC1("It just said Jack on the note",
                     "メモにはジャックとだけ",
                     "Er stond gewoon Jack op het briefje",
                     "Il y avait juste Jack sur le mot",
                     "Solo decía Jack en la nota", char=KIRA)
g_mc_why_k     = MC1("Why", "なんで？", "Waarom", "Pourquoi", "¿Por qué?", char=KIRA)
g_kira_nothing = K("nothing", "なんでもない", "niets", "rien", "nada")
g_kira_ijust   = K("I just", "ただ…", "Ik gewoon", "Je juste", "Yo solo")
g_kira_jacki   = K("there's a Jack I know of",
                   "知ってるジャックがいて",
                   "er is een Jack die ik ken",
                   "il y a un Jack que je connais",
                   "hay un Jack que conozco")
g_kira_diffone = K("probably a different one... I hope",
                   "多分別の人…そう願う",
                   "waarschijnlijk een andere… ik hoop het",
                   "probablement un autre… j'espère",
                   "probablemente otro… eso espero")
g_kira_common  = K("its a common name", "よくある名前だし", "het is een veelvoorkomende naam", "c'est un prénom courant", "es un nombre común")
g_mc_morethan1 = MC1("Yeah, there are probably more than one Jacks out there haha",
                     "うん、ジャックはたくさんいるよね笑",
                     "Ja, er zijn waarschijnlijk meer dan één Jack haha",
                     "Ouais il y a probablement plus d'un Jack haha",
                     "Sí, probablemente hay más de un Jack jaja", char=KIRA)
g_kira_yeahyeah= K("yeah yeah", "そうだね", "ja ja", "ouais ouais", "sí sí")
g_kira_dontworry= K("don't worry about it", "気にしないで", "maak je geen zorgen", "t'inquiète pas", "no te preocupes")
g_kira_gotalk  = K("I'll go talk to her after the lecture... I want to see those texts",
                   "講義の後に話しかける…あのテキスト見たい",
                   "Ik ga na het college met haar praten… ik wil die berichten zien",
                   "Je vais lui parler après le cours… je veux voir ces messages",
                   "Voy a hablar con ella después de clase… quiero ver esos mensajes")

# ── MIA: After her lecture ────────────────────────────────────────────────────
g_tl_after_lect= M("", tl=("After her lecture...", "講義の後…", "Na haar college…", "Après son cours…", "Después de su clase…"))
g_mia_survived = M("okay I survived", "なんとか生き延びた", "oké ik heb het overleefd", "ok j'ai survécu", "ok sobreviví")
g_mc_proud     = MC1("Proud of you", "誇りに思う", "Trots op je", "Fier de toi", "Orgulloso de ti")
g_mia_lessyd   = M("I understood even less than yesterday but that's not the point",
                   "昨日よりさらに理解できなかったけど、それは関係ない",
                   "Ik begreep nog minder dan gisteren maar dat is niet het punt",
                   "J'ai encore moins compris qu'hier mais c'est pas le sujet",
                   "Entendí aún menos que ayer pero eso no importa")
g_mc_notpoint  = MC1("Definitely not the point", "確かに関係ない", "Zeker niet het punt", "Clairement pas le sujet", "Definitivamente no importa")
g_mia_kirafound= M("Kira found me in the hallway",
                   "キラが廊下で見つけてくれた",
                   "Kira vond me in de gang",
                   "Kira m'a trouvée dans le couloir",
                   "Kira me encontró en el pasillo")
g_mia_sitting2 = M("We are sitting outside for a bit",
                   "少し外に座ってる",
                   "We zitten even buiten",
                   "On est assises dehors un moment",
                   "Estamos sentadas afuera un momento")
g_mia_bench    = M("", spr=S_KIRA_MIA_BCH)
g_mc_howkira   = MC1("How is Kira doing", "キラは元気？", "Hoe is het met Kira", "Comment va Kira", "¿Cómo está Kira?")
g_mia_shaking  = M("She is literally shaking my shoulder telling me to reply to him",
                   "文字通り肩を揺さぶりながら返事しろって言ってる",
                   "Ze schudt letterlijk mijn schouder en zegt me te antwoorden",
                   "Elle secoue littéralement mon épaule en me disant de lui répondre",
                   "Literalmente me está sacudiendo el hombro diciéndome que le responda")
g_mia_notstop  = M("She will not stop", "止まらない", "Ze stopt niet", "Elle arrête pas", "No para")
g_mc_point_k   = MC1("She has a point", "彼女は正しい", "Ze heeft een punt", "Elle a raison", "Tiene razón")
g_mia_okok3    = M("Okay okay...", "わかったわかった…", "Oké oké…", "Ok ok…", "Ok ok…")
g_mia_strange  = M("I just read the texts... They are a bit... strange",
                   "テキスト読んだ…ちょっと…変だな",
                   "Ik heb de berichten gelezen… ze zijn een beetje… raar",
                   "Je viens de lire les messages… ils sont un peu… bizarres",
                   "Acabo de leer los mensajes… son un poco… raros")
g_mia_dontyou  = M("Don't you think?", "そう思わない？", "Vind je ook niet", "T'en penses pas autant ?", "¿No crees?")
g_mc_yeathey   = MC1("Yea\u2026 they are",
                     "うん…そうだね",
                     "Ja\u2026 dat zijn ze",
                     "Ouais\u2026 ils le sont",
                     "Sí\u2026 lo son")
g_mia_kirasays = M("Kira is saying I should just text back...",
                   "キラはとにかく返事すべきって言ってる…",
                   "Kira zegt dat ik gewoon terug moet berigen…",
                   "Kira dit que je devrais juste répondre…",
                   "Kira dice que simplemente debería responderle…")
g_mc_iagree    = MC1("I agree with her... you can always just cut it off right...",
                     "同意する…いつでもやめられるし…",
                     "Ik ben het met haar eens… je kunt het altijd gewoon stoppen...",
                     "Je suis d'accord avec elle… tu peux toujours juste couper court...",
                     "Estoy de acuerdo con ella... siempre puedes cortarlo ¿no?")
g_mc_probwont  = MC1("You probably won't though once you get to know him (;",
                     "でも知り合ったらやめないと思うけど (;",
                     "Je zult het waarschijnlijk niet doen als je hem leert kennen (;",
                     "Mais tu le feras probablement pas une fois que tu le connaîtras (;",
                     "Pero probablemente no lo harás cuando lo conozcas (;")
g_mc_liked_bench= MC1("Like you liked how he looked on that bench right... he's probably just nervous that's why he is texting weird",
                      "ベンチでの見た目が好きだったんでしょ…多分緊張してるだけだよ",
                      "Je vond hoe hij eruitzag op dat bankje toch… hij is waarschijnlijk gewoon nerveus",
                      "T'aimais comment il était sur ce banc… il est probablement juste nerveux",
                      "Te gustó como se veía en ese banco ¿verdad?... probablemente solo está nervioso")
g_mia_right_r2 = M("right...", "そうかな…", "juist…", "ouais…", "claro…")
g_mia_iguess2  = M("I.. I guess", "私…そうかな", "Ik.. ik denk het wel", "Je.. j'imagine", "Yo.. supongo")
g_mia_whatdo2  = M("But what do I say?", "でも何て言えばいい？", "Maar wat moet ik zeggen", "Mais qu'est-ce que je dis ?", "Pero ¿qué le digo?")
g_mc_simple2   = MC1("Keep it simple", "シンプルに", "Hou het simpel", "Reste simple", "Mantenlo simple")
g_mc_asksomethg= MC1("Ask him something back", "何か聞いてみて", "Vraag hem iets terug", "Demande-lui quelque chose en retour", "Pregúntale algo también")
g_mia_likewhat = M("Like what", "例えば？", "Zoals wat", "Genre quoi", "¿Como qué?")
g_mc_anything  = MC1("Anything", "何でも", "Iets", "N'importe quoi", "Cualquier cosa")
g_mc_dontleave = MC1("Just don't leave him on read",
                     "既読無視しないで",
                     "Laat hem gewoon niet op gelezen staan",
                     "Laisse-le juste pas en vu",
                     "Solo no lo dejes en visto")
g_mia_okay2    = M("Okay", "わかった", "Oké", "Ok", "Ok")
g_mia_sec2     = M("Give me a second", "ちょっと待って", "Geef me even", "Donne-moi une seconde", "Dame un segundo")
g_tl_3m        = M("", tl=("3 minutes later...", "3分後…", "3 minuten later…", "3 minutes plus tard…", "3 minutos después…"))
g_mia_said_hey = M("I said: Hey, no didn't throw it out... how are you doing?",
                   "「ねえ、捨ててないよ…元気？」って送った",
                   "Ik heb gezegd: Hey, nee ik heb het niet weggegooid… hoe gaat het?",
                   "J'ai dit : Hey, non je l'ai pas jeté… tu vas bien ?",
                   "Dije: Hey, no lo tiré... ¿cómo estás?")
g_mia_isgood   = M("Is that good?", "よかった？", "Is dat goed?", "C'est bien ?", "¿Estuvo bien?")
g_mc_yeagood   = MC1("Yea, it's good", "うん、いいよ", "Ja, dat is goed", "Ouais c'est bien", "Sí, está bien")
g_mc_natural   = MC1("Natural", "自然だよ", "Natuurlijk", "Naturel", "Natural")
g_mia_nowwait  = M("Now we wait again", "また待つ", "Nu wachten we weer", "Maintenant on attend encore", "Ahora esperamos de nuevo")
g_mc_nowwait   = MC1("Now we wait", "また待とう", "Nu wachten we", "Maintenant on attend", "Ahora esperamos")
g_tl_2m        = M("", tl=("2 minutes later", "2分後", "2 minuten later", "2 minutes plus tard", "2 minutos después"))
g_mia_replied3 = M("He replied already...", "もう返事来た…", "Hij heeft al geantwoord…", "Il a déjà répondu…", "Ya respondió…")
g_mia_ohgod    = M("Oh god.... He said...", "うわ…何て言ったか…", "Oh god…. Hij zei…", "Oh dieu…. Il a dit…", "Dios…. Dijo…")
g_mia_yeagood  = M("\"Yea I'm good\"",
                   "「うん、元気」",
                   "\"Ja gaat goed\"",
                   "\"Ouais ça va\"",
                   "\"Sí estoy bien\"")
g_mia_tonight2 = M("\"So, me and you tonight? my dorm?\"",
                   "「じゃあ今夜、俺と君で？俺の寮で？」",
                   "\"Dus, ik en jij vanavond? mijn kamer?\"",
                   "\"Alors, moi et toi ce soir ? ma chambre ?\"",
                   "\"¿Entonces, tú y yo esta noche? ¿mi dorm?\"")
g_mia_selfie_too= M("He sent a selfie aswell", "セルフィーも送ってきた", "Hij stuurde ook een selfie", "Il a aussi envoyé un selfie", "También envió una selfie")
g_mia_jack     = M("", spr=S_JACK_SELFIE)
g_mc_wait      = MC1("Wait", "ちょっと待って", "Wacht", "Attends", "Espera")
g_mia_dots7    = M("...", "…", "…", "…", "…")
g_mc_whosguy   = MC1("Who's that guy?", "誰この人？", "Wie is die kerel?", "C'est qui ce gars ?", "¿Quién es ese tipo?")
g_mia_notbench = M("This is not the guy from the bench...",
                   "これ、ベンチの人じゃない…",
                   "Dit is niet de kerel van het bankje…",
                   "C'est pas le gars du banc…",
                   "Este no es el chico del banco…")
g_mia_kiragq   = M("Kira just went quiet", "キラが急に黙った", "Kira werd ineens stil", "Kira vient de se taire", "Kira se quedó callada de repente")

# ── KIRA: while sitting next to Mia ──────────────────────────────────────────
g_tl_kira_sits = K("", tl=("While Kira sits next to Mia...",
                            "キラがミアの隣に座る中…",
                            "Terwijl Kira naast Mia zit…",
                            "Pendant que Kira est assise à côté de Mia…",
                            "Mientras Kira está sentada junto a Mia…"))
g_kira_itshim  = K("Its... him...", "これは…彼だ…", "Het is… hem…", "C'est… lui…", "Es… él…")
g_kira_talking = K("This is the guy I was talking about...",
                   "私が言ってた人だ…",
                   "Dit is de kerel over wie ik het had…",
                   "C'est le gars dont je parlais…",
                   "Este es el chico del que hablaba…")
g_kira_notnice = K("This is not a nice guy...",
                   "いい人じゃない…",
                   "Dit is geen aardige kerel…",
                   "C'est pas un gars sympa…",
                   "Este no es un buen chico…")
g_kira_tread   = K("Tread carefully please",
                   "慎重にお願い",
                   "Pas alsjeblieft op",
                   "Fais attention s'il te plaît",
                   "Ten cuidado por favor")
g_mc_howbad    = MC1("How bad?", "どのくらい？", "Hoe erg?", "C'est grave ?", "¿Qué tan malo?", char=KIRA)
g_kira_thrownout= K("He has been thrown out of the club I work at multiple times bad...",
                    "私が働いてるクラブから何度も追い出されてる…",
                    "Hij is meerdere keren uit de club waar ik werk gezet…",
                    "Il s'est fait virer du club où je travaille plusieurs fois…",
                    "Lo han echado del club donde trabajo varias veces…")
g_mc_ohw       = MC1("ohw...", "あー…", "ohw…", "oh…", "ah…", char=KIRA)

# ── MIA: What do I do ────────────────────────────────────────────────────────
g_mia_whatdo3  = M("What do I do?", "どうすればいい？", "Wat moet ik doen?", "Qu'est-ce que je fais ?", "¿Qué hago?")

g_end = END()

# =============================================================================
# WIRE LINKS
# =============================================================================

# Start → kira_path IF
links.append((g_start, g_if_kira_start))

# kira_path IF: true → night scene; false → morning
g_if_kira_start_node = ifn("Kira_Path", 0, "", g_tl_night, g_tl_morn,
                           preset_guid=g_if_kira_start, after_guid=g_start)

# Night scene chain (kira_path true)
chain([g_tl_night, g_kira_door, g_tl_2h_a, g_mc_doggy, g_mc_facial, g_kira_night])
links.append((g_kira_night, g_tl_morn))  # night merges into morning

# Morning chain (both paths)
chain([g_tl_morn, g_mc_penpal1, g_mc_quiet, g_tl_10m,
       g_mia_hi, g_mia_sorry1, g_mia_minute,
       g_mc_hours, g_mia_sorry2, g_mc_alright,
       g_mia_sleep, g_mia_woke, g_mia_kira_fd,
       g_mc_facedown, g_mia_basically])

# kira_path IF for extra morning content
g_if_kira_mia_morn_node = ifn("Kira_Path", 0, "", g_mia_late1, g_mia_bar,
                              preset_guid=g_if_kira_mia_morn, after_guid=g_mia_basically)
links.append((g_mia_basically, g_if_kira_mia_morn_node))

# kira true extra
chain([g_mia_late1, g_mia_late2, g_mc_wow, g_mc_bignight,
       g_mia_guess1, g_mia_weird, g_mc_what,
       g_mia_noalc, g_mia_atall, g_mia_allnight, g_mia_soap,
       g_mc_hm, g_mc_sobered, g_mia_guess2, g_mia_stillwd,
       g_mc_overthink, g_mia_right1])
links.append((g_mia_right1, g_mc_breath))

# kira false extra
links.append((g_mia_bar, g_mc_breath))

# Continue for both — rest of morning note scene
chain([g_mc_breath, g_mia_unfort, g_mia_noise, g_mc_human, g_mia_barely,
       g_mc_letsleep, g_mc_keptup, g_mia_dots1,
       g_mia_noteimg, g_mia_sitting, g_mia_looking,
       g_mc_haha1, g_mc_oneway, g_mia_dontthink,
       g_mc_staring, g_mia_dots2, g_mia_iknow1, g_mia_3times,
       g_mc_and1, g_mia_putdown, g_mia_stupid1,
       g_mc_notstupid, g_mia_cant, g_mia_IS,
       g_mc_notused, g_mc_notsame,
       g_mia_dots3, g_mia_whatif1, g_mc_whatif2, g_mia_whatif3,
       g_mc_note_bag, g_mc_clearly, g_mia_guess3,
       g_mia_alive, g_mia_pointing, g_mc_agree,
       g_mia_quote, g_mia_threat, g_mc_listen,
       g_mia_asleep, g_mia_insane,
       g_mc_shesright, g_mc_twovotes, g_mc_outnumb,
       g_mia_dots4, g_mia_fine, g_mia_scared])

# CHOICE: just say hi vs ass pic
links.append((g_mia_scared, g_choice_text))
links.append((g_choice_text, g_mia_justhi))    # port 0: just say hi
links.append((g_choice_text, g_mia_WHAT))      # port 1: ass pic

# Choice 1 path
chain([g_mia_justhi, g_mc_simple, g_mia_feelwrd, g_mc_mia_ch1, g_mia_okok1])
links.append((g_mia_okok1, g_mia_givesec))

# Choice 2 → sub-choice
links.append((g_mia_WHAT, g_choice_ass))
links.append((g_choice_ass, g_mc_sayhi_21))   # port 0: joking
links.append((g_choice_ass, g_mc_sayhi_22))   # port 1: just kidding

# Choice 2.1
chain([g_mc_sayhi_21, g_mia_omg, g_mia_hateu, g_mia_heart,
       g_mc_sorry_haha, g_mc_seriously, g_mia_dots_21, g_mia_never, g_mc_nopromise])
links.append((g_mc_nopromise, g_mia_givesec))

# Choice 2.2
chain([g_mc_sayhi_22, g_mia_I, g_mia_what2, g_mc_hahajust,
       g_mia_cantbel, g_mia_uwant, g_mc_uwont, g_mia_I2,
       g_mia_question, g_mc_cry_emoji, g_mc_notmind,
       g_mia_uhm, g_mia_youre, g_mc_amazing, g_mia_weird_mc,
       g_mc_okok22, g_mc_gotext, g_mia_iwill])
links.append((g_mia_iwill, g_mia_givesec))

# Continue for all
chain([g_mia_givesec, g_tl_5m, g_mia_did_it, g_mia_ocean,
       g_mc_whatusay, g_mia_justsaid, g_mc_perfect,
       g_mia_tonight, g_mc_survive, g_mia_easy,
       g_mc_getready, g_mc_stare, g_mia_notstare,
       g_mc_mia2, g_mia_okiam, g_mc_go,
       g_mia_fine2, g_mia_talater, g_mc_later])

# KIRA morning section — IF kira_path
g_if_kira_morn_node = ifn("Kira_Path", 0, "", g_tl_later_a, g_tl_later_b,
                          preset_guid=g_if_kira_morn, after_guid=g_mc_later)
links.append((g_mc_later, g_if_kira_morn_node))

# kira_path true morning
chain([g_tl_later_a, g_kira_morn, g_kira_head, g_mc_gm_kira,
       g_kira_dont, g_mc_dontwhat, g_kira_cheerful, g_kira_early,
       g_mc_9am, g_kira_exactly, g_kira_waytoo,
       g_kira_clubbed, g_kira_woke, g_kira_notmove,
       g_mc_alive_k, g_kira_exp, g_kira_heard, g_kira_somethn,
       g_kira_getText, g_mc_haha_k, g_kira_good, g_kira_wkwould,
       g_mc_threwout, g_kira_orthat, g_kira_getout,
       g_kira_finally, g_kira_talater, g_mc_sure_k,
       g_tl_30m_k, g_kira_solate, g_kira_vid, g_kira_doeswork,
       g_kira_noanswer, g_mc_works, g_kira_yourloss, g_kira_bye])
links.append((g_kira_bye, g_social_mia))

# kira_path false morning
chain([g_tl_later_b, g_kira_hey, g_mc_mornkira2,
       g_kira_somiaxd, g_kira_casual, g_kira_notcasul,
       g_mc_convinced, g_kira_stranger, g_kira_genuinely,
       g_mc_2vs1, g_kira_isee, g_kira_goodfor, g_kira_bubble,
       g_kira_dormout, g_kira_lookgo, g_kira_nervous, g_kira_spooks,
       g_mc_iknow_k, g_kira_good2, g_kira_byenerd])
links.append((g_kira_byenerd, g_social_mia))

# Social posts + later Mia chat
chain([g_social_mia, g_mc_penpal2, g_mc_socialq,
       g_mia_godsaw, g_mc_ofcourse, g_mc_ilike,
       g_mia_4times, g_mc_why2, g_mia_feltwd, g_mia_whowants,
       g_mc_ido, g_mia_dots5, g_mia_okay,
       g_mc_dontdel, g_mia_wont, g_mc_good2,
       g_mc_so, g_mc_textback,
       g_mia_no, g_mia_notyet, g_mc_oksthat,
       g_mia_iknow2, g_mia_nostare2, g_mc_mia3, g_mia_iam,
       g_mc_ok3, g_mia_dots6, g_mia_letknow,
       g_mc_iknowyou, g_mia_shutup,
       g_mia_lecture2, g_mia_lecture_pic,
       g_mc_backrow, g_mia_obviously, g_mc_learn,
       g_mia_bye2, g_mc_bye2])

# Kira social post
chain([g_mc_bye2, g_social_kira])

# HE REPLIED section
chain([g_social_kira, g_tl_1h,
       g_mia_replied1, g_mia_replied2, g_mc_breathe,
       g_mia_cant2, g_mia_hallway, g_mia_cantread,
       g_mc_screenshot, g_mia_okok2, g_mia_holdon,
       g_mia_screensht, g_mia_omg2, g_mia_cutie, g_mia_cutie2,
       g_mc_dots_r])

# CHOICE: Mia slow down / That's great
links.append((g_mc_dots_r, g_choice_reply))
links.append((g_choice_reply, g_mia_what_r))    # port 0: slow down
links.append((g_choice_reply, g_mc_great_r))    # port 1: great

# Choice 1 (slow down)
chain([g_mia_what_r, g_mc_idontknow, g_mc_notsound, g_mia_whatmean,
       g_mc_forward, g_mc_reallyfw, g_mc_hitorask,
       g_mia_confident, g_mia_left_note, g_mc_iguess,
       g_mia_beingwrd, g_mia_thought, g_mc_iam, g_mc_careful,
       g_mia_canbe, g_mia_justtxting])
links.append((g_mia_justtxting, g_mia_nextlect))

# Choice 2 (great)
chain([g_mc_great_r, g_mia_right_r, g_mc_gowitit, g_mc_clearlyinto,
       g_mia_whatback, g_mc_sayanything, g_mc_hardpart,
       g_mia_okay_r, g_mia_okayican, g_mia_whatdo,
       g_mc_uptoyou, g_mc_textcasual,
       g_mia_rightcas, g_mia_sonotcas])
links.append((g_mia_sonotcas, g_mia_nextlect))

# Continue for both
chain([g_mia_nextlect, g_mia_cantthink, g_mc_try, g_mia_talater2, g_mc_bye3])

# KIRA 2 hours later
chain([g_mc_bye3, g_tl_2h_k, g_mc_kira2, g_mc_replied_k,
       g_kira_wait, g_kira_noteguy, g_mc_yeah_k,
       g_kira_and, g_mc_losing, g_kira_haha_k, g_kira_lecture2])

# IF kira_path for lecture vibrating line
g_if_kira_lect_node = ifn("Kira_Path", 0, "", g_kira_vibrate, g_kira_whatname,
                          preset_guid=g_if_kira_lect, after_guid=g_kira_lecture2)
links.append((g_kira_lecture2, g_if_kira_lect_node))
links.append((g_kira_vibrate, g_kira_whatname))

# Continue kira Jack section
chain([g_kira_whatname, g_mc_jack, g_kira_dots_k, g_kira_jackwhat,
       g_mc_justsaid_k, g_mc_why_k,
       g_kira_nothing, g_kira_ijust, g_kira_jacki,
       g_kira_diffone, g_kira_common,
       g_mc_morethan1, g_kira_yeahyeah, g_kira_dontworry, g_kira_gotalk])

# MIA after lecture
chain([g_kira_gotalk, g_tl_after_lect,
       g_mia_survived, g_mc_proud, g_mia_lessyd, g_mc_notpoint,
       g_mia_kirafound, g_mia_sitting2, g_mia_bench,
       g_mc_howkira, g_mia_shaking, g_mia_notstop,
       g_mc_point_k, g_mia_okok3, g_mia_strange, g_mia_dontyou,
       g_mc_yeathey, g_mia_kirasays, g_mc_iagree, g_mc_probwont, g_mc_liked_bench,
       g_mia_right_r2, g_mia_iguess2,
       g_mia_whatdo2, g_mc_simple2, g_mc_asksomethg,
       g_mia_likewhat, g_mc_anything, g_mc_dontleave,
       g_mia_okay2, g_mia_sec2,
       g_tl_3m,
       g_mia_said_hey, g_mia_isgood, g_mc_yeagood, g_mc_natural,
       g_mia_nowwait, g_mc_nowwait,
       g_tl_2m,
       g_mia_replied3, g_mia_ohgod, g_mia_yeagood, g_mia_tonight2,
       g_mia_selfie_too, g_mia_jack,
       g_mc_wait, g_mia_dots7, g_mc_whosguy,
       g_mia_notbench, g_mia_kiragq])

# KIRA while sitting next to Mia
chain([g_mia_kiragq, g_tl_kira_sits,
       g_kira_itshim, g_kira_talking, g_kira_notnice, g_kira_tread,
       g_mc_howbad, g_kira_thrownout, g_mc_ohw])

# MIA final line + END
chain([g_mc_ohw, g_mia_whatdo3, g_end])

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
    "  m_Name: Chapter 3 - Something's off\n"
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
out_path = os.path.join(os.path.dirname(__file__), "Chapter 3 - Something's off.asset")
with open(out_path, "w", encoding="utf-8") as f:
    f.write(out)
print(f"Written {len(out)} bytes  ->  {out_path}")
print(f"Nodes: {len(cnodes)} choice | {len(dnodes)} dialogue | "
      f"{len(enodes)} event | {len(ifnodes)} if | {len(stnodes)} start | {len(endnodes)} end")
print(f"Links: {len(links)}")
