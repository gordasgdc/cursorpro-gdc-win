# Genereaza Instructiuni-CursorProGDC.pdf, RO/EN/ES, cu reportlab.
# Foloseste Arial (nu Helvetica standard-14) pentru diacriticele romanesti.
#
# [2026-09-06] Ghid NOU pentru clientul Windows — separat de
# CursorPro/installer/generate_pdf.py (Mac), care documenteaza toate
# functiile Mac (Desen, Efecte de Clic, Afisare Taste, Preseturi Focus,
# Semnal multi-display). Clientul Windows are ASTAZI doar Halo & Spotlight,
# Zoom de baza si Licenta (vezi CLAUDE.md, sectiunea "Arhitectura INCA
# NEPORTATA") — un ghid identic cu cel de Mac ar documenta functii care
# nu exista inca aici, derutant pentru utilizator. Continutul de mai jos
# a fost verificat DIRECT in PreferencesWindow.xaml (nu presupus).
#
# Ruleaza cu: python3 installer/generate_pdf.py
import os
from reportlab.lib.pagesizes import A4
from reportlab.lib.units import cm
from reportlab.lib import colors
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import SimpleDocTemplate, Paragraph, ListFlowable, ListItem, PageBreak

OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "Instructiuni-CursorProGDC.pdf")

pdfmetrics.registerFont(TTFont("Arial", "/System/Library/Fonts/Supplemental/Arial.ttf"))
pdfmetrics.registerFont(TTFont("Arial-Bold", "/System/Library/Fonts/Supplemental/Arial Bold.ttf"))
styles = getSampleStyleSheet()
ACCENT = colors.HexColor("#B96A1E")
MUTED = colors.HexColor("#6a6a6a")
NOTE_BG = colors.HexColor("#FBF1E6")

title_style = ParagraphStyle("Title", parent=styles["Title"], fontName="Arial-Bold", fontSize=19, spaceAfter=2, textColor=colors.HexColor("#1a1a1a"))
subtitle_style = ParagraphStyle("Subtitle", parent=styles["Normal"], fontName="Arial", fontSize=11, textColor=MUTED, spaceAfter=20)
h2_style = ParagraphStyle("H2", parent=styles["Heading2"], fontName="Arial-Bold", fontSize=13, textColor=ACCENT, spaceBefore=16, spaceAfter=6)
body_style = ParagraphStyle("Body", parent=styles["Normal"], fontName="Arial", fontSize=10.5, leading=15, textColor=colors.HexColor("#1a1a1a"), spaceAfter=6)
step_style = ParagraphStyle("Step", parent=body_style, leftIndent=4, spaceAfter=5)
note_style = ParagraphStyle("Note", parent=body_style, backColor=NOTE_BG, leftIndent=10, fontSize=10)
footer_style = ParagraphStyle("Footer", parent=styles["Normal"], fontName="Arial", fontSize=8.5, textColor=colors.HexColor("#8a8a8a"), spaceBefore=20)


def numbered(items):
    return ListFlowable(
        [ListItem(Paragraph(it, step_style), leftIndent=16) for it in items],
        bulletType="1", start="1", leftIndent=16, spaceBefore=2, spaceAfter=8,
    )


def note(text):
    return Paragraph(text, note_style)


def page(d):
    flow = [Paragraph("CursorPro GDC — Windows", title_style), Paragraph(d["subtitle"], subtitle_style)]
    for h, body in d["sections"]:
        flow.append(Paragraph(h, h2_style))
        if isinstance(body, list):
            flow.append(numbered(body))
        elif isinstance(body, tuple):
            flow.append(note(body[0]))
        else:
            flow.append(Paragraph(body, body_style))
    flow.append(Paragraph("CursorPro GDC — github.com/gordasgdc/cursorpro-gdc-win", footer_style))
    return flow


