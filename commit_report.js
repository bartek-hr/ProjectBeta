const {
  Document, Packer, Paragraph, TextRun, Table, TableRow, TableCell,
  AlignmentType, BorderStyle, WidthType, ShadingType, HeadingLevel
} = require('docx');
const fs = require('fs');

const data = [
  { name: "Bartek",   aliases: "bartek-hr (HR) + bartek (work)",     commits: 26, color: "4472C4" },
  { name: "RMJTromp", aliases: "me@rmjtromp + istudlions",           commits: 23, color: "ED7D31" },
  { name: "Burak",    aliases: "brkzmn + Burak Özmen (noreply)",      commits: 19, color: "A9D18E" },
  { name: "Floris",   aliases: "Floris28 (HR) + Floris Landman",     commits: 11, color: "FFD966" },
  { name: "Dominik",  aliases: "Dominik Nagy Costa",                  commits: 2,  color: "C9C9C9" },
];

const total = data.reduce((s, d) => s + d.commits, 0);
const maxCommits = Math.max(...data.map(d => d.commits));
const BAR_MAX_DXA = 5400;

const border = { style: BorderStyle.SINGLE, size: 1, color: "DDDDDD" };
const borders = { top: border, bottom: border, left: border, right: border };
const noBorder = { style: BorderStyle.NONE, size: 0, color: "FFFFFF" };
const noBorders = { top: noBorder, bottom: noBorder, left: noBorder, right: noBorder };

// Header row
const headerRow = new TableRow({
  children: [
    new TableCell({
      borders, width: { size: 1500, type: WidthType.DXA },
      shading: { fill: "2E4057", type: ShadingType.CLEAR },
      margins: { top: 100, bottom: 100, left: 120, right: 120 },
      children: [new Paragraph({ children: [new TextRun({ text: "Contributor", bold: true, color: "FFFFFF", font: "Arial", size: 20 })] })]
    }),
    new TableCell({
      borders, width: { size: 1000, type: WidthType.DXA },
      shading: { fill: "2E4057", type: ShadingType.CLEAR },
      margins: { top: 100, bottom: 100, left: 120, right: 120 },
      children: [new Paragraph({ alignment: AlignmentType.CENTER, children: [new TextRun({ text: "Commits", bold: true, color: "FFFFFF", font: "Arial", size: 20 })] })]
    }),
    new TableCell({
      borders, width: { size: 1000, type: WidthType.DXA },
      shading: { fill: "2E4057", type: ShadingType.CLEAR },
      margins: { top: 100, bottom: 100, left: 120, right: 120 },
      children: [new Paragraph({ alignment: AlignmentType.CENTER, children: [new TextRun({ text: "%", bold: true, color: "FFFFFF", font: "Arial", size: 20 })] })]
    }),
    new TableCell({
      borders, width: { size: BAR_MAX_DXA + 500, type: WidthType.DXA },
      shading: { fill: "2E4057", type: ShadingType.CLEAR },
      margins: { top: 100, bottom: 100, left: 120, right: 120 },
      children: [new Paragraph({ children: [new TextRun({ text: "Distribution", bold: true, color: "FFFFFF", font: "Arial", size: 20 })] })]
    }),
  ]
});

