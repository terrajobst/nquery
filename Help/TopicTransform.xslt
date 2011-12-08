<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet version="1.0"
				xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:MSHelp="http://msdn.microsoft.com/mshelp">

	<xsl:output method="html" />

	<xsl:variable name="iconPath">/icons/</xsl:variable>
	<xsl:variable name="scriptPath">/scripts/</xsl:variable>
	<xsl:variable name="stylePath">/styles/</xsl:variable>

	<xsl:template match="/topic">
		<xsl:if test="not (head/toc/@exclude or head/toc/@exclude = true)">
			<html xmlns:mshelp="http://msdn.microsoft.com/mshelp"
				  xmlns:ddue="http://ddue.schemas.microsoft.com/authoring/2003/5"
				  xmlns:xlink="http://www.w3.org/1999/xlink"
				  xmlns:msxsl="urn:schemas-microsoft-com:xslt">
				<head>
					<meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
					<meta name="save" content="history" />
					
					<title><xsl:value-of select="head/title"/></title>

					<link rel="stylesheet" type="text/css" href="{$stylePath}presentation.css" />
					<link rel="stylesheet" type="text/css" href="ms-help://Hx/HxRuntime/HxLink.css" />
					
					<script type="text/javascript" src="{$scriptPath}EventUtilities.js"></script>
					<script type="text/javascript" src="{$scriptPath}SplitScreen.js"></script>
					<script type="text/javascript" src="{$scriptPath}Dropdown.js"></script>
					<script type="text/javascript" src="{$scriptPath}script_manifold.js"></script>
					<script type="text/javascript" src="{$scriptPath}script_feedBack.js"> </script>
					<script type="text/javascript" src="{$scriptPath}CheckboxMenu.js"> </script>
					<script type="text/javascript" src="{$scriptPath}CommonUtilities.js"> </script>

					<xsl:call-template name="xmlIsland">
						<xsl:with-param name="keywords" select="head/keywords" />
					</xsl:call-template>
				</head>
				<body>

					<input type="hidden" id="userDataCache" class="userDataStyle" />
					<input type="hidden" id="hiddenScrollOffset" />
					<img id="collapseImage" style="display: none; height: 0; width: 0;" src="{$iconPath}collapse_all.gif" title="Collapse image" />
					<img id="expandImage" style="display: none; height: 0; width: 0;" src="{$iconPath}expand_all.gif" title="Expand Image" />
					<img id="collapseAllImage" style="display: none; height: 0; width: 0;" src="{$iconPath}collapse_all.gif" />
					<img id="expandAllImage" style="display: none; height: 0; width: 0;" src="{$iconPath}expand_all.gif" />
					<img id="dropDownImage" style="display: none; height: 0; width: 0;" src="{$iconPath}dropdown.gif" />
					<img id="dropDownHoverImage" style="display: none; height: 0; width: 0;" src="{$iconPath}dropdownHover.gif" />
					<img id="copyImage" style="display: none; height: 0; width: 0;" src="{$iconPath}copycode.gif" title="Copy image" />
					<img id="copyHoverImage" style="display: none; height: 0; width: 0;" src="{$iconPath}copycodeHighlight.gif" title="CopyHover image" />
					<div id="header">
						<table id="topTable" cellspacing="0" cellpadding="0">
							<tr>
								<td>
									<span onclick="ExpandCollapseAll(toggleAllImage)" style="cursor: default;" onkeypress="ExpandCollapseAll_CheckKey(toggleAllImage, event)" tabindex="0" xml:space="preserve">
										<img id="toggleAllImage" class="toggleAll" src="{$iconPath}collapse_all.gif" />
										<label id="collapseAllLabel" for="toggleAllImage" style="display: none;">Collapse All</label>
										<label id="expandAllLabel" for="toggleAllImage" style="display: none;">Expand All</label>
									</span>
								</td>
							</tr>
						</table>
						<table id="bottomTable" cellpadding="0" cellspacing="0">
							<tr id="headerTableRow1">
								<td align="left">
									<span id="runningHeaderText">NQuery Documentation</span>
								</td>
							</tr>
							<tr id="headerTableRow2">
								<td align="left">
									<span id="nsrTitle"><xsl:value-of select="head/title"/></span>
								</td>
							</tr>
							<tr id="headerTableRow3">
								<td align="left">
									<xsl:for-each select="head/links/link" xml:space="preserve">
										<a href="{@href}"><xsl:value-of select="."/></a>										
									</xsl:for-each>
								</td>
							</tr>
						</table>
						<table id="gradientTable">
							<tr>
								<td class="nsrBottom" background="{$iconPath}gradient.gif" />
							</tr>
						</table>
					</div>
					<div id="mainSection">
						<div id="mainBody">
							<div id="allHistory" class="saveHistory" onsave="saveAll()" onload="loadAll()">
								<span style="color: DarkGray"></span>
							</div>

							<xsl:apply-templates select="body/summary"/>
							<xsl:apply-templates select="body/syntax"/>
							<xsl:apply-templates select="body/parameters"/>
							<xsl:apply-templates select="body/returns"/>
							<xsl:apply-templates select="body/section" />
							<xsl:apply-templates select="body/remarks"/>
							<xsl:apply-templates select="body/examples"/>
						</div>

						<div id="footer">
							<div class="footerLine">
								<img width="100%" height="3px" src="{$iconPath}footer.gif" title="Footer image" />
							</div>
							<a name="feedback"></a><span id="fb" class="feedbackcss" style="display: none;"></span>
							<p />
							<xsl:text disable-output-escaping="yes"><![CDATA[<@CopyrightInfo>]]></xsl:text>
						</div>
					</div>
				</body>
			</html>
		</xsl:if>
		<xsl:comment xml:space="preserve">@SortOrder <xsl:value-of select="head/toc/@index" /> </xsl:comment>
		<xsl:if test="head/toc/@default">
			<xsl:comment xml:space="preserve">@DefaultTopic </xsl:comment>
		</xsl:if>
	</xsl:template>

	<xsl:template name="xmlIsland">
		<xsl:param name="keywords" />
		<xml>
			<xsl:for-each select="$keywords">
				<xsl:variable name="keyword" select="@term" />
				<xsl:choose>
					<xsl:when test="contains($keyword,',')">
						<xsl:variable name="parent" select="normalize-space(substring-before($keyword,','))" />
						<xsl:variable name="child" select="concat($parent, ',', normalize-space(substring-after($keyword,',')))" />
						<meta name="MS-HKWD" content="{$parent}" />
						<meta name="MS-HKWD" content="{$child}" />
					</xsl:when>
					<xsl:otherwise>
						<meta name="MS-HKWD" content="{$keyword}" />
					</xsl:otherwise>
				</xsl:choose>
			</xsl:for-each>
			<MSHelp:Attr Name="DevLang" Value="CSharp" />
			<MSHelp:Attr Name="Locale" Value="en-us" />
			<MSHelp:Attr Name="TopicType" Value="kbSyntax" />
			<MSHelp:Attr Name="TopicType" Value="apiref" />
			<MSHelp:Attr Name="DocSet" Value="NQuery" />
		</xml>
	</xsl:template>

	<xsl:template match="summary">
		<div class="summary">
			<xsl:apply-templates/>
		</div>
	</xsl:template>

	<xsl:template match="syntax | parameters | returns | remarks | examples">
		<xsl:variable name="sectionName" select="name()" />
		<xsl:variable name="sectionTitle">
			<xsl:choose>
				<xsl:when test="name() = 'syntax'">
					<xsl:text>Syntax</xsl:text>
				</xsl:when>
				<xsl:when test="name() = 'parameters'">
					<xsl:text>Parameters</xsl:text>
				</xsl:when>
				<xsl:when test="name() = 'returns'">
					<xsl:text>Returns</xsl:text>
				</xsl:when>
				<xsl:when test="name() = 'remarks'">
					<xsl:text>Remarks</xsl:text>
				</xsl:when>
				<xsl:when test="name() = 'examples'">
					<xsl:text>Examples</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="name()"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:call-template name="emitSection">
			<xsl:with-param name="sectionName" select="$sectionName" />
			<xsl:with-param name="sectionTitle" select="$sectionTitle" />
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="section">
		<xsl:call-template name="emitSection">
			<xsl:with-param name="sectionName" select="generate-id(.)" />
			<xsl:with-param name="sectionTitle" select="@title" />
		</xsl:call-template>
	</xsl:template>

	<xsl:template name="emitSection">
		<xsl:param name="sectionName" />
		<xsl:param name="sectionTitle" />

		<h1 class="heading">
			<span onclick="ExpandCollapse({$sectionName}Toggle)" style="cursor: default;" onkeypress="ExpandCollapse_CheckKey({$sectionName}Toggle, event)" tabindex="0">
				<img id="{$sectionName}Toggle" onload="OnLoadImage(event)" class="toggle" name="toggleSwitch" src="{$iconPath}collapse_all.gif"/>
				<xsl:value-of select="$sectionTitle"/>
			</span>
		</h1>
		<div id="{$sectionName}Section" class="section" name="collapseableSection" style="">
			<xsl:apply-templates/>
		</div>
	</xsl:template>

	<xsl:template match="note">
		<div class="alert">
			<table width="100%" cellspacing="0" cellpadding="0">
				<tr>
					<th align="left">
						<img class="note" alt="Note" src="{$iconPath}alert_note.gif" />Note
					</th>
				</tr>
				<tr>
					<td>
						<xsl:apply-templates />
					</td>
				</tr>
			</table>
		</div>
	</xsl:template>

	<xsl:template match="code">
		<div id="syntaxCodeBlocks" class="code">
			<table>
				<tr>
					<td style="border-top: silver 1px solid; border-bottom: silver 1px solid; border-right-style: none; border-left-style: none; background-color: whitesmoke;">
						<pre xml:space="preserve"><xsl:apply-templates /></pre>
					</td>
				</tr>
			</table>
		</div>
	</xsl:template>

	<xsl:template match="sampleCode">
		<xsl:choose>
			<xsl:when test="* or normalize-space()">
				<div id="syntaxCodeBlocks" class="code">
					<table>
						<tr>
							<td style="border-top: silver 1px solid; border-bottom: silver 1px solid; border-right-style: none; border-left-style: none; background-color: whitesmoke;">
								<pre xml:space="preserve"><xsl:apply-templates /></pre>
							</td>
						</tr>
					</table>
				</div>
			</xsl:when>
			<xsl:otherwise>
				<code>
					<xsl:copy-of select="@*" />
				</code>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="params">
		<xsl:for-each select="param">
			<dl>
				<dt>
					<span class="parameter">
						<xsl:value-of select="@name"/>
					</span>
				</dt>
				<dd>
					<xsl:apply-templates select="child::*" />
				</dd>
			</dl>
		</xsl:for-each>
	</xsl:template>

	<xsl:template match="c">
		<span xml:space="preserve" class="code"><xsl:value-of select="."/></span>
	</xsl:template>

	<!-- pass through special tags -->

	<xsl:template match="see">
		<xsl:copy>
			<xsl:copy-of select="@*" />
			<xsl:apply-templates />
		</xsl:copy>
	</xsl:template>

	<!-- pass through html tags -->

	<xsl:template match="p|ol|ul|li|dl|dt|dd|table|tr|th|td|a|img|b|i|strong|em|del|sub|sup|br|hr|h1|h2|h3|h4|h5|h6|pre|div|span|blockquote|abbr|acronym">
		<xsl:copy>
			<xsl:copy-of select="@*" />
			<xsl:apply-templates />
		</xsl:copy>
	</xsl:template>

</xsl:stylesheet>