RO = dict(
    subtitle="Instrucțiuni de instalare și utilizare — Română",
    sections=[
        ("1. Instalare", [
            "Descarcă instalatorul <b>CursorProGDC-Setup.exe</b> de pe pagina de descărcare sau din secțiunea Releases de pe GitHub.",
            "Dublu-click pe fișierul descărcat. Dacă apare avertismentul Windows SmartScreen, apasă „Mai multe informații” apoi „Rulează oricum”.",
            "La pagina de licență din instalator, selectează „Accept termenii acordului” — butonul „Următorul” rămâne dezactivat până bifezi asta.",
            "Urmează pașii instalatorului (locație implicită recomandată) până la „Instalează”.",
        ]),
        ("2. Cum se folosește — iconița din bara de sistem", [
            "CursorPro GDC nu are o fereastră principală — rulează permanent ca o iconiță mică în bara de sistem (System Tray, lângă ceas, colțul din dreapta-jos al ecranului).",
            "<b>Click stânga</b> pe iconiță — deschide direct fereastra de Preferințe.",
            "<b>Click dreapta</b> pe iconiță — arată un meniu scurt: starea licenței, „Preferințe…” și „Închide CursorPro GDC”.",
        ]),
        ("3. Halo cursor și Spotlight", [
            "Deschide Preferințe → tab-ul „Halo & Spotlight”.",
            "<b>Halo cursor</b> — bifează „Activează Halo”, apoi alege stilul (Inel / Umplut / Cruce), culoarea, diametrul și grosimea liniei. Halo-ul rămâne vizibil permanent pe ecran, nu are nevoie de o tastă ținută apăsată.",
            "<b>Spotlight</b> — întunecă tot ecranul în jurul cursorului, ca un reflector. Alege raza, cât de mult se întunecă restul ecranului (Întunecare), și tasta de activare (Alt/Ctrl/Shift/Windows). Ține apăsată acea tastă ca să activezi Spotlight — se dezactivează când o eliberezi.",
        ]),
        ("4. Zoom (lupă)", [
            "Deschide Preferințe → tab-ul „Zoom”.",
            "Alege nivelul de mărire (1.1x–12x) și tasta de activare (Alt/Ctrl/Shift/Windows).",
            "Ține apăsată tasta aleasă — o lupă circulară urmărește cursorul, mărind exact zona din jurul lui.",
        ]),
        ("", ("<b>Notă:</b> nivelul de mărire nu se poate ajusta încă live cu rotița de scroll (ca pe versiunea Mac) — se setează din Preferințe, înainte de a folosi lupa. Această ajustare live urmează într-o versiune viitoare.",)),
        ("5. Trial și activare", [
            "Aplicația funcționează complet, fără restricții, timp de <b>3 zile</b> de la prima pornire. După aceea, funcțiile principale se opresc automat până activezi o licență.",
            "Deschide Preferințe → tab-ul „Licență”. Acolo vezi ID-ul unic al calculatorului tău (Machine ID) — apasă „Copiază” dacă ai nevoie de el.",
            "Apasă „Cere activare pe WhatsApp” — mesajul include automat ID-ul tău.",
            "După ce primești codul de licență, lipește-l în câmpul „Cod de activare” și apasă „Activează”.",
        ]),
        ("", ("<b>Donație:</b> 9 € — susține continuarea dezvoltării aplicației, o singură dată, fără abonament. Nu e o vânzare — activarea se face manual, prin WhatsApp, pe baza donației.",)),
        ("6. Suport", "Pentru orice întrebare, scrie pe WhatsApp (buton în Preferințe → Licență) sau deschide un Issue pe GitHub."),
    ],
)

EN = dict(
    subtitle="Installation and usage instructions — English",
    sections=[
        ("1. Installation", [
            "Download the <b>CursorProGDC-Setup.exe</b> installer from the download page or the GitHub Releases section.",
            "Double-click the downloaded file. If the Windows SmartScreen warning appears, click “More info” then “Run anyway”.",
            "On the installer's license page, select “I accept the agreement” — the “Next” button stays disabled until you check this.",
            "Follow the installer steps (default location recommended) through to “Install”.",
        ]),
        ("2. How it works — the system tray icon", [
            "CursorPro GDC has no main window — it runs permanently as a small icon in the system tray (next to the clock, bottom-right corner of the screen).",
            "<b>Left-click</b> the icon — opens the Preferences window directly.",
            "<b>Right-click</b> the icon — shows a short menu: license status, “Preferences…”, and “Quit CursorPro GDC”.",
        ]),
        ("3. Cursor Halo and Spotlight", [
            "Open Preferences → the “Halo & Spotlight” tab.",
            "<b>Cursor Halo</b> — check “Enable Halo”, then pick the style (Ring / Filled / Cross), color, diameter, and line width. The halo stays permanently visible, no key needs to be held.",
            "<b>Spotlight</b> — dims the whole screen around the cursor, like a spotlight. Pick the radius, how much the rest of the screen dims (Dim), and the activation key (Alt/Ctrl/Shift/Windows). Hold that key to activate Spotlight — it turns off when you release it.",
        ]),
        ("4. Zoom (loupe)", [
            "Open Preferences → the “Zoom” tab.",
            "Pick the magnification level (1.1x–12x) and the activation key (Alt/Ctrl/Shift/Windows).",
            "Hold the chosen key — a circular loupe follows the cursor, magnifying exactly the area around it.",
        ]),
        ("", ("<b>Note:</b> the magnification level can't be adjusted live with the scroll wheel yet (like on the Mac version) — set it in Preferences before using the loupe. Live adjustment is coming in a future version.",)),
        ("5. Trial and activation", [
            "The app works fully, with no restrictions, for <b>3 days</b> from first launch. After that, the main features stop automatically until you activate a license.",
            "Open Preferences → the “License” tab. There you'll see your computer's unique ID (Machine ID) — click “Copy” if you need it.",
            "Click “Request activation on WhatsApp” — the message automatically includes your ID.",
            "Once you receive the license code, paste it into the “Activation code” field and click “Activate”.",
        ]),
        ("", ("<b>A donation, not a list price:</b> €9 — supports ongoing development, one-time, no subscription. Not a sale — activation happens manually, over WhatsApp, based on the donation.",)),
        ("6. Support", "For any question, message WhatsApp (button in Preferences → License) or open an Issue on GitHub."),
    ],
)

