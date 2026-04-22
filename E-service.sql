CREATE DATABASE Projekti

USE Projekti

CREATE TABLE Userat (
	UserID INT IDENTITY(1,1) PRIMARY KEY,
	Username NVARCHAR(50) UNIQUE NOT NULL,
	Password VARBINARY(64) NOT NULL,
	Salt VARBINARY(16) NOT NULL,
	PhoneNumber NVARCHAR(15) UNIQUE NOT NULL,
	ContractNumber NVARCHAR(20) UNIQUE NOT NULL,
	Email NVARCHAR(255) UNIQUE NOT NULL,
	CreatedAT DATETIME DEFAULT GETDATE(),
	UpdatedAT DATETIME DEFAULT GETDATE(),
	GoogleID NVARCHAR(100) UNIQUE NULL
);

CREATE TABLE FailedLogins (
	AttemptID INT IDENTITY(1,1) PRIMARY KEY,
	Username NVARCHAR(50) NOT NULL,
	AttemptTime DATETIME DEFAULT GETDATE(),
	IPAddress NVARCHAR(45) NULL
);

CREATE TABLE LoginLogs (
    LogID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    LoginTime DATETIME DEFAULT GETDATE(),
    IPAddress NVARCHAR(50),
    FOREIGN KEY (UserID) REFERENCES Userat(UserID)
);

CREATE TABLE PasswordChangeLogs (
    LogID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    ChangeTime DATETIME DEFAULT GETDATE(),
    IPAddress NVARCHAR(50),
    FOREIGN KEY (UserID) REFERENCES Userat(UserID)
);

CREATE INDEX IX_LoginLogs_UserID_LoginTime ON LoginLogs (UserID, LoginTime);
CREATE INDEX IX_PasswordChangeLogs_UserID_ChangeTime ON PasswordChangeLogs (UserID, ChangeTime);

-- Modifikimi i tabeles LoginLogs per te mundsuar fshirjen
ALTER TABLE LoginLogs
ADD CONSTRAINT FK_LoginLogs_UserID
FOREIGN KEY (UserID) REFERENCES Userat(UserID) ON DELETE CASCADE;

-- Modifikimi i tabeles PasswordChangeLogs per te mundsuar fshirjen
ALTER TABLE PasswordChangeLogs
ADD CONSTRAINT FK_PasswordChangeLogs_UserID
FOREIGN KEY (UserID) REFERENCES Userat(UserID) ON DELETE CASCADE;

-- Modifikimi i tabeles Userat 
ALTER TABLE Userat
DROP CONSTRAINT UQ__Userat__A6FBF31BD7F3A39A;

-- Krijimi i UNIQUE Per te lejuar null te shumefishte 
CREATE UNIQUE NONCLUSTERED INDEX IX_Userat_GoogleID
ON Userat (GoogleID)
WHERE GoogleID IS NOT NULL;


/* Deklarimi dhe insertimi i userave
   Selektimi i tabelave
*/


-- Deklarimi i variablave dhe shtimi i passwordit gjithashtu edhe hashimi i tij.
DECLARE @Salt VARBINARY(16) = CAST(NEWID() AS VARBINARY(16));
DECLARE @Password NVARCHAR(50) = 'diamanti';
DECLARE @HashedPassword VARBINARY(64) = HASHBYTES('SHA2_256', @Password + CAST(@Salt AS NVARCHAR(32)));

-- Shtimi i nje useri te ri permes databases
INSERT INTO Userat (Username, Password, Salt, PhoneNumber, ContractNumber, Email, GoogleID)
VALUES ('Diamant', @HashedPassword, @Salt, '045-102-482', 'RE-01740/22', 'diamant2.gashi@universitetiaab.com', 'NULL');

-- Thirrja e userave me emrin testues
SELECT * FROM Userat WHERE Username = 'testues';

-- Thirrja e te gjithe userave
SELECT * FROM Userat;

SELECT * FROM Lendet;


-- Fshirja e userit qe eshte autentikuar me google per ta testuar se si do te reagoj forma ne c# .net 
DELETE FROM Userat
WHERE UserID = 1002;

-- Thirrja e tentuesve qe kan provuar te hyjne pa pasur llogari ose kan deshtuar



-- Thirrja apo selektimi i thjesht i tabeles LoginLogs
SELECT * FROM LoginLogs;

SELECT
	l.LogID,
	l.UserID,
	u.Username,
	l.LoginTime,
	l.IPAddress
FROM LoginLogs AS l
JOIN Userat AS u
ON l.UserID = u.UserID;

-- Thirrja me e avancuar e login logs duke shtuar edhe emrin(username)
 
-- Thirrja e thjesht per testim 
SELECT * FROM PasswordChangeLogs;

-- Thirrja me e avancuar per te pare username
SELECT 
	PCL.LogID,
	PCL.UserID,
	U.Username,
	PCL.ChangeTime,
	PCL.IPAddress
FROM PasswordChangeLogs AS PCL
JOIN Userat AS U
ON PCL.UserID = U.UserID;

-- Krijimi i Roleve 

CREATE TABLE Roles (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    CanRead BIT NOT NULL DEFAULT 1,
    CanWrite BIT NOT NULL DEFAULT 0,
    CanDelete BIT NOT NULL DEFAULT 0,
    CanManageUsers BIT NOT NULL DEFAULT 0
);

-- Insertimi dhe vendosja e qasjeve nga pozita 
INSERT INTO Roles (RoleName, CanRead, CanWrite, CanDelete, CanManageUsers)
VALUES 
    ('Student', 1, 0, 0, 0), -- Vetëm lexon
    ('Professor', 1, 1, 0, 0), -- Lexon dhe shkruan
    ('Admin', 1, 1, 1, 1); -- Ka qasje të plotë


SELECT * FROM Roles;

-- Modifikimi i tabales Userat per ta shtuar si foreign key Roles

ALTER TABLE Userat
ADD RoleID INT;

SELECT * FROM Userat;

ALTER TABLE Userat
ALTER COLUMN RoleID INT NOT NULL; 

ALTER TABLE Userat
ADD CONSTRAINT FK_Userat_Roles_RoleID
FOREIGN KEY (RoleID) REFERENCES Roles(RoleID);

SELECT * FROM Userat;


SELECT *
FROM Userat AS U 
RIGHT JOIN Roles AS r 
ON U.RoleID = r.RoleID;

-- Shtimi i picture per tabelen userat
ALTER TABLE Userat
ADD Photo VARBINARY(MAX) NULL;

-- Shtimi i nje picture per nje user(student)
UPDATE Userat
SET Photo = (SELECT BulkColumn FROM OPENROWSET(BULK N'C:\Users\yqafl\OneDrive\Desktop\Joni.jpg', SINGLE_BLOB) AS Image)
WHERE UserID = 1017;

-- Ndryshimi i role id per nje user(testim)
UPDATE Userat
SET RoleID = 3
WHERE UserID = 2;

SELECT * FROM Userat;
SELECT * FROM Roles;

SELECT * FROM FailedLogins;
SELECT * FROM LoginLogs;

SELECT 
	L.LogID,
	L.UserID,
	U.Username,
	L.LoginTime,
	L.IPAddress
FROM LoginLogs AS L
LEFT JOIN Userat AS U 
ON L.UserID = U.UserID

SELECT * FROM Userat;


-- Shtojme kolonen PersonalNumber si Varbinary64 qe pastaj me bo hash
ALTER TABLE Userat
ADD PersonalNumber VARBINARY(64) NULL;

ALTER TABLE Userat
ADD Salt_PersonalNumber VARBINARY(16) NULL;

SELECT * FROM Userat; 


-- Shto personal number ne kolonat e userave
DECLARE @UserID INT = 2017; -- UserID i përdoruesit ekzistues
DECLARE @NewPersonalNumber NVARCHAR(50) = '5264412121'; -- Numri personal i ri

-- Gjenerimi i salt dhe hash
DECLARE @SaltPN VARBINARY(16) = CAST(NEWID() AS VARBINARY(16));
DECLARE @HashedPN VARBINARY(64) = HASHBYTES('SHA2_256', @NewPersonalNumber + CAST(@SaltPN AS NVARCHAR(32)));

-- Përditësimi i të dhënave
UPDATE Userat
SET 
    PersonalNumber = @HashedPN,
    Salt_PersonalNumber = @SaltPN
WHERE 
    UserID = @UserID;


CREATE TABLE BlockedIPs (
    IPAddress NVARCHAR(45) PRIMARY KEY,
    BlockTime DATETIME NOT NULL,
    ExpirationTime DATETIME NOT NULL
);


-- Insertimi i nje useri te ri 

-- Parametrat nga jashtë (nga aplikacioni yt ose nga skripta jote)
DECLARE @Password NVARCHAR(50) = 'Valdeti';
DECLARE @NewPersonalNumber NVARCHAR(50) = '20233885558';
DECLARE @BackupCode NVARCHAR(6) = '815274'; -- Ky vjen nga jashtë

-- Hashimi për Password
DECLARE @Salt VARBINARY(16) = CAST(NEWID() AS VARBINARY(16));
DECLARE @HashedPassword VARBINARY(64) = HASHBYTES('SHA2_256', @Password + CAST(@Salt AS NVARCHAR(32)));

