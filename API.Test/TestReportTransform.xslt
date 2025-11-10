<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl">
    <xsl:output method="html" encoding="utf-8" indent="yes"/>

    <xsl:template match="/">
        <xsl:text disable-output-escaping="yes">&lt;!DOCTYPE html&gt;</xsl:text>
        <html lang="en">
            <head>
                <meta charset="UTF-8"/>
                <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
                <title>Test Results Report</title>
                <style>
                    * {
                        margin: 0;
                        padding: 0;
                        box-sizing: border-box;
                    }

                    body {
                        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                        min-height: 100vh;
                        padding: 20px;
                    }

                    .container {
                        max-width: 1200px;
                        margin: 0 auto;
                        background: white;
                        border-radius: 8px;
                        box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
                        overflow: hidden;
                    }

                    header {
                        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                        color: white;
                        padding: 40px 20px;
                        text-align: center;
                    }

                    header h1 {
                        font-size: 28px;
                        margin-bottom: 10px;
                    }

                    header p {
                        font-size: 14px;
                        opacity: 0.9;
                    }

                    .controls {
                        padding: 20px;
                        background: #f5f5f5;
                        display: flex;
                        justify-content: space-between;
                        align-items: center;
                        flex-wrap: wrap;
                        gap: 15px;
                        border-bottom: 1px solid #ddd;
                    }

                    .filter-section {
                        display: flex;
                        align-items: center;
                        gap: 10px;
                    }

                    .filter-section label {
                        font-weight: 600;
                        color: #333;
                    }

                    .filter-dropdown {
                        padding: 8px 12px;
                        border: 2px solid #667eea;
                        border-radius: 4px;
                        background: white;
                        color: #333;
                        font-size: 14px;
                        cursor: pointer;
                        transition: all 0.3s ease;
                    }

                    .filter-dropdown:hover {
                        background: #667eea;
                        color: white;
                    }

                    .summary {
                        padding: 20px;
                        display: grid;
                        grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
                        gap: 15px;
                        background: white;
                    }

                    .summary-item {
                        padding: 15px;
                        border-radius: 4px;
                        text-align: center;
                        border-left: 4px solid #667eea;
                    }

                    .summary-item.passed {
                        border-left-color: #4caf50;
                        background: #f1f8f4;
                    }

                    .summary-item.failed {
                        border-left-color: #f44336;
                        background: #fdf5f5;
                    }

                    .summary-item.skipped {
                        border-left-color: #ff9800;
                        background: #fffaf0;
                    }

                    .summary-item h3 {
                        font-size: 24px;
                        color: #333;
                        margin-bottom: 5px;
                    }

                    .summary-item p {
                        font-size: 12px;
                        color: #666;
                        text-transform: uppercase;
                    }

                    .results-section {
                        padding: 20px;
                    }

                    .results-table {
                        width: 100%;
                        border-collapse: collapse;
                    }

                    .results-table thead {
                        background: #f5f5f5;
                        border-bottom: 2px solid #667eea;
                    }

                    .results-table th {
                        padding: 12px;
                        text-align: left;
                        font-weight: 600;
                        color: #333;
                    }

                    .results-table tbody tr {
                        border-bottom: 1px solid #eee;
                        transition: background 0.3s ease;
                    }

                    .results-table tbody tr:hover {
                        background: #f9f9f9;
                    }

                    .results-table tbody tr.hidden {
                        display: none;
                    }

                    .results-table td {
                        padding: 12px;
                    }

                    .test-name {
                        font-weight: 500;
                        color: #333;
                    }

                    .outcome {
                        display: inline-block;
                        padding: 4px 8px;
                        border-radius: 4px;
                        font-size: 12px;
                        font-weight: 600;
                        text-transform: uppercase;
                    }

                    .outcome.passed {
                        background: #e8f5e9;
                        color: #2e7d32;
                    }

                    .outcome.failed {
                        background: #ffebee;
                        color: #c62828;
                    }

                    .outcome.skipped {
                        background: #fff3e0;
                        color: #e65100;
                    }

                    .duration {
                        color: #666;
                        font-size: 12px;
                    }

                    .error-message {
                        color: #f44336;
                        font-size: 12px;
                        padding: 5px 0;
                        max-width: 300px;
                        word-wrap: break-word;
                    }

                    footer {
                        background: #f5f5f5;
                        padding: 20px;
                        text-align: center;
                        border-top: 1px solid #ddd;
                        font-size: 12px;
                        color: #666;
                    }

                    .no-results {
                        text-align: center;
                        padding: 40px;
                        color: #999;
                        font-size: 16px;
                    }

                    @media (max-width: 768px) {
                        .controls {
                            flex-direction: column;
                            align-items: flex-start;
                        }

                        .summary {
                            grid-template-columns: 1fr;
                        }

                        .results-table {
                            font-size: 12px;
                        }

                        .results-table td {
                            padding: 8px;
                        }

                        header h1 {
                            font-size: 20px;
                        }
                    }
                </style>
            </head>
            <body>
                <div class="container">
                    <header>
                        <h1>✓ Test Results Report</h1>
                        <p>Automated Test Execution Summary</p>
                    </header>

                    <div class="controls">
                        <div class="filter-section">
                            <label for="outcomeFilter">Filter by Outcome:</label>
                            <select id="outcomeFilter" class="filter-dropdown">
                                <option value="all">All Results</option>
                                <option value="passed">Passed Only</option>
                                <option value="failed">Failed Only</option>
                                <option value="skipped">Skipped Only</option>
                            </select>
                        </div>
                    </div>

                    <div class="summary">
                        <div class="summary-item">
                            <h3>
                                <xsl:value-of select="count(//UnitTestResult)"/>
                            </h3>
                            <p>Total Tests</p>
                        </div>
                        <div class="summary-item passed">
                            <h3>
                                <xsl:value-of select="count(//UnitTestResult[@outcome='Passed'])"/>
                            </h3>
                            <p>Passed</p>
                        </div>
                        <div class="summary-item failed">
                            <h3>
                                <xsl:value-of select="count(//UnitTestResult[@outcome='Failed'])"/>
                            </h3>
                            <p>Failed</p>
                        </div>
                        <div class="summary-item skipped">
                            <h3>
                                <xsl:value-of select="count(//UnitTestResult[@outcome='NotExecuted']) + count(//UnitTestResult[@outcome='Inconclusive'])"/>
                            </h3>
                            <p>Skipped</p>
                        </div>
                    </div>

                    <div class="results-section">
                        <table class="results-table">
                            <thead>
                                <tr>
                                    <th>Test Name</th>
                                    <th>Outcome</th>
                                    <th>Duration</th>
                                    <th>Details</th>
                                </tr>
                            </thead>
                            <tbody id="resultsBody">
                                <xsl:for-each select="//UnitTestResult">
                                    <xsl:variable name="outcome">
                                        <xsl:choose>
                                            <xsl:when test="@outcome='Passed'">Passed</xsl:when>
                                            <xsl:when test="@outcome='Failed'">Failed</xsl:when>
                                            <xsl:otherwise>Skipped</xsl:otherwise>
                                        </xsl:choose>
                                    </xsl:variable>
                                    <tr data-outcome="{translate($outcome, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')}">
                                        <td class="test-name">
                                            <xsl:value-of select="@testName"/>
                                        </td>
                                        <td>
                                            <span class="outcome {translate($outcome, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')}">
                                                <xsl:value-of select="$outcome"/>
                                            </span>
                                        </td>
                                        <td class="duration">
                                            <xsl:value-of select="@duration"/>
                                        </td>
                                        <td>
                                            <xsl:if test="Output/ErrorInfo/Message">
                                                <div class="error-message">
                                                    <xsl:value-of select="Output/ErrorInfo/Message"/>
                                                </div>
                                            </xsl:if>
                                        </td>
                                    </tr>
                                </xsl:for-each>
                            </tbody>
                        </table>
                    </div>

                    <footer>
                        <p>Test report generated on <span id="generatedTime">
                            <xsl:value-of select="/TestRun/@runUser"/> - <xsl:value-of select="/TestRun/@name"/>
                        </span></p>
                        <p>For more information about the test results, please review the detailed logs.</p>
                    </footer>
                </div>

                <script>
                    function applyFilter(filterValue) {
                        const rows = document.querySelectorAll('#resultsBody tr');
                        let visibleCount = 0;

                        rows.forEach(row => {
                            const outcome = row.dataset.outcome;

                            if (filterValue === 'all' || outcome === filterValue) {
                                row.classList.remove('hidden');
                                visibleCount++;
                            } else {
                                row.classList.add('hidden');
                            }
                        });

                        const noResults = document.getElementById('noResults');
                        if (noResults) {
                            noResults.style.display = visibleCount === 0 ? 'block' : 'none';
                        }
                    }

                    document.addEventListener('DOMContentLoaded', function() {
                        const filterDropdown = document.getElementById('outcomeFilter');
                        if (filterDropdown) {
                            filterDropdown.addEventListener('change', (e) => {
                                applyFilter(e.target.value);
                            });
                        }
                    });
                </script>
            </body>
        </html>
    </xsl:template>
</xsl:stylesheet>
