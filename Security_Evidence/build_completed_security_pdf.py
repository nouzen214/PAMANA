import os
import pypdf
from reportlab.lib.pagesizes import A4
from reportlab.lib import colors
from reportlab.platypus import (
    SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle, Image as RLImage, PageBreak
)
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.pdfgen import canvas

WORKDIR = r"D:\CLIPS\Capstone\Security_Evidence"
ORIGINAL_PDF = r"C:\Users\Nouzen\Downloads\Policarpio_IT9_SoftwareSecurity.pdf"
HEADER_IMG = os.path.join(WORKDIR, "uphsl_header.png")
OUTPUT_PDF = os.path.join(WORKDIR, "Policarpio_IT9_SoftwareSecurity_Completed.pdf")

# Evidence Images
EV1 = os.path.join(WORKDIR, "Security_Evidence_1_Session_Isolation.png")
EV2 = os.path.join(WORKDIR, "Security_Evidence_2_Defensive_Validation.png")
EV3 = os.path.join(WORKDIR, "Security_Evidence_3_Access_Enforcement.png")
EV4 = os.path.join(WORKDIR, "Security_Evidence_4_Git_Traceability.png")

class NumberedCanvas(canvas.Canvas):
    def __init__(self, *args, **kwargs):
        super(NumberedCanvas, self).__init__(*args, **kwargs)
        self._saved_page_states = []

    def showPage(self):
        self._saved_page_states.append(dict(self.__dict__))
        self._startPage()

    def save(self):
        num_pages = len(self._saved_page_states)
        for state in self._saved_page_states:
            self.__dict__.update(state)
            self.draw_decorations(num_pages)
            super(NumberedCanvas, self).showPage()
        super(NumberedCanvas, self).save()

    def draw_decorations(self, num_pages):
        # Draw header banner at top
        if os.path.exists(HEADER_IMG):
            self.drawImage(HEADER_IMG, -1.125, 752.0, width=597.75, height=89.25, preserveAspectRatio=False)
        
        # Draw footer page number (starts at 12)
        page_num = self._pageNumber + 11  # First 11 pages from original PDF
        self.setFont("Helvetica", 10)
        self.drawRightString(542, 36, str(page_num))