// Data rows
const dataRows = data.map(d => {
  const pct = ((d.commits / total) * 100).toFixed(1);
  const barWidth = Math.round((d.commits / maxCommits) * BAR_MAX_DXA);
  const remainWidth = BAR_MAX_DXA - barWidth;

  const barCellChildren = [
    new Table({
      width: { size: BAR_MAX_DXA, type: WidthType.DXA },
      columnWidths: remainWidth > 0 ? [barWidth, remainWidth] : [barWidth],
      borders: { insideH: noBorder, insideV: noBorder },
      rows: [
        new TableRow({
          children: [
            new TableCell({
              borders: noBorders,
              width: { size: barWidth, type: WidthType.DXA },
              shading: { fill: d.color, type: ShadingType.CLEAR },
              margins: { top: 60, bottom: 60, left: 0, right: 0 },
              children: [new Paragraph({ children: [] })]
            }),
            ...(remainWidth > 0 ? [new TableCell({
              borders: noBorders,
              width: { size: remainWidth, type: WidthType.DXA },
              shading: { fill: "F5F5F5", type: ShadingType.CLEAR },
              margins: { top: 60, bottom: 60, left: 0, right: 0 },
              children: [new Paragraph({ children: [] })]
            })] : [])
          ]
        })
      ]
    })
  ];

  return new TableRow({
    children: [
      new TableCell({
        borders, width: { size: 1500, type: WidthType.DXA },
        margins: { top: 100, bottom: 100, left: 120, right: 120 },
        children: [new Paragraph({ children: [new TextRun({ text: d.name, bold: true, font: "Arial", size: 20 })] })]
      }),
      new TableCell({
        borders, width: { size: 1000, type: WidthType.DXA },
        margins: { top: 100, bottom: 100, left: 120, right: 120 },
        children: [new Paragraph({ alignment: AlignmentType.CENTER, children: [new TextRun({ text: String(d.commits), font: "Arial", size: 20 })] })]
      }),
      new TableCell({
        borders, width: { size: 1000, type: WidthType.DXA },
        margins: { top: 100, bottom: 100, left: 120, right: 120 },
        children: [new Paragraph({ alignment: AlignmentType.CENTER, children: [new TextRun({ text: `${pct}%`, font: "Arial", size: 20, color: "555555" })] })]
      }),
      new TableCell({
        borders, width: { size: BAR_MAX_DXA + 500, type: WidthType.DXA },
        margins: { top: 100, bottom: 100, left: 120, right: 120 },
        children: barCellChildren
      }),
    ]
  });
});

// Alias rows
const aliasRows = data.map(d => new TableRow({
  children: [
    new TableCell({
      borders: noBorders, width: { size: 1500, type: WidthType.DXA },
      margins: { top: 0, bottom: 60, left: 120, right: 120 },
      children: [new Paragraph({ children: [] })]
    }),
    new TableCell({
      columnSpan: 3,
      borders: noBorders, width: { size: BAR_MAX_DXA + 2500, type: WidthType.DXA },
      margins: { top: 0, bottom: 60, left: 120, right: 120 },
      children: [new Paragraph({ children: [new TextRun({ text: `Aliases: ${d.aliases}`, italics: true, color: "888888", font: "Arial", size: 16 })] })]
    }),
  ]
}));

// Interleave data rows with alias rows
const allRows = [headerRow];
data.forEach((_, i) => {
  allRows.push(dataRows[i]);
  allRows.push(aliasRows[i]);
});

const doc = new Document({
  sections: [{
    properties: {
      page: {
        size: { width: 12240, height: 15840 },
        margin: { top: 1440, right: 1440, bottom: 1440, left: 1440 }
      }
    },
    children: [
      new Paragraph({
        heading: HeadingLevel.HEADING_1,
        children: [new TextRun({ text: "Commit Report — acceptance branch", font: "Arial", size: 36, bold: true, color: "2E4057" })]
      }),
      new Paragraph({
        children: [new TextRun({ text: `Total commits: ${total}  |  Contributors: ${data.length}`, font: "Arial", size: 20, color: "555555" })]
      }),
      new Paragraph({ children: [] }),
      new Table({
        width: { size: 9000, type: WidthType.DXA },
        columnWidths: [1500, 1000, 1000, BAR_MAX_DXA + 500],
        rows: allRows
      }),
      new Paragraph({ children: [] }),
      new Paragraph({
        children: [new TextRun({ text: `Generated on ${new Date().toLocaleDateString('en-GB', { day: '2-digit', month: 'long', year: 'numeric' })}`, font: "Arial", size: 16, color: "AAAAAA", italics: true })]
      }),
    ]
  }]
});

Packer.toBuffer(doc).then(buffer => {
  fs.writeFileSync("commit_report.docx", buffer);
  console.log("Done: commit_report.docx");
});
