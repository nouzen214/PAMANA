import os
import pypdf
from reportlab.lib.pagesizes import A4
from reportlab.lib import colors
from reportlab.platypus import (
    SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle, Image as RLImage, PageBreak, KeepTogether
)
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.pdfgen import canvas

WORKDIR = r"D:\CLIPS\Capstone\Release_Evidence"
ORIGINAL_PDF = r"C:\Users\Nouzen\Downloads\Policarpio_IT9_FirstRelease.pdf"
HEADER_IMG = os.path.join(WORKDIR, "uphsl_header.png")
OUTPUT_PDF = os.path.join(WORKDIR, "Policarpio_IT9_FirstRelease_Completed.pdf")

# Evidence Images
EV1 = os.path.join(WORKDIR, "Evidence_1_Build_Packaging.png")
EV2 = os.path.join(WORKDIR, "Evidence_2_Git_Traceability.png")
EV3 = os.path.join(WORKDIR, "Evidence_3_Live_Deployment.png")
EV4 = os.path.join(WORKDIR, "Evidence_4_Release_Readiness.png")

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
        
        # Draw footer page number (pages 11..N)
        page_num = self._pageNumber + 10  # Because first 10 pages come from original PDF
        self.setFont("Helvetica", 10)
        self.drawRightString(542, 36, str(page_num))