-- Hashimi për PersonalNumber
DECLARE @SaltPN VARBINARY(16) = CAST(NEWID() AS VARBINARY(16));
DECLARE @HashedPN VARBINARY(64) = HASHBYTES('SHA2_256', @NewPersonalNumber + CAST(@SaltPN AS NVARCHAR(32)));

-- Ngarkimi i fotos nga path-i lokal
DECLARE @Photo VARBINARY(MAX);
SELECT @Photo = BulkColumn
FROM OPENROWSET(BULK N'C:\Users\yqafl\OneDrive\Desktop\Joni.jpg', SINGLE_BLOB) AS Image;

-- INSERT në tabelën Userat
INSERT INTO Userat (
    Username,
    Password,
    Salt,
    PhoneNumber,
    ContractNumber,
    Email,
    RoleID,
    Photo,
    BackupCode,
    PersonalNumber,
    Salt_PersonalNumber
)
VALUES (
    'Joni Mjeku',
    @HashedPassword,
    @Salt,
    '045-000-000',
    'RE-14321/25',
    'joni.mjeku@universitetiaab.com',
    2,
    @Photo,
    @BackupCode,
    @HashedPN,
    @SaltPN
);

-- Update per usera te google nese eshte e nevojshme...

-- Deklaro variablat për të dhënat e reja
DECLARE @UserID INT = 2038; -- Zëvendëso me UserID e përdoruesit që do të përditësosh
DECLARE @Password NVARCHAR(50) = 'NewPassword123'; -- Fjalëkalimi i ri
DECLARE @PhoneNumber NVARCHAR(20) = '045123456'; -- Numri i telefonit i ri
DECLARE @ContractNumber NVARCHAR(20) = 'RE-12345/25'; -- Numri i kontratës (i gjeneruar nga skripti yt në Go)
DECLARE @BackupCode NVARCHAR(6) = '123456'; -- Backup Code (i gjeneruar nga skripti yt në Go)
DECLARE @PersonalNumber NVARCHAR(50) = '1234567890'; -- Numri personal i ri
DECLARE @Photo VARBINARY(MAX) = NULL; -- Fotoja (NULL për momentin, mund të shtohet më vonë)

-- Gjenero SALT për Password
DECLARE @Salt VARBINARY(16) = CAST(NEWID() AS VARBINARY(16));
DECLARE @HashedPassword VARBINARY(64);

-- Hash-o Password me SHA-256 dhe SALT
IF @Password IS NOT NULL
BEGIN
    SET @HashedPassword = HASHBYTES('SHA2_256', @Password + CAST(@Salt AS NVARCHAR(32)));
END
ELSE
BEGIN
    SET @HashedPassword = NULL;
    SET @Salt = NULL;
END

-- Gjenero SALT për PersonalNumber
DECLARE @SaltPN VARBINARY(16) = CAST(NEWID() AS VARBINARY(16));
DECLARE @HashedPN VARBINARY(64);

-- Hash-o PersonalNumber me SHA-256 dhe SALT
IF @PersonalNumber IS NOT NULL
BEGIN
    SET @HashedPN = HASHBYTES('SHA2_256', @PersonalNumber + CAST(@SaltPN AS NVARCHAR(32)));
END
ELSE
BEGIN
    SET @HashedPN = NULL;
    SET @SaltPN = NULL;
END

-- Përditëso përdoruesin në tabelën Userat
UPDATE Userat
SET Password = @HashedPassword,
    Salt = @Salt,
    PhoneNumber = @PhoneNumber,
    ContractNumber = @ContractNumber,
    BackupCode = @BackupCode,
    PersonalNumber = @HashedPN,
    Salt_PersonalNumber = @SaltPN,
    Photo = @Photo
WHERE UserID = @UserID;

-- Shfaq rezultatin për verifikim
SELECT * FROM Userat WHERE UserID = 2038;


-- Selektimi i kolonave dhe manipulimi

SELECT * FROM Userat;
SELECT * FROM FailedLogins;
SELECT * FROM LoginLogs;
SELECT * FROM Roles;
SELECT * FROM PasswordChangeLogs;
SELECT * FROM BlockedIPs;

SELECT 
	l.LogID,
	l.UserID,
	u.Username,
	l.LoginTime,
	l.IPAddress
FROM Userat AS U
JOIN LoginLogs AS L
ON U.UserID = L.UserID;

UPDATE Userat
SET ContractNumber = 'RE-95376/25'
WHERE UserID = 1003;

DELETE FROM FailedLogins;

DELETE FROM BlockedIPs
WHERE IPAddress = '192.168.178.27';

DELETE FROM BlockedIPs
WHERE IPAddress = '192.168.100.215';

DELETE FROM Userat
WHERE UserID = 2038;


/* BIG UPDATE 
	Tabelat e tjera po shtohen pasi qe projekti eshte duke u zgjeruar...
	Tabelat si Orari, Financat Lendet, Drejtimet, Nendrejtimet,
	Grupet, Materialet, Shkarkimet e materialeve etj.
*/

-- Tabela për Drejtimet
CREATE TABLE Drejtimet (
    DrejtimID INT IDENTITY(1,1) PRIMARY KEY,
    EmriDrejtimit NVARCHAR(100) NOT NULL UNIQUE
);

-- Tabela për Nëndrejtimet
CREATE TABLE Nendrejtimet (
    NendrejtimID INT IDENTITY(1,1) PRIMARY KEY,
    DrejtimID INT NOT NULL,
    EmriNendrejtimit NVARCHAR(100) NOT NULL UNIQUE,
    FOREIGN KEY (DrejtimID) REFERENCES Drejtimet(DrejtimID)
);

-- Shto kolona për Drejtim dhe Nëndrejtim në tabelën Userat
ALTER TABLE Userat
ADD DrejtimID INT NULL,
    NendrejtimID INT NULL;

ALTER TABLE Userat
ADD CONSTRAINT FK_Userat_Drejtimet FOREIGN KEY (DrejtimID) REFERENCES Drejtimet(DrejtimID),
    CONSTRAINT FK_Userat_Nendrejtimet FOREIGN KEY (NendrejtimID) REFERENCES Nendrejtimet(NendrejtimID);

-- Shto disa drejtime dhe nëndrejtime si shembull
INSERT INTO Drejtimet (EmriDrejtimit) VALUES ('Shkenca Kompjuterike');
INSERT INTO Drejtimet (EmriDrejtimit) VALUES ('Inxhinieri Elektrike');

DECLARE @ShkencaKompjuterikeID INT = (SELECT DrejtimID FROM Drejtimet WHERE EmriDrejtimit = 'Shkenca Kompjuterike');
DECLARE @InxhinieriElektrikeID INT = (SELECT DrejtimID FROM Drejtimet WHERE EmriDrejtimit = 'Inxhinieri Elektrike');

INSERT INTO Nendrejtimet (DrejtimID, EmriNendrejtimit) VALUES (@ShkencaKompjuterikeID, 'Inxhinieri Softuerike');
INSERT INTO Nendrejtimet (DrejtimID, EmriNendrejtimit) VALUES (@ShkencaKompjuterikeID, 'Siguri Kibernetike');
INSERT INTO Nendrejtimet (DrejtimID, EmriNendrejtimit) VALUES (@InxhinieriElektrikeID, 'Elektronikë');

-- Shtimi i grupeve dhe orareve

-- Tabela për Grupet
CREATE TABLE Grupet (
    GrupID INT IDENTITY(1,1) PRIMARY KEY,
    EmriGrupit NVARCHAR(50) NOT NULL UNIQUE -- p.sh., Grupi 1, Grupi 2, Grupi 3 (pa shkëputje)
);

-- Tabela për Oraret
CREATE TABLE Oraret (
    OrarID INT IDENTITY(1,1) PRIMARY KEY,
    GrupID INT NOT NULL,
    Dita NVARCHAR(20) NOT NULL, -- p.sh., E Hënë
    KohaFillimit TIME NOT NULL, -- p.sh., 09:00
    KohaMbarimit TIME NOT NULL, -- p.sh., 10:30
    Lloji NVARCHAR(50) NOT NULL, -- p.sh., Ligjëratë, Ushtrime
    FOREIGN KEY (GrupID) REFERENCES Grupet(GrupID)
);

-- Shto kolonën GrupID në tabelën Userat
ALTER TABLE Userat
ADD GrupID INT NULL;

ALTER TABLE Userat
ADD CONSTRAINT FK_Userat_Grupet FOREIGN KEY (GrupID) REFERENCES Grupet(GrupID);

-- Shto disa grupe dhe orare si shembull
INSERT INTO Grupet (EmriGrupit) VALUES ('Grupi 1');
INSERT INTO Grupet (EmriGrupit) VALUES ('Grupi 2');
INSERT INTO Grupet (EmriGrupit) VALUES ('Grupi 3 (pa shkëputje)');