ES = dict(
    subtitle="Instrucciones de instalación y uso — Español",
    sections=[
        ("1. Instalación", [
            "Descarga el instalador <b>CursorProGDC-Setup.exe</b> desde la página de descarga o la sección Releases de GitHub.",
            "Haz doble clic en el archivo descargado. Si aparece el aviso de Windows SmartScreen, pulsa “Más información” y luego “Ejecutar de todas formas”.",
            "En la página de licencia del instalador, selecciona “Acepto el acuerdo” — el botón “Siguiente” permanece desactivado hasta que lo marques.",
            "Sigue los pasos del instalador (se recomienda la ubicación predeterminada) hasta “Instalar”.",
        ]),
        ("2. Cómo funciona — el icono de la bandeja del sistema", [
            "CursorPro GDC no tiene ventana principal — se ejecuta permanentemente como un pequeño icono en la bandeja del sistema (junto al reloj, esquina inferior derecha de la pantalla).",
            "<b>Clic izquierdo</b> en el icono — abre directamente la ventana de Preferencias.",
            "<b>Clic derecho</b> en el icono — muestra un menú breve: estado de la licencia, “Preferencias…” y “Salir de CursorPro GDC”.",
        ]),
        ("3. Halo del cursor y Spotlight", [
            "Abre Preferencias → la pestaña “Halo & Spotlight”.",
            "<b>Halo del cursor</b> — marca “Activar Halo”, luego elige el estilo (Anillo / Relleno / Cruz), el color, el diámetro y el grosor de línea. El halo permanece siempre visible, no necesita ninguna tecla pulsada.",
            "<b>Spotlight</b> — oscurece toda la pantalla alrededor del cursor, como un foco. Elige el radio, cuánto se oscurece el resto de la pantalla (Oscurecer), y la tecla de activación (Alt/Ctrl/Shift/Windows). Mantén pulsada esa tecla para activar Spotlight — se desactiva al soltarla.",
        ]),
        ("4. Zoom (lupa)", [
            "Abre Preferencias → la pestaña “Zoom”.",
            "Elige el nivel de ampliación (1.1x–12x) y la tecla de activación (Alt/Ctrl/Shift/Windows).",
            "Mantén pulsada la tecla elegida — una lupa circular sigue al cursor, ampliando exactamente el área alrededor de él.",
        ]),
        ("", ("<b>Nota:</b> el nivel de ampliación todavía no se puede ajustar en vivo con la rueda del ratón (como en la versión Mac) — se configura en Preferencias antes de usar la lupa. El ajuste en vivo llegará en una versión futura.",)),
        ("5. Prueba y activación", [
            "La app funciona completamente, sin restricciones, durante <b>3 días</b> desde el primer inicio. Después, las funciones principales se detienen automáticamente hasta que actives una licencia.",
            "Abre Preferencias → la pestaña “Licencia”. Allí verás el ID único de tu ordenador (Machine ID) — pulsa “Copiar” si lo necesitas.",
            "Pulsa “Solicitar activación por WhatsApp” — el mensaje incluye automáticamente tu ID.",
            "Cuando recibas el código de licencia, pégalo en el campo “Código de activación” y pulsa “Activar”.",
        ]),
        ("", ("<b>Una donación, no un precio de lista:</b> 9 € — apoya el desarrollo continuo, una sola vez, sin suscripción. No es una venta — la activación se hace manualmente, por WhatsApp, en base a la donación.",)),
        ("6. Soporte", "Para cualquier pregunta, escribe por WhatsApp (botón en Preferencias → Licencia) o abre un Issue en GitHub."),
    ],
)

doc = SimpleDocTemplate(OUT, pagesize=A4, leftMargin=2 * cm, rightMargin=2 * cm, topMargin=2.2 * cm, bottomMargin=2.2 * cm)
story = []
for i, lang in enumerate([RO, EN, ES]):
    story.extend(page(lang))
    if i < 2:
        story.append(PageBreak())
doc.build(story)
print("wrote", OUT)
