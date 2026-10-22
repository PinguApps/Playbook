<!-- snippet:transform -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="/"><route><xsl:value-of select="dispatch/destination" /></route></xsl:template>
</xsl:stylesheet>
<!-- endsnippet:transform -->