DECLARE @Grup1ID INT = (SELECT GrupID FROM Grupet WHERE EmriGrupit = 'Grupi 1');
DECLARE @Grup2ID INT = (SELECT GrupID FROM Grupet WHERE EmriGrupit = 'Grupi 2');
DECLARE @Grup3ID INT = (SELECT GrupID FROM Grupet WHERE EmriGrupit = 'Grupi 3 (pa shkëputje)');

-- Oraret për Grupin 1 (E Hënë)
INSERT INTO Oraret (GrupID, Dita, KohaFillimit, KohaMbarimit, Lloji)
VALUES (@Grup1ID, 'E Premte', '09:00', '10:30', 'Ligjëratë'),
       (@Grup1ID, 'E Premte', '11:00', '12:30', 'Ushtrime');

-- Oraret për Grupin 2 (E Hënë)
INSERT INTO Oraret (GrupID, Dita, KohaFillimit, KohaMbarimit, Lloji)
VALUES (@Grup2ID, 'E Premte', '13:00', '14:30', 'Ligjëratë'),
       (@Grup2ID, 'E Premte', '14:45', '16:15', 'Ushtrime');

-- Oraret për Grupin 3 (E Hënë)
INSERT INTO Oraret (GrupID, Dita, KohaFillimit, KohaMbarimit, Lloji)
VALUES (@Grup3ID, 'E Premte', '16:30', '18:00', 'Ligjëratë'),
       (@Grup3ID, 'E Premte', '18:15', '19:45', 'Ushtrime'); 


-- Tabela për Lëndët
CREATE TABLE Lendet (
    LendeID INT IDENTITY(1,1) PRIMARY KEY,
    DrejtimID INT NOT NULL,
    NendrejtimID INT NOT NULL,
    EmriLendes NVARCHAR(100) NOT NULL,
    Viti INT NOT NULL, -- p.sh., Viti 1, Viti 2
    FOREIGN KEY (DrejtimID) REFERENCES Drejtimet(DrejtimID),
    FOREIGN KEY (NendrejtimID) REFERENCES Nendrejtimet(NendrejtimID)
);

-- Deklaro të gjitha variablat e nevojshme
DECLARE @ShkencaKompjuterikeID INT = (SELECT DrejtimID FROM Drejtimet WHERE EmriDrejtimit = 'Shkenca Kompjuterike');
DECLARE @InxhinieriSoftuerikeID INT = (SELECT NendrejtimID FROM Nendrejtimet WHERE EmriNendrejtimit = 'Inxhinieri Softuerike');

INSERT INTO Lendet (DrejtimID, NendrejtimID, EmriLendes, Viti)
VALUES (@ShkencaKompjuterikeID, @InxhinieriSoftuerikeID, 'Programim 1', 1),
       (@ShkencaKompjuterikeID, @InxhinieriSoftuerikeID, 'Baza e të Dhënave', 2);

-- Verifiko që lëndët janë shtuar
SELECT * FROM Lendet;

-- Tabela për Materialet
CREATE TABLE Materialet (
    MaterialID INT IDENTITY(1,1) PRIMARY KEY,
    LendeID INT NOT NULL,
    ProfesoriID INT NOT NULL, -- Profesori që e ka postuar
    DrejtimID INT NOT NULL,
    NendrejtimID INT NOT NULL,
    Viti INT NOT NULL,
    Titulli NVARCHAR(200) NOT NULL,
    Pershkrimi NVARCHAR(500) NULL,
    FileData VARBINARY(MAX) NOT NULL, -- Skedari (PDF, Word, etj.)
    FileName NVARCHAR(100) NOT NULL,
    DataPostimit DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (LendeID) REFERENCES Lendet(LendeID),
    FOREIGN KEY (ProfesoriID) REFERENCES Userat(UserID),
    FOREIGN KEY (DrejtimID) REFERENCES Drejtimet(DrejtimID),
    FOREIGN KEY (NendrejtimID) REFERENCES Nendrejtimet(NendrejtimID)
);

-- Tabela për Shkarkimet e Materialeve
CREATE TABLE ShkarkimetMaterialeve (
    ShkarkimID INT IDENTITY(1,1) PRIMARY KEY,
    MaterialID INT NOT NULL,
    StudentiID INT NOT NULL,
    DataShkarkimit DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MaterialID) REFERENCES Materialet(MaterialID),
    FOREIGN KEY (StudentiID) REFERENCES Userat(UserID)
);

-- Tabela për Financat
CREATE TABLE Financat (
    FinancaID INT IDENTITY(1,1) PRIMARY KEY,
    StudentiID INT NOT NULL,
    Shuma DECIMAL(10, 2) NOT NULL,
    Pershkrimi NVARCHAR(200) NULL,
    DataPageses DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (StudentiID) REFERENCES Userat(UserID)
);

-- Tabela për Provimet
CREATE TABLE Provimet (
    ProvimID INT IDENTITY(1,1) PRIMARY KEY,
    LendeID INT NOT NULL,
    StudentiID INT NOT NULL,
    ProfesoriID INT NOT NULL,
    DataProvimit DATETIME NOT NULL,
    Piket INT NOT NULL, -- Piket e provimit (p.sh., 0-100)
    FOREIGN KEY (LendeID) REFERENCES Lendet(LendeID),
    FOREIGN KEY (StudentiID) REFERENCES Userat(UserID),
    FOREIGN KEY (ProfesoriID) REFERENCES Userat(UserID)
);

-- Tabela për Rezultatet (p.sh., detyra, aktivitete)
CREATE TABLE Rezultatet (
    RezultatID INT IDENTITY(1,1) PRIMARY KEY,
    LendeID INT NOT NULL,
    StudentiID INT NOT NULL,
    ProfesoriID INT NOT NULL,
    Piket INT NOT NULL, -- Piket e rezultatit (p.sh., 0-100)
    Pershkrimi NVARCHAR(200) NULL,
    DataRegjistrimit DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (LendeID) REFERENCES Lendet(LendeID),
    FOREIGN KEY (StudentiID) REFERENCES Userat(UserID),
    FOREIGN KEY (ProfesoriID) REFERENCES Userat(UserID)
);

-- Tabela për Notën Përfundimtare
CREATE TABLE NotatPerfundimtare (
    NotaID INT IDENTITY(1,1) PRIMARY KEY,
    LendeID INT NOT NULL,
    StudentiID INT NOT NULL,
    NotaPerfundimtare INT NOT NULL, -- p.sh., 5-10
    DataLlogaritjes DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (LendeID) REFERENCES Lendet(LendeID),
    FOREIGN KEY (StudentiID) REFERENCES Userat(UserID)
);

ALTER TABLE Userat
ADD Viti INT NULL;


SELECT * FROM Oraret;
SELECT * FROM ShkarkimetMaterialeve;
SELECT * FROM Drejtimet;
SELECT * FROM Nendrejtimet;
SELECT * FROM Grupet;
SELECT * FROM Userat;
SELECT * FROM Roles;
SELECT * FROM Materialet;

SELECT * FROM Financat;



INSERT INTO Financat(StudentiID, Shuma, Pershkrimi, DataPageses)
VALUES
(2014, 1410.00, 'Pagesë për tërë vitin', '10-15-2022');



-- TESTIM 1^
DECLARE @LendeID INT = (SELECT LendeID FROM Lendet WHERE EmriLendes = 'Programim 1');
DECLARE @ProfesoriID INT = 2014; -- Zëvendëso me UserID e një profesori
DECLARE @DrejtimID INT = (SELECT DrejtimID FROM Drejtimet WHERE EmriDrejtimit = 'Shkenca Kompjuterike');
DECLARE @NendrejtimID INT = (SELECT NendrejtimID FROM Nendrejtimet WHERE EmriNendrejtimit = 'Inxhinieri Softuerike');

INSERT INTO Materialet (LendeID, ProfesoriID, DrejtimID, NendrejtimID, Viti, Titulli, Pershkrimi, FileData, FileName)
VALUES (@LendeID, @ProfesoriID, @DrejtimID, @NendrejtimID, 1, 'Ligjëratë 1', 'Material për ligjëratën e parë', 0x0, 'E-Service.pdf'),
       (@LendeID, @ProfesoriID, @DrejtimID, @NendrejtimID, 1, 'Detyrë 1', 'Detyrë për javën e parë', 0x0, 'E-Service.pdf');

DELETE FROM FailedLogins;
DELETE FROM BlockedIPs;
SELECT * FROM Userat;

SELECT * FROM Materialet;
SELECT * FROM Financat;
SELECT * FROM Financat WHERE UserID = 1003;

DECLARE @LendeID INT = 28; -- LendeID për "Programim 1"
DECLARE @ProfessorID INT = 2042; -- ProfesorID për ""

-- Shto materialet që mungojnë (Ligji 2, Ligji 3, Ligji 4)
INSERT INTO Materialet (LendeID, ProfesoriID, DrejtimID, NendrejtimID, Viti, Titulli, Pershkrimi, FileName, FileData, DataPostimit)
VALUES 
    (@LendeID, @ProfessorID, 1, 1, 1, 'Ligji 2', 'Material për ligjëratën e dytë', 'C:\Users\yqafl\OneDrive\Desktop\Ligjerata - 1.pdf', CONVERT(VARBINARY(MAX), 0x), GETDATE()),
    (@LendeID, @ProfessorID, 1, 1, 1, 'Ligji 3', 'Material për ligjëratën e tretë', 'C:\Users\yqafl\OneDrive\Desktop\Ligjerata - 4.pdf', CONVERT(VARBINARY(MAX), 0x), GETDATE()),
    (@LendeID, @ProfessorID, 1, 1, 1, 'Ligji 4', 'Material për ligjëratën e katërt', 'C:\Users\yqafl\OneDrive\Desktop\Ligjerata - 4.pdf', CONVERT(VARBINARY(MAX), 0x), GETDATE());