def build_security_part():
    temp_pdf = os.path.join(WORKDIR, "part_viii_generated.pdf")
    doc = SimpleDocTemplate(
        temp_pdf,
        pagesize=A4,
        leftMargin=54,
        rightMargin=54,
        topMargin=95,   # Below header banner
        bottomMargin=54
    )

    styles = getSampleStyleSheet()

    title_style = ParagraphStyle(
        'SectionTitle',
        parent=styles['Normal'],
        fontName='Helvetica-Bold',
        fontSize=13,
        leading=16,
        textColor=colors.HexColor('#1E3A8A'),
        spaceAfter=4
    )
    subtitle_style = ParagraphStyle(
        'SubTitle',
        parent=styles['Normal'],
        fontName='Helvetica-Bold',
        fontSize=10,
        leading=13,
        textColor=colors.HexColor('#1F2937'),
        spaceBefore=4,
        spaceAfter=4
    )
    body_style = ParagraphStyle(
        'Body',
        parent=styles['Normal'],
        fontName='Helvetica',
        fontSize=9.5,
        leading=13.5,
        textColor=colors.HexColor('#1F2937'),
        spaceAfter=4
    )
    body_bold = ParagraphStyle(
        'BodyBold',
        parent=styles['Normal'],
        fontName='Helvetica-Bold',
        fontSize=9.5,
        leading=13.5,
        textColor=colors.HexColor('#111827')
    )
    caption_style = ParagraphStyle(
        'Caption',
        parent=styles['Normal'],
        fontName='Helvetica-Bold',
        fontSize=8.5,
        leading=11,
        textColor=colors.HexColor('#374151'),
        alignment=1, # Center
        spaceBefore=3,
        spaceAfter=6
    )

    story = []

    # --- PAGE 12: PART VIII HEADER & INDIVIDUAL CONTRIBUTION ---
    story.append(Paragraph("PART VIII. INDIVIDUAL SECURITY CONTRIBUTION", title_style))
    story.append(Paragraph("5 Points", ParagraphStyle('Pts', fontName='Helvetica-Bold', fontSize=9, textColor=colors.HexColor('#4B5563'))))
    story.append(Paragraph("Although the system security review is performed as a group, each student must identify their own contribution.", body_style))
    story.append(Spacer(1, 4))

    # Contribution Table
    table_data = [
        [Paragraph("<b>Student Name:</b>", body_style), Paragraph("Policarpio, Lance Angelo A.", body_bold)],
        [Paragraph("<b>Group Number:</b>", body_style), Paragraph("Group IT9", body_style)],
        [Paragraph("<b>Security Area Reviewed:</b>", body_style), 
         Paragraph("Broken Access Control, Session Bleed Prevention, and Defensive Data Boundary Validation (OWASP A01, A04, A07)", body_bold)],
        [Paragraph("<b>Component / File / Configuration:</b>", body_style), 
         Paragraph("• <code>Assets/Scripts/Firebase/FirebaseDatabaseManager.cs</code> (<code>ClearAllLocalGameData()</code>, <code>ValidatePlayerData()</code>)<br/>"
                   "• <code>Assets/Scripts/MainMenuController.cs</code> (<code>UpdateMenuAccountState()</code>, <code>OnLogoutClicked()</code>)<br/>"
                   "• <code>Assets/Scripts/CharacterSelectManager.cs</code> (<code>SanitizeAndValidateName()</code>)<br/>"
                   "• <code>Assets/Scripts/LockedPortalDoor.cs</code> (Physical collider access barrier)", body_style)],
        [Paragraph("<b>Specific Contribution:</b>", body_style),
         Paragraph("As Lead Developer, I engineered the session lifecycle, account isolation, and defensive boundary validation mechanisms in PAMANA to protect student data on shared lab devices:<br/>"
                   "• <b>Session Isolation & Account Purging:</b> Implemented <code>ClearAllLocalGameData()</code> in <code>FirebaseDatabaseManager.cs</code>. When a student signs out or switches accounts, it completely wipes cached in-memory progression, silences inventory triggers, and deletes all <code>PlayerPrefs</code> keys, guaranteeing zero cross-account data residue.<br/>"
                   "• <b>Dynamic Authentication Flow:</b> Designed the conditional Google account logout button in <code>MainMenuController.cs</code>, ensuring session termination exclusively for Google-authenticated users while hiding it for local guests to maintain least privilege.<br/>"
                   "• <b>Defensive Boundary Validation:</b> Created <code>ValidatePlayerData()</code> to clamp and normalize progress payloads (coins clamped >= 0, relics clamped 0..5, rooms clamped 1..12) prior to JSON serialization and cloud dispatch.<br/>"
                   "• <b>Input Sanitization:</b> Authored <code>SanitizeAndValidateName()</code> in <code>CharacterSelectManager.cs</code>, utilizing character whitelisting and 15-character length constraints to prevent UI injection and database key corruption.", body_style)],
        [Paragraph("<b>Security Improvement or Test Performed:</b>", body_style),
         Paragraph("• <b>Test 1 (Negative Session Bleed Test):</b> Logged in as Student Account A, progressed into Room 1, and retrieved an artifact. Clicked Logout to trigger <code>ClearAllLocalGameData()</code>. Logged in as Account B and verified 0 relics and 0 coins, confirming complete cache purging.<br/>"
                   "• <b>Test 2 (Negative Injection & Fuzzing Test):</b> Injected <code>&lt;script&gt;alert('xss')&lt;/script&gt;</code> and 200+ characters into the character naming field; verified input was sanitized to 'scriptalert' within 15 chars.<br/>"
                   "• <b>Test 3 (Negative Data Tampering Test):</b> Injected negative currency (-500 coins) and invalid relic counts (99); verified <code>ValidatePlayerData()</code> normalized values to valid bounds.", body_style)],
        [Paragraph("<b>Repository / Changeset / Commit Evidence:</b>", body_style),
         Paragraph("• Latest Commit: <code>d4e924c</code> (Implement new Main Menu with hover states, Continue cloud save flow, and dynamic Google logout button)<br/>"
                   "• Baseline Commit: <code>86c5f68</code> (Add input sanitization, database boundary clamping, and session clearing)<br/>"
                   "• Repository URL: <code>https://github.com/nouzen214/PAMANA.git</code> (Branch: <code>main</code>)", body_style)],
    ]

    t = Table(table_data, colWidths=[150, 338])
    t.setStyle(TableStyle([
        ('BACKGROUND', (0,0), (-1,-1), colors.HexColor('#F8FAFC')),
        ('BOX', (0,0), (-1,-1), 1, colors.HexColor('#CBD5E1')),
        ('INNERGRID', (0,0), (-1,-1), 0.5, colors.HexColor('#E2E8F0')),
        ('TOPPADDING', (0,0), (-1,-1), 3),
        ('BOTTOMPADDING', (0,0), (-1,-1), 3),
        ('LEFTPADDING', (0,0), (-1,-1), 6),
        ('RIGHTPADDING', (0,0), (-1,-1), 6),
    ]))
    story.append(t)

    # --- PAGE 13: Evidence Figures 1 & 2 ---
    story.append(PageBreak())
    story.append(Paragraph("<b>Security Evidence (Screenshots & Source Implementation):</b>", subtitle_style))
    story.append(Paragraph("The following screenshots and technical logs document the implemented security controls and negative test validations:", body_style))
    story.append(Spacer(1, 4))

    if os.path.exists(EV1):
        story.append(RLImage(EV1, width=485, height=270))
        story.append(Paragraph("<b>Figure 1:</b> Session Isolation & Account Purge on Sign-Out (MainMenuController logout button & ClearAllLocalGameData() implementation).", caption_style))
        story.append(Spacer(1, 8))

    if os.path.exists(EV2):
        story.append(RLImage(EV2, width=485, height=270))
        story.append(Paragraph("<b>Figure 2:</b> Defensive Input Sanitization & Numeric Boundary Clamping (SanitizeAndValidateName() and ValidatePlayerData() verification).", caption_style))

    # --- PAGE 14: Evidence Figures 3 & 4 ---
    story.append(PageBreak())
    if os.path.exists(EV3):
        story.append(RLImage(EV3, width=485, height=270))
        story.append(Paragraph("<b>Figure 3:</b> Client-Side Access Enforcement & Room Barriers (LockedPortalDoor collider barrier and prerequisite verification).", caption_style))
        story.append(Spacer(1, 8))

    if os.path.exists(EV4):
        story.append(RLImage(EV4, width=485, height=270))
        story.append(Paragraph("<b>Figure 4:</b> Version Control Traceability & Security Commits (Author: Lance Angelo Policarpio [nouzen214], clean credentials audit).", caption_style))

    # --- PAGE 15: Reflection Question ---
    story.append(PageBreak())
    story.append(Paragraph("<b>Briefly answer:</b>", subtitle_style))
    story.append(Paragraph("<b>What security consideration will you personally check while developing succeeding features?</b>", subtitle_style))
    story.append(Paragraph("<i>Limit your response to approximately 100 words.</i>", ParagraphStyle('Ital', fontName='Helvetica-Oblique', fontSize=8.5, textColor=colors.HexColor('#6B7280'), spaceAfter=8)))

    reflection_text = (
        "While developing succeeding features—such as the battle retake system, museum restoration murals, and audio settings—I "
        "will personally check that all state transitions and game rewards are validated defensively on both the client and "
        "backend rather than trusting client-side state flags. I will ensure that every new interactive object (such as restored "
        "pedestals or reward triggers) validates prerequisites before altering progress, that outgoing database payloads remain "
        "strictly bounded within legitimate numerical ranges, and that no unverified or unsanitized user input reaches our cloud "
        "endpoints or local storage, preventing logic manipulation and progress corruption."
    )
    words = reflection_text.split()
    word_count = len(words)

    story.append(Paragraph(reflection_text, ParagraphStyle('Reflect', fontName='Helvetica', fontSize=10, leading=15, textColor=colors.HexColor('#111827'))))
    story.append(Spacer(1, 6))
    story.append(Paragraph(f"<b>Word Count:</b> {word_count} words", ParagraphStyle('WCount', fontName='Helvetica-Bold', fontSize=8.5, textColor=colors.HexColor('#059669'))))
    story.append(Spacer(1, 14))

    # Security Verification Summary Table
    sec_summary = [
        [Paragraph("<b>Security Verification Summary</b>", ParagraphStyle('H', fontName='Helvetica-Bold', fontSize=10, textColor=colors.HexColor('#1E3A8A'))), 
         Paragraph("<b>Outcome</b>", ParagraphStyle('H2', fontName='Helvetica-Bold', fontSize=10, textColor=colors.HexColor('#1E3A8A'), alignment=1))],
        [Paragraph("Session Isolation & Account Purging", body_style), Paragraph("<font color='#166534'><b>PASS (Zero session bleed verified)</b></font>", body_style)],
        [Paragraph("User Input Sanitization & XSS Defense", body_style), Paragraph("<font color='#166534'><b>PASS (Malicious characters stripped)</b></font>", body_style)],
        [Paragraph("Defensive Data Boundary Clamping", body_style), Paragraph("<font color='#166534'><b>PASS (Normalized to legitimate bounds)</b></font>", body_style)],
        [Paragraph("Access Control & Barrier Enforcement", body_style), Paragraph("<font color='#166534'><b>PASS (Door clipping barrier active)</b></font>", body_style)],
        [Paragraph("Transport Security & Secrets Sanitization", body_style), Paragraph("<font color='#166534'><b>PASS (HTTPS enforced; 0 secret keys)</b></font>", body_style)],
    ]
    st = Table(sec_summary, colWidths=[310, 178])
    st.setStyle(TableStyle([
        ('BACKGROUND', (0,0), (-1,0), colors.HexColor('#EFF6FF')),
        ('BACKGROUND', (0,1), (-1,-1), colors.HexColor('#FFFFFF')),
        ('BOX', (0,0), (-1,-1), 1, colors.HexColor('#93C5FD')),
        ('INNERGRID', (0,0), (-1,-1), 0.5, colors.HexColor('#E2E8F0')),
        ('TOPPADDING', (0,0), (-1,-1), 5),
        ('BOTTOMPADDING', (0,0), (-1,-1), 5),
        ('LEFTPADDING', (0,0), (-1,-1), 8),
        ('RIGHTPADDING', (0,0), (-1,-1), 8),
    ]))
    story.append(st)

    doc.build(story, canvasmaker=NumberedCanvas)
    print("Built part_viii_generated.pdf")
    return temp_pdf

def merge_security_pdfs(generated_part_pdf):
    reader_orig = pypdf.PdfReader(ORIGINAL_PDF)
    reader_gen = pypdf.PdfReader(generated_part_pdf)
    writer = pypdf.PdfWriter()

    # Append pages 1 to 11 from original PDF (Parts I to VII)
    for i in range(11):
        writer.add_page(reader_orig.pages[i])

    # Append generated pages (Pages 12, 13, 14, 15)
    for p in reader_gen.pages:
        writer.add_page(p)

    with open(OUTPUT_PDF, 'wb') as f:
        writer.write(f)

    print(f"Successfully generated {OUTPUT_PDF} with {len(writer.pages)} pages total!")

    import shutil
    shutil.copyfile(OUTPUT_PDF, ORIGINAL_PDF)
    print(f"Updated {ORIGINAL_PDF} directly!")

if __name__ == "__main__":
    gen_pdf = build_security_part()
    merge_security_pdfs(gen_pdf)

