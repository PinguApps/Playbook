SET NOCOUNT ON;
SELECT CONVERT(nvarchar(32), SERVERPROPERTY('ProductVersion')) AS engine_version,
       CONVERT(nvarchar(64), SERVERPROPERTY('Edition')) AS edition,
       CONVERT(int, SERVERPROPERTY('IsFullTextInstalled')) AS full_text_installed,
       CONVERT(nvarchar(64), DATABASEPROPERTYEX(DB_NAME(), 'Collation')) AS collation;
IF CONVERT(int, SERVERPROPERTY('ProductMajorVersion')) <> 17
    THROW 51000, 'Expected SQL Server 2025', 1;
IF FULLTEXTSERVICEPROPERTY('IsFullTextInstalled') <> 1
    THROW 51000, 'Full-text is missing', 1;
IF HAS_PERMS_BY_NAME(NULL, 'SERVER', 'CREATE ANY DATABASE') <> 0
    THROW 51000, 'Test login must not create databases', 1;
IF HAS_DBACCESS('master') <> 1
    THROW 51000, 'Expected normal public master access', 1;
GO
-- A real lower compatibility database, not an older-engine claim.
DECLARE @sql nvarchar(max) = N'ALTER DATABASE ' + QUOTENAME(DB_NAME()) + N' SET COMPATIBILITY_LEVEL = 150;';
EXEC (@sql);
GO
IF (SELECT compatibility_level FROM sys.databases WHERE name = DB_NAME()) <> 150
    THROW 51000, 'Lower compatibility was not applied', 1;
IF JSON_PATH_EXISTS(N'{"reef":{"name":null}}', '$.reef.name') <> 1
    THROW 51000, 'JSON path existence failed', 1;
DECLARE @sql nvarchar(max) = N'ALTER DATABASE ' + QUOTENAME(DB_NAME()) + N' SET COMPATIBILITY_LEVEL = 170;';
EXEC (@sql);
GO
ALTER DATABASE SCOPED CONFIGURATION SET PREVIEW_FEATURES = ON;
CREATE TABLE dbo.ReefNotes (
    Id int NOT NULL CONSTRAINT PK_ReefNotes PRIMARY KEY CLUSTERED,
    Body nvarchar(400) NOT NULL,
    Document json NOT NULL,
    Embedding vector(3) NOT NULL
);
INSERT dbo.ReefNotes VALUES
    (1, N'Coral reef observations', '{"habitat":"reef","species":["coral","wrasse"]}', '[1,0,0]'),
    (2, N'Kelp forest observations', '{"habitat":"kelp","species":["kelp"]}', '[0,1,0]');
IF (SELECT COUNT(*) FROM dbo.ReefNotes WHERE JSON_CONTAINS(Document, 'coral', '$.species[*]') = 1) <> 1
    THROW 51000, 'Native JSON containment failed', 1;
CREATE JSON INDEX IX_ReefNotes_Document ON dbo.ReefNotes(Document);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.ReefNotes') AND type_desc = 'JSON')
    THROW 51000, 'JSON index is missing', 1;
IF (SELECT TOP (1) Id FROM dbo.ReefNotes ORDER BY VECTOR_DISTANCE('cosine', Embedding, CAST('[1,0,0]' AS vector(3)))) <> 1
    THROW 51000, 'Exact vector search failed', 1;
GO
-- More than 100 rows also accommodates builds with a minimum index training set.
INSERT dbo.ReefNotes
SELECT value + 2, N'Coral reef sample', '{"habitat":"reef"}', '[0.9,0.1,0]'
FROM GENERATE_SERIES(1, 150);
CREATE VECTOR INDEX IX_ReefNotes_Vector ON dbo.ReefNotes(Embedding) WITH (METRIC = 'cosine', TYPE = 'DiskANN');
GO
DECLARE @query vector(3) = '[1,0,0]';
SELECT TOP (5) notes.Id, matches.distance
FROM VECTOR_SEARCH(TABLE = dbo.ReefNotes AS notes, COLUMN = Embedding,
    SIMILAR_TO = @query, METRIC = 'cosine', TOP_N = 5) AS matches
ORDER BY matches.distance;
IF @@ROWCOUNT <> 5
    THROW 51000, 'Indexed approximate vector search did not return five rows', 1;
GO
CREATE FULLTEXT CATALOG ReefCatalogue AS DEFAULT;
CREATE FULLTEXT INDEX ON dbo.ReefNotes(Body LANGUAGE 1033)
    KEY INDEX PK_ReefNotes WITH CHANGE_TRACKING AUTO;
GO
DECLARE @attempt int = 0;
WHILE NOT EXISTS (SELECT 1 FROM CONTAINSTABLE(dbo.ReefNotes, Body, N'coral')) AND @attempt < 30
BEGIN
    WAITFOR DELAY '00:00:01';
    SET @attempt += 1;
END;
IF NOT EXISTS (SELECT 1 FROM CONTAINSTABLE(dbo.ReefNotes, Body, N'coral'))
    THROW 51000, 'Ranked full-text search returned no rows', 1;
SELECT name, compatibility_level FROM sys.databases WHERE name = DB_NAME();
SELECT 'PASS: lower compatibility, JSON path/containment/index, exact/indexed vector and ranked full-text queries' AS result;