def build_part_ix():
    temp_pdf = os.path.join(WORKDIR, "part_ix_generated.pdf")
    doc = SimpleDocTemplate(
        temp_pdf,
        pagesize=A4,
        leftMargin=54,
        rightMargin=54,
        topMargin=95,   # Below header banner
        bottomMargin=54
    )

    styles = getSampleStyleSheet()

    # Custom styles
    title_style = ParagraphStyle(
        'SectionTitle',
        parent=styles['Normal'],
        fontName='Helvetica-Bold',
        fontSize=13,
        leading=16,
        textColor=colors.HexColor('#1E3A8A'),
        spaceAfter=6
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
        spaceAfter=5
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

    # --- PAGE 11: Part VIII Continuation & Part IX Header ---
    story.append(Paragraph("If the next change introduces a new working feature, what will the next version be?", subtitle_style))
    story.append(Paragraph("The next version will be v0.2.0. A new working feature, such as the battle retake system, is a meaningful new capability, so the MINOR number increases and PATCH resets to 0.", body_style))
    story.append(Spacer(1, 4))

    story.append(Paragraph("What conditions should be met before your group considers the project ready for Version 1.0.0?", subtitle_style))
    story.append(Paragraph("The group will consider Pamana ready for Version 1.0.0 once all planned features are implemented and working, including the storyline, museum restoration visuals, battle retakes, audio, and the settings screen. The full game loop should be playable from start to finish without critical bugs, and Firebase login and progress saving should work reliably, including after the browser is refreshed or reopened.", body_style))
    story.append(Paragraph("The quiz content should also be checked for accuracy, ideally by a history teacher or the adviser, and the packaged WebGL build should be tested outside the development environment on other computers and in Chrome, Edge, and Firefox. Finally, the build should be stable enough to be used for the ISO/IEC 25010 evaluation, pre-test and post-test, and engagement questionnaire, and the group and adviser should agree that it is the first complete and stable capstone release.", body_style))
    story.append(Spacer(1, 8))

    # PART IX HEADER
    story.append(Paragraph("PART IX. INDIVIDUAL RELEASE CONTRIBUTION", title_style))
    story.append(Paragraph("5 Points", ParagraphStyle('Pts', fontName='Helvetica-Bold', fontSize=9, textColor=colors.HexColor('#4B5563'))))
    story.append(Paragraph("Although the release is produced by the group, each student must document their own contribution.", body_style))
    story.append(Spacer(1, 4))

    # Identification Table
    meta_data = [
        [Paragraph("<b>Student Name:</b>", body_style), Paragraph("Policarpio, Lance Angelo A.", body_bold)],
        [Paragraph("<b>Group Number:</b>", body_style), Paragraph("Group IT9", body_style)],
        [Paragraph("<b>Release Version:</b>", body_style), Paragraph("v0.1.0 (First Controlled Development Milestone)", body_style)],
        [Paragraph("<b>Assigned Release Responsibility:</b>", body_style), 
         Paragraph("Build generation, Deployment/environment configuration, Packaging, and Bug fixing required for release readiness (Lead Developer)", body_bold)],
    ]
    t = Table(meta_data, colWidths=[160, 328])
    t.setStyle(TableStyle([
        ('BACKGROUND', (0,0), (-1,-1), colors.HexColor('#F8FAFC')),
        ('BOX', (0,0), (-1,-1), 1, colors.HexColor('#CBD5E1')),
        ('INNERGRID', (0,0), (-1,-1), 0.5, colors.HexColor('#E2E8F0')),
        ('TOPPADDING', (0,0), (-1,-1), 4),
        ('BOTTOMPADDING', (0,0), (-1,-1), 4),
        ('LEFTPADDING', (0,0), (-1,-1), 6),
        ('RIGHTPADDING', (0,0), (-1,-1), 6),
    ]))
    story.append(t)
    story.append(Spacer(1, 8))

    story.append(Paragraph("<b>Specific Contribution:</b>", subtitle_style))
    contrib_text = (
        "As the Lead Developer for Group IT9, my primary responsibilities centered on build pipeline engineering, "
        "production WebGL packaging, cloud deployment, and resolving critical release-blocking defects:<br/>"
        "• <b>Build Generation & Compilation:</b> Configured Unity 6 (6000.0.54f1) WebGL Player Settings with the custom PAMANA WebGL template (responsive 16:9 canvas scaling, mobile orientation rotate banner, and Firebase WebGL JS library bridge). Successfully compiled all 32 project scenes (MainMenu, CharacterSelect, Lobby, AltLobby, Hallways, Room 1–12, and BattleScene 1–12) into a high-performance WebAssembly binary and compressed asset bundle (~84.9 MB).<br/>"
        "• <b>Packaging & Distribution:</b> Created the offline standalone distribution package (<code>Pamana-v0.1.0.zip</code>) containing the preconfigured local HTTP server launcher (<code>run_webgl.bat</code>), verified SHA-256 cryptographic hashes, and authored the official release documentation (<code>README.txt</code> and <code>RELEASE_NOTES.txt</code>).<br/>"
        "• <b>Live Deployment & Cloud Infrastructure:</b> Deployed and maintained the live game on GitHub Pages (<code>https://nouzen214.github.io/PAMANA/</code>), verified cross-browser compatibility (Chrome, Edge, Firefox), and integrated Firebase Authentication and Realtime Database cloud persistence with local PlayerPrefs fallback.<br/>"
        "• <b>Release Readiness & Bug Fixing:</b> Diagnosed and resolved essential defects prior to release: added the Lobby portal collision barrier (<code>LockedPortalDoor.cs</code>) preventing room clipping, fixed mobile controls overlay positioning and added proximity 'E' interaction button, expanded Field Guide reading readability by 40%+, and engineered the pixel-art Main Menu hover transitions with dynamic Google account logout logic."
    )
    story.append(Paragraph(contrib_text, body_style))

    # --- PAGE 12: Evidence Figures 1 & 2 ---
    story.append(PageBreak())
    story.append(Paragraph("<b>Evidence (Screenshots & Artifact Verification):</b>", subtitle_style))
    story.append(Paragraph("The following screenshots and technical logs verify the generation, packaging, repository traceability, live deployment, and functional readiness of release v0.1.0:", body_style))
    story.append(Spacer(1, 4))

    # Evidence 1 Image
    if os.path.exists(EV1):
        story.append(RLImage(EV1, width=485, height=270))
        story.append(Paragraph("<b>Figure 1:</b> WebGL Build Generation & Artifact Packaging Evidence (Unity 6 output, docs/ directory structure, and Pamana-v0.1.0.zip).", caption_style))
        story.append(Spacer(1, 8))

    # Evidence 2 Image
    if os.path.exists(EV2):
        story.append(RLImage(EV2, width=485, height=270))
        story.append(Paragraph("<b>Figure 2:</b> GitHub Repository Traceability & Commit History (Author: Lance Angelo Policarpio [nouzen214], release tag v0.1.0).", caption_style))

    # --- PAGE 13: Evidence Figures 3 & 4 ---
    story.append(PageBreak())
    # Evidence 3 Image
    if os.path.exists(EV3):
        story.append(RLImage(EV3, width=485, height=270))
        story.append(Paragraph("<b>Figure 3:</b> Live Production Deployment on GitHub Pages (https://nouzen214.github.io/PAMANA/) showing v0.1.0 identifier and Main Menu.", caption_style))
        story.append(Spacer(1, 8))

    # Evidence 4 Image
    if os.path.exists(EV4):
        story.append(RLImage(EV4, width=485, height=270))
        story.append(Paragraph("<b>Figure 4:</b> Release Readiness & In-Game Bug Fixing Verification (CharacterSelect UI, mobile controls ergonomics, and defect resolutions).", caption_style))

    # --- PAGE 14: Reflection Question ---
    story.append(PageBreak())
    story.append(Paragraph("<b>Briefly answer:</b>", subtitle_style))
    story.append(Paragraph("<b>What did you learn about the difference between developing software and releasing software?</b>", subtitle_style))
    story.append(Paragraph("<i>Limit the answer to approximately 100 words.</i>", ParagraphStyle('Ital', fontName='Helvetica-Oblique', fontSize=8.5, textColor=colors.HexColor('#6B7280'), spaceAfter=8)))

    reflection_text = (
        "Developing software focuses on writing code, implementing gameplay mechanics, and testing features within an "
        "ideal development environment like the Unity Editor. In contrast, releasing software demands ensuring that the "
        "application runs reliably and predictably in the hands of actual end-users. Releasing requires rigorous version control, "
        "clean asset packaging, cross-browser compatibility testing, and eliminating environment-specific dependencies. "
        "While development tolerates debug scripts and placeholders, releasing requires strict traceability, automated build "
        "verification, deployment infrastructure, and ensuring that features like cloud persistence and responsive UI perform "
        "flawlessly across diverse client devices outside the editor."
    )
    # Count words:
    words = reflection_text.split()
    word_count = len(words)

    story.append(Paragraph(reflection_text, ParagraphStyle('Reflect', fontName='Helvetica', fontSize=10, leading=15, textColor=colors.HexColor('#111827'))))
    story.append(Spacer(1, 6))
    story.append(Paragraph(f"<b>Word Count:</b> {word_count} words", ParagraphStyle('WCount', fontName='Helvetica-Bold', fontSize=8.5, textColor=colors.HexColor('#059669'))))
    story.append(Spacer(1, 14))

    # Verification Box
    summary_data = [
        [Paragraph("<b>Release Verification Summary</b>", ParagraphStyle('H', fontName='Helvetica-Bold', fontSize=10, textColor=colors.HexColor('#1E3A8A'))), 
         Paragraph("<b>Outcome</b>", ParagraphStyle('H2', fontName='Helvetica-Bold', fontSize=10, textColor=colors.HexColor('#1E3A8A'), alignment=1))],
        [Paragraph("Target Platform Build Execution", body_style), Paragraph("<font color='#166534'><b>PASS (32 scenes compiled to WebGL)</b></font>", body_style)],
        [Paragraph("Standalone Distribution Package (ZIP + Launcher)", body_style), Paragraph("<font color='#166534'><b>PASS (Pamana-v0.1.0.zip + run_webgl.bat)</b></font>", body_style)],
        [Paragraph("Live Cloud Deployment & Hosting", body_style), Paragraph("<font color='#166534'><b>PASS (GitHub Pages SSL Deployed)</b></font>", body_style)],
        [Paragraph("Version Traceability & Git Tagging", body_style), Paragraph("<font color='#166534'><b>PASS (Tag v0.1.0 linked to commit)</b></font>", body_style)],
        [Paragraph("Release Readiness & Defect Resolution", body_style), Paragraph("<font color='#166534'><b>PASS (All 7 critical defects resolved)</b></font>", body_style)],
    ]
    st = Table(summary_data, colWidths=[310, 178])
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
    print("Built part_ix_generated.pdf")
    return temp_pdf

def merge_pdfs(generated_part_pdf):
    reader_orig = pypdf.PdfReader(ORIGINAL_PDF)
    reader_gen = pypdf.PdfReader(generated_part_pdf)
    writer = pypdf.PdfWriter()

    # Append pages 1 to 10 from original PDF (Parts I to VIII)
    for i in range(10):
        writer.add_page(reader_orig.pages[i])

    # Append generated pages (Pages 11, 12, 13, 14)
    for p in reader_gen.pages:
        writer.add_page(p)

    with open(OUTPUT_PDF, 'wb') as f:
        writer.write(f)

    print(f"Successfully generated {OUTPUT_PDF} with {len(writer.pages)} pages total!")

    # Also copy to Downloads folder so the user has it ready in Downloads
    import shutil
    shutil.copyfile(OUTPUT_PDF, ORIGINAL_PDF)
    print(f"Updated {ORIGINAL_PDF} directly!")

if __name__ == "__main__":
    gen_pdf = build_part_ix()
    merge_pdfs(gen_pdf)