SELECT * FROM Materialet;
SELECT * FROM ShkarkimetMaterialeve;



UPDATE Materialet
SET FileData = (SELECT * FROM OPENROWSET(BULK 'C:/Users/yqafl/OneDrive/Desktop/Ligji2.pdf', SINGLE_BLOB) AS FileData)
WHERE MaterialID = 1;

SELECT * FROM ShkarkimetMaterialeve;

-- Selekto te gjitha studentat qe kan shkarkuar materialet...
SELECT * 
FROM Userat as U
JOIN ShkarkimetMaterialeve AS SM 
ON U.UserID = SM.StudentiID;


-- Shtimi i kolones 'Data e lindjes' pasi qe me duhet ta therras ne forme. 
ALTER TABLE Userat
ADD DataLindjes DATE;

-- Shtimi i ditlindjes se yllit <3/.
UPDATE Userat
SET DataLindjes = '2001-03-16'
WHERE UserID = 1003;



INSERT INTO Drejtimet(EmriDrejtimit)
VALUES
('Administratë Publike'),
('Arkitekturë'),
('Arte'),
('Ekonomik'),
('Gjuhë të Huaja'),
('Juridik'),
('Kulturë Fizike dhe Sport'),
('Komunikim Masiv'),
('Shkenca Sociale'),
('Psikologji'),
('Shkenca Shëndetsore'),
('Stomatologji');

SELECT * FROM Drejtimet;
SELECT * FROM Lendet;
SELECT * FROM Userat;
SELECT * FROM Nendrejtimet;

UPDATE Userat
SET Viti = 3
WHERE UserID = 1003;

INSERT INTO Nendrejtimet(DrejtimID, EmriNendrejtimit)
VALUES
(1, 'Programimi për lojëra Kompjuterike')



ALTER TABLE Lendet
ADD Semestri INT;

SELECT * FROM Lendet;

-- Supozojmë që ke disa lëndë në tabelën Lendet
UPDATE Lendet
SET Semestri = 3
WHERE LendeID = 2;

UPDATE Lendet
SET Semestri = 1
WHERE LendeID IN (1, 3, 4, 5, 6); -- Lëndët e Semestrit 1

UPDATE Lendet
SET Semestri = 2
WHERE LendeID IN (7, 8, 9, 10, 11); -- Lëndët e Semestrit 2


UPDATE Userat
SET Semestri = 2
WHERE UserID = 1003;


SELECT * FROM Materialet;
SELECT * FROM Lendet;
SELECT * FROM Userat;
SELECT * FROM Roles;

SELECT * FROM Materialet WHERE LendeID = 1;


UPDATE Userat
SET Viti = 1
WHERE UserID = 1003;


UPDATE Userat
SET Semestri = 6
WHERE UserID = 1003;

UPDATE Userat
SET Viti = 3
WHERE UserID = 1003;

SELECT * FROM Materialet;

UPDATE Userat
SET Username = 'Ylli Qafleshi'
WHERE UserID = 1003;

SELECT * FROM Lendet WHERE Viti = 3;
SELECT * FROM Oraret;

INSERT INTO Lendet(DrejtimID, NendrejtimID, EmriLendes, Viti, Semestri)
VALUES
(1,1, 'Siguria e të dhënave', 3, 6),
(1,1, 'Tema e Diplomës', 3, 6),
(1,1, 'Programimi i Grafikës', 3, 6),
(1,1, 'Sistemet e avancuara të bazës së dhënave', 3, 6);

SELECT * FROM Oraret;


SELECT * FROM Financat;

INSERT INTO Financat(StudentiID, Shuma, Pershkrimi, DataPageses)
VALUES
(1003, 400.00, 'Pagesë Cash', '2022-09-11');


UPDATE Financat 
SET DataPageses = '2023-09-11'
WHERE FinancaID = 12;

SELECT * FROM Userat;

UPDATE Userat
SET NendrejtimID = 1
WHERE UserID = 2039;

SELECT * FROM Lendet;

UPDATE Userat
SET Viti = 3 , Semestri = 5
WHERE UserID = 1003;

SELECT * FROM Rezultatet;
SELECT * FROM Provimet;

-- Shtimi i kolones ECTS
ALTER TABLE Lendet
ADD ECTS INT NULL;

-- Shtimi i foreign key 
ALTER TABLE Lendet
ADD ProfesoriID INT NULL,
FOREIGN KEY (ProfesoriID) REFERENCES Userat(UserID);

SELECT * FROM Userat;

UPDATE Lendet
SET ECTS = 8
WHERE LendeID = 28;

SELECT * FROM Lendet;
SELECT * FROM Userat;

-- Insertimet e userave

-- Parametrat nga jashtë (të dhënat e përdoruesit)
DECLARE @Password NVARCHAR(50) = 'Valdete123'; -- Fjalëkalimi i ri për Ilir Keka
DECLARE @PersonalNumber NVARCHAR(50) = '8661546329'; -- Numri personal (opsional)
DECLARE @Photo VARBINARY(MAX);

-- Ngarkimi i fotos nga një rrugë specifike
SELECT @Photo = BulkColumn
FROM OPENROWSET(BULK N'C:\Users\yqafl\OneDrive\Desktop\Valdete.jpg', SINGLE_BLOB) AS Image;

-- Hashimi për Password
DECLARE @Salt VARBINARY(16) = CAST(NEWID() AS VARBINARY(16));
DECLARE @HashedPassword VARBINARY(64) = HASHBYTES('SHA2_256', @Password + CAST(@Salt AS NVARCHAR(32)));

-- Hashimi për PersonalNumber (opsional)
DECLARE @SaltPN VARBINARY(16) = CAST(NEWID() AS VARBINARY(16));
DECLARE @HashedPN VARBINARY(64) = HASHBYTES('SHA2_256', @PersonalNumber + CAST(@SaltPN AS NVARCHAR(32)));

-- INSERT në tabelën Userat
INSERT INTO Userat (
    Username,
    Password,
    Salt,
    PhoneNumber,
    ContractNumber,
    Email,
    RoleID,
    Photo,
    BackupCode,
    PersonalNumber,
    Salt_PersonalNumber
)
VALUES (
    'Valdete Daku',
    @HashedPassword,
    @Salt,
    '045-999-888',
    'RE-17110/25',
    'Valdete.Daku@universitetiaab.com',
    2, -- RoleID = 2 (Profesor)
    @Photo,
    '424082',
    @HashedPN,
    @SaltPN
);


SELECT * FROM Userat;
SELECT * FROM Lendet;
SELECT * FROM NotatPerfundimtare;





-- Insertimi i profesorave te lendet

DECLARE @ProfesoriID INT = 2042;

-- Përditëso tabelën duke përdorur variablin
UPDATE Lendet
SET ProfesoriID = @ProfesoriID
WHERE LendeID = 19;

SELECT * FROM Oraret;

SELECT * FROM Lendet;

UPDATE Userat
SET Viti = 3
WHERE UserID = 1003;

UPDATE Userat
SET Semestri = 6
WHERE UserID = 1003;

SELECT * FROM Rezultatet;

INSERT INTO Rezultatet(LendeID, StudentiID, ProfesoriID, Piket, Pershkrimi)
VALUES
(1, 1003, 2044, 30, 'Kolofium 1');





-- Ndryshime ne tabelat Oraret 

ALTER TABLE Oraret
ADD Salla NVARCHAR(50) NULL;

ALTER TABLE Oraret
ADD LendeID INT NULL;

ALTER TABLE Oraret
ADD CONSTRAINT FK_Oraret_Lendet FOREIGN KEY (LendeID) REFERENCES Lendet(LendeID);

-- Përditëso rekordet ekzistuese me Salla dhe LendeID
UPDATE Oraret
SET Salla = 'Salla 101', LendeID = 1 -- Programim 1
WHERE OrarID = 1; -- Grupi 1, E Premte, 09:00 - 10:30, Ligjëratë

UPDATE Oraret
SET Salla = 'Salla 102', LendeID = 1 -- Programim 1
WHERE OrarID = 2; -- Grupi 1, E Premte, 11:00 - 12:30, Ushtrime

UPDATE Oraret
SET Salla = 'Salla 103', LendeID = 2 -- Baza e të Dhënave
WHERE OrarID = 3; -- Grupi 2, E Premte, 13:00 - 14:30, Ligjëratë

UPDATE Oraret
SET Salla = 'Salla 104', LendeID = 2 -- Baza e të Dhënave
WHERE OrarID = 4; -- Grupi 2, E Premte, 14:45 - 16:15, Ushtrime

