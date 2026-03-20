#!/usr/bin/env python3
"""Gera um único PDF com a documentação da CpfApi."""

import markdown
from weasyprint import HTML, CSS
from pathlib import Path

BASE = Path("/home/user/IA/CpfApi")

DOCS = [
    ("README", BASE / "README.md"),
    ("API", BASE / "docs" / "API.md"),
    ("TESTES", BASE / "docs" / "TESTES.md"),
]

CSS_STYLE = """
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;600;700&family=JetBrains+Mono:wght@400;600&display=swap');

@page {
    size: A4;
    margin: 2.2cm 2.5cm 2.2cm 2.5cm;
    @bottom-center {
        content: counter(page) " / " counter(pages);
        font-size: 9pt;
        color: #aaa;
    }
}

* { box-sizing: border-box; }

body {
    font-family: 'Inter', 'Segoe UI', Arial, sans-serif;
    font-size: 10.5pt;
    line-height: 1.65;
    color: #1a1a2e;
    margin: 0;
}

/* ── Page breaks ─────────────────────────────── */
.doc-section { page-break-after: always; }
.doc-section:last-child { page-break-after: avoid; }

/* ── Section header ─────────────────────────── */
.section-header {
    background: linear-gradient(135deg, #1a1a2e 0%, #16213e 60%, #0f3460 100%);
    color: #fff;
    padding: 36pt 30pt 28pt;
    margin: -2.2cm -2.5cm 28pt;
    border-bottom: 3pt solid #e94560;
}
.section-header .label {
    font-size: 8pt;
    letter-spacing: 2pt;
    text-transform: uppercase;
    color: #a0c4ff;
    margin-bottom: 6pt;
}
.section-header h1 {
    margin: 0;
    font-size: 22pt;
    font-weight: 700;
    color: #fff;
    border: none;
    padding: 0;
}
.section-header h1::after { display: none; }

/* ── Headings ───────────────────────────────── */
h1 {
    font-size: 17pt;
    font-weight: 700;
    color: #0f3460;
    margin: 24pt 0 8pt;
    padding-bottom: 5pt;
    border-bottom: 2pt solid #e94560;
}
h1::after {
    content: '';
    display: block;
    height: 2pt;
    background: #a0c4ff;
    margin-top: 2pt;
}
h2 {
    font-size: 13pt;
    font-weight: 700;
    color: #16213e;
    margin: 20pt 0 6pt;
    padding-left: 8pt;
    border-left: 3pt solid #e94560;
}
h3 {
    font-size: 11pt;
    font-weight: 600;
    color: #0f3460;
    margin: 14pt 0 4pt;
}
h4 {
    font-size: 10pt;
    font-weight: 600;
    color: #444;
    margin: 10pt 0 3pt;
}

/* ── Tables ─────────────────────────────────── */
table {
    width: 100%;
    border-collapse: collapse;
    margin: 10pt 0 14pt;
    font-size: 9.5pt;
    page-break-inside: avoid;
}
thead tr {
    background: #0f3460;
    color: #fff;
}
thead th {
    padding: 6pt 9pt;
    text-align: left;
    font-weight: 600;
    font-size: 9pt;
    letter-spacing: 0.3pt;
}
tbody tr:nth-child(even) { background: #f0f4ff; }
tbody tr:nth-child(odd)  { background: #fff; }
tbody td {
    padding: 5pt 9pt;
    border-bottom: 0.5pt solid #dde3f0;
    vertical-align: top;
}

/* ── Code ───────────────────────────────────── */
code {
    font-family: 'JetBrains Mono', 'Consolas', monospace;
    font-size: 8.5pt;
    background: #f0f4ff;
    color: #0f3460;
    padding: 1pt 4pt;
    border-radius: 3pt;
}
pre {
    background: #1a1a2e;
    color: #e2e8f0;
    padding: 11pt 14pt;
    border-radius: 5pt;
    font-size: 8pt;
    line-height: 1.55;
    overflow-x: auto;
    page-break-inside: avoid;
    margin: 8pt 0 12pt;
    border-left: 3pt solid #e94560;
}
pre code {
    background: transparent;
    color: #e2e8f0;
    padding: 0;
    font-size: 8pt;
}

/* ── Blockquote / note ──────────────────────── */
blockquote {
    background: #fff8e1;
    border-left: 3pt solid #ffb300;
    margin: 10pt 0;
    padding: 8pt 12pt;
    border-radius: 0 4pt 4pt 0;
    font-size: 9.5pt;
    color: #5a4000;
}

/* ── Lists ──────────────────────────────────── */
ul, ol {
    margin: 6pt 0 8pt 0;
    padding-left: 18pt;
}
li { margin-bottom: 3pt; }

/* ── Horizontal rule ────────────────────────── */
hr {
    border: none;
    border-top: 1pt solid #dde3f0;
    margin: 16pt 0;
}

/* ── Paragraph ──────────────────────────────── */
p { margin: 5pt 0 8pt; }

/* ── Strong / em ────────────────────────────── */
strong { color: #0f3460; }
"""

MD_EXTENSIONS = ["tables", "fenced_code", "codehilite", "toc", "nl2br", "sane_lists"]


def md_to_html(text: str) -> str:
    return markdown.markdown(text, extensions=MD_EXTENSIONS)


SECTION_LABELS = {
    "README": "Visão Geral",
    "API": "Documentação de Endpoints",
    "TESTES": "Suite de Testes",
}

SECTION_TITLES = {
    "README": "CpfApi — Consulta de CPF e CNPJ",
    "API": "Documentação da API",
    "TESTES": "Documentação de Testes",
}


def build_html() -> str:
    sections = []
    for key, path in DOCS:
        raw = path.read_text(encoding="utf-8")
        # Strip the first H1 since we render it in the section header
        lines = raw.splitlines()
        if lines and lines[0].startswith("# "):
            lines = lines[1:]
        body = md_to_html("\n".join(lines))

        section = f"""
<div class="doc-section">
  <div class="section-header">
    <div class="label">{SECTION_LABELS[key]}</div>
    <h1>{SECTION_TITLES[key]}</h1>
  </div>
  {body}
</div>"""
        sections.append(section)

    content = "\n".join(sections)
    return f"""<!DOCTYPE html>
<html lang="pt-BR">
<head>
<meta charset="utf-8">
<title>CpfApi — Documentação</title>
</head>
<body>
{content}
</body>
</html>"""


def main():
    out = Path("/home/user/IA/CpfApi_Documentacao.pdf")
    html = build_html()
    HTML(string=html, base_url=str(BASE)).write_pdf(
        out,
        stylesheets=[CSS(string=CSS_STYLE)],
    )
    print(f"PDF gerado: {out}  ({out.stat().st_size // 1024} KB)")


if __name__ == "__main__":
    main()
