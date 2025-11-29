import React, { useEffect, useState } from 'react';
import * as XLSX from 'xlsx';

interface ExcelViewerProps {
  fileUrl: string;
  filePath?: string;
  onPrint?: () => void;
}

export const ExcelViewer: React.FC<ExcelViewerProps> = ({ fileUrl, filePath, onPrint }) => {
  const [htmlTable, setHtmlTable] = useState<string>('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const handleOpenInExcel = () => {
    if (filePath) {
      // Create a custom protocol link that Windows will handle
      window.location.href = `ms-excel:ofv|u|file:///${filePath.replace(/\\/g, '/')}`;
    }
  };

  useEffect(() => {
    const loadExcelFile = async () => {
      try {
        setLoading(true);
        setError(null);

        // Fetch the Excel file
        const response = await fetch(fileUrl);
        if (!response.ok) {
          throw new Error('Failed to load Excel file');
        }

        const arrayBuffer = await response.arrayBuffer();
        const workbook = XLSX.read(arrayBuffer, { type: 'array', cellStyles: true });

        // Get the first sheet
        const firstSheetName = workbook.SheetNames[0];
        const worksheet = workbook.Sheets[firstSheetName];

        // Convert to HTML with cell styling
        const html = XLSX.utils.sheet_to_html(worksheet, { 
          id: 'excel-table',
          editable: false,
          header: '',
          footer: ''
        });
        
        // Parse and enhance the HTML to include cell colors
        const parser = new DOMParser();
        const doc = parser.parseFromString(html, 'text/html');
        const table = doc.getElementById('excel-table');
        
        if (table) {
          // Get cell range
          const range = XLSX.utils.decode_range(worksheet['!ref'] || 'A1');
          
          // Apply cell styles
          for (let R = range.s.r; R <= range.e.r; ++R) {
            for (let C = range.s.c; C <= range.e.c; ++C) {
              const cellAddress = XLSX.utils.encode_cell({ r: R, c: C });
              const cell = worksheet[cellAddress];
              
              if (cell && cell.s) {
                const tdElement = table.querySelector(`tr:nth-child(${R + 1}) td:nth-child(${C + 1})`) as HTMLElement;
                if (tdElement) {
                  // Apply background color
                  if (cell.s.fgColor && cell.s.fgColor.rgb) {
                    const rgb = cell.s.fgColor.rgb;
                    tdElement.style.backgroundColor = `#${rgb}`;
                  } else if (cell.s.bgColor && cell.s.bgColor.rgb) {
                    const rgb = cell.s.bgColor.rgb;
                    tdElement.style.backgroundColor = `#${rgb}`;
                  }
                  
                  // Apply font styles
                  if (cell.s.font) {
                    if (cell.s.font.bold) tdElement.style.fontWeight = 'bold';
                    if (cell.s.font.italic) tdElement.style.fontStyle = 'italic';
                    if (cell.s.font.color && cell.s.font.color.rgb) {
                      tdElement.style.color = `#${cell.s.font.color.rgb}`;
                    }
                  }
                  
                  // Apply alignment
                  if (cell.s.alignment) {
                    if (cell.s.alignment.horizontal) {
                      tdElement.style.textAlign = cell.s.alignment.horizontal;
                    }
                    if (cell.s.alignment.vertical) {
                      tdElement.style.verticalAlign = cell.s.alignment.vertical;
                    }
                  }
                }
              }
            }
          }
        }
        
        setHtmlTable(table ? table.outerHTML : html);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load file');
      } finally {
        setLoading(false);
      }
    };

    loadExcelFile();
  }, [fileUrl]);

  const handlePrint = () => {
    if (onPrint) {
      onPrint();
    } else {
      window.print();
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center p-8">
        <div className="text-gray-600">Loading Excel file...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex items-center justify-center p-8">
        <div className="text-red-600">Error: {error}</div>
      </div>
    );
  }

  return (
    <div className="excel-viewer">
      <div className="mb-4 flex justify-between items-center print:hidden">
        <div>
          {filePath && (
            <button
              onClick={handleOpenInExcel}
              className="px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700 flex items-center gap-2"
              title="Open in Microsoft Excel"
            >
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14" />
              </svg>
              Open in Excel
            </button>
          )}
        </div>
        <button
          onClick={handlePrint}
          className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 flex items-center gap-2"
        >
          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 17h2a2 2 0 002-2v-4a2 2 0 00-2-2H5a2 2 0 00-2 2v4a2 2 0 002 2h2m2 4h6a2 2 0 002-2v-4a2 2 0 00-2-2H9a2 2 0 00-2 2v4a2 2 0 002 2zm8-12V5a2 2 0 00-2-2H9a2 2 0 00-2 2v4h10z" />
          </svg>
          Print
        </button>
      </div>
      <div 
        className="overflow-auto bg-white shadow-lg"
        style={{ maxHeight: '85vh' }}
        dangerouslySetInnerHTML={{ __html: htmlTable }}
      />
      <style>{`
        #excel-table {
          border-collapse: collapse;
          width: auto;
          font-size: 11pt;
          font-family: 'Calibri', 'Segoe UI', 'Arial', sans-serif;
          background-color: white;
          margin: 0;
        }
        #excel-table td, #excel-table th {
          border: 1px solid #c0c0c0;
          padding: 2px 6px;
          text-align: left;
          color: #000;
          background-color: #fff;
          white-space: nowrap;
          line-height: 1.4;
        }
        #excel-table td {
          vertical-align: middle;
          min-height: 20px;
        }
        #excel-table th {
          background-color: #e7e6e6;
          color: #000;
          font-weight: normal;
          text-align: center;
          padding: 2px 4px;
        }
        #excel-table tr {
          height: auto;
        }
        /* Preserve merged cells and wide columns */
        #excel-table td[colspan] {
          text-align: center;
        }
        @media print {
          .print\\:hidden {
            display: none !important;
          }
          #excel-table {
            font-size: 9pt;
          }
          #excel-table td, #excel-table th {
            padding: 1px 4px;
          }
        }
      `}</style>
    </div>
  );
};