UPDATE Oraret
SET Salla = 'Salla 105', LendeID = 1 -- Programim 1
WHERE OrarID = 5; -- Grupi 3, E Premte, 16:30 - 18:00, Ligjëratë

UPDATE Oraret
SET Salla = 'Salla 106', LendeID = 1 -- Programim 1
WHERE OrarID = 6; -- Grupi 3, E Premte, 18:15 - 19:45, Ushtrime

UPDATE Userat
SET Semestri = 1
WHERE UserID = 1003;

SELECT * FROM Lendet;

SELECT * FROM Oraret;

SELECT * FROM Nendrejtimet;

SELECT *
FROM Userat AS U
JOIN Lendet AS L
ON U.UserID = L.ProfesoriID
WHERE EmriLendes = 'Programim 1'

SELECT * FROM Userat;

UPDATE Userat
SET GrupID = 3
WHERE UserID = 1013;

SELECT * FROM Financat;

Update Userat
SET GrupID = 3
WHERE UserID = 2017;




INSERT INTO Financat(StudentiID, Shuma, Pershkrimi, DataPageses)
VALUES
(1013, 4410, 'Djali e pagujti krejt fakulltetin', '2022-10-15');

INSERT INTO Financat(StudentiID, Shuma, Pershkrimi, DataPageses)
VALUES
(2017, 2500, 'Transferi me i qellum najher <3', '2023-01-19');

SELECT * FROM Oraret;

SELECT * FROM Grupet;
SELECT * FROM Lendet;

INSERT INTO Oraret(GrupID, Dita, KohaFillimit, KohaMbarimit, Lloji, Salla, LendeID)
VALUES
(1, 'E Hënë', '09:00', '10:30', 'Ligjeratë', 'A-105', 1),
(1, 'E Hënë', '11:00', '12:30', 'Ushtrime', 'LAB-01', 1),
(1, 'E Martë', '09:00', '10:30', 'Ligjeratë', 'A-105', 3),
(1, 'E Martë', '11:00', '12:30', 'Ushtrime', 'LAB-01', 3),
(1, 'E Merkure', '09:00', '10:30', 'Ligjeratë', 'A-105', 4),
(1, 'E Merkure', '11:00', '12:30', 'Ushtrime', 'LAB-03', 4),
(1, 'E Ejte', '09:00', '10:30', 'Ligjeratë', 'A-105', 5),
(1, 'E Ejte', '11:00', '12:30', 'Ushtrime', 'LAB-03', 5),
(1, 'E Premte', '09:00', '10:30', 'Ligjeratë', 'A-105', 6),
(1, 'E Premte', '11:00', '12:30', 'Ushtrime', 'LAB-04', 6),
(2, 'E Hënë', '13:00', '14:30', 'Ligjeratë', 'A-105', 1),
(2, 'E Hënë', '14:45', '16:15', 'Ushtrime', 'LAB-01', 1),
(2, 'E Martë', '13:00', '14:30', 'Ligjeratë', 'A-105', 3),
(2, 'E Martë', '14:45', '16:15', 'Ushtrime', 'LAB-01', 3),
(2, 'E Merkure', '13:00', '14:30', 'Ligjeratë', 'A-105', 4),
(2, 'E Merkure', '14:45', '16:15', 'Ushtrime', 'LAB-03', 4),
(2, 'E Ejte', '13:00', '14:30', 'Ligjeratë', 'A-105', 5),
(2, 'E Ejte', '14:45', '16:15', 'Ushtrime', 'LAB-04', 5),
(2, 'E Premte', '13:00', '14:30', 'Ligjeratë', 'A-105', 6),
(2, 'E Premte', '14:45', '16:15', 'Ushtrime', 'LAB-02', 6),
(3, 'E Hënë', '16:30', '18:00', 'Ligjeratë', 'A-105', 1),
(3, 'E Hënë', '18:15', '19:45', 'Ushtrime', 'LAB-01', 1),
(3, 'E Martë', '16:30', '18:00', 'Ligjeratë', 'A-105', 3),
(3, 'E Martë', '18:15', '19:45', 'Ushtrime', 'LAB-01', 3),
(3, 'E Merkure', '16:30', '18:00', 'Ligjeratë', 'A-105', 4),
(3, 'E Merkure', '18:15', '19:45', 'Ushtrime', 'LAB-03', 4),
(3, 'E Ejte', '16:30', '18:00', 'Ligjeratë', 'A-105', 5),
(3, 'E Ejte', '18:15', '19:45', 'Ushtrime', 'LAB-04', 5),
(3, 'E Premte', '16:30', '18:00', 'Ligjeratë', 'A-105', 6),
(3, 'E Premte', '18:15', '19:45', 'Ushtrime', 'LAB-02', 6);

SELECT * FROM Oraret;

UPDATE Userat
SET GrupID = 1
WHERE UserID = 1003;

SELECT * FROM Userat;

UPDATE Lendet
SET ProfesoriID = 2044
WHERE LendeID = 1;

UPDATE Userat
SET NendrejtimID = 1
WHERE UserID = 2044;

UPDATE Userat 
SET DrejtimID = 1, NendrejtimID = 1
WHERE UserID = 2052;

SELECT * FROM Userat;

SELECT * FROM Lendet;

SELECT * FROM LoginLogs;

SELECT * FROM Oraret;


SELECT * FROM Provimet;

INSERT INTO Provimet(LendeID, StudentiID, ProfesoriID, DataProvimit, Piket, Nota, Afati)
VALUES
(1, 1003, 2042, '2023-01-18', 91, 10, 'Janar');

DELETE FROM FailedLogins;
DELETE FROM BlockedIPs;


SELECT * FROM Materialet;

SELECT * FROM Lendet;

SELECT * FROM ShkarkimetMaterialeve;

UPDATE Userat
SET Viti = 3
WHERE UserID = 1003;

ALTER TABLE Materialet
ADD Tipi NVARCHAR(50);

SELECT * FROM Userat;

UPDATE Userat
SET NendrejtimID = 1
WHERE UserID = 2042;

SELECT * FROM Roles;

SELECT * FROM ShkarkimetMaterialeve;

SELECT * FROM Rezultatet;

SELECT 
	u.Username,
	r.Pershkrimi,
	r.Piket
FROM Userat AS U 
JOIN Rezultatet AS R 
ON StudentiID = U.UserID;

UPDATE Userat
SET Viti = 3
WHERE UserID = 1003;

SELECT * FROM Userat WHERE UserID = 1013;

SELECT * FROM Userat;

SELECT * FROM LoginLogs;

SELECT * FROM Drejtimet;

ALTER TABLE FailedLogins ADD IsSuspiciousIP BIT NOT NULL DEFAULT 0;
ALTER TABLE LoginLogs ADD IsSuspiciousIP BIT NOT NULL DEFAULT 0;

SELECT * FROM FailedLogins;

DELETE FROM BlockedIPs;

DELETE FROM FailedLogins;

SELECT * FROM BlockedIPs;
SELECT * FROM FailedLogins;

-- Shto foto 
UPDATE Userat
SET Photo = (SELECT BulkColumn FROM OPENROWSET(BULK N'C:\Users\yqafl\OneDrive\Desktop\Diamant.jpg', SINGLE_BLOB) AS Image)
WHERE UserID = 1013;

SELECT * FROM Userat;


-- Krijimi i tabeles Njoftimet per te mundesuar publikimin e profesoreve te njoftimeve per studente..

CREATE TABLE Njoftimet (
    NjoftimID INT IDENTITY(1,1) PRIMARY KEY,
    ProfesoriID INT NOT NULL,
    LendeID INT NOT NULL,
    Titulli NVARCHAR(200) NOT NULL,
    Permbajtja NVARCHAR(1000) NOT NULL,
    DataPublikimit DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (ProfesoriID) REFERENCES Userat(UserID),
    FOREIGN KEY (LendeID) REFERENCES Lendet(LendeID)
);

SELECT * FROM Njoftimet;
SELECT * FROM Lendet;

SELECT * FROM Userat;

SELECT * FROM Roles;

SELECT * FROM Financat;
SELECT * FROM Userat;

UPDATE Userat
SET RoleID = 3
WHERE UserID = 2039;


-- Shtimi i adminid per te shfaqur se cili admin i ka ruajtur financat.
ALTER TABLE Financat
ADD AdminID INT;

ALTER TABLE Financat
ADD CONSTRAINT FK_Financat_Admin FOREIGN KEY (AdminID) REFERENCES Userat(UserID);

DROP TABLE BlockedIPs;

-- Krijimi i nje tabele te re per te bllokuar paisjen dhe ip 
CREATE TABLE BlockedDevices (
    DeviceID NVARCHAR(100) NOT NULL,
    Username NVARCHAR(50) NOT NULL,
    IPAddress NVARCHAR(50) NULL, -- NULL për IP publike, plotësohet për IP private
    BlockTime DATETIME NOT NULL,
    ExpirationTime DATETIME NOT NULL,
    PRIMARY KEY (DeviceID, Username)
);

-- Modifikimi i tabelave per te shtuar deviceid
ALTER TABLE FailedLogins
ADD DeviceID NVARCHAR(100);

ALTER TABLE LoginLogs
ADD DeviceID NVARCHAR(100);

-- Krijimi i listes se bardhe per te lejuar ip publike si ajo e aab 
CREATE TABLE WhitelistedIPs (
    IPAddress NVARCHAR(50) NOT NULL,
    PRIMARY KEY (IPAddress)
);

-- Shtimi i IP's te kolegjit aab
INSERT INTO WhitelistedIPs (IPAddress) VALUES ('10.53.0.136');

SELECT * FROM FailedLogins;
SELECT * FROM LoginLogs;
SELECT * FROM BlockedIPs;

DELETE FROM FailedLogins;

SELECT * FROM Userat
WHERE RoleID = 1;

DELETE FROM FailedLogins;
DELETE FROM BlockedIps;

SELECT * FROM Userat;

SELECT * FROM BlockedDevices;
SELECT * FROM BlockedIPs;

SELECT * FROM FailedLogins;

SELECT * FROM Userat;


UPDATE Userat
SET Semestri = 1
WHERE UserID = 1

ALTER TABLE Userat
ADD Semestri INT NULL;

SELECT * FROM Provimet;

ALTER TABLE Provimet
ADD Nota INT NULL,
    Afati NVARCHAR(50) NULL;


INSERT INTO Lendet (DrejtimID, NendrejtimID, EmriLendes, Viti, Semestri)
VALUES
(1,1, 'Programimi per lojëra kompjuterike',3,5);

SELECT * FROM Lendet;

UPDATE Lendet
SET ECTS = 4
WHERE LendeID = 28;

SELECT * FROM Userat;

UPDATE Userat
SET Viti = 3
WHERE UserID = 1;



SELECT * FROM Userat;
SELECT * FROM Drejtimet;
SELECT * FROM Nendrejtimet;
SELECT * FROM Financat;
SELECT * FROM Grupet;
SELECT * FROM Oraret;
SELECT * FROM WhitelistedIPs;
SELECT * FROM Roles;



SELECT * FROM Userat;
SELECT * FROM Roles;




-- 1. Deklarimi i variablave kryesore
DECLARE @PlainPassword       NVARCHAR(100) = 'elonahaxhaj';          
DECLARE @Salt                VARBINARY(16) = CAST(NEWID() AS VARBINARY(16));
DECLARE @HashedPassword      VARBINARY(64) = HASHBYTES('SHA2_256', @PlainPassword + CAST(@Salt AS NVARCHAR(4000)));

-- 2. Numri personal (opsional por shumë i rëndësishëm për siguri)
DECLARE @PersonalNumberPlain NVARCHAR(50)  = '1221323432';         
DECLARE @SaltPN              VARBINARY(16) = NULL;
DECLARE @HashedPersonalNumber VARBINARY(64) = NULL;

IF @PersonalNumberPlain IS NOT NULL AND LEN(@PersonalNumberPlain) > 0
BEGIN
    SET @SaltPN = CAST(NEWID() AS VARBINARY(16));
    SET @HashedPersonalNumber = HASHBYTES('SHA2_256', @PersonalNumberPlain + CAST(@SaltPN AS NVARCHAR(4000)));
END


DECLARE @Photo VARBINARY(MAX) = NULL;


SELECT @Photo = BulkColumn 
FROM OPENROWSET(BULK N'C:\Users\yqafl\Downloads\elonahaxhaj-20260225-0001.jpg', SINGLE_BLOB) AS Foto;



-- 4. INSERT-i i plotë me të gjitha fushat kryesore
INSERT INTO Userat (
    Username,
    Password,
    Salt,
    PhoneNumber,
    ContractNumber,
    Email,
    GoogleID,                   
    RoleID,                     
    Photo,
    PersonalNumber,
    Salt_PersonalNumber,
    Viti,
    Semestri,
    GrupID,
    DrejtimID,
    NendrejtimID,
    DataLindjes,
    BackupCode                  -- opsionale për 2FA recovery
)
VALUES (
    'Elona.haxhaj',            -- Username (unik)
    @HashedPassword,
    @Salt,
    '045-091-149',              -- PhoneNumber (unik)
    'RE-53827/26',              -- ContractNumber (unik)
    'elona.haxhaj@universitetiaab.com',  -- Email (unik)
    NULL,                       -- GoogleID → NULL për llogari normale
    1,                          -- RoleID 1 - Student, 2 -Professor, 3 - Admin.
    @Photo,                     -- Foto (aktualisht NULL ose nga OPENROWSET)
    @HashedPersonalNumber,
    @SaltPN,
    1,                          -- Viti (p.sh. 2)
    1,                          -- Semestri (p.sh. 3)
    3,                          -- GrupID (shiko tabelën Grupet)
    1,                          -- DrejtimID (Shkenca Kompjuterike)
    1,                          -- NendrejtimID (Inxhinieri Softuerike)
    '2004-05-31',               -- DataLindjes (YYYY-MM-DD)
    '305946'                    -- BackupCode 6 shifra (opsionale)
);

SELECT * FROM Userat
WHERE RoleID = 1;



SELECT * FROM Lendet;

UPDATE Lendet
SET ProfesoriID = 30
WHERE LendeID = 27;

SELECT * FROM Userat;

UPDATE Userat
SET DataLindjes = '2001-03-16'
WHERE UserID = 2;

UPDATE Userat
SET Semestri = 1
WHERE UserID = 1;


-- Selektimi pa dublikim i rreshtave te kolones tipi.
SELECT DISTINCT Tipi FROM Materialet

-- Fshirja e rreshtit detyre pas inspektimit te nje gabimi...
DELETE FROM Materialet
WHERE tipi = 'Detyre';

SELECT * FROM BlockedDevices;

SELECT * FROM BlockedIPs;
SELECT * FROM WhitelistedIPs;
SELECT * FROM NotatPerfundimtare;

SELECT * FROM Oraret;
SELECT * FROM Userat;


UPDATE Userat
SET GrupID = 1
Where UserID = 1;



ALTER TABLE Provimet
ADD Statusi NVARCHAR(20) NULL;




DELETE FROM Provimet;
SELECT * FROM Provimet;

/* Pas testimit te applikacionit ne formen e studentit duhet 
modifikim i triggerit pasi qe nuk ndryshon statusin kur studenti e refuzon nje note */

-- Trigeri i ri per formen e profesorit ne menyre automatike

ALTER TRIGGER trg_SetStatusFromNota
ON Provimet
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE p
    SET Statusi = 
        CASE 
            WHEN i.Nota = 5 THEN 'Dështuar'
            WHEN i.Nota BETWEEN 6 AND 10 THEN 'Kaluar'
            ELSE NULL   -- ose 'Në Pritje' nëse dëshiron default tjetër
        END
    FROM Provimet p
    INNER JOIN inserted i ON p.ProvimID = i.ProvimID
    WHERE 
        -- Vepro vetëm kur Nota ka vlerë (për të shmangur probleme me NULL)
        i.Nota IS NOT NULL
        -- Dhe mos e prek nëse është 'Refuzuar' (siç e kishim bërë më parë)
        AND ISNULL(i.Statusi, '') <> 'Refuzuar';
END;

SELECT * FROM NotatPerfundimtare


/* Krijimi i nje procedure te ruajtur per ta perdorur per automatizimin 
e provimeve qe te vendosen direkt tek tabela = 'NotatPerfundimtare' 
nese nuk eshte refuzuar mbrenda 1 jave.
Pra cdo note qe eshte vendosur nga profesori dhe nuk eshte refuzuar 
nga studenti ajo vendoset automatikisht nga kjo procedure e klikuar nga 
nje skript e windowsit. */

CREATE OR ALTER PROCEDURE dbo.TransferoNotatNeFinale
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO NotatPerfundimtare 
        (
            LendeID,
            StudentiID,
            NotaPerfundimtare,
            DataLlogaritjes
        )
        SELECT 
            p.LendeID,
            p.StudentiID,
            p.Nota,
            GETDATE() AS DataLlogaritjes
        FROM Provimet p
        WHERE 
            p.Nota BETWEEN 5 AND 10
            AND ISNULL(p.Statusi, '') <> 'Refuzuar'
            AND p.DataProvimit <= DATEADD(DAY, -7, GETDATE())
            AND NOT EXISTS (
                SELECT 1 
                FROM NotatPerfundimtare np 
                WHERE np.LendeID = p.LendeID 
                  AND np.StudentiID = p.StudentiID
            );

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        
    END CATCH
END;
GO


-- Testim per te pare nese funksionon stored procedure
SELECT * FROM Userat;
-- Shto një provim test 10 ditë më parë me StudentID = 1
INSERT INTO Provimet 
(
    LendeID, 
    StudentiID, 
    ProfesoriID, 
    DataProvimit, 
    Piket, 
    Nota, 
    Afati, 
    Statusi
)
VALUES 
(
    1,                  -- LendeID 
    1,                  -- StudentiID 
    13,               -- ProfesoriID 
    DATEADD(DAY, -10, GETDATE()),  
    85,                 -- Pikët
    9,                  -- Nota 
    'Janar',            -- Afati
    'Kaluar'            -- Statusi (jo 'Refuzuar')
);


EXEC dbo.TransferoNotatNeFinale;

SELECT * FROM NotatPerfundimtare 
WHERE StudentiID = 1 
ORDER BY DataLlogaritjes DESC;

SELECT * FROM Oraret;

SELECT * FROM Userat;

-- Insertimi i orareve te reja


SELECT * FROM Provimet;

SELECT * FROM Lendet;

SELECT * FROM Materialet;



-- Fshirja e constraintit egzistues
ALTER TABLE ShkarkimetMaterialeve
DROP CONSTRAINT FK__Shkarkime__Mater__6FE99F9F;

-- Modifikimi per te fshire nje material ne kuader te professorit edhe nese eshte shkarkuar nga studenti.
ALTER TABLE ShkarkimetMaterialeve
ADD CONSTRAINT FK_ShkarkimetMaterialeve_Materialet
FOREIGN KEY (MaterialID) REFERENCES Materialet(MaterialID) ON DELETE CASCADE;

SELECT * FROM ShkarkimetMaterialeve;
SELECT * FROM Userat;

UPDATE Userat
SET Semestri = 6
WHERE UserID = 45;

SELECT * FROM Lendet;


/* Modifikime ne tabelen e orareve pasi qe oraret nuk lidhen mire ne applikacion
shkaku i mos lidhjes jane : NendrejtimID viti dhe semestri qe e ben qdo student 
pa marre parasysh nga cili vit dhe cili semester e ka orarin e njejte me ate qe studion
ne drejtim tjeter apo nendrejtim tjeter... */ 

ALTER TABLE Oraret
ADD DrejtimID INT NULL,
    NendrejtimID INT NULL,
    Viti INT NULL,
    Semestri INT NULL;

-- Shto foreign keys (opsionale, por të mira për integritet)
ALTER TABLE Oraret
ADD CONSTRAINT FK_Oraret_Drejtimet FOREIGN KEY (DrejtimID) REFERENCES Drejtimet(DrejtimID);

ALTER TABLE Oraret
ADD CONSTRAINT FK_Oraret_Nendrejtimet FOREIGN KEY (NendrejtimID) REFERENCES Nendrejtimet(NendrejtimID);


SELECT * FROM Lendet;



-- Viti 1, Semestri 1 – Inxhinieri Softuerike (NendrejtimID = 1)
INSERT INTO Oraret (GrupID, Dita, KohaFillimit, KohaMbarimit, Lloji, Salla, LendeID, DrejtimID, NendrejtimID, Viti, Semestri)
VALUES
-- Grup 1
(1, 'E Hënë', '09:00', '10:30', 'Ligjeratë', 'A-105', 10, 1, 1, 1, 1),
(1, 'E Hënë', '11:00', '12:30', 'Ushtrime', 'LAB-01', 10, 1, 1, 1, 1),
(1, 'E Martë', '09:00', '10:30', 'Ligjeratë', 'A-105', 9, 1, 1, 1, 1),
(1, 'E Martë', '11:00', '12:30', 'Ushtrime', 'LAB-01', 9, 1, 1, 1, 1),
(1, 'E Mërkurë', '09:00', '10:30', 'Ligjeratë', 'A-105', 1, 1, 1, 1, 1),
(1, 'E Mërkurë', '11:00', '12:30', 'Ushtrime', 'LAB-01', 1, 1, 1, 1, 1),
(1, 'E Enjte', '09:00', '10:30', 'Ligjeratë', 'A-105', 7, 1, 1, 1, 1),
(1, 'E Enjte', '11:00', '12:30', 'Ushtrime', 'LAB-01', 7, 1, 1, 1, 1),
(1, 'E Premte', '09:00', '10:30', 'Ligjeratë', 'A-105', 8, 1, 1, 1, 1),
(1, 'E Premte', '11:00', '12:30', 'Ushtrime', 'LAB-01', 8, 1, 1, 1, 1),

-- Grup 2 (oraret e pasdites)
(2, 'E Hënë', '12:45', '14:15', 'Ligjeratë', 'A-105', 10, 1, 1, 1, 1),
(2, 'E Hënë', '14:30', '16:00', 'Ushtrime', 'LAB-01', 10, 1, 1, 1, 1),
(2, 'E Martë', '12:45', '14:15', 'Ligjeratë', 'A-105', 9, 1, 1, 1, 1),
(2, 'E Martë', '14:30', '16:00', 'Ushtrime', 'LAB-01', 9, 1, 1, 1, 1),
(2, 'E Mërkurë', '12:45', '14:15', 'Ligjeratë', 'A-105', 1, 1, 1, 1, 1),
(2, 'E Mërkurë', '14:30', '16:00', 'Ushtrime', 'LAB-01', 1, 1, 1, 1, 1),
(2, 'E Enjte', '12:45', '14:15', 'Ligjeratë', 'A-105', 7, 1, 1, 1, 1),
(2, 'E Enjte', '14:30', '16:00', 'Ushtrime', 'LAB-01', 7, 1, 1, 1, 1),
(2, 'E Premte', '12:45', '14:15', 'Ligjeratë', 'A-105', 8, 1, 1, 1, 1),
(2, 'E Premte', '14:30', '16:00', 'Ushtrime', 'LAB-01', 8, 1, 1, 1, 1),

-- ... vazhdo për ditët e tjera si në shembullin tënd
-- (për të mos e bërë shumë të gjatë, mund ta kopjosh vetë strukturën)

-- Grup 3 (pasdite vonë)
(3, 'E Hënë', '16:30', '18:00', 'Ligjeratë', 'A-105', 10, 1, 1, 1, 1),
(3, 'E Hënë', '18:15', '19:45', 'Ushtrime', 'LAB-01', 10, 1, 1, 1, 1),
(3, 'E Martë', '16:30', '18:00', 'Ligjeratë', 'A-105', 9, 1, 1, 1, 1),
(3, 'E Martë', '18:15', '19:45', 'Ushtrime', 'LAB-01', 9, 1, 1, 1, 1),
(3, 'E Mërkurë', '16:30', '18:00', 'Ligjeratë', 'A-105', 1, 1, 1, 1, 1),
(3, 'E Mërkurë', '18:15', '19:45', 'Ushtrime', 'LAB-01', 1, 1, 1, 1, 1),
(3, 'E Enjte', '16:30', '18:00', 'Ligjeratë', 'A-105', 7, 1, 1, 1, 1),
(3, 'E Enjte', '18:15', '19:45', 'Ushtrime', 'LAB-01', 7, 1, 1, 1, 1),
(3, 'E Premte', '16:30', '18:00', 'Ligjeratë', 'A-105', 8, 1, 1, 1, 1),
(3, 'E Premte', '18:15', '19:45', 'Ushtrime', 'LAB-01', 8, 1, 1, 1, 1);


SELECT * FROM Lendet;

-- Viti 1, Semestri 2 – Inxhinieri Softuerike (NendrejtimID = 1)
INSERT INTO Oraret (GrupID, Dita, KohaFillimit, KohaMbarimit, Lloji, Salla, LendeID, DrejtimID, NendrejtimID, Viti, Semestri)
VALUES
-- Grup 1
(1, 'E Hënë', '09:00', '10:30', 'Ligjeratë', 'A-205', 15, 1, 1, 1, 2),
(1, 'E Hënë', '11:00', '12:30', 'Ushtrime', 'A-205', 15, 1, 1, 1, 2),
(1, 'E Martë', '09:00', '10:30', 'Ligjeratë', 'A-205', 16, 1, 1, 1, 2),
(1, 'E Martë', '11:00', '12:30', 'Ushtrime', 'LAB-04', 16, 1, 1, 1, 2),
(1, 'E Mërkurë', '09:00', '10:30', 'Ligjeratë', 'A-205', 14, 1, 1, 1, 2),
(1, 'E Mërkurë', '11:00', '12:30', 'Ushtrime', 'LAB-04', 14, 1, 1, 1, 2),
(1, 'E Enjte', '09:00', '10:30', 'Ligjeratë', 'A-205', 17, 1, 1, 1, 2),
(1, 'E Enjte', '11:00', '12:30', 'Ushtrime', 'LAB-04', 17, 1, 1, 1, 2),
(1, 'E Premte', '09:00', '10:30', 'Ligjeratë', 'A-205', 13, 1, 1, 1, 2),
(1, 'E Premte', '11:00', '11:45', 'Ushtrime', 'LAB-05', 13, 1, 1, 1, 2),

-- Grup 2 
(2, 'E Hënë', '12:45', '14:15', 'Ligjeratë', 'A-105', 15, 1, 1, 1, 2),
(2, 'E Hënë', '14:30', '16:00', 'Ushtrime', 'LAB-01', 15, 1, 1, 1, 2),
(2, 'E Martë', '12:45', '14:15', 'Ligjeratë', 'A-105', 16, 1, 1, 1, 2),
(2, 'E Martë', '14:30', '16:00', 'Ushtrime', 'LAB-01', 16, 1, 1, 1, 2),
(2, 'E Mërkurë', '12:45', '14:15', 'Ligjeratë', 'A-105', 14, 1, 1, 1, 2),
(2, 'E Mërkurë', '14:30', '16:00', 'Ushtrime', 'LAB-01', 14, 1, 1, 1, 2),
(2, 'E Enjte', '12:45', '14:15', 'Ligjeratë', 'A-105', 17, 1, 1, 1, 2),
(2, 'E Enjte', '14:30', '16:00', 'Ushtrime', 'LAB-01', 17, 1, 1, 1, 2),
(2, 'E Premte', '12:45', '14:15', 'Ligjeratë', 'A-105', 13, 1, 1, 1, 2),
(2, 'E Premte', '14:30', '15:15', 'Ushtrime', 'LAB-01', 13, 1, 1, 1, 2),


-- Grup 3 
(3, 'E Hënë', '16:30', '18:00', 'Ligjeratë', 'A-105', 15, 1, 1, 1, 2),
(3, 'E Hënë', '18:15', '19:45', 'Ushtrime', 'LAB-01', 15, 1, 1, 1, 2),
(3, 'E Martë', '16:30', '18:00', 'Ligjeratë', 'A-105', 16, 1, 1, 1, 2),
(3, 'E Martë', '18:15', '19:45', 'Ushtrime', 'LAB-01', 16, 1, 1, 1, 2),
(3, 'E Mërkurë', '16:30', '18:00', 'Ligjeratë', 'A-105', 14, 1, 1, 1, 2),
(3, 'E Mërkurë', '18:15', '19:45', 'Ushtrime', 'LAB-01', 14, 1, 1, 1, 2),
(3, 'E Enjte', '16:30', '18:00', 'Ligjeratë', 'A-105', 17, 1, 1, 1, 2),
(3, 'E Enjte', '18:15', '19:45', 'Ushtrime', 'LAB-01', 17, 1, 1, 1, 2),
(3, 'E Premte', '16:30', '18:00', 'Ligjeratë', 'A-105', 13, 1, 1, 1, 2),
(3, 'E Premte', '18:15', '19:00', 'Ushtrime', 'LAB-01', 13, 1, 1, 1, 2);


SELECT * FROM Lendet WHERE Semestri = 6;

-- Vitet tjera..  
INSERT INTO Oraret (GrupID, Dita, KohaFillimit, KohaMbarimit, Lloji, Salla, LendeID, DrejtimID, NendrejtimID, Viti, Semestri)
VALUES
-- Grup 1

(1, 'E Martë', '09:00', '10:30', 'Ligjeratë', 'A-205', 4, 1, 1, 3, 6),
(1, 'E Martë', '11:00', '12:30', 'Ushtrime', 'LAB-04', 4, 1, 1, 3, 6),
(1, 'E Mërkurë', '09:00', '10:30', 'Ligjeratë', 'A-205', 6, 1, 1, 3, 6),
(1, 'E Mërkurë', '11:00', '12:30', 'Ushtrime', 'LAB-04', 6, 1, 1, 3, 6),
(1, 'E Enjte', '09:00', '10:30', 'Ligjeratë', 'A-205', 3, 1, 1, 3, 6),
(1, 'E Enjte', '11:00', '12:30', 'Ushtrime', 'LAB-04', 3, 1, 1, 3, 6),
(1, 'E Premte', '09:00', '10:30', 'Ligjeratë', 'A-205', 5, 1, 1, 3, 6),
(1, 'E Premte', '11:00', '11:45', 'Ushtrime', 'LAB-05', 5, 1, 1, 3, 6),

-- Grup 2 

(2, 'E Martë', '12:45', '14:15', 'Ligjeratë', 'A-105', 4, 1, 1, 3, 6),
(2, 'E Martë', '14:30', '16:00', 'Ushtrime', 'LAB-01', 4, 1, 1, 3, 6),
(2, 'E Mërkurë', '12:45', '14:15', 'Ligjeratë', 'A-105', 6, 1, 1, 3, 6),
(2, 'E Mërkurë', '14:30', '16:00', 'Ushtrime', 'LAB-01', 6, 1, 1, 3, 6),
(2, 'E Enjte', '12:45', '14:15', 'Ligjeratë', 'A-105', 3, 1, 1, 3, 6),
(2, 'E Enjte', '14:30', '16:00', 'Ushtrime', 'LAB-01', 3, 1, 1, 3, 6),
(2, 'E Premte', '12:45', '14:15', 'Ligjeratë', 'A-105', 5, 1, 1, 3, 6),
(2, 'E Premte', '14:30', '15:15', 'Ushtrime', 'LAB-01', 5, 1, 1, 3, 6),


-- Grup 3 

(3, 'E Martë', '16:30', '18:00', 'Ligjeratë', 'A-105', 4, 1, 1, 3, 6),
(3, 'E Martë', '18:15', '19:45', 'Ushtrime', 'LAB-01', 4, 1, 1, 3, 6),
(3, 'E Mërkurë', '16:30', '18:00', 'Ligjeratë', 'A-105', 6, 1, 1, 3, 6),
(3, 'E Mërkurë', '18:15', '19:45', 'Ushtrime', 'LAB-01', 6, 1, 1, 3, 6),
(3, 'E Enjte', '16:30', '18:00', 'Ligjeratë', 'A-105', 3, 1, 1, 3, 6),
(3, 'E Enjte', '18:15', '19:45', 'Ushtrime', 'LAB-01', 3, 1, 1, 3, 6),
(3, 'E Premte', '16:30', '18:00', 'Ligjeratë', 'A-105', 5, 1, 1, 3, 6),
(3, 'E Premte', '18:15', '19:00', 'Ushtrime', 'LAB-01', 5, 1, 1, 3, 6);

SELECT * FROM LoginLogs;


SELECT * FROM WhitelistedIPs;
INSERT INTO WhitelistedIPs(IPAddress)
VALUES
('46.99.48.245');

DELETE FROM WhitelistedIPs
WHERE IPAddress = ('46.99.48.245');



SELECT * FROM Userat;

DELETE  FROM BlockedIPs;

SELECT * FROM BlockedIps;
SELECT * FROM Loginlogs;
SELECT * FROM FailedLogins;

SELECT * FROM Roles;

SELECT * FROM Userat;

SELECT * FROM WhitelistedIPs;

-- Krijimi i inkeksit per loginlogs qe ta bejme me te lehte shfaqjen ne e-administratori!!!
CREATE NONCLUSTERED INDEX IX_LoginLogs_UserID_LoginTime_DESC
ON LoginLogs (UserID, LoginTime DESC)
INCLUDE (IPAddress); -- posaqerisht per klaulozen select

SELECT * FROM NotatPerfundimtare;

SELECT * FROM Userat
WHERE RoleID = 1;

SELECT * FROM Lendet;

SELECT * FROM Oraret;


-- Index per hapjen e formes se orarit me te shpejt 

CREATE NONCLUSTERED INDEX IX_Oraret_Viti_Semestri_GrupID 
ON Oraret (Viti, Semestri, GrupID) 
INCLUDE (LendeID, Dita, KohaFillimit, KohaMbarimit, Lloji, Salla);

SELECT * FROM Lendet;

SELECT * FROM Userat WHERE RoleID = 1;
SELECT * FROM NotatPerfundimtare;


ALTER TABLE Userat
ADD OnHold BIT NOT NULL DEFAULT 0;

/* Stored procedure per automatizimin e nderrimit te semestrit dhe vitit
duke u bazuar ne perfundimin e semestrit dhe vitit
Semestri nderrohet ne mars, kurse viti ne tetor... */


-- 1. AvancoSemestrin (Mars) - me OnHold

CREATE OR ALTER PROCEDURE dbo.AvancoSemestrin
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE Userat
        SET Semestri = Semestri + 1,
            UpdatedAT = GETDATE()
        WHERE RoleID = 1
          AND OnHold = 0                    -- mos avanco nëse është OnHold
          AND Semestri IN (1, 3, 5)
          AND Semestri IS NOT NULL
          AND Viti IS NOT NULL;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @ErrorMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMsg, 16, 1);
    END CATCH
END;
GO



-- 2. AvancoVitin (Tetor) - me OnHold

CREATE OR ALTER PROCEDURE dbo.AvancoVitin
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Rasti normal (studentë që nuk kanë mbaruar ende)
        UPDATE Userat
        SET Viti     = Viti + 1,
            Semestri = Semestri + 1,
            UpdatedAT = GETDATE()
        WHERE RoleID = 1
          AND OnHold = 0
          AND Semestri IN (2, 4, 6)
          AND Semestri IS NOT NULL
          AND Viti IS NOT NULL
          AND NOT (Viti = 3 AND Semestri = 6);   -- mos prek ata që janë në fund

        -- Rasti special: studentët që arrijnë në Vitin 3 Semestrin 6 → OnHold
        UPDATE Userat
        SET OnHold = 1,
            UpdatedAT = GETDATE()
        WHERE RoleID = 1
          AND OnHold = 0
          AND Viti = 3
          AND Semestri = 6;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @ErrorMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMsg, 16, 1);
    END CATCH
END;
GO



SELECT * FROM Userat;

UPDATE Userat
SET Semestri = 2
WHERE UserID IN (52,53,54);

UPDATE Userat
SET Viti = 2
WHERE UserID = 2;










